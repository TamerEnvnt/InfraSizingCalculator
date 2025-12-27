using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.Pricing;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Models.Pricing;
using InfraSizingCalculator.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components.Pricing;

/// <summary>
/// Tests for OnPremPricingPanel component - On-premises cost estimation
/// </summary>
public class OnPremPricingPanelTests : TestContext
{
    private static OnPremPricing CreateDefaultPricing() => new();

    #region Rendering Tests

    [Fact]
    public void OnPremPricingPanel_RendersContainer()
    {
        // Act
        var cut = RenderComponent<OnPremPricingPanel>();

        // Assert
        cut.Find(".onprem-pricing-panel").Should().NotBeNull();
    }

    [Fact]
    public void OnPremPricingPanel_RendersToggleSection()
    {
        // Act
        var cut = RenderComponent<OnPremPricingPanel>();

        // Assert
        cut.Find(".pricing-toggle-section").Should().NotBeNull();
        cut.Find(".toggle-label").TextContent.Should().Contain("Include Pricing");
    }

    [Fact]
    public void OnPremPricingPanel_ToggleOff_ShowsHint()
    {
        // Act
        var cut = RenderComponent<OnPremPricingPanel>(parameters => parameters
            .Add(p => p.IncludePricing, false));

        // Assert
        cut.Find(".toggle-hint").TextContent.Should().Contain("Costs will show as");
    }

    [Fact]
    public void OnPremPricingPanel_ToggleOn_ShowsPricingSections()
    {
        // Act
        var cut = RenderComponent<OnPremPricingPanel>(parameters => parameters
            .Add(p => p.IncludePricing, true)
            .Add(p => p.OnPremDefaults, CreateDefaultPricing())
            .Add(p => p.NodeCount, 6)
            .Add(p => p.TotalCores, 48)
            .Add(p => p.TotalRamGB, 192));

        // Assert
        cut.FindAll(".pricing-section").Should().HaveCount(4); // License, Infra, DataCenter, Labor
    }

    #endregion

    #region Pricing Sections Tests

    [Fact]
    public void OnPremPricingPanel_ShowsLicenseSection()
    {
        // Act
        var cut = RenderComponent<OnPremPricingPanel>(parameters => parameters
            .Add(p => p.IncludePricing, true)
            .Add(p => p.Distribution, Distribution.OpenShift)
            .Add(p => p.OnPremDefaults, CreateDefaultPricing())
            .Add(p => p.NodeCount, 6));

        // Assert
        var sections = cut.FindAll(".pricing-section summary");
        sections.Should().Contain(s => s.TextContent.Contains("License Costs"));
    }

    [Fact]
    public void OnPremPricingPanel_ShowsInfrastructureSection()
    {
        // Act
        var cut = RenderComponent<OnPremPricingPanel>(parameters => parameters
            .Add(p => p.IncludePricing, true)
            .Add(p => p.OnPremDefaults, CreateDefaultPricing())
            .Add(p => p.NodeCount, 6));

        // Assert
        var sections = cut.FindAll(".pricing-section summary");
        sections.Should().Contain(s => s.TextContent.Contains("Infrastructure Costs"));
    }

    [Fact]
    public void OnPremPricingPanel_ShowsDataCenterSection()
    {
        // Act
        var cut = RenderComponent<OnPremPricingPanel>(parameters => parameters
            .Add(p => p.IncludePricing, true)
            .Add(p => p.OnPremDefaults, CreateDefaultPricing())
            .Add(p => p.NodeCount, 6));

        // Assert
        var sections = cut.FindAll(".pricing-section summary");
        sections.Should().Contain(s => s.TextContent.Contains("Data Center Costs"));
    }

    [Fact]
    public void OnPremPricingPanel_ShowsLaborSection()
    {
        // Act
        var cut = RenderComponent<OnPremPricingPanel>(parameters => parameters
            .Add(p => p.IncludePricing, true)
            .Add(p => p.OnPremDefaults, CreateDefaultPricing())
            .Add(p => p.NodeCount, 6));

        // Assert
        var sections = cut.FindAll(".pricing-section summary");
        sections.Should().Contain(s => s.TextContent.Contains("Labor Costs"));
    }

