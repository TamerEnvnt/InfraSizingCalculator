using System.ComponentModel.DataAnnotations;
using InfraSizingCalculator.Models.Enums;

namespace InfraSizingCalculator.Models;

/// <summary>
/// Input model for VM sizing calculation
/// </summary>
public class VMSizingInput : IValidatableObject
{
    [Required(ErrorMessage = "Technology is required")]
    [EnumDataType(typeof(Technology), ErrorMessage = "Invalid technology")]
    public Technology Technology { get; set; } = Technology.DotNet;

    /// <summary>
    /// Per-environment VM configurations
    /// </summary>
    [Required(ErrorMessage = "Environment configurations are required")]
    public Dictionary<EnvironmentType, VMEnvironmentConfig> EnvironmentConfigs { get; set; } = new();

    /// <summary>
    /// Enabled environments
    /// </summary>
    [Required(ErrorMessage = "Enabled environments is required")]
    public HashSet<EnvironmentType> EnabledEnvironments { get; set; } = new()
    {
        EnvironmentType.Dev,
        EnvironmentType.Test,
        EnvironmentType.Stage,
        EnvironmentType.Prod,
        EnvironmentType.DR
    };

    /// <summary>
    /// System overhead percentage for VMs (e.g., OS, agents)
    /// </summary>
    [Range(0, 50, ErrorMessage = "System overhead must be between 0 and 50%")]
    public double SystemOverheadPercent { get; set; } = 15;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Production must always be enabled
        if (!EnabledEnvironments.Contains(EnvironmentType.Prod))
        {
            yield return new ValidationResult(
                "Production environment must always be enabled",
                new[] { nameof(EnabledEnvironments) });
        }

        // Validate that enabled environments have configurations
        foreach (var env in EnabledEnvironments)
        {
            if (!EnvironmentConfigs.ContainsKey(env))
            {
                yield return new ValidationResult(
                    $"Configuration required for enabled environment: {env}",
                    new[] { nameof(EnvironmentConfigs) });
            }
        }
    }
}

/// <summary>
/// Per-environment VM configuration
/// </summary>
public class VMEnvironmentConfig : IValidatableObject
{
    /// <summary>
    /// Environment type
    /// </summary>
    public EnvironmentType Environment { get; set; }

    /// <summary>
    /// Whether this environment is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Server role configurations for this environment
    /// </summary>
    public List<VMRoleConfig> Roles { get; set; } = new();

    /// <summary>
    /// High availability pattern for this environment
    /// </summary>
    public HAPattern HAPattern { get; set; } = HAPattern.None;

    /// <summary>
    /// Disaster recovery pattern
    /// </summary>
    public DRPattern DRPattern { get; set; } = DRPattern.None;

    /// <summary>
    /// Load balancer option
    /// </summary>
    public LoadBalancerOption LoadBalancer { get; set; } = LoadBalancerOption.None;

    /// <summary>
    /// Total storage in GB for this environment
    /// </summary>
    [Range(0, 1000000, ErrorMessage = "Storage must be between 0 and 1,000,000 GB")]
    public int StorageGB { get; set; } = 100;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Enabled && Roles.Count == 0)
        {
            yield return new ValidationResult(
                "At least one server role is required for enabled environments",
                new[] { nameof(Roles) });
        }
    }
}

/// <summary>
/// Configuration for a specific server role
/// </summary>
public class VMRoleConfig
{
    /// <summary>
    /// Server role type (generic fallback)
    /// </summary>
    public ServerRole Role { get; set; }

    /// <summary>
    /// Technology-specific role ID (e.g., "os-controller", "mendix-app")
    /// </summary>
    public string? RoleId { get; set; }

    /// <summary>
    /// Display name for the role (e.g., "Deployment Controller", "Front-End Server")
    /// </summary>
    public string? RoleName { get; set; }

    /// <summary>
    /// Icon for UI display
    /// </summary>
    public string? RoleIcon { get; set; }

    /// <summary>
    /// Description of what this role does
    /// </summary>
    public string? RoleDescription { get; set; }

    /// <summary>
    /// Memory multiplier for this role type
    /// </summary>
    public double MemoryMultiplier { get; set; } = 1.0;

    /// <summary>
    /// VM size tier
    /// </summary>
    public AppTier Size { get; set; } = AppTier.Medium;

    /// <summary>
    /// Number of instances for this role
    /// </summary>
    [Range(1, 100, ErrorMessage = "Instance count must be between 1 and 100")]
    public int InstanceCount { get; set; } = 1;

    /// <summary>
    /// Custom CPU cores (overrides tier default)
    /// </summary>
    public int? CustomCpu { get; set; }

    /// <summary>
    /// Custom RAM in GB (overrides tier default)
    /// </summary>
    public int? CustomRam { get; set; }

    /// <summary>
    /// Custom disk in GB per instance
    /// </summary>
    [Range(10, 10000, ErrorMessage = "Disk must be between 10 and 10,000 GB")]
    public int DiskGB { get; set; } = 100;
}
