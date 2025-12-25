using InfraSizingCalculator.Models.Enums;

namespace InfraSizingCalculator.Models;

/// <summary>
/// Centralized settings for all calculator configurable values.
/// Eliminates hardcoded constants from services.
/// </summary>
public class CalculatorSettings
{
    /// <summary>
    /// K8s infrastructure settings
    /// </summary>
    public K8sInfrastructureSettings K8sInfrastructure { get; set; } = new();

    /// <summary>
    /// Environment default settings (replicas, headroom)
    /// </summary>
    public EnvironmentDefaultSettings EnvironmentDefaults { get; set; } = new();

    /// <summary>
    /// HA pattern multiplier settings
    /// </summary>
    public HAPatternSettings HAPatterns { get; set; } = new();

    /// <summary>
    /// Load balancer specification settings
    /// </summary>
    public LoadBalancerSettings LoadBalancers { get; set; } = new();

    /// <summary>
    /// VM role base specifications per tier
    /// </summary>
    public VMRoleSettings VMRoles { get; set; } = new();
}

/// <summary>
/// K8s infrastructure settings (BR-W001, BR-W002, BR-I002-I005, BR-M003)
/// </summary>
public class K8sInfrastructureSettings
{
    /// <summary>
    /// BR-W002: System reserve percentage (default 15%)
    /// Calculated as 1 - SystemReservePercent/100 for available capacity
    /// </summary>
    public double SystemReservePercent { get; set; } = 15;

    /// <summary>
    /// BR-W001: Minimum worker nodes
    /// </summary>
    public int MinWorkers { get; set; } = 3;

    /// <summary>
    /// BR-I004: Apps per infrastructure node ratio
    /// </summary>
    public int AppsPerInfra { get; set; } = 25;

    /// <summary>
    /// BR-I002: Minimum infrastructure nodes
    /// </summary>
    public int MinInfra { get; set; } = 3;

    /// <summary>
    /// BR-I003: Maximum infrastructure nodes
    /// </summary>
    public int MaxInfra { get; set; } = 10;

    /// <summary>
    /// BR-I005: Large deployment threshold (apps count)
    /// </summary>
    public int LargeDeploymentThreshold { get; set; } = 50;

    /// <summary>
    /// BR-I005: Minimum infra nodes for large production deployments
    /// </summary>
    public int MinProdInfraLarge { get; set; } = 5;

    /// <summary>
    /// BR-M003: Worker count threshold for large cluster (5 masters vs 3)
    /// </summary>
    public int LargeClusterWorkerThreshold { get; set; } = 100;

    /// <summary>
    /// Computed system reserve factor (0.85 for 15% reserve)
    /// </summary>
    public double SystemReserveFactor => 1 - (SystemReservePercent / 100);
}

/// <summary>
/// Environment default settings for replicas and headroom
/// </summary>
public class EnvironmentDefaultSettings
{
    /// <summary>
    /// Default replica counts per environment
    /// </summary>
    public ReplicaDefaults Replicas { get; set; } = new();

    /// <summary>
    /// Default headroom percentages per environment
    /// </summary>
    public HeadroomDefaults Headroom { get; set; } = new();

    /// <summary>
    /// Default overcommit ratios
    /// </summary>
    public OvercommitDefaults Overcommit { get; set; } = new();
}

/// <summary>
/// Default replica counts per environment
/// BR-R001, BR-R002, BR-R003
/// </summary>
public class ReplicaDefaults
{
    public int Dev { get; set; } = 1;
    public int Test { get; set; } = 1;
    public int Stage { get; set; } = 2;
    public int Prod { get; set; } = 3;
    public int DR { get; set; } = 3;
}

/// <summary>
/// Default headroom percentages per environment
/// BR-H003 through BR-H007
/// </summary>
public class HeadroomDefaults
{
    public double Dev { get; set; } = 33;
    public double Test { get; set; } = 33;
    public double Stage { get; set; } = 0;
    public double Prod { get; set; } = 37.5;
    public double DR { get; set; } = 37.5;
}

/// <summary>
/// Default overcommit ratios for Prod and NonProd
/// </summary>
public class OvercommitDefaults
{
    public double ProdCpu { get; set; } = 1.0;
    public double ProdMemory { get; set; } = 1.0;
    public double NonProdCpu { get; set; } = 1.0;
    public double NonProdMemory { get; set; } = 1.0;
}

/// <summary>
/// HA pattern multiplier settings
/// </summary>
public class HAPatternSettings
{
    public double None { get; set; } = 1.0;
    public double ActiveActive { get; set; } = 2.0;
    public double ActivePassive { get; set; } = 2.0;
    public double NPlus1 { get; set; } = 1.5;
    public double NPlus2 { get; set; } = 1.67;

    public double GetMultiplier(HAPattern pattern)
    {
        return pattern switch
        {
            HAPattern.None => None,
            HAPattern.ActiveActive => ActiveActive,
            HAPattern.ActivePassive => ActivePassive,
            HAPattern.NPlus1 => NPlus1,
            HAPattern.NPlus2 => NPlus2,
            _ => 1.0
        };
    }
}

/// <summary>
/// Load balancer specification settings
/// </summary>
public class LoadBalancerSettings
{
    /// <summary>
    /// Single load balancer specs (VMs, CPU per VM, RAM per VM)
    /// </summary>
    public LBSpecs Single { get; set; } = new(1, 2, 4);

    /// <summary>
    /// HA pair load balancer specs (VMs, CPU per VM, RAM per VM)
    /// </summary>
    public LBSpecs HAPair { get; set; } = new(2, 2, 4);

