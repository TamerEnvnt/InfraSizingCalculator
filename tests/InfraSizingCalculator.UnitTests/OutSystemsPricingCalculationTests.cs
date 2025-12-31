using InfraSizingCalculator.Models.Pricing;

namespace InfraSizingCalculator.UnitTests;

/// <summary>
/// Tests for OutSystems pricing calculations
/// Based on OutSystems Partner Price Calculator (2024/2025)
/// </summary>
public class OutSystemsPricingCalculationTests
{
    private readonly OutSystemsPricingSettings _settings;

    public OutSystemsPricingCalculationTests()
    {
        _settings = new OutSystemsPricingSettings();
    }

    #region Edition Base Pricing

    /// <summary>
    /// Standard and Enterprise editions should have same base price of $36,300/year
    /// </summary>
    [Theory]
    [InlineData(OutSystemsEdition.Standard, 36300)]
    [InlineData(OutSystemsEdition.Enterprise, 36300)]
    public void GetEditionBasePrice_ReturnsCorrectPrice(OutSystemsEdition edition, decimal expectedPrice)
    {
        var price = _settings.GetEditionBasePrice(edition);
        Assert.Equal(expectedPrice, price);
    }

    /// <summary>
    /// Standard edition should include 100 internal users
    /// </summary>
    [Fact]
    public void StandardEdition_Includes100InternalUsers()
    {
        Assert.Equal(100, _settings.StandardEditionInternalUsersIncluded);
    }

    /// <summary>
    /// Enterprise edition should include 500 internal users
    /// </summary>
    [Fact]
    public void EnterpriseEdition_Includes500InternalUsers()
    {
        Assert.Equal(500, _settings.EnterpriseEditionInternalUsersIncluded);
    }

