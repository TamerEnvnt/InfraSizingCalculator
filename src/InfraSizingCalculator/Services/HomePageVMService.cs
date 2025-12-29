using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Services.Interfaces;

namespace InfraSizingCalculator.Services;

/// <summary>
/// Provides VM configuration helpers for the Home page component.
/// Extracted from Home.razor.cs for testability and separation of concerns.
/// </summary>
public interface IHomePageVMService
{
    // VM Configuration
    VMEnvironmentConfig CreateVMConfig(
        EnvironmentType env,
        HashSet<EnvironmentType> enabledEnvironments,
        Technology? technology,
        ITechnologyService technologyService);

    List<VMRoleConfig> CreateTechnologySpecificRoles(
        EnvironmentType env,
        Technology? technology,
        ITechnologyService technologyService);

    List<VMRoleConfig> CreateFallbackRoles(EnvironmentType env);

    // Technology Role helpers
    IEnumerable<TechnologyServerRole> GetAvailableTechnologyRoles(
        Technology? technology,
        ITechnologyService technologyService);

    int GetRoleMaxInstances(
        VMRoleConfig roleConfig,
        Technology? technology,
        ITechnologyService technologyService);

    bool IsRoleSingleInstance(
        VMRoleConfig roleConfig,
        Technology? technology,
        ITechnologyService technologyService);

    // Management Environment helpers
    bool HasManagementEnvironment(Technology? technology, ITechnologyService technologyService);
    string GetManagementEnvironmentName(Technology? technology, ITechnologyService technologyService);
    IEnumerable<TechnologyServerRole> GetManagementEnvironmentRoles(
        Technology? technology,
        ITechnologyService technologyService);

    // Display helpers
    string GetVMEnvSummary(
        EnvironmentType env,
        HashSet<EnvironmentType> enabledEnvironments,
        Dictionary<EnvironmentType, VMEnvironmentConfig> vmConfigs);

    string GetHAPatternDisplay(HAPattern pattern);
    string GetDRPatternDisplay(DRPattern pattern);
    string GetLBOptionDisplay(LoadBalancerOption option);
    string FormatHAPattern(HAPattern pattern);
    string GetRoleIcon(ServerRole role);

    // Environment display helpers (string-based for razor compatibility)
    string GetShortEnvName(string envName);
    string GetEnvCssClass(string envName);
    string GetEnvIcon(EnvironmentType env);

    // Environment helpers
    bool IsProdEnvironment(EnvironmentType env);
}

public class HomePageVMService : IHomePageVMService
{
    #region VM Configuration

    public VMEnvironmentConfig CreateVMConfig(
        EnvironmentType env,
        HashSet<EnvironmentType> enabledEnvironments,
        Technology? technology,
        ITechnologyService technologyService)
    {
        var isProd = IsProdEnvironment(env);
        return new VMEnvironmentConfig
        {
            Environment = env,
            Enabled = enabledEnvironments.Contains(env),
            HAPattern = isProd ? HAPattern.ActivePassive : HAPattern.None,
            DRPattern = DRPattern.None,
            LoadBalancer = isProd ? LoadBalancerOption.HAPair : LoadBalancerOption.None,
            StorageGB = isProd ? 500 : 100,
            Roles = CreateTechnologySpecificRoles(env, technology, technologyService)
        };
    }

