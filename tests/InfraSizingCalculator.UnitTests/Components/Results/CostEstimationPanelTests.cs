using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.Results;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Models.Pricing;
using InfraSizingCalculator.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components.Results;

/// <summary>
/// Tests for CostEstimationPanel component - Expandable cost estimation panel
/// </summary>
public class CostEstimationPanelTests : TestContext
{
    private readonly ICostEstimationService _costEstimationService;
    private readonly IPricingService _pricingService;

    public CostEstimationPanelTests()
    {
        _costEstimationService = Substitute.For<ICostEstimationService>();
        _pricingService = Substitute.For<IPricingService>();

        _pricingService.GetRegions(Arg.Any<CloudProvider>()).Returns(new List<RegionInfo>
        {
            new() { Code = "us-east-1", DisplayName = "US East", IsPreferred = true }
        });

        Services.AddSingleton(_costEstimationService);
        Services.AddSingleton(_pricingService);
    }

    private static K8sSizingResult CreateK8sResult()
    {
        return new K8sSizingResult
        {
            Environments = new List<EnvironmentResult>(),
            Configuration = new K8sSizingInput(),
            GrandTotal = new GrandTotal
            {
                TotalNodes = 6,
                TotalCpu = 48,
                TotalRam = 192,
                TotalDisk = 600
            }
        };
    }

    #region Rendering Tests

    [Fact]
    public void CostEstimationPanel_RendersContainer()
    {
        // Act
        var cut = RenderComponent<CostEstimationPanel>();

        // Assert
        cut.Find(".cost-estimation-panel").Should().NotBeNull();
    }

    [Fact]
    public void CostEstimationPanel_RendersHeader()
    {
        // Act
        var cut = RenderComponent<CostEstimationPanel>();

        // Assert
        cut.Find(".cost-panel-header").Should().NotBeNull();
        cut.Find(".panel-title").TextContent.Should().Contain("Cost Estimation");
    }

    [Fact]
    public void CostEstimationPanel_CollapsedByDefault()
    {
        // Act
        var cut = RenderComponent<CostEstimationPanel>();

        // Assert
        cut.Find(".cost-estimation-panel").ClassList.Should().Contain("collapsed");
    }

    [Fact]
    public void CostEstimationPanel_ShowsExpandIcon_WhenCollapsed()
    {
        // Act
        var cut = RenderComponent<CostEstimationPanel>(parameters => parameters
            .Add(p => p.IsExpanded, false));

        // Assert
        cut.Find(".panel-icon").TextContent.Should().Be("▶");
    }

    [Fact]
    public void CostEstimationPanel_ShowsCollapseIcon_WhenExpanded()
    {
        // Act
        var cut = RenderComponent<CostEstimationPanel>(parameters => parameters
            .Add(p => p.IsExpanded, true));

        // Assert
        cut.Find(".panel-icon").TextContent.Should().Be("▼");
    }

    #endregion

    #region Expand/Collapse Tests

    [Fact]
    public void CostEstimationPanel_Expanded_ShowsContent()
    {
        // Act
        var cut = RenderComponent<CostEstimationPanel>(parameters => parameters
            .Add(p => p.IsExpanded, true));

        // Assert
        cut.Find(".cost-panel-content").Should().NotBeNull();
    }

    [Fact]
    public void CostEstimationPanel_Collapsed_HidesContent()
    {
        // Act
        var cut = RenderComponent<CostEstimationPanel>(parameters => parameters
            .Add(p => p.IsExpanded, false));

        // Assert
        cut.FindAll(".cost-panel-content").Should().BeEmpty();
    }

    [Fact]
    public async Task CostEstimationPanel_ClickingHeader_TogglesExpansion()
    {
        // Arrange
        bool? newExpandedState = null;
        var cut = RenderComponent<CostEstimationPanel>(parameters => parameters
            .Add(p => p.IsExpanded, false)
            .Add(p => p.IsExpandedChanged, EventCallback.Factory.Create<bool>(this, v => newExpandedState = v)));

        // Act
        await cut.InvokeAsync(() => cut.Find(".cost-panel-header").Click());

        // Assert
        newExpandedState.Should().BeTrue();
    }

    [Fact]
    public async Task CostEstimationPanel_ClickingExpanded_Collapses()
    {
        // Arrange
        bool? newExpandedState = null;
        var cut = RenderComponent<CostEstimationPanel>(parameters => parameters
            .Add(p => p.IsExpanded, true)
            .Add(p => p.IsExpandedChanged, EventCallback.Factory.Create<bool>(this, v => newExpandedState = v)));

        // Act
        await cut.InvokeAsync(() => cut.Find(".cost-panel-header").Click());

        // Assert
        newExpandedState.Should().BeFalse();
    }

    #endregion

    #region Parameter Tests

    [Fact]
    public void CostEstimationPanel_WithK8sResult_SetsContext()
    {
        // Arrange
        var k8sResult = CreateK8sResult();

        // Act
        var cut = RenderComponent<CostEstimationPanel>(parameters => parameters
            .Add(p => p.K8sResult, k8sResult)
            .Add(p => p.IsExpanded, true));

        // Assert
        cut.Find(".cost-panel-content").Should().NotBeNull();
    }

    [Fact]
    public void CostEstimationPanel_WithDistribution_SetsOption()
    {
        // Act
        var cut = RenderComponent<CostEstimationPanel>(parameters => parameters
            .Add(p => p.Distribution, "OpenShift")
            .Add(p => p.IsExpanded, true));

        // Assert
        cut.Find(".cost-panel-content").Should().NotBeNull();
    }

    #endregion

    #region State Update Tests

    [Fact]
    public void CostEstimationPanel_UpdatingExpanded_ChangesState()
    {
        // Arrange
        var cut = RenderComponent<CostEstimationPanel>(parameters => parameters
            .Add(p => p.IsExpanded, false));

        // Assert initial
        cut.Find(".cost-estimation-panel").ClassList.Should().Contain("collapsed");

        // Act
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.IsExpanded, true));

        // Assert
        cut.Find(".cost-estimation-panel").ClassList.Should().Contain("expanded");
    }

    #endregion
}
