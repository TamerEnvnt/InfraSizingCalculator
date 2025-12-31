namespace InfraSizingCalculator.Models.Pricing;

#region Enums

/// <summary>
/// OutSystems edition types
/// </summary>
public enum OutSystemsEdition
{
    Standard,       // Starting tier for small-medium deployments (1 AO pack included)
    Enterprise      // Full-featured for large enterprise deployments (1 AO pack included)
}

/// <summary>
/// OutSystems deployment types
/// </summary>
public enum OutSystemsDeploymentType
{
    Cloud,          // OutSystems Cloud (managed PaaS)
    SelfManaged     // Self-managed on-premises or private cloud
}

/// <summary>
/// Cloud provider for self-managed deployments on cloud infrastructure
/// </summary>
public enum OutSystemsCloudProvider
{
    OnPremises,     // Traditional on-premises data center
    Azure,          // Microsoft Azure
    AWS             // Amazon Web Services
}

/// <summary>
/// Azure VM instance types recommended for OutSystems
/// Based on OutSystems hardware requirements documentation
/// </summary>
public enum OutSystemsAzureInstanceType
{
    F4s_v2,         // 4 vCPU, 8 GB RAM - Default/recommended
    D4s_v3,         // 4 vCPU, 16 GB RAM
    D8s_v3,         // 8 vCPU, 32 GB RAM
    D16s_v3         // 16 vCPU, 64 GB RAM
}

/// <summary>
/// AWS EC2 instance types recommended for OutSystems
/// Based on OutSystems hardware requirements documentation
/// </summary>
public enum OutSystemsAwsInstanceType
{
    M5Large,        // 2 vCPU, 8 GB RAM
    M5XLarge,       // 4 vCPU, 16 GB RAM
    M52XLarge       // 8 vCPU, 32 GB RAM
}

/// <summary>
/// OutSystems user license types
/// </summary>
public enum OutSystemsUserLicenseType
{
    Named,          // Named users - each user has dedicated license
    Concurrent,     // Concurrent users - shared floating licenses
    External,       // External/Anonymous - session-based for public apps (1,000 per pack)
    Unlimited       // Unlimited users - flat fee option
}

/// <summary>
/// OutSystems Success Plan tiers (support and services)
/// </summary>
public enum OutSystemsSuccessPlan
{
    None,           // No additional support
    Essential,      // Essential Success Plan
    Premier         // Premier Success Plan (enhanced support)
}

/// <summary>
/// OutSystems support tier (legacy - for backwards compatibility)
/// </summary>
public enum OutSystemsSupportTier
{
    Standard,       // Included in subscription
    Premium,        // 24/7 with faster SLAs
    Elite           // Dedicated support with custom SLAs
}

#endregion

#region Pricing Settings

/// <summary>
/// Complete OutSystems pricing configuration
/// Based on OutSystems Partner Price Calculator (2024/2025)
/// All prices are annual unless otherwise noted
/// </summary>
public class OutSystemsPricingSettings
{
    // ==================== EDITION PRICING ====================

    /// <summary>
    /// Standard Edition base subscription (annual)
    /// Includes 1 AO pack (150 AOs)
    /// </summary>
    public decimal StandardEditionBasePrice { get; set; } = 36300m;
    public int StandardEditionAOsIncluded { get; set; } = 150;
    public int StandardEditionInternalUsersIncluded { get; set; } = 100;

    /// <summary>
    /// Enterprise Edition base subscription (annual)
    /// Includes 1 AO pack (150 AOs), same base price as Standard
    /// Enterprise adds features, not more included AOs
    /// </summary>
    public decimal EnterpriseEditionBasePrice { get; set; } = 36300m;
    public int EnterpriseEditionAOsIncluded { get; set; } = 150;
    public int EnterpriseEditionInternalUsersIncluded { get; set; } = 500;

    // ==================== APPLICATION OBJECTS ====================

