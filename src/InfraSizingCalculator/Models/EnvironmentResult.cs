using InfraSizingCalculator.Models.Enums;

namespace InfraSizingCalculator.Models;

/// <summary>
/// Per-environment calculation result
/// </summary>
public class EnvironmentResult
{
    public EnvironmentType Environment { get; init; }
    public string EnvironmentName { get; init; } = string.Empty;

    /// <summary>
    /// BR-E003: Production and DR are Production type
    /// </summary>
    public bool IsProd { get; init; }

    public int Apps { get; init; }
    public int Replicas { get; init; }

    /// <summary>
    /// BR-R005: Total pods = Number of applications × Replica count
    /// </summary>
    public int Pods { get; init; }

    /// <summary>
    /// Control plane / Master nodes
    /// </summary>
    public int Masters { get; init; }

    /// <summary>
    /// Infrastructure nodes (OpenShift only per BR-I001)
    /// </summary>
    public int Infra { get; init; }

    /// <summary>
    /// Worker nodes
    /// </summary>
    public int Workers { get; init; }

    /// <summary>
    /// External etcd nodes (when using ExternalEtcd control plane HA pattern)
    /// </summary>
    public int EtcdNodes { get; init; }

    /// <summary>
    /// DR site nodes (calculated based on DR pattern: WarmStandby, HotStandby, ActiveActive)
    /// </summary>
    public int DRNodes { get; init; }

    /// <summary>
    /// Cost multiplier from DR configuration (1.0 = no DR, 2.0 = full active-active)
    /// </summary>
    public double DRCostMultiplier { get; init; } = 1.0;

    /// <summary>
    /// Number of availability zones workers are distributed across
    /// </summary>
    public int AvailabilityZones { get; init; } = 1;

    /// <summary>
    /// BR-RC005: Total nodes = masters + infra + workers + etcd
    /// </summary>
    public int TotalNodes { get; init; }

    /// <summary>
    /// BR-RC006: Total vCPU
    /// </summary>
    public int TotalCpu { get; init; }

    /// <summary>
    /// BR-RC007: Total RAM in GB
    /// </summary>
    public int TotalRam { get; init; }

    /// <summary>
    /// Total Disk in GB
    /// </summary>
    public int TotalDisk { get; init; }
}
