using InfraSizingCalculator.Models.Pricing.Base;

namespace InfraSizingCalculator.Models.Pricing.CloudProviders;

/// <summary>
/// Tencent Cloud pricing implementation.
/// Includes TKE (Tencent Kubernetes Engine).
/// Key advantage: Strong presence in China, competitive pricing.
/// Prices based on Tencent Cloud public pricing as of 2025.
/// </summary>
public sealed class TencentCloudPricing : CloudProviderPricingBase
{
    /// <inheritdoc />
    public override CloudProvider Provider => CloudProvider.Tencent;

    /// <inheritdoc />
    public override string DefaultRegion => "ap-guangzhou";

    /// <inheritdoc />
    protected override string ProviderDisplayName => "Tencent Cloud";

    /// <inheritdoc />
    public override IReadOnlyList<RegionInfo> GetAvailableRegions() => CloudRegions.GenericRegions;

    /// <inheritdoc />
    public override ComputePricing GetComputePricing(string? region = null)
    {
        var isInternational = !region?.StartsWith("ap-") ?? true;
        var multiplier = isInternational ? 1.15m : 1.0m;

        return new ComputePricing
        {
            CpuPerHour = 0.035m * multiplier,
            RamGBPerHour = 0.005m * multiplier,
            ManagedControlPlanePerHour = 0m, // TKE free tier
            InstanceTypePrices = new Dictionary<string, decimal>
            {
                ["S5.MEDIUM4"] = 0.06m * multiplier,    // 2 vCPU, 4GB
                ["S5.MEDIUM8"] = 0.08m * multiplier,    // 2 vCPU, 8GB
                ["S5.LARGE8"] = 0.12m * multiplier,     // 4 vCPU, 8GB
                ["S5.LARGE16"] = 0.16m * multiplier,    // 4 vCPU, 16GB
                ["S5.2XLARGE16"] = 0.24m * multiplier,  // 8 vCPU, 16GB
                ["S5.2XLARGE32"] = 0.32m * multiplier   // 8 vCPU, 32GB
            }
        };
    }

    /// <inheritdoc />
    public override StoragePricing GetStoragePricing(string? region = null)
    {
        return new StoragePricing
        {
            SsdPerGBMonth = 0.07m,
            HddPerGBMonth = 0.04m,
            ObjectStoragePerGBMonth = 0.02m,
            BackupPerGBMonth = 0.04m,
            RegistryPerGBMonth = 0.05m
        };
    }

    /// <inheritdoc />
    public override NetworkPricing GetNetworkPricing(string? region = null)
    {
        return new NetworkPricing
        {
            EgressPerGB = 0.08m,
            LoadBalancerPerHour = 0.02m,
            NatGatewayPerHour = 0.03m,
            VpnPerHour = 0.05m,
            PublicIpPerHour = 0.003m
        };
    }

    /// <inheritdoc />
    public override decimal GetControlPlaneCostPerHour(bool isHA = false)
    {
        // TKE: Free standard, managed HA available
        return isHA ? 0.08m : 0m;
    }
}
