using InfraSizingCalculator.Models.Pricing.Base;

namespace InfraSizingCalculator.Models.Pricing.CloudProviders;

/// <summary>
/// On-premises/self-hosted pricing implementation.
/// Returns zero for cloud-specific costs since infrastructure is owned.
/// Actual costs (hardware, power, cooling, space) handled separately.
/// </summary>
public sealed class OnPremCloudPricing : CloudProviderPricingBase
{
    /// <inheritdoc />
    public override CloudProvider Provider => CloudProvider.OnPrem;

    /// <inheritdoc />
    public override string DefaultRegion => "On-Premises";

    /// <inheritdoc />
    protected override string ProviderDisplayName => "On-Premises";

    /// <inheritdoc />
    public override IReadOnlyList<RegionInfo> GetAvailableRegions() => new List<RegionInfo>
    {
        new() { Code = "On-Premises", DisplayName = "On-Premises Data Center", Provider = CloudProvider.OnPrem }
    };

    /// <inheritdoc />
    public override ComputePricing GetComputePricing(string? region = null)
    {
        // On-premises: No hourly cloud compute costs
        // Hardware costs are typically calculated via TCO models
        return new ComputePricing
        {
            CpuPerHour = 0m,
            RamGBPerHour = 0m,
            ManagedControlPlanePerHour = 0m,
            InstanceTypePrices = new Dictionary<string, decimal>()
        };
    }

    /// <inheritdoc />
    public override StoragePricing GetStoragePricing(string? region = null)
    {
        // On-premises: No monthly cloud storage costs
        // Storage costs are part of hardware TCO
        return new StoragePricing
        {
            SsdPerGBMonth = 0m,
            HddPerGBMonth = 0m,
            ObjectStoragePerGBMonth = 0m,
            BackupPerGBMonth = 0m,
            RegistryPerGBMonth = 0m
        };
    }

    /// <inheritdoc />
    public override NetworkPricing GetNetworkPricing(string? region = null)
    {
        // On-premises: No egress or cloud network costs
        return new NetworkPricing
        {
            EgressPerGB = 0m,
            LoadBalancerPerHour = 0m,
            NatGatewayPerHour = 0m,
            VpnPerHour = 0m,
            PublicIpPerHour = 0m
        };
    }

    /// <inheritdoc />
    public override decimal GetControlPlaneCostPerHour(bool isHA = false)
    {
        // Self-managed control plane
        return 0m;
    }

}
