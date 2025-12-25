using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Services.Interfaces;

namespace InfraSizingCalculator.Services;

/// <summary>
/// K8s sizing calculation service implementing all business rules
/// BR-M001 through BR-M004: Master node rules
/// BR-I001 through BR-I006: Infrastructure node rules
/// BR-W001 through BR-W006: Worker node rules
/// BR-H001 through BR-H009: Headroom rules
/// BR-RC001 through BR-RC007: Resource calculation rules
/// </summary>
public class K8sSizingService : IK8sSizingService
{
    private readonly IDistributionService _distributionService;
    private readonly ITechnologyService _technologyService;
    private readonly CalculatorSettings _settings;

    // Settings-based properties for business rules
    private double SystemReserve => _settings.K8sInfrastructure.SystemReserveFactor;
    private int MinWorkers => _settings.K8sInfrastructure.MinWorkers;
    private int AppsPerInfra => _settings.K8sInfrastructure.AppsPerInfra;
    private int MinInfra => _settings.K8sInfrastructure.MinInfra;
    private int MaxInfra => _settings.K8sInfrastructure.MaxInfra;
    private int LargeDeploymentThreshold => _settings.K8sInfrastructure.LargeDeploymentThreshold;
    private int MinProdInfraLarge => _settings.K8sInfrastructure.MinProdInfraLarge;
    private int LargeClusterWorkerThreshold => _settings.K8sInfrastructure.LargeClusterWorkerThreshold;

    public K8sSizingService(
        IDistributionService distributionService,
        ITechnologyService technologyService,
        CalculatorSettings? settings = null)
    {
        _distributionService = distributionService;
        _technologyService = technologyService;
        _settings = settings ?? new CalculatorSettings();
    }

    public K8sSizingResult Calculate(K8sSizingInput input)
    {
        var distroConfig = input.CustomNodeSpecs ?? _distributionService.GetConfig(input.Distribution);
        var techConfig = _technologyService.GetConfig(input.Technology);

        return input.ClusterMode switch
        {
            ClusterMode.MultiCluster => CalculateMultiCluster(input, distroConfig, techConfig),
            ClusterMode.SharedCluster => CalculateSharedCluster(input, distroConfig, techConfig),
            ClusterMode.PerEnvironment => CalculatePerEnvironment(input, distroConfig, techConfig),
            _ => throw new ArgumentOutOfRangeException(nameof(input.ClusterMode))
        };
    }

    private K8sSizingResult CalculateMultiCluster(
        K8sSizingInput input,
        DistributionConfig distroConfig,
        TechnologyConfig techConfig)
    {
        var results = new List<EnvironmentResult>();

        foreach (var env in input.EnabledEnvironments.OrderBy(e => e))
        {
            var envResult = CalculateEnvironment(env, input, distroConfig, techConfig);
            results.Add(envResult);
        }

        return new K8sSizingResult
        {
            Environments = results,
            GrandTotal = CalculateGrandTotal(results),
            Configuration = input,
            DistributionName = distroConfig.Name,
            TechnologyName = techConfig.Name,
            NodeSpecs = distroConfig,
            CalculatedAt = DateTime.UtcNow
        };
    }

