using FluentAssertions;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Models.Pricing;
using InfraSizingCalculator.Services.Interfaces;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Models.Pricing;

// Note: OnPremCostBreakdown is defined in InfraSizingCalculator.Services.Interfaces.IPricingSettingsService
// Note: MendixPricingResult is defined in InfraSizingCalculator.Models.Pricing.MendixPricing

/// <summary>
/// Tests for pricing model classes - PricingStepResult, CostEstimate, CostBreakdown, etc.
/// </summary>
public class PricingModelsTests
{
    #region PricingStepResult Tests

    [Fact]
    public void PricingStepResult_HasCosts_ReturnsFalse_WhenOnPremWithoutToggle()
    {
        // Arrange
        var result = new PricingStepResult
        {
            IsOnPrem = true,
            IncludePricing = false,
            MendixCost = null
        };

        // Act & Assert
        result.HasCosts.Should().BeFalse();
    }

    [Fact]
    public void PricingStepResult_HasCosts_ReturnsTrue_WhenOnPremWithToggle()
    {
        // Arrange
        var result = new PricingStepResult
        {
            IsOnPrem = true,
            IncludePricing = true
        };

        // Act & Assert
        result.HasCosts.Should().BeTrue();
    }

    [Fact]
    public void PricingStepResult_HasCosts_ReturnsTrue_WhenCloud()
    {
        // Arrange
        var result = new PricingStepResult
        {
            IsOnPrem = false,
            IncludePricing = false // Doesn't matter for cloud
        };

        // Act & Assert
        result.HasCosts.Should().BeTrue();
    }

    [Fact]
    public void PricingStepResult_HasCosts_ReturnsTrue_WhenHasMendixCost()
    {
        // Arrange
        var result = new PricingStepResult
        {
            IsOnPrem = true,
            IncludePricing = false,
            MendixCost = new MendixPricingResult() // Even with toggle off, Mendix costs count
        };

        // Act & Assert
        result.HasCosts.Should().BeTrue();
    }

    [Fact]
    public void PricingStepResult_MonthlyCost_ReturnsNull_WhenNoCosts()
    {
        // Arrange
        var result = new PricingStepResult
        {
            IsOnPrem = true,
            IncludePricing = false
        };

        // Act & Assert
        result.MonthlyCost.Should().BeNull();
    }

    [Fact]
    public void PricingStepResult_MonthlyCost_ReturnsOnPremCost_WhenOnPrem()
    {
        // Arrange
        var result = new PricingStepResult
        {
            IsOnPrem = true,
            IncludePricing = true,
            // OnPremCostBreakdown.MonthlyTotal is computed from its components
            OnPremCost = new OnPremCostBreakdown
            {
                MonthlyHardware = 2000m,
                MonthlyDataCenter = 1000m,
                MonthlyLabor = 1500m,
                MonthlyLicense = 500m
            } // MonthlyTotal = 5000m
        };

        // Act & Assert
        result.MonthlyCost.Should().Be(5000m);
    }

    [Fact]
    public void PricingStepResult_MonthlyCost_ReturnsCloudCost_WhenCloud()
    {
        // Arrange
        var result = new PricingStepResult
        {
            IsOnPrem = false,
            CloudCost = new CostEstimate { MonthlyTotal = 8000m }
        };

        // Act & Assert
        result.MonthlyCost.Should().Be(8000m);
    }

    [Fact]
    public void PricingStepResult_YearlyCost_CalculatesCorrectly_ForOnPrem()
    {
        // Arrange - YearlyTotal is MonthlyTotal * 12
        var result = new PricingStepResult
        {
            IsOnPrem = true,
            IncludePricing = true,
            OnPremCost = new OnPremCostBreakdown
            {
                MonthlyHardware = 2000m,
                MonthlyDataCenter = 1000m,
                MonthlyLabor = 1500m,
                MonthlyLicense = 500m
            } // MonthlyTotal = 5000m, YearlyTotal = 60000m
        };

        // Act & Assert
        result.YearlyCost.Should().Be(60000m);
    }

    [Fact]
    public void PricingStepResult_ThreeYearTCO_CalculatesCorrectly_ForCloud()
    {
        // Arrange
        var result = new PricingStepResult
        {
            IsOnPrem = false,
            CloudCost = new CostEstimate { MonthlyTotal = 1000m } // YearlyTotal = 12000, 3-year = 36000
        };

        // Act & Assert
        result.ThreeYearTCO.Should().Be(36000m);
    }

