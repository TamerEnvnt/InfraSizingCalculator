using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.Shared;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components;

/// <summary>
/// Tests for HorizontalAccordion component
/// </summary>
public class HorizontalAccordionTests : TestContext
{
    [Fact]
    public void HorizontalAccordion_RendersContainer()
    {
        // Act
        var cut = RenderComponent<HorizontalAccordion>();

        // Assert
        cut.FindAll(".h-accordion").Should().HaveCount(1);
    }

    [Fact]
    public void HorizontalAccordion_AppliesSingleExpandClass_WhenSingleExpandIsTrue()
    {
        // Act
        var cut = RenderComponent<HorizontalAccordion>(parameters => parameters
            .Add(p => p.SingleExpand, true));

        // Assert
        cut.Find(".h-accordion").ClassList.Should().Contain("single-expand");
    }

    [Fact]
    public void HorizontalAccordion_DoesNotApplySingleExpandClass_WhenSingleExpandIsFalse()
    {
        // Act
        var cut = RenderComponent<HorizontalAccordion>(parameters => parameters
            .Add(p => p.SingleExpand, false));

        // Assert
        cut.Find(".h-accordion").ClassList.Should().NotContain("single-expand");
    }

    [Fact]
    public void HorizontalAccordion_AppliesAdditionalClass()
    {
        // Act
        var cut = RenderComponent<HorizontalAccordion>(parameters => parameters
            .Add(p => p.AdditionalClass, "custom-accordion"));

        // Assert
        cut.Find(".h-accordion").ClassList.Should().Contain("custom-accordion");
    }

    [Fact]
    public void HorizontalAccordion_RendersChildContent()
    {
        // Act
        var cut = RenderComponent<HorizontalAccordion>(parameters => parameters
            .AddChildContent("<div class='test-panel'>Panel Content</div>"));

        // Assert
        cut.Find(".test-panel").TextContent.Should().Be("Panel Content");
    }

    [Fact]
    public void HorizontalAccordion_IsPanelExpanded_ReturnsTrueForExpandedPanel()
    {
        // Arrange
        var cut = RenderComponent<HorizontalAccordion>(parameters => parameters
            .Add(p => p.ExpandedPanel, "panel1"));

        // Act
        var isExpanded = cut.Instance.IsPanelExpanded("panel1");

        // Assert
        isExpanded.Should().BeTrue();
    }

    [Fact]
    public void HorizontalAccordion_IsPanelExpanded_ReturnsFalseForOtherPanel()
    {
        // Arrange
        var cut = RenderComponent<HorizontalAccordion>(parameters => parameters
            .Add(p => p.ExpandedPanel, "panel1"));

        // Act
        var isExpanded = cut.Instance.IsPanelExpanded("panel2");

        // Assert
        isExpanded.Should().BeFalse();
    }

    [Fact]
    public void HorizontalAccordion_IsPanelExpanded_ReturnsFalseWhenNoExpanded()
    {
        // Arrange
        var cut = RenderComponent<HorizontalAccordion>();

        // Act
        var isExpanded = cut.Instance.IsPanelExpanded("panel1");

        // Assert
        isExpanded.Should().BeFalse();
    }

    [Fact]
    public async Task HorizontalAccordion_TogglePanel_ExpandsPanel()
    {
        // Arrange
        string? expandedPanel = null;
        var cut = RenderComponent<HorizontalAccordion>(parameters => parameters
            .Add(p => p.SingleExpand, true)
            .Add(p => p.ExpandedPanelChanged, (string? p) => expandedPanel = p));

        // Act - Use InvokeAsync to properly handle the Blazor dispatcher
        await cut.InvokeAsync(() => cut.Instance.TogglePanel("panel1"));

        // Assert
        expandedPanel.Should().Be("panel1");
    }

    [Fact]
    public async Task HorizontalAccordion_TogglePanel_CollapsesExpandedPanel()
    {
        // Arrange
        string? expandedPanel = "panel1";
        var cut = RenderComponent<HorizontalAccordion>(parameters => parameters
            .Add(p => p.SingleExpand, true)
            .Add(p => p.ExpandedPanel, "panel1")
            .Add(p => p.ExpandedPanelChanged, (string? p) => expandedPanel = p));

        // Act - Use InvokeAsync to properly handle the Blazor dispatcher
        await cut.InvokeAsync(() => cut.Instance.TogglePanel("panel1"));

        // Assert
        expandedPanel.Should().BeNull();
    }

    [Fact]
    public async Task HorizontalAccordion_TogglePanel_SwitchesPanels()
    {
        // Arrange
        string? expandedPanel = "panel1";
        var cut = RenderComponent<HorizontalAccordion>(parameters => parameters
            .Add(p => p.SingleExpand, true)
            .Add(p => p.ExpandedPanel, "panel1")
            .Add(p => p.ExpandedPanelChanged, (string? p) => expandedPanel = p));

        // Act - Use InvokeAsync to properly handle the Blazor dispatcher
        await cut.InvokeAsync(() => cut.Instance.TogglePanel("panel2"));

        // Assert
        expandedPanel.Should().Be("panel2");
    }

    [Fact]
    public void HorizontalAccordion_DefaultsSingleExpandToTrue()
    {
        // Act
        var cut = RenderComponent<HorizontalAccordion>();

        // Assert
        cut.Find(".h-accordion").ClassList.Should().Contain("single-expand");
    }
}
