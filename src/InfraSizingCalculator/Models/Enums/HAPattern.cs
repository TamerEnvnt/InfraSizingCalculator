namespace InfraSizingCalculator.Models.Enums;

/// <summary>
/// High Availability patterns for VM deployments
/// </summary>
public enum HAPattern
{
    /// <summary>No HA - Single instance</summary>
    None,

    /// <summary>Active-Active - All instances serve traffic</summary>
    ActiveActive,

    /// <summary>Active-Passive - Standby instance takes over on failure</summary>
    ActivePassive,

    /// <summary>N+1 - One spare instance for any failure</summary>
    NPlus1,

    /// <summary>N+2 - Two spare instances for multiple failures</summary>
    NPlus2
}
