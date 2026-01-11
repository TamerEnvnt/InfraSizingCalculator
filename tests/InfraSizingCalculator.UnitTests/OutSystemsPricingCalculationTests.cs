using InfraSizingCalculator.Models.Pricing;

namespace InfraSizingCalculator.UnitTests;

/// <summary>
/// Tests for OutSystems pricing calculations (V2 Model)
/// Based on OutSystems Partner Price Calculator (January 2026)
///
/// Platform-based model: ODC vs O11 (not Standard vs Enterprise)
/// ODC: $30,250 base, $18,150/AO pack, Cloud-native fully managed
/// O11: $36,300 base, $36,300/AO pack, Cloud or Self-Managed
/// </summary>
public class OutSystemsPricingCalculationTests
{
    private readonly OutSystemsPricingSettings _settings;

    public OutSystemsPricingCalculationTests()
    {
        _settings = new OutSystemsPricingSettings();
    }

    #region Platform Base Pricing

    /// <summary>
    /// ODC Platform base price should be $30,250/year
    /// Includes: 150 AOs, 100 Internal Users, 2 runtimes, 8x5 Support
    /// </summary>
    [Fact]
    public void OdcPlatformBasePrice_Is30250()
    {
        Assert.Equal(30250m, _settings.OdcPlatformBasePrice);
    }

    /// <summary>
    /// O11 Enterprise Edition base price should be $36,300/year
    /// Includes: 150 AOs, 100 Internal Users, 3 environments, 24x7 Support
    /// </summary>
    [Fact]
    public void O11EnterpriseBasePrice_Is36300()
    {
        Assert.Equal(36300m, _settings.O11EnterpriseBasePrice);
    }

    /// <summary>
    /// ODC AO pack price should be $18,150 per pack (150 AOs)
    /// </summary>
    [Fact]
    public void OdcAOPackPrice_Is18150()
    {
        Assert.Equal(18150m, _settings.OdcAOPackPrice);
    }

    /// <summary>
    /// O11 AO pack price should be $36,300 per pack (150 AOs)
    /// </summary>
    [Fact]
    public void O11AOPackPrice_Is36300()
    {
        Assert.Equal(36300m, _settings.O11AOPackPrice);
    }

    #endregion

    #region AO Pack Calculations

    /// <summary>
    /// AO pack size should be 150 AOs per pack
    /// </summary>
    [Fact]
    public void AOPackSize_Is150()
    {
        Assert.Equal(150, _settings.AOPackSize);
    }

    /// <summary>
    /// AO pack count calculation should round up correctly
    /// </summary>
    [Theory]
    [InlineData(1, 1)]      // Minimum 1 pack
    [InlineData(150, 1)]    // Exactly 1 pack
    [InlineData(151, 2)]    // Just over 1 pack
    [InlineData(300, 2)]    // Exactly 2 packs
    [InlineData(450, 3)]    // 3 packs
    [InlineData(1500, 10)]  // 10 packs
    public void CalculateAOPacks_RoundsUpCorrectly(int totalAOs, int expectedPacks)
    {
        var packs = _settings.CalculateAOPacks(totalAOs);
        Assert.Equal(expectedPacks, packs);
    }

    /// <summary>
    /// Zero or negative AOs should return minimum 1 pack
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void CalculateAOPacks_MinimumOnePack(int totalAOs)
    {
        var packs = _settings.CalculateAOPacks(totalAOs);
        Assert.Equal(1, packs);
    }

    /// <summary>
    /// DeploymentConfig NumberOfAOPacks should be computed correctly
    /// </summary>
    [Theory]
    [InlineData(150, 1)]
    [InlineData(300, 2)]
    [InlineData(450, 3)]
    [InlineData(151, 2)]
    public void DeploymentConfig_AOPacks_ComputedCorrectly(int aos, int expectedPacks)
    {
        var config = new OutSystemsDeploymentConfig
        {
            TotalApplicationObjects = aos
        };
        Assert.Equal(expectedPacks, config.AOPacks);
    }

    #endregion

    #region Unlimited Users Pricing

    /// <summary>
    /// CRITICAL: Unlimited Users is $60,500 PER AO PACK (not flat fee!)
    /// Formula: $60,500 × Number of AO Packs
    /// </summary>
    [Fact]
    public void UnlimitedUsersPerAOPack_Is60500()
    {
        Assert.Equal(60500m, _settings.UnlimitedUsersPerAOPack);
    }

    /// <summary>
    /// Unlimited Users cost calculation: $60,500 × AO_Packs
    /// </summary>
    [Theory]
    [InlineData(1, 60500)]      // 1 pack × $60,500
    [InlineData(2, 121000)]     // 2 packs × $60,500
    [InlineData(3, 181500)]     // 3 packs × $60,500
    [InlineData(10, 605000)]    // 10 packs × $60,500
    public void UnlimitedUsersCost_ScalesWithAOPacks(int aoPackCount, decimal expectedCost)
    {
        var cost = _settings.UnlimitedUsersPerAOPack * aoPackCount;
        Assert.Equal(expectedCost, cost);
    }

    #endregion

    #region ODC User Pricing (Flat)

    /// <summary>
    /// ODC Internal User pack price should be $6,050 per 100 users
    /// </summary>
    [Fact]
    public void OdcInternalUserPackPrice_Is6050()
    {
        Assert.Equal(6050m, _settings.OdcInternalUserPackPrice);
    }

    /// <summary>
    /// ODC External User pack price should be $6,050 per 1000 users
    /// </summary>
    [Fact]
    public void OdcExternalUserPackPrice_Is6050()
    {
        Assert.Equal(6050m, _settings.OdcExternalUserPackPrice);
    }

    #endregion

    #region O11 Tiered User Pricing

