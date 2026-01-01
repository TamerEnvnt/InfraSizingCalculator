namespace InfraSizingCalculator.Data.Entities;

/// <summary>
/// OutSystems pricing settings stored in database
/// Verified against OutSystems Partner Calculator (January 2026)
/// All prices are annual (per year) unless otherwise noted
///
/// Supports: ODC, O11 Cloud, O11 Self-Managed
/// </summary>
public class OutSystemsPricingEntity
{
    public int Id { get; set; }

    // ===========================================
    // ODC PLATFORM PRICING
    // ===========================================

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

    // ===========================================
    // O11 PLATFORM PRICING
    // ===========================================

    /// <summary>
    /// O11 Enterprise Edition base price (annual)
    /// Includes: 150 AOs (1 pack), 100 Internal Users, 3 environments, 24x7 Support
    /// Verified: $36,300
    /// </summary>
    public decimal O11EnterpriseBasePrice { get; set; } = 36300m;

    /// <summary>
    /// O11 Additional AO pack price (150 AOs per pack)
    /// Verified: $36,300 per pack
    /// </summary>
    public decimal O11AOPackPrice { get; set; } = 36300m;

    // ===========================================
    // UNLIMITED USERS (Both Platforms)
    // ===========================================

    /// <summary>
    /// Unlimited Users price PER AO PACK (not flat fee!)
    /// Formula: $60,500 × Number of AO Packs
    /// Verified from tooltip: "$60,500.00 per pack of AOs"
    /// </summary>
    public decimal UnlimitedUsersPerAOPack { get; set; } = 60500m;

    // ===========================================
    // O11 TIERED USER PRICING (JSON)
    // ===========================================

