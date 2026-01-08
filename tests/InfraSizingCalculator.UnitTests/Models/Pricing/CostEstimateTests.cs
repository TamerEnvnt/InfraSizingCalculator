using FluentAssertions;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Models.Pricing;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Models.Pricing;

/// <summary>
/// Tests for CostEstimate, CostBreakdown, CostLineItem, EnvironmentCost, and CostComparison.
/// </summary>
public class CostEstimateTests
{
    #region CostEstimate Default Values

    [Fact]
    public void CostEstimate_DefaultRegion_IsEmptyString()
    {
        var estimate = new CostEstimate();
        estimate.Region.Should().BeEmpty();
    }

    [Fact]
    public void CostEstimate_DefaultCurrency_IsUSD()
    {
        var estimate = new CostEstimate();
        estimate.Currency.Should().Be(Currency.USD);
    }

    [Fact]
    public void CostEstimate_DefaultMonthlyTotal_IsZero()
    {
        var estimate = new CostEstimate();
        estimate.MonthlyTotal.Should().Be(0);
    }

    [Fact]
    public void CostEstimate_DefaultBreakdown_IsEmpty()
    {
        var estimate = new CostEstimate();
        estimate.Breakdown.Should().NotBeNull();
        estimate.Breakdown.Should().BeEmpty();
    }

    [Fact]
    public void CostEstimate_DefaultEnvironmentCosts_IsEmpty()
    {
        var estimate = new CostEstimate();
        estimate.EnvironmentCosts.Should().NotBeNull();
        estimate.EnvironmentCosts.Should().BeEmpty();
    }

    [Fact]
    public void CostEstimate_DefaultPricingSource_IsEmptyString()
    {
        var estimate = new CostEstimate();
        estimate.PricingSource.Should().BeEmpty();
    }

    [Fact]
    public void CostEstimate_DefaultNotes_IsEmpty()
    {
        var estimate = new CostEstimate();
        estimate.Notes.Should().NotBeNull();
        estimate.Notes.Should().BeEmpty();
    }

    [Fact]
    public void CostEstimate_DefaultCalculatedAt_IsRecentUtc()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var estimate = new CostEstimate();
        var after = DateTime.UtcNow.AddSeconds(1);

