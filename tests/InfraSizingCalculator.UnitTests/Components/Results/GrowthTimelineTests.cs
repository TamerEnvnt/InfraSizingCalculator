using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.Results;
using InfraSizingCalculator.Models.Growth;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components.Results;

/// <summary>
/// Tests for GrowthTimeline component - Warnings, recommendations, and milestones
/// </summary>
public class GrowthTimelineTests : TestContext
{
    private static GrowthProjection CreateProjection(bool includeWarnings = true, bool includeRecommendations = true)
    {
        var projection = new GrowthProjection
        {
            Settings = new GrowthSettings
            {
                ProjectionYears = 3,
                ShowClusterLimitWarnings = true
            },
            Summary = new ProjectionSummary
            {
                WarningCount = includeWarnings ? 2 : 0,
                CriticalWarningCount = includeWarnings ? 1 : 0,
                MajorScalingYear = includeWarnings ? 2 : null
            }
        };

        if (includeWarnings)
        {
            projection.Warnings = new List<ClusterLimitWarning>
            {
                new()
                {
                    Type = WarningType.NodeLimit,
                    Severity = WarningSeverity.Warning,
                    YearTriggered = 2,
                    Message = "Approaching maximum node count for cluster",
                    CurrentValue = 12,
                    ProjectedValue = 19,
                    Limit = 25,
                    PercentageOfLimit = 76,
                    ResourceType = "Nodes"
                },
                new()
                {
                    Type = WarningType.CpuCapacity,
                    Severity = WarningSeverity.Critical,
                    YearTriggered = 3,
                    Message = "CPU capacity will exceed recommended threshold",
                    CurrentValue = 96,
                    ProjectedValue = 192,
                    Limit = 200,
                    PercentageOfLimit = 96,
                    ResourceType = "CPU"
                }
            };
        }

        if (includeRecommendations)
        {
            projection.Recommendations = new List<ScalingRecommendation>
            {
                new()
                {
                    Type = RecommendationType.AddWorkerNodes,
                    RecommendedYear = 2,
                    Priority = 1,
                    Title = "Add Worker Nodes",
                    Description = "Scale out by adding 3 additional worker nodes",
                    EstimatedCostImpact = 1500m,
                    Icon = "🖥️"
                },
                new()
                {
                    Type = RecommendationType.UpgradeNodeSize,
                    RecommendedYear = 3,
                    Priority = 2,
                    Title = "Upgrade Node Size",
                    Description = "Consider upgrading to larger instance types",
                    EstimatedCostImpact = 2500m,
                    Icon = "⬆️"
                }
            };
        }

        return projection;
    }

    #region Rendering Tests

    [Fact]
    public void GrowthTimeline_RendersContainer()
    {
        // Act
        var cut = RenderComponent<GrowthTimeline>(parameters => parameters
            .Add(p => p.Projection, CreateProjection()));

        // Assert
        cut.Find(".growth-timeline-dashboard").Should().NotBeNull();
    }

    [Fact]
    public void GrowthTimeline_NoProjection_ShowsNoProjectionMessage()
    {
        // Act
        var cut = RenderComponent<GrowthTimeline>();

        // Assert
        cut.Find(".empty-timeline").Should().NotBeNull();
        cut.Find(".empty-icon").TextContent.Should().Be("📅");
    }

    [Fact]
    public void GrowthTimeline_AppliesAdditionalClass()
    {
        // Act
        var cut = RenderComponent<GrowthTimeline>(parameters => parameters
            .Add(p => p.Projection, CreateProjection())
            .Add(p => p.AdditionalClass, "custom-class"));

        // Assert
        cut.Find(".growth-timeline-dashboard").ClassList.Should().Contain("custom-class");
    }

    #endregion

    #region Warnings Section Tests

