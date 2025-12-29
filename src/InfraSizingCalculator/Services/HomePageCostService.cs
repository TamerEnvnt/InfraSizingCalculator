using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Models.Pricing;
using InfraSizingCalculator.Services.Interfaces;

namespace InfraSizingCalculator.Services;

/// <summary>
/// Handles cost-related calculations for the Home page.
/// Extracted from Home.razor for testability and separation of concerns.
/// </summary>
public interface IHomePageCostService
{
    /// <summary>
    /// Gets the monthly cost estimate based on current state.
    /// </summary>
    decimal GetMonthlyEstimate(HomePageCostContext context);

    /// <summary>
    /// Gets the cost provider display name.
    /// </summary>
    string? GetCostProvider(HomePageCostContext context);

    /// <summary>
    /// Determines if pricing should show N/A.
    /// </summary>
    bool ShouldShowPricingNA(HomePageCostContext context);

    /// <summary>
    /// Gets compute cost for cloud instances.
    /// </summary>
    decimal GetComputeCost(string provider, string instanceType, int nodeCount);

    /// <summary>
    /// Formats a cost amount for preview display.
    /// </summary>
    string FormatCostPreview(decimal? amount);

    /// <summary>
    /// Formats a currency amount.
    /// </summary>
    string FormatCurrency(decimal amount);

    /// <summary>
    /// Gets the icon for a cost category.
    /// </summary>
    string GetCostCategoryIcon(CostCategory category);

    /// <summary>
    /// Gets the Mendix VM (Other deployment) license monthly cost.
    /// </summary>
    decimal GetMendixVMLicenseMonthly(MendixOtherDeployment deployment, bool isUnlimitedApps, int numberOfApps);
}

/// <summary>
/// Context for cost calculations - contains all the state needed from the page.
/// </summary>
public class HomePageCostContext
{
    public Technology? SelectedTechnology { get; set; }
    public DeploymentModel? SelectedDeployment { get; set; }
    public Distribution? SelectedDistribution { get; set; }
    public PricingStepResult? PricingResult { get; set; }
    public CostEstimate? K8sCostEstimate { get; set; }
    public CostEstimate? VMCostEstimate { get; set; }
    public VMSizingResult? VMResults { get; set; }

    // Mendix-specific settings
    public MendixDeploymentCategory? MendixDeploymentCategory { get; set; }
    public MendixCloudType? MendixCloudType { get; set; }
    public MendixPrivateCloudProvider? MendixPrivateCloudProvider { get; set; }
    public MendixOtherDeployment? MendixOtherDeployment { get; set; }
    public bool MendixIsUnlimitedApps { get; set; }
    public int MendixNumberOfApps { get; set; }
}

public class HomePageCostService : IHomePageCostService
{
    private readonly IPricingSettingsService _pricingSettingsService;

    public HomePageCostService(IPricingSettingsService pricingSettingsService)
    {
        _pricingSettingsService = pricingSettingsService;
    }

    /// <inheritdoc/>
    public decimal GetMonthlyEstimate(HomePageCostContext context)
    {
        // If pricing should show N/A, return 0
        if (ShouldShowPricingNA(context))
        {
            return 0;
        }

        // For Mendix K8s deployments, use MendixCost from pricing result
        if (context.SelectedTechnology == Technology.Mendix &&
            context.SelectedDeployment == DeploymentModel.Kubernetes &&
            context.PricingResult?.MendixCost != null)
        {
            return context.PricingResult.MendixCost.TotalPerMonth;
        }

        // For Mendix VM (Other) deployments, calculate infrastructure + license cost
        if (context.SelectedTechnology == Technology.Mendix &&
            context.SelectedDeployment == DeploymentModel.VMs &&
            context.VMResults != null)
        {
            var infraCost = context.VMCostEstimate?.MonthlyTotal ?? 0;
            var licenseCost = GetMendixVMLicenseMonthly(
                context.MendixOtherDeployment ?? MendixOtherDeployment.Server,
                context.MendixIsUnlimitedApps,
                context.MendixNumberOfApps);
            return infraCost + licenseCost;
        }

        // Use pricingResult if available (from pricing step)
        if (context.PricingResult?.MonthlyCost.HasValue == true)
        {
            return context.PricingResult.MonthlyCost.Value;
        }

        if (context.K8sCostEstimate != null)
        {
            return context.K8sCostEstimate.MonthlyTotal;
        }
        if (context.VMCostEstimate != null)
        {
            return context.VMCostEstimate.MonthlyTotal;
        }
        return 0;
    }

    /// <inheritdoc/>
    public decimal GetMendixVMLicenseMonthly(MendixOtherDeployment deployment, bool isUnlimitedApps, int numberOfApps)
    {
        var (perApp, unlimited) = GetMendixVMDeploymentPricing(deployment);
        var yearlyLicense = isUnlimitedApps ? unlimited : perApp * numberOfApps;
        return yearlyLicense / 12;
    }

    private (decimal perApp, decimal unlimited) GetMendixVMDeploymentPricing(MendixOtherDeployment deployment)
    {
        // Default pricing based on deployment type
        // Actual implementation should use IPricingSettingsService.GetMendixPricingSettings()
        return deployment switch
        {
            MendixOtherDeployment.Server => (8000m, 90000m),
            MendixOtherDeployment.StackIT => (9000m, 100000m),
            MendixOtherDeployment.SapBtp => (7500m, 85000m),
            _ => (8000m, 90000m)
        };
    }

