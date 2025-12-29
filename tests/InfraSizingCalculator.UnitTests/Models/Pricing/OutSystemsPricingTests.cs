using FluentAssertions;
using InfraSizingCalculator.Models.Pricing;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Models.Pricing;

public class OutSystemsPricingSettingsTests
{
    private readonly OutSystemsPricingSettings _settings;

    public OutSystemsPricingSettingsTests()
    {
        _settings = new OutSystemsPricingSettings();
    }

    #region Default Values

    [Fact]
    public void DefaultValues_EditionPricing_IsCorrect()
    {
        _settings.StandardEditionBasePrice.Should().Be(36300m);
        _settings.EnterpriseEditionBasePrice.Should().Be(72600m);
    }

    [Fact]
    public void DefaultValues_AOPackaging_IsCorrect()
    {
        _settings.AOPackSize.Should().Be(150);
        _settings.AdditionalAOPackPrice.Should().Be(18000m);
        _settings.StandardEditionAOsIncluded.Should().Be(150);
        _settings.EnterpriseEditionAOsIncluded.Should().Be(450);
    }

    [Fact]
    public void DefaultValues_UserLicensing_IsCorrect()
    {
        _settings.InternalUserPackSize.Should().Be(100);
        _settings.AdditionalInternalUserPackPrice.Should().Be(6000m);
        _settings.ExternalUserPackSize.Should().Be(10000);
        _settings.ExternalUserPackPricePerYear.Should().Be(12000m);
    }

    [Fact]
    public void DefaultValues_CloudAddOns_IsCorrect()
    {
        _settings.CloudAdditionalProdEnvPrice.Should().Be(12000m);
        _settings.CloudAdditionalNonProdEnvPrice.Should().Be(6000m);
        _settings.CloudHAAddOnPrice.Should().Be(24000m);
        _settings.CloudDRAddOnPrice.Should().Be(18000m);
    }

    [Fact]
    public void DefaultValues_SelfManaged_IsCorrect()
    {
        _settings.SelfManagedBasePrice.Should().Be(48000m);
        _settings.SelfManagedPerEnvironmentPrice.Should().Be(9600m);
        _settings.SelfManagedPerFrontEndPrice.Should().Be(4800m);
    }

    [Fact]
    public void DefaultValues_Support_IsCorrect()
    {
        _settings.PremiumSupportPercent.Should().Be(15m);
        _settings.EliteSupportPercent.Should().Be(25m);
    }

    #endregion

    #region GetEditionBasePrice Tests

    [Fact]
    public void GetEditionBasePrice_Standard_ReturnsStandardPrice()
    {
        _settings.GetEditionBasePrice(OutSystemsEdition.Standard).Should().Be(36300m);
    }

    [Fact]
    public void GetEditionBasePrice_Enterprise_ReturnsEnterprisePrice()
    {
        _settings.GetEditionBasePrice(OutSystemsEdition.Enterprise).Should().Be(72600m);
    }

    [Fact]
    public void GetEditionBasePrice_Invalid_ReturnsStandardAsDefault()
    {
        _settings.GetEditionBasePrice((OutSystemsEdition)99).Should().Be(36300m);
    }

    #endregion

    #region GetIncludedAOs Tests

    [Fact]
    public void GetIncludedAOs_Standard_Returns150()
    {
        _settings.GetIncludedAOs(OutSystemsEdition.Standard).Should().Be(150);
    }

    [Fact]
    public void GetIncludedAOs_Enterprise_Returns450()
    {
        _settings.GetIncludedAOs(OutSystemsEdition.Enterprise).Should().Be(450);
    }

    #endregion

    #region GetIncludedInternalUsers Tests

    [Fact]
    public void GetIncludedInternalUsers_Standard_Returns100()
    {
        _settings.GetIncludedInternalUsers(OutSystemsEdition.Standard).Should().Be(100);
    }

    [Fact]
    public void GetIncludedInternalUsers_Enterprise_Returns500()
    {
        _settings.GetIncludedInternalUsers(OutSystemsEdition.Enterprise).Should().Be(500);
    }

    #endregion

    #region CalculateAdditionalAOsCost Tests

    [Fact]
    public void CalculateAdditionalAOsCost_BelowIncluded_ReturnsZero()
    {
        _settings.CalculateAdditionalAOsCost(OutSystemsEdition.Standard, 100).Should().Be(0);
    }

    [Fact]
    public void CalculateAdditionalAOsCost_EqualToIncluded_ReturnsZero()
    {
        _settings.CalculateAdditionalAOsCost(OutSystemsEdition.Standard, 150).Should().Be(0);
    }

    [Fact]
    public void CalculateAdditionalAOsCost_OnePack_ReturnsOnePackPrice()
    {
        // Standard includes 150, so 151-300 needs 1 additional pack
        _settings.CalculateAdditionalAOsCost(OutSystemsEdition.Standard, 200).Should().Be(18000m);
    }

