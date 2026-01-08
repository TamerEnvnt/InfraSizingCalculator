using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Models.Pricing;
using InfraSizingCalculator.Services.Pricing;
using Microsoft.JSInterop;
using NSubstitute;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Services;

/// <summary>
/// Tests for OutSystems pricing calculations in PricingSettingsService.
/// Covers O11 and ODC platforms, add-ons, user licensing, discounts, and cloud VM costs.
/// </summary>
public class OutSystemsPricingServiceTests
{
    private readonly IJSRuntime _jsRuntime;
    private readonly PricingSettingsService _service;

    public OutSystemsPricingServiceTests()
    {
        _jsRuntime = Substitute.For<IJSRuntime>();
        _service = new PricingSettingsService(_jsRuntime);
    }

    #region O11 License Cost Tests

    [Fact]
    public void CalculateOutSystemsCost_O11_BaseEdition_ReturnsCorrectPrice()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.Cloud,
            TotalApplicationObjects = 150 // 1 AO pack
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        Assert.Equal(OutSystemsPlatform.O11, result.Platform);
        Assert.Equal(1, result.AOPackCount);
        Assert.Equal(36300m, result.EditionCost); // Enterprise base
        Assert.Equal(0m, result.AOPacksCost); // 1 pack included
    }

    [Fact]
    public void CalculateOutSystemsCost_O11_AdditionalAOPacks_CalculatesCorrectly()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.Cloud,
            TotalApplicationObjects = 450 // 3 AO packs (2 additional)
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        Assert.Equal(3, result.AOPackCount);
        Assert.Equal(36300m, result.EditionCost);
        Assert.Equal(72600m, result.AOPacksCost); // 2 × $36,300
    }

    [Fact]
    public void CalculateOutSystemsCost_O11_UnlimitedUsers_ScalesWithAOPacks()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.Cloud,
            TotalApplicationObjects = 300, // 2 AO packs
            UseUnlimitedUsers = true
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        Assert.True(result.UsedUnlimitedUsers);
        Assert.Equal(121000m, result.UnlimitedUsersCost); // 2 × $60,500
    }

    [Fact]
    public void CalculateOutSystemsCost_O11_InternalUsers_IncludesFirst100Free()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.Cloud,
            TotalApplicationObjects = 150,
            InternalUsers = 100 // All included free
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        Assert.Equal(0m, result.InternalUsersCost);
    }

    [Fact]
    public void CalculateOutSystemsCost_O11_AdditionalInternalUsers_UsesTieredPricing()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.Cloud,
            TotalApplicationObjects = 150,
            InternalUsers = 200 // 100 additional users
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        Assert.True(result.InternalUsersCost > 0);
        Assert.True(result.InternalUserPackCount > 0);
    }

    [Fact]
    public void CalculateOutSystemsCost_O11_ExternalUsers_UsesTieredPricing()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.Cloud,
            TotalApplicationObjects = 150,
            ExternalUsers = 5000
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        Assert.True(result.ExternalUsersCost > 0);
        Assert.True(result.ExternalUserPackCount > 0);
    }

    #endregion

    #region ODC License Cost Tests

    [Fact]
    public void CalculateOutSystemsCost_ODC_BasePrice_ReturnsCorrectAmount()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.ODC,
            TotalApplicationObjects = 150
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        Assert.Equal(OutSystemsPlatform.ODC, result.Platform);
        Assert.Equal(30250m, result.EditionCost); // ODC base price
    }

    [Fact]
    public void CalculateOutSystemsCost_ODC_AdditionalAOPacks_CalculatesCorrectly()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.ODC,
            TotalApplicationObjects = 450 // 3 AO packs
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        Assert.Equal(3, result.AOPackCount);
        Assert.Equal(36300m, result.AOPacksCost); // 2 × $18,150
    }

    [Fact]
    public void CalculateOutSystemsCost_ODC_UnlimitedUsers_ScalesWithAOPacks()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.ODC,
            TotalApplicationObjects = 300, // 2 AO packs
            UseUnlimitedUsers = true
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        Assert.True(result.UsedUnlimitedUsers);
        Assert.Equal(121000m, result.UnlimitedUsersCost); // 2 × $60,500
    }

    [Fact]
    public void CalculateOutSystemsCost_ODC_InternalUsers_FlatPackPricing()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.ODC,
            TotalApplicationObjects = 150,
            InternalUsers = 250 // 150 additional (1 included + 1 pack)
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        Assert.True(result.InternalUsersCost > 0);
    }

    [Fact]
    public void CalculateOutSystemsCost_ODC_ExternalUsers_FlatPackPricing()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.ODC,
            TotalApplicationObjects = 150,
            ExternalUsers = 2000 // 2 packs of 1000
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        Assert.Equal(2, result.ExternalUserPackCount);
        Assert.True(result.ExternalUsersCost > 0);
    }

    #endregion

    #region O11 Add-Ons Tests

    [Fact]
    public void CalculateOutSystemsCost_O11_Support24x7Premium_ScalesWithAOPacks()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.Cloud,
            TotalApplicationObjects = 300, // 2 AO packs
            O11Support24x7Premium = true
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        Assert.True(result.AddOnCosts.ContainsKey("Support 24x7 Premium"));
        Assert.Equal(7260m, result.AddOnCosts["Support 24x7 Premium"]); // 2 × $3,630
    }

    [Fact]
    public void CalculateOutSystemsCost_O11_Sentry_IncludesHA_CloudOnly()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.Cloud,
            TotalApplicationObjects = 150,
            O11Sentry = true
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        Assert.True(result.AddOnCosts.ContainsKey("Sentry (incl. HA)"));
        Assert.False(result.AddOnCosts.ContainsKey("High Availability")); // HA included in Sentry
    }

    [Fact]
    public void CalculateOutSystemsCost_O11_Sentry_NotAvailable_SelfManaged()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.SelfManaged,
            TotalApplicationObjects = 150,
            O11Sentry = true // Should be ignored for self-managed
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        Assert.False(result.AddOnCosts.ContainsKey("Sentry (incl. HA)"));
    }

    [Fact]
    public void CalculateOutSystemsCost_O11_HighAvailability_CloudOnly()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.Cloud,
            TotalApplicationObjects = 150,
            O11HighAvailability = true
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        Assert.True(result.AddOnCosts.ContainsKey("High Availability"));
    }

    [Fact]
    public void CalculateOutSystemsCost_O11_DisasterRecovery_SelfManagedOnly()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.SelfManaged,
            TotalApplicationObjects = 150,
            O11DisasterRecovery = true
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        Assert.True(result.AddOnCosts.ContainsKey("Disaster Recovery"));
    }

    [Fact]
    public void CalculateOutSystemsCost_O11_NonProdEnv_SupportsQuantity()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.Cloud,
            TotalApplicationObjects = 150, // 1 AO pack
            O11NonProdEnvQuantity = 3
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        Assert.True(result.AddOnCosts.Any(k => k.Key.Contains("Non-Production")));
    }

    [Fact]
    public void CalculateOutSystemsCost_O11_LoadTestEnv_CloudOnly()
    {
        // Arrange
        var cloudConfig = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.Cloud,
            TotalApplicationObjects = 150,
            O11LoadTestEnvQuantity = 2
        };

        var selfManagedConfig = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.SelfManaged,
            TotalApplicationObjects = 150,
            O11LoadTestEnvQuantity = 2
        };

        // Act
        var cloudResult = _service.CalculateOutSystemsCost(cloudConfig);
        var selfManagedResult = _service.CalculateOutSystemsCost(selfManagedConfig);

        // Assert
        Assert.True(cloudResult.AddOnCosts.Any(k => k.Key.Contains("Load Test")));
        Assert.False(selfManagedResult.AddOnCosts.Any(k => k.Key.Contains("Load Test")));
    }

    [Fact]
    public void CalculateOutSystemsCost_O11_LogStreaming_FlatFee_CloudOnly()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.Cloud,
            TotalApplicationObjects = 150,
            O11LogStreamingQuantity = 2
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        Assert.True(result.AddOnCosts.Any(k => k.Key.Contains("Log Streaming")));
    }

    [Fact]
    public void CalculateOutSystemsCost_O11_DatabaseReplica_FlatFee_CloudOnly()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.Cloud,
            TotalApplicationObjects = 150,
            O11DatabaseReplicaQuantity = 1
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        Assert.True(result.AddOnCosts.Any(k => k.Key.Contains("Database Replica")));
    }

    #endregion

    #region ODC Add-Ons Tests

    [Fact]
    public void CalculateOutSystemsCost_ODC_Support24x7Premium_TakesPrecedence()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.ODC,
            TotalApplicationObjects = 150,
            OdcSupport24x7Premium = true,
            OdcSupport24x7Extended = true // Should be ignored
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        Assert.True(result.AddOnCosts.ContainsKey("Support 24x7 Premium"));
        Assert.False(result.AddOnCosts.ContainsKey("Support 24x7 Extended"));
    }

    [Fact]
    public void CalculateOutSystemsCost_ODC_Sentry_IncludesHA()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.ODC,
            TotalApplicationObjects = 150,
            OdcSentry = true
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        Assert.True(result.AddOnCosts.ContainsKey("Sentry"));
        Assert.False(result.AddOnCosts.ContainsKey("High Availability")); // Included in Sentry
    }

    [Fact]
    public void CalculateOutSystemsCost_ODC_NonProdRuntime_SupportsQuantity()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.ODC,
            TotalApplicationObjects = 300, // 2 AO packs
            OdcNonProdRuntimeQuantity = 2
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        Assert.True(result.AddOnCosts.Any(k => k.Key.Contains("Non-Production Runtime")));
        // Cost should be: pricePerPack × aoPackCount × quantity
    }

    [Fact]
    public void CalculateOutSystemsCost_ODC_PrivateGateway_ScalesWithAOPacks()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.ODC,
            TotalApplicationObjects = 300, // 2 AO packs
            OdcPrivateGateway = true
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        Assert.True(result.AddOnCosts.ContainsKey("Private Gateway"));
    }

    #endregion

    #region Services Tests

    [Fact]
    public void CalculateOutSystemsCost_EssentialSuccessPlan_AppliesQuantity()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.Cloud,
            TotalApplicationObjects = 150,
            EssentialSuccessPlanQuantity = 2
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        Assert.True(result.ServiceCosts.Any(k => k.Key.Contains("Essential Success Plan")));
    }

    [Fact]
    public void CalculateOutSystemsCost_PremierSuccessPlan_AppliesQuantity()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.Cloud,
            TotalApplicationObjects = 150,
            PremierSuccessPlanQuantity = 1
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        Assert.True(result.ServiceCosts.Any(k => k.Key.Contains("Premier Success Plan")));
    }

    [Fact]
    public void CalculateOutSystemsCost_TrainingSessions_CalculatesCorrectly()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.Cloud,
            TotalApplicationObjects = 150,
            DedicatedGroupSessionQuantity = 3,
            PublicSessionQuantity = 5
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        Assert.True(result.ServiceCosts.Any(k => k.Key.Contains("Dedicated Group Session")));
        Assert.True(result.ServiceCosts.Any(k => k.Key.Contains("Public Session")));
    }

    #endregion

    #region Discount Tests

    [Fact]
    public void CalculateOutSystemsCost_PercentageDiscount_AppliesToLicenses()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.Cloud,
            TotalApplicationObjects = 150,
            Discount = new OutSystemsDiscount
            {
                Type = OutSystemsDiscountType.Percentage,
                Scope = OutSystemsDiscountScope.LicenseOnly,
                Value = 10
            }
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        Assert.True(result.DiscountAmount > 0);
        Assert.Contains("10%", result.DiscountDescription);
        Assert.Contains("LicenseOnly", result.DiscountDescription);
    }

    [Fact]
    public void CalculateOutSystemsCost_FixedDiscount_AppliesToAll()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.Cloud,
            TotalApplicationObjects = 150,
            Discount = new OutSystemsDiscount
            {
                Type = OutSystemsDiscountType.FixedAmount,
                Scope = OutSystemsDiscountScope.Total,
                Value = 5000,
                Notes = "Volume deal"
            }
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        Assert.Equal(5000m, result.DiscountAmount);
        Assert.Contains("$5,000", result.DiscountDescription);
        Assert.Contains("Volume deal", result.DiscountDescription);
    }

    [Fact]
    public void CalculateOutSystemsCost_NoDiscount_ZeroDiscountAmount()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.Cloud,
            TotalApplicationObjects = 150,
            Discount = null
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        Assert.Equal(0m, result.DiscountAmount);
        Assert.Null(result.DiscountDescription);
    }

    #endregion

    #region Cloud VM Cost Tests

    [Fact]
    public void CalculateOutSystemsCost_O11_SelfManaged_Azure_CalculatesVMCost()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.SelfManaged,
            CloudProvider = OutSystemsCloudProvider.Azure,
            TotalApplicationObjects = 150,
            AzureInstanceType = OutSystemsAzureInstanceType.D4s_v3,
            FrontEndServersPerEnvironment = 1,
            TotalEnvironments = 3
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        Assert.True(result.InfrastructureSubtotal > 0);
    }

    [Fact]
    public void CalculateOutSystemsCost_O11_SelfManaged_AWS_CalculatesVMCost()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.SelfManaged,
            CloudProvider = OutSystemsCloudProvider.AWS,
            TotalApplicationObjects = 150,
            AwsInstanceType = OutSystemsAwsInstanceType.M5XLarge,
            FrontEndServersPerEnvironment = 1,
            TotalEnvironments = 2
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        Assert.True(result.InfrastructureSubtotal > 0);
    }

    [Fact]
    public void CalculateOutSystemsCost_O11_SelfManaged_OnPrem_NoVMCost()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.SelfManaged,
            CloudProvider = OutSystemsCloudProvider.OnPremises,
            TotalApplicationObjects = 150
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        Assert.Equal(0m, result.InfrastructureSubtotal);
    }

    [Fact]
    public void CalculateOutSystemsCost_O11_Cloud_NoVMCost()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.Cloud,
            TotalApplicationObjects = 150
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        Assert.Equal(0m, result.InfrastructureSubtotal);
    }

    #endregion

    #region Instance Recommendation Tests

    [Fact]
    public void RecommendAzureInstance_LowSpecs_ReturnsF4s_v2()
    {
        // Arrange & Act
        var instance = _service.RecommendAzureInstance(4, 8);

        // Assert
        Assert.Equal(OutSystemsAzureInstanceType.F4s_v2, instance);
    }

    [Fact]
    public void RecommendAzureInstance_MediumSpecs_ReturnsD4s_v3()
    {
        // Arrange & Act
        var instance = _service.RecommendAzureInstance(4, 16);

        // Assert
        Assert.Equal(OutSystemsAzureInstanceType.D4s_v3, instance);
    }

    [Fact]
    public void RecommendAzureInstance_HighSpecs_ReturnsD16s_v3()
    {
        // Arrange & Act
        var instance = _service.RecommendAzureInstance(16, 64);

        // Assert
        Assert.Equal(OutSystemsAzureInstanceType.D16s_v3, instance);
    }

    [Fact]
    public void RecommendAwsInstance_LowSpecs_ReturnsM5Large()
    {
        // Arrange & Act
        var instance = _service.RecommendAwsInstance(2, 8);

        // Assert
        Assert.Equal(OutSystemsAwsInstanceType.M5Large, instance);
    }

    [Fact]
    public void RecommendAwsInstance_HighSpecs_ReturnsM52XLarge()
    {
        // Arrange & Act
        var instance = _service.RecommendAwsInstance(8, 32);

        // Assert
        Assert.Equal(OutSystemsAwsInstanceType.M52XLarge, instance);
    }

    #endregion

    #region Cloud-Only Feature Tests

    [Theory]
    [InlineData("Sentry", true)]
    [InlineData("HighAvailability", true)]
    [InlineData("LoadTestEnv", true)]
    [InlineData("LogStreaming", true)]
    [InlineData("DatabaseReplica", true)]
    [InlineData("Support24x7Premium", false)]
    [InlineData("NonProdEnv", false)]
    public void IsOutSystemsCloudOnlyFeature_ReturnsCorrectValue(string featureName, bool expected)
    {
        // Act
        var result = _service.IsOutSystemsCloudOnlyFeature(featureName);

        // Assert
        Assert.Equal(expected, result);
    }

    #endregion

    #region Line Items Tests

    [Fact]
    public void CalculateOutSystemsCost_BuildsLineItems_Complete()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.Cloud,
            TotalApplicationObjects = 300,
            InternalUsers = 200,
            ExternalUsers = 1000,
            O11Support24x7Premium = true,
            EssentialSuccessPlanQuantity = 1
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        Assert.True(result.LicenseBreakdown.Count > 0);
        Assert.True(result.AddOnCosts.Count > 0);
        Assert.True(result.ServiceCosts.Count > 0);
    }

    [Fact]
    public void CalculateOutSystemsCost_TotalCost_SumsAllCategories()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.Cloud,
            TotalApplicationObjects = 150,
            O11Support24x7Premium = true,
            EssentialSuccessPlanQuantity = 1
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        var expectedTotal = result.LicenseSubtotal + result.AddOnsSubtotal +
                           result.ServicesSubtotal + result.InfrastructureSubtotal -
                           result.DiscountAmount;
        Assert.Equal(expectedTotal, result.NetTotal);
    }

    #endregion

    #region Validation Warnings Tests

    [Fact]
    public void CalculateOutSystemsCost_CloudOnlyFeature_SelfManaged_AddsWarning()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.SelfManaged,
            TotalApplicationObjects = 150,
            O11Sentry = true, // Cloud-only feature
            O11HighAvailability = true // Cloud-only feature
        };

        // Act
        var result = _service.CalculateOutSystemsCost(config);

        // Assert
        Assert.True(result.Warnings.Count > 0);
    }

    #endregion
}
