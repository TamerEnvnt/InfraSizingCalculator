using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Services;

namespace InfraSizingCalculator.UnitTests;

/// <summary>
/// Critical validation test case from SRS Appendix C.1
/// 70 medium .NET apps on OpenShift = 70 nodes, 848 vCPU, 3,392 GB RAM
/// </summary>
public class ValidationTests
{
    private readonly K8sSizingService _service;

    public ValidationTests()
    {
        var distributionService = new DistributionService();
        var technologyService = new TechnologyService();
        _service = new K8sSizingService(distributionService, technologyService);
    }

    [Fact]
    public void Calculate_70MediumApps_OpenShift_Returns70Nodes_848CPU_3392RAM()
    {
        // Arrange - matches SRS Appendix C.1
        var input = new K8sSizingInput
        {
            Distribution = Distribution.OpenShift,
            Technology = Technology.DotNet,
            ClusterMode = ClusterMode.MultiCluster,
            ProdApps = new AppConfig { Medium = 70 },
            NonProdApps = new AppConfig { Medium = 70 },
            EnabledEnvironments = new HashSet<EnvironmentType>
            {
                EnvironmentType.Dev,
                EnvironmentType.Test,
                EnvironmentType.Stage,
                EnvironmentType.Prod,
                EnvironmentType.DR
            },
            EnableHeadroom = true,
            Replicas = new ReplicaSettings
            {
                Prod = 3,     // BR-R001
                NonProd = 1,  // BR-R002
                Stage = 2     // BR-R003
            },
            Headroom = new HeadroomSettings
            {
                Dev = 33,     // BR-H003
                Test = 33,    // BR-H004
                Stage = 0,    // BR-H005
                Prod = 37.5,  // BR-H006
                DR = 37.5     // BR-H007
            }
        };

        // Act
        var result = _service.Calculate(input);

        // Assert grand totals
        // BR-W006: Worker specs ALWAYS use ProdWorker (16 CPU, 64 GB RAM)
        // Dev/Test: 3×8 + 3×8 + 4×16 = 112, Stage: 3×8 + 3×8 + 6×16 = 144, Prod/DR: 3×8 + 5×8 + 11×16 = 240
        // Total: 112+112+144+240+240 = 848 CPU
        Assert.Equal(70, result.GrandTotal.TotalNodes);
        Assert.Equal(848, result.GrandTotal.TotalCpu);
        Assert.Equal(3392, result.GrandTotal.TotalRam);

        // Assert Development environment
        var dev = result.Environments.First(e => e.Environment == EnvironmentType.Dev);
        Assert.Equal(10, dev.TotalNodes);
        Assert.Equal(3, dev.Masters);
        Assert.Equal(3, dev.Infra);
        Assert.Equal(4, dev.Workers);

        // Assert Test environment
        var test = result.Environments.First(e => e.Environment == EnvironmentType.Test);
        Assert.Equal(10, test.TotalNodes);
        Assert.Equal(3, test.Masters);
        Assert.Equal(3, test.Infra);
        Assert.Equal(4, test.Workers);

        // Assert Staging environment
        var stage = result.Environments.First(e => e.Environment == EnvironmentType.Stage);
        Assert.Equal(12, stage.TotalNodes);
        Assert.Equal(3, stage.Masters);
        Assert.Equal(3, stage.Infra);
        Assert.Equal(6, stage.Workers);

        // Assert Production environment
        var prod = result.Environments.First(e => e.Environment == EnvironmentType.Prod);
        Assert.Equal(19, prod.TotalNodes);
        Assert.Equal(3, prod.Masters);
        Assert.Equal(5, prod.Infra);  // BR-I005: >= 50 apps = 5 infra
        Assert.Equal(11, prod.Workers);

        // Assert DR environment
        var dr = result.Environments.First(e => e.Environment == EnvironmentType.DR);
        Assert.Equal(19, dr.TotalNodes);
        Assert.Equal(3, dr.Masters);
        Assert.Equal(5, dr.Infra);  // BR-I005
        Assert.Equal(11, dr.Workers);
    }

    [Fact]
    public void Calculate_70MediumApps_VerifyNodeCountBreakdown()
    {
        // Arrange
        var input = new K8sSizingInput
        {
            Distribution = Distribution.OpenShift,
            Technology = Technology.DotNet,
            ClusterMode = ClusterMode.MultiCluster,
            ProdApps = new AppConfig { Medium = 70 },
            NonProdApps = new AppConfig { Medium = 70 },
            EnabledEnvironments = new HashSet<EnvironmentType>
            {
                EnvironmentType.Dev,
                EnvironmentType.Test,
                EnvironmentType.Stage,
                EnvironmentType.Prod,
                EnvironmentType.DR
            },
            EnableHeadroom = true
        };

        // Act
        var result = _service.Calculate(input);

        // Assert total breakdown: 15 masters + 19 infra + 36 workers = 70 nodes
        Assert.Equal(15, result.GrandTotal.TotalMasters);
        Assert.Equal(19, result.GrandTotal.TotalInfra);
        Assert.Equal(36, result.GrandTotal.TotalWorkers);
        Assert.Equal(70, result.GrandTotal.TotalNodes);
    }
}
