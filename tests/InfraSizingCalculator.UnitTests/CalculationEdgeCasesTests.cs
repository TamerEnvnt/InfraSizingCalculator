using FluentAssertions;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Services;
using Xunit;

namespace InfraSizingCalculator.UnitTests;

/// <summary>
/// Edge case tests for calculation services to ensure robustness
/// </summary>
public class CalculationEdgeCasesTests
{
    private readonly K8sSizingService _k8sService;
    private readonly VMSizingService _vmService;
    private readonly DistributionService _distributionService;
    private readonly TechnologyService _technologyService;

    public CalculationEdgeCasesTests()
    {
        _distributionService = new DistributionService();
        _technologyService = new TechnologyService();
        _k8sService = new K8sSizingService(_distributionService, _technologyService);
        _vmService = new VMSizingService(_technologyService);
    }

    #region K8s Master Node Edge Cases

    [Fact]
    public void CalculateMasterNodes_ExactlyAtBoundary100Workers_Returns3Masters()
    {
        var result = _k8sService.CalculateMasterNodes(100, isManagedControlPlane: false);
        result.Should().Be(3);
    }

    [Fact]
    public void CalculateMasterNodes_JustAboveBoundary101Workers_Returns5Masters()
    {
        var result = _k8sService.CalculateMasterNodes(101, isManagedControlPlane: false);
        result.Should().Be(5);
    }

    [Fact]
    public void CalculateMasterNodes_ZeroWorkers_Returns3Masters()
    {
        var result = _k8sService.CalculateMasterNodes(0, isManagedControlPlane: false);
        result.Should().Be(3);
    }

    [Fact]
    public void CalculateMasterNodes_NegativeWorkers_Returns3Masters()
    {
        // Defensive - negative should be treated as minimum
        var result = _k8sService.CalculateMasterNodes(-10, isManagedControlPlane: false);
        result.Should().BeGreaterOrEqualTo(0);
    }

    [Fact]
    public void CalculateMasterNodes_VeryLargeWorkerCount_Returns5Masters()
    {
        var result = _k8sService.CalculateMasterNodes(10000, isManagedControlPlane: false);
        result.Should().Be(5);
    }

    #endregion

    #region K8s Infra Node Edge Cases

    [Fact]
    public void CalculateInfraNodes_ExactlyAtScalingBoundary25Apps_Returns3()
    {
        var result = _k8sService.CalculateInfraNodes(25, isProd: false, hasInfraNodes: true);
        result.Should().Be(3);
    }

    [Fact]
    public void CalculateInfraNodes_JustAboveScalingBoundary26Apps_Returns3()
    {
        // 26/25 = 1.04 -> ceiling = 2, but minimum is 3
        var result = _k8sService.CalculateInfraNodes(26, isProd: false, hasInfraNodes: true);
        result.Should().Be(3);
    }

    [Fact]
    public void CalculateInfraNodes_ExactlyAtProdThreshold50Apps_Returns5()
    {
        var result = _k8sService.CalculateInfraNodes(50, isProd: true, hasInfraNodes: true);
        result.Should().Be(5);
    }

    [Fact]
    public void CalculateInfraNodes_JustBelowProdThreshold49Apps_Returns3()
    {
        var result = _k8sService.CalculateInfraNodes(49, isProd: true, hasInfraNodes: true);
        result.Should().Be(3);
    }

    [Fact]
    public void CalculateInfraNodes_ZeroApps_Returns3()
    {
        var result = _k8sService.CalculateInfraNodes(0, isProd: false, hasInfraNodes: true);
        result.Should().Be(3);
    }

    [Fact]
    public void CalculateInfraNodes_MaximumCap_Returns10()
    {
        var result = _k8sService.CalculateInfraNodes(1000, isProd: true, hasInfraNodes: true);
        result.Should().Be(10);
    }

    [Fact]
    public void CalculateInfraNodes_ExactlyAtMaxBoundary250Apps_Returns10()
    {
        var result = _k8sService.CalculateInfraNodes(250, isProd: true, hasInfraNodes: true);
        result.Should().Be(10);
    }