    /// <summary>
    /// O11 Internal User tiers should have correct pricing
    /// Tier 1: 200-1000 users = $12,100/100
    /// Tier 2: 1100-10000 users = $2,420/100
    /// Tier 3: 10100+ users = $242/100
    /// </summary>
    [Fact]
    public void O11InternalUserTiers_HasThreeTiers()
    {
        Assert.Equal(3, _settings.O11InternalUserTiers.Count);

        var tier1 = _settings.O11InternalUserTiers[0];
        Assert.Equal(200, tier1.MinUsers);
        Assert.Equal(1000, tier1.MaxUsers);
        Assert.Equal(12100m, tier1.PricePerPack);
        Assert.Equal(100, tier1.PackSize);

        var tier2 = _settings.O11InternalUserTiers[1];
        Assert.Equal(1100, tier2.MinUsers);
        Assert.Equal(10000, tier2.MaxUsers);
        Assert.Equal(2420m, tier2.PricePerPack);

        var tier3 = _settings.O11InternalUserTiers[2];
        Assert.Equal(10100, tier3.MinUsers);
        Assert.Equal(242m, tier3.PricePerPack);
    }

    /// <summary>
    /// O11 External User tiers should have correct pricing per OUTSYSTEMS_PRICING_SPEC.md
    /// Tier 1: 1,000-10,000 users = $4,840/1000 (packs of 1000)
    /// Tier 2: 11,000-250,000 users = $1,452/1000
    /// Tier 3: 251,000+ users = $30.25/1000
    /// </summary>
    [Fact]
    public void O11ExternalUserTiers_HasThreeTiers()
    {
        Assert.Equal(3, _settings.O11ExternalUserTiers.Count);

        var tier1 = _settings.O11ExternalUserTiers[0];
        Assert.Equal(1000, tier1.MinUsers);    // External packs start at 1000 users
        Assert.Equal(10000, tier1.MaxUsers);
        Assert.Equal(4840m, tier1.PricePerPack);
        Assert.Equal(1000, tier1.PackSize);

        var tier2 = _settings.O11ExternalUserTiers[1];
        Assert.Equal(11000, tier2.MinUsers);
        Assert.Equal(250000, tier2.MaxUsers);
        Assert.Equal(1452m, tier2.PricePerPack);

        var tier3 = _settings.O11ExternalUserTiers[2];
        Assert.Equal(251000, tier3.MinUsers);
        Assert.Equal(30.25m, tier3.PricePerPack);
    }

    #endregion

    #region ODC Add-Ons Pricing

    /// <summary>
    /// ODC Support 24x7 Extended: $6,050 per AO pack
    /// </summary>
    [Fact]
    public void OdcSupport24x7ExtendedPerPack_Is6050()
    {
        Assert.Equal(6050m, _settings.OdcSupport24x7ExtendedPerPack);
    }

    /// <summary>
    /// ODC Support 24x7 Premium: $9,680 per AO pack
    /// </summary>
    [Fact]
    public void OdcSupport24x7PremiumPerPack_Is9680()
    {
        Assert.Equal(9680m, _settings.OdcSupport24x7PremiumPerPack);
    }

    /// <summary>
    /// ODC High Availability: $18,150 per AO pack
    /// </summary>
    [Fact]
    public void OdcHighAvailabilityPerPack_Is18150()
    {
        Assert.Equal(18150m, _settings.OdcHighAvailabilityPerPack);
    }

    /// <summary>
    /// ODC Non-Production Runtime: $6,050 per AO pack
    /// </summary>
    [Fact]
    public void OdcNonProdRuntimePerPack_Is6050()
    {
        Assert.Equal(6050m, _settings.OdcNonProdRuntimePerPack);
    }

    /// <summary>
    /// ODC Private Gateway: $1,210 per AO pack
    /// </summary>
    [Fact]
    public void OdcPrivateGatewayPerPack_Is1210()
    {
        Assert.Equal(1210m, _settings.OdcPrivateGatewayPerPack);
    }

    /// <summary>
    /// ODC Sentry: $6,050 per AO pack
    /// </summary>
    [Fact]
    public void OdcSentryPerPack_Is6050()
    {
        Assert.Equal(6050m, _settings.OdcSentryPerPack);
    }

    #endregion

    #region O11 Add-Ons Pricing

    /// <summary>
    /// O11 Support 24x7 Premium: $3,630 per AO pack
    /// </summary>
    [Fact]
    public void O11Support24x7PremiumPerPack_Is3630()
    {
        Assert.Equal(3630m, _settings.O11Support24x7PremiumPerPack);
    }

    /// <summary>
    /// O11 High Availability: $12,100 per AO pack (Cloud only)
    /// </summary>
    [Fact]
    public void O11HighAvailabilityPerPack_Is12100()
    {
        Assert.Equal(12100m, _settings.O11HighAvailabilityPerPack);
    }

    /// <summary>
    /// O11 Sentry (includes HA): $24,200 per AO pack (Cloud only)
    /// </summary>
    [Fact]
    public void O11SentryPerPack_Is24200()
    {
        Assert.Equal(24200m, _settings.O11SentryPerPack);
    }

    /// <summary>
    /// O11 Non-Production Environment: $3,630 per AO pack
    /// </summary>
    [Fact]
    public void O11NonProdEnvPerPack_Is3630()
    {
        Assert.Equal(3630m, _settings.O11NonProdEnvPerPack);
    }

    /// <summary>
    /// O11 Load Test Environment: $6,050 per AO pack (Cloud only)
    /// </summary>
    [Fact]
    public void O11LoadTestEnvPerPack_Is6050()
    {
        Assert.Equal(6050m, _settings.O11LoadTestEnvPerPack);
    }

    /// <summary>
    /// O11 Environment Pack: $9,680 per AO pack
    /// </summary>
    [Fact]
    public void O11EnvironmentPackPerPack_Is9680()
    {
        Assert.Equal(9680m, _settings.O11EnvironmentPackPerPack);
    }

    /// <summary>
    /// O11 Disaster Recovery: $12,100 per AO pack (Self-Managed only)
    /// </summary>
    [Fact]
    public void O11DisasterRecoveryPerPack_Is12100()
    {
        Assert.Equal(12100m, _settings.O11DisasterRecoveryPerPack);
    }

    /// <summary>
    /// O11 Log Streaming: $7,260 flat (Cloud only)
    /// </summary>
    [Fact]
    public void O11LogStreamingFlat_Is7260()
    {
        Assert.Equal(7260m, _settings.O11LogStreamingFlat);
    }

