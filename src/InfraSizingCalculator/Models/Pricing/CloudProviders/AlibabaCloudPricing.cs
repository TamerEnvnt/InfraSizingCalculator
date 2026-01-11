using InfraSizingCalculator.Models.Pricing.Base;

namespace InfraSizingCalculator.Models.Pricing.CloudProviders;

/// <summary>
/// Alibaba Cloud pricing implementation.
/// Includes ACK (Alibaba Container Service for Kubernetes).
/// Key advantage: Strong presence in Asia-Pacific region.
/// Prices based on Alibaba Cloud public pricing as of 2025.
/// </summary>
public sealed class AlibabaCloudPricing : CloudProviderPricingBase
{
    /// <inheritdoc />
    public override CloudProvider Provider => CloudProvider.Alibaba;

    /// <inheritdoc />
    public override string DefaultRegion => "cn-hangzhou";

    /// <inheritdoc />
    protected override string ProviderDisplayName => "Alibaba Cloud";

    /// <inheritdoc />
    public override IReadOnlyList<RegionInfo> GetAvailableRegions() => CloudRegions.AlibabaRegions;

    /// <inheritdoc />
    public override ComputePricing GetComputePricing(string? region = null)
    {
        var isInternational = !region?.StartsWith("cn-") ?? false;
        var multiplier = isInternational ? 1.1m : 1.0m;

        return new ComputePricing
        {
            CpuPerHour = 0.04m * multiplier,
            RamGBPerHour = 0.005m * multiplier,
            ManagedControlPlanePerHour = 0m, // ACK free tier
            InstanceTypePrices = new Dictionary<string, decimal>
            {
                ["ecs.g6.large"] = 0.096m * multiplier,
                ["ecs.g6.xlarge"] = 0.192m * multiplier,
                ["ecs.g6.2xlarge"] = 0.384m * multiplier,
                ["ecs.c6.xlarge"] = 0.17m * multiplier,
                ["ecs.r6.xlarge"] = 0.25m * multiplier
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
            BackupPerGBMonth = 0.05m,
            RegistryPerGBMonth = 0.08m
        };
    }

    /// <inheritdoc />
    public override NetworkPricing GetNetworkPricing(string? region = null)
    {
        return new NetworkPricing
        {
            EgressPerGB = 0.12m,
            LoadBalancerPerHour = 0.02m,
            NatGatewayPerHour = 0.04m,
            VpnPerHour = 0.05m,
            PublicIpPerHour = 0.003m
        };
    }

    /// <inheritdoc />
    public override decimal GetControlPlaneCostPerHour(bool isHA = false)
    {
        return isHA ? 0.10m : 0m;
    }
}