        estimate.CalculatedAt.Should().BeAfter(before);
        estimate.CalculatedAt.Should().BeBefore(after);
    }

    #endregion

    #region CostEstimate Computed Properties

    [Theory]
    [InlineData(0, 0)]
    [InlineData(100, 1200)]
    [InlineData(1000, 12000)]
    [InlineData(8333.33, 99999.96)]
    public void CostEstimate_YearlyTotal_IsMonthlyTimes12(decimal monthly, decimal expectedYearly)
    {
        var estimate = new CostEstimate { MonthlyTotal = monthly };
        estimate.YearlyTotal.Should().Be(expectedYearly);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(100, 3600)]
    [InlineData(1000, 36000)]
    public void CostEstimate_ThreeYearTCO_IsYearlyTimes3(decimal monthly, decimal expectedTCO)
    {
        var estimate = new CostEstimate { MonthlyTotal = monthly };
        estimate.ThreeYearTCO.Should().Be(expectedTCO);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(100, 6000)]
    [InlineData(1000, 60000)]
    public void CostEstimate_FiveYearTCO_IsYearlyTimes5(decimal monthly, decimal expectedTCO)
    {
        var estimate = new CostEstimate { MonthlyTotal = monthly };
        estimate.FiveYearTCO.Should().Be(expectedTCO);
    }

    #endregion

    #region CostEstimate Category Cost Properties

    [Fact]
    public void ComputeCost_WhenBreakdownEmpty_ReturnsZero()
    {
        var estimate = new CostEstimate();
        estimate.ComputeCost.Should().Be(0);
    }

    [Fact]
    public void ComputeCost_WhenBreakdownHasCompute_ReturnsCorrectValue()
    {
        var estimate = new CostEstimate
        {
            Breakdown = new Dictionary<CostCategory, CostBreakdown>
            {
                { CostCategory.Compute, new CostBreakdown { Monthly = 500m } }
            }
        };
        estimate.ComputeCost.Should().Be(500m);
    }

    [Fact]
    public void StorageCost_WhenBreakdownHasStorage_ReturnsCorrectValue()
    {
        var estimate = new CostEstimate
        {
            Breakdown = new Dictionary<CostCategory, CostBreakdown>
            {
                { CostCategory.Storage, new CostBreakdown { Monthly = 200m } }
            }
        };
        estimate.StorageCost.Should().Be(200m);
    }

    [Fact]
    public void NetworkCost_WhenBreakdownHasNetwork_ReturnsCorrectValue()
    {
        var estimate = new CostEstimate
        {
            Breakdown = new Dictionary<CostCategory, CostBreakdown>
            {
                { CostCategory.Network, new CostBreakdown { Monthly = 150m } }
            }
        };
        estimate.NetworkCost.Should().Be(150m);
    }

    [Fact]
    public void LicenseCost_WhenBreakdownHasLicense_ReturnsCorrectValue()
    {
        var estimate = new CostEstimate
        {
            Breakdown = new Dictionary<CostCategory, CostBreakdown>
            {
                { CostCategory.License, new CostBreakdown { Monthly = 300m } }
            }
        };
        estimate.LicenseCost.Should().Be(300m);
    }

    [Fact]
    public void SupportCost_WhenBreakdownHasSupport_ReturnsCorrectValue()
    {
        var estimate = new CostEstimate
        {
            Breakdown = new Dictionary<CostCategory, CostBreakdown>
            {
                { CostCategory.Support, new CostBreakdown { Monthly = 100m } }
            }
        };
        estimate.SupportCost.Should().Be(100m);
    }

    [Fact]
    public void AllCategoryCosts_WithFullBreakdown_ReturnCorrectValues()
    {
        var estimate = new CostEstimate
        {
            Breakdown = new Dictionary<CostCategory, CostBreakdown>
            {
                { CostCategory.Compute, new CostBreakdown { Monthly = 500m } },
                { CostCategory.Storage, new CostBreakdown { Monthly = 200m } },
                { CostCategory.Network, new CostBreakdown { Monthly = 150m } },
                { CostCategory.License, new CostBreakdown { Monthly = 300m } },
                { CostCategory.Support, new CostBreakdown { Monthly = 100m } }
            }
        };

        estimate.ComputeCost.Should().Be(500m);
        estimate.StorageCost.Should().Be(200m);
        estimate.NetworkCost.Should().Be(150m);
        estimate.LicenseCost.Should().Be(300m);
        estimate.SupportCost.Should().Be(100m);
    }

    #endregion

    #region CostEstimate Property Setting

    [Fact]
    public void CostEstimate_ProviderCanBeSet()
    {
        var estimate = new CostEstimate { Provider = CloudProvider.AWS };
        estimate.Provider.Should().Be(CloudProvider.AWS);
    }

    [Fact]
    public void CostEstimate_RegionCanBeSet()
    {
        var estimate = new CostEstimate { Region = "us-east-1" };
        estimate.Region.Should().Be("us-east-1");
    }

    [Fact]
    public void CostEstimate_PricingTypeCanBeSet()
    {
        var estimate = new CostEstimate { PricingType = PricingType.Reserved1Year };
        estimate.PricingType.Should().Be(PricingType.Reserved1Year);
    }

    [Fact]
    public void CostEstimate_CurrencyCanBeSet()
    {
        var estimate = new CostEstimate { Currency = Currency.EUR };
        estimate.Currency.Should().Be(Currency.EUR);
    }

    [Fact]
    public void CostEstimate_NotesCanBeModified()
    {
        var estimate = new CostEstimate();
        estimate.Notes.Add("Test note");
        estimate.Notes.Should().Contain("Test note");
    }

    #endregion

    #region CostBreakdown Tests

    [Fact]
    public void CostBreakdown_DefaultMonthly_IsZero()
    {
        var breakdown = new CostBreakdown();
        breakdown.Monthly.Should().Be(0);
    }

    [Fact]
    public void CostBreakdown_DefaultDescription_IsEmpty()
    {
        var breakdown = new CostBreakdown();
        breakdown.Description.Should().BeEmpty();
    }

    [Fact]
    public void CostBreakdown_DefaultLineItems_IsEmpty()
    {
        var breakdown = new CostBreakdown();
        breakdown.LineItems.Should().NotBeNull();
        breakdown.LineItems.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(100, 1200)]
    [InlineData(500, 6000)]
    [InlineData(99.99, 1199.88)]
    public void CostBreakdown_Yearly_IsMonthlyTimes12(decimal monthly, decimal expectedYearly)
    {
        var breakdown = new CostBreakdown { Monthly = monthly };
        breakdown.Yearly.Should().Be(expectedYearly);
    }

    [Fact]
    public void CostBreakdown_CategoryCanBeSet()
    {
        var breakdown = new CostBreakdown { Category = CostCategory.Compute };
        breakdown.Category.Should().Be(CostCategory.Compute);
    }

    [Fact]
    public void CostBreakdown_PercentageCanBeSet()
    {
        var breakdown = new CostBreakdown { Percentage = 45.5m };
        breakdown.Percentage.Should().Be(45.5m);
    }

    #endregion

    #region CostLineItem Tests

    [Fact]
    public void CostLineItem_DefaultDescription_IsEmpty()
    {
        var item = new CostLineItem();
        item.Description.Should().BeEmpty();
    }

    [Fact]
    public void CostLineItem_DefaultQuantity_IsZero()
    {
        var item = new CostLineItem();
        item.Quantity.Should().Be(0);
    }

    [Fact]
    public void CostLineItem_DefaultUnit_IsEmpty()
    {
        var item = new CostLineItem();
        item.Unit.Should().BeEmpty();
    }

    [Fact]
    public void CostLineItem_DefaultUnitPrice_IsZero()
    {
        var item = new CostLineItem();
        item.UnitPrice.Should().Be(0);
    }

    [Fact]
    public void CostLineItem_DefaultNotes_IsNull()
    {
        var item = new CostLineItem();
        item.Notes.Should().BeNull();
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(1, 100, 100)]
    [InlineData(10, 50, 500)]
    [InlineData(5, 0, 0)]
    [InlineData(0, 100, 0)]
    [InlineData(2.5, 40, 100)]
    public void CostLineItem_Total_IsQuantityTimesUnitPrice(decimal quantity, decimal unitPrice, decimal expectedTotal)
    {
        var item = new CostLineItem { Quantity = quantity, UnitPrice = unitPrice };
        item.Total.Should().Be(expectedTotal);
    }

    [Fact]
    public void CostLineItem_AllPropertiesCanBeSet()
    {
        var item = new CostLineItem
        {
            Description = "Worker Nodes",
            Quantity = 10,
            Unit = "nodes",
            UnitPrice = 250.00m,
            Notes = "Standard worker nodes"
        };

        item.Description.Should().Be("Worker Nodes");
        item.Quantity.Should().Be(10);
        item.Unit.Should().Be("nodes");
        item.UnitPrice.Should().Be(250.00m);
        item.Notes.Should().Be("Standard worker nodes");
        item.Total.Should().Be(2500.00m);
    }

    #endregion

    #region EnvironmentCost Tests

    [Fact]
    public void EnvironmentCost_DefaultEnvironmentName_IsEmpty()
    {
        var cost = new EnvironmentCost();
        cost.EnvironmentName.Should().BeEmpty();
    }

    [Fact]
    public void EnvironmentCost_DefaultMonthlyCost_IsZero()
    {
        var cost = new EnvironmentCost();
        cost.MonthlyCost.Should().Be(0);
    }

    [Fact]
    public void EnvironmentCost_DefaultNodes_IsZero()
    {
        var cost = new EnvironmentCost();
        cost.Nodes.Should().Be(0);
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(1000, 0, 0)]
    [InlineData(1000, 5, 200)]
    [InlineData(3000, 10, 300)]
    [InlineData(500, 2, 250)]
    public void EnvironmentCost_CostPerNode_IsMonthlyCostDividedByNodes(decimal monthly, int nodes, decimal expected)
    {
        var cost = new EnvironmentCost { MonthlyCost = monthly, Nodes = nodes };
        cost.CostPerNode.Should().Be(expected);
    }

    [Fact]
    public void EnvironmentCost_AllPropertiesCanBeSet()
    {
        var cost = new EnvironmentCost
        {
            Environment = EnvironmentType.Prod,
            EnvironmentName = "Production",
            MonthlyCost = 5000m,
            Percentage = 60m,
            Nodes = 10,
            TotalCpu = 80,
            TotalRamGB = 320,
            TotalDiskGB = 1000
        };

        cost.Environment.Should().Be(EnvironmentType.Prod);
        cost.EnvironmentName.Should().Be("Production");
        cost.MonthlyCost.Should().Be(5000m);
        cost.Percentage.Should().Be(60m);
        cost.Nodes.Should().Be(10);
        cost.TotalCpu.Should().Be(80);
        cost.TotalRamGB.Should().Be(320);
        cost.TotalDiskGB.Should().Be(1000);
        cost.CostPerNode.Should().Be(500m);
    }

    #endregion

    #region CostComparison Tests

    [Fact]
    public void CostComparison_DefaultEstimates_IsEmpty()
    {
        var comparison = new CostComparison();
        comparison.Estimates.Should().NotBeNull();
        comparison.Estimates.Should().BeEmpty();
    }

    [Fact]
    public void CostComparison_DefaultCheapestOption_IsNull()
    {
        var comparison = new CostComparison();
        comparison.CheapestOption.Should().BeNull();
    }

    [Fact]
    public void CostComparison_DefaultMostExpensiveOption_IsNull()
    {
        var comparison = new CostComparison();
        comparison.MostExpensiveOption.Should().BeNull();
    }

    [Fact]
    public void CostComparison_DefaultPotentialSavings_IsEmpty()
    {
        var comparison = new CostComparison();
        comparison.PotentialSavings.Should().NotBeNull();
        comparison.PotentialSavings.Should().BeEmpty();
    }

    [Fact]
    public void CostComparison_DefaultInsights_IsEmpty()
    {
        var comparison = new CostComparison();
        comparison.Insights.Should().NotBeNull();
        comparison.Insights.Should().BeEmpty();
    }

    [Fact]
    public void CostComparison_DefaultComparedAt_IsRecentUtc()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var comparison = new CostComparison();
        var after = DateTime.UtcNow.AddSeconds(1);

        comparison.ComparedAt.Should().BeAfter(before);
        comparison.ComparedAt.Should().BeBefore(after);
    }

    [Fact]
    public void CostComparison_CanHoldMultipleEstimates()
    {
        var comparison = new CostComparison
        {
            Estimates = new List<CostEstimate>
            {
                new() { Provider = CloudProvider.AWS, MonthlyTotal = 1000 },
                new() { Provider = CloudProvider.Azure, MonthlyTotal = 900 },
                new() { Provider = CloudProvider.GCP, MonthlyTotal = 950 }
            }
        };

        comparison.Estimates.Should().HaveCount(3);
    }

    [Fact]
    public void CostComparison_AllPropertiesCanBeSet()
    {
        var cheapest = new CostEstimate { Provider = CloudProvider.Azure, MonthlyTotal = 900 };
        var expensive = new CostEstimate { Provider = CloudProvider.AWS, MonthlyTotal = 1000 };

        var comparison = new CostComparison
        {
            Estimates = new List<CostEstimate> { cheapest, expensive },
            CheapestOption = cheapest,
            MostExpensiveOption = expensive,
            PotentialSavings = new Dictionary<string, decimal> { { "AWS", 100m } },
            Insights = new List<string> { "Azure is 10% cheaper than AWS" }
        };

        comparison.Estimates.Should().HaveCount(2);
        comparison.CheapestOption.Should().Be(cheapest);
        comparison.MostExpensiveOption.Should().Be(expensive);
        comparison.PotentialSavings["AWS"].Should().Be(100m);
        comparison.Insights.Should().Contain("Azure is 10% cheaper than AWS");
    }

    #endregion
}
