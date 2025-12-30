using InfraSizingCalculator.Models.Enums;

namespace InfraSizingCalculator.Models;

/// <summary>
/// Template defining server roles for a specific technology stack.
/// Used to pre-populate VM configurations based on technology selection.
/// </summary>
public class TechnologyRoleTemplate
{
    /// <summary>
    /// The technology this template applies to
    /// </summary>
    public Technology Technology { get; set; }

    /// <summary>
    /// Display name for the template (e.g., ".NET Web Application Stack")
    /// </summary>
    public string TemplateName { get; set; } = string.Empty;

    /// <summary>
    /// Description of what this template provides
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Icon identifier for UI display
    /// </summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is a low-code platform with fixed role structure
    /// </summary>
    public bool IsLowCode { get; set; }

    /// <summary>
    /// Server roles included in this template
    /// </summary>
    public List<VMRoleTemplateItem> Roles { get; set; } = new();

    /// <summary>
    /// Get only the required roles from this template
    /// </summary>
    public IEnumerable<VMRoleTemplateItem> RequiredRoles => Roles.Where(r => r.IsRequired);

    /// <summary>
    /// Get only the optional roles from this template
    /// </summary>
    public IEnumerable<VMRoleTemplateItem> OptionalRoles => Roles.Where(r => !r.IsRequired);
}

/// <summary>
/// Template item representing a single server role configuration.
/// Used as a starting point when building VMRoleConfig instances.
/// </summary>
public class VMRoleTemplateItem
{
    /// <summary>
    /// Generic server role category
    /// </summary>
    public ServerRole Role { get; set; }

    /// <summary>
    /// Technology-specific role identifier (e.g., "os-controller", "mendix-runtime")
    /// </summary>
    public string RoleId { get; set; } = string.Empty;

    /// <summary>
    /// Display name for the role (e.g., "Deployment Controller", "Mendix Runtime Server")
    /// </summary>
    public string RoleName { get; set; } = string.Empty;

    /// <summary>
    /// Short description of what this role does
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Icon identifier for UI display
    /// </summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// Default VM size tier for this role
    /// </summary>
    public AppTier DefaultSize { get; set; } = AppTier.Medium;

    /// <summary>
    /// Default number of instances for non-production environments
    /// </summary>
    public int DefaultInstancesNonProd { get; set; } = 1;

    /// <summary>
    /// Default number of instances for production environments
    /// </summary>
    public int DefaultInstancesProd { get; set; } = 2;

    /// <summary>
    /// Default disk size in GB per instance
    /// </summary>
    public int DefaultDiskGB { get; set; } = 100;

    /// <summary>
    /// Memory multiplier for this role (e.g., 1.5 for Java/Mendix high-memory roles)
    /// </summary>
    public double MemoryMultiplier { get; set; } = 1.0;

    /// <summary>
    /// Whether this role is required for the technology stack
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// Whether this role can be scaled horizontally (multiple instances)
    /// </summary>
    public bool IsScalable { get; set; } = true;

    /// <summary>
    /// Minimum number of instances allowed
    /// </summary>
    public int MinInstances { get; set; } = 1;

    /// <summary>
    /// Maximum number of instances allowed (0 = unlimited)
    /// </summary>
    public int MaxInstances { get; set; } = 0;

    /// <summary>
    /// Creates a VMRoleConfig from this template item
    /// </summary>
    /// <param name="isProd">Whether this is for a production environment</param>
    /// <returns>Configured VMRoleConfig instance</returns>
    public VMRoleConfig ToRoleConfig(bool isProd)
    {
        return new VMRoleConfig
        {
            Role = Role,
            RoleId = RoleId,
            RoleName = RoleName,
            RoleIcon = Icon,
            RoleDescription = Description,
            Size = DefaultSize,
            InstanceCount = isProd ? DefaultInstancesProd : DefaultInstancesNonProd,
            DiskGB = DefaultDiskGB,
            MemoryMultiplier = MemoryMultiplier
        };
    }
}
