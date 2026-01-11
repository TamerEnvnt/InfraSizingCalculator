using InfraSizingCalculator.Models.Pricing.Base;

namespace InfraSizingCalculator.Models.Pricing.CloudProviders;

/// <summary>
/// AWS (Amazon Web Services) pricing implementation.
/// Includes EKS, EC2, EBS, and related services.
/// Prices based on AWS public pricing as of 2025.
/// </summary>
public sealed class AwsCloudPricing : CloudProviderPricingBase
{
    /// <inheritdoc />
    public override CloudProvider Provider => CloudProvider.AWS;

    /// <inheritdoc />
    public override string DefaultRegion => "us-east-1";

    /// <inheritdoc />
    protected override string ProviderDisplayName => "AWS";

    /// <inheritdoc />
    public override IReadOnlyList<RegionInfo> GetAvailableRegions() => CloudRegions.AWSRegions;

    /// <inheritdoc />
    public override ComputePricing GetComputePricing(string? region = null)
    {
        // Base pricing for us-east-1, apply regional multipliers for other regions
        var multiplier = GetRegionalMultiplier(region ?? DefaultRegion);

        return new ComputePricing
        {
            // Based on m6i.xlarge pricing (~$0.192/hour for 4 vCPU, 16GB)
            CpuPerHour = 0.048m * multiplier,
            RamGBPerHour = 0.006m * multiplier,
            ManagedControlPlanePerHour = 0.10m, // EKS control plane (flat rate)
            OpenShiftServiceFeePerWorkerHour = 0.171m, // ROSA worker fee
            InstanceTypePrices = new Dictionary<string, decimal>
            {
                ["t3.medium"] = 0.0416m * multiplier,
                ["t3.large"] = 0.0832m * multiplier,
                ["m6i.large"] = 0.096m * multiplier,
                ["m6i.xlarge"] = 0.192m * multiplier,
                ["m6i.2xlarge"] = 0.384m * multiplier,
                ["m6i.4xlarge"] = 0.768m * multiplier,
                ["c6i.xlarge"] = 0.17m * multiplier,
                ["r6i.xlarge"] = 0.252m * multiplier,
                ["m5.xlarge"] = 0.192m * multiplier,
                ["m5.2xlarge"] = 0.384m * multiplier,
                ["m5.4xlarge"] = 0.768m * multiplier
            }
        };
    }

    /// <inheritdoc />
    public override StoragePricing GetStoragePricing(string? region = null)
    {
        var multiplier = GetRegionalMultiplier(region ?? DefaultRegion);

        return new StoragePricing
        {
            SsdPerGBMonth = 0.08m * multiplier,           // gp3 SSD
            HddPerGBMonth = 0.045m * multiplier,          // st1 HDD
            ObjectStoragePerGBMonth = 0.023m * multiplier, // S3 Standard
            BackupPerGBMonth = 0.05m * multiplier,        // EBS Snapshots
            RegistryPerGBMonth = 0.10m * multiplier       // ECR
        };
    }

    /// <inheritdoc />
    public override NetworkPricing GetNetworkPricing(string? region = null)
    {
        return new NetworkPricing
        {
            EgressPerGB = 0.09m,           // First 10TB
            LoadBalancerPerHour = 0.0225m, // ALB
            NatGatewayPerHour = 0.045m,
            VpnPerHour = 0.05m,
            PublicIpPerHour = 0.005m
        };
    }

    /// <inheritdoc />
    public override decimal GetControlPlaneCostPerHour(bool isHA = false)
    {
        // EKS: Always $0.10/hr regardless of HA
        return 0.10m;
    }

    /// <summary>
    /// Get ROSA (Red Hat OpenShift on AWS) specific pricing
    /// </summary>
    public decimal GetRosaWorkerFeePerHour() => 0.171m;

    /// <summary>
    /// Get regional price multiplier
    /// </summary>
    private static decimal GetRegionalMultiplier(string region)
    {
        return region switch
        {
            "us-east-1" or "us-east-2" or "us-west-2" => 1.0m,
            "us-west-1" => 1.1m,
            "eu-west-1" or "eu-central-1" => 1.05m,
            "eu-west-2" or "eu-west-3" => 1.08m,
            "ap-southeast-1" or "ap-southeast-2" => 1.1m,
            "ap-northeast-1" => 1.15m,
            "ap-south-1" => 0.95m,
            "me-south-1" or "me-central-1" => 1.2m,
            "sa-east-1" => 1.25m,
            _ => 1.0m
        };
    }
}
