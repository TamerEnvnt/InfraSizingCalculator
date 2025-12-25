using InfraSizingCalculator.Models.Enums;

namespace InfraSizingCalculator.Models;

/// <summary>
/// User default settings that persist across sessions via localStorage.
/// </summary>
public class UserDefaults
{
    /// <summary>
    /// Last selected platform type (Native/LowCode).
    /// </summary>
    public PlatformType? DefaultPlatform { get; set; }

    /// <summary>
    /// Last selected deployment model (Kubernetes/VMs).
    /// </summary>
    public DeploymentModel? DefaultDeployment { get; set; }

    /// <summary>
    /// Last selected technology.
    /// </summary>
    public Technology? DefaultTechnology { get; set; }

    /// <summary>
    /// Last selected K8s distribution.
    /// </summary>
    public Distribution? DefaultDistribution { get; set; }

    /// <summary>
    /// Default cluster mode preference.
    /// </summary>
    public ClusterMode DefaultClusterMode { get; set; } = ClusterMode.MultiCluster;

    /// <summary>
    /// Default replica settings.
    /// </summary>
    public ReplicaSettings DefaultReplicas { get; set; } = new();

    /// <summary>
    /// Default headroom settings.
    /// </summary>
    public HeadroomSettings DefaultHeadroom { get; set; } = new();

    /// <summary>
    /// Default production overcommit settings.
    /// </summary>
    public OvercommitSettings DefaultProdOvercommit { get; set; } = new();

    /// <summary>
    /// Default non-production overcommit settings.
    /// </summary>
    public OvercommitSettings DefaultNonProdOvercommit { get; set; } = new();

    /// <summary>
    /// Whether headroom is enabled by default.
    /// </summary>
    public bool HeadroomEnabled { get; set; } = true;

    /// <summary>
    /// User's preferred theme (dark/light).
    /// </summary>
    public string Theme { get; set; } = "dark";

    /// <summary>
    /// When these defaults were last saved.
    /// </summary>
    public DateTime LastSaved { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether to auto-load defaults on startup.
    /// </summary>
    public bool AutoLoadDefaults { get; set; } = true;
}
