using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Models.Pricing;
using InfraSizingCalculator.Services.Interfaces;

namespace InfraSizingCalculator.Services;

/// <summary>
/// Provides UI helper methods for the Home page component.
/// Extracted from Home.razor.cs for testability and separation of concerns.
/// </summary>
public interface IHomePageUIHelperService
{
    // Cluster mode display helpers
    string GetClusterModeIcon(ClusterMode? clusterMode, string singleClusterScope);
    string GetClusterModeDescription(ClusterMode? clusterMode, string singleClusterScope, int environmentCount);
    string GetClusterModeBannerClass(ClusterMode? clusterMode, string singleClusterScope);

    // Step calculation helpers
    int GetConfigStep(DeploymentModel? deployment, Technology? technology);
    int GetPricingStep(DeploymentModel? deployment, Technology? technology);
    int GetResultsStep(DeploymentModel? deployment, Technology? technology);

    // Pricing estimation helpers
    int GetTotalNodesForPricing(HashSet<EnvironmentType> enabledEnvironments, Dictionary<EnvironmentType, AppConfig> envApps);
    int GetTotalCoresForPricing(HashSet<EnvironmentType> enabledEnvironments, Dictionary<EnvironmentType, AppConfig> envApps);
    int GetTotalRamForPricing(HashSet<EnvironmentType> enabledEnvironments, Dictionary<EnvironmentType, AppConfig> envApps);

    // Display name helpers
    string GetDistributionDisplayName(
        Technology? technology,
        Distribution? distribution,
        MendixDeploymentCategory? mendixCategory,
        MendixCloudType mendixCloudType,
        MendixPrivateCloudProvider? mendixPrivateCloudProvider);
    string GetMendixVMDeploymentDisplayName(MendixOtherDeployment? deployment);
    string GetMendixVMDeploymentName(MendixOtherDeployment? deployment);

    // Environment helpers
    EnvironmentType GetSingleClusterEnvironment(string singleClusterScope);
    IEnumerable<EnvironmentType> GetSingleClusterEnvironments(string singleClusterScope);

    // Icon and CSS class helpers
    string GetEnvCssClass(EnvironmentType env);
    string GetEnvIcon(EnvironmentType env);
    string GetShortEnvName(EnvironmentType env);
    string GetCostCategoryIcon(string category);

    // Tag formatting helpers
    string FormatTagLabel(string tag);
}

public class HomePageUIHelperService : IHomePageUIHelperService
{
    #region Cluster Mode Display Helpers

    public string GetClusterModeIcon(ClusterMode? clusterMode, string singleClusterScope)
    {
        if (clusterMode == ClusterMode.MultiCluster)
        {
            return "MC";
        }

        // Single Cluster mode - check scope
        return singleClusterScope == "Shared" ? "SC" : "SE";
    }

    public string GetClusterModeDescription(ClusterMode? clusterMode, string singleClusterScope, int environmentCount)
    {
        if (clusterMode == ClusterMode.MultiCluster)
        {
            return $"Multi-Cluster: {environmentCount} separate clusters (one per environment)";
        }

        // Single Cluster mode - check scope
        if (singleClusterScope == "Shared")
        {
            return "Shared Cluster: All environments in a single cluster with namespace isolation";
        }

        return $"Single Environment: Sizing for {singleClusterScope} only";
    }

    public string GetClusterModeBannerClass(ClusterMode? clusterMode, string singleClusterScope)
    {
        if (clusterMode == ClusterMode.MultiCluster)
        {
            return "multi";
        }

        // Single Cluster mode - check scope
        return singleClusterScope == "Shared" ? "shared" : "single";
    }

    #endregion

    #region Step Calculation Helpers

    public int GetConfigStep(DeploymentModel? deployment, Technology? technology)
    {
        if (deployment == DeploymentModel.Kubernetes)
            return 5;
        // Mendix VMs: config at step 5 (after deployment type at step 4)
        if (technology == Technology.Mendix)
            return 5;
        // Non-Mendix VMs: config at step 4 (no deployment type step)
        return 4;
    }

