using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.Wizard.Steps;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Models.Pricing;
using InfraSizingCalculator.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components.Wizard;

/// <summary>
/// Tests for PricingStep component - Step 5 of the wizard (Pricing Configuration)
/// </summary>
public class PricingStepTests : TestContext
{
    private readonly IPricingSettingsService _pricingSettingsService;

    public PricingStepTests()
    {
        _pricingSettingsService = Substitute.For<IPricingSettingsService>();

        // Setup default return values
        _pricingSettingsService.IncludePricingInResults.Returns(false);
        _pricingSettingsService.GetOnPremDefaults().Returns(new OnPremPricing());
        _pricingSettingsService.GetMendixPricingSettings().Returns(new MendixPricingSettings());
        _pricingSettingsService.IsMendixSupportedProvider(Arg.Any<MendixPrivateCloudProvider>()).Returns(true);

        Services.AddSingleton(_pricingSettingsService);
    }

    #region Rendering Tests

    [Fact]
    public void PricingStep_RendersWithDefaultState()
    {
        // Act
        var cut = RenderComponent<PricingStep>(parameters => parameters
            .Add(p => p.Distribution, Distribution.OpenShift)
            .Add(p => p.TotalNodes, 6)
            .Add(p => p.WorkerNodes, 3));

        // Assert
        cut.Find(".pricing-step").Should().NotBeNull();
        cut.Find(".step-header").Should().NotBeNull();
        cut.Find(".pricing-tabs").Should().NotBeNull();
    }

    [Fact]
    public void PricingStep_RendersStepTitle()
    {
        // Act
        var cut = RenderComponent<PricingStep>(parameters => parameters
            .Add(p => p.Distribution, Distribution.OpenShift)
            .Add(p => p.TotalNodes, 6));

        // Assert
        cut.Find(".step-header h3").Should().NotBeNull();
    }

    [Fact]
    public void PricingStep_ShowsOnPremTabForOnPremDistribution()
    {
        // Act
        var cut = RenderComponent<PricingStep>(parameters => parameters
            .Add(p => p.Distribution, Distribution.OpenShift)
            .Add(p => p.TotalNodes, 6));

        // Assert - On-prem tab should be visible
        cut.FindAll(".pricing-tab").Should().Contain(t => t.TextContent.Contains("Infrastructure Costs"));
    }

    [Fact]
    public void PricingStep_ShowsCloudAlternativesTabForOnPremDistribution()
    {
        // Act
        var cut = RenderComponent<PricingStep>(parameters => parameters
            .Add(p => p.Distribution, Distribution.OpenShift)
            .Add(p => p.TotalNodes, 6));

        // Assert
        cut.FindAll(".pricing-tab").Should().Contain(t => t.TextContent.Contains("Cloud Alternatives"));
    }

    #endregion

    #region Tab Navigation Tests

    [Fact]
    public void PricingStep_OnPremTab_IsDefaultForOnPremDistribution()
    {
        // Act
        var cut = RenderComponent<PricingStep>(parameters => parameters
            .Add(p => p.Distribution, Distribution.OpenShift)
            .Add(p => p.TotalNodes, 6));

        // Assert - First tab should be active (on-prem)
        var tabs = cut.FindAll(".pricing-tab");
        tabs[0].ClassList.Should().Contain("active");
    }

    [Fact]
    public void PricingStep_CloudTab_IsDefaultForCloudDistribution()
    {
        // Act
        var cut = RenderComponent<PricingStep>(parameters => parameters
            .Add(p => p.Distribution, Distribution.EKS)
            .Add(p => p.TotalNodes, 6)
            .Add(p => p.IsManagedControlPlane, true));

        // Assert - Cloud tab should be active
        var activeTab = cut.Find(".pricing-tab.active");
        activeTab.TextContent.Should().Contain("Cloud");
    }

    [Fact]
    public void PricingStep_ClickingCloudTab_SwitchesToCloudContent()
    {
        // Arrange
        var cut = RenderComponent<PricingStep>(parameters => parameters
            .Add(p => p.Distribution, Distribution.OpenShift)
            .Add(p => p.TotalNodes, 6));

        // Act - Click cloud alternatives tab
        var cloudTab = cut.FindAll(".pricing-tab")[1];
        cloudTab.Click();

        // Assert - After clicking, the cloud panel should be visible
        // Note: The content change is the primary indicator of successful tab switch
        cut.WaitForAssertion(() =>
        {
            cut.Find(".cloud-panel-wide").Should().NotBeNull();
            // The active tab should now be the cloud tab
            cut.Find(".pricing-tab.active").TextContent.Should().Contain("Cloud");
        });
    }

