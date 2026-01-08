using InfraSizingCalculator.Models.Pricing;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Models.Pricing;

/// <summary>
/// Tests for OutSystems pricing models.
/// Tests discount calculations, AO pack calculations, AppShield tiers,
/// feature availability, validation warnings, and result calculations.
/// </summary>
public class OutSystemsPricingTests
{
    #region OutSystemsDiscount Tests

    [Fact]
    public void CalculateDiscount_PercentageOnTotal_CalculatesCorrectly()
    {
        // Arrange
        var discount = new OutSystemsDiscount
        {
            Type = OutSystemsDiscountType.Percentage,
            Scope = OutSystemsDiscountScope.Total,
            Value = 10
        };

        // Act - 10% of (1000 + 500 + 300) = 10% of 1800 = 180
        var result = discount.CalculateDiscount(1000m, 500m, 300m);

        // Assert
        Assert.Equal(180m, result);
    }

    [Fact]
    public void CalculateDiscount_PercentageOnLicenseOnly_CalculatesCorrectly()
    {
        // Arrange
        var discount = new OutSystemsDiscount
        {
            Type = OutSystemsDiscountType.Percentage,
            Scope = OutSystemsDiscountScope.LicenseOnly,
            Value = 20
        };

        // Act - 20% of 1000 = 200
        var result = discount.CalculateDiscount(1000m, 500m, 300m);

        // Assert
        Assert.Equal(200m, result);
    }

    [Fact]
    public void CalculateDiscount_PercentageOnAddOnsOnly_CalculatesCorrectly()
    {
        // Arrange
        var discount = new OutSystemsDiscount
        {
            Type = OutSystemsDiscountType.Percentage,
            Scope = OutSystemsDiscountScope.AddOnsOnly,
            Value = 15
        };

        // Act - 15% of 500 = 75
        var result = discount.CalculateDiscount(1000m, 500m, 300m);

        // Assert
        Assert.Equal(75m, result);
    }

    [Fact]
    public void CalculateDiscount_PercentageOnServicesOnly_CalculatesCorrectly()
    {
        // Arrange
        var discount = new OutSystemsDiscount
        {
            Type = OutSystemsDiscountType.Percentage,
            Scope = OutSystemsDiscountScope.ServicesOnly,
            Value = 25
        };

        // Act - 25% of 300 = 75
        var result = discount.CalculateDiscount(1000m, 500m, 300m);

        // Assert
        Assert.Equal(75m, result);
    }

    [Fact]
    public void CalculateDiscount_FixedAmountOnTotal_CalculatesCorrectly()
    {
        // Arrange
        var discount = new OutSystemsDiscount
        {
            Type = OutSystemsDiscountType.FixedAmount,
            Scope = OutSystemsDiscountScope.Total,
            Value = 500
        };

        // Act
        var result = discount.CalculateDiscount(1000m, 500m, 300m);

        // Assert
        Assert.Equal(500m, result);
    }

    [Fact]
    public void CalculateDiscount_FixedAmountExceedsTotal_CapsAtTotal()
    {
        // Arrange
        var discount = new OutSystemsDiscount
        {
            Type = OutSystemsDiscountType.FixedAmount,
            Scope = OutSystemsDiscountScope.LicenseOnly,
            Value = 2000 // More than license subtotal of 1000
        };

        // Act
        var result = discount.CalculateDiscount(1000m, 500m, 300m);

        // Assert - Should cap at 1000 (license only)
        Assert.Equal(1000m, result);
    }

    [Fact]
    public void CalculateDiscount_ZeroValue_ReturnsZero()
    {
        // Arrange
        var discount = new OutSystemsDiscount
        {
            Type = OutSystemsDiscountType.Percentage,
            Scope = OutSystemsDiscountScope.Total,
            Value = 0
        };

        // Act
        var result = discount.CalculateDiscount(1000m, 500m, 300m);

        // Assert
        Assert.Equal(0m, result);
    }

    #endregion

    #region OutSystemsPricingSettings.CalculateAOPacks Tests