    #endregion

    #region License Description Tests

    [Fact]
    public void OnPremPricingPanel_OpenShift_ShowsLicenseDescription()
    {
        // Act
        var cut = RenderComponent<OnPremPricingPanel>(parameters => parameters
            .Add(p => p.IncludePricing, true)
            .Add(p => p.Distribution, Distribution.OpenShift)
            .Add(p => p.OnPremDefaults, CreateDefaultPricing())
            .Add(p => p.NodeCount, 6));

        // Assert
        cut.Markup.Should().Contain("Red Hat OpenShift");
    }

    [Fact]
    public void OnPremPricingPanel_Tanzu_ShowsLicenseDescription()
    {
        // Act
        var cut = RenderComponent<OnPremPricingPanel>(parameters => parameters
            .Add(p => p.IncludePricing, true)
            .Add(p => p.Distribution, Distribution.Tanzu)
            .Add(p => p.OnPremDefaults, CreateDefaultPricing())
            .Add(p => p.NodeCount, 6));

        // Assert
        cut.Markup.Should().Contain("VMware Tanzu");
    }

    [Fact]
    public void OnPremPricingPanel_K3s_ShowsNoLicenseMessage()
    {
        // Act
        var cut = RenderComponent<OnPremPricingPanel>(parameters => parameters
            .Add(p => p.IncludePricing, true)
            .Add(p => p.Distribution, Distribution.K3s)
            .Add(p => p.OnPremDefaults, CreateDefaultPricing())
            .Add(p => p.NodeCount, 6));

        // Assert - K3s is open source, should show no license message
        cut.Markup.Should().Contain("open source");
    }

    #endregion

    #region Cost Summary Tests

    [Fact]
    public void OnPremPricingPanel_ShowsCostSummary()
    {
        // Act
        var cut = RenderComponent<OnPremPricingPanel>(parameters => parameters
            .Add(p => p.IncludePricing, true)
            .Add(p => p.OnPremDefaults, CreateDefaultPricing())
            .Add(p => p.NodeCount, 6)
            .Add(p => p.TotalCores, 48)
            .Add(p => p.TotalRamGB, 192));

        // Assert
        cut.Find(".cost-summary").Should().NotBeNull();
        cut.Find(".summary-header").TextContent.Should().Contain("Total On-Premises Cost");
    }

    [Fact]
    public void OnPremPricingPanel_SummaryShowsMonthlyYearlyAndTCO()
    {
        // Act
        var cut = RenderComponent<OnPremPricingPanel>(parameters => parameters
            .Add(p => p.IncludePricing, true)
            .Add(p => p.OnPremDefaults, CreateDefaultPricing())
            .Add(p => p.NodeCount, 6)
            .Add(p => p.TotalCores, 48)
            .Add(p => p.TotalRamGB, 192));

        // Assert
        var summaryItems = cut.FindAll(".summary-item");
        summaryItems.Should().HaveCount(3);
        summaryItems.Should().Contain(s => s.TextContent.Contains("Monthly"));
        summaryItems.Should().Contain(s => s.TextContent.Contains("Yearly"));
        summaryItems.Should().Contain(s => s.TextContent.Contains("3-Year TCO"));
    }

    #endregion

    #region Breakdown Tests

    [Fact]
    public void OnPremPricingPanel_ShowsServerBreakdown()
    {
        // Act
        var cut = RenderComponent<OnPremPricingPanel>(parameters => parameters
            .Add(p => p.IncludePricing, true)
            .Add(p => p.OnPremDefaults, CreateDefaultPricing())
            .Add(p => p.NodeCount, 6));

        // Assert
        cut.Markup.Should().Contain("Servers");
    }

    [Fact]
    public void OnPremPricingPanel_ShowsCpuBreakdown()
    {
        // Act
        var cut = RenderComponent<OnPremPricingPanel>(parameters => parameters
            .Add(p => p.IncludePricing, true)
            .Add(p => p.OnPremDefaults, CreateDefaultPricing())
            .Add(p => p.TotalCores, 48));

        // Assert
        cut.Markup.Should().Contain("CPU");
        cut.Markup.Should().Contain("48 cores");
    }

