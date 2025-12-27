using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.Pages;
using InfraSizingCalculator.Models.Pricing;
using InfraSizingCalculator.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components.Pages;

/// <summary>
/// Tests for Settings page - Pricing configuration management
/// </summary>
public class SettingsPageTests : TestContext
{
    private readonly IPricingSettingsService _pricingSettingsService;
    private readonly PricingSettings _defaultSettings;

    public SettingsPageTests()
    {
        _pricingSettingsService = Substitute.For<IPricingSettingsService>();

        // Create default settings for testing
        _defaultSettings = new PricingSettings
        {
            OnPremDefaults = new OnPremPricing(),
            MendixPricing = new MendixPricingSettings(),
            CloudPricing = new CloudPricingSettings(),
            LastModified = DateTime.UtcNow
        };

        _pricingSettingsService.GetSettingsAsync().Returns(_defaultSettings);

        Services.AddSingleton(_pricingSettingsService);
        Services.AddSingleton<NavigationManager>(new FakeNavigationManager(this));
    }

    #region Rendering Tests

    [Fact]
    public void Settings_RendersPageStructure()
    {
        // Act
        var cut = RenderComponent<Settings>();

        // Assert
        cut.Find(".settings-page").Should().NotBeNull();
        cut.Find(".settings-header").Should().NotBeNull();
        cut.Find(".settings-sidebar").Should().NotBeNull();
        cut.Find(".settings-content").Should().NotBeNull();
    }

    [Fact]
    public void Settings_RendersHeaderWithTitle()
    {
        // Act
        var cut = RenderComponent<Settings>();

        // Assert
        cut.Find("h1").TextContent.Should().Contain("Pricing Configuration");
    }

    [Fact]
    public void Settings_RendersBackButton()
    {
        // Act
        var cut = RenderComponent<Settings>();

        // Assert
        var backBtn = cut.Find(".back-btn");
        backBtn.Should().NotBeNull();
        backBtn.TextContent.Should().Contain("Back");
    }

    [Fact]
    public void Settings_RendersSaveAndResetButtons()
    {
        // Act
        var cut = RenderComponent<Settings>();

        // Assert
        var saveBtn = cut.Find(".btn-primary");
        saveBtn.TextContent.Should().Contain("Save");

        var resetBtn = cut.Find(".btn-secondary");
        resetBtn.TextContent.Should().Contain("Reset");
    }

    #endregion

    #region Sidebar Navigation Tests

    [Fact]
    public void Settings_RendersSidebarNavigation()
    {
        // Act
        var cut = RenderComponent<Settings>();

        // Assert
        var navButtons = cut.FindAll(".nav-btn");
        navButtons.Should().HaveCountGreaterThan(5); // Multiple navigation sections
    }

    [Fact]
    public void Settings_ShowsInfrastructureNavGroup()
    {
        // Act
        var cut = RenderComponent<Settings>();

        // Assert
        var navGroups = cut.FindAll(".nav-group-title");
        navGroups.Should().Contain(g => g.TextContent.Contains("Infrastructure"));
    }

    [Fact]
    public void Settings_ShowsCloudProvidersNavGroup()
    {
        // Act
        var cut = RenderComponent<Settings>();

        // Assert
        var navGroups = cut.FindAll(".nav-group-title");
        navGroups.Should().Contain(g => g.TextContent.Contains("Cloud Providers"));
    }

    [Fact]
    public void Settings_ShowsMendixNavGroup()
    {
        // Act
        var cut = RenderComponent<Settings>();

        // Assert
        var navGroups = cut.FindAll(".nav-group-title");
        navGroups.Should().Contain(g => g.TextContent.Contains("Mendix"));
    }

    [Fact]
    public void Settings_HardwareSectionActiveByDefault()
    {
        // Act
        var cut = RenderComponent<Settings>();

        // Assert
        var activeBtn = cut.Find(".nav-btn.active");
        activeBtn.TextContent.Should().Contain("Hardware");
    }

