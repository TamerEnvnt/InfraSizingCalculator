namespace InfraSizingCalculator.Models.Enums;

/// <summary>
/// Disaster Recovery patterns for VM deployments
/// </summary>
public enum DRPattern
{
    /// <summary>No DR - Single site only</summary>
    None,

    /// <summary>Pilot Light - Minimal DR footprint with core services only, scales up on failover</summary>
    PilotLight,

    /// <summary>Warm Standby - DR site with minimal resources, scales up on failover</summary>
    WarmStandby,

    /// <summary>Hot Standby - DR site fully provisioned, ready for immediate failover</summary>
    HotStandby,

    /// <summary>Multi-Region Active-Active - Multiple regions serving traffic simultaneously</summary>
    MultiRegion
}
