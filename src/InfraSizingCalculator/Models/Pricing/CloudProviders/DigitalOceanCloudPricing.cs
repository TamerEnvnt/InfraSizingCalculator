using InfraSizingCalculator.Models.Pricing.Base;

namespace InfraSizingCalculator.Models.Pricing.CloudProviders;

/// <summary>
/// DigitalOcean pricing implementation.
/// Includes DOKS, Droplets, Volumes, and related services.
/// Key advantage: Simple, predictable pricing, free control plane.
/// Prices based on DigitalOcean public pricing as of 2025.
/// </summary>
public sealed class DigitalOceanCloudPricing : CloudProviderPricingBase
{
    /// <inheritdoc />
    public override CloudProvider Provider => CloudProvider.DigitalOcean;

    /// <inheritdoc />
    public override string DefaultRegion => "nyc1";

    /// <inheritdoc />
    protected override string ProviderDisplayName => "DigitalOcean";

    /// <inheritdoc />
    public override IReadOnlyList<RegionInfo> GetAvailableRegions() => CloudRegions.DigitalOceanRegions;

    /// <inheritdoc />
    public override ComputePricing GetComputePricing(string? region = null)
    {
        return new ComputePricing
        {
            CpuPerHour = 0.018m,
            RamGBPerHour = 0.003m,
            ManagedControlPlanePerHour = 0m, // DOKS free tier
            InstanceTypePrices = new Dictionary<string, decimal>
            {
                ["s-2vcpu-4gb"] = 0.030m,      // $22/mo
                ["s-4vcpu-8gb"] = 0.065m,      // $48/mo
                ["g-2vcpu-8gb"] = 0.091m,      // General Purpose
                ["g-4vcpu-16gb"] = 0.182m,
                ["g-8vcpu-32gb"] = 0.364m,
                ["c-4vcpu-8gb"] = 0.126m,      // CPU-Optimized
                ["m-2vcpu-16gb"] = 0.126m      // Memory-Optimized
            }
        };
    }

    /// <inheritdoc />
    public override StoragePricing GetStoragePricing(string? region = null)
    {
        return new StoragePricing
        {
            SsdPerGBMonth = 0.10m,            // Block Storage
            HddPerGBMonth = 0.10m,
            ObjectStoragePerGBMonth = 0.02m,   // Spaces
            BackupPerGBMonth = 0.05m,
            RegistryPerGBMonth = 0.02m        // Container Registry
        };
    }

    /// <inheritdoc />
    public override NetworkPricing GetNetworkPricing(string? region = null)
    {
        return new NetworkPricing
        {
            EgressPerGB = 0.01m,             // Very competitive
            LoadBalancerPerHour = 0.015m,    // $10/mo base
            NatGatewayPerHour = 0m,          // Included
            VpnPerHour = 0m,
            PublicIpPerHour = 0m             // Included with droplet
        };
    }

    /// <inheritdoc />
    public override decimal GetControlPlaneCostPerHour(bool isHA = false)
    {
        // DOKS: Free standard, ~$40/mo for HA
        return isHA ? 0.055m : 0m;
    }
}