    [Theory]
    [InlineData(1, 1)]      // 1 AO = 1 pack (minimum)
    [InlineData(150, 1)]    // 150 AOs = 1 pack (exact)
    [InlineData(151, 2)]    // 151 AOs = 2 packs
    [InlineData(300, 2)]    // 300 AOs = 2 packs (exact)
    [InlineData(301, 3)]    // 301 AOs = 3 packs
    [InlineData(1000, 7)]   // 1000 AOs = 7 packs (ceiling of 6.67)
    public void CalculateAOPacks_ReturnsCorrectPackCount(int totalAOs, int expectedPacks)
    {
        // Arrange
        var settings = new OutSystemsPricingSettings();

        // Act
        var result = settings.CalculateAOPacks(totalAOs);

        // Assert
        Assert.Equal(expectedPacks, result);
    }

    [Fact]
    public void CalculateAOPacks_ZeroAOs_ReturnsMinimumOnePack()
    {
        // Arrange
        var settings = new OutSystemsPricingSettings();

        // Act
        var result = settings.CalculateAOPacks(0);

        // Assert
        Assert.Equal(1, result);
    }

    #endregion

    #region OutSystemsPricingSettings.GetAppShieldPrice Tests

    [Theory]
    [InlineData(5000, 18150)]      // Tier 1: 0-10,000
    [InlineData(10000, 18150)]     // Tier 1 boundary
    [InlineData(10001, 32670)]     // Tier 2: 10,001-50,000
    [InlineData(50000, 32670)]     // Tier 2 boundary
    [InlineData(50001, 54450)]     // Tier 3: 50,001-100,000
    [InlineData(100001, 96800)]    // Tier 4: 100,001-500,000
    [InlineData(1000000, 234740)]  // Tier 5: 500,001-1,000,000
    public void GetAppShieldPrice_ReturnsCorrectTierPrice(int userVolume, decimal expectedPrice)
    {
        // Arrange
        var settings = new OutSystemsPricingSettings();

        // Act
        var result = settings.GetAppShieldPrice(userVolume);

        // Assert
        Assert.Equal(expectedPrice, result);
    }

    [Fact]
    public void GetAppShieldPrice_ExceedsMaxTier_ReturnsLastTierPrice()
    {
        // Arrange
        var settings = new OutSystemsPricingSettings();

        // Act - Volume beyond max tier (15M)
        var result = settings.GetAppShieldPrice(20000000);

        // Assert - Should return last tier price
        Assert.Equal(1476200m, result);
    }

    #endregion

    #region OutSystemsPricingSettings.GetServicesPricing Tests

    [Fact]
    public void GetServicesPricing_Africa_ReturnsCorrectPricing()
    {
        // Arrange
        var settings = new OutSystemsPricingSettings();

        // Act
        var result = settings.GetServicesPricing(OutSystemsRegion.Africa);

        // Assert
        Assert.Equal(30250m, result.EssentialSuccessPlan);
        Assert.Equal(60500m, result.PremierSuccessPlan);
        Assert.Equal(2670m, result.DedicatedGroupSession);
        Assert.Equal(480m, result.PublicSession);
        Assert.Equal(1400m, result.ExpertDay);
    }

    [Fact]
    public void GetServicesPricing_MiddleEast_ReturnsCorrectPricing()
    {
        // Arrange
        var settings = new OutSystemsPricingSettings();

        // Act
        var result = settings.GetServicesPricing(OutSystemsRegion.MiddleEast);

        // Assert
        Assert.Equal(30250m, result.EssentialSuccessPlan);
        Assert.Equal(3820m, result.DedicatedGroupSession);
        Assert.Equal(720m, result.PublicSession);
        Assert.Equal(2130m, result.ExpertDay);
    }

