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
        var result = new OutSystemsPricingResult
        {
            Edition = config.Edition,
            DeploymentType = config.DeploymentType,
            CloudProvider = config.CloudProvider,
            TotalAOs = config.TotalApplicationObjects,
            AOPackCount = config.NumberOfAOPacks,
            IncludedAOs = pricing.GetIncludedAOs(config.Edition),
            Warnings = config.GetValidationWarnings()
        };

        // Set deployment type name
        result.DeploymentTypeName = config.DeploymentType == OutSystemsDeploymentType.Cloud
            ? "OutSystems Cloud"
            : config.CloudProvider switch
            {
                OutSystemsCloudProvider.Azure => "Self-Managed on Azure",
                OutSystemsCloudProvider.AWS => "Self-Managed on AWS",
                _ => "Self-Managed On-Premises"
            };

        // Edition base cost
        result.EditionBaseCost = pricing.GetEditionBasePrice(config.Edition);

        // Additional AO packs (1 pack included in base)
        result.AdditionalAOPacks = Math.Max(0, result.AOPackCount - 1);
        result.AdditionalAOsCost = result.AdditionalAOPacks * pricing.AdditionalAOPackPrice;

        // User licensing
        CalculateOutSystemsUserLicenseCost(config, pricing, result);

        // Add-ons
        CalculateOutSystemsAddOnCosts(config, pricing, result);

        // Infrastructure (for self-managed on cloud)
        if (config.DeploymentType == OutSystemsDeploymentType.SelfManaged &&
            config.CloudProvider != OutSystemsCloudProvider.OnPremises)
        {
            CalculateOutSystemsCloudVMCost(config, pricing, result);
        }

        // Services
        result.SuccessPlanCost = pricing.CalculateSuccessPlanCost(config.SuccessPlan);
        result.TrainingCost = (config.DedicatedGroupSessions * pricing.DedicatedGroupSessionPrice) +
                              (config.PublicSessions * pricing.PublicSessionPrice);
        result.ExpertDaysCost = config.ExpertDays * pricing.ExpertDayPrice;

        // Environment details
        result.EnvironmentDetails = $"{config.ProductionEnvironments} Production, " +
                                    $"{config.NonProductionEnvironments} Non-Production";

        return result;
    }

    private void CalculateOutSystemsUserLicenseCost(
        OutSystemsDeploymentConfig config,
        OutSystemsPricingSettings pricing,
        OutSystemsPricingResult result)
    {
        if (config.UseUnlimitedUsers)
        {
            result.UserLicenseCost = pricing.UnlimitedUsersPrice;
            result.UserLicenseDetails = "Unlimited Users";
            return;
        }

        decimal userCost = 0;
        var details = new List<string>();

        var includedInternal = pricing.GetIncludedInternalUsers(config.Edition);
        var additionalInternal = Math.Max(0, config.InternalUsers - includedInternal);

        if (additionalInternal > 0)
        {
            var internalPacks = (int)Math.Ceiling(additionalInternal / (double)pricing.InternalUserPackSize);
            userCost += internalPacks * pricing.AdditionalInternalUserPackPrice;
            details.Add($"{config.InternalUsers} internal ({includedInternal} included)");
        }
        else
        {
            details.Add($"{config.InternalUsers} internal (included)");
        }

        if (config.ExternalUsers > 0)
        {
            userCost += pricing.CalculateExternalUsersCost(config.ExternalUsers);
            var externalPacks = (int)Math.Ceiling(config.ExternalUsers / (double)pricing.ExternalUserPackSize);
            details.Add($"{config.ExternalUsers} external ({externalPacks} pack(s))");
        }

        result.UserLicenseCost = userCost;
        result.UserLicenseDetails = string.Join(", ", details);
    }

    private void CalculateOutSystemsAddOnCosts(
        OutSystemsDeploymentConfig config,
        OutSystemsPricingSettings pricing,
        OutSystemsPricingResult result)
    {
        var aoPackCount = result.AOPackCount;
        var isCloud = config.DeploymentType == OutSystemsDeploymentType.Cloud;

        if (config.Include24x7PremiumSupport)
            result.Support24x7PremiumCost = pricing.Support24x7PremiumPerAOPack * aoPackCount;

        if (config.IncludeNonProductionEnv)
            result.NonProductionEnvCost = pricing.NonProductionEnvPerAOPack * aoPackCount;

        if (config.IncludeLoadTestEnv && isCloud)
            result.LoadTestEnvCost = pricing.LoadTestEnvPerAOPack * aoPackCount;

        if (config.IncludeEnvironmentPack)
            result.EnvironmentPackCost = pricing.EnvironmentPackPerAOPack * aoPackCount;

        if (config.IncludeSentry && isCloud)
            result.SentryCost = pricing.SentryPerAOPack * aoPackCount;
        else if (config.IncludeHA && isCloud)
            result.HACost = pricing.HighAvailabilityPerAOPack * aoPackCount;

        if (config.IncludeDR)
            result.DRCost = pricing.DisasterRecoveryPerAOPack * aoPackCount;

        if (config.IncludeLogStreaming && isCloud)
            result.LogStreamingCost = pricing.LogStreamingPrice;

        if (config.IncludeDatabaseReplica && isCloud)
            result.DatabaseReplicaCost = pricing.DatabaseReplicaPrice;

        if (config.AppShieldUsers > 0)
            result.AppShieldCost = pricing.AppShieldPerUser * config.AppShieldUsers;
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
            result.MonthlyVMCost = pricing.CalculateAzureMonthlyVMCost(config.AzureInstanceType, totalServers);
            var specs = OutSystemsPricingSettings.GetAzureInstanceSpecs(config.AzureInstanceType);
            result.VMDetails = $"{totalServers}x Azure {config.AzureInstanceType} ({specs.vCPU} vCPU, {specs.RamGB} GB)";
        }
        else if (config.CloudProvider == OutSystemsCloudProvider.AWS)
        {
            result.MonthlyVMCost = pricing.CalculateAwsMonthlyVMCost(config.AwsInstanceType, totalServers);
            var specs = OutSystemsPricingSettings.GetAwsInstanceSpecs(config.AwsInstanceType);
            result.VMDetails = $"{totalServers}x AWS {config.AwsInstanceType} ({specs.vCPU} vCPU, {specs.RamGB} GB)";
        }
    }

    public bool IsOutSystemsCloudOnlyFeature(string featureName)
    {
        return OutSystemsPricingSettings.IsCloudOnlyFeature(featureName);
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
