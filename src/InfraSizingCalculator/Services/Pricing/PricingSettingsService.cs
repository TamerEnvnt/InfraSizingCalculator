using System.Text.Json;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Models.Pricing;
using InfraSizingCalculator.Services.Interfaces;
using Microsoft.JSInterop;

namespace InfraSizingCalculator.Services.Pricing;

/// <summary>
/// Service for managing pricing settings, stored in browser localStorage.
/// </summary>
public class PricingSettingsService : IPricingSettingsService
{
    private readonly IJSRuntime _jsRuntime;
    private PricingSettings _settings = new();
    private bool _initialized = false;

    private const string SettingsKey = "infra-sizing-pricing-settings";
    private const string CacheKey = "infra-sizing-pricing-cache";

    /// <summary>
    /// On-premises distributions that should show the pricing toggle
    /// </summary>
    private static readonly HashSet<Distribution> OnPremDistributions = new()
    {
        Distribution.OpenShift,
        Distribution.Rancher,
        Distribution.RKE2,
        Distribution.K3s,
        Distribution.Tanzu,
        Distribution.Charmed,
        Distribution.Kubernetes,
        Distribution.MicroK8s
    };

    public PricingSettingsService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public bool IncludePricingInResults
    {
        get => _settings.IncludePricingInResults;
        set
        {
            if (_settings.IncludePricingInResults != value)
            {
                _settings.IncludePricingInResults = value;
                OnSettingsChanged?.Invoke();
                _ = SaveSettingsInternalAsync();
            }
        }
    }

    public event Action? OnSettingsChanged;

    public async Task<PricingSettings> GetSettingsAsync()
    {
        await EnsureInitializedAsync();
        return _settings;
    }

    public async Task SaveSettingsAsync(PricingSettings settings)
    {
        _settings = settings;
        _settings.LastModified = DateTime.UtcNow;
        await SaveSettingsInternalAsync();
        OnSettingsChanged?.Invoke();
    }

    public async Task ResetToDefaultsAsync()
    {
        _settings = new PricingSettings
        {
            LastModified = DateTime.UtcNow,
            LastCacheReset = DateTime.UtcNow
        };
        await SaveSettingsInternalAsync();
        OnSettingsChanged?.Invoke();
    }