    #endregion

    #region On-Prem Pricing Tests

    [Fact]
    public void PricingStep_OnPremPanel_ShowsIncludePricingToggle()
    {
        // Act
        var cut = RenderComponent<PricingStep>(parameters => parameters
            .Add(p => p.Distribution, Distribution.OpenShift)
            .Add(p => p.TotalNodes, 6));

        // Assert
        cut.Find(".toggle-card").Should().NotBeNull();
        cut.Find("input[type='checkbox']").Should().NotBeNull();
    }

    [Fact]
    public void PricingStep_OnPremPanel_HidesCostSections_WhenPricingDisabled()
    {
        // Arrange
        _pricingSettingsService.IncludePricingInResults.Returns(false);

        // Act
        var cut = RenderComponent<PricingStep>(parameters => parameters
            .Add(p => p.Distribution, Distribution.OpenShift)
            .Add(p => p.TotalNodes, 6));

        // Assert - Cost sections should not be visible
        cut.FindAll(".cost-sections").Should().BeEmpty();
        cut.Find(".no-pricing-note").Should().NotBeNull();
    }

    [Fact]
    public void PricingStep_OnPremPanel_ShowsCostSections_WhenPricingEnabled()
    {
        // Arrange
        _pricingSettingsService.IncludePricingInResults.Returns(true);

        // Act
        var cut = RenderComponent<PricingStep>(parameters => parameters
            .Add(p => p.Distribution, Distribution.OpenShift)
            .Add(p => p.TotalNodes, 6));

        // Toggle the checkbox to enable pricing
        var checkbox = cut.Find("input[type='checkbox']");
        checkbox.Change(true);

        // Assert
        cut.Find(".cost-sections").Should().NotBeNull();
    }

    [Fact]
    public void PricingStep_OnPremPanel_ShowsCostBreakdown()
    {
        // Arrange
        _pricingSettingsService.IncludePricingInResults.Returns(true);

        // Act
        var cut = RenderComponent<PricingStep>(parameters => parameters
            .Add(p => p.Distribution, Distribution.OpenShift)
            .Add(p => p.TotalNodes, 6));

        // Enable pricing
        var checkbox = cut.Find("input[type='checkbox']");
        checkbox.Change(true);

        // Assert - Should show cost breakdown
        var costRows = cut.FindAll(".cost-row");
        costRows.Should().HaveCountGreaterThan(0);
    }

    #endregion

    #region Cloud Alternatives Tests

    [Fact]
    public void PricingStep_CloudPanel_ShowsCloudAlternatives()
    {
        // Arrange
        var cut = RenderComponent<PricingStep>(parameters => parameters
            .Add(p => p.Distribution, Distribution.OpenShift)
            .Add(p => p.TotalNodes, 6)
            .Add(p => p.WorkerNodes, 3));

        // Act - Switch to cloud tab
        var cloudTab = cut.FindAll(".pricing-tab").First(t => t.TextContent.Contains("Cloud"));
        cloudTab.Click();

        // Assert
        cut.Find(".cloud-panel-wide").Should().NotBeNull();
    }

    [Fact]
    public void PricingStep_CloudPanel_ShowsNodeComparison()
    {
        // Arrange
        var cut = RenderComponent<PricingStep>(parameters => parameters
            .Add(p => p.Distribution, Distribution.OpenShift)
            .Add(p => p.TotalNodes, 9)
            .Add(p => p.MasterNodes, 3)
            .Add(p => p.InfraNodes, 3)
            .Add(p => p.WorkerNodes, 3));

        // Act - Switch to cloud tab
        var cloudTab = cut.FindAll(".pricing-tab").First(t => t.TextContent.Contains("Cloud"));
        cloudTab.Click();

        // Assert - Node comparison should be in sidebar
        cut.Find(".cloud-sidebar").Should().NotBeNull();
    }