    [Fact]
    public void PricingStepResult_FormatCost_ReturnsNA_WhenNull()
    {
        // Act
        var formatted = PricingStepResult.FormatCost(null);

        // Assert
        formatted.Should().Be("N/A");
    }

    [Fact]
    public void PricingStepResult_FormatCost_FormatsZero()
    {
        // Act
        var formatted = PricingStepResult.FormatCost(0m);

        // Assert
        formatted.Should().Be("$0");
    }

    [Fact]
    public void PricingStepResult_FormatCost_RoundsToNearestDollar()
    {
        // Act
        var formatted = PricingStepResult.FormatCost(1234.56m);

        // Assert
        formatted.Should().Be("$1,235");
    }

    [Fact]
    public void PricingStepResult_FormatCost_FormatsLargeNumbers()
    {
        // Act
        var formatted = PricingStepResult.FormatCost(999999.99m);

        // Assert
        formatted.Should().Be("$1,000,000");
    }

    #endregion

    #region CostEstimate Tests

    [Fact]
    public void CostEstimate_YearlyTotal_IsMonthlyTimes12()
    {
        // Arrange
        var estimate = new CostEstimate { MonthlyTotal = 1000m };

        // Act & Assert
        estimate.YearlyTotal.Should().Be(12000m);
    }

    [Fact]
    public void CostEstimate_ThreeYearTCO_IsYearlyTimes3()
    {
        // Arrange
        var estimate = new CostEstimate { MonthlyTotal = 1000m };

        // Act & Assert
        estimate.ThreeYearTCO.Should().Be(36000m);
    }

    [Fact]
    public void CostEstimate_FiveYearTCO_IsYearlyTimes5()
    {
        // Arrange
        var estimate = new CostEstimate { MonthlyTotal = 1000m };

        // Act & Assert
        estimate.FiveYearTCO.Should().Be(60000m);
    }

    [Fact]
    public void CostEstimate_ComputeCost_ReturnsZero_WhenNoBreakdown()
    {
        // Arrange
        var estimate = new CostEstimate();

        // Act & Assert
        estimate.ComputeCost.Should().Be(0m);
    }

    [Fact]
    public void CostEstimate_ComputeCost_ReturnsValue_WhenBreakdownExists()
    {
        // Arrange
        var estimate = new CostEstimate
        {
            Breakdown = new Dictionary<CostCategory, CostBreakdown>
            {
                { CostCategory.Compute, new CostBreakdown { Monthly = 500m } }
            }
        };

        // Act & Assert
        estimate.ComputeCost.Should().Be(500m);
    }

    [Fact]
    public void CostEstimate_AllCostAccessors_ReturnCorrectValues()
    {
        // Arrange
        var estimate = new CostEstimate
        {
            Breakdown = new Dictionary<CostCategory, CostBreakdown>
            {
                { CostCategory.Compute, new CostBreakdown { Monthly = 100m } },
                { CostCategory.Storage, new CostBreakdown { Monthly = 200m } },
                { CostCategory.Network, new CostBreakdown { Monthly = 50m } },
                { CostCategory.License, new CostBreakdown { Monthly = 300m } },
                { CostCategory.Support, new CostBreakdown { Monthly = 150m } }
            }
        };

        // Act & Assert
        estimate.ComputeCost.Should().Be(100m);
        estimate.StorageCost.Should().Be(200m);
        estimate.NetworkCost.Should().Be(50m);
        estimate.LicenseCost.Should().Be(300m);
        estimate.SupportCost.Should().Be(150m);
    }

    [Fact]
    public void CostEstimate_DefaultValues_AreCorrect()
    {
        // Act
        var estimate = new CostEstimate();

        // Assert
        estimate.Currency.Should().Be(Currency.USD);
        estimate.Region.Should().BeEmpty();
        estimate.PricingSource.Should().BeEmpty();
        estimate.Notes.Should().BeEmpty();
        estimate.Breakdown.Should().BeEmpty();
        estimate.EnvironmentCosts.Should().BeEmpty();
    }

    #endregion

    #region CostBreakdown Tests

    [Fact]
    public void CostBreakdown_Yearly_IsMonthlyTimes12()
    {
        // Arrange
        var breakdown = new CostBreakdown { Monthly = 250m };

        // Act & Assert
        breakdown.Yearly.Should().Be(3000m);
    }

    [Fact]
    public void CostBreakdown_DefaultValues_AreCorrect()
    {
        // Act
        var breakdown = new CostBreakdown();

        // Assert
        breakdown.Monthly.Should().Be(0m);
        breakdown.Percentage.Should().Be(0m);
        breakdown.LineItems.Should().BeEmpty();
        breakdown.Description.Should().BeEmpty();
    }

