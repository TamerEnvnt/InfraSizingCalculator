using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.Results;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Models.Pricing;
using InfraSizingCalculator.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components.Results;

/// <summary>
/// Tests for CostAnalysisView component - Cost analysis with progressive disclosure
/// </summary>
public class CostAnalysisViewTests : TestContext
{
    private readonly IPricingService _pricingService;

    public CostAnalysisViewTests()
    {
        _pricingService = Substitute.For<IPricingService>();
        _pricingService.GetRegions(Arg.Any<CloudProvider>()).Returns(new List<RegionInfo>
        {
            new() { Code = "us-east-1", DisplayName = "US East", IsPreferred = true }
        });

        Services.AddSingleton(_pricingService);
    }

    private static CostEstimate CreateCostEstimate()
    {
        return new CostEstimate
        {
            Provider = CloudProvider.AWS,
            Region = "us-east-1",
            PricingType = PricingType.OnDemand,
            MonthlyTotal = 15000m, // YearlyTotal, ThreeYearTCO, FiveYearTCO are calculated from this
            PricingSource = "Default pricing",
            CalculatedAt = DateTime.Now,
            Breakdown = new Dictionary<CostCategory, CostBreakdown>
            {
                [CostCategory.Compute] = new()
                {
                    Category = CostCategory.Compute,
                    Description = "Compute",
                    Monthly = 12000m,
                    Percentage = 80
                },
                [CostCategory.Storage] = new()
                {
                    Category = CostCategory.Storage,
                    Description = "Storage",
                    Monthly = 3000m,
                    Percentage = 20
                }
            },
            EnvironmentCosts = new Dictionary<EnvironmentType, EnvironmentCost>
            {
                [EnvironmentType.Prod] = new()
                {
                    EnvironmentName = "Prod",
                    MonthlyCost = 10000m,
                    Nodes = 12,
                    TotalCpu = 96,
                    TotalRamGB = 384,
                    Percentage = 66.7m
                },
                [EnvironmentType.Dev] = new()
                {
                    EnvironmentName = "Dev",
                    MonthlyCost = 5000m,
                    Nodes = 6,
                    TotalCpu = 48,
                    TotalRamGB = 192,
                    Percentage = 33.3m
                }
            },
            Notes = new List<string>
            {
                "Based on on-demand pricing",
                "Excludes egress data transfer costs"
            }
        };
    }

    #region Rendering Tests

    [Fact]
    public void CostAnalysisView_RendersContainer()
    {
        // Act
        var cut = RenderComponent<CostAnalysisView>();

        // Assert
        cut.Find(".cost-analysis-view").Should().NotBeNull();
    }

    [Fact]
    public void CostAnalysisView_NoEstimate_ShowsNoResults()
    {
        // Act
        var cut = RenderComponent<CostAnalysisView>(parameters => parameters
            .Add(p => p.ShowPricingSelector, false));

        // Assert
        cut.Find(".no-results").Should().NotBeNull();
        // Component now uses text "$" instead of emoji
        cut.Find(".no-results-icon").TextContent.Should().Be("$");
    }

    #endregion

    #region Cost Summary Tests

    [Fact]
    public void CostAnalysisView_WithEstimate_ShowsSummaryRow()
    {
        // Arrange
        var estimate = CreateCostEstimate();

        // Act
        var cut = RenderComponent<CostAnalysisView>(parameters => parameters
            .Add(p => p.CostEstimate, estimate));

        // Assert
        cut.Find(".cost-summary-row").Should().NotBeNull();
    }

    [Fact]
    public void CostAnalysisView_ShowsAllCostCards()
    {
        // Arrange
        var estimate = CreateCostEstimate();

        // Act
        var cut = RenderComponent<CostAnalysisView>(parameters => parameters
            .Add(p => p.CostEstimate, estimate));

        // Assert
        var costCards = cut.FindAll(".cost-card");
        costCards.Should().HaveCount(4); // Monthly, Yearly, 3-Year, 5-Year
    }

    [Fact]
    public void CostAnalysisView_MonthlyCardIsPrimary()
    {
        // Arrange
        var estimate = CreateCostEstimate();

        // Act
        var cut = RenderComponent<CostAnalysisView>(parameters => parameters
            .Add(p => p.CostEstimate, estimate));

        // Assert
        cut.Find(".cost-card.primary").Should().NotBeNull();
        cut.Find(".cost-card.primary .cost-label").TextContent.Should().Contain("Monthly");
    }

    [Fact]
    public void CostAnalysisView_ShowsFormattedCosts()
    {
        // Arrange
        var estimate = CreateCostEstimate();

        // Act
        var cut = RenderComponent<CostAnalysisView>(parameters => parameters
            .Add(p => p.CostEstimate, estimate));

        // Assert
        cut.Find(".cost-card.primary .cost-value").TextContent.Should().Contain("$15.0K");
    }

    #endregion

    #region Pricing Info Tests

    [Fact]
    public void CostAnalysisView_ShowsPricingInfo()
    {
        // Arrange
        var estimate = CreateCostEstimate();

        // Act
        var cut = RenderComponent<CostAnalysisView>(parameters => parameters
            .Add(p => p.CostEstimate, estimate));

        // Assert
        cut.Find(".pricing-info").Should().NotBeNull();
        cut.Find(".pricing-source").TextContent.Should().Contain("AWS");
        cut.Find(".pricing-source").TextContent.Should().Contain("us-east-1");
    }

