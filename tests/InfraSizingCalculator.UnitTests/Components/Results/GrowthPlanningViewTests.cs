using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.Results;
using InfraSizingCalculator.Models.Growth;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components.Results;

/// <summary>
/// Tests for GrowthPlanningView component - Compact growth dashboard with inline settings
/// </summary>
public class GrowthPlanningViewTests : TestContext
{
    private static GrowthSettings CreateSettings() => new()
    {
        AnnualGrowthRate = 25,
        ProjectionYears = 3,
        Pattern = GrowthPattern.Linear,
        IncludeCostProjections = true,
        ShowClusterLimitWarnings = true
    };

    private static GrowthProjection CreateProjection()
    {
        return new GrowthProjection
        {
            Settings = CreateSettings(),
            Baseline = new ProjectionPoint
            {
                Year = 0,
                Label = "Current",
                ProjectedApps = 50,
                ProjectedNodes = 12,
                ProjectedCpu = 96,
                ProjectedRamGB = 384,
                ProjectedMonthlyCost = 15000m
            },
            Points = new List<ProjectionPoint>
            {
                new()
                {
                    Year = 1,
                    Label = "Year 1",
                    ProjectedApps = 63,
                    ProjectedNodes = 15,
                    ProjectedCpu = 120,
                    ProjectedRamGB = 480,
                    ProjectedMonthlyCost = 18750m,
                    GrowthFromPrevious = 25,
                    CumulativeGrowth = 25
                },
                new()
                {
                    Year = 2,
                    Label = "Year 2",
                    ProjectedApps = 79,
                    ProjectedNodes = 19,
                    ProjectedCpu = 152,
                    ProjectedRamGB = 608,
                    ProjectedMonthlyCost = 23438m,
                    GrowthFromPrevious = 25,
                    CumulativeGrowth = 56
                },
                new()
                {
                    Year = 3,
                    Label = "Year 3",
                    ProjectedApps = 99,
                    ProjectedNodes = 24,
                    ProjectedCpu = 192,
                    ProjectedRamGB = 768,
                    ProjectedMonthlyCost = 29297m,
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
                CostIncrease = 14297m,
                PercentageCostIncrease = 95,
                WarningCount = 0
            },
            Warnings = new List<ClusterLimitWarning>(),
            Recommendations = new List<ScalingRecommendation>()
        };
    }

    #region Rendering Tests

    [Fact]
    public void GrowthPlanningView_RendersContainer()
    {
        // Act
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, CreateSettings()));

