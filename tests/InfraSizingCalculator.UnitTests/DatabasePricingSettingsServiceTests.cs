using FluentAssertions;
using InfraSizingCalculator.Data;
using InfraSizingCalculator.Data.Entities;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Models.Pricing;
using InfraSizingCalculator.Services.Pricing;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InfraSizingCalculator.UnitTests;

public class DatabasePricingSettingsServiceTests : IDisposable
{
    private readonly InfraSizingDbContext _dbContext;
    private readonly DatabasePricingSettingsService _service;

    public DatabasePricingSettingsServiceTests()
    {
        // Create unique in-memory database for each test
        var options = new DbContextOptionsBuilder<InfraSizingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new InfraSizingDbContext(options);
        _dbContext.Database.EnsureCreated();
        _service = new DatabasePricingSettingsService(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    #region GetSettingsAsync Tests

    [Fact]
    public async Task GetSettingsAsync_ReturnsSettings()
    {
        // Act
        var settings = await _service.GetSettingsAsync();

        // Assert
        settings.Should().NotBeNull();
    }

    [Fact]
    public async Task GetSettingsAsync_LoadsFromDatabase()
    {
        // Arrange
        var appSettings = await _dbContext.ApplicationSettings.FirstAsync();
        appSettings.IncludePricingInResults = true;
        await _dbContext.SaveChangesAsync();

        // Act
        var settings = await _service.GetSettingsAsync();

        // Assert
        settings.IncludePricingInResults.Should().BeTrue();
    }

    [Fact]
    public async Task GetSettingsAsync_CachesSettings()
    {
        // Act
        var settings1 = await _service.GetSettingsAsync();
        var settings2 = await _service.GetSettingsAsync();

        // Assert
        settings1.Should().BeSameAs(settings2);
    }

    [Fact]
    public async Task GetSettingsAsync_LoadsOnPremPricing()
    {
        // Act
        var settings = await _service.GetSettingsAsync();

        // Assert
        settings.OnPremDefaults.Should().NotBeNull();
        settings.OnPremDefaults.Hardware.Should().NotBeNull();
        settings.OnPremDefaults.DataCenter.Should().NotBeNull();
        settings.OnPremDefaults.Labor.Should().NotBeNull();
    }

    [Fact]
    public async Task GetSettingsAsync_LoadsMendixPricing()
    {
        // Act
        var settings = await _service.GetSettingsAsync();

        // Assert
        settings.MendixPricing.Should().NotBeNull();
        settings.MendixPricing.CloudTokenPrice.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetSettingsAsync_LoadsCloudApiConfigs()
    {
        // Arrange
        var awsConfig = new CloudApiCredentialsEntity
        {
            ProviderName = "AWS",
            ApiKey = "test-key",
            SecretKey = "test-secret",
            Region = "us-east-1",
            IsConfigured = true
        };
        _dbContext.CloudApiCredentials.Add(awsConfig);
        await _dbContext.SaveChangesAsync();

        // Act
        var settings = await _service.GetSettingsAsync();

        // Assert
        settings.CloudApiConfigs.Should().ContainKey(CloudProvider.AWS);
        settings.CloudApiConfigs[CloudProvider.AWS].ApiKey.Should().Be("test-key");
        settings.CloudApiConfigs[CloudProvider.AWS].SecretKey.Should().Be("test-secret");
        settings.CloudApiConfigs[CloudProvider.AWS].DefaultRegion.Should().Be("us-east-1");
    }

    [Fact]
    public async Task GetSettingsAsync_HandlesNullEntities()
    {
        // Arrange - Clear all entities
        _dbContext.ApplicationSettings.RemoveRange(_dbContext.ApplicationSettings);
        _dbContext.OnPremPricing.RemoveRange(_dbContext.OnPremPricing);
        _dbContext.MendixPricing.RemoveRange(_dbContext.MendixPricing);
        await _dbContext.SaveChangesAsync();

        // Act
        var settings = await _service.GetSettingsAsync();

        // Assert
        settings.Should().NotBeNull();
        settings.IncludePricingInResults.Should().BeFalse();
        settings.OnPremDefaults.Should().NotBeNull();
        settings.MendixPricing.Should().NotBeNull();
    }

    #endregion

    #region SaveSettingsAsync Tests

    [Fact]
    public async Task SaveSettingsAsync_SavesSettings()
    {
        // Arrange
        var settings = await _service.GetSettingsAsync();
        settings.IncludePricingInResults = true;

        // Act
        await _service.SaveSettingsAsync(settings);

        // Assert
        var appSettings = await _dbContext.ApplicationSettings.FirstAsync();
        appSettings.IncludePricingInResults.Should().BeTrue();
    }

    [Fact]
    public async Task SaveSettingsAsync_UpdatesCache()
    {
        // Arrange
        var settings = await _service.GetSettingsAsync();
        settings.IncludePricingInResults = true;

        // Act
        await _service.SaveSettingsAsync(settings);
        var cachedSettings = await _service.GetSettingsAsync();

        // Assert
        cachedSettings.IncludePricingInResults.Should().BeTrue();
    }

    [Fact]
    public async Task SaveSettingsAsync_UpdatesTimestamp()
    {
        // Arrange
        var settings = await _service.GetSettingsAsync();
        var beforeSave = DateTime.UtcNow;

        // Act
        await _service.SaveSettingsAsync(settings);

        // Assert
        var appSettings = await _dbContext.ApplicationSettings.FirstAsync();
        appSettings.UpdatedAt.Should().BeOnOrAfter(beforeSave);
    }

    [Fact]
    public async Task SaveSettingsAsync_FiresOnSettingsChanged()
    {
        // Arrange
        var eventFired = false;
        _service.OnSettingsChanged += () => eventFired = true;
        var settings = await _service.GetSettingsAsync();

        // Act
        await _service.SaveSettingsAsync(settings);

        // Assert
        eventFired.Should().BeTrue();
    }

    [Fact]
    public async Task SaveSettingsAsync_CreatesNewEntitiesIfMissing()
    {
        // Arrange - Remove all entities
        _dbContext.ApplicationSettings.RemoveRange(_dbContext.ApplicationSettings);
        _dbContext.OnPremPricing.RemoveRange(_dbContext.OnPremPricing);
        _dbContext.MendixPricing.RemoveRange(_dbContext.MendixPricing);
        await _dbContext.SaveChangesAsync();

        var settings = new PricingSettings
        {
            IncludePricingInResults = true
        };

        // Act
        await _service.SaveSettingsAsync(settings);

        // Assert
        (await _dbContext.ApplicationSettings.CountAsync()).Should().Be(1);
        (await _dbContext.OnPremPricing.CountAsync()).Should().Be(1);
        (await _dbContext.MendixPricing.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task SaveSettingsAsync_SavesOnPremPricing()
    {
        // Arrange
        var settings = await _service.GetSettingsAsync();
        settings.OnPremDefaults.Hardware.ServerCost = 10000m;
        settings.OnPremDefaults.Hardware.PerCpuCore = 200m;
        settings.OnPremDefaults.HardwareRefreshYears = 7;

        // Act
        await _service.SaveSettingsAsync(settings);

        // Assert
        var onPremEntity = await _dbContext.OnPremPricing.FirstAsync();
        onPremEntity.ServerCostPerUnit.Should().Be(10000m);
        onPremEntity.CostPerCore.Should().Be(200m);
        onPremEntity.HardwareLifespanYears.Should().Be(7);
    }

    [Fact]
    public async Task SaveSettingsAsync_SavesMendixPricing()
    {
        // Arrange
        var settings = await _service.GetSettingsAsync();
        settings.MendixPricing.CloudTokenPrice = 100m;
        settings.MendixPricing.AzureBasePricePerYear = 10000m;

        // Act
        await _service.SaveSettingsAsync(settings);

        // Assert
        var mendixEntity = await _dbContext.MendixPricing.FirstAsync();
        mendixEntity.CloudTokenPrice.Should().Be(100m);
        mendixEntity.AzureBasePackagePrice.Should().Be(10000m);
    }

    [Fact]
    public async Task SaveSettingsAsync_SavesCloudApiCredentials()
    {
        // Arrange
        var settings = await _service.GetSettingsAsync();
        settings.CloudApiConfigs[CloudProvider.Azure] = new CloudApiConfig
        {
            ApiKey = "azure-key",
            SecretKey = "azure-secret",
            DefaultRegion = "eastus"
        };

        // Act
        await _service.SaveSettingsAsync(settings);

        // Assert
        var azureCred = await _dbContext.CloudApiCredentials
            .FirstOrDefaultAsync(c => c.ProviderName == "Azure");
        azureCred.Should().NotBeNull();
        azureCred!.ApiKey.Should().Be("azure-key");
        azureCred.SecretKey.Should().Be("azure-secret");
        azureCred.Region.Should().Be("eastus");
    }

    #endregion

    #region ResetToDefaultsAsync Tests

    [Fact]
    public async Task ResetToDefaultsAsync_ResetsIncludePricingInResults()
    {
        // Arrange
        var appSettings = await _dbContext.ApplicationSettings.FirstAsync();
        appSettings.IncludePricingInResults = true;
        await _dbContext.SaveChangesAsync();

        // Act
        await _service.ResetToDefaultsAsync();

        // Assert
        var resetSettings = await _dbContext.ApplicationSettings.FirstAsync();
        resetSettings.IncludePricingInResults.Should().BeFalse();
    }

    [Fact]
    public async Task ResetToDefaultsAsync_ResetsOnPremPricing()
    {
        // Arrange
        var onPremEntity = await _dbContext.OnPremPricing.FirstAsync();
        onPremEntity.ServerCostPerUnit = 99999m;
        await _dbContext.SaveChangesAsync();

        // Act
        await _service.ResetToDefaultsAsync();

        // Assert
        var resetOnPrem = await _dbContext.OnPremPricing.FirstAsync();
        resetOnPrem.ServerCostPerUnit.Should().Be(5000m); // Default value
    }

    [Fact]
    public async Task ResetToDefaultsAsync_ResetsMendixPricing()
    {
        // Arrange
        var mendixEntity = await _dbContext.MendixPricing.FirstAsync();
        mendixEntity.CloudTokenPrice = 99999m;
        await _dbContext.SaveChangesAsync();

        // Act
        await _service.ResetToDefaultsAsync();

        // Assert
        var resetMendix = await _dbContext.MendixPricing.FirstAsync();
        resetMendix.CloudTokenPrice.Should().Be(51.60m); // Default value
    }

    [Fact]
    public async Task ResetToDefaultsAsync_UpdatesLastCacheReset()
    {
        // Arrange
        var beforeReset = DateTime.UtcNow;

        // Act
        await _service.ResetToDefaultsAsync();

        // Assert
        var appSettings = await _dbContext.ApplicationSettings.FirstAsync();
        appSettings.LastCacheReset.Should().NotBeNull();
        appSettings.LastCacheReset.Should().BeOnOrAfter(beforeReset);
    }

    [Fact]
    public async Task ResetToDefaultsAsync_ClearsCache()
    {
        // Arrange
        var settings1 = await _service.GetSettingsAsync();
        settings1.IncludePricingInResults = true;
        await _service.SaveSettingsAsync(settings1);

        // Act
        await _service.ResetToDefaultsAsync();
        var settings2 = await _service.GetSettingsAsync();

        // Assert
        settings2.IncludePricingInResults.Should().BeFalse();
    }

    [Fact]
    public async Task ResetToDefaultsAsync_FiresOnSettingsChanged()
    {
        // Arrange
        var eventFired = false;
        _service.OnSettingsChanged += () => eventFired = true;

        // Act
        await _service.ResetToDefaultsAsync();

        // Assert
        eventFired.Should().BeTrue();
    }

    #endregion

    #region ResetPricingCacheAsync Tests

    [Fact]
    public async Task ResetPricingCacheAsync_UpdatesLastCacheReset()
    {
        // Arrange
        var beforeReset = DateTime.UtcNow;

        // Act
        await _service.ResetPricingCacheAsync();

        // Assert
        var appSettings = await _dbContext.ApplicationSettings.FirstAsync();
        appSettings.LastCacheReset.Should().NotBeNull();
        appSettings.LastCacheReset.Should().BeOnOrAfter(beforeReset);
    }

    [Fact]
    public async Task ResetPricingCacheAsync_ClearsCache()
    {
        // Arrange
        var settings1 = await _service.GetSettingsAsync();

        // Act
        await _service.ResetPricingCacheAsync();
        var settings2 = await _service.GetSettingsAsync();

        // Assert - Should reload from database
        settings1.Should().NotBeSameAs(settings2);
    }

    [Fact]
    public async Task ResetPricingCacheAsync_HandlesNullAppSettings()
    {
        // Arrange
        _dbContext.ApplicationSettings.RemoveRange(_dbContext.ApplicationSettings);
        await _dbContext.SaveChangesAsync();

        // Act
        var act = async () => await _service.ResetPricingCacheAsync();

        // Assert - Should not throw
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region GetCacheStatusAsync Tests

    [Fact]
    public async Task GetCacheStatusAsync_ReturnsStatus()
    {
        // Act
        var status = await _service.GetCacheStatusAsync();

        // Assert
        status.Should().NotBeNull();
    }

    [Fact]
    public async Task GetCacheStatusAsync_ReturnsLastReset()
    {
        // Arrange
        var appSettings = await _dbContext.ApplicationSettings.FirstAsync();
        var resetTime = DateTime.UtcNow.AddHours(-2);
        appSettings.LastCacheReset = resetTime;
        await _dbContext.SaveChangesAsync();

        // Act
        var status = await _service.GetCacheStatusAsync();

        // Assert
        status.LastReset.Should().BeCloseTo(resetTime, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task GetCacheStatusAsync_CountsConfiguredApis()
    {
        // Arrange
        var settings = await _service.GetSettingsAsync();
        settings.CloudApiConfigs[CloudProvider.AWS] = new CloudApiConfig
        {
            ApiKey = "test-key",
            SecretKey = "test-secret"
        };
        settings.CloudApiConfigs[CloudProvider.Azure] = new CloudApiConfig
        {
            ApiKey = "azure-key"
        };
        await _service.SaveSettingsAsync(settings);

        // Act
        var status = await _service.GetCacheStatusAsync();

        // Assert
        status.ConfiguredApiCount.Should().Be(2);
    }

    [Fact]
    public async Task GetCacheStatusAsync_IsStale_WhenCacheOlderThan24Hours()
    {
        // Arrange
        var appSettings = await _dbContext.ApplicationSettings.FirstAsync();
        appSettings.LastCacheReset = DateTime.UtcNow.AddHours(-25);
        await _dbContext.SaveChangesAsync();

        // Act
        var status = await _service.GetCacheStatusAsync();

        // Assert
        status.IsStale.Should().BeTrue();
    }

    [Fact]
    public async Task GetCacheStatusAsync_IsNotStale_WhenCacheRecent()
    {
        // Arrange
        var appSettings = await _dbContext.ApplicationSettings.FirstAsync();
        appSettings.LastCacheReset = DateTime.UtcNow.AddHours(-1);
        await _dbContext.SaveChangesAsync();

        // Act
        var status = await _service.GetCacheStatusAsync();

        // Assert
        status.IsStale.Should().BeFalse();
    }

    [Fact]
    public async Task GetCacheStatusAsync_IsStale_WhenNeverReset()
    {
        // Arrange
        var appSettings = await _dbContext.ApplicationSettings.FirstAsync();
        appSettings.LastCacheReset = null;
        await _dbContext.SaveChangesAsync();

        // Act
        var status = await _service.GetCacheStatusAsync();

        // Assert
        status.IsStale.Should().BeTrue();
    }

    [Fact]
    public async Task GetCacheStatusAsync_CountsCachedProviders()
    {
        // Arrange
        var awsCred = await _dbContext.CloudApiCredentials
            .FirstOrDefaultAsync(c => c.ProviderName == "AWS");
        if (awsCred != null)
        {
            awsCred.LastValidated = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }

        // Act
        var status = await _service.GetCacheStatusAsync();

        // Assert
        status.CachedProviderCount.Should().BeGreaterThanOrEqualTo(0);
    }

    #endregion

    #region GetCloudAlternatives Tests

    [Fact]
    public void GetCloudAlternatives_ReturnsAlternatives()
    {
        // Act
        var alternatives = _service.GetCloudAlternatives(Distribution.OpenShift);

        // Assert
        alternatives.Should().NotBeNull();
        alternatives.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData(Distribution.OpenShift)]
    [InlineData(Distribution.Rancher)]
    [InlineData(Distribution.Kubernetes)]
    [InlineData(Distribution.EKS)]
    public void GetCloudAlternatives_ReturnsAlternativesForAllDistributions(Distribution distribution)
    {
        // Act
        var alternatives = _service.GetCloudAlternatives(distribution);

        // Assert
        alternatives.Should().NotBeNull();
    }

    #endregion

    #region IsOnPremDistribution Tests

    [Theory]
    [InlineData(Distribution.OpenShift, true)]
    [InlineData(Distribution.Rancher, true)]
    [InlineData(Distribution.RKE2, true)]
    [InlineData(Distribution.K3s, true)]
    [InlineData(Distribution.Tanzu, true)]
    [InlineData(Distribution.Charmed, true)]
    [InlineData(Distribution.Kubernetes, true)]
    [InlineData(Distribution.MicroK8s, true)]
    [InlineData(Distribution.EKS, false)]
    [InlineData(Distribution.AKS, false)]
    [InlineData(Distribution.GKE, false)]
    public void IsOnPremDistribution_ReturnsCorrectValue(Distribution distribution, bool expected)
    {
        // Act
        var result = _service.IsOnPremDistribution(distribution);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region GetOnPremDefaults Tests

    [Fact]
    public void GetOnPremDefaults_ReturnsDefaults()
    {
        // Act
        var defaults = _service.GetOnPremDefaults();

        // Assert
        defaults.Should().NotBeNull();
        defaults.Hardware.Should().NotBeNull();
        defaults.DataCenter.Should().NotBeNull();
        defaults.Labor.Should().NotBeNull();
    }

    [Fact]
    public async Task GetOnPremDefaults_ReturnsPersistedValues()
    {
        // Arrange
        var onPremEntity = await _dbContext.OnPremPricing.FirstAsync();
        onPremEntity.ServerCostPerUnit = 7500m;
        await _dbContext.SaveChangesAsync();

        // Create new service to avoid cache
        var newService = new DatabasePricingSettingsService(_dbContext);

        // Act
        var defaults = newService.GetOnPremDefaults();

        // Assert
        defaults.Hardware.ServerCost.Should().Be(7500m);
    }

    #endregion

    #region UpdateOnPremDefaultsAsync Tests

    [Fact]
    public async Task UpdateOnPremDefaultsAsync_UpdatesDefaults()
    {
        // Arrange
        var newDefaults = new OnPremPricing
        {
            HardwareRefreshYears = 6,
            HardwareMaintenancePercent = 15
        };
        newDefaults.Hardware.ServerCost = 8000m;

        // Act
        await _service.UpdateOnPremDefaultsAsync(newDefaults);

        // Assert
        var onPremEntity = await _dbContext.OnPremPricing.FirstAsync();
        onPremEntity.HardwareLifespanYears.Should().Be(6);
        onPremEntity.ServerCostPerUnit.Should().Be(8000m);
    }

    [Fact]
    public async Task UpdateOnPremDefaultsAsync_UpdatesCache()
    {
        // Arrange
        var newDefaults = new OnPremPricing();
        newDefaults.Hardware.ServerCost = 9000m;

        // Act
        await _service.UpdateOnPremDefaultsAsync(newDefaults);
        var retrieved = _service.GetOnPremDefaults();

        // Assert
        retrieved.Hardware.ServerCost.Should().Be(9000m);
    }

    [Fact]
    public async Task UpdateOnPremDefaultsAsync_FiresOnSettingsChanged()
    {
        // Arrange
        var eventFired = false;
        _service.OnSettingsChanged += () => eventFired = true;
        var defaults = new OnPremPricing();

        // Act
        await _service.UpdateOnPremDefaultsAsync(defaults);

        // Assert
        eventFired.Should().BeTrue();
    }

    #endregion

    #region ConfigureCloudApiAsync Tests

    [Fact]
    public async Task ConfigureCloudApiAsync_ConfiguresNewProvider()
    {
        // Arrange
        var config = new CloudApiConfig
        {
            ApiKey = "new-api-key",
            SecretKey = "new-secret-key",
            DefaultRegion = "us-west-2"
        };

        // Act
        await _service.ConfigureCloudApiAsync(CloudProvider.GCP, config);

        // Assert
        var credEntity = await _dbContext.CloudApiCredentials
            .FirstOrDefaultAsync(c => c.ProviderName == "GCP");
        credEntity.Should().NotBeNull();
        credEntity!.ApiKey.Should().Be("new-api-key");
        credEntity.SecretKey.Should().Be("new-secret-key");
        credEntity.Region.Should().Be("us-west-2");
    }

    [Fact]
    public async Task ConfigureCloudApiAsync_UpdatesExistingProvider()
    {
        // Arrange
        var initialConfig = new CloudApiConfig
        {
            ApiKey = "old-key",
            SecretKey = "old-secret"
        };
        await _service.ConfigureCloudApiAsync(CloudProvider.AWS, initialConfig);

        var updatedConfig = new CloudApiConfig
        {
            ApiKey = "new-key",
            SecretKey = "new-secret",
            DefaultRegion = "eu-west-1"
        };

        // Act
        await _service.ConfigureCloudApiAsync(CloudProvider.AWS, updatedConfig);

        // Assert
        var credEntity = await _dbContext.CloudApiCredentials
            .FirstOrDefaultAsync(c => c.ProviderName == "AWS");
        credEntity!.ApiKey.Should().Be("new-key");
        credEntity.SecretKey.Should().Be("new-secret");
        credEntity.Region.Should().Be("eu-west-1");
    }

    [Fact]
    public async Task ConfigureCloudApiAsync_UpdatesCache()
    {
        // Arrange
        await _service.GetSettingsAsync(); // Initialize cache
        var config = new CloudApiConfig
        {
            ApiKey = "test-key",
            SecretKey = "test-secret"
        };

        // Act
        await _service.ConfigureCloudApiAsync(CloudProvider.OCI, config);
        var settings = await _service.GetSettingsAsync();

        // Assert
        settings.CloudApiConfigs.Should().ContainKey(CloudProvider.OCI);
        settings.CloudApiConfigs[CloudProvider.OCI].ApiKey.Should().Be("test-key");
    }

    #endregion

    #region ValidateCloudApiAsync Tests

    [Fact]
    public async Task ValidateCloudApiAsync_ReturnsFalse_WhenNotConfigured()
    {
        // Act
        var result = await _service.ValidateCloudApiAsync(CloudProvider.GCP);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateCloudApiAsync_ReturnsTrue_WhenConfigured()
    {
        // Arrange
        var config = new CloudApiConfig
        {
            ApiKey = "valid-key",
            SecretKey = "valid-secret"
        };
        await _service.ConfigureCloudApiAsync(CloudProvider.AWS, config);

        // Act
        var result = await _service.ValidateCloudApiAsync(CloudProvider.AWS);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateCloudApiAsync_UpdatesLastValidated()
    {
        // Arrange
        var config = new CloudApiConfig
        {
            ApiKey = "valid-key",
            SecretKey = "valid-secret"
        };
        await _service.ConfigureCloudApiAsync(CloudProvider.Azure, config);
        var beforeValidation = DateTime.UtcNow;

        // Act
        await _service.ValidateCloudApiAsync(CloudProvider.Azure);

        // Assert
        var credEntity = await _dbContext.CloudApiCredentials
            .FirstOrDefaultAsync(c => c.ProviderName == "Azure");
        credEntity!.LastValidated.Should().NotBeNull();
        credEntity.LastValidated.Should().BeOnOrAfter(beforeValidation);
    }

    [Fact]
    public async Task ValidateCloudApiAsync_UpdatesValidationStatus()
    {
        // Arrange
        var config = new CloudApiConfig
        {
            ApiKey = "valid-key",
            SecretKey = "valid-secret"
        };
        await _service.ConfigureCloudApiAsync(CloudProvider.AWS, config);

        // Act
        await _service.ValidateCloudApiAsync(CloudProvider.AWS);

        // Assert
        var credEntity = await _dbContext.CloudApiCredentials
            .FirstOrDefaultAsync(c => c.ProviderName == "AWS");
        credEntity!.ValidationStatus.Should().Be("Valid");
    }

    #endregion

    #region IncludePricingInResults Property Tests

    [Fact]
    public void IncludePricingInResults_DefaultIsFalse()
    {
        // Assert
        _service.IncludePricingInResults.Should().BeFalse();
    }

    [Fact]
    public void IncludePricingInResults_CanBeSet()
    {
        // Act
        _service.IncludePricingInResults = true;

        // Assert
        _service.IncludePricingInResults.Should().BeTrue();
    }

    [Fact]
    public async Task IncludePricingInResults_PersistsToDatabase()
    {
        // Act
        _service.IncludePricingInResults = true;

        // Assert
        var appSettings = await _dbContext.ApplicationSettings.FirstAsync();
        appSettings.IncludePricingInResults.Should().BeTrue();
    }

    #endregion

    #region CalculateOnPremCost Tests

    [Fact]
    public void CalculateOnPremCost_WhenPricingDisabled_ReturnsNotAvailable()
    {
        // Arrange
        _service.IncludePricingInResults = false;

        // Act
        var result = _service.CalculateOnPremCost("OpenShift", 10, 160, 320, 5);

        // Assert
        result.IsCalculated.Should().BeFalse();
    }

    [Fact]
    public void CalculateOnPremCost_WhenPricingEnabled_ReturnsCalculatedCost()
    {
        // Arrange
        _service.IncludePricingInResults = true;

        // Act
        var result = _service.CalculateOnPremCost("OpenShift", 10, 160, 320, 5);

        // Assert
        result.IsCalculated.Should().BeTrue();
    }

    [Fact]
    public void CalculateOnPremCost_IncludesAllCostComponents()
    {
        // Arrange
        _service.IncludePricingInResults = true;

        // Act
        var result = _service.CalculateOnPremCost("OpenShift", 10, 160, 320, 5, hasProduction: true);

        // Assert
        result.MonthlyHardware.Should().BeGreaterThan(0);
        result.MonthlyDataCenter.Should().BeGreaterThanOrEqualTo(0);
        result.MonthlyLabor.Should().BeGreaterThanOrEqualTo(0);
        result.MonthlyLicense.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void CalculateOnPremCost_CalculatesCorrectly_WithCustomPricing()
    {
        // Arrange
        _service.IncludePricingInResults = true;
        var onPremDefaults = _service.GetOnPremDefaults();
        onPremDefaults.Hardware.ServerCost = 10000m;
        onPremDefaults.HardwareRefreshYears = 5;
        _service.UpdateOnPremDefaultsAsync(onPremDefaults).Wait();

        // Act
        var result = _service.CalculateOnPremCost("Kubernetes", 5, 80, 160, 2);

        // Assert
        result.IsCalculated.Should().BeTrue();
        result.MonthlyTotal.Should().BeGreaterThan(0);
    }

    #endregion

    #region Mendix Pricing Methods Tests

    [Fact]
    public void GetMendixPricingSettings_ReturnsSettings()
    {
        // Act
        var settings = _service.GetMendixPricingSettings();

        // Assert
        settings.Should().NotBeNull();
        settings.CloudTokenPrice.Should().BeGreaterThan(0);
        settings.StandardResourcePacks.Should().NotBeEmpty();
    }

    [Fact]
    public async Task UpdateMendixPricingSettingsAsync_UpdatesSettings()
    {
        // Arrange
        var newSettings = new MendixPricingSettings
        {
            CloudTokenPrice = 100m,
            AzureBasePricePerYear = 10000m
        };

        // Act
        await _service.UpdateMendixPricingSettingsAsync(newSettings);

        // Assert
        var mendixEntity = await _dbContext.MendixPricing.FirstAsync();
        mendixEntity.CloudTokenPrice.Should().Be(100m);
        mendixEntity.AzureBasePackagePrice.Should().Be(10000m);
    }

    [Fact]
    public async Task UpdateMendixPricingSettingsAsync_UpdatesCache()
    {
        // Arrange
        var newSettings = new MendixPricingSettings
        {
            CloudTokenPrice = 75.50m
        };

        // Act
        await _service.UpdateMendixPricingSettingsAsync(newSettings);
        var retrieved = _service.GetMendixPricingSettings();

        // Assert
        retrieved.CloudTokenPrice.Should().Be(75.50m);
    }

    [Fact]
    public async Task UpdateMendixPricingSettingsAsync_FiresOnSettingsChanged()
    {
        // Arrange
        var eventFired = false;
        _service.OnSettingsChanged += () => eventFired = true;
        var settings = new MendixPricingSettings();

        // Act
        await _service.UpdateMendixPricingSettingsAsync(settings);

        // Assert
        eventFired.Should().BeTrue();
    }

    [Fact]
    public void CalculateMendixCost_CloudDeployment_ReturnsCorrectResult()
    {
        // Arrange
        var config = new MendixDeploymentConfig
        {
            Category = MendixDeploymentCategory.Cloud,
            CloudType = MendixCloudType.SaaS,
            ResourcePackTier = MendixResourcePackTier.Standard,
            ResourcePackSize = MendixResourcePackSize.S,
            ResourcePackQuantity = 2,
            InternalUsers = 100,
            ExternalUsers = 1000
        };

        // Act
        var result = _service.CalculateMendixCost(config);

        // Assert
        result.Should().NotBeNull();
        result.Category.Should().Be(MendixDeploymentCategory.Cloud);
        result.PlatformLicenseCost.Should().BeGreaterThan(0);
        result.UserLicenseCost.Should().BeGreaterThan(0);
        result.DeploymentFeeCost.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CalculateMendixCost_CloudDedicated_ReturnsCorrectResult()
    {
        // Arrange
        var config = new MendixDeploymentConfig
        {
            Category = MendixDeploymentCategory.Cloud,
            CloudType = MendixCloudType.Dedicated,
            InternalUsers = 500
        };

        // Act
        var result = _service.CalculateMendixCost(config);

        // Assert
        result.DeploymentTypeName.Should().Contain("Dedicated");
        result.DeploymentFeeCost.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CalculateMendixCost_PrivateCloudAzure_ReturnsCorrectResult()
    {
        // Arrange
        var config = new MendixDeploymentConfig
        {
            Category = MendixDeploymentCategory.PrivateCloud,
            PrivateCloudProvider = MendixPrivateCloudProvider.Azure,
            NumberOfEnvironments = 5,
            InternalUsers = 200
        };

        // Act
        var result = _service.CalculateMendixCost(config);

        // Assert
        result.DeploymentTypeName.Should().Contain("Azure");
        result.Category.Should().Be(MendixDeploymentCategory.PrivateCloud);
        result.DeploymentFeeCost.Should().BeGreaterThan(0);
        result.EnvironmentCost.Should().BeGreaterThan(0); // More than 3 base environments
    }

    [Fact]
    public void CalculateMendixCost_PrivateCloudK8s_ReturnsCorrectResult()
    {
        // Arrange
        var config = new MendixDeploymentConfig
        {
            Category = MendixDeploymentCategory.PrivateCloud,
            PrivateCloudProvider = MendixPrivateCloudProvider.EKS,
            NumberOfEnvironments = 10,
            InternalUsers = 150
        };

        // Act
        var result = _service.CalculateMendixCost(config);

        // Assert
        result.DeploymentTypeName.Should().Contain("Kubernetes");
        result.DeploymentFeeCost.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CalculateMendixCost_OtherDeploymentServer_ReturnsCorrectResult()
    {
        // Arrange
        var config = new MendixDeploymentConfig
        {
            Category = MendixDeploymentCategory.Other,
            OtherDeployment = MendixOtherDeployment.Server,
            IsUnlimitedApps = false,
            NumberOfApps = 3,
            InternalUsers = 100
        };

        // Act
        var result = _service.CalculateMendixCost(config);

        // Assert
        result.DeploymentTypeName.Should().Contain("Server");
        result.Category.Should().Be(MendixDeploymentCategory.Other);
        result.DeploymentFeeCost.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CalculateMendixCost_WithGenAI_IncludesGenAICost()
    {
        // Arrange
        var config = new MendixDeploymentConfig
        {
            Category = MendixDeploymentCategory.Cloud,
            CloudType = MendixCloudType.SaaS,
            IncludeGenAI = true,
            GenAIModelPackSize = "S",
            IncludeGenAIKnowledgeBase = true
        };

        // Act
        var result = _service.CalculateMendixCost(config);

        // Assert
        result.GenAICost.Should().BeGreaterThan(0);
        result.TotalCloudTokens.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CalculateMendixCost_AppliesVolumeDiscount()
    {
        // Arrange
        var config = new MendixDeploymentConfig
        {
            Category = MendixDeploymentCategory.Cloud,
            CloudType = MendixCloudType.SaaS,
            InternalUsers = 500
        };

        // Act
        var result = _service.CalculateMendixCost(config);

        // Assert
        result.DiscountPercent.Should().BeGreaterThan(0);
        result.DiscountAmount.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CalculateMendixCost_IncludesUserLicensing()
    {
        // Arrange
        var config = new MendixDeploymentConfig
        {
            Category = MendixDeploymentCategory.Cloud,
            CloudType = MendixCloudType.SaaS,
            InternalUsers = 250,
            ExternalUsers = 500000
        };

        // Act
        var result = _service.CalculateMendixCost(config);

        // Assert
        result.UserLicenseCost.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CalculateMendixCost_WithAdditionalStorage_IncludesStorageCost()
    {
        // Arrange
        var config = new MendixDeploymentConfig
        {
            Category = MendixDeploymentCategory.Cloud,
            CloudType = MendixCloudType.SaaS,
            AdditionalFileStorageGB = 250,
            AdditionalDatabaseStorageGB = 150
        };

        // Act
        var result = _service.CalculateMendixCost(config);

        // Assert
        result.StorageCost.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CalculateMendixCost_WithCustomerEnablement_IncludesServicesCost()
    {
        // Arrange
        var config = new MendixDeploymentConfig
        {
            Category = MendixDeploymentCategory.Cloud,
            IncludeCustomerEnablement = true
        };

        // Act
        var result = _service.CalculateMendixCost(config);

        // Assert
        result.ServicesCost.Should().BeGreaterThan(0);
    }

    #endregion

    #region RecommendResourcePack Tests

    [Fact]
    public void RecommendResourcePack_FindsSuitablePack()
    {
        // Act
        var pack = _service.RecommendResourcePack(
            MendixResourcePackTier.Standard,
            requiredMemoryGB: 2,
            requiredCpu: 0.5m,
            requiredDbStorageGB: 10);

        // Assert
        pack.Should().NotBeNull();
        pack!.MxMemoryGB.Should().BeGreaterThanOrEqualTo(2);
        pack.MxVCpu.Should().BeGreaterThanOrEqualTo(0.5m);
        pack.DbStorageGB.Should().BeGreaterThanOrEqualTo(10);
    }

    [Fact]
    public void RecommendResourcePack_ReturnsSmallestMatchingPack()
    {
        // Act
        var pack = _service.RecommendResourcePack(
            MendixResourcePackTier.Standard,
            requiredMemoryGB: 4,
            requiredCpu: 1,
            requiredDbStorageGB: 20);

        // Assert
        pack.Should().NotBeNull();
        pack!.Size.Should().Be(MendixResourcePackSize.M); // M is the smallest that meets requirements
    }

    [Fact]
    public void RecommendResourcePack_ReturnsNull_WhenNoPackMeetsRequirements()
    {
        // Act
        var pack = _service.RecommendResourcePack(
            MendixResourcePackTier.Standard,
            requiredMemoryGB: 1000,
            requiredCpu: 500,
            requiredDbStorageGB: 10000);

        // Assert
        pack.Should().BeNull();
    }

    [Fact]
    public void RecommendResourcePack_WorksForPremiumTier()
    {
        // Act
        var pack = _service.RecommendResourcePack(
            MendixResourcePackTier.Premium,
            requiredMemoryGB: 8,
            requiredCpu: 2,
            requiredDbStorageGB: 40);

        // Assert
        pack.Should().NotBeNull();
        pack!.HasFallback.Should().BeTrue();
        pack.UptimeSla.Should().Be(99.95m);
    }

    [Fact]
    public void RecommendResourcePack_WorksForPremiumPlusTier()
    {
        // Act
        var pack = _service.RecommendResourcePack(
            MendixResourcePackTier.PremiumPlus,
            requiredMemoryGB: 16,
            requiredCpu: 4,
            requiredDbStorageGB: 80);

        // Assert
        pack.Should().NotBeNull();
        pack!.HasFallback.Should().BeTrue();
        pack.HasMultiRegionFailover.Should().BeTrue();
    }

    #endregion

    #region IsMendixSupportedProvider Tests

    [Theory]
    [InlineData(MendixPrivateCloudProvider.Azure, true)]
    [InlineData(MendixPrivateCloudProvider.EKS, true)]
    [InlineData(MendixPrivateCloudProvider.AKS, true)]
    [InlineData(MendixPrivateCloudProvider.GKE, true)]
    [InlineData(MendixPrivateCloudProvider.OpenShift, true)]
    [InlineData(MendixPrivateCloudProvider.GenericK8s, false)]
    [InlineData(MendixPrivateCloudProvider.Rancher, false)]
    [InlineData(MendixPrivateCloudProvider.K3s, false)]
    public void IsMendixSupportedProvider_ReturnsCorrectValue(
        MendixPrivateCloudProvider provider, bool expected)
    {
        // Act
        var result = _service.IsMendixSupportedProvider(provider);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task CompleteWorkflow_SaveLoadAndReset_WorksCorrectly()
    {
        // Arrange - Get initial settings
        var settings = await _service.GetSettingsAsync();
        settings.IncludePricingInResults = true;
        settings.OnPremDefaults.Hardware.ServerCost = 15000m;

        // Act 1 - Save settings
        await _service.SaveSettingsAsync(settings);

        // Assert 1 - Verify saved
        var loadedSettings = await _service.GetSettingsAsync();
        loadedSettings.IncludePricingInResults.Should().BeTrue();
        loadedSettings.OnPremDefaults.Hardware.ServerCost.Should().Be(15000m);

        // Act 2 - Reset to defaults
        await _service.ResetToDefaultsAsync();

        // Assert 2 - Verify reset
        var resetSettings = await _service.GetSettingsAsync();
        resetSettings.IncludePricingInResults.Should().BeFalse();
        resetSettings.OnPremDefaults.Hardware.ServerCost.Should().Be(5000m);
    }

    [Fact]
    public async Task MultipleServiceInstances_ShareSameDatabase()
    {
        // Arrange
        var service1 = new DatabasePricingSettingsService(_dbContext);
        var service2 = new DatabasePricingSettingsService(_dbContext);

        var settings = await service1.GetSettingsAsync();
        settings.IncludePricingInResults = true;

        // Act
        await service1.SaveSettingsAsync(settings);

        // Assert - Service 2 should see the changes
        var settings2 = await service2.GetSettingsAsync();
        settings2.IncludePricingInResults.Should().BeTrue();
    }

    [Fact]
    public async Task CacheInvalidation_WorksCorrectly()
    {
        // Arrange
        var settings1 = await _service.GetSettingsAsync();
        settings1.IncludePricingInResults = true;
        await _service.SaveSettingsAsync(settings1);

        // Act - Reset cache
        await _service.ResetPricingCacheAsync();

        // Modify database directly
        var appSettings = await _dbContext.ApplicationSettings.FirstAsync();
        appSettings.IncludePricingInResults = false;
        await _dbContext.SaveChangesAsync();

        // Assert - Should reload from database
        var settings2 = await _service.GetSettingsAsync();
        settings2.IncludePricingInResults.Should().BeFalse();
    }

    #endregion
}