    private K8sSizingResult CalculateSharedCluster(
        K8sSizingInput input,
        DistributionConfig distroConfig,
        TechnologyConfig techConfig)
    {
        // Shared cluster uses UNIFIED node specs (no Prod/Non-Prod distinction)
        // All namespaces share the same cluster with the same node specifications

        // Calculate combined resources for all namespaces
        double totalCpuRequired = 0;
        double totalRamRequired = 0;
        int totalApps = 0;

        foreach (var env in input.EnabledEnvironments)
        {
            var isProd = IsProdEnvironment(env);
            var apps = GetAppsForEnvironment(env, input, isProd);
            // Use a single replica count for shared cluster (Prod replicas as baseline)
            var replicas = input.Replicas.Prod;

            var (cpuReq, ramReq) = CalculateAppResources(apps, techConfig.Tiers, replicas);
            totalCpuRequired += cpuReq;
            totalRamRequired += ramReq;
            totalApps += apps.TotalApps;
        }

        // UNIFIED SPECS: Use prod worker specs for all workloads in shared cluster
        // No distinction between Prod/Non-Prod in shared cluster mode
        var workerSpecs = distroConfig.ProdWorker;

        // Use unified overcommit settings (prod settings apply to entire cluster)
        var overcommit = input.ProdOvercommit;

        // Calculate workers
        int workers = CalculateWorkerNodesInternal(
            totalCpuRequired,
            totalRamRequired,
            workerSpecs,
            overcommit);

        // Apply unified headroom (prod headroom for shared cluster)
        if (input.EnableHeadroom)
        {
            workers = ApplyHeadroom(workers, input.Headroom.Prod);
        }

        var masters = CalculateMasterNodes(workers, distroConfig.HasManagedControlPlane);
        var infra = CalculateInfraNodes(totalApps, true, distroConfig.HasInfraNodes);

        // Calculate resources using unified specs
        var resources = CalculateSharedClusterResources(masters, infra, workers, distroConfig);

        var sharedResult = new EnvironmentResult
        {
            Environment = EnvironmentType.Prod,
            EnvironmentName = "Shared Cluster",
            IsProd = true,
            Apps = totalApps,
            Replicas = input.Replicas.Prod,
            Pods = totalApps * input.Replicas.Prod,
            Masters = masters,
            Infra = infra,
            Workers = workers,
            TotalNodes = resources.totalNodes,
            TotalCpu = resources.totalCpu,
            TotalRam = resources.totalRam,
            TotalDisk = resources.totalDisk
        };

        return new K8sSizingResult
        {
            Environments = new List<EnvironmentResult> { sharedResult },
            GrandTotal = new GrandTotal
            {
                TotalNodes = resources.totalNodes,
                TotalMasters = masters,
                TotalInfra = infra,
                TotalWorkers = workers,
                TotalCpu = resources.totalCpu,
                TotalRam = resources.totalRam,
                TotalDisk = resources.totalDisk
            },
            Configuration = input,
            DistributionName = distroConfig.Name,
            TechnologyName = techConfig.Name,
            NodeSpecs = distroConfig,
            CalculatedAt = DateTime.UtcNow
        };
    }

    private K8sSizingResult CalculatePerEnvironment(
        K8sSizingInput input,
        DistributionConfig distroConfig,
        TechnologyConfig techConfig)
    {
        // Per-Environment is a SINGLE CLUSTER - use unified specs (same as Shared Cluster)
        // The only difference from Shared is that we're sizing for one specific environment's workload
        var env = input.SelectedEnvironment;
        var isProd = IsProdEnvironment(env);
        var apps = GetAppsForEnvironment(env, input, isProd);
        var replicas = GetReplicasForEnvironment(env, input.Replicas);
        var headroom = GetHeadroomForEnvironment(env, input.Headroom, input.EnableHeadroom);

        // UNIFIED SPECS: Use prod settings for single cluster (same node specs for entire cluster)
        var overcommit = input.ProdOvercommit;
        var workerSpecs = distroConfig.ProdWorker;

        // Calculate app resources
        var (cpuRequired, ramRequired) = CalculateAppResources(apps, techConfig.Tiers, replicas);

        // Calculate workers using unified specs
        int workers = CalculateWorkerNodesInternal(cpuRequired, ramRequired, workerSpecs, overcommit);

        // Apply headroom
        if (headroom > 0)
        {
            workers = ApplyHeadroom(workers, headroom);
        }

        // Calculate masters and infra
        var masters = CalculateMasterNodes(workers, distroConfig.HasManagedControlPlane);
        var infra = CalculateInfraNodes(apps.TotalApps, isProd, distroConfig.HasInfraNodes);

        // UNIFIED RESOURCES: Use unified specs for resource calculation (always use Prod specs)
        var resources = CalculateSharedClusterResources(masters, infra, workers, distroConfig);

        var envResult = new EnvironmentResult
        {
            Environment = env,
            EnvironmentName = $"{GetEnvironmentName(env)} Cluster",
            IsProd = isProd,
            Apps = apps.TotalApps,
            Replicas = replicas,
            Pods = apps.TotalApps * replicas,
            Masters = masters,
            Infra = infra,
            Workers = workers,
            TotalNodes = resources.totalNodes,
            TotalCpu = resources.totalCpu,
            TotalRam = resources.totalRam,
            TotalDisk = resources.totalDisk
        };

        return new K8sSizingResult
        {
            Environments = new List<EnvironmentResult> { envResult },
            GrandTotal = new GrandTotal
            {
                TotalNodes = envResult.TotalNodes,
                TotalMasters = envResult.Masters,
                TotalInfra = envResult.Infra,
                TotalWorkers = envResult.Workers,
                TotalCpu = envResult.TotalCpu,
                TotalRam = envResult.TotalRam,
                TotalDisk = envResult.TotalDisk
            },
            Configuration = input,
            DistributionName = distroConfig.Name,
            TechnologyName = techConfig.Name,
            NodeSpecs = distroConfig,
            CalculatedAt = DateTime.UtcNow
        };
    }

