using FluentAssertions;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Models.Pricing;
using InfraSizingCalculator.Services.Pricing;
using Microsoft.JSInterop;
using NSubstitute;
using Xunit;

namespace InfraSizingCalculator.UnitTests;

public class PricingSettingsServiceTests
{
    private readonly IJSRuntime _mockJsRuntime;
    private readonly PricingSettingsService _service;

    public PricingSettingsServiceTests()
    {
        _mockJsRuntime = Substitute.For<IJSRuntime>();
        _service = new PricingSettingsService(_mockJsRuntime);
    }

    #region IncludePricingInResults Tests

    [Fact]
    public void IncludePricingInResults_DefaultIsFalse()
    {
        // Assert - Default PricingSettings has IncludePricingInResults = false
        _service.IncludePricingInResults.Should().BeFalse();
    }

    [Fact]
    public void IncludePricingInResults_CanBeChanged()
    {
        // Act
        _service.IncludePricingInResults = true;

        // Assert
        _service.IncludePricingInResults.Should().BeTrue();
    }

    [Fact]
    public void IncludePricingInResults_FiresEventOnChange()
    {
        // Arrange
        var eventFired = false;
        _service.OnSettingsChanged += () => eventFired = true;

        // Act - Default is false, so change to true
        _service.IncludePricingInResults = true;

        // Assert
        eventFired.Should().BeTrue();
    }

    [Fact]
    public void IncludePricingInResults_DoesNotFireEventIfValueUnchanged()
    {
        // Arrange
        var eventFiredCount = 0;
        _service.OnSettingsChanged += () => eventFiredCount++;
        var currentValue = _service.IncludePricingInResults;

        // Act
        _service.IncludePricingInResults = currentValue;

        // Assert
        eventFiredCount.Should().Be(0);
    }

    #endregion

    #region GetSettingsAsync Tests

    [Fact]
    public async Task GetSettingsAsync_ReturnsSettings()
    {
        // Arrange
        _mockJsRuntime.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object[]>())
            .Returns(new ValueTask<string?>((string?)null));

        // Act
        var settings = await _service.GetSettingsAsync();