    [Fact]
    public void GrowthTimeline_WithWarnings_ShowsWarningsSection()
    {
        // Act
        var cut = RenderComponent<GrowthTimeline>(parameters => parameters
            .Add(p => p.Projection, CreateProjection()));

        // Assert
        cut.Find(".warnings-section").Should().NotBeNull();
        var header = cut.Find(".warnings-section .section-header h3");
        header.TextContent.Should().Contain("Warnings");
    }

    [Fact]
    public void GrowthTimeline_ShowsWarningCount()
    {
        // Act
        var cut = RenderComponent<GrowthTimeline>(parameters => parameters
            .Add(p => p.Projection, CreateProjection()));

        // Assert
        cut.FindAll(".count-badge").Should().NotBeEmpty();
    }

    [Fact]
    public void GrowthTimeline_ShowsWarningItems()
    {
        // Act
        var cut = RenderComponent<GrowthTimeline>(parameters => parameters
            .Add(p => p.Projection, CreateProjection()));

        // Assert
        cut.FindAll(".warning-card").Should().HaveCount(2);
    }

    [Fact]
    public void GrowthTimeline_WarningItem_ShowsYearBadge()
    {
        // Act
        var cut = RenderComponent<GrowthTimeline>(parameters => parameters
            .Add(p => p.Projection, CreateProjection()));

        // Assert
        var yearBadges = cut.FindAll(".year-badge");
        yearBadges.Should().Contain(b => b.TextContent.Contains("Year 2"));
    }

    [Fact]
    public void GrowthTimeline_WarningItem_ShowsSeverity()
    {
        // Act
        var cut = RenderComponent<GrowthTimeline>(parameters => parameters
            .Add(p => p.Projection, CreateProjection()));

        // Assert
        cut.FindAll(".severity-badge").Should().NotBeEmpty();
        cut.FindAll(".warning-card.warning").Should().NotBeEmpty();
        cut.FindAll(".warning-card.critical").Should().NotBeEmpty();
    }

    [Fact]
    public void GrowthTimeline_WarningItem_ShowsMessage()
    {
        // Act
        var cut = RenderComponent<GrowthTimeline>(parameters => parameters
            .Add(p => p.Projection, CreateProjection()));

        // Assert
        var messages = cut.FindAll(".warning-message");
        messages.Should().Contain(m => m.TextContent.Contains("Approaching maximum node count"));
    }

    [Fact]
    public void GrowthTimeline_WarningItem_ShowsDetails()
    {
        // Act
        var cut = RenderComponent<GrowthTimeline>(parameters => parameters
            .Add(p => p.Projection, CreateProjection()));

        // Assert
        var metrics = cut.Find(".warning-metrics");
        metrics.Should().NotBeNull();
        metrics.TextContent.Should().Contain("Current");
        metrics.TextContent.Should().Contain("Projected");
        metrics.TextContent.Should().Contain("Limit");
        metrics.TextContent.Should().Contain("Utilization");
    }

    [Fact]
    public void GrowthTimeline_WarningItem_ShowsResourceType()
    {
        // Act
        var cut = RenderComponent<GrowthTimeline>(parameters => parameters
            .Add(p => p.Projection, CreateProjection()));

        // Assert
        var warningTypes = cut.FindAll(".warning-type");
        warningTypes.Should().Contain(t => t.TextContent.Contains("Nodes"));
    }

    #endregion

    #region Recommendations Section Tests

    [Fact]
    public void GrowthTimeline_WithRecommendations_ShowsRecommendationsSection()
    {
        // Act
        var cut = RenderComponent<GrowthTimeline>(parameters => parameters
            .Add(p => p.Projection, CreateProjection()));

        // Assert
        cut.Find(".recommendations-section").Should().NotBeNull();
    }

    [Fact]
    public void GrowthTimeline_ShowsRecommendationCount()
    {
        // Act
        var cut = RenderComponent<GrowthTimeline>(parameters => parameters
            .Add(p => p.Projection, CreateProjection()));

        // Assert
        var subtitle = cut.Find(".recommendations-section .section-subtitle");
        subtitle.TextContent.Should().Contain("2");
    }

