namespace InfraSizingCalculator.Models.Pricing.Base;

/// <summary>
/// Abstract base class for cloud provider pricing implementations.
/// Provides common functionality and enforces consistent structure.
/// Follows Template Method Pattern - defines skeleton, subclasses fill in specifics.
/// </summary>
public abstract class CloudProviderPricingBase : ICloudProviderPricing
{
    /// <summary>
    /// Hours per month for calculations (730 = average month)
    /// </summary>
    protected const int HoursPerMonth = 730;

    /// <inheritdoc />
    public abstract CloudProvider Provider { get; }

    /// <inheritdoc />
    public abstract string DefaultRegion { get; }

    /// <summary>
    /// Provider display name for pricing source
    /// </summary>
    protected abstract string ProviderDisplayName { get; }

    /// <summary>
    /// Pricing source description (e.g., "AWS Public Pricing 2025")
    /// </summary>
    protected virtual string PricingSource => $"Default ({ProviderDisplayName} Public Pricing 2025)";

    /// <inheritdoc />
    public virtual PricingModel GetPricing(string? region = null)
    {
        var effectiveRegion = region ?? DefaultRegion;
        var regions = GetAvailableRegions();
        var regionInfo = regions.FirstOrDefault(r => r.Code == effectiveRegion);

        return new PricingModel
        {
            Provider = Provider,
            Region = effectiveRegion,
            RegionDisplayName = regionInfo?.DisplayName ?? effectiveRegion,
            Currency = Currency.USD,
            PricingType = PricingType.OnDemand,
            Source = PricingSource,
            LastUpdated = DateTime.UtcNow,
            IsLive = false,
            Compute = GetComputePricing(effectiveRegion),
            Storage = GetStoragePricing(effectiveRegion),
            Network = GetNetworkPricing(effectiveRegion),
            Licenses = GetLicensePricing(),
            Support = GetSupportPricing()
        };
    }

    /// <inheritdoc />
    public abstract IReadOnlyList<RegionInfo> GetAvailableRegions();

    /// <inheritdoc />
    public virtual bool IsRegionSupported(string region)
    {
        return GetAvailableRegions().Any(r =>
            r.Code.Equals(region, StringComparison.OrdinalIgnoreCase));
    }

    /// <inheritdoc />
    public abstract ComputePricing GetComputePricing(string? region = null);

    /// <inheritdoc />
    public abstract StoragePricing GetStoragePricing(string? region = null);

    /// <inheritdoc />
    public abstract NetworkPricing GetNetworkPricing(string? region = null);

    /// <summary>
    /// Get license pricing (common across providers)
    /// </summary>
    protected virtual LicensePricing GetLicensePricing()
    {
        return new LicensePricing
        {
            OpenShiftPerNodeYear = 2500m,
            RancherEnterprisePerNodeYear = 1000m,
            TanzuPerCoreYear = 1500m,
            CharmedK8sPerNodeYear = 500m
        };
    }

    /// <summary>
    /// Get support pricing tiers
    /// </summary>
    protected virtual SupportPricing GetSupportPricing()
    {
        return new SupportPricing
        {
            BasicSupportPercent = 0,
            DeveloperSupportPercent = 3,
            BusinessSupportPercent = 10,
            EnterpriseSupportPercent = 15
        };
    }

    /// <inheritdoc />
    public abstract decimal GetControlPlaneCostPerHour(bool isHA = false);

    /// <inheritdoc />
    public virtual decimal GetInstancePrice(string instanceType, string? region = null)
    {
        var compute = GetComputePricing(region);
        return compute.InstanceTypePrices.TryGetValue(instanceType, out var price)
            ? price
            : compute.CpuPerHour * 4; // Default to 4 vCPU equivalent
    }

    /// <inheritdoc />
    public virtual decimal CalculateMonthlyCost(int cpuCores, int ramGB, int storageGB, string? region = null)
    {
        var compute = GetComputePricing(region);
        var storage = GetStoragePricing(region);

        var computeCost = compute.CalculateMonthlyCost(cpuCores, ramGB);
        var storageCost = storageGB * storage.SsdPerGBMonth;
        var controlPlaneCost = GetControlPlaneCostPerHour() * HoursPerMonth;

        return computeCost + storageCost + controlPlaneCost;
    }
}
