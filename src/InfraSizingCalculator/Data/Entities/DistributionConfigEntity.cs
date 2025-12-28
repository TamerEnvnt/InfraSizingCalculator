using InfraSizingCalculator.Models.Enums;

namespace InfraSizingCalculator.Data.Entities;

/// <summary>
/// Database entity for Kubernetes distribution configurations.
/// Stores all distribution-specific node specifications and metadata.
/// BR-D001 through BR-D008
/// </summary>
public class DistributionConfigEntity
{
    public int Id { get; set; }

    /// <summary>
    /// Distribution enum value stored as string for readability
    /// </summary>
    public string DistributionKey { get; set; } = string.Empty;

    /// <summary>
    /// Display name for the distribution
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Vendor/provider name
    /// </summary>
    public string Vendor { get; set; } = string.Empty;

    /// <summary>
    /// Icon identifier for UI
    /// </summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// Brand color in hex format
    /// </summary>
    public string BrandColor { get; set; } = "#326CE5";

    /// <summary>
    /// Comma-separated tags (e.g., "cloud,managed,enterprise")
    /// </summary>
    public string Tags { get; set; } = string.Empty;

    // Production Control Plane Node Specs
    public int ProdControlPlaneCpu { get; set; }
    public int ProdControlPlaneRam { get; set; }
    public int ProdControlPlaneDisk { get; set; }

    // Non-Production Control Plane Node Specs
    public int NonProdControlPlaneCpu { get; set; }
    public int NonProdControlPlaneRam { get; set; }
    public int NonProdControlPlaneDisk { get; set; }

    // Production Worker Node Specs
    public int ProdWorkerCpu { get; set; }
    public int ProdWorkerRam { get; set; }
    public int ProdWorkerDisk { get; set; }

    // Non-Production Worker Node Specs
    public int NonProdWorkerCpu { get; set; }
    public int NonProdWorkerRam { get; set; }
    public int NonProdWorkerDisk { get; set; }

    // Production Infrastructure Node Specs (OpenShift only)
    public int ProdInfraCpu { get; set; }
    public int ProdInfraRam { get; set; }
    public int ProdInfraDisk { get; set; }

    // Non-Production Infrastructure Node Specs (OpenShift only)
    public int NonProdInfraCpu { get; set; }
    public int NonProdInfraRam { get; set; }
    public int NonProdInfraDisk { get; set; }

    /// <summary>
    /// BR-D007: Only OpenShift distributions have infrastructure nodes
    /// </summary>
    public bool HasInfraNodes { get; set; }

    /// <summary>
    /// BR-D006: EKS, AKS, GKE have managed control planes
    /// </summary>
    public bool HasManagedControlPlane { get; set; }

    /// <summary>
    /// Sort order for display in UI
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Whether this distribution is currently active/visible
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