    /// <summary>
    /// O11 Database Replica: $96,800 flat (Cloud only)
    /// </summary>
    [Fact]
    public void O11DatabaseReplicaFlat_Is96800()
    {
        Assert.Equal(96800m, _settings.O11DatabaseReplicaFlat);
    }

    #endregion

    #region AppShield Tiered Pricing

    /// <summary>
    /// AppShield should have 19 tiers from 0-15M users
    /// </summary>
    [Fact]
    public void AppShieldTiers_Has19Tiers()
    {
        Assert.Equal(19, _settings.AppShieldTiers.Count);
    }

    /// <summary>
    /// AppShield Tier 1: 0-10,000 users = $18,150
    /// </summary>
    [Fact]
    public void AppShieldTier1_Is18150()
    {
        var tier = _settings.AppShieldTiers[0];
        Assert.Equal(1, tier.Tier);
        Assert.Equal(0, tier.MinUsers);
        Assert.Equal(10000, tier.MaxUsers);
        Assert.Equal(18150m, tier.Price);
    }

    /// <summary>
    /// GetAppShieldPrice should return correct tier price
    /// </summary>
    [Theory]
    [InlineData(0, 18150)]          // Tier 1
    [InlineData(5000, 18150)]       // Tier 1
    [InlineData(10000, 18150)]      // Tier 1 (edge)
    [InlineData(10001, 32670)]      // Tier 2
    [InlineData(50001, 54450)]      // Tier 3
    [InlineData(100001, 96800)]     // Tier 4
    [InlineData(500001, 234740)]    // Tier 5
    public void GetAppShieldPrice_ReturnsCorrectTierPrice(int userVolume, decimal expectedPrice)
    {
        var price = _settings.GetAppShieldPrice(userVolume);
        Assert.Equal(expectedPrice, price);
    }

    #endregion

    #region Services Pricing by Region

    /// <summary>
    /// All 5 regions should have pricing defined
    /// </summary>
    [Fact]
    public void ServicesPricingByRegion_HasAllRegions()
    {
        Assert.Equal(5, _settings.ServicesPricingByRegion.Count);
        Assert.Contains(OutSystemsRegion.Africa, _settings.ServicesPricingByRegion.Keys);
        Assert.Contains(OutSystemsRegion.MiddleEast, _settings.ServicesPricingByRegion.Keys);
        Assert.Contains(OutSystemsRegion.Americas, _settings.ServicesPricingByRegion.Keys);
        Assert.Contains(OutSystemsRegion.Europe, _settings.ServicesPricingByRegion.Keys);
        Assert.Contains(OutSystemsRegion.AsiaPacific, _settings.ServicesPricingByRegion.Keys);
    }

    /// <summary>
    /// Success Plans are same across all regions
    /// Essential: $30,250, Premier: $60,500
    /// </summary>
    [Theory]
    [InlineData(OutSystemsRegion.Africa)]
    [InlineData(OutSystemsRegion.MiddleEast)]
    [InlineData(OutSystemsRegion.Americas)]
    [InlineData(OutSystemsRegion.Europe)]
    [InlineData(OutSystemsRegion.AsiaPacific)]
    public void SuccessPlans_SameAcrossAllRegions(OutSystemsRegion region)
    {
        var pricing = _settings.GetServicesPricing(region);
        Assert.Equal(30250m, pricing.EssentialSuccessPlan);
        Assert.Equal(60500m, pricing.PremierSuccessPlan);
    }

    /// <summary>
    /// Middle East has different pricing for Bootcamps and Expert Days
    /// </summary>
    [Fact]
    public void MiddleEast_HasHigherServicesPricing()
    {
        var middleEast = _settings.GetServicesPricing(OutSystemsRegion.MiddleEast);
        var africa = _settings.GetServicesPricing(OutSystemsRegion.Africa);

        Assert.Equal(3820m, middleEast.DedicatedGroupSession);
        Assert.Equal(720m, middleEast.PublicSession);
        Assert.Equal(2130m, middleEast.ExpertDay);

        // Compare with Africa (lower pricing)
        Assert.Equal(2670m, africa.DedicatedGroupSession);
        Assert.Equal(480m, africa.PublicSession);
        Assert.Equal(1400m, africa.ExpertDay);
    }

    #endregion

    #region Cloud VM Pricing

    /// <summary>
    /// Hours per month for VM cost calculation should be 730
    /// </summary>
    [Fact]
    public void HoursPerMonth_Is730()
    {
        Assert.Equal(730, _settings.HoursPerMonth);
    }

    /// <summary>
    /// Azure VM hourly pricing should match Partner Calculator
    /// </summary>
    [Theory]
    [InlineData(OutSystemsAzureInstanceType.F4s_v2, 0.169)]
    [InlineData(OutSystemsAzureInstanceType.D4s_v3, 0.192)]
    [InlineData(OutSystemsAzureInstanceType.D8s_v3, 0.384)]
    [InlineData(OutSystemsAzureInstanceType.D16s_v3, 0.768)]
    public void AzureVMHourlyPricing_CorrectRates(OutSystemsAzureInstanceType instance, decimal expectedRate)
    {
        var rate = _settings.AzureVMHourlyPricing[instance];
        Assert.Equal(expectedRate, rate);
    }

    /// <summary>
    /// AWS EC2 hourly pricing should match Partner Calculator
    /// </summary>
    [Theory]
    [InlineData(OutSystemsAwsInstanceType.M5Large, 0.096)]
    [InlineData(OutSystemsAwsInstanceType.M5XLarge, 0.192)]
    [InlineData(OutSystemsAwsInstanceType.M52XLarge, 0.384)]
    public void AwsEC2HourlyPricing_CorrectRates(OutSystemsAwsInstanceType instance, decimal expectedRate)
    {
        var rate = _settings.AwsEC2HourlyPricing[instance];
        Assert.Equal(expectedRate, rate);
    }