    /// <summary>
    /// AO pack size (150 AOs per pack)
    /// AOs = screens + database tables + API methods
    /// </summary>
    public int AOPackSize { get; set; } = 150;

    /// <summary>
    /// Price per additional AO pack (150 AOs) per year
    /// From Partner Calculator: $36,300 per pack
    /// </summary>
    public decimal AdditionalAOPackPrice { get; set; } = 36300m;

    // ==================== USER LICENSING ====================

    /// <summary>
    /// Internal user pack size (100 users per pack)
    /// </summary>
    public int InternalUserPackSize { get; set; } = 100;

    /// <summary>
    /// Price per additional internal user pack (100 users) per year
    /// </summary>
    public decimal AdditionalInternalUserPackPrice { get; set; } = 6000m;

    /// <summary>
    /// External user pack size (1,000 sessions per pack)
    /// From Partner Calculator: 1,000 pack
    /// </summary>
    public int ExternalUserPackSize { get; set; } = 1000;

    /// <summary>
    /// External user pack price per year
    /// From Partner Calculator: $4,840 per 1,000 pack
    /// </summary>
    public decimal ExternalUserPackPricePerYear { get; set; } = 4840m;

    /// <summary>
    /// Unlimited users option - flat fee per year
    /// From Partner Calculator: $181,500
    /// </summary>
    public decimal UnlimitedUsersPrice { get; set; } = 181500m;

    // ==================== ADD-ONS (AO-Pack Scaled) ====================
    // These add-ons scale with the number of AO packs: cost = rate * aoPackCount

    /// <summary>
    /// 24x7 Premium Support per AO pack per year
    /// From Partner Calculator: $3,630 per pack (total shown: $10,890 for 3 packs)
    /// </summary>
    public decimal Support24x7PremiumPerAOPack { get; set; } = 3630m;

    /// <summary>
    /// Non-Production Environment add-on per AO pack per year
    /// From Partner Calculator: $3,630 per pack
    /// </summary>
    public decimal NonProductionEnvPerAOPack { get; set; } = 3630m;

    /// <summary>
    /// Load Testing Environment add-on per AO pack per year (Cloud only)
    /// From Partner Calculator: $6,050 per pack
    /// </summary>
    public decimal LoadTestEnvPerAOPack { get; set; } = 6050m;

    /// <summary>
    /// Environment Pack add-on per AO pack per year
    /// From Partner Calculator: $9,680 per pack
    /// </summary>
    public decimal EnvironmentPackPerAOPack { get; set; } = 9680m;

    /// <summary>
    /// High Availability add-on per AO pack per year (Cloud only)
    /// From Partner Calculator: $12,100 per pack (total shown: $36,300 for 3 packs)
    /// </summary>
    public decimal HighAvailabilityPerAOPack { get; set; } = 12100m;

    /// <summary>
    /// Sentry add-on per AO pack per year (Cloud only, includes HA)
    /// From Partner Calculator: $24,200 per pack (total shown: $72,600 for 3 packs)
    /// </summary>
    public decimal SentryPerAOPack { get; set; } = 24200m;

    /// <summary>
    /// Disaster Recovery add-on per AO pack per year
    /// From Partner Calculator: $12,100 per pack (total shown: $36,300 for 3 packs)
    /// </summary>
    public decimal DisasterRecoveryPerAOPack { get; set; } = 12100m;

    // ==================== ADD-ONS (Flat Fee) ====================

    /// <summary>
    /// Log Streaming add-on per year (Cloud only)
    /// From Partner Calculator: $7,260
    /// </summary>
    public decimal LogStreamingPrice { get; set; } = 7260m;

    /// <summary>
    /// Database Replica add-on (Cloud only)
    /// From Partner Calculator: $96,800
    /// </summary>
    public decimal DatabaseReplicaPrice { get; set; } = 96800m;

    /// <summary>
    /// AppShield per user per year
    /// Estimated: ~$16.50 per user
    /// </summary>
    public decimal AppShieldPerUser { get; set; } = 16.50m;

