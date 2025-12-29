using FluentAssertions;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Models.Pricing;
using InfraSizingCalculator.Services;
using InfraSizingCalculator.Services.Interfaces;
using NSubstitute;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Services;

/// <summary>
/// Tests for HomePageCostService - Cost calculation logic extracted from Home.razor
/// </summary>
public class HomePageCostServiceTests
{
    private readonly IPricingSettingsService _pricingSettingsService;
    private readonly IHomePageCostService _sut;

    public HomePageCostServiceTests()
    {
        _pricingSettingsService = Substitute.For<IPricingSettingsService>();
        _sut = new HomePageCostService(_pricingSettingsService);
    }

    #region GetMonthlyEstimate Tests

    [Fact]
    public void GetMonthlyEstimate_WhenPricingNA_ReturnsZero()
    {
        // Arrange
        _pricingSettingsService.IsOnPremDistribution(Distribution.OpenShift).Returns(true);
        var context = new HomePageCostContext
        {
            SelectedTechnology = Technology.DotNet,
            SelectedDistribution = Distribution.OpenShift,
            PricingResult = null
        };

        // Act
        var result = _sut.GetMonthlyEstimate(context);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void GetMonthlyEstimate_MendixK8s_UsesMendixCostFromPricingResult()
    {
        // Arrange
        // MendixPricingResult.TotalPerMonth = TotalPerYear / 12
        // Setting PlatformLicenseCost = 60000 gives TotalPerMonth = 5000
        var mendixCost = new MendixPricingResult { PlatformLicenseCost = 60000m };
        var context = new HomePageCostContext
        {
            SelectedTechnology = Technology.Mendix,
            SelectedDeployment = DeploymentModel.Kubernetes,
            PricingResult = new PricingStepResult { MendixCost = mendixCost }
        };

        // Act
        var result = _sut.GetMonthlyEstimate(context);

        // Assert
        result.Should().Be(5000m);
    }

    [Fact]
    public void GetMonthlyEstimate_MendixVMs_CalculatesInfraAndLicenseCost()
    {
        // Arrange
        var context = new HomePageCostContext
        {
            SelectedTechnology = Technology.Mendix,
            SelectedDeployment = DeploymentModel.VMs,
            VMResults = new VMSizingResult { Environments = new List<VMEnvironmentResult>() },
            VMCostEstimate = new CostEstimate { MonthlyTotal = 2000m },
            MendixOtherDeployment = MendixOtherDeployment.Server,
            MendixIsUnlimitedApps = false,
            MendixNumberOfApps = 5
        };

        // Act
        var result = _sut.GetMonthlyEstimate(context);

        // Assert
        // Infrastructure cost = 2000
        // License cost = 8000 * 5 / 12 = 3333.33
        result.Should().BeApproximately(2000m + (8000m * 5 / 12), 0.01m);
    }

    [Fact]
    public void GetMonthlyEstimate_MendixVMs_UnlimitedApps_UsesUnlimitedPricing()
    {
        // Arrange
        var context = new HomePageCostContext
        {
            SelectedTechnology = Technology.Mendix,
            SelectedDeployment = DeploymentModel.VMs,
            VMResults = new VMSizingResult { Environments = new List<VMEnvironmentResult>() },
            VMCostEstimate = new CostEstimate { MonthlyTotal = 1000m },
            MendixOtherDeployment = MendixOtherDeployment.Server,
            MendixIsUnlimitedApps = true,
            MendixNumberOfApps = 100
        };

        // Act
        var result = _sut.GetMonthlyEstimate(context);

        // Assert
        // Infrastructure cost = 1000
        // License cost = 90000 / 12 = 7500
        result.Should().BeApproximately(1000m + (90000m / 12), 0.01m);
    }

    [Fact]
    public void GetMonthlyEstimate_WithPricingResultMonthlyCost_ReturnsThatValue()
    {
        // Arrange
        // MonthlyCost is a computed property - we need to set CloudCost to get a MonthlyCost value
        var context = new HomePageCostContext
        {
            SelectedTechnology = Technology.DotNet,
            SelectedDistribution = Distribution.EKS,
            PricingResult = new PricingStepResult
            {
                IsOnPrem = false,
                CloudCost = new CostEstimate { MonthlyTotal = 15000m }
            }
        };

        // Act
        var result = _sut.GetMonthlyEstimate(context);

        // Assert
        result.Should().Be(15000m);
    }

    [Fact]
    public void GetMonthlyEstimate_WithK8sCostEstimate_ReturnsThatValue()
    {
        // Arrange
        var context = new HomePageCostContext
        {
            SelectedTechnology = Technology.DotNet,
            SelectedDistribution = Distribution.EKS,
            K8sCostEstimate = new CostEstimate { MonthlyTotal = 8000m }
        };

        // Act
        var result = _sut.GetMonthlyEstimate(context);

        // Assert
        result.Should().Be(8000m);
    }

    [Fact]
    public void GetMonthlyEstimate_WithVMCostEstimate_ReturnsThatValue()
    {
        // Arrange
        var context = new HomePageCostContext
        {
            SelectedTechnology = Technology.DotNet,
            SelectedDeployment = DeploymentModel.VMs,
            VMCostEstimate = new CostEstimate { MonthlyTotal = 4000m }
        };

        // Act
        var result = _sut.GetMonthlyEstimate(context);

        // Assert
        result.Should().Be(4000m);
    }

    [Fact]
    public void GetMonthlyEstimate_NoEstimatesAvailable_ReturnsZero()
    {
        // Arrange
        var context = new HomePageCostContext
        {
            SelectedTechnology = Technology.DotNet,
            SelectedDistribution = Distribution.EKS
        };

        // Act
        var result = _sut.GetMonthlyEstimate(context);

        // Assert
        result.Should().Be(0);
    }

    #endregion

    #region GetMendixVMLicenseMonthly Tests

    [Fact]
    public void GetMendixVMLicenseMonthly_Server_PerApp_CalculatesCorrectly()
    {
        // Act
        var result = _sut.GetMendixVMLicenseMonthly(MendixOtherDeployment.Server, false, 10);

        // Assert
        // 8000 per app * 10 apps / 12 months = 6666.67
        result.Should().BeApproximately(8000m * 10 / 12, 0.01m);
    }

    [Fact]
    public void GetMendixVMLicenseMonthly_Server_Unlimited_CalculatesCorrectly()
    {
        // Act
        var result = _sut.GetMendixVMLicenseMonthly(MendixOtherDeployment.Server, true, 10);

        // Assert
        // 90000 unlimited / 12 months = 7500
        result.Should().Be(90000m / 12);
    }

    [Fact]
    public void GetMendixVMLicenseMonthly_StackIT_PerApp_CalculatesCorrectly()
    {
        // Act
        var result = _sut.GetMendixVMLicenseMonthly(MendixOtherDeployment.StackIT, false, 5);

        // Assert
        // 9000 per app * 5 apps / 12 months = 3750
        result.Should().BeApproximately(9000m * 5 / 12, 0.01m);
    }

    [Fact]
    public void GetMendixVMLicenseMonthly_SapBtp_Unlimited_CalculatesCorrectly()
    {
        // Act
        var result = _sut.GetMendixVMLicenseMonthly(MendixOtherDeployment.SapBtp, true, 100);

        // Assert
        // 85000 unlimited / 12 months = 7083.33
        result.Should().BeApproximately(85000m / 12, 0.01m);
    }

    #endregion

    #region GetCostProvider Tests

    [Fact]
    public void GetCostProvider_MendixCloud_SaaS_ReturnsMendixCloud()
    {
        // Arrange
        var context = new HomePageCostContext
        {
            SelectedTechnology = Technology.Mendix,
            SelectedDeployment = DeploymentModel.Kubernetes,
            MendixDeploymentCategory = MendixDeploymentCategory.Cloud,
            MendixCloudType = MendixCloudType.SaaS
        };

        // Act
        var result = _sut.GetCostProvider(context);

        // Assert
        result.Should().Be("Mendix Cloud");
    }

    [Fact]
    public void GetCostProvider_MendixCloud_Dedicated_ReturnsMendixDedicated()
    {
        // Arrange
        var context = new HomePageCostContext
        {
            SelectedTechnology = Technology.Mendix,
            SelectedDeployment = DeploymentModel.Kubernetes,
            MendixDeploymentCategory = MendixDeploymentCategory.Cloud,
            MendixCloudType = MendixCloudType.Dedicated
        };

        // Act
        var result = _sut.GetCostProvider(context);

        // Assert
        result.Should().Be("Mendix Dedicated");
    }

    [Theory]
    [InlineData(MendixPrivateCloudProvider.Azure, "Mendix Azure")]
    [InlineData(MendixPrivateCloudProvider.EKS, "Mendix EKS")]
    [InlineData(MendixPrivateCloudProvider.AKS, "Mendix AKS")]
    [InlineData(MendixPrivateCloudProvider.GKE, "Mendix GKE")]
    [InlineData(MendixPrivateCloudProvider.OpenShift, "Mendix OpenShift")]
    public void GetCostProvider_MendixPrivateCloud_ReturnsCorrectProvider(
        MendixPrivateCloudProvider provider, string expected)
    {
        // Arrange
        var context = new HomePageCostContext
        {
            SelectedTechnology = Technology.Mendix,
            SelectedDeployment = DeploymentModel.Kubernetes,
            MendixDeploymentCategory = MendixDeploymentCategory.PrivateCloud,
            MendixPrivateCloudProvider = provider
        };

        // Act
        var result = _sut.GetCostProvider(context);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(MendixOtherDeployment.Server, "Mendix Server")]
    [InlineData(MendixOtherDeployment.StackIT, "Mendix StackIT")]
    [InlineData(MendixOtherDeployment.SapBtp, "Mendix SAP BTP")]
    public void GetCostProvider_MendixOther_ReturnsCorrectProvider(
        MendixOtherDeployment deployment, string expected)
    {
        // Arrange
        var context = new HomePageCostContext
        {
            SelectedTechnology = Technology.Mendix,
            SelectedDeployment = DeploymentModel.Kubernetes,
            MendixDeploymentCategory = MendixDeploymentCategory.Other,
            MendixOtherDeployment = deployment
        };

        // Act
        var result = _sut.GetCostProvider(context);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(MendixOtherDeployment.Server, "Mendix Server")]
    [InlineData(MendixOtherDeployment.StackIT, "Mendix StackIT")]
    [InlineData(MendixOtherDeployment.SapBtp, "Mendix SAP BTP")]
    public void GetCostProvider_MendixVMs_ReturnsCorrectProvider(
        MendixOtherDeployment deployment, string expected)
    {
        // Arrange
        var context = new HomePageCostContext
        {
            SelectedTechnology = Technology.Mendix,
            SelectedDeployment = DeploymentModel.VMs,
            MendixOtherDeployment = deployment
        };

        // Act
        var result = _sut.GetCostProvider(context);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(Distribution.AKS, "Azure")]
    [InlineData(Distribution.EKS, "AWS")]
    [InlineData(Distribution.GKE, "Google Cloud")]
    public void GetCostProvider_CloudDistribution_ReturnsCloudName(
        Distribution distribution, string expected)
    {
        // Arrange
        var context = new HomePageCostContext
        {
            SelectedTechnology = Technology.DotNet,
            SelectedDistribution = distribution
        };

        // Act
        var result = _sut.GetCostProvider(context);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetCostProvider_OnPremWithCostEstimate_ReturnsOnPrem()
    {
        // Arrange
        var context = new HomePageCostContext
        {
            SelectedTechnology = Technology.DotNet,
            SelectedDistribution = Distribution.OpenShift,
            K8sCostEstimate = new CostEstimate { MonthlyTotal = 5000m }
        };

        // Act
        var result = _sut.GetCostProvider(context);

        // Assert
        result.Should().Be("On-Prem");
    }

    [Fact]
    public void GetCostProvider_OnPremWithConfiguredPricing_ReturnsConfigured()
    {
        // Arrange
        // HasCosts is a computed property = !IsOnPrem || IncludePricing || MendixCost != null
        // To get HasCosts = true for on-prem, we set IncludePricing = true
        var context = new HomePageCostContext
        {
            SelectedTechnology = Technology.DotNet,
            SelectedDistribution = Distribution.OpenShift,
            PricingResult = new PricingStepResult { IsOnPrem = true, IncludePricing = true }
        };

        // Act
        var result = _sut.GetCostProvider(context);

        // Assert
        result.Should().Be("Configured");
    }

    [Fact]
    public void GetCostProvider_NoDistributionSet_ReturnsNull()
    {
        // Arrange
        var context = new HomePageCostContext
        {
            SelectedTechnology = Technology.DotNet,
            SelectedDistribution = null
        };

        // Act
        var result = _sut.GetCostProvider(context);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region ShouldShowPricingNA Tests

    [Fact]
    public void ShouldShowPricingNA_MendixTechnology_ReturnsFalse()
    {
        // Arrange
        var context = new HomePageCostContext
        {
            SelectedTechnology = Technology.Mendix,
            SelectedDistribution = Distribution.OpenShift
        };

        // Act
        var result = _sut.ShouldShowPricingNA(context);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldShowPricingNA_CloudDistribution_ReturnsFalse()
    {
        // Arrange
        _pricingSettingsService.IsOnPremDistribution(Distribution.EKS).Returns(false);
        var context = new HomePageCostContext
        {
            SelectedTechnology = Technology.DotNet,
            SelectedDistribution = Distribution.EKS
        };

        // Act
        var result = _sut.ShouldShowPricingNA(context);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldShowPricingNA_OnPremWithNoPricing_ReturnsTrue()
    {
        // Arrange
        _pricingSettingsService.IsOnPremDistribution(Distribution.OpenShift).Returns(true);
        var context = new HomePageCostContext
        {
            SelectedTechnology = Technology.DotNet,
            SelectedDistribution = Distribution.OpenShift,
            PricingResult = null
        };

        // Act
        var result = _sut.ShouldShowPricingNA(context);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldShowPricingNA_OnPremWithEmptyPricing_ReturnsTrue()
    {
        // Arrange
        _pricingSettingsService.IsOnPremDistribution(Distribution.OpenShift).Returns(true);
        // HasCosts = !IsOnPrem || IncludePricing || MendixCost != null
        // To get HasCosts = false, set IsOnPrem = true and IncludePricing = false (and no MendixCost)
        var context = new HomePageCostContext
        {
            SelectedTechnology = Technology.DotNet,
            SelectedDistribution = Distribution.OpenShift,
            PricingResult = new PricingStepResult { IsOnPrem = true, IncludePricing = false }
        };

        // Act
        var result = _sut.ShouldShowPricingNA(context);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldShowPricingNA_OnPremWithConfiguredPricing_ReturnsFalse()
    {
        // Arrange
        _pricingSettingsService.IsOnPremDistribution(Distribution.OpenShift).Returns(true);
        // HasCosts = !IsOnPrem || IncludePricing || MendixCost != null
        // To get HasCosts = true for on-prem, set IncludePricing = true
        var context = new HomePageCostContext
        {
            SelectedTechnology = Technology.DotNet,
            SelectedDistribution = Distribution.OpenShift,
            PricingResult = new PricingStepResult { IsOnPrem = true, IncludePricing = true }
        };

        // Act
        var result = _sut.ShouldShowPricingNA(context);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldShowPricingNA_NoDistributionSet_ReturnsFalse()
    {
        // Arrange
        var context = new HomePageCostContext
        {
            SelectedTechnology = Technology.DotNet,
            SelectedDistribution = null
        };

        // Act
        var result = _sut.ShouldShowPricingNA(context);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetComputeCost Tests

    [Theory]
    [InlineData("m6i.xlarge", 1, 0.192 * 730)]
    [InlineData("m6i.xlarge", 3, 0.192 * 730 * 3)]
    [InlineData("m6i.2xlarge", 1, 0.384 * 730)]
    [InlineData("m6i.4xlarge", 2, 0.768 * 730 * 2)]
    public void GetComputeCost_AWSInstances_CalculatesCorrectly(
        string instanceType, int nodeCount, decimal expected)
    {
        // Act
        var result = _sut.GetComputeCost("aws", instanceType, nodeCount);

        // Assert
        result.Should().BeApproximately(expected, 0.01m);
    }

    [Theory]
    [InlineData("Standard_D4s_v5", 1, 0.192 * 730)]
    [InlineData("Standard_D8s_v5", 2, 0.384 * 730 * 2)]
    [InlineData("Standard_D16s_v5", 3, 0.768 * 730 * 3)]
    public void GetComputeCost_AzureInstances_CalculatesCorrectly(
        string instanceType, int nodeCount, decimal expected)
    {
        // Act
        var result = _sut.GetComputeCost("azure", instanceType, nodeCount);

        // Assert
        result.Should().BeApproximately(expected, 0.01m);
    }

    [Theory]
    [InlineData("n2-standard-4", 1, 0.195 * 730)]
    [InlineData("n2-standard-8", 2, 0.389 * 730 * 2)]
    [InlineData("n2-standard-16", 4, 0.778 * 730 * 4)]
    public void GetComputeCost_GCPInstances_CalculatesCorrectly(
        string instanceType, int nodeCount, decimal expected)
    {
        // Act
        var result = _sut.GetComputeCost("gcp", instanceType, nodeCount);

        // Assert
        result.Should().BeApproximately(expected, 0.01m);
    }

    [Fact]
    public void GetComputeCost_UnknownInstanceType_UsesDefaultRate()
    {
        // Act
        var result = _sut.GetComputeCost("any", "unknown-instance", 1);

        // Assert
        result.Should().BeApproximately(0.40m * 730, 0.01m);
    }

    #endregion

    #region FormatCostPreview Tests

    [Fact]
    public void FormatCostPreview_NullAmount_ReturnsDoubleDash()
    {
        // Act
        var result = _sut.FormatCostPreview(null);

        // Assert
        result.Should().Be("--");
    }

    [Fact]
    public void FormatCostPreview_SmallAmount_FormatsWithDollar()
    {
        // Act
        var result = _sut.FormatCostPreview(500m);

        // Assert
        result.Should().Be("$500");
    }

    [Fact]
    public void FormatCostPreview_ThousandAmount_FormatsWithK()
    {
        // Act
        var result = _sut.FormatCostPreview(5000m);

        // Assert
        result.Should().Be("$5.0K");
    }

    [Fact]
    public void FormatCostPreview_MillionAmount_FormatsWithM()
    {
        // Act
        var result = _sut.FormatCostPreview(1500000m);

        // Assert
        result.Should().Be("$1.50M");
    }

    [Theory]
    [InlineData(999, "$999")]
    [InlineData(1000, "$1.0K")]
    [InlineData(1234, "$1.2K")]
    [InlineData(999999, "$1000.0K")]
    [InlineData(1000000, "$1.00M")]
    [InlineData(12345678, "$12.35M")]
    public void FormatCostPreview_VariousAmounts_FormatsCorrectly(decimal amount, string expected)
    {
        // Act
        var result = _sut.FormatCostPreview(amount);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region FormatCurrency Tests

    [Fact]
    public void FormatCurrency_SmallAmount_FormatsWithTwoDecimals()
    {
        // Act
        var result = _sut.FormatCurrency(500.25m);

        // Assert
        result.Should().Be("$500.25");
    }

    [Fact]
    public void FormatCurrency_ThousandAmount_FormatsWithK()
    {
        // Act
        var result = _sut.FormatCurrency(5000m);

        // Assert
        result.Should().Be("$5.0K");
    }

    [Fact]
    public void FormatCurrency_MillionAmount_FormatsWithM()
    {
        // Act
        var result = _sut.FormatCurrency(2500000m);

        // Assert
        result.Should().Be("$2.50M");
    }

    [Theory]
    [InlineData(0, "$0.00")]
    [InlineData(99.99, "$99.99")]
    [InlineData(999.99, "$999.99")]
    [InlineData(1000, "$1.0K")]
    [InlineData(50000, "$50.0K")]
    [InlineData(1000000, "$1.00M")]
    public void FormatCurrency_VariousAmounts_FormatsCorrectly(decimal amount, string expected)
    {
        // Act
        var result = _sut.FormatCurrency(amount);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region GetCostCategoryIcon Tests

    [Theory]
    [InlineData(CostCategory.Compute, "CPU")]
    [InlineData(CostCategory.Storage, "HDD")]
    [InlineData(CostCategory.Network, "NET")]
    [InlineData(CostCategory.License, "LIC")]
    [InlineData(CostCategory.Support, "SUP")]
    [InlineData(CostCategory.DataCenter, "DC")]
    [InlineData(CostCategory.Labor, "OPS")]
    public void GetCostCategoryIcon_KnownCategory_ReturnsCorrectIcon(
        CostCategory category, string expected)
    {
        // Act
        var result = _sut.GetCostCategoryIcon(category);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetCostCategoryIcon_UnknownCategory_ReturnsEmptyString()
    {
        // Act
        var result = _sut.GetCostCategoryIcon((CostCategory)999);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region Context Initialization Tests

    [Fact]
    public void HomePageCostContext_DefaultValues_AreCorrect()
    {
        // Act
        var context = new HomePageCostContext();

        // Assert
        context.SelectedTechnology.Should().BeNull();
        context.SelectedDeployment.Should().BeNull();
        context.SelectedDistribution.Should().BeNull();
        context.PricingResult.Should().BeNull();
        context.K8sCostEstimate.Should().BeNull();
        context.VMCostEstimate.Should().BeNull();
        context.VMResults.Should().BeNull();
        context.MendixDeploymentCategory.Should().BeNull();
        context.MendixCloudType.Should().BeNull();
        context.MendixPrivateCloudProvider.Should().BeNull();
        context.MendixOtherDeployment.Should().BeNull();
        context.MendixIsUnlimitedApps.Should().BeFalse();
        context.MendixNumberOfApps.Should().Be(0);
    }

    [Fact]
    public void HomePageCostContext_CanSetAllProperties()
    {
        // Arrange & Act
        // MonthlyCost is computed from CloudCost.MonthlyTotal (when IsOnPrem = false)
        var context = new HomePageCostContext
        {
            SelectedTechnology = Technology.Mendix,
            SelectedDeployment = DeploymentModel.Kubernetes,
            SelectedDistribution = Distribution.AKS,
            PricingResult = new PricingStepResult
            {
                IsOnPrem = false,
                CloudCost = new CostEstimate { MonthlyTotal = 5000m }
            },
            K8sCostEstimate = new CostEstimate { MonthlyTotal = 4000m },
            VMCostEstimate = new CostEstimate { MonthlyTotal = 3000m },
            VMResults = new VMSizingResult { Environments = new List<VMEnvironmentResult>() },
            MendixDeploymentCategory = MendixDeploymentCategory.Cloud,
            MendixCloudType = MendixCloudType.Dedicated,
            MendixPrivateCloudProvider = MendixPrivateCloudProvider.Azure,
            MendixOtherDeployment = MendixOtherDeployment.Server,
            MendixIsUnlimitedApps = true,
            MendixNumberOfApps = 50
        };

        // Assert
        context.SelectedTechnology.Should().Be(Technology.Mendix);
        context.SelectedDeployment.Should().Be(DeploymentModel.Kubernetes);
        context.SelectedDistribution.Should().Be(Distribution.AKS);
        context.PricingResult.Should().NotBeNull();
        context.PricingResult!.MonthlyCost.Should().Be(5000m);
        context.K8sCostEstimate.Should().NotBeNull();
        context.VMCostEstimate.Should().NotBeNull();
        context.VMResults.Should().NotBeNull();
        context.MendixDeploymentCategory.Should().Be(MendixDeploymentCategory.Cloud);
        context.MendixCloudType.Should().Be(MendixCloudType.Dedicated);
        context.MendixPrivateCloudProvider.Should().Be(MendixPrivateCloudProvider.Azure);
        context.MendixOtherDeployment.Should().Be(MendixOtherDeployment.Server);
        context.MendixIsUnlimitedApps.Should().BeTrue();
        context.MendixNumberOfApps.Should().Be(50);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void GetMonthlyEstimate_MendixVMs_WithNullVMCostEstimate_UsesZeroInfraCost()
    {
        // Arrange
        var context = new HomePageCostContext
        {
            SelectedTechnology = Technology.Mendix,
            SelectedDeployment = DeploymentModel.VMs,
            VMResults = new VMSizingResult { Environments = new List<VMEnvironmentResult>() },
            VMCostEstimate = null,
            MendixOtherDeployment = MendixOtherDeployment.Server,
            MendixIsUnlimitedApps = false,
            MendixNumberOfApps = 1
        };

        // Act
        var result = _sut.GetMonthlyEstimate(context);

        // Assert
        // Only license cost: 8000 * 1 / 12 = 666.67
        result.Should().BeApproximately(8000m / 12, 0.01m);
    }

    [Fact]
    public void GetMonthlyEstimate_MendixVMs_WithNullOtherDeployment_UsesServerDefault()
    {
        // Arrange
        var context = new HomePageCostContext
        {
            SelectedTechnology = Technology.Mendix,
            SelectedDeployment = DeploymentModel.VMs,
            VMResults = new VMSizingResult { Environments = new List<VMEnvironmentResult>() },
            VMCostEstimate = new CostEstimate { MonthlyTotal = 1000m },
            MendixOtherDeployment = null, // Null - should default to Server
            MendixIsUnlimitedApps = false,
            MendixNumberOfApps = 2
        };

        // Act
        var result = _sut.GetMonthlyEstimate(context);

        // Assert
        // Infra: 1000, License: 8000 * 2 / 12 = 1333.33
        result.Should().BeApproximately(1000m + (8000m * 2 / 12), 0.01m);
    }

    [Fact]
    public void GetMendixVMLicenseMonthly_ZeroApps_ReturnsZero()
    {
        // Act
        var result = _sut.GetMendixVMLicenseMonthly(MendixOtherDeployment.Server, false, 0);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void GetCostProvider_MendixPrivateCloud_NullProvider_ReturnsMendix()
    {
        // Arrange
        var context = new HomePageCostContext
        {
            SelectedTechnology = Technology.Mendix,
            SelectedDeployment = DeploymentModel.Kubernetes,
            MendixDeploymentCategory = MendixDeploymentCategory.PrivateCloud,
            MendixPrivateCloudProvider = null
        };

        // Act
        var result = _sut.GetCostProvider(context);

        // Assert
        result.Should().Be("Mendix");
    }

    [Fact]
    public void GetCostProvider_MendixVMs_NullDeployment_ReturnsMendixVM()
    {
        // Arrange
        var context = new HomePageCostContext
        {
            SelectedTechnology = Technology.Mendix,
            SelectedDeployment = DeploymentModel.VMs,
            MendixOtherDeployment = null
        };

        // Act
        var result = _sut.GetCostProvider(context);

        // Assert
        result.Should().Be("Mendix VM");
    }

    [Fact]
    public void GetCostProvider_MendixK8s_NoCategory_ReturnsNull()
    {
        // Arrange
        var context = new HomePageCostContext
        {
            SelectedTechnology = Technology.Mendix,
            SelectedDeployment = DeploymentModel.Kubernetes,
            MendixDeploymentCategory = null
        };

        // Act
        var result = _sut.GetCostProvider(context);

        // Assert
        // No matching category, falls through to null
        result.Should().BeNull();
    }

    #endregion
}
