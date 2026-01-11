using InfraSizingCalculator.Models.Pricing.Base;

namespace InfraSizingCalculator.Models.Pricing.CloudProviders;

/// <summary>
/// Scaleway pricing implementation.
/// Includes Kosmos (Scaleway Kubernetes).
/// Key advantage: European hosting, simple pricing, developer-friendly.
/// Prices based on Scaleway public pricing as of 2025.
/// </summary>
public sealed class ScalewayCloudPricing : CloudProviderPricingBase
{
    /// <inheritdoc />
    public override CloudProvider Provider => CloudProvider.Scaleway;

    /// <inheritdoc />
    public override string DefaultRegion => "fr-par";

    /// <inheritdoc />
    protected override string ProviderDisplayName => "Scaleway";

    /// <inheritdoc />
    public override IReadOnlyList<RegionInfo> GetAvailableRegions() => CloudRegions.GenericRegions;

    /// <inheritdoc />
    public override ComputePricing GetComputePricing(string? region = null)
    {
        return new ComputePricing
        {
            CpuPerHour = 0.008m,
            RamGBPerHour = 0.002m,
            ManagedControlPlanePerHour = 0m, // Free control plane
            InstanceTypePrices = new Dictionary<string, decimal>
            {
                ["DEV1-S"] = 0.007m,           // 2 vCPU, 2GB - €0.0070/hr
                ["DEV1-M"] = 0.015m,           // 3 vCPU, 4GB
                ["DEV1-L"] = 0.030m,           // 4 vCPU, 8GB
                ["GP1-XS"] = 0.024m,           // General Purpose 4 vCPU
                ["GP1-S"] = 0.048m,            // 8 vCPU
                ["GP1-M"] = 0.096m,            // 16 vCPU
                ["GP1-L"] = 0.192m             // 32 vCPU
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
            ObjectStoragePerGBMonth = 0.01m,  // Very competitive
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
            NatGatewayPerHour = 0.01m,
            VpnPerHour = 0m,
            PublicIpPerHour = 0.002m
        };
    }

    /// <inheritdoc />
    public override decimal GetControlPlaneCostPerHour(bool isHA = false)
    {
        // Scaleway Kosmos: Free control plane
        return 0m;
    }
}