    public List<VMRoleConfig> CreateTechnologySpecificRoles(
        EnvironmentType env,
        Technology? technology,
        ITechnologyService technologyService)
    {
        if (!technology.HasValue) return CreateFallbackRoles(env);

        var techConfig = technologyService.GetConfig(technology.Value);

        // For LifeTime environment, use management environment roles
        if (env == EnvironmentType.LifeTime && techConfig.ManagementEnvironmentRoles != null)
        {
            return techConfig.ManagementEnvironmentRoles.Roles
                .Where(r => r.Required)
                .Select(r => new VMRoleConfig
                {
                    Role = ServerRole.App,
                    RoleId = r.Id,
                    RoleName = r.Name,
                    RoleIcon = r.Icon,
                    RoleDescription = r.Description,
                    Size = r.DefaultSize,
                    InstanceCount = r.MinInstances,
                    DiskGB = r.DefaultDiskGB,
                    MemoryMultiplier = r.MemoryMultiplier
                })
                .ToList();
        }

        // For regular environments, use standard VM roles
        var vmRoles = technologyService.GetVMRoles(technology.Value);
        if (vmRoles != null)
        {
            var isProd = IsProdEnvironment(env);
            return vmRoles.Roles
                .Where(r => r.Required)
                .Select(r => new VMRoleConfig
                {
                    Role = ServerRole.App,
                    RoleId = r.Id,
                    RoleName = r.Name,
                    RoleIcon = r.Icon,
                    RoleDescription = r.Description,
                    Size = r.DefaultSize,
                    InstanceCount = r.ScaleHorizontally && isProd ? Math.Max(2, r.MinInstances) : r.MinInstances,
                    DiskGB = r.DefaultDiskGB,
                    MemoryMultiplier = r.MemoryMultiplier
                })
                .ToList();
        }

        return CreateFallbackRoles(env);
    }

    public List<VMRoleConfig> CreateFallbackRoles(EnvironmentType env)
    {
        var isProd = IsProdEnvironment(env);
        return new List<VMRoleConfig>
        {
            new() { Role = ServerRole.Web, RoleName = "Web Server", RoleIcon = "web", Size = AppTier.Medium, InstanceCount = isProd ? 2 : 1, DiskGB = 100 },
            new() { Role = ServerRole.App, RoleName = "Application Server", RoleIcon = "app", Size = AppTier.Medium, InstanceCount = isProd ? 2 : 1, DiskGB = 100 },
            new() { Role = ServerRole.Database, RoleName = "Database Server", RoleIcon = "db", Size = AppTier.Large, InstanceCount = 1, DiskGB = 500 }
        };
    }

    #endregion

    #region Technology Role Helpers

    public IEnumerable<TechnologyServerRole> GetAvailableTechnologyRoles(
        Technology? technology,
        ITechnologyService technologyService)
    {
        if (!technology.HasValue) return Enumerable.Empty<TechnologyServerRole>();
        var vmRoles = technologyService.GetVMRoles(technology.Value);
        return vmRoles?.Roles ?? Enumerable.Empty<TechnologyServerRole>();
    }

    public int GetRoleMaxInstances(
        VMRoleConfig roleConfig,
        Technology? technology,
        ITechnologyService technologyService)
    {
        if (string.IsNullOrEmpty(roleConfig.RoleId) || technology == null)
            return 100; // Default max for generic roles

        var vmRoles = technologyService.GetVMRoles(technology.Value);
        var techRole = vmRoles?.Roles.FirstOrDefault(r => r.Id == roleConfig.RoleId);
        return techRole?.MaxInstances ?? 100;
    }

    public bool IsRoleSingleInstance(
        VMRoleConfig roleConfig,
        Technology? technology,
        ITechnologyService technologyService)
    {
        return GetRoleMaxInstances(roleConfig, technology, technologyService) == 1;
    }

    #endregion

    #region Management Environment Helpers

    public bool HasManagementEnvironment(Technology? technology, ITechnologyService technologyService)
    {
        if (!technology.HasValue) return false;
        var config = technologyService.GetConfig(technology.Value);
        return config.HasSeparateManagementEnvironment;
    }

    public string GetManagementEnvironmentName(Technology? technology, ITechnologyService technologyService)
    {
        if (!technology.HasValue) return "Management";
        var config = technologyService.GetConfig(technology.Value);
        return config.ManagementEnvironmentName;
    }

