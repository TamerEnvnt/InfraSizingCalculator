using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Models.Pricing;

namespace InfraSizingCalculator.Models;

/// <summary>
/// High Availability and Disaster Recovery configuration for K8s clusters.
/// Contains settings for control plane HA, node distribution, and DR strategies.
/// </summary>
public class K8sHADRConfig
{
    /// <summary>
    /// Control plane high availability mode.
    /// For managed K8s (EKS, AKS, GKE), this is automatically "Managed".
    /// For self-managed distributions, specifies the control plane redundancy.
    /// </summary>
    public K8sControlPlaneHA ControlPlaneHA { get; set; } = K8sControlPlaneHA.Managed;

    /// <summary>
    /// Number of control plane nodes (for self-managed clusters).
    /// Only applies when ControlPlaneHA is StackedHA or ExternalEtcd.
    /// </summary>
    public int ControlPlaneNodes { get; set; } = 3;

    /// <summary>
    /// Worker node distribution across availability zones.
    /// </summary>
    public K8sNodeDistribution NodeDistribution { get; set; } = K8sNodeDistribution.MultiAZ;

    /// <summary>
    /// Number of availability zones to use.
    /// </summary>
    public int AvailabilityZones { get; set; } = 3;

    /// <summary>
    /// Disaster recovery pattern for the cluster.
    /// </summary>
    public K8sDRPattern DRPattern { get; set; } = K8sDRPattern.None;

    /// <summary>
    /// Backup strategy for workloads and cluster state.
    /// </summary>
    public K8sBackupStrategy BackupStrategy { get; set; } = K8sBackupStrategy.None;

    /// <summary>
    /// Backup frequency in hours (0 = no scheduled backup).
    /// </summary>
    public int BackupFrequencyHours { get; set; } = 24;

    /// <summary>
    /// Backup retention period in days.
    /// </summary>
    public int BackupRetentionDays { get; set; } = 30;

    /// <summary>
    /// Whether to enable pod disruption budgets for workloads.
    /// </summary>
    public bool EnablePodDisruptionBudgets { get; set; } = true;

    /// <summary>
    /// Minimum replicas for critical workloads during disruptions.
    /// </summary>
    public int MinAvailableReplicas { get; set; } = 1;

    /// <summary>
    /// Whether to use pod topology spread constraints.
    /// Ensures pods are spread across zones/nodes.
    /// </summary>
    public bool EnableTopologySpread { get; set; } = true;

    /// <summary>
    /// For DR patterns with standby clusters, the target region for DR.
    /// </summary>
    public string? DRRegion { get; set; }

    /// <summary>
    /// Target Recovery Time Objective in minutes.
    /// </summary>
    public int? RTOMinutes { get; set; }

    /// <summary>
    /// Target Recovery Point Objective in minutes.
    /// </summary>
    public int? RPOMinutes { get; set; }

    /// <summary>
    /// Additional cost multiplier for HA/DR configuration.
    /// Calculated based on settings and industry-standard pricing research.
    ///
    /// PRICING SOURCES (researched Dec 2024):
    /// - Multi-AZ: AWS $0.01/GB cross-AZ, Azure FREE, GCP $0.01/GB (avg ~2-3% overhead)
    /// - Multi-Region: AWS $0.02-0.09/GB, Azure $0.05/GB, GCP $0.05-0.12/GB (avg ~20% overhead)
    /// - DR patterns: Industry standard ratios from AWS Well-Architected, Azure DR docs
    /// - Backup costs: Velero free, Kasten K10 ~$225/node/yr, Portworx ~$2100/node/yr
    /// </summary>
    public decimal GetCostMultiplier()
    {
        decimal multiplier = 1.0m;

        // Control plane HA: Self-managed clusters need additional master nodes
        // Each control plane node adds ~10-15% base compute cost
        // External etcd adds dedicated etcd cluster overhead
        if (ControlPlaneHA == K8sControlPlaneHA.StackedHA)
        {
            // Stacked HA: etcd runs on control plane nodes
            // 3 nodes = +0.2, 5 nodes = +0.4 (additional masters over single)
            multiplier += 0.10m * (ControlPlaneNodes - 1);
        }
        else if (ControlPlaneHA == K8sControlPlaneHA.ExternalEtcd)
        {
            // External etcd: Separate etcd cluster (3-7 nodes) + control plane nodes
            // Higher overhead due to dedicated etcd infrastructure
            multiplier += 0.12m * (ControlPlaneNodes - 1) + 0.15m; // +15% for etcd cluster
        }

        // Cross-AZ/Region data transfer costs
        // Source: AWS Direct Connect pricing, Azure VNet pricing, GCP Cloud Interconnect
        // AWS/GCP: $0.01/GB cross-AZ ($0.02 round-trip), typical workload ~15-25% of compute
        // Azure: FREE within region (huge advantage for multi-AZ)
        // Using weighted average (assuming typical cross-AZ traffic patterns)
        if (NodeDistribution == K8sNodeDistribution.DualAZ)
        {
            multiplier += 0.02m; // ~2% overhead for 2 AZs
        }
        else if (NodeDistribution == K8sNodeDistribution.MultiAZ)
        {
            multiplier += 0.03m; // ~3% overhead for 3+ AZs (more traffic distribution)
        }
        else if (NodeDistribution == K8sNodeDistribution.MultiRegion)
        {
            // Multi-region: Significantly higher costs
            // Inter-region transfer: AWS $0.02-0.09/GB, GCP $0.05-0.12/GB
            // Plus increased latency considerations
            multiplier += 0.20m; // ~20% overhead for multi-region
        }

        // DR patterns - Based on AWS Well-Architected DR pillar
        // and industry-standard DR cost ratios
        // Source: https://aws.amazon.com/blogs/architecture/disaster-recovery-dr-architecture-on-aws-part-i/
        switch (DRPattern)
        {
            case K8sDRPattern.BackupRestore:
                // Backup/Restore: Only storage costs (S3/Blob/GCS + backup tool)
                // Velero: Free (Apache 2.0), <2% resource overhead
                // Enterprise (Kasten K10): ~$225/node/year
                // Typical backup storage: 5-10% of primary compute cost
                multiplier += 0.08m; // 8% for backup infrastructure + storage
                break;

            case K8sDRPattern.WarmStandby:
                // Warm Standby: Minimal running infrastructure (scaled down)
                // Typical: 10-30% of production capacity running
                // Plus data replication costs
                // Industry standard: 30-50% of primary cost
                multiplier += 0.40m; // 40% (mid-range of 30-50%)
                break;

            case K8sDRPattern.HotStandby:
                // Hot Standby: Near-full capacity ready to take over
                // Typical: 80-100% capacity, just not serving traffic
                // Plus continuous data replication
                // Industry standard: 80-100% of primary cost
                multiplier += 0.90m; // 90% (near-full duplication)
                break;

            case K8sDRPattern.ActiveActive:
                // Active-Active: Full capacity in both regions, serving traffic
                // 100% duplication + global load balancer + increased data sync
                // Industry standard: 100-120% of primary cost
                // AWS Global Accelerator: ~$0.025/GB + $0.015/hr per accelerator
                multiplier += 1.10m; // 110% (full duplication + GLB overhead)
                break;
        }

        // Backup strategy additional costs (if not covered by DR pattern)
        if (DRPattern == K8sDRPattern.None && BackupStrategy != K8sBackupStrategy.None)
        {
            multiplier += BackupStrategy switch
            {
                K8sBackupStrategy.Velero => 0.02m,      // Free tool, minimal storage
                K8sBackupStrategy.Kasten => 0.05m,      // ~$225/node/yr enterprise (Kasten K10)
                K8sBackupStrategy.Portworx => 0.08m,    // ~$2100/node/yr PX-Backup
                K8sBackupStrategy.CloudNative => 0.03m, // Cloud provider snapshots
                _ => 0.0m
            };
        }

        return multiplier;
    }

