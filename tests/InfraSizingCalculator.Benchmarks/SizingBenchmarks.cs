using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Services;

namespace InfraSizingCalculator.Benchmarks;

/// <summary>
/// Performance benchmarks for sizing calculation services.
/// Run with: dotnet run -c Release
/// For quick test: dotnet run -c Release -- --job short
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class SizingBenchmarks
{
    private K8sSizingService _k8sService = null!;
    private VMSizingService _vmService = null!;
    private DistributionService _distributionService = null!;
    private TechnologyService _technologyService = null!;

    private K8sSizingInput _smallK8sInput = null!;
    private K8sSizingInput _largeK8sInput = null!;
    private VMSizingInput _smallVMInput = null!;
    private VMSizingInput _largeVMInput = null!;

    [GlobalSetup]
    public void Setup()
    {
        _distributionService = new DistributionService();
        _technologyService = new TechnologyService();
        _k8sService = new K8sSizingService(_distributionService, _technologyService);
        _vmService = new VMSizingService(_technologyService);

        // Create test inputs with varying sizes
        _smallK8sInput = CreateK8sInput(5, 3, 1);      // Small deployment
        _largeK8sInput = CreateK8sInput(50, 25, 10);   // Large deployment

        _smallVMInput = CreateVMInput(2, 1, 0);        // Small VM deployment
        _largeVMInput = CreateVMInput(5, 3, 1);        // Larger VM deployment
    }

    private static K8sSizingInput CreateK8sInput(int small, int medium, int large)
    {
        return new K8sSizingInput
        {
            Distribution = Distribution.Kubernetes,
            Technology = Technology.DotNet,
            ClusterMode = ClusterMode.MultiCluster,
            ProdApps = new AppConfig { Small = small, Medium = medium, Large = large },
            NonProdApps = new AppConfig { Small = small / 2, Medium = medium / 2, Large = large / 2 },
            EnabledEnvironments = new HashSet<EnvironmentType>
            {
                EnvironmentType.Dev,
                EnvironmentType.Test,
                EnvironmentType.Prod
            },
            Replicas = new ReplicaSettings { Prod = 3, NonProd = 2 },
            Headroom = new HeadroomSettings { Prod = 37.5, Dev = 33.0, Test = 33.0 },
            EnableHeadroom = true,
            ProdOvercommit = new OvercommitSettings { Cpu = 2.0, Memory = 1.5 },
            NonProdOvercommit = new OvercommitSettings { Cpu = 3.0, Memory = 2.0 },
            HADRConfig = new K8sHADRConfig
            {
                ControlPlaneHA = K8sControlPlaneHA.StackedHA,
                ControlPlaneNodes = 3,
                NodeDistribution = K8sNodeDistribution.MultiAZ,
                AvailabilityZones = 3
            }
        };
    }

    private static VMSizingInput CreateVMInput(int webInstances, int appInstances, int dbInstances)
    {
        var input = new VMSizingInput
        {
            Technology = Technology.DotNet,
            EnabledEnvironments = new HashSet<EnvironmentType>
            {
                EnvironmentType.Dev,
                EnvironmentType.Test,
                EnvironmentType.Prod
            },
            EnvironmentConfigs = new Dictionary<EnvironmentType, VMEnvironmentConfig>()
        };

        // Configure each environment
        foreach (var env in input.EnabledEnvironments)
        {
            input.EnvironmentConfigs[env] = new VMEnvironmentConfig
            {
                Environment = env,
                Enabled = true,
                Roles = new List<VMRoleConfig>
                {
                    new VMRoleConfig { Role = ServerRole.Web, InstanceCount = webInstances, Size = AppTier.Medium },
                    new VMRoleConfig { Role = ServerRole.App, InstanceCount = appInstances, Size = AppTier.Medium },
                    new VMRoleConfig { Role = ServerRole.Database, InstanceCount = dbInstances, Size = AppTier.Large }
                },
                HAPattern = env == EnvironmentType.Prod ? HAPattern.ActiveActive : HAPattern.None,
                StorageGB = 100
            };
        }

        return input;
    }

    // K8s Sizing Benchmarks
    [Benchmark(Description = "K8s: Small deployment (5/3/1 apps)")]
    public K8sSizingResult K8s_SmallDeployment() => _k8sService.Calculate(_smallK8sInput);

    [Benchmark(Description = "K8s: Large deployment (50/25/10 apps)")]
    public K8sSizingResult K8s_LargeDeployment() => _k8sService.Calculate(_largeK8sInput);

    // VM Sizing Benchmarks
    [Benchmark(Description = "VM: Small deployment (2/1/0 instances)")]
    public VMSizingResult VM_SmallDeployment() => _vmService.Calculate(_smallVMInput);

    [Benchmark(Description = "VM: Large deployment (5/3/1 instances)")]
    public VMSizingResult VM_LargeDeployment() => _vmService.Calculate(_largeVMInput);

    // Distribution Service Benchmarks
    [Benchmark(Description = "Distribution: Get config by enum")]
    public DistributionConfig Distribution_GetConfig()
    {
        return _distributionService.GetConfig(Distribution.OpenShift);
    }

    [Benchmark(Description = "Distribution: Get all configs")]
    public IEnumerable<DistributionConfig> Distribution_GetAllConfigs()
    {
        return _distributionService.GetAll();
    }

    // Technology Service Benchmarks
    [Benchmark(Description = "Technology: Get config by enum")]
    public TechnologyConfig Technology_GetConfig()
    {
        return _technologyService.GetConfig(Technology.DotNet);
    }

    [Benchmark(Description = "Technology: Get all configs")]
    public IEnumerable<TechnologyConfig> Technology_GetAllConfigs()
    {
        return _technologyService.GetAll();
    }

    // K8s HA/DR Configuration Benchmarks
    [Benchmark(Description = "K8s HA/DR: Cost multiplier calculation")]
    public decimal K8s_HADRCostMultiplier()
    {
        var config = new K8sHADRConfig
        {
            ControlPlaneHA = K8sControlPlaneHA.ExternalEtcd,
            ControlPlaneNodes = 5,
            NodeDistribution = K8sNodeDistribution.MultiRegion,
            DRPattern = K8sDRPattern.WarmStandby,
            BackupStrategy = K8sBackupStrategy.Kasten
        };
        return config.GetCostMultiplier(Distribution.OpenShift);
    }
}
