namespace InfraSizingCalculator.Data.Entities;

/// <summary>
/// OutSystems pricing settings stored in database
/// Based on OutSystems Partner Price Calculator (2024/2025)
/// All prices are annual (per year) unless otherwise noted
/// </summary>
public class OutSystemsPricingEntity
{
    public int Id { get; set; }

    // ===========================================
    // Edition Base Pricing
    // ===========================================

    /// <summary>
    /// Standard Edition - base annual subscription
    /// Includes 1 AO pack (150 AOs)
    /// </summary>
    public decimal StandardEditionBase { get; set; } = 36300m;

    /// <summary>
    /// Number of Application Objects included in Standard Edition (1 pack = 150 AOs)
    /// </summary>
    public int StandardEditionAOsIncluded { get; set; } = 150;

    /// <summary>
    /// Number of internal users included in Standard Edition
    /// </summary>
    public int StandardEditionInternalUsersIncluded { get; set; } = 100;

    /// <summary>
    /// Enterprise Edition - base annual subscription
    /// Same price as Standard, but with additional features
    /// </summary>
    public decimal EnterpriseEditionBase { get; set; } = 36300m;

    /// <summary>
    /// Number of Application Objects included in Enterprise Edition (1 pack = 150 AOs)
    /// </summary>
    public int EnterpriseEditionAOsIncluded { get; set; } = 150;

    /// <summary>
    /// Number of internal users included in Enterprise Edition
    /// </summary>
    public int EnterpriseEditionInternalUsersIncluded { get; set; } = 500;

    // ===========================================
    // Application Objects Pricing
    // ===========================================

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

    // ===========================================
    // User Licensing
    // ===========================================

    /// <summary>
    /// Internal user pack size (100 users per pack)
    /// </summary>
    public int InternalUserPackSize { get; set; } = 100;

    /// <summary>
    /// Price per additional internal user pack (100 users) per year
    /// </summary>
    public decimal AdditionalInternalUserPackPrice { get; set; } = 6000m;

    /// <summary>
    /// External user pack size (1,000 per pack)
    /// </summary>
    public int ExternalUserPackSize { get; set; } = 1000;

    /// <summary>
    /// External user pack price per year (per 1,000 pack)
    /// From Partner Calculator: $4,840
    /// </summary>
    public decimal ExternalUserPackPerYear { get; set; } = 4840m;

    /// <summary>
    /// Unlimited users flat fee per year
    /// From Partner Calculator: $181,500
    /// </summary>
    public decimal UnlimitedUsersPrice { get; set; } = 181500m;

    // ===========================================
    // Add-ons: AO-Pack Scaled (cost = rate * aoPackCount)
    // ===========================================

    /// <summary>
    /// 24x7 Premium Support per AO pack per year
    /// From Partner Calculator: $3,630 per pack
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
    /// From Partner Calculator: $12,100 per pack
    /// </summary>
    public decimal HighAvailabilityPerAOPack { get; set; } = 12100m;

    /// <summary>
    /// Sentry add-on per AO pack per year (Cloud only, includes HA)
    /// From Partner Calculator: $24,200 per pack
    /// </summary>
    public decimal SentryPerAOPack { get; set; } = 24200m;

    /// <summary>
    /// Disaster Recovery add-on per AO pack per year
    /// From Partner Calculator: $12,100 per pack
    /// </summary>
    public decimal DisasterRecoveryPerAOPack { get; set; } = 12100m;

    // ===========================================
    // Add-ons: Flat Fee
    // ===========================================

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

    // ===========================================
    // Services
    // ===========================================

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

    // ===========================================
    // Legacy Self-Managed (backward compatibility)
    // ===========================================

    /// <summary>
    /// Self-Managed - base platform license (includes LifeTime)
    /// </summary>
    public decimal SelfManagedBase { get; set; } = 48000m;

    /// <summary>
    /// Self-Managed - per additional runtime environment
    /// </summary>
    public decimal SelfManagedPerEnvironment { get; set; } = 9600m;

    /// <summary>
    /// Self-Managed - per front-end server license
    /// </summary>
    public decimal SelfManagedPerFrontEnd { get; set; } = 4800m;

    // ===========================================
    // Legacy Support (backward compatibility)
    // ===========================================

    /// <summary>
    /// Premium Support - percentage of license cost (legacy)
    /// </summary>
    public decimal PremiumSupportPercent { get; set; } = 15m;

    /// <summary>
    /// Elite Support - percentage of license cost (legacy)
    /// </summary>
    public decimal EliteSupportPercent { get; set; } = 25m;

    // ===========================================
    // Cloud VM Pricing (JSON storage for flexibility)
    // ===========================================

    /// <summary>
    /// Azure VM hourly pricing as JSON
    /// Format: {"F4s_v2": 0.169, "D4s_v3": 0.192, "D8s_v3": 0.384, "D16s_v3": 0.768}
    /// </summary>
    public string? AzureVMPricingJson { get; set; } = "{\"F4s_v2\":0.169,\"D4s_v3\":0.192,\"D8s_v3\":0.384,\"D16s_v3\":0.768}";

    /// <summary>
    /// AWS EC2 hourly pricing as JSON
    /// Format: {"M5Large": 0.096, "M5XLarge": 0.192, "M52XLarge": 0.384}
    /// </summary>
    public string? AwsEC2PricingJson { get; set; } = "{\"M5Large\":0.096,\"M5XLarge\":0.192,\"M52XLarge\":0.384}";

    /// <summary>
    /// Hours per month for VM cost calculation
    /// </summary>
    public int HoursPerMonth { get; set; } = 730;

    // ===========================================
    // Cloud-Only Features Configuration
    // ===========================================

    /// <summary>
    /// List of cloud-only feature names as JSON array
    /// </summary>
    public string? CloudOnlyFeaturesJson { get; set; } = "[\"HighAvailability\",\"Sentry\",\"LoadTestEnv\",\"LogStreaming\",\"DatabaseReplica\"]";

    // ===========================================
    // Timestamps
    // ===========================================

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