    // ==================== SERVICES ====================

    /// <summary>
    /// Essential Success Plan per year
    /// From Partner Calculator: $30,250
    /// </summary>
    public decimal EssentialSuccessPlanPrice { get; set; } = 30250m;

    /// <summary>
    /// Premier Success Plan per year
    /// From Partner Calculator: $60,500
    /// </summary>
    public decimal PremierSuccessPlanPrice { get; set; } = 60500m;

    /// <summary>
    /// Dedicated Group Session (training)
    /// From Partner Calculator: $3,820
    /// </summary>
    public decimal DedicatedGroupSessionPrice { get; set; } = 3820m;

    /// <summary>
    /// Public Session (training)
    /// From Partner Calculator: $720
    /// </summary>
    public decimal PublicSessionPrice { get; set; } = 720m;

    /// <summary>
    /// Expert Day (consulting)
    /// From Partner Calculator: $2,640
    /// </summary>
    public decimal ExpertDayPrice { get; set; } = 2640m;

    // ==================== LEGACY SELF-MANAGED (for backward compatibility) ====================

    public decimal SelfManagedBasePrice { get; set; } = 48000m;
    public decimal SelfManagedPerEnvironmentPrice { get; set; } = 9600m;
    public decimal SelfManagedPerFrontEndPrice { get; set; } = 4800m;

    // ==================== LEGACY SUPPORT (for backward compatibility) ====================

    public decimal PremiumSupportPercent { get; set; } = 15m;
    public decimal EliteSupportPercent { get; set; } = 25m;

    // ==================== CLOUD VM PRICING (Hourly Rates) ====================

    /// <summary>
    /// Azure VM hourly pricing (East US region, Pay-as-you-go)
    /// </summary>
    public Dictionary<OutSystemsAzureInstanceType, decimal> AzureVMHourlyPricing { get; set; } = new()
    {
        { OutSystemsAzureInstanceType.F4s_v2, 0.169m },   // 4 vCPU, 8 GB
        { OutSystemsAzureInstanceType.D4s_v3, 0.192m },   // 4 vCPU, 16 GB
        { OutSystemsAzureInstanceType.D8s_v3, 0.384m },   // 8 vCPU, 32 GB
        { OutSystemsAzureInstanceType.D16s_v3, 0.768m }   // 16 vCPU, 64 GB
    };

    /// <summary>
    /// AWS EC2 hourly pricing (us-east-1, On-Demand)
    /// </summary>
    public Dictionary<OutSystemsAwsInstanceType, decimal> AwsEC2HourlyPricing { get; set; } = new()
    {
        { OutSystemsAwsInstanceType.M5Large, 0.096m },    // 2 vCPU, 8 GB
        { OutSystemsAwsInstanceType.M5XLarge, 0.192m },   // 4 vCPU, 16 GB
        { OutSystemsAwsInstanceType.M52XLarge, 0.384m }   // 8 vCPU, 32 GB
    };

    /// <summary>
    /// Hours per month for VM cost calculation (730 hours = 365 days * 24 hours / 12 months)
    /// </summary>
    public int HoursPerMonth { get; set; } = 730;

    // ==================== CLOUD-ONLY FEATURES ====================

    /// <summary>
    /// Features that are only available in OutSystems Cloud deployments
    /// </summary>
    public static readonly HashSet<string> CloudOnlyFeatures = new()
    {
        "HighAvailability",
        "Sentry",
        "LoadTestEnv",
        "LogStreaming",
        "DatabaseReplica"
    };

    /// <summary>
    /// Check if a feature is cloud-only
    /// </summary>
    public static bool IsCloudOnlyFeature(string featureName) => CloudOnlyFeatures.Contains(featureName);

    // ==================== HELPER METHODS ====================

