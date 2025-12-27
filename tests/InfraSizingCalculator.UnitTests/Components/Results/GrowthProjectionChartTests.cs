using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.Results;
using InfraSizingCalculator.Models.Growth;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components.Results;

/// <summary>
/// Tests for GrowthProjectionChart component - Dashboard visualization with
/// hero metrics, year cards, and tabbed views (Resources, Cost, Table)
/// </summary>
public class GrowthProjectionChartTests : TestContext
{
    private static GrowthProjection CreateProjection(bool includeCosts = true)
    {
        return new GrowthProjection
        {
            Settings = new GrowthSettings
            {
                ProjectionYears = 3,
                IncludeCostProjections = includeCosts
            },
            Baseline = new ProjectionPoint
            {
                Year = 0,
                Label = "Current",
                ProjectedApps = 50,
                ProjectedNodes = 12,
                ProjectedWorkerNodes = 9,
                ProjectedCpu = 96,
                ProjectedRamGB = 384,
                ProjectedStorageGB = 1200,
                ProjectedMonthlyCost = 15000m,
                ProjectedYearlyCost = 180000m
            },
            Points = new List<ProjectionPoint>
            {
                new()
                {
                    Year = 1,
                    Label = "Year 1",
                    ProjectedApps = 63,
                    ProjectedNodes = 15,
                    ProjectedWorkerNodes = 12,
                    ProjectedCpu = 120,
                    ProjectedRamGB = 480,
                    ProjectedStorageGB = 1500,
                    ProjectedMonthlyCost = 18750m,
                    ProjectedYearlyCost = 225000m,
                    GrowthFromPrevious = 25,
                    CumulativeGrowth = 25
                },
                new()
                {
                    Year = 2,
                    Label = "Year 2",
                    ProjectedApps = 79,
                    ProjectedNodes = 19,
                    ProjectedWorkerNodes = 15,
                    ProjectedCpu = 152,
                    ProjectedRamGB = 608,
                    ProjectedStorageGB = 1900,
                    ProjectedMonthlyCost = 23438m,
                    ProjectedYearlyCost = 281256m,
                    GrowthFromPrevious = 25,
                    CumulativeGrowth = 56
                },
                new()
                {
                    Year = 3,
                    Label = "Year 3",
                    ProjectedApps = 99,
                    ProjectedNodes = 24,
                    ProjectedWorkerNodes = 19,
                    ProjectedCpu = 192,
                    ProjectedRamGB = 768,
                    ProjectedStorageGB = 2400,
                    ProjectedMonthlyCost = 29297m,
                    ProjectedYearlyCost = 351564m,
                    GrowthFromPrevious = 25,
                    CumulativeGrowth = 95
                }
            },
            Summary = new ProjectionSummary
            {
                TotalAppGrowth = 49,
                PercentageAppGrowth = 98,
                TotalNodeGrowth = 12,
                PercentageNodeGrowth = 100,
                TotalCostOverPeriod = 1037820m,
                AverageYearlyCost = 259455m,
                CostIncrease = 171564m,
                PercentageCostIncrease = 95
            }
        };
    }

    #region Rendering Tests

    [Fact]
    public void GrowthProjectionChart_RendersContainer()
    {
        // Act
        var cut = RenderComponent<GrowthProjectionChart>(parameters => parameters
            .Add(p => p.Projection, CreateProjection()));

        // Assert
        cut.Find(".growth-projection-dashboard").Should().NotBeNull();
    }

    [Fact]
    public void GrowthProjectionChart_NoProjection_ShowsEmptyDashboard()
    {
        // Act
        var cut = RenderComponent<GrowthProjectionChart>();

        // Assert
        cut.Find(".empty-dashboard").Should().NotBeNull();
        cut.Find(".empty-icon").TextContent.Should().Be("📊");
    }

    [Fact]
    public void GrowthProjectionChart_AppliesAdditionalClass()
    {
        // Act
        var cut = RenderComponent<GrowthProjectionChart>(parameters => parameters
            .Add(p => p.Projection, CreateProjection())
            .Add(p => p.AdditionalClass, "custom-class"));

        // Assert
        cut.Find(".growth-projection-dashboard").ClassList.Should().Contain("custom-class");
    }

    #endregion

    #region Hero Metrics Tests

