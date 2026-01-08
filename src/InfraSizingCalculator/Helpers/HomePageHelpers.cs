using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Models.Pricing;

namespace InfraSizingCalculator.Helpers;

/// <summary>
/// Pure/stateless helper methods extracted from Home.razor for testability.
/// These methods contain business logic that was previously embedded in the UI component.
/// </summary>
public static class HomePageHelpers
{
    #region Wizard Step Navigation

    /// <summary>
    /// Gets the total number of steps in the wizard based on deployment and technology selections.
    /// K8s: 7 steps, Mendix VMs: 7 steps, Other VMs: 6 steps.
    /// </summary>
    public static int GetTotalSteps(DeploymentModel? selectedDeployment, Technology? selectedTechnology)
    {
        // K8s: 1-Platform, 2-Deployment, 3-Technology, 4-Distribution, 5-Configure, 6-Pricing, 7-Results
        if (selectedDeployment == DeploymentModel.Kubernetes)
            return 7;

        // VMs with Mendix have deployment type selection step
        if (selectedDeployment == DeploymentModel.VMs && selectedTechnology == Technology.Mendix)
            return 7;

        // Non-Mendix VMs: skip deployment type step but include pricing
        return 6;
    }

    /// <summary>
    /// Gets the step number for the configuration step.
    /// </summary>
    public static int GetConfigStep(DeploymentModel? selectedDeployment, Technology? selectedTechnology)
    {
        if (selectedDeployment == DeploymentModel.Kubernetes)
            return 5;

        // Mendix VMs: config at step 5 (after deployment type at step 4)
        if (selectedTechnology == Technology.Mendix)
            return 5;

        // Non-Mendix VMs: config at step 4 (no deployment type step)
        return 4;
    }

    /// <summary>
    /// Gets the step number for the pricing step.
    /// </summary>
    public static int GetPricingStep(DeploymentModel? selectedDeployment, Technology? selectedTechnology)
    {
        if (selectedDeployment == DeploymentModel.Kubernetes)
            return 6;

        // Mendix VMs: pricing at step 6 (after config at step 5)
        if (selectedTechnology == Technology.Mendix)
            return 6;

        // Non-Mendix VMs: pricing at step 5 (after config at step 4)
        return 5;
    }

    /// <summary>
    /// Gets the step number for the results step.
    /// </summary>
    public static int GetResultsStep(DeploymentModel? selectedDeployment, Technology? selectedTechnology)
    {
        if (selectedDeployment == DeploymentModel.Kubernetes)
            return 7;

        // Mendix VMs: results at step 7 (after pricing at step 6)
        if (selectedTechnology == Technology.Mendix)
            return 7;

        // Non-Mendix VMs: results at step 6 (after pricing at step 5)
        return 6;
    }

    /// <summary>
    /// Gets the label for a step in the wizard.
    /// </summary>
    public static string GetStepLabel(int step, DeploymentModel? selectedDeployment, Technology? selectedTechnology)
    {
        if (selectedDeployment == DeploymentModel.Kubernetes)
        {
            return step switch
            {
                1 => "Platform",
                2 => "Deployment",
                3 => "Technology",
                4 => "Distribution",
                5 => "Configure",
                6 => "Pricing",
                7 => "Results",
                _ => ""
            };
        }

        // VMs with Mendix
        if (selectedTechnology == Technology.Mendix)
        {
            return step switch
            {
                1 => "Platform",
                2 => "Deployment",
                3 => "Technology",
                4 => "Deployment Type",
                5 => "Configure",
                6 => "Pricing",
                7 => "Results",
                _ => ""
            };
        }

        // Non-Mendix VMs
        return step switch
        {
            1 => "Platform",
            2 => "Deployment",
            3 => "Technology",
            4 => "Configure",
            5 => "Pricing",
            6 => "Results",
            _ => ""
        };
    }

    #endregion

    #region Cluster Mode Helpers

    /// <summary>
    /// Gets the icon abbreviation for the cluster mode.
    /// </summary>
    public static string GetClusterModeIcon(ClusterMode? selectedClusterMode, string singleClusterScope)
    {
        if (selectedClusterMode == ClusterMode.MultiCluster)
            return "MC";

        // Single Cluster mode - check scope
        return singleClusterScope == "Shared" ? "SC" : "SE";
    }

