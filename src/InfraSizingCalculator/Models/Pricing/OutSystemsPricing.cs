namespace InfraSizingCalculator.Models.Pricing;

#region Enums

/// <summary>
/// OutSystems platform type (ODC vs O11)
/// </summary>
public enum OutSystemsPlatform
{
    /// <summary>OutSystems Developer Cloud - Cloud-native, Kubernetes-based, fully managed</summary>
    ODC,
    /// <summary>OutSystems 11 - Traditional .NET platform</summary>
    O11
}

/// <summary>
/// OutSystems O11 deployment type (Cloud vs Self-Managed)
/// Only applicable for O11 platform
/// </summary>
public enum OutSystemsDeployment
{
    /// <summary>OutSystems managed cloud infrastructure</summary>
    Cloud,
    /// <summary>Customer-managed infrastructure (on-premises or private cloud)</summary>
    SelfManaged
}

/// <summary>
/// Service pricing region - affects Bootcamps and Expert Days pricing
/// </summary>
public enum OutSystemsRegion
{
    Africa,
    MiddleEast,
    Americas,
    Europe,
    AsiaPacific
}

/// <summary>
/// Discount type for manual discounts
/// </summary>
public enum OutSystemsDiscountType
{
    /// <summary>Percentage discount (0-100)</summary>
    Percentage,
    /// <summary>Fixed dollar amount discount</summary>
    FixedAmount
}

/// <summary>
/// Discount scope - which part of the quote the discount applies to
/// </summary>
public enum OutSystemsDiscountScope
{
    /// <summary>Apply to entire quote (License + Add-Ons + Services)</summary>
    Total,
    /// <summary>Apply only to License costs (Edition + AOs + Users)</summary>
    LicenseOnly,
    /// <summary>Apply only to Add-Ons costs</summary>
    AddOnsOnly,
    /// <summary>Apply only to Services costs</summary>
    ServicesOnly
}

/// <summary>
/// Cloud provider for self-managed deployments on cloud infrastructure
/// </summary>
public enum OutSystemsCloudProvider
{
    OnPremises,
    Azure,
    AWS
}

/// <summary>
/// Azure VM instance types for OutSystems self-managed
/// </summary>
public enum OutSystemsAzureInstanceType
{
    F4s_v2,     // 4 vCPU, 8 GB RAM - Default
    D4s_v3,     // 4 vCPU, 16 GB RAM
    D8s_v3,     // 8 vCPU, 32 GB RAM
    D16s_v3     // 16 vCPU, 64 GB RAM
}

/// <summary>
/// AWS EC2 instance types for OutSystems self-managed
/// </summary>
public enum OutSystemsAwsInstanceType
{
    M5Large,    // 2 vCPU, 8 GB RAM
    M5XLarge,   // 4 vCPU, 16 GB RAM
    M52XLarge   // 8 vCPU, 32 GB RAM
}

// ==================== BACKWARD COMPATIBILITY ====================
// These enums exist for backward compatibility with existing UI components
// They will be removed in Phase 5 when the UI is updated to use the new model

/// <summary>
/// DEPRECATED: Use OutSystemsPlatform + OutSystemsDeployment instead
/// Maintained for backward compatibility with existing UI
/// </summary>
[Obsolete("Use OutSystemsPlatform + OutSystemsDeployment instead")]
public enum OutSystemsDeploymentType
{
    Cloud,
    SelfManaged
}

/// <summary>
/// DEPRECATED: Use OutSystemsPlatform instead
/// The new model uses Platform (ODC vs O11) rather than Standard vs Enterprise
/// Maintained for backward compatibility with existing UI
/// </summary>
[Obsolete("Use OutSystemsPlatform instead. ODC and O11 have different pricing models.")]
public enum OutSystemsEdition
{
    Standard,
    Enterprise
}

#endregion

#region Discount

/// <summary>
/// Manual discount configuration
/// </summary>
public class OutSystemsDiscount
{
    /// <summary>Type of discount (Percentage or Fixed Amount)</summary>
    public OutSystemsDiscountType Type { get; set; } = OutSystemsDiscountType.Percentage;

    /// <summary>Scope of discount (Total, License, Add-Ons, or Services)</summary>
    public OutSystemsDiscountScope Scope { get; set; } = OutSystemsDiscountScope.Total;

    /// <summary>Discount value (percentage 0-100 or fixed dollar amount)</summary>
    public decimal Value { get; set; }

