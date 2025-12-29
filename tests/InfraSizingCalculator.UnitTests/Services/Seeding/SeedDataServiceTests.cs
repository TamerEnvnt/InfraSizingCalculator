using FluentAssertions;
using InfraSizingCalculator.Data;
using InfraSizingCalculator.Data.Entities;
using InfraSizingCalculator.Services.Seeding;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Services.Seeding;

/// <summary>
/// Tests for SeedDataService - hash-based database seeding with version tracking.
/// </summary>
public class SeedDataServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _seedDataPath;
    private readonly ILogger<SeedDataService> _mockLogger;
    private readonly IWebHostEnvironment _mockEnvironment;
    private readonly string _dbName;

    public SeedDataServiceTests()
    {
        // Create unique temp directory for seed files
        _tempDir = Path.Combine(Path.GetTempPath(), $"SeedTest_{Guid.NewGuid()}");
        _seedDataPath = Path.Combine(_tempDir, "Data", "Seeds");
        Directory.CreateDirectory(_seedDataPath);

        _dbName = $"TestDb_{Guid.NewGuid()}";

        // Setup mocks
        _mockLogger = Substitute.For<ILogger<SeedDataService>>();
        _mockEnvironment = Substitute.For<IWebHostEnvironment>();
        _mockEnvironment.ContentRootPath.Returns(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }

    private ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddDbContext<InfraSizingDbContext>(options =>
            options.UseInMemoryDatabase(_dbName)
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
        return services.BuildServiceProvider();
    }

    private void CreateSeedFile(string fileName, string content)
    {
        File.WriteAllText(Path.Combine(_seedDataPath, fileName), content);
    }

    private void CreateDefaultSeedFiles()
    {
        CreateSeedFile("application-settings.json", @"{
            ""data"": {
                ""includePricingInResults"": true,
                ""defaultCurrency"": ""EUR"",
                ""pricingCacheDurationHours"": 48
            }
        }");

        CreateSeedFile("cloud-api-credentials.json", @"{
            ""data"": [
                { ""providerName"": ""AWS"", ""isConfigured"": false },
                { ""providerName"": ""Azure"", ""isConfigured"": false }
            ]
        }");

        CreateSeedFile("distributions.json", @"{
            ""data"": [
                {
                    ""key"": ""TestK8s"",
                    ""name"": ""Test Kubernetes"",
                    ""vendor"": ""TestVendor"",
                    ""icon"": ""test"",
                    ""brandColor"": ""#123456"",
                    ""tags"": ""test"",
                    ""sortOrder"": 1,
                    ""hasInfraNodes"": false,
                    ""hasManagedControlPlane"": false,
                    ""prodControlPlane"": { ""cpu"": 4, ""ram"": 16, ""disk"": 100 },
                    ""nonProdControlPlane"": { ""cpu"": 2, ""ram"": 8, ""disk"": 50 },
                    ""prodWorker"": { ""cpu"": 8, ""ram"": 32, ""disk"": 200 },
                    ""nonProdWorker"": { ""cpu"": 4, ""ram"": 16, ""disk"": 100 }
                }
            ]
        }");
    }

    #region CurrentVersion Tests

    [Fact]
    public void CurrentVersion_IsNotEmpty()
    {
        SeedDataService.CurrentVersion.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void CurrentVersion_IsSemanticVersion()
    {
        var version = SeedDataService.CurrentVersion;
        var parts = version.Split('.');
        parts.Length.Should().Be(3);
        int.TryParse(parts[0], out _).Should().BeTrue();
        int.TryParse(parts[1], out _).Should().BeTrue();
        int.TryParse(parts[2], out _).Should().BeTrue();
    }

    [Fact]
    public void CurrentVersion_ShouldBe100()
    {
        SeedDataService.CurrentVersion.Should().Be("1.0.0");
    }

    #endregion

    #region EnsureSeedDataAsync Tests

    [Fact]
    public async Task EnsureSeedDataAsync_WhenNoMetadata_PerformsInitialSeed()
    {
        // Arrange
        CreateDefaultSeedFiles();
        using var serviceProvider = CreateServiceProvider();
        var service = new SeedDataService(serviceProvider, _mockLogger, _mockEnvironment);

        // Act
        await service.EnsureSeedDataAsync();

        // Assert
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<InfraSizingDbContext>();

        var metadata = await dbContext.Set<SeedMetadataEntity>().FirstOrDefaultAsync();
        metadata.Should().NotBeNull();
        metadata!.SeedCount.Should().Be(1);
        metadata.Version.Should().Be("1.0.0");
        metadata.SeedHash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task EnsureSeedDataAsync_WhenHashUnchanged_SkipsSeeding()
    {
        // Arrange
        CreateDefaultSeedFiles();
        using var serviceProvider = CreateServiceProvider();
        var service = new SeedDataService(serviceProvider, _mockLogger, _mockEnvironment);

        // First seed
        await service.EnsureSeedDataAsync();

        // Act - Second call with same files
        await service.EnsureSeedDataAsync();

        // Assert - SeedCount should still be 1
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<InfraSizingDbContext>();

        var metadata = await dbContext.Set<SeedMetadataEntity>().FirstOrDefaultAsync();
        metadata!.SeedCount.Should().Be(1);
    }

    [Fact]
    public async Task EnsureSeedDataAsync_WhenHashChanges_ReSeeds()
    {
        // Arrange
        CreateDefaultSeedFiles();
        using var serviceProvider = CreateServiceProvider();
        var service = new SeedDataService(serviceProvider, _mockLogger, _mockEnvironment);

        // First seed
        await service.EnsureSeedDataAsync();

        // Modify seed file
        CreateSeedFile("application-settings.json", @"{
            ""data"": {
                ""includePricingInResults"": false,
                ""defaultCurrency"": ""GBP"",
                ""pricingCacheDurationHours"": 12
            }
        }");

        // Act - Second call with changed files
        await service.EnsureSeedDataAsync();

        // Assert - SeedCount should be 2
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<InfraSizingDbContext>();

        var metadata = await dbContext.Set<SeedMetadataEntity>().FirstOrDefaultAsync();
        metadata!.SeedCount.Should().Be(2);
    }

    [Fact]
    public async Task EnsureSeedDataAsync_SeedsApplicationSettings()
    {
        // Arrange
        CreateDefaultSeedFiles();
        using var serviceProvider = CreateServiceProvider();
        var service = new SeedDataService(serviceProvider, _mockLogger, _mockEnvironment);

        // Act
        await service.EnsureSeedDataAsync();

        // Assert
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<InfraSizingDbContext>();

        var settings = await dbContext.ApplicationSettings.FirstOrDefaultAsync();
        settings.Should().NotBeNull();
        settings!.IncludePricingInResults.Should().BeTrue();
        settings.DefaultCurrency.Should().Be("EUR");
        settings.PricingCacheDurationHours.Should().Be(48);
    }

    [Fact]
    public async Task EnsureSeedDataAsync_SeedsCloudCredentials()
    {
        // Arrange
        CreateDefaultSeedFiles();
        using var serviceProvider = CreateServiceProvider();
        var service = new SeedDataService(serviceProvider, _mockLogger, _mockEnvironment);

        // Act
        await service.EnsureSeedDataAsync();

        // Assert
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<InfraSizingDbContext>();

        var credentials = await dbContext.CloudApiCredentials.ToListAsync();
        credentials.Should().HaveCount(2);
        credentials.Should().Contain(c => c.ProviderName == "AWS");
        credentials.Should().Contain(c => c.ProviderName == "Azure");
    }

    [Fact]
    public async Task EnsureSeedDataAsync_SeedsOnPremPricing()
    {
        // Arrange
        CreateDefaultSeedFiles();
        using var serviceProvider = CreateServiceProvider();
        var service = new SeedDataService(serviceProvider, _mockLogger, _mockEnvironment);

        // Act
        await service.EnsureSeedDataAsync();

        // Assert
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<InfraSizingDbContext>();

        var pricing = await dbContext.OnPremPricing.FirstOrDefaultAsync();
        pricing.Should().NotBeNull();
    }

    [Fact]
    public async Task EnsureSeedDataAsync_SeedsMendixPricing()
    {
        // Arrange
        CreateDefaultSeedFiles();
        using var serviceProvider = CreateServiceProvider();
        var service = new SeedDataService(serviceProvider, _mockLogger, _mockEnvironment);

        // Act
        await service.EnsureSeedDataAsync();

        // Assert
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<InfraSizingDbContext>();

        var pricing = await dbContext.MendixPricing.FirstOrDefaultAsync();
        pricing.Should().NotBeNull();
    }

    [Fact]
    public async Task EnsureSeedDataAsync_SeedsOutSystemsPricing()
    {
        // Arrange
        CreateDefaultSeedFiles();
        using var serviceProvider = CreateServiceProvider();
        var service = new SeedDataService(serviceProvider, _mockLogger, _mockEnvironment);

        // Act
        await service.EnsureSeedDataAsync();

        // Assert
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<InfraSizingDbContext>();

        var pricing = await dbContext.OutSystemsPricing.FirstOrDefaultAsync();
        pricing.Should().NotBeNull();
    }

    [Fact]
    public async Task EnsureSeedDataAsync_SeedsDistributions()
    {
        // Arrange
        CreateDefaultSeedFiles();
        using var serviceProvider = CreateServiceProvider();
        var service = new SeedDataService(serviceProvider, _mockLogger, _mockEnvironment);

        // Act
        await service.EnsureSeedDataAsync();

        // Assert
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<InfraSizingDbContext>();

        var distributions = await dbContext.DistributionConfigs.ToListAsync();
        distributions.Should().HaveCount(1);
        distributions[0].DistributionKey.Should().Be("TestK8s");
        distributions[0].Name.Should().Be("Test Kubernetes");
        distributions[0].ProdControlPlaneCpu.Should().Be(4);
        distributions[0].ProdControlPlaneRam.Should().Be(16);
    }

    [Fact]
    public async Task EnsureSeedDataAsync_DoesNotOverwriteExistingSettings()
    {
        // Arrange
        CreateDefaultSeedFiles();
        using var serviceProvider = CreateServiceProvider();

        // Pre-seed settings
        using (var scope = serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<InfraSizingDbContext>();
            dbContext.ApplicationSettings.Add(new ApplicationSettingsEntity
            {
                Id = 1,
                IncludePricingInResults = false,
                DefaultCurrency = "GBP",
                PricingCacheDurationHours = 72
            });
            await dbContext.SaveChangesAsync();
        }

        var service = new SeedDataService(serviceProvider, _mockLogger, _mockEnvironment);

        // Act
        await service.EnsureSeedDataAsync();

        // Assert - Original settings preserved
        using var verifyScope = serviceProvider.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<InfraSizingDbContext>();

        var settings = await verifyContext.ApplicationSettings.FirstOrDefaultAsync();
        settings!.DefaultCurrency.Should().Be("GBP");
        settings.PricingCacheDurationHours.Should().Be(72);
    }

    [Fact]
    public async Task EnsureSeedDataAsync_DoesNotOverwriteExistingCredentials()
    {
        // Arrange
        CreateDefaultSeedFiles();
        using var serviceProvider = CreateServiceProvider();

        // Pre-seed credentials
        using (var scope = serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<InfraSizingDbContext>();
            dbContext.CloudApiCredentials.Add(new CloudApiCredentialsEntity
            {
                Id = 1,
                ProviderName = "Custom",
                IsConfigured = true
            });
            await dbContext.SaveChangesAsync();
        }

        var service = new SeedDataService(serviceProvider, _mockLogger, _mockEnvironment);

        // Act
        await service.EnsureSeedDataAsync();

        // Assert - Original credentials preserved
        using var verifyScope = serviceProvider.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<InfraSizingDbContext>();

        var credentials = await verifyContext.CloudApiCredentials.ToListAsync();
        credentials.Should().HaveCount(1);
        credentials[0].ProviderName.Should().Be("Custom");
    }

    [Fact]
    public async Task EnsureSeedDataAsync_ReplacesDistributionsOnReseed()
    {
        // Arrange
        CreateDefaultSeedFiles();
        using var serviceProvider = CreateServiceProvider();
        var service = new SeedDataService(serviceProvider, _mockLogger, _mockEnvironment);

        // First seed
        await service.EnsureSeedDataAsync();

        // Modify distributions
        CreateSeedFile("distributions.json", @"{
            ""data"": [
                {
                    ""key"": ""NewK8s"",
                    ""name"": ""New Kubernetes"",
                    ""vendor"": ""NewVendor"",
                    ""icon"": ""new"",
                    ""brandColor"": ""#654321"",
                    ""tags"": ""new"",
                    ""sortOrder"": 1,
                    ""hasInfraNodes"": true,
                    ""hasManagedControlPlane"": true
                }
            ]
        }");

        // Act - Reseed
        await service.EnsureSeedDataAsync();

        // Assert - Distributions replaced
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<InfraSizingDbContext>();

        var distributions = await dbContext.DistributionConfigs.ToListAsync();
        distributions.Should().HaveCount(1);
        distributions[0].DistributionKey.Should().Be("NewK8s");
        distributions[0].HasInfraNodes.Should().BeTrue();
        distributions[0].HasManagedControlPlane.Should().BeTrue();
    }

    [Fact]
    public async Task EnsureSeedDataAsync_WithCancellation_ThrowsOperationCanceled()
    {
        // Arrange
        CreateDefaultSeedFiles();
        using var serviceProvider = CreateServiceProvider();
        var service = new SeedDataService(serviceProvider, _mockLogger, _mockEnvironment);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert - Cancellation should throw at first async checkpoint
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.EnsureSeedDataAsync(cts.Token));
    }

    #endregion

    #region ForceSeedAsync Tests

    [Fact]
    public async Task ForceSeedAsync_AlwaysSeeds()
    {
        // Arrange
        CreateDefaultSeedFiles();
        using var serviceProvider = CreateServiceProvider();
        var service = new SeedDataService(serviceProvider, _mockLogger, _mockEnvironment);

        // First seed via ensure
        await service.EnsureSeedDataAsync();

        // Act - Force reseed with same data
        await service.ForceSeedAsync();

        // Assert - SeedCount should be 2
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<InfraSizingDbContext>();

        var metadata = await dbContext.Set<SeedMetadataEntity>().FirstOrDefaultAsync();
        metadata!.SeedCount.Should().Be(2);
    }

    [Fact]
    public async Task ForceSeedAsync_UpdatesHash()
    {
        // Arrange
        CreateDefaultSeedFiles();
        using var serviceProvider = CreateServiceProvider();
        var service = new SeedDataService(serviceProvider, _mockLogger, _mockEnvironment);

        await service.EnsureSeedDataAsync();

        // Modify file
        CreateSeedFile("application-settings.json", @"{
            ""data"": { ""defaultCurrency"": ""JPY"" }
        }");

        // Act
        await service.ForceSeedAsync();

        // Assert - Hash updated
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<InfraSizingDbContext>();

        var metadata = await dbContext.Set<SeedMetadataEntity>().FirstOrDefaultAsync();
        metadata!.SeedHash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ForceSeedAsync_WithCancellation_ThrowsOperationCanceled()
    {
        // Arrange
        CreateDefaultSeedFiles();
        using var serviceProvider = CreateServiceProvider();
        var service = new SeedDataService(serviceProvider, _mockLogger, _mockEnvironment);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert - Cancellation should throw at first async checkpoint
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.ForceSeedAsync(cts.Token));
    }

    #endregion

    #region GetMetadataAsync Tests

    [Fact]
    public async Task GetMetadataAsync_WhenNoMetadata_ReturnsNull()
    {
        // Arrange
        CreateDefaultSeedFiles();
        using var serviceProvider = CreateServiceProvider();
        var service = new SeedDataService(serviceProvider, _mockLogger, _mockEnvironment);

        // Act
        var result = await service.GetMetadataAsync();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetMetadataAsync_AfterSeeding_ReturnsMetadata()
    {
        // Arrange
        CreateDefaultSeedFiles();
        using var serviceProvider = CreateServiceProvider();
        var service = new SeedDataService(serviceProvider, _mockLogger, _mockEnvironment);
        await service.EnsureSeedDataAsync();

        // Act
        var result = await service.GetMetadataAsync();

        // Assert
        result.Should().NotBeNull();
        result!.Version.Should().Be("1.0.0");
        result.SeedCount.Should().Be(1);
        result.SeedHash.Should().NotBeNullOrEmpty();
        result.LastSeededAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    #endregion

    #region Missing/Invalid Seed File Tests

    [Fact]
    public async Task EnsureSeedDataAsync_WithMissingSettingsFile_UsesDefaults()
    {
        // Arrange - Only create some files, not application-settings.json
        CreateSeedFile("cloud-api-credentials.json", @"{ ""data"": [] }");
        CreateSeedFile("distributions.json", @"{ ""data"": [] }");

        using var serviceProvider = CreateServiceProvider();
        var service = new SeedDataService(serviceProvider, _mockLogger, _mockEnvironment);

        // Act
        await service.EnsureSeedDataAsync();

        // Assert - Default values used
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<InfraSizingDbContext>();

        var settings = await dbContext.ApplicationSettings.FirstOrDefaultAsync();
        settings.Should().NotBeNull();
        settings!.DefaultCurrency.Should().Be("USD"); // Default
        settings.PricingCacheDurationHours.Should().Be(24); // Default
    }

    [Fact]
    public async Task EnsureSeedDataAsync_WithMissingCredentialsFile_UsesDefaults()
    {
        // Arrange
        CreateSeedFile("application-settings.json", @"{ ""data"": {} }");
        CreateSeedFile("distributions.json", @"{ ""data"": [] }");

        using var serviceProvider = CreateServiceProvider();
        var service = new SeedDataService(serviceProvider, _mockLogger, _mockEnvironment);

        // Act
        await service.EnsureSeedDataAsync();

        // Assert - Default providers created
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<InfraSizingDbContext>();

        var credentials = await dbContext.CloudApiCredentials.ToListAsync();
        credentials.Should().HaveCount(4); // AWS, Azure, GCP, Oracle
    }

    [Fact]
    public async Task EnsureSeedDataAsync_WithEmptyDistributions_LogsWarning()
    {
        // Arrange
        CreateSeedFile("application-settings.json", @"{ ""data"": {} }");
        CreateSeedFile("cloud-api-credentials.json", @"{ ""data"": [] }");
        CreateSeedFile("distributions.json", @"{ ""data"": [] }");

        using var serviceProvider = CreateServiceProvider();
        var service = new SeedDataService(serviceProvider, _mockLogger, _mockEnvironment);

        // Act
        await service.EnsureSeedDataAsync();

        // Assert - No distributions added
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<InfraSizingDbContext>();

        var distributions = await dbContext.DistributionConfigs.ToListAsync();
        distributions.Should().BeEmpty();
    }

    [Fact]
    public async Task EnsureSeedDataAsync_WithInvalidJson_HandlesGracefully()
    {
        // Arrange
        CreateSeedFile("application-settings.json", "{ invalid json }");
        CreateSeedFile("cloud-api-credentials.json", @"{ ""data"": [] }");
        CreateSeedFile("distributions.json", @"{ ""data"": [] }");

        using var serviceProvider = CreateServiceProvider();
        var service = new SeedDataService(serviceProvider, _mockLogger, _mockEnvironment);

        // Act
        await service.EnsureSeedDataAsync();

        // Assert - Defaults used despite invalid file
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<InfraSizingDbContext>();

        var settings = await dbContext.ApplicationSettings.FirstOrDefaultAsync();
        settings.Should().NotBeNull();
        settings!.DefaultCurrency.Should().Be("USD");
    }

    #endregion

    #region Distribution Seeding Edge Cases

    [Fact]
    public async Task SeedDistributions_WithNullNodeSpecs_UsesZeroDefaults()
    {
        // Arrange
        CreateSeedFile("application-settings.json", @"{ ""data"": {} }");
        CreateSeedFile("cloud-api-credentials.json", @"{ ""data"": [] }");
        CreateSeedFile("distributions.json", @"{
            ""data"": [{
                ""key"": ""ManagedK8s"",
                ""name"": ""Managed K8s"",
                ""vendor"": ""Cloud"",
                ""icon"": ""cloud"",
                ""brandColor"": ""#000"",
                ""tags"": ""managed"",
                ""sortOrder"": 1,
                ""hasInfraNodes"": false,
                ""hasManagedControlPlane"": true
            }]
        }");

        using var serviceProvider = CreateServiceProvider();
        var service = new SeedDataService(serviceProvider, _mockLogger, _mockEnvironment);

        // Act
        await service.EnsureSeedDataAsync();

        // Assert - Null specs default to 0
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<InfraSizingDbContext>();

        var dist = await dbContext.DistributionConfigs.FirstAsync();
        dist.ProdControlPlaneCpu.Should().Be(0);
        dist.ProdControlPlaneRam.Should().Be(0);
        dist.ProdControlPlaneDisk.Should().Be(0);
        dist.ProdWorkerCpu.Should().Be(0);
        dist.ProdInfraCpu.Should().Be(0);
    }

    [Fact]
    public async Task SeedDistributions_WithPartialSpecs_MapsCorrectly()
    {
        // Arrange
        CreateSeedFile("application-settings.json", @"{ ""data"": {} }");
        CreateSeedFile("cloud-api-credentials.json", @"{ ""data"": [] }");
        CreateSeedFile("distributions.json", @"{
            ""data"": [{
                ""key"": ""Partial"",
                ""name"": ""Partial Specs"",
                ""vendor"": ""Test"",
                ""icon"": ""test"",
                ""brandColor"": ""#FFF"",
                ""tags"": ""test"",
                ""sortOrder"": 1,
                ""hasInfraNodes"": true,
                ""hasManagedControlPlane"": false,
                ""prodWorker"": { ""cpu"": 8, ""ram"": 32, ""disk"": 200 },
                ""prodInfra"": { ""cpu"": 4, ""ram"": 16, ""disk"": 100 }
            }]
        }");

        using var serviceProvider = CreateServiceProvider();
        var service = new SeedDataService(serviceProvider, _mockLogger, _mockEnvironment);

        // Act
        await service.EnsureSeedDataAsync();

        // Assert
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<InfraSizingDbContext>();

        var dist = await dbContext.DistributionConfigs.FirstAsync();
        dist.ProdWorkerCpu.Should().Be(8);
        dist.ProdWorkerRam.Should().Be(32);
        dist.ProdInfraCpu.Should().Be(4);
        dist.ProdControlPlaneCpu.Should().Be(0); // Not specified
        dist.NonProdWorkerCpu.Should().Be(0); // Not specified
    }

    [Fact]
    public async Task SeedDistributions_SetsCorrectTimestamps()
    {
        // Arrange
        CreateDefaultSeedFiles();
        using var serviceProvider = CreateServiceProvider();
        var service = new SeedDataService(serviceProvider, _mockLogger, _mockEnvironment);
        var beforeSeed = DateTime.UtcNow;

        // Act
        await service.EnsureSeedDataAsync();

        // Assert
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<InfraSizingDbContext>();

        var dist = await dbContext.DistributionConfigs.FirstAsync();
        dist.CreatedAt.Should().BeOnOrAfter(beforeSeed);
        dist.UpdatedAt.Should().BeOnOrAfter(beforeSeed);
        dist.IsActive.Should().BeTrue();
    }

    #endregion

    #region Hash Calculation Tests

    [Fact]
    public async Task HashCalculation_IsDeterministic()
    {
        // Arrange
        CreateDefaultSeedFiles();
        using var serviceProvider = CreateServiceProvider();
        var service = new SeedDataService(serviceProvider, _mockLogger, _mockEnvironment);

        // Act - Seed and get first hash
        await service.EnsureSeedDataAsync();
        var firstMetadata = await service.GetMetadataAsync();
        var firstHash = firstMetadata!.SeedHash;

        // Force reseed
        await service.ForceSeedAsync();
        var secondMetadata = await service.GetMetadataAsync();
        var secondHash = secondMetadata!.SeedHash;

        // Assert - Same hash for same files
        firstHash.Should().Be(secondHash);
    }

    [Fact]
    public async Task HashCalculation_ChangesWithFileContent()
    {
        // Arrange
        CreateDefaultSeedFiles();
        using var serviceProvider = CreateServiceProvider();
        var service = new SeedDataService(serviceProvider, _mockLogger, _mockEnvironment);

        await service.EnsureSeedDataAsync();
        var firstHash = (await service.GetMetadataAsync())!.SeedHash;

        // Modify a file
        CreateSeedFile("application-settings.json", @"{ ""data"": { ""defaultCurrency"": ""CHF"" } }");
        await service.ForceSeedAsync();
        var secondHash = (await service.GetMetadataAsync())!.SeedHash;

        // Assert - Different hash
        firstHash.Should().NotBe(secondHash);
    }

    [Fact]
    public async Task HashCalculation_IsLowercaseHex()
    {
        // Arrange
        CreateDefaultSeedFiles();
        using var serviceProvider = CreateServiceProvider();
        var service = new SeedDataService(serviceProvider, _mockLogger, _mockEnvironment);

        // Act
        await service.EnsureSeedDataAsync();
        var metadata = await service.GetMetadataAsync();

        // Assert - SHA256 = 64 hex characters, all lowercase
        metadata!.SeedHash.Length.Should().Be(64);
        metadata.SeedHash.Should().MatchRegex("^[a-f0-9]+$");
    }

    #endregion

    #region Metadata Update Tests

    [Fact]
    public async Task MetadataUpdate_IncrementsSeedCount()
    {
        // Arrange
        CreateDefaultSeedFiles();
        using var serviceProvider = CreateServiceProvider();
        var service = new SeedDataService(serviceProvider, _mockLogger, _mockEnvironment);

        // Act
        await service.EnsureSeedDataAsync();
        await service.ForceSeedAsync();
        await service.ForceSeedAsync();

        // Assert
        var metadata = await service.GetMetadataAsync();
        metadata!.SeedCount.Should().Be(3);
    }

    [Fact]
    public async Task MetadataUpdate_UpdatesLastSeededAt()
    {
        // Arrange
        CreateDefaultSeedFiles();
        using var serviceProvider = CreateServiceProvider();
        var service = new SeedDataService(serviceProvider, _mockLogger, _mockEnvironment);

        await service.EnsureSeedDataAsync();
        var firstTime = (await service.GetMetadataAsync())!.LastSeededAt;

        await Task.Delay(10); // Small delay

        // Act
        await service.ForceSeedAsync();

        // Assert
        var secondTime = (await service.GetMetadataAsync())!.LastSeededAt;
        secondTime.Should().BeAfter(firstTime);
    }

    #endregion
}

#region Entity Tests

public class SeedMetadataEntityTests
{
    [Fact]
    public void SeedMetadataEntity_HasDefaultValues()
    {
        var entity = new SeedMetadataEntity();

        entity.Id.Should().Be(1);
        entity.Version.Should().Be("1.0.0");
        entity.SeedHash.Should().BeEmpty();
        entity.SeedCount.Should().Be(0);
    }

    [Fact]
    public void SeedMetadataEntity_CanSetProperties()
    {
        var entity = new SeedMetadataEntity();
        var now = DateTime.UtcNow;

        entity.SeedHash = "testhash123";
        entity.Version = "2.0.0";
        entity.LastSeededAt = now;
        entity.SeedCount = 5;

        entity.SeedHash.Should().Be("testhash123");
        entity.Version.Should().Be("2.0.0");
        entity.LastSeededAt.Should().Be(now);
        entity.SeedCount.Should().Be(5);
    }
}

public class DistributionConfigEntityTests
{
    [Fact]
    public void DistributionConfigEntity_HasDefaultValues()
    {
        var entity = new DistributionConfigEntity();

        entity.Id.Should().Be(0);
        entity.DistributionKey.Should().BeEmpty();
        entity.Name.Should().BeEmpty();
        entity.Vendor.Should().BeEmpty();
        entity.IsActive.Should().BeTrue();
        entity.BrandColor.Should().Be("#326CE5");
    }

    [Fact]
    public void DistributionConfigEntity_CanSetAllNodeSpecs()
    {
        var now = DateTime.UtcNow;

        var entity = new DistributionConfigEntity
        {
            Id = 1,
            DistributionKey = "openshift",
            Name = "OpenShift",
            Vendor = "Red Hat",
            Icon = "fas fa-hat-cowboy",
            BrandColor = "#EE0000",
            Tags = "enterprise,production",
            SortOrder = 1,
            HasInfraNodes = true,
            HasManagedControlPlane = false,
            ProdControlPlaneCpu = 4,
            ProdControlPlaneRam = 16,
            ProdControlPlaneDisk = 100,
            NonProdControlPlaneCpu = 2,
            NonProdControlPlaneRam = 8,
            NonProdControlPlaneDisk = 50,
            ProdWorkerCpu = 8,
            ProdWorkerRam = 32,
            ProdWorkerDisk = 200,
            NonProdWorkerCpu = 4,
            NonProdWorkerRam = 16,
            NonProdWorkerDisk = 100,
            ProdInfraCpu = 4,
            ProdInfraRam = 16,
            ProdInfraDisk = 100,
            NonProdInfraCpu = 2,
            NonProdInfraRam = 8,
            NonProdInfraDisk = 50,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        entity.DistributionKey.Should().Be("openshift");
        entity.HasInfraNodes.Should().BeTrue();
        entity.ProdControlPlaneCpu.Should().Be(4);
        entity.ProdWorkerRam.Should().Be(32);
        entity.NonProdInfraDisk.Should().Be(50);
    }
}

public class ApplicationSettingsEntityTests
{
    [Fact]
    public void ApplicationSettingsEntity_HasDefaultValues()
    {
        var entity = new ApplicationSettingsEntity();

        entity.Id.Should().Be(0);
        entity.IncludePricingInResults.Should().BeFalse();
        entity.DefaultCurrency.Should().Be("USD");
        entity.PricingCacheDurationHours.Should().Be(24);
    }

    [Fact]
    public void ApplicationSettingsEntity_CanSetProperties()
    {
        var now = DateTime.UtcNow;

        var entity = new ApplicationSettingsEntity
        {
            Id = 1,
            IncludePricingInResults = true,
            DefaultCurrency = "EUR",
            PricingCacheDurationHours = 48,
            CreatedAt = now,
            UpdatedAt = now
        };

        entity.IncludePricingInResults.Should().BeTrue();
        entity.DefaultCurrency.Should().Be("EUR");
        entity.PricingCacheDurationHours.Should().Be(48);
    }
}

public class CloudApiCredentialsEntityTests
{
    [Fact]
    public void CloudApiCredentialsEntity_HasDefaultValues()
    {
        var entity = new CloudApiCredentialsEntity();

        entity.Id.Should().Be(0);
        entity.ProviderName.Should().BeEmpty();
        entity.IsConfigured.Should().BeFalse();
        entity.ApiKey.Should().BeNull();
        entity.SecretKey.Should().BeNull();
    }

    [Fact]
    public void CloudApiCredentialsEntity_CanSetAllProperties()
    {
        var now = DateTime.UtcNow;

        var entity = new CloudApiCredentialsEntity
        {
            Id = 1,
            ProviderName = "AWS",
            IsConfigured = true,
            ApiKey = "test-api-key",
            SecretKey = "test-secret-key",
            Region = "us-east-1",
            TenantId = "tenant-123",
            SubscriptionId = "sub-456",
            ProjectId = "project-789",
            ValidationStatus = "Valid",
            LastValidated = now
        };

        entity.ProviderName.Should().Be("AWS");
        entity.IsConfigured.Should().BeTrue();
        entity.Region.Should().Be("us-east-1");
        entity.ValidationStatus.Should().Be("Valid");
    }
}

public class OnPremPricingEntityTests
{
    [Fact]
    public void OnPremPricingEntity_HasDefaultValues()
    {
        var entity = new OnPremPricingEntity();

        entity.ServerCostPerUnit.Should().Be(5000m);
        entity.CostPerCore.Should().Be(100m);
        entity.CostPerGBRam.Should().Be(10m);
        entity.StorageCostPerTB.Should().Be(200m);
        entity.HardwareLifespanYears.Should().Be(5);
        entity.RackUnitCostPerMonth.Should().Be(50m);
        entity.PowerCostPerKwhMonth.Should().Be(0.10m);
        entity.CoolingPUE.Should().Be(1.5m);
        entity.NetworkCostPerMonth.Should().Be(500m);
        entity.DevOpsSalaryPerYear.Should().Be(120000m);
        entity.SysAdminSalaryPerYear.Should().Be(90000m);
        entity.NodesPerDevOpsEngineer.Should().Be(50);
        entity.NodesPerSysAdmin.Should().Be(100);
    }

    [Fact]
    public void OnPremPricingEntity_HasDistributionLicensing()
    {
        var entity = new OnPremPricingEntity();

        entity.OpenShiftPerNodeYear.Should().Be(2500m);
        entity.TanzuPerCoreYear.Should().Be(1500m);
        entity.RancherEnterprisePerNodeYear.Should().Be(1000m);
        entity.CharmedK8sPerNodeYear.Should().Be(500m);
        entity.RKE2PerNodeYear.Should().Be(0m);
        entity.K3sPerNodeYear.Should().Be(0m);
    }
}

public class MendixPricingEntityTests
{
    [Fact]
    public void MendixPricingEntity_HasCloudPricing()
    {
        var entity = new MendixPricingEntity();

        entity.CloudTokenPrice.Should().BeGreaterThan(0);
        entity.CloudDedicatedPrice.Should().BeGreaterThan(0);
    }

    [Fact]
    public void MendixPricingEntity_HasStoragePricing()
    {
        var entity = new MendixPricingEntity();

        entity.AdditionalFileStoragePer100GB.Should().BeGreaterThan(0);
        entity.AdditionalDatabaseStoragePer100GB.Should().BeGreaterThan(0);
    }

    [Fact]
    public void MendixPricingEntity_HasK8sTierPricing()
    {
        var entity = new MendixPricingEntity();

        entity.K8sBasePackagePrice.Should().BeGreaterThan(0);
        entity.K8sEnvTier1Price.Should().BeGreaterThan(0);
        entity.K8sEnvTier2Price.Should().BeGreaterThan(0);
        entity.K8sEnvTier3Price.Should().BeGreaterThan(0);
        entity.K8sEnvTier4Price.Should().Be(0); // Free tier
    }

    [Fact]
    public void MendixPricingEntity_HasServerPricing()
    {
        var entity = new MendixPricingEntity();

        entity.ServerPerAppPrice.Should().BeGreaterThan(0);
        entity.ServerUnlimitedAppsPrice.Should().BeGreaterThan(0);
    }

    [Fact]
    public void MendixPricingEntity_HasGenAIPricing()
    {
        var entity = new MendixPricingEntity();

        entity.GenAIModelPackSPrice.Should().BeGreaterThan(0);
        entity.GenAIModelPackMPrice.Should().BeGreaterThan(0);
        entity.GenAIModelPackLPrice.Should().BeGreaterThan(0);
        entity.GenAIKnowledgeBasePrice.Should().BeGreaterThan(0);
    }
}

public class OutSystemsPricingEntityTests
{
    [Fact]
    public void OutSystemsPricingEntity_HasEditionPricing()
    {
        var entity = new OutSystemsPricingEntity();

        entity.StandardEditionBase.Should().BeGreaterThan(0);
        entity.EnterpriseEditionBase.Should().BeGreaterThan(0);
        entity.AdditionalAOPackPrice.Should().BeGreaterThan(0);
    }

    [Fact]
    public void OutSystemsPricingEntity_HasCloudAddOns()
    {
        var entity = new OutSystemsPricingEntity();

        entity.CloudAdditionalProdEnv.Should().BeGreaterThan(0);
        entity.CloudAdditionalNonProdEnv.Should().BeGreaterThan(0);
        entity.CloudHAAddOn.Should().BeGreaterThan(0);
        entity.CloudDRAddOn.Should().BeGreaterThan(0);
    }

    [Fact]
    public void OutSystemsPricingEntity_HasSelfManagedPricing()
    {
        var entity = new OutSystemsPricingEntity();

        entity.SelfManagedBase.Should().BeGreaterThan(0);
        entity.SelfManagedPerEnvironment.Should().BeGreaterThan(0);
        entity.SelfManagedPerFrontEnd.Should().BeGreaterThan(0);
    }

    [Fact]
    public void OutSystemsPricingEntity_HasUserLicensing()
    {
        var entity = new OutSystemsPricingEntity();

        entity.AdditionalInternalUserPackPrice.Should().BeGreaterThan(0);
        entity.ExternalUserPackPerYear.Should().BeGreaterThan(0);
    }

    [Fact]
    public void OutSystemsPricingEntity_HasSupportTiers()
    {
        var entity = new OutSystemsPricingEntity();

        entity.StandardSupportPercent.Should().Be(0m);
        entity.PremiumSupportPercent.Should().BeGreaterThan(0);
        entity.EliteSupportPercent.Should().BeGreaterThan(entity.PremiumSupportPercent);
    }

    [Fact]
    public void OutSystemsPricingEntity_HasProfessionalServices()
    {
        var entity = new OutSystemsPricingEntity();

        entity.ProfessionalServicesDayRate.Should().BeGreaterThan(0);
        entity.TrainingPerPersonPerDay.Should().BeGreaterThan(0);
    }
}

#endregion