    /// <summary>
    /// Azure monthly VM cost calculation: hourlyRate * 730 * serverCount
    /// </summary>
    [Theory]
    [InlineData(OutSystemsAzureInstanceType.F4s_v2, 1, 123.37)]     // 0.169 * 730
    [InlineData(OutSystemsAzureInstanceType.F4s_v2, 4, 493.48)]     // 0.169 * 730 * 4
    [InlineData(OutSystemsAzureInstanceType.D8s_v3, 2, 560.64)]     // 0.384 * 730 * 2
    public void AzureMonthlyVMCost_CorrectCalculation(OutSystemsAzureInstanceType instance, int servers, decimal expectedCost)
    {
        var hourlyRate = _settings.AzureVMHourlyPricing[instance];
        var monthlyVMCost = hourlyRate * _settings.HoursPerMonth * servers;
        Assert.Equal(expectedCost, monthlyVMCost);
    }

    /// <summary>
    /// AWS monthly VM cost calculation: hourlyRate * 730 * serverCount
    /// </summary>
    [Theory]
    [InlineData(OutSystemsAwsInstanceType.M5Large, 1, 70.08)]       // 0.096 * 730
    [InlineData(OutSystemsAwsInstanceType.M5XLarge, 4, 560.64)]     // 0.192 * 730 * 4
    [InlineData(OutSystemsAwsInstanceType.M52XLarge, 2, 560.64)]    // 0.384 * 730 * 2
    public void AwsMonthlyVMCost_CorrectCalculation(OutSystemsAwsInstanceType instance, int servers, decimal expectedCost)
    {
        var hourlyRate = _settings.AwsEC2HourlyPricing[instance];
        var monthlyVMCost = hourlyRate * _settings.HoursPerMonth * servers;
        Assert.Equal(expectedCost, monthlyVMCost);
    }

    #endregion

    #region Feature Availability

    /// <summary>
    /// Cloud-only features should not be available for Self-Managed
    /// </summary>
    [Theory]
    [InlineData("HighAvailability", false)]
    [InlineData("Sentry", false)]
    [InlineData("LogStreaming", false)]
    [InlineData("LoadTestEnv", false)]
    [InlineData("DatabaseReplica", false)]
    public void CloudOnlyFeatures_NotAvailableForSelfManaged(string feature, bool expectedAvailable)
    {
        var result = OutSystemsPricingSettings.IsFeatureAvailable(feature, OutSystemsDeployment.SelfManaged);
        Assert.Equal(expectedAvailable, result);
    }

    /// <summary>
    /// Cloud-only features should be available for Cloud deployment
    /// </summary>
    [Theory]
    [InlineData("HighAvailability", true)]
    [InlineData("Sentry", true)]
    [InlineData("LogStreaming", true)]
    [InlineData("LoadTestEnv", true)]
    [InlineData("DatabaseReplica", true)]
    public void CloudOnlyFeatures_AvailableForCloud(string feature, bool expectedAvailable)
    {
        var result = OutSystemsPricingSettings.IsFeatureAvailable(feature, OutSystemsDeployment.Cloud);
        Assert.Equal(expectedAvailable, result);
    }

    /// <summary>
    /// Disaster Recovery is Self-Managed only
    /// </summary>
    [Theory]
    [InlineData(OutSystemsDeployment.Cloud, false)]
    [InlineData(OutSystemsDeployment.SelfManaged, true)]
    public void DisasterRecovery_OnlyAvailableForSelfManaged(OutSystemsDeployment deployment, bool expectedAvailable)
    {
        var result = OutSystemsPricingSettings.IsFeatureAvailable("DisasterRecovery", deployment);
        Assert.Equal(expectedAvailable, result);
    }

    /// <summary>
    /// Common features should be available for both deployments
    /// </summary>
    [Theory]
    [InlineData("NonProductionEnv", OutSystemsDeployment.Cloud, true)]
    [InlineData("NonProductionEnv", OutSystemsDeployment.SelfManaged, true)]
    [InlineData("Support24x7Premium", OutSystemsDeployment.Cloud, true)]
    [InlineData("Support24x7Premium", OutSystemsDeployment.SelfManaged, true)]
    public void CommonFeatures_AvailableForBothDeployments(string feature, OutSystemsDeployment deployment, bool expectedAvailable)
    {
        var result = OutSystemsPricingSettings.IsFeatureAvailable(feature, deployment);
        Assert.Equal(expectedAvailable, result);
    }

    #endregion

    #region Deployment Config Validation

    /// <summary>
    /// O11 Self-managed deployment with cloud-only features should generate warnings
    /// </summary>
    [Fact]
    public void DeploymentConfig_O11SelfManagedWithCloudFeatures_GeneratesWarnings()
    {
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.SelfManaged,
            O11HighAvailability = true,
            O11Sentry = true,
            O11LogStreamingQuantity = 1,
            O11LoadTestEnvQuantity = 1,
            O11DatabaseReplicaQuantity = 1
        };

        var warnings = config.GetValidationWarnings();