    private EnvironmentResult CalculateEnvironment(
        EnvironmentType env,
        K8sSizingInput input,
        DistributionConfig distroConfig,
        TechnologyConfig techConfig)
    {
        var isProd = IsProdEnvironment(env);
        // Use per-environment app counts if available, otherwise fall back to Prod/NonProd
        var apps = GetAppsForEnvironment(env, input, isProd);
        var replicas = GetReplicasForEnvironment(env, input.Replicas);
        var headroom = GetHeadroomForEnvironment(env, input.Headroom, input.EnableHeadroom);
        var overcommit = isProd ? input.ProdOvercommit : input.NonProdOvercommit;

        // BR-RC001, BR-RC002: Calculate app resources
        var (cpuRequired, ramRequired) = CalculateAppResources(apps, techConfig.Tiers, replicas);

        // IMPORTANT: Use prod worker specs for capacity calculation (matches JS implementation)
        var workerSpecs = distroConfig.ProdWorker;

        // BR-W001 through BR-W005: Calculate workers
        int workers = CalculateWorkerNodesInternal(cpuRequired, ramRequired, workerSpecs, overcommit);

        // BR-H001, BR-H002: Apply headroom
        if (headroom > 0)
        {
            workers = ApplyHeadroom(workers, headroom);
        }

        // BR-M001 through BR-M004: Calculate masters
        var masters = CalculateMasterNodes(workers, distroConfig.HasManagedControlPlane);

        // BR-I001 through BR-I006: Calculate infra nodes
        var infra = CalculateInfraNodes(apps.TotalApps, isProd, distroConfig.HasInfraNodes);

        // BR-RC005 through BR-RC007, BR-W006: Calculate resources (now per-environment)
        var resources = CalculateClusterResources(masters, infra, workers, distroConfig, env);

        return new EnvironmentResult
        {
            Environment = env,
            EnvironmentName = GetEnvironmentName(env),
            IsProd = isProd,
            Apps = apps.TotalApps,
            Replicas = replicas,
            Pods = apps.TotalApps * replicas,  // BR-R005
            Masters = masters,
            Infra = infra,
            Workers = workers,
            TotalNodes = resources.totalNodes,
            TotalCpu = resources.totalCpu,
            TotalRam = resources.totalRam,
            TotalDisk = resources.totalDisk
        };
    }

    /// <summary>
    /// BR-M001 through BR-M004: Master node calculation
    /// </summary>
    public int CalculateMasterNodes(int workerCount, bool isManagedControlPlane)
    {
        // BR-M001: Managed control plane (EKS, AKS, GKE) = 0 masters
        if (isManagedControlPlane)
            return 0;

        // BR-M003: Large clusters (100+ workers) need 5 masters
        if (workerCount > LargeClusterWorkerThreshold)
            return 5;

        // BR-M002, BR-M004: Standard HA quorum = 3 masters
        return 3;
    }

    /// <summary>
    /// BR-I001 through BR-I006: Infrastructure node calculation
    /// </summary>
    public int CalculateInfraNodes(int totalApps, bool isProd, bool hasInfraNodes)
    {
        // BR-I001: Only OpenShift has infra nodes
        if (!hasInfraNodes)
            return 0;

        // BR-I004: Scale at 1 per 25 apps
        int infraNodes = Math.Max(MinInfra, (int)Math.Ceiling((double)totalApps / AppsPerInfra));

        // BR-I005: Large production (>=50 apps) needs minimum 5 infra
        // BR-I006: Small production (<50 apps) can use 3
        if (isProd && totalApps >= LargeDeploymentThreshold && infraNodes < MinProdInfraLarge)
        {
            infraNodes = MinProdInfraLarge;
        }

        // BR-I003: Cap at maximum
        return Math.Min(infraNodes, MaxInfra);
    }

    /// <summary>
    /// BR-W001 through BR-W006: Worker node calculation (public interface)
    /// </summary>
    public int CalculateWorkerNodes(
        AppConfig apps,
        Dictionary<AppTier, TierSpecs> tierSpecs,
        int replicas,
        NodeSpecs workerSpecs,
        double headroomPercent,
        OvercommitSettings overcommit)
    {
        var (cpuRequired, ramRequired) = CalculateAppResources(apps, tierSpecs, replicas);
        int workers = CalculateWorkerNodesInternal(cpuRequired, ramRequired, workerSpecs, overcommit);

        if (headroomPercent > 0)
        {
            workers = ApplyHeadroom(workers, headroomPercent);
        }

        return workers;
    }