    #endregion

    #region CostLineItem Tests

    [Fact]
    public void CostLineItem_Total_CalculatesCorrectly()
    {
        // Arrange
        var lineItem = new CostLineItem
        {
            Quantity = 10,
            UnitPrice = 5.50m
        };

        // Act & Assert
        lineItem.Total.Should().Be(55m);
    }

    [Fact]
    public void CostLineItem_Total_HandlesZeroQuantity()
    {
        // Arrange
        var lineItem = new CostLineItem
        {
            Quantity = 0,
            UnitPrice = 100m
        };

        // Act & Assert
        lineItem.Total.Should().Be(0m);
    }

    [Fact]
    public void CostLineItem_Total_HandlesFractionalValues()
    {
        // Arrange
        var lineItem = new CostLineItem
        {
            Quantity = 2.5m,
            UnitPrice = 0.04m
        };

        // Act & Assert
        lineItem.Total.Should().Be(0.1m);
    }

    [Fact]
    public void CostLineItem_DefaultValues_AreCorrect()
    {
        // Act
        var lineItem = new CostLineItem();

        // Assert
        lineItem.Description.Should().BeEmpty();
        lineItem.Unit.Should().BeEmpty();
        lineItem.Quantity.Should().Be(0m);
        lineItem.UnitPrice.Should().Be(0m);
        lineItem.Notes.Should().BeNull();
    }

    #endregion

    #region EnvironmentCost Tests

    [Fact]
    public void EnvironmentCost_CostPerNode_CalculatesCorrectly()
    {
        // Arrange
        var envCost = new EnvironmentCost
        {
            MonthlyCost = 1000m,
            Nodes = 4
        };

        // Act & Assert
        envCost.CostPerNode.Should().Be(250m);
    }

    [Fact]
    public void EnvironmentCost_CostPerNode_ReturnsZero_WhenNoNodes()
    {
        // Arrange
        var envCost = new EnvironmentCost
        {
            MonthlyCost = 1000m,
            Nodes = 0
        };

        // Act & Assert
        envCost.CostPerNode.Should().Be(0m);
    }

    [Fact]
    public void EnvironmentCost_DefaultValues_AreCorrect()
    {
        // Act
        var envCost = new EnvironmentCost();

        // Assert
        envCost.EnvironmentName.Should().BeEmpty();
        envCost.MonthlyCost.Should().Be(0m);
        envCost.Percentage.Should().Be(0m);
        envCost.Nodes.Should().Be(0);
        envCost.TotalCpu.Should().Be(0);
        envCost.TotalRamGB.Should().Be(0);
        envCost.TotalDiskGB.Should().Be(0);
    }

    #endregion

    #region CostComparison Tests

    [Fact]
    public void CostComparison_DefaultValues_AreCorrect()
    {
        // Act
        var comparison = new CostComparison();

        // Assert
        comparison.Estimates.Should().BeEmpty();
        comparison.CheapestOption.Should().BeNull();
        comparison.MostExpensiveOption.Should().BeNull();
        comparison.PotentialSavings.Should().BeEmpty();
        comparison.Insights.Should().BeEmpty();
    }

    [Fact]
    public void CostComparison_CanStoreMultipleEstimates()
    {
        // Arrange
        var comparison = new CostComparison
        {
            Estimates = new List<CostEstimate>
            {
                new() { MonthlyTotal = 1000m, Provider = CloudProvider.AWS },
                new() { MonthlyTotal = 1200m, Provider = CloudProvider.Azure },
                new() { MonthlyTotal = 950m, Provider = CloudProvider.GCP }
            }
        };

        // Assert
        comparison.Estimates.Should().HaveCount(3);
    }

    [Fact]
    public void CostComparison_CanTrackCheapestAndMostExpensive()
    {
        // Arrange
        var cheapest = new CostEstimate { MonthlyTotal = 500m };
        var expensive = new CostEstimate { MonthlyTotal = 2000m };
        var comparison = new CostComparison
        {
            CheapestOption = cheapest,
            MostExpensiveOption = expensive
        };

        // Assert
        comparison.CheapestOption!.MonthlyTotal.Should().Be(500m);
        comparison.MostExpensiveOption!.MonthlyTotal.Should().Be(2000m);
    }

    #endregion

    #region AwsPricing Tests

    [Fact]
    public void AwsPricing_DefaultValues_AreCorrect()
    {
        // Act
        var pricing = new AwsPricing();

        // Assert
        pricing.EksControlPlanePerHour.Should().Be(0.10m);
        pricing.M5XlargePerHour.Should().Be(0.192m);
        pricing.M52XlargePerHour.Should().Be(0.384m);
        pricing.M54XlargePerHour.Should().Be(0.768m);
        pricing.EbsGp3PerGBMonth.Should().Be(0.08m);
        pricing.RosaPerWorkerHour.Should().Be(0.171m);
    }