    public (int VMs, int CpuPerVM, int RamPerVM) GetSpecs(LoadBalancerOption option)
    {
        return option switch
        {
            LoadBalancerOption.None => (0, 0, 0),
            LoadBalancerOption.Single => (Single.VMs, Single.CpuPerVM, Single.RamPerVM),
            LoadBalancerOption.HAPair => (HAPair.VMs, HAPair.CpuPerVM, HAPair.RamPerVM),
            LoadBalancerOption.CloudLB => (0, 0, 0),
            _ => (0, 0, 0)
        };
    }
}

/// <summary>
/// Load balancer specifications
/// </summary>
public class LBSpecs
{
    public int VMs { get; set; }
    public int CpuPerVM { get; set; }
    public int RamPerVM { get; set; }

    public LBSpecs() { }

    public LBSpecs(int vms, int cpuPerVM, int ramPerVM)
    {
        VMs = vms;
        CpuPerVM = cpuPerVM;
        RamPerVM = ramPerVM;
    }
}

/// <summary>
/// VM role base specifications settings
/// </summary>
public class VMRoleSettings
{
    /// <summary>
    /// Web server specs per tier
    /// </summary>
    public Dictionary<AppTier, RoleSpecs> Web { get; set; } = new()
    {
        [AppTier.Small] = new(2, 4),
        [AppTier.Medium] = new(4, 8),
        [AppTier.Large] = new(8, 16),
        [AppTier.XLarge] = new(16, 32)
    };

    /// <summary>
    /// Application server specs per tier
    /// </summary>
    public Dictionary<AppTier, RoleSpecs> App { get; set; } = new()
    {
        [AppTier.Small] = new(2, 4),
        [AppTier.Medium] = new(4, 8),
        [AppTier.Large] = new(8, 16),
        [AppTier.XLarge] = new(16, 32)
    };

    /// <summary>
    /// Database server specs per tier
    /// </summary>
    public Dictionary<AppTier, RoleSpecs> Database { get; set; } = new()
    {
        [AppTier.Small] = new(4, 16),
        [AppTier.Medium] = new(8, 32),
        [AppTier.Large] = new(16, 64),
        [AppTier.XLarge] = new(32, 128)
    };

    /// <summary>
    /// Cache server specs per tier
    /// </summary>
    public Dictionary<AppTier, RoleSpecs> Cache { get; set; } = new()
    {
        [AppTier.Small] = new(2, 8),
        [AppTier.Medium] = new(4, 16),
        [AppTier.Large] = new(8, 32),
        [AppTier.XLarge] = new(16, 64)
    };

    /// <summary>
    /// Message queue server specs per tier
    /// </summary>
    public Dictionary<AppTier, RoleSpecs> MessageQueue { get; set; } = new()
    {
        [AppTier.Small] = new(2, 4),
        [AppTier.Medium] = new(4, 8),
        [AppTier.Large] = new(8, 16),
        [AppTier.XLarge] = new(16, 32)
    };

    /// <summary>
    /// Search server specs per tier
    /// </summary>
    public Dictionary<AppTier, RoleSpecs> Search { get; set; } = new()
    {
        [AppTier.Small] = new(4, 16),
        [AppTier.Medium] = new(8, 32),
        [AppTier.Large] = new(16, 64),
        [AppTier.XLarge] = new(32, 128)
    };

    /// <summary>
    /// Storage server specs per tier
    /// </summary>
    public Dictionary<AppTier, RoleSpecs> Storage { get; set; } = new()
    {
        [AppTier.Small] = new(2, 4),
        [AppTier.Medium] = new(4, 8),
        [AppTier.Large] = new(8, 16),
        [AppTier.XLarge] = new(16, 32)
    };

    /// <summary>
    /// Monitoring server specs per tier
    /// </summary>
    public Dictionary<AppTier, RoleSpecs> Monitoring { get; set; } = new()
    {
        [AppTier.Small] = new(2, 4),
        [AppTier.Medium] = new(4, 8),
        [AppTier.Large] = new(8, 16),
        [AppTier.XLarge] = new(16, 32)
    };

    /// <summary>
    /// Bastion host specs (fixed size)
    /// </summary>
    public RoleSpecs Bastion { get; set; } = new(2, 4);

    /// <summary>
    /// Memory multiplier for Java, Mendix, OutSystems
    /// </summary>
    public double HighMemoryMultiplier { get; set; } = 1.5;

    public (int Cpu, int Ram) GetSpecs(ServerRole role, AppTier size)
    {
        var specs = role switch
        {
            ServerRole.Web => Web.GetValueOrDefault(size, new(4, 8)),
            ServerRole.App => App.GetValueOrDefault(size, new(4, 8)),
            ServerRole.Database => Database.GetValueOrDefault(size, new(8, 32)),
            ServerRole.Cache => Cache.GetValueOrDefault(size, new(4, 16)),
            ServerRole.MessageQueue => MessageQueue.GetValueOrDefault(size, new(4, 8)),
            ServerRole.Search => Search.GetValueOrDefault(size, new(8, 32)),
            ServerRole.Storage => Storage.GetValueOrDefault(size, new(4, 8)),
            ServerRole.Monitoring => Monitoring.GetValueOrDefault(size, new(4, 8)),
            ServerRole.Bastion => Bastion,
            _ => new(4, 8)
        };
        return (specs.Cpu, specs.Ram);
    }
}

/// <summary>
/// Role specifications (CPU and RAM)
/// </summary>
public class RoleSpecs
{
    public int Cpu { get; set; }
    public int Ram { get; set; }

    public RoleSpecs() { }

    public RoleSpecs(int cpu, int ram)
    {
        Cpu = cpu;
        Ram = ram;
    }
}
