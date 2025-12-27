namespace InfraSizingCalculator.Models.Enums;

/// <summary>
/// Backup strategy options for Kubernetes clusters and workloads.
/// </summary>
public enum K8sBackupStrategy
{
    /// <summary>No automated backup configured</summary>
    None,

    /// <summary>Velero - Open source, CNCF project, broad compatibility</summary>
    Velero,

    /// <summary>Kasten K10 - Enterprise-grade, application-centric backup</summary>
    Kasten,

    /// <summary>Portworx PX-Backup - Storage-integrated backup solution</summary>
    Portworx,

    /// <summary>Cloud Native - AWS Backup, Azure Backup, GCP Backup for GKE</summary>
    CloudNative,

    /// <summary>Custom - Organization-specific backup tooling</summary>
    Custom
}