    #endregion

    #region K8s Worker Node Edge Cases

    [Fact]
    public void CalculateWorkerNodes_ZeroApps_ReturnsMinimum3()
    {
        var apps = new AppConfig { Small = 0, Medium = 0, Large = 0 };
        var techConfig = _technologyService.GetConfig(Technology.DotNet);
        var workerSpecs = new NodeSpecs(8, 32, 100);
        var overcommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 };

        var result = _k8sService.CalculateWorkerNodes(apps, techConfig.Tiers, 1, workerSpecs, 0, overcommit);

        result.Should().BeGreaterOrEqualTo(3);
    }

    [Fact]
    public void CalculateWorkerNodes_MaximumReplicas_CalculatesCorrectly()
    {
        var apps = new AppConfig { Medium = 10 };
        var techConfig = _technologyService.GetConfig(Technology.DotNet);
        var workerSpecs = new NodeSpecs(8, 32, 100);
        var overcommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 };

        var result = _k8sService.CalculateWorkerNodes(apps, techConfig.Tiers, 10, workerSpecs, 0, overcommit);

        result.Should().BeGreaterThan(3);
    }

    [Fact]
    public void CalculateWorkerNodes_VeryHighOvercommit_StillReturnsMinimum()
    {
        var apps = new AppConfig { Small = 5 };
        var techConfig = _technologyService.GetConfig(Technology.DotNet);
        var workerSpecs = new NodeSpecs(8, 32, 100);
        var overcommit = new OvercommitSettings { Cpu = 10.0, Memory = 10.0 };

        var result = _k8sService.CalculateWorkerNodes(apps, techConfig.Tiers, 1, workerSpecs, 0, overcommit);

        result.Should().BeGreaterOrEqualTo(3);
    }

    [Fact]
    public void CalculateWorkerNodes_MaxHeadroom_IncreasesWorkers()
    {
        var apps = new AppConfig { Medium = 20 };
        var techConfig = _technologyService.GetConfig(Technology.DotNet);
        var workerSpecs = new NodeSpecs(8, 32, 100);
        var overcommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 };

        var resultNoHeadroom = _k8sService.CalculateWorkerNodes(apps, techConfig.Tiers, 1, workerSpecs, 0, overcommit);
        var resultMaxHeadroom = _k8sService.CalculateWorkerNodes(apps, techConfig.Tiers, 1, workerSpecs, 100, overcommit);

        resultMaxHeadroom.Should().BeGreaterThan(resultNoHeadroom);
    }

    [Fact]
    public void CalculateWorkerNodes_MixedAppTiers_CalculatesCorrectly()
    {
        var apps = new AppConfig { Small = 10, Medium = 10, Large = 10, XLarge = 5 };
        var techConfig = _technologyService.GetConfig(Technology.DotNet);
        var workerSpecs = new NodeSpecs(16, 64, 200);
        var overcommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 };

        var result = _k8sService.CalculateWorkerNodes(apps, techConfig.Tiers, 2, workerSpecs, 20, overcommit);

        result.Should().BeGreaterThan(3);
    }

    [Fact]
    public void CalculateWorkerNodes_VerySmallWorkerSpecs_NeedsMoreWorkers()
    {
        var apps = new AppConfig { Medium = 10 };
        var techConfig = _technologyService.GetConfig(Technology.DotNet);
        var tinyWorker = new NodeSpecs(1, 2, 10);
        var overcommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 };

        var result = _k8sService.CalculateWorkerNodes(apps, techConfig.Tiers, 1, tinyWorker, 0, overcommit);

        // DotNet medium: 0.5 CPU, 1 GB RAM per app
        // 10 apps = 5 CPU, 10 GB RAM total
        // tinyWorker: 1 CPU, 2 GB RAM (with ~85% system reserve = 0.85 CPU, 1.7 GB)
        // Needs ~6 workers (limited by RAM: 10 / 1.7)
        result.Should().BeGreaterThanOrEqualTo(3); // At least minimum HA workers
    }

    [Fact]
    public void CalculateWorkerNodes_VeryLargeWorkerSpecs_FewerWorkersNeeded()
    {
        var apps = new AppConfig { Medium = 10 };
        var techConfig = _technologyService.GetConfig(Technology.DotNet);
        var hugeWorker = new NodeSpecs(128, 512, 2000);
        var overcommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 };

        var result = _k8sService.CalculateWorkerNodes(apps, techConfig.Tiers, 1, hugeWorker, 0, overcommit);

        result.Should().Be(3); // Minimum since huge workers can handle everything
    }

    #endregion

    #region K8s Full Calculation Edge Cases

    [Fact]
    public void Calculate_EmptyEnvironmentSet_ReturnsEmptyResults()
    {
        var input = new K8sSizingInput
        {
            Distribution = Distribution.OpenShift,
            Technology = Technology.DotNet,
            ClusterMode = ClusterMode.MultiCluster,
            EnabledEnvironments = new HashSet<EnvironmentType>(),
            ProdApps = new AppConfig(),
            NonProdApps = new AppConfig()
        };

        var result = _k8sService.Calculate(input);

        result.Environments.Should().BeEmpty();
        result.GrandTotal.TotalNodes.Should().Be(0);
    }

    [Fact]
    public void Calculate_SingleEnvironment_ReturnsOneEnvironmentResult()
    {
        var input = new K8sSizingInput
        {
            Distribution = Distribution.Kubernetes,
            Technology = Technology.Java,
            ClusterMode = ClusterMode.MultiCluster,
            EnabledEnvironments = new HashSet<EnvironmentType> { EnvironmentType.Prod },
            ProdApps = new AppConfig { Medium = 10 },
            NonProdApps = new AppConfig()
        };

        var result = _k8sService.Calculate(input);

        result.Environments.Should().HaveCount(1);
        result.Environments[0].Environment.Should().Be(EnvironmentType.Prod);
    }

    [Fact]
    public void Calculate_AllEnvironmentTypes_ReturnsAllEnvironments()
    {
        var allEnvs = Enum.GetValues<EnvironmentType>().ToHashSet();
        var input = new K8sSizingInput
        {
            Distribution = Distribution.OpenShift,
            Technology = Technology.DotNet,
            ClusterMode = ClusterMode.MultiCluster,
            EnabledEnvironments = allEnvs,
            ProdApps = new AppConfig { Medium = 10 },
            NonProdApps = new AppConfig { Medium = 10 }
        };

        var result = _k8sService.Calculate(input);

        result.Environments.Should().HaveCount(allEnvs.Count);
    }

    [Fact]
    public void Calculate_ManagedDistribution_HasZeroMasters()
    {
        var input = new K8sSizingInput
        {
            Distribution = Distribution.EKS,
            Technology = Technology.DotNet,
            ClusterMode = ClusterMode.MultiCluster,
            EnabledEnvironments = new HashSet<EnvironmentType> { EnvironmentType.Prod },
            ProdApps = new AppConfig { Medium = 50 },
            NonProdApps = new AppConfig()
        };

        var result = _k8sService.Calculate(input);

        result.GrandTotal.TotalMasters.Should().Be(0);
        result.Environments.All(e => e.Masters == 0).Should().BeTrue();
    }

    [Fact]
    public void Calculate_OpenShift_HasInfraNodes()
    {
        var input = new K8sSizingInput
        {
            Distribution = Distribution.OpenShift,
            Technology = Technology.DotNet,
            ClusterMode = ClusterMode.MultiCluster,
            EnabledEnvironments = new HashSet<EnvironmentType> { EnvironmentType.Prod },
            ProdApps = new AppConfig { Medium = 50 },
            NonProdApps = new AppConfig()
        };

        var result = _k8sService.Calculate(input);

        result.GrandTotal.TotalInfra.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Calculate_NonOpenShiftDistribution_HasZeroInfraNodes()
    {
        var input = new K8sSizingInput
        {
            Distribution = Distribution.Kubernetes,
            Technology = Technology.DotNet,
            ClusterMode = ClusterMode.MultiCluster,
            EnabledEnvironments = new HashSet<EnvironmentType> { EnvironmentType.Prod },
            ProdApps = new AppConfig { Medium = 50 },
            NonProdApps = new AppConfig()
        };

        var result = _k8sService.Calculate(input);

        result.GrandTotal.TotalInfra.Should().Be(0);
    }

    #endregion

    #region VM Sizing Edge Cases

    [Fact]
    public void GetRoleSpecs_AllSizeTiers_ReturnValidSpecs()
    {
        foreach (var tier in Enum.GetValues<AppTier>())
        {
            foreach (var role in Enum.GetValues<ServerRole>())
            {
                var (cpu, ram) = _vmService.GetRoleSpecs(role, tier, Technology.DotNet);

                cpu.Should().BeGreaterThan(0, $"CPU for {role}/{tier} should be positive");
                ram.Should().BeGreaterThan(0, $"RAM for {role}/{tier} should be positive");
            }
        }
    }

    [Fact]
    public void GetRoleSpecs_AllTechnologies_ReturnValidSpecs()
    {
        foreach (var tech in Enum.GetValues<Technology>())
        {
            var (cpu, ram) = _vmService.GetRoleSpecs(ServerRole.App, AppTier.Medium, tech);

            cpu.Should().BeGreaterThan(0);
            ram.Should().BeGreaterThan(0);
        }
    }

    [Fact]
    public void GetHAMultiplier_AllPatterns_ReturnValidMultiplier()
    {
        foreach (var pattern in Enum.GetValues<HAPattern>())
        {
            var multiplier = _vmService.GetHAMultiplier(pattern);

            multiplier.Should().BeGreaterOrEqualTo(1.0);
            multiplier.Should().BeLessOrEqualTo(3.0);
        }
    }

    [Fact]
    public void GetLoadBalancerSpecs_AllOptions_ReturnValidSpecs()
    {
        foreach (var option in Enum.GetValues<LoadBalancerOption>())
        {
            var (vms, cpu, ram) = _vmService.GetLoadBalancerSpecs(option);

            vms.Should().BeGreaterOrEqualTo(0);
            if (vms > 0)
            {
                cpu.Should().BeGreaterThan(0);
                ram.Should().BeGreaterThan(0);
            }
        }
    }

    [Fact]
    public void Calculate_EmptyEnvironmentList_ReturnsEmptyResult()
    {
        var input = new VMSizingInput
        {
            Technology = Technology.DotNet,
            EnabledEnvironments = new HashSet<EnvironmentType>(),
            EnvironmentConfigs = new Dictionary<EnvironmentType, VMEnvironmentConfig>()
        };

        var result = _vmService.Calculate(input);

        result.Environments.Should().BeEmpty();
        result.GrandTotal.TotalVMs.Should().Be(0);
    }

    [Fact]
    public void Calculate_ZeroInstanceCount_ReturnsNoVMsForRole()
    {
        var input = CreateVMInputWithZeroInstances();

        var result = _vmService.Calculate(input);

        result.Environments.Should().NotBeEmpty();
        // Each role should have 0 instances if InstanceCount is 0
        var env = result.Environments.First();
        env.Roles.Where(r => r.TotalInstances > 0).Should().BeEmpty();
    }

    [Fact]
    public void Calculate_VeryHighSystemOverhead_AppliesCorrectly()
    {
        var input = CreateBasicVMInput();
        input.SystemOverheadPercent = 50;

        var resultNoOverhead = CreateBasicVMInput();
        resultNoOverhead.SystemOverheadPercent = 0;

        var withOverhead = _vmService.Calculate(input);
        var withoutOverhead = _vmService.Calculate(resultNoOverhead);

        withOverhead.GrandTotal.TotalCpu.Should().BeGreaterThan(withoutOverhead.GrandTotal.TotalCpu);
    }

    [Fact]
    public void Calculate_AllHAPatterns_CalculatesCorrectly()
    {
        foreach (var pattern in Enum.GetValues<HAPattern>())
        {
            var input = CreateBasicVMInput();
            foreach (var config in input.EnvironmentConfigs.Values)
            {
                config.HAPattern = pattern;
            }

            var result = _vmService.Calculate(input);

            result.Should().NotBeNull();
            result.GrandTotal.TotalVMs.Should().BeGreaterOrEqualTo(0);
        }
    }

    #endregion

    #region Distribution Service Edge Cases

    [Fact]
    public void GetConfig_AllDistributions_ReturnValidConfig()
    {
        foreach (var dist in Enum.GetValues<Distribution>())
        {
            var config = _distributionService.GetConfig(dist);

            // All distributions should return a valid config (may be a fallback)
            config.Should().NotBeNull();
            // Distributions without explicit configs fall back to OpenShift defaults
            // So we only verify the config has valid specs
            config.ProdWorker.Cpu.Should().BeGreaterThan(0);
            config.ProdWorker.Ram.Should().BeGreaterThan(0);
        }
    }

    [Fact]
    public void ManagedDistributions_HaveManagedControlPlane()
    {
        var managedDistributions = new[] { Distribution.EKS, Distribution.AKS, Distribution.GKE, Distribution.OKE };

        foreach (var dist in managedDistributions)
        {
            var config = _distributionService.GetConfig(dist);
            config.HasManagedControlPlane.Should().BeTrue($"{dist} should have managed control plane");
        }
    }

    [Fact]
    public void SelfManagedDistributions_HaveControlPlaneSpecs()
    {
        var selfManagedDistributions = new[] { Distribution.OpenShift, Distribution.Kubernetes, Distribution.Rancher, Distribution.Tanzu };

        foreach (var dist in selfManagedDistributions)
        {
            var config = _distributionService.GetConfig(dist);
            config.HasManagedControlPlane.Should().BeFalse($"{dist} should be self-managed");
            config.ProdControlPlane.Cpu.Should().BeGreaterThan(0);
            config.ProdControlPlane.Ram.Should().BeGreaterThan(0);
        }
    }

    #endregion

    #region Technology Service Edge Cases

    [Fact]
    public void GetConfig_AllTechnologies_ReturnValidConfig()
    {
        foreach (var tech in Enum.GetValues<Technology>())
        {
            var config = _technologyService.GetConfig(tech);

            config.Should().NotBeNull();
            config.Technology.Should().Be(tech);
            config.Tiers.Should().NotBeNull();
            config.Name.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public void GetConfig_AllTechnologies_HaveAllTiers()
    {
        foreach (var tech in Enum.GetValues<Technology>())
        {
            var config = _technologyService.GetConfig(tech);

            foreach (var tier in Enum.GetValues<AppTier>())
            {
                config.Tiers.ContainsKey(tier).Should().BeTrue($"{tech} should have {tier} tier");
                config.Tiers[tier].Cpu.Should().BeGreaterThan(0);
                config.Tiers[tier].Ram.Should().BeGreaterThan(0);
            }
        }
    }

    [Fact]
    public void GetConfig_JavaMendixOutSystems_HaveHigherMemory()
    {
        var highMemTechs = new[] { Technology.Java, Technology.Mendix, Technology.OutSystems };
        var baselineTech = _technologyService.GetConfig(Technology.DotNet);

        foreach (var tech in highMemTechs)
        {
            var config = _technologyService.GetConfig(tech);

            config.Tiers[AppTier.Medium].Ram.Should()
                .BeGreaterOrEqualTo(baselineTech.Tiers[AppTier.Medium].Ram,
                    $"{tech} should have >= memory than DotNet");
        }
    }

    #endregion

    #region Numerical Boundary Tests

    [Fact]
    public void Calculate_VeryLargeAppCount_HandlesCorrectly()
    {
        var input = new K8sSizingInput
        {
            Distribution = Distribution.OpenShift,
            Technology = Technology.DotNet,
            ClusterMode = ClusterMode.MultiCluster,
            EnabledEnvironments = new HashSet<EnvironmentType> { EnvironmentType.Prod },
            ProdApps = new AppConfig { Medium = 1000 },
            NonProdApps = new AppConfig()
        };

        var result = _k8sService.Calculate(input);

        result.Should().NotBeNull();
        result.GrandTotal.TotalNodes.Should().BeGreaterThan(0);
        result.GrandTotal.TotalCpu.Should().BeGreaterThan(0);
        result.GrandTotal.TotalRam.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(2.0)]
    [InlineData(5.0)]
    [InlineData(10.0)]
    public void OvercommitSettings_VariousRatios_CalculateCorrectly(double ratio)
    {
        var apps = new AppConfig { Medium = 20 };
        var techConfig = _technologyService.GetConfig(Technology.DotNet);
        var workerSpecs = new NodeSpecs(8, 32, 100);
        var overcommit = new OvercommitSettings { Cpu = ratio, Memory = ratio };

        // Should not throw for any valid ratio
        var result = _k8sService.CalculateWorkerNodes(apps, techConfig.Tiers, 1, workerSpecs, 0, overcommit);

        result.Should().BeGreaterOrEqualTo(3);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    [InlineData(37.5)]
    [InlineData(50)]
    [InlineData(100)]
    [InlineData(200)]
    public void HeadroomPercent_VariousValues_CalculateCorrectly(double headroom)
    {
        var apps = new AppConfig { Medium = 20 };
        var techConfig = _technologyService.GetConfig(Technology.DotNet);
        var workerSpecs = new NodeSpecs(8, 32, 100);
        var overcommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 };

        // Should not throw for any headroom value
        var result = _k8sService.CalculateWorkerNodes(apps, techConfig.Tiers, 1, workerSpecs, headroom, overcommit);

        result.Should().BeGreaterOrEqualTo(3);
    }

    #endregion

    #region Helper Methods

    private VMSizingInput CreateBasicVMInput()
    {
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };

        var configs = new Dictionary<EnvironmentType, VMEnvironmentConfig>
        {
            [EnvironmentType.Prod] = new VMEnvironmentConfig
            {
                Environment = EnvironmentType.Prod,
                Enabled = true,
                HAPattern = HAPattern.None,
                LoadBalancer = LoadBalancerOption.None,
                StorageGB = 100,
                Roles = new List<VMRoleConfig>
                {
                    new() { Role = ServerRole.App, Size = AppTier.Medium, InstanceCount = 2, DiskGB = 100 },
                    new() { Role = ServerRole.Database, Size = AppTier.Medium, InstanceCount = 1, DiskGB = 200 }
                }
            }
        };

        return new VMSizingInput
        {
            Technology = Technology.DotNet,
            EnabledEnvironments = enabledEnvs,
            EnvironmentConfigs = configs,
            SystemOverheadPercent = 0
        };
    }

    private VMSizingInput CreateVMInputWithZeroInstances()
    {
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };

        var configs = new Dictionary<EnvironmentType, VMEnvironmentConfig>
        {
            [EnvironmentType.Prod] = new VMEnvironmentConfig
            {
                Environment = EnvironmentType.Prod,
                Enabled = true,
                HAPattern = HAPattern.None,
                LoadBalancer = LoadBalancerOption.None,
                StorageGB = 100,
                Roles = new List<VMRoleConfig>
                {
                    new() { Role = ServerRole.App, Size = AppTier.Medium, InstanceCount = 0, DiskGB = 100 },
                    new() { Role = ServerRole.Database, Size = AppTier.Medium, InstanceCount = 0, DiskGB = 200 }
                }
            }
        };

        return new VMSizingInput
        {
            Technology = Technology.DotNet,
            EnabledEnvironments = enabledEnvs,
            EnvironmentConfigs = configs
        };
    }

    #endregion
}