    /// <summary>
    /// Get edition base price
    /// </summary>
    public decimal GetEditionBasePrice(OutSystemsEdition edition) => edition switch
    {
        OutSystemsEdition.Standard => StandardEditionBasePrice,
        OutSystemsEdition.Enterprise => EnterpriseEditionBasePrice,
        _ => StandardEditionBasePrice
    };

    /// <summary>
    /// Get included AOs for edition (1 pack = 150 AOs for both editions)
    /// </summary>
    public int GetIncludedAOs(OutSystemsEdition edition) => edition switch
    {
        OutSystemsEdition.Standard => StandardEditionAOsIncluded,
        OutSystemsEdition.Enterprise => EnterpriseEditionAOsIncluded,
        _ => StandardEditionAOsIncluded
    };

    /// <summary>
    /// Get included internal users for edition
    /// </summary>
    public int GetIncludedInternalUsers(OutSystemsEdition edition) => edition switch
    {
        OutSystemsEdition.Standard => StandardEditionInternalUsersIncluded,
        OutSystemsEdition.Enterprise => EnterpriseEditionInternalUsersIncluded,
        _ => StandardEditionInternalUsersIncluded
    };

    /// <summary>
    /// Calculate number of AO packs needed
    /// </summary>
    public int CalculateAOPackCount(int totalAOs)
    {
        if (totalAOs <= 0) return 1; // Minimum 1 pack
        return (int)Math.Ceiling(totalAOs / (double)AOPackSize);
    }

    /// <summary>
    /// Calculate cost for additional AOs beyond what's included in edition
    /// </summary>
    public decimal CalculateAdditionalAOsCost(OutSystemsEdition edition, int totalAOs)
    {
        var included = GetIncludedAOs(edition);
        if (totalAOs <= included) return 0;

        var additional = totalAOs - included;
        var packs = (int)Math.Ceiling(additional / (double)AOPackSize);
        return packs * AdditionalAOPackPrice;
    }

    /// <summary>
    /// Calculate AO-pack scaled add-on cost
    /// Formula: perPackRate * aoPackCount
    /// </summary>
    public decimal CalculateAOPackScaledCost(decimal perPackRate, int totalAOs)
    {
        var packCount = CalculateAOPackCount(totalAOs);
        return perPackRate * packCount;
    }

    /// <summary>
    /// Calculate external user pack cost
    /// </summary>
    public decimal CalculateExternalUsersCost(int userCount)
    {
        if (userCount <= 0) return 0;
        var packs = (int)Math.Ceiling(userCount / (double)ExternalUserPackSize);
        return packs * ExternalUserPackPricePerYear;
    }

    /// <summary>
    /// Calculate monthly VM cost for Azure
    /// </summary>
    public decimal CalculateAzureMonthlyVMCost(OutSystemsAzureInstanceType instanceType, int serverCount)
    {
        if (!AzureVMHourlyPricing.TryGetValue(instanceType, out var hourlyRate))
            return 0;
        return hourlyRate * HoursPerMonth * serverCount;
    }

    /// <summary>
    /// Calculate monthly VM cost for AWS
    /// </summary>
    public decimal CalculateAwsMonthlyVMCost(OutSystemsAwsInstanceType instanceType, int serverCount)
    {
        if (!AwsEC2HourlyPricing.TryGetValue(instanceType, out var hourlyRate))
            return 0;
        return hourlyRate * HoursPerMonth * serverCount;
    }

    /// <summary>
    /// Calculate Success Plan cost
    /// </summary>
    public decimal CalculateSuccessPlanCost(OutSystemsSuccessPlan plan) => plan switch
    {
        OutSystemsSuccessPlan.Essential => EssentialSuccessPlanPrice,
        OutSystemsSuccessPlan.Premier => PremierSuccessPlanPrice,
        _ => 0
    };

