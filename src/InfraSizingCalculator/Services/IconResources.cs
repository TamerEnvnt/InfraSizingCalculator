using InfraSizingCalculator.Models.Enums;

namespace InfraSizingCalculator.Services;

/// <summary>
/// Centralized icon resource mappings for consistent iconography across the application.
/// Icons are defined as CSS classes in /wwwroot/css/icons.css using inline SVG data URIs.
/// </summary>
public static class IconResources
{
    #region Environment Icons

    /// <summary>
    /// Gets the CSS class for environment icons (Dev, Test, Stage, Prod, DR, LifeTime).
    /// </summary>
    public static string GetEnvIconClass(EnvironmentType env) => env switch
    {
        EnvironmentType.Dev => "env-icon dev",
        EnvironmentType.Test => "env-icon test",
        EnvironmentType.Stage => "env-icon stage",
        EnvironmentType.Prod => "env-icon prod",
        EnvironmentType.DR => "env-icon dr",
        EnvironmentType.LifeTime => "env-icon lifetime",
        _ => "env-icon"
    };

    /// <summary>
    /// Gets the display name for an environment type.
    /// </summary>
    public static string GetEnvDisplayName(EnvironmentType env) => env switch
    {
        EnvironmentType.Dev => "Development",
        EnvironmentType.Test => "Testing",
        EnvironmentType.Stage => "Staging",
        EnvironmentType.Prod => "Production",
        EnvironmentType.DR => "Disaster Recovery",
        EnvironmentType.LifeTime => "LifeTime",
        _ => env.ToString()
    };

    /// <summary>
    /// Gets the short label for an environment type.
    /// </summary>
    public static string GetEnvShortLabel(EnvironmentType env) => env switch
    {
        EnvironmentType.Dev => "DEV",
        EnvironmentType.Test => "TEST",
        EnvironmentType.Stage => "STG",
        EnvironmentType.Prod => "PROD",
        EnvironmentType.DR => "DR",
        EnvironmentType.LifeTime => "LT",
        _ => env.ToString()
    };

    #endregion

    #region Server Role Icons

    /// <summary>
    /// Gets the CSS class for server role icons (Web, App, Database, etc.).
    /// </summary>
    public static string GetRoleIconClass(ServerRole role) => role switch
    {
        ServerRole.Web => "role-icon web",
        ServerRole.App => "role-icon app",
        ServerRole.Database => "role-icon database",
        ServerRole.Cache => "role-icon cache",
        ServerRole.MessageQueue => "role-icon mq",
        ServerRole.Search => "role-icon search",
        ServerRole.Storage => "role-icon storage",
        ServerRole.Monitoring => "role-icon monitoring",
        ServerRole.Bastion => "role-icon bastion",
        _ => "role-icon"
    };

    /// <summary>
    /// Gets the CSS class for server role icons from a string icon name.
    /// Used for TechnologyServerRole.Icon values like "web", "db", "app", "cache".
    /// </summary>
    public static string GetRoleIconClassFromString(string? icon) => icon?.ToLowerInvariant() switch
    {
        "web" => "role-icon web",
        "app" => "role-icon app",
        "db" or "database" => "role-icon database",
        "cache" => "role-icon cache",
        "mq" or "queue" => "role-icon mq",
        "search" => "role-icon search",
        "storage" => "role-icon storage",
        "monitoring" or "mon" => "role-icon monitoring",
        "bastion" => "role-icon bastion",
        _ => "role-icon"
    };

    /// <summary>
    /// Gets the display name for a server role.
    /// </summary>
    public static string GetRoleDisplayName(ServerRole role) => role switch
    {
        ServerRole.Web => "Web Server",
        ServerRole.App => "Application Server",
        ServerRole.Database => "Database Server",
        ServerRole.Cache => "Cache Server",
        ServerRole.MessageQueue => "Message Queue",
        ServerRole.Search => "Search Server",
        ServerRole.Storage => "Storage Server",
        ServerRole.Monitoring => "Monitoring",
        ServerRole.Bastion => "Bastion Host",
        _ => role.ToString()
    };

    #endregion

    #region K8s Node Type Icons

    /// <summary>
    /// Gets the CSS class for K8s node type icons.
    /// </summary>
    public static string GetNodeIconClass(string nodeType) => nodeType?.ToLowerInvariant() switch
    {
        "control" or "controlplane" or "cp" => "node-icon control",
        "infra" or "infrastructure" => "node-icon infra",
        "worker" or "w" => "node-icon worker",
        _ => "node-icon"
    };

    /// <summary>
    /// Gets the display name for a K8s node type.
    /// </summary>
    public static string GetNodeDisplayName(string nodeType) => nodeType?.ToLowerInvariant() switch
    {
        "control" or "controlplane" or "cp" => "Control Plane",
        "infra" or "infrastructure" => "Infrastructure",
        "worker" or "w" => "Worker",
        _ => nodeType ?? "Unknown"
    };

    #endregion

    #region Utility Icons

    /// <summary>
    /// Gets the CSS class for utility icons (users, deployment, cost).
    /// </summary>
    public static string GetUtilIconClass(string utilType) => utilType?.ToLowerInvariant() switch
    {
        "users" => "util-icon users",
        "deployment" => "util-icon deployment",
        "cost" => "util-icon cost",
        _ => "util-icon"
    };

    #endregion

    #region Size Classes

    /// <summary>
    /// Gets the size modifier class for icons.
    /// </summary>
    public static string GetSizeClass(string size) => size?.ToLowerInvariant() switch
    {
        "sm" or "small" => "icon-sm",
        "md" or "medium" => "icon-md",
        "lg" or "large" => "icon-lg",
        _ => ""
    };

    #endregion
}