        // Assert
        cut.Find(".growth-dashboard").Should().NotBeNull();
    }

    [Fact]
    public void GrowthPlanningView_RendersCompactSettingsRow()
    {
        // Act
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, CreateSettings()));

        // Assert
        cut.Find(".settings-bar-compact").Should().NotBeNull();
        cut.Find(".settings-inline").Should().NotBeNull();
    }

    [Fact]
    public void GrowthPlanningView_RendersCalculateButton()
    {
        // Act
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, CreateSettings()));

        // Assert
        cut.Find(".btn-calc-sm").Should().NotBeNull();
        cut.Find(".btn-calc-sm").TextContent.Should().Contain("Calculate");
    }

    #endregion

    #region Settings Controls Tests

    [Fact]
    public void GrowthPlanningView_ShowsGrowthRateSlider()
    {
        // Act
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, CreateSettings()));

        // Assert
        var settingLabels = cut.FindAll(".setting-inline .setting-label");
        settingLabels.Should().Contain(s => s.TextContent.Contains("Growth"));
        cut.Find(".rate-input-sm").Should().NotBeNull();
    }

    [Fact]
    public void GrowthPlanningView_ShowsYearsDropdown()
    {
        // Act
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, CreateSettings()));

        // Assert
        var settingLabels = cut.FindAll(".setting-inline .setting-label");
        settingLabels.Should().Contain(s => s.TextContent == "Period");
    }

    [Fact]
    public void GrowthPlanningView_ShowsPatternDropdown()
    {
        // Act
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, CreateSettings()));

        // Assert
        var settingLabels = cut.FindAll(".setting-inline .setting-label");
        settingLabels.Should().Contain(s => s.TextContent == "Pattern");
    }

    [Fact]
    public void GrowthPlanningView_ShowsIncludeCostsToggle()
    {
        // Act
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, CreateSettings()));

        // Assert
        var toggleItem = cut.Find(".toggle-sm");
        toggleItem.Should().NotBeNull();
        toggleItem.TextContent.Should().Contain("Cost");
    }

    [Fact]
    public void GrowthPlanningView_DisplaysGrowthRateValue()
    {
        // Arrange
        var settings = CreateSettings();
        settings.AnnualGrowthRate = 30;

        // Act
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, settings));

        // Assert
        var rateInput = cut.Find(".rate-input-sm input[type='number']");
        rateInput.GetAttribute("value").Should().Be("30");
    }

    #endregion

    #region No Projection State Tests

    [Fact]
    public void GrowthPlanningView_NoProjection_ShowsNoProjectionMessage()
    {
        // Act
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, CreateSettings())
            .Add(p => p.Projection, null));

        // Assert
        cut.Find(".empty-state-sm").Should().NotBeNull();
        cut.Find(".empty-state-sm").TextContent.Should().Contain("📊");
    }

    [Fact]
    public void GrowthPlanningView_NoProjection_ShowsHelpText()
    {
        // Act
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, CreateSettings())
            .Add(p => p.Projection, null));

        // Assert
        cut.Find(".empty-state-sm").TextContent.Should().Contain("Calculate");
    }

    #endregion

    #region Projection Display Tests

    [Fact]
    public void GrowthPlanningView_WithProjection_ShowsSummaryCards()
    {
        // Arrange
        var projection = CreateProjection();

        // Act
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, CreateSettings())
            .Add(p => p.Projection, projection));

        // Assert
        cut.Find(".hero-strip").Should().NotBeNull();
        cut.FindAll(".hero-item").Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public void GrowthPlanningView_WithProjection_ShowsAppGrowth()
    {
        // Arrange
        var projection = CreateProjection();

        // Act
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, CreateSettings())
            .Add(p => p.Projection, projection));

        // Assert
        var heroItems = cut.FindAll(".hero-item");
        heroItems.Should().Contain(c => c.TextContent.Contains("Apps"));
    }

    [Fact]
    public void GrowthPlanningView_WithProjection_ShowsNodeGrowth()
    {
        // Arrange
        var projection = CreateProjection();

        // Act
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, CreateSettings())
            .Add(p => p.Projection, projection));

        // Assert
        var heroItems = cut.FindAll(".hero-item");
        heroItems.Should().Contain(c => c.TextContent.Contains("Nodes"));
    }

    [Fact]
    public void GrowthPlanningView_WithCostProjections_ShowsCostCard()
    {
        // Arrange
        var projection = CreateProjection();

        // Act
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, CreateSettings())
            .Add(p => p.Projection, projection));

        // Assert
        var heroItems = cut.FindAll(".hero-item");
        heroItems.Should().Contain(c => c.TextContent.Contains("Investment"));
    }

    [Fact]
    public void GrowthPlanningView_WithProjection_ShowsProjectionCards()
    {
        // Arrange
        var projection = CreateProjection();

        // Act
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, CreateSettings())
            .Add(p => p.Projection, projection));

        // Assert
        cut.Find(".year-cards-compact").Should().NotBeNull();
        cut.FindAll(".year-card-sm").Should().HaveCount(4); // Baseline + 3 years
    }

    [Fact]
    public void GrowthPlanningView_ProjectionCard_ShowsMetrics()
    {
        // Arrange
        var projection = CreateProjection();

        // Act
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, CreateSettings())
            .Add(p => p.Projection, projection));

        // Assert
        var firstCard = cut.FindAll(".year-card-sm").First();
        firstCard.QuerySelector(".yc-main").Should().NotBeNull();
        firstCard.QuerySelector(".yc-footer").Should().NotBeNull();
    }

    [Fact]
    public void GrowthPlanningView_ProjectionCard_ShowsMonthlyCost()
    {
        // Arrange
        var projection = CreateProjection();

        // Act
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, CreateSettings())
            .Add(p => p.Projection, projection));

        // Assert
        // Costs shown in hero strip and table when IncludeCostProjections is true
        cut.Find(".hero-item .hero-label-sm").TextContent.Should().NotBeEmpty();
    }

    #endregion

    #region Warnings Tests

    [Fact]
    public void GrowthPlanningView_WithWarnings_ShowsWarningSection()
    {
        // Arrange
        var projection = CreateProjection();
        projection.Summary.WarningCount = 1;
        projection.Warnings.Add(new ClusterLimitWarning
        {
            YearTriggered = 2,
            Severity = WarningSeverity.Warning,
            Message = "Approaching node limit",
            ResourceType = "Nodes",
            PercentageOfLimit = 80
        });

        // Act - Need to switch to timeline tab to see warnings
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, CreateSettings())
            .Add(p => p.Projection, projection));

        // Click timeline tab
        cut.Find(".tab-sm:last-child").Click();

        // Assert
        cut.Find(".alerts-section.warnings").Should().NotBeNull();
        cut.Find(".alerts-header").Should().NotBeNull();
    }

    [Fact]
    public void GrowthPlanningView_WithWarnings_ShowsWarningCount()
    {
        // Arrange
        var projection = CreateProjection();
        projection.Summary.WarningCount = 2;
        projection.Warnings.Add(new ClusterLimitWarning { YearTriggered = 2, Message = "Warning 1", ResourceType = "CPU", PercentageOfLimit = 80 });
        projection.Warnings.Add(new ClusterLimitWarning { YearTriggered = 3, Message = "Warning 2", ResourceType = "Memory", PercentageOfLimit = 90 });

        // Act
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, CreateSettings())
            .Add(p => p.Projection, projection));

        // Click timeline tab
        cut.Find(".tab-sm:last-child").Click();

        // Assert
        cut.Find(".alerts-count").TextContent.Should().Contain("2");
    }

    [Fact]
    public void GrowthPlanningView_WithWarnings_ShowsWarningOnCard()
    {
        // Arrange
        var projection = CreateProjection();
        projection.Summary.WarningCount = 1;
        projection.Warnings.Add(new ClusterLimitWarning
        {
            YearTriggered = 2,
            Message = "Approaching limit",
            ResourceType = "Nodes",
            PercentageOfLimit = 85
        });

        // Act
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, CreateSettings())
            .Add(p => p.Projection, projection));

        // Click timeline tab
        cut.Find(".tab-sm:last-child").Click();

        // Assert
        cut.FindAll(".alert-item").Should().NotBeEmpty();
    }

    #endregion

    #region Callback Tests

    [Fact]
    public async Task GrowthPlanningView_ClickingCalculate_InvokesCallback()
    {
        // Arrange
        GrowthSettings? receivedSettings = null;
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, CreateSettings())
            .Add(p => p.OnCalculate, EventCallback.Factory.Create<GrowthSettings>(this, s => receivedSettings = s)));

        // Act
        await cut.InvokeAsync(() => cut.Find(".btn-calc-sm").Click());

        // Assert
        receivedSettings.Should().NotBeNull();
    }

    [Fact]
    public async Task GrowthPlanningView_ChangingSettings_InvokesSettingsChanged()
    {
        // Arrange
        GrowthSettings? changedSettings = null;
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, CreateSettings())
            .Add(p => p.SettingsChanged, EventCallback.Factory.Create<GrowthSettings>(this, s => changedSettings = s)));

        // Act - change the projection years
        var yearSelect = cut.FindAll(".setting-inline select").First();
        await cut.InvokeAsync(() => yearSelect.Change("5"));

        // Assert
        changedSettings.Should().NotBeNull();
    }

    #endregion

    #region Currency Formatting Tests

    [Fact]
    public void GrowthPlanningView_FormatsLargeCurrency()
    {
        // Arrange
        var projection = CreateProjection();
        projection.Summary.TotalCostOverPeriod = 1_500_000m;

        // Act
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, CreateSettings())
            .Add(p => p.Projection, projection));

        // Assert
        cut.Find(".hero-strip").TextContent.Should().Contain("$");
    }

    #endregion
}
