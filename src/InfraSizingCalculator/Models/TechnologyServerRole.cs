using InfraSizingCalculator.Models.Enums;

namespace InfraSizingCalculator.Models;

/// <summary>
/// Defines a server role template for a specific technology in VM deployments.
/// Each technology has its own set of required and optional server roles.
/// </summary>
public record TechnologyServerRole
{
    /// <summary>Unique identifier for this role within the technology</summary>
    public required string Id { get; init; }

    /// <summary>Display name for this role</summary>
    public required string Name { get; init; }

    /// <summary>Icon for UI display</summary>
    public string Icon { get; init; } = "⚙️";

    /// <summary>Description of what this server role does</summary>
    public required string Description { get; init; }

    /// <summary>Default tier size for this role</summary>
    public AppTier DefaultSize { get; init; } = AppTier.Medium;

    /// <summary>Default disk size in GB</summary>
    public int DefaultDiskGB { get; init; } = 100;

    /// <summary>Whether this role is required for the technology to function</summary>
    public bool Required { get; init; } = true;

    /// <summary>Whether this role should be scaled horizontally (multiple instances)</summary>
    public bool ScaleHorizontally { get; init; } = false;

    /// <summary>Minimum instance count</summary>
    public int MinInstances { get; init; } = 1;

    /// <summary>Maximum instance count (null = unlimited)</summary>
    public int? MaxInstances { get; init; } = null;

    /// <summary>
    /// Memory multiplier for this role (e.g., database servers typically need more RAM).
    /// Applied on top of tier specs.
    /// </summary>
    public double MemoryMultiplier { get; init; } = 1.0;
}

/// <summary>
/// Collection of server roles for a technology's VM deployment
/// </summary>
public record TechnologyVMRoles
{
    /// <summary>The technology these roles belong to</summary>
    public required Technology Technology { get; init; }

    /// <summary>Display name for the VM deployment type</summary>
    public required string DeploymentName { get; init; }

    /// <summary>Description of this deployment type</summary>
    public required string Description { get; init; }

    /// <summary>The server roles for this technology</summary>
    public required IReadOnlyList<TechnologyServerRole> Roles { get; init; }
}