    [Fact]
    public void GrowthTimeline_ShowsRecommendationItems()
    {
        // Act
        var cut = RenderComponent<GrowthTimeline>(parameters => parameters
            .Add(p => p.Projection, CreateProjection()));

        // Assert
        cut.FindAll(".recommendation-card").Should().HaveCount(2);
    }

    [Fact]
    public void GrowthTimeline_RecommendationItem_ShowsPriorityBadge()
    {
        // Act
        var cut = RenderComponent<GrowthTimeline>(parameters => parameters
            .Add(p => p.Projection, CreateProjection()));

        // Assert
        var priorityNumbers = cut.FindAll(".priority-number");
        priorityNumbers.Should().Contain(b => b.TextContent.Contains("1"));
        priorityNumbers.Should().Contain(b => b.TextContent.Contains("2"));
    }

    [Fact]
    public void GrowthTimeline_RecommendationItem_ShowsTitle()
    {
        // Act
        var cut = RenderComponent<GrowthTimeline>(parameters => parameters
            .Add(p => p.Projection, CreateProjection()));

        // Assert
        var titles = cut.FindAll(".rec-title");
        titles.Should().Contain(t => t.TextContent == "Add Worker Nodes");
    }

    [Fact]
    public void GrowthTimeline_RecommendationItem_ShowsDescription()
    {
        // Act
        var cut = RenderComponent<GrowthTimeline>(parameters => parameters
            .Add(p => p.Projection, CreateProjection()));

        // Assert
        var descriptions = cut.FindAll(".rec-description");
        descriptions.Should().Contain(d => d.TextContent.Contains("Scale out by adding"));
    }

    [Fact]
    public void GrowthTimeline_RecommendationItem_ShowsCostImpact()
    {
        // Act
        var cut = RenderComponent<GrowthTimeline>(parameters => parameters
            .Add(p => p.Projection, CreateProjection()));

        // Assert
        cut.FindAll(".rec-impact").Should().NotBeEmpty();
        cut.FindAll(".rec-impact.increase").Should().NotBeEmpty();
    }

    [Fact]
    public void GrowthTimeline_RecommendationItem_ShowsIcon()
    {
        // Act
        var cut = RenderComponent<GrowthTimeline>(parameters => parameters
            .Add(p => p.Projection, CreateProjection()));

        // Assert
        var icons = cut.FindAll(".rec-icon");
        icons.Should().Contain(i => i.TextContent == "🖥️");
    }

    [Fact]
    public void GrowthTimeline_RecommendationItem_ShowsYear()
    {
        // Act
        var cut = RenderComponent<GrowthTimeline>(parameters => parameters
            .Add(p => p.Projection, CreateProjection()));

        // Assert
        var recYears = cut.FindAll(".rec-year");
        recYears.Should().Contain(y => y.TextContent.Contains("Year 2"));
    }

    #endregion

    #region Milestones Section Tests

    [Fact]
    public void GrowthTimeline_WithEvents_ShowsMilestonesSection()
    {
        // Act
        var cut = RenderComponent<GrowthTimeline>(parameters => parameters
            .Add(p => p.Projection, CreateProjection()));

        // Assert
        cut.Find(".visual-timeline-section").Should().NotBeNull();
    }

    [Fact]
    public void GrowthTimeline_ShowsTimelineLine()
    {
        // Act
        var cut = RenderComponent<GrowthTimeline>(parameters => parameters
            .Add(p => p.Projection, CreateProjection()));

        // Assert
        cut.Find(".timeline-track").Should().NotBeNull();
    }

    [Fact]
    public void GrowthTimeline_ShowsMilestoneMarkers()
    {
        // Act
        var cut = RenderComponent<GrowthTimeline>(parameters => parameters
            .Add(p => p.Projection, CreateProjection()));

        // Assert
        // 3 years + 1 baseline
        cut.FindAll(".timeline-node").Should().HaveCount(4);
    }

