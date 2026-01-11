namespace InfraSizingCalculator.Models.Pricing.Base;

/// <summary>
/// Interface for cloud provider pricing implementations.
/// Each cloud provider (AWS, Azure, GCP, etc.) implements this interface.
/// Follows Interface Segregation Principle - focused on pricing operations.
/// </summary>
public interface ICloudProviderPricing
{
    /// <summary>
    /// The cloud provider this pricing applies to
    /// </summary>
    CloudProvider Provider { get; }

    /// <summary>
    /// Default region for this provider
    /// </summary>
    string DefaultRegion { get; }

    /// <summary>
    /// Get pricing model for a specific region
    /// </summary>
    /// <param name="region">Region code (e.g., us-east-1, eastus)</param>
    /// <returns>Complete pricing model for the region</returns>
    PricingModel GetPricing(string? region = null);

    /// <summary>
    /// Get available regions for this provider
    /// </summary>
    IReadOnlyList<RegionInfo> GetAvailableRegions();

    /// <summary>
    /// Check if a region is supported by this provider
    /// </summary>
    bool IsRegionSupported(string region);

    /// <summary>
    /// Get compute pricing for the provider
    /// </summary>
    ComputePricing GetComputePricing(string? region = null);

    /// <summary>
    /// Get storage pricing for the provider
    /// </summary>
    StoragePricing GetStoragePricing(string? region = null);

    /// <summary>
    /// Get network pricing for the provider
    /// </summary>
    NetworkPricing GetNetworkPricing(string? region = null);

    /// <summary>
    /// Get managed Kubernetes control plane cost per hour
    /// </summary>
    decimal GetControlPlaneCostPerHour(bool isHA = false);

    /// <summary>
    /// Get instance type hourly price
    /// </summary>
    decimal GetInstancePrice(string instanceType, string? region = null);

    /// <summary>
    /// Calculate monthly cost for given resources
    /// </summary>
    decimal CalculateMonthlyCost(int cpuCores, int ramGB, int storageGB, string? region = null);
}
