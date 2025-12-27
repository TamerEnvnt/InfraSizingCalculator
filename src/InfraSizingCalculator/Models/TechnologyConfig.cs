using InfraSizingCalculator.Models.Enums;

namespace InfraSizingCalculator.Models;

/// <summary>
/// Technology-specific application tier specifications
/// BR-T001: Each technology has specific CPU/RAM requirements per application tier
/// </summary>
public class TechnologyConfig
{
    public required Technology Technology { get; init; }
    public required string Name { get; init; }
    public required Dictionary<AppTier, TierSpecs> Tiers { get; init; }
    public string Icon { get; init; } = string.Empty;
    public string BrandColor { get; init; } = "#58a6ff";
    public string Vendor { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public PlatformType PlatformType { get; init; } = PlatformType.Native;
    public bool IsLowCode => PlatformType == PlatformType.LowCode;

    /// <summary>
    /// VM server role templates for this technology.
    /// Defines the specific server roles needed for VM deployments.
    /// </summary>
    public TechnologyVMRoles? VMRoles { get; init; }

    /// <summary>
    /// Indicates if this technology requires a separate management environment.
    /// Example: OutSystems requires LifeTime as a dedicated environment (not a role).
    /// </summary>
    public bool HasSeparateManagementEnvironment { get; init; } = false;

    /// <summary>
    /// Name of the separate management environment (e.g., "LifeTime" for OutSystems).
    /// </summary>
    public string ManagementEnvironmentName { get; init; } = string.Empty;

    /// <summary>
    /// VM server role templates for the management environment.
    /// This is separate from application environments.
    /// </summary>
    public TechnologyVMRoles? ManagementEnvironmentRoles { get; init; }
}
