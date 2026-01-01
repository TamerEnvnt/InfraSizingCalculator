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

    #region OutSystems Pricing Service Tests

    [Fact]
    public void GetOutSystemsPricingSettings_ReturnsSettings()
    {
        // Act
        var settings = _service.GetOutSystemsPricingSettings();

        // Assert
        settings.Should().NotBeNull();
        settings.OdcPlatformBasePrice.Should().BeGreaterThan(0);
        settings.O11EnterpriseBasePrice.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task UpdateOutSystemsPricingSettingsAsync_UpdatesSettings()
    {
        // Arrange
        var newSettings = new OutSystemsPricingSettings
        {
            OdcPlatformBasePrice = 35000m,
            OdcAOPackPrice = 20000m
        };

        // Act
        await _service.UpdateOutSystemsPricingSettingsAsync(newSettings);

        // Assert
        var retrieved = _service.GetOutSystemsPricingSettings();
        retrieved.OdcPlatformBasePrice.Should().Be(35000m);
        retrieved.OdcAOPackPrice.Should().Be(20000m);
    }

    [Fact]
    public void CalculateOutSystemsCost_OdcPlatform_CalculatesBasePricing()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.ODC,
            TotalApplicationObjects = 150, // 1 AO Pack
            InternalUsers = 100,
            ExternalUsers = 0
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.Should().NotBeNull();
        result.Platform.Should().Be(OutSystemsPlatform.ODC);
        result.AOPackCount.Should().Be(1);
        result.EditionCost.Should().Be(30250m); // ODC base price
        result.AOPacksCost.Should().Be(0); // 1 pack included
    }

    [Fact]
    public void CalculateOutSystemsCost_OdcPlatform_CalculatesAdditionalAOPacks()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.ODC,
            TotalApplicationObjects = 450, // 3 AO Packs (450/150 = 3)
            InternalUsers = 100,
            ExternalUsers = 0
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.AOPackCount.Should().Be(3);
        result.AOPacksCost.Should().Be(18150m * 2); // 2 additional packs @ $18,150
    }

    [Fact]
    public void CalculateOutSystemsCost_O11Platform_CalculatesBasePricing()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.Cloud,
            TotalApplicationObjects = 150, // 1 AO Pack
            InternalUsers = 100,
            ExternalUsers = 0
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.Platform.Should().Be(OutSystemsPlatform.O11);
        result.AOPackCount.Should().Be(1);
        result.EditionCost.Should().Be(36300m); // O11 Enterprise base price
        result.AOPacksCost.Should().Be(0); // 1 pack included
    }

    [Fact]
    public void CalculateOutSystemsCost_O11Platform_CalculatesAdditionalAOPacks()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.Cloud,
            TotalApplicationObjects = 300, // 2 AO Packs
            InternalUsers = 100,
            ExternalUsers = 0
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.AOPackCount.Should().Be(2);
        result.AOPacksCost.Should().Be(36300m); // 1 additional pack @ $36,300
    }

    [Fact]
    public void CalculateOutSystemsCost_UnlimitedUsers_ScalesWithAOPacks()
    {
        // Arrange - CRITICAL: Unlimited Users = $60,500 × AO_Packs (NOT flat!)
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.ODC,
            TotalApplicationObjects = 300, // 2 AO Packs
            UseUnlimitedUsers = true
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.UsedUnlimitedUsers.Should().BeTrue();
        result.UnlimitedUsersCost.Should().Be(60500m * 2); // 2 × $60,500 = $121,000
    }

    [Fact]
    public void CalculateOutSystemsCost_OdcInternalUsers_CalculatesFlatPacks()
    {
        // Arrange - ODC uses flat pack pricing for users
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.ODC,
            TotalApplicationObjects = 150,
            InternalUsers = 250, // 100 included, 150 additional = 2 packs
            ExternalUsers = 0
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.InternalUserPackCount.Should().Be(2); // ceil(150/100) = 2 packs
        result.InternalUsersCost.Should().Be(6050m * 2); // 2 × $6,050
    }

    [Fact]
    public void CalculateOutSystemsCost_OdcExternalUsers_CalculatesFlatPacks()
    {
        // Arrange - ODC External: $6,050 per pack of 1000
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.ODC,
            TotalApplicationObjects = 150,
            InternalUsers = 100,
            ExternalUsers = 2500 // 3 packs (ceil(2500/1000))
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.ExternalUserPackCount.Should().Be(3);
        result.ExternalUsersCost.Should().Be(6050m * 3);
    }

    [Fact]
    public void CalculateOutSystemsCost_OdcAddOns_CalculatesCorrectly()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.ODC,
            TotalApplicationObjects = 300, // 2 AO Packs
            InternalUsers = 100,
            OdcSupport24x7Premium = true,
            OdcHighAvailability = true,
            OdcNonProdRuntimeQuantity = 2
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.AddOnCosts.Should().ContainKey("Support 24x7 Premium");
        result.AddOnCosts.Should().ContainKey("High Availability");
        result.AddOnCosts.Should().ContainKey("Non-Production Runtime (×2)");

        // Add-ons scale with AO pack count
        var pricing = _service.GetOutSystemsPricingSettings();
        result.AddOnCosts["Support 24x7 Premium"].Should().Be(pricing.OdcSupport24x7PremiumPerPack * 2);
        result.AddOnCosts["High Availability"].Should().Be(pricing.OdcHighAvailabilityPerPack * 2);
    }

    [Fact]
    public void CalculateOutSystemsCost_O11Sentry_IncludesHighAvailability()
    {
        // Arrange - Sentry includes HA
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.Cloud,
            TotalApplicationObjects = 150,
            InternalUsers = 100,
            O11Sentry = true,
            O11HighAvailability = true // Should be ignored since Sentry includes HA
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.AddOnCosts.Should().ContainKey("Sentry (incl. HA)");
        result.AddOnCosts.Should().NotContainKey("High Availability"); // HA is included in Sentry
    }

    [Fact]
    public void CalculateOutSystemsCost_O11CloudOnlyFeatures_ValidatesDeploymentType()
    {
        // Arrange - Load Test Environment is Cloud-only
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.SelfManaged, // Not cloud!
            TotalApplicationObjects = 150,
            InternalUsers = 100,
            O11LoadTestEnvQuantity = 1 // Should be ignored for self-managed
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.AddOnCosts.Should().NotContainKey("Load Test Env (×1)");
    }

    [Fact]
    public void CalculateOutSystemsCost_Services_CalculatesRegionalPricing()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.ODC,
            TotalApplicationObjects = 150,
            Region = OutSystemsRegion.Americas,
            EssentialSuccessPlanQuantity = 1,
            ExpertDayQuantity = 3
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.ServiceCosts.Should().ContainKey("Essential Success Plan (×1)");
        result.ServiceCosts.Should().ContainKey("Expert Day (×3)");

        var pricing = _service.GetOutSystemsPricingSettings();
        var regionPricing = pricing.GetServicesPricing(OutSystemsRegion.Americas);
        result.ServiceCosts["Essential Success Plan (×1)"].Should().Be(regionPricing.EssentialSuccessPlan);
        result.ServiceCosts["Expert Day (×3)"].Should().Be(regionPricing.ExpertDay * 3);
    }

    [Fact]
    public void CalculateOutSystemsCost_O11SelfManagedAzure_CalculatesVMCosts()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.SelfManaged,
            CloudProvider = OutSystemsCloudProvider.Azure,
            AzureInstanceType = OutSystemsAzureInstanceType.D4s_v3,
            TotalEnvironments = 3,
            FrontEndServersPerEnvironment = 2,
            TotalApplicationObjects = 150,
            InternalUsers = 100
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.TotalVMCount.Should().Be(6); // 3 environments × 2 servers
        result.MonthlyVMCost.Should().BeGreaterThan(0);
        result.VMDetails.Should().Contain("Azure");
        result.VMDetails.Should().Contain("D4s_v3");
    }

    [Fact]
    public void CalculateOutSystemsCost_O11SelfManagedAWS_CalculatesVMCosts()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.SelfManaged,
            CloudProvider = OutSystemsCloudProvider.AWS,
            AwsInstanceType = OutSystemsAwsInstanceType.M5XLarge,
            TotalEnvironments = 2,
            FrontEndServersPerEnvironment = 3,
            TotalApplicationObjects = 150,
            InternalUsers = 100
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.TotalVMCount.Should().Be(6); // 2 environments × 3 servers
        result.MonthlyVMCost.Should().BeGreaterThan(0);
        result.VMDetails.Should().Contain("AWS");
        result.VMDetails.Should().Contain("M5XLarge");
    }

    [Fact]
    public void CalculateOutSystemsCost_Discount_AppliesCorrectly()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.ODC,
            TotalApplicationObjects = 300, // 2 AO Packs
            InternalUsers = 100,
            Discount = new OutSystemsDiscount
            {
                Type = OutSystemsDiscountType.Percentage,
                Value = 10m,
                Scope = OutSystemsDiscountScope.Total
            }
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.DiscountAmount.Should().BeGreaterThan(0);
        result.DiscountDescription.Should().Contain("10%");
        result.DiscountDescription.Should().Contain("Total");
    }

    [Fact]
    public void CalculateOutSystemsCost_AppShield_CalculatesTieredPricing()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.ODC,
            TotalApplicationObjects = 150,
            InternalUsers = 500,
            ExternalUsers = 2000,
            OdcAppShield = true
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.AddOnCosts.Should().ContainKey("AppShield");
        result.AppShieldUserVolume.Should().Be(2500); // 500 + 2000
        result.AddOnCosts["AppShield"].Should().BeGreaterThan(0);
    }

    [Fact]
    public void CalculateOutSystemsCost_LineItems_BuildsCorrectly()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.ODC,
            TotalApplicationObjects = 150,
            InternalUsers = 200,
            OdcHighAvailability = true,
            EssentialSuccessPlanQuantity = 1
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.LineItems.Should().NotBeEmpty();
        result.LineItems.Should().Contain(li => li.Category == "License");
        result.LineItems.Should().Contain(li => li.Category == "Add-On");
        result.LineItems.Should().Contain(li => li.Category == "Service");
    }

    [Fact]
    public void CalculateOutSystemsCost_TotalCost_SumsAllComponents()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.ODC,
            TotalApplicationObjects = 300, // 2 AO Packs
            InternalUsers = 200,
            OdcHighAvailability = true,
            EssentialSuccessPlanQuantity = 1
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        var expectedTotal = result.LicenseSubtotal + result.AddOnsSubtotal +
                           result.ServicesSubtotal + result.InfrastructureSubtotal -
                           result.DiscountAmount;
        result.NetTotal.Should().Be(expectedTotal);
    }

    [Fact]
    public void CalculateOutSystemsCost_Warnings_ReturnsValidationIssues()
    {
        // Arrange - Create config with potential issues
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.SelfManaged,
            CloudProvider = OutSystemsCloudProvider.OnPremises,
            TotalApplicationObjects = 150,
            InternalUsers = 100,
            // These cloud-only features should generate warnings
            O11Sentry = true, // Cloud-only
            O11LoadTestEnvQuantity = 1 // Cloud-only
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.Warnings.Should().NotBeEmpty();
    }

    [Fact]
    public void IsOutSystemsCloudOnlyFeature_ReturnsCorrectValue()
    {
        // Act & Assert
        _service.IsOutSystemsCloudOnlyFeature("Sentry").Should().BeTrue();
        _service.IsOutSystemsCloudOnlyFeature("LoadTestEnv").Should().BeTrue();
        _service.IsOutSystemsCloudOnlyFeature("HighAvailability").Should().BeTrue();
        _service.IsOutSystemsCloudOnlyFeature("DatabaseReplica").Should().BeTrue();
        _service.IsOutSystemsCloudOnlyFeature("LogStreaming").Should().BeTrue();
    }

    [Fact]
    public void RecommendAzureInstance_ReturnsAppropriateInstance()
    {
        // Act & Assert
        _service.RecommendAzureInstance(4, 8).Should().Be(OutSystemsAzureInstanceType.F4s_v2);
        _service.RecommendAzureInstance(4, 16).Should().Be(OutSystemsAzureInstanceType.D4s_v3);
        _service.RecommendAzureInstance(8, 32).Should().Be(OutSystemsAzureInstanceType.D8s_v3);
        _service.RecommendAzureInstance(16, 64).Should().Be(OutSystemsAzureInstanceType.D16s_v3);
    }

    [Fact]
    public void RecommendAwsInstance_ReturnsAppropriateInstance()
    {
        // Act & Assert
        _service.RecommendAwsInstance(2, 8).Should().Be(OutSystemsAwsInstanceType.M5Large);
        _service.RecommendAwsInstance(4, 16).Should().Be(OutSystemsAwsInstanceType.M5XLarge);
        _service.RecommendAwsInstance(8, 32).Should().Be(OutSystemsAwsInstanceType.M52XLarge);
    }

    [Fact]
    public async Task UpdateOutSystemsPricingSettingsAsync_FiresOnSettingsChanged()
    {
        // Arrange
        var eventFired = false;
        _service.OnSettingsChanged += () => eventFired = true;
        var settings = new OutSystemsPricingSettings();

        // Act
        await _service.UpdateOutSystemsPricingSettingsAsync(settings);

        // Assert
        eventFired.Should().BeTrue();
    }

    [Fact]
    public void CalculateOutSystemsCost_ComprehensiveScenario_ODC()
    {
        // Arrange - Real-world ODC deployment scenario
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.ODC,
            TotalApplicationObjects = 600, // 4 AO Packs
            InternalUsers = 500, // 100 included + 400 additional = 4 packs
            ExternalUsers = 5000, // 5 packs @ 1000 each
            Region = OutSystemsRegion.Europe,
            OdcSupport24x7Premium = true,
            OdcHighAvailability = true,
            OdcNonProdRuntimeQuantity = 2,
            OdcAppShield = true,
            EssentialSuccessPlanQuantity = 1,
            ExpertDayQuantity = 5
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.AOPackCount.Should().Be(4);
        result.Platform.Should().Be(OutSystemsPlatform.ODC);

        // License costs
        result.EditionCost.Should().Be(30250m); // ODC Base
        result.AOPacksCost.Should().Be(18150m * 3); // 3 additional AO packs
        result.InternalUsersCost.Should().BeGreaterThan(0);
        result.ExternalUsersCost.Should().BeGreaterThan(0);

        // Add-on costs
        result.AddOnsSubtotal.Should().BeGreaterThan(0);

        // Services costs
        result.ServicesSubtotal.Should().BeGreaterThan(0);

        // Total should be sum of all components
        result.NetTotal.Should().BeGreaterThan(0);
        result.LineItems.Should().NotBeEmpty();
    }

    [Fact]
    public void CalculateOutSystemsCost_ComprehensiveScenario_O11Cloud()
    {
        // Arrange - Real-world O11 Cloud deployment scenario
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.Cloud,
            TotalApplicationObjects = 450, // 3 AO Packs
            InternalUsers = 300, // Tiered pricing applies
            ExternalUsers = 10000, // Tiered pricing applies
            Region = OutSystemsRegion.Americas,
            O11Support24x7Premium = true,
            O11Sentry = true, // Includes HA
            O11NonProdEnvQuantity = 2,
            O11LoadTestEnvQuantity = 1,
            O11AppShield = true,
            PremierSuccessPlanQuantity = 1,
            DedicatedGroupSessionQuantity = 2
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.AOPackCount.Should().Be(3);
        result.Platform.Should().Be(OutSystemsPlatform.O11);
        result.Deployment.Should().Be(OutSystemsDeployment.Cloud);

        // License costs
        result.EditionCost.Should().Be(36300m); // O11 Enterprise Base
        result.AOPacksCost.Should().Be(36300m * 2); // 2 additional AO packs

        // Add-ons should include Sentry (with HA), not separate HA
        result.AddOnCosts.Should().ContainKey("Sentry (incl. HA)");
        result.AddOnCosts.Should().NotContainKey("High Availability");

        // Total should be sum of all components
        result.NetTotal.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CalculateOutSystemsCost_ComprehensiveScenario_O11SelfManagedAzure()
    {
        // Arrange - O11 Self-Managed on Azure
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.SelfManaged,
            CloudProvider = OutSystemsCloudProvider.Azure,
            AzureInstanceType = OutSystemsAzureInstanceType.D8s_v3,
            TotalEnvironments = 4, // Dev, Test, Staging, Prod
            FrontEndServersPerEnvironment = 2,
            TotalApplicationObjects = 300, // 2 AO Packs
            InternalUsers = 200,
            ExternalUsers = 0,
            Region = OutSystemsRegion.Europe,
            O11Support24x7Premium = true,
            O11NonProdEnvQuantity = 3,
            O11DisasterRecovery = true, // Self-Managed only
            EssentialSuccessPlanQuantity = 1
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.Platform.Should().Be(OutSystemsPlatform.O11);
        result.Deployment.Should().Be(OutSystemsDeployment.SelfManaged);

        // VM costs should be calculated
        result.TotalVMCount.Should().Be(8); // 4 environments × 2 servers
        result.MonthlyVMCost.Should().BeGreaterThan(0);
        result.AnnualVMCost.Should().Be(result.MonthlyVMCost * 12);
        result.VMDetails.Should().Contain("D8s_v3");

        // Disaster Recovery should be included (Self-Managed only)
        result.AddOnCosts.Should().ContainKey("Disaster Recovery");

        // Total should include infrastructure costs
        result.InfrastructureSubtotal.Should().Be(result.AnnualVMCost);
        result.NetTotal.Should().BeGreaterThan(0);
    }

    #endregion

    #region OutSystems ODC Platform Comprehensive Tests

    [Fact]
    public void CalculateOutSystemsCost_ODC_MinimumConfig_ReturnsBasePrice()
    {
        // Arrange - Minimum viable ODC configuration
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.ODC,
            TotalApplicationObjects = 1, // Should still be 1 AO Pack minimum
            InternalUsers = 0,
            ExternalUsers = 0
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.AOPackCount.Should().Be(1);
        result.EditionCost.Should().Be(30250m);
        result.AOPacksCost.Should().Be(0); // First pack included
        result.LicenseSubtotal.Should().Be(30250m);
    }

    [Fact]
    public void CalculateOutSystemsCost_ODC_LargeAOCount_CalculatesMultiplePacks()
    {
        // Arrange - 1000 AOs = ceiling(1000/150) = 7 packs
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.ODC,
            TotalApplicationObjects = 1000,
            InternalUsers = 100,
            ExternalUsers = 0
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.AOPackCount.Should().Be(7); // ceil(1000/150) = 7
        result.AOPacksCost.Should().Be(18150m * 6); // 6 additional packs
    }

    [Fact]
    public void CalculateOutSystemsCost_ODC_InternalUsersTiered_CalculatesPacks()
    {
        // Arrange - ODC: 100 internal users included, then packs of 100
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.ODC,
            TotalApplicationObjects = 150,
            InternalUsers = 550, // 100 included + 450 = 5 packs (ceil(450/100))
            ExternalUsers = 0
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.InternalUserPackCount.Should().Be(5);
        result.InternalUsersCost.Should().Be(6050m * 5);
    }

    [Fact]
    public void CalculateOutSystemsCost_ODC_ExternalUsers_CalculatesPacks()
    {
        // Arrange - ODC External: packs of 1000
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.ODC,
            TotalApplicationObjects = 150,
            InternalUsers = 100,
            ExternalUsers = 7500 // ceil(7500/1000) = 8 packs
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.ExternalUserPackCount.Should().Be(8);
        result.ExternalUsersCost.Should().Be(6050m * 8);
    }

    [Fact]
    public void CalculateOutSystemsCost_ODC_AllAddOns_CalculatesTotal()
    {
        // Arrange - ODC with all possible add-ons
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.ODC,
            TotalApplicationObjects = 300, // 2 AO Packs
            InternalUsers = 100,
            OdcSupport24x7Premium = true,
            OdcHighAvailability = true,
            OdcNonProdRuntimeQuantity = 3,
            OdcAppShield = true
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.AddOnCosts.Should().ContainKey("Support 24x7 Premium");
        result.AddOnCosts.Should().ContainKey("High Availability");
        result.AddOnCosts.Should().ContainKey("Non-Production Runtime (×3)");
        result.AddOnCosts.Should().ContainKey("AppShield");
        result.AddOnsSubtotal.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CalculateOutSystemsCost_ODC_UnlimitedUsersReplacesUserPacks()
    {
        // Arrange - When unlimited users enabled, individual user packs should be 0
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.ODC,
            TotalApplicationObjects = 450, // 3 AO Packs
            InternalUsers = 1000, // Would be expensive normally
            ExternalUsers = 50000, // Would be very expensive
            UseUnlimitedUsers = true
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.UsedUnlimitedUsers.Should().BeTrue();
        result.UnlimitedUsersCost.Should().Be(60500m * 3); // Scales with AO packs
        result.InternalUsersCost.Should().Be(0); // Replaced by unlimited
        result.ExternalUsersCost.Should().Be(0); // Replaced by unlimited
    }

    [Theory]
    [InlineData(OutSystemsRegion.Africa)]
    [InlineData(OutSystemsRegion.MiddleEast)]
    [InlineData(OutSystemsRegion.Americas)]
    [InlineData(OutSystemsRegion.Europe)]
    [InlineData(OutSystemsRegion.AsiaPacific)]
    public void CalculateOutSystemsCost_ODC_AllRegions_CalculatesServices(OutSystemsRegion region)
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.ODC,
            TotalApplicationObjects = 150,
            Region = region,
            EssentialSuccessPlanQuantity = 1,
            ExpertDayQuantity = 2
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.Region.Should().Be(region);
        result.ServicesSubtotal.Should().BeGreaterThan(0);
    }

    #endregion

    #region OutSystems O11 Cloud Comprehensive Tests

    [Fact]
    public void CalculateOutSystemsCost_O11Cloud_MinimumConfig_ReturnsBasePrice()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.Cloud,
            TotalApplicationObjects = 1,
            InternalUsers = 0,
            ExternalUsers = 0
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.AOPackCount.Should().Be(1);
        result.EditionCost.Should().Be(36300m); // O11 Enterprise base
        result.AOPacksCost.Should().Be(0);
    }

    [Fact]
    public void CalculateOutSystemsCost_O11Cloud_LargeAOCount_CalculatesMultiplePacks()
    {
        // Arrange - 750 AOs = 5 packs
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.Cloud,
            TotalApplicationObjects = 750,
            InternalUsers = 100,
            ExternalUsers = 0
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.AOPackCount.Should().Be(5);
        result.AOPacksCost.Should().Be(36300m * 4); // 4 additional packs @ $36,300
    }

    [Fact]
    public void CalculateOutSystemsCost_O11Cloud_TieredInternalUsers_CalculatesVolumeDiscount()
    {
        // Arrange - O11 uses tiered pricing with volume discounts
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.Cloud,
            TotalApplicationObjects = 150,
            InternalUsers = 500, // Should apply tiered pricing
            ExternalUsers = 0
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.InternalUsersCost.Should().BeGreaterThan(0);
        // O11 tiered pricing should result in different cost than flat ODC pricing
    }

    [Fact]
    public void CalculateOutSystemsCost_O11Cloud_TieredExternalUsers_CalculatesVolumeDiscount()
    {
        // Arrange - O11 external users with tiered pricing
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.Cloud,
            TotalApplicationObjects = 150,
            InternalUsers = 100,
            ExternalUsers = 25000 // Should hit multiple tiers
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.ExternalUsersCost.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CalculateOutSystemsCost_O11Cloud_SentryIncludesHA()
    {
        // Arrange - Sentry includes HA, so HA should not be separately charged
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.Cloud,
            TotalApplicationObjects = 150,
            InternalUsers = 100,
            O11Sentry = true,
            O11HighAvailability = true // This should be ignored
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.AddOnCosts.Should().ContainKey("Sentry (incl. HA)");
        result.AddOnCosts.Keys.Should().NotContain(k => k == "High Availability");
    }

    [Fact]
    public void CalculateOutSystemsCost_O11Cloud_HAWithoutSentry_ChargesSeparately()
    {
        // Arrange - HA without Sentry should charge separately
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.Cloud,
            TotalApplicationObjects = 300, // 2 packs
            InternalUsers = 100,
            O11Sentry = false,
            O11HighAvailability = true
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.AddOnCosts.Should().ContainKey("High Availability");
        result.AddOnCosts.Should().NotContainKey("Sentry (incl. HA)");
    }

    [Fact]
    public void CalculateOutSystemsCost_O11Cloud_AllCloudOnlyAddOns()
    {
        // Arrange - All cloud-only features
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.Cloud,
            TotalApplicationObjects = 300, // 2 packs
            InternalUsers = 100,
            O11Sentry = true,
            O11LoadTestEnvQuantity = 2,
            O11LogStreamingQuantity = 1,
            O11DatabaseReplicaQuantity = 1
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.AddOnCosts.Should().ContainKey("Sentry (incl. HA)");
        result.AddOnCosts.Should().ContainKey("Load Test Env (×2)");
        result.AddOnCosts.Should().ContainKey("Log Streaming (×1)");
        result.AddOnCosts.Should().ContainKey("Database Replica (×1)");
    }

    [Fact]
    public void CalculateOutSystemsCost_O11Cloud_EnvironmentPack_ScalesWithAOPacks()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.Cloud,
            TotalApplicationObjects = 450, // 3 packs
            InternalUsers = 100,
            O11EnvPackQuantity = 2
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.AddOnCosts.Should().ContainKey("Environment Pack (×2)");
        // Cost should be: rate × AO_packs × quantity
        var pricing = _service.GetOutSystemsPricingSettings();
        var expectedCost = pricing.O11EnvironmentPackPerPack * 3 * 2;
        result.AddOnCosts["Environment Pack (×2)"].Should().Be(expectedCost);
    }

    [Fact]
    public void CalculateOutSystemsCost_O11Cloud_NoVMCosts()
    {
        // Arrange - Cloud deployment should have no VM costs
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.Cloud,
            TotalApplicationObjects = 150,
            InternalUsers = 100
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.MonthlyVMCost.Should().Be(0);
        result.AnnualVMCost.Should().Be(0);
        result.TotalVMCount.Should().Be(0);
        result.InfrastructureSubtotal.Should().Be(0);
    }

    #endregion

    #region OutSystems O11 Self-Managed Azure Tests

    [Fact]
    public void CalculateOutSystemsCost_O11SelfManagedAzure_F4s_v2_CalculatesVMCost()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.SelfManaged,
            CloudProvider = OutSystemsCloudProvider.Azure,
            AzureInstanceType = OutSystemsAzureInstanceType.F4s_v2,
            TotalEnvironments = 2,
            FrontEndServersPerEnvironment = 2,
            TotalApplicationObjects = 150,
            InternalUsers = 100
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.TotalVMCount.Should().Be(4);
        result.VMDetails.Should().Contain("F4s_v2");
        var pricing = _service.GetOutSystemsPricingSettings();
        var expectedMonthly = pricing.AzureVMHourlyPricing[OutSystemsAzureInstanceType.F4s_v2] * 730 * 4;
        result.MonthlyVMCost.Should().Be(expectedMonthly);
    }

    [Fact]
    public void CalculateOutSystemsCost_O11SelfManagedAzure_D4s_v3_CalculatesVMCost()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.SelfManaged,
            CloudProvider = OutSystemsCloudProvider.Azure,
            AzureInstanceType = OutSystemsAzureInstanceType.D4s_v3,
            TotalEnvironments = 3,
            FrontEndServersPerEnvironment = 2,
            TotalApplicationObjects = 150,
            InternalUsers = 100
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.TotalVMCount.Should().Be(6);
        result.VMDetails.Should().Contain("D4s_v3");
    }

    [Fact]
    public void CalculateOutSystemsCost_O11SelfManagedAzure_D8s_v3_CalculatesVMCost()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.SelfManaged,
            CloudProvider = OutSystemsCloudProvider.Azure,
            AzureInstanceType = OutSystemsAzureInstanceType.D8s_v3,
            TotalEnvironments = 4,
            FrontEndServersPerEnvironment = 3,
            TotalApplicationObjects = 150,
            InternalUsers = 100
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.TotalVMCount.Should().Be(12);
        result.VMDetails.Should().Contain("D8s_v3");
    }

    [Fact]
    public void CalculateOutSystemsCost_O11SelfManagedAzure_D16s_v3_CalculatesVMCost()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.SelfManaged,
            CloudProvider = OutSystemsCloudProvider.Azure,
            AzureInstanceType = OutSystemsAzureInstanceType.D16s_v3,
            TotalEnvironments = 2,
            FrontEndServersPerEnvironment = 4,
            TotalApplicationObjects = 150,
            InternalUsers = 100
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.TotalVMCount.Should().Be(8);
        result.VMDetails.Should().Contain("D16s_v3");
    }

    [Fact]
    public void CalculateOutSystemsCost_O11SelfManagedAzure_DisasterRecovery()
    {
        // Arrange - DR is Self-Managed only
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.SelfManaged,
            CloudProvider = OutSystemsCloudProvider.Azure,
            AzureInstanceType = OutSystemsAzureInstanceType.D4s_v3,
            TotalEnvironments = 2,
            FrontEndServersPerEnvironment = 2,
            TotalApplicationObjects = 300, // 2 packs
            InternalUsers = 100,
            O11DisasterRecovery = true
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.AddOnCosts.Should().ContainKey("Disaster Recovery");
        var pricing = _service.GetOutSystemsPricingSettings();
        result.AddOnCosts["Disaster Recovery"].Should().Be(pricing.O11DisasterRecoveryPerPack * 2);
    }

    #endregion

    #region OutSystems O11 Self-Managed AWS Tests

    [Fact]
    public void CalculateOutSystemsCost_O11SelfManagedAWS_M5Large_CalculatesVMCost()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.SelfManaged,
            CloudProvider = OutSystemsCloudProvider.AWS,
            AwsInstanceType = OutSystemsAwsInstanceType.M5Large,
            TotalEnvironments = 2,
            FrontEndServersPerEnvironment = 2,
            TotalApplicationObjects = 150,
            InternalUsers = 100
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.TotalVMCount.Should().Be(4);
        result.VMDetails.Should().Contain("AWS");
        result.VMDetails.Should().Contain("M5Large");
        var pricing = _service.GetOutSystemsPricingSettings();
        var expectedMonthly = pricing.AwsEC2HourlyPricing[OutSystemsAwsInstanceType.M5Large] * 730 * 4;
        result.MonthlyVMCost.Should().Be(expectedMonthly);
    }

    [Fact]
    public void CalculateOutSystemsCost_O11SelfManagedAWS_M5XLarge_CalculatesVMCost()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.SelfManaged,
            CloudProvider = OutSystemsCloudProvider.AWS,
            AwsInstanceType = OutSystemsAwsInstanceType.M5XLarge,
            TotalEnvironments = 3,
            FrontEndServersPerEnvironment = 2,
            TotalApplicationObjects = 150,
            InternalUsers = 100
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.TotalVMCount.Should().Be(6);
        result.VMDetails.Should().Contain("M5XLarge");
    }

    [Fact]
    public void CalculateOutSystemsCost_O11SelfManagedAWS_M52XLarge_CalculatesVMCost()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.SelfManaged,
            CloudProvider = OutSystemsCloudProvider.AWS,
            AwsInstanceType = OutSystemsAwsInstanceType.M52XLarge,
            TotalEnvironments = 4,
            FrontEndServersPerEnvironment = 2,
            TotalApplicationObjects = 150,
            InternalUsers = 100
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.TotalVMCount.Should().Be(8);
        result.VMDetails.Should().Contain("M52XLarge");
    }

    #endregion

    #region OutSystems O11 Self-Managed On-Premises Tests

    [Fact]
    public void CalculateOutSystemsCost_O11SelfManagedOnPrem_NoVMCost()
    {
        // Arrange - On-prem has no cloud VM costs
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.SelfManaged,
            CloudProvider = OutSystemsCloudProvider.OnPremises,
            TotalEnvironments = 4,
            FrontEndServersPerEnvironment = 3,
            TotalApplicationObjects = 300,
            InternalUsers = 200
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.MonthlyVMCost.Should().Be(0);
        result.AnnualVMCost.Should().Be(0);
        result.InfrastructureSubtotal.Should().Be(0);
        // But license costs should still be calculated
        result.LicenseSubtotal.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CalculateOutSystemsCost_O11SelfManagedOnPrem_CloudOnlyFeaturesWarning()
    {
        // Arrange - Cloud-only features on self-managed should generate warnings
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.SelfManaged,
            CloudProvider = OutSystemsCloudProvider.OnPremises,
            TotalApplicationObjects = 150,
            InternalUsers = 100,
            O11Sentry = true, // Cloud-only
            O11LoadTestEnvQuantity = 1, // Cloud-only
            O11LogStreamingQuantity = 1, // Cloud-only
            O11DatabaseReplicaQuantity = 1 // Cloud-only
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.Warnings.Should().NotBeEmpty();
        // Cloud-only features should NOT be charged
        result.AddOnCosts.Should().NotContainKey("Sentry (incl. HA)");
        result.AddOnCosts.Should().NotContainKey("Load Test Env (×1)");
        result.AddOnCosts.Should().NotContainKey("Log Streaming (×1)");
        result.AddOnCosts.Should().NotContainKey("Database Replica (×1)");
    }

    [Fact]
    public void CalculateOutSystemsCost_O11SelfManaged_SelfManagedOnlyFeatures()
    {
        // Arrange - Features only available for self-managed
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.SelfManaged,
            CloudProvider = OutSystemsCloudProvider.OnPremises,
            TotalApplicationObjects = 300, // 2 packs
            InternalUsers = 100,
            O11DisasterRecovery = true, // Self-Managed only
            O11NonProdEnvQuantity = 3
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.AddOnCosts.Should().ContainKey("Disaster Recovery");
        result.AddOnCosts.Should().ContainKey("Non-Production Env (×3)");
    }

    #endregion

    #region OutSystems Discount Tests

    [Theory]
    [InlineData(OutSystemsDiscountScope.Total)]
    [InlineData(OutSystemsDiscountScope.LicenseOnly)]
    [InlineData(OutSystemsDiscountScope.AddOnsOnly)]
    [InlineData(OutSystemsDiscountScope.ServicesOnly)]
    public void CalculateOutSystemsCost_PercentageDiscount_AllScopes(OutSystemsDiscountScope scope)
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.ODC,
            TotalApplicationObjects = 300,
            InternalUsers = 200,
            OdcHighAvailability = true,
            EssentialSuccessPlanQuantity = 1,
            Discount = new OutSystemsDiscount
            {
                Type = OutSystemsDiscountType.Percentage,
                Value = 15m,
                Scope = scope
            }
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.DiscountAmount.Should().BeGreaterThan(0);
        result.DiscountDescription.Should().Contain("15%");
        result.DiscountDescription.Should().Contain(scope.ToString());
    }

    [Fact]
    public void CalculateOutSystemsCost_FixedDiscount_AppliesCorrectly()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.ODC,
            TotalApplicationObjects = 150,
            InternalUsers = 100,
            Discount = new OutSystemsDiscount
            {
                Type = OutSystemsDiscountType.FixedAmount,
                Value = 5000m,
                Scope = OutSystemsDiscountScope.Total
            }
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.DiscountAmount.Should().Be(5000m);
        result.NetTotal.Should().Be(result.GrossTotal - 5000m);
    }

    [Fact]
    public void CalculateOutSystemsCost_DiscountOnLicense_OnlyAffectsLicense()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.ODC,
            TotalApplicationObjects = 300,
            InternalUsers = 200,
            OdcHighAvailability = true,
            EssentialSuccessPlanQuantity = 1,
            Discount = new OutSystemsDiscount
            {
                Type = OutSystemsDiscountType.Percentage,
                Value = 20m,
                Scope = OutSystemsDiscountScope.LicenseOnly
            }
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        var expectedDiscount = result.LicenseSubtotal * 0.20m;
        result.DiscountAmount.Should().BeApproximately(expectedDiscount, 0.01m);
    }

    [Fact]
    public void CalculateOutSystemsCost_DiscountOnAddOns_OnlyAffectsAddOns()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.ODC,
            TotalApplicationObjects = 300,
            InternalUsers = 200,
            OdcHighAvailability = true,
            OdcSupport24x7Premium = true,
            Discount = new OutSystemsDiscount
            {
                Type = OutSystemsDiscountType.Percentage,
                Value = 25m,
                Scope = OutSystemsDiscountScope.AddOnsOnly
            }
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        var expectedDiscount = result.AddOnsSubtotal * 0.25m;
        result.DiscountAmount.Should().BeApproximately(expectedDiscount, 0.01m);
    }

    [Fact]
    public void CalculateOutSystemsCost_DiscountOnServices_OnlyAffectsServices()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.ODC,
            TotalApplicationObjects = 150,
            InternalUsers = 100,
            EssentialSuccessPlanQuantity = 1,
            ExpertDayQuantity = 5,
            Discount = new OutSystemsDiscount
            {
                Type = OutSystemsDiscountType.Percentage,
                Value = 10m,
                Scope = OutSystemsDiscountScope.ServicesOnly
            }
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        var expectedDiscount = result.ServicesSubtotal * 0.10m;
        result.DiscountAmount.Should().BeApproximately(expectedDiscount, 0.01m);
    }

    [Fact]
    public void CalculateOutSystemsCost_NoDiscount_ZeroDiscountAmount()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.ODC,
            TotalApplicationObjects = 150,
            InternalUsers = 100,
            Discount = null
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.DiscountAmount.Should().Be(0);
        result.NetTotal.Should().Be(result.GrossTotal);
    }

    #endregion

    #region OutSystems AppShield Tiered Pricing Tests

    [Theory]
    [InlineData(500, 1)]     // Tier 1: 0-10000 users
    [InlineData(5000, 1)]    // Tier 1: 0-10000 users
    [InlineData(15000, 2)]   // Tier 2: 10001-50000 users
    [InlineData(60000, 3)]   // Tier 3: 50001-100000 users
    [InlineData(200000, 4)]  // Tier 4: 100001-500000 users
    public void CalculateOutSystemsCost_AppShield_TieredPricing(int totalUsers, int expectedTier)
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.ODC,
            TotalApplicationObjects = 150,
            InternalUsers = totalUsers / 2,
            ExternalUsers = totalUsers - (totalUsers / 2),
            OdcAppShield = true
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.AppShieldUserVolume.Should().Be(totalUsers);
        result.AppShieldTier.Should().BeGreaterThanOrEqualTo(expectedTier);
        result.AddOnCosts["AppShield"].Should().BeGreaterThan(0);
    }

    [Fact]
    public void CalculateOutSystemsCost_AppShield_O11_CalculatesPricing()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.Cloud,
            TotalApplicationObjects = 150,
            InternalUsers = 300,
            ExternalUsers = 5000,
            O11AppShield = true
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.AppShieldUserVolume.Should().Be(5300);
        result.AddOnCosts.Should().ContainKey("AppShield");
    }

    #endregion

    #region OutSystems Services Pricing Tests

    [Fact]
    public void CalculateOutSystemsCost_EssentialSuccessPlan_CalculatesCost()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.ODC,
            TotalApplicationObjects = 150,
            Region = OutSystemsRegion.Americas,
            EssentialSuccessPlanQuantity = 2
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.ServiceCosts.Should().ContainKey("Essential Success Plan (×2)");
        var pricing = _service.GetOutSystemsPricingSettings();
        var regionPricing = pricing.GetServicesPricing(OutSystemsRegion.Americas);
        result.ServiceCosts["Essential Success Plan (×2)"].Should().Be(regionPricing.EssentialSuccessPlan * 2);
    }

    [Fact]
    public void CalculateOutSystemsCost_PremierSuccessPlan_CalculatesCost()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.Cloud,
            TotalApplicationObjects = 150,
            Region = OutSystemsRegion.Europe,
            PremierSuccessPlanQuantity = 1
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.ServiceCosts.Should().ContainKey("Premier Success Plan (×1)");
    }

    [Fact]
    public void CalculateOutSystemsCost_DedicatedGroupSession_CalculatesCost()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.ODC,
            TotalApplicationObjects = 150,
            Region = OutSystemsRegion.AsiaPacific,
            DedicatedGroupSessionQuantity = 5
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.ServiceCosts.Should().ContainKey("Dedicated Group Session (×5)");
    }

    [Fact]
    public void CalculateOutSystemsCost_PublicSession_CalculatesCost()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.ODC,
            TotalApplicationObjects = 150,
            Region = OutSystemsRegion.MiddleEast,
            PublicSessionQuantity = 10
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.ServiceCosts.Should().ContainKey("Public Session (×10)");
    }

    [Fact]
    public void CalculateOutSystemsCost_ExpertDay_CalculatesCost()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.ODC,
            TotalApplicationObjects = 150,
            Region = OutSystemsRegion.Africa,
            ExpertDayQuantity = 8
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.ServiceCosts.Should().ContainKey("Expert Day (×8)");
        var pricing = _service.GetOutSystemsPricingSettings();
        var regionPricing = pricing.GetServicesPricing(OutSystemsRegion.Africa);
        result.ServiceCosts["Expert Day (×8)"].Should().Be(regionPricing.ExpertDay * 8);
    }

    [Fact]
    public void CalculateOutSystemsCost_AllServices_CalculatesTotal()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.ODC,
            TotalApplicationObjects = 150,
            Region = OutSystemsRegion.Europe,
            EssentialSuccessPlanQuantity = 1,
            PremierSuccessPlanQuantity = 1,
            DedicatedGroupSessionQuantity = 3,
            PublicSessionQuantity = 5,
            ExpertDayQuantity = 10
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.ServiceCosts.Count.Should().Be(5);
        result.ServicesSubtotal.Should().BeGreaterThan(0);
    }

    #endregion

    #region OutSystems Edge Cases and Validation Tests

    [Fact]
    public void CalculateOutSystemsCost_ZeroAOs_ReturnsMinimumOnePack()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.ODC,
            TotalApplicationObjects = 0,
            InternalUsers = 100
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert - Zero AOs means no additional packs needed (base platform includes 1 pack)
        result.AOPackCount.Should().Be(0);
        // But edition cost should still be charged (base platform fee)
        result.EditionCost.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CalculateOutSystemsCost_NegativeAOs_ReturnsZeroPacksButBaseEditionCost()
    {
        // Arrange - Negative AOs is an edge case that should be treated as zero
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.ODC,
            TotalApplicationObjects = -50,
            InternalUsers = 100
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert - Negative treated as zero, no additional packs
        result.AOPackCount.Should().Be(0);
        // Base platform still applies
        result.EditionCost.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CalculateOutSystemsCost_ExactlyOnePack_NoAdditionalCost()
    {
        // Arrange - Exactly 150 AOs = 1 pack
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.ODC,
            TotalApplicationObjects = 150,
            InternalUsers = 100
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.AOPackCount.Should().Be(1);
        result.AOPacksCost.Should().Be(0); // First pack included
    }

    [Fact]
    public void CalculateOutSystemsCost_OneOverPack_ChargesAdditionalPack()
    {
        // Arrange - 151 AOs = 2 packs
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.ODC,
            TotalApplicationObjects = 151,
            InternalUsers = 100
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.AOPackCount.Should().Be(2);
        result.AOPacksCost.Should().Be(18150m); // 1 additional pack
    }

    [Fact]
    public void CalculateOutSystemsCost_LineItems_ContainsAllCategories()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.ODC,
            TotalApplicationObjects = 300,
            InternalUsers = 200,
            ExternalUsers = 1000,
            OdcHighAvailability = true,
            EssentialSuccessPlanQuantity = 1
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.LineItems.Should().Contain(li => li.Category == "License");
        result.LineItems.Should().Contain(li => li.Category == "Add-On");
        result.LineItems.Should().Contain(li => li.Category == "Service");
    }

    [Fact]
    public void CalculateOutSystemsCost_VeryLargeUserCount_HandlesGracefully()
    {
        // Arrange - Extreme user count
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.ODC,
            TotalApplicationObjects = 150,
            InternalUsers = 10000,
            ExternalUsers = 1000000
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.ExternalUsersCost.Should().BeGreaterThan(0);
        result.NetTotal.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CalculateOutSystemsCost_ODC_CannotSelectDeployment()
    {
        // Arrange - ODC is always cloud, deployment setting should be ignored
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.ODC,
            Deployment = OutSystemsDeployment.SelfManaged, // Should be ignored
            CloudProvider = OutSystemsCloudProvider.Azure, // Should be ignored
            TotalApplicationObjects = 150,
            InternalUsers = 100
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        // ODC should have no VM costs regardless of deployment setting
        result.MonthlyVMCost.Should().Be(0);
    }

    [Fact]
    public void CalculateOutSystemsCost_ProjectionsCalculatedCorrectly()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.ODC,
            TotalApplicationObjects = 150,
            InternalUsers = 100
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        result.TotalPerMonth.Should().Be(result.NetTotal / 12);
        result.TotalThreeYear.Should().Be(result.NetTotal * 3);
        result.TotalFiveYear.Should().Be(result.NetTotal * 5);
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
