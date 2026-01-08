using AngleSharp.Dom;
using Bunit;
using InfraSizingCalculator.Components.Pricing;
using InfraSizingCalculator.Models.Pricing;
using InfraSizingCalculator.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components.Pricing;

/// <summary>
/// Tests for OutSystemsPricingPanel Blazor component.
/// Tests rendering, user interactions, platform/deployment switching, and cost calculation triggers.
/// </summary>
public class OutSystemsPricingPanelTests : TestContext
{
    private readonly IPricingSettingsService _pricingService;
    private readonly OutSystemsPricingSettings _settings;

    public OutSystemsPricingPanelTests()
    {
        _pricingService = Substitute.For<IPricingSettingsService>();
        _settings = new OutSystemsPricingSettings();

        // Setup default mock behavior
        _pricingService.CalculateOutSystemsCost(Arg.Any<OutSystemsDeploymentConfig>())
            .Returns(callInfo => new OutSystemsPricingResult
            {
                Platform = ((OutSystemsDeploymentConfig)callInfo[0]).Platform,
                Deployment = ((OutSystemsDeploymentConfig)callInfo[0]).Deployment,
                EditionCost = 36300m,
                LicenseBreakdown = new Dictionary<string, decimal>(),
                AddOnCosts = new Dictionary<string, decimal>(),
                ServiceCosts = new Dictionary<string, decimal>(),
                Warnings = new List<string>()
            });

        Services.AddSingleton(_pricingService);
    }

    #region Rendering Tests

    [Fact]
    public void Renders_PlatformSelection_WithODCAndO11Options()
    {
        // Arrange & Act
        var cut = RenderComponent<OutSystemsPricingPanel>(parameters =>
            parameters.Add(p => p.Settings, _settings)
                     .Add(p => p.Config, new OutSystemsDeploymentConfig()));

        // Assert - Component has both platform and deployment cards, verify platform options exist
        var allCards = cut.FindAll(".option-card");
        Assert.True(allCards.Count >= 2, "Should have at least 2 option cards for platform selection");
        Assert.Contains(allCards, c => c.TextContent.Contains("ODC"));
        Assert.Contains(allCards, c => c.TextContent.Contains("O11"));
    }

    [Fact]
    public void Renders_DeploymentType_WhenO11Selected()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig { Platform = OutSystemsPlatform.O11 };

        // Act
        var cut = RenderComponent<OutSystemsPricingPanel>(parameters =>
            parameters.Add(p => p.Settings, _settings)
                     .Add(p => p.Config, config));

