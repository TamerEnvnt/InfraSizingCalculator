namespace InfraSizingCalculator.Models.Enums;

/// <summary>
/// Control plane high availability options for K8s clusters.
/// For managed K8s (EKS, AKS, GKE), control plane HA is automatic.
/// For self-managed distributions, this determines control plane redundancy.
/// </summary>
public enum K8sControlPlaneHA
{
    /// <summary>Managed by cloud provider (EKS, AKS, GKE) - automatic HA</summary>
    Managed,

    /// <summary>Single control plane node - no HA (dev/test only)</summary>
    Single,

    /// <summary>Stacked HA - 3+ control planes with co-located etcd</summary>
    StackedHA,

    /// <summary>External etcd - 3+ control planes with separate etcd cluster</summary>
    ExternalEtcd
}
