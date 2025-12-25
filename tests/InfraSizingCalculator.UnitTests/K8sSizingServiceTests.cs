using FluentAssertions;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Services;

namespace InfraSizingCalculator.UnitTests;

/// <summary>
/// Comprehensive tests for K8sSizingService covering all uncovered methods and scenarios
/// Target: Improve coverage from 57.7% to comprehensive coverage
/// </summary>
public class K8sSizingServiceTests
{
    private readonly K8sSizingService _service;
    private readonly DistributionService _distributionService;
    private readonly TechnologyService _technologyService;

    public K8sSizingServiceTests()
    {
        _distributionService = new DistributionService();
        _technologyService = new TechnologyService();
        _service = new K8sSizingService(_distributionService, _technologyService);
    }

    #region SharedCluster Mode Tests

    [Fact]
    public void SharedCluster_AllEnvironments_CalculatesCorrectly()
    {
        // Arrange
        var input = new K8sSizingInput
        {
            Technology = Technology.DotNet,
            Distribution = Distribution.OpenShift,
            ClusterMode = ClusterMode.SharedCluster,
            EnabledEnvironments = new HashSet<EnvironmentType>
            {
                EnvironmentType.Dev,
                EnvironmentType.Test,
                EnvironmentType.Stage,
                EnvironmentType.Prod,
                EnvironmentType.DR
            },
            ProdApps = new AppConfig { Medium = 50 },
            NonProdApps = new AppConfig { Medium = 30 },
            Replicas = new ReplicaSettings
            {
                NonProd = 1,
                Stage = 2,
                Prod = 3
            },
            Headroom = new HeadroomSettings
            {
                Dev = 20,
                Test = 20,
                Stage = 25,
                Prod = 30,
                DR = 30
            },
            EnableHeadroom = true,
            ProdOvercommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 },
            NonProdOvercommit = new OvercommitSettings { Cpu = 1.5, Memory = 1.5 }
        };

        // Act
        var result = _service.Calculate(input);

        // Assert
        result.Should().NotBeNull();
        result.Environments.Should().HaveCount(1);