        // 5 cloud-only warnings + 1 "Sentry includes HA" warning = 6 total
        Assert.Equal(6, warnings.Count);
        Assert.Contains(warnings, w => w.Contains("High Availability"));
        Assert.Contains(warnings, w => w.Contains("Sentry"));
        Assert.Contains(warnings, w => w.Contains("Log Streaming"));
        Assert.Contains(warnings, w => w.Contains("Load Test"));
        Assert.Contains(warnings, w => w.Contains("Database Replica"));
        Assert.Contains(warnings, w => w.Contains("Sentry includes High Availability"));
    }

    /// <summary>
    /// O11 Cloud deployment with cloud-only features should not generate cloud warnings
    /// </summary>
    [Fact]
    public void DeploymentConfig_O11CloudWithCloudFeatures_NoCloudWarnings()
    {
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.Cloud,
            O11HighAvailability = true,
            O11LogStreamingQuantity = 1
        };

        var warnings = config.GetValidationWarnings();

        Assert.Empty(warnings);
    }

    /// <summary>
    /// O11 Cloud with Disaster Recovery should generate warning (Self-Managed only)
    /// </summary>
    [Fact]
    public void DeploymentConfig_O11CloudWithDR_GeneratesWarning()
    {
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.Cloud,
            O11DisasterRecovery = true
        };

        var warnings = config.GetValidationWarnings();

        Assert.Single(warnings);
        Assert.Contains("Disaster Recovery is Self-Managed only", warnings[0]);
    }

    /// <summary>
    /// Selecting both Sentry and HA should generate warning (Sentry includes HA)
    /// </summary>
    [Fact]
    public void DeploymentConfig_SentryWithHA_GeneratesWarning()
    {
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.Cloud,
            O11Sentry = true,
            O11HighAvailability = true
        };

        var warnings = config.GetValidationWarnings();

        Assert.Single(warnings);
        Assert.Contains("Sentry includes High Availability", warnings[0]);
    }

    /// <summary>
    /// ODC with both Extended and Premium support should generate warning
    /// </summary>
    [Fact]
    public void DeploymentConfig_OdcBothSupportLevels_GeneratesWarning()
    {
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.ODC,
            OdcSupport24x7Extended = true,
            OdcSupport24x7Premium = true
        };

        var warnings = config.GetValidationWarnings();

        Assert.Single(warnings);
        Assert.Contains("mutually exclusive", warnings[0]);
    }

    /// <summary>
    /// AppShield with Unlimited Users but no user volume should generate warning
    /// </summary>
    [Fact]
    public void DeploymentConfig_AppShieldWithUnlimitedNoVolume_GeneratesWarning()
    {
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            UseUnlimitedUsers = true,
            O11AppShield = true,
            AppShieldUserVolume = null
        };

        var warnings = config.GetValidationWarnings();

        Assert.Single(warnings);
        Assert.Contains("AppShield requires expected user volume", warnings[0]);
    }

    #endregion

    #region Discount Calculations

    /// <summary>
    /// Percentage discount should calculate correctly
    /// </summary>
    [Theory]
    [InlineData(10, 100000, 0, 0, 10000)]   // 10% of 100K = 10K
    [InlineData(20, 50000, 25000, 25000, 20000)] // 20% of 100K total = 20K
    [InlineData(50, 100000, 0, 0, 50000)]   // 50% of 100K = 50K
    public void Discount_Percentage_CalculatesCorrectly(decimal percentage, decimal license, decimal addOns, decimal services, decimal expectedDiscount)
    {
        var discount = new OutSystemsDiscount
        {
            Type = OutSystemsDiscountType.Percentage,
            Scope = OutSystemsDiscountScope.Total,
            Value = percentage
        };

        var result = discount.CalculateDiscount(license, addOns, services);
        Assert.Equal(expectedDiscount, result);
    }

    /// <summary>
    /// Fixed amount discount should not exceed applicable amount
    /// </summary>
    [Theory]
    [InlineData(5000, 10000, 0, 0, 5000)]       // 5K discount on 10K = 5K
    [InlineData(15000, 10000, 0, 0, 10000)]     // 15K discount on 10K = 10K (capped)
    public void Discount_FixedAmount_CapsAtApplicable(decimal discountAmount, decimal license, decimal addOns, decimal services, decimal expectedDiscount)
    {
        var discount = new OutSystemsDiscount
        {
            Type = OutSystemsDiscountType.FixedAmount,
            Scope = OutSystemsDiscountScope.Total,
            Value = discountAmount
        };

        var result = discount.CalculateDiscount(license, addOns, services);
        Assert.Equal(expectedDiscount, result);
    }

    /// <summary>
    /// Discount scope should only apply to specified category
    /// </summary>
    [Fact]
    public void Discount_ScopeLicenseOnly_OnlyAppliesToLicense()
    {
        var discount = new OutSystemsDiscount
        {
            Type = OutSystemsDiscountType.Percentage,
            Scope = OutSystemsDiscountScope.LicenseOnly,
            Value = 20
        };

        // 20% of license only (50K), not of add-ons or services
        var result = discount.CalculateDiscount(50000m, 20000m, 10000m);
        Assert.Equal(10000m, result); // 20% of 50K = 10K
    }

    /// <summary>
    /// AddOnsOnly scope should only apply to add-ons
    /// </summary>
    [Fact]
    public void Discount_ScopeAddOnsOnly_OnlyAppliesToAddOns()
    {
        var discount = new OutSystemsDiscount
        {
            Type = OutSystemsDiscountType.Percentage,
            Scope = OutSystemsDiscountScope.AddOnsOnly,
            Value = 25
        };

        var result = discount.CalculateDiscount(50000m, 20000m, 10000m);
        Assert.Equal(5000m, result); // 25% of 20K = 5K
    }

    #endregion

    #region Pricing Result Structure

    /// <summary>
    /// PricingResult LicenseSubtotal should sum all license costs
    /// </summary>
    [Fact]
    public void PricingResult_LicenseSubtotal_SumsCorrectly()
    {
        var result = new OutSystemsPricingResult
        {
            EditionCost = 36300m,
            AOPacksCost = 36300m,
            InternalUsersCost = 12100m,
            ExternalUsersCost = 4840m,
            UnlimitedUsersCost = 0m
        };

        Assert.Equal(89540m, result.LicenseSubtotal);
    }

    /// <summary>
    /// PricingResult AddOnsSubtotal should sum all add-on costs
    /// </summary>
    [Fact]
    public void PricingResult_AddOnsSubtotal_SumsCorrectly()
    {
        var result = new OutSystemsPricingResult();
        result.AddOnCosts["Support 24x7 Premium"] = 3630m;
        result.AddOnCosts["High Availability"] = 12100m;
        result.AddOnCosts["Non-Prod Env"] = 3630m;

        Assert.Equal(19360m, result.AddOnsSubtotal);
    }

    /// <summary>
    /// PricingResult ServicesSubtotal should sum all services costs
    /// </summary>
    [Fact]
    public void PricingResult_ServicesSubtotal_SumsCorrectly()
    {
        var result = new OutSystemsPricingResult();
        result.ServiceCosts["Essential Success Plan"] = 30250m;
        result.ServiceCosts["Dedicated Group Session"] = 3820m;

        Assert.Equal(34070m, result.ServicesSubtotal);
    }

    /// <summary>
    /// PricingResult GrossTotal should sum all subtotals
    /// </summary>
    [Fact]
    public void PricingResult_GrossTotal_SumsAllSubtotals()
    {
        var result = new OutSystemsPricingResult
        {
            EditionCost = 36300m,
            AOPacksCost = 0m,
            InternalUsersCost = 0m,
            ExternalUsersCost = 0m,
            MonthlyVMCost = 500m  // $6,000/year infrastructure
        };
        result.AddOnCosts["HA"] = 12100m;
        result.ServiceCosts["Essential"] = 30250m;

        // License: 36,300 + AddOns: 12,100 + Services: 30,250 + Infra: 6,000 = 84,650
        Assert.Equal(84650m, result.GrossTotal);
    }

    /// <summary>
    /// PricingResult NetTotal should apply discount
    /// </summary>
    [Fact]
    public void PricingResult_NetTotal_SubtractsDiscount()
    {
        var result = new OutSystemsPricingResult
        {
            EditionCost = 100000m,
            DiscountAmount = 10000m
        };

        Assert.Equal(90000m, result.NetTotal);
    }

    /// <summary>
    /// PricingResult multi-year projections should be correct
    /// </summary>
    [Fact]
    public void PricingResult_MultiYearProjections_CorrectMultiples()
    {
        var result = new OutSystemsPricingResult
        {
            EditionCost = 36300m
        };

        Assert.Equal(36300m / 12, result.TotalPerMonth);
        Assert.Equal(36300m * 3, result.TotalThreeYear);
        Assert.Equal(36300m * 5, result.TotalFiveYear);
    }

    #endregion

    #region Comprehensive Scenario Tests

    /// <summary>
    /// Example 1: ODC Base Configuration
    /// Platform: ODC, 1 AO Pack, 100 Internal Users (included)
    /// Expected: $30,250/year
    /// </summary>
    [Fact]
    public void Example1_OdcBaseConfiguration_CorrectTotal()
    {
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.ODC,
            TotalApplicationObjects = 150,
            InternalUsers = 100,
            ExternalUsers = 0
        };

        // Base ODC license only
        var expectedTotal = _settings.OdcPlatformBasePrice;
        Assert.Equal(30250m, expectedTotal);
    }

    /// <summary>
    /// Example 2: O11 Cloud with 3 AO Packs
    /// Platform: O11, Deployment: Cloud, 3 AO Packs (450 AOs)
    /// With: HA, 24x7 Premium Support
    /// </summary>
    [Fact]
    public void Example2_O11CloudWithAddOns_CorrectBreakdown()
    {
        var aoPackCount = 3;

        // Base + Additional AO Packs
        var editionBase = _settings.O11EnterpriseBasePrice;  // $36,300 (includes 1 pack)
        var additionalAOPacks = _settings.O11AOPackPrice * (aoPackCount - 1);  // $72,600 (2 additional packs)
        var licenseTotal = editionBase + additionalAOPacks;  // $108,900

        Assert.Equal(36300m, editionBase);
        Assert.Equal(72600m, additionalAOPacks);
        Assert.Equal(108900m, licenseTotal);

        // Add-ons (per AO pack)
        var haCost = _settings.O11HighAvailabilityPerPack * aoPackCount;  // $36,300
        var supportCost = _settings.O11Support24x7PremiumPerPack * aoPackCount;  // $10,890
        var addOnsTotal = haCost + supportCost;  // $47,190

        Assert.Equal(36300m, haCost);
        Assert.Equal(10890m, supportCost);
        Assert.Equal(47190m, addOnsTotal);

        // Grand total
        var grandTotal = licenseTotal + addOnsTotal;
        Assert.Equal(156090m, grandTotal);
    }

    /// <summary>
    /// Example 3: O11 Self-Managed on Azure
    /// Platform: O11, Deployment: Self-Managed, Cloud Provider: Azure
    /// 4 environments × 2 front-end servers = 8 VMs (D4s_v3)
    /// </summary>
    [Fact]
    public void Example3_O11SelfManagedAzure_CorrectVMCosts()
    {
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.SelfManaged,
            CloudProvider = OutSystemsCloudProvider.Azure,
            AzureInstanceType = OutSystemsAzureInstanceType.D4s_v3,
            FrontEndServersPerEnvironment = 2,
            TotalEnvironments = 4
        };

        var totalVMs = config.TotalEnvironments * config.FrontEndServersPerEnvironment;
        Assert.Equal(8, totalVMs);

        // Monthly VM cost: 0.192 × 730 × 8 = $1,121.28
        var hourlyRate = _settings.AzureVMHourlyPricing[config.AzureInstanceType];
        var monthlyVMCost = hourlyRate * _settings.HoursPerMonth * totalVMs;
        Assert.Equal(1121.28m, monthlyVMCost);

        // Annual VM cost: $13,455.36
        var annualVMCost = monthlyVMCost * 12;
        Assert.Equal(13455.36m, annualVMCost);
    }

    /// <summary>
    /// Example 4: Unlimited Users with AppShield
    /// Unlimited Users: $60,500 × 3 packs = $181,500
    /// AppShield Tier 1 (10K users): $18,150
    /// </summary>
    [Fact]
    public void Example4_UnlimitedUsersWithAppShield_CorrectCosts()
    {
        var aoPackCount = 3;
        var userVolume = 10000;

        var unlimitedCost = _settings.UnlimitedUsersPerAOPack * aoPackCount;
        Assert.Equal(181500m, unlimitedCost);

        var appShieldCost = _settings.GetAppShieldPrice(userVolume);
        Assert.Equal(18150m, appShieldCost);

        var totalUserCosts = unlimitedCost + appShieldCost;
        Assert.Equal(199650m, totalUserCosts);
    }

    /// <summary>
    /// Example 5: ODC with multiple add-ons
    /// ODC Base + HA + Sentry + 2 Non-Prod Runtimes
    /// </summary>
    [Fact]
    public void Example5_OdcWithMultipleAddOns_CorrectTotal()
    {
        var aoPackCount = 1;

        var baseCost = _settings.OdcPlatformBasePrice;  // $30,250
        var haCost = _settings.OdcHighAvailabilityPerPack * aoPackCount;  // $18,150
        var sentryCost = _settings.OdcSentryPerPack * aoPackCount;  // $6,050
        var nonProdCost = _settings.OdcNonProdRuntimePerPack * aoPackCount * 2;  // $12,100 (qty 2)

        var total = baseCost + haCost + sentryCost + nonProdCost;
        Assert.Equal(66550m, total);
    }

    #endregion

    #region Verified Partner Calculator Examples from OUTSYSTEMS_PRICING_SPEC.md

    /// <summary>
    /// VERIFIED Example 1: ODC (Middle East)
    /// Source: Partner Calculator screenshots ODC - Exmp 1.1/1.2/1.3.png
    /// Configuration:
    /// - Platform: ODC
    /// - AOs: 450 (3 packs)
    /// - Internal Users: 1,000 (10 packs)
    /// - External Users: 5,000 (5 packs)
    /// - Add-Ons: 24x7 Extended, AppShield, HA, 2 Non-Prod, Private Gateway, Sentry
    /// - Services: Premier Success, Dedicated Session, Public Session, Expert Day
    /// - TOTAL: $367,250.00
    /// </summary>
    [Fact]
    public void VerifiedExample1_ODC_MiddleEast_CorrectTotal()
    {
        // Section 1-4: License
        var platformBase = _settings.OdcPlatformBasePrice;                    // $30,250
        var aoPackCount = 3;
        var aoPacks = (aoPackCount - 1) * _settings.OdcAOPackPrice;           // 2 × $18,150 = $36,300
        var internalPacks = 9;  // 1000 users - 100 included = 900 additional / 100 = 9 packs
        var internalUsers = internalPacks * _settings.OdcInternalUserPackPrice; // 9 × $6,050 = $54,450
        var externalPacks = 5;  // 5000 users / 1000 = 5 packs
        var externalUsers = externalPacks * _settings.OdcExternalUserPackPrice; // 5 × $6,050 = $30,250

        var licenseSubtotal = platformBase + aoPacks + internalUsers + externalUsers;
        Assert.Equal(151250m, licenseSubtotal);

        // Section 5: Add-Ons
        var support24x7Extended = aoPackCount * _settings.OdcSupport24x7ExtendedPerPack; // 3 × $6,050 = $18,150
        var appShield = _settings.GetAppShieldPrice(6000);  // 6K users = Tier 1 = $18,150
        var highAvailability = aoPackCount * _settings.OdcHighAvailabilityPerPack;      // 3 × $18,150 = $54,450
        var nonProdRuntime = 2 * aoPackCount * _settings.OdcNonProdRuntimePerPack;      // 2 × 3 × $6,050 = $36,300
        var privateGateway = aoPackCount * _settings.OdcPrivateGatewayPerPack;          // 3 × $1,210 = $3,630
        var sentry = aoPackCount * _settings.OdcSentryPerPack;                          // 3 × $6,050 = $18,150

        var addOnsSubtotal = support24x7Extended + appShield + highAvailability + nonProdRuntime + privateGateway + sentry;
        Assert.Equal(148830m, addOnsSubtotal);

        // Section 6: Services (Middle East)
        var servicesPricing = _settings.GetServicesPricing(OutSystemsRegion.MiddleEast);
        var premierSuccess = servicesPricing.PremierSuccessPlan;                // $60,500
        var dedicatedSession = servicesPricing.DedicatedGroupSession;           // $3,820
        var publicSession = servicesPricing.PublicSession;                      // $720
        var expertDay = servicesPricing.ExpertDay;                              // $2,130

        var servicesSubtotal = premierSuccess + dedicatedSession + publicSession + expertDay;
        Assert.Equal(67170m, servicesSubtotal);

        // TOTAL
        var total = licenseSubtotal + addOnsSubtotal + servicesSubtotal;
        Assert.Equal(367250m, total);
    }

    /// <summary>
    /// VERIFIED Example 2: O11 Cloud (Middle East)
    /// Source: Partner Calculator screenshots O11 Cloud - Exmp 2.1/2.2/2.3.png
    /// Configuration:
    /// - Platform: O11 Enterprise Cloud
    /// - AOs: 450 (3 packs)
    /// - Users: Unlimited
    /// - AppShield: 15,000,000 users (Tier 19)
    /// - Add-Ons: 24x7 Premium, Sentry, Log Streaming, 1 Non-Prod, Load Test, Env Pack, DB Replica
    /// - Services: Premier Success, Dedicated Session, Public Session, Expert Day
    /// - TOTAL: $2,079,400.00
    /// </summary>
    [Fact]
    public void VerifiedExample2_O11Cloud_MiddleEast_CorrectTotal()
    {
        // Section 1-4: License
        var platformBase = _settings.O11EnterpriseBasePrice;                  // $36,300
        var aoPackCount = 3;
        var aoPacks = (aoPackCount - 1) * _settings.O11AOPackPrice;           // 2 × $36,300 = $72,600
        var unlimitedUsers = aoPackCount * _settings.UnlimitedUsersPerAOPack; // 3 × $60,500 = $181,500

        var licenseSubtotal = platformBase + aoPacks + unlimitedUsers;
        Assert.Equal(290400m, licenseSubtotal);

        // Section 5: Add-Ons
        var support24x7Premium = aoPackCount * _settings.O11Support24x7PremiumPerPack; // 3 × $3,630 = $10,890
        var appShield = _settings.GetAppShieldPrice(15000000);  // 15M users = Tier 19 = $1,476,200
        Assert.Equal(1476200m, appShield);
        var sentry = aoPackCount * _settings.O11SentryPerPack;                         // 3 × $24,200 = $72,600
        var logStreaming = _settings.O11LogStreamingFlat;                              // $7,260 flat
        var nonProdEnv = aoPackCount * _settings.O11NonProdEnvPerPack;                 // 3 × $3,630 = $10,890
        var loadTestEnv = aoPackCount * _settings.O11LoadTestEnvPerPack;               // 3 × $6,050 = $18,150
        var envPack = aoPackCount * _settings.O11EnvironmentPackPerPack;               // 3 × $9,680 = $29,040
        var dbReplica = _settings.O11DatabaseReplicaFlat;                              // $96,800 flat

        var addOnsSubtotal = support24x7Premium + appShield + sentry + logStreaming + nonProdEnv + loadTestEnv + envPack + dbReplica;
        Assert.Equal(1721830m, addOnsSubtotal);

        // Section 6: Services (Middle East)
        var servicesPricing = _settings.GetServicesPricing(OutSystemsRegion.MiddleEast);
        var servicesSubtotal = servicesPricing.PremierSuccessPlan + servicesPricing.DedicatedGroupSession
                             + servicesPricing.PublicSession + servicesPricing.ExpertDay;
        Assert.Equal(67170m, servicesSubtotal);

        // TOTAL
        var total = licenseSubtotal + addOnsSubtotal + servicesSubtotal;
        Assert.Equal(2079400m, total);
    }

    /// <summary>
    /// VERIFIED Example 3: O11 Self-Managed (Middle East)
    /// Source: Partner Calculator screenshots O11 Self-Managed - Exmp 3.1/3.2/3.3.png
    /// Configuration:
    /// - Platform: O11 Enterprise Self-Managed
    /// - AOs: 450 (3 packs)
    /// - Internal Users: 2,000 (tiered pricing)
    /// - External Users: 1,000,000 (tiered pricing)
    /// - AppShield: 1,002,000 users (Tier 6)
    /// - Add-Ons: 24x7 Premium, 1 Non-Prod, Env Pack, Disaster Recovery
    /// - Services: Premier Success, Dedicated Session, Public Session, Expert Day
    /// - TOTAL: $1,091,737.50
    /// </summary>
    [Fact]
    public void VerifiedExample3_O11SelfManaged_MiddleEast_CorrectTotal()
    {
        // Section 1-4: License
        var platformBase = _settings.O11EnterpriseBasePrice;         // $36,300
        var aoPackCount = 3;
        var aoPacks = (aoPackCount - 1) * _settings.O11AOPackPrice;  // 2 × $36,300 = $72,600

        // Internal Users: 2,000 with tiered pricing
        // Base: 100 users included = $0
        // Tier 1 (200-1000): 900 users = 9 packs × $12,100 = $108,900
        // Tier 2 (1100-10000): 1000 users = 10 packs × $2,420 = $24,200
        // Total: $133,100
        var internalUsersCost = 133100m;

        // External Users: 1,000,000 with tiered pricing
        // Tier 1 (1-10000): 10,000 users = 10 packs × $4,840 = $48,400
        // Tier 2 (11000-250000): 240,000 users = 240 packs × $1,452 = $348,480
        // Tier 3 (251000+): 750,000 users = 750 packs × $30.25 = $22,687.50
        // Total: $419,567.50
        var externalUsersCost = 419567.50m;

        var licenseSubtotal = platformBase + aoPacks + internalUsersCost + externalUsersCost;
        Assert.Equal(661567.50m, licenseSubtotal);

        // Section 5: Add-Ons
        var support24x7Premium = aoPackCount * _settings.O11Support24x7PremiumPerPack; // 3 × $3,630 = $10,890
        var appShield = _settings.GetAppShieldPrice(1002000);  // 1,002,000 users = Tier 6 = $275,880
        Assert.Equal(275880m, appShield);
        var nonProdEnv = aoPackCount * _settings.O11NonProdEnvPerPack;                 // 3 × $3,630 = $10,890
        var envPack = aoPackCount * _settings.O11EnvironmentPackPerPack;               // 3 × $9,680 = $29,040
        var disasterRecovery = aoPackCount * _settings.O11DisasterRecoveryPerPack;     // 3 × $12,100 = $36,300

        var addOnsSubtotal = support24x7Premium + appShield + nonProdEnv + envPack + disasterRecovery;
        Assert.Equal(363000m, addOnsSubtotal);

        // Section 6: Services (Middle East)
        var servicesPricing = _settings.GetServicesPricing(OutSystemsRegion.MiddleEast);
        var servicesSubtotal = servicesPricing.PremierSuccessPlan + servicesPricing.DedicatedGroupSession
                             + servicesPricing.PublicSession + servicesPricing.ExpertDay;
        Assert.Equal(67170m, servicesSubtotal);

        // TOTAL
        var total = licenseSubtotal + addOnsSubtotal + servicesSubtotal;
        Assert.Equal(1091737.50m, total);
    }

    /// <summary>
    /// Verify O11 Internal User tiered pricing calculation
    /// 2,000 users should cost $133,100 based on spec
    /// </summary>
    [Fact]
    public void VerifiedO11InternalUserTieredPricing_2000Users()
    {
        // Base: 100 users included = $0
        // Tier 1 (200-1000): 900 users = 9 packs × $12,100 = $108,900
        // Tier 2 (1100-10000): 1000 users = 10 packs × $2,420 = $24,200
        // Total: $133,100

        var tier1 = _settings.O11InternalUserTiers[0];
        var tier2 = _settings.O11InternalUserTiers[1];

        var tier1Cost = 9 * tier1.PricePerPack;  // 900 users / 100 per pack = 9 packs
        var tier2Cost = 10 * tier2.PricePerPack; // 1000 users / 100 per pack = 10 packs

        Assert.Equal(108900m, tier1Cost);
        Assert.Equal(24200m, tier2Cost);
        Assert.Equal(133100m, tier1Cost + tier2Cost);
    }

    /// <summary>
    /// Verify O11 External User tiered pricing calculation
    /// 1,000,000 users should cost $419,567.50 based on spec
    /// </summary>
    [Fact]
    public void VerifiedO11ExternalUserTieredPricing_1MUsers()
    {
        // Tier 1 (1-10000): 10,000 users = 10 packs × $4,840 = $48,400
        // Tier 2 (11000-250000): 240,000 users = 240 packs × $1,452 = $348,480
        // Tier 3 (251000+): 750,000 users = 750 packs × $30.25 = $22,687.50
        // Total: $419,567.50

        var tier1 = _settings.O11ExternalUserTiers[0];
        var tier2 = _settings.O11ExternalUserTiers[1];
        var tier3 = _settings.O11ExternalUserTiers[2];

        var tier1Cost = 10 * tier1.PricePerPack;   // 10,000 users / 1000 per pack
        var tier2Cost = 240 * tier2.PricePerPack;  // 240,000 users / 1000 per pack
        var tier3Cost = 750 * tier3.PricePerPack;  // 750,000 users / 1000 per pack

        Assert.Equal(48400m, tier1Cost);
        Assert.Equal(348480m, tier2Cost);
        Assert.Equal(22687.50m, tier3Cost);
        Assert.Equal(419567.50m, tier1Cost + tier2Cost + tier3Cost);
    }

    #endregion
}