    /// <summary>Optional notes about the discount (e.g., "Partner discount")</summary>
    public string? Notes { get; set; }

    /// <summary>Calculate discount amount for given subtotals</summary>
    public decimal CalculateDiscount(decimal licenseSubtotal, decimal addOnsSubtotal, decimal servicesSubtotal)
    {
        decimal applicableAmount = Scope switch
        {
            OutSystemsDiscountScope.Total => licenseSubtotal + addOnsSubtotal + servicesSubtotal,
            OutSystemsDiscountScope.LicenseOnly => licenseSubtotal,
            OutSystemsDiscountScope.AddOnsOnly => addOnsSubtotal,
            OutSystemsDiscountScope.ServicesOnly => servicesSubtotal,
            _ => licenseSubtotal + addOnsSubtotal + servicesSubtotal
        };

        return Type switch
        {
            OutSystemsDiscountType.Percentage => applicableAmount * (Value / 100m),
            OutSystemsDiscountType.FixedAmount => Math.Min(Value, applicableAmount),
            _ => 0
        };
    }
}

#endregion

#region Pricing Settings

/// <summary>
/// Complete OutSystems pricing configuration
/// Verified against OutSystems Partner Calculator (January 2026)
/// All prices are annual unless otherwise noted
/// </summary>
public class OutSystemsPricingSettings
{
    // ==================== ODC PLATFORM PRICING ====================

    /// <summary>
    /// ODC Platform base price (annual)
    /// Includes: 150 AOs (1 pack), 100 Internal Users, 2 runtimes (Dev, Prod), 8x5 Support
    /// Verified: $30,250
    /// </summary>
    public decimal OdcPlatformBasePrice { get; set; } = 30250m;

    /// <summary>
    /// ODC Additional AO pack price (150 AOs per pack)
    /// Verified: $18,150 per pack
    /// </summary>
    public decimal OdcAOPackPrice { get; set; } = 18150m;

    /// <summary>
    /// ODC Internal User pack price (100 users per pack)
    /// First pack (100 users) included in base
    /// Verified: $6,050 per pack
    /// </summary>
    public decimal OdcInternalUserPackPrice { get; set; } = 6050m;

    /// <summary>
    /// ODC External User pack price (1000 users per pack)
    /// No packs included in base
    /// Verified: $6,050 per pack
    /// </summary>
    public decimal OdcExternalUserPackPrice { get; set; } = 6050m;

    // ==================== O11 PLATFORM PRICING ====================

    /// <summary>
    /// O11 Enterprise Edition base price (annual)
    /// Includes: 150 AOs (1 pack), 100 Internal Users, 3 environments (Dev, Non-Prod, Prod), 24x7 Support
    /// Verified: $36,300
    /// </summary>
    public decimal O11EnterpriseBasePrice { get; set; } = 36300m;

    /// <summary>
    /// O11 Additional AO pack price (150 AOs per pack)
    /// Verified: $36,300 per pack
    /// </summary>
    public decimal O11AOPackPrice { get; set; } = 36300m;

    // ==================== UNLIMITED USERS (Both Platforms) ====================

    /// <summary>
    /// Unlimited Users price PER AO PACK (not flat fee!)
    /// Formula: $60,500 × Number of AO Packs
    /// Verified from tooltip: "$60,500.00 per pack of AOs"
    /// </summary>
    public decimal UnlimitedUsersPerAOPack { get; set; } = 60500m;

    // ==================== O11 TIERED USER PRICING ====================

    /// <summary>
    /// O11 Internal User tiered pricing (per 100 users)
    /// First 100 included in Enterprise Edition
    /// </summary>
    public List<OutSystemsUserTier> O11InternalUserTiers { get; set; } = new()
    {
        new() { MinUsers = 200, MaxUsers = 1000, PricePerPack = 12100m, PackSize = 100 },
        new() { MinUsers = 1100, MaxUsers = 10000, PricePerPack = 2420m, PackSize = 100 },
        new() { MinUsers = 10100, MaxUsers = 100000000, PricePerPack = 242m, PackSize = 100 }
    };

    /// <summary>
    /// O11 External User tiered pricing (per 1000 users)
    /// No users included in Enterprise Edition
    /// </summary>
    public List<OutSystemsUserTier> O11ExternalUserTiers { get; set; } = new()
    {
        new() { MinUsers = 1000, MaxUsers = 10000, PricePerPack = 4840m, PackSize = 1000 },
        new() { MinUsers = 11000, MaxUsers = 250000, PricePerPack = 1452m, PackSize = 1000 },
        new() { MinUsers = 251000, MaxUsers = 100000000, PricePerPack = 30.25m, PackSize = 1000 }
    };