    /// <summary>
    /// Provider-aware cost multiplier calculation.
    /// Takes into account cloud-specific pricing differences:
    /// - Azure: FREE cross-AZ data transfer (significant cost savings)
    /// - AWS/GCP: $0.01/GB cross-AZ each direction
    /// - On-Prem: No cloud data transfer costs
    /// </summary>
    /// <param name="distribution">The K8s distribution to determine cloud provider</param>
    public decimal GetCostMultiplier(Distribution distribution)
    {
        var provider = distribution.GetCloudProvider();
        decimal multiplier = 1.0m;

        // Control plane HA costs (same across providers)
        if (ControlPlaneHA == K8sControlPlaneHA.StackedHA)
        {
            multiplier += 0.10m * (ControlPlaneNodes - 1);
        }
        else if (ControlPlaneHA == K8sControlPlaneHA.ExternalEtcd)
        {
            multiplier += 0.12m * (ControlPlaneNodes - 1) + 0.15m;
        }

        // Provider-specific cross-AZ costs
        if (NodeDistribution != K8sNodeDistribution.SingleAZ)
        {
            multiplier += provider.GetCrossAZCostMultiplier(AvailabilityZones);

            // Multi-region still adds significant cost on all providers
            if (NodeDistribution == K8sNodeDistribution.MultiRegion)
            {
                // Additional inter-region transfer costs
                multiplier += 0.17m; // ~17% on top of cross-AZ
            }
        }

        // DR patterns (consistent across providers)
        switch (DRPattern)
        {
            case K8sDRPattern.BackupRestore:
                multiplier += 0.08m;
                break;
            case K8sDRPattern.WarmStandby:
                multiplier += 0.40m;
                break;
            case K8sDRPattern.HotStandby:
                multiplier += 0.90m;
                break;
            case K8sDRPattern.ActiveActive:
                multiplier += 1.10m;
                break;
        }

        // Backup strategy (if not covered by DR)
        if (DRPattern == K8sDRPattern.None && BackupStrategy != K8sBackupStrategy.None)
        {
            multiplier += BackupStrategy switch
            {
                K8sBackupStrategy.Velero => 0.02m,
                K8sBackupStrategy.Kasten => 0.05m,
                K8sBackupStrategy.Portworx => 0.08m,
                K8sBackupStrategy.CloudNative => 0.03m,
                _ => 0.0m
            };
        }

        return multiplier;
    }

    /// <summary>
    /// Get a summary description of the HA/DR configuration.
    /// </summary>
    public string GetSummary()
    {
        var parts = new List<string>();

        if (ControlPlaneHA != K8sControlPlaneHA.Managed && ControlPlaneHA != K8sControlPlaneHA.Single)
            parts.Add($"CP HA ({ControlPlaneNodes} nodes)");

        if (NodeDistribution != K8sNodeDistribution.SingleAZ)
            parts.Add($"{AvailabilityZones} AZs");

        if (DRPattern != K8sDRPattern.None)
            parts.Add(DRPattern.ToString());

        if (BackupStrategy != K8sBackupStrategy.None)
            parts.Add($"Backup ({BackupStrategy})");

        return parts.Count > 0 ? string.Join(" • ", parts) : "Basic (no HA/DR)";
    }
}
