using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Models.Pricing;
using InfraSizingCalculator.Services.Interfaces;

namespace InfraSizingCalculator.Services;

/// <summary>
/// Handles distribution and technology-related operations for the Home page.
/// Extracted from Home.razor for testability and separation of concerns.
/// </summary>
public interface IHomePageDistributionService
{
    /// <summary>
    /// Gets the display name for a distribution.
    /// </summary>
    string GetDistributionName(Distribution? distro);

    /// <summary>
    /// Gets the display name for a technology.
    /// </summary>
    string GetTechnologyName(Technology? tech);

    /// <summary>
    /// Determines if the current deployment is on-premises based on technology,
    /// deployment model, and Mendix-specific settings.
    /// </summary>
    bool IsOnPremDistribution(HomePageDistributionContext context);

    /// <summary>
    /// Checks if the selected distribution is part of the OpenShift family.
    /// </summary>
    bool IsOpenShiftDistribution(Distribution? distro);

    /// <summary>
    /// Checks if the distribution has a managed control plane (cloud-hosted).
    /// </summary>
    bool IsManagedControlPlane(Distribution? distro);

    /// <summary>
    /// Gets all available distributions.
    /// </summary>
    IEnumerable<DistributionConfig> GetAllDistributions();

    /// <summary>
    /// Gets distributions filtered by tag (e.g., "on-prem", "cloud").
    /// </summary>
    IEnumerable<DistributionConfig> GetDistributionsByTag(string tag);
}

/// <summary>
/// Context for distribution-related calculations.
/// </summary>
public class HomePageDistributionContext
{
    public Technology? SelectedTechnology { get; set; }
    public DeploymentModel? SelectedDeployment { get; set; }
    public Distribution? SelectedDistribution { get; set; }
    public MendixDeploymentCategory? MendixDeploymentCategory { get; set; }
    public MendixPrivateCloudProvider? MendixPrivateCloudProvider { get; set; }
}

public class HomePageDistributionService : IHomePageDistributionService
{
    private readonly IDistributionService _distributionService;

    public HomePageDistributionService(IDistributionService distributionService)
    {
        _distributionService = distributionService;
    }

    /// <inheritdoc/>
    public string GetDistributionName(Distribution? distro)
    {
        if (!distro.HasValue)
            return "Unknown";

        var config = _distributionService.GetConfig(distro.Value);
        return config.Name;
    }

    /// <inheritdoc/>
    public string GetTechnologyName(Technology? tech) => tech switch
    {
        Technology.DotNet => ".NET",
        Technology.Java => "Java",
        Technology.NodeJs => "Node.js",
        Technology.Python => "Python",
        Technology.Go => "Go",
        Technology.Mendix => "Mendix",
        Technology.OutSystems => "OutSystems",
        _ => "Unknown"
    };

    /// <inheritdoc/>
    public bool IsOnPremDistribution(HomePageDistributionContext context)
    {
        // Check Mendix deployment category first
        if (context.SelectedTechnology == Technology.Mendix &&
            context.SelectedDeployment == DeploymentModel.Kubernetes)
        {
            return IsMendixOnPrem(context);
        }

        // For non-Mendix K8s, check the distribution
        if (context.SelectedDistribution == null)
            return false;

        // On-prem distributions don't have managed control plane and are not cloud-native
        return context.SelectedDistribution.Value switch
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

    private bool IsMendixOnPrem(HomePageDistributionContext context)
    {
        // Mendix Cloud (SaaS/Dedicated) is never on-prem
        if (context.MendixDeploymentCategory == MendixDeploymentCategory.Cloud)
            return false;

        // Mendix Private Cloud - check the provider
        if (context.MendixDeploymentCategory == MendixDeploymentCategory.PrivateCloud)
        {
            // Azure, EKS, AKS, GKE are cloud-managed, not on-prem
            return context.MendixPrivateCloudProvider switch
            {
                MendixPrivateCloudProvider.Azure => false,
                MendixPrivateCloudProvider.EKS => false,
                MendixPrivateCloudProvider.AKS => false,
                MendixPrivateCloudProvider.GKE => false,
                MendixPrivateCloudProvider.OpenShift => true,
                _ => false
            };
        }

        // Mendix Other K8s - check the provider (Rancher, K3s, GenericK8s are self-managed)
        if (context.MendixDeploymentCategory == MendixDeploymentCategory.Other)
        {
            return context.MendixPrivateCloudProvider switch
            {
                MendixPrivateCloudProvider.Rancher => true,
                MendixPrivateCloudProvider.K3s => true,
                MendixPrivateCloudProvider.GenericK8s => true,
                _ => true
            };
        }

        return false;
    }

    /// <inheritdoc/>
    public bool IsOpenShiftDistribution(Distribution? distro)
    {
        if (distro == null) return false;

        return distro.Value switch
        {
            Distribution.OpenShift => true,
            Distribution.OpenShiftROSA => true,
            Distribution.OpenShiftARO => true,
            Distribution.OpenShiftDedicated => true,
            Distribution.OpenShiftIBM => true,
            _ => false
        };
    }

    /// <inheritdoc/>
    public bool IsManagedControlPlane(Distribution? distro)
    {
        if (distro == null) return false;

        var config = _distributionService.GetConfig(distro.Value);
        return config.HasManagedControlPlane;
    }

    /// <inheritdoc/>
    public IEnumerable<DistributionConfig> GetAllDistributions()
    {
        return _distributionService.GetAll();
    }

    /// <inheritdoc/>
    public IEnumerable<DistributionConfig> GetDistributionsByTag(string tag)
    {
        return _distributionService.GetByTag(tag);
    }
}