    /// <summary>
    /// Get Azure instance specs
    /// </summary>
    public static (int vCPU, int RamGB) GetAzureInstanceSpecs(OutSystemsAzureInstanceType instanceType) => instanceType switch
    {
        OutSystemsAzureInstanceType.F4s_v2 => (4, 8),
        OutSystemsAzureInstanceType.D4s_v3 => (4, 16),
        OutSystemsAzureInstanceType.D8s_v3 => (8, 32),
        OutSystemsAzureInstanceType.D16s_v3 => (16, 64),
        _ => (4, 8)
    };

    /// <summary>
    /// Get AWS instance specs
    /// </summary>
    public static (int vCPU, int RamGB) GetAwsInstanceSpecs(OutSystemsAwsInstanceType instanceType) => instanceType switch
    {
        OutSystemsAwsInstanceType.M5Large => (2, 8),
        OutSystemsAwsInstanceType.M5XLarge => (4, 16),
        OutSystemsAwsInstanceType.M52XLarge => (8, 32),
        _ => (2, 8)
    };
}

#endregion

#region Deployment Configuration

/// <summary>
/// OutSystems deployment configuration selected by user
/// </summary>
public class OutSystemsDeploymentConfig
{
    // ==================== BASIC CONFIGURATION ====================

    public OutSystemsEdition Edition { get; set; } = OutSystemsEdition.Standard;
    public OutSystemsDeploymentType DeploymentType { get; set; } = OutSystemsDeploymentType.Cloud;

    // ==================== APPLICATION OBJECTS ====================

    /// <summary>
    /// Total Application Objects (AOs = screens + tables + API methods)
    /// </summary>
    public int TotalApplicationObjects { get; set; } = 150;

    /// <summary>
    /// Computed number of AO packs (read-only, calculated from TotalApplicationObjects)
    /// </summary>
    public int NumberOfAOPacks => (int)Math.Ceiling(TotalApplicationObjects / 150.0);

    // ==================== SELF-MANAGED CLOUD PROVIDER ====================

    /// <summary>
    /// Cloud provider for self-managed deployments
    /// </summary>
    public OutSystemsCloudProvider CloudProvider { get; set; } = OutSystemsCloudProvider.OnPremises;

    /// <summary>
    /// Azure instance type (when CloudProvider is Azure)
    /// </summary>
    public OutSystemsAzureInstanceType AzureInstanceType { get; set; } = OutSystemsAzureInstanceType.F4s_v2;

    /// <summary>
    /// AWS instance type (when CloudProvider is AWS)
    /// </summary>
    public OutSystemsAwsInstanceType AwsInstanceType { get; set; } = OutSystemsAwsInstanceType.M5XLarge;

    /// <summary>
    /// Number of front-end servers per environment (for self-managed)
    /// </summary>
    public int FrontEndServersPerEnvironment { get; set; } = 2;

    // ==================== ENVIRONMENTS ====================

    /// <summary>
    /// Number of production environments
    /// </summary>
    public int ProductionEnvironments { get; set; } = 1;

    /// <summary>
    /// Number of non-production environments (Dev, Test, Stage, etc.)
    /// </summary>
    public int NonProductionEnvironments { get; set; } = 3;

    /// <summary>
    /// Total environments for VM count calculation
    /// </summary>
    public int TotalEnvironments => ProductionEnvironments + NonProductionEnvironments;

    // ==================== USER LICENSING ====================

    public OutSystemsUserLicenseType UserLicenseType { get; set; } = OutSystemsUserLicenseType.Named;

    /// <summary>
    /// Number of named/internal users
    /// </summary>
    public int InternalUsers { get; set; } = 100;

    /// <summary>
    /// Number of external users (for External license type)
    /// </summary>
    public int ExternalUsers { get; set; } = 0;

    /// <summary>
    /// Use unlimited users option
    /// </summary>
    public bool UseUnlimitedUsers { get; set; } = false;

    // ==================== ADD-ONS ====================

    /// <summary>
    /// Include 24x7 Premium Support add-on
    /// </summary>
    public bool Include24x7PremiumSupport { get; set; } = false;