    public IEnumerable<TechnologyServerRole> GetManagementEnvironmentRoles(
        Technology? technology,
        ITechnologyService technologyService)
    {
        if (!technology.HasValue) return Enumerable.Empty<TechnologyServerRole>();
        var config = technologyService.GetConfig(technology.Value);
        return config.ManagementEnvironmentRoles?.Roles ?? Enumerable.Empty<TechnologyServerRole>();
    }

    #endregion

    #region Display Helpers

    public string GetVMEnvSummary(
        EnvironmentType env,
        HashSet<EnvironmentType> enabledEnvironments,
        Dictionary<EnvironmentType, VMEnvironmentConfig> vmConfigs)
    {
        if (!enabledEnvironments.Contains(env)) return "Disabled";
        if (!vmConfigs.TryGetValue(env, out var config)) return "Not configured";
        return $"{config.Roles.Count} roles, {config.Roles.Sum(r => r.InstanceCount)} VMs";
    }

    public string GetHAPatternDisplay(HAPattern pattern) => pattern switch
    {
        HAPattern.None => "None",
        HAPattern.ActiveActive => "Active-Active",
        HAPattern.ActivePassive => "Active-Passive",
        HAPattern.NPlus1 => "N+1",
        HAPattern.NPlus2 => "N+2",
        _ => pattern.ToString()
    };

    public string GetDRPatternDisplay(DRPattern pattern) => pattern switch
    {
        DRPattern.None => "None",
        DRPattern.WarmStandby => "Warm Standby",
        DRPattern.HotStandby => "Hot Standby",
        DRPattern.MultiRegion => "Multi-Region",
        _ => pattern.ToString()
    };

    public string GetLBOptionDisplay(LoadBalancerOption option) => option switch
    {
        LoadBalancerOption.None => "None",
        LoadBalancerOption.Single => "Single LB",
        LoadBalancerOption.HAPair => "HA Pair",
        LoadBalancerOption.CloudLB => "Cloud LB",
        _ => option.ToString()
    };

    public string FormatHAPattern(HAPattern pattern) => pattern switch
    {
        HAPattern.None => "None",
        HAPattern.ActiveActive => "Active-Active",
        HAPattern.ActivePassive => "Active-Passive",
        HAPattern.NPlus1 => "N+1 Redundancy",
        HAPattern.NPlus2 => "N+2 Redundancy",
        _ => pattern.ToString()
    };

    public string GetRoleIcon(ServerRole role) => role switch
    {
        ServerRole.Web => "web",
        ServerRole.App => "app",
        ServerRole.Database => "db",
        ServerRole.Cache => "cache",
        ServerRole.MessageQueue => "mq",
        ServerRole.Search => "search",
        ServerRole.Storage => "storage",
        ServerRole.Monitoring => "mon",
        ServerRole.Bastion => "bastion",
        _ => "server"
    };

    #endregion

    #region Environment Display Helpers

    public string GetShortEnvName(string envName) => envName switch
    {
        "Development" => "DEV",
        "Test" => "TEST",
        "Testing" => "TEST",
        "Staging" => "STG",
        "Production" => "PROD",
        "Disaster Recovery" => "DR",
        _ => envName.Length > 4 ? envName[..4].ToUpper() : envName.ToUpper()
    };

    public string GetEnvCssClass(string envName) => envName.ToLower() switch
    {
        "development" => "env-dev",
        "test" or "testing" => "env-test",
        "staging" => "env-stage",
        "production" => "env-prod",
        "disaster recovery" => "env-dr",
        _ => "env-default"
    };

    public string GetEnvIcon(EnvironmentType env) => env switch
    {
        EnvironmentType.Dev => "D",
        EnvironmentType.Test => "T",
        EnvironmentType.Stage => "S",
        EnvironmentType.Prod => "P",
        EnvironmentType.DR => "DR",
        _ => "E"
    };

    #endregion

    #region Environment Helpers

    public bool IsProdEnvironment(EnvironmentType env) =>
        env == EnvironmentType.Prod || env == EnvironmentType.DR;

    #endregion
}
