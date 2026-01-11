using InfraSizingCalculator.Models.Pricing.Base;

namespace InfraSizingCalculator.Models.Pricing.CloudProviders;

/// <summary>
/// IBM Cloud pricing implementation.
/// Includes IKS (IBM Kubernetes Service) and ROKS (Red Hat OpenShift on IBM Cloud).
/// Key advantage: Free control plane, strong enterprise support.
/// Prices based on IBM Cloud public pricing as of 2025.
/// </summary>
public sealed class IbmCloudPricing : CloudProviderPricingBase
{
    /// <inheritdoc />
    public override CloudProvider Provider => CloudProvider.IBM;

    /// <inheritdoc />
    public override string DefaultRegion => "us-south";

    /// <inheritdoc />
    protected override string ProviderDisplayName => "IBM Cloud";

    /// <inheritdoc />
    public override IReadOnlyList<RegionInfo> GetAvailableRegions() => CloudRegions.IBMRegions;

    /// <inheritdoc />
    public override ComputePricing GetComputePricing(string? region = null)
    {
        return new ComputePricing
        {
            CpuPerHour = 0.05m,
            RamGBPerHour = 0.007m,
            ManagedControlPlanePerHour = 0m, // IKS control plane free
            OpenShiftServiceFeePerWorkerHour = 0.20m, // ROKS fee
            InstanceTypePrices = new Dictionary<string, decimal>
            {
                ["bx2-4x16"] = 0.192m,      // 4 vCPU, 16GB
                ["bx2-8x32"] = 0.384m,
                ["bx2-16x64"] = 0.768m,
                ["cx2-4x8"] = 0.17m,        // Compute optimized
                ["mx2-4x32"] = 0.25m        // Memory optimized
            }
        };
    }

    /// <inheritdoc />
    public override StoragePricing GetStoragePricing(string? region = null)
    {
        return new StoragePricing
        {
            SsdPerGBMonth = 0.10m,
            HddPerGBMonth = 0.05m,
            ObjectStoragePerGBMonth = 0.022m,
            BackupPerGBMonth = 0.05m,
            RegistryPerGBMonth = 0.10m
        };
    }

    /// <inheritdoc />
    public override NetworkPricing GetNetworkPricing(string? region = null)
    {
        return new NetworkPricing
        {
            EgressPerGB = 0.09m,
            LoadBalancerPerHour = 0.025m,
            NatGatewayPerHour = 0.045m,
            VpnPerHour = 0.05m,
            PublicIpPerHour = 0.004m
        };
    }

    /// <inheritdoc />
    public override decimal GetControlPlaneCostPerHour(bool isHA = false)
    {
        // IKS: Free control plane
        return 0m;
    }

    /// <summary>
    /// Get ROKS (Red Hat OpenShift on IBM Cloud) specific pricing
    /// </summary>
    public decimal GetRoksWorkerFeePerHour() => 0.20m;
}