    #endregion

    #region Cost Breakdown Section Tests

    [Fact]
    public void CostAnalysisView_ShowsBreakdownSection()
    {
        // Arrange
        var estimate = CreateCostEstimate();

        // Act
        var cut = RenderComponent<CostAnalysisView>(parameters => parameters
            .Add(p => p.CostEstimate, estimate));

        // Assert
        var sections = cut.FindAll(".cost-section");
        sections.Should().Contain(s => s.TextContent.Contains("Cost Breakdown"));
    }

    [Fact]
    public void CostAnalysisView_ShowsBreakdownCards()
    {
        // Arrange
        var estimate = CreateCostEstimate();

        // Act
        var cut = RenderComponent<CostAnalysisView>(parameters => parameters
            .Add(p => p.CostEstimate, estimate));

        // Assert
        cut.FindAll(".breakdown-card").Should().HaveCount(2); // Compute and Storage
    }

    [Fact]
    public void CostAnalysisView_BreakdownShowsPercentage()
    {
        // Arrange
        var estimate = CreateCostEstimate();

        // Act
        var cut = RenderComponent<CostAnalysisView>(parameters => parameters
            .Add(p => p.CostEstimate, estimate));

        // Assert
        var percentages = cut.FindAll(".breakdown-percent");
        percentages.Should().Contain(p => p.TextContent.Contains("80.0%"));
    }

    #endregion

    #region Environment Costs Section Tests

    [Fact]
    public void CostAnalysisView_ShowsEnvironmentSection()
    {
        // Arrange
        var estimate = CreateCostEstimate();

        // Act
        var cut = RenderComponent<CostAnalysisView>(parameters => parameters
            .Add(p => p.CostEstimate, estimate));

        // Assert
        var sections = cut.FindAll(".cost-section");
        sections.Should().Contain(s => s.TextContent.Contains("Cost by Environment"));
    }

    [Fact]
    public void CostAnalysisView_ShowsEnvironmentCards()
    {
        // Arrange
        var estimate = CreateCostEstimate();

        // Act
        var cut = RenderComponent<CostAnalysisView>(parameters => parameters
            .Add(p => p.CostEstimate, estimate));

        // Assert
        cut.FindAll(".env-cost-card").Should().HaveCount(2); // Prod and Dev
    }

    [Fact]
    public void CostAnalysisView_EnvironmentShowsDetails()
    {
        // Arrange
        var estimate = CreateCostEstimate();

        // Act
        var cut = RenderComponent<CostAnalysisView>(parameters => parameters
            .Add(p => p.CostEstimate, estimate));

        // Assert
        var envDetails = cut.Find(".env-cost-details");
        envDetails.TextContent.Should().Contain("nodes");
        envDetails.TextContent.Should().Contain("vCPU");
        envDetails.TextContent.Should().Contain("GB RAM");
    }

    #endregion

    #region Notes Section Tests

    [Fact]
    public void CostAnalysisView_ShowsNotesSection()
    {
        // Arrange
        var estimate = CreateCostEstimate();

        // Act
        var cut = RenderComponent<CostAnalysisView>(parameters => parameters
            .Add(p => p.CostEstimate, estimate));

        // Assert
        var sections = cut.FindAll(".cost-section");
        sections.Should().Contain(s => s.TextContent.Contains("Notes & Assumptions"));
    }

    [Fact]
    public void CostAnalysisView_ShowsNotesList()
    {
        // Arrange
        var estimate = CreateCostEstimate();

        // Act
        var cut = RenderComponent<CostAnalysisView>(parameters => parameters
            .Add(p => p.CostEstimate, estimate));

        // Assert
        cut.FindAll(".notes-list li").Should().HaveCount(2);
    }

    #endregion

    #region Category Icon Tests

    [Fact]
    public void CostAnalysisView_ShowsComputeIcon()
    {
        // Arrange
        var estimate = CreateCostEstimate();

        // Act
        var cut = RenderComponent<CostAnalysisView>(parameters => parameters
            .Add(p => p.CostEstimate, estimate));

        // Assert - Component now uses text "CPU" instead of emoji
        var icons = cut.FindAll(".category-icon");
        icons.Should().Contain(i => i.TextContent == "CPU");
    }

    [Fact]
    public void CostAnalysisView_ShowsStorageIcon()
    {
        // Arrange
        var estimate = CreateCostEstimate();

        // Act
        var cut = RenderComponent<CostAnalysisView>(parameters => parameters
            .Add(p => p.CostEstimate, estimate));

        // Assert - Component now uses text "Disk" instead of emoji
        var icons = cut.FindAll(".category-icon");
        icons.Should().Contain(i => i.TextContent == "Disk");
    }

    #endregion

    #region Loading State Tests

    [Fact]
    public void CostAnalysisView_Loading_ShowsOverlay()
    {
        // Arrange
        var estimate = CreateCostEstimate();

        // Act
        var cut = RenderComponent<CostAnalysisView>(parameters => parameters
            .Add(p => p.CostEstimate, estimate)
            .Add(p => p.IsLoading, true));

        // Assert
        cut.Find(".loading-overlay").Should().NotBeNull();
        cut.Find(".loading-spinner").Should().NotBeNull();
    }

    #endregion
}
