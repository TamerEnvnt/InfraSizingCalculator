using FluentAssertions;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Models.Growth;
using InfraSizingCalculator.Models.Pricing;
using InfraSizingCalculator.Services;
using Xunit;
using WarningSeverity = InfraSizingCalculator.Models.Growth.WarningSeverity;

namespace InfraSizingCalculator.UnitTests;

public class GrowthPlanningServiceTests
{
    private readonly GrowthPlanningService _service;

    public GrowthPlanningServiceTests()
    {
        _service = new GrowthPlanningService();
    }

    #region K8s Growth Projection Tests

    [Fact]
    public void CalculateK8sGrowthProjection_WithDefaultSettings_ReturnsValidProjection()
    {
        // Arrange
        var sizingResult = CreateBasicK8sSizingResult();
        var costEstimate = CreateBasicCostEstimate();
        var settings = new GrowthSettings();

        // Act
        var projection = _service.CalculateK8sGrowthProjection(
            sizingResult, costEstimate, settings, Distribution.Kubernetes);

        // Assert
        projection.Should().NotBeNull();
        projection.Baseline.Should().NotBeNull();
        projection.Points.Should().HaveCount(settings.ProjectionYears);
        projection.Summary.Should().NotBeNull();
    }

    [Fact]
    public void CalculateK8sGrowthProjection_BaselineContainsCorrectValues()
    {
        // Arrange
        var sizingResult = CreateBasicK8sSizingResult();
        var costEstimate = CreateBasicCostEstimate();
        var settings = new GrowthSettings { ProjectionYears = 3 };

        // Act
        var projection = _service.CalculateK8sGrowthProjection(
            sizingResult, costEstimate, settings, Distribution.Kubernetes);

        // Assert
        projection.Baseline.Year.Should().Be(0);
        projection.Baseline.Label.Should().Be("Current");
        projection.Baseline.ProjectedNodes.Should().Be(sizingResult.GrandTotal.TotalNodes);
        projection.Baseline.ProjectedCpu.Should().Be(sizingResult.GrandTotal.TotalCpu);
        projection.Baseline.ProjectedRamGB.Should().Be(sizingResult.GrandTotal.TotalRam);
        projection.Baseline.ProjectedMonthlyCost.Should().Be(costEstimate.MonthlyTotal);
    }