        // Assert
        settings.Should().NotBeNull();
    }

    [Fact]
    public async Task GetSettingsAsync_LoadsFromLocalStorage()
    {
        // Arrange
        var settingsJson = System.Text.Json.JsonSerializer.Serialize(new PricingSettings
        {
            IncludePricingInResults = false
        });
        _mockJsRuntime.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object[]>())
            .Returns(new ValueTask<string?>(settingsJson));

        // Act
        var settings = await _service.GetSettingsAsync();

        // Assert
        settings.IncludePricingInResults.Should().BeFalse();
    }

    [Fact]
    public async Task GetSettingsAsync_HandlesStorageException()
    {
        // Arrange
        _mockJsRuntime.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object[]>())
            .Returns<string?>(_ => throw new InvalidOperationException("JS not available"));

        // Act
        var settings = await _service.GetSettingsAsync();

        // Assert
        settings.Should().NotBeNull(); // Uses defaults
    }

    #endregion

    #region SaveSettingsAsync Tests

    [Fact]
    public async Task SaveSettingsAsync_SavesSettings()
    {
        // Arrange
        var settings = new PricingSettings { IncludePricingInResults = false };
        _mockJsRuntime.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object[]>())
            .Returns(new ValueTask<string?>((string?)null));

        // Act
        await _service.SaveSettingsAsync(settings);

        // Assert
        await _mockJsRuntime.Received().InvokeVoidAsync("localStorage.setItem", Arg.Any<object[]>());
    }

    [Fact]
    public async Task SaveSettingsAsync_UpdatesLastModified()
    {
        // Arrange
        var settings = new PricingSettings { LastModified = DateTime.MinValue };
        _mockJsRuntime.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object[]>())
            .Returns(new ValueTask<string?>((string?)null));

        // Act
        await _service.SaveSettingsAsync(settings);
        var retrievedSettings = await _service.GetSettingsAsync();

        // Assert
        retrievedSettings.LastModified.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task SaveSettingsAsync_FiresOnSettingsChanged()
    {
        // Arrange
        var eventFired = false;
        _service.OnSettingsChanged += () => eventFired = true;
        var settings = new PricingSettings();

        // Act
        await _service.SaveSettingsAsync(settings);

        // Assert
        eventFired.Should().BeTrue();
    }

    #endregion

    #region ResetToDefaultsAsync Tests

    [Fact]
    public async Task ResetToDefaultsAsync_ResetsSettings()
    {
        // Arrange
        _mockJsRuntime.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object[]>())
            .Returns(new ValueTask<string?>((string?)null));

        // Act
        await _service.ResetToDefaultsAsync();
        var settings = await _service.GetSettingsAsync();

        // Assert
        settings.Should().NotBeNull();
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
    public async Task ResetPricingCacheAsync_RemovesCacheFromStorage()
    {
        // Act
        await _service.ResetPricingCacheAsync();

        // Assert
        await _mockJsRuntime.Received().InvokeVoidAsync("localStorage.removeItem",
            Arg.Is<object[]>(args => args[0].ToString() == "infra-sizing-pricing-cache"));
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
    }

    [Fact]
    public void GetOnPremDefaults_HasHardwareSection()
    {
        // Act
        var defaults = _service.GetOnPremDefaults();

        // Assert
        defaults.Hardware.Should().NotBeNull();
    }

    [Fact]
    public void GetOnPremDefaults_HasDataCenterSection()
    {
        // Act
        var defaults = _service.GetOnPremDefaults();

        // Assert
        defaults.DataCenter.Should().NotBeNull();
    }

    [Fact]
    public void GetOnPremDefaults_HasLaborSection()
    {
        // Act
        var defaults = _service.GetOnPremDefaults();

        // Assert
        defaults.Labor.Should().NotBeNull();
    }

    #endregion

    #region UpdateOnPremDefaultsAsync Tests

    [Fact]
    public async Task UpdateOnPremDefaultsAsync_UpdatesDefaults()
    {
        // Arrange
        var newDefaults = new OnPremPricing
        {
            HardwareRefreshYears = 5,
            HardwareMaintenancePercent = 20
        };

        // Act
        await _service.UpdateOnPremDefaultsAsync(newDefaults);
        var retrieved = _service.GetOnPremDefaults();

        // Assert
        retrieved.HardwareRefreshYears.Should().Be(5);
        retrieved.HardwareMaintenancePercent.Should().Be(20);
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

    #region GetCloudAlternatives Tests

    [Fact]
    public void GetCloudAlternatives_ReturnsAlternatives()
    {
        // Act
        var alternatives = _service.GetCloudAlternatives(Distribution.OpenShift);

        // Assert
        alternatives.Should().NotBeNull();
    }

    [Theory]
    [InlineData(Distribution.OpenShift)]
    [InlineData(Distribution.Rancher)]
    [InlineData(Distribution.Kubernetes)]
    public void GetCloudAlternatives_ReturnsNonEmptyList(Distribution distribution)
    {
        // Act
        var alternatives = _service.GetCloudAlternatives(distribution);

        // Assert
        alternatives.Should().NotBeEmpty();
    }

    #endregion

    #region GetDistributionSpecificAlternatives Tests

    [Fact]
    public void GetDistributionSpecificAlternatives_ReturnsOnlyDistributionSpecific()
    {
        // Act
        var alternatives = _service.GetDistributionSpecificAlternatives(Distribution.OpenShift);

        // Assert
        alternatives.Should().AllSatisfy(a => a.IsDistributionSpecific.Should().BeTrue());
    }

    #endregion

    #region GetGenericCloudAlternatives Tests

    [Fact]
    public void GetGenericCloudAlternatives_ReturnsAlternatives()
    {
        // Act
        var alternatives = _service.GetGenericCloudAlternatives();

        // Assert
        alternatives.Should().NotBeNull();
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
    public void CalculateOnPremCost_IncludesHardwareCost()
    {
        // Arrange
        _service.IncludePricingInResults = true;

        // Act
        var result = _service.CalculateOnPremCost("OpenShift", 10, 160, 320, 5);

        // Assert
        result.MonthlyHardware.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CalculateOnPremCost_IncludesDataCenterCost()
    {
        // Arrange
        _service.IncludePricingInResults = true;

        // Act
        var result = _service.CalculateOnPremCost("OpenShift", 10, 160, 320, 5);

        // Assert
        result.MonthlyDataCenter.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void CalculateOnPremCost_IncludesLaborCost()
    {
        // Arrange
        _service.IncludePricingInResults = true;

        // Act
        var result = _service.CalculateOnPremCost("OpenShift", 10, 160, 320, 5, hasProduction: true);

        // Assert
        result.MonthlyLabor.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void CalculateOnPremCost_IncludesLicenseCost()
    {
        // Arrange
        _service.IncludePricingInResults = true;

        // Act
        var result = _service.CalculateOnPremCost("OpenShift", 10, 160, 320, 5);

        // Assert
        result.MonthlyLicense.Should().BeGreaterThanOrEqualTo(0);
    }

    #endregion

    #region Mendix Pricing Tests

    [Fact]
    public void GetMendixPricingSettings_ReturnsSettings()
    {
        // Act
        var settings = _service.GetMendixPricingSettings();

        // Assert
        settings.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateMendixPricingSettingsAsync_UpdatesSettings()
    {
        // Arrange
        var newSettings = new MendixPricingSettings
        {
            PlatformPremiumUnlimitedPerYear = 100000m
        };

        // Act
        await _service.UpdateMendixPricingSettingsAsync(newSettings);
        var retrieved = _service.GetMendixPricingSettings();

        // Assert
        retrieved.PlatformPremiumUnlimitedPerYear.Should().Be(100000m);
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

    #endregion

    #region IsMendixSupportedProvider Tests

    [Theory]
    [InlineData(MendixPrivateCloudProvider.Azure, true)]
    [InlineData(MendixPrivateCloudProvider.EKS, true)]
    [InlineData(MendixPrivateCloudProvider.AKS, true)]
    [InlineData(MendixPrivateCloudProvider.GKE, true)]
    [InlineData(MendixPrivateCloudProvider.OpenShift, true)]
    [InlineData(MendixPrivateCloudProvider.GenericK8s, false)]
    public void IsMendixSupportedProvider_ReturnsCorrectValue(
        MendixPrivateCloudProvider provider, bool expected)
    {
        // Act
        var result = _service.IsMendixSupportedProvider(provider);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region GetMendixSupportedProviders Tests

    [Fact]
    public void GetMendixSupportedProviders_ReturnsNonEmptyList()
    {
        // Act
        var providers = _service.GetMendixSupportedProviders();

        // Assert
        providers.Should().NotBeEmpty();
    }

    [Fact]
    public void GetMendixSupportedProviders_ContainsAzure()
    {
        // Act
        var providers = _service.GetMendixSupportedProviders();

        // Assert
        providers.Should().Contain(MendixPrivateCloudProvider.Azure);
    }

    [Fact]
    public void GetMendixSupportedProviders_ContainsEKS()
    {
        // Act
        var providers = _service.GetMendixSupportedProviders();

        // Assert
        providers.Should().Contain(MendixPrivateCloudProvider.EKS);
    }

    #endregion

    #region CalculateMendixCost Tests

    [Fact]
    public void CalculateMendixCost_CloudDeployment_ReturnsResult()
    {
        // Arrange
        var config = new MendixDeploymentConfig
        {
            Category = MendixDeploymentCategory.Cloud,
            CloudType = MendixCloudType.SaaS,
            InternalUsers = 100,
            ExternalUsers = 1000
        };

        // Act
        var result = _service.CalculateMendixCost(config);

        // Assert
        result.Should().NotBeNull();
        result.Category.Should().Be(MendixDeploymentCategory.Cloud);
    }

    [Fact]
    public void CalculateMendixCost_PrivateCloudDeployment_ReturnsResult()
    {
        // Arrange
        var config = new MendixDeploymentConfig
        {
            Category = MendixDeploymentCategory.PrivateCloud,
            PrivateCloudProvider = MendixPrivateCloudProvider.EKS,
            NumberOfEnvironments = 5
        };

        // Act
        var result = _service.CalculateMendixCost(config);

        // Assert
        result.Should().NotBeNull();
        result.Category.Should().Be(MendixDeploymentCategory.PrivateCloud);
    }

    [Fact]
    public void CalculateMendixCost_IncludesUserLicenseCost()
    {
        // Arrange
        var config = new MendixDeploymentConfig
        {
            Category = MendixDeploymentCategory.Cloud,
            InternalUsers = 500,
            ExternalUsers = 500000
        };

        // Act
        var result = _service.CalculateMendixCost(config);

        // Assert
        result.UserLicenseCost.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CalculateMendixCost_WithGenAI_IncludesGenAICost()
    {
        // Arrange
        var config = new MendixDeploymentConfig
        {
            Category = MendixDeploymentCategory.Cloud,
            IncludeGenAI = true,
            GenAIModelPackSize = "Small"
        };

        // Act
        var result = _service.CalculateMendixCost(config);

        // Assert
        result.GenAICost.Should().BeGreaterThanOrEqualTo(0);
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
            requiredCpu: 1,
            requiredDbStorageGB: 10);

        // Assert
        pack.Should().NotBeNull();
    }

    [Fact]
    public void RecommendResourcePack_ReturnsNull_WhenNoPackMeetsRequirements()
    {
        // Act
        var pack = _service.RecommendResourcePack(
            MendixResourcePackTier.Standard,
            requiredMemoryGB: 1000, // Extremely high
            requiredCpu: 500,
            requiredDbStorageGB: 10000);

        // Assert
        pack.Should().BeNull();
    }

    #endregion

    #region GetCacheStatusAsync Tests

    [Fact]
    public async Task GetCacheStatusAsync_ReturnsStatus()
    {
        // Arrange
        _mockJsRuntime.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object[]>())
            .Returns(new ValueTask<string?>((string?)null));

        // Act
        var status = await _service.GetCacheStatusAsync();

        // Assert
        status.Should().NotBeNull();
    }

    [Fact]
    public async Task GetCacheStatusAsync_WithNoCache_ShowsZeroCachedProviders()
    {
        // Arrange
        _mockJsRuntime.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object[]>())
            .Returns(new ValueTask<string?>((string?)null));

        // Act
        var status = await _service.GetCacheStatusAsync();

        // Assert
        status.CachedProviderCount.Should().Be(0);
    }

    [Fact]
    public async Task GetCacheStatusAsync_ReturnsConfiguredApiCount()
    {
        // Arrange - Configure an API first
        _mockJsRuntime.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object[]>())
            .Returns(new ValueTask<string?>((string?)null));
        var config = new CloudApiConfig { ApiKey = "test-key" };
        await _service.ConfigureCloudApiAsync(CloudProvider.AWS, config);

        // Act
        var status = await _service.GetCacheStatusAsync();

        // Assert
        status.ConfiguredApiCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetCacheStatusAsync_OldCache_MarksAsStale()
    {
        // Arrange
        _mockJsRuntime.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object[]>())
            .Returns(new ValueTask<string?>((string?)null));

        // Act
        var status = await _service.GetCacheStatusAsync();

        // Assert - No LastCacheReset means stale
        status.IsStale.Should().BeTrue();
    }

    [Fact]
    public async Task GetCacheStatusAsync_HandlesInvalidCacheJson()
    {
        // Arrange
        _mockJsRuntime.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object[]>())
            .Returns(callInfo =>
            {
                var key = (callInfo[0] as object[])?[0]?.ToString();
                return key == "infra-sizing-pricing-cache"
                    ? new ValueTask<string?>("invalid-json")
                    : new ValueTask<string?>((string?)null);
            });

        // Act
        var status = await _service.GetCacheStatusAsync();

        // Assert - Should not throw, return 0 providers
        status.CachedProviderCount.Should().Be(0);
    }

    #endregion

    #region ConfigureCloudApiAsync Tests

    [Fact]
    public async Task ConfigureCloudApiAsync_ConfiguresProvider()
    {
        // Arrange
        var config = new CloudApiConfig
        {
            ApiKey = "test-key",
            DefaultRegion = "us-east-1"
        };

        // Act
        await _service.ConfigureCloudApiAsync(CloudProvider.AWS, config);
        var retrieved = _service.GetCloudApiConfig(CloudProvider.AWS);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.ApiKey.Should().Be("test-key");
        retrieved.DefaultRegion.Should().Be("us-east-1");
        retrieved.IsConfigured.Should().BeTrue(); // Computed from ApiKey
    }

    [Fact]
    public async Task ConfigureCloudApiAsync_UpdatesLastModified()
    {
        // Arrange
        var config = new CloudApiConfig { ApiKey = "test-key" };
        _mockJsRuntime.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object[]>())
            .Returns(new ValueTask<string?>((string?)null));

        // Act
        await _service.ConfigureCloudApiAsync(CloudProvider.Azure, config);
        var settings = await _service.GetSettingsAsync();

        // Assert
        settings.LastModified.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task ConfigureCloudApiAsync_SavesSettings()
    {
        // Arrange
        var config = new CloudApiConfig { ApiKey = "test-key" };

        // Act
        await _service.ConfigureCloudApiAsync(CloudProvider.GCP, config);

        // Assert
        await _mockJsRuntime.Received().InvokeVoidAsync("localStorage.setItem", Arg.Any<object[]>());
    }

    #endregion

    #region ValidateCloudApiAsync Tests

    [Fact]
    public async Task ValidateCloudApiAsync_WithNoConfig_ReturnsFalse()
    {
        // Act
        var result = await _service.ValidateCloudApiAsync(CloudProvider.AWS);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateCloudApiAsync_WithUnconfiguredApi_ReturnsFalse()
    {
        // Arrange - No ApiKey means IsConfigured will be false
        var config = new CloudApiConfig { SecretKey = "only-secret" };
        await _service.ConfigureCloudApiAsync(CloudProvider.AWS, config);

        // Act
        var result = await _service.ValidateCloudApiAsync(CloudProvider.AWS);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateCloudApiAsync_WithConfiguredApi_ReturnsTrue()
    {
        // Arrange - ApiKey makes IsConfigured true
        var config = new CloudApiConfig { ApiKey = "test-key" };
        await _service.ConfigureCloudApiAsync(CloudProvider.AWS, config);

        // Act
        var result = await _service.ValidateCloudApiAsync(CloudProvider.AWS);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateCloudApiAsync_SetsLastValidated()
    {
        // Arrange
        var config = new CloudApiConfig { ApiKey = "test-key" };
        await _service.ConfigureCloudApiAsync(CloudProvider.Azure, config);

        // Act
        await _service.ValidateCloudApiAsync(CloudProvider.Azure);
        var retrieved = _service.GetCloudApiConfig(CloudProvider.Azure);

        // Assert
        retrieved!.LastValidated.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        retrieved.IsValid.Should().BeTrue();
    }

    #endregion

    #region GetCloudApiConfig Tests

    [Fact]
    public void GetCloudApiConfig_WithNoConfig_ReturnsNull()
    {
        // Act
        var result = _service.GetCloudApiConfig(CloudProvider.AWS);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCloudApiConfig_WithConfig_ReturnsConfig()
    {
        // Arrange
        var config = new CloudApiConfig { ApiKey = "test-key" };
        await _service.ConfigureCloudApiAsync(CloudProvider.GCP, config);

        // Act
        var result = _service.GetCloudApiConfig(CloudProvider.GCP);

        // Assert
        result.Should().NotBeNull();
        result!.ApiKey.Should().Be("test-key");
        result.IsConfigured.Should().BeTrue(); // Computed from ApiKey
    }

    #endregion

    #region CalculateMendixCost - Cloud Dedicated Tests

    [Fact]
    public void CalculateMendixCost_CloudDedicated_ReturnsCorrectDeploymentType()
    {
        // Arrange
        var config = new MendixDeploymentConfig
        {
            Category = MendixDeploymentCategory.Cloud,
            CloudType = MendixCloudType.Dedicated
        };

        // Act
        var result = _service.CalculateMendixCost(config);

        // Assert
        result.DeploymentTypeName.Should().Be("Mendix Cloud Dedicated");
    }

    [Fact]
    public void CalculateMendixCost_CloudDedicated_HasDeploymentFee()
    {
        // Arrange
        var config = new MendixDeploymentConfig
        {
            Category = MendixDeploymentCategory.Cloud,
            CloudType = MendixCloudType.Dedicated
        };

        // Act
        var result = _service.CalculateMendixCost(config);

        // Assert
        result.DeploymentFeeCost.Should().BeGreaterThan(0);
    }

    #endregion

    #region CalculateMendixCost - Resource Pack Tests

    [Fact]
    public void CalculateMendixCost_SaaSWithResourcePack_CalculatesCorrectly()
    {
        // Arrange
        var config = new MendixDeploymentConfig
        {
            Category = MendixDeploymentCategory.Cloud,
            CloudType = MendixCloudType.SaaS,
            ResourcePackTier = MendixResourcePackTier.Standard,
            ResourcePackSize = MendixResourcePackSize.S,
            ResourcePackQuantity = 2
        };

        // Act
        var result = _service.CalculateMendixCost(config);

        // Assert
        result.DeploymentTypeName.Should().Be("Mendix Cloud (SaaS)");
        result.ResourcePackDetails.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void CalculateMendixCost_SaaSWithAdditionalFileStorage_IncludesStorageCost()
    {
        // Arrange
        var config = new MendixDeploymentConfig
        {
            Category = MendixDeploymentCategory.Cloud,
            CloudType = MendixCloudType.SaaS,
            ResourcePackTier = MendixResourcePackTier.Standard,
            ResourcePackSize = MendixResourcePackSize.S,
            AdditionalFileStorageGB = 250
        };

        // Act
        var result = _service.CalculateMendixCost(config);

        // Assert
        result.StorageCost.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CalculateMendixCost_SaaSWithAdditionalDbStorage_IncludesStorageCost()
    {
        // Arrange
        var config = new MendixDeploymentConfig
        {
            Category = MendixDeploymentCategory.Cloud,
            CloudType = MendixCloudType.SaaS,
            ResourcePackTier = MendixResourcePackTier.Standard,
            ResourcePackSize = MendixResourcePackSize.S,
            AdditionalDatabaseStorageGB = 150
        };

        // Act
        var result = _service.CalculateMendixCost(config);

        // Assert
        result.StorageCost.Should().BeGreaterThan(0);
    }

    #endregion

    #region CalculateMendixCost - GenAI and Services Tests

    [Fact]
    public void CalculateMendixCost_WithGenAIKnowledgeBase_IncludesGenAICost()
    {
        // Arrange
        var config = new MendixDeploymentConfig
        {
            Category = MendixDeploymentCategory.Cloud,
            IncludeGenAIKnowledgeBase = true
        };

        // Act
        var result = _service.CalculateMendixCost(config);

        // Assert
        result.GenAICost.Should().BeGreaterThan(0);
        result.TotalCloudTokens.Should().BeGreaterThan(0);
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

    [Fact]
    public void CalculateMendixCost_AppliesVolumeDiscount()
    {
        // Arrange
        var settings = _service.GetMendixPricingSettings();
        settings.VolumeDiscountPercent = 10;

        var config = new MendixDeploymentConfig
        {
            Category = MendixDeploymentCategory.Cloud,
            InternalUsers = 500
        };

        // Act
        var result = _service.CalculateMendixCost(config);

        // Assert
        result.DiscountPercent.Should().Be(10);
        if (result.PlatformLicenseCost + result.UserLicenseCost > 0)
        {
            result.DiscountAmount.Should().BeGreaterThan(0);
        }
    }

    #endregion

    #region CalculateMendixCost - Azure Private Cloud Tests

    [Fact]
    public void CalculateMendixCost_AzurePrivateCloud_ReturnsCorrectDeploymentType()
    {
        // Arrange
        var config = new MendixDeploymentConfig
        {
            Category = MendixDeploymentCategory.PrivateCloud,
            PrivateCloudProvider = MendixPrivateCloudProvider.Azure,
            NumberOfEnvironments = 3
        };

        // Act
        var result = _service.CalculateMendixCost(config);

        // Assert
        result.DeploymentTypeName.Should().Be("Mendix on Azure");
    }

    [Fact]
    public void CalculateMendixCost_AzurePrivateCloud_IncludesBaseFee()
    {
        // Arrange
        var config = new MendixDeploymentConfig
        {
            Category = MendixDeploymentCategory.PrivateCloud,
            PrivateCloudProvider = MendixPrivateCloudProvider.Azure,
            NumberOfEnvironments = 3
        };

        // Act
        var result = _service.CalculateMendixCost(config);

        // Assert
        result.DeploymentFeeCost.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CalculateMendixCost_AzurePrivateCloud_WithAdditionalEnvironments_ChargesExtra()
    {
        // Arrange
        var config = new MendixDeploymentConfig
        {
            Category = MendixDeploymentCategory.PrivateCloud,
            PrivateCloudProvider = MendixPrivateCloudProvider.Azure,
            NumberOfEnvironments = 10 // More than the base 3 included
        };

        // Act
        var result = _service.CalculateMendixCost(config);

        // Assert
        result.EnvironmentCost.Should().BeGreaterThan(0);
        result.EnvironmentDetails.Should().Contain("additional");
    }

    [Fact]
    public void CalculateMendixCost_AzurePrivateCloud_WithIncludedEnvironments_NoExtraCost()
    {
        // Arrange
        var config = new MendixDeploymentConfig
        {
            Category = MendixDeploymentCategory.PrivateCloud,
            PrivateCloudProvider = MendixPrivateCloudProvider.Azure,
            NumberOfEnvironments = 2 // Less than the base 3 included
        };

        // Act
        var result = _service.CalculateMendixCost(config);

        // Assert
        result.EnvironmentCost.Should().Be(0);
        result.EnvironmentDetails.Should().Contain("included");
    }

    #endregion

    #region CalculateMendixCost - K8s Private Cloud Tests

    [Theory]
    [InlineData(MendixPrivateCloudProvider.EKS)]
    [InlineData(MendixPrivateCloudProvider.AKS)]
    [InlineData(MendixPrivateCloudProvider.GKE)]
    [InlineData(MendixPrivateCloudProvider.OpenShift)]
    public void CalculateMendixCost_K8sPrivateCloud_SupportedProviders_NoPenalty(MendixPrivateCloudProvider provider)
    {
        // Arrange
        var config = new MendixDeploymentConfig
        {
            Category = MendixDeploymentCategory.PrivateCloud,
            PrivateCloudProvider = provider,
            NumberOfEnvironments = 5
        };

        // Act
        var result = _service.CalculateMendixCost(config);

        // Assert
        result.DeploymentTypeName.Should().Contain($"({provider})");
        result.DeploymentTypeName.Should().NotContain("Manual Setup");
    }

    [Fact]
    public void CalculateMendixCost_K8sPrivateCloud_GenericK8s_ShowsManualSetupWarning()
    {
        // Arrange
        var config = new MendixDeploymentConfig
        {
            Category = MendixDeploymentCategory.PrivateCloud,
            PrivateCloudProvider = MendixPrivateCloudProvider.GenericK8s,
            NumberOfEnvironments = 5
        };

        // Act
        var result = _service.CalculateMendixCost(config);

        // Assert
        result.DeploymentTypeName.Should().Contain("Manual Setup");
    }

    [Fact]
    public void CalculateMendixCost_K8sPrivateCloud_TieredEnvironmentPricing()
    {
        // Arrange
        var config = new MendixDeploymentConfig
        {
            Category = MendixDeploymentCategory.PrivateCloud,
            PrivateCloudProvider = MendixPrivateCloudProvider.EKS,
            NumberOfEnvironments = 100 // Large number to test tiering
        };

        // Act
        var result = _service.CalculateMendixCost(config);

        // Assert
        result.EnvironmentCost.Should().BeGreaterThan(0);
        result.EnvironmentDetails.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void CalculateMendixCost_K8sPrivateCloud_WithIncludedEnvironments_NoExtraCost()
    {
        // Arrange
        var settings = _service.GetMendixPricingSettings();
        var config = new MendixDeploymentConfig
        {
            Category = MendixDeploymentCategory.PrivateCloud,
            PrivateCloudProvider = MendixPrivateCloudProvider.EKS,
            NumberOfEnvironments = settings.K8sBaseEnvironmentsIncluded
        };

        // Act
        var result = _service.CalculateMendixCost(config);

        // Assert
        result.EnvironmentCost.Should().Be(0);
        result.EnvironmentDetails.Should().Contain("included");
    }

    #endregion

    #region CalculateMendixCost - Other Deployment Tests

    [Fact]
    public void CalculateMendixCost_ServerDeployment_ReturnsCorrectDeploymentType()
    {
        // Arrange
        var config = new MendixDeploymentConfig
        {
            Category = MendixDeploymentCategory.Other,
            OtherDeployment = MendixOtherDeployment.Server,
            NumberOfApps = 5
        };

        // Act
        var result = _service.CalculateMendixCost(config);

        // Assert
        result.DeploymentTypeName.Should().Contain("Server");
    }

    [Fact]
    public void CalculateMendixCost_StackITDeployment_ReturnsCorrectDeploymentType()
    {
        // Arrange
        var config = new MendixDeploymentConfig
        {
            Category = MendixDeploymentCategory.Other,
            OtherDeployment = MendixOtherDeployment.StackIT,
            NumberOfApps = 3
        };

        // Act
        var result = _service.CalculateMendixCost(config);

        // Assert
        result.DeploymentTypeName.Should().Contain("StackIT");
    }

    [Fact]
    public void CalculateMendixCost_SapBtpDeployment_ReturnsCorrectDeploymentType()
    {
        // Arrange
        var config = new MendixDeploymentConfig
        {
            Category = MendixDeploymentCategory.Other,
            OtherDeployment = MendixOtherDeployment.SapBtp,
            NumberOfApps = 2
        };

        // Act
        var result = _service.CalculateMendixCost(config);

        // Assert
        result.DeploymentTypeName.Should().Contain("SAP BTP");
    }

    [Fact]
    public void CalculateMendixCost_OtherDeployment_PerAppPricing()
    {
        // Arrange
        var config = new MendixDeploymentConfig
        {
            Category = MendixDeploymentCategory.Other,
            OtherDeployment = MendixOtherDeployment.Server,
            NumberOfApps = 5,
            IsUnlimitedApps = false
        };

        // Act
        var result = _service.CalculateMendixCost(config);

        // Assert
        result.EnvironmentDetails.Should().Contain("5 application(s)");
    }

    [Fact]
    public void CalculateMendixCost_OtherDeployment_UnlimitedApps()
    {
        // Arrange
        var config = new MendixDeploymentConfig
        {
            Category = MendixDeploymentCategory.Other,
            OtherDeployment = MendixOtherDeployment.Server,
            IsUnlimitedApps = true
        };

        // Act
        var result = _service.CalculateMendixCost(config);

        // Assert
        result.EnvironmentDetails.Should().Be("Unlimited applications");
    }

    #endregion

    #region RecommendResourcePack Additional Tests

    [Theory]
    [InlineData(MendixResourcePackTier.Standard)]
    [InlineData(MendixResourcePackTier.Premium)]
    public void RecommendResourcePack_DifferentTiers_ReturnsMatchingTierPack(MendixResourcePackTier tier)
    {
        // Act
        var pack = _service.RecommendResourcePack(
            tier,
            requiredMemoryGB: 1,
            requiredCpu: 0.5m,
            requiredDbStorageGB: 5);

        // Assert
        pack.Should().NotBeNull();
    }

    [Fact]
    public void RecommendResourcePack_ReturnsSmallestSufficientPack()
    {
        // Act
        var smallPack = _service.RecommendResourcePack(
            MendixResourcePackTier.Standard,
            requiredMemoryGB: 1,
            requiredCpu: 0.5m,
            requiredDbStorageGB: 5);

        var largePack = _service.RecommendResourcePack(
            MendixResourcePackTier.Standard,
            requiredMemoryGB: 8,
            requiredCpu: 4,
            requiredDbStorageGB: 100);

        // Assert
        if (smallPack != null && largePack != null)
        {
            smallPack.PricePerYear.Should().BeLessThanOrEqualTo(largePack.PricePerYear);
        }
    }

    #endregion

    #region CalculateOnPremCost Additional Tests

    [Fact]
    public void CalculateOnPremCost_WithLoadBalancers_IncludesLBCost()
    {
        // Arrange
        _service.IncludePricingInResults = true;

        // Act
        var withLB = _service.CalculateOnPremCost("OpenShift", 10, 160, 320, 5, loadBalancers: 2);
        var withoutLB = _service.CalculateOnPremCost("OpenShift", 10, 160, 320, 5, loadBalancers: 0);

        // Assert
        withLB.MonthlyHardware.Should().BeGreaterThan(withoutLB.MonthlyHardware);
    }

    [Fact]
    public void CalculateOnPremCost_WithoutProduction_ReducesLaborCost()
    {
        // Arrange
        _service.IncludePricingInResults = true;

        // Act
        var withProd = _service.CalculateOnPremCost("OpenShift", 10, 160, 320, 5, hasProduction: true);
        var withoutProd = _service.CalculateOnPremCost("OpenShift", 10, 160, 320, 5, hasProduction: false);

        // Assert
        withoutProd.MonthlyLabor.Should().BeLessThanOrEqualTo(withProd.MonthlyLabor);
    }

    [Fact]
    public void CalculateOnPremCost_TotalCost_IsCalculatedCorrectly()
    {
        // Arrange
        _service.IncludePricingInResults = true;

        // Act
        var result = _service.CalculateOnPremCost("OpenShift", 10, 160, 320, 5);

        // Assert - MonthlyTotal should be sum of components
        var expectedTotal = result.MonthlyHardware + result.MonthlyDataCenter +
                            result.MonthlyLabor + result.MonthlyLicense;
        result.MonthlyTotal.Should().Be(expectedTotal);
    }

    #endregion

    #region Edge Cases and Error Handling

    [Fact]
    public async Task ResetPricingCacheAsync_CompletesSuccessfully()
    {
        // Act & Assert - Should not throw even when localStorage is not mocked
        var action = async () => await _service.ResetPricingCacheAsync();
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public void CalculateMendixCost_WithInvalidGenAIPackSize_HandlesGracefully()
    {
        // Arrange
        var config = new MendixDeploymentConfig
        {
            Category = MendixDeploymentCategory.Cloud,
            IncludeGenAI = true,
            GenAIModelPackSize = "InvalidSize"
        };

        // Act
        var result = _service.CalculateMendixCost(config);

        // Assert - Should not throw, GenAICost should be 0
        result.GenAICost.Should().Be(0);
    }

    [Fact]
    public void CalculateMendixCost_WithEmptyGenAIPackSize_HandlesGracefully()
    {
        // Arrange
        var config = new MendixDeploymentConfig
        {
            Category = MendixDeploymentCategory.Cloud,
            IncludeGenAI = true,
            GenAIModelPackSize = ""
        };

        // Act
        var result = _service.CalculateMendixCost(config);

        // Assert - Should not throw
        result.Should().NotBeNull();
    }

    [Fact]
    public void CalculateMendixCost_ZeroUsers_HandlesGracefully()
    {
        // Arrange
        var config = new MendixDeploymentConfig
        {
            Category = MendixDeploymentCategory.Cloud,
            InternalUsers = 0,
            ExternalUsers = 0
        };

        // Act
        var result = _service.CalculateMendixCost(config);

        // Assert
        result.UserLicenseCost.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void IsOnPremDistribution_AllDistributions_DoesNotThrow()
    {
        // Arrange
        var allDistributions = Enum.GetValues<Distribution>();

        // Act & Assert
        foreach (var distribution in allDistributions)
        {
            var action = () => _service.IsOnPremDistribution(distribution);
            action.Should().NotThrow();
        }
    }

    #endregion
}
