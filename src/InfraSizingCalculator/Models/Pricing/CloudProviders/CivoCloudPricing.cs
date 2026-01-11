using InfraSizingCalculator.Models.Pricing.Base;

namespace InfraSizingCalculator.Models.Pricing.CloudProviders;

/// <summary>
/// Civo Cloud pricing implementation.
/// Includes Civo Kubernetes (built on K3s).
/// Key advantage: Super fast cluster provisioning (~90 seconds), developer-friendly.
/// Prices based on Civo public pricing as of 2025.
/// </summary>
public sealed class CivoCloudPricing : CloudProviderPricingBase
{
    /// <inheritdoc />
    public override CloudProvider Provider => CloudProvider.Civo;

    /// <inheritdoc />
    public override string DefaultRegion => "lon1";

    /// <inheritdoc />
    protected override string ProviderDisplayName => "Civo";

    /// <inheritdoc />
    public override IReadOnlyList<RegionInfo> GetAvailableRegions() => CloudRegions.CivoRegions;

    /// <inheritdoc />
    public override ComputePricing GetComputePricing(string? region = null)
    {
        return new ComputePricing
        {
            CpuPerHour = 0.0075m,
            RamGBPerHour = 0.0025m,
            ManagedControlPlanePerHour = 0m, // Free control plane
            InstanceTypePrices = new Dictionary<string, decimal>
            {
                ["g4s.xsmall"] = 0.0075m,      // 1 vCPU, 1GB - $5/mo
                ["g4s.small"] = 0.015m,        // 1 vCPU, 2GB - $10/mo
                ["g4s.medium"] = 0.030m,       // 2 vCPU, 4GB - $20/mo
                ["g4s.large"] = 0.060m,        // 4 vCPU, 8GB - $40/mo
                ["g4s.xlarge"] = 0.119m,       // 6 vCPU, 16GB - $80/mo
                ["g4s.2xlarge"] = 0.238m,      // 8 vCPU, 32GB - $160/mo
                ["g4p.small"] = 0.045m,        // Performance tier
                ["g4p.medium"] = 0.089m,
                ["g4p.large"] = 0.179m
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
            LoadBalancerPerHour = 0.015m,
            NatGatewayPerHour = 0m,
            VpnPerHour = 0m,
            PublicIpPerHour = 0m       // Included
        };
    }

    /// <inheritdoc />
    public override decimal GetControlPlaneCostPerHour(bool isHA = false)
    {
        // Civo: Free control plane (uses K3s lightweight control plane)
        return 0m;
    }
}
