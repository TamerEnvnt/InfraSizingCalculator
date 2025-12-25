using InfraSizingCalculator.Models.Enums;

namespace InfraSizingCalculator.Models;

/// <summary>
/// Result of VM sizing calculation
/// </summary>
public class VMSizingResult
{
    /// <summary>
    /// Per-environment results
    /// </summary>
    public List<VMEnvironmentResult> Environments { get; set; } = new();

    /// <summary>
    /// Grand totals across all environments
    /// </summary>
    public VMGrandTotal GrandTotal { get; set; } = new();

    /// <summary>
    /// Input configuration that produced this result
    /// </summary>
    public VMSizingInput? Configuration { get; set; }

    /// <summary>
    /// Technology name
    /// </summary>
    public string TechnologyName { get; set; } = string.Empty;

    /// <summary>
    /// When the calculation was performed
    /// </summary>
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// VM sizing result for a single environment
/// </summary>
public class VMEnvironmentResult
{
    /// <summary>
    /// Environment type
    /// </summary>
    public EnvironmentType Environment { get; set; }

    /// <summary>
    /// Environment display name
    /// </summary>
    public string EnvironmentName { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is a production-type environment
    /// </summary>
    public bool IsProd { get; set; }

    /// <summary>
    /// HA pattern used
    /// </summary>
    public HAPattern HAPattern { get; set; }

    /// <summary>
    /// DR pattern used
    /// </summary>
    public DRPattern DRPattern { get; set; }

    /// <summary>
    /// Load balancer option
    /// </summary>
    public LoadBalancerOption LoadBalancer { get; set; }

    /// <summary>
    /// Per-role breakdown
    /// </summary>
    public List<VMRoleResult> Roles { get; set; } = new();

    /// <summary>
    /// Total VMs in this environment
    /// </summary>
    public int TotalVMs { get; set; }

    /// <summary>
    /// Total vCPU in this environment
    /// </summary>
    public int TotalCpu { get; set; }

    /// <summary>
    /// Total RAM in GB in this environment
    /// </summary>
    public int TotalRam { get; set; }

    /// <summary>
    /// Total disk in GB in this environment
    /// </summary>
    public int TotalDisk { get; set; }

    /// <summary>
    /// Load balancer VMs (if applicable)
    /// </summary>
    public int LoadBalancerVMs { get; set; }

    /// <summary>
    /// Load balancer CPU
    /// </summary>
    public int LoadBalancerCpu { get; set; }

    /// <summary>
    /// Load balancer RAM
    /// </summary>
    public int LoadBalancerRam { get; set; }
}

/// <summary>
/// Result for a specific server role
/// </summary>
public class VMRoleResult
{
    /// <summary>
    /// Role type
    /// </summary>
    public ServerRole Role { get; set; }

    /// <summary>
    /// Role display name
    /// </summary>
    public string RoleName { get; set; } = string.Empty;

    /// <summary>
    /// Size tier
    /// </summary>
    public AppTier Size { get; set; }

    /// <summary>
    /// Base instance count (before HA)
    /// </summary>
    public int BaseInstances { get; set; }

    /// <summary>
    /// Total instances (after HA multiplier)
    /// </summary>
    public int TotalInstances { get; set; }

    /// <summary>
    /// CPU per instance
    /// </summary>
    public int CpuPerInstance { get; set; }

    /// <summary>
    /// RAM per instance in GB
    /// </summary>
    public int RamPerInstance { get; set; }

    /// <summary>
    /// Disk per instance in GB
    /// </summary>
    public int DiskPerInstance { get; set; }

    /// <summary>
    /// Total CPU for this role
    /// </summary>
    public int TotalCpu { get; set; }

    /// <summary>
    /// Total RAM for this role in GB
    /// </summary>
    public int TotalRam { get; set; }

    /// <summary>
    /// Total disk for this role in GB
    /// </summary>
    public int TotalDisk { get; set; }
}

/// <summary>
/// Grand totals for VM sizing
/// </summary>
public class VMGrandTotal
{
    /// <summary>
    /// Total VMs across all environments
    /// </summary>
    public int TotalVMs { get; set; }

    /// <summary>
    /// Total vCPU across all environments
    /// </summary>
    public int TotalCpu { get; set; }

    /// <summary>
    /// Total RAM in GB across all environments
    /// </summary>
    public int TotalRam { get; set; }

    /// <summary>
    /// Total disk in GB across all environments
    /// </summary>
    public int TotalDisk { get; set; }

    /// <summary>
    /// Total load balancer VMs
    /// </summary>
    public int TotalLoadBalancerVMs { get; set; }
}