    /// <summary>
    /// Both editions include 150 AOs (1 pack)
    /// </summary>
    [Theory]
    [InlineData(OutSystemsEdition.Standard, 150)]
    [InlineData(OutSystemsEdition.Enterprise, 150)]
    public void GetIncludedAOs_ReturnsCorrectCount(OutSystemsEdition edition, int expectedAOs)
    {
        var aos = _settings.GetIncludedAOs(edition);
        Assert.Equal(expectedAOs, aos);
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
    public void CalculateAOPackCount_RoundsUpCorrectly(int totalAOs, int expectedPacks)
    {
        var packs = _settings.CalculateAOPackCount(totalAOs);
        Assert.Equal(expectedPacks, packs);
    }

    /// <summary>
    /// Zero or negative AOs should return minimum 1 pack
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void CalculateAOPackCount_MinimumOnePack(int totalAOs)
    {
        var packs = _settings.CalculateAOPackCount(totalAOs);
        Assert.Equal(1, packs);
    }

    /// <summary>
    /// Additional AO pack price should be $36,300 per pack
    /// </summary>
    [Fact]
    public void AdditionalAOPackPrice_Is36300()
    {
        Assert.Equal(36300m, _settings.AdditionalAOPackPrice);
    }

    /// <summary>
    /// Additional AO cost should only apply when exceeding included AOs
    /// </summary>
    [Theory]
    [InlineData(OutSystemsEdition.Standard, 150, 0)]        // Exactly included
    [InlineData(OutSystemsEdition.Standard, 100, 0)]        // Under included
    [InlineData(OutSystemsEdition.Standard, 300, 36300)]    // 1 additional pack
    [InlineData(OutSystemsEdition.Standard, 450, 72600)]    // 2 additional packs
    [InlineData(OutSystemsEdition.Enterprise, 150, 0)]      // Exactly included
    [InlineData(OutSystemsEdition.Enterprise, 300, 36300)]  // 1 additional pack
    public void CalculateAdditionalAOsCost_CorrectCost(OutSystemsEdition edition, int totalAOs, decimal expectedCost)
    {
        var cost = _settings.CalculateAdditionalAOsCost(edition, totalAOs);
        Assert.Equal(expectedCost, cost);
    }

    #endregion

    #region AO-Pack Scaled Add-on Pricing

    /// <summary>
    /// AO-pack scaled cost formula: perPackRate * aoPackCount
    /// </summary>
    [Theory]
    [InlineData(3630, 150, 3630)]       // 1 pack × $3,630
    [InlineData(3630, 300, 7260)]       // 2 packs × $3,630
    [InlineData(3630, 450, 10890)]      // 3 packs × $3,630 (matches Partner Calculator example)
    [InlineData(12100, 450, 36300)]     // 3 packs × $12,100 (HA example)
    [InlineData(24200, 450, 72600)]     // 3 packs × $24,200 (Sentry example)
    public void CalculateAOPackScaledCost_CorrectCalculation(decimal perPackRate, int totalAOs, decimal expectedCost)
    {
        var cost = _settings.CalculateAOPackScaledCost(perPackRate, totalAOs);
        Assert.Equal(expectedCost, cost);
    }

    /// <summary>
    /// 24x7 Premium Support should be $3,630 per AO pack
    /// </summary>
    [Fact]
    public void Support24x7PremiumPerAOPack_Is3630()
    {
        Assert.Equal(3630m, _settings.Support24x7PremiumPerAOPack);
    }

    /// <summary>
    /// Non-Production Environment add-on should be $3,630 per AO pack
    /// </summary>
    [Fact]
    public void NonProductionEnvPerAOPack_Is3630()
    {
        Assert.Equal(3630m, _settings.NonProductionEnvPerAOPack);
    }

    /// <summary>
    /// Load Testing Environment add-on should be $6,050 per AO pack
    /// </summary>
    [Fact]
    public void LoadTestEnvPerAOPack_Is6050()
    {
        Assert.Equal(6050m, _settings.LoadTestEnvPerAOPack);
    }

    /// <summary>
    /// Environment Pack add-on should be $9,680 per AO pack
    /// </summary>
    [Fact]
    public void EnvironmentPackPerAOPack_Is9680()
    {
        Assert.Equal(9680m, _settings.EnvironmentPackPerAOPack);
    }

    /// <summary>
    /// High Availability add-on should be $12,100 per AO pack
    /// </summary>
    [Fact]
    public void HighAvailabilityPerAOPack_Is12100()
    {
        Assert.Equal(12100m, _settings.HighAvailabilityPerAOPack);
    }

    /// <summary>
    /// Sentry add-on should be $24,200 per AO pack
    /// </summary>
    [Fact]
    public void SentryPerAOPack_Is24200()
    {
        Assert.Equal(24200m, _settings.SentryPerAOPack);
    }

    /// <summary>
    /// Disaster Recovery add-on should be $12,100 per AO pack
    /// </summary>
    [Fact]
    public void DisasterRecoveryPerAOPack_Is12100()
    {
        Assert.Equal(12100m, _settings.DisasterRecoveryPerAOPack);
    }

    #endregion

    #region Flat Fee Add-ons

    /// <summary>
    /// Log Streaming should be $7,260 flat fee
    /// </summary>
    [Fact]
    public void LogStreamingPrice_Is7260()
    {
        Assert.Equal(7260m, _settings.LogStreamingPrice);
    }

    /// <summary>
    /// Database Replica should be $96,800 flat fee
    /// </summary>
    [Fact]
    public void DatabaseReplicaPrice_Is96800()
    {
        Assert.Equal(96800m, _settings.DatabaseReplicaPrice);
    }

    /// <summary>
    /// Unlimited users should be $181,500 flat fee
    /// </summary>
    [Fact]
    public void UnlimitedUsersPrice_Is181500()
    {
        Assert.Equal(181500m, _settings.UnlimitedUsersPrice);
    }

    /// <summary>
    /// AppShield should be approximately $16.50 per user
    /// </summary>
    [Fact]
    public void AppShieldPerUser_Is16_50()
    {
        Assert.Equal(16.50m, _settings.AppShieldPerUser);
    }

    #endregion

    #region User Licensing

    /// <summary>
    /// External user pack size should be 1,000 users
    /// </summary>
    [Fact]
    public void ExternalUserPackSize_Is1000()
    {
        Assert.Equal(1000, _settings.ExternalUserPackSize);
    }

    /// <summary>
    /// External user pack price should be $4,840 per 1,000 users
    /// </summary>
    [Fact]
    public void ExternalUserPackPrice_Is4840()
    {
        Assert.Equal(4840m, _settings.ExternalUserPackPricePerYear);
    }

    /// <summary>
    /// External user cost calculation should round up to pack size
    /// </summary>
    [Theory]
    [InlineData(0, 0)]            // No users
    [InlineData(1, 4840)]         // 1 user = 1 pack
    [InlineData(1000, 4840)]      // Exactly 1 pack
    [InlineData(1001, 9680)]      // Just over 1 pack = 2 packs
    [InlineData(2500, 14520)]     // 3 packs
    [InlineData(10000, 48400)]    // 10 packs
    public void CalculateExternalUsersCost_RoundsUpCorrectly(int users, decimal expectedCost)
    {
        var cost = _settings.CalculateExternalUsersCost(users);
        Assert.Equal(expectedCost, cost);
    }

    /// <summary>
    /// Internal user pack size should be 100 users
    /// </summary>
    [Fact]
    public void InternalUserPackSize_Is100()
    {
        Assert.Equal(100, _settings.InternalUserPackSize);
    }

    /// <summary>
    /// Additional internal user pack price should be $6,000 per 100 users
    /// </summary>
    [Fact]
    public void AdditionalInternalUserPackPrice_Is6000()
    {
        Assert.Equal(6000m, _settings.AdditionalInternalUserPackPrice);
    }

    #endregion

    #region Success Plan and Services

    /// <summary>
    /// Essential Success Plan should be $30,250 per year
    /// </summary>
    [Fact]
    public void EssentialSuccessPlanPrice_Is30250()
    {
        Assert.Equal(30250m, _settings.EssentialSuccessPlanPrice);
    }

    /// <summary>
    /// Premier Success Plan should be $60,500 per year
    /// </summary>
    [Fact]
    public void PremierSuccessPlanPrice_Is60500()
    {
        Assert.Equal(60500m, _settings.PremierSuccessPlanPrice);
    }

    /// <summary>
    /// Success Plan cost calculation
    /// </summary>
    [Theory]
    [InlineData(OutSystemsSuccessPlan.None, 0)]
    [InlineData(OutSystemsSuccessPlan.Essential, 30250)]
    [InlineData(OutSystemsSuccessPlan.Premier, 60500)]
    public void CalculateSuccessPlanCost_ReturnsCorrectPrice(OutSystemsSuccessPlan plan, decimal expectedCost)
    {
        var cost = _settings.CalculateSuccessPlanCost(plan);
        Assert.Equal(expectedCost, cost);
    }

    /// <summary>
    /// Dedicated Group Session should be $3,820 each
    /// </summary>
    [Fact]
    public void DedicatedGroupSessionPrice_Is3820()
    {
        Assert.Equal(3820m, _settings.DedicatedGroupSessionPrice);
    }

    /// <summary>
    /// Public Session should be $720 each
    /// </summary>
    [Fact]
    public void PublicSessionPrice_Is720()
    {
        Assert.Equal(720m, _settings.PublicSessionPrice);
    }

    /// <summary>
    /// Expert Day should be $2,640 each
    /// </summary>
    [Fact]
    public void ExpertDayPrice_Is2640()
    {
        Assert.Equal(2640m, _settings.ExpertDayPrice);
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
    public void CalculateAzureMonthlyVMCost_CorrectCalculation(OutSystemsAzureInstanceType instance, int servers, decimal expectedCost)
    {
        var cost = _settings.CalculateAzureMonthlyVMCost(instance, servers);
        Assert.Equal(expectedCost, cost);
    }

    /// <summary>
    /// AWS monthly VM cost calculation: hourlyRate * 730 * serverCount
    /// </summary>
    [Theory]
    [InlineData(OutSystemsAwsInstanceType.M5Large, 1, 70.08)]       // 0.096 * 730
    [InlineData(OutSystemsAwsInstanceType.M5XLarge, 4, 560.64)]     // 0.192 * 730 * 4
    [InlineData(OutSystemsAwsInstanceType.M52XLarge, 2, 560.64)]    // 0.384 * 730 * 2
    public void CalculateAwsMonthlyVMCost_CorrectCalculation(OutSystemsAwsInstanceType instance, int servers, decimal expectedCost)
    {
        var cost = _settings.CalculateAwsMonthlyVMCost(instance, servers);
        Assert.Equal(expectedCost, cost);
    }

    /// <summary>
    /// Azure instance specs should be correct
    /// </summary>
    [Theory]
    [InlineData(OutSystemsAzureInstanceType.F4s_v2, 4, 8)]
    [InlineData(OutSystemsAzureInstanceType.D4s_v3, 4, 16)]
    [InlineData(OutSystemsAzureInstanceType.D8s_v3, 8, 32)]
    [InlineData(OutSystemsAzureInstanceType.D16s_v3, 16, 64)]
    public void GetAzureInstanceSpecs_ReturnsCorrectSpecs(OutSystemsAzureInstanceType instance, int expectedCpu, int expectedRam)
    {
        var specs = OutSystemsPricingSettings.GetAzureInstanceSpecs(instance);
        Assert.Equal(expectedCpu, specs.vCPU);
        Assert.Equal(expectedRam, specs.RamGB);
    }

    /// <summary>
    /// AWS instance specs should be correct
    /// </summary>
    [Theory]
    [InlineData(OutSystemsAwsInstanceType.M5Large, 2, 8)]
    [InlineData(OutSystemsAwsInstanceType.M5XLarge, 4, 16)]
    [InlineData(OutSystemsAwsInstanceType.M52XLarge, 8, 32)]
    public void GetAwsInstanceSpecs_ReturnsCorrectSpecs(OutSystemsAwsInstanceType instance, int expectedCpu, int expectedRam)
    {
        var specs = OutSystemsPricingSettings.GetAwsInstanceSpecs(instance);
        Assert.Equal(expectedCpu, specs.vCPU);
        Assert.Equal(expectedRam, specs.RamGB);
    }

    #endregion

    #region Cloud-Only Features

    /// <summary>
    /// Cloud-only features should be correctly identified
    /// </summary>
    [Theory]
    [InlineData("HighAvailability", true)]
    [InlineData("Sentry", true)]
    [InlineData("LoadTestEnv", true)]
    [InlineData("LogStreaming", true)]
    [InlineData("DatabaseReplica", true)]
    [InlineData("NonProductionEnv", false)]      // Not cloud-only
    [InlineData("DisasterRecovery", false)]      // Not cloud-only
    [InlineData("Support24x7Premium", false)]    // Not cloud-only
    public void IsCloudOnlyFeature_CorrectlyIdentifies(string feature, bool expectedResult)
    {
        var result = OutSystemsPricingSettings.IsCloudOnlyFeature(feature);
        Assert.Equal(expectedResult, result);
    }

    #endregion

    #region Deployment Config Validation

    /// <summary>
    /// NumberOfAOPacks should be computed correctly from TotalApplicationObjects
    /// </summary>
    [Theory]
    [InlineData(150, 1)]
    [InlineData(300, 2)]
    [InlineData(450, 3)]
    [InlineData(151, 2)]
    public void DeploymentConfig_NumberOfAOPacks_ComputedCorrectly(int aos, int expectedPacks)
    {
        var config = new OutSystemsDeploymentConfig
        {
            TotalApplicationObjects = aos
        };
        Assert.Equal(expectedPacks, config.NumberOfAOPacks);
    }

    /// <summary>
    /// TotalEnvironments should sum production and non-production
    /// </summary>
    [Theory]
    [InlineData(1, 3, 4)]
    [InlineData(2, 2, 4)]
    [InlineData(1, 0, 1)]
    public void DeploymentConfig_TotalEnvironments_SumsCorrectly(int prod, int nonProd, int expected)
    {
        var config = new OutSystemsDeploymentConfig
        {
            ProductionEnvironments = prod,
            NonProductionEnvironments = nonProd
        };
        Assert.Equal(expected, config.TotalEnvironments);
    }

    /// <summary>
    /// Self-managed deployment with cloud-only features should generate warnings
    /// </summary>
    [Fact]
    public void DeploymentConfig_SelfManagedWithCloudFeatures_GeneratesWarnings()
    {
        var config = new OutSystemsDeploymentConfig
        {
            DeploymentType = OutSystemsDeploymentType.SelfManaged,
            IncludeHA = true,
            IncludeSentry = true,
            IncludeLoadTestEnv = true,
            IncludeLogStreaming = true,
            IncludeDatabaseReplica = true
        };

        var warnings = config.GetValidationWarnings();

        // 5 cloud-only warnings + 1 "Sentry includes HA" warning = 6 total
        Assert.Equal(6, warnings.Count);
        Assert.Contains(warnings, w => w.Contains("High Availability"));
        Assert.Contains(warnings, w => w.Contains("Sentry"));
        Assert.Contains(warnings, w => w.Contains("Load Testing"));
        Assert.Contains(warnings, w => w.Contains("Log Streaming"));
        Assert.Contains(warnings, w => w.Contains("Database Replica"));
        Assert.Contains(warnings, w => w.Contains("Sentry already includes"));
    }

    /// <summary>
    /// Cloud deployment with cloud-only features should not generate warnings
    /// </summary>
    [Fact]
    public void DeploymentConfig_CloudWithCloudFeatures_NoWarnings()
    {
        var config = new OutSystemsDeploymentConfig
        {
            DeploymentType = OutSystemsDeploymentType.Cloud,
            IncludeHA = true,
            IncludeLogStreaming = true
        };

        var warnings = config.GetValidationWarnings();

        Assert.Empty(warnings);
    }

    /// <summary>
    /// Selecting both Sentry and HA should generate a warning since Sentry includes HA
    /// </summary>
    [Fact]
    public void DeploymentConfig_SentryWithHA_GeneratesWarning()
    {
        var config = new OutSystemsDeploymentConfig
        {
            DeploymentType = OutSystemsDeploymentType.Cloud,
            IncludeSentry = true,
            IncludeHA = true
        };

        var warnings = config.GetValidationWarnings();

        Assert.Single(warnings);
        Assert.Contains("Sentry already includes High Availability", warnings[0]);
    }

    #endregion

    #region Pricing Result Totals

    /// <summary>
    /// OutSystemsPricingResult should calculate subtotals correctly
    /// </summary>
    [Fact]
    public void PricingResult_LicenseSubtotal_CalculatedCorrectly()
    {
        var result = new OutSystemsPricingResult
        {
            EditionBaseCost = 36300m,
            AdditionalAOsCost = 36300m,
            UserLicenseCost = 4840m
        };

        Assert.Equal(77440m, result.LicenseSubtotal);
    }

    /// <summary>
    /// OutSystemsPricingResult add-ons subtotal should sum all add-on costs
    /// </summary>
    [Fact]
    public void PricingResult_AddOnsSubtotal_CalculatedCorrectly()
    {
        var result = new OutSystemsPricingResult
        {
            Support24x7PremiumCost = 3630m,
            NonProductionEnvCost = 3630m,
            HACost = 12100m,
            DRCost = 12100m
        };

        Assert.Equal(31460m, result.AddOnsSubtotal);
    }

    /// <summary>
    /// OutSystemsPricingResult total per year should sum all subtotals
    /// </summary>
    [Fact]
    public void PricingResult_TotalPerYear_CalculatedCorrectly()
    {
        var result = new OutSystemsPricingResult
        {
            EditionBaseCost = 36300m,
            AdditionalAOsCost = 36300m,
            UserLicenseCost = 4840m,
            Support24x7PremiumCost = 3630m,
            HACost = 12100m,
            MonthlyVMCost = 500m,       // Monthly becomes 6000/year
            SuccessPlanCost = 30250m
        };

        // License: 77440 + AddOns: 15730 + Infra: 6000 + Services: 30250 = 129420
        Assert.Equal(129420m, result.TotalPerYear);
    }

    /// <summary>
    /// OutSystemsPricingResult monthly should be yearly / 12
    /// </summary>
    [Fact]
    public void PricingResult_TotalPerMonth_CalculatedCorrectly()
    {
        var result = new OutSystemsPricingResult
        {
            EditionBaseCost = 36300m  // LicenseSubtotal = 36300
        };

        Assert.Equal(36300m / 12, result.TotalPerMonth);
    }

    /// <summary>
    /// OutSystemsPricingResult 3-year and 5-year totals
    /// </summary>
    [Fact]
    public void PricingResult_MultiYearTotals_CalculatedCorrectly()
    {
        var result = new OutSystemsPricingResult
        {
            EditionBaseCost = 36300m
        };

        Assert.Equal(36300m * 3, result.TotalThreeYear);
        Assert.Equal(36300m * 5, result.TotalFiveYear);
    }

    #endregion

    #region Comprehensive Scenario Tests

    /// <summary>
    /// Validate Partner Calculator example: 3 AO packs with common add-ons
    /// </summary>
    [Fact]
    public void PartnerCalculatorScenario_ThreeAOPacks_CorrectTotals()
    {
        // Scenario: 3 AO packs (450 AOs), Enterprise edition
        // With: 24x7 Premium Support, HA, DR

        var totalAOs = 450;
        var packCount = _settings.CalculateAOPackCount(totalAOs);
        Assert.Equal(3, packCount);

        // Edition base (1 pack included): $36,300
        var editionBase = _settings.GetEditionBasePrice(OutSystemsEdition.Enterprise);
        Assert.Equal(36300m, editionBase);

        // Additional AO packs (2 packs): 2 × $36,300 = $72,600
        var additionalAOs = _settings.CalculateAdditionalAOsCost(OutSystemsEdition.Enterprise, totalAOs);
        Assert.Equal(72600m, additionalAOs);

        // 24x7 Premium Support: 3 × $3,630 = $10,890
        var support = _settings.CalculateAOPackScaledCost(_settings.Support24x7PremiumPerAOPack, totalAOs);
        Assert.Equal(10890m, support);

        // High Availability: 3 × $12,100 = $36,300
        var ha = _settings.CalculateAOPackScaledCost(_settings.HighAvailabilityPerAOPack, totalAOs);
        Assert.Equal(36300m, ha);

        // Disaster Recovery: 3 × $12,100 = $36,300
        var dr = _settings.CalculateAOPackScaledCost(_settings.DisasterRecoveryPerAOPack, totalAOs);
        Assert.Equal(36300m, dr);

        // Total for this scenario: $192,390
        var total = editionBase + additionalAOs + support + ha + dr;
        Assert.Equal(192390m, total);
    }

    /// <summary>
    /// Validate self-managed on Azure scenario
    /// </summary>
    [Fact]
    public void SelfManagedAzureScenario_CorrectVMCosts()
    {
        // Scenario: 4 environments × 2 front-end servers = 8 VMs
        // Using D4s_v3 instance

        var config = new OutSystemsDeploymentConfig
        {
            DeploymentType = OutSystemsDeploymentType.SelfManaged,
            CloudProvider = OutSystemsCloudProvider.Azure,
            AzureInstanceType = OutSystemsAzureInstanceType.D4s_v3,
            FrontEndServersPerEnvironment = 2,
            ProductionEnvironments = 1,
            NonProductionEnvironments = 3
        };

        var totalVMs = config.TotalEnvironments * config.FrontEndServersPerEnvironment;
        Assert.Equal(8, totalVMs);

        // Monthly VM cost: 0.192 × 730 × 8 = $1,121.28
        var monthlyVMCost = _settings.CalculateAzureMonthlyVMCost(config.AzureInstanceType, totalVMs);
        Assert.Equal(1121.28m, monthlyVMCost);

        // Annual VM cost: $13,455.36
        var annualVMCost = monthlyVMCost * 12;
        Assert.Equal(13455.36m, annualVMCost);
    }

    #endregion
}
