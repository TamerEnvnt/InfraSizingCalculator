using InfraSizingCalculator.Models.Pricing.Base;

namespace InfraSizingCalculator.Models.Pricing.CloudProviders;

/// <summary>
/// Hetzner Cloud pricing implementation.
/// Key advantage: Extremely competitive European pricing.
/// Prices based on Hetzner public pricing as of 2025.
/// </summary>
public sealed class HetznerCloudPricing : CloudProviderPricingBase
{
    /// <inheritdoc />
    public override CloudProvider Provider => CloudProvider.Hetzner;

    /// <inheritdoc />
    public override string DefaultRegion => "fsn1";

    /// <inheritdoc />
    protected override string ProviderDisplayName => "Hetzner";

    /// <inheritdoc />
    public override IReadOnlyList<RegionInfo> GetAvailableRegions() => CloudRegions.HetznerRegions;

    /// <inheritdoc />
    public override ComputePricing GetComputePricing(string? region = null)
    {
        return new ComputePricing
        {
            CpuPerHour = 0.006m,     // Very competitive
            RamGBPerHour = 0.002m,
            ManagedControlPlanePerHour = 0m, // Hetzner K8s is self-managed
            InstanceTypePrices = new Dictionary<string, decimal>
            {
                // CX series (Intel Shared vCPU)
                ["cx11"] = 0.005m,       // 1 vCPU, 2GB - €3.29/mo
                ["cx21"] = 0.008m,       // 2 vCPU, 4GB - €5.83/mo
                ["cx31"] = 0.015m,       // 2 vCPU, 8GB
                ["cx41"] = 0.028m,       // 4 vCPU, 16GB
                ["cx51"] = 0.055m,       // 8 vCPU, 32GB

                // CPX series (AMD EPYC Shared)
                ["cpx11"] = 0.006m,
                ["cpx21"] = 0.011m,
                ["cpx31"] = 0.021m,
                ["cpx41"] = 0.041m,
                ["cpx51"] = 0.082m,

                // CCX series (AMD EPYC Dedicated)
                ["ccx13"] = 0.055m,      // 2 vCPU, 8GB dedicated
                ["ccx23"] = 0.082m,      // 4 vCPU, 16GB dedicated
                ["ccx33"] = 0.137m,      // 8 vCPU, 32GB dedicated
                ["ccx43"] = 0.274m,      // 16 vCPU, 64GB dedicated
                ["ccx53"] = 0.548m       // 32 vCPU, 128GB dedicated
            }
        };
    }

    /// <inheritdoc />
    public override StoragePricing GetStoragePricing(string? region = null)
    {
        return new StoragePricing
        {
            SsdPerGBMonth = 0.044m,           // Local SSD included
            HddPerGBMonth = 0.044m,
            ObjectStoragePerGBMonth = 0.02m,
            BackupPerGBMonth = 0.02m,
            RegistryPerGBMonth = 0m            // Use external
        };
    }

    /// <inheritdoc />
    public override NetworkPricing GetNetworkPricing(string? region = null)
    {
        return new NetworkPricing
        {
            EgressPerGB = 0m,                // 20TB free included!
            LoadBalancerPerHour = 0.008m,    // €5.83/mo
            NatGatewayPerHour = 0m,
            VpnPerHour = 0m,
            PublicIpPerHour = 0.001m         // €0.50/mo
        };
    }

    /// <inheritdoc />
    public override decimal GetControlPlaneCostPerHour(bool isHA = false)
    {
        // Hetzner doesn't have managed K8s - self-managed only
        return 0m;
    }
}
