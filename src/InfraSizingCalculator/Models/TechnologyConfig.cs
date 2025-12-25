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
}
