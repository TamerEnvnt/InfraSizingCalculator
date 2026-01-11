using InfraSizingCalculator.Models.Pricing.Base;

namespace InfraSizingCalculator.Models.Pricing.CloudProviders;

/// <summary>
/// Oracle Cloud Infrastructure (OCI) pricing implementation.
/// Includes OKE, Compute, Block Volume, and related services.
/// Key advantage: Competitive pricing, OKE control plane free.
/// Prices based on OCI public pricing as of 2025.
/// </summary>
public sealed class OciCloudPricing : CloudProviderPricingBase
{
    /// <inheritdoc />
    public override CloudProvider Provider => CloudProvider.OCI;

    /// <inheritdoc />
    public override string DefaultRegion => "us-ashburn-1";

    /// <inheritdoc />
    protected override string ProviderDisplayName => "OCI";

    /// <inheritdoc />
    public override IReadOnlyList<RegionInfo> GetAvailableRegions() => CloudRegions.OCIRegions;

    /// <inheritdoc />
    public override ComputePricing GetComputePricing(string? region = null)
    {
        var multiplier = GetRegionalMultiplier(region ?? DefaultRegion);

        return new ComputePricing
        {
            // Based on VM.Standard.E4.Flex (~$0.03/OCPU/hour)
            CpuPerHour = 0.03m * multiplier,
            RamGBPerHour = 0.0015m * multiplier,
            ManagedControlPlanePerHour = 0m, // OKE basic is FREE
            InstanceTypePrices = new Dictionary<string, decimal>
            {
                ["VM.Standard.E4.Flex.1"] = 0.03m * multiplier,   // 1 OCPU
                ["VM.Standard.E4.Flex.2"] = 0.06m * multiplier,   // 2 OCPU
                ["VM.Standard.E4.Flex.4"] = 0.12m * multiplier,   // 4 OCPU
                ["VM.Standard.E4.Flex.8"] = 0.24m * multiplier,   // 8 OCPU
                ["VM.Standard3.Flex.4"] = 0.128m * multiplier,
                ["VM.Standard3.Flex.8"] = 0.256m * multiplier
            }
        };
    }

    /// <inheritdoc />
    public override StoragePricing GetStoragePricing(string? region = null)
    {
        return new StoragePricing
        {
            SsdPerGBMonth = 0.0255m,           // Block Volume Performance
            HddPerGBMonth = 0.0255m,           // Block Volume Balanced
            ObjectStoragePerGBMonth = 0.0255m,  // Object Storage Standard
            BackupPerGBMonth = 0.05m,
            RegistryPerGBMonth = 0.0255m       // OCIR
        };
    }

    /// <inheritdoc />
    public override NetworkPricing GetNetworkPricing(string? region = null)
    {
        return new NetworkPricing
        {
            EgressPerGB = 0.0085m,           // Very competitive
            LoadBalancerPerHour = 0.01m,     // Flexible LB
            NatGatewayPerHour = 0.03m,
            VpnPerHour = 0.04m,
            PublicIpPerHour = 0m             // Free reserved public IPs
        };
    }

    /// <inheritdoc />
    public override decimal GetControlPlaneCostPerHour(bool isHA = false)
    {
        // OKE: Basic is FREE, Enhanced is $0.10/hr (capped at $74.40/mo)
        return isHA ? 0.10m : 0m;
    }

    private static decimal GetRegionalMultiplier(string region)
    {
        return region switch
        {
            "us-ashburn-1" or "us-phoenix-1" => 1.0m,
            "uk-london-1" or "eu-frankfurt-1" => 1.05m,
            "ap-tokyo-1" => 1.1m,
            "ap-sydney-1" or "ap-melbourne-1" => 1.08m,
            "me-dubai-1" or "me-jeddah-1" => 1.15m,
            "sa-saopaulo-1" => 1.2m,
            _ => 1.0m
        };
    }
}
