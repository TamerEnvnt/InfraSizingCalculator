namespace InfraSizingCalculator.Models.Enums;

/// <summary>
/// Node distribution strategy across availability zones.
/// Determines how worker nodes are spread for high availability.
/// </summary>
public enum K8sNodeDistribution
{
    /// <summary>Single AZ - All nodes in one availability zone (lowest cost, no AZ redundancy)</summary>
    SingleAZ,

    /// <summary>Multi-AZ (2) - Nodes spread across 2 availability zones</summary>
    DualAZ,

    /// <summary>Multi-AZ (3+) - Nodes spread across 3 or more availability zones (recommended for prod)</summary>
    MultiAZ,

    /// <summary>Region-spanning - Nodes across multiple regions (highest availability, highest cost)</summary>
    MultiRegion
}
