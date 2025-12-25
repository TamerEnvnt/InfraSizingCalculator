using FluentAssertions;
using InfraSizingCalculator.Models.Pricing;
using InfraSizingCalculator.Services.Pricing;
using Microsoft.JSInterop;
using NSubstitute;
using Xunit;

namespace InfraSizingCalculator.UnitTests;

public class PricingServiceTests
{
    private readonly IJSRuntime _mockJsRuntime;
    private readonly PricingService _service;

    public PricingServiceTests()
    {
        _mockJsRuntime = Substitute.For<IJSRuntime>();
        _service = new PricingService(_mockJsRuntime);
    }

    #region GetPricingAsync Tests

    [Theory]
    [InlineData(CloudProvider.AWS)]
    [InlineData(CloudProvider.Azure)]
    [InlineData(CloudProvider.GCP)]
    [InlineData(CloudProvider.OCI)]
    public async Task GetPricingAsync_ReturnsDefaultPricing_WhenNoCacheExists(CloudProvider provider)
    {
        // Arrange
        _mockJsRuntime.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object[]>())
            .Returns(new ValueTask<string?>((string?)null));

        // Act
        var pricing = await _service.GetPricingAsync(provider, "us-east-1");

        // Assert
        pricing.Should().NotBeNull();
        pricing.Provider.Should().Be(provider);
        pricing.IsLive.Should().BeFalse();
    }

    [Fact]
    public async Task GetPricingAsync_ReturnsDefaultPricing_WhenStorageAccessFails()
    {
        // Arrange
        _mockJsRuntime.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object[]>())
            .Returns<string?>(_ => throw new InvalidOperationException("JS Interop not available"));

        // Act
        var pricing = await _service.GetPricingAsync(CloudProvider.AWS, "us-east-1");

        // Assert
        pricing.Should().NotBeNull();
        pricing.Provider.Should().Be(CloudProvider.AWS);
    }

    [Theory]
    [InlineData(CloudProvider.AWS, "us-east-1")]
    [InlineData(CloudProvider.Azure, "eastus")]
    [InlineData(CloudProvider.GCP, "us-central1")]
    [InlineData(CloudProvider.OCI, "us-ashburn-1")]
    public async Task GetPricingAsync_DefaultPricingHasAllComponents(CloudProvider provider, string region)
    {
        // Arrange
        _mockJsRuntime.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object[]>())
            .Returns(new ValueTask<string?>((string?)null));

        // Act
        var pricing = await _service.GetPricingAsync(provider, region);

        // Assert
        pricing.Compute.Should().NotBeNull();
        pricing.Storage.Should().NotBeNull();
        pricing.Network.Should().NotBeNull();
        pricing.Licenses.Should().NotBeNull();
        pricing.Support.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPricingAsync_ComputePricingHasValidValues()
    {
        // Arrange
        _mockJsRuntime.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object[]>())
            .Returns(new ValueTask<string?>((string?)null));

        // Act
        var pricing = await _service.GetPricingAsync(CloudProvider.AWS, "us-east-1");

        // Assert
        pricing.Compute.CpuPerHour.Should().BeGreaterThan(0);
        pricing.Compute.RamGBPerHour.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetPricingAsync_StoragePricingHasValidValues()
    {
        // Arrange
        _mockJsRuntime.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object[]>())
            .Returns(new ValueTask<string?>((string?)null));

        // Act
        var pricing = await _service.GetPricingAsync(CloudProvider.AWS, "us-east-1");

        // Assert
        pricing.Storage.SsdPerGBMonth.Should().BeGreaterThan(0);
        pricing.Storage.HddPerGBMonth.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetPricingAsync_ReturnsCachedPricing_OnSecondCall()
    {
        // Arrange
        _mockJsRuntime.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object[]>())
            .Returns(new ValueTask<string?>((string?)null));

        // Act
        var pricing1 = await _service.GetPricingAsync(CloudProvider.AWS, "us-east-1");
        var pricing2 = await _service.GetPricingAsync(CloudProvider.AWS, "us-east-1");

        // Assert - should be the same object from memory cache
        pricing2.Should().BeSameAs(pricing1);
    }

    [Fact]
    public async Task GetPricingAsync_DefaultPricing_ReturnsOnDemandType()
    {
        // Arrange
        _mockJsRuntime.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object[]>())
            .Returns(new ValueTask<string?>((string?)null));

        // Act - Request any pricing type, default pricing always returns OnDemand
        var pricing = await _service.GetPricingAsync(CloudProvider.AWS, "us-east-1", PricingType.Reserved3Year);

        // Assert - Default pricing is always OnDemand (actual API implementation would return correct type)
        pricing.Should().NotBeNull();
        pricing.PricingType.Should().Be(PricingType.OnDemand);
    }

    #endregion

    #region GetRegions Tests

    [Theory]
    [InlineData(CloudProvider.AWS)]
    [InlineData(CloudProvider.Azure)]
    [InlineData(CloudProvider.GCP)]
    [InlineData(CloudProvider.OCI)]
    public void GetRegions_ReturnsNonEmptyList(CloudProvider provider)
    {
        // Act
        var regions = _service.GetRegions(provider);

        // Assert
        regions.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData(CloudProvider.AWS)]
    [InlineData(CloudProvider.Azure)]
    [InlineData(CloudProvider.GCP)]
    [InlineData(CloudProvider.OCI)]
    public void GetRegions_AllRegionsHaveRequiredFields(CloudProvider provider)
    {
        // Act
        var regions = _service.GetRegions(provider);

        // Assert
        foreach (var region in regions)
        {
            region.Code.Should().NotBeNullOrEmpty();
            region.DisplayName.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public void GetRegions_AWSHasUsEast1()
    {
        // Act
        var regions = _service.GetRegions(CloudProvider.AWS);

        // Assert
        regions.Should().Contain(r => r.Code == "us-east-1");
    }

    [Fact]
    public void GetRegions_AzureHasEastUs()
    {
        // Act
        var regions = _service.GetRegions(CloudProvider.Azure);

        // Assert
        regions.Should().Contain(r => r.Code == "eastus");
    }

    #endregion

    #region GetDefaultPricing Tests

    [Theory]
    [InlineData(CloudProvider.AWS)]
    [InlineData(CloudProvider.Azure)]
    [InlineData(CloudProvider.GCP)]
    [InlineData(CloudProvider.OCI)]
    public void GetDefaultPricing_ReturnsValidPricing(CloudProvider provider)
    {
        // Act
        var pricing = _service.GetDefaultPricing(provider);

        // Assert
        pricing.Should().NotBeNull();
        pricing.Provider.Should().Be(provider);
        pricing.Compute.Should().NotBeNull();
        pricing.Storage.Should().NotBeNull();
        pricing.Network.Should().NotBeNull();
    }

    [Fact]
    public void GetDefaultPricing_AWS_HasManagedControlPlaneCost()
    {
        // Act
        var pricing = _service.GetDefaultPricing(CloudProvider.AWS);

        // Assert
        pricing.Compute.ManagedControlPlanePerHour.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GetDefaultPricing_HasLicensePricing()
    {
        // Act
        var pricing = _service.GetDefaultPricing(CloudProvider.AWS);

        // Assert
        pricing.Licenses.OpenShiftPerNodeYear.Should().BeGreaterThan(0);
        pricing.Licenses.RancherEnterprisePerNodeYear.Should().BeGreaterThan(0);
        pricing.Licenses.TanzuPerCoreYear.Should().BeGreaterThan(0);
    }

    #endregion

    #region OnPrem Pricing Tests

    [Fact]
    public void GetOnPremPricing_ReturnsDefaultPricing()
    {
        // Act
        var pricing = _service.GetOnPremPricing();

        // Assert
        pricing.Should().NotBeNull();
    }

    [Fact]
    public void UpdateOnPremPricing_UpdatesPricing()
    {
        // Arrange
        var newPricing = new OnPremPricing
        {
            HardwareRefreshYears = 5,
            HardwareMaintenancePercent = 15
        };

        // Act
        _service.UpdateOnPremPricing(newPricing);
        var retrieved = _service.GetOnPremPricing();

        // Assert
        retrieved.HardwareRefreshYears.Should().Be(5);
        retrieved.HardwareMaintenancePercent.Should().Be(15);
    }

    [Fact]
    public void GetOnPremPricing_HasHardwareDefaults()
    {
        // Act
        var pricing = _service.GetOnPremPricing();

        // Assert
        pricing.Hardware.Should().NotBeNull();
    }

    [Fact]
    public void GetOnPremPricing_HasDataCenterDefaults()
    {
        // Act
        var pricing = _service.GetOnPremPricing();

        // Assert
        pricing.DataCenter.Should().NotBeNull();
    }

    [Fact]
    public void GetOnPremPricing_HasLaborDefaults()
    {
        // Act
        var pricing = _service.GetOnPremPricing();

        // Assert
        pricing.Labor.Should().NotBeNull();
    }

    #endregion

    #region IsPricingStale Tests

    [Fact]
    public void IsPricingStale_WhenNoCachedPricing_ReturnsTrue()
    {
        // Act
        var isStale = _service.IsPricingStale(CloudProvider.AWS, "us-east-1");

        // Assert
        isStale.Should().BeTrue();
    }

    [Fact]
    public async Task IsPricingStale_AfterFetching_ReturnsFalse()
    {
        // Arrange
        _mockJsRuntime.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object[]>())
            .Returns(new ValueTask<string?>((string?)null));

        await _service.GetPricingAsync(CloudProvider.AWS, "us-east-1");

        // Act
        var isStale = _service.IsPricingStale(CloudProvider.AWS, "us-east-1");

        // Assert
        isStale.Should().BeFalse();
    }

    #endregion

    #region GetLastPricingUpdate Tests

    [Fact]
    public void GetLastPricingUpdate_WhenNoCachedPricing_ReturnsNull()
    {
        // Act
        var lastUpdate = _service.GetLastPricingUpdate(CloudProvider.AWS, "us-east-1");

        // Assert
        lastUpdate.Should().BeNull();
    }

    [Fact]
    public async Task GetLastPricingUpdate_AfterFetching_ReturnsDateTime()
    {
        // Arrange
        _mockJsRuntime.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object[]>())
            .Returns(new ValueTask<string?>((string?)null));

        var beforeFetch = DateTime.UtcNow;
        await _service.GetPricingAsync(CloudProvider.AWS, "us-east-1");

        // Act
        var lastUpdate = _service.GetLastPricingUpdate(CloudProvider.AWS, "us-east-1");

        // Assert
        lastUpdate.Should().NotBeNull();
        lastUpdate.Should().BeOnOrAfter(beforeFetch.AddSeconds(-1));
    }

    #endregion

    #region RefreshPricingAsync Tests

    [Fact]
    public async Task RefreshPricingAsync_ReturnsNull_WhenNoLiveApiAvailable()
    {
        // Act
        var pricing = await _service.RefreshPricingAsync(CloudProvider.AWS, "us-east-1");

        // Assert
        // Currently returns null as live API is not implemented
        pricing.Should().BeNull();
    }

    #endregion

    #region Compute Pricing Calculation Tests

    [Fact]
    public void ComputePricing_CalculateHourlyCost_ReturnsCorrectValue()
    {
        // Arrange
        var compute = new ComputePricing
        {
            CpuPerHour = 0.05m,
            RamGBPerHour = 0.01m
        };

        // Act
        var hourlyCost = compute.CalculateHourlyCost(8, 32);

        // Assert
        hourlyCost.Should().Be((8 * 0.05m) + (32 * 0.01m)); // 0.40 + 0.32 = 0.72
    }

    [Fact]
    public void ComputePricing_CalculateMonthlyCost_ReturnsCorrectValue()
    {
        // Arrange
        var compute = new ComputePricing
        {
            CpuPerHour = 0.05m,
            RamGBPerHour = 0.01m
        };

        // Act
        var monthlyCost = compute.CalculateMonthlyCost(8, 32);
        var expectedHourly = (8 * 0.05m) + (32 * 0.01m);
        var expected = expectedHourly * ComputePricing.HoursPerMonth;

        // Assert
        monthlyCost.Should().Be(expected);
    }

    #endregion

    #region Storage Pricing Calculation Tests

    [Fact]
    public void StoragePricing_CalculateMonthlyCost_ReturnsCorrectValue()
    {
        // Arrange
        var storage = new StoragePricing
        {
            SsdPerGBMonth = 0.10m,
            HddPerGBMonth = 0.02m,
            ObjectStoragePerGBMonth = 0.023m,
            BackupPerGBMonth = 0.05m
        };

        // Act
        var cost = storage.CalculateMonthlyCost(500, 1000, 200, 100);

        // Assert
        var expected = (500 * 0.10m) + (1000 * 0.02m) + (200 * 0.023m) + (100 * 0.05m);
        cost.Should().Be(expected);
    }

    [Fact]
    public void StoragePricing_CalculateMonthlyCost_WithOnlySsd_ReturnsCorrectValue()
    {
        // Arrange
        var storage = new StoragePricing { SsdPerGBMonth = 0.10m };

        // Act
        var cost = storage.CalculateMonthlyCost(500);

        // Assert
        cost.Should().Be(50m);
    }

    #endregion

    #region Network Pricing Calculation Tests

    [Fact]
    public void NetworkPricing_CalculateMonthlyCost_ReturnsCorrectValue()
    {
        // Arrange
        var network = new NetworkPricing
        {
            LoadBalancerPerHour = 0.025m,
            EgressPerGB = 0.09m,
            PublicIpPerHour = 0.005m
        };

        // Act
        var cost = network.CalculateMonthlyCost(2, 500, 4);

        // Assert
        var expected = (2 * 0.025m * 730) + (500 * 0.09m) + (4 * 0.005m * 730);
        cost.Should().Be(expected);
    }

    #endregion

    #region Support Pricing Calculation Tests

    [Theory]
    [InlineData(SupportLevel.None, 0)]
    [InlineData(SupportLevel.Basic, 0)]
    [InlineData(SupportLevel.Developer, 3)]
    [InlineData(SupportLevel.Business, 10)]
    [InlineData(SupportLevel.Enterprise, 15)]
    public void SupportPricing_GetSupportCost_ReturnsCorrectPercentage(
        SupportLevel level, decimal expectedPercent)
    {
        // Arrange
        var support = new SupportPricing
        {
            BasicSupportPercent = 0,
            DeveloperSupportPercent = 3,
            BusinessSupportPercent = 10,
            EnterpriseSupportPercent = 15
        };
        var baseCost = 10000m;

        // Act
        var cost = support.GetSupportCost(baseCost, level);

        // Assert
        cost.Should().Be(baseCost * (expectedPercent / 100));
    }

    #endregion

    #region License Pricing Tests

    [Fact]
    public void LicensePricing_GetLicensePerNodeYear_OpenShift_ReturnsCorrectValue()
    {
        // Arrange
        var licenses = new LicensePricing { OpenShiftPerNodeYear = 2500m };

        // Act
        var cost = licenses.GetLicensePerNodeYear(Models.Enums.Distribution.OpenShift);

        // Assert
        cost.Should().Be(2500m);
    }

    [Fact]
    public void LicensePricing_GetLicensePerNodeYear_ManagedK8s_ReturnsZero()
    {
        // Arrange
        var licenses = new LicensePricing
        {
            OpenShiftPerNodeYear = 2500m,
            RancherEnterprisePerNodeYear = 1500m
        };

        // Act & Assert
        licenses.GetLicensePerNodeYear(Models.Enums.Distribution.EKS).Should().Be(0);
        licenses.GetLicensePerNodeYear(Models.Enums.Distribution.AKS).Should().Be(0);
        licenses.GetLicensePerNodeYear(Models.Enums.Distribution.GKE).Should().Be(0);
        licenses.GetLicensePerNodeYear(Models.Enums.Distribution.OKE).Should().Be(0);
    }

    [Fact]
    public void LicensePricing_GetLicensePerNodeYear_ByName_ReturnsCorrectValue()
    {
        // Arrange
        var licenses = new LicensePricing
        {
            OpenShiftPerNodeYear = 2500m,
            RancherEnterprisePerNodeYear = 1500m
        };

        // Act & Assert
        licenses.GetLicensePerNodeYear("openshift").Should().Be(2500m);
        licenses.GetLicensePerNodeYear("rancher").Should().Be(1500m);
        licenses.GetLicensePerNodeYear("unknown").Should().Be(0);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task GetPricingAsync_DifferentRegions_ReturnsDifferentCacheEntries()
    {
        // Arrange
        _mockJsRuntime.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object[]>())
            .Returns(new ValueTask<string?>((string?)null));

        // Act
        var usEast = await _service.GetPricingAsync(CloudProvider.AWS, "us-east-1");
        var euWest = await _service.GetPricingAsync(CloudProvider.AWS, "eu-west-1");

        // Assert
        usEast.Should().NotBeSameAs(euWest);
    }

    [Fact]
    public async Task GetPricingAsync_DifferentProviders_ReturnsDifferentCacheEntries()
    {
        // Arrange
        _mockJsRuntime.InvokeAsync<string?>("localStorage.getItem", Arg.Any<object[]>())
            .Returns(new ValueTask<string?>((string?)null));

        // Act
        var aws = await _service.GetPricingAsync(CloudProvider.AWS, "us-east-1");
        var azure = await _service.GetPricingAsync(CloudProvider.Azure, "eastus");

        // Assert
        aws.Should().NotBeSameAs(azure);
        aws.Provider.Should().Be(CloudProvider.AWS);
        azure.Provider.Should().Be(CloudProvider.Azure);
    }

    #endregion
}