    [Fact]
    public void CalculateAdditionalAOsCost_TwoPacks_ReturnsTwoPacksPrice()
    {
        // Standard includes 150, so 301-450 needs 2 additional packs
        _settings.CalculateAdditionalAOsCost(OutSystemsEdition.Standard, 400).Should().Be(36000m);
    }

    [Fact]
    public void CalculateAdditionalAOsCost_Enterprise_UsesHigherBaseIncluded()
    {
        // Enterprise includes 450, so 450 needs no additional
        _settings.CalculateAdditionalAOsCost(OutSystemsEdition.Enterprise, 450).Should().Be(0);

        // 451+ needs 1 pack
        _settings.CalculateAdditionalAOsCost(OutSystemsEdition.Enterprise, 500).Should().Be(18000m);
    }

    #endregion

    #region CalculateAdditionalInternalUsersCost Tests

    [Fact]
    public void CalculateAdditionalInternalUsersCost_BelowIncluded_ReturnsZero()
    {
        _settings.CalculateAdditionalInternalUsersCost(OutSystemsEdition.Standard, 50).Should().Be(0);
    }

    [Fact]
    public void CalculateAdditionalInternalUsersCost_EqualToIncluded_ReturnsZero()
    {
        _settings.CalculateAdditionalInternalUsersCost(OutSystemsEdition.Standard, 100).Should().Be(0);
    }

    [Fact]
    public void CalculateAdditionalInternalUsersCost_OnePack_ReturnsOnePackPrice()
    {
        // Standard includes 100, so 101-200 needs 1 pack
        _settings.CalculateAdditionalInternalUsersCost(OutSystemsEdition.Standard, 150).Should().Be(6000m);
    }

    [Fact]
    public void CalculateAdditionalInternalUsersCost_MultiplePacks_CalculatesCorrectly()
    {
        // Standard includes 100, so 250 users needs ceiling((250-100)/100) = 2 packs
        _settings.CalculateAdditionalInternalUsersCost(OutSystemsEdition.Standard, 250).Should().Be(12000m);
    }

    #endregion

    #region CalculateExternalUsersCost Tests

    [Fact]
    public void CalculateExternalUsersCost_Zero_ReturnsZero()
    {
        _settings.CalculateExternalUsersCost(0).Should().Be(0);
    }

    [Fact]
    public void CalculateExternalUsersCost_Negative_ReturnsZero()
    {
        _settings.CalculateExternalUsersCost(-100).Should().Be(0);
    }

    [Fact]
    public void CalculateExternalUsersCost_OnePack_ReturnsOnePackPrice()
    {
        _settings.CalculateExternalUsersCost(5000).Should().Be(12000m);
    }

    [Fact]
    public void CalculateExternalUsersCost_TwoPacks_ReturnsTwoPacksPrice()
    {
        _settings.CalculateExternalUsersCost(15000).Should().Be(24000m);
    }

    [Fact]
    public void CalculateExternalUsersCost_ExactPackSize_ReturnsOnePackPrice()
    {
        _settings.CalculateExternalUsersCost(10000).Should().Be(12000m);
    }

    #endregion

    #region CalculateSupportCost Tests

    [Fact]
    public void CalculateSupportCost_Standard_ReturnsZero()
    {
        _settings.CalculateSupportCost(OutSystemsSupportTier.Standard, 100000m).Should().Be(0);
    }

    [Fact]
    public void CalculateSupportCost_Premium_Returns15Percent()
    {
        _settings.CalculateSupportCost(OutSystemsSupportTier.Premium, 100000m).Should().Be(15000m);
    }

    [Fact]
    public void CalculateSupportCost_Elite_Returns25Percent()
    {
        _settings.CalculateSupportCost(OutSystemsSupportTier.Elite, 100000m).Should().Be(25000m);
    }

    [Fact]
    public void CalculateSupportCost_Invalid_ReturnsZero()
    {
        _settings.CalculateSupportCost((OutSystemsSupportTier)99, 100000m).Should().Be(0);
    }

    #endregion
}

public class OutSystemsDeploymentConfigTests
{
    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var config = new OutSystemsDeploymentConfig();