    [Theory]
    [InlineData(OutSystemsRegion.Africa)]
    [InlineData(OutSystemsRegion.MiddleEast)]
    [InlineData(OutSystemsRegion.Americas)]
    [InlineData(OutSystemsRegion.Europe)]
    [InlineData(OutSystemsRegion.AsiaPacific)]
    public void GetServicesPricing_AllRegions_ReturnsPricing(OutSystemsRegion region)
    {
        // Arrange
        var settings = new OutSystemsPricingSettings();

        // Act
        var result = settings.GetServicesPricing(region);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.EssentialSuccessPlan > 0);
        Assert.True(result.PremierSuccessPlan > 0);
    }

    #endregion

    #region OutSystemsPricingSettings.IsFeatureAvailable Tests

    [Theory]
    [InlineData("HighAvailability", OutSystemsDeployment.SelfManaged, false)]
    [InlineData("HighAvailability", OutSystemsDeployment.Cloud, true)]
    [InlineData("Sentry", OutSystemsDeployment.SelfManaged, false)]
    [InlineData("Sentry", OutSystemsDeployment.Cloud, true)]
    [InlineData("LogStreaming", OutSystemsDeployment.SelfManaged, false)]
    [InlineData("LogStreaming", OutSystemsDeployment.Cloud, true)]
    [InlineData("LoadTestEnv", OutSystemsDeployment.SelfManaged, false)]
    [InlineData("LoadTestEnv", OutSystemsDeployment.Cloud, true)]
    [InlineData("DatabaseReplica", OutSystemsDeployment.SelfManaged, false)]
    [InlineData("DatabaseReplica", OutSystemsDeployment.Cloud, true)]
    public void IsFeatureAvailable_CloudOnlyFeatures_ReturnsCorrectAvailability(
        string feature, OutSystemsDeployment deployment, bool expected)
    {
        // Act
        var result = OutSystemsPricingSettings.IsFeatureAvailable(feature, deployment);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("DisasterRecovery", OutSystemsDeployment.SelfManaged, true)]
    [InlineData("DisasterRecovery", OutSystemsDeployment.Cloud, false)]
    public void IsFeatureAvailable_SelfManagedOnlyFeatures_ReturnsCorrectAvailability(
        string feature, OutSystemsDeployment deployment, bool expected)
    {
        // Act
        var result = OutSystemsPricingSettings.IsFeatureAvailable(feature, deployment);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Support24x7Premium", OutSystemsDeployment.Cloud, true)]
    [InlineData("Support24x7Premium", OutSystemsDeployment.SelfManaged, true)]
    [InlineData("NonProdEnv", OutSystemsDeployment.Cloud, true)]
    [InlineData("NonProdEnv", OutSystemsDeployment.SelfManaged, true)]
    public void IsFeatureAvailable_CommonFeatures_AvailableForBoth(
        string feature, OutSystemsDeployment deployment, bool expected)
    {
        // Act
        var result = OutSystemsPricingSettings.IsFeatureAvailable(feature, deployment);

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion

    #region OutSystemsDeploymentConfig.AOPacks Tests

    [Theory]
    [InlineData(150, 1)]
    [InlineData(151, 2)]
    [InlineData(300, 2)]
    [InlineData(450, 3)]
    public void DeploymentConfig_AOPacks_CalculatesCorrectly(int totalAOs, int expectedPacks)
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            TotalApplicationObjects = totalAOs
        };

        // Act
        var result = config.AOPacks;

        // Assert
        Assert.Equal(expectedPacks, result);
    }

    #endregion

    #region OutSystemsDeploymentConfig.GetValidationWarnings Tests

    [Fact]
    public void GetValidationWarnings_SelfManagedWithCloudOnlyFeatures_ReturnsWarnings()
    {
        // Arrange
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

        // Act
        var warnings = config.GetValidationWarnings();

        // Assert - 5 cloud-only warnings + 1 "Sentry includes HA" warning
        Assert.Equal(6, warnings.Count);
        Assert.Contains(warnings, w => w.Contains("High Availability") && w.Contains("Cloud-only"));
        Assert.Contains(warnings, w => w.Contains("Sentry") && w.Contains("Cloud-only"));
        Assert.Contains(warnings, w => w.Contains("Log Streaming"));
        Assert.Contains(warnings, w => w.Contains("Load Test"));
        Assert.Contains(warnings, w => w.Contains("Database Replica"));
        Assert.Contains(warnings, w => w.Contains("Sentry includes High Availability"));
    }

    [Fact]
    public void GetValidationWarnings_CloudWithDisasterRecovery_ReturnsWarning()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.Cloud,
            O11DisasterRecovery = true
        };

        // Act
        var warnings = config.GetValidationWarnings();

        // Assert
        Assert.Single(warnings);
        Assert.Contains("Disaster Recovery", warnings[0]);
    }

    [Fact]
    public void GetValidationWarnings_SentryWithHA_ReturnsIncludedWarning()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.Cloud,
            O11Sentry = true,
            O11HighAvailability = true
        };

        // Act
        var warnings = config.GetValidationWarnings();

        // Assert
        Assert.Contains(warnings, w => w.Contains("Sentry includes High Availability"));
    }

    [Fact]
    public void GetValidationWarnings_BothOdcSupportTypes_ReturnsMutuallyExclusiveWarning()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.ODC,
            OdcSupport24x7Extended = true,
            OdcSupport24x7Premium = true
        };

        // Act
        var warnings = config.GetValidationWarnings();

        // Assert
        Assert.Contains(warnings, w => w.Contains("mutually exclusive"));
    }

    [Fact]
    public void GetValidationWarnings_UnlimitedUsersWithAppShieldNoVolume_ReturnsWarning()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            UseUnlimitedUsers = true,
            O11AppShield = true,
            AppShieldUserVolume = null
        };

        // Act
        var warnings = config.GetValidationWarnings();

        // Assert
        Assert.Contains(warnings, w => w.Contains("AppShield requires expected user volume"));
    }

    [Fact]
    public void GetValidationWarnings_ValidConfiguration_ReturnsNoWarnings()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.Cloud,
            TotalApplicationObjects = 150,
            InternalUsers = 100
        };

        // Act
        var warnings = config.GetValidationWarnings();

        // Assert
        Assert.Empty(warnings);
    }

    #endregion

    #region OutSystemsPricingResult Calculated Properties Tests

    [Fact]
    public void PricingResult_LicenseSubtotal_SumsCorrectly()
    {
        // Arrange
        var result = new OutSystemsPricingResult
        {
            EditionCost = 36300m,
            AOPacksCost = 36300m,
            InternalUsersCost = 12100m,
            ExternalUsersCost = 4840m,
            UnlimitedUsersCost = 0m
        };

        // Act
        var subtotal = result.LicenseSubtotal;

        // Assert
        Assert.Equal(89540m, subtotal);
    }

    [Fact]
    public void PricingResult_AddOnsSubtotal_SumsCorrectly()
    {
        // Arrange
        var result = new OutSystemsPricingResult
        {
            AddOnCosts = new Dictionary<string, decimal>
            {
                { "HighAvailability", 12100m },
                { "Support24x7Premium", 3630m },
                { "NonProdEnv", 7260m }
            }
        };

        // Act
        var subtotal = result.AddOnsSubtotal;

        // Assert
        Assert.Equal(22990m, subtotal);
    }

    [Fact]
    public void PricingResult_ServicesSubtotal_SumsCorrectly()
    {
        // Arrange
        var result = new OutSystemsPricingResult
        {
            ServiceCosts = new Dictionary<string, decimal>
            {
                { "EssentialSuccessPlan", 30250m },
                { "ExpertDays", 5600m }
            }
        };

        // Act
        var subtotal = result.ServicesSubtotal;

        // Assert
        Assert.Equal(35850m, subtotal);
    }

    [Fact]
    public void PricingResult_AnnualVMCost_CalculatesFromMonthly()
    {
        // Arrange
        var result = new OutSystemsPricingResult
        {
            MonthlyVMCost = 1000m
        };

        // Act
        var annualCost = result.AnnualVMCost;

        // Assert
        Assert.Equal(12000m, annualCost);
    }

    [Fact]
    public void PricingResult_GrossTotal_SumsAllSubtotals()
    {
        // Arrange
        var result = new OutSystemsPricingResult
        {
            EditionCost = 36300m,
            AddOnCosts = new Dictionary<string, decimal> { { "HA", 12100m } },
            ServiceCosts = new Dictionary<string, decimal> { { "ESP", 30250m } },
            MonthlyVMCost = 500m
        };

        // Act
        var grossTotal = result.GrossTotal;

        // Assert - 36300 + 12100 + 30250 + (500*12) = 84650
        Assert.Equal(84650m, grossTotal);
    }

    [Fact]
    public void PricingResult_NetTotal_AppliesDiscount()
    {
        // Arrange
        var result = new OutSystemsPricingResult
        {
            EditionCost = 100000m,
            DiscountAmount = 10000m
        };

        // Act
        var netTotal = result.NetTotal;

        // Assert
        Assert.Equal(90000m, netTotal);
    }

    [Fact]
    public void PricingResult_TotalPerMonth_CalculatesCorrectly()
    {
        // Arrange
        var result = new OutSystemsPricingResult
        {
            EditionCost = 120000m // $120,000/year
        };

        // Act
        var perMonth = result.TotalPerMonth;

        // Assert
        Assert.Equal(10000m, perMonth);
    }

    [Fact]
    public void PricingResult_MultiYearProjections_CalculateCorrectly()
    {
        // Arrange
        var result = new OutSystemsPricingResult
        {
            EditionCost = 100000m
        };

        // Act & Assert
        Assert.Equal(300000m, result.TotalThreeYear);
        Assert.Equal(500000m, result.TotalFiveYear);
    }

    #endregion

    #region Pricing Settings Default Values Tests

    [Fact]
    public void PricingSettings_OdcDefaults_MatchVerifiedPricing()
    {
        // Arrange
        var settings = new OutSystemsPricingSettings();

        // Assert - Verified ODC pricing
        Assert.Equal(30250m, settings.OdcPlatformBasePrice);
        Assert.Equal(18150m, settings.OdcAOPackPrice);
        Assert.Equal(6050m, settings.OdcInternalUserPackPrice);
        Assert.Equal(6050m, settings.OdcExternalUserPackPrice);
    }

    [Fact]
    public void PricingSettings_O11Defaults_MatchVerifiedPricing()
    {
        // Arrange
        var settings = new OutSystemsPricingSettings();

        // Assert - Verified O11 pricing
        Assert.Equal(36300m, settings.O11EnterpriseBasePrice);
        Assert.Equal(36300m, settings.O11AOPackPrice);
    }

    [Fact]
    public void PricingSettings_UnlimitedUsersPerAOPack_MatchesVerifiedPricing()
    {
        // Arrange
        var settings = new OutSystemsPricingSettings();

        // Assert
        Assert.Equal(60500m, settings.UnlimitedUsersPerAOPack);
    }

    [Fact]
    public void PricingSettings_CloudVMPricing_ContainsAllInstanceTypes()
    {
        // Arrange
        var settings = new OutSystemsPricingSettings();

        // Assert - Azure
        Assert.Equal(4, settings.AzureVMHourlyPricing.Count);
        Assert.Contains(OutSystemsAzureInstanceType.F4s_v2, settings.AzureVMHourlyPricing.Keys);
        Assert.Contains(OutSystemsAzureInstanceType.D16s_v3, settings.AzureVMHourlyPricing.Keys);

        // Assert - AWS
        Assert.Equal(3, settings.AwsEC2HourlyPricing.Count);
        Assert.Contains(OutSystemsAwsInstanceType.M5Large, settings.AwsEC2HourlyPricing.Keys);
        Assert.Contains(OutSystemsAwsInstanceType.M52XLarge, settings.AwsEC2HourlyPricing.Keys);
    }

    [Fact]
    public void PricingSettings_PackSizes_AreCorrect()
    {
        // Arrange
        var settings = new OutSystemsPricingSettings();

        // Assert
        Assert.Equal(150, settings.AOPackSize);
        Assert.Equal(100, settings.InternalUserPackSize);
        Assert.Equal(1000, settings.ExternalUserPackSize);
        Assert.Equal(730, settings.HoursPerMonth);
    }

    #endregion

    #region User Tier Tests

    [Fact]
    public void O11InternalUserTiers_HasCorrectTierCount()
    {
        // Arrange
        var settings = new OutSystemsPricingSettings();

        // Assert
        Assert.Equal(3, settings.O11InternalUserTiers.Count);
    }

    [Fact]
    public void O11ExternalUserTiers_HasCorrectTierCount()
    {
        // Arrange
        var settings = new OutSystemsPricingSettings();

        // Assert
        Assert.Equal(3, settings.O11ExternalUserTiers.Count);
    }

    [Fact]
    public void AppShieldTiers_HasCorrectTierCount()
    {
        // Arrange
        var settings = new OutSystemsPricingSettings();

        // Assert
        Assert.Equal(19, settings.AppShieldTiers.Count);
    }

    #endregion
}
