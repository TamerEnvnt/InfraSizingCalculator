using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.Results;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Models.Pricing;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components;

/// <summary>
/// Tests for CostSummary component
/// </summary>
public class CostSummaryTests : TestContext
{
    private CostEstimate CreateTestCostEstimate()
    {
        return new CostEstimate
        {
            Provider = CloudProvider.AWS,
            Region = "us-east-1",
            PricingType = PricingType.OnDemand,
            MonthlyTotal = 15000m,
            PricingSource = "AWS Calculator",
            CalculatedAt = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc),
            Breakdown = new Dictionary<CostCategory, CostBreakdown>
            {
                {
                    CostCategory.Compute,
                    new CostBreakdown
                    {
                        Category = CostCategory.Compute,
                        Description = "Compute Resources",
                        Monthly = 10000m,
                        Percentage = 66.67m
                    }
                },
                {
                    CostCategory.Storage,
                    new CostBreakdown
                    {
                        Category = CostCategory.Storage,
                        Description = "Storage",
                        Monthly = 3000m,
                        Percentage = 20m
                    }
                },
                {
                    CostCategory.Network,
                    new CostBreakdown
                    {
                        Category = CostCategory.Network,
                        Description = "Networking",
                        Monthly = 2000m,
                        Percentage = 13.33m
                    }
                }
            },
            EnvironmentCosts = new Dictionary<EnvironmentType, EnvironmentCost>
            {
                {
                    EnvironmentType.Prod,
                    new EnvironmentCost
                    {
                        Environment = EnvironmentType.Prod,
                        EnvironmentName = "Production",
                        MonthlyCost = 8000m,
                        Percentage = 53.33m,
                        Nodes = 10,
                        TotalCpu = 80,
                        TotalRamGB = 320
                    }
                },
                {
                    EnvironmentType.Dev,
                    new EnvironmentCost
                    {
                        Environment = EnvironmentType.Dev,
                        EnvironmentName = "Development",
                        MonthlyCost = 4000m,
                        Percentage = 26.67m,
                        Nodes = 5,
                        TotalCpu = 40,
                        TotalRamGB = 160
                    }
                }
            },
            Notes = new List<string>
            {
                "Prices based on standard instances",
                "Network costs estimated"
            }
        };
    }

    #region Rendering Tests

    [Fact]
    public void CostSummary_DoesNotRender_WhenCostEstimateIsNull()
    {
        // Act
        var cut = RenderComponent<CostSummary>(parameters => parameters
            .Add(p => p.CostEstimate, (CostEstimate?)null));

        // Assert
        cut.FindAll(".cost-summary").Should().BeEmpty();
    }

    [Fact]
    public void CostSummary_RendersMainContainer()
    {
        // Act
        var cut = RenderComponent<CostSummary>(parameters => parameters
            .Add(p => p.CostEstimate, CreateTestCostEstimate()));

        // Assert
        cut.Find(".cost-summary").Should().NotBeNull();
    }

    [Fact]
    public void CostSummary_AppliesAdditionalClass()
    {
        // Act
        var cut = RenderComponent<CostSummary>(parameters => parameters
            .Add(p => p.CostEstimate, CreateTestCostEstimate())
            .Add(p => p.AdditionalClass, "custom-cost"));

        // Assert
        cut.Find(".cost-summary").ClassList.Should().Contain("custom-cost");
    }

    [Fact]
    public void CostSummary_DisplaysTitle()
    {
        // Act
        var cut = RenderComponent<CostSummary>(parameters => parameters
            .Add(p => p.CostEstimate, CreateTestCostEstimate()));

        // Assert
        cut.Find("h3").TextContent.Should().Be("Cost Estimate");
    }

    #endregion

    #region Cost Overview Tests

    [Fact]
    public void CostSummary_DisplaysCostOverviewCards()
    {
        // Act
        var cut = RenderComponent<CostSummary>(parameters => parameters
            .Add(p => p.CostEstimate, CreateTestCostEstimate()));

        // Assert
        var costCards = cut.FindAll(".cost-card");
        costCards.Should().HaveCount(4); // Monthly, Yearly, 3-Year, 5-Year
    }

    [Fact]
    public void CostSummary_DisplaysMonthlyTotal()
    {
        // Act
        var cut = RenderComponent<CostSummary>(parameters => parameters
            .Add(p => p.CostEstimate, CreateTestCostEstimate()));

        // Assert
        var monthlyCostCard = cut.Find(".cost-card-primary");
        monthlyCostCard.QuerySelector(".cost-label")?.TextContent.Should().Be("Monthly");
        monthlyCostCard.QuerySelector(".cost-value")?.TextContent.Should().Be("$15.0K");
    }

    [Fact]
    public void CostSummary_DisplaysYearlyTotal()
    {
        // Act
        var cut = RenderComponent<CostSummary>(parameters => parameters
            .Add(p => p.CostEstimate, CreateTestCostEstimate()));

        // Assert
        var cards = cut.FindAll(".cost-card");
        var yearlyCard = cards[1];
        yearlyCard.QuerySelector(".cost-label")?.TextContent.Should().Be("Yearly");
        yearlyCard.QuerySelector(".cost-value")?.TextContent.Should().Be("$180.0K");
    }

    [Fact]
    public void CostSummary_DisplaysThreeYearTCO()
    {
        // Act
        var cut = RenderComponent<CostSummary>(parameters => parameters
            .Add(p => p.CostEstimate, CreateTestCostEstimate()));

        // Assert
        var cards = cut.FindAll(".cost-card");
        var threeYearCard = cards[2];
        threeYearCard.QuerySelector(".cost-label")?.TextContent.Should().Be("3-Year TCO");
        threeYearCard.QuerySelector(".cost-value")?.TextContent.Should().Be("$540.0K");
    }

    [Fact]
    public void CostSummary_DisplaysFiveYearTCO()
    {
        // Act
        var cut = RenderComponent<CostSummary>(parameters => parameters
            .Add(p => p.CostEstimate, CreateTestCostEstimate()));

        // Assert
        var cards = cut.FindAll(".cost-card");
        var fiveYearCard = cards[3];
        fiveYearCard.QuerySelector(".cost-label")?.TextContent.Should().Be("5-Year TCO");
        fiveYearCard.QuerySelector(".cost-value")?.TextContent.Should().Be("$900.0K");
    }

    #endregion

    #region Cost Breakdown Tests

    [Fact]
    public void CostSummary_DisplaysCostBreakdownSection()
    {
        // Act
        var cut = RenderComponent<CostSummary>(parameters => parameters
            .Add(p => p.CostEstimate, CreateTestCostEstimate()));

        // Assert
        var breakdownSection = cut.Find(".cost-details");
        breakdownSection.Should().NotBeNull();
        breakdownSection.QuerySelector("h4")?.TextContent.Should().Be("Cost Breakdown");
    }

    [Fact]
    public void CostSummary_DisplaysAllBreakdownCategories()
    {
        // Act
        var cut = RenderComponent<CostSummary>(parameters => parameters
            .Add(p => p.CostEstimate, CreateTestCostEstimate()));

        // Assert
        var breakdownRows = cut.FindAll(".breakdown-row");
        breakdownRows.Should().HaveCount(3); // Compute, Storage, Network
    }

    [Fact]
    public void CostSummary_BreakdownRowsOrderedByMonthlyCost()
    {
        // Act
        var cut = RenderComponent<CostSummary>(parameters => parameters
            .Add(p => p.CostEstimate, CreateTestCostEstimate()));

        // Assert - Should be ordered descending
        var categoryNames = cut.FindAll(".category-name");
        categoryNames[0].TextContent.Should().Be("Compute Resources"); // $10K
        categoryNames[1].TextContent.Should().Be("Storage");           // $3K
        categoryNames[2].TextContent.Should().Be("Networking");        // $2K
    }

    [Fact]
    public void CostSummary_DisplaysBreakdownMonthlyValues()
    {
        // Act
        var cut = RenderComponent<CostSummary>(parameters => parameters
            .Add(p => p.CostEstimate, CreateTestCostEstimate()));

        // Assert
        var monthlyValues = cut.FindAll(".breakdown-monthly");
        monthlyValues[0].TextContent.Should().Be("$10.0K/mo");
        monthlyValues[1].TextContent.Should().Be("$3.0K/mo");
        monthlyValues[2].TextContent.Should().Be("$2.0K/mo");
    }

    [Fact]
    public void CostSummary_DisplaysBreakdownPercentages()
    {
        // Act
        var cut = RenderComponent<CostSummary>(parameters => parameters
            .Add(p => p.CostEstimate, CreateTestCostEstimate()));

        // Assert
        var percentages = cut.FindAll(".breakdown-percent");
        percentages[0].TextContent.Should().Be("66.7%");
        percentages[1].TextContent.Should().Be("20.0%");
        percentages[2].TextContent.Should().Be("13.3%");
    }

    [Fact]
    public void CostSummary_DisplaysBreakdownBars()
    {
        // Act
        var cut = RenderComponent<CostSummary>(parameters => parameters
            .Add(p => p.CostEstimate, CreateTestCostEstimate()));

        // Assert
        var barFills = cut.FindAll(".breakdown-bar-fill");
        barFills.Should().HaveCount(3);

        // Check bar widths match percentages
        barFills[0].GetAttribute("style").Should().Contain("width: 66.67%");
        barFills[1].GetAttribute("style").Should().Contain("width: 20%");
        barFills[2].GetAttribute("style").Should().Contain("width: 13.33%");
    }

    [Fact]
    public void CostSummary_DisplaysCategoryIcons()
    {
        // Act
        var cut = RenderComponent<CostSummary>(parameters => parameters
            .Add(p => p.CostEstimate, CreateTestCostEstimate()));

        // Assert
        var icons = cut.FindAll(".category-icon");
        icons[0].TextContent.Should().Be("CPU"); // Compute
        icons[1].TextContent.Should().Be("HDD"); // Storage
        icons[2].TextContent.Should().Be("NET"); // Network
    }

    #endregion

    #region Environment Costs Tests

    [Fact]
    public void CostSummary_DisplaysEnvironmentCostsSection()
    {
        // Act
        var cut = RenderComponent<CostSummary>(parameters => parameters
            .Add(p => p.CostEstimate, CreateTestCostEstimate()));

        // Assert
        var envSection = cut.Find(".cost-by-environment");
        envSection.Should().NotBeNull();
        envSection.QuerySelector("h4")?.TextContent.Should().Be("Cost by Environment");
    }

    [Fact]
    public void CostSummary_DisplaysEnvironmentCostsTable()
    {
        // Act
        var cut = RenderComponent<CostSummary>(parameters => parameters
            .Add(p => p.CostEstimate, CreateTestCostEstimate()));

        // Assert
        var table = cut.Find(".env-cost-table");
        table.Should().NotBeNull();

        var headers = table.QuerySelectorAll("th");
        headers.Should().HaveCount(4);
        headers[0].TextContent.Should().Be("Environment");
        headers[1].TextContent.Should().Be("Monthly");
        headers[2].TextContent.Should().Be("% of Total");
        headers[3].TextContent.Should().Be("Resources");
    }

    [Fact]
    public void CostSummary_EnvironmentRowsOrderedByMonthlyCost()
    {
        // Act
        var cut = RenderComponent<CostSummary>(parameters => parameters
            .Add(p => p.CostEstimate, CreateTestCostEstimate()));

        // Assert
        var rows = cut.FindAll(".env-cost-table tbody tr");
        rows.Should().HaveCount(2);

        // First row should be Prod (higher cost)
        var cells = rows[0].QuerySelectorAll("td");
        cells[0].TextContent.Should().Be("Production");
        cells[1].TextContent.Should().Be("$8.0K");
    }

    [Fact]
    public void CostSummary_DisplaysEnvironmentResources()
    {
        // Act
        var cut = RenderComponent<CostSummary>(parameters => parameters
            .Add(p => p.CostEstimate, CreateTestCostEstimate()));

        // Assert
        var rows = cut.FindAll(".env-cost-table tbody tr");
        var prodResources = rows[0].QuerySelectorAll("td")[3];

        prodResources.TextContent.Should().Contain("10 nodes");
        prodResources.TextContent.Should().Contain("80 CPU");
        prodResources.TextContent.Should().Contain("320 GB RAM");
    }

    [Fact]
    public void CostSummary_DoesNotDisplayEnvironmentSection_WhenNoEnvironmentCosts()
    {
        // Arrange
        var estimate = CreateTestCostEstimate();
        estimate.EnvironmentCosts = new Dictionary<EnvironmentType, EnvironmentCost>();

        // Act
        var cut = RenderComponent<CostSummary>(parameters => parameters
            .Add(p => p.CostEstimate, estimate));

        // Assert
        cut.FindAll(".cost-by-environment").Should().BeEmpty();
    }

    #endregion

    #region Pricing Info Tests

    [Fact]
    public void CostSummary_DisplaysPricingInfo()
    {
        // Act
        var cut = RenderComponent<CostSummary>(parameters => parameters
            .Add(p => p.CostEstimate, CreateTestCostEstimate()));

        // Assert
        var pricingInfo = cut.Find(".pricing-info");
        pricingInfo.Should().NotBeNull();

        pricingInfo.QuerySelector(".pricing-source")?.TextContent.Should().Contain("AWS Calculator");
        // Check for date presence without specific format (culture-dependent)
        pricingInfo.QuerySelector(".pricing-date")?.TextContent.Should().Contain("Calculated:");
    }

    [Fact]
    public void CostSummary_DoesNotDisplayPricingInfo_WhenSourceEmpty()
    {
        // Arrange
        var estimate = CreateTestCostEstimate();
        estimate.PricingSource = "";

        // Act
        var cut = RenderComponent<CostSummary>(parameters => parameters
            .Add(p => p.CostEstimate, estimate));

        // Assert
        cut.FindAll(".pricing-info").Should().BeEmpty();
    }

    #endregion

    #region Notes Tests

    [Fact]
    public void CostSummary_DisplaysNotes()
    {
        // Act
        var cut = RenderComponent<CostSummary>(parameters => parameters
            .Add(p => p.CostEstimate, CreateTestCostEstimate()));

        // Assert
        var notesSection = cut.Find(".cost-notes");
        notesSection.Should().NotBeNull();
        notesSection.QuerySelector("h4")?.TextContent.Should().Be("Notes");

        var noteItems = notesSection.QuerySelectorAll("li");
        noteItems.Should().HaveCount(2);
        noteItems[0].TextContent.Should().Be("Prices based on standard instances");
        noteItems[1].TextContent.Should().Be("Network costs estimated");
    }

    [Fact]
    public void CostSummary_DoesNotDisplayNotes_WhenEmpty()
    {
        // Arrange
        var estimate = CreateTestCostEstimate();
        estimate.Notes = new List<string>();

        // Act
        var cut = RenderComponent<CostSummary>(parameters => parameters
            .Add(p => p.CostEstimate, estimate));

        // Assert
        cut.FindAll(".cost-notes").Should().BeEmpty();
    }

    #endregion

    #region Currency Formatting Tests

    [Fact]
    public void CostSummary_FormatsSmallAmounts()
    {
        // Arrange
        var estimate = CreateTestCostEstimate();
        estimate.MonthlyTotal = 250.50m;

        // Act
        var cut = RenderComponent<CostSummary>(parameters => parameters
            .Add(p => p.CostEstimate, estimate));

        // Assert
        var monthlyValue = cut.Find(".cost-card-primary .cost-value");
        monthlyValue.TextContent.Should().Be("$250.50");
    }

    [Fact]
    public void CostSummary_FormatsThousands()
    {
        // Arrange
        var estimate = CreateTestCostEstimate();
        estimate.MonthlyTotal = 5500m;

        // Act
        var cut = RenderComponent<CostSummary>(parameters => parameters
            .Add(p => p.CostEstimate, estimate));

        // Assert
        var monthlyValue = cut.Find(".cost-card-primary .cost-value");
        monthlyValue.TextContent.Should().Be("$5.5K");
    }

    [Fact]
    public void CostSummary_FormatsMillions()
    {
        // Arrange
        var estimate = CreateTestCostEstimate();
        estimate.MonthlyTotal = 2_500_000m;

        // Act
        var cut = RenderComponent<CostSummary>(parameters => parameters
            .Add(p => p.CostEstimate, estimate));

        // Assert
        var monthlyValue = cut.Find(".cost-card-primary .cost-value");
        monthlyValue.TextContent.Should().Be("$2.50M");
    }

    [Fact]
    public void CostSummary_FormatsZero()
    {
        // Arrange
        var estimate = CreateTestCostEstimate();
        estimate.MonthlyTotal = 0m;

        // Act
        var cut = RenderComponent<CostSummary>(parameters => parameters
            .Add(p => p.CostEstimate, estimate));

        // Assert
        var monthlyValue = cut.Find(".cost-card-primary .cost-value");
        monthlyValue.TextContent.Should().Be("$0.00");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void CostSummary_HandlesEmptyBreakdown()
    {
        // Arrange
        var estimate = CreateTestCostEstimate();
        estimate.Breakdown = new Dictionary<CostCategory, CostBreakdown>();

        // Act
        var cut = RenderComponent<CostSummary>(parameters => parameters
            .Add(p => p.CostEstimate, estimate));

        // Assert - Should still render
        cut.Find(".cost-summary").Should().NotBeNull();
        cut.FindAll(".breakdown-row").Should().BeEmpty();
    }

    [Fact]
    public void CostSummary_HandlesAllCategoryIcons()
    {
        // Arrange
        var estimate = CreateTestCostEstimate();
        estimate.Breakdown = new Dictionary<CostCategory, CostBreakdown>
        {
            { CostCategory.Compute, new CostBreakdown { Category = CostCategory.Compute, Description = "Compute", Monthly = 100m, Percentage = 10m } },
            { CostCategory.Storage, new CostBreakdown { Category = CostCategory.Storage, Description = "Storage", Monthly = 100m, Percentage = 10m } },
            { CostCategory.Network, new CostBreakdown { Category = CostCategory.Network, Description = "Network", Monthly = 100m, Percentage = 10m } },
            { CostCategory.License, new CostBreakdown { Category = CostCategory.License, Description = "License", Monthly = 100m, Percentage = 10m } },
            { CostCategory.Support, new CostBreakdown { Category = CostCategory.Support, Description = "Support", Monthly = 100m, Percentage = 10m } },
            { CostCategory.DataCenter, new CostBreakdown { Category = CostCategory.DataCenter, Description = "Data Center", Monthly = 100m, Percentage = 10m } },
            { CostCategory.Labor, new CostBreakdown { Category = CostCategory.Labor, Description = "Labor", Monthly = 100m, Percentage = 10m } }
        };

        // Act
        var cut = RenderComponent<CostSummary>(parameters => parameters
            .Add(p => p.CostEstimate, estimate));

        // Assert - Should render all categories
        var icons = cut.FindAll(".category-icon");
        icons.Should().HaveCount(7);
        icons[0].TextContent.Should().Be("CPU"); // Compute
        icons[1].TextContent.Should().Be("HDD"); // Storage
        icons[2].TextContent.Should().Be("NET"); // Network
        icons[3].TextContent.Should().Be("LIC"); // License
        icons[4].TextContent.Should().Be("SUP"); // Support
        icons[5].TextContent.Should().Be("DC");  // DataCenter
        icons[6].TextContent.Should().Be("OPS"); // Labor
    }

    [Fact]
    public void CostSummary_HandlesVeryLargeNumbers()
    {
        // Arrange
        var estimate = CreateTestCostEstimate();
        estimate.MonthlyTotal = 999_999_999m;

        // Act
        var cut = RenderComponent<CostSummary>(parameters => parameters
            .Add(p => p.CostEstimate, estimate));

        // Assert - Should format correctly
        var monthlyValue = cut.Find(".cost-card-primary .cost-value");
        monthlyValue.TextContent.Should().Be("$1000.00M");
    }

    [Fact]
    public void CostSummary_AllCostCardsHaveIcons()
    {
        // Act
        var cut = RenderComponent<CostSummary>(parameters => parameters
            .Add(p => p.CostEstimate, CreateTestCostEstimate()));

        // Assert
        var costCards = cut.FindAll(".cost-card");
        foreach (var card in costCards)
        {
            card.QuerySelector(".cost-icon").Should().NotBeNull();
            card.QuerySelector(".cost-icon")!.TextContent.Should().Be("$");
        }
    }

    #endregion
}