        config.Edition.Should().Be(OutSystemsEdition.Standard);
        config.DeploymentType.Should().Be(OutSystemsDeploymentType.SelfManaged);
        config.TotalApplicationObjects.Should().Be(20);
        config.ProductionEnvironments.Should().Be(1);
        config.NonProductionEnvironments.Should().Be(3);
        config.FrontEndServers.Should().Be(2);
        config.IncludeHA.Should().BeFalse();
        config.IncludeDR.Should().BeFalse();
        config.UserLicenseType.Should().Be(OutSystemsUserLicenseType.Named);
        config.NamedUsers.Should().Be(100);
        config.ConcurrentUsers.Should().Be(0);
        config.ExternalSessions.Should().Be(0);
        config.SupportTier.Should().Be(OutSystemsSupportTier.Standard);
    }

    [Fact]
    public void CanSetAllProperties()
    {
        var config = new OutSystemsDeploymentConfig
        {
            Edition = OutSystemsEdition.Enterprise,
            DeploymentType = OutSystemsDeploymentType.Cloud,
            TotalApplicationObjects = 500,
            ProductionEnvironments = 2,
            NonProductionEnvironments = 5,
            FrontEndServers = 4,
            IncludeHA = true,
            IncludeDR = true,
            UserLicenseType = OutSystemsUserLicenseType.Concurrent,
            NamedUsers = 0,
            ConcurrentUsers = 50,
            ExternalSessions = 100000,
            SupportTier = OutSystemsSupportTier.Elite
        };

        config.Edition.Should().Be(OutSystemsEdition.Enterprise);
        config.DeploymentType.Should().Be(OutSystemsDeploymentType.Cloud);
        config.TotalApplicationObjects.Should().Be(500);
        config.ProductionEnvironments.Should().Be(2);
        config.NonProductionEnvironments.Should().Be(5);
        config.FrontEndServers.Should().Be(4);
        config.IncludeHA.Should().BeTrue();
        config.IncludeDR.Should().BeTrue();
        config.UserLicenseType.Should().Be(OutSystemsUserLicenseType.Concurrent);
        config.ConcurrentUsers.Should().Be(50);
        config.ExternalSessions.Should().Be(100000);
        config.SupportTier.Should().Be(OutSystemsSupportTier.Elite);
    }
}

public class OutSystemsPricingResultTests
{
    [Fact]
    public void TotalPerYear_CalculatesCorrectly()
    {
        var result = new OutSystemsPricingResult
        {
            EditionBaseCost = 36300m,
            AdditionalAOsCost = 18000m,
            EnvironmentCost = 6000m,
            FrontEndCost = 9600m,
            HACost = 0m,
            DRCost = 0m,
            UserLicenseCost = 12000m,
            SupportCost = 5445m
        };

        result.TotalPerYear.Should().Be(87345m);
    }

    [Fact]
    public void TotalPerMonth_IsYearlyDividedBy12()
    {
        var result = new OutSystemsPricingResult
        {
            EditionBaseCost = 36000m
        };

        result.TotalPerMonth.Should().Be(3000m);
    }

    [Fact]
    public void TotalThreeYear_IsYearlyTimes3()
    {
        var result = new OutSystemsPricingResult
        {
            EditionBaseCost = 36000m
        };

        result.TotalThreeYear.Should().Be(108000m);
    }

    [Fact]
    public void DefaultValues_AreCorrect()
    {
        var result = new OutSystemsPricingResult();

        result.DeploymentTypeName.Should().BeEmpty();
        result.TotalAOs.Should().Be(0);
        result.IncludedAOs.Should().Be(0);
        result.AdditionalAOs.Should().Be(0);
        result.EnvironmentDetails.Should().BeNull();
        result.UserLicenseDetails.Should().BeNull();
    }
}

public class OutSystemsEnumTests
{
    [Theory]
    [InlineData(OutSystemsEdition.Standard, 0)]
    [InlineData(OutSystemsEdition.Enterprise, 1)]
    public void OutSystemsEdition_HasCorrectValues(OutSystemsEdition edition, int expectedValue)
    {
        ((int)edition).Should().Be(expectedValue);
    }

    [Theory]
    [InlineData(OutSystemsDeploymentType.Cloud, 0)]
    [InlineData(OutSystemsDeploymentType.SelfManaged, 1)]
    public void OutSystemsDeploymentType_HasCorrectValues(OutSystemsDeploymentType type, int expectedValue)
    {
        ((int)type).Should().Be(expectedValue);
    }

    [Theory]
    [InlineData(OutSystemsUserLicenseType.Named, 0)]
    [InlineData(OutSystemsUserLicenseType.Concurrent, 1)]
    [InlineData(OutSystemsUserLicenseType.External, 2)]
    public void OutSystemsUserLicenseType_HasCorrectValues(OutSystemsUserLicenseType type, int expectedValue)
    {
        ((int)type).Should().Be(expectedValue);
    }

    [Theory]
    [InlineData(OutSystemsSupportTier.Standard, 0)]
    [InlineData(OutSystemsSupportTier.Premium, 1)]
    [InlineData(OutSystemsSupportTier.Elite, 2)]
    public void OutSystemsSupportTier_HasCorrectValues(OutSystemsSupportTier tier, int expectedValue)
    {
        ((int)tier).Should().Be(expectedValue);
    }
}
