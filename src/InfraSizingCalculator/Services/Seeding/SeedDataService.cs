using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using InfraSizingCalculator.Data;
using InfraSizingCalculator.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace InfraSizingCalculator.Services.Seeding;

/// <summary>
/// Manages database seeding with version tracking.
/// Uses hash-based comparison for efficient startup checks.
/// </summary>
public class SeedDataService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SeedDataService> _logger;
    private readonly string _seedDataPath;

    /// <summary>
    /// Current seed data version. Increment when making breaking changes.
    /// </summary>
    public const string CurrentVersion = "1.0.0";

    public SeedDataService(
        IServiceProvider serviceProvider,
        ILogger<SeedDataService> logger,
        IWebHostEnvironment environment)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _seedDataPath = Path.Combine(environment.ContentRootPath, "Data", "Seeds");
    }

    /// <summary>
    /// Checks if seeding is required and performs it if necessary.
    /// This is an O(1) operation - only one DB query to check hash.
    /// </summary>
    public async Task EnsureSeedDataAsync(CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<InfraSizingDbContext>();

        // Ensure database exists
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        // Calculate current seed data hash
        var currentHash = await CalculateSeedHashAsync();

        // Check if we need to seed (single query)
        var metadata = await dbContext.Set<SeedMetadataEntity>()
            .FirstOrDefaultAsync(cancellationToken);

        if (metadata == null)
        {
            _logger.LogInformation("No seed metadata found - performing initial seed");
            await PerformSeedingAsync(dbContext, currentHash, isInitial: true, cancellationToken);
        }
        else if (metadata.SeedHash != currentHash)
        {
            _logger.LogInformation(
                "Seed data changed (hash mismatch). Previous: {OldHash}, Current: {NewHash}",
                metadata.SeedHash[..8] + "...",
                currentHash[..8] + "...");
            await PerformSeedingAsync(dbContext, currentHash, isInitial: false, cancellationToken);
        }
        else
        {
            _logger.LogDebug("Seed data unchanged - skipping seeding");
        }
    }

    /// <summary>
    /// Forces a re-seed regardless of hash comparison.
    /// </summary>
    public async Task ForceSeedAsync(CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<InfraSizingDbContext>();

        var currentHash = await CalculateSeedHashAsync();
        await PerformSeedingAsync(dbContext, currentHash, isInitial: false, cancellationToken);
    }

    /// <summary>
    /// Gets the current seed metadata.
    /// </summary>
    public async Task<SeedMetadataEntity?> GetMetadataAsync(CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<InfraSizingDbContext>();
        return await dbContext.Set<SeedMetadataEntity>().FirstOrDefaultAsync(cancellationToken);
    }

    private async Task PerformSeedingAsync(
        InfraSizingDbContext dbContext,
        string currentHash,
        bool isInitial,
        CancellationToken cancellationToken)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

            // Seed each entity type
            await SeedApplicationSettingsAsync(dbContext, cancellationToken);
            await SeedCloudApiCredentialsAsync(dbContext, cancellationToken);
            await SeedOnPremPricingAsync(dbContext, cancellationToken);
            await SeedMendixPricingAsync(dbContext, cancellationToken);
            await SeedOutSystemsPricingAsync(dbContext, cancellationToken);
            await SeedDistributionsAsync(dbContext, cancellationToken);
            await SeedTechnologiesAsync(dbContext, cancellationToken);
            await SeedInfoTypesAsync(dbContext, cancellationToken);

            // Update metadata
            await UpdateMetadataAsync(dbContext, currentHash, isInitial, cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            stopwatch.Stop();
            _logger.LogInformation(
                "Database seeding completed in {ElapsedMs}ms",
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database seeding failed");
            throw;
        }
    }

    private async Task SeedApplicationSettingsAsync(InfraSizingDbContext dbContext, CancellationToken ct)
    {
        var existing = await dbContext.ApplicationSettings.FirstOrDefaultAsync(ct);
        if (existing != null) return; // Don't overwrite user settings

        var seedData = await LoadSeedFileAsync<ApplicationSettingsSeed>("application-settings.json");

        dbContext.ApplicationSettings.Add(new ApplicationSettingsEntity
        {
            Id = 1,
            IncludePricingInResults = seedData?.Data?.IncludePricingInResults ?? false,
            DefaultCurrency = seedData?.Data?.DefaultCurrency ?? "USD",
            PricingCacheDurationHours = seedData?.Data?.PricingCacheDurationHours ?? 24,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(ct);
        _logger.LogDebug("Seeded application settings");
    }

    private async Task SeedCloudApiCredentialsAsync(InfraSizingDbContext dbContext, CancellationToken ct)
    {
        var existing = await dbContext.CloudApiCredentials.AnyAsync(ct);
        if (existing) return;

        var seedData = await LoadSeedFileAsync<CloudApiCredentialsSeed>("cloud-api-credentials.json");
        var providers = seedData?.Data ?? new[]
        {
            new CloudProviderSeed { ProviderName = "AWS", IsConfigured = false },
            new CloudProviderSeed { ProviderName = "Azure", IsConfigured = false },
            new CloudProviderSeed { ProviderName = "GCP", IsConfigured = false },
            new CloudProviderSeed { ProviderName = "Oracle", IsConfigured = false }
        };

        var id = 1;
        foreach (var provider in providers)
        {
            dbContext.CloudApiCredentials.Add(new CloudApiCredentialsEntity
            {
                Id = id++,
                ProviderName = provider.ProviderName,
                IsConfigured = provider.IsConfigured
            });
        }

        await dbContext.SaveChangesAsync(ct);
        _logger.LogDebug("Seeded {Count} cloud API credentials", providers.Length);
    }

    private async Task SeedOnPremPricingAsync(InfraSizingDbContext dbContext, CancellationToken ct)
    {
        var existing = await dbContext.OnPremPricing.FirstOrDefaultAsync(ct);
        if (existing != null) return;

        // OnPremPricingEntity has defaults in the entity itself
        dbContext.OnPremPricing.Add(new OnPremPricingEntity
        {
            Id = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(ct);
        _logger.LogDebug("Seeded on-prem pricing defaults");
    }

    private async Task SeedMendixPricingAsync(InfraSizingDbContext dbContext, CancellationToken ct)
    {
        var existing = await dbContext.MendixPricing.FirstOrDefaultAsync(ct);
        if (existing != null) return;

        // MendixPricingEntity has defaults in the entity itself
        dbContext.MendixPricing.Add(new MendixPricingEntity
        {
            Id = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(ct);
        _logger.LogDebug("Seeded Mendix pricing defaults");
    }

    private async Task SeedOutSystemsPricingAsync(InfraSizingDbContext dbContext, CancellationToken ct)
    {
        var existing = await dbContext.OutSystemsPricing.FirstOrDefaultAsync(ct);
        if (existing != null) return;

        // OutSystemsPricingEntity has defaults in the entity itself
        dbContext.OutSystemsPricing.Add(new OutSystemsPricingEntity
        {
            Id = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(ct);
        _logger.LogDebug("Seeded OutSystems pricing defaults");
    }

    private async Task SeedDistributionsAsync(InfraSizingDbContext dbContext, CancellationToken ct)
    {
        var seedData = await LoadSeedFileAsync<DistributionsSeed>("distributions.json");
        if (seedData?.Data == null || seedData.Data.Length == 0)
        {
            _logger.LogWarning("No distribution seed data found");
            return;
        }

        // Clear existing and reseed (distributions are config, not user data)
        var existingCount = await dbContext.DistributionConfigs.CountAsync(ct);
        if (existingCount > 0)
        {
            dbContext.DistributionConfigs.RemoveRange(dbContext.DistributionConfigs);
            await dbContext.SaveChangesAsync(ct);
        }

        var id = 1;
        foreach (var dist in seedData.Data)
        {
            dbContext.DistributionConfigs.Add(new DistributionConfigEntity
            {
                Id = id++,
                DistributionKey = dist.Key,
                Name = dist.Name,
                Vendor = dist.Vendor,
                Icon = dist.Icon,
                BrandColor = dist.BrandColor,
                Tags = dist.Tags,
                SortOrder = dist.SortOrder,
                HasInfraNodes = dist.HasInfraNodes,
                HasManagedControlPlane = dist.HasManagedControlPlane,
                ProdControlPlaneCpu = dist.ProdControlPlane?.Cpu ?? 0,
                ProdControlPlaneRam = dist.ProdControlPlane?.Ram ?? 0,
                ProdControlPlaneDisk = dist.ProdControlPlane?.Disk ?? 0,
                NonProdControlPlaneCpu = dist.NonProdControlPlane?.Cpu ?? 0,
                NonProdControlPlaneRam = dist.NonProdControlPlane?.Ram ?? 0,
                NonProdControlPlaneDisk = dist.NonProdControlPlane?.Disk ?? 0,
                ProdWorkerCpu = dist.ProdWorker?.Cpu ?? 0,
                ProdWorkerRam = dist.ProdWorker?.Ram ?? 0,
                ProdWorkerDisk = dist.ProdWorker?.Disk ?? 0,
                NonProdWorkerCpu = dist.NonProdWorker?.Cpu ?? 0,
                NonProdWorkerRam = dist.NonProdWorker?.Ram ?? 0,
                NonProdWorkerDisk = dist.NonProdWorker?.Disk ?? 0,
                ProdInfraCpu = dist.ProdInfra?.Cpu ?? 0,
                ProdInfraRam = dist.ProdInfra?.Ram ?? 0,
                ProdInfraDisk = dist.ProdInfra?.Disk ?? 0,
                NonProdInfraCpu = dist.NonProdInfra?.Cpu ?? 0,
                NonProdInfraRam = dist.NonProdInfra?.Ram ?? 0,
                NonProdInfraDisk = dist.NonProdInfra?.Disk ?? 0,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await dbContext.SaveChangesAsync(ct);
        _logger.LogDebug("Seeded {Count} distribution configurations", seedData.Data.Length);
    }

    private async Task SeedTechnologiesAsync(InfraSizingDbContext dbContext, CancellationToken ct)
    {
        var seedData = await LoadSeedFileAsync<TechnologiesSeed>("technologies.json");
        if (seedData?.Data == null || seedData.Data.Length == 0)
        {
            _logger.LogWarning("No technology seed data found");
            return;
        }

        // Clear existing and reseed (technologies are config, not user data)
        var existingCount = await dbContext.TechnologyConfigs.CountAsync(ct);
        if (existingCount > 0)
        {
            dbContext.TechnologyConfigs.RemoveRange(dbContext.TechnologyConfigs);
            await dbContext.SaveChangesAsync(ct);
        }

        var id = 1;
        foreach (var tech in seedData.Data)
        {
            dbContext.TechnologyConfigs.Add(new TechnologyConfigEntity
            {
                Id = id++,
                TechnologyKey = tech.Key,
                Name = tech.Name,
                Vendor = tech.Vendor,
                Icon = tech.Icon,
                BrandColor = tech.BrandColor,
                Category = tech.Category,
                SortOrder = tech.SortOrder,
                CpuMultiplier = tech.CpuMultiplier,
                MemoryMultiplier = tech.MemoryMultiplier,
                SmallAppCpu = tech.SmallApp?.Cpu ?? 2,
                SmallAppRam = tech.SmallApp?.Ram ?? 4,
                MediumAppCpu = tech.MediumApp?.Cpu ?? 4,
                MediumAppRam = tech.MediumApp?.Ram ?? 8,
                LargeAppCpu = tech.LargeApp?.Cpu ?? 8,
                LargeAppRam = tech.LargeApp?.Ram ?? 16,
                XLargeAppCpu = tech.XlargeApp?.Cpu ?? 16,
                XLargeAppRam = tech.XlargeApp?.Ram ?? 32,
                ShortDescription = tech.ShortDescription,
                PerformanceNotes = tech.PerformanceNotes,
                UseCases = tech.UseCases,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await dbContext.SaveChangesAsync(ct);
        _logger.LogDebug("Seeded {Count} technology configurations", seedData.Data.Length);
    }

    private async Task SeedInfoTypesAsync(InfraSizingDbContext dbContext, CancellationToken ct)
    {
        var seedData = await LoadSeedFileAsync<InfoTypesSeed>("info-types.json");
        if (seedData?.Data == null || seedData.Data.Length == 0)
        {
            _logger.LogWarning("No info type seed data found");
            return;
        }

        // Clear existing and reseed (info types are config, not user data)
        var existingCount = await dbContext.InfoTypeContents.CountAsync(ct);
        if (existingCount > 0)
        {
            dbContext.InfoTypeContents.RemoveRange(dbContext.InfoTypeContents);
            await dbContext.SaveChangesAsync(ct);
        }

        var id = 1;
        foreach (var info in seedData.Data)
        {
            dbContext.InfoTypeContents.Add(new InfoTypeContentEntity
            {
                Id = id++,
                InfoTypeKey = info.Key,
                Title = info.Title,
                ContentHtml = info.ContentHtml,
                Category = info.Category,
                SortOrder = info.SortOrder,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await dbContext.SaveChangesAsync(ct);
        _logger.LogDebug("Seeded {Count} info type configurations", seedData.Data.Length);
    }

    private async Task UpdateMetadataAsync(
        InfraSizingDbContext dbContext,
        string currentHash,
        bool isInitial,
        CancellationToken ct)
    {
        var metadata = await dbContext.Set<SeedMetadataEntity>().FirstOrDefaultAsync(ct);

        if (metadata == null)
        {
            dbContext.Set<SeedMetadataEntity>().Add(new SeedMetadataEntity
            {
                Id = 1,
                SeedHash = currentHash,
                Version = CurrentVersion,
                LastSeededAt = DateTime.UtcNow,
                SeedCount = 1
            });
        }
        else
        {
            metadata.SeedHash = currentHash;
            metadata.Version = CurrentVersion;
            metadata.LastSeededAt = DateTime.UtcNow;
            metadata.SeedCount++;
        }

        await dbContext.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Calculates a SHA256 hash of all seed data files.
    /// </summary>
    private async Task<string> CalculateSeedHashAsync()
    {
        var files = Directory.GetFiles(_seedDataPath, "*.json")
            .OrderBy(f => f)
            .ToList();

        using var sha256 = SHA256.Create();
        var combinedContent = new StringBuilder();

        foreach (var file in files)
        {
            var content = await File.ReadAllTextAsync(file);
            combinedContent.Append(content);
        }

        // Also include the version in hash calculation
        combinedContent.Append(CurrentVersion);

        var bytes = Encoding.UTF8.GetBytes(combinedContent.ToString());
        var hashBytes = sha256.ComputeHash(bytes);

        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    private async Task<T?> LoadSeedFileAsync<T>(string fileName) where T : class
    {
        var filePath = Path.Combine(_seedDataPath, fileName);

        if (!File.Exists(filePath))
        {
            _logger.LogWarning("Seed file not found: {FilePath}", filePath);
            return null;
        }

        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load seed file: {FilePath}", filePath);
            return null;
        }
    }

    #region Seed Data Models

    private class ApplicationSettingsSeed
    {
        public ApplicationSettingsData? Data { get; set; }
    }

    private class ApplicationSettingsData
    {
        public bool IncludePricingInResults { get; set; }
        public string DefaultCurrency { get; set; } = "USD";
        public int PricingCacheDurationHours { get; set; } = 24;
    }

    private class CloudApiCredentialsSeed
    {
        public CloudProviderSeed[]? Data { get; set; }
    }

    private class CloudProviderSeed
    {
        public string ProviderName { get; set; } = string.Empty;
        public bool IsConfigured { get; set; }
    }

    private class DistributionsSeed
    {
        public DistributionSeed[]? Data { get; set; }
    }

    private class DistributionSeed
    {
        public string Key { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Vendor { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string BrandColor { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool HasInfraNodes { get; set; }
        public bool HasManagedControlPlane { get; set; }
        public NodeSpecsSeed? ProdControlPlane { get; set; }
        public NodeSpecsSeed? NonProdControlPlane { get; set; }
        public NodeSpecsSeed? ProdWorker { get; set; }
        public NodeSpecsSeed? NonProdWorker { get; set; }
        public NodeSpecsSeed? ProdInfra { get; set; }
        public NodeSpecsSeed? NonProdInfra { get; set; }
    }

    private class NodeSpecsSeed
    {
        public int Cpu { get; set; }
        public int Ram { get; set; }
        public int Disk { get; set; }
    }

    private class TechnologiesSeed
    {
        public TechnologySeed[]? Data { get; set; }
    }

    private class TechnologySeed
    {
        public string Key { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Vendor { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string BrandColor { get; set; } = string.Empty;
        public string Category { get; set; } = "native";
        public int SortOrder { get; set; }
        public decimal CpuMultiplier { get; set; } = 1.0m;
        public decimal MemoryMultiplier { get; set; } = 1.0m;
        public AppSizeSeed? SmallApp { get; set; }
        public AppSizeSeed? MediumApp { get; set; }
        public AppSizeSeed? LargeApp { get; set; }
        public AppSizeSeed? XlargeApp { get; set; }
        public string ShortDescription { get; set; } = string.Empty;
        public string PerformanceNotes { get; set; } = string.Empty;
        public string UseCases { get; set; } = string.Empty;
    }

    private class AppSizeSeed
    {
        public int Cpu { get; set; }
        public int Ram { get; set; }
    }

    private class InfoTypesSeed
    {
        public InfoTypeSeed[]? Data { get; set; }
    }

    private class InfoTypeSeed
    {
        public string Key { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = "general";
        public int SortOrder { get; set; }
        public string ContentHtml { get; set; } = string.Empty;
    }

    #endregion
}
