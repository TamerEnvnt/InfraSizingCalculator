using InfraSizingCalculator.Models.Pricing.Base;

namespace InfraSizingCalculator.Models.Pricing.CloudProviders;

/// <summary>
/// Google Cloud Platform pricing implementation.
/// Includes GKE, Compute Engine, Persistent Disk, and related services.
/// Prices based on GCP public pricing as of 2025.
/// </summary>
public sealed class GcpCloudPricing : CloudProviderPricingBase
{
    /// <inheritdoc />
    public override CloudProvider Provider => CloudProvider.GCP;

    /// <inheritdoc />
    public override string DefaultRegion => "us-central1";

    /// <inheritdoc />
    protected override string ProviderDisplayName => "GCP";

    /// <inheritdoc />
    public override IReadOnlyList<RegionInfo> GetAvailableRegions() => CloudRegions.GCPRegions;

    /// <inheritdoc />
    public override ComputePricing GetComputePricing(string? region = null)
    {
        var multiplier = GetRegionalMultiplier(region ?? DefaultRegion);

        return new ComputePricing
        {
            // Based on e2-standard-4 (~$0.134/hour for 4 vCPU, 16GB)
            CpuPerHour = 0.0335m * multiplier,
            RamGBPerHour = 0.0045m * multiplier,
            ManagedControlPlanePerHour = 0.10m, // GKE Autopilot
            OpenShiftServiceFeePerWorkerHour = 0.171m, // OSD worker fee
            InstanceTypePrices = new Dictionary<string, decimal>
            {
                ["e2-medium"] = 0.0335m * multiplier,
                ["e2-standard-2"] = 0.067m * multiplier,
                ["e2-standard-4"] = 0.134m * multiplier,
                ["e2-standard-8"] = 0.268m * multiplier,
                ["e2-standard-16"] = 0.536m * multiplier,
                ["n2-standard-4"] = 0.194m * multiplier,
                ["n2-standard-8"] = 0.388m * multiplier,
                ["c2-standard-4"] = 0.209m * multiplier,
                ["n2-highmem-4"] = 0.262m * multiplier
            }
        };
    }

    /// <inheritdoc />
    public override StoragePricing GetStoragePricing(string? region = null)
    {
        var multiplier = GetRegionalMultiplier(region ?? DefaultRegion);

        return new StoragePricing
        {
            SsdPerGBMonth = 0.17m * multiplier,            // PD-SSD
            HddPerGBMonth = 0.04m * multiplier,            // PD-Standard
            ObjectStoragePerGBMonth = 0.02m * multiplier,   // GCS Standard
            BackupPerGBMonth = 0.05m * multiplier,
            RegistryPerGBMonth = 0.10m * multiplier        // Artifact Registry
        };
    }

    /// <inheritdoc />
    public override NetworkPricing GetNetworkPricing(string? region = null)
    {
        return new NetworkPricing
        {
            EgressPerGB = 0.12m,            // Premium tier
            LoadBalancerPerHour = 0.025m,
            NatGatewayPerHour = 0.045m,
            VpnPerHour = 0.05m,
            PublicIpPerHour = 0.004m
        };
    }

    /// <inheritdoc />
    public override decimal GetControlPlaneCostPerHour(bool isHA = false)
    {
        // GKE: $0.10/hr for Autopilot, first zonal cluster free
        return 0.10m;
    }

    /// <summary>
    /// Get OSD (OpenShift Dedicated on GCP) specific pricing
    /// </summary>
    public decimal GetOsdWorkerFeePerHour() => 0.171m;

    /// <summary>
    /// GCP sustained use discount (automatic for VMs running > 25% of month)
    /// </summary>
    public decimal GetSustainedUseDiscount() => 0.20m;

    private static decimal GetRegionalMultiplier(string region)
    {
        return region switch
        {
            "us-central1" or "us-east1" or "us-west1" => 1.0m,
            "us-east4" or "us-west2" or "us-west3" or "us-west4" => 1.05m,
            "europe-west1" or "europe-west4" => 1.05m,
            "europe-west2" or "europe-west3" => 1.1m,
            "asia-southeast1" or "asia-east1" => 1.1m,
            "asia-northeast1" => 1.2m,
            "australia-southeast1" => 1.15m,
            "asia-south1" => 0.95m,
            "me-west1" => 1.25m,
            "southamerica-east1" => 1.3m,
            _ => 1.0m
        };
    }
}