    // ==================== ODC ADD-ONS (Per AO Pack) ====================

    /// <summary>ODC Support 24x7 Extended: $6,050 per pack of AOs</summary>
    public decimal OdcSupport24x7ExtendedPerPack { get; set; } = 6050m;

    /// <summary>ODC Support 24x7 Premium: $9,680 per pack of AOs</summary>
    public decimal OdcSupport24x7PremiumPerPack { get; set; } = 9680m;

    /// <summary>ODC High Availability: $18,150 per pack of AOs</summary>
    public decimal OdcHighAvailabilityPerPack { get; set; } = 18150m;

    /// <summary>ODC Non-Production Runtime: $6,050 per pack of AOs (supports quantity)</summary>
    public decimal OdcNonProdRuntimePerPack { get; set; } = 6050m;

    /// <summary>ODC Private Gateway: $1,210 per pack of AOs</summary>
    public decimal OdcPrivateGatewayPerPack { get; set; } = 1210m;

    /// <summary>ODC Sentry: $6,050 per pack of AOs</summary>
    public decimal OdcSentryPerPack { get; set; } = 6050m;

    // ==================== O11 ADD-ONS (Per AO Pack) ====================

    /// <summary>O11 Support 24x7 Premium: $3,630 per pack of AOs</summary>
    public decimal O11Support24x7PremiumPerPack { get; set; } = 3630m;

    /// <summary>O11 High Availability: $12,100 per pack of AOs (Cloud only)</summary>
    public decimal O11HighAvailabilityPerPack { get; set; } = 12100m;

    /// <summary>O11 Sentry (includes HA): $24,200 per pack of AOs (Cloud only)</summary>
    public decimal O11SentryPerPack { get; set; } = 24200m;

    /// <summary>O11 Non-Production Environment: $3,630 per pack of AOs (supports quantity)</summary>
    public decimal O11NonProdEnvPerPack { get; set; } = 3630m;

    /// <summary>O11 Load Test Environment: $6,050 per pack of AOs (Cloud only)</summary>
    public decimal O11LoadTestEnvPerPack { get; set; } = 6050m;

    /// <summary>O11 Environment Pack: $9,680 per pack of AOs (supports quantity)</summary>
    public decimal O11EnvironmentPackPerPack { get; set; } = 9680m;

    /// <summary>O11 Disaster Recovery: $12,100 per pack of AOs (Self-Managed only)</summary>
    public decimal O11DisasterRecoveryPerPack { get; set; } = 12100m;

    // ==================== O11 ADD-ONS (Flat Fee) ====================

    /// <summary>O11 Log Streaming: $7,260 flat (Cloud only)</summary>
    public decimal O11LogStreamingFlat { get; set; } = 7260m;

    /// <summary>O11 Database Replica: $96,800 flat (Cloud only)</summary>
    public decimal O11DatabaseReplicaFlat { get; set; } = 96800m;

    // ==================== APPSHIELD TIERED PRICING (Both Platforms) ====================

    /// <summary>
    /// AppShield tiered pricing (flat price per tier, not per-user)
    /// 19 tiers from 0-15M users
    /// </summary>
    public List<OutSystemsAppShieldTier> AppShieldTiers { get; set; } = new()
    {
        new() { Tier = 1, MinUsers = 0, MaxUsers = 10000, Price = 18150m },
        new() { Tier = 2, MinUsers = 10001, MaxUsers = 50000, Price = 32670m },
        new() { Tier = 3, MinUsers = 50001, MaxUsers = 100000, Price = 54450m },
        new() { Tier = 4, MinUsers = 100001, MaxUsers = 500000, Price = 96800m },
        new() { Tier = 5, MinUsers = 500001, MaxUsers = 1000000, Price = 234740m },
        new() { Tier = 6, MinUsers = 1000001, MaxUsers = 2000000, Price = 275880m },
        new() { Tier = 7, MinUsers = 2000001, MaxUsers = 3000000, Price = 358160m },
        new() { Tier = 8, MinUsers = 3000001, MaxUsers = 4000000, Price = 411400m },
        new() { Tier = 9, MinUsers = 4000001, MaxUsers = 5000000, Price = 508200m },
        new() { Tier = 10, MinUsers = 5000001, MaxUsers = 6000000, Price = 605000m },
        new() { Tier = 11, MinUsers = 6000001, MaxUsers = 7000000, Price = 701800m },
        new() { Tier = 12, MinUsers = 7000001, MaxUsers = 8000000, Price = 798600m },
        new() { Tier = 13, MinUsers = 8000001, MaxUsers = 9000000, Price = 895400m },
        new() { Tier = 14, MinUsers = 9000001, MaxUsers = 10000000, Price = 992200m },
        new() { Tier = 15, MinUsers = 10000001, MaxUsers = 11000000, Price = 1089000m },
        new() { Tier = 16, MinUsers = 11000001, MaxUsers = 12000000, Price = 1185800m },
        new() { Tier = 17, MinUsers = 12000001, MaxUsers = 13000000, Price = 1282600m },
        new() { Tier = 18, MinUsers = 13000001, MaxUsers = 14000000, Price = 1379400m },
        new() { Tier = 19, MinUsers = 14000001, MaxUsers = 15000000, Price = 1476200m }
    };

