using InfraSizingCalculator.Models.Pricing.Base;

namespace InfraSizingCalculator.Models.Pricing.CloudProviders;

/// <summary>
/// Vultr Cloud pricing implementation.
/// Includes VKE (Vultr Kubernetes Engine).
/// Key advantage: Simple pricing, global presence, developer-friendly.
/// Prices based on Vultr public pricing as of 2025.
/// </summary>
public sealed class VultrCloudPricing : CloudProviderPricingBase
{
    /// <inheritdoc />
    public override CloudProvider Provider => CloudProvider.Vultr;

    /// <inheritdoc />
    public override string DefaultRegion => "ewr";

    /// <inheritdoc />
    protected override string ProviderDisplayName => "Vultr";

    /// <inheritdoc />
    public override IReadOnlyList<RegionInfo> GetAvailableRegions() => CloudRegions.VultrRegions;

    /// <inheritdoc />
    public override ComputePricing GetComputePricing(string? region = null)
    {
        return new ComputePricing
        {
            CpuPerHour = 0.012m,
            RamGBPerHour = 0.003m,
            ManagedControlPlanePerHour = 0m, // VKE free control plane
            InstanceTypePrices = new Dictionary<string, decimal>
            {
                ["vc2-1c-2gb"] = 0.015m,      // $10/mo
                ["vc2-2c-4gb"] = 0.030m,      // $20/mo
                ["vc2-4c-8gb"] = 0.060m,      // $40/mo
                ["vc2-6c-16gb"] = 0.119m,     // $80/mo
                ["vc2-8c-32gb"] = 0.238m,     // $160/mo
                ["vhf-2c-4gb"] = 0.036m,      // High Frequency
                ["vhf-4c-8gb"] = 0.071m,
                ["vhf-8c-32gb"] = 0.286m
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
            BackupPerGBMonth = 0.05m,
            RegistryPerGBMonth = 0m  // Use external
        };
    }

    /// <inheritdoc />
    public override NetworkPricing GetNetworkPricing(string? region = null)
    {
        return new NetworkPricing
        {
            EgressPerGB = 0.01m,
            LoadBalancerPerHour = 0.015m,  // $10/mo
            NatGatewayPerHour = 0m,
            VpnPerHour = 0m,
            PublicIpPerHour = 0m           // Included
        };
    }

    /// <inheritdoc />
    public override decimal GetControlPlaneCostPerHour(bool isHA = false)
    {
        // VKE: Free control plane
        return 0m;
    }
}