    /// <summary>
    /// Gets a description of the cluster mode for display.
    /// </summary>
    public static string GetClusterModeDescription(ClusterMode? selectedClusterMode, string singleClusterScope, int environmentCount)
    {
        if (selectedClusterMode == ClusterMode.MultiCluster)
            return $"Multi-Cluster: {environmentCount} separate clusters (one per environment)";

        // Single Cluster mode - check scope
        if (singleClusterScope == "Shared")
            return "Shared Cluster: All environments in a single cluster with namespace isolation";

        return $"Single Environment: Sizing for {singleClusterScope} only";
    }

    /// <summary>
    /// Gets the CSS class for the cluster mode banner.
    /// </summary>
    public static string GetClusterModeBannerClass(ClusterMode? selectedClusterMode, string singleClusterScope)
    {
        if (selectedClusterMode == ClusterMode.MultiCluster)
            return "multi";

        // Single Cluster mode - check scope
        return singleClusterScope == "Shared" ? "shared" : "single";
    }

    /// <summary>
    /// Determines if the current mode is single cluster mode.
    /// </summary>
    public static bool IsSingleClusterMode(ClusterMode? selectedClusterMode)
    {
        return selectedClusterMode == ClusterMode.SharedCluster || selectedClusterMode == ClusterMode.PerEnvironment;
    }

    /// <summary>
    /// Determines if it's a shared cluster mode specifically.
    /// </summary>
    public static bool IsSharedClusterMode(ClusterMode? selectedClusterMode, string singleClusterScope)
    {
        return IsSingleClusterMode(selectedClusterMode) && singleClusterScope == "Shared";
    }

    /// <summary>
    /// Gets the environment type for single cluster scope.
    /// </summary>
    public static EnvironmentType GetSingleClusterEnvironment(string singleClusterScope)
    {
        return singleClusterScope switch
        {
            "Dev" => EnvironmentType.Dev,
            "Test" => EnvironmentType.Test,
            "Stage" => EnvironmentType.Stage,
            "Prod" => EnvironmentType.Prod,
            "DR" => EnvironmentType.DR,
            _ => EnvironmentType.Prod // Default for "Shared"
        };
    }

    #endregion

    #region Cloud Pricing Calculations

    /// <summary>
    /// Gets the appropriate instance type based on CPU and RAM requirements.
    /// </summary>
    public static string GetInstanceType(string provider, decimal cpuPerWorker, decimal ramPerWorker)
    {
        return provider switch
        {
            "aws" => cpuPerWorker >= 16 ? "m6i.4xlarge" : cpuPerWorker >= 8 ? "m6i.2xlarge" : "m6i.xlarge",
            "azure" => cpuPerWorker >= 16 ? "Standard_D16s_v5" : cpuPerWorker >= 8 ? "Standard_D8s_v5" : "Standard_D4s_v5",
            "gcp" => cpuPerWorker >= 16 ? "n2-standard-16" : cpuPerWorker >= 8 ? "n2-standard-8" : "n2-standard-4",
            _ => "standard"
        };
    }

    /// <summary>
    /// Calculates the monthly compute cost for a given instance type and node count.
    /// </summary>
    public static decimal GetComputeCost(string provider, string instanceType, int nodeCount)
    {
        var hourlyRate = instanceType switch
        {
            // AWS instances
            "m6i.xlarge" => 0.192m,
            "m6i.2xlarge" => 0.384m,
            "m6i.4xlarge" => 0.768m,
            // Azure instances
            "Standard_D4s_v5" => 0.192m,
            "Standard_D8s_v5" => 0.384m,
            "Standard_D16s_v5" => 0.768m,
            // GCP instances
            "n2-standard-4" => 0.195m,
            "n2-standard-8" => 0.389m,
            "n2-standard-16" => 0.778m,
            // Default fallback
            _ => 0.40m
        };
        return hourlyRate * 730 * nodeCount; // 730 hours/month
    }

    #endregion

    #region Cost Formatting

    /// <summary>
    /// Formats a cost amount for preview display (abbreviated format).
    /// </summary>
    public static string FormatCostPreview(decimal? amount)
    {
        if (!amount.HasValue) return "--";
        var value = amount.Value;
        if (value >= 1_000_000)
            return $"${value / 1_000_000:F2}M";
        if (value >= 1_000)
            return $"${value / 1_000:F1}K";
        return $"${value:F0}";
    }

    /// <summary>
    /// Formats a currency amount for full display.
    /// </summary>
    public static string FormatCurrency(decimal amount)
    {
        if (amount >= 1_000_000)
            return $"${amount / 1_000_000:F2}M";
        if (amount >= 1_000)
            return $"${amount / 1_000:F1}K";
        return $"${amount:F2}";
    }

