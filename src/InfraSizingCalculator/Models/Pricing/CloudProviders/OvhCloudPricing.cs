using InfraSizingCalculator.Models.Pricing.Base;

namespace InfraSizingCalculator.Models.Pricing.CloudProviders;

/// <summary>
/// OVHcloud pricing implementation.
/// Includes OVH Managed Kubernetes Service.
/// Key advantage: European hosting, competitive pricing, GDPR compliance.
/// Prices based on OVHcloud public pricing as of 2025.
/// </summary>
public sealed class OvhCloudPricing : CloudProviderPricingBase
{
    /// <inheritdoc />
    public override CloudProvider Provider => CloudProvider.OVH;

    /// <inheritdoc />
    public override string DefaultRegion => "gra";

    /// <inheritdoc />
    protected override string ProviderDisplayName => "OVHcloud";

    /// <inheritdoc />
    public override IReadOnlyList<RegionInfo> GetAvailableRegions() => CloudRegions.GenericRegions;

    /// <inheritdoc />
    public override ComputePricing GetComputePricing(string? region = null)
    {
        return new ComputePricing
        {
            CpuPerHour = 0.010m,
            RamGBPerHour = 0.003m,
            ManagedControlPlanePerHour = 0m, // Free control plane
            InstanceTypePrices = new Dictionary<string, decimal>
            {
                ["b2-7"] = 0.026m,            // 2 vCPU, 7GB - €18/mo
                ["b2-15"] = 0.052m,           // 4 vCPU, 15GB
                ["b2-30"] = 0.104m,           // 8 vCPU, 30GB
                ["b2-60"] = 0.208m,           // 16 vCPU, 60GB
                ["c2-7"] = 0.032m,            // CPU-optimized
                ["c2-15"] = 0.064m,
                ["r2-30"] = 0.070m,           // RAM-optimized
                ["r2-60"] = 0.140m
            }
        };
    }

    /// <inheritdoc />
    public override StoragePricing GetStoragePricing(string? region = null)
    {
        return new StoragePricing
        {
            SsdPerGBMonth = 0.04m,
            HddPerGBMonth = 0.02m,
            ObjectStoragePerGBMonth = 0.01m,
            BackupPerGBMonth = 0.02m,
            RegistryPerGBMonth = 0.02m
        };
    }

    /// <inheritdoc />
    public override NetworkPricing GetNetworkPricing(string? region = null)
    {
        return new NetworkPricing
        {
            EgressPerGB = 0.01m,
            LoadBalancerPerHour = 0.012m,
            NatGatewayPerHour = 0m,
            VpnPerHour = 0.03m,
            PublicIpPerHour = 0m              // Included
        };
    }

    /// <inheritdoc />
    public override decimal GetControlPlaneCostPerHour(bool isHA = false)
    {
        // OVH K8s: Free control plane
        return 0m;
    }
}
