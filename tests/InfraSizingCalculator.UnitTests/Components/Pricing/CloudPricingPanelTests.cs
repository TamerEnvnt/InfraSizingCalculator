using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.Pricing;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Models.Pricing;
using InfraSizingCalculator.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components.Pricing;

/// <summary>
/// Tests for CloudPricingPanel component - Cloud provider cost estimation
/// </summary>
public class CloudPricingPanelTests : TestContext
{
    private readonly IPricingService _pricingService;
    private readonly ICostEstimationService _costEstimationService;

    public CloudPricingPanelTests()
    {
        _pricingService = Substitute.For<IPricingService>();
        _costEstimationService = Substitute.For<ICostEstimationService>();

        // Setup default regions
        _pricingService.GetRegions(Arg.Any<CloudProvider>()).Returns(new List<RegionInfo>
        {
            new() { Code = "us-east-1", DisplayName = "US East (N. Virginia)", IsPreferred = true },
            new() { Code = "us-west-2", DisplayName = "US West (Oregon)" },
            new() { Code = "eu-west-1", DisplayName = "EU (Ireland)" }
        });

        // Setup default pricing
        _pricingService.GetDefaultPricing(Arg.Any<CloudProvider>()).Returns(new PricingModel
        {
            Provider = CloudProvider.AWS,
            Compute = new ComputePricing
            {
                CpuPerHour = 0.05m,
                RamGBPerHour = 0.01m,
                ManagedControlPlanePerHour = 0.10m
            }
        });

        Services.AddSingleton(_pricingService);
        Services.AddSingleton(_costEstimationService);
    }

    #region Rendering Tests

    [Fact]
    public void CloudPricingPanel_RendersContainer()
    {
        // Act
        var cut = RenderComponent<CloudPricingPanel>(parameters => parameters
            .Add(p => p.Provider, CloudProvider.AWS));

        // Assert
        cut.Find(".cloud-pricing-panel").Should().NotBeNull();
    }

    [Fact]
    public void CloudPricingPanel_RendersHeader()
    {
        // Act
        var cut = RenderComponent<CloudPricingPanel>(parameters => parameters
            .Add(p => p.Provider, CloudProvider.AWS));

        // Assert
        cut.Find(".panel-header h4").Should().NotBeNull();
    }

    [Fact]
    public void CloudPricingPanel_ShowsProviderName_AWS()
    {
        // Act
        var cut = RenderComponent<CloudPricingPanel>(parameters => parameters
            .Add(p => p.Provider, CloudProvider.AWS));

        // Assert
        cut.Find(".panel-header h4").TextContent.Should().Contain("AWS");
    }

    [Fact]
    public void CloudPricingPanel_ShowsProviderName_Azure()
    {
        // Act
        var cut = RenderComponent<CloudPricingPanel>(parameters => parameters
            .Add(p => p.Provider, CloudProvider.Azure));

        // Assert
        cut.Find(".panel-header h4").TextContent.Should().Contain("Azure");
    }

    [Fact]
    public void CloudPricingPanel_ShowsProviderName_GCP()
    {
        // Act
        var cut = RenderComponent<CloudPricingPanel>(parameters => parameters
            .Add(p => p.Provider, CloudProvider.GCP));

        // Assert
        cut.Find(".panel-header h4").TextContent.Should().Contain("Google Cloud");
    }

    #endregion

    #region Controls Tests

    [Fact]
    public void CloudPricingPanel_RendersPricingControls()
    {
        // Act
        var cut = RenderComponent<CloudPricingPanel>(parameters => parameters
            .Add(p => p.Provider, CloudProvider.AWS));

        // Assert
        cut.Find(".pricing-controls").Should().NotBeNull();
    }

    [Fact]
    public void CloudPricingPanel_RendersRegionDropdown()
    {
        // Act
        var cut = RenderComponent<CloudPricingPanel>(parameters => parameters
            .Add(p => p.Provider, CloudProvider.AWS));

        // Assert
        var controlLabels = cut.FindAll(".control-group label");
        controlLabels.Should().Contain(l => l.TextContent == "Region");
    }

    [Fact]
    public void CloudPricingPanel_RendersPricingTypeDropdown()
    {
        // Act
        var cut = RenderComponent<CloudPricingPanel>(parameters => parameters
            .Add(p => p.Provider, CloudProvider.AWS));

        // Assert
        var controlLabels = cut.FindAll(".control-group label");
        controlLabels.Should().Contain(l => l.TextContent == "Pricing Type");
    }