    /// <summary>
    /// Gets the icon abbreviation for a cost category.
    /// </summary>
    public static string GetCostCategoryIcon(CostCategory category)
    {
        return category switch
        {
            CostCategory.Compute => "CPU",
            CostCategory.Storage => "HDD",
            CostCategory.Network => "NET",
            CostCategory.License => "LIC",
            CostCategory.Support => "SUP",
            CostCategory.DataCenter => "DC",
            CostCategory.Labor => "OPS",
            _ => ""
        };
    }

    #endregion

    #region Environment Display Helpers

    /// <summary>
    /// Gets the short name for an environment (e.g., "Development" -> "DEV").
    /// </summary>
    public static string GetShortEnvName(string envName)
    {
        return envName switch
        {
            "Development" => "DEV",
            "Test" => "TEST",
            "Testing" => "TEST",
            "Staging" => "STG",
            "Production" => "PROD",
            "Disaster Recovery" => "DR",
            _ => envName.Length > 4 ? envName[..4].ToUpper() : envName.ToUpper()
        };
    }

    /// <summary>
    /// Gets the CSS class for an environment.
    /// </summary>
    public static string GetEnvCssClass(string envName)
    {
        return envName.ToLower() switch
        {
            "development" => "env-dev",
            "test" or "testing" => "env-test",
            "staging" => "env-stage",
            "production" => "env-prod",
            "disaster recovery" => "env-dr",
            _ => "env-default"
        };
    }

    /// <summary>
    /// Gets the icon abbreviation for an environment type.
    /// </summary>
    public static string GetEnvIcon(EnvironmentType env)
    {
        return env switch
        {
            EnvironmentType.Dev => "D",
            EnvironmentType.Test => "T",
            EnvironmentType.Stage => "S",
            EnvironmentType.Prod => "P",
            EnvironmentType.DR => "DR",
            _ => "E"
        };
    }

    /// <summary>
    /// Determines if an environment is a production environment.
    /// </summary>
    public static bool IsProdEnvironment(EnvironmentType env) => env == EnvironmentType.Prod;

    #endregion

    #region Pattern Display Helpers

    /// <summary>
    /// Formats an HA pattern for display.
    /// </summary>
    public static string FormatHAPattern(HAPattern pattern)
    {
        return pattern switch
        {
            HAPattern.None => "None",
            HAPattern.ActiveActive => "Active-Active",
            HAPattern.ActivePassive => "Active-Passive",
            HAPattern.NPlus1 => "N+1 Redundancy",
            HAPattern.NPlus2 => "N+2 Redundancy",
            _ => pattern.ToString()
        };
    }

    /// <summary>
    /// Gets display string for HA pattern (shorter version).
    /// </summary>
    public static string GetHAPatternDisplay(HAPattern pattern)
    {
        return pattern switch
        {
            HAPattern.None => "None",
            HAPattern.ActiveActive => "Active-Active",
            HAPattern.ActivePassive => "Active-Passive",
            HAPattern.NPlus1 => "N+1",
            HAPattern.NPlus2 => "N+2",
            _ => pattern.ToString()
        };
    }

    /// <summary>
    /// Gets display string for DR pattern.
    /// </summary>
    public static string GetDRPatternDisplay(DRPattern pattern)
    {
        return pattern switch
        {
            DRPattern.None => "None",
            DRPattern.WarmStandby => "Warm Standby",
            DRPattern.HotStandby => "Hot Standby",
            DRPattern.MultiRegion => "Multi-Region",
            _ => pattern.ToString()
        };
    }

    /// <summary>
    /// Gets display string for load balancer option.
    /// </summary>
    public static string GetLBOptionDisplay(LoadBalancerOption option)
    {
        return option switch
        {
            LoadBalancerOption.None => "None",
            LoadBalancerOption.Single => "Single LB",
            LoadBalancerOption.HAPair => "HA Pair",
            LoadBalancerOption.CloudLB => "Cloud LB",
            _ => option.ToString()
        };
    }

    #endregion

    #region Server Role Helpers

    /// <summary>
    /// Gets the icon for a server role.
    /// </summary>
    public static string GetRoleIcon(ServerRole role)
    {
        return role switch
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
    }

    #endregion

    #region Parsing Helpers