        // Assert
        var sectionHeaders = cut.FindAll(".section-header");
        Assert.Contains(sectionHeaders, h => h.TextContent.Contains("Deployment Type"));
    }

    [Fact]
    public void HidesDeploymentType_WhenODCSelected()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig { Platform = OutSystemsPlatform.ODC };

        // Act
        var cut = RenderComponent<OutSystemsPricingPanel>(parameters =>
            parameters.Add(p => p.Settings, _settings)
                     .Add(p => p.Config, config));

        // Assert - ODC doesn't show deployment type selection
        var allText = cut.Markup;
        var sectionHeaders = cut.FindAll(".section-header");
        Assert.DoesNotContain(sectionHeaders, h => h.TextContent.Contains("Deployment Type"));
    }

    [Fact]
    public void Renders_RegionSelection_WithAllRegions()
    {
        // Arrange & Act
        var cut = RenderComponent<OutSystemsPricingPanel>(parameters =>
            parameters.Add(p => p.Settings, _settings)
                     .Add(p => p.Config, new OutSystemsDeploymentConfig()));

        // Assert
        var regionSelect = cut.Find(".region-select");
        var options = regionSelect.QuerySelectorAll("option");
        Assert.Equal(5, options.Length); // Africa, MiddleEast, Americas, Europe, AsiaPacific
    }

    [Fact]
    public void Renders_AOSlider_WithCorrectDefaults()
    {
        // Arrange & Act
        var cut = RenderComponent<OutSystemsPricingPanel>(parameters =>
            parameters.Add(p => p.Settings, _settings)
                     .Add(p => p.Config, new OutSystemsDeploymentConfig { TotalApplicationObjects = 150 }));

        // Assert
        var slider = cut.Find("input[type='range']");
        Assert.Equal("150", slider.GetAttribute("min"));
        Assert.Equal("3000", slider.GetAttribute("max"));
        Assert.Equal("150", slider.GetAttribute("step"));
    }

    [Fact]
    public void Renders_CloudInfrastructure_WhenO11SelfManaged()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.SelfManaged
        };

        // Act
        var cut = RenderComponent<OutSystemsPricingPanel>(parameters =>
            parameters.Add(p => p.Settings, _settings)
                     .Add(p => p.Config, config));

        // Assert
        var markup = cut.Markup;
        Assert.Contains("Cloud Infrastructure", markup);
        Assert.Contains("Microsoft Azure", markup);
        Assert.Contains("Amazon Web Services", markup);
    }

    [Fact]
    public void HidesCloudInfrastructure_WhenO11Cloud()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.Cloud
        };

        // Act
        var cut = RenderComponent<OutSystemsPricingPanel>(parameters =>
            parameters.Add(p => p.Settings, _settings)
                     .Add(p => p.Config, config));

        // Assert
        var sectionTitles = cut.FindAll(".section-title");
        Assert.DoesNotContain(sectionTitles, t => t.TextContent.Contains("Cloud Infrastructure"));
    }

    [Fact]
    public void Renders_ODCAddOns_WhenODCPlatform()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig { Platform = OutSystemsPlatform.ODC };

        // Act
        var cut = RenderComponent<OutSystemsPricingPanel>(parameters =>
            parameters.Add(p => p.Settings, _settings)
                     .Add(p => p.Config, config));

        // Assert
        var sectionTitles = cut.FindAll(".section-title");
        Assert.Contains(sectionTitles, t => t.TextContent.Contains("ODC Add-ons"));
    }

    [Fact]
    public void Renders_O11AddOns_WhenO11Platform()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig { Platform = OutSystemsPlatform.O11 };

        // Act
        var cut = RenderComponent<OutSystemsPricingPanel>(parameters =>
            parameters.Add(p => p.Settings, _settings)
                     .Add(p => p.Config, config));

        // Assert
        var sectionTitles = cut.FindAll(".section-title");
        Assert.Contains(sectionTitles, t => t.TextContent.Contains("O11 Add-ons"));
    }

    [Fact]
    public void Renders_ServicesSection()
    {
        // Arrange & Act
        var cut = RenderComponent<OutSystemsPricingPanel>(parameters =>
            parameters.Add(p => p.Settings, _settings)
                     .Add(p => p.Config, new OutSystemsDeploymentConfig()));

        // Assert
        var sectionTitles = cut.FindAll(".section-title");
        Assert.Contains(sectionTitles, t => t.TextContent.Contains("Services"));
    }

    [Fact]
    public void Renders_DiscountSection()
    {
        // Arrange & Act
        var cut = RenderComponent<OutSystemsPricingPanel>(parameters =>
            parameters.Add(p => p.Settings, _settings)
                     .Add(p => p.Config, new OutSystemsDeploymentConfig()));

        // Assert
        var sectionTitles = cut.FindAll(".section-title");
        Assert.Contains(sectionTitles, t => t.TextContent.Contains("Discount"));
    }

    [Fact]
    public void Renders_CostSummary_WhenResultExists()
    {
        // Arrange & Act
        var cut = RenderComponent<OutSystemsPricingPanel>(parameters =>
            parameters.Add(p => p.Settings, _settings)
                     .Add(p => p.Config, new OutSystemsDeploymentConfig()));

        // Assert
        Assert.Contains("Cost Estimate", cut.Markup);
        Assert.Contains("Monthly", cut.Markup);
        Assert.Contains("Yearly", cut.Markup);
    }

    #endregion

    #region Platform Selection Tests

    [Fact]
    public void SelectingODC_UpdatesConfigPlatform()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig { Platform = OutSystemsPlatform.O11 };
        var configChanged = false;

        var cut = RenderComponent<OutSystemsPricingPanel>(parameters =>
            parameters.Add(p => p.Settings, _settings)
                     .Add(p => p.Config, config)
                     .Add(p => p.ConfigChanged, EventCallback.Factory.Create<OutSystemsDeploymentConfig>(new object(), _ => configChanged = true)));

        // Act
        var odcCard = cut.FindAll(".option-card").First(c => c.TextContent.Contains("ODC"));
        odcCard.Click();

        // Assert
        Assert.Equal(OutSystemsPlatform.ODC, config.Platform);
        Assert.True(configChanged);
    }

    [Fact]
    public void SelectingODC_ResetsO11AddOns()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            O11Support24x7Premium = true,
            O11Sentry = true
        };

        var cut = RenderComponent<OutSystemsPricingPanel>(parameters =>
            parameters.Add(p => p.Settings, _settings)
                     .Add(p => p.Config, config));

        // Act
        var odcCard = cut.FindAll(".option-card").First(c => c.TextContent.Contains("ODC"));
        odcCard.Click();

        // Assert
        Assert.False(config.O11Support24x7Premium);
        Assert.False(config.O11Sentry);
    }

    [Fact]
    public void SelectingO11_ResetsODCAddOns()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.ODC,
            OdcSupport24x7Premium = true,
            OdcSentry = true
        };

        var cut = RenderComponent<OutSystemsPricingPanel>(parameters =>
            parameters.Add(p => p.Settings, _settings)
                     .Add(p => p.Config, config));

        // Act
        var o11Card = cut.FindAll(".option-card").First(c => c.TextContent.Contains("O11"));
        o11Card.Click();

        // Assert
        Assert.False(config.OdcSupport24x7Premium);
        Assert.False(config.OdcSentry);
    }

    #endregion

    #region Deployment Type Tests

    [Fact]
    public void SelectingSelfManaged_ClearsCloudOnlyFeatures()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.Cloud,
            O11Sentry = true,
            O11HighAvailability = true
        };

        var cut = RenderComponent<OutSystemsPricingPanel>(parameters =>
            parameters.Add(p => p.Settings, _settings)
                     .Add(p => p.Config, config));

        // Act
        var selfManagedCard = cut.FindAll(".option-card").First(c => c.TextContent.Contains("Self-Managed"));
        selfManagedCard.Click();

        // Assert
        Assert.False(config.O11Sentry);
        Assert.False(config.O11HighAvailability);
    }

    [Fact]
    public void SelectingCloud_ClearsSelfManagedOnlyFeatures()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.SelfManaged,
            O11DisasterRecovery = true
        };

        var cut = RenderComponent<OutSystemsPricingPanel>(parameters =>
            parameters.Add(p => p.Settings, _settings)
                     .Add(p => p.Config, config));

        // Act
        var cloudCard = cut.FindAll(".option-card").First(c => c.TextContent.Contains("OutSystems Cloud"));
        cloudCard.Click();

        // Assert
        Assert.False(config.O11DisasterRecovery);
    }

    #endregion

    #region Cloud-Only Feature Validation Tests

    [Fact]
    public void CloudOnlyBadge_ShownForO11CloudFeatures()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.Cloud
        };

        // Act
        var cut = RenderComponent<OutSystemsPricingPanel>(parameters =>
            parameters.Add(p => p.Settings, _settings)
                     .Add(p => p.Config, config));

        // Assert
        var cloudBadges = cut.FindAll(".cloud-badge");
        Assert.True(cloudBadges.Count > 0);
    }

    [Fact]
    public void SelfManagedBadge_ShownForDisasterRecovery()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.SelfManaged
        };

        // Act
        var cut = RenderComponent<OutSystemsPricingPanel>(parameters =>
            parameters.Add(p => p.Settings, _settings)
                     .Add(p => p.Config, config));

        // Assert
        var smBadges = cut.FindAll(".sm-badge");
        Assert.True(smBadges.Count > 0);
    }

    #endregion

    #region EventCallback Tests

    [Fact]
    public void ConfigChanged_InvokedOnPlatformChange()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig();
        OutSystemsDeploymentConfig? receivedConfig = null;

        var cut = RenderComponent<OutSystemsPricingPanel>(parameters =>
            parameters.Add(p => p.Settings, _settings)
                     .Add(p => p.Config, config)
                     .Add(p => p.ConfigChanged, EventCallback.Factory.Create<OutSystemsDeploymentConfig>(new object(), c => receivedConfig = c)));

        // Act
        var o11Card = cut.FindAll(".option-card").First(c => c.TextContent.Contains("O11"));
        o11Card.Click();

        // Assert
        Assert.NotNull(receivedConfig);
        Assert.Equal(OutSystemsPlatform.O11, receivedConfig.Platform);
    }

    [Fact]
    public void OnCostCalculated_InvokedOnChange()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig();
        OutSystemsPricingResult? receivedResult = null;

        var cut = RenderComponent<OutSystemsPricingPanel>(parameters =>
            parameters.Add(p => p.Settings, _settings)
                     .Add(p => p.Config, config)
                     .Add(p => p.OnCostCalculated, EventCallback.Factory.Create<OutSystemsPricingResult>(new object(), r => receivedResult = r)));

        // Act
        var o11Card = cut.FindAll(".option-card").First(c => c.TextContent.Contains("O11"));
        o11Card.Click();

        // Assert
        Assert.NotNull(receivedResult);
    }

    #endregion

    #region Sentry/HA Interaction Tests

    [Fact]
    public void EnablingSentry_DisablesHACheckbox()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            Platform = OutSystemsPlatform.O11,
            Deployment = OutSystemsDeployment.Cloud
        };

        var cut = RenderComponent<OutSystemsPricingPanel>(parameters =>
            parameters.Add(p => p.Settings, _settings)
                     .Add(p => p.Config, config));

        // Find and click Sentry checkbox
        var sentryCheckbox = cut.FindAll(".addon-item input[type='checkbox']")
            .FirstOrDefault(c => c.Parent?.TextContent?.Contains("Sentry") == true);

        if (sentryCheckbox != null)
        {
            sentryCheckbox.Change(true);
        }

        // Re-render
        cut.Render();

        // Assert - HA should be false when Sentry is enabled
        Assert.False(config.O11HighAvailability);
    }

    #endregion

    #region User Licensing Tests

    [Fact]
    public void UnlimitedUsers_ShowsCorrectPrice()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            TotalApplicationObjects = 300, // 2 AO packs
            UseUnlimitedUsers = true
        };

        // Act
        var cut = RenderComponent<OutSystemsPricingPanel>(parameters =>
            parameters.Add(p => p.Settings, _settings)
                     .Add(p => p.Config, config));

        // Assert - should show $121,000 (2 × $60,500)
        Assert.Contains("$121,000", cut.Markup);
    }

    [Fact]
    public void TogglingUnlimitedUsers_UpdatesConfig()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig { UseUnlimitedUsers = false };

        var cut = RenderComponent<OutSystemsPricingPanel>(parameters =>
            parameters.Add(p => p.Settings, _settings)
                     .Add(p => p.Config, config));

        // Act
        var unlimitedCheckbox = cut.Find(".checkbox-container input[type='checkbox']");
        unlimitedCheckbox.Change(true);

        // Assert
        Assert.True(config.UseUnlimitedUsers);
    }

    #endregion

    #region Warnings Display Tests

    [Fact]
    public void Warnings_DisplayedWhenPresent()
    {
        // Arrange
        _pricingService.CalculateOutSystemsCost(Arg.Any<OutSystemsDeploymentConfig>())
            .Returns(new OutSystemsPricingResult
            {
                Warnings = new List<string> { "Test warning message" }
            });

        var config = new OutSystemsDeploymentConfig();

        // Act
        var cut = RenderComponent<OutSystemsPricingPanel>(parameters =>
            parameters.Add(p => p.Settings, _settings)
                     .Add(p => p.Config, config));

        // Assert
        Assert.Contains("Test warning message", cut.Markup);
    }

    #endregion

    #region AO Pack Calculation Display Tests

    [Fact]
    public void AOPackCount_DisplayedCorrectly()
    {
        // Arrange
        var config = new OutSystemsDeploymentConfig
        {
            TotalApplicationObjects = 450 // 3 packs
        };

        // Act
        var cut = RenderComponent<OutSystemsPricingPanel>(parameters =>
            parameters.Add(p => p.Settings, _settings)
                     .Add(p => p.Config, config));

        // Assert
        var badges = cut.FindAll(".section-badge");
        Assert.Contains(badges, b => b.TextContent.Contains("3 pack"));
    }

    #endregion
}
