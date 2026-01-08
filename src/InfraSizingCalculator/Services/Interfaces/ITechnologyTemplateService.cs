using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;

namespace InfraSizingCalculator.Services.Interfaces;

/// <summary>
/// Service for managing technology-specific server role templates.
/// Provides pre-defined VM configurations based on technology selection.
/// </summary>
public interface ITechnologyTemplateService
{
    /// <summary>
    /// Gets the role template for a specific technology
    /// </summary>
    /// <param name="technology">The technology to get template for</param>
    /// <returns>The technology role template</returns>
    TechnologyRoleTemplate GetTemplate(Technology technology);

    /// <summary>
    /// Gets all available technology role templates
    /// </summary>
    /// <returns>Collection of all templates</returns>
    IEnumerable<TechnologyRoleTemplate> GetAllTemplates();

    /// <summary>
    /// Gets templates filtered by platform type (Native or LowCode)
    /// </summary>
    /// <param name="platformType">The platform type to filter by</param>
    /// <returns>Collection of matching templates</returns>
    IEnumerable<TechnologyRoleTemplate> GetTemplatesByPlatformType(PlatformType platformType);

    /// <summary>
    /// Applies a technology template to create a VM environment configuration
    /// </summary>
    /// <param name="technology">The technology to apply</param>
    /// <param name="environment">The target environment type</param>
    /// <param name="includeOptionalRoles">Whether to include optional roles</param>
    /// <returns>Configured VMEnvironmentConfig with template roles</returns>
    VMEnvironmentConfig ApplyTemplate(
        Technology technology,
        EnvironmentType environment,
        bool includeOptionalRoles = false);

    /// <summary>
    /// Gets the default roles for a technology as VMRoleConfig instances
    /// </summary>
    /// <param name="technology">The technology to get roles for</param>
    /// <param name="isProd">Whether this is for production environment</param>
    /// <param name="includeOptional">Whether to include optional roles</param>
    /// <returns>List of configured VMRoleConfig instances</returns>
    List<VMRoleConfig> GetDefaultRoles(
        Technology technology,
        bool isProd,
        bool includeOptional = false);

    /// <summary>
    /// Checks if a technology is a low-code platform
    /// </summary>
    /// <param name="technology">The technology to check</param>
    /// <returns>True if low-code platform (Mendix, OutSystems)</returns>
    bool IsLowCodePlatform(Technology technology);
}