    [Fact]
    public void AwsPricing_GetMonthlyControlPlaneCost_CalculatesCorrectly()
    {
        // Arrange
        var pricing = new AwsPricing();

        // Act
        var cost = pricing.GetMonthlyControlPlaneCost();

        // Assert - 0.10 * 730 = 73
        cost.Should().Be(73m);
    }

    [Fact]
    public void AwsPricing_GetMonthlyControlPlaneCost_UsesCustomHourlyRate()
    {
        // Arrange
        var pricing = new AwsPricing { EksControlPlanePerHour = 0.20m };

        // Act
        var cost = pricing.GetMonthlyControlPlaneCost();

        // Assert - 0.20 * 730 = 146
        cost.Should().Be(146m);
    }

    [Theory]
    [InlineData("m5.xlarge", 1, 140.16)]   // 0.192 * 730 * 1
    [InlineData("m5.xlarge", 3, 420.48)]   // 0.192 * 730 * 3
    [InlineData("m5.2xlarge", 1, 280.32)]  // 0.384 * 730 * 1
    [InlineData("m5.2xlarge", 2, 560.64)]  // 0.384 * 730 * 2
    [InlineData("m5.4xlarge", 1, 560.64)]  // 0.768 * 730 * 1
    [InlineData("m5.4xlarge", 4, 2242.56)] // 0.768 * 730 * 4
    public void AwsPricing_GetMonthlyInstanceCost_CalculatesForKnownTypes(string type, int count, decimal expected)
    {
        // Arrange
        var pricing = new AwsPricing();

        // Act
        var cost = pricing.GetMonthlyInstanceCost(type, count);

        // Assert
        cost.Should().Be(expected);
    }

    [Theory]
    [InlineData("m5.8xlarge")]
    [InlineData("c5.large")]
    [InlineData("unknown")]
    [InlineData("")]
    public void AwsPricing_GetMonthlyInstanceCost_ReturnsM5XlargeForUnknownTypes(string type)
    {
        // Arrange
        var pricing = new AwsPricing();

        // Act
        var cost = pricing.GetMonthlyInstanceCost(type, 1);

        // Assert - defaults to m5.xlarge: 0.192 * 730 = 140.16
        cost.Should().Be(140.16m);
    }

    #endregion

    #region AzurePricing Tests

    [Fact]
    public void AzurePricing_DefaultValues_AreCorrect()
    {
        // Act
        var pricing = new AzurePricing();

        // Assert
        pricing.AksControlPlanePerHour.Should().Be(0m); // Free tier
        pricing.D4sV3PerHour.Should().Be(0.192m);
        pricing.D8sV3PerHour.Should().Be(0.384m);
        pricing.D16sV3PerHour.Should().Be(0.768m);
        pricing.ManagedDiskPremiumPerGBMonth.Should().Be(0.12m);
        pricing.AroPerWorkerHour.Should().Be(0.35m);
    }

    [Fact]
    public void AzurePricing_GetMonthlyControlPlaneCost_ReturnsZeroForFreeTier()
    {
        // Arrange
        var pricing = new AzurePricing();

        // Act
        var cost = pricing.GetMonthlyControlPlaneCost();

        // Assert - 0 * 730 = 0
        cost.Should().Be(0m);
    }

    [Fact]
    public void AzurePricing_GetMonthlyControlPlaneCost_UsesCustomHourlyRate()
    {
        // Arrange - simulate paid tier
        var pricing = new AzurePricing { AksControlPlanePerHour = 0.10m };

        // Act
        var cost = pricing.GetMonthlyControlPlaneCost();

        // Assert - 0.10 * 730 = 73
        cost.Should().Be(73m);
    }

    [Theory]
    [InlineData("D4s_v3", 1, 140.16)]   // 0.192 * 730 * 1
    [InlineData("D4s_v3", 3, 420.48)]   // 0.192 * 730 * 3
    [InlineData("D8s_v3", 1, 280.32)]   // 0.384 * 730 * 1
    [InlineData("D8s_v3", 2, 560.64)]   // 0.384 * 730 * 2
    [InlineData("D16s_v3", 1, 560.64)]  // 0.768 * 730 * 1
    [InlineData("D16s_v3", 4, 2242.56)] // 0.768 * 730 * 4
    public void AzurePricing_GetMonthlyInstanceCost_CalculatesForKnownTypes(string type, int count, decimal expected)
    {
        // Arrange
        var pricing = new AzurePricing();

        // Act
        var cost = pricing.GetMonthlyInstanceCost(type, count);

        // Assert
        cost.Should().Be(expected);
    }

