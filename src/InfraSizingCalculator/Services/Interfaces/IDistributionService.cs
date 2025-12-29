using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;

namespace InfraSizingCalculator.Services.Interfaces;

public interface IDistributionService
{
    DistributionConfig GetConfig(Distribution distribution);
    IEnumerable<DistributionConfig> GetAll();
    IEnumerable<DistributionConfig> GetByTag(string tag);

    /// <summary>
    /// Gets distributions by deployment type (on-prem, cloud).
    /// </summary>
    IEnumerable<DistributionConfig> GetByDeploymentType(string deploymentType);

    /// <summary>
    /// Gets count of distributions by deployment type.
    /// </summary>
    int GetCountByDeploymentType(string deploymentType);

    /// <summary>
    /// Gets cloud distributions by category (major, openshift, rancher, tanzu, ubuntu, developer).
    /// </summary>
    IEnumerable<DistributionConfig> GetCloudByCategory(string category);

    /// <summary>
    /// Gets count of cloud distributions by category.
    /// </summary>
    int GetCountByCloudCategory(string category);

    /// <summary>
    /// Gets filtered distributions with optional text search.
    /// </summary>
    IEnumerable<DistributionConfig> GetFiltered(string? deploymentType, string? cloudCategory, string? searchText);

    /// <summary>
    /// Gets the cloud category for a given distribution.
    /// </summary>
    string? GetCloudCategory(Distribution distribution);
}