    // ==================== SERVICES PRICING BY REGION ====================

    /// <summary>
    /// Services pricing by region
    /// Success Plans are same across regions, Bootcamps and Expert Days vary
    /// </summary>
    public Dictionary<OutSystemsRegion, OutSystemsRegionServicePricing> ServicesPricingByRegion { get; set; } = new()
    {
        {
            OutSystemsRegion.Africa, new OutSystemsRegionServicePricing
            {
                EssentialSuccessPlan = 30250m,
                PremierSuccessPlan = 60500m,
                DedicatedGroupSession = 2670m,
                PublicSession = 480m,
                ExpertDay = 1400m
            }
        },
        {
            OutSystemsRegion.MiddleEast, new OutSystemsRegionServicePricing
            {
                EssentialSuccessPlan = 30250m,
                PremierSuccessPlan = 60500m,
                DedicatedGroupSession = 3820m,
                PublicSession = 720m,
                ExpertDay = 2130m
            }
        },
        {
            OutSystemsRegion.Americas, new OutSystemsRegionServicePricing
            {
                EssentialSuccessPlan = 30250m,
                PremierSuccessPlan = 60500m,
                DedicatedGroupSession = 2670m, // TBD - using Africa as default
                PublicSession = 480m,
                ExpertDay = 1400m
            }
        },
        {
            OutSystemsRegion.Europe, new OutSystemsRegionServicePricing
            {
                EssentialSuccessPlan = 30250m,
                PremierSuccessPlan = 60500m,
                DedicatedGroupSession = 2670m, // TBD - using Africa as default
                PublicSession = 480m,
                ExpertDay = 1400m
            }
        },
        {
            OutSystemsRegion.AsiaPacific, new OutSystemsRegionServicePricing
            {
                EssentialSuccessPlan = 30250m,
                PremierSuccessPlan = 60500m,
                DedicatedGroupSession = 2670m, // TBD - using Africa as default
                PublicSession = 480m,
                ExpertDay = 1400m
            }
        }
    };

    // ==================== CLOUD VM PRICING (Self-Managed) ====================

    public Dictionary<OutSystemsAzureInstanceType, decimal> AzureVMHourlyPricing { get; set; } = new()
    {
        { OutSystemsAzureInstanceType.F4s_v2, 0.169m },
        { OutSystemsAzureInstanceType.D4s_v3, 0.192m },
        { OutSystemsAzureInstanceType.D8s_v3, 0.384m },
        { OutSystemsAzureInstanceType.D16s_v3, 0.768m }
    };

    public Dictionary<OutSystemsAwsInstanceType, decimal> AwsEC2HourlyPricing { get; set; } = new()
    {
        { OutSystemsAwsInstanceType.M5Large, 0.096m },
        { OutSystemsAwsInstanceType.M5XLarge, 0.192m },
        { OutSystemsAwsInstanceType.M52XLarge, 0.384m }
    };

    public int HoursPerMonth { get; set; } = 730;

    // ==================== PACK SIZES ====================

    public int AOPackSize { get; set; } = 150;
    public int InternalUserPackSize { get; set; } = 100;
    public int ExternalUserPackSize { get; set; } = 1000;

    // ==================== HELPER METHODS ====================

