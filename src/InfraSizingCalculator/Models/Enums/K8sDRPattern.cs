namespace InfraSizingCalculator.Models.Enums;

/// <summary>
/// Disaster recovery patterns for Kubernetes clusters.
/// Defines how clusters handle regional failures.
/// </summary>
public enum K8sDRPattern
{
    /// <summary>No DR - Single cluster, rely on multi-AZ for availability</summary>
    None,

    /// <summary>Backup/Restore - Regular backups with Velero/Kasten, manual restore on failure</summary>
    BackupRestore,

    /// <summary>Warm Standby - Standby cluster with minimal resources, scales up on failover</summary>
    WarmStandby,

    /// <summary>Hot Standby - Fully provisioned standby cluster, quick failover (RTO < 15min)</summary>
    HotStandby,

    /// <summary>Active-Active - Multiple clusters serving traffic simultaneously with global load balancing</summary>
    ActiveActive
}