    [Fact]
    public void PricingStep_CloudPanel_ShowsManagedOpenShiftOptions_ForOpenShift()
    {
        // Arrange
        var cut = RenderComponent<PricingStep>(parameters => parameters
            .Add(p => p.Distribution, Distribution.OpenShift)
            .Add(p => p.TotalNodes, 6)
            .Add(p => p.WorkerNodes, 3));

        // Act - Switch to cloud tab
        var cloudTab = cut.FindAll(".pricing-tab").First(t => t.TextContent.Contains("Cloud"));
        cloudTab.Click();

        // Assert - Should show managed OpenShift section
        var sectionLabels = cut.FindAll(".section-label");
        sectionLabels.Should().Contain(s => s.TextContent.Contains("Managed OpenShift"));
    }

    [Fact]
    public void PricingStep_CloudPanel_ShowsGenericK8sOptions_ForNonOpenShift()
    {
        // Arrange
        var cut = RenderComponent<PricingStep>(parameters => parameters
            .Add(p => p.Distribution, Distribution.Kubernetes)
            .Add(p => p.TotalNodes, 6)
            .Add(p => p.WorkerNodes, 3));

        // Act - Switch to cloud tab
        var cloudTab = cut.FindAll(".pricing-tab").First(t => t.TextContent.Contains("Cloud"));
        cloudTab.Click();

        // Assert - Should show cloud cards without OpenShift section
        cut.FindAll(".cloud-card-detail").Should().HaveCountGreaterThan(0);
    }

    #endregion

    #region Low-Code Platform Tests

    [Fact]
    public void PricingStep_ShowsMendixTab_ForLowCodePlatform()
    {
        // Act
        var cut = RenderComponent<PricingStep>(parameters => parameters
            .Add(p => p.Distribution, Distribution.OpenShift)
            .Add(p => p.Platform, PlatformType.LowCode)
            .Add(p => p.TotalNodes, 6));

        // Assert
        cut.FindAll(".pricing-tab").Should().Contain(t => t.TextContent.Contains("Mendix"));
    }

    [Fact]
    public void PricingStep_HidesMendixTab_ForNativePlatform()
    {
        // Act
        var cut = RenderComponent<PricingStep>(parameters => parameters
            .Add(p => p.Distribution, Distribution.OpenShift)
            .Add(p => p.Platform, PlatformType.Native)
            .Add(p => p.TotalNodes, 6));

        // Assert
        cut.FindAll(".pricing-tab").Should().NotContain(t => t.TextContent.Contains("Mendix"));
    }

    [Fact]
    public void PricingStep_LowCodeTab_ShowsCloudPricing_ForMendixCloud()
    {
        // Arrange & Act
        // Note: When MendixCategory is Cloud, the component shows only one static tab
        // and defaults to showing lowcode-panel content immediately (no click needed)
        var cut = RenderComponent<PricingStep>(parameters => parameters
            .Add(p => p.Distribution, Distribution.OpenShift)
            .Add(p => p.Platform, PlatformType.LowCode)
            .Add(p => p.MendixCategory, MendixDeploymentCategory.Cloud)
            .Add(p => p.TotalNodes, 6));

        // Assert - The lowcode panel should already be visible (default for Mendix Cloud mode)
        cut.Find(".lowcode-panel").Should().NotBeNull();
        // Verify only one tab exists for Mendix Cloud mode
        cut.FindAll(".pricing-tab").Should().HaveCount(1);
        cut.Find(".pricing-tab").TextContent.Should().Contain("Mendix Cloud");
    }

    [Fact]
    public void PricingStep_LowCodeTab_ShowsUserLicensing()
    {
        // Act
        var cut = RenderComponent<PricingStep>(parameters => parameters
            .Add(p => p.Distribution, Distribution.OpenShift)
            .Add(p => p.Platform, PlatformType.LowCode)
            .Add(p => p.MendixCategory, MendixDeploymentCategory.PrivateCloud)
            .Add(p => p.MendixInternalUsers, 100)
            .Add(p => p.MendixExternalUsers, 1000)
            .Add(p => p.TotalNodes, 6));

        // Click Mendix tab
        var mendixTab = cut.FindAll(".pricing-tab").First(t => t.TextContent.Contains("Mendix"));
        mendixTab.Click();

        // Assert - User licensing is now rendered as an accordion with util-icon.users
        var userLicensingAccordion = cut.FindAll(".mendix-accordion").First(a => a.QuerySelector(".util-icon.users") != null);
        userLicensingAccordion.QuerySelector(".accordion-title")!.TextContent.Should().Contain("User Licensing");
    }