    /// <summary>
    /// Include additional Non-Production Environment
    /// </summary>
    public bool IncludeNonProductionEnv { get; set; } = false;

    /// <summary>
    /// Include Load Testing Environment (Cloud only)
    /// </summary>
    public bool IncludeLoadTestEnv { get; set; } = false;

    /// <summary>
    /// Include Environment Pack add-on
    /// </summary>
    public bool IncludeEnvironmentPack { get; set; } = false;

    /// <summary>
    /// Include High Availability (Cloud only)
    /// </summary>
    public bool IncludeHA { get; set; } = false;

    /// <summary>
    /// Include Sentry (Cloud only, includes HA)
    /// </summary>
    public bool IncludeSentry { get; set; } = false;

    /// <summary>
    /// Include Disaster Recovery
    /// </summary>
    public bool IncludeDR { get; set; } = false;

    /// <summary>
    /// Include Log Streaming (Cloud only)
    /// </summary>
    public bool IncludeLogStreaming { get; set; } = false;

    /// <summary>
    /// Include Database Replica (Cloud only)
    /// </summary>
    public bool IncludeDatabaseReplica { get; set; } = false;

    /// <summary>
    /// Include AppShield (number of users)
    /// </summary>
    public int AppShieldUsers { get; set; } = 0;

    // ==================== SERVICES ====================

    /// <summary>
    /// Success Plan selection
    /// </summary>
    public OutSystemsSuccessPlan SuccessPlan { get; set; } = OutSystemsSuccessPlan.None;

    /// <summary>
    /// Number of Dedicated Group Sessions (training)
    /// </summary>
    public int DedicatedGroupSessions { get; set; } = 0;

    /// <summary>
    /// Number of Public Sessions (training)
    /// </summary>
    public int PublicSessions { get; set; } = 0;

    /// <summary>
    /// Number of Expert Days (consulting)
    /// </summary>
    public int ExpertDays { get; set; } = 0;

    // ==================== LEGACY (for backward compatibility) ====================

    [Obsolete("Use InternalUsers instead")]
    public int NamedUsers { get => InternalUsers; set => InternalUsers = value; }

    [Obsolete("Use InternalUsers instead")]
    public int ConcurrentUsers { get; set; } = 0;

    [Obsolete("Use ExternalUsers instead")]
    public int ExternalSessions { get => ExternalUsers; set => ExternalUsers = value; }

    [Obsolete("Use SuccessPlan instead")]
    public OutSystemsSupportTier SupportTier { get; set; } = OutSystemsSupportTier.Standard;

    [Obsolete("Use FrontEndServersPerEnvironment instead")]
    public int FrontEndServers { get => FrontEndServersPerEnvironment; set => FrontEndServersPerEnvironment = value; }

    // ==================== VALIDATION ====================

    /// <summary>
    /// Validate configuration and return warnings for cloud-only features in self-managed
    /// </summary>
    public List<string> GetValidationWarnings()
    {
        var warnings = new List<string>();

        if (DeploymentType == OutSystemsDeploymentType.SelfManaged)
        {
            if (IncludeHA)
                warnings.Add("High Availability is a Cloud-only feature. It will be ignored for self-managed deployments.");
            if (IncludeSentry)
                warnings.Add("Sentry is a Cloud-only feature. It will be ignored for self-managed deployments.");
            if (IncludeLoadTestEnv)
                warnings.Add("Load Testing Environment is a Cloud-only feature. It will be ignored for self-managed deployments.");
            if (IncludeLogStreaming)
                warnings.Add("Log Streaming is a Cloud-only feature. It will be ignored for self-managed deployments.");
            if (IncludeDatabaseReplica)
                warnings.Add("Database Replica is a Cloud-only feature. It will be ignored for self-managed deployments.");
        }

        if (IncludeSentry && IncludeHA)
            warnings.Add("Sentry already includes High Availability. HA add-on will be ignored.");

        return warnings;
    }
}