    /// <inheritdoc/>
    public string? GetCostProvider(HomePageCostContext context)
    {
        // Handle Mendix-specific deployments - always show Mendix as the provider
        if (context.SelectedTechnology == Technology.Mendix)
        {
            if (context.SelectedDeployment == DeploymentModel.Kubernetes)
            {
                // Mendix Cloud (SaaS/Dedicated)
                if (context.MendixDeploymentCategory == Models.Pricing.MendixDeploymentCategory.Cloud)
                {
                    return context.MendixCloudType == Models.Pricing.MendixCloudType.Dedicated
                        ? "Mendix Dedicated"
                        : "Mendix Cloud";
                }
                // Mendix Private Cloud
                if (context.MendixDeploymentCategory == Models.Pricing.MendixDeploymentCategory.PrivateCloud)
                {
                    return context.MendixPrivateCloudProvider switch
                    {
                        Models.Pricing.MendixPrivateCloudProvider.Azure => "Mendix Azure",
                        Models.Pricing.MendixPrivateCloudProvider.EKS => "Mendix EKS",
                        Models.Pricing.MendixPrivateCloudProvider.AKS => "Mendix AKS",
                        Models.Pricing.MendixPrivateCloudProvider.GKE => "Mendix GKE",
                        Models.Pricing.MendixPrivateCloudProvider.OpenShift => "Mendix OpenShift",
                        _ => "Mendix"
                    };
                }
                // Other K8s (Rancher, K3s, Generic)
                if (context.MendixDeploymentCategory == Models.Pricing.MendixDeploymentCategory.Other)
                {
                    return context.MendixOtherDeployment switch
                    {
                        MendixOtherDeployment.Server => "Mendix Server",
                        MendixOtherDeployment.StackIT => "Mendix StackIT",
                        MendixOtherDeployment.SapBtp => "Mendix SAP BTP",
                        _ => "Mendix"
                    };
                }
            }
            if (context.SelectedDeployment == DeploymentModel.VMs)
            {
                return context.MendixOtherDeployment switch
                {
                    MendixOtherDeployment.Server => "Mendix Server",
                    MendixOtherDeployment.StackIT => "Mendix StackIT",
                    MendixOtherDeployment.SapBtp => "Mendix SAP BTP",
                    _ => "Mendix VM"
                };
            }
        }

        // For cloud distributions, show the cloud provider
        if (context.SelectedDistribution.HasValue)
        {
            return context.SelectedDistribution.Value switch
            {
                Distribution.AKS => "Azure",
                Distribution.EKS => "AWS",
                Distribution.GKE => "Google Cloud",
                _ => GetOnPremProvider(context)
            };
        }

        return null;
    }

    private string? GetOnPremProvider(HomePageCostContext context)
    {
        // For on-prem, show based on configured pricing
        if (context.K8sCostEstimate != null || context.VMCostEstimate != null)
        {
            return "On-Prem";
        }
        return context.PricingResult?.HasCosts == true ? "Configured" : null;
    }

    /// <inheritdoc/>
    public bool ShouldShowPricingNA(HomePageCostContext context)
    {
        // Mendix deployments always have pricing available
        if (context.SelectedTechnology == Technology.Mendix)
        {
            return false;
        }

        // Show N/A when:
        // 1. It's an on-prem distribution AND
        // 2. Pricing is not configured (either pricingResult is null or HasCosts is false)
        if (!context.SelectedDistribution.HasValue) return false;

        var isOnPrem = _pricingSettingsService.IsOnPremDistribution(context.SelectedDistribution.Value);
        if (!isOnPrem) return false;

        // Check if pricing was configured in the pricing step
        return context.PricingResult == null || !context.PricingResult.HasCosts;
    }

    /// <inheritdoc/>
    public decimal GetComputeCost(string provider, string instanceType, int nodeCount)
    {
        // Approximate monthly costs for common instance types
        var hourlyRate = instanceType switch
        {
            // AWS instance types
            "m6i.xlarge" => 0.192m,
            "m6i.2xlarge" => 0.384m,
            "m6i.4xlarge" => 0.768m,
            // Azure instance types
            "Standard_D4s_v5" => 0.192m,
            "Standard_D8s_v5" => 0.384m,
            "Standard_D16s_v5" => 0.768m,
            // GCP instance types
            "n2-standard-4" => 0.195m,
            "n2-standard-8" => 0.389m,
            "n2-standard-16" => 0.778m,
            _ => 0.40m
        };
        return hourlyRate * 730 * nodeCount; // 730 hours/month
    }

    /// <inheritdoc/>
    public string FormatCostPreview(decimal? amount)
    {
        if (!amount.HasValue) return "--";
        var value = amount.Value;
        if (value >= 1_000_000)
            return $"${value / 1_000_000:F2}M";
        if (value >= 1_000)
            return $"${value / 1_000:F1}K";
        return $"${value:F0}";
    }

    /// <inheritdoc/>
    public string FormatCurrency(decimal amount)
    {
        if (amount >= 1_000_000)
            return $"${amount / 1_000_000:F2}M";
        if (amount >= 1_000)
            return $"${amount / 1_000:F1}K";
        return $"${amount:F2}";
    }

    /// <inheritdoc/>
    public string GetCostCategoryIcon(CostCategory category)
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
}