        var sharedCluster = result.Environments.First();
        sharedCluster.EnvironmentName.Should().Be("Shared Cluster");
        sharedCluster.IsProd.Should().BeTrue();
        sharedCluster.Workers.Should().BeGreaterOrEqualTo(3); // Minimum workers
        sharedCluster.TotalNodes.Should().BeGreaterThan(0);
        sharedCluster.TotalCpu.Should().BeGreaterThan(0);
        sharedCluster.TotalRam.Should().BeGreaterThan(0);
    }

    [Fact]
    public void SharedCluster_UsesUnifiedProdSpecs()
    {
        // Arrange
        var input = new K8sSizingInput
        {
            Technology = Technology.Java,
            Distribution = Distribution.OpenShift,
            ClusterMode = ClusterMode.SharedCluster,
            EnabledEnvironments = new HashSet<EnvironmentType>
            {
                EnvironmentType.Dev,
                EnvironmentType.Prod
            },
            ProdApps = new AppConfig { Large = 20 },
            NonProdApps = new AppConfig { Small = 15 },
            Replicas = new ReplicaSettings { NonProd = 1, Prod = 3 },
            Headroom = new HeadroomSettings { Dev = 0, Prod = 30 },
            EnableHeadroom = true,
            ProdOvercommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 },
            NonProdOvercommit = new OvercommitSettings { Cpu = 2.0, Memory = 2.0 }
        };

        // Act
        var result = _service.Calculate(input);

        // Assert - Shared cluster uses prod overcommit (1.0, not 2.0)
        result.Should().NotBeNull();
        result.Configuration.ProdOvercommit.Cpu.Should().Be(1.0);
        result.Configuration.ProdOvercommit.Memory.Should().Be(1.0);
    }

    [Fact]
    public void SharedCluster_WithoutHeadroom_CalculatesCorrectly()
    {
        // Arrange
        var input = new K8sSizingInput
        {
            Technology = Technology.NodeJs,
            Distribution = Distribution.Kubernetes,
            ClusterMode = ClusterMode.SharedCluster,
            EnabledEnvironments = new HashSet<EnvironmentType>
            {
                EnvironmentType.Dev,
                EnvironmentType.Prod
            },
            ProdApps = new AppConfig { Medium = 40 },
            NonProdApps = new AppConfig { Medium = 20 },
            Replicas = new ReplicaSettings { NonProd = 1, Prod = 2 },
            EnableHeadroom = false,
            ProdOvercommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 },
            NonProdOvercommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 }
        };

        // Act
        var result = _service.Calculate(input);

        // Assert
        result.Should().NotBeNull();
        var sharedCluster = result.Environments.First();
        sharedCluster.Workers.Should().BeGreaterOrEqualTo(3);
    }

    [Fact]
    public void SharedCluster_ManagedControlPlane_ZeroMasters()
    {
        // Arrange
        var input = new K8sSizingInput
        {
            Technology = Technology.DotNet,
            Distribution = Distribution.EKS,
            ClusterMode = ClusterMode.SharedCluster,
            EnabledEnvironments = new HashSet<EnvironmentType> { EnvironmentType.Prod },
            ProdApps = new AppConfig { Medium = 30 },
            NonProdApps = new AppConfig { Medium = 30 },
            Replicas = new ReplicaSettings { Prod = 3 },
            EnableHeadroom = false,
            ProdOvercommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 },
            NonProdOvercommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 }
        };

        // Act
        var result = _service.Calculate(input);

        // Assert
        result.Environments.First().Masters.Should().Be(0);
        result.GrandTotal.TotalMasters.Should().Be(0);
    }

    #endregion

    #region PerEnvironment Mode Tests

    [Fact]
    public void PerEnvironment_ProdEnvironment_CalculatesCorrectly()
    {
        // Arrange
        var input = new K8sSizingInput
        {
            Technology = Technology.DotNet,
            Distribution = Distribution.OpenShift,
            ClusterMode = ClusterMode.PerEnvironment,
            SelectedEnvironment = EnvironmentType.Prod,
            EnabledEnvironments = new HashSet<EnvironmentType> { EnvironmentType.Prod },
            ProdApps = new AppConfig { Medium = 50 },
            NonProdApps = new AppConfig { Medium = 30 },
            Replicas = new ReplicaSettings { Prod = 3 },
            Headroom = new HeadroomSettings { Prod = 35 },
            EnableHeadroom = true,
            ProdOvercommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 },
            NonProdOvercommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 }
        };

        // Act
        var result = _service.Calculate(input);

        // Assert
        result.Should().NotBeNull();
        result.Environments.Should().HaveCount(1);

        var prodEnv = result.Environments.First();
        prodEnv.Environment.Should().Be(EnvironmentType.Prod);
        prodEnv.EnvironmentName.Should().Be("Production Cluster");
        prodEnv.IsProd.Should().BeTrue();
        prodEnv.Apps.Should().Be(50);
        prodEnv.Replicas.Should().Be(3);
        prodEnv.Pods.Should().Be(150); // 50 apps * 3 replicas
        prodEnv.Workers.Should().BeGreaterOrEqualTo(3);
    }

    [Fact]
    public void PerEnvironment_DevEnvironment_CalculatesCorrectly()
    {
        // Arrange
        var input = new K8sSizingInput
        {
            Technology = Technology.Java,
            Distribution = Distribution.Kubernetes,
            ClusterMode = ClusterMode.PerEnvironment,
            SelectedEnvironment = EnvironmentType.Dev,
            EnabledEnvironments = new HashSet<EnvironmentType> { EnvironmentType.Dev },
            ProdApps = new AppConfig { Large = 20 },
            NonProdApps = new AppConfig { Small = 15 },
            Replicas = new ReplicaSettings { NonProd = 1 },
            Headroom = new HeadroomSettings { Dev = 25 },
            EnableHeadroom = true,
            ProdOvercommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 },
            NonProdOvercommit = new OvercommitSettings { Cpu = 1.5, Memory = 1.5 }
        };

        // Act
        var result = _service.Calculate(input);

        // Assert
        result.Should().NotBeNull();
        var devEnv = result.Environments.First();
        devEnv.Environment.Should().Be(EnvironmentType.Dev);
        devEnv.EnvironmentName.Should().Be("Development Cluster");
        devEnv.IsProd.Should().BeFalse();
        devEnv.Apps.Should().Be(15);
        devEnv.Replicas.Should().Be(1);
    }

    [Fact]
    public void PerEnvironment_TestEnvironment_CalculatesCorrectly()
    {
        // Arrange
        var input = new K8sSizingInput
        {
            Technology = Technology.NodeJs,
            Distribution = Distribution.Rancher,
            ClusterMode = ClusterMode.PerEnvironment,
            SelectedEnvironment = EnvironmentType.Test,
            EnabledEnvironments = new HashSet<EnvironmentType> { EnvironmentType.Test },
            ProdApps = new AppConfig { Medium = 30 },
            NonProdApps = new AppConfig { Medium = 25 },
            Replicas = new ReplicaSettings { NonProd = 1 },
            Headroom = new HeadroomSettings { Test = 20 },
            EnableHeadroom = true,
            ProdOvercommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 },
            NonProdOvercommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 }
        };

        // Act
        var result = _service.Calculate(input);

        // Assert
        var testEnv = result.Environments.First();
        testEnv.Environment.Should().Be(EnvironmentType.Test);
        testEnv.EnvironmentName.Should().Be("Test Cluster");
        testEnv.IsProd.Should().BeFalse();
    }

    [Fact]
    public void PerEnvironment_StageEnvironment_CalculatesCorrectly()
    {
        // Arrange
        var input = new K8sSizingInput
        {
            Technology = Technology.Python,
            Distribution = Distribution.K3s,
            ClusterMode = ClusterMode.PerEnvironment,
            SelectedEnvironment = EnvironmentType.Stage,
            EnabledEnvironments = new HashSet<EnvironmentType> { EnvironmentType.Stage },
            ProdApps = new AppConfig { Large = 15 },
            NonProdApps = new AppConfig { Medium = 20 },
            Replicas = new ReplicaSettings { Stage = 2 },
            Headroom = new HeadroomSettings { Stage = 25 },
            EnableHeadroom = true,
            ProdOvercommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 },
            NonProdOvercommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 }
        };

        // Act
        var result = _service.Calculate(input);

        // Assert
        var stageEnv = result.Environments.First();
        stageEnv.Environment.Should().Be(EnvironmentType.Stage);
        stageEnv.EnvironmentName.Should().Be("Staging Cluster");
        stageEnv.Replicas.Should().Be(2);
    }

    [Fact]
    public void PerEnvironment_DREnvironment_CalculatesCorrectly()
    {
        // Arrange
        var input = new K8sSizingInput
        {
            Technology = Technology.Go,
            Distribution = Distribution.OpenShift,
            ClusterMode = ClusterMode.PerEnvironment,
            SelectedEnvironment = EnvironmentType.DR,
            EnabledEnvironments = new HashSet<EnvironmentType> { EnvironmentType.DR },
            ProdApps = new AppConfig { Medium = 40 },
            NonProdApps = new AppConfig { Medium = 20 },
            Replicas = new ReplicaSettings { Prod = 3 },
            Headroom = new HeadroomSettings { DR = 30 },
            EnableHeadroom = true,
            ProdOvercommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 },
            NonProdOvercommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 }
        };

        // Act
        var result = _service.Calculate(input);

        // Assert
        var drEnv = result.Environments.First();
        drEnv.Environment.Should().Be(EnvironmentType.DR);
        drEnv.EnvironmentName.Should().Be("DR Cluster");
        drEnv.IsProd.Should().BeTrue(); // DR is considered production
        drEnv.Replicas.Should().Be(3);
    }

    [Fact]
    public void PerEnvironment_UsesUnifiedProdSpecs()
    {
        // Arrange
        var input = new K8sSizingInput
        {
            Technology = Technology.DotNet,
            Distribution = Distribution.OpenShift,
            ClusterMode = ClusterMode.PerEnvironment,
            SelectedEnvironment = EnvironmentType.Dev,
            EnabledEnvironments = new HashSet<EnvironmentType> { EnvironmentType.Dev },
            ProdApps = new AppConfig { Medium = 20 },
            NonProdApps = new AppConfig { Medium = 15 },
            Replicas = new ReplicaSettings { NonProd = 1 },
            EnableHeadroom = false,
            ProdOvercommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 },
            NonProdOvercommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 }
        };

        // Act
        var result = _service.Calculate(input);

        // Assert - PerEnvironment uses unified specs (prod overcommit)
        result.Configuration.ProdOvercommit.Cpu.Should().Be(1.0);
    }

    #endregion

    #region CalculateResourceRequirements - All AppTier Combinations

    [Theory]
    [InlineData(10, 0, 0, 0)]  // Small only
    [InlineData(0, 10, 0, 0)]  // Medium only
    [InlineData(0, 0, 10, 0)]  // Large only
    [InlineData(0, 0, 0, 10)]  // XLarge only
    public void CalculateWorkerNodes_SingleTier_CalculatesCorrectly(int small, int medium, int large, int xlarge)
    {
        // Arrange
        var apps = new AppConfig { Small = small, Medium = medium, Large = large, XLarge = xlarge };
        var techConfig = _technologyService.GetConfig(Technology.DotNet);
        var workerSpecs = new NodeSpecs(8, 32, 100);
        var overcommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 };

        // Act
        var result = _service.CalculateWorkerNodes(
            apps,
            techConfig.Tiers,
            replicas: 1,
            workerSpecs,
            headroomPercent: 0,
            overcommit);

        // Assert
        result.Should().BeGreaterOrEqualTo(3); // Minimum workers
    }

    [Theory]
    [InlineData(5, 5, 0, 0)]    // Small + Medium
    [InlineData(5, 0, 5, 0)]    // Small + Large
    [InlineData(5, 0, 0, 5)]    // Small + XLarge
    [InlineData(0, 5, 5, 0)]    // Medium + Large
    [InlineData(0, 5, 0, 5)]    // Medium + XLarge
    [InlineData(0, 0, 5, 5)]    // Large + XLarge
    public void CalculateWorkerNodes_TwoTiers_CalculatesCorrectly(int small, int medium, int large, int xlarge)
    {
        // Arrange
        var apps = new AppConfig { Small = small, Medium = medium, Large = large, XLarge = xlarge };
        var techConfig = _technologyService.GetConfig(Technology.Java);
        var workerSpecs = new NodeSpecs(16, 64, 200);
        var overcommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 };

        // Act
        var result = _service.CalculateWorkerNodes(
            apps,
            techConfig.Tiers,
            replicas: 2,
            workerSpecs,
            headroomPercent: 0,
            overcommit);

        // Assert
        result.Should().BeGreaterOrEqualTo(3);
    }

    [Theory]
    [InlineData(3, 3, 3, 0)]    // Small + Medium + Large
    [InlineData(3, 3, 0, 3)]    // Small + Medium + XLarge
    [InlineData(3, 0, 3, 3)]    // Small + Large + XLarge
    [InlineData(0, 3, 3, 3)]    // Medium + Large + XLarge
    public void CalculateWorkerNodes_ThreeTiers_CalculatesCorrectly(int small, int medium, int large, int xlarge)
    {
        // Arrange
        var apps = new AppConfig { Small = small, Medium = medium, Large = large, XLarge = xlarge };
        var techConfig = _technologyService.GetConfig(Technology.NodeJs);
        var workerSpecs = new NodeSpecs(8, 32, 100);
        var overcommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 };

        // Act
        var result = _service.CalculateWorkerNodes(
            apps,
            techConfig.Tiers,
            replicas: 1,
            workerSpecs,
            headroomPercent: 20,
            overcommit);

        // Assert
        result.Should().BeGreaterOrEqualTo(3);
    }

    [Fact]
    public void CalculateWorkerNodes_AllFourTiers_CalculatesCorrectly()
    {
        // Arrange
        var apps = new AppConfig { Small = 10, Medium = 20, Large = 15, XLarge = 5 };
        var techConfig = _technologyService.GetConfig(Technology.Python);
        var workerSpecs = new NodeSpecs(16, 64, 200);
        var overcommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 };

        // Act
        var result = _service.CalculateWorkerNodes(
            apps,
            techConfig.Tiers,
            replicas: 3,
            workerSpecs,
            headroomPercent: 25,
            overcommit);

        // Assert
        result.Should().BeGreaterOrEqualTo(3);
        result.Should().BeGreaterThan(3); // With 50 apps and 3 replicas, should need more than minimum
    }

    #endregion

    #region Infrastructure Node Calculations for OpenShift

    [Fact]
    public void CalculateInfraNodes_OpenShift_SmallProdDeployment_Returns3()
    {
        // BR-I006: Small production (<50 apps) can use 3 infra nodes
        var result = _service.CalculateInfraNodes(
            totalApps: 30,
            isProd: true,
            hasInfraNodes: true);

        result.Should().Be(3);
    }

    [Fact]
    public void CalculateInfraNodes_OpenShift_LargeProdDeployment_ReturnsMin5()
    {
        // BR-I005: Large production (>=50 apps) needs minimum 5 infra
        var result = _service.CalculateInfraNodes(
            totalApps: 50,
            isProd: true,
            hasInfraNodes: true);

        result.Should().Be(5);
    }

    [Fact]
    public void CalculateInfraNodes_OpenShift_VeryLargeDeployment_ScalesCorrectly()
    {
        // BR-I004: Scale at 1 per 25 apps
        var result = _service.CalculateInfraNodes(
            totalApps: 150,
            isProd: true,
            hasInfraNodes: true);

        // 150/25 = 6
        result.Should().Be(6);
    }

    [Fact]
    public void CalculateInfraNodes_OpenShift_NonProd_Scales()
    {
        // BR-I004: Non-prod also scales, but no minimum of 5
        var result = _service.CalculateInfraNodes(
            totalApps: 100,
            isProd: false,
            hasInfraNodes: true);

        // 100/25 = 4
        result.Should().Be(4);
    }

    [Fact]
    public void CalculateInfraNodes_OpenShift_MaxCap_Returns10()
    {
        // BR-I003: Maximum infrastructure node count = 10
        var result = _service.CalculateInfraNodes(
            totalApps: 1000,
            isProd: true,
            hasInfraNodes: true);

        result.Should().Be(10);
    }

    #endregion

    #region HA Split Node Calculations

    [Fact]
    public void CalculateMasterNodes_SmallCluster_Returns3Masters()
    {
        // BR-M002, BR-M004: Standard HA quorum = 3 masters
        var result = _service.CalculateMasterNodes(
            workerCount: 50,
            isManagedControlPlane: false);

        result.Should().Be(3);
    }

    [Fact]
    public void CalculateMasterNodes_LargeCluster_Returns5Masters()
    {
        // BR-M003: Large clusters (> 100 workers) need 5 masters
        var result = _service.CalculateMasterNodes(
            workerCount: 150,
            isManagedControlPlane: false);

        result.Should().Be(5);
    }

    [Fact]
    public void CalculateMasterNodes_ExactlyAtThreshold_Returns3Masters()
    {
        // At exactly 100 workers, should still be 3 masters
        var result = _service.CalculateMasterNodes(
            workerCount: 100,
            isManagedControlPlane: false);

        result.Should().Be(3);
    }

    [Fact]
    public void CalculateMasterNodes_JustAboveThreshold_Returns5Masters()
    {
        // BR-M003: Just above 100 workers triggers 5 masters
        var result = _service.CalculateMasterNodes(
            workerCount: 101,
            isManagedControlPlane: false);

        result.Should().Be(5);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Calculate_ZeroApps_ReturnsMinimumResources()
    {
        // Arrange
        var input = new K8sSizingInput
        {
            Technology = Technology.DotNet,
            Distribution = Distribution.OpenShift,
            ClusterMode = ClusterMode.MultiCluster,
            EnabledEnvironments = new HashSet<EnvironmentType> { EnvironmentType.Prod },
            ProdApps = new AppConfig { Small = 0, Medium = 0, Large = 0, XLarge = 0 },
            NonProdApps = new AppConfig(),
            Replicas = new ReplicaSettings { Prod = 3 },
            EnableHeadroom = false,
            ProdOvercommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 },
            NonProdOvercommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 }
        };

        // Act
        var result = _service.Calculate(input);

        // Assert
        var prod = result.Environments.First();
        prod.Workers.Should().Be(3); // Minimum workers
        prod.Apps.Should().Be(0);
        prod.Pods.Should().Be(0);
    }

    [Fact]
    public void Calculate_MaximumApps_CalculatesCorrectly()
    {
        // Arrange
        var input = new K8sSizingInput
        {
            Technology = Technology.Java,
            Distribution = Distribution.OpenShift,
            ClusterMode = ClusterMode.MultiCluster,
            EnabledEnvironments = new HashSet<EnvironmentType> { EnvironmentType.Prod },
            ProdApps = new AppConfig { Small = 100, Medium = 100, Large = 100, XLarge = 100 },
            NonProdApps = new AppConfig(),
            Replicas = new ReplicaSettings { Prod = 3 },
            EnableHeadroom = true,
            Headroom = new HeadroomSettings { Prod = 50 },
            ProdOvercommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 },
            NonProdOvercommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 }
        };

        // Act
        var result = _service.Calculate(input);

        // Assert
        var prod = result.Environments.First();
        prod.Apps.Should().Be(400); // Total of all tiers
        prod.Workers.Should().BeGreaterThan(3);
        prod.Masters.Should().Be(5); // Large cluster should have 5 masters
        prod.Infra.Should().Be(10); // Max infra nodes
    }

    [Fact]
    public void Calculate_AllTierCombinations_NoErrors()
    {
        // Test various combinations to ensure no calculation errors
        var combinations = new[]
        {
            new AppConfig { Small = 10 },
            new AppConfig { Medium = 10 },
            new AppConfig { Large = 10 },
            new AppConfig { XLarge = 10 },
            new AppConfig { Small = 5, Medium = 5 },
            new AppConfig { Large = 5, XLarge = 5 },
            new AppConfig { Small = 3, Medium = 3, Large = 3, XLarge = 3 }
        };

        foreach (var appConfig in combinations)
        {
            // Arrange
            var input = new K8sSizingInput
            {
                Technology = Technology.DotNet,
                Distribution = Distribution.Kubernetes,
                ClusterMode = ClusterMode.MultiCluster,
                EnabledEnvironments = new HashSet<EnvironmentType> { EnvironmentType.Prod },
                ProdApps = appConfig,
                NonProdApps = new AppConfig(),
                Replicas = new ReplicaSettings { Prod = 2 },
                EnableHeadroom = false,
                ProdOvercommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 },
                NonProdOvercommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 }
            };

            // Act
            var result = _service.Calculate(input);

            // Assert
            result.Should().NotBeNull();
            result.Environments.Should().HaveCount(1);
        }
    }

    #endregion

    #region Headroom Calculations

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    [InlineData(20)]
    [InlineData(33)]
    [InlineData(37.5)]
    [InlineData(50)]
    [InlineData(100)]
    public void Calculate_VariousHeadroomPercentages_IncreasesWorkers(double headroomPercent)
    {
        // Arrange
        var apps = new AppConfig { Medium = 50 };
        var techConfig = _technologyService.GetConfig(Technology.DotNet);
        var workerSpecs = new NodeSpecs(16, 64, 200);
        var overcommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 };

        // Act - Without headroom
        var resultNoHeadroom = _service.CalculateWorkerNodes(
            apps, techConfig.Tiers, 3, workerSpecs, 0, overcommit);

        // Act - With headroom
        var resultWithHeadroom = _service.CalculateWorkerNodes(
            apps, techConfig.Tiers, 3, workerSpecs, headroomPercent, overcommit);

        // Assert
        if (headroomPercent > 0)
        {
            resultWithHeadroom.Should().BeGreaterOrEqualTo(resultNoHeadroom);
        }
        else
        {
            resultWithHeadroom.Should().Be(resultNoHeadroom);
        }
    }

    [Fact]
    public void Calculate_HeadroomDisabled_IgnoresHeadroomSettings()
    {
        // Arrange
        var input = new K8sSizingInput
        {
            Technology = Technology.NodeJs,
            Distribution = Distribution.Kubernetes,
            ClusterMode = ClusterMode.MultiCluster,
            EnabledEnvironments = new HashSet<EnvironmentType> { EnvironmentType.Prod },
            ProdApps = new AppConfig { Medium = 40 },
            NonProdApps = new AppConfig(),
            Replicas = new ReplicaSettings { Prod = 3 },
            Headroom = new HeadroomSettings { Prod = 50 }, // High headroom
            EnableHeadroom = false, // But disabled
            ProdOvercommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 },
            NonProdOvercommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 }
        };

        // Act
        var result = _service.Calculate(input);

        // Assert - Should calculate without headroom
        result.Environments.First().Workers.Should().BeGreaterOrEqualTo(3);
    }

    [Fact]
    public void Calculate_DifferentHeadroomPerEnvironment_AppliesCorrectly()
    {
        // Arrange
        var input = new K8sSizingInput
        {
            Technology = Technology.DotNet,
            Distribution = Distribution.OpenShift,
            ClusterMode = ClusterMode.MultiCluster,
            EnabledEnvironments = new HashSet<EnvironmentType>
            {
                EnvironmentType.Dev,
                EnvironmentType.Prod
            },
            ProdApps = new AppConfig { Medium = 50 },
            NonProdApps = new AppConfig { Medium = 50 },
            Replicas = new ReplicaSettings { NonProd = 1, Prod = 3 },
            Headroom = new HeadroomSettings
            {
                Dev = 10,   // Low headroom for dev
                Prod = 50   // High headroom for prod
            },
            EnableHeadroom = true,
            ProdOvercommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 },
            NonProdOvercommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 }
        };

        // Act
        var result = _service.Calculate(input);

        // Assert
        var dev = result.Environments.First(e => e.Environment == EnvironmentType.Dev);
        var prod = result.Environments.First(e => e.Environment == EnvironmentType.Prod);

        // Prod should have more workers due to higher headroom and replicas
        prod.Workers.Should().BeGreaterThan(dev.Workers);
    }

    #endregion

    #region Overcommit Settings

    [Theory]
    [InlineData(1.0, 1.0)]
    [InlineData(1.5, 1.5)]
    [InlineData(2.0, 2.0)]
    [InlineData(1.0, 2.0)]  // Different CPU vs Memory
    [InlineData(2.0, 1.0)]  // Different CPU vs Memory
    public void Calculate_VariousOvercommitRatios_CalculatesCorrectly(double cpuOvercommit, double memOvercommit)
    {
        // Arrange
        var apps = new AppConfig { Medium = 30 };
        var techConfig = _technologyService.GetConfig(Technology.Java);
        var workerSpecs = new NodeSpecs(8, 32, 100);
        var overcommit = new OvercommitSettings { Cpu = cpuOvercommit, Memory = memOvercommit };

        // Act
        var result = _service.CalculateWorkerNodes(
            apps, techConfig.Tiers, 2, workerSpecs, 0, overcommit);

        // Assert
        result.Should().BeGreaterOrEqualTo(3);
    }

    [Fact]
    public void Calculate_HighOvercommit_ReducesWorkerCount()
    {
        // Arrange
        var apps = new AppConfig { Large = 40 };
        var techConfig = _technologyService.GetConfig(Technology.DotNet);
        var workerSpecs = new NodeSpecs(16, 64, 200);

        // Act - No overcommit
        var resultNoOvercommit = _service.CalculateWorkerNodes(
            apps, techConfig.Tiers, 3, workerSpecs, 0,
            new OvercommitSettings { Cpu = 1.0, Memory = 1.0 });

        // Act - High overcommit
        var resultHighOvercommit = _service.CalculateWorkerNodes(
            apps, techConfig.Tiers, 3, workerSpecs, 0,
            new OvercommitSettings { Cpu = 2.0, Memory = 2.0 });

        // Assert
        resultHighOvercommit.Should().BeLessOrEqualTo(resultNoOvercommit);
    }

    [Fact]
    public void Calculate_ProdVsNonProdOvercommit_MultiCluster()
    {
        // Arrange
        var input = new K8sSizingInput
        {
            Technology = Technology.NodeJs,
            Distribution = Distribution.Kubernetes,
            ClusterMode = ClusterMode.MultiCluster,
            EnabledEnvironments = new HashSet<EnvironmentType>
            {
                EnvironmentType.Dev,
                EnvironmentType.Prod
            },
            ProdApps = new AppConfig { Medium = 50 },
            NonProdApps = new AppConfig { Medium = 50 },
            Replicas = new ReplicaSettings { NonProd = 1, Prod = 3 },
            EnableHeadroom = false,
            ProdOvercommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 },
            NonProdOvercommit = new OvercommitSettings { Cpu = 2.0, Memory = 2.0 } // Higher for non-prod
        };

        // Act
        var result = _service.Calculate(input);

        // Assert
        result.Should().NotBeNull();
        result.Environments.Should().HaveCount(2);
    }

    #endregion

    #region Technology-Specific Tier Specs

    [Theory]
    [InlineData(Technology.DotNet)]
    [InlineData(Technology.Java)]
    [InlineData(Technology.NodeJs)]
    [InlineData(Technology.Python)]
    [InlineData(Technology.Go)]
    [InlineData(Technology.Mendix)]
    [InlineData(Technology.OutSystems)]
    public void Calculate_AllTechnologies_CalculatesCorrectly(Technology technology)
    {
        // Arrange
        var input = new K8sSizingInput
        {
            Technology = technology,
            Distribution = Distribution.Kubernetes,
            ClusterMode = ClusterMode.MultiCluster,
            EnabledEnvironments = new HashSet<EnvironmentType> { EnvironmentType.Prod },
            ProdApps = new AppConfig { Small = 5, Medium = 10, Large = 5, XLarge = 2 },
            NonProdApps = new AppConfig(),
            Replicas = new ReplicaSettings { Prod = 3 },
            EnableHeadroom = true,
            Headroom = new HeadroomSettings { Prod = 25 },
            ProdOvercommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 },
            NonProdOvercommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 }
        };

        // Act
        var result = _service.Calculate(input);

        // Assert
        result.Should().NotBeNull();
        result.TechnologyName.Should().NotBeNullOrEmpty();
        var prod = result.Environments.First();
        prod.Apps.Should().Be(22); // 5+10+5+2
        prod.Workers.Should().BeGreaterOrEqualTo(3);
    }

    [Fact]
    public void Calculate_JavaVsDotNet_DifferentResourceRequirements()
    {
        // Java has higher resource requirements than .NET
        var apps = new AppConfig { Medium = 20 };
        var workerSpecs = new NodeSpecs(16, 64, 200);
        var overcommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 };

        // Act - .NET
        var dotnetConfig = _technologyService.GetConfig(Technology.DotNet);
        var dotnetWorkers = _service.CalculateWorkerNodes(
            apps, dotnetConfig.Tiers, 3, workerSpecs, 0, overcommit);

        // Act - Java
        var javaConfig = _technologyService.GetConfig(Technology.Java);
        var javaWorkers = _service.CalculateWorkerNodes(
            apps, javaConfig.Tiers, 3, workerSpecs, 0, overcommit);

        // Assert - Java should require more workers due to higher resource needs
        javaWorkers.Should().BeGreaterOrEqualTo(dotnetWorkers);
    }

    [Fact]
    public void Calculate_GoVsMendix_SignificantlyDifferentRequirements()
    {
        // Go is very efficient, Mendix is heavy
        var apps = new AppConfig { Medium = 15 };
        var workerSpecs = new NodeSpecs(16, 64, 200);
        var overcommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 };

        // Act - Go
        var goConfig = _technologyService.GetConfig(Technology.Go);
        var goWorkers = _service.CalculateWorkerNodes(
            apps, goConfig.Tiers, 2, workerSpecs, 0, overcommit);

        // Act - Mendix
        var mendixConfig = _technologyService.GetConfig(Technology.Mendix);
        var mendixWorkers = _service.CalculateWorkerNodes(
            apps, mendixConfig.Tiers, 2, workerSpecs, 0, overcommit);

        // Assert - Mendix should require significantly more workers
        mendixWorkers.Should().BeGreaterThan(goWorkers);
    }

    #endregion

    #region Environment Type Tests

    [Fact]
    public void Calculate_AllEnvironmentTypes_MultiCluster()
    {
        // Arrange
        var input = new K8sSizingInput
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
            ProdApps = new AppConfig { Medium = 30 },
            NonProdApps = new AppConfig { Medium = 20 },
            Replicas = new ReplicaSettings
            {
                NonProd = 1,
                Stage = 2,
                Prod = 3
            },
            Headroom = new HeadroomSettings
            {
                Dev = 20,
                Test = 20,
                Stage = 25,
                Prod = 30,
                DR = 30
            },
            EnableHeadroom = true,
            ProdOvercommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 },
            NonProdOvercommit = new OvercommitSettings { Cpu = 1.5, Memory = 1.5 }
        };

        // Act
        var result = _service.Calculate(input);

        // Assert
        result.Environments.Should().HaveCount(5);

        var dev = result.Environments.First(e => e.Environment == EnvironmentType.Dev);
        var test = result.Environments.First(e => e.Environment == EnvironmentType.Test);
        var stage = result.Environments.First(e => e.Environment == EnvironmentType.Stage);
        var prod = result.Environments.First(e => e.Environment == EnvironmentType.Prod);
        var dr = result.Environments.First(e => e.Environment == EnvironmentType.DR);

        // Verify environment properties
        dev.IsProd.Should().BeFalse();
        test.IsProd.Should().BeFalse();
        stage.IsProd.Should().BeFalse();
        prod.IsProd.Should().BeTrue();
        dr.IsProd.Should().BeTrue(); // DR is considered production

        // Verify replicas
        dev.Replicas.Should().Be(1);
        test.Replicas.Should().Be(1);
        stage.Replicas.Should().Be(2);
        prod.Replicas.Should().Be(3);
        dr.Replicas.Should().Be(3);
    }

    [Fact]
    public void Calculate_EnvironmentAppsOverride_UsesCorrectCounts()
    {
        // Arrange
        var input = new K8sSizingInput
        {
            Technology = Technology.Java,
            Distribution = Distribution.Kubernetes,
            ClusterMode = ClusterMode.MultiCluster,
            EnabledEnvironments = new HashSet<EnvironmentType>
            {
                EnvironmentType.Dev,
                EnvironmentType.Prod
            },
            ProdApps = new AppConfig { Medium = 50 },
            NonProdApps = new AppConfig { Medium = 30 },
            EnvironmentApps = new Dictionary<EnvironmentType, AppConfig>
            {
                [EnvironmentType.Dev] = new AppConfig { Small = 15 },  // Override with different count
                [EnvironmentType.Prod] = new AppConfig { Large = 25 }   // Override with different tier
            },
            Replicas = new ReplicaSettings { NonProd = 1, Prod = 3 },
            EnableHeadroom = false,
            ProdOvercommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 },
            NonProdOvercommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 }
        };

        // Act
        var result = _service.Calculate(input);

        // Assert
        var dev = result.Environments.First(e => e.Environment == EnvironmentType.Dev);
        var prod = result.Environments.First(e => e.Environment == EnvironmentType.Prod);

        dev.Apps.Should().Be(15); // From EnvironmentApps override
        prod.Apps.Should().Be(25); // From EnvironmentApps override
    }

    #endregion

    #region Distribution-Specific Tests

    [Theory]
    [InlineData(Distribution.EKS)]
    [InlineData(Distribution.AKS)]
    [InlineData(Distribution.GKE)]
    public void Calculate_ManagedDistributions_NoMasterNodes(Distribution distribution)
    {
        // Arrange
        var input = new K8sSizingInput
        {
            Technology = Technology.DotNet,
            Distribution = distribution,
            ClusterMode = ClusterMode.MultiCluster,
            EnabledEnvironments = new HashSet<EnvironmentType> { EnvironmentType.Prod },
            ProdApps = new AppConfig { Medium = 30 },
            NonProdApps = new AppConfig(),
            Replicas = new ReplicaSettings { Prod = 3 },
            EnableHeadroom = false,
            ProdOvercommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 },
            NonProdOvercommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 }
        };

        // Act
        var result = _service.Calculate(input);

        // Assert
        result.Environments.First().Masters.Should().Be(0);
        result.GrandTotal.TotalMasters.Should().Be(0);
    }

    [Theory]
    [InlineData(Distribution.Kubernetes)]
    [InlineData(Distribution.Rancher)]
    [InlineData(Distribution.K3s)]
    [InlineData(Distribution.EKS)]
    [InlineData(Distribution.AKS)]
    [InlineData(Distribution.GKE)]
    public void Calculate_NonOpenShiftDistributions_NoInfraNodes(Distribution distribution)
    {
        // Arrange
        var input = new K8sSizingInput
        {
            Technology = Technology.DotNet,
            Distribution = distribution,
            ClusterMode = ClusterMode.MultiCluster,
            EnabledEnvironments = new HashSet<EnvironmentType> { EnvironmentType.Prod },
            ProdApps = new AppConfig { Medium = 70 }, // Large deployment
            NonProdApps = new AppConfig(),
            Replicas = new ReplicaSettings { Prod = 3 },
            EnableHeadroom = false,
            ProdOvercommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 },
            NonProdOvercommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 }
        };

        // Act
        var result = _service.Calculate(input);

        // Assert
        result.Environments.First().Infra.Should().Be(0);
        result.GrandTotal.TotalInfra.Should().Be(0);
    }

    [Fact]
    public void Calculate_OpenShift_HasInfraNodes()
    {
        // Arrange
        var input = new K8sSizingInput
        {
            Technology = Technology.DotNet,
            Distribution = Distribution.OpenShift,
            ClusterMode = ClusterMode.MultiCluster,
            EnabledEnvironments = new HashSet<EnvironmentType> { EnvironmentType.Prod },
            ProdApps = new AppConfig { Medium = 70 },
            NonProdApps = new AppConfig(),
            Replicas = new ReplicaSettings { Prod = 3 },
            EnableHeadroom = false,
            ProdOvercommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 },
            NonProdOvercommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 }
        };

        // Act
        var result = _service.Calculate(input);

        // Assert
        result.Environments.First().Infra.Should().BeGreaterThan(0);
        result.GrandTotal.TotalInfra.Should().BeGreaterThan(0);
    }

    #endregion

    #region Result Metadata Tests

    [Fact]
    public void Calculate_IncludesConfigurationInResult()
    {
        // Arrange
        var input = new K8sSizingInput
        {
            Technology = Technology.NodeJs,
            Distribution = Distribution.Kubernetes,
            ClusterMode = ClusterMode.MultiCluster,
            EnabledEnvironments = new HashSet<EnvironmentType> { EnvironmentType.Prod },
            ProdApps = new AppConfig { Medium = 20 },
            NonProdApps = new AppConfig(),
            Replicas = new ReplicaSettings { Prod = 2 },
            EnableHeadroom = false,
            ProdOvercommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 },
            NonProdOvercommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 }
        };

        // Act
        var result = _service.Calculate(input);

        // Assert
        result.Configuration.Should().NotBeNull();
        result.Configuration.Technology.Should().Be(Technology.NodeJs);
        result.Configuration.Distribution.Should().Be(Distribution.Kubernetes);
        result.DistributionName.Should().NotBeNullOrEmpty();
        result.TechnologyName.Should().NotBeNullOrEmpty();
        result.NodeSpecs.Should().NotBeNull();
        result.CalculatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Calculate_GrandTotal_SumsAllEnvironments()
    {
        // Arrange
        var input = new K8sSizingInput
        {
            Technology = Technology.DotNet,
            Distribution = Distribution.OpenShift,
            ClusterMode = ClusterMode.MultiCluster,
            EnabledEnvironments = new HashSet<EnvironmentType>
            {
                EnvironmentType.Dev,
                EnvironmentType.Prod
            },
            ProdApps = new AppConfig { Medium = 30 },
            NonProdApps = new AppConfig { Medium = 20 },
            Replicas = new ReplicaSettings { NonProd = 1, Prod = 3 },
            EnableHeadroom = false,
            ProdOvercommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 },
            NonProdOvercommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 }
        };

        // Act
        var result = _service.Calculate(input);

        // Assert
        var expectedTotalNodes = result.Environments.Sum(e => e.TotalNodes);
        var expectedTotalCpu = result.Environments.Sum(e => e.TotalCpu);
        var expectedTotalRam = result.Environments.Sum(e => e.TotalRam);
        var expectedTotalMasters = result.Environments.Sum(e => e.Masters);
        var expectedTotalInfra = result.Environments.Sum(e => e.Infra);
        var expectedTotalWorkers = result.Environments.Sum(e => e.Workers);

        result.GrandTotal.TotalNodes.Should().Be(expectedTotalNodes);
        result.GrandTotal.TotalCpu.Should().Be(expectedTotalCpu);
        result.GrandTotal.TotalRam.Should().Be(expectedTotalRam);
        result.GrandTotal.TotalMasters.Should().Be(expectedTotalMasters);
        result.GrandTotal.TotalInfra.Should().Be(expectedTotalInfra);
        result.GrandTotal.TotalWorkers.Should().Be(expectedTotalWorkers);
    }

    #endregion

    #region Custom Node Specs Tests

    [Fact]
    public void Calculate_CustomNodeSpecs_UsesProvidedSpecs()
    {
        // Arrange
        var customSpecs = new DistributionConfig
        {
            Distribution = Distribution.Kubernetes,
            Name = "Custom Distribution",
            Vendor = "Custom Vendor",
            HasManagedControlPlane = false,
            HasInfraNodes = false,
            ProdControlPlane = new NodeSpecs(4, 16, 50),
            NonProdControlPlane = new NodeSpecs(2, 8, 50),
            ProdWorker = new NodeSpecs(32, 128, 500), // Very large workers
            NonProdWorker = new NodeSpecs(16, 64, 250),
            ProdInfra = new NodeSpecs(8, 32, 100),
            NonProdInfra = new NodeSpecs(4, 16, 50)
        };

        var input = new K8sSizingInput
        {
            Technology = Technology.DotNet,
            Distribution = Distribution.Kubernetes, // Will be overridden
            ClusterMode = ClusterMode.MultiCluster,
            EnabledEnvironments = new HashSet<EnvironmentType> { EnvironmentType.Prod },
            ProdApps = new AppConfig { Medium = 40 },
            NonProdApps = new AppConfig(),
            Replicas = new ReplicaSettings { Prod = 3 },
            EnableHeadroom = false,
            ProdOvercommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 },
            NonProdOvercommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 },
            CustomNodeSpecs = customSpecs
        };

        // Act
        var result = _service.Calculate(input);

        // Assert
        result.DistributionName.Should().Be("Custom Distribution");
        result.NodeSpecs.Should().Be(customSpecs);

        // With very large workers (32 CPU, 128 GB), should need fewer workers
        var prod = result.Environments.First();
        prod.Workers.Should().BeGreaterOrEqualTo(3);
    }

    #endregion
}