    [Fact]
    public void CloudPricingPanel_RendersSupportTierDropdown()
    {
        // Act
        var cut = RenderComponent<CloudPricingPanel>(parameters => parameters
            .Add(p => p.Provider, CloudProvider.AWS));

        // Assert
        var controlLabels = cut.FindAll(".control-group label");
        controlLabels.Should().Contain(l => l.TextContent == "Support Tier");
    }

    [Fact]
    public void CloudPricingPanel_RendersToggles()
    {
        // Act
        var cut = RenderComponent<CloudPricingPanel>(parameters => parameters
            .Add(p => p.Provider, CloudProvider.AWS));

        // Assert
        var toggles = cut.FindAll(".toggle-item");
        toggles.Should().HaveCountGreaterThanOrEqualTo(4);
        toggles.Should().Contain(t => t.TextContent.Contains("Include Storage"));
        toggles.Should().Contain(t => t.TextContent.Contains("Include Network"));
        toggles.Should().Contain(t => t.TextContent.Contains("Include Support"));
        toggles.Should().Contain(t => t.TextContent.Contains("Include Managed Control Plane"));
    }

    #endregion

    #region Region Tests

    [Fact]
    public void CloudPricingPanel_LoadsRegions()
    {
        // Act
        var cut = RenderComponent<CloudPricingPanel>(parameters => parameters
            .Add(p => p.Provider, CloudProvider.AWS));

        // Assert
        _pricingService.Received().GetRegions(CloudProvider.AWS);
        var regionOptions = cut.FindAll(".control-group select")[0].QuerySelectorAll("option");
        regionOptions.Should().HaveCount(3);
    }

    [Fact]
    public void CloudPricingPanel_SelectsPreferredRegion()
    {
        // Act
        var cut = RenderComponent<CloudPricingPanel>(parameters => parameters
            .Add(p => p.Provider, CloudProvider.AWS));

        // Assert
        var regionSelect = cut.FindAll(".control-group select")[0];
        regionSelect.GetAttribute("value").Should().Be("us-east-1");
    }

    #endregion

    #region Pricing Type Tests

    [Fact]
    public void CloudPricingPanel_HasAllPricingTypes()
    {
        // Act
        var cut = RenderComponent<CloudPricingPanel>(parameters => parameters
            .Add(p => p.Provider, CloudProvider.AWS));

        // Assert
        var pricingSelect = cut.FindAll(".control-group select")[1];
        var options = pricingSelect.QuerySelectorAll("option");
        options.Should().HaveCount(4);
        options.Should().Contain(o => o.TextContent.Contains("On-Demand"));
        options.Should().Contain(o => o.TextContent.Contains("1-Year Reserved"));
        options.Should().Contain(o => o.TextContent.Contains("3-Year Reserved"));
        options.Should().Contain(o => o.TextContent.Contains("Spot"));
    }

    #endregion

    #region Support Tier Tests

    [Fact]
    public void CloudPricingPanel_HasAllSupportTiers()
    {
        // Act
        var cut = RenderComponent<CloudPricingPanel>(parameters => parameters
            .Add(p => p.Provider, CloudProvider.AWS));

        // Assert
        var supportSelect = cut.FindAll(".control-group select")[2];
        var options = supportSelect.QuerySelectorAll("option");
        options.Should().HaveCount(5);
        options.Should().Contain(o => o.TextContent == "None");
        options.Should().Contain(o => o.TextContent == "Basic");
        options.Should().Contain(o => o.TextContent == "Developer");
        options.Should().Contain(o => o.TextContent == "Business");
        options.Should().Contain(o => o.TextContent == "Enterprise");
    }

    #endregion

    #region Cost Calculation Tests

    [Fact]
    public void CloudPricingPanel_CalculatesCostOnRender()
    {
        // Act
        var cut = RenderComponent<CloudPricingPanel>(parameters => parameters
            .Add(p => p.Provider, CloudProvider.AWS)
            .Add(p => p.NodeCount, 6)
            .Add(p => p.TotalCores, 48)
            .Add(p => p.TotalRamGB, 192));

        // Assert - Should eventually show cost results
        cut.WaitForAssertion(() =>
        {
            cut.FindAll(".cost-results").Should().NotBeEmpty();
        });
    }