    /// <summary>Calculate number of AO packs for given total AOs</summary>
    public int CalculateAOPacks(int totalAOs) => Math.Max(1, (int)Math.Ceiling(totalAOs / (double)AOPackSize));

    /// <summary>Get AppShield price for given user volume</summary>
    public decimal GetAppShieldPrice(int userVolume)
    {
        var tier = AppShieldTiers.FirstOrDefault(t => userVolume >= t.MinUsers && userVolume <= t.MaxUsers);
        return tier?.Price ?? AppShieldTiers.Last().Price;
    }

    /// <summary>Get services pricing for region</summary>
    public OutSystemsRegionServicePricing GetServicesPricing(OutSystemsRegion region)
    {
        return ServicesPricingByRegion.TryGetValue(region, out var pricing)
            ? pricing
            : ServicesPricingByRegion[OutSystemsRegion.Africa]; // Default
    }

    /// <summary>Check if feature is available for O11 deployment type</summary>
    public static bool IsFeatureAvailable(string featureName, OutSystemsDeployment deployment)
    {
        // Cloud-only features
        var cloudOnly = new HashSet<string> { "HighAvailability", "Sentry", "LogStreaming", "LoadTestEnv", "DatabaseReplica" };
        // Self-Managed only features
        var selfManagedOnly = new HashSet<string> { "DisasterRecovery" };

        if (deployment == OutSystemsDeployment.SelfManaged && cloudOnly.Contains(featureName))
            return false;
        if (deployment == OutSystemsDeployment.Cloud && selfManagedOnly.Contains(featureName))
            return false;
        return true;
    }
}

/// <summary>User tier for O11 tiered pricing</summary>
public class OutSystemsUserTier
{
    public int MinUsers { get; set; }
    public int MaxUsers { get; set; }
    public decimal PricePerPack { get; set; }
    public int PackSize { get; set; }
}

/// <summary>AppShield tier pricing</summary>
public class OutSystemsAppShieldTier
{
    public int Tier { get; set; }
    public int MinUsers { get; set; }
    public int MaxUsers { get; set; }
    public decimal Price { get; set; }
}

/// <summary>Region-specific services pricing</summary>
public class OutSystemsRegionServicePricing
{
    public decimal EssentialSuccessPlan { get; set; }
    public decimal PremierSuccessPlan { get; set; }
    public decimal DedicatedGroupSession { get; set; }
    public decimal PublicSession { get; set; }
    public decimal ExpertDay { get; set; }
}

#endregion

#region Deployment Configuration

/// <summary>
/// OutSystems deployment configuration selected by user
/// </summary>
public class OutSystemsDeploymentConfig
{
    // ==================== PLATFORM & DEPLOYMENT ====================

    /// <summary>Platform: ODC or O11</summary>
    public OutSystemsPlatform Platform { get; set; } = OutSystemsPlatform.O11;

    /// <summary>Deployment type (O11 only): Cloud or Self-Managed</summary>
    public OutSystemsDeployment Deployment { get; set; } = OutSystemsDeployment.Cloud;

    /// <summary>Service region for pricing</summary>
    public OutSystemsRegion Region { get; set; } = OutSystemsRegion.Africa;

    // ==================== APPLICATION OBJECTS ====================

    /// <summary>Total Application Objects (AOs = screens + tables + API methods)</summary>
    public int TotalApplicationObjects { get; set; } = 150;

    /// <summary>Calculated AO pack count</summary>
    public int AOPacks => (int)Math.Ceiling(TotalApplicationObjects / 150.0);

    // ==================== USER CAPACITY ====================

    /// <summary>Use unlimited users pricing</summary>
    public bool UseUnlimitedUsers { get; set; } = false;

    /// <summary>Number of internal users (ignored if UseUnlimitedUsers)</summary>
    public int InternalUsers { get; set; } = 100;

    /// <summary>Number of external users (ignored if UseUnlimitedUsers)</summary>
    public int ExternalUsers { get; set; } = 0;

    /// <summary>
    /// Expected user volume for AppShield when using Unlimited Users
    /// Required if UseUnlimitedUsers = true and AppShield is enabled
    /// </summary>
    public int? AppShieldUserVolume { get; set; }

    // ==================== ODC ADD-ONS ====================

    /// <summary>ODC: Support 24x7 Extended (mutually exclusive with Premium)</summary>
    public bool OdcSupport24x7Extended { get; set; } = false;