    [Fact]
    public void CalculateK8sGrowthProjection_WithNullCost_UsesZeroCost()
    {
        // Arrange
        var sizingResult = CreateBasicK8sSizingResult();
        var settings = new GrowthSettings();

        // Act
        var projection = _service.CalculateK8sGrowthProjection(
            sizingResult, null, settings, Distribution.Kubernetes);

        // Assert
        projection.Baseline.ProjectedMonthlyCost.Should().Be(0);
        projection.Baseline.ProjectedYearlyCost.Should().Be(0);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public void CalculateK8sGrowthProjection_ReturnsCorrectNumberOfPoints(int years)
    {
        // Arrange
        var sizingResult = CreateBasicK8sSizingResult();
        var settings = new GrowthSettings { ProjectionYears = years };

        // Act
        var projection = _service.CalculateK8sGrowthProjection(
            sizingResult, null, settings, Distribution.Kubernetes);

        // Assert
        projection.Points.Should().HaveCount(years);
        for (int i = 0; i < years; i++)
        {
            projection.Points[i].Year.Should().Be(i + 1);
            projection.Points[i].Label.Should().Be($"Year {i + 1}");
        }
    }

    [Fact]
    public void CalculateK8sGrowthProjection_LinearGrowth_AppliesCorrectGrowthRate()
    {
        // Arrange
        var sizingResult = CreateBasicK8sSizingResult();
        var settings = new GrowthSettings
        {
            AnnualGrowthRate = 20,
            ProjectionYears = 1,
            Pattern = GrowthPattern.Linear,
            IncludeCostProjections = false
        };

        // Act
        var projection = _service.CalculateK8sGrowthProjection(
            sizingResult, null, settings, Distribution.Kubernetes);

        // Assert
        var expectedNodes = (int)Math.Ceiling(sizingResult.GrandTotal.TotalNodes * 1.2);
        projection.Points[0].ProjectedNodes.Should().Be(expectedNodes);
    }

    [Fact]
    public void CalculateK8sGrowthProjection_ExponentialGrowth_CompoundsCorrectly()
    {
        // Arrange
        var sizingResult = CreateBasicK8sSizingResult();
        var settings = new GrowthSettings
        {
            AnnualGrowthRate = 20,
            ProjectionYears = 3,
            Pattern = GrowthPattern.Exponential
        };

        // Act
        var projection = _service.CalculateK8sGrowthProjection(
            sizingResult, null, settings, Distribution.Kubernetes);

        // Assert
        // Each year should show growth from previous year
        for (int i = 1; i < projection.Points.Count; i++)
        {
            projection.Points[i].ProjectedNodes.Should()
                .BeGreaterThan(projection.Points[i - 1].ProjectedNodes);
        }
    }

    [Fact]
    public void CalculateK8sGrowthProjection_SCurveGrowth_AppliesVariableGrowth()
    {
        // Arrange
        var sizingResult = CreateBasicK8sSizingResult();
        var settings = new GrowthSettings
        {
            AnnualGrowthRate = 20,
            ProjectionYears = 5,
            Pattern = GrowthPattern.SCurve
        };

        // Act
        var projection = _service.CalculateK8sGrowthProjection(
            sizingResult, null, settings, Distribution.Kubernetes);

        // Assert
        projection.Points.Should().HaveCount(5);
        // S-curve should show increasing growth rates in middle years
        projection.Points.Last().ProjectedNodes.Should()
            .BeGreaterThan(projection.Baseline.ProjectedNodes);
    }

    [Fact]
    public void CalculateK8sGrowthProjection_IncludesCostInflation()
    {
        // Arrange
        var sizingResult = CreateBasicK8sSizingResult();
        var costEstimate = CreateBasicCostEstimate();
        var settings = new GrowthSettings
        {
            AnnualGrowthRate = 0, // No app growth
            AnnualCostInflation = 10, // 10% inflation
            ProjectionYears = 1,
            IncludeCostProjections = true
        };

        // Act
        var projection = _service.CalculateK8sGrowthProjection(
            sizingResult, costEstimate, settings, Distribution.Kubernetes);

        // Assert
        // Cost should increase by inflation rate
        projection.Points[0].ProjectedMonthlyCost.Should()
            .BeGreaterThan(projection.Baseline.ProjectedMonthlyCost);
    }

    [Fact]
    public void CalculateK8sGrowthProjection_WhenCostProjectionsDisabled_ReturnsZeroCost()
    {
        // Arrange
        var sizingResult = CreateBasicK8sSizingResult();
        var costEstimate = CreateBasicCostEstimate();
        var settings = new GrowthSettings
        {
            IncludeCostProjections = false,
            ProjectionYears = 3
        };

        // Act
        var projection = _service.CalculateK8sGrowthProjection(
            sizingResult, costEstimate, settings, Distribution.Kubernetes);

        // Assert
        foreach (var point in projection.Points)
        {
            point.ProjectedMonthlyCost.Should().Be(0);
            point.ProjectedYearlyCost.Should().Be(0);
        }
    }

    [Fact]
    public void CalculateK8sGrowthProjection_CalculatesCumulativeGrowth()
    {
        // Arrange
        var sizingResult = CreateBasicK8sSizingResult();
        var settings = new GrowthSettings
        {
            AnnualGrowthRate = 50,
            ProjectionYears = 3,
            Pattern = GrowthPattern.Linear
        };

        // Act
        var projection = _service.CalculateK8sGrowthProjection(
            sizingResult, null, settings, Distribution.Kubernetes);

        // Assert
        projection.Points.Last().CumulativeGrowth.Should().BeGreaterThan(0);
        foreach (var point in projection.Points)
        {
            point.CumulativeGrowth.Should().BeGreaterThanOrEqualTo(point.GrowthFromPrevious);
        }
    }

    [Fact]
    public void CalculateK8sGrowthProjection_IncludesEnvironmentBreakdown()
    {
        // Arrange
        var sizingResult = CreateMultiEnvironmentK8sSizingResult();
        var costEstimate = CreateMultiEnvironmentCostEstimate();
        var settings = new GrowthSettings();

        // Act
        var projection = _service.CalculateK8sGrowthProjection(
            sizingResult, costEstimate, settings, Distribution.Kubernetes);

        // Assert
        projection.Baseline.EnvironmentBreakdown.Should().NotBeEmpty();
        projection.Baseline.EnvironmentBreakdown.Should().ContainKey(EnvironmentType.Prod);
    }

    #endregion

    #region VM Growth Projection Tests

    [Fact]
    public void CalculateVMGrowthProjection_WithDefaultSettings_ReturnsValidProjection()
    {
        // Arrange
        var vmResult = CreateBasicVMSizingResult();
        var costEstimate = CreateBasicCostEstimate();
        var settings = new GrowthSettings();

        // Act
        var projection = _service.CalculateVMGrowthProjection(vmResult, costEstimate, settings);

        // Assert
        projection.Should().NotBeNull();
        projection.Baseline.Should().NotBeNull();
        projection.Points.Should().HaveCount(settings.ProjectionYears);
        projection.Summary.Should().NotBeNull();
    }

    [Fact]
    public void CalculateVMGrowthProjection_BaselineReflectsVMCounts()
    {
        // Arrange
        var vmResult = CreateBasicVMSizingResult();
        var settings = new GrowthSettings();

        // Act
        var projection = _service.CalculateVMGrowthProjection(vmResult, null, settings);

        // Assert
        projection.Baseline.ProjectedApps.Should().Be(vmResult.GrandTotal.TotalVMs);
        projection.Baseline.ProjectedNodes.Should().Be(vmResult.GrandTotal.TotalVMs);
        projection.Baseline.ProjectedCpu.Should().Be(vmResult.GrandTotal.TotalCpu);
        projection.Baseline.ProjectedRamGB.Should().Be(vmResult.GrandTotal.TotalRam);
    }

    [Fact]
    public void CalculateVMGrowthProjection_AppliesGrowthToAllMetrics()
    {
        // Arrange
        var vmResult = CreateBasicVMSizingResult();
        var settings = new GrowthSettings
        {
            AnnualGrowthRate = 25,
            ProjectionYears = 3,
            Pattern = GrowthPattern.Linear
        };

        // Act
        var projection = _service.CalculateVMGrowthProjection(vmResult, null, settings);

        // Assert
        var lastPoint = projection.Points.Last();
        lastPoint.ProjectedApps.Should().BeGreaterThan(projection.Baseline.ProjectedApps);
        lastPoint.ProjectedCpu.Should().BeGreaterThan(projection.Baseline.ProjectedCpu);
        lastPoint.ProjectedRamGB.Should().BeGreaterThan(projection.Baseline.ProjectedRamGB);
        lastPoint.ProjectedStorageGB.Should().BeGreaterThan(projection.Baseline.ProjectedStorageGB);
    }

    [Fact]
    public void CalculateVMGrowthProjection_NoClusterLimitWarnings()
    {
        // Arrange
        var vmResult = CreateBasicVMSizingResult();
        var settings = new GrowthSettings
        {
            AnnualGrowthRate = 100,
            ProjectionYears = 5,
            ShowClusterLimitWarnings = true
        };

        // Act
        var projection = _service.CalculateVMGrowthProjection(vmResult, null, settings);

        // Assert
        // VM projections shouldn't have cluster limit warnings
        projection.Warnings.Should().BeEmpty();
    }

    #endregion

    #region Cluster Limits Tests

    [Theory]
    [InlineData(Distribution.EKS, 5000)]
    [InlineData(Distribution.AKS, 5000)]
    [InlineData(Distribution.GKE, 15000)]
    [InlineData(Distribution.OKE, 2000)]
    [InlineData(Distribution.OpenShift, 2000)]
    [InlineData(Distribution.K3s, 500)]
    [InlineData(Distribution.MicroK8s, 200)]
    [InlineData(Distribution.Kubernetes, 5000)]
    public void GetClusterLimits_ReturnsCorrectNodeLimitsForDistribution(
        Distribution distribution, int expectedNodeLimit)
    {
        // Act
        var limits = _service.GetClusterLimits(distribution);

        // Assert
        limits.Should().ContainKey("Nodes");
        limits["Nodes"].Should().Be(expectedNodeLimit);
    }

    [Theory]
    [InlineData(Distribution.AKS, 250)]
    [InlineData(Distribution.OpenShift, 250)]
    [InlineData(Distribution.EKS, 110)]
    [InlineData(Distribution.Kubernetes, 110)]
    public void GetClusterLimits_ReturnsCorrectPodsPerNodeLimit(
        Distribution distribution, int expectedPodsPerNode)
    {
        // Act
        var limits = _service.GetClusterLimits(distribution);

        // Assert
        limits.Should().ContainKey("PodsPerNode");
        limits["PodsPerNode"].Should().Be(expectedPodsPerNode);
    }

    [Fact]
    public void GetClusterLimits_AllDistributionsHaveTotalPodsLimit()
    {
        // Arrange
        var distributions = new[]
        {
            Distribution.EKS, Distribution.AKS, Distribution.GKE, Distribution.OKE,
            Distribution.OpenShift, Distribution.K3s, Distribution.MicroK8s, Distribution.Kubernetes
        };

        // Act & Assert
        foreach (var distribution in distributions)
        {
            var limits = _service.GetClusterLimits(distribution);
            limits.Should().ContainKey("TotalPods");
            limits["TotalPods"].Should().BeGreaterThan(0);
        }
    }

    #endregion

    #region Year To Limit Calculation Tests

    [Fact]
    public void CalculateYearToLimit_CurrentValueAtLimit_ReturnsZero()
    {
        // Arrange
        double currentValue = 100;
        double limit = 100;
        double growthRate = 20;

        // Act
        var result = _service.CalculateYearToLimit(currentValue, limit, growthRate, GrowthPattern.Linear);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void CalculateYearToLimit_CurrentValueAboveLimit_ReturnsZero()
    {
        // Arrange
        double currentValue = 150;
        double limit = 100;
        double growthRate = 20;

        // Act
        var result = _service.CalculateYearToLimit(currentValue, limit, growthRate, GrowthPattern.Linear);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void CalculateYearToLimit_NoGrowth_ReturnsNull()
    {
        // Arrange
        double currentValue = 50;
        double limit = 100;
        double growthRate = 0;

        // Act
        var result = _service.CalculateYearToLimit(currentValue, limit, growthRate, GrowthPattern.Linear);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void CalculateYearToLimit_NegativeGrowth_ReturnsNull()
    {
        // Arrange
        double currentValue = 50;
        double limit = 100;
        double growthRate = -10;

        // Act
        var result = _service.CalculateYearToLimit(currentValue, limit, growthRate, GrowthPattern.Linear);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void CalculateYearToLimit_HighGrowthReachesLimitQuickly_ReturnsCorrectYear()
    {
        // Arrange
        double currentValue = 50;
        double limit = 100;
        double growthRate = 100; // 100% growth = doubles each year

        // Act
        var result = _service.CalculateYearToLimit(currentValue, limit, growthRate, GrowthPattern.Linear);

        // Assert
        result.Should().Be(1); // Should reach limit in year 1
    }

    [Fact]
    public void CalculateYearToLimit_SlowGrowth_ReturnsLaterYear()
    {
        // Arrange
        double currentValue = 80;
        double limit = 100;
        double growthRate = 10; // 10% growth

        // Act
        var result = _service.CalculateYearToLimit(currentValue, limit, growthRate, GrowthPattern.Linear);

        // Assert
        result.Should().BeGreaterThanOrEqualTo(2);
        result.Should().BeLessThanOrEqualTo(10);
    }

    [Fact]
    public void CalculateYearToLimit_VerySlowGrowthNeverReaches_ReturnsNull()
    {
        // Arrange
        double currentValue = 10;
        double limit = 10000;
        double growthRate = 1; // 1% growth per year

        // Act
        var result = _service.CalculateYearToLimit(currentValue, limit, growthRate, GrowthPattern.Linear);

        // Assert
        result.Should().BeNull(); // Won't reach in 10 years
    }

    #endregion

    #region Cluster Limit Warnings Tests

    [Fact]
    public void CalculateK8sGrowthProjection_WhenApproaching70PercentOfLimit_GeneratesWarning()
    {
        // Arrange
        var sizingResult = CreateLargeK8sSizingResult(350); // 350 nodes on K3s limit of 500
        var settings = new GrowthSettings
        {
            AnnualGrowthRate = 10,
            ProjectionYears = 3,
            ShowClusterLimitWarnings = true
        };

        // Act
        var projection = _service.CalculateK8sGrowthProjection(
            sizingResult, null, settings, Distribution.K3s);

        // Assert
        projection.Warnings.Should().ContainSingle(w =>
            w.Type == WarningType.NodeLimit &&
            w.Severity == WarningSeverity.Warning);
    }

    [Fact]
    public void CalculateK8sGrowthProjection_WhenApproaching90PercentOfLimit_GeneratesCriticalWarning()
    {
        // Arrange
        var sizingResult = CreateLargeK8sSizingResult(400); // 400 nodes on K3s limit of 500
        var settings = new GrowthSettings
        {
            AnnualGrowthRate = 30,
            ProjectionYears = 3,
            ShowClusterLimitWarnings = true
        };

        // Act
        var projection = _service.CalculateK8sGrowthProjection(
            sizingResult, null, settings, Distribution.K3s);

        // Assert
        projection.Warnings.Should().Contain(w =>
            w.Type == WarningType.NodeLimit &&
            w.Severity == WarningSeverity.Critical);
    }

    [Fact]
    public void CalculateK8sGrowthProjection_WhenWarningsDisabled_NoWarningsGenerated()
    {
        // Arrange
        var sizingResult = CreateLargeK8sSizingResult(480); // Very close to K3s limit
        var settings = new GrowthSettings
        {
            AnnualGrowthRate = 50,
            ProjectionYears = 3,
            ShowClusterLimitWarnings = false
        };

        // Act
        var projection = _service.CalculateK8sGrowthProjection(
            sizingResult, null, settings, Distribution.K3s);

        // Assert
        projection.Warnings.Should().BeEmpty();
    }

    #endregion

    #region Recommendations Tests

    [Fact]
    public void GenerateRecommendations_HighGrowth_RecommendsAutoscaling()
    {
        // Arrange
        var sizingResult = CreateBasicK8sSizingResult();
        var settings = new GrowthSettings
        {
            AnnualGrowthRate = 100,
            ProjectionYears = 3
        };

        // Act
        var projection = _service.CalculateK8sGrowthProjection(
            sizingResult, null, settings, Distribution.Kubernetes);

        // Assert
        projection.Recommendations.Should().Contain(r =>
            r.Type == RecommendationType.EnableAutoscaling);
    }

    [Fact]
    public void GenerateRecommendations_SignificantNodeGrowth_RecommendsLargerNodes()
    {
        // Arrange
        var sizingResult = CreateBasicK8sSizingResult();
        var settings = new GrowthSettings
        {
            AnnualGrowthRate = 60,
            ProjectionYears = 3
        };

        // Act
        var projection = _service.CalculateK8sGrowthProjection(
            sizingResult, null, settings, Distribution.Kubernetes);

        // Assert
        projection.Recommendations.Should().Contain(r =>
            r.Type == RecommendationType.UpgradeNodeSize);
    }

    [Fact]
    public void GenerateRecommendations_CriticalWarnings_RecommendsClusterSplit()
    {
        // Arrange
        var sizingResult = CreateLargeK8sSizingResult(180); // Near MicroK8s limit of 200
        var settings = new GrowthSettings
        {
            AnnualGrowthRate = 30,
            ProjectionYears = 3,
            ShowClusterLimitWarnings = true
        };

        // Act
        var projection = _service.CalculateK8sGrowthProjection(
            sizingResult, null, settings, Distribution.MicroK8s);

        // Assert
        if (projection.Warnings.Any(w => w.Severity == WarningSeverity.Critical))
        {
            projection.Recommendations.Should().Contain(r =>
                r.Type == RecommendationType.SplitCluster);
        }
    }

    [Fact]
    public void GenerateRecommendations_K3sWithManyNodes_RecommendsManagedService()
    {
        // Arrange
        var sizingResult = CreateLargeK8sSizingResult(250);
        var settings = new GrowthSettings
        {
            AnnualGrowthRate = 10,
            ProjectionYears = 1
        };

        // Act
        var projection = _service.CalculateK8sGrowthProjection(
            sizingResult, null, settings, Distribution.K3s);

        // Assert
        projection.Recommendations.Should().Contain(r =>
            r.Type == RecommendationType.ConsiderManagedService);
    }

    [Fact]
    public void GenerateRecommendations_MicroK8sWithManyNodes_RecommendsEnterpriseDistribution()
    {
        // Arrange
        var sizingResult = CreateLargeK8sSizingResult(150);
        var settings = new GrowthSettings
        {
            AnnualGrowthRate = 10,
            ProjectionYears = 1
        };

        // Act
        var projection = _service.CalculateK8sGrowthProjection(
            sizingResult, null, settings, Distribution.MicroK8s);

        // Assert
        projection.Recommendations.Should().Contain(r =>
            r.Type == RecommendationType.ConsiderManagedService);
    }

    [Fact]
    public void GenerateRecommendations_HighGrowthRate_RecommendsScaling()
    {
        // Arrange
        var sizingResult = CreateBasicK8sSizingResult();
        var costEstimate = CreateBasicCostEstimate();
        var settings = new GrowthSettings
        {
            AnnualGrowthRate = 50,
            ProjectionYears = 3,
            IncludeCostProjections = true,
            AnnualCostInflation = 5
        };

        // Act
        var projection = _service.CalculateK8sGrowthProjection(
            sizingResult, costEstimate, settings, Distribution.Kubernetes);

        // Assert - High growth rates should generate scaling recommendations
        // such as EnableAutoscaling or UpgradeNodeSize
        projection.Recommendations.Should().Contain(r =>
            r.Type == RecommendationType.EnableAutoscaling ||
            r.Type == RecommendationType.UpgradeNodeSize);
    }

    [Fact]
    public void GenerateRecommendations_RecommendationsArePrioritized()
    {
        // Arrange
        var sizingResult = CreateLargeK8sSizingResult(180);
        var costEstimate = CreateBasicCostEstimate();
        var settings = new GrowthSettings
        {
            AnnualGrowthRate = 100,
            ProjectionYears = 5,
            IncludeCostProjections = true,
            ShowClusterLimitWarnings = true
        };

        // Act
        var projection = _service.CalculateK8sGrowthProjection(
            sizingResult, costEstimate, settings, Distribution.MicroK8s);

        // Assert
        if (projection.Recommendations.Count > 1)
        {
            for (int i = 1; i < projection.Recommendations.Count; i++)
            {
                projection.Recommendations[i].Priority.Should()
                    .BeGreaterThanOrEqualTo(projection.Recommendations[i - 1].Priority);
            }
        }
    }

    #endregion

    #region Summary Calculation Tests

    [Fact]
    public void CalculateK8sGrowthProjection_SummaryContainsCorrectGrowthStats()
    {
        // Arrange
        var sizingResult = CreateBasicK8sSizingResult();
        var settings = new GrowthSettings
        {
            AnnualGrowthRate = 20,
            ProjectionYears = 3
        };

        // Act
        var projection = _service.CalculateK8sGrowthProjection(
            sizingResult, null, settings, Distribution.Kubernetes);

        // Assert
        projection.Summary.TotalAppGrowth.Should().BeGreaterThanOrEqualTo(0);
        projection.Summary.TotalNodeGrowth.Should().BeGreaterThanOrEqualTo(0);
        projection.Summary.PercentageAppGrowth.Should().BeGreaterThanOrEqualTo(0);
        projection.Summary.PercentageNodeGrowth.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void CalculateK8sGrowthProjection_SummaryContainsCostStats()
    {
        // Arrange
        var sizingResult = CreateBasicK8sSizingResult();
        var costEstimate = CreateBasicCostEstimate();
        var settings = new GrowthSettings
        {
            AnnualGrowthRate = 20,
            ProjectionYears = 3,
            IncludeCostProjections = true
        };

        // Act
        var projection = _service.CalculateK8sGrowthProjection(
            sizingResult, costEstimate, settings, Distribution.Kubernetes);

        // Assert
        projection.Summary.TotalCostOverPeriod.Should().BeGreaterThan(0);
        projection.Summary.AverageYearlyCost.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CalculateK8sGrowthProjection_SummaryTracksWarnings()
    {
        // Arrange
        var sizingResult = CreateLargeK8sSizingResult(400);
        var settings = new GrowthSettings
        {
            AnnualGrowthRate = 30,
            ProjectionYears = 5,
            ShowClusterLimitWarnings = true
        };

        // Act
        var projection = _service.CalculateK8sGrowthProjection(
            sizingResult, null, settings, Distribution.K3s);

        // Assert
        projection.Summary.WarningCount.Should().Be(projection.Warnings.Count);
        projection.Summary.CriticalWarningCount.Should().Be(
            projection.Warnings.Count(w => w.Severity == WarningSeverity.Critical));
    }

    [Fact]
    public void CalculateK8sGrowthProjection_MajorScalingYear_ReflectsCriticalWarning()
    {
        // Arrange
        var sizingResult = CreateLargeK8sSizingResult(400);
        var settings = new GrowthSettings
        {
            AnnualGrowthRate = 30,
            ProjectionYears = 5,
            ShowClusterLimitWarnings = true
        };

        // Act
        var projection = _service.CalculateK8sGrowthProjection(
            sizingResult, null, settings, Distribution.K3s);

        // Assert
        var criticalWarning = projection.Warnings.FirstOrDefault(w =>
            w.Severity == WarningSeverity.Critical);
        if (criticalWarning != null)
        {
            projection.Summary.MajorScalingYear.Should().Be(criticalWarning.YearTriggered);
        }
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void CalculateK8sGrowthProjection_ZeroGrowthRate_NoChange()
    {
        // Arrange
        var sizingResult = CreateBasicK8sSizingResult();
        var settings = new GrowthSettings
        {
            AnnualGrowthRate = 0,
            ProjectionYears = 3,
            AnnualCostInflation = 0,
            IncludeCostProjections = false
        };

        // Act
        var projection = _service.CalculateK8sGrowthProjection(
            sizingResult, null, settings, Distribution.Kubernetes);

        // Assert
        foreach (var point in projection.Points)
        {
            point.ProjectedNodes.Should().Be(projection.Baseline.ProjectedNodes);
            point.ProjectedCpu.Should().Be(projection.Baseline.ProjectedCpu);
        }
    }

    [Fact]
    public void CalculateK8sGrowthProjection_VeryHighGrowthRate_HandlesLargeNumbers()
    {
        // Arrange
        var sizingResult = CreateBasicK8sSizingResult();
        var settings = new GrowthSettings
        {
            AnnualGrowthRate = 200, // 200% growth
            ProjectionYears = 5
        };

        // Act
        var projection = _service.CalculateK8sGrowthProjection(
            sizingResult, null, settings, Distribution.Kubernetes);

        // Assert
        projection.Points.Should().HaveCount(5);
        projection.Points.Last().ProjectedNodes.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CalculateK8sGrowthProjection_EmptyEnvironments_HandlesGracefully()
    {
        // Arrange
        var input = new K8sSizingInput
        {
            Distribution = Distribution.Kubernetes,
            ProdApps = new AppConfig(),
            NonProdApps = new AppConfig()
        };
        var sizingResult = new K8sSizingResult
        {
            Environments = new List<EnvironmentResult>(),
            GrandTotal = new GrandTotal
            {
                TotalNodes = 0,
                TotalCpu = 0,
                TotalRam = 0,
                TotalDisk = 0
            },
            Configuration = input
        };
        var settings = new GrowthSettings();

        // Act
        var projection = _service.CalculateK8sGrowthProjection(
            sizingResult, null, settings, Distribution.Kubernetes);

        // Assert
        projection.Should().NotBeNull();
        projection.Baseline.ProjectedNodes.Should().Be(0);
    }

    #endregion

    #region Helper Methods

    private K8sSizingResult CreateBasicK8sSizingResult()
    {
        var input = new K8sSizingInput
        {
            Distribution = Distribution.Kubernetes,
            ProdApps = new AppConfig { Small = 5, Medium = 10, Large = 5 },
            NonProdApps = new AppConfig { Small = 5, Medium = 10, Large = 5 }
        };
        return new K8sSizingResult
        {
            Environments = new List<EnvironmentResult>
            {
                new EnvironmentResult
                {
                    Environment = EnvironmentType.Prod,
                    EnvironmentName = "Production",
                    IsProd = true,
                    Apps = 20,
                    Masters = 3,
                    Workers = 10,
                    Infra = 3,
                    TotalNodes = 16,
                    TotalCpu = 160,
                    TotalRam = 320,
                    TotalDisk = 1000
                }
            },
            GrandTotal = new GrandTotal
            {
                TotalNodes = 16,
                TotalMasters = 3,
                TotalWorkers = 10,
                TotalInfra = 3,
                TotalCpu = 160,
                TotalRam = 320,
                TotalDisk = 1000
            },
            Configuration = input
        };
    }

    private K8sSizingResult CreateMultiEnvironmentK8sSizingResult()
    {
        var input = new K8sSizingInput
        {
            Distribution = Distribution.Kubernetes,
            ProdApps = new AppConfig { Small = 5, Medium = 10, Large = 5 },
            NonProdApps = new AppConfig { Small = 5, Medium = 10, Large = 5 }
        };
        return new K8sSizingResult
        {
            Environments = new List<EnvironmentResult>
            {
                new EnvironmentResult
                {
                    Environment = EnvironmentType.Prod,
                    EnvironmentName = "Production",
                    IsProd = true,
                    Apps = 20,
                    Masters = 3,
                    Workers = 15,
                    Infra = 3,
                    TotalNodes = 21,
                    TotalCpu = 210,
                    TotalRam = 420,
                    TotalDisk = 1500
                },
                new EnvironmentResult
                {
                    Environment = EnvironmentType.Dev,
                    EnvironmentName = "Development",
                    IsProd = false,
                    Apps = 20,
                    Masters = 3,
                    Workers = 5,
                    Infra = 0,
                    TotalNodes = 8,
                    TotalCpu = 80,
                    TotalRam = 160,
                    TotalDisk = 500
                }
            },
            GrandTotal = new GrandTotal
            {
                TotalNodes = 29,
                TotalMasters = 6,
                TotalWorkers = 20,
                TotalInfra = 3,
                TotalCpu = 290,
                TotalRam = 580,
                TotalDisk = 2000
            },
            Configuration = input
        };
    }

    private K8sSizingResult CreateLargeK8sSizingResult(int workerNodes)
    {
        var input = new K8sSizingInput
        {
            Distribution = Distribution.Kubernetes,
            ProdApps = new AppConfig { Small = workerNodes, Medium = workerNodes * 2, Large = workerNodes * 2 },
            NonProdApps = new AppConfig { Small = workerNodes }
        };
        return new K8sSizingResult
        {
            Environments = new List<EnvironmentResult>
            {
                new EnvironmentResult
                {
                    Environment = EnvironmentType.Prod,
                    EnvironmentName = "Production",
                    IsProd = true,
                    Apps = workerNodes * 5,
                    Masters = 5,
                    Workers = workerNodes,
                    Infra = 5,
                    TotalNodes = workerNodes + 10,
                    TotalCpu = workerNodes * 16,
                    TotalRam = workerNodes * 32,
                    TotalDisk = workerNodes * 100
                }
            },
            GrandTotal = new GrandTotal
            {
                TotalNodes = workerNodes + 10,
                TotalMasters = 5,
                TotalWorkers = workerNodes,
                TotalInfra = 5,
                TotalCpu = workerNodes * 16,
                TotalRam = workerNodes * 32,
                TotalDisk = workerNodes * 100
            },
            Configuration = input
        };
    }

    private VMSizingResult CreateBasicVMSizingResult()
    {
        return new VMSizingResult
        {
            Environments = new List<VMEnvironmentResult>
            {
                new VMEnvironmentResult
                {
                    Environment = EnvironmentType.Prod,
                    EnvironmentName = "Production",
                    IsProd = true,
                    Roles = new List<VMRoleResult>
                    {
                        new VMRoleResult
                        {
                            Role = ServerRole.App,
                            RoleName = "Application",
                            TotalInstances = 4,
                            CpuPerInstance = 8,
                            RamPerInstance = 16
                        }
                    }
                }
            },
            GrandTotal = new VMGrandTotal
            {
                TotalVMs = 10,
                TotalCpu = 80,
                TotalRam = 160,
                TotalDisk = 500
            }
        };
    }

    private CostEstimate CreateBasicCostEstimate()
    {
        return new CostEstimate
        {
            MonthlyTotal = 10000m,
            // YearlyTotal is a computed property
            EnvironmentCosts = new Dictionary<EnvironmentType, EnvironmentCost>
            {
                [EnvironmentType.Prod] = new EnvironmentCost
                {
                    Environment = EnvironmentType.Prod,
                    MonthlyCost = 10000m
                }
            }
        };
    }

    private CostEstimate CreateMultiEnvironmentCostEstimate()
    {
        return new CostEstimate
        {
            MonthlyTotal = 15000m,
            // YearlyTotal is a computed property
            EnvironmentCosts = new Dictionary<EnvironmentType, EnvironmentCost>
            {
                [EnvironmentType.Prod] = new EnvironmentCost
                {
                    Environment = EnvironmentType.Prod,
                    MonthlyCost = 10000m
                },
                [EnvironmentType.Dev] = new EnvironmentCost
                {
                    Environment = EnvironmentType.Dev,
                    MonthlyCost = 5000m
                }
            }
        };
    }

    #endregion
}