    [Fact]
    public void GrowthProjectionChart_ShowsHeroMetrics()
    {
        // Act
        var cut = RenderComponent<GrowthProjectionChart>(parameters => parameters
            .Add(p => p.Projection, CreateProjection()));

        // Assert
        cut.Find(".hero-metrics").Should().NotBeNull();
        cut.FindAll(".hero-metric").Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public void GrowthProjectionChart_ShowsAppGrowthMetric()
    {
        // Act
        var cut = RenderComponent<GrowthProjectionChart>(parameters => parameters
            .Add(p => p.Projection, CreateProjection()));

        // Assert
        cut.Find(".hero-metric.apps").Should().NotBeNull();
        cut.Find(".hero-metric.apps").TextContent.Should().Contain("Application Growth");
    }

    [Fact]
    public void GrowthProjectionChart_ShowsNodeGrowthMetric()
    {
        // Act
        var cut = RenderComponent<GrowthProjectionChart>(parameters => parameters
            .Add(p => p.Projection, CreateProjection()));

        // Assert
        cut.Find(".hero-metric.nodes").Should().NotBeNull();
        cut.Find(".hero-metric.nodes").TextContent.Should().Contain("Infrastructure Growth");
    }

    [Fact]
    public void GrowthProjectionChart_WithCosts_ShowsCostMetrics()
    {
        // Act
        var cut = RenderComponent<GrowthProjectionChart>(parameters => parameters
            .Add(p => p.Projection, CreateProjection(includeCosts: true)));

        // Assert
        cut.Find(".hero-metric.cost").Should().NotBeNull();
        cut.Find(".hero-metric.average").Should().NotBeNull();
    }

    [Fact]
    public void GrowthProjectionChart_WithoutCosts_HidesCostMetrics()
    {
        // Act
        var cut = RenderComponent<GrowthProjectionChart>(parameters => parameters
            .Add(p => p.Projection, CreateProjection(includeCosts: false)));

        // Assert
        cut.FindAll(".hero-metric.cost").Should().BeEmpty();
        cut.FindAll(".hero-metric.average").Should().BeEmpty();
    }

    #endregion

    #region Dashboard Tabs Tests

    [Fact]
    public void GrowthProjectionChart_ShowsDashboardTabs()
    {
        // Act
        var cut = RenderComponent<GrowthProjectionChart>(parameters => parameters
            .Add(p => p.Projection, CreateProjection()));

        // Assert
        cut.Find(".dashboard-tabs").Should().NotBeNull();
    }

    [Fact]
    public void GrowthProjectionChart_HasResourcesTabButton()
    {
        // Act
        var cut = RenderComponent<GrowthProjectionChart>(parameters => parameters
            .Add(p => p.Projection, CreateProjection()));

        // Assert
        var buttons = cut.FindAll(".tab-btn");
        buttons.Should().Contain(b => b.TextContent.Contains("Resources"));
    }

    [Fact]
    public void GrowthProjectionChart_WithCosts_HasCostAnalysisTabButton()
    {
        // Act
        var cut = RenderComponent<GrowthProjectionChart>(parameters => parameters
            .Add(p => p.Projection, CreateProjection(includeCosts: true)));

        // Assert
        var buttons = cut.FindAll(".tab-btn");
        buttons.Should().Contain(b => b.TextContent.Contains("Cost"));
    }

    [Fact]
    public void GrowthProjectionChart_HasDataTableTabButton()
    {
        // Act
        var cut = RenderComponent<GrowthProjectionChart>(parameters => parameters
            .Add(p => p.Projection, CreateProjection()));

        // Assert
        var buttons = cut.FindAll(".tab-btn");
        buttons.Should().Contain(b => b.TextContent.Contains("Table"));
    }

    [Fact]
    public void GrowthProjectionChart_ResourcesTabIsDefaultActive()
    {
        // Act
        var cut = RenderComponent<GrowthProjectionChart>(parameters => parameters
            .Add(p => p.Projection, CreateProjection()));

        // Assert
        var activeButton = cut.Find(".tab-btn.active");
        activeButton.TextContent.Should().Contain("Resources");
    }

    [Fact]
    public async Task GrowthProjectionChart_ClickingTableTab_SwitchesToTable()
    {
        // Arrange
        var cut = RenderComponent<GrowthProjectionChart>(parameters => parameters
            .Add(p => p.Projection, CreateProjection()));

        // Act
        var tableButton = cut.FindAll(".tab-btn").First(b => b.TextContent.Contains("Table"));
        await cut.InvokeAsync(() => tableButton.Click());

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Find(".full-table-section").Should().NotBeNull();
        });
    }

    #endregion

    #region Year Progress Cards Tests (Resources View)

    [Fact]
    public void GrowthProjectionChart_ResourcesView_ShowsYearProgressCards()
    {
        // Act
        var cut = RenderComponent<GrowthProjectionChart>(parameters => parameters
            .Add(p => p.Projection, CreateProjection()));

        // Assert
        cut.Find(".year-progress-cards").Should().NotBeNull();
    }

    [Fact]
    public void GrowthProjectionChart_YearCards_ShowsAllPoints()
    {
        // Act
        var cut = RenderComponent<GrowthProjectionChart>(parameters => parameters
            .Add(p => p.Projection, CreateProjection()));

        // Assert
        var yearCards = cut.FindAll(".year-card");
        yearCards.Should().HaveCount(4); // Baseline + 3 years
    }

    [Fact]
    public void GrowthProjectionChart_YearCards_ShowsNodeCounts()
    {
        // Act
        var cut = RenderComponent<GrowthProjectionChart>(parameters => parameters
            .Add(p => p.Projection, CreateProjection()));

        // Assert
        cut.FindAll(".node-number").Should().NotBeEmpty();
    }

    [Fact]
    public void GrowthProjectionChart_YearCards_ShowsLabels()
    {
        // Act
        var cut = RenderComponent<GrowthProjectionChart>(parameters => parameters
            .Add(p => p.Projection, CreateProjection()));

        // Assert
        var labels = cut.FindAll(".year-label");
        labels.Should().Contain(l => l.TextContent == "Current");
        labels.Should().Contain(l => l.TextContent == "Year 1");
    }

    [Fact]
    public void GrowthProjectionChart_YearCards_ShowsGrowthPercentage()
    {
        // Act
        var cut = RenderComponent<GrowthProjectionChart>(parameters => parameters
            .Add(p => p.Projection, CreateProjection()));

        // Assert
        var growthLabels = cut.FindAll(".year-growth");
        growthLabels.Should().NotBeEmpty();
        growthLabels.Should().Contain(g => g.TextContent.Contains("+25%"));
    }

    [Fact]
    public void GrowthProjectionChart_YearCards_BaselineIsHighlighted()
    {
        // Act
        var cut = RenderComponent<GrowthProjectionChart>(parameters => parameters
            .Add(p => p.Projection, CreateProjection()));

        // Assert
        cut.FindAll(".year-card.baseline").Should().NotBeEmpty();
    }

    #endregion

    #region Resource Breakdown Tests

    [Fact]
    public void GrowthProjectionChart_ShowsBreakdownSection()
    {
        // Act
        var cut = RenderComponent<GrowthProjectionChart>(parameters => parameters
            .Add(p => p.Projection, CreateProjection()));

        // Assert
        cut.Find(".breakdown-section").Should().NotBeNull();
        cut.Find(".breakdown-section .section-header h3").TextContent.Should().Contain("Resource Details");
    }

    [Fact]
    public void GrowthProjectionChart_BreakdownShowsResourceTable()
    {
        // Act
        var cut = RenderComponent<GrowthProjectionChart>(parameters => parameters
            .Add(p => p.Projection, CreateProjection()));

        // Assert
        cut.Find(".resource-table").Should().NotBeNull();
    }

    [Fact]
    public void GrowthProjectionChart_ResourceTable_HasCorrectHeaders()
    {
        // Act
        var cut = RenderComponent<GrowthProjectionChart>(parameters => parameters
            .Add(p => p.Projection, CreateProjection()));

        // Assert
        var headers = cut.FindAll(".resource-table th");
        headers.Should().Contain(h => h.TextContent.Contains("Period"));
        headers.Should().Contain(h => h.TextContent.Contains("Apps"));
        headers.Should().Contain(h => h.TextContent.Contains("Nodes"));
        headers.Should().Contain(h => h.TextContent.Contains("vCPU"));
        headers.Should().Contain(h => h.TextContent.Contains("RAM"));
        headers.Should().Contain(h => h.TextContent.Contains("Storage"));
    }

    [Fact]
    public void GrowthProjectionChart_ResourceTable_BaselineRowIsHighlighted()
    {
        // Act
        var cut = RenderComponent<GrowthProjectionChart>(parameters => parameters
            .Add(p => p.Projection, CreateProjection()));

        // Assert
        cut.FindAll(".baseline-row").Should().NotBeEmpty();
    }

    #endregion

    #region Table View Tests

    [Fact]
    public async Task GrowthProjectionChart_TableView_ShowsFullTable()
    {
        // Arrange
        var cut = RenderComponent<GrowthProjectionChart>(parameters => parameters
            .Add(p => p.Projection, CreateProjection()));

        // Act
        var tableButton = cut.FindAll(".tab-btn").First(b => b.TextContent.Contains("Table"));
        await cut.InvokeAsync(() => tableButton.Click());

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Find(".full-table").Should().NotBeNull();
        });
    }

    [Fact]
    public async Task GrowthProjectionChart_TableView_HasCorrectHeaders()
    {
        // Arrange
        var cut = RenderComponent<GrowthProjectionChart>(parameters => parameters
            .Add(p => p.Projection, CreateProjection()));

        // Act
        var tableButton = cut.FindAll(".tab-btn").First(b => b.TextContent.Contains("Table"));
        await cut.InvokeAsync(() => tableButton.Click());

        // Assert
        cut.WaitForAssertion(() =>
        {
            var headers = cut.FindAll(".full-table th");
            headers.Should().Contain(h => h.TextContent.Contains("Period"));
            headers.Should().Contain(h => h.TextContent.Contains("Apps"));
            headers.Should().Contain(h => h.TextContent.Contains("Nodes"));
            headers.Should().Contain(h => h.TextContent.Contains("Growth"));
        });
    }

    [Fact]
    public async Task GrowthProjectionChart_TableView_WithCosts_ShowsCostColumns()
    {
        // Arrange
        var cut = RenderComponent<GrowthProjectionChart>(parameters => parameters
            .Add(p => p.Projection, CreateProjection(includeCosts: true)));

        // Act
        var tableButton = cut.FindAll(".tab-btn").First(b => b.TextContent.Contains("Table"));
        await cut.InvokeAsync(() => tableButton.Click());

        // Assert
        cut.WaitForAssertion(() =>
        {
            var headers = cut.FindAll(".full-table th");
            headers.Should().Contain(h => h.TextContent.Contains("Monthly"));
            headers.Should().Contain(h => h.TextContent.Contains("Yearly"));
        });
    }

    [Fact]
    public async Task GrowthProjectionChart_TableView_BaselineRow_IsHighlighted()
    {
        // Arrange
        var cut = RenderComponent<GrowthProjectionChart>(parameters => parameters
            .Add(p => p.Projection, CreateProjection()));

        // Act
        var tableButton = cut.FindAll(".tab-btn").First(b => b.TextContent.Contains("Table"));
        await cut.InvokeAsync(() => tableButton.Click());

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.FindAll(".baseline-row").Should().NotBeEmpty();
        });
    }

    #endregion

    #region Cost View Tests

    [Fact]
    public async Task GrowthProjectionChart_CostView_ShowsCostBars()
    {
        // Arrange
        var cut = RenderComponent<GrowthProjectionChart>(parameters => parameters
            .Add(p => p.Projection, CreateProjection()));

        // Act
        var costButton = cut.FindAll(".tab-btn").First(b => b.TextContent.Contains("Cost"));
        await cut.InvokeAsync(() => costButton.Click());

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Find(".cost-bars").Should().NotBeNull();
            cut.FindAll(".cost-bar").Should().NotBeEmpty();
        });
    }

    [Fact]
    public async Task GrowthProjectionChart_CostView_ShowsCostSummaryCards()
    {
        // Arrange
        var cut = RenderComponent<GrowthProjectionChart>(parameters => parameters
            .Add(p => p.Projection, CreateProjection()));

        // Act
        var costButton = cut.FindAll(".tab-btn").First(b => b.TextContent.Contains("Cost"));
        await cut.InvokeAsync(() => costButton.Click());

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Find(".cost-summary-grid").Should().NotBeNull();
            cut.Find(".cost-summary-card.total").Should().NotBeNull();
            cut.Find(".cost-summary-card.total").TextContent.Should().Contain("Total Investment");
        });
    }

    #endregion

    #region Currency Formatting Tests

    [Fact]
    public void GrowthProjectionChart_FormatsMillions()
    {
        // Arrange
        var projection = CreateProjection();
        projection.Summary.TotalCostOverPeriod = 2500000m;

        // Act
        var cut = RenderComponent<GrowthProjectionChart>(parameters => parameters
            .Add(p => p.Projection, projection));

        // Assert
        cut.Markup.Should().Contain("M");
    }

    [Fact]
    public void GrowthProjectionChart_FormatsThousands()
    {
        // Arrange
        var projection = CreateProjection();

        // Act
        var cut = RenderComponent<GrowthProjectionChart>(parameters => parameters
            .Add(p => p.Projection, projection));

        // Assert
        cut.Markup.Should().Contain("K");
    }

    #endregion
}