    [Fact]
    public void CloudPricingPanel_ShowsCostSummaryBar()
    {
        // Act
        var cut = RenderComponent<CloudPricingPanel>(parameters => parameters
            .Add(p => p.Provider, CloudProvider.AWS)
            .Add(p => p.NodeCount, 6)
            .Add(p => p.TotalCores, 48)
            .Add(p => p.TotalRamGB, 192));

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Find(".cost-summary-bar").Should().NotBeNull();
            var items = cut.FindAll(".cost-summary-bar .summary-item");
            items.Should().HaveCount(3);
        });
    }

    [Fact]
    public void CloudPricingPanel_ShowsCostBreakdown()
    {
        // Act
        var cut = RenderComponent<CloudPricingPanel>(parameters => parameters
            .Add(p => p.Provider, CloudProvider.AWS)
            .Add(p => p.NodeCount, 6)
            .Add(p => p.TotalCores, 48)
            .Add(p => p.TotalRamGB, 192));

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Find(".cost-breakdown").Should().NotBeNull();
            cut.Find(".cost-breakdown h5").TextContent.Should().Contain("Cost Breakdown");
        });
    }

    #endregion

    #region Spot Warning Tests

    [Fact]
    public async Task CloudPricingPanel_SpotPricing_ShowsWarning()
    {
        // Arrange
        var cut = RenderComponent<CloudPricingPanel>(parameters => parameters
            .Add(p => p.Provider, CloudProvider.AWS)
            .Add(p => p.NodeCount, 6)
            .Add(p => p.TotalCores, 48)
            .Add(p => p.TotalRamGB, 192));

        // Wait for initial render
        cut.WaitForAssertion(() =>
        {
            cut.FindAll(".cost-results").Should().NotBeEmpty();
        });

        // Act - Select Spot pricing
        var pricingSelect = cut.FindAll(".control-group select")[1];
        await cut.InvokeAsync(() => pricingSelect.Change(PricingType.Spot.ToString()));

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Find(".note.warning").Should().NotBeNull();
            cut.Find(".note.warning").TextContent.Should().Contain("terminated");
        });
    }

    #endregion

    #region Callback Tests

    [Fact]
    public void CloudPricingPanel_InvokesPricingCalculatedCallback()
    {
        // Arrange
        CostEstimate? estimate = null;
        var cut = RenderComponent<CloudPricingPanel>(parameters => parameters
            .Add(p => p.Provider, CloudProvider.AWS)
            .Add(p => p.NodeCount, 6)
            .Add(p => p.TotalCores, 48)
            .Add(p => p.TotalRamGB, 192)
            .Add(p => p.OnPricingCalculated, EventCallback.Factory.Create<CostEstimate>(this, e => estimate = e)));

        // Assert
        cut.WaitForAssertion(() =>
        {
            estimate.Should().NotBeNull();
            estimate!.Provider.Should().Be(CloudProvider.AWS);
        });
    }

    #endregion

    #region Provider Display Name Tests

    [Theory]
    [InlineData(CloudProvider.AWS, "AWS")]
    [InlineData(CloudProvider.Azure, "Azure")]
    [InlineData(CloudProvider.GCP, "Google Cloud")]
    [InlineData(CloudProvider.OCI, "Oracle Cloud")]
    [InlineData(CloudProvider.DigitalOcean, "DigitalOcean")]
    public void CloudPricingPanel_ShowsCorrectProviderName(CloudProvider provider, string expectedName)
    {
        // Act
        var cut = RenderComponent<CloudPricingPanel>(parameters => parameters
            .Add(p => p.Provider, provider));

        // Assert
        cut.Find(".panel-header h4").TextContent.Should().Contain(expectedName);
    }

    #endregion

    #region Parameter Update Tests

    [Fact]
    public async Task CloudPricingPanel_ChangingRegion_RecalculatesCost()
    {
        // Arrange
        CostEstimate? estimate = null;
        var callCount = 0;
        var cut = RenderComponent<CloudPricingPanel>(parameters => parameters
            .Add(p => p.Provider, CloudProvider.AWS)
            .Add(p => p.NodeCount, 6)
            .Add(p => p.TotalCores, 48)
            .Add(p => p.TotalRamGB, 192)
            .Add(p => p.OnPricingCalculated, EventCallback.Factory.Create<CostEstimate>(this, e =>
            {
                estimate = e;
                callCount++;
            })));

        // Wait for initial calculation
        cut.WaitForAssertion(() => estimate.Should().NotBeNull());
        var initialCount = callCount;

        // Act
        var regionSelect = cut.FindAll(".control-group select")[0];
        await cut.InvokeAsync(() => regionSelect.Change("eu-west-1"));

        // Assert - Callback should be called again
        cut.WaitForAssertion(() => callCount.Should().BeGreaterThan(initialCount));
    }

    #endregion
}