    #endregion

    #region Event Callback Tests

    [Fact]
    public async Task PricingStep_TogglingPricing_InvokesOnPricingConfigured()
    {
        // Arrange
        PricingStepResult? result = null;
        var cut = RenderComponent<PricingStep>(parameters => parameters
            .Add(p => p.Distribution, Distribution.OpenShift)
            .Add(p => p.TotalNodes, 6)
            .Add(p => p.OnPricingConfigured, EventCallback.Factory.Create<PricingStepResult>(this,
                r => result = r)));

        // Act - Toggle pricing checkbox
        var checkbox = cut.Find("input[type='checkbox']");
        await cut.InvokeAsync(() => checkbox.Change(true));

        // Assert
        result.Should().NotBeNull();
        result!.IncludePricing.Should().BeTrue();
    }

    [Fact]
    public async Task PricingStep_ClickingSaveScenario_InvokesOnSaveScenario()
    {
        // Arrange
        bool saveClicked = false;
        var cut = RenderComponent<PricingStep>(parameters => parameters
            .Add(p => p.Distribution, Distribution.OpenShift)
            .Add(p => p.TotalNodes, 6)
            .Add(p => p.WorkerNodes, 3)
            .Add(p => p.OnSaveScenario, EventCallback.Factory.Create(this, () => saveClicked = true)));

        // Switch to cloud tab to see save button
        var cloudTab = cut.FindAll(".pricing-tab").First(t => t.TextContent.Contains("Cloud"));
        await cut.InvokeAsync(() => cloudTab.Click());

        // Act
        var saveButton = cut.Find(".btn-save-full");
        await cut.InvokeAsync(() => saveButton.Click());

        // Assert
        saveClicked.Should().BeTrue();
    }

    #endregion

    #region Cloud Cost Calculation Tests

    [Fact]
    public void PricingStep_CalculatesEKSCost()
    {
        // Arrange
        var cut = RenderComponent<PricingStep>(parameters => parameters
            .Add(p => p.Distribution, Distribution.OpenShift)
            .Add(p => p.TotalNodes, 9)
            .Add(p => p.MasterNodes, 3)
            .Add(p => p.InfraNodes, 3)
            .Add(p => p.WorkerNodes, 3));

        // Switch to cloud tab
        var cloudTab = cut.FindAll(".pricing-tab").First(t => t.TextContent.Contains("Cloud"));
        cloudTab.Click();

        // Assert - EKS card should exist and show a price
        var eksCard = cut.FindAll(".cloud-card-detail").FirstOrDefault(c => c.TextContent.Contains("EKS"));
        eksCard.Should().NotBeNull();
        eksCard!.QuerySelector(".card-total")!.TextContent.Should().Contain("$");
    }

    [Fact]
    public void PricingStep_CalculatesAKSCost()
    {
        // Arrange
        var cut = RenderComponent<PricingStep>(parameters => parameters
            .Add(p => p.Distribution, Distribution.OpenShift)
            .Add(p => p.TotalNodes, 9)
            .Add(p => p.WorkerNodes, 3));

        // Switch to cloud tab
        var cloudTab = cut.FindAll(".pricing-tab").First(t => t.TextContent.Contains("Cloud"));
        cloudTab.Click();

        // Assert - AKS card should exist
        var aksCard = cut.FindAll(".cloud-card-detail").FirstOrDefault(c => c.TextContent.Contains("AKS"));
        aksCard.Should().NotBeNull();
    }

    [Fact]
    public void PricingStep_HighlightsLowestCostOption()
    {
        // Arrange
        var cut = RenderComponent<PricingStep>(parameters => parameters
            .Add(p => p.Distribution, Distribution.OpenShift)
            .Add(p => p.TotalNodes, 9)
            .Add(p => p.WorkerNodes, 3));

        // Switch to cloud tab
        var cloudTab = cut.FindAll(".pricing-tab").First(t => t.TextContent.Contains("Cloud"));
        cloudTab.Click();

        // Assert - One card should have "lowest" class
        cut.FindAll(".cloud-card-detail.lowest").Should().HaveCount(1);
    }