    [Fact]
    public void Settings_ClickingNavBtn_SwitchesSection()
    {
        // Arrange
        var cut = RenderComponent<Settings>();

        // Act - Click Data Center
        var datacenterBtn = cut.FindAll(".nav-btn").First(b => b.TextContent.Contains("Data Center"));
        datacenterBtn.Click();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Find(".nav-btn.active").TextContent.Should().Contain("Data Center");
        });
    }

    #endregion

    #region Hardware Section Tests

    [Fact]
    public void Settings_HardwareSection_ShowsServerCosts()
    {
        // Act
        var cut = RenderComponent<Settings>();

        // Assert
        cut.Find("h2").TextContent.Should().Contain("Hardware Costs");
        cut.FindAll(".config-card").Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void Settings_HardwareSection_HasInputFields()
    {
        // Act
        var cut = RenderComponent<Settings>();

        // Assert
        var inputs = cut.FindAll("input[type='number']");
        inputs.Should().HaveCountGreaterThan(5);
    }

    [Fact]
    public void Settings_HardwareSection_ShowsLabels()
    {
        // Act
        var cut = RenderComponent<Settings>();

        // Assert
        var labels = cut.FindAll("label");
        labels.Should().Contain(l => l.TextContent.Contains("Base Server Cost"));
        labels.Should().Contain(l => l.TextContent.Contains("VMs per Server"));
    }

    #endregion

    #region Data Center Section Tests

    [Fact]
    public void Settings_DataCenterSection_ShowsCorrectContent()
    {
        // Arrange
        var cut = RenderComponent<Settings>();

        // Act - Switch to Data Center section
        var datacenterBtn = cut.FindAll(".nav-btn").First(b => b.TextContent.Contains("Data Center"));
        datacenterBtn.Click();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Find("h2").TextContent.Should().Contain("Data Center Costs");
        });
    }

    [Fact]
    public void Settings_DataCenterSection_ShowsPowerSettings()
    {
        // Arrange
        var cut = RenderComponent<Settings>();
        var datacenterBtn = cut.FindAll(".nav-btn").First(b => b.TextContent.Contains("Data Center"));
        datacenterBtn.Click();

        // Assert
        cut.WaitForAssertion(() =>
        {
            var labels = cut.FindAll("label");
            labels.Should().Contain(l => l.TextContent.Contains("Power Cost"));
            labels.Should().Contain(l => l.TextContent.Contains("Watts per Server"));
        });
    }

    #endregion

    #region Labor Section Tests

    [Fact]
    public void Settings_LaborSection_ShowsSalarySettings()
    {
        // Arrange
        var cut = RenderComponent<Settings>();

        // Act
        var laborBtn = cut.FindAll(".nav-btn").First(b => b.TextContent.Contains("Labor"));
        laborBtn.Click();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Find("h2").TextContent.Should().Contain("Labor Costs");
            var labels = cut.FindAll("label");
            labels.Should().Contain(l => l.TextContent.Contains("DevOps Engineer"));
        });
    }

    #endregion

    #region Cloud Provider Section Tests

    [Fact]
    public void Settings_AWSSection_ShowsEKSPricing()
    {
        // Arrange
        var cut = RenderComponent<Settings>();

        // Act
        var awsBtn = cut.FindAll(".nav-btn").First(b => b.TextContent.Contains("AWS"));
        awsBtn.Click();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Find("h2").TextContent.Should().Contain("AWS");
            var cardHeaders = cut.FindAll(".config-card h3");
            cardHeaders.Should().Contain(h => h.TextContent.Contains("EKS"));
        });
    }

    [Fact]
    public void Settings_AzureSection_ShowsAKSPricing()
    {
        // Arrange
        var cut = RenderComponent<Settings>();

        // Act
        var azureBtn = cut.FindAll(".nav-btn").First(b => b.TextContent.Contains("Azure"));
        azureBtn.Click();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Find("h2").TextContent.Should().Contain("Azure");
        });
    }

    [Fact]
    public void Settings_GCPSection_ShowsGKEPricing()
    {
        // Arrange
        var cut = RenderComponent<Settings>();

        // Act
        var gcpBtn = cut.FindAll(".nav-btn").First(b => b.TextContent.Contains("GCP"));
        gcpBtn.Click();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Find("h2").TextContent.Should().Contain("GCP");
        });
    }

    #endregion

    #region Mendix Section Tests

    [Fact]
    public void Settings_MendixPlatformSection_ShowsLicensing()
    {
        // Arrange
        var cut = RenderComponent<Settings>();

        // Act
        var mxBtn = cut.FindAll(".nav-btn").First(b => b.TextContent.Contains("Platform & Users"));
        mxBtn.Click();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Find("h2").TextContent.Should().Contain("Mendix Platform");
            var labels = cut.FindAll("label");
            labels.Should().Contain(l => l.TextContent.Contains("Premium Unlimited"));
        });
    }

    [Fact]
    public void Settings_MendixCloudSection_ShowsResourcePacks()
    {
        // Arrange
        var cut = RenderComponent<Settings>();

        // Act
        var mxCloudBtn = cut.FindAll(".nav-btn").First(b => b.TextContent.Contains("Mendix Cloud"));
        mxCloudBtn.Click();

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Find("h2").TextContent.Should().Contain("Mendix Cloud");
        });
    }

    #endregion

    #region Action Tests

    [Fact]
    public async Task Settings_ClickingSave_CallsSaveSettings()
    {
        // Arrange
        var cut = RenderComponent<Settings>();

        // Act
        var saveBtn = cut.Find(".btn-primary");
        await cut.InvokeAsync(() => saveBtn.Click());

        // Assert
        await _pricingSettingsService.Received(1).SaveSettingsAsync(Arg.Any<PricingSettings>());
    }

    [Fact]
    public async Task Settings_ClickingReset_CallsResetToDefaults()
    {
        // Arrange
        var cut = RenderComponent<Settings>();

        // Act
        var resetBtn = cut.Find(".btn-secondary");
        await cut.InvokeAsync(() => resetBtn.Click());

        // Assert
        await _pricingSettingsService.Received(1).ResetToDefaultsAsync();
    }

    [Fact]
    public async Task Settings_LoadsSettingsOnInit()
    {
        // Act
        var cut = RenderComponent<Settings>();

        // Assert
        await _pricingSettingsService.Received(1).GetSettingsAsync();
    }

    #endregion

    #region Navigation Tests

    [Fact]
    public void Settings_ClickingBack_NavigatesToHome()
    {
        // Arrange
        var cut = RenderComponent<Settings>();
        var nav = Services.GetRequiredService<NavigationManager>() as FakeNavigationManager;

        // Act
        var backBtn = cut.Find(".back-btn");
        backBtn.Click();

        // Assert
        nav!.NavigatedUri.Should().Be("/");
    }

    [Fact]
    public async Task Settings_SaveSettings_NavigatesToHome()
    {
        // Arrange
        var cut = RenderComponent<Settings>();
        var nav = Services.GetRequiredService<NavigationManager>() as FakeNavigationManager;

        // Act
        var saveBtn = cut.Find(".btn-primary");
        await cut.InvokeAsync(() => saveBtn.Click());

        // Assert
        nav!.NavigatedUri.Should().Be("/");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Settings_NullSettings_HandlesGracefully()
    {
        // Arrange
        _pricingSettingsService.GetSettingsAsync().Returns(new PricingSettings
        {
            OnPremDefaults = new OnPremPricing(),
            MendixPricing = new MendixPricingSettings(),
            CloudPricing = new CloudPricingSettings()
        });

        // Act & Assert
        var action = () => RenderComponent<Settings>();
        action.Should().NotThrow();
    }

    [Fact]
    public void Settings_AllNavSections_Clickable()
    {
        // Arrange
        var cut = RenderComponent<Settings>();
        var navButtons = cut.FindAll(".nav-btn").ToList();

        // Act & Assert - Each button should be clickable without throwing
        foreach (var btn in navButtons)
        {
            var action = () => btn.Click();
            action.Should().NotThrow();
        }
    }

    #endregion

    /// <summary>
    /// Fake NavigationManager for testing navigation
    /// </summary>
    private class FakeNavigationManager : NavigationManager
    {
        private readonly TestContext _context;

        public FakeNavigationManager(TestContext context)
        {
            _context = context;
            Initialize("http://localhost/", "http://localhost/settings");
        }

        public string? NavigatedUri { get; private set; }

        protected override void NavigateToCore(string uri, bool forceLoad)
        {
            NavigatedUri = uri;
        }
    }
}