    [Fact]
    public void GrowthTimeline_MajorScalingYear_IsHighlighted()
    {
        // Act
        var cut = RenderComponent<GrowthTimeline>(parameters => parameters
            .Add(p => p.Projection, CreateProjection()));

        // Assert
        cut.FindAll(".timeline-node.major").Should().NotBeEmpty();
    }

    [Fact]
    public void GrowthTimeline_YearWithCriticalWarning_IsMarkedCritical()
    {
        // Act
        var cut = RenderComponent<GrowthTimeline>(parameters => parameters
            .Add(p => p.Projection, CreateProjection()));

        // Assert
        cut.FindAll(".timeline-node.critical").Should().NotBeEmpty();
    }

    [Fact]
    public void GrowthTimeline_YearWithEvents_HasEventBadges()
    {
        // Act
        var cut = RenderComponent<GrowthTimeline>(parameters => parameters
            .Add(p => p.Projection, CreateProjection()));

        // Assert
        cut.FindAll(".event-count").Should().NotBeEmpty();
        cut.FindAll(".timeline-node.warning").Should().NotBeEmpty();
    }

    #endregion

    #region Summary Section Tests

    [Fact]
    public void GrowthTimeline_ShowsSummarySection()
    {
        // Act
        var cut = RenderComponent<GrowthTimeline>(parameters => parameters
            .Add(p => p.Projection, CreateProjection()));

        // Assert
        cut.Find(".timeline-stats").Should().NotBeNull();
    }

    [Fact]
    public void GrowthTimeline_Summary_ShowsProjectionPeriod()
    {
        // Act
        var cut = RenderComponent<GrowthTimeline>(parameters => parameters
            .Add(p => p.Projection, CreateProjection()));

        // Assert
        var statCards = cut.FindAll(".stat-card");
        statCards.Should().Contain(s => s.TextContent.Contains("Projection Period"));
        statCards.Should().Contain(s => s.TextContent.Contains("3 Year(s)"));
    }

    [Fact]
    public void GrowthTimeline_Summary_ShowsGrowthPattern()
    {
        // Act
        var cut = RenderComponent<GrowthTimeline>(parameters => parameters
            .Add(p => p.Projection, CreateProjection()));

        // Assert
        var statCards = cut.FindAll(".stat-card");
        statCards.Should().Contain(s => s.TextContent.Contains("Growth Pattern"));
    }

    [Fact]
    public void GrowthTimeline_Summary_ShowsWarningCounts()
    {
        // Act
        var cut = RenderComponent<GrowthTimeline>(parameters => parameters
            .Add(p => p.Projection, CreateProjection()));

        // Assert
        var statCards = cut.FindAll(".stat-card");
        statCards.Should().Contain(s => s.TextContent.Contains("Status"));
        statCards.Should().Contain(s => s.TextContent.Contains("Critical"));
    }

    [Fact]
    public void GrowthTimeline_Summary_ShowsMajorScalingYear()
    {
        // Act
        var cut = RenderComponent<GrowthTimeline>(parameters => parameters
            .Add(p => p.Projection, CreateProjection()));

        // Assert
        // In the new design, major scaling year is shown in the timeline nodes with .major class
        cut.FindAll(".timeline-node.major").Should().NotBeEmpty();
    }

    #endregion

    #region No Issues State Tests

    [Fact]
    public void GrowthTimeline_NoWarningsOrRecommendations_ShowsNoIssuesState()
    {
        // Act
        var cut = RenderComponent<GrowthTimeline>(parameters => parameters
            .Add(p => p.Projection, CreateProjection(includeWarnings: false, includeRecommendations: false)));

        // Assert
        cut.Find(".all-clear-section").Should().NotBeNull();
        cut.Find(".all-clear-icon").TextContent.Should().Be("✅");
    }