    /// <summary>
    /// Parses an object value to integer, returning 0 if parsing fails.
    /// </summary>
    public static int ParseInt(object? value)
    {
        return int.TryParse(value?.ToString(), out var result) ? result : 0;
    }

    /// <summary>
    /// Parses an object value to double, returning 0 if parsing fails.
    /// </summary>
    public static double ParseDouble(object? value)
    {
        return double.TryParse(value?.ToString(), out var result) ? result : 0;
    }

    #endregion

    #region Mendix Deployment Helpers

    /// <summary>
    /// Gets the display name for a Mendix VM deployment type.
    /// </summary>
    public static string GetMendixVMDeploymentDisplayName(MendixOtherDeployment? mendixOtherDeployment)
    {
        return mendixOtherDeployment switch
        {
            MendixOtherDeployment.Server => "Server (VMs)",
            MendixOtherDeployment.StackIT => "StackIT",
            MendixOtherDeployment.SapBtp => "SAP BTP",
            _ => "Not selected"
        };
    }

    /// <summary>
    /// Gets the full name for a Mendix VM deployment.
    /// </summary>
    public static string GetMendixVMDeploymentName(MendixOtherDeployment? mendixOtherDeployment)
    {
        return mendixOtherDeployment switch
        {
            MendixOtherDeployment.Server => "Mendix on Server (VMs/Docker)",
            MendixOtherDeployment.StackIT => "Mendix on StackIT",
            MendixOtherDeployment.SapBtp => "Mendix on SAP BTP",
            _ => "Mendix Deployment"
        };
    }

    #endregion

    #region Distribution Helpers

    /// <summary>
    /// Determines if a distribution is an on-premises distribution.
    /// </summary>
    public static bool IsOnPremDistribution(Distribution distribution)
    {
        return distribution switch
        {
            Distribution.OpenShift => true,
            Distribution.Kubernetes => true,
            Distribution.Rancher => true,
            Distribution.RKE2 => true,
            Distribution.K3s => true,
            Distribution.MicroK8s => true,
            Distribution.Charmed => true,
            Distribution.Tanzu => true,
            _ => false
        };
    }

    /// <summary>
    /// Determines if a distribution is an OpenShift variant.
    /// </summary>
    public static bool IsOpenShiftDistribution(Distribution? distribution)
    {
        if (distribution == null) return false;
        return distribution.Value switch
        {
            Distribution.OpenShift => true,
            Distribution.OpenShiftROSA => true,
            Distribution.OpenShiftARO => true,
            Distribution.OpenShiftDedicated => true,
            Distribution.OpenShiftIBM => true,
            _ => false
        };
    }

    /// <summary>
    /// Maps a Mendix private cloud provider to the corresponding K8s distribution.
    /// </summary>
    public static Distribution MapMendixProviderToDistribution(MendixPrivateCloudProvider? provider)
    {
        return provider switch
        {
            MendixPrivateCloudProvider.Azure => Distribution.AKS,
            MendixPrivateCloudProvider.EKS => Distribution.EKS,
            MendixPrivateCloudProvider.AKS => Distribution.AKS,
            MendixPrivateCloudProvider.GKE => Distribution.GKE,
            MendixPrivateCloudProvider.OpenShift => Distribution.OpenShift,
            MendixPrivateCloudProvider.Rancher => Distribution.Rancher,
            MendixPrivateCloudProvider.K3s => Distribution.K3s,
            MendixPrivateCloudProvider.GenericK8s => Distribution.Kubernetes,
            MendixPrivateCloudProvider.Docker => Distribution.Kubernetes,
            _ => Distribution.Kubernetes
        };
    }

    #endregion

    #region Pricing Estimation Helpers

    /// <summary>
    /// Estimates total nodes for pricing based on app counts.
    /// </summary>
    public static int GetTotalNodesForPricing(Dictionary<EnvironmentType, AppConfig> envApps, HashSet<EnvironmentType> enabledEnvironments)
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

    /// <summary>
    /// Estimates total cores for pricing based on app sizes.
    /// </summary>
    public static int GetTotalCoresForPricing(Dictionary<EnvironmentType, AppConfig> envApps, HashSet<EnvironmentType> enabledEnvironments)
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

    /// <summary>
    /// Estimates total RAM for pricing based on app sizes.
    /// </summary>
    public static int GetTotalRamForPricing(Dictionary<EnvironmentType, AppConfig> envApps, HashSet<EnvironmentType> enabledEnvironments)
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
}