    /// <summary>
    /// Internal worker calculation from CPU/RAM requirements
    /// </summary>
    private int CalculateWorkerNodesInternal(
        double cpuRequired,
        double ramRequired,
        NodeSpecs workerSpecs,
        OvercommitSettings overcommit)
    {
        // BR-RC003: Worker CPU available (with system reserve and overcommit)
        double workerCpuAvail = workerSpecs.Cpu * SystemReserve * overcommit.Cpu;
        // BR-RC004: Worker RAM available
        double workerRamAvail = workerSpecs.Ram * SystemReserve * overcommit.Memory;

        // BR-W003: Workers by CPU
        int workersByCpu = (int)Math.Ceiling(cpuRequired / workerCpuAvail);
        // BR-W004: Workers by RAM
        int workersByRam = (int)Math.Ceiling(ramRequired / workerRamAvail);

        // BR-W005: Final = MAX(byCpu, byRam, minimum)
        // BR-W001: Minimum workers = 3
        return Math.Max(Math.Max(workersByCpu, workersByRam), MinWorkers);
    }

    /// <summary>
    /// BR-H001, BR-H002: Apply headroom to worker count
    /// </summary>
    private int ApplyHeadroom(int workers, double headroomPercent)
    {
        if (headroomPercent <= 0) return workers;
        return (int)Math.Ceiling(workers * (1 + headroomPercent / 100));
    }

    /// <summary>
    /// BR-RC001, BR-RC002: Calculate total CPU and RAM required
    /// </summary>
    private (double cpuRequired, double ramRequired) CalculateAppResources(
        AppConfig apps,
        Dictionary<AppTier, TierSpecs> tierSpecs,
        int replicas)
    {
        double cpuRequired = 0;
        double ramRequired = 0;

        if (apps.Small > 0 && tierSpecs.TryGetValue(AppTier.Small, out var smallSpec))
        {
            cpuRequired += apps.Small * smallSpec.Cpu * replicas;
            ramRequired += apps.Small * smallSpec.Ram * replicas;
        }

        if (apps.Medium > 0 && tierSpecs.TryGetValue(AppTier.Medium, out var mediumSpec))
        {
            cpuRequired += apps.Medium * mediumSpec.Cpu * replicas;
            ramRequired += apps.Medium * mediumSpec.Ram * replicas;
        }

        if (apps.Large > 0 && tierSpecs.TryGetValue(AppTier.Large, out var largeSpec))
        {
            cpuRequired += apps.Large * largeSpec.Cpu * replicas;
            ramRequired += apps.Large * largeSpec.Ram * replicas;
        }

        if (apps.XLarge > 0 && tierSpecs.TryGetValue(AppTier.XLarge, out var xlargeSpec))
        {
            cpuRequired += apps.XLarge * xlargeSpec.Cpu * replicas;
            ramRequired += apps.XLarge * xlargeSpec.Ram * replicas;
        }

        return (cpuRequired, ramRequired);
    }

    /// <summary>
    /// Calculate resources for shared cluster using UNIFIED specs
    /// All node types use the same (prod) specifications
    /// </summary>
    private (int totalNodes, int totalCpu, int totalRam, int totalDisk) CalculateSharedClusterResources(
        int masters,
        int infra,
        int workers,
        DistributionConfig distroConfig)
    {
        // UNIFIED SPECS: Use prod specs for ALL nodes in shared cluster
        var masterSpecs = distroConfig.ProdControlPlane;
        var infraSpecs = distroConfig.ProdInfra;
        var workerSpecs = distroConfig.ProdWorker;

        int totalNodes = masters + infra + workers;
        int totalCpu = (masters * masterSpecs.Cpu) +
                       (infra * infraSpecs.Cpu) +
                       (workers * workerSpecs.Cpu);
        int totalRam = (masters * masterSpecs.Ram) +
                       (infra * infraSpecs.Ram) +
                       (workers * workerSpecs.Ram);
        int totalDisk = (masters * masterSpecs.Disk) +
                        (infra * infraSpecs.Disk) +
                        (workers * workerSpecs.Disk);

        return (totalNodes, totalCpu, totalRam, totalDisk);
    }