    /// <summary>
    /// O11 Internal User tiered pricing (per 100 users)
    /// First 100 included in Enterprise Edition
    /// JSON format: [{"MinUsers":200,"MaxUsers":1000,"PricePerPack":12100,"PackSize":100},...]
    /// </summary>
    public string? O11InternalUserTiersJson { get; set; } = @"[
        {""MinUsers"":200,""MaxUsers"":1000,""PricePerPack"":12100,""PackSize"":100},
        {""MinUsers"":1100,""MaxUsers"":10000,""PricePerPack"":2420,""PackSize"":100},
        {""MinUsers"":10100,""MaxUsers"":100000000,""PricePerPack"":242,""PackSize"":100}
    ]";

    /// <summary>
    /// O11 External User tiered pricing (per 1000 users)
    /// No users included in Enterprise Edition
    /// JSON format: [{"MinUsers":1,"MaxUsers":10000,"PricePerPack":4840,"PackSize":1000},...]
    /// </summary>
    public string? O11ExternalUserTiersJson { get; set; } = @"[
        {""MinUsers"":1,""MaxUsers"":10000,""PricePerPack"":4840,""PackSize"":1000},
        {""MinUsers"":11000,""MaxUsers"":250000,""PricePerPack"":1452,""PackSize"":1000},
        {""MinUsers"":251000,""MaxUsers"":100000000,""PricePerPack"":30.25,""PackSize"":1000}
    ]";

    // ===========================================
    // ODC ADD-ONS (Per AO Pack)
    // ===========================================

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

    // ===========================================
    // O11 ADD-ONS (Per AO Pack)
    // ===========================================

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

    // ===========================================
    // O11 ADD-ONS (Flat Fee)
    // ===========================================

    /// <summary>O11 Log Streaming: $7,260 flat (Cloud only)</summary>
    public decimal O11LogStreamingFlat { get; set; } = 7260m;

    /// <summary>O11 Database Replica: $96,800 flat (Cloud only)</summary>
    public decimal O11DatabaseReplicaFlat { get; set; } = 96800m;

    // ===========================================
    // APPSHIELD TIERED PRICING (Both Platforms)
    // ===========================================

    /// <summary>
    /// AppShield tiered pricing (flat price per tier, not per-user)
    /// 19 tiers from 0-15M users
    /// JSON format: [{"Tier":1,"MinUsers":0,"MaxUsers":10000,"Price":18150},...]
    /// </summary>
    public string? AppShieldTiersJson { get; set; } = @"[
        {""Tier"":1,""MinUsers"":0,""MaxUsers"":10000,""Price"":18150},
        {""Tier"":2,""MinUsers"":10001,""MaxUsers"":50000,""Price"":32670},
        {""Tier"":3,""MinUsers"":50001,""MaxUsers"":100000,""Price"":54450},
        {""Tier"":4,""MinUsers"":100001,""MaxUsers"":500000,""Price"":96800},
        {""Tier"":5,""MinUsers"":500001,""MaxUsers"":1000000,""Price"":234740},
        {""Tier"":6,""MinUsers"":1000001,""MaxUsers"":2000000,""Price"":275880},
        {""Tier"":7,""MinUsers"":2000001,""MaxUsers"":3000000,""Price"":358160},
        {""Tier"":8,""MinUsers"":3000001,""MaxUsers"":4000000,""Price"":411400},
        {""Tier"":9,""MinUsers"":4000001,""MaxUsers"":5000000,""Price"":508200},
        {""Tier"":10,""MinUsers"":5000001,""MaxUsers"":6000000,""Price"":605000},
        {""Tier"":11,""MinUsers"":6000001,""MaxUsers"":7000000,""Price"":701800},
        {""Tier"":12,""MinUsers"":7000001,""MaxUsers"":8000000,""Price"":798600},
        {""Tier"":13,""MinUsers"":8000001,""MaxUsers"":9000000,""Price"":895400},
        {""Tier"":14,""MinUsers"":9000001,""MaxUsers"":10000000,""Price"":992200},
        {""Tier"":15,""MinUsers"":10000001,""MaxUsers"":11000000,""Price"":1089000},
        {""Tier"":16,""MinUsers"":11000001,""MaxUsers"":12000000,""Price"":1185800},
        {""Tier"":17,""MinUsers"":12000001,""MaxUsers"":13000000,""Price"":1282600},
        {""Tier"":18,""MinUsers"":13000001,""MaxUsers"":14000000,""Price"":1379400},
        {""Tier"":19,""MinUsers"":14000001,""MaxUsers"":15000000,""Price"":1476200}
    ]";

    // ===========================================
    // SERVICES PRICING BY REGION
    // ===========================================

    /// <summary>
    /// Services pricing by region (Success Plans same, Bootcamps/Expert vary)
    /// JSON format: {"Africa":{...},"MiddleEast":{...},...}
    /// </summary>
    public string? ServicesPricingByRegionJson { get; set; } = @"{
        ""Africa"": {
            ""EssentialSuccessPlan"": 30250,
            ""PremierSuccessPlan"": 60500,
            ""DedicatedGroupSession"": 2670,
            ""PublicSession"": 480,
            ""ExpertDay"": 1400
        },
        ""MiddleEast"": {
            ""EssentialSuccessPlan"": 30250,
            ""PremierSuccessPlan"": 60500,
            ""DedicatedGroupSession"": 3820,
            ""PublicSession"": 720,
            ""ExpertDay"": 2130
        },
        ""Americas"": {
            ""EssentialSuccessPlan"": 30250,
            ""PremierSuccessPlan"": 60500,
            ""DedicatedGroupSession"": 2670,
            ""PublicSession"": 480,
            ""ExpertDay"": 1400
        },
        ""Europe"": {
            ""EssentialSuccessPlan"": 30250,
            ""PremierSuccessPlan"": 60500,
            ""DedicatedGroupSession"": 2670,
            ""PublicSession"": 480,
            ""ExpertDay"": 1400
        },
        ""AsiaPacific"": {
            ""EssentialSuccessPlan"": 30250,
            ""PremierSuccessPlan"": 60500,
            ""DedicatedGroupSession"": 2670,
            ""PublicSession"": 480,
            ""ExpertDay"": 1400
        }
    }";

    // ===========================================
    // CLOUD VM PRICING (Self-Managed)
    // ===========================================

    /// <summary>
    /// Azure VM hourly pricing as JSON
    /// Format: {"F4s_v2": 0.169, "D4s_v3": 0.192, "D8s_v3": 0.384, "D16s_v3": 0.768}
    /// </summary>
    public string? AzureVMPricingJson { get; set; } = @"{
        ""F4s_v2"": 0.169,
        ""D4s_v3"": 0.192,
        ""D8s_v3"": 0.384,
        ""D16s_v3"": 0.768
    }";

    /// <summary>
    /// AWS EC2 hourly pricing as JSON
    /// Format: {"M5Large": 0.096, "M5XLarge": 0.192, "M52XLarge": 0.384}
    /// </summary>
    public string? AwsEC2PricingJson { get; set; } = @"{
        ""M5Large"": 0.096,
        ""M5XLarge"": 0.192,
        ""M52XLarge"": 0.384
    }";

    /// <summary>
    /// Hours per month for VM cost calculation
    /// </summary>
    public int HoursPerMonth { get; set; } = 730;

    // ===========================================
    // PACK SIZES
    // ===========================================

    /// <summary>AO pack size (150 AOs per pack)</summary>
    public int AOPackSize { get; set; } = 150;

    /// <summary>Internal user pack size (100 users per pack)</summary>
    public int InternalUserPackSize { get; set; } = 100;

    /// <summary>External user pack size (1000 users per pack)</summary>
    public int ExternalUserPackSize { get; set; } = 1000;

    // ===========================================
    // FEATURE AVAILABILITY
    // ===========================================

    /// <summary>
    /// O11 Cloud-only feature names as JSON array
    /// </summary>
    public string? O11CloudOnlyFeaturesJson { get; set; } = @"[
        ""HighAvailability"",
        ""Sentry"",
        ""LogStreaming"",
        ""LoadTestEnv"",
        ""DatabaseReplica""
    ]";

    /// <summary>
    /// O11 Self-Managed only feature names as JSON array
    /// </summary>
    public string? O11SelfManagedOnlyFeaturesJson { get; set; } = @"[
        ""DisasterRecovery""
    ]";

    // ===========================================
    // TIMESTAMPS
    // ===========================================

    /// <summary>When the entity was created</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>When the entity was last updated</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Data source version/date for reference</summary>
    public string DataSourceVersion { get; set; } = "Partner Calculator January 2026";
}
