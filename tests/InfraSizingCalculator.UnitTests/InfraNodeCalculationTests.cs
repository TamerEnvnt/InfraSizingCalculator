using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Services;

namespace InfraSizingCalculator.UnitTests;

/// <summary>
/// Tests for infrastructure node calculation rules BR-I001 through BR-I006
/// </summary>
public class InfraNodeCalculationTests
{
    private readonly K8sSizingService _service;

    public InfraNodeCalculationTests()
    {
        var distributionService = new DistributionService();
        var technologyService = new TechnologyService();
        _service = new K8sSizingService(distributionService, technologyService);
    }

    /// <summary>
    /// BR-I001: Only OpenShift has infra nodes; all other distributions have 0
    /// </summary>
    [Theory]
    [InlineData(10, true)]
    [InlineData(70, true)]
    [InlineData(100, false)]
    public void NoInfraNodes_Returns0(int totalApps, bool isProd)
    {
        var result = _service.CalculateInfraNodes(totalApps, isProd, hasInfraNodes: false);
        Assert.Equal(0, result);
    }

    /// <summary>
    /// BR-I002: Minimum infrastructure node count = 3
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(25)]
    public void SmallAppCount_ReturnsMinimum3Infra(int totalApps)
    {
        var result = _service.CalculateInfraNodes(totalApps, isProd: false, hasInfraNodes: true);
        Assert.Equal(3, result);
    }

    /// <summary>
    /// BR-I003: Maximum infrastructure node count = 10
    /// </summary>
    [Theory]
    [InlineData(300)]
    [InlineData(500)]
    [InlineData(1000)]
    public void LargeAppCount_ReturnsCappedAt10Infra(int totalApps)
    {
        var result = _service.CalculateInfraNodes(totalApps, isProd: true, hasInfraNodes: true);
        Assert.Equal(10, result);
    }

    /// <summary>
    /// BR-I004: Infrastructure nodes scale at 1 node per 25 applications
    /// </summary>
    [Theory]
    [InlineData(26, false, 3)]  // ceiling(26/25) = 2, but min is 3
    [InlineData(50, false, 3)]  // ceiling(50/25) = 2, but min is 3
    [InlineData(75, false, 3)]  // ceiling(75/25) = 3
    [InlineData(76, false, 4)]  // ceiling(76/25) = 4
    [InlineData(100, false, 4)] // ceiling(100/25) = 4
    [InlineData(125, false, 5)] // ceiling(125/25) = 5
    public void InfraNodes_ScaleAt1Per25Apps_NonProd(int totalApps, bool isProd, int expected)
    {
        var result = _service.CalculateInfraNodes(totalApps, isProd, hasInfraNodes: true);
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// BR-I005: Large production (>=50 apps) needs minimum 5 infra
    /// </summary>
    [Theory]
    [InlineData(50, 5)]   // Exactly threshold
    [InlineData(70, 5)]   // Above threshold but calculation would give less
    [InlineData(100, 5)]  // 100/25 = 4, but prod >= 50 forces 5
    [InlineData(125, 5)]  // 125/25 = 5, matches minimum
    [InlineData(150, 6)]  // 150/25 = 6, above minimum
    public void LargeProduction_ReturnsMinimum5Infra(int totalApps, int expected)
    {
        var result = _service.CalculateInfraNodes(totalApps, isProd: true, hasInfraNodes: true);
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// BR-I006: Small production (<50 apps) can use 3 infra nodes
    /// </summary>
    [Theory]
    [InlineData(10, 3)]
    [InlineData(25, 3)]
    [InlineData(49, 3)]  // Just below threshold
    public void SmallProduction_CanUse3Infra(int totalApps, int expected)
    {
        var result = _service.CalculateInfraNodes(totalApps, isProd: true, hasInfraNodes: true);
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// Verify OpenShift distribution has infra nodes
    /// </summary>
    [Fact]
    public void OpenShift_HasInfraNodes()
    {
        var distributionService = new DistributionService();
        var config = distributionService.GetConfig(Distribution.OpenShift);

        Assert.True(config.HasInfraNodes);
    }

    /// <summary>
    /// Verify non-OpenShift distributions don't have infra nodes
    /// </summary>
    [Theory]
    [InlineData(Distribution.Kubernetes)]
    [InlineData(Distribution.EKS)]
    [InlineData(Distribution.AKS)]
    [InlineData(Distribution.GKE)]
    [InlineData(Distribution.K3s)]
    [InlineData(Distribution.Rancher)]
    public void NonOpenShift_NoInfraNodes(Distribution distribution)
    {
        var distributionService = new DistributionService();
        var config = distributionService.GetConfig(distribution);

        Assert.False(config.HasInfraNodes);
    }
}
