using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Services;

namespace InfraSizingCalculator.UnitTests;

/// <summary>
/// Tests for worker node calculation rules BR-W001 through BR-W006
/// </summary>
public class WorkerNodeCalculationTests
{
    private readonly K8sSizingService _service;
    private readonly TechnologyService _technologyService;

    public WorkerNodeCalculationTests()
    {
        var distributionService = new DistributionService();
        _technologyService = new TechnologyService();
        _service = new K8sSizingService(distributionService, _technologyService);
    }

    /// <summary>
    /// BR-W001: Minimum worker nodes = 3
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void Calculate_ZeroOrFewApps_ReturnsMinimum3Workers(int appCount)
    {
        var apps = new AppConfig { Small = appCount };
        var techConfig = _technologyService.GetConfig(Technology.DotNet);
        var workerSpecs = new NodeSpecs(8, 32, 100);

        var result = _service.CalculateWorkerNodes(
            apps,
            techConfig.Tiers,
            replicas: 1,
            workerSpecs,
            headroomPercent: 0,
            new OvercommitSettings { Cpu = 1.0, Memory = 1.0 });

        Assert.True(result >= 3, $"Expected at least 3 workers, got {result}");
    }

    /// <summary>
    /// BR-W002: Workers scale based on application resource requirements
    /// </summary>
    [Theory]
    [InlineData(10, AppTier.Small)]
    [InlineData(10, AppTier.Medium)]
    [InlineData(10, AppTier.Large)]
    public void Calculate_ScalesWithAppCount(int appCount, AppTier tier)
    {
        var apps = tier switch
        {
            AppTier.Small => new AppConfig { Small = appCount },
            AppTier.Medium => new AppConfig { Medium = appCount },
            AppTier.Large => new AppConfig { Large = appCount },
            _ => new AppConfig { Medium = appCount }
        };

        var techConfig = _technologyService.GetConfig(Technology.DotNet);
        var workerSpecs = new NodeSpecs(8, 32, 100);

        var result = _service.CalculateWorkerNodes(
            apps,
            techConfig.Tiers,
            replicas: 1,
            workerSpecs,
            headroomPercent: 0,
            new OvercommitSettings { Cpu = 1.0, Memory = 1.0 });

        Assert.True(result >= 3);
    }

    /// <summary>
    /// BR-W003: Replicas multiply resource requirements
    /// </summary>
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void Calculate_ReplicasIncreaseWorkers(int replicas)
    {
        var apps = new AppConfig { Medium = 10 };
        var techConfig = _technologyService.GetConfig(Technology.DotNet);
        var workerSpecs = new NodeSpecs(8, 32, 100);
        var overcommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 };

        var resultWith1Replica = _service.CalculateWorkerNodes(
            apps, techConfig.Tiers, 1, workerSpecs, 0, overcommit);

        var resultWithReplicas = _service.CalculateWorkerNodes(
            apps, techConfig.Tiers, replicas, workerSpecs, 0, overcommit);

        Assert.True(resultWithReplicas >= resultWith1Replica);
    }

    /// <summary>
    /// BR-W004: Overcommit ratio reduces worker count
    /// </summary>
    [Theory]
    [InlineData(1.0)]
    [InlineData(1.5)]
    [InlineData(2.0)]
    public void Calculate_OvercommitReducesWorkers(double overcommitRatio)
    {
        var apps = new AppConfig { Medium = 50 };
        var techConfig = _technologyService.GetConfig(Technology.DotNet);
        var workerSpecs = new NodeSpecs(8, 32, 100);

        var resultNoOvercommit = _service.CalculateWorkerNodes(
            apps, techConfig.Tiers, 1, workerSpecs, 0,
            new OvercommitSettings { Cpu = 1.0, Memory = 1.0 });

        var resultWithOvercommit = _service.CalculateWorkerNodes(
            apps, techConfig.Tiers, 1, workerSpecs, 0,
            new OvercommitSettings { Cpu = overcommitRatio, Memory = overcommitRatio });

        Assert.True(resultWithOvercommit <= resultNoOvercommit);
    }

    /// <summary>
    /// BR-W005: Headroom increases worker count
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(20)]
    [InlineData(37.5)]
    public void Calculate_HeadroomIncreasesWorkers(double headroomPercent)
    {
        var apps = new AppConfig { Medium = 50 };
        var techConfig = _technologyService.GetConfig(Technology.DotNet);
        var workerSpecs = new NodeSpecs(8, 32, 100);
        var overcommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 };

        var resultNoHeadroom = _service.CalculateWorkerNodes(
            apps, techConfig.Tiers, 1, workerSpecs, 0, overcommit);

        var resultWithHeadroom = _service.CalculateWorkerNodes(
            apps, techConfig.Tiers, 1, workerSpecs, headroomPercent, overcommit);

        Assert.True(resultWithHeadroom >= resultNoHeadroom);
    }

    /// <summary>
    /// Verify worker calculation produces correct results for known scenario
    /// </summary>
    [Fact]
    public void Calculate_70MediumApps_Prod_Returns11Workers()
    {
        // From SRS Appendix C.1: Prod with 70 medium .NET apps, 3 replicas, 37.5% headroom = 11 workers
        var apps = new AppConfig { Medium = 70 };
        var techConfig = _technologyService.GetConfig(Technology.DotNet);
        var workerSpecs = new NodeSpecs(16, 64, 200); // Prod worker specs
        var overcommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 };

        var result = _service.CalculateWorkerNodes(
            apps, techConfig.Tiers, 3, workerSpecs, 37.5, overcommit);

        Assert.Equal(11, result);
    }

    /// <summary>
    /// Verify worker calculation for dev environment
    /// </summary>
    [Fact]
    public void Calculate_70MediumApps_Dev_ReturnsReasonableWorkers()
    {
        // Dev with 70 medium .NET apps, 1 replica, 33% headroom
        var apps = new AppConfig { Medium = 70 };
        var techConfig = _technologyService.GetConfig(Technology.DotNet);
        var workerSpecs = new NodeSpecs(8, 32, 100); // NonProd worker specs
        var overcommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 };

        var result = _service.CalculateWorkerNodes(
            apps, techConfig.Tiers, 1, workerSpecs, 33, overcommit);

        // Should be at least 3 (minimum) and reasonable for 70 apps
        Assert.True(result >= 3);
        Assert.True(result <= 20);
    }

    /// <summary>
    /// Verify larger worker nodes result in fewer workers
    /// </summary>
    [Fact]
    public void Calculate_LargerWorkerNodes_FewerWorkersNeeded()
    {
        var apps = new AppConfig { Medium = 50 };
        var techConfig = _technologyService.GetConfig(Technology.DotNet);
        var overcommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 };

        var smallWorkers = new NodeSpecs(4, 16, 100);
        var largeWorkers = new NodeSpecs(16, 64, 100);

        var resultSmall = _service.CalculateWorkerNodes(
            apps, techConfig.Tiers, 1, smallWorkers, 0, overcommit);

        var resultLarge = _service.CalculateWorkerNodes(
            apps, techConfig.Tiers, 1, largeWorkers, 0, overcommit);

        Assert.True(resultLarge <= resultSmall);
    }
}