    [Fact]
    public void GrowthTimeline_NoIssues_ShowsSuccessMessage()
    {
        // Act
        var cut = RenderComponent<GrowthTimeline>(parameters => parameters
            .Add(p => p.Projection, CreateProjection(includeWarnings: false, includeRecommendations: false)));

        // Assert
        cut.Find(".all-clear-section p").TextContent.Should().Contain("handle the growth");
    }

    #endregion

    #region Warning Type Icon Tests

    [Theory]
    [InlineData(WarningType.NodeLimit, "🖥️")]
    [InlineData(WarningType.PodLimit, "📦")]
    [InlineData(WarningType.CpuCapacity, "⚡")]
    [InlineData(WarningType.MemoryCapacity, "💾")]
    [InlineData(WarningType.StorageCapacity, "💿")]
    [InlineData(WarningType.CostThreshold, "💰")]
    [InlineData(WarningType.ClusterSplit, "🔀")]
    public void GrowthTimeline_ShowsCorrectWarningIcon(WarningType type, string expectedIcon)
    {
        // Arrange
        var projection = new GrowthProjection
        {
            Settings = new GrowthSettings { ShowClusterLimitWarnings = true },
            Warnings = new List<ClusterLimitWarning>
            {
                new() { Type = type, ResourceType = "Test" }
            },
            Recommendations = new List<ScalingRecommendation>(),
            Summary = new ProjectionSummary { WarningCount = 1 }
        };

        // Act
        var cut = RenderComponent<GrowthTimeline>(parameters => parameters
            .Add(p => p.Projection, projection));

        // Assert
        cut.Find(".warning-type").TextContent.Should().Contain(expectedIcon);
    }

    #endregion

    #region Recommendation Type Label Tests

    [Theory]
    [InlineData(RecommendationType.UpgradeNodeSize, "Upgrade")]
    [InlineData(RecommendationType.AddWorkerNodes, "Scale Out")]
    [InlineData(RecommendationType.SplitCluster, "Split")]
    [InlineData(RecommendationType.AddCluster, "New Cluster")]
    [InlineData(RecommendationType.EnableAutoscaling, "Autoscale")]
    [InlineData(RecommendationType.OptimizeResources, "Optimize")]
    public void GrowthTimeline_ShowsCorrectRecommendationLabel(RecommendationType type, string expectedLabel)
    {
        // Arrange
        var projection = new GrowthProjection
        {
            Settings = new GrowthSettings(),
            Warnings = new List<ClusterLimitWarning>(),
            Recommendations = new List<ScalingRecommendation>
            {
                new() { Type = type, Title = "Test" }
            },
            Summary = new ProjectionSummary()
        };

        // Act
        var cut = RenderComponent<GrowthTimeline>(parameters => parameters
            .Add(p => p.Projection, projection));

        // Assert
        cut.Find(".rec-type").TextContent.Should().Contain(expectedLabel);
    }

    #endregion

    #region Cost Impact Formatting Tests

    [Fact]
    public void GrowthTimeline_PositiveCostImpact_ShowsIncrease()
    {
        // Arrange
        var projection = CreateProjection(includeWarnings: false);
        projection.Recommendations[0].EstimatedCostImpact = 5000m;

        // Act
        var cut = RenderComponent<GrowthTimeline>(parameters => parameters
            .Add(p => p.Projection, projection));

        // Assert
        cut.Find(".rec-impact.increase").Should().NotBeNull();
        cut.Find(".rec-impact").TextContent.Should().Contain("+");
    }

    [Fact]
    public void GrowthTimeline_NegativeCostImpact_ShowsSavings()
    {
        // Arrange
        var projection = CreateProjection(includeWarnings: false);
        projection.Recommendations[0].EstimatedCostImpact = -2000m;

        // Act
        var cut = RenderComponent<GrowthTimeline>(parameters => parameters
            .Add(p => p.Projection, projection));

        // Assert
        cut.Find(".rec-impact.savings").Should().NotBeNull();
    }

    #endregion
}
