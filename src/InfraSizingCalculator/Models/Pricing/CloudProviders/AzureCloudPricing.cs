using InfraSizingCalculator.Models.Pricing.Base;

namespace InfraSizingCalculator.Models.Pricing.CloudProviders;

/// <summary>
/// Microsoft Azure pricing implementation.
/// Includes AKS, Azure VMs, Managed Disks, and related services.
/// Key advantage: FREE cross-AZ data transfer within regions.
/// Prices based on Azure public pricing as of 2025.
/// </summary>
public sealed class AzureCloudPricing : CloudProviderPricingBase
{
    /// <inheritdoc />
    public override CloudProvider Provider => CloudProvider.Azure;

    /// <inheritdoc />
    public override string DefaultRegion => "eastus";

    /// <inheritdoc />
    protected override string ProviderDisplayName => "Azure";

    /// <inheritdoc />
    public override IReadOnlyList<RegionInfo> GetAvailableRegions() => CloudRegions.AzureRegions;

    /// <inheritdoc />
    public override ComputePricing GetComputePricing(string? region = null)
    {
        var multiplier = GetRegionalMultiplier(region ?? DefaultRegion);

        return new ComputePricing
        {
            // Based on Standard_D4s_v5 (~$0.192/hour for 4 vCPU, 16GB)
            CpuPerHour = 0.048m * multiplier,
            RamGBPerHour = 0.006m * multiplier,
            ManagedControlPlanePerHour = 0m, // AKS free tier
            OpenShiftServiceFeePerWorkerHour = 0.35m, // ARO worker fee
            InstanceTypePrices = new Dictionary<string, decimal>
            {
                ["Standard_B2ms"] = 0.0832m * multiplier,
                ["Standard_D2s_v5"] = 0.096m * multiplier,
                ["Standard_D4s_v5"] = 0.192m * multiplier,
                ["Standard_D8s_v5"] = 0.384m * multiplier,
                ["Standard_D16s_v5"] = 0.768m * multiplier,
                ["Standard_F4s_v2"] = 0.169m * multiplier,
                ["Standard_E4s_v5"] = 0.252m * multiplier,
                ["Standard_D4s_v3"] = 0.192m * multiplier,
                ["Standard_D8s_v3"] = 0.384m * multiplier,
                ["Standard_D16s_v3"] = 0.768m * multiplier
            }
        };
    }

    /// <inheritdoc />
    public override StoragePricing GetStoragePricing(string? region = null)
    {
        var multiplier = GetRegionalMultiplier(region ?? DefaultRegion);

        return new StoragePricing
        {
            SsdPerGBMonth = 0.075m * multiplier,           // Premium SSD
            HddPerGBMonth = 0.04m * multiplier,            // Standard HDD
            ObjectStoragePerGBMonth = 0.0184m * multiplier, // Blob Hot
            BackupPerGBMonth = 0.05m * multiplier,
            RegistryPerGBMonth = 0.10m * multiplier        // ACR
        };
    }

    /// <inheritdoc />
    public override NetworkPricing GetNetworkPricing(string? region = null)
    {
        return new NetworkPricing
        {
            EgressPerGB = 0.087m,
            LoadBalancerPerHour = 0.025m,
            NatGatewayPerHour = 0.045m,
            VpnPerHour = 0.05m,
            PublicIpPerHour = 0.004m
        };
    }

    /// <inheritdoc />
    public override decimal GetControlPlaneCostPerHour(bool isHA = false)
    {
        // AKS: Free tier available, Standard tier $0.10/hr
        return isHA ? 0.10m : 0m;
    }

    /// <summary>
    /// Get ARO (Azure Red Hat OpenShift) specific pricing
    /// </summary>
    public decimal GetAroWorkerFeePerHour() => 0.35m;

    /// <summary>
    /// Azure has FREE cross-AZ data transfer (major cost advantage)
    /// </summary>
    public decimal GetCrossAzCostMultiplier() => 0m;

    private static decimal GetRegionalMultiplier(string region)
    {
        return region switch
        {
            "eastus" or "eastus2" or "westus2" => 1.0m,
            "westus" or "westus3" => 1.05m,
            "westeurope" or "northeurope" => 1.05m,
            "uksouth" or "ukwest" => 1.08m,
            "germanywestcentral" => 1.1m,
            "southeastasia" or "eastasia" => 1.1m,
            "japaneast" => 1.15m,
            "australiaeast" => 1.12m,
            "centralindia" => 0.95m,
            "uaenorth" => 1.2m,
            "brazilsouth" => 1.25m,
            _ => 1.0m
        };
    }
}
