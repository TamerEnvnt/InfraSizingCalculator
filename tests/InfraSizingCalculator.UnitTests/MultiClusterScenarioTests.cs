using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Services;

namespace InfraSizingCalculator.UnitTests;

/// <summary>
/// Comprehensive tests for the primary test case:
/// 70 Medium .NET Apps on OpenShift Multi-Cluster mode
///
/// Expected Results from SRS Appendix C.1:
/// - Dev:   10 nodes (4 workers, 3 masters, 3 infra) - 112 CPU, 448 GB RAM
/// - Test:  10 nodes (4 workers, 3 masters, 3 infra) - 112 CPU, 448 GB RAM
/// - Stage: 12 nodes (6 workers, 3 masters, 3 infra) - 144 CPU, 576 GB RAM
/// - Prod:  19 nodes (11 workers, 3 masters, 5 infra) - 240 CPU, 960 GB RAM
/// - DR:    19 nodes (11 workers, 3 masters, 5 infra) - 240 CPU, 960 GB RAM
/// - Grand Total: 70 nodes, 848 CPU, 3,392 GB RAM
/// </summary>
public class MultiClusterScenarioTests
{
    private readonly K8sSizingService _service;
    private readonly DistributionService _distributionService;
    private readonly TechnologyService _technologyService;

    public MultiClusterScenarioTests()
    {
        _distributionService = new DistributionService();
        _technologyService = new TechnologyService();
        _service = new K8sSizingService(_distributionService, _technologyService);
    }