    #endregion

    #region Mendix Cloud Deployment Tests

    [Fact]
    public void PricingStep_MendixCloud_ShowsOnlyCloudPricingTab()
    {
        // Act
        var cut = RenderComponent<PricingStep>(parameters => parameters
            .Add(p => p.Platform, PlatformType.LowCode)
            .Add(p => p.MendixCategory, MendixDeploymentCategory.Cloud)
            .Add(p => p.MendixCloudType, MendixCloudType.SaaS)
            .Add(p => p.TotalNodes, 0));

        // Assert - Only Mendix Cloud pricing tab
        var tabs = cut.FindAll(".pricing-tab");
        tabs.Should().HaveCount(1);
        tabs[0].TextContent.Should().Contain("Mendix Cloud");
    }

    [Fact]
    public void PricingStep_MendixCloudDedicated_ShowsDedicatedPricing()
    {
        // Act
        var cut = RenderComponent<PricingStep>(parameters => parameters
            .Add(p => p.Platform, PlatformType.LowCode)
            .Add(p => p.MendixCategory, MendixDeploymentCategory.Cloud)
            .Add(p => p.MendixCloudType, MendixCloudType.Dedicated)
            .Add(p => p.TotalNodes, 0));

        // Assert
        cut.Markup.Should().Contain("Mendix Cloud Dedicated");
    }

    #endregion

    #region Private Cloud Provider Tests

    [Fact]
    public void PricingStep_PrivateCloudAzure_ShowsAzurePricing()
    {
        // Act
        var cut = RenderComponent<PricingStep>(parameters => parameters
            .Add(p => p.Platform, PlatformType.LowCode)
            .Add(p => p.MendixCategory, MendixDeploymentCategory.PrivateCloud)
            .Add(p => p.MendixPrivateProvider, MendixPrivateCloudProvider.Azure)
            .Add(p => p.MendixEnvironments, 5)
            .Add(p => p.TotalNodes, 6));

        // Click Mendix tab
        var mendixTab = cut.FindAll(".pricing-tab").First(t => t.TextContent.Contains("Mendix"));
        mendixTab.Click();

        // Assert
        cut.Markup.Should().Contain("Mendix on Azure");
    }

    [Fact]
    public void PricingStep_PrivateCloudEKS_ShowsK8sPricing()
    {
        // Act
        var cut = RenderComponent<PricingStep>(parameters => parameters
            .Add(p => p.Platform, PlatformType.LowCode)
            .Add(p => p.MendixCategory, MendixDeploymentCategory.PrivateCloud)
            .Add(p => p.MendixPrivateProvider, MendixPrivateCloudProvider.EKS)
            .Add(p => p.MendixEnvironments, 10)
            .Add(p => p.TotalNodes, 6));

        // Click Mendix tab
        var mendixTab = cut.FindAll(".pricing-tab").First(t => t.TextContent.Contains("Mendix"));
        mendixTab.Click();

        // Assert
        cut.Markup.Should().Contain("EKS");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void PricingStep_ZeroNodes_HandlesGracefully()
    {
        // Act
        var action = () => RenderComponent<PricingStep>(parameters => parameters
            .Add(p => p.Distribution, Distribution.OpenShift)
            .Add(p => p.TotalNodes, 0)
            .Add(p => p.WorkerNodes, 0));

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void PricingStep_NullDistribution_HandlesGracefully()
    {
        // Act
        var action = () => RenderComponent<PricingStep>(parameters => parameters
            .Add(p => p.TotalNodes, 6));

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void PricingStep_NoMendixCategory_ShowsSelectionNote()
    {
        // Act
        var cut = RenderComponent<PricingStep>(parameters => parameters
            .Add(p => p.Platform, PlatformType.LowCode)
            .Add(p => p.MendixCategory, (MendixDeploymentCategory?)null)
            .Add(p => p.TotalNodes, 6));

        // Click Mendix tab
        var mendixTab = cut.FindAll(".pricing-tab").First(t => t.TextContent.Contains("Mendix"));
        mendixTab.Click();

        // Assert
        cut.Find(".no-selection-note").Should().NotBeNull();
    }

    #endregion
}