    public int GetPricingStep(DeploymentModel? deployment, Technology? technology)
    {
        if (deployment == DeploymentModel.Kubernetes)
            return 6;
        // Mendix VMs: pricing at step 6 (after config at step 5)
        if (technology == Technology.Mendix)
            return 6;
        // Non-Mendix VMs: pricing at step 5 (after config at step 4)
        return 5;
    }

    public int GetResultsStep(DeploymentModel? deployment, Technology? technology)
    {
        if (deployment == DeploymentModel.Kubernetes)
            return 7;
        // Mendix VMs: results at step 7 (after pricing at step 6)
        if (technology == Technology.Mendix)
            return 7;
        // Non-Mendix VMs: results at step 6 (after pricing at step 5)
        return 6;
    }

    #endregion

    #region Pricing Estimation Helpers

    public int GetTotalNodesForPricing(HashSet<EnvironmentType> enabledEnvironments, Dictionary<EnvironmentType, AppConfig> envApps)
    {
        var totalApps = 0;
        foreach (var env in enabledEnvironments)
        {
            if (envApps.TryGetValue(env, out var apps))
            {
                totalApps += apps.Small + apps.Medium + apps.Large + apps.XLarge;
            }
        }
        // Rough estimate: 1 node per 3 apps minimum
        return Math.Max(3, (int)Math.Ceiling(totalApps / 3.0));
    }

    public int GetTotalCoresForPricing(HashSet<EnvironmentType> enabledEnvironments, Dictionary<EnvironmentType, AppConfig> envApps)
    {
        var totalCores = 0;
        foreach (var env in enabledEnvironments)
        {
            if (envApps.TryGetValue(env, out var apps))
            {
                totalCores += apps.Small * 2 + apps.Medium * 4 + apps.Large * 8 + apps.XLarge * 16;
            }
        }
        return Math.Max(8, totalCores); // Minimum 8 cores
    }

    public int GetTotalRamForPricing(HashSet<EnvironmentType> enabledEnvironments, Dictionary<EnvironmentType, AppConfig> envApps)
    {
        var totalRam = 0;
        foreach (var env in enabledEnvironments)
        {
            if (envApps.TryGetValue(env, out var apps))
            {
                totalRam += apps.Small * 4 + apps.Medium * 8 + apps.Large * 16 + apps.XLarge * 32;
            }
        }
        return Math.Max(16, totalRam); // Minimum 16 GB
    }

    #endregion

    #region Display Name Helpers

    public string GetDistributionDisplayName(
        Technology? technology,
        Distribution? distribution,
        MendixDeploymentCategory? mendixCategory,
        MendixCloudType mendixCloudType,
        MendixPrivateCloudProvider? mendixPrivateCloudProvider)
    {
        // For Mendix deployments, show the Mendix deployment type instead of K8s distribution
        if (technology == Technology.Mendix)
        {
            if (mendixCategory == MendixDeploymentCategory.Cloud)
            {
                return mendixCloudType == MendixCloudType.Dedicated
                    ? "Mendix Cloud Dedicated"
                    : "Mendix Cloud SaaS";
            }
            if (mendixCategory == MendixDeploymentCategory.PrivateCloud)
            {
                return mendixPrivateCloudProvider switch
                {
                    MendixPrivateCloudProvider.Azure => "Mendix on Azure",
                    MendixPrivateCloudProvider.EKS => "Mendix on EKS",
                    MendixPrivateCloudProvider.AKS => "Mendix on AKS",
                    MendixPrivateCloudProvider.GKE => "Mendix on GKE",
                    MendixPrivateCloudProvider.OpenShift => "Mendix on OpenShift",
                    _ => distribution?.ToString() ?? "Unknown"
                };
            }
            if (mendixCategory == MendixDeploymentCategory.Other)
            {
                return mendixPrivateCloudProvider switch
                {
                    MendixPrivateCloudProvider.Rancher => "Rancher (Manual)",
                    MendixPrivateCloudProvider.K3s => "K3s (Manual)",
                    MendixPrivateCloudProvider.GenericK8s => "Generic K8s (Manual)",
                    _ => distribution?.ToString() ?? "Unknown"
                };
            }
        }
        return distribution?.ToString() ?? "Unknown";
    }

