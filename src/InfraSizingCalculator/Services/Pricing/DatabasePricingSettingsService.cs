using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using InfraSizingCalculator.Data;
using InfraSizingCalculator.Data.Entities;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Models.Pricing;
using InfraSizingCalculator.Services.Interfaces;

namespace InfraSizingCalculator.Services.Pricing;

/// <summary>
/// Database-backed implementation of IPricingSettingsService
/// All settings and pricing data are persisted to SQLite database
/// </summary>
public class DatabasePricingSettingsService : IPricingSettingsService
{
    private readonly InfraSizingDbContext _dbContext;
    private PricingSettings? _cachedSettings;

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

    public event Action? OnSettingsChanged;

    public bool IncludePricingInResults
    {
        get => GetSettingsAsync().Result.IncludePricingInResults;
        set
        {
            var settings = GetSettingsAsync().Result;
            settings.IncludePricingInResults = value;
            SaveSettingsAsync(settings).Wait();
        }
    }

    public DatabasePricingSettingsService(InfraSizingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PricingSettings> GetSettingsAsync()
    {
        if (_cachedSettings != null)
            return _cachedSettings;

        var appSettings = await _dbContext.ApplicationSettings.FirstOrDefaultAsync();
        var onPremEntity = await _dbContext.OnPremPricing.FirstOrDefaultAsync();
        var mendixEntity = await _dbContext.MendixPricing.FirstOrDefaultAsync();

        var outSystemsEntity = await _dbContext.OutSystemsPricing.FirstOrDefaultAsync();

        _cachedSettings = new PricingSettings
        {
            IncludePricingInResults = appSettings?.IncludePricingInResults ?? false,
            LastCacheReset = appSettings?.LastCacheReset,
            OnPremDefaults = MapOnPremFromEntity(onPremEntity),
            MendixPricing = MapMendixFromEntity(mendixEntity),
            OutSystemsPricing = MapOutSystemsFromEntity(outSystemsEntity)
        };

        // Load cloud API configs
        var cloudCredentials = await _dbContext.CloudApiCredentials.ToListAsync();
        foreach (var cred in cloudCredentials)
        {
            if (Enum.TryParse<CloudProvider>(cred.ProviderName, out var provider))
            {
                _cachedSettings.CloudApiConfigs[provider] = new CloudApiConfig
                {
                    ApiKey = cred.ApiKey,
                    SecretKey = cred.SecretKey,
                    DefaultRegion = cred.Region
                };
            }
        }

        return _cachedSettings;
    }

    public async Task SaveSettingsAsync(PricingSettings settings)
    {
        // Update application settings
        var appSettings = await _dbContext.ApplicationSettings.FirstOrDefaultAsync();
        if (appSettings == null)
        {
            appSettings = new ApplicationSettingsEntity();
            _dbContext.ApplicationSettings.Add(appSettings);
        }

        appSettings.IncludePricingInResults = settings.IncludePricingInResults;
        appSettings.LastCacheReset = settings.LastCacheReset;
        appSettings.UpdatedAt = DateTime.UtcNow;

        // Update on-prem pricing
        var onPremEntity = await _dbContext.OnPremPricing.FirstOrDefaultAsync();
        if (onPremEntity == null)
        {
            onPremEntity = new OnPremPricingEntity();
            _dbContext.OnPremPricing.Add(onPremEntity);
        }

        MapOnPremToEntity(settings.OnPremDefaults, onPremEntity);
        onPremEntity.UpdatedAt = DateTime.UtcNow;

        // Update Mendix pricing
        var mendixEntity = await _dbContext.MendixPricing.FirstOrDefaultAsync();
        if (mendixEntity == null)
        {
            mendixEntity = new MendixPricingEntity();
            _dbContext.MendixPricing.Add(mendixEntity);
        }

        MapMendixToEntity(settings.MendixPricing, mendixEntity);
        mendixEntity.UpdatedAt = DateTime.UtcNow;

        // Update OutSystems pricing
        var outSystemsEntity = await _dbContext.OutSystemsPricing.FirstOrDefaultAsync();
        if (outSystemsEntity == null)
        {
            outSystemsEntity = new OutSystemsPricingEntity();
            _dbContext.OutSystemsPricing.Add(outSystemsEntity);
        }

        MapOutSystemsToEntity(settings.OutSystemsPricing, outSystemsEntity);
        outSystemsEntity.UpdatedAt = DateTime.UtcNow;

        // Update cloud API credentials
        foreach (var (provider, config) in settings.CloudApiConfigs)
        {
            var credEntity = await _dbContext.CloudApiCredentials
                .FirstOrDefaultAsync(c => c.ProviderName == provider.ToString());

            if (credEntity == null)
            {
                credEntity = new CloudApiCredentialsEntity { ProviderName = provider.ToString() };
                _dbContext.CloudApiCredentials.Add(credEntity);
            }

            credEntity.ApiKey = config.ApiKey;
            credEntity.SecretKey = config.SecretKey;
            credEntity.Region = config.DefaultRegion;
            credEntity.IsConfigured = config.IsConfigured;
            credEntity.UpdatedAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync();

        // Update cache
        _cachedSettings = settings;

        OnSettingsChanged?.Invoke();
    }

    public async Task ResetToDefaultsAsync()
    {
        // Reset application settings to defaults
        var appSettings = await _dbContext.ApplicationSettings.FirstOrDefaultAsync();
        if (appSettings != null)
        {
            appSettings.IncludePricingInResults = false;
            appSettings.LastCacheReset = DateTime.UtcNow;
            appSettings.UpdatedAt = DateTime.UtcNow;
        }

        // Reset on-prem pricing to defaults
        var onPremEntity = await _dbContext.OnPremPricing.FirstOrDefaultAsync();
        if (onPremEntity != null)
        {
            var defaults = new OnPremPricingEntity();
            onPremEntity.ServerCostPerUnit = defaults.ServerCostPerUnit;
            onPremEntity.CostPerCore = defaults.CostPerCore;
            onPremEntity.CostPerGBRam = defaults.CostPerGBRam;
            onPremEntity.StorageCostPerTB = defaults.StorageCostPerTB;
            onPremEntity.HardwareLifespanYears = defaults.HardwareLifespanYears;
            onPremEntity.RackUnitCostPerMonth = defaults.RackUnitCostPerMonth;
            onPremEntity.PowerCostPerKwhMonth = defaults.PowerCostPerKwhMonth;
            onPremEntity.CoolingPUE = defaults.CoolingPUE;
            onPremEntity.NetworkCostPerMonth = defaults.NetworkCostPerMonth;
            onPremEntity.DevOpsSalaryPerYear = defaults.DevOpsSalaryPerYear;
            onPremEntity.SysAdminSalaryPerYear = defaults.SysAdminSalaryPerYear;
            onPremEntity.NodesPerDevOpsEngineer = defaults.NodesPerDevOpsEngineer;
            onPremEntity.NodesPerSysAdmin = defaults.NodesPerSysAdmin;
            onPremEntity.OpenShiftPerNodeYear = defaults.OpenShiftPerNodeYear;
            onPremEntity.TanzuPerCoreYear = defaults.TanzuPerCoreYear;
            onPremEntity.RancherEnterprisePerNodeYear = defaults.RancherEnterprisePerNodeYear;
            onPremEntity.CharmedK8sPerNodeYear = defaults.CharmedK8sPerNodeYear;
            onPremEntity.RKE2PerNodeYear = defaults.RKE2PerNodeYear;
            onPremEntity.K3sPerNodeYear = defaults.K3sPerNodeYear;
            onPremEntity.UpdatedAt = DateTime.UtcNow;
        }

        // Reset Mendix pricing to defaults (June 2025 Pricebook values)
        var mendixEntity = await _dbContext.MendixPricing.FirstOrDefaultAsync();
        if (mendixEntity != null)
        {
            var defaults = new MendixPricingEntity();

            // Cloud Token & Dedicated
            mendixEntity.CloudTokenPrice = defaults.CloudTokenPrice;
            mendixEntity.CloudDedicatedPrice = defaults.CloudDedicatedPrice;

            // Additional Storage
            mendixEntity.AdditionalFileStoragePer100GB = defaults.AdditionalFileStoragePer100GB;
            mendixEntity.AdditionalDatabaseStoragePer100GB = defaults.AdditionalDatabaseStoragePer100GB;

            // Azure pricing
            mendixEntity.AzureBasePackagePrice = defaults.AzureBasePackagePrice;
            mendixEntity.AzureBaseEnvironmentsIncluded = defaults.AzureBaseEnvironmentsIncluded;
            mendixEntity.AzureAdditionalEnvironmentPrice = defaults.AzureAdditionalEnvironmentPrice;
            mendixEntity.AzureAdditionalEnvironmentTokens = defaults.AzureAdditionalEnvironmentTokens;

            // K8s pricing
            mendixEntity.K8sBasePackagePrice = defaults.K8sBasePackagePrice;
            mendixEntity.K8sBaseEnvironmentsIncluded = defaults.K8sBaseEnvironmentsIncluded;
            mendixEntity.K8sEnvTier1Price = defaults.K8sEnvTier1Price;
            mendixEntity.K8sEnvTier1Max = defaults.K8sEnvTier1Max;
            mendixEntity.K8sEnvTier2Price = defaults.K8sEnvTier2Price;
            mendixEntity.K8sEnvTier2Max = defaults.K8sEnvTier2Max;
            mendixEntity.K8sEnvTier3Price = defaults.K8sEnvTier3Price;
            mendixEntity.K8sEnvTier3Max = defaults.K8sEnvTier3Max;
            mendixEntity.K8sEnvTier4Price = defaults.K8sEnvTier4Price;

            // Server/StackIT/SAP pricing
            mendixEntity.ServerPerAppPrice = defaults.ServerPerAppPrice;
            mendixEntity.ServerUnlimitedAppsPrice = defaults.ServerUnlimitedAppsPrice;
            mendixEntity.StackITPerAppPrice = defaults.StackITPerAppPrice;
            mendixEntity.StackITUnlimitedAppsPrice = defaults.StackITUnlimitedAppsPrice;
            mendixEntity.SapBtpPerAppPrice = defaults.SapBtpPerAppPrice;
            mendixEntity.SapBtpUnlimitedAppsPrice = defaults.SapBtpUnlimitedAppsPrice;

            // GenAI pricing
            mendixEntity.GenAIModelPackSPrice = defaults.GenAIModelPackSPrice;
            mendixEntity.GenAIModelPackSTokens = defaults.GenAIModelPackSTokens;
            mendixEntity.GenAIModelPackMPrice = defaults.GenAIModelPackMPrice;
            mendixEntity.GenAIModelPackMTokens = defaults.GenAIModelPackMTokens;
            mendixEntity.GenAIModelPackLPrice = defaults.GenAIModelPackLPrice;
            mendixEntity.GenAIModelPackLTokens = defaults.GenAIModelPackLTokens;
            mendixEntity.GenAIKnowledgeBasePrice = defaults.GenAIKnowledgeBasePrice;
            mendixEntity.GenAIKnowledgeBaseTokens = defaults.GenAIKnowledgeBaseTokens;
            mendixEntity.GenAIKnowledgeBaseDiskGB = defaults.GenAIKnowledgeBaseDiskGB;

            // Resource packs and providers
            mendixEntity.ResourcePackPricingJson = defaults.ResourcePackPricingJson;
            mendixEntity.SupportedProvidersJson = defaults.SupportedProvidersJson;

            mendixEntity.UpdatedAt = DateTime.UtcNow;
        }

        // Reset OutSystems pricing to defaults (Partner Calculator 2024/2025 values)
        var outSystemsEntity = await _dbContext.OutSystemsPricing.FirstOrDefaultAsync();
        if (outSystemsEntity != null)
        {
            var defaults = new OutSystemsPricingEntity();

            // Edition pricing
            outSystemsEntity.StandardEditionBase = defaults.StandardEditionBase;
            outSystemsEntity.StandardEditionAOsIncluded = defaults.StandardEditionAOsIncluded;
            outSystemsEntity.StandardEditionInternalUsersIncluded = defaults.StandardEditionInternalUsersIncluded;
            outSystemsEntity.EnterpriseEditionBase = defaults.EnterpriseEditionBase;
            outSystemsEntity.EnterpriseEditionAOsIncluded = defaults.EnterpriseEditionAOsIncluded;
            outSystemsEntity.EnterpriseEditionInternalUsersIncluded = defaults.EnterpriseEditionInternalUsersIncluded;

            // AO pricing
            outSystemsEntity.AOPackSize = defaults.AOPackSize;
            outSystemsEntity.AdditionalAOPackPrice = defaults.AdditionalAOPackPrice;

            // User licensing
            outSystemsEntity.InternalUserPackSize = defaults.InternalUserPackSize;
            outSystemsEntity.AdditionalInternalUserPackPrice = defaults.AdditionalInternalUserPackPrice;
            outSystemsEntity.ExternalUserPackSize = defaults.ExternalUserPackSize;
            outSystemsEntity.ExternalUserPackPerYear = defaults.ExternalUserPackPerYear;
            outSystemsEntity.UnlimitedUsersPrice = defaults.UnlimitedUsersPrice;

            // AO-Pack scaled add-ons
            outSystemsEntity.Support24x7PremiumPerAOPack = defaults.Support24x7PremiumPerAOPack;
            outSystemsEntity.NonProductionEnvPerAOPack = defaults.NonProductionEnvPerAOPack;
            outSystemsEntity.LoadTestEnvPerAOPack = defaults.LoadTestEnvPerAOPack;
            outSystemsEntity.EnvironmentPackPerAOPack = defaults.EnvironmentPackPerAOPack;
            outSystemsEntity.HighAvailabilityPerAOPack = defaults.HighAvailabilityPerAOPack;
            outSystemsEntity.SentryPerAOPack = defaults.SentryPerAOPack;
            outSystemsEntity.DisasterRecoveryPerAOPack = defaults.DisasterRecoveryPerAOPack;

            // Flat fee add-ons
            outSystemsEntity.LogStreamingPrice = defaults.LogStreamingPrice;
            outSystemsEntity.DatabaseReplicaPrice = defaults.DatabaseReplicaPrice;
            outSystemsEntity.AppShieldPerUser = defaults.AppShieldPerUser;

            // Services
            outSystemsEntity.EssentialSuccessPlanPrice = defaults.EssentialSuccessPlanPrice;
            outSystemsEntity.PremierSuccessPlanPrice = defaults.PremierSuccessPlanPrice;
            outSystemsEntity.DedicatedGroupSessionPrice = defaults.DedicatedGroupSessionPrice;
            outSystemsEntity.PublicSessionPrice = defaults.PublicSessionPrice;
            outSystemsEntity.ExpertDayPrice = defaults.ExpertDayPrice;

            // Cloud VM pricing
            outSystemsEntity.AzureVMPricingJson = defaults.AzureVMPricingJson;
            outSystemsEntity.AwsEC2PricingJson = defaults.AwsEC2PricingJson;
            outSystemsEntity.HoursPerMonth = defaults.HoursPerMonth;
            outSystemsEntity.CloudOnlyFeaturesJson = defaults.CloudOnlyFeaturesJson;

            outSystemsEntity.UpdatedAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync();

        // Clear cache
        _cachedSettings = null;

        OnSettingsChanged?.Invoke();
    }

    public async Task ResetPricingCacheAsync()
    {
        var appSettings = await _dbContext.ApplicationSettings.FirstOrDefaultAsync();
        if (appSettings != null)
        {
            appSettings.LastCacheReset = DateTime.UtcNow;
            appSettings.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }

        // Clear cache
        _cachedSettings = null;
    }

    public async Task<PricingCacheStatus> GetCacheStatusAsync()
    {
        var settings = await GetSettingsAsync();

        var status = new PricingCacheStatus
        {
            LastReset = settings.LastCacheReset,
            ConfiguredApiCount = settings.CloudApiConfigs.Count(c => c.Value.IsConfigured),
            IsStale = settings.LastCacheReset == null ||
                      (DateTime.UtcNow - settings.LastCacheReset.Value).TotalHours > 24
        };

        var credentialsWithCachedData = await _dbContext.CloudApiCredentials
            .Where(c => c.LastValidated != null)
            .CountAsync();
        status.CachedProviderCount = credentialsWithCachedData;

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
        return GetSettingsAsync().Result.OnPremDefaults;
    }

    public async Task UpdateOnPremDefaultsAsync(OnPremPricing defaults)
    {
        var settings = await GetSettingsAsync();
        settings.OnPremDefaults = defaults;
        settings.LastModified = DateTime.UtcNow;
        await SaveSettingsAsync(settings);
    }

    public async Task ConfigureCloudApiAsync(CloudProvider provider, CloudApiConfig config)
    {
        var credEntity = await _dbContext.CloudApiCredentials
            .FirstOrDefaultAsync(c => c.ProviderName == provider.ToString());

        if (credEntity == null)
        {
            credEntity = new CloudApiCredentialsEntity { ProviderName = provider.ToString() };
            _dbContext.CloudApiCredentials.Add(credEntity);
        }

        credEntity.ApiKey = config.ApiKey;
        credEntity.SecretKey = config.SecretKey;
        credEntity.Region = config.DefaultRegion;
        credEntity.IsConfigured = config.IsConfigured;
        credEntity.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        // Update cache
        if (_cachedSettings != null)
        {
            _cachedSettings.CloudApiConfigs[provider] = config;
        }
    }

    public async Task<bool> ValidateCloudApiAsync(CloudProvider provider)
    {
        var credEntity = await _dbContext.CloudApiCredentials
            .FirstOrDefaultAsync(c => c.ProviderName == provider.ToString());

        if (credEntity == null || !credEntity.IsConfigured)
        {
            return false;
        }

        // In a real implementation, this would make API calls to validate credentials
        // For now, we just mark it as validated
        credEntity.LastValidated = DateTime.UtcNow;
        credEntity.ValidationStatus = "Valid";
        await _dbContext.SaveChangesAsync();

        return true;
    }

    public CloudApiConfig? GetCloudApiConfig(CloudProvider provider)
    {
        var settings = GetSettingsAsync().Result;
        return settings.CloudApiConfigs.TryGetValue(provider, out var config) ? config : null;
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

        var onPrem = GetOnPremDefaults();

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

    #region Mendix Pricing Methods

    public MendixPricingSettings GetMendixPricingSettings()
    {
        return GetSettingsAsync().Result.MendixPricing;
    }

    public async Task UpdateMendixPricingSettingsAsync(MendixPricingSettings settings)
    {
        var mendixEntity = await _dbContext.MendixPricing.FirstOrDefaultAsync();
        if (mendixEntity == null)
        {
            mendixEntity = new MendixPricingEntity();
            _dbContext.MendixPricing.Add(mendixEntity);
        }

        MapMendixToEntity(settings, mendixEntity);
        mendixEntity.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        // Update cache
        if (_cachedSettings != null)
        {
            _cachedSettings.MendixPricing = settings;
        }

        OnSettingsChanged?.Invoke();
    }

    public MendixPricingResult CalculateMendixCost(MendixDeploymentConfig config)
    {
        var mendix = GetMendixPricingSettings();
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
            result.DeploymentTypeName = "Mendix Cloud Dedicated";
            result.DeploymentFeeCost = mendix.CloudDedicatedPricePerYear;
            result.PlatformLicenseCost = mendix.PlatformPremiumUnlimitedPerYear;
        }
        else
        {
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

            result.PlatformLicenseCost = mendix.PlatformPremiumUnlimitedPerYear;

            if (config.AdditionalFileStorageGB > 0)
            {
                var fileStorageBlocks = (int)Math.Ceiling((double)config.AdditionalFileStorageGB / 100);
                result.StorageCost += fileStorageBlocks * mendix.AdditionalFileStoragePer100GB;
            }

            if (config.AdditionalDatabaseStorageGB > 0)
            {
                var dbStorageBlocks = (int)Math.Ceiling((double)config.AdditionalDatabaseStorageGB / 100);
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
            result.DeploymentTypeName = "Mendix on Azure";
            result.DeploymentFeeCost = mendix.AzureBasePricePerYear;

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
            var isSupported = IsMendixSupportedProvider(config.PrivateCloudProvider ?? MendixPrivateCloudProvider.GenericK8s);
            var providerName = config.PrivateCloudProvider?.ToString() ?? "Kubernetes";

            result.DeploymentTypeName = isSupported
                ? $"Mendix on Kubernetes ({providerName})"
                : $"Mendix on Kubernetes ({providerName}) - Manual Setup";

            result.DeploymentFeeCost = mendix.K8sBasePricePerYear;

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
        var mendix = GetMendixPricingSettings();
        var packs = mendix.GetAvailablePacks(tier);

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

    #endregion

    #region OutSystems Pricing Methods

    public OutSystemsPricingSettings GetOutSystemsPricingSettings()
    {
        return GetSettingsAsync().Result.OutSystemsPricing;
    }

    public async Task UpdateOutSystemsPricingSettingsAsync(OutSystemsPricingSettings settings)
    {
        var outSystemsEntity = await _dbContext.OutSystemsPricing.FirstOrDefaultAsync();
        if (outSystemsEntity == null)
        {
            outSystemsEntity = new OutSystemsPricingEntity();
            _dbContext.OutSystemsPricing.Add(outSystemsEntity);
        }

        MapOutSystemsToEntity(settings, outSystemsEntity);
        outSystemsEntity.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        // Update cache
        if (_cachedSettings != null)
        {
            _cachedSettings.OutSystemsPricing = settings;
        }

        OnSettingsChanged?.Invoke();
    }

    public OutSystemsPricingResult CalculateOutSystemsCost(OutSystemsDeploymentConfig config)
    {
        var pricing = GetOutSystemsPricingSettings();
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

        // ==================== LICENSE COSTS ====================

        // Edition base cost
        result.EditionBaseCost = pricing.GetEditionBasePrice(config.Edition);

        // Additional AO packs (1 pack included in base)
        result.AdditionalAOPacks = Math.Max(0, result.AOPackCount - 1);
        result.AdditionalAOsCost = result.AdditionalAOPacks * pricing.AdditionalAOPackPrice;

        // User licensing
        CalculateUserLicenseCost(config, pricing, result);

        // ==================== ADD-ON COSTS ====================

        // AO-pack scaled add-ons
        var aoPackCount = result.AOPackCount;
        var isCloud = config.DeploymentType == OutSystemsDeploymentType.Cloud;

        if (config.Include24x7PremiumSupport)
        {
            result.Support24x7PremiumCost = pricing.Support24x7PremiumPerAOPack * aoPackCount;
            AddLineItem(result, "Add-ons", "24x7 Premium Support",
                $"${pricing.Support24x7PremiumPerAOPack}/pack × {aoPackCount} packs",
                result.Support24x7PremiumCost, false);
        }

        if (config.IncludeNonProductionEnv)
        {
            result.NonProductionEnvCost = pricing.NonProductionEnvPerAOPack * aoPackCount;
            AddLineItem(result, "Add-ons", "Non-Production Environment",
                $"${pricing.NonProductionEnvPerAOPack}/pack × {aoPackCount} packs",
                result.NonProductionEnvCost, false);
        }

        if (config.IncludeLoadTestEnv && isCloud)
        {
            result.LoadTestEnvCost = pricing.LoadTestEnvPerAOPack * aoPackCount;
            AddLineItem(result, "Add-ons", "Load Testing Environment",
                $"${pricing.LoadTestEnvPerAOPack}/pack × {aoPackCount} packs",
                result.LoadTestEnvCost, true);
        }

        if (config.IncludeEnvironmentPack)
        {
            result.EnvironmentPackCost = pricing.EnvironmentPackPerAOPack * aoPackCount;
            AddLineItem(result, "Add-ons", "Environment Pack",
                $"${pricing.EnvironmentPackPerAOPack}/pack × {aoPackCount} packs",
                result.EnvironmentPackCost, false);
        }

        // Sentry includes HA, so only add one
        if (config.IncludeSentry && isCloud)
        {
            result.SentryCost = pricing.SentryPerAOPack * aoPackCount;
            AddLineItem(result, "Add-ons", "Sentry (includes HA)",
                $"${pricing.SentryPerAOPack}/pack × {aoPackCount} packs",
                result.SentryCost, true);
        }
        else if (config.IncludeHA && isCloud)
        {
            result.HACost = pricing.HighAvailabilityPerAOPack * aoPackCount;
            AddLineItem(result, "Add-ons", "High Availability",
                $"${pricing.HighAvailabilityPerAOPack}/pack × {aoPackCount} packs",
                result.HACost, true);
        }

        if (config.IncludeDR)
        {
            result.DRCost = pricing.DisasterRecoveryPerAOPack * aoPackCount;
            AddLineItem(result, "Add-ons", "Disaster Recovery",
                $"${pricing.DisasterRecoveryPerAOPack}/pack × {aoPackCount} packs",
                result.DRCost, false);
        }

        // Flat fee add-ons
        if (config.IncludeLogStreaming && isCloud)
        {
            result.LogStreamingCost = pricing.LogStreamingPrice;
            AddLineItem(result, "Add-ons", "Log Streaming",
                "Flat fee/year",
                result.LogStreamingCost, true);
        }

        if (config.IncludeDatabaseReplica && isCloud)
        {
            result.DatabaseReplicaCost = pricing.DatabaseReplicaPrice;
            AddLineItem(result, "Add-ons", "Database Replica",
                "Flat fee/year",
                result.DatabaseReplicaCost, true);
        }

        if (config.AppShieldUsers > 0)
        {
            result.AppShieldCost = pricing.AppShieldPerUser * config.AppShieldUsers;
            AddLineItem(result, "Add-ons", "AppShield",
                $"${pricing.AppShieldPerUser}/user × {config.AppShieldUsers} users",
                result.AppShieldCost, false);
        }

        // ==================== INFRASTRUCTURE COSTS ====================

        if (config.DeploymentType == OutSystemsDeploymentType.SelfManaged &&
            config.CloudProvider != OutSystemsCloudProvider.OnPremises)
        {
            CalculateCloudVMCost(config, pricing, result);
        }

        // ==================== SERVICES COSTS ====================

        result.SuccessPlanCost = pricing.CalculateSuccessPlanCost(config.SuccessPlan);
        if (config.SuccessPlan != OutSystemsSuccessPlan.None)
        {
            AddLineItem(result, "Services", $"{config.SuccessPlan} Success Plan",
                "Annual subscription",
                result.SuccessPlanCost, false);
        }

        // Training costs
        result.TrainingCost = (config.DedicatedGroupSessions * pricing.DedicatedGroupSessionPrice) +
                              (config.PublicSessions * pricing.PublicSessionPrice);
        if (result.TrainingCost > 0)
        {
            AddLineItem(result, "Services", "Training",
                $"{config.DedicatedGroupSessions} dedicated + {config.PublicSessions} public sessions",
                result.TrainingCost, false);
        }

        // Expert days
        result.ExpertDaysCost = config.ExpertDays * pricing.ExpertDayPrice;
        if (result.ExpertDaysCost > 0)
        {
            AddLineItem(result, "Services", "Expert Days",
                $"{config.ExpertDays} days × ${pricing.ExpertDayPrice}/day",
                result.ExpertDaysCost, false);
        }

        // ==================== ENVIRONMENT DETAILS ====================

        result.EnvironmentDetails = $"{config.ProductionEnvironments} Production, " +
                                    $"{config.NonProductionEnvironments} Non-Production";

        return result;
    }

    private void CalculateUserLicenseCost(
        OutSystemsDeploymentConfig config,
        OutSystemsPricingSettings pricing,
        OutSystemsPricingResult result)
    {
        if (config.UseUnlimitedUsers)
        {
            result.UserLicenseCost = pricing.UnlimitedUsersPrice;
            result.UserLicenseDetails = "Unlimited Users";
            AddLineItem(result, "Licensing", "Unlimited Users",
                "Flat fee/year",
                result.UserLicenseCost, false);
            return;
        }

        decimal userCost = 0;
        var details = new List<string>();

        // Internal users (some included in edition)
        var includedInternal = pricing.GetIncludedInternalUsers(config.Edition);
        var additionalInternal = Math.Max(0, config.InternalUsers - includedInternal);

        if (additionalInternal > 0)
        {
            var internalPacks = (int)Math.Ceiling(additionalInternal / (double)pricing.InternalUserPackSize);
            var internalCost = internalPacks * pricing.AdditionalInternalUserPackPrice;
            userCost += internalCost;
            details.Add($"{config.InternalUsers} internal ({includedInternal} included, {additionalInternal} additional)");

            if (internalCost > 0)
            {
                AddLineItem(result, "Licensing", "Additional Internal Users",
                    $"{internalPacks} pack(s) × ${pricing.AdditionalInternalUserPackPrice}",
                    internalCost, false);
            }
        }
        else
        {
            details.Add($"{config.InternalUsers} internal (included in {config.Edition})");
        }

        // External users
        if (config.ExternalUsers > 0)
        {
            var externalCost = pricing.CalculateExternalUsersCost(config.ExternalUsers);
            userCost += externalCost;
            var externalPacks = (int)Math.Ceiling(config.ExternalUsers / (double)pricing.ExternalUserPackSize);
            details.Add($"{config.ExternalUsers} external ({externalPacks} pack(s))");

            AddLineItem(result, "Licensing", "External Users",
                $"{externalPacks} pack(s) × ${pricing.ExternalUserPackPricePerYear}",
                externalCost, false);
        }

        result.UserLicenseCost = userCost;
        result.UserLicenseDetails = string.Join(", ", details);
    }

    private void CalculateCloudVMCost(
        OutSystemsDeploymentConfig config,
        OutSystemsPricingSettings pricing,
        OutSystemsPricingResult result)
    {
        var totalEnvs = config.TotalEnvironments;
        var serversPerEnv = config.FrontEndServersPerEnvironment;
        var totalServers = totalEnvs * serversPerEnv;

        result.TotalVMCount = totalServers;

        if (config.CloudProvider == OutSystemsCloudProvider.Azure)
        {
            var monthlyVMCost = pricing.CalculateAzureMonthlyVMCost(config.AzureInstanceType, totalServers);
            result.MonthlyVMCost = monthlyVMCost;

            var specs = OutSystemsPricingSettings.GetAzureInstanceSpecs(config.AzureInstanceType);
            result.VMDetails = $"{totalServers}x Azure {config.AzureInstanceType} " +
                              $"({specs.vCPU} vCPU, {specs.RamGB} GB each)";

            AddLineItem(result, "Infrastructure", "Azure VMs",
                $"{totalServers} × {config.AzureInstanceType} @ {pricing.HoursPerMonth}h/month",
                result.AnnualVMCost, false);
        }
        else if (config.CloudProvider == OutSystemsCloudProvider.AWS)
        {
            var monthlyVMCost = pricing.CalculateAwsMonthlyVMCost(config.AwsInstanceType, totalServers);
            result.MonthlyVMCost = monthlyVMCost;

            var specs = OutSystemsPricingSettings.GetAwsInstanceSpecs(config.AwsInstanceType);
            result.VMDetails = $"{totalServers}x AWS {config.AwsInstanceType} " +
                              $"({specs.vCPU} vCPU, {specs.RamGB} GB each)";

            AddLineItem(result, "Infrastructure", "AWS EC2 Instances",
                $"{totalServers} × {config.AwsInstanceType} @ {pricing.HoursPerMonth}h/month",
                result.AnnualVMCost, false);
        }
    }

    private static void AddLineItem(
        OutSystemsPricingResult result,
        string category,
        string name,
        string? description,
        decimal amount,
        bool isCloudOnly)
    {
        result.LineItems.Add(new OutSystemsCostLineItem
        {
            Category = category,
            Name = name,
            Description = description,
            Amount = amount,
            IsCloudOnly = isCloudOnly,
            IsIncluded = false
        });
    }

    public bool IsOutSystemsCloudOnlyFeature(string featureName)
    {
        return OutSystemsPricingSettings.IsCloudOnlyFeature(featureName);
    }

    public OutSystemsAzureInstanceType RecommendAzureInstance(int totalCores, int totalRamGB)
    {
        // Recommend based on memory requirements primarily
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
        // Recommend based on memory requirements primarily
        if (totalRamGB <= 8 && totalCores <= 2)
            return OutSystemsAwsInstanceType.M5Large;
        if (totalRamGB <= 16 && totalCores <= 4)
            return OutSystemsAwsInstanceType.M5XLarge;
        return OutSystemsAwsInstanceType.M52XLarge;
    }

    #endregion

    #region Mapping Helpers

    private static OnPremPricing MapOnPremFromEntity(OnPremPricingEntity? entity)
    {
        if (entity == null)
            return new OnPremPricing();

        var pricing = new OnPremPricing();

        // Map hardware costs using correct property names
        pricing.Hardware.ServerCost = entity.ServerCostPerUnit;
        pricing.Hardware.PerCpuCore = entity.CostPerCore;
        pricing.Hardware.PerGBRam = entity.CostPerGBRam;
        pricing.Hardware.PerTBSsd = entity.StorageCostPerTB;

        // Map data center costs using correct property names
        pricing.DataCenter.RackUnitPerMonth = entity.RackUnitCostPerMonth;
        pricing.DataCenter.PowerPerKWh = entity.PowerCostPerKwhMonth;
        pricing.DataCenter.PUE = entity.CoolingPUE;

        // Map labor costs using correct property names
        pricing.Labor.DevOpsEngineerMonthly = entity.DevOpsSalaryPerYear / 12;
        pricing.Labor.SysAdminMonthly = entity.SysAdminSalaryPerYear / 12;
        pricing.Labor.NodesPerEngineer = entity.NodesPerDevOpsEngineer;

        // Map license costs using correct property names
        pricing.Licensing.OpenShiftPerNodeYear = entity.OpenShiftPerNodeYear;
        pricing.Licensing.TanzuPerCoreYear = entity.TanzuPerCoreYear;
        pricing.Licensing.RancherEnterprisePerNodeYear = entity.RancherEnterprisePerNodeYear;
        pricing.Licensing.CharmedK8sPerNodeYear = entity.CharmedK8sPerNodeYear;
        pricing.Licensing.RKE2PerNodeYear = entity.RKE2PerNodeYear;
        pricing.Licensing.K3sPerNodeYear = entity.K3sPerNodeYear;

        pricing.HardwareRefreshYears = entity.HardwareLifespanYears;

        return pricing;
    }

    private static void MapOnPremToEntity(OnPremPricing pricing, OnPremPricingEntity entity)
    {
        entity.ServerCostPerUnit = pricing.Hardware.ServerCost;
        entity.CostPerCore = pricing.Hardware.PerCpuCore;
        entity.CostPerGBRam = pricing.Hardware.PerGBRam;
        entity.StorageCostPerTB = pricing.Hardware.PerTBSsd;
        entity.HardwareLifespanYears = pricing.HardwareRefreshYears;

        entity.RackUnitCostPerMonth = pricing.DataCenter.RackUnitPerMonth;
        entity.PowerCostPerKwhMonth = pricing.DataCenter.PowerPerKWh;
        entity.CoolingPUE = pricing.DataCenter.PUE;

        entity.DevOpsSalaryPerYear = pricing.Labor.DevOpsEngineerMonthly * 12;
        entity.SysAdminSalaryPerYear = pricing.Labor.SysAdminMonthly * 12;
        entity.NodesPerDevOpsEngineer = pricing.Labor.NodesPerEngineer;

        entity.OpenShiftPerNodeYear = pricing.Licensing.OpenShiftPerNodeYear;
        entity.TanzuPerCoreYear = pricing.Licensing.TanzuPerCoreYear;
        entity.RancherEnterprisePerNodeYear = pricing.Licensing.RancherEnterprisePerNodeYear;
        entity.CharmedK8sPerNodeYear = pricing.Licensing.CharmedK8sPerNodeYear;
        entity.RKE2PerNodeYear = pricing.Licensing.RKE2PerNodeYear;
        entity.K3sPerNodeYear = pricing.Licensing.K3sPerNodeYear;
    }

    private static MendixPricingSettings MapMendixFromEntity(MendixPricingEntity? entity)
    {
        if (entity == null)
            return new MendixPricingSettings();

        var settings = new MendixPricingSettings
        {
            // Cloud Token
            CloudTokenPrice = entity.CloudTokenPrice,

            // Cloud Dedicated
            CloudDedicatedPricePerYear = entity.CloudDedicatedPrice,

            // Additional Storage
            AdditionalFileStoragePer100GB = entity.AdditionalFileStoragePer100GB,
            AdditionalDatabaseStoragePer100GB = entity.AdditionalDatabaseStoragePer100GB,

            // Azure pricing
            AzureBasePricePerYear = entity.AzureBasePackagePrice,
            AzureBaseEnvironmentsIncluded = entity.AzureBaseEnvironmentsIncluded,
            AzureAdditionalEnvironmentPrice = entity.AzureAdditionalEnvironmentPrice,
            AzureAdditionalEnvironmentTokens = entity.AzureAdditionalEnvironmentTokens,

            // K8s pricing
            K8sBasePricePerYear = entity.K8sBasePackagePrice,
            K8sBaseEnvironmentsIncluded = entity.K8sBaseEnvironmentsIncluded,
            K8sEnvironmentTiers = new List<MendixK8sEnvironmentTier>
            {
                new() { MinEnvironments = 1, MaxEnvironments = entity.K8sEnvTier1Max, PricePerEnvironment = entity.K8sEnvTier1Price, Description = "Up to 50 environments" },
                new() { MinEnvironments = entity.K8sEnvTier1Max + 1, MaxEnvironments = entity.K8sEnvTier2Max, PricePerEnvironment = entity.K8sEnvTier2Price, Description = "51 to 100 environments" },
                new() { MinEnvironments = entity.K8sEnvTier2Max + 1, MaxEnvironments = entity.K8sEnvTier3Max, PricePerEnvironment = entity.K8sEnvTier3Price, Description = "101 to 150 environments" },
                new() { MinEnvironments = entity.K8sEnvTier3Max + 1, MaxEnvironments = -1, PricePerEnvironment = entity.K8sEnvTier4Price, Description = "From 151+ (Free)" }
            },

            // Server pricing
            ServerPerAppPricePerYear = entity.ServerPerAppPrice,
            ServerUnlimitedAppsPricePerYear = entity.ServerUnlimitedAppsPrice,

            // StackIT pricing
            StackITPerAppPricePerYear = entity.StackITPerAppPrice,
            StackITUnlimitedAppsPricePerYear = entity.StackITUnlimitedAppsPrice,

            // SAP BTP pricing
            SapBtpPerAppPricePerYear = entity.SapBtpPerAppPrice,
            SapBtpUnlimitedAppsPricePerYear = entity.SapBtpUnlimitedAppsPrice,

            // GenAI pricing
            GenAIModelPacks = new List<MendixGenAIModelPack>
            {
                new() { Size = "S", PricePerYear = entity.GenAIModelPackSPrice, CloudTokens = entity.GenAIModelPackSTokens, ClaudeTokensInPerMonth = 2500000, ClaudeTokensOutPerMonth = 1250000, CohereTokensInPerMonth = 5000000 },
                new() { Size = "M", PricePerYear = entity.GenAIModelPackMPrice, CloudTokens = entity.GenAIModelPackMTokens, ClaudeTokensInPerMonth = 5000000, ClaudeTokensOutPerMonth = 2500000, CohereTokensInPerMonth = 10000000 },
                new() { Size = "L", PricePerYear = entity.GenAIModelPackLPrice, CloudTokens = entity.GenAIModelPackLTokens, ClaudeTokensInPerMonth = 10000000, ClaudeTokensOutPerMonth = 5000000, CohereTokensInPerMonth = 20000000 }
            },
            GenAIKnowledgeBasePricePerYear = entity.GenAIKnowledgeBasePrice,
            GenAIKnowledgeBaseTokens = entity.GenAIKnowledgeBaseTokens
        };

        return settings;
    }

    private static void MapMendixToEntity(MendixPricingSettings settings, MendixPricingEntity entity)
    {
        // Cloud Token
        entity.CloudTokenPrice = settings.CloudTokenPrice;
        entity.CloudDedicatedPrice = settings.CloudDedicatedPricePerYear;

        // Additional Storage
        entity.AdditionalFileStoragePer100GB = settings.AdditionalFileStoragePer100GB;
        entity.AdditionalDatabaseStoragePer100GB = settings.AdditionalDatabaseStoragePer100GB;

        // Azure pricing
        entity.AzureBasePackagePrice = settings.AzureBasePricePerYear;
        entity.AzureBaseEnvironmentsIncluded = settings.AzureBaseEnvironmentsIncluded;
        entity.AzureAdditionalEnvironmentPrice = settings.AzureAdditionalEnvironmentPrice;
        entity.AzureAdditionalEnvironmentTokens = settings.AzureAdditionalEnvironmentTokens;

        // K8s pricing
        entity.K8sBasePackagePrice = settings.K8sBasePricePerYear;
        entity.K8sBaseEnvironmentsIncluded = settings.K8sBaseEnvironmentsIncluded;

        // Map K8s tiers
        if (settings.K8sEnvironmentTiers.Count >= 4)
        {
            entity.K8sEnvTier1Price = settings.K8sEnvironmentTiers[0].PricePerEnvironment;
            entity.K8sEnvTier1Max = settings.K8sEnvironmentTiers[0].MaxEnvironments;
            entity.K8sEnvTier2Price = settings.K8sEnvironmentTiers[1].PricePerEnvironment;
            entity.K8sEnvTier2Max = settings.K8sEnvironmentTiers[1].MaxEnvironments;
            entity.K8sEnvTier3Price = settings.K8sEnvironmentTiers[2].PricePerEnvironment;
            entity.K8sEnvTier3Max = settings.K8sEnvironmentTiers[2].MaxEnvironments;
            entity.K8sEnvTier4Price = settings.K8sEnvironmentTiers[3].PricePerEnvironment;
        }

        // Server pricing
        entity.ServerPerAppPrice = settings.ServerPerAppPricePerYear;
        entity.ServerUnlimitedAppsPrice = settings.ServerUnlimitedAppsPricePerYear;

        // StackIT pricing
        entity.StackITPerAppPrice = settings.StackITPerAppPricePerYear;
        entity.StackITUnlimitedAppsPrice = settings.StackITUnlimitedAppsPricePerYear;

        // SAP BTP pricing
        entity.SapBtpPerAppPrice = settings.SapBtpPerAppPricePerYear;
        entity.SapBtpUnlimitedAppsPrice = settings.SapBtpUnlimitedAppsPricePerYear;

        // GenAI pricing
        if (settings.GenAIModelPacks.Count >= 3)
        {
            var smlPacks = settings.GenAIModelPacks.OrderBy(p => p.Size).ToList();
            entity.GenAIModelPackSPrice = smlPacks[0].PricePerYear;
            entity.GenAIModelPackSTokens = smlPacks[0].CloudTokens;
            entity.GenAIModelPackMPrice = smlPacks[1].PricePerYear;
            entity.GenAIModelPackMTokens = smlPacks[1].CloudTokens;
            entity.GenAIModelPackLPrice = smlPacks[2].PricePerYear;
            entity.GenAIModelPackLTokens = smlPacks[2].CloudTokens;
        }
        entity.GenAIKnowledgeBasePrice = settings.GenAIKnowledgeBasePricePerYear;
        entity.GenAIKnowledgeBaseTokens = settings.GenAIKnowledgeBaseTokens;
    }

    private static OutSystemsPricingSettings MapOutSystemsFromEntity(OutSystemsPricingEntity? entity)
    {
        if (entity == null)
            return new OutSystemsPricingSettings();

        var settings = new OutSystemsPricingSettings
        {
            // Edition pricing
            StandardEditionBasePrice = entity.StandardEditionBase,
            StandardEditionAOsIncluded = entity.StandardEditionAOsIncluded,
            StandardEditionInternalUsersIncluded = entity.StandardEditionInternalUsersIncluded,
            EnterpriseEditionBasePrice = entity.EnterpriseEditionBase,
            EnterpriseEditionAOsIncluded = entity.EnterpriseEditionAOsIncluded,
            EnterpriseEditionInternalUsersIncluded = entity.EnterpriseEditionInternalUsersIncluded,

            // AO pricing
            AOPackSize = entity.AOPackSize,
            AdditionalAOPackPrice = entity.AdditionalAOPackPrice,

            // User licensing
            InternalUserPackSize = entity.InternalUserPackSize,
            AdditionalInternalUserPackPrice = entity.AdditionalInternalUserPackPrice,
            ExternalUserPackSize = entity.ExternalUserPackSize,
            ExternalUserPackPricePerYear = entity.ExternalUserPackPerYear,
            UnlimitedUsersPrice = entity.UnlimitedUsersPrice,

            // AO-Pack scaled add-ons
            Support24x7PremiumPerAOPack = entity.Support24x7PremiumPerAOPack,
            NonProductionEnvPerAOPack = entity.NonProductionEnvPerAOPack,
            LoadTestEnvPerAOPack = entity.LoadTestEnvPerAOPack,
            EnvironmentPackPerAOPack = entity.EnvironmentPackPerAOPack,
            HighAvailabilityPerAOPack = entity.HighAvailabilityPerAOPack,
            SentryPerAOPack = entity.SentryPerAOPack,
            DisasterRecoveryPerAOPack = entity.DisasterRecoveryPerAOPack,

            // Flat fee add-ons
            LogStreamingPrice = entity.LogStreamingPrice,
            DatabaseReplicaPrice = entity.DatabaseReplicaPrice,
            AppShieldPerUser = entity.AppShieldPerUser,

            // Services
            EssentialSuccessPlanPrice = entity.EssentialSuccessPlanPrice,
            PremierSuccessPlanPrice = entity.PremierSuccessPlanPrice,
            DedicatedGroupSessionPrice = entity.DedicatedGroupSessionPrice,
            PublicSessionPrice = entity.PublicSessionPrice,
            ExpertDayPrice = entity.ExpertDayPrice,

            // Legacy self-managed
            SelfManagedBasePrice = entity.SelfManagedBase,
            SelfManagedPerEnvironmentPrice = entity.SelfManagedPerEnvironment,
            SelfManagedPerFrontEndPrice = entity.SelfManagedPerFrontEnd,

            // Legacy support
            PremiumSupportPercent = entity.PremiumSupportPercent,
            EliteSupportPercent = entity.EliteSupportPercent,

            // Hours per month for VM calculation
            HoursPerMonth = entity.HoursPerMonth
        };

        // Parse Azure VM pricing from JSON
        if (!string.IsNullOrEmpty(entity.AzureVMPricingJson))
        {
            try
            {
                var azurePricing = JsonSerializer.Deserialize<Dictionary<string, decimal>>(entity.AzureVMPricingJson);
                if (azurePricing != null)
                {
                    settings.AzureVMHourlyPricing.Clear();
                    foreach (var (key, value) in azurePricing)
                    {
                        if (Enum.TryParse<OutSystemsAzureInstanceType>(key, out var instanceType))
                        {
                            settings.AzureVMHourlyPricing[instanceType] = value;
                        }
                    }
                }
            }
            catch { /* Use defaults */ }
        }

        // Parse AWS EC2 pricing from JSON
        if (!string.IsNullOrEmpty(entity.AwsEC2PricingJson))
        {
            try
            {
                var awsPricing = JsonSerializer.Deserialize<Dictionary<string, decimal>>(entity.AwsEC2PricingJson);
                if (awsPricing != null)
                {
                    settings.AwsEC2HourlyPricing.Clear();
                    foreach (var (key, value) in awsPricing)
                    {
                        if (Enum.TryParse<OutSystemsAwsInstanceType>(key, out var instanceType))
                        {
                            settings.AwsEC2HourlyPricing[instanceType] = value;
                        }
                    }
                }
            }
            catch { /* Use defaults */ }
        }

        return settings;
    }

    private static void MapOutSystemsToEntity(OutSystemsPricingSettings settings, OutSystemsPricingEntity entity)
    {
        // Edition pricing
        entity.StandardEditionBase = settings.StandardEditionBasePrice;
        entity.StandardEditionAOsIncluded = settings.StandardEditionAOsIncluded;
        entity.StandardEditionInternalUsersIncluded = settings.StandardEditionInternalUsersIncluded;
        entity.EnterpriseEditionBase = settings.EnterpriseEditionBasePrice;
        entity.EnterpriseEditionAOsIncluded = settings.EnterpriseEditionAOsIncluded;
        entity.EnterpriseEditionInternalUsersIncluded = settings.EnterpriseEditionInternalUsersIncluded;

        // AO pricing
        entity.AOPackSize = settings.AOPackSize;
        entity.AdditionalAOPackPrice = settings.AdditionalAOPackPrice;

        // User licensing
        entity.InternalUserPackSize = settings.InternalUserPackSize;
        entity.AdditionalInternalUserPackPrice = settings.AdditionalInternalUserPackPrice;
        entity.ExternalUserPackSize = settings.ExternalUserPackSize;
        entity.ExternalUserPackPerYear = settings.ExternalUserPackPricePerYear;
        entity.UnlimitedUsersPrice = settings.UnlimitedUsersPrice;

        // AO-Pack scaled add-ons
        entity.Support24x7PremiumPerAOPack = settings.Support24x7PremiumPerAOPack;
        entity.NonProductionEnvPerAOPack = settings.NonProductionEnvPerAOPack;
        entity.LoadTestEnvPerAOPack = settings.LoadTestEnvPerAOPack;
        entity.EnvironmentPackPerAOPack = settings.EnvironmentPackPerAOPack;
        entity.HighAvailabilityPerAOPack = settings.HighAvailabilityPerAOPack;
        entity.SentryPerAOPack = settings.SentryPerAOPack;
        entity.DisasterRecoveryPerAOPack = settings.DisasterRecoveryPerAOPack;

        // Flat fee add-ons
        entity.LogStreamingPrice = settings.LogStreamingPrice;
        entity.DatabaseReplicaPrice = settings.DatabaseReplicaPrice;
        entity.AppShieldPerUser = settings.AppShieldPerUser;

        // Services
        entity.EssentialSuccessPlanPrice = settings.EssentialSuccessPlanPrice;
        entity.PremierSuccessPlanPrice = settings.PremierSuccessPlanPrice;
        entity.DedicatedGroupSessionPrice = settings.DedicatedGroupSessionPrice;
        entity.PublicSessionPrice = settings.PublicSessionPrice;
        entity.ExpertDayPrice = settings.ExpertDayPrice;

        // Legacy self-managed
        entity.SelfManagedBase = settings.SelfManagedBasePrice;
        entity.SelfManagedPerEnvironment = settings.SelfManagedPerEnvironmentPrice;
        entity.SelfManagedPerFrontEnd = settings.SelfManagedPerFrontEndPrice;

        // Legacy support
        entity.PremiumSupportPercent = settings.PremiumSupportPercent;
        entity.EliteSupportPercent = settings.EliteSupportPercent;

        // Hours per month
        entity.HoursPerMonth = settings.HoursPerMonth;

        // Serialize Azure VM pricing to JSON
        var azureDict = settings.AzureVMHourlyPricing.ToDictionary(
            kvp => kvp.Key.ToString(),
            kvp => kvp.Value);
        entity.AzureVMPricingJson = JsonSerializer.Serialize(azureDict);

        // Serialize AWS EC2 pricing to JSON
        var awsDict = settings.AwsEC2HourlyPricing.ToDictionary(
            kvp => kvp.Key.ToString(),
            kvp => kvp.Value);
        entity.AwsEC2PricingJson = JsonSerializer.Serialize(awsDict);
    }

    #endregion
}