    /// <summary>
    /// BR-RC005 through BR-RC007, BR-W006: Calculate cluster resources
    /// Calculates cluster resources matching JS implementation:
    /// - Master/Infra specs: Prod or Non-Prod based on environment type
    /// - Worker specs: ALWAYS use Prod specs (consistent across all environments)
    /// </summary>
    private (int totalNodes, int totalCpu, int totalRam, int totalDisk) CalculateClusterResources(
        int masters,
        int infra,
        int workers,
        DistributionConfig distroConfig,
        EnvironmentType env)
    {
        var isProd = IsProdEnvironment(env);
        // Master and Infra use Prod/NonProd distinction
        var masterSpecs = isProd ? distroConfig.ProdControlPlane : distroConfig.NonProdControlPlane;
        var infraSpecs = isProd ? distroConfig.ProdInfra : distroConfig.NonProdInfra;
        // Worker specs: ALWAYS use ProdWorker (matches JS implementation)
        var workerSpecs = distroConfig.ProdWorker;

        // BR-RC005: Total nodes
        int totalNodes = masters + infra + workers;

        // BR-RC006: Total vCPU
        int totalCpu = (masters * masterSpecs.Cpu) +
                       (infra * infraSpecs.Cpu) +
                       (workers * workerSpecs.Cpu);

        // BR-RC007: Total RAM
        int totalRam = (masters * masterSpecs.Ram) +
                       (infra * infraSpecs.Ram) +
                       (workers * workerSpecs.Ram);

        // Total Disk
        int totalDisk = (masters * masterSpecs.Disk) +
                        (infra * infraSpecs.Disk) +
                        (workers * workerSpecs.Disk);

        return (totalNodes, totalCpu, totalRam, totalDisk);
    }

    private GrandTotal CalculateGrandTotal(List<EnvironmentResult> results)
    {
        return new GrandTotal
        {
            TotalNodes = results.Sum(r => r.TotalNodes),
            TotalMasters = results.Sum(r => r.Masters),
            TotalInfra = results.Sum(r => r.Infra),
            TotalWorkers = results.Sum(r => r.Workers),
            TotalCpu = results.Sum(r => r.TotalCpu),
            TotalRam = results.Sum(r => r.TotalRam),
            TotalDisk = results.Sum(r => r.TotalDisk)
        };
    }

    /// <summary>
    /// BR-E003: Production and DR are Production type
    /// </summary>
    private bool IsProdEnvironment(EnvironmentType env)
    {
        return env == EnvironmentType.Prod || env == EnvironmentType.DR;
    }

    /// <summary>
    /// Get apps for a specific environment - uses per-environment counts if available
    /// </summary>
    private AppConfig GetAppsForEnvironment(EnvironmentType env, K8sSizingInput input, bool isProd)
    {
        // If per-environment app counts are provided, use them
        if (input.EnvironmentApps != null && input.EnvironmentApps.TryGetValue(env, out var envApps))
        {
            return envApps;
        }
        // Fall back to Prod/NonProd
        return isProd ? input.ProdApps : input.NonProdApps;
    }

    /// <summary>
    /// BR-R001, BR-R002, BR-R003: Get replicas for environment
    /// </summary>
    private int GetReplicasForEnvironment(EnvironmentType env, ReplicaSettings settings)
    {
        return env switch
        {
            EnvironmentType.Dev => settings.NonProd,    // BR-R002
            EnvironmentType.Test => settings.NonProd,   // BR-R002
            EnvironmentType.Stage => settings.Stage,    // BR-R003
            EnvironmentType.Prod => settings.Prod,      // BR-R001
            EnvironmentType.DR => settings.Prod,        // BR-E004
            _ => settings.NonProd
        };
    }

    /// <summary>
    /// BR-H003 through BR-H009: Get headroom for environment
    /// </summary>
    private double GetHeadroomForEnvironment(EnvironmentType env, HeadroomSettings settings, bool enabled)
    {
        // BR-H009: When disabled, headroom = 0
        if (!enabled) return 0;

        return env switch
        {
            EnvironmentType.Dev => settings.Dev,      // BR-H003
            EnvironmentType.Test => settings.Test,    // BR-H004
            EnvironmentType.Stage => settings.Stage,  // BR-H005
            EnvironmentType.Prod => settings.Prod,    // BR-H006
            EnvironmentType.DR => settings.DR,        // BR-H007
            _ => 0
        };
    }

    private double CalculateAverageHeadroom(HashSet<EnvironmentType> environments, HeadroomSettings settings)
    {
        if (!environments.Any()) return 0;

        double total = 0;
        foreach (var env in environments)
        {
            total += GetHeadroomForEnvironment(env, settings, true);
        }
        return total / environments.Count;
    }

    private string GetEnvironmentName(EnvironmentType env)
    {
        return env switch
        {
            EnvironmentType.Dev => "Development",
            EnvironmentType.Test => "Test",
            EnvironmentType.Stage => "Staging",
            EnvironmentType.Prod => "Production",
            EnvironmentType.DR => "DR",
            _ => env.ToString()
        };
    }
}
