using InfraSizingCalculator.Models.Pricing.Base;

namespace InfraSizingCalculator.Models.Pricing.CloudProviders;

/// <summary>
/// Linode (Akamai) Cloud pricing implementation.
/// Includes LKE (Linode Kubernetes Engine).
/// Key advantage: Simple pricing, good developer experience.
/// Prices based on Linode public pricing as of 2025.
/// </summary>
public sealed class LinodeCloudPricing : CloudProviderPricingBase
{
    /// <inheritdoc />
    public override CloudProvider Provider => CloudProvider.Linode;

    /// <inheritdoc />
    public override string DefaultRegion => "us-east";

    /// <inheritdoc />
    protected override string ProviderDisplayName => "Linode";

    /// <inheritdoc />
    public override IReadOnlyList<RegionInfo> GetAvailableRegions() => CloudRegions.LinodeRegions;

    /// <inheritdoc />
    public override ComputePricing GetComputePricing(string? region = null)
    {
        return new ComputePricing
        {
            CpuPerHour = 0.015m,
            RamGBPerHour = 0.003m,
            ManagedControlPlanePerHour = 0m, // LKE free tier
            InstanceTypePrices = new Dictionary<string, decimal>
            {
                ["g6-standard-2"] = 0.018m,    // 2GB - $12/mo
                ["g6-standard-4"] = 0.036m,    // 4GB - $24/mo
                ["g6-standard-6"] = 0.054m,    // 8GB - $36/mo
                ["g6-standard-8"] = 0.072m,    // 16GB
                ["g6-dedicated-4"] = 0.054m,   // Dedicated 2 vCPU
                ["g6-dedicated-8"] = 0.108m,   // Dedicated 4 vCPU
                ["g6-dedicated-16"] = 0.216m   // Dedicated 8 vCPU
            }
        };
    }

    /// <inheritdoc />
    public override StoragePricing GetStoragePricing(string? region = null)
    {
        return new StoragePricing
        {
            SsdPerGBMonth = 0.10m,
            HddPerGBMonth = 0.10m,
            ObjectStoragePerGBMonth = 0.02m,
            BackupPerGBMonth = 0.025m,  // 25% of Linode cost
            RegistryPerGBMonth = 0m     // Use external
        };
    }

    /// <inheritdoc />
    public override NetworkPricing GetNetworkPricing(string? region = null)
    {
        return new NetworkPricing
        {
            EgressPerGB = 0.01m,        // After free tier
            LoadBalancerPerHour = 0.015m,
            NatGatewayPerHour = 0m,
            VpnPerHour = 0m,
            PublicIpPerHour = 0m        // Included
        };
    }

    /// <inheritdoc />
    public override decimal GetControlPlaneCostPerHour(bool isHA = false)
    {
        // LKE: Free standard, ~$60/mo for HA
        return isHA ? 0.083m : 0m;
    }
}
