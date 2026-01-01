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

        // Reset OutSystems pricing to defaults (Partner Calculator January 2026 values)
        var outSystemsEntity = await _dbContext.OutSystemsPricing.FirstOrDefaultAsync();
        if (outSystemsEntity != null)
        {
            var defaults = new OutSystemsPricingEntity();

            // ODC Platform Pricing
            outSystemsEntity.OdcPlatformBasePrice = defaults.OdcPlatformBasePrice;
            outSystemsEntity.OdcAOPackPrice = defaults.OdcAOPackPrice;
            outSystemsEntity.OdcInternalUserPackPrice = defaults.OdcInternalUserPackPrice;
            outSystemsEntity.OdcExternalUserPackPrice = defaults.OdcExternalUserPackPrice;

            // O11 Platform Pricing
            outSystemsEntity.O11EnterpriseBasePrice = defaults.O11EnterpriseBasePrice;
            outSystemsEntity.O11AOPackPrice = defaults.O11AOPackPrice;

            // Unlimited Users (per AO Pack)
            outSystemsEntity.UnlimitedUsersPerAOPack = defaults.UnlimitedUsersPerAOPack;

            // O11 Tiered User Pricing (JSON)
            outSystemsEntity.O11InternalUserTiersJson = defaults.O11InternalUserTiersJson;
            outSystemsEntity.O11ExternalUserTiersJson = defaults.O11ExternalUserTiersJson;

            // ODC Add-ons
            outSystemsEntity.OdcSupport24x7ExtendedPerPack = defaults.OdcSupport24x7ExtendedPerPack;
            outSystemsEntity.OdcSupport24x7PremiumPerPack = defaults.OdcSupport24x7PremiumPerPack;
            outSystemsEntity.OdcHighAvailabilityPerPack = defaults.OdcHighAvailabilityPerPack;
            outSystemsEntity.OdcNonProdRuntimePerPack = defaults.OdcNonProdRuntimePerPack;
            outSystemsEntity.OdcPrivateGatewayPerPack = defaults.OdcPrivateGatewayPerPack;
            outSystemsEntity.OdcSentryPerPack = defaults.OdcSentryPerPack;

            // O11 Add-ons
            outSystemsEntity.O11Support24x7PremiumPerPack = defaults.O11Support24x7PremiumPerPack;
            outSystemsEntity.O11HighAvailabilityPerPack = defaults.O11HighAvailabilityPerPack;
            outSystemsEntity.O11SentryPerPack = defaults.O11SentryPerPack;
            outSystemsEntity.O11NonProdEnvPerPack = defaults.O11NonProdEnvPerPack;
            outSystemsEntity.O11LoadTestEnvPerPack = defaults.O11LoadTestEnvPerPack;
            outSystemsEntity.O11EnvironmentPackPerPack = defaults.O11EnvironmentPackPerPack;
            outSystemsEntity.O11DisasterRecoveryPerPack = defaults.O11DisasterRecoveryPerPack;
            outSystemsEntity.O11LogStreamingFlat = defaults.O11LogStreamingFlat;
            outSystemsEntity.O11DatabaseReplicaFlat = defaults.O11DatabaseReplicaFlat;

            // AppShield and Services
            outSystemsEntity.AppShieldTiersJson = defaults.AppShieldTiersJson;
            outSystemsEntity.ServicesPricingByRegionJson = defaults.ServicesPricingByRegionJson;

            // Cloud VM pricing
            outSystemsEntity.AzureVMPricingJson = defaults.AzureVMPricingJson;
            outSystemsEntity.AwsEC2PricingJson = defaults.AwsEC2PricingJson;
            outSystemsEntity.HoursPerMonth = defaults.HoursPerMonth;

            // Pack sizes
            outSystemsEntity.AOPackSize = defaults.AOPackSize;
            outSystemsEntity.InternalUserPackSize = defaults.InternalUserPackSize;
            outSystemsEntity.ExternalUserPackSize = defaults.ExternalUserPackSize;

            // Feature availability
            outSystemsEntity.O11CloudOnlyFeaturesJson = defaults.O11CloudOnlyFeaturesJson;
            outSystemsEntity.O11SelfManagedOnlyFeaturesJson = defaults.O11SelfManagedOnlyFeaturesJson;

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
            // ODC Platform Pricing
            OdcPlatformBasePrice = entity.OdcPlatformBasePrice,
            OdcAOPackPrice = entity.OdcAOPackPrice,
            OdcInternalUserPackPrice = entity.OdcInternalUserPackPrice,
            OdcExternalUserPackPrice = entity.OdcExternalUserPackPrice,

            // O11 Platform Pricing
            O11EnterpriseBasePrice = entity.O11EnterpriseBasePrice,
            O11AOPackPrice = entity.O11AOPackPrice,

            // Unlimited Users (per AO Pack)
            UnlimitedUsersPerAOPack = entity.UnlimitedUsersPerAOPack,

            // ODC Add-ons
            OdcSupport24x7ExtendedPerPack = entity.OdcSupport24x7ExtendedPerPack,
            OdcSupport24x7PremiumPerPack = entity.OdcSupport24x7PremiumPerPack,
            OdcHighAvailabilityPerPack = entity.OdcHighAvailabilityPerPack,
            OdcNonProdRuntimePerPack = entity.OdcNonProdRuntimePerPack,
            OdcPrivateGatewayPerPack = entity.OdcPrivateGatewayPerPack,
            OdcSentryPerPack = entity.OdcSentryPerPack,

            // O11 Add-ons
            O11Support24x7PremiumPerPack = entity.O11Support24x7PremiumPerPack,
            O11HighAvailabilityPerPack = entity.O11HighAvailabilityPerPack,
            O11SentryPerPack = entity.O11SentryPerPack,
            O11NonProdEnvPerPack = entity.O11NonProdEnvPerPack,
            O11LoadTestEnvPerPack = entity.O11LoadTestEnvPerPack,
            O11EnvironmentPackPerPack = entity.O11EnvironmentPackPerPack,
            O11DisasterRecoveryPerPack = entity.O11DisasterRecoveryPerPack,
            O11LogStreamingFlat = entity.O11LogStreamingFlat,
            O11DatabaseReplicaFlat = entity.O11DatabaseReplicaFlat,

            // Pack sizes
            AOPackSize = entity.AOPackSize,
            InternalUserPackSize = entity.InternalUserPackSize,
            ExternalUserPackSize = entity.ExternalUserPackSize,

            // Hours per month for VM calculation
            HoursPerMonth = entity.HoursPerMonth
        };

        // Parse O11 Internal User Tiers from JSON
        if (!string.IsNullOrEmpty(entity.O11InternalUserTiersJson))
        {
            try
            {
                var tiers = JsonSerializer.Deserialize<List<OutSystemsUserTier>>(entity.O11InternalUserTiersJson);
                if (tiers != null)
                    settings.O11InternalUserTiers = tiers;
            }
            catch { /* Use defaults */ }
        }

        // Parse O11 External User Tiers from JSON
        if (!string.IsNullOrEmpty(entity.O11ExternalUserTiersJson))
        {
            try
            {
                var tiers = JsonSerializer.Deserialize<List<OutSystemsUserTier>>(entity.O11ExternalUserTiersJson);
                if (tiers != null)
                    settings.O11ExternalUserTiers = tiers;
            }
            catch { /* Use defaults */ }
        }

        // Parse AppShield Tiers from JSON
        if (!string.IsNullOrEmpty(entity.AppShieldTiersJson))
        {
            try
            {
                var tiers = JsonSerializer.Deserialize<List<OutSystemsAppShieldTier>>(entity.AppShieldTiersJson);
                if (tiers != null)
                    settings.AppShieldTiers = tiers;
            }
            catch { /* Use defaults */ }
        }

        // Parse Services Pricing by Region from JSON
        if (!string.IsNullOrEmpty(entity.ServicesPricingByRegionJson))
        {
            try
            {
                var regionPricing = JsonSerializer.Deserialize<Dictionary<string, OutSystemsRegionServicePricing>>(entity.ServicesPricingByRegionJson);
                if (regionPricing != null)
                {
                    settings.ServicesPricingByRegion.Clear();
                    foreach (var (key, value) in regionPricing)
                    {
                        if (Enum.TryParse<OutSystemsRegion>(key, out var region))
                        {
                            settings.ServicesPricingByRegion[region] = value;
                        }
                    }
                }
            }
            catch { /* Use defaults */ }
        }

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
        // ODC Platform Pricing
        entity.OdcPlatformBasePrice = settings.OdcPlatformBasePrice;
        entity.OdcAOPackPrice = settings.OdcAOPackPrice;
        entity.OdcInternalUserPackPrice = settings.OdcInternalUserPackPrice;
        entity.OdcExternalUserPackPrice = settings.OdcExternalUserPackPrice;

        // O11 Platform Pricing
        entity.O11EnterpriseBasePrice = settings.O11EnterpriseBasePrice;
        entity.O11AOPackPrice = settings.O11AOPackPrice;

        // Unlimited Users (per AO Pack)
        entity.UnlimitedUsersPerAOPack = settings.UnlimitedUsersPerAOPack;

        // ODC Add-ons
        entity.OdcSupport24x7ExtendedPerPack = settings.OdcSupport24x7ExtendedPerPack;
        entity.OdcSupport24x7PremiumPerPack = settings.OdcSupport24x7PremiumPerPack;
        entity.OdcHighAvailabilityPerPack = settings.OdcHighAvailabilityPerPack;
        entity.OdcNonProdRuntimePerPack = settings.OdcNonProdRuntimePerPack;
        entity.OdcPrivateGatewayPerPack = settings.OdcPrivateGatewayPerPack;
        entity.OdcSentryPerPack = settings.OdcSentryPerPack;

        // O11 Add-ons
        entity.O11Support24x7PremiumPerPack = settings.O11Support24x7PremiumPerPack;
        entity.O11HighAvailabilityPerPack = settings.O11HighAvailabilityPerPack;
        entity.O11SentryPerPack = settings.O11SentryPerPack;
        entity.O11NonProdEnvPerPack = settings.O11NonProdEnvPerPack;
        entity.O11LoadTestEnvPerPack = settings.O11LoadTestEnvPerPack;
        entity.O11EnvironmentPackPerPack = settings.O11EnvironmentPackPerPack;
        entity.O11DisasterRecoveryPerPack = settings.O11DisasterRecoveryPerPack;
        entity.O11LogStreamingFlat = settings.O11LogStreamingFlat;
        entity.O11DatabaseReplicaFlat = settings.O11DatabaseReplicaFlat;

        // Pack sizes
        entity.AOPackSize = settings.AOPackSize;
        entity.InternalUserPackSize = settings.InternalUserPackSize;
        entity.ExternalUserPackSize = settings.ExternalUserPackSize;

        // Hours per month
        entity.HoursPerMonth = settings.HoursPerMonth;

        // Serialize O11 Internal User Tiers to JSON
        entity.O11InternalUserTiersJson = JsonSerializer.Serialize(settings.O11InternalUserTiers);

        // Serialize O11 External User Tiers to JSON
        entity.O11ExternalUserTiersJson = JsonSerializer.Serialize(settings.O11ExternalUserTiers);

        // Serialize AppShield Tiers to JSON
        entity.AppShieldTiersJson = JsonSerializer.Serialize(settings.AppShieldTiers);

        // Serialize Services Pricing by Region to JSON
        var regionDict = settings.ServicesPricingByRegion.ToDictionary(
            kvp => kvp.Key.ToString(),
            kvp => kvp.Value);
        entity.ServicesPricingByRegionJson = JsonSerializer.Serialize(regionDict);

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