    /// <summary>
    /// Create the standard test input for 70 Medium .NET apps on OpenShift MultiCluster
    /// </summary>
    private K8sSizingInput CreateStandardInput()
    {
        return new K8sSizingInput
        {
            Technology = Technology.DotNet,
            Distribution = Distribution.OpenShift,
            ClusterMode = ClusterMode.MultiCluster,
            EnabledEnvironments = new HashSet<EnvironmentType>
            {
                EnvironmentType.Dev,
                EnvironmentType.Test,
                EnvironmentType.Stage,
                EnvironmentType.Prod,
                EnvironmentType.DR
            },
            ProdApps = new AppConfig { Medium = 70 },
            NonProdApps = new AppConfig { Medium = 70 },
            Replicas = new ReplicaSettings
            {
                NonProd = 1,  // Dev, Test
                Stage = 2,
                Prod = 3      // Prod, DR
            },
            Headroom = new HeadroomSettings
            {
                Dev = 33,
                Test = 33,
                Stage = 0,
                Prod = 37.5,
                DR = 37.5
            },
            EnableHeadroom = true,
            ProdOvercommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 },
            NonProdOvercommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 }
        };
    }

    [Fact]
    public void MultiCluster_70MediumDotNet_OpenShift_GrandTotal()
    {
        // Arrange
        var input = CreateStandardInput();

        // Act
        var result = _service.Calculate(input);

        // Assert - Grand Totals (from SRS Appendix C.1)
        Assert.Equal(70, result.GrandTotal.TotalNodes);
        Assert.Equal(848, result.GrandTotal.TotalCpu);
        Assert.Equal(3392, result.GrandTotal.TotalRam);

        // Verify node breakdown
        Assert.Equal(15, result.GrandTotal.TotalMasters);  // 5 environments × 3 masters
        Assert.Equal(19, result.GrandTotal.TotalInfra);    // 3+3+3+5+5
        Assert.Equal(36, result.GrandTotal.TotalWorkers);  // 4+4+6+11+11
    }

    [Fact]
    public void MultiCluster_DevEnvironment_Returns10Nodes()
    {
        // Arrange
        var input = CreateStandardInput();

        // Act
        var result = _service.Calculate(input);
        var dev = result.Environments.First(e => e.Environment == EnvironmentType.Dev);

        // Assert - Dev: 70 apps × 1 replica + 33% headroom = 4 workers
        Assert.Equal("Development", dev.EnvironmentName);
        Assert.Equal(70, dev.Apps);
        Assert.Equal(1, dev.Replicas);
        Assert.Equal(70, dev.Pods);
        Assert.Equal(4, dev.Workers);
        Assert.Equal(3, dev.Masters);
        Assert.Equal(3, dev.Infra);
        Assert.Equal(10, dev.TotalNodes);
        Assert.Equal(112, dev.TotalCpu);
        Assert.Equal(448, dev.TotalRam);
    }

    [Fact]
    public void MultiCluster_TestEnvironment_Returns10Nodes()
    {
        // Arrange
        var input = CreateStandardInput();

        // Act
        var result = _service.Calculate(input);
        var test = result.Environments.First(e => e.Environment == EnvironmentType.Test);

        // Assert - Test: 70 apps × 1 replica + 33% headroom = 4 workers
        Assert.Equal("Test", test.EnvironmentName);
        Assert.Equal(70, test.Apps);
        Assert.Equal(1, test.Replicas);
        Assert.Equal(70, test.Pods);
        Assert.Equal(4, test.Workers);
        Assert.Equal(3, test.Masters);
        Assert.Equal(3, test.Infra);
        Assert.Equal(10, test.TotalNodes);
        Assert.Equal(112, test.TotalCpu);
        Assert.Equal(448, test.TotalRam);
    }

    [Fact]
    public void MultiCluster_StageEnvironment_Returns12Nodes()
    {
        // Arrange
        var input = CreateStandardInput();

        // Act
        var result = _service.Calculate(input);
        var stage = result.Environments.First(e => e.Environment == EnvironmentType.Stage);

        // Assert - Stage: 70 apps × 2 replicas + 0% headroom = 6 workers
        Assert.Equal("Staging", stage.EnvironmentName);
        Assert.Equal(70, stage.Apps);
        Assert.Equal(2, stage.Replicas);
        Assert.Equal(140, stage.Pods);
        Assert.Equal(6, stage.Workers);
        Assert.Equal(3, stage.Masters);
        Assert.Equal(3, stage.Infra);
        Assert.Equal(12, stage.TotalNodes);
        Assert.Equal(144, stage.TotalCpu);
        Assert.Equal(576, stage.TotalRam);
    }

    [Fact]
    public void MultiCluster_ProdEnvironment_Returns19Nodes()
    {
        // Arrange
        var input = CreateStandardInput();

        // Act
        var result = _service.Calculate(input);
        var prod = result.Environments.First(e => e.Environment == EnvironmentType.Prod);

        // Assert - Prod: 70 apps × 3 replicas + 37.5% headroom = 11 workers
        // BR-I005: >=50 apps means 5 infra nodes minimum
        Assert.Equal("Production", prod.EnvironmentName);
        Assert.Equal(70, prod.Apps);
        Assert.Equal(3, prod.Replicas);
        Assert.Equal(210, prod.Pods);
        Assert.Equal(11, prod.Workers);
        Assert.Equal(3, prod.Masters);
        Assert.Equal(5, prod.Infra);  // Large deployment (>=50 apps) = 5 infra
        Assert.Equal(19, prod.TotalNodes);
        Assert.Equal(240, prod.TotalCpu);
        Assert.Equal(960, prod.TotalRam);
    }

    [Fact]
    public void MultiCluster_DREnvironment_Returns19Nodes()
    {
        // Arrange
        var input = CreateStandardInput();

        // Act
        var result = _service.Calculate(input);
        var dr = result.Environments.First(e => e.Environment == EnvironmentType.DR);

        // Assert - DR: Same as Prod (70 apps × 3 replicas + 37.5% headroom = 11 workers)
        Assert.Equal("DR", dr.EnvironmentName);
        Assert.Equal(70, dr.Apps);
        Assert.Equal(3, dr.Replicas);
        Assert.Equal(210, dr.Pods);
        Assert.Equal(11, dr.Workers);
        Assert.Equal(3, dr.Masters);
        Assert.Equal(5, dr.Infra);  // Large deployment (>=50 apps) = 5 infra
        Assert.Equal(19, dr.TotalNodes);
        Assert.Equal(240, dr.TotalCpu);
        Assert.Equal(960, dr.TotalRam);
    }

    [Fact]
    public void MultiCluster_AllEnvironmentsPresent()
    {
        // Arrange
        var input = CreateStandardInput();

        // Act
        var result = _service.Calculate(input);

        // Assert - All 5 environments should be present
        Assert.Equal(5, result.Environments.Count);
        Assert.Contains(result.Environments, e => e.Environment == EnvironmentType.Dev);
        Assert.Contains(result.Environments, e => e.Environment == EnvironmentType.Test);
        Assert.Contains(result.Environments, e => e.Environment == EnvironmentType.Stage);
        Assert.Contains(result.Environments, e => e.Environment == EnvironmentType.Prod);
        Assert.Contains(result.Environments, e => e.Environment == EnvironmentType.DR);
    }

    [Fact]
    public void MultiCluster_DistributionName_IsOpenShift()
    {
        // Arrange
        var input = CreateStandardInput();

        // Act
        var result = _service.Calculate(input);

        // Assert
        Assert.Equal("OpenShift (On-Prem)", result.DistributionName);
        Assert.Equal(".NET", result.TechnologyName);
    }

    [Fact]
    public void MultiCluster_DiskCalculation_Included()
    {
        // Arrange
        var input = CreateStandardInput();

        // Act
        var result = _service.Calculate(input);

        // Assert - Disk should be calculated for each environment
        foreach (var env in result.Environments)
        {
            Assert.True(env.TotalDisk > 0, $"Environment {env.EnvironmentName} should have disk calculated");
        }
        Assert.True(result.GrandTotal.TotalDisk > 0, "Grand total disk should be calculated");
    }

    /// <summary>
    /// Test managed control plane distributions (EKS, AKS, GKE) have 0 masters
    /// </summary>
    [Theory]
    [InlineData(Distribution.EKS)]
    [InlineData(Distribution.AKS)]
    [InlineData(Distribution.GKE)]
    public void ManagedDistributions_HaveZeroMasters(Distribution distribution)
    {
        // Arrange
        var input = CreateStandardInput();
        input.Distribution = distribution;

        // Act
        var result = _service.Calculate(input);

        // Assert - BR-M001: Managed control plane = 0 masters
        foreach (var env in result.Environments)
        {
            Assert.Equal(0, env.Masters);
        }
        Assert.Equal(0, result.GrandTotal.TotalMasters);
    }

    /// <summary>
    /// Test that only OpenShift has infra nodes
    /// </summary>
    [Theory]
    [InlineData(Distribution.Kubernetes)]
    [InlineData(Distribution.Rancher)]
    [InlineData(Distribution.K3s)]
    public void NonOpenShiftDistributions_HaveZeroInfraNodes(Distribution distribution)
    {
        // Arrange
        var input = CreateStandardInput();
        input.Distribution = distribution;

        // Act
        var result = _service.Calculate(input);

        // Assert - BR-I001: Only OpenShift has infra nodes
        foreach (var env in result.Environments)
        {
            Assert.Equal(0, env.Infra);
        }
        Assert.Equal(0, result.GrandTotal.TotalInfra);
    }
}