    public async Task ResetPricingCacheAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", CacheKey);
            _settings.LastCacheReset = DateTime.UtcNow;
            await SaveSettingsInternalAsync();
        }
        catch
        {
            // Ignore localStorage errors
        }
    }

    public async Task<PricingCacheStatus> GetCacheStatusAsync()
    {
        await EnsureInitializedAsync();

        var status = new PricingCacheStatus
        {
            LastReset = _settings.LastCacheReset,
            ConfiguredApiCount = _settings.CloudApiConfigs.Count(c => c.Value.IsConfigured),
            IsStale = _settings.LastCacheReset == null ||
                      (DateTime.UtcNow - _settings.LastCacheReset.Value).TotalHours > 24
        };

        // Check which providers have cached data
        try
        {
            var cacheJson = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", CacheKey);
            if (!string.IsNullOrEmpty(cacheJson))
            {
                var cache = JsonSerializer.Deserialize<Dictionary<string, DateTime>>(cacheJson);
                if (cache != null)
                {
                    status.CachedProviderCount = cache.Count;
                }
            }
        }
        catch
        {
            // Ignore cache read errors
        }

        return status;
    }

    public List<CloudAlternative> GetCloudAlternatives(Distribution distribution)
    {
        var alternatives = new List<CloudAlternative>();

        // First, add distribution-specific alternatives
        alternatives.AddRange(GetDistributionSpecificAlternatives(distribution));

        // Then add generic alternatives (but avoid duplicates)
        var existingProviders = alternatives.Select(a => a.Provider).ToHashSet();
        var generic = GetGenericCloudAlternatives()
            .Where(a => !existingProviders.Contains(a.Provider) || a.IsDistributionSpecific);

        alternatives.AddRange(generic);

        return alternatives;
    }

    public List<CloudAlternative> GetDistributionSpecificAlternatives(Distribution distribution)
    {
        return CloudAlternatives.GetForDistribution(distribution)
            .Where(a => a.IsDistributionSpecific)
            .ToList();
    }

    public List<CloudAlternative> GetGenericCloudAlternatives()
    {
        return CloudAlternatives.GetGenericAlternatives();
    }

    public bool IsOnPremDistribution(Distribution distribution)
    {
        return OnPremDistributions.Contains(distribution);
    }

    public OnPremPricing GetOnPremDefaults()
    {
        return _settings.OnPremDefaults;
    }

    public async Task UpdateOnPremDefaultsAsync(OnPremPricing defaults)
    {
        _settings.OnPremDefaults = defaults;
        _settings.LastModified = DateTime.UtcNow;
        await SaveSettingsInternalAsync();
        OnSettingsChanged?.Invoke();
    }

    public async Task ConfigureCloudApiAsync(CloudProvider provider, CloudApiConfig config)
    {
        _settings.CloudApiConfigs[provider] = config;
        _settings.LastModified = DateTime.UtcNow;
        await SaveSettingsInternalAsync();
    }

    public async Task<bool> ValidateCloudApiAsync(CloudProvider provider)
    {
        if (!_settings.CloudApiConfigs.TryGetValue(provider, out var config) || !config.IsConfigured)
        {
            return false;
        }

        // In a real implementation, this would make API calls to validate credentials
        // For now, we just mark it as validated
        config.LastValidated = DateTime.UtcNow;
        config.IsValid = true;
        await SaveSettingsInternalAsync();

        return true;
    }

    public CloudApiConfig? GetCloudApiConfig(CloudProvider provider)
    {
        return _settings.CloudApiConfigs.TryGetValue(provider, out var config) ? config : null;
    }

    public OnPremCostBreakdown CalculateOnPremCost(
        string distribution,
        int nodeCount,
        int totalCores,
        int totalRamGB,
        int totalStorageTB,
        int loadBalancers = 0,
        bool hasProduction = true)
    {
        if (!IncludePricingInResults)
        {
            return OnPremCostBreakdown.NotAvailable;
        }

        var onPrem = _settings.OnPremDefaults;

        // Calculate number of physical servers needed
        var serversNeeded = (int)Math.Ceiling((decimal)nodeCount / onPrem.Hardware.VMsPerServer);

        // Hardware cost (amortized)
        var totalHardwareCost = onPrem.Hardware.CalculateTotalCost(
            totalCores, totalRamGB, totalStorageTB, loadBalancers);
        var monthlyHardware = totalHardwareCost / (onPrem.HardwareRefreshYears * 12);
        var maintenanceMonthly = (totalHardwareCost * (onPrem.HardwareMaintenancePercent / 100)) / 12;

        // Data center cost
        var monthlyDataCenter = onPrem.DataCenter.CalculateMonthlyCost(serversNeeded);

        // Labor cost
        var monthlyLabor = onPrem.Labor.CalculateMonthlyCost(nodeCount, hasProduction);

        // License cost
        var monthlyLicense = onPrem.Licensing.GetMonthlyLicenseCost(distribution, nodeCount, totalCores);

        return new OnPremCostBreakdown
        {
            MonthlyHardware = monthlyHardware + maintenanceMonthly,
            MonthlyDataCenter = monthlyDataCenter,
            MonthlyLabor = monthlyLabor,
            MonthlyLicense = monthlyLicense,
            IsCalculated = true
        };
    }

    // ==================== MENDIX PRICING ====================

    /// <summary>
    /// Officially supported Mendix private cloud providers
    /// </summary>
    private static readonly HashSet<MendixPrivateCloudProvider> SupportedMendixProviders = new()
    {
        MendixPrivateCloudProvider.Azure,
        MendixPrivateCloudProvider.EKS,
        MendixPrivateCloudProvider.AKS,
        MendixPrivateCloudProvider.GKE,
        MendixPrivateCloudProvider.OpenShift
    };

    public MendixPricingSettings GetMendixPricingSettings()
    {
        return _settings.MendixPricing;
    }

    public async Task UpdateMendixPricingSettingsAsync(MendixPricingSettings settings)
    {
        _settings.MendixPricing = settings;
        _settings.LastModified = DateTime.UtcNow;
        await SaveSettingsInternalAsync();
        OnSettingsChanged?.Invoke();
    }

    public MendixPricingResult CalculateMendixCost(MendixDeploymentConfig config)
    {
        var mendix = _settings.MendixPricing;
        var result = new MendixPricingResult();

        // Calculate based on deployment category
        switch (config.Category)
        {
            case MendixDeploymentCategory.Cloud:
                CalculateCloudCost(config, mendix, result);
                break;

            case MendixDeploymentCategory.PrivateCloud:
                CalculatePrivateCloudCost(config, mendix, result);
                break;

            case MendixDeploymentCategory.Other:
                CalculateOtherDeploymentCost(config, mendix, result);
                break;
        }

        // User licensing (applies to all deployments)
        var internalUserBlocks = (int)Math.Ceiling(config.InternalUsers / 100.0);
        var externalUserBlocks = (int)Math.Ceiling(config.ExternalUsers / 250000.0);

        result.UserLicenseCost = (internalUserBlocks * mendix.InternalUsersPer100PerYear) +
                                  (externalUserBlocks * mendix.ExternalUsersPer250KPerYear);

        // GenAI add-ons
        if (config.IncludeGenAI && !string.IsNullOrEmpty(config.GenAIModelPackSize))
        {
            var genAIPack = mendix.GenAIModelPacks.FirstOrDefault(p => p.Size == config.GenAIModelPackSize);
            if (genAIPack != null)
            {
                result.GenAICost += genAIPack.PricePerYear;
                result.TotalCloudTokens += genAIPack.CloudTokens;
            }
        }

        if (config.IncludeGenAIKnowledgeBase)
        {
            result.GenAICost += mendix.GenAIKnowledgeBasePricePerYear;
            result.TotalCloudTokens += mendix.GenAIKnowledgeBaseTokens;
        }

        // Optional services
        if (config.IncludeCustomerEnablement)
        {
            result.ServicesCost = mendix.CustomerEnablementPrice;
        }

        // Apply volume discount to platform + user licenses (not to deployment fees)
        var discountableAmount = result.PlatformLicenseCost + result.UserLicenseCost;
        result.DiscountPercent = mendix.VolumeDiscountPercent;
        result.DiscountAmount = discountableAmount * (mendix.VolumeDiscountPercent / 100);

        return result;
    }

    private void CalculateCloudCost(MendixDeploymentConfig config, MendixPricingSettings mendix, MendixPricingResult result)
    {
        result.Category = MendixDeploymentCategory.Cloud;

        if (config.CloudType == MendixCloudType.Dedicated)
        {
            // Mendix Cloud Dedicated
            result.DeploymentTypeName = "Mendix Cloud Dedicated";
            result.DeploymentFeeCost = mendix.CloudDedicatedPricePerYear;
            result.PlatformLicenseCost = mendix.PlatformPremiumUnlimitedPerYear;
        }
        else
        {
            // Mendix Cloud SaaS - Resource Pack based
            result.DeploymentTypeName = "Mendix Cloud (SaaS)";

            if (config.ResourcePackTier.HasValue && config.ResourcePackSize.HasValue)
            {
                var pack = mendix.GetResourcePack(config.ResourcePackTier.Value, config.ResourcePackSize.Value);
                if (pack != null)
                {
                    result.DeploymentFeeCost = pack.PricePerYear * config.ResourcePackQuantity;
                    result.TotalCloudTokens = pack.CloudTokens * config.ResourcePackQuantity;
                    result.ResourcePackDetails = $"{config.ResourcePackQuantity}x {config.ResourcePackTier} {pack.DisplayName} " +
                                                  $"({pack.MxMemoryGB}GB RAM, {pack.MxVCpu} vCPU, {pack.DbStorageGB}GB DB)";
                }
            }

            // Platform license is separate for SaaS
            result.PlatformLicenseCost = mendix.PlatformPremiumUnlimitedPerYear;

            // Additional storage
            if (config.AdditionalFileStorageGB > 0)
            {
                var fileStorageBlocks = (int)Math.Ceiling(config.AdditionalFileStorageGB / 100);
                result.StorageCost += fileStorageBlocks * mendix.AdditionalFileStoragePer100GB;
            }

            if (config.AdditionalDatabaseStorageGB > 0)
            {
                var dbStorageBlocks = (int)Math.Ceiling(config.AdditionalDatabaseStorageGB / 100);
                result.StorageCost += dbStorageBlocks * mendix.AdditionalDatabaseStoragePer100GB;
            }
        }
    }

    private void CalculatePrivateCloudCost(MendixDeploymentConfig config, MendixPricingSettings mendix, MendixPricingResult result)
    {
        result.Category = MendixDeploymentCategory.PrivateCloud;
        result.PlatformLicenseCost = mendix.PlatformPremiumUnlimitedPerYear;

        if (config.PrivateCloudProvider == MendixPrivateCloudProvider.Azure)
        {
            // Mendix on Azure (managed service)
            result.DeploymentTypeName = "Mendix on Azure";
            result.DeploymentFeeCost = mendix.AzureBasePricePerYear;

            // Additional environments beyond included 3
            if (config.NumberOfEnvironments > mendix.AzureBaseEnvironmentsIncluded)
            {
                var additionalEnvs = config.NumberOfEnvironments - mendix.AzureBaseEnvironmentsIncluded;
                result.EnvironmentCost = additionalEnvs * mendix.AzureAdditionalEnvironmentPrice;
                result.TotalCloudTokens = additionalEnvs * mendix.AzureAdditionalEnvironmentTokens;
                result.EnvironmentDetails = $"{mendix.AzureBaseEnvironmentsIncluded} included + {additionalEnvs} additional @ ${mendix.AzureAdditionalEnvironmentPrice}/env";
            }
            else
            {
                result.EnvironmentDetails = $"{config.NumberOfEnvironments} environments (up to {mendix.AzureBaseEnvironmentsIncluded} included)";
            }
        }
        else
        {
            // Mendix on Kubernetes (EKS, AKS, GKE, OpenShift, etc.)
            var isSupported = IsMendixSupportedProvider(config.PrivateCloudProvider ?? MendixPrivateCloudProvider.GenericK8s);
            var providerName = config.PrivateCloudProvider?.ToString() ?? "Kubernetes";

            result.DeploymentTypeName = isSupported
                ? $"Mendix on Kubernetes ({providerName})"
                : $"Mendix on Kubernetes ({providerName}) ⚠️ Manual Setup";

            result.DeploymentFeeCost = mendix.K8sBasePricePerYear;

            // Calculate environment cost using tiered pricing
            if (config.NumberOfEnvironments > mendix.K8sBaseEnvironmentsIncluded)
            {
                result.EnvironmentCost = mendix.CalculateK8sEnvironmentCost(config.NumberOfEnvironments);
                result.EnvironmentDetails = BuildK8sEnvironmentDetails(config.NumberOfEnvironments, mendix);
            }
            else
            {
                result.EnvironmentDetails = $"{config.NumberOfEnvironments} environments ({mendix.K8sBaseEnvironmentsIncluded} included in base)";
            }
        }
    }

    private string BuildK8sEnvironmentDetails(int totalEnvironments, MendixPricingSettings mendix)
    {
        if (totalEnvironments <= mendix.K8sBaseEnvironmentsIncluded)
        {
            return $"{totalEnvironments} environments ({mendix.K8sBaseEnvironmentsIncluded} included)";
        }

        var additional = totalEnvironments - mendix.K8sBaseEnvironmentsIncluded;
        var parts = new List<string> { $"{mendix.K8sBaseEnvironmentsIncluded} included" };

        int remaining = additional;
        foreach (var tier in mendix.K8sEnvironmentTiers.OrderBy(t => t.MinEnvironments))
        {
            if (remaining <= 0) break;

            int tierCount;
            if (tier.MinEnvironments == 1) tierCount = Math.Min(50, remaining);
            else if (tier.MinEnvironments == 51) tierCount = Math.Min(50, remaining);
            else if (tier.MinEnvironments == 101) tierCount = Math.Min(50, remaining);
            else tierCount = remaining;

            if (tierCount > 0 && tier.PricePerEnvironment > 0)
            {
                parts.Add($"{tierCount} @ ${tier.PricePerEnvironment}/env");
            }
            else if (tierCount > 0)
            {
                parts.Add($"{tierCount} free");
            }

            remaining -= tierCount;
        }

        return string.Join(" + ", parts);
    }

    private void CalculateOtherDeploymentCost(MendixDeploymentConfig config, MendixPricingSettings mendix, MendixPricingResult result)
    {
        result.Category = MendixDeploymentCategory.Other;
        result.PlatformLicenseCost = mendix.PlatformPremiumUnlimitedPerYear;

        var (perApp, unlimited, name) = config.OtherDeployment switch
        {
            MendixOtherDeployment.Server => (mendix.ServerPerAppPricePerYear, mendix.ServerUnlimitedAppsPricePerYear, "Mendix on Server (VMs/Docker)"),
            MendixOtherDeployment.StackIT => (mendix.StackITPerAppPricePerYear, mendix.StackITUnlimitedAppsPricePerYear, "Mendix on StackIT"),
            MendixOtherDeployment.SapBtp => (mendix.SapBtpPerAppPricePerYear, mendix.SapBtpUnlimitedAppsPricePerYear, "Mendix on SAP BTP"),
            _ => (mendix.ServerPerAppPricePerYear, mendix.ServerUnlimitedAppsPricePerYear, "Mendix on Server")
        };

        result.DeploymentTypeName = name;

        if (config.IsUnlimitedApps)
        {
            result.DeploymentFeeCost = unlimited;
            result.EnvironmentDetails = "Unlimited applications";
        }
        else
        {
            result.DeploymentFeeCost = perApp * config.NumberOfApps;
            result.EnvironmentDetails = $"{config.NumberOfApps} application(s) @ ${perApp}/app";
        }
    }

    public MendixResourcePackSpec? RecommendResourcePack(
        MendixResourcePackTier tier,
        decimal requiredMemoryGB,
        decimal requiredCpu,
        decimal requiredDbStorageGB)
    {
        var packs = _settings.MendixPricing.GetAvailablePacks(tier);

        // Find the smallest pack that meets all requirements
        return packs
            .Where(p => p.MxMemoryGB >= requiredMemoryGB &&
                        p.MxVCpu >= requiredCpu &&
                        p.DbStorageGB >= requiredDbStorageGB)
            .OrderBy(p => p.PricePerYear)
            .FirstOrDefault();
    }

    public bool IsMendixSupportedProvider(MendixPrivateCloudProvider provider)
    {
        return SupportedMendixProviders.Contains(provider);
    }

    public List<MendixPrivateCloudProvider> GetMendixSupportedProviders()
    {
        return SupportedMendixProviders.ToList();
    }

    // ==================== OUTSYSTEMS PRICING ====================

    public OutSystemsPricingSettings GetOutSystemsPricingSettings()
    {
        return _settings.OutSystemsPricing;
    }

    public async Task UpdateOutSystemsPricingSettingsAsync(OutSystemsPricingSettings settings)
    {
        _settings.OutSystemsPricing = settings;
        _settings.LastModified = DateTime.UtcNow;
        await SaveSettingsInternalAsync();
        OnSettingsChanged?.Invoke();
    }

    public OutSystemsPricingResult CalculateOutSystemsCost(OutSystemsDeploymentConfig config)
    {
        var pricing = _settings.OutSystemsPricing;
        var aoPackCount = config.AOPacks;

        var result = new OutSystemsPricingResult
        {
            Platform = config.Platform,
            Deployment = config.Deployment,
            Region = config.Region,
            AOPackCount = aoPackCount,
            Warnings = config.GetValidationWarnings()
        };

        // Calculate license costs based on platform
        CalculateOutSystemsLicenseCosts(config, pricing, result);

        // Calculate add-on costs based on platform
        CalculateOutSystemsAddOnCosts(config, pricing, result);

        // Calculate services costs (region-dependent)
        CalculateOutSystemsServicesCosts(config, pricing, result);

        // Infrastructure (for O11 self-managed on cloud)
        if (config.Platform == OutSystemsPlatform.O11 &&
            config.Deployment == OutSystemsDeployment.SelfManaged &&
            config.CloudProvider != OutSystemsCloudProvider.OnPremises)
        {
            CalculateOutSystemsCloudVMCost(config, pricing, result);
        }

        // Apply discount if configured
        if (config.Discount != null && config.Discount.Value > 0)
        {
            result.DiscountAmount = config.Discount.CalculateDiscount(
                result.LicenseSubtotal,
                result.AddOnsSubtotal,
                result.ServicesSubtotal);
            result.DiscountDescription = config.Discount.Type == OutSystemsDiscountType.Percentage
                ? $"{config.Discount.Value}% discount on {config.Discount.Scope}"
                : $"${config.Discount.Value:N0} discount on {config.Discount.Scope}";
            if (!string.IsNullOrEmpty(config.Discount.Notes))
                result.DiscountDescription += $" ({config.Discount.Notes})";
        }

        // Build line items for detailed display
        BuildOutSystemsLineItems(config, pricing, result);

        return result;
    }

    private void CalculateOutSystemsLicenseCosts(
        OutSystemsDeploymentConfig config,
        OutSystemsPricingSettings pricing,
        OutSystemsPricingResult result)
    {
        var aoPackCount = result.AOPackCount;

        if (config.Platform == OutSystemsPlatform.ODC)
        {
            // ODC: Base + Additional AO Packs + Users
            result.EditionCost = pricing.OdcPlatformBasePrice;
            result.LicenseBreakdown["Platform Base (ODC)"] = result.EditionCost;

            // Additional AO packs (1 included in base)
            var additionalPacks = Math.Max(0, aoPackCount - 1);
            result.AOPacksCost = additionalPacks * pricing.OdcAOPackPrice;
            if (additionalPacks > 0)
                result.LicenseBreakdown[$"Additional AO Packs ({additionalPacks}×$18,150)"] = result.AOPacksCost;

            // User licensing - ODC uses flat pack pricing
            CalculateOdcUserLicenseCost(config, pricing, result);
        }
        else // O11
        {
            // O11: Enterprise Base + Additional AO Packs + Users (tiered)
            result.EditionCost = pricing.O11EnterpriseBasePrice;
            result.LicenseBreakdown["Enterprise Edition (O11)"] = result.EditionCost;

            // Additional AO packs (1 included in base)
            var additionalPacks = Math.Max(0, aoPackCount - 1);
            result.AOPacksCost = additionalPacks * pricing.O11AOPackPrice;
            if (additionalPacks > 0)
                result.LicenseBreakdown[$"Additional AO Packs ({additionalPacks}×$36,300)"] = result.AOPacksCost;

            // User licensing - O11 uses tiered pricing
            CalculateO11UserLicenseCost(config, pricing, result);
        }
    }

    private void CalculateOdcUserLicenseCost(
        OutSystemsDeploymentConfig config,
        OutSystemsPricingSettings pricing,
        OutSystemsPricingResult result)
    {
        result.UsedUnlimitedUsers = config.UseUnlimitedUsers;

        if (config.UseUnlimitedUsers)
        {
            // CRITICAL: Unlimited Users = $60,500 × AO Packs (NOT flat!)
            result.UnlimitedUsersCost = pricing.UnlimitedUsersPerAOPack * result.AOPackCount;
            result.LicenseBreakdown[$"Unlimited Users ({result.AOPackCount}×$60,500)"] = result.UnlimitedUsersCost;
            return;
        }

        // ODC Internal Users: 100 included, $6,050 per additional pack of 100
        var includedInternal = 100;
        var additionalInternal = Math.Max(0, config.InternalUsers - includedInternal);
        if (additionalInternal > 0)
        {
            result.InternalUserPackCount = (int)Math.Ceiling(additionalInternal / (double)pricing.InternalUserPackSize);
            result.InternalUsersCost = result.InternalUserPackCount * pricing.OdcInternalUserPackPrice;
            result.LicenseBreakdown[$"Internal Users (+{additionalInternal} users, {result.InternalUserPackCount} pack(s))"] = result.InternalUsersCost;
        }

        // ODC External Users: None included, $6,050 per pack of 1000
        if (config.ExternalUsers > 0)
        {
            result.ExternalUserPackCount = (int)Math.Ceiling(config.ExternalUsers / (double)pricing.ExternalUserPackSize);
            result.ExternalUsersCost = result.ExternalUserPackCount * pricing.OdcExternalUserPackPrice;
            result.LicenseBreakdown[$"External Users ({config.ExternalUsers} users, {result.ExternalUserPackCount} pack(s))"] = result.ExternalUsersCost;
        }
    }

    private void CalculateO11UserLicenseCost(
        OutSystemsDeploymentConfig config,
        OutSystemsPricingSettings pricing,
        OutSystemsPricingResult result)
    {
        result.UsedUnlimitedUsers = config.UseUnlimitedUsers;

        if (config.UseUnlimitedUsers)
        {
            // CRITICAL: Unlimited Users = $60,500 × AO Packs (NOT flat!)
            result.UnlimitedUsersCost = pricing.UnlimitedUsersPerAOPack * result.AOPackCount;
            result.LicenseBreakdown[$"Unlimited Users ({result.AOPackCount}×$60,500)"] = result.UnlimitedUsersCost;
            return;
        }

        // O11 Internal Users: 100 included, TIERED pricing for additional
        var includedInternal = 100;
        var additionalInternal = Math.Max(0, config.InternalUsers - includedInternal);
        if (additionalInternal > 0)
        {
            result.InternalUsersCost = CalculateO11TieredUserCost(
                config.InternalUsers, includedInternal, pricing.O11InternalUserTiers, pricing.InternalUserPackSize);
            result.InternalUserPackCount = (int)Math.Ceiling(additionalInternal / (double)pricing.InternalUserPackSize);
            result.LicenseBreakdown[$"Internal Users (+{additionalInternal} tiered)"] = result.InternalUsersCost;
        }

        // O11 External Users: None included, TIERED pricing
        if (config.ExternalUsers > 0)
        {
            result.ExternalUsersCost = CalculateO11TieredUserCost(
                config.ExternalUsers, 0, pricing.O11ExternalUserTiers, pricing.ExternalUserPackSize);
            result.ExternalUserPackCount = (int)Math.Ceiling(config.ExternalUsers / (double)pricing.ExternalUserPackSize);
            result.LicenseBreakdown[$"External Users ({config.ExternalUsers} tiered)"] = result.ExternalUsersCost;
        }
    }

    private static decimal CalculateO11TieredUserCost(
        int totalUsers,
        int includedUsers,
        List<OutSystemsUserTier> tiers,
        int packSize)
    {
        var billableUsers = Math.Max(0, totalUsers - includedUsers);
        if (billableUsers == 0) return 0;

        decimal totalCost = 0;
        var remaining = billableUsers;
        var currentUser = includedUsers + 1;

        foreach (var tier in tiers.OrderBy(t => t.MinUsers))
        {
            if (remaining <= 0) break;

            // How many users fall in this tier?
            var tierStart = Math.Max(currentUser, tier.MinUsers);
            var tierEnd = Math.Min(currentUser + remaining - 1, tier.MaxUsers);

            if (tierStart <= tierEnd)
            {
                var usersInTier = tierEnd - tierStart + 1;
                var packsInTier = (int)Math.Ceiling(usersInTier / (double)tier.PackSize);
                totalCost += packsInTier * tier.PricePerPack;
                remaining -= usersInTier;
                currentUser = tierEnd + 1;
            }
        }

        return totalCost;
    }

    private void CalculateOutSystemsAddOnCosts(
        OutSystemsDeploymentConfig config,
        OutSystemsPricingSettings pricing,
        OutSystemsPricingResult result)
    {
        var aoPackCount = result.AOPackCount;

        if (config.Platform == OutSystemsPlatform.ODC)
        {
            CalculateOdcAddOnCosts(config, pricing, result, aoPackCount);
        }
        else
        {
            CalculateO11AddOnCosts(config, pricing, result, aoPackCount);
        }

        // AppShield (both platforms) - uses tiered flat pricing
        if (config.Platform == OutSystemsPlatform.ODC ? config.OdcAppShield : config.O11AppShield)
        {
            var userVolume = config.UseUnlimitedUsers
                ? config.AppShieldUserVolume ?? 10000 // Default if not specified
                : config.InternalUsers + config.ExternalUsers;

            var appShieldPrice = pricing.GetAppShieldPrice(userVolume);
            var tier = pricing.AppShieldTiers.FirstOrDefault(t => userVolume >= t.MinUsers && userVolume <= t.MaxUsers);

            result.AddOnCosts["AppShield"] = appShieldPrice;
            result.AppShieldUserVolume = userVolume;
            result.AppShieldTier = tier?.Tier;
        }
    }

    private static void CalculateOdcAddOnCosts(
        OutSystemsDeploymentConfig config,
        OutSystemsPricingSettings pricing,
        OutSystemsPricingResult result,
        int aoPackCount)
    {
        // Support (mutually exclusive - Premium takes precedence)
        if (config.OdcSupport24x7Premium)
            result.AddOnCosts["Support 24x7 Premium"] = pricing.OdcSupport24x7PremiumPerPack * aoPackCount;
        else if (config.OdcSupport24x7Extended)
            result.AddOnCosts["Support 24x7 Extended"] = pricing.OdcSupport24x7ExtendedPerPack * aoPackCount;

        // Sentry includes HA
        if (config.OdcSentry)
        {
            result.AddOnCosts["Sentry"] = pricing.OdcSentryPerPack * aoPackCount;
            // HA is included when Sentry is enabled
        }
        else if (config.OdcHighAvailability)
        {
            result.AddOnCosts["High Availability"] = pricing.OdcHighAvailabilityPerPack * aoPackCount;
        }

        // Non-Production Runtime (supports quantity)
        if (config.OdcNonProdRuntimeQuantity > 0)
            result.AddOnCosts[$"Non-Production Runtime (×{config.OdcNonProdRuntimeQuantity})"] =
                pricing.OdcNonProdRuntimePerPack * aoPackCount * config.OdcNonProdRuntimeQuantity;

        // Private Gateway
        if (config.OdcPrivateGateway)
            result.AddOnCosts["Private Gateway"] = pricing.OdcPrivateGatewayPerPack * aoPackCount;
    }

    private static void CalculateO11AddOnCosts(
        OutSystemsDeploymentConfig config,
        OutSystemsPricingSettings pricing,
        OutSystemsPricingResult result,
        int aoPackCount)
    {
        var isCloud = config.Deployment == OutSystemsDeployment.Cloud;

        // Support 24x7 Premium (24x7 is included in Enterprise Edition)
        if (config.O11Support24x7Premium)
            result.AddOnCosts["Support 24x7 Premium"] = pricing.O11Support24x7PremiumPerPack * aoPackCount;

        // Sentry includes HA (Cloud only)
        if (config.O11Sentry && isCloud)
        {
            result.AddOnCosts["Sentry (incl. HA)"] = pricing.O11SentryPerPack * aoPackCount;
        }
        else if (config.O11HighAvailability && isCloud)
        {
            result.AddOnCosts["High Availability"] = pricing.O11HighAvailabilityPerPack * aoPackCount;
        }

        // Non-Production Environment (supports quantity)
        if (config.O11NonProdEnvQuantity > 0)
            result.AddOnCosts[$"Non-Production Env (×{config.O11NonProdEnvQuantity})"] =
                pricing.O11NonProdEnvPerPack * aoPackCount * config.O11NonProdEnvQuantity;

        // Load Test Environment (Cloud only, supports quantity)
        if (config.O11LoadTestEnvQuantity > 0 && isCloud)
            result.AddOnCosts[$"Load Test Env (×{config.O11LoadTestEnvQuantity})"] =
                pricing.O11LoadTestEnvPerPack * aoPackCount * config.O11LoadTestEnvQuantity;

        // Environment Pack (supports quantity)
        if (config.O11EnvPackQuantity > 0)
            result.AddOnCosts[$"Environment Pack (×{config.O11EnvPackQuantity})"] =
                pricing.O11EnvironmentPackPerPack * aoPackCount * config.O11EnvPackQuantity;

        // Disaster Recovery (Self-Managed only)
        if (config.O11DisasterRecovery && !isCloud)
            result.AddOnCosts["Disaster Recovery"] = pricing.O11DisasterRecoveryPerPack * aoPackCount;

        // Log Streaming (Cloud only, flat fee, supports quantity)
        if (config.O11LogStreamingQuantity > 0 && isCloud)
            result.AddOnCosts[$"Log Streaming (×{config.O11LogStreamingQuantity})"] =
                pricing.O11LogStreamingFlat * config.O11LogStreamingQuantity;

        // Database Replica (Cloud only, flat fee, supports quantity)
        if (config.O11DatabaseReplicaQuantity > 0 && isCloud)
            result.AddOnCosts[$"Database Replica (×{config.O11DatabaseReplicaQuantity})"] =
                pricing.O11DatabaseReplicaFlat * config.O11DatabaseReplicaQuantity;
    }

    private void CalculateOutSystemsServicesCosts(
        OutSystemsDeploymentConfig config,
        OutSystemsPricingSettings pricing,
        OutSystemsPricingResult result)
    {
        var regionPricing = pricing.GetServicesPricing(config.Region);

        // Success Plans
        if (config.EssentialSuccessPlanQuantity > 0)
            result.ServiceCosts[$"Essential Success Plan (×{config.EssentialSuccessPlanQuantity})"] =
                regionPricing.EssentialSuccessPlan * config.EssentialSuccessPlanQuantity;

        if (config.PremierSuccessPlanQuantity > 0)
            result.ServiceCosts[$"Premier Success Plan (×{config.PremierSuccessPlanQuantity})"] =
                regionPricing.PremierSuccessPlan * config.PremierSuccessPlanQuantity;

        // Training (Bootcamps)
        if (config.DedicatedGroupSessionQuantity > 0)
            result.ServiceCosts[$"Dedicated Group Session (×{config.DedicatedGroupSessionQuantity})"] =
                regionPricing.DedicatedGroupSession * config.DedicatedGroupSessionQuantity;

        if (config.PublicSessionQuantity > 0)
            result.ServiceCosts[$"Public Session (×{config.PublicSessionQuantity})"] =
                regionPricing.PublicSession * config.PublicSessionQuantity;

        // Expert Days
        if (config.ExpertDayQuantity > 0)
            result.ServiceCosts[$"Expert Day (×{config.ExpertDayQuantity})"] =
                regionPricing.ExpertDay * config.ExpertDayQuantity;
    }

    private void CalculateOutSystemsCloudVMCost(
        OutSystemsDeploymentConfig config,
        OutSystemsPricingSettings pricing,
        OutSystemsPricingResult result)
    {
        var totalServers = config.TotalEnvironments * config.FrontEndServersPerEnvironment;
        result.TotalVMCount = totalServers;

        if (config.CloudProvider == OutSystemsCloudProvider.Azure)
        {
            var hourlyRate = pricing.AzureVMHourlyPricing.TryGetValue(config.AzureInstanceType, out var rate)
                ? rate : 0.169m; // Default to F4s_v2
            result.MonthlyVMCost = hourlyRate * pricing.HoursPerMonth * totalServers;
            var specs = GetAzureInstanceSpecs(config.AzureInstanceType);
            result.VMDetails = $"{totalServers}× Azure {config.AzureInstanceType} ({specs.vCPU} vCPU, {specs.RamGB} GB)";
        }
        else if (config.CloudProvider == OutSystemsCloudProvider.AWS)
        {
            var hourlyRate = pricing.AwsEC2HourlyPricing.TryGetValue(config.AwsInstanceType, out var rate)
                ? rate : 0.192m; // Default to M5XLarge
            result.MonthlyVMCost = hourlyRate * pricing.HoursPerMonth * totalServers;
            var specs = GetAwsInstanceSpecs(config.AwsInstanceType);
            result.VMDetails = $"{totalServers}× AWS {config.AwsInstanceType} ({specs.vCPU} vCPU, {specs.RamGB} GB)";
        }
    }

    private static void BuildOutSystemsLineItems(
        OutSystemsDeploymentConfig config,
        OutSystemsPricingSettings pricing,
        OutSystemsPricingResult result)
    {
        // License line items
        foreach (var (name, amount) in result.LicenseBreakdown)
        {
            result.LineItems.Add(new OutSystemsCostLineItem
            {
                Category = "License",
                Name = name,
                Amount = amount,
                Quantity = 1
            });
        }

        // Add-on line items
        foreach (var (name, amount) in result.AddOnCosts)
        {
            result.LineItems.Add(new OutSystemsCostLineItem
            {
                Category = "Add-On",
                Name = name,
                Amount = amount,
                Quantity = 1
            });
        }

        // Service line items
        foreach (var (name, amount) in result.ServiceCosts)
        {
            result.LineItems.Add(new OutSystemsCostLineItem
            {
                Category = "Service",
                Name = name,
                Amount = amount,
                Quantity = 1
            });
        }

        // Infrastructure line item
        if (result.AnnualVMCost > 0)
        {
            result.LineItems.Add(new OutSystemsCostLineItem
            {
                Category = "Infrastructure",
                Name = result.VMDetails ?? "Cloud VMs",
                Description = $"${result.MonthlyVMCost:N0}/month",
                Amount = result.AnnualVMCost,
                Quantity = result.TotalVMCount
            });
        }
    }

    private static (int vCPU, int RamGB) GetAzureInstanceSpecs(OutSystemsAzureInstanceType instanceType)
    {
        return instanceType switch
        {
            OutSystemsAzureInstanceType.F4s_v2 => (4, 8),
            OutSystemsAzureInstanceType.D4s_v3 => (4, 16),
            OutSystemsAzureInstanceType.D8s_v3 => (8, 32),
            OutSystemsAzureInstanceType.D16s_v3 => (16, 64),
            _ => (4, 8)
        };
    }

    private static (int vCPU, int RamGB) GetAwsInstanceSpecs(OutSystemsAwsInstanceType instanceType)
    {
        return instanceType switch
        {
            OutSystemsAwsInstanceType.M5Large => (2, 8),
            OutSystemsAwsInstanceType.M5XLarge => (4, 16),
            OutSystemsAwsInstanceType.M52XLarge => (8, 32),
            _ => (4, 16)
        };
    }

    public bool IsOutSystemsCloudOnlyFeature(string featureName)
    {
        return !OutSystemsPricingSettings.IsFeatureAvailable(featureName, OutSystemsDeployment.SelfManaged);
    }

    public OutSystemsAzureInstanceType RecommendAzureInstance(int totalCores, int totalRamGB)
    {
        if (totalRamGB <= 8 && totalCores <= 4)
            return OutSystemsAzureInstanceType.F4s_v2;
        if (totalRamGB <= 16 && totalCores <= 4)
            return OutSystemsAzureInstanceType.D4s_v3;
        if (totalRamGB <= 32 && totalCores <= 8)
            return OutSystemsAzureInstanceType.D8s_v3;
        return OutSystemsAzureInstanceType.D16s_v3;
    }

    public OutSystemsAwsInstanceType RecommendAwsInstance(int totalCores, int totalRamGB)
    {
        if (totalRamGB <= 8 && totalCores <= 2)
            return OutSystemsAwsInstanceType.M5Large;
        if (totalRamGB <= 16 && totalCores <= 4)
            return OutSystemsAwsInstanceType.M5XLarge;
        return OutSystemsAwsInstanceType.M52XLarge;
    }

    private async Task EnsureInitializedAsync()
    {
        if (_initialized) return;

        try
        {
            var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", SettingsKey);
            if (!string.IsNullOrEmpty(json))
            {
                var loaded = JsonSerializer.Deserialize<PricingSettings>(json);
                if (loaded != null)
                {
                    _settings = loaded;
                }
            }
        }
        catch
        {
            // Use defaults if localStorage is not available
        }

        _initialized = true;
    }

    private async Task SaveSettingsInternalAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(_settings);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", SettingsKey, json);
        }
        catch
        {
            // Ignore save errors
        }
    }
}
