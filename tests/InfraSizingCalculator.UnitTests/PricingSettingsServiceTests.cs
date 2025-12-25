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
}
