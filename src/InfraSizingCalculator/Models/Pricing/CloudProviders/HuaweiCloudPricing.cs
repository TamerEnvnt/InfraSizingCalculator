using InfraSizingCalculator.Models.Pricing.Base;

namespace InfraSizingCalculator.Models.Pricing.CloudProviders;

/// <summary>
/// Huawei Cloud pricing implementation.
/// Includes CCE (Cloud Container Engine).
/// Key advantage: Strong presence in China and developing markets.
/// Prices based on Huawei Cloud public pricing as of 2025.
/// </summary>
public sealed class HuaweiCloudPricing : CloudProviderPricingBase
{
    /// <inheritdoc />
    public override CloudProvider Provider => CloudProvider.Huawei;

    /// <inheritdoc />
    public override string DefaultRegion => "cn-north-4";

    /// <inheritdoc />
    protected override string ProviderDisplayName => "Huawei Cloud";

    /// <inheritdoc />
    public override IReadOnlyList<RegionInfo> GetAvailableRegions() => CloudRegions.GenericRegions;

    /// <inheritdoc />
    public override ComputePricing GetComputePricing(string? region = null)
    {
        var isInternational = !region?.StartsWith("cn-") ?? true;
        var multiplier = isInternational ? 1.1m : 1.0m;

        return new ComputePricing
        {
            CpuPerHour = 0.038m * multiplier,
            RamGBPerHour = 0.005m * multiplier,
            ManagedControlPlanePerHour = 0m, // CCE free tier
            InstanceTypePrices = new Dictionary<string, decimal>
            {
                ["s6.medium.2"] = 0.05m * multiplier,   // 1 vCPU, 2GB
                ["s6.large.2"] = 0.08m * multiplier,    // 2 vCPU, 4GB
                ["s6.xlarge.2"] = 0.16m * multiplier,   // 4 vCPU, 8GB
                ["s6.2xlarge.2"] = 0.32m * multiplier,  // 8 vCPU, 16GB
                ["c6.xlarge.2"] = 0.14m * multiplier,   // Compute-optimized
                ["m6.xlarge.8"] = 0.24m * multiplier    // Memory-optimized
            }
        };
    }

    /// <inheritdoc />
    public override StoragePricing GetStoragePricing(string? region = null)
    {
        return new StoragePricing
        {
            SsdPerGBMonth = 0.08m,
            HddPerGBMonth = 0.04m,
            ObjectStoragePerGBMonth = 0.02m,
            BackupPerGBMonth = 0.04m,
            RegistryPerGBMonth = 0.06m
        };
    }

    /// <inheritdoc />
    public override NetworkPricing GetNetworkPricing(string? region = null)
    {
        return new NetworkPricing
        {
            EgressPerGB = 0.10m,
            LoadBalancerPerHour = 0.02m,
            NatGatewayPerHour = 0.04m,
            VpnPerHour = 0.05m,
            PublicIpPerHour = 0.003m
        };
    }

    /// <inheritdoc />
    public override decimal GetControlPlaneCostPerHour(bool isHA = false)
    {
        // CCE: Free standard, HA available
        return isHA ? 0.09m : 0m;
    }
}