    [Theory]
    [InlineData("D32s_v3")]
    [InlineData("E4s_v3")]
    [InlineData("unknown")]
    [InlineData("")]
    public void AzurePricing_GetMonthlyInstanceCost_ReturnsD4sV3ForUnknownTypes(string type)
    {
        // Arrange
        var pricing = new AzurePricing();

        // Act
        var cost = pricing.GetMonthlyInstanceCost(type, 1);

        // Assert - defaults to D4s_v3: 0.192 * 730 = 140.16
        cost.Should().Be(140.16m);
    }

    #endregion

    #region GcpPricing Tests

    [Fact]
    public void GcpPricing_DefaultValues_AreCorrect()
    {
        // Act
        var pricing = new GcpPricing();

        // Assert
        pricing.GkeControlPlanePerHour.Should().Be(0.10m);
        pricing.E2Standard4PerHour.Should().Be(0.134m);
        pricing.E2Standard8PerHour.Should().Be(0.268m);
        pricing.E2Standard16PerHour.Should().Be(0.536m);
        pricing.PdSsdPerGBMonth.Should().Be(0.17m);
        pricing.OsdPerWorkerHour.Should().Be(0.171m);
    }

    [Fact]
    public void GcpPricing_GetMonthlyControlPlaneCost_CalculatesCorrectly()
    {
        // Arrange
        var pricing = new GcpPricing();

        // Act
        var cost = pricing.GetMonthlyControlPlaneCost();

        // Assert - 0.10 * 730 = 73
        cost.Should().Be(73m);
    }

    [Fact]
    public void GcpPricing_GetMonthlyControlPlaneCost_UsesCustomHourlyRate()
    {
        // Arrange
        var pricing = new GcpPricing { GkeControlPlanePerHour = 0.15m };

        // Act
        var cost = pricing.GetMonthlyControlPlaneCost();

        // Assert - 0.15 * 730 = 109.50
        cost.Should().Be(109.50m);
    }

    [Theory]
    [InlineData("e2-standard-4", 1, 97.82)]    // 0.134 * 730 * 1
    [InlineData("e2-standard-4", 3, 293.46)]   // 0.134 * 730 * 3
    [InlineData("e2-standard-8", 1, 195.64)]   // 0.268 * 730 * 1
    [InlineData("e2-standard-8", 2, 391.28)]   // 0.268 * 730 * 2
    [InlineData("e2-standard-16", 1, 391.28)]  // 0.536 * 730 * 1
    [InlineData("e2-standard-16", 4, 1565.12)] // 0.536 * 730 * 4
    public void GcpPricing_GetMonthlyInstanceCost_CalculatesForKnownTypes(string type, int count, decimal expected)
    {
        // Arrange
        var pricing = new GcpPricing();

        // Act
        var cost = pricing.GetMonthlyInstanceCost(type, count);

        // Assert
        cost.Should().Be(expected);
    }

    [Theory]
    [InlineData("e2-standard-32")]
    [InlineData("n2-standard-4")]
    [InlineData("unknown")]
    [InlineData("")]
    public void GcpPricing_GetMonthlyInstanceCost_ReturnsE2Standard4ForUnknownTypes(string type)
    {
        // Arrange
        var pricing = new GcpPricing();

        // Act
        var cost = pricing.GetMonthlyInstanceCost(type, 1);

        // Assert - defaults to e2-standard-4: 0.134 * 730 = 97.82
        cost.Should().Be(97.82m);
    }

    #endregion

    #region CloudPricingSettings Tests

    [Fact]
    public void CloudPricingSettings_DefaultValues_AreInitialized()
    {
        // Act
        var settings = new CloudPricingSettings();

        // Assert
        settings.Aws.Should().NotBeNull();
        settings.Azure.Should().NotBeNull();
        settings.Gcp.Should().NotBeNull();
    }

    [Fact]
    public void CloudPricingSettings_CanAccessAllProviderPricing()
    {
        // Arrange
        var settings = new CloudPricingSettings();

        // Act & Assert - verify we can access nested properties
        settings.Aws.EksControlPlanePerHour.Should().Be(0.10m);
        settings.Azure.AksControlPlanePerHour.Should().Be(0m);
        settings.Gcp.GkeControlPlanePerHour.Should().Be(0.10m);
    }

    #endregion
}