    /// <summary>ODC: Support 24x7 Premium (mutually exclusive with Extended)</summary>
    public bool OdcSupport24x7Premium { get; set; } = false;

    /// <summary>ODC: AppShield enabled</summary>
    public bool OdcAppShield { get; set; } = false;

    /// <summary>ODC: High Availability</summary>
    public bool OdcHighAvailability { get; set; } = false;

    /// <summary>ODC: Non-Production Runtime quantity (can be > 1)</summary>
    public int OdcNonProdRuntimeQuantity { get; set; } = 0;

    /// <summary>ODC: Private Gateway</summary>
    public bool OdcPrivateGateway { get; set; } = false;

    /// <summary>ODC: Sentry</summary>
    public bool OdcSentry { get; set; } = false;

    // ==================== O11 ADD-ONS ====================

    /// <summary>O11: Support 24x7 Premium (Extended is included)</summary>
    public bool O11Support24x7Premium { get; set; } = false;

    /// <summary>O11: AppShield enabled</summary>
    public bool O11AppShield { get; set; } = false;

    /// <summary>O11: High Availability (Cloud only, included when Sentry enabled)</summary>
    public bool O11HighAvailability { get; set; } = false;

    /// <summary>O11: Sentry with HA (Cloud only)</summary>
    public bool O11Sentry { get; set; } = false;

    /// <summary>O11: Log Streaming quantity (Cloud only)</summary>
    public int O11LogStreamingQuantity { get; set; } = 0;

    /// <summary>O11: Non-Production Environment quantity</summary>
    public int O11NonProdEnvQuantity { get; set; } = 0;

    /// <summary>O11: Load Test Environment quantity (Cloud only)</summary>
    public int O11LoadTestEnvQuantity { get; set; } = 0;

    /// <summary>O11: Environment Pack quantity</summary>
    public int O11EnvPackQuantity { get; set; } = 0;

    /// <summary>O11: Disaster Recovery (Self-Managed only)</summary>
    public bool O11DisasterRecovery { get; set; } = false;

    /// <summary>O11: Database Replica quantity (Cloud only)</summary>
    public int O11DatabaseReplicaQuantity { get; set; } = 0;

    // ==================== SERVICES ====================

    /// <summary>Essential Success Plan quantity</summary>
    public int EssentialSuccessPlanQuantity { get; set; } = 0;

    /// <summary>Premier Success Plan quantity</summary>
    public int PremierSuccessPlanQuantity { get; set; } = 0;

    /// <summary>Dedicated Group Session quantity</summary>
    public int DedicatedGroupSessionQuantity { get; set; } = 0;

    /// <summary>Public Session quantity</summary>
    public int PublicSessionQuantity { get; set; } = 0;

    /// <summary>Expert Day quantity</summary>
    public int ExpertDayQuantity { get; set; } = 0;

    // ==================== DISCOUNT ====================

    /// <summary>Optional discount configuration</summary>
    public OutSystemsDiscount? Discount { get; set; }

    // ==================== SELF-MANAGED INFRASTRUCTURE ====================

    /// <summary>Cloud provider for self-managed</summary>
    public OutSystemsCloudProvider CloudProvider { get; set; } = OutSystemsCloudProvider.OnPremises;

    /// <summary>Azure instance type</summary>
    public OutSystemsAzureInstanceType AzureInstanceType { get; set; } = OutSystemsAzureInstanceType.F4s_v2;

    /// <summary>AWS instance type</summary>
    public OutSystemsAwsInstanceType AwsInstanceType { get; set; } = OutSystemsAwsInstanceType.M5XLarge;

    /// <summary>Front-end servers per environment</summary>
    public int FrontEndServersPerEnvironment { get; set; } = 2;

    /// <summary>Total environments for VM calculation</summary>
    public int TotalEnvironments { get; set; } = 4;

    // ==================== VALIDATION ====================

