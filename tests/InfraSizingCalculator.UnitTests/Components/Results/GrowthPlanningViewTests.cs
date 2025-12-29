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

    #region Tab Switching Tests

    [Fact]
    public void GrowthPlanningView_DefaultsToResourcesTab()
    {
        // Arrange
        var projection = CreateProjection();

        // Act
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, CreateSettings())
            .Add(p => p.Projection, projection));

        // Assert
        var resourcesTab = cut.FindAll(".tab-sm").First();
        resourcesTab.ClassList.Should().Contain("active");
        cut.Find(".resources-grid").Should().NotBeNull();
    }

    [Fact]
    public void GrowthPlanningView_SwitchingToCostTab_ShowsCostContent()
    {
        // Arrange
        var projection = CreateProjection();

        // Act
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, CreateSettings())
            .Add(p => p.Projection, projection));

        // Click Cost tab
        var costTab = cut.FindAll(".tab-sm").First(t => t.TextContent.Contains("Cost"));
        costTab.Click();

        // Assert
        cut.Find(".cost-grid").Should().NotBeNull();
        cut.Find(".cost-chart-compact").Should().NotBeNull();
    }

    [Fact]
    public void GrowthPlanningView_SwitchingToTimelineTab_ShowsTimelineContent()
    {
        // Arrange
        var projection = CreateProjection();

        // Act
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, CreateSettings())
            .Add(p => p.Projection, projection));

        // Click Timeline tab
        var timelineTab = cut.FindAll(".tab-sm").Last();
        timelineTab.Click();

        // Assert
        cut.Find(".timeline-content-compact").Should().NotBeNull();
        cut.Find(".timeline-visual-compact").Should().NotBeNull();
    }

    [Fact]
    public void GrowthPlanningView_CostTabHidden_WhenIncludeCostProjectionsIsFalse()
    {
        // Arrange
        var settings = CreateSettings();
        settings.IncludeCostProjections = false;
        var projection = CreateProjection();
        projection.Settings.IncludeCostProjections = false;

        // Act
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, settings)
            .Add(p => p.Projection, projection));

        // Assert
        var tabs = cut.FindAll(".tab-sm");
        tabs.Should().HaveCount(2); // Only Resources and Timeline
        tabs.Should().NotContain(t => t.TextContent.Contains("Cost"));
    }

    #endregion

    #region Cost Tab Content Tests

    [Fact]
    public void GrowthPlanningView_CostTab_ShowsBarChart()
    {
        // Arrange
        var projection = CreateProjection();

        // Act
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, CreateSettings())
            .Add(p => p.Projection, projection));

        // Click Cost tab
        cut.FindAll(".tab-sm").First(t => t.TextContent.Contains("Cost")).Click();

        // Assert
        cut.FindAll(".bar-group-sm").Should().HaveCount(4); // Baseline + 3 years
        cut.Find(".bar-group-sm.baseline").Should().NotBeNull();
    }

    [Fact]
    public void GrowthPlanningView_CostTab_ShowsCostSummaryCards()
    {
        // Arrange
        var projection = CreateProjection();

        // Act
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, CreateSettings())
            .Add(p => p.Projection, projection));

        // Click Cost tab
        cut.FindAll(".tab-sm").First(t => t.TextContent.Contains("Cost")).Click();

        // Assert
        cut.FindAll(".cost-card-sm").Should().HaveCount(4);
        cut.Find(".cost-card-sm.total").Should().NotBeNull();
        cut.Find(".cost-card-sm.increase").Should().NotBeNull();
        cut.Find(".cost-card-sm.average").Should().NotBeNull();
        cut.Find(".cost-card-sm.monthly").Should().NotBeNull();
    }

    [Fact]
    public void GrowthPlanningView_CostTab_ShowsGrowthPercentage()
    {
        // Arrange
        var projection = CreateProjection();
        projection.Summary.PercentageCostIncrease = 95;

        // Act
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, CreateSettings())
            .Add(p => p.Projection, projection));

        // Click Cost tab
        cut.FindAll(".tab-sm").First(t => t.TextContent.Contains("Cost")).Click();

        // Assert
        cut.Find(".cost-card-sm.increase").TextContent.Should().Contain("+95%");
    }

    [Fact]
    public void GrowthPlanningView_CostTab_ShowsBarValues()
    {
        // Arrange
        var projection = CreateProjection();
        projection.Baseline.ProjectedYearlyCost = 180000m;

        // Act
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, CreateSettings())
            .Add(p => p.Projection, projection));

        // Click Cost tab
        cut.FindAll(".tab-sm").First(t => t.TextContent.Contains("Cost")).Click();

        // Assert - FormatCurrencyShort formats $180K
        var barValues = cut.FindAll(".bar-val");
        barValues.Should().NotBeEmpty();
    }

    [Fact]
    public void GrowthPlanningView_CostTab_ShowsGrowthBadgesOnBars()
    {
        // Arrange
        var projection = CreateProjection();

        // Act
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, CreateSettings())
            .Add(p => p.Projection, projection));

        // Click Cost tab
        cut.FindAll(".tab-sm").First(t => t.TextContent.Contains("Cost")).Click();

        // Assert - non-baseline bars with growth show percentage badges
        cut.FindAll(".bar-growth").Should().NotBeEmpty();
    }

    #endregion

    #region IsCalculating State Tests

    [Fact]
    public async Task GrowthPlanningView_ShowsSpinner_WhenCalculating()
    {
        // Arrange
        var tcs = new TaskCompletionSource<bool>();
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, CreateSettings())
            .Add(p => p.OnCalculate, EventCallback.Factory.Create<GrowthSettings>(this, async _ =>
            {
                await tcs.Task;
            })));

        // Act - Start calculation
        await cut.InvokeAsync(() => cut.Find(".btn-calc-sm").Click());

        // Assert - Spinner should be visible
        cut.Find(".spinner-sm").Should().NotBeNull();

        // Cleanup
        tcs.SetResult(true);
    }

    [Fact]
    public async Task GrowthPlanningView_DisablesButton_WhenCalculating()
    {
        // Arrange
        var tcs = new TaskCompletionSource<bool>();
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, CreateSettings())
            .Add(p => p.OnCalculate, EventCallback.Factory.Create<GrowthSettings>(this, async _ =>
            {
                await tcs.Task;
            })));

        // Act - Start calculation
        await cut.InvokeAsync(() => cut.Find(".btn-calc-sm").Click());

        // Assert - Button should be disabled
        cut.Find(".btn-calc-sm").GetAttribute("disabled").Should().NotBeNull();

        // Cleanup
        tcs.SetResult(true);
    }

    [Fact]
    public async Task GrowthPlanningView_ResetsCalculatingState_AfterCalculation()
    {
        // Arrange
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, CreateSettings())
            .Add(p => p.OnCalculate, EventCallback.Factory.Create<GrowthSettings>(this, _ => Task.CompletedTask)));

        // Act
        await cut.InvokeAsync(() => cut.Find(".btn-calc-sm").Click());

        // Assert - Button should be re-enabled
        cut.Find(".btn-calc-sm").GetAttribute("disabled").Should().BeNull();
        cut.FindAll(".spinner-sm").Should().BeEmpty();
    }

    #endregion

    #region Timeline Tab Content Tests

    [Fact]
    public void GrowthPlanningView_TimelineTab_ShowsVisualTimeline()
    {
        // Arrange
        var projection = CreateProjection();

        // Act
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, CreateSettings())
            .Add(p => p.Projection, projection));

        // Click Timeline tab
        cut.Find(".tab-sm:last-child").Click();

        // Assert
        cut.Find(".tl-track").Should().NotBeNull();
        cut.Find(".tl-nodes").Should().NotBeNull();
        cut.Find(".tl-node.baseline").Should().NotBeNull();
    }

    [Fact]
    public void GrowthPlanningView_TimelineTab_ShowsYearNodes()
    {
        // Arrange
        var projection = CreateProjection();

        // Act
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, CreateSettings())
            .Add(p => p.Projection, projection));

        // Click Timeline tab
        cut.Find(".tab-sm:last-child").Click();

        // Assert - Baseline "Now" + 3 year nodes
        cut.FindAll(".tl-node").Should().HaveCount(4);
        cut.Find(".tl-label").TextContent.Should().Be("Now");
    }

    [Fact]
    public void GrowthPlanningView_TimelineTab_ShowsAllClear_WhenNoWarningsOrRecommendations()
    {
        // Arrange
        var projection = CreateProjection();
        projection.Warnings.Clear();
        projection.Recommendations.Clear();

        // Act
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, CreateSettings())
            .Add(p => p.Projection, projection));

        // Click Timeline tab
        cut.Find(".tab-sm:last-child").Click();

        // Assert
        cut.Find(".all-clear-compact").Should().NotBeNull();
        cut.Find(".all-clear-compact").TextContent.Should().Contain("All Clear");
    }

    [Fact]
    public void GrowthPlanningView_TimelineTab_ShowsRecommendations()
    {
        // Arrange
        var projection = CreateProjection();
        projection.Recommendations.Add(new ScalingRecommendation
        {
            Title = "Add worker nodes",
            Icon = "🔧",
            Priority = 1,
            RecommendedYear = 2
        });

        // Act
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, CreateSettings())
            .Add(p => p.Projection, projection));

        // Click Timeline tab
        cut.Find(".tab-sm:last-child").Click();

        // Assert
        cut.Find(".alerts-section.recommendations").Should().NotBeNull();
        cut.Find(".alerts-section.recommendations .alert-title").TextContent.Should().Contain("Add worker nodes");
    }

    [Fact]
    public void GrowthPlanningView_TimelineTab_ShowsMoreLink_WhenManyWarnings()
    {
        // Arrange
        var projection = CreateProjection();
        projection.Summary.WarningCount = 5;
        for (int i = 0; i < 5; i++)
        {
            projection.Warnings.Add(new ClusterLimitWarning
            {
                YearTriggered = 1 + (i % 3),
                Message = $"Warning {i + 1}",
                ResourceType = $"Resource{i}",
                PercentageOfLimit = 80 + i
            });
        }

        // Act
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, CreateSettings())
            .Add(p => p.Projection, projection));

        // Click Timeline tab
        cut.Find(".tab-sm:last-child").Click();

        // Assert - Only 3 warnings shown, with "+2 more" link
        cut.Find(".more-link").Should().NotBeNull();
        cut.Find(".more-link").TextContent.Should().Contain("+2 more");
    }

    [Fact]
    public void GrowthPlanningView_TimelineTab_ShowsMoreLink_WhenManyRecommendations()
    {
        // Arrange
        var projection = CreateProjection();
        for (int i = 0; i < 5; i++)
        {
            projection.Recommendations.Add(new ScalingRecommendation
            {
                Title = $"Recommendation {i + 1}",
                Icon = "💡",
                Priority = i + 1,
                RecommendedYear = i % 3 + 1
            });
        }

        // Act
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, CreateSettings())
            .Add(p => p.Projection, projection));

        // Click Timeline tab
        cut.Find(".tab-sm:last-child").Click();

        // Assert
        var recSection = cut.Find(".alerts-section.recommendations");
        recSection.QuerySelector(".more-link")?.TextContent.Should().Contain("+2 more");
    }

    [Fact]
    public void GrowthPlanningView_TimelineNode_ShowsCriticalStyling()
    {
        // Arrange
        var projection = CreateProjection();
        projection.Warnings.Add(new ClusterLimitWarning
        {
            YearTriggered = 2,
            Severity = WarningSeverity.Critical,
            Message = "Critical resource exhaustion",
            ResourceType = "CPU",
            PercentageOfLimit = 100
        });

        // Act
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, CreateSettings())
            .Add(p => p.Projection, projection));

        // Click Timeline tab
        cut.Find(".tab-sm:last-child").Click();

        // Assert - Year 2 node should have critical class
        cut.Find(".tl-node.critical").Should().NotBeNull();
    }

    [Fact]
    public void GrowthPlanningView_TimelineNode_ShowsWarningStyling()
    {
        // Arrange
        var projection = CreateProjection();
        projection.Warnings.Add(new ClusterLimitWarning
        {
            YearTriggered = 1,
            Severity = WarningSeverity.Warning,
            Message = "Approaching limit",
            ResourceType = "Memory",
            PercentageOfLimit = 85
        });

        // Act
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, CreateSettings())
            .Add(p => p.Projection, projection));

        // Click Timeline tab
        cut.Find(".tab-sm:last-child").Click();

        // Assert - Year 1 node should have warning class
        cut.Find(".tl-node.warning").Should().NotBeNull();
    }

    #endregion

    #region GetWarningIcon Tests

    [Theory]
    [InlineData(WarningType.NodeLimit, "🖥️")]
    [InlineData(WarningType.PodLimit, "📦")]
    [InlineData(WarningType.CpuCapacity, "⚡")]
    [InlineData(WarningType.MemoryCapacity, "💾")]
    [InlineData(WarningType.StorageCapacity, "💿")]
    [InlineData(WarningType.CostThreshold, "💰")]
    public void GrowthPlanningView_ShowsCorrectWarningIcon(WarningType type, string expectedIcon)
    {
        // Arrange
        var projection = CreateProjection();
        projection.Warnings.Add(new ClusterLimitWarning
        {
            YearTriggered = 2,
            Message = "Test warning",
            ResourceType = type.ToString(),
            PercentageOfLimit = 80,
            Type = type
        });

        // Act
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, CreateSettings())
            .Add(p => p.Projection, projection));

        // Click Timeline tab
        cut.Find(".tab-sm:last-child").Click();

        // Assert
        cut.Find(".alert-icon").TextContent.Should().Contain(expectedIcon);
    }

    #endregion

    #region Hero Strip Tests

    [Fact]
    public void GrowthPlanningView_HeroStrip_ShowsAppGrowthPercentage()
    {
        // Arrange
        var projection = CreateProjection();
        projection.Summary.PercentageAppGrowth = 98;

        // Act
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, CreateSettings())
            .Add(p => p.Projection, projection));

        // Assert
        cut.Find(".hero-badge.positive").TextContent.Should().Contain("+98%");
    }

    [Fact]
    public void GrowthPlanningView_HeroStrip_ShowsNodeGrowthCount()
    {
        // Arrange
        var projection = CreateProjection();
        projection.Summary.TotalNodeGrowth = 12;

        // Act
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, CreateSettings())
            .Add(p => p.Projection, projection));

        // Assert
        cut.FindAll(".hero-badge.positive").Should().Contain(b => b.TextContent.Contains("+12"));
    }

    [Fact]
    public void GrowthPlanningView_HeroStrip_HidesCostItems_WhenIncludeCostProjectionsIsFalse()
    {
        // Arrange
        var settings = CreateSettings();
        settings.IncludeCostProjections = false;
        var projection = CreateProjection();

        // Act
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, settings)
            .Add(p => p.Projection, projection));

        // Assert - Only 2 hero items (Apps and Nodes), not 4
        cut.FindAll(".hero-item").Should().HaveCount(2);
    }

    [Fact]
    public void GrowthPlanningView_HeroStrip_ShowsAverageMonthly()
    {
        // Arrange
        var projection = CreateProjection();
        projection.Summary.AverageYearlyCost = 240_000m; // $20K/month

        // Act
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, CreateSettings())
            .Add(p => p.Projection, projection));

        // Assert
        cut.Find(".hero-strip").TextContent.Should().Contain("Avg/Month");
    }

    #endregion

    #region Resources Tab Detail Tests

    [Fact]
    public void GrowthPlanningView_ResourcesTab_ShowsYearCards()
    {
        // Arrange
        var projection = CreateProjection();

        // Act
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, CreateSettings())
            .Add(p => p.Projection, projection));

        // Assert
        cut.FindAll(".year-card-sm").Should().HaveCount(4); // Baseline + 3 years
        cut.Find(".year-card-sm.baseline").Should().NotBeNull();
    }

    [Fact]
    public void GrowthPlanningView_ResourcesTab_ShowsDataTable()
    {
        // Arrange
        var projection = CreateProjection();

        // Act
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, CreateSettings())
            .Add(p => p.Projection, projection));

        // Assert
        cut.Find(".data-table-sm").Should().NotBeNull();
        cut.Find(".data-table-sm thead").Should().NotBeNull();
        cut.FindAll(".data-table-sm tbody tr").Should().HaveCount(4);
    }

    [Fact]
    public void GrowthPlanningView_ResourcesTab_ShowsProgressBars()
    {
        // Arrange
        var projection = CreateProjection();

        // Act
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, CreateSettings())
            .Add(p => p.Projection, projection));

        // Assert
        cut.FindAll(".yc-progress").Should().HaveCount(4);
        cut.FindAll(".yc-fill").Should().HaveCount(4);
    }

    [Fact]
    public void GrowthPlanningView_ResourcesTab_ShowsGrowthBadges()
    {
        // Arrange
        var projection = CreateProjection();

        // Act
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, CreateSettings())
            .Add(p => p.Projection, projection));

        // Assert - Non-baseline cards have growth badges
        cut.FindAll(".yc-badge").Should().HaveCount(3);
    }

    [Fact]
    public void GrowthPlanningView_ResourcesTab_HidesMonthlyCostColumn_WhenNoCostProjections()
    {
        // Arrange
        var settings = CreateSettings();
        settings.IncludeCostProjections = false;
        var projection = CreateProjection();

        // Act
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, settings)
            .Add(p => p.Projection, projection));

        // Assert
        var headers = cut.FindAll(".data-table-sm th");
        headers.Should().NotContain(h => h.TextContent.Contains("Monthly"));
    }

    #endregion

    #region Settings Change Tests

    [Fact]
    public async Task GrowthPlanningView_ChangingGrowthRate_InvokesSettingsChanged()
    {
        // Arrange
        GrowthSettings? changedSettings = null;
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, CreateSettings())
            .Add(p => p.SettingsChanged, EventCallback.Factory.Create<GrowthSettings>(this, s => changedSettings = s)));

        // Act - Change growth rate
        var rateInput = cut.Find(".rate-input-sm input[type='number']");
        await cut.InvokeAsync(() => rateInput.Change("50"));

        // Assert
        changedSettings.Should().NotBeNull();
    }

    [Fact]
    public async Task GrowthPlanningView_ChangingPattern_InvokesSettingsChanged()
    {
        // Arrange
        GrowthSettings? changedSettings = null;
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, CreateSettings())
            .Add(p => p.SettingsChanged, EventCallback.Factory.Create<GrowthSettings>(this, s => changedSettings = s)));

        // Act - Change pattern
        var patternSelect = cut.FindAll(".setting-inline select").First(s => s.TextContent.Contains("Exponential"));
        await cut.InvokeAsync(() => patternSelect.Change(GrowthPattern.Exponential.ToString()));

        // Assert
        changedSettings.Should().NotBeNull();
    }

    [Fact]
    public async Task GrowthPlanningView_TogglingIncludeCostProjections_InvokesSettingsChanged()
    {
        // Arrange
        GrowthSettings? changedSettings = null;
        var cut = RenderComponent<GrowthPlanningView>(parameters => parameters
            .Add(p => p.Settings, CreateSettings())
            .Add(p => p.SettingsChanged, EventCallback.Factory.Create<GrowthSettings>(this, s => changedSettings = s)));

        // Act - Toggle cost checkbox
        var costCheckbox = cut.Find(".toggle-sm input[type='checkbox']");
        await cut.InvokeAsync(() => costCheckbox.Change(false));

        // Assert
        changedSettings.Should().NotBeNull();
    }

    #endregion
}