    [Fact]
    public void OnPremPricingPanel_ShowsRamBreakdown()
    {
        // Act
        var cut = RenderComponent<OnPremPricingPanel>(parameters => parameters
            .Add(p => p.IncludePricing, true)
            .Add(p => p.OnPremDefaults, CreateDefaultPricing())
            .Add(p => p.TotalRamGB, 192));

        // Assert
        cut.Markup.Should().Contain("RAM");
        cut.Markup.Should().Contain("192 GB");
    }

    [Fact]
    public void OnPremPricingPanel_ShowsDevOpsEngineers()
    {
        // Act
        var cut = RenderComponent<OnPremPricingPanel>(parameters => parameters
            .Add(p => p.IncludePricing, true)
            .Add(p => p.OnPremDefaults, CreateDefaultPricing())
            .Add(p => p.NodeCount, 20));

        // Assert
        cut.Markup.Should().Contain("DevOps Engineers");
    }

    #endregion

    #region Callback Tests

    [Fact]
    public async Task OnPremPricingPanel_TogglingPricing_InvokesCallback()
    {
        // Arrange
        bool? newValue = null;
        var cut = RenderComponent<OnPremPricingPanel>(parameters => parameters
            .Add(p => p.IncludePricing, false)
            .Add(p => p.IncludePricingChanged, EventCallback.Factory.Create<bool>(this, v => newValue = v)));

        // Act
        var toggle = cut.Find(".toggle-container input[type='checkbox']");
        await cut.InvokeAsync(() => toggle.Change(true));

        // Assert
        newValue.Should().BeTrue();
    }

    [Fact]
    public async Task OnPremPricingPanel_EnablingPricing_InvokesCostCalculatedCallback()
    {
        // Arrange
        OnPremCostBreakdown? breakdown = null;
        var cut = RenderComponent<OnPremPricingPanel>(parameters => parameters
            .Add(p => p.IncludePricing, false)
            .Add(p => p.OnPremDefaults, CreateDefaultPricing())
            .Add(p => p.NodeCount, 6)
            .Add(p => p.TotalCores, 48)
            .Add(p => p.TotalRamGB, 192)
            .Add(p => p.OnCostCalculated, EventCallback.Factory.Create<OnPremCostBreakdown>(this, b => breakdown = b)));

        // Act
        var toggle = cut.Find(".toggle-container input[type='checkbox']");
        await cut.InvokeAsync(() => toggle.Change(true));

        // Assert
        breakdown.Should().NotBeNull();
        breakdown!.IsCalculated.Should().BeTrue();
    }

    #endregion

    #region Parameter Update Tests

    [Fact]
    public void OnPremPricingPanel_UpdatingNodeCount_RecalculatesCosts()
    {
        // Arrange
        var cut = RenderComponent<OnPremPricingPanel>(parameters => parameters
            .Add(p => p.IncludePricing, true)
            .Add(p => p.OnPremDefaults, CreateDefaultPricing())
            .Add(p => p.NodeCount, 6));

        // Get initial markup
        var initialMarkup = cut.Markup;

        // Act
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.NodeCount, 12));

        // Assert - Markup should change due to recalculation
        cut.Markup.Should().NotBe(initialMarkup);
    }

    [Fact]
    public void OnPremPricingPanel_ToggleOff_HidesPricingSections()
    {
        // Arrange
        var cut = RenderComponent<OnPremPricingPanel>(parameters => parameters
            .Add(p => p.IncludePricing, true)
            .Add(p => p.OnPremDefaults, CreateDefaultPricing())
            .Add(p => p.NodeCount, 6));

        // Assert initial
        cut.FindAll(".pricing-section").Should().HaveCount(4);

        // Act
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.IncludePricing, false));

        // Assert
        cut.FindAll(".pricing-section").Should().BeEmpty();
    }

    #endregion
}