    /// <summary>Get validation warnings for current configuration</summary>
    public List<string> GetValidationWarnings()
    {
        var warnings = new List<string>();

        if (Platform == OutSystemsPlatform.O11 && Deployment == OutSystemsDeployment.SelfManaged)
        {
            if (O11HighAvailability)
                warnings.Add("High Availability is Cloud-only. It will not be included for Self-Managed.");
            if (O11Sentry)
                warnings.Add("Sentry is Cloud-only. It will not be included for Self-Managed.");
            if (O11LogStreamingQuantity > 0)
                warnings.Add("Log Streaming is Cloud-only. It will not be included for Self-Managed.");
            if (O11LoadTestEnvQuantity > 0)
                warnings.Add("Load Test Environment is Cloud-only. It will not be included for Self-Managed.");
            if (O11DatabaseReplicaQuantity > 0)
                warnings.Add("Database Replica is Cloud-only. It will not be included for Self-Managed.");
        }

        if (Platform == OutSystemsPlatform.O11 && Deployment == OutSystemsDeployment.Cloud)
        {
            if (O11DisasterRecovery)
                warnings.Add("Disaster Recovery is Self-Managed only. It will not be included for Cloud.");
        }

        if (O11Sentry && O11HighAvailability)
            warnings.Add("Sentry includes High Availability. HA cost is included in Sentry.");

        if (OdcSupport24x7Extended && OdcSupport24x7Premium)
            warnings.Add("Support Extended and Premium are mutually exclusive. Only Premium will be applied.");

        if (UseUnlimitedUsers && (OdcAppShield || O11AppShield) && !AppShieldUserVolume.HasValue)
            warnings.Add("AppShield requires expected user volume when using Unlimited Users.");

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

    public OutSystemsPlatform Platform { get; set; }
    public OutSystemsDeployment Deployment { get; set; }
    public OutSystemsRegion Region { get; set; }
    public int AOPackCount { get; set; }

    // ==================== LICENSE BREAKDOWN ====================

    public decimal EditionCost { get; set; }
    public decimal AOPacksCost { get; set; }
    public decimal InternalUsersCost { get; set; }
    public decimal ExternalUsersCost { get; set; }
    public decimal UnlimitedUsersCost { get; set; }

    public decimal LicenseSubtotal => EditionCost + AOPacksCost + InternalUsersCost + ExternalUsersCost + UnlimitedUsersCost;

    /// <summary>Detailed breakdown of license costs</summary>
    public Dictionary<string, decimal> LicenseBreakdown { get; set; } = new();

    // ==================== ADD-ONS BREAKDOWN ====================

    /// <summary>Detailed breakdown of add-on costs by name</summary>
    public Dictionary<string, decimal> AddOnCosts { get; set; } = new();

    public decimal AddOnsSubtotal => AddOnCosts.Values.Sum();

    // ==================== SERVICES BREAKDOWN ====================

    /// <summary>Detailed breakdown of services costs by name</summary>
    public Dictionary<string, decimal> ServiceCosts { get; set; } = new();

    public decimal ServicesSubtotal => ServiceCosts.Values.Sum();

    // ==================== INFRASTRUCTURE (Self-Managed) ====================

    public decimal MonthlyVMCost { get; set; }
    public decimal AnnualVMCost => MonthlyVMCost * 12;
    public int TotalVMCount { get; set; }
    public string? VMDetails { get; set; }

    public decimal InfrastructureSubtotal => AnnualVMCost;

    // ==================== TOTALS ====================

    public decimal GrossTotal => LicenseSubtotal + AddOnsSubtotal + ServicesSubtotal + InfrastructureSubtotal;

    /// <summary>Discount amount applied</summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>Description of discount applied</summary>
    public string? DiscountDescription { get; set; }

    public decimal NetTotal => GrossTotal - DiscountAmount;

    // ==================== PROJECTIONS ====================

    public decimal TotalPerMonth => NetTotal / 12;
    public decimal TotalThreeYear => NetTotal * 3;
    public decimal TotalFiveYear => NetTotal * 5;

    // ==================== METADATA ====================

    /// <summary>Validation warnings</summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>Cost optimization recommendations</summary>
    public List<string> Recommendations { get; set; } = new();

    /// <summary>Line items for detailed display</summary>
    public List<OutSystemsCostLineItem> LineItems { get; set; } = new();

    // ==================== USER DETAILS (for transparency) ====================

    public int InternalUserPackCount { get; set; }
    public int ExternalUserPackCount { get; set; }
    public bool UsedUnlimitedUsers { get; set; }
    public int? AppShieldUserVolume { get; set; }
    public int? AppShieldTier { get; set; }
}

/// <summary>
/// Individual cost line item for detailed breakdown
/// </summary>
public class OutSystemsCostLineItem
{
    public string Category { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal Amount { get; set; }
    public bool IsCloudOnly { get; set; }
    public bool IsSelfManagedOnly { get; set; }
    public bool IsIncluded { get; set; }
    public bool IsDisabled { get; set; }
}

#endregion
