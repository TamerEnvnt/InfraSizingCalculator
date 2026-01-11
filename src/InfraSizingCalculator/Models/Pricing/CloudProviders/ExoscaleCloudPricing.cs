using InfraSizingCalculator.Models.Pricing.Base;

namespace InfraSizingCalculator.Models.Pricing.CloudProviders;

/// <summary>
/// Exoscale pricing implementation.
/// Includes SKS (Scalable Kubernetes Service).
/// Key advantage: Swiss-based, GDPR compliant, strong European presence.
/// Prices based on Exoscale public pricing as of 2025.
/// </summary>
public sealed class ExoscaleCloudPricing : CloudProviderPricingBase
{
    /// <inheritdoc />
    public override CloudProvider Provider => CloudProvider.Exoscale;

    /// <inheritdoc />
    public override string DefaultRegion => "ch-gva-2";

    /// <inheritdoc />
    protected override string ProviderDisplayName => "Exoscale";

    /// <inheritdoc />
    public override IReadOnlyList<RegionInfo> GetAvailableRegions() => CloudRegions.ExoscaleRegions;

    /// <inheritdoc />
    public override ComputePricing GetComputePricing(string? region = null)
    {
        return new ComputePricing
        {
            CpuPerHour = 0.012m,
            RamGBPerHour = 0.003m,
            ManagedControlPlanePerHour = 0m, // SKS free control plane
            InstanceTypePrices = new Dictionary<string, decimal>
            {
                ["micro"] = 0.008m,            // 1 vCPU, 512MB
                ["tiny"] = 0.015m,             // 1 vCPU, 1GB
                ["small"] = 0.023m,            // 2 vCPU, 2GB
                ["medium"] = 0.046m,           // 2 vCPU, 4GB
                ["large"] = 0.069m,            // 4 vCPU, 8GB
                ["extra-large"] = 0.115m,      // 4 vCPU, 16GB
                ["huge"] = 0.231m,             // 8 vCPU, 32GB
                ["mega"] = 0.462m,             // 12 vCPU, 64GB
                ["titan"] = 0.923m,            // 16 vCPU, 128GB
                ["gpu-small"] = 0.577m,        // GPU instances
                ["gpu-medium"] = 1.154m
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
            ObjectStoragePerGBMonth = 0.02m,  // S3-compatible
            BackupPerGBMonth = 0.03m,
            RegistryPerGBMonth = 0.02m
        };
    }

    /// <inheritdoc />
    public override NetworkPricing GetNetworkPricing(string? region = null)
    {
        return new NetworkPricing
        {
            EgressPerGB = 0.02m,               // 1TB free then charged
            LoadBalancerPerHour = 0.015m,
            NatGatewayPerHour = 0m,
            VpnPerHour = 0m,
            PublicIpPerHour = 0.003m
        };
    }

    /// <inheritdoc />
    public override decimal GetControlPlaneCostPerHour(bool isHA = false)
    {
        // SKS: Free control plane (always HA)
        return 0m;
    }
}
