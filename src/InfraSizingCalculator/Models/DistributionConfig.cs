using InfraSizingCalculator.Models.Enums;

namespace InfraSizingCalculator.Models;

/// <summary>
/// Distribution-specific node specifications
/// BR-D001: Each distribution has its own default node specifications
/// BR-D006: EKS, AKS, GKE have managed control planes
/// BR-D007: Only OpenShift has infrastructure nodes
/// </summary>
public class DistributionConfig
{
    public required Distribution Distribution { get; init; }
    public required string Name { get; init; }
    public required string Vendor { get; init; }
    public required NodeSpecs ProdControlPlane { get; init; }
    public required NodeSpecs NonProdControlPlane { get; init; }
    public required NodeSpecs ProdWorker { get; init; }
    public required NodeSpecs NonProdWorker { get; init; }
    public NodeSpecs ProdInfra { get; init; } = NodeSpecs.Zero;
    public NodeSpecs NonProdInfra { get; init; } = NodeSpecs.Zero;
    public bool HasInfraNodes { get; init; }
    public bool HasManagedControlPlane { get; init; }
    public string[] Tags { get; init; } = Array.Empty<string>();
    public string Icon { get; init; } = string.Empty;
    public string BrandColor { get; init; } = "#326CE5";

    /// <summary>
    /// Per-environment control plane specs (for multi-cluster mode)
    /// When set, overrides ProdControlPlane/NonProdControlPlane based on environment
    /// </summary>
    public Dictionary<EnvironmentType, NodeSpecs>? PerEnvControlPlane { get; init; }

    /// <summary>
    /// Per-environment worker specs (for multi-cluster mode)
    /// When set, overrides ProdWorker/NonProdWorker based on environment
    /// </summary>
    public Dictionary<EnvironmentType, NodeSpecs>? PerEnvWorker { get; init; }

    /// <summary>
    /// Per-environment infra specs (for multi-cluster mode)
    /// When set, overrides ProdInfra/NonProdInfra based on environment
    /// </summary>
    public Dictionary<EnvironmentType, NodeSpecs>? PerEnvInfra { get; init; }

    /// <summary>
    /// Get control plane specs for a specific environment.
    /// Uses per-env specs if available, otherwise falls back to Prod/NonProd distinction.
    /// </summary>
    public NodeSpecs GetControlPlaneForEnv(EnvironmentType env)
    {
        if (PerEnvControlPlane?.TryGetValue(env, out var specs) == true)
            return specs;

        // Fallback to Prod/NonProd distinction
        return IsProdLike(env) ? ProdControlPlane : NonProdControlPlane;
    }

    /// <summary>
    /// Get worker specs for a specific environment.
    /// Uses per-env specs if available, otherwise falls back to Prod/NonProd distinction.
    /// </summary>
    public NodeSpecs GetWorkerForEnv(EnvironmentType env)
    {
        if (PerEnvWorker?.TryGetValue(env, out var specs) == true)
            return specs;

        // Fallback to Prod/NonProd distinction
        return IsProdLike(env) ? ProdWorker : NonProdWorker;
    }

    /// <summary>
    /// Get infra specs for a specific environment.
    /// Uses per-env specs if available, otherwise falls back to Prod/NonProd distinction.
    /// </summary>
    public NodeSpecs GetInfraForEnv(EnvironmentType env)
    {
        if (PerEnvInfra?.TryGetValue(env, out var specs) == true)
            return specs;

        // Fallback to Prod/NonProd distinction
        return IsProdLike(env) ? ProdInfra : NonProdInfra;
    }

    /// <summary>
    /// Determines if an environment should use production-like specs.
    /// Prod and DR are considered production-like.
    /// </summary>
    private static bool IsProdLike(EnvironmentType env) =>
        env == EnvironmentType.Prod || env == EnvironmentType.DR;
}