    public string GetMendixVMDeploymentDisplayName(MendixOtherDeployment? deployment)
    {
        return deployment switch
        {
            MendixOtherDeployment.Server => "Server (VMs)",
            MendixOtherDeployment.StackIT => "StackIT",
            MendixOtherDeployment.SapBtp => "SAP BTP",
            _ => "Not selected"
        };
    }

    public string GetMendixVMDeploymentName(MendixOtherDeployment? deployment)
    {
        return deployment switch
        {
            MendixOtherDeployment.Server => "Mendix on Server (VMs/Docker)",
            MendixOtherDeployment.StackIT => "Mendix on StackIT",
            MendixOtherDeployment.SapBtp => "Mendix on SAP BTP",
            _ => "Mendix Deployment"
        };
    }

    #endregion

    #region Environment Helpers

    public EnvironmentType GetSingleClusterEnvironment(string singleClusterScope)
    {
        return singleClusterScope switch
        {
            "Dev" => EnvironmentType.Dev,
            "Test" => EnvironmentType.Test,
            "Stage" => EnvironmentType.Stage,
            "Prod" => EnvironmentType.Prod,
            "DR" => EnvironmentType.DR,
            _ => EnvironmentType.Prod // Default for "Shared" - will use all envs
        };
    }

    public IEnumerable<EnvironmentType> GetSingleClusterEnvironments(string singleClusterScope)
    {
        if (singleClusterScope == "Shared")
        {
            return new[] { EnvironmentType.Dev, EnvironmentType.Test, EnvironmentType.Stage, EnvironmentType.Prod };
        }
        return new[] { GetSingleClusterEnvironment(singleClusterScope) };
    }

    #endregion

    #region Icon and CSS Class Helpers

    public string GetEnvCssClass(EnvironmentType env)
    {
        return env switch
        {
            EnvironmentType.Dev => "env-dev",
            EnvironmentType.Test => "env-test",
            EnvironmentType.Stage => "env-stage",
            EnvironmentType.Prod => "env-prod",
            EnvironmentType.DR => "env-dr",
            _ => "env-default"
        };
    }

    public string GetEnvIcon(EnvironmentType env)
    {
        return env switch
        {
            EnvironmentType.Dev => "fas fa-code",
            EnvironmentType.Test => "fas fa-flask",
            EnvironmentType.Stage => "fas fa-clipboard-check",
            EnvironmentType.Prod => "fas fa-rocket",
            EnvironmentType.DR => "fas fa-shield-alt",
            _ => "fas fa-server"
        };
    }

    public string GetShortEnvName(EnvironmentType env)
    {
        return env switch
        {
            EnvironmentType.Dev => "Dev",
            EnvironmentType.Test => "Test",
            EnvironmentType.Stage => "Stage",
            EnvironmentType.Prod => "Prod",
            EnvironmentType.DR => "DR",
            _ => env.ToString()
        };
    }

    public string GetCostCategoryIcon(string category)
    {
        return category.ToLower() switch
        {
            "compute" => "fas fa-microchip",
            "storage" => "fas fa-hdd",
            "network" => "fas fa-network-wired",
            "license" => "fas fa-key",
            "support" => "fas fa-headset",
            "labor" => "fas fa-users",
            "datacenter" => "fas fa-building",
            "control-plane" => "fas fa-server",
            _ => "fas fa-dollar-sign"
        };
    }

    #endregion

    #region Tag Formatting Helpers

    /// <summary>
    /// Formats a tag string as a display label (e.g., "on-prem" -> "On-Prem").
    /// </summary>
    public string FormatTagLabel(string tag)
    {
        if (string.IsNullOrEmpty(tag))
            return string.Empty;

        return tag switch
        {
            "on-prem" => "On-Prem",
            "cloud" => "Cloud",
            "managed" => "Managed",
            "enterprise" => "Enterprise",
            "developer" => "Developer",
            "lightweight" => "Lightweight",
            "security" => "Security",
            "ubuntu" => "Ubuntu",
            "open-source" => "Open Source",
            "edge" => "Edge",
            "cost-effective" => "Cost-Effective",
            "free" => "Free",
            "infra" => "Infra Nodes",
            _ => System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(tag.Replace("-", " "))
        };
    }

    #endregion
}