#endregion

#region Pricing Result

/// <summary>
/// Calculated OutSystems pricing result with detailed breakdown
/// </summary>
public class OutSystemsPricingResult
{
    // ==================== CONFIGURATION INFO ====================

    public OutSystemsEdition Edition { get; set; }
    public OutSystemsDeploymentType DeploymentType { get; set; }
    public string DeploymentTypeName { get; set; } = string.Empty;
    public OutSystemsCloudProvider CloudProvider { get; set; }

    // ==================== AO DETAILS ====================

    public int TotalAOs { get; set; }
    public int AOPackCount { get; set; }
    public int IncludedAOs { get; set; }
    public int AdditionalAOPacks { get; set; }

    // ==================== LICENSE COSTS ====================

    public decimal EditionBaseCost { get; set; }
    public decimal AdditionalAOsCost { get; set; }
    public decimal UserLicenseCost { get; set; }
    public string? UserLicenseDetails { get; set; }

    public decimal LicenseSubtotal => EditionBaseCost + AdditionalAOsCost + UserLicenseCost;

    // ==================== ADD-ON COSTS ====================

    public decimal Support24x7PremiumCost { get; set; }
    public decimal NonProductionEnvCost { get; set; }
    public decimal LoadTestEnvCost { get; set; }
    public decimal EnvironmentPackCost { get; set; }
    public decimal HACost { get; set; }
    public decimal SentryCost { get; set; }
    public decimal DRCost { get; set; }
    public decimal LogStreamingCost { get; set; }
    public decimal DatabaseReplicaCost { get; set; }
    public decimal AppShieldCost { get; set; }

    public decimal AddOnsSubtotal =>
        Support24x7PremiumCost + NonProductionEnvCost + LoadTestEnvCost +
        EnvironmentPackCost + HACost + SentryCost + DRCost +
        LogStreamingCost + DatabaseReplicaCost + AppShieldCost;

    // ==================== INFRASTRUCTURE COSTS (Self-Managed on Cloud) ====================

    public decimal MonthlyVMCost { get; set; }
    public decimal AnnualVMCost => MonthlyVMCost * 12;
    public int TotalVMCount { get; set; }
    public string? VMDetails { get; set; }

    public decimal InfrastructureSubtotal => AnnualVMCost;

    // ==================== SERVICES COSTS ====================

    public decimal SuccessPlanCost { get; set; }
    public decimal TrainingCost { get; set; }
    public decimal ExpertDaysCost { get; set; }

    public decimal ServicesSubtotal => SuccessPlanCost + TrainingCost + ExpertDaysCost;

    // ==================== LEGACY COSTS (for backward compatibility) ====================

    [Obsolete("Use LicenseSubtotal + AddOnsSubtotal instead")]
    public decimal EnvironmentCost { get; set; }
    [Obsolete("Use InfrastructureSubtotal instead")]
    public decimal FrontEndCost { get; set; }
    [Obsolete("Use Support24x7PremiumCost instead")]
    public decimal SupportCost { get; set; }

    // ==================== TOTALS ====================

    public decimal TotalPerYear => LicenseSubtotal + AddOnsSubtotal + InfrastructureSubtotal + ServicesSubtotal;
    public decimal TotalPerMonth => TotalPerYear / 12;
    public decimal TotalThreeYear => TotalPerYear * 3;
    public decimal TotalFiveYear => TotalPerYear * 5;

    // ==================== METADATA ====================

    public string? EnvironmentDetails { get; set; }
    public List<string> Warnings { get; set; } = new();
    public List<OutSystemsCostLineItem> LineItems { get; set; } = new();
}

/// <summary>
/// Individual cost line item for detailed breakdown display
/// </summary>
public class OutSystemsCostLineItem
{
    public string Category { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public bool IsCloudOnly { get; set; }
    public bool IsIncluded { get; set; }
}

#endregion
