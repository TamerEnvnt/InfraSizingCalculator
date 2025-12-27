using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.Shared;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components.Shared;

/// <summary>
/// Tests for HorizontalAccordionPanel component - Expandable panel for horizontal accordion
/// </summary>
public class HorizontalAccordionPanelTests : TestContext
{
    #region Rendering Tests

    [Fact]
    public void HorizontalAccordionPanel_RendersContainer()
    {
        // Act
        var cut = RenderComponent<HorizontalAccordionPanel>();

        // Assert
        cut.Find(".h-accordion-panel").Should().NotBeNull();
    }

    [Fact]
    public void HorizontalAccordionPanel_AppliesAdditionalClass()
    {
        // Act
        var cut = RenderComponent<HorizontalAccordionPanel>(parameters => parameters
            .Add(p => p.AdditionalClass, "custom-class"));

        // Assert
        cut.Find(".h-accordion-panel").ClassList.Should().Contain("custom-class");
    }

    [Fact]
    public void HorizontalAccordionPanel_RendersHeader()
    {
        // Act
        var cut = RenderComponent<HorizontalAccordionPanel>();

        // Assert
        cut.Find(".h-accordion-header").Should().NotBeNull();
    }

    [Fact]
    public void HorizontalAccordionPanel_RendersTitle()
    {
        // Act
        var cut = RenderComponent<HorizontalAccordionPanel>(parameters => parameters
            .Add(p => p.Title, "Test Title"));

        // Assert
        cut.Find(".panel-title").TextContent.Should().Be("Test Title");
    }

    [Fact]
    public void HorizontalAccordionPanel_RendersIcon_WhenProvided()
    {
        // Act
        var cut = RenderComponent<HorizontalAccordionPanel>(parameters => parameters
            .Add(p => p.Icon, "🔧"));

        // Assert
        cut.Find(".panel-icon").TextContent.Should().Be("🔧");
    }

    [Fact]
    public void HorizontalAccordionPanel_HidesIcon_WhenNotProvided()
    {
        // Act
        var cut = RenderComponent<HorizontalAccordionPanel>();

        // Assert
        cut.FindAll(".panel-icon").Should().BeEmpty();
    }

    [Fact]
    public void HorizontalAccordionPanel_RendersIconClass_WhenProvided()
    {
        // Act
        var cut = RenderComponent<HorizontalAccordionPanel>(parameters => parameters
            .Add(p => p.IconClass, "env-icon dev"));

        // Assert
        var iconElement = cut.Find(".env-icon");
        iconElement.Should().NotBeNull();
        iconElement.ClassList.Should().Contain("dev");
    }

    [Fact]
    public void HorizontalAccordionPanel_IconClass_TakesPrecedenceOverIcon()
    {
        // Act - provide both IconClass and Icon, IconClass should be used
        var cut = RenderComponent<HorizontalAccordionPanel>(parameters => parameters
            .Add(p => p.IconClass, "role-icon web")
            .Add(p => p.Icon, "🔧"));

        // Assert - IconClass element should be present, not panel-icon with emoji
        cut.FindAll(".role-icon").Should().NotBeEmpty();
        cut.FindAll(".panel-icon").Should().BeEmpty();
    }

    [Fact]
    public void HorizontalAccordionPanel_RendersExpandIndicator()
    {
        // Act
        var cut = RenderComponent<HorizontalAccordionPanel>();

        // Assert
        cut.Find(".expand-indicator").Should().NotBeNull();
    }

    #endregion

    #region Collapsed State Tests

    [Fact]
    public void HorizontalAccordionPanel_CollapsedByDefault()
    {
        // Act
        var cut = RenderComponent<HorizontalAccordionPanel>();

        // Assert
        cut.Find(".h-accordion-panel").ClassList.Should().Contain("collapsed");
        cut.Find(".h-accordion-panel").ClassList.Should().NotContain("expanded");
    }

    [Fact]
    public void HorizontalAccordionPanel_Collapsed_HidesContent()
    {
        // Act
        var cut = RenderComponent<HorizontalAccordionPanel>(parameters => parameters
            .Add(p => p.IsExpanded, false)
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "Child content")));

        // Assert
        cut.FindAll(".h-accordion-content").Should().BeEmpty();
    }

    [Fact]
    public void HorizontalAccordionPanel_Collapsed_ShowsSubtitle()
    {
        // Act
        var cut = RenderComponent<HorizontalAccordionPanel>(parameters => parameters
            .Add(p => p.IsExpanded, false)
            .Add(p => p.Subtitle, "Panel summary"));

        // Assert
        cut.Find(".panel-subtitle").TextContent.Should().Be("Panel summary");
    }

    [Fact]
    public void HorizontalAccordionPanel_Collapsed_HasRightArrowIndicator()
    {
        // Act
        var cut = RenderComponent<HorizontalAccordionPanel>(parameters => parameters
            .Add(p => p.IsExpanded, false));

        // Assert
        cut.Find(".expand-indicator").ClassList.Should().Contain("icon-arrow-right");
    }

    [Fact]
    public void HorizontalAccordionPanel_Collapsed_AppliesCollapsedWidth()
    {
        // Act
        var cut = RenderComponent<HorizontalAccordionPanel>(parameters => parameters
            .Add(p => p.IsExpanded, false)
            .Add(p => p.CollapsedWidth, "80px"));

        // Assert
        cut.Find(".h-accordion-panel").GetAttribute("style").Should().Contain("flex: 0 0 80px");
    }

    #endregion

    #region Expanded State Tests

    [Fact]
    public void HorizontalAccordionPanel_Expanded_HasExpandedClass()
    {
        // Act
        var cut = RenderComponent<HorizontalAccordionPanel>(parameters => parameters
            .Add(p => p.IsExpanded, true));

        // Assert
        cut.Find(".h-accordion-panel").ClassList.Should().Contain("expanded");
        cut.Find(".h-accordion-panel").ClassList.Should().NotContain("collapsed");
    }

    [Fact]
    public void HorizontalAccordionPanel_Expanded_ShowsContent()
    {
        // Act
        var cut = RenderComponent<HorizontalAccordionPanel>(parameters => parameters
            .Add(p => p.IsExpanded, true)
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "Child content")));

        // Assert
        cut.Find(".h-accordion-content").Should().NotBeNull();
        cut.Find(".h-accordion-content").TextContent.Should().Contain("Child content");
    }

    [Fact]
    public void HorizontalAccordionPanel_Expanded_HidesSubtitle()
    {
        // Act
        var cut = RenderComponent<HorizontalAccordionPanel>(parameters => parameters
            .Add(p => p.IsExpanded, true)
            .Add(p => p.Subtitle, "Panel summary"));

        // Assert
        cut.FindAll(".panel-subtitle").Should().BeEmpty();
    }

    [Fact]
    public void HorizontalAccordionPanel_Expanded_HasLeftArrowIndicator()
    {
        // Act
        var cut = RenderComponent<HorizontalAccordionPanel>(parameters => parameters
            .Add(p => p.IsExpanded, true));

        // Assert
        cut.Find(".expand-indicator").ClassList.Should().Contain("icon-arrow-left");
    }

    [Fact]
    public void HorizontalAccordionPanel_Expanded_AppliesExpandedFlex()
    {
        // Act
        var cut = RenderComponent<HorizontalAccordionPanel>(parameters => parameters
            .Add(p => p.IsExpanded, true)
            .Add(p => p.ExpandedFlex, 2));

        // Assert
        cut.Find(".h-accordion-panel").GetAttribute("style").Should().Contain("flex: 2 1 auto");
    }

    #endregion

    #region Click Behavior Tests (Standalone)

    [Fact]
    public async Task HorizontalAccordionPanel_ClickingHeader_TogglesExpansion()
    {
        // Arrange
        var cut = RenderComponent<HorizontalAccordionPanel>(parameters => parameters
            .Add(p => p.IsExpanded, false));

        // Act
        await cut.InvokeAsync(() => cut.Find(".h-accordion-header").Click());

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Find(".h-accordion-panel").ClassList.Should().Contain("expanded");
        });
    }

    [Fact]
    public async Task HorizontalAccordionPanel_ClickingExpanded_Collapses()
    {
        // Arrange
        var cut = RenderComponent<HorizontalAccordionPanel>(parameters => parameters
            .Add(p => p.IsExpanded, true));

        // Act
        await cut.InvokeAsync(() => cut.Find(".h-accordion-header").Click());

        // Assert
        cut.WaitForAssertion(() =>
        {
            cut.Find(".h-accordion-panel").ClassList.Should().Contain("collapsed");
        });
    }

    [Fact]
    public async Task HorizontalAccordionPanel_ClickingHeader_InvokesIsExpandedChangedCallback()
    {
        // Arrange
        bool? newExpandedState = null;
        var cut = RenderComponent<HorizontalAccordionPanel>(parameters => parameters
            .Add(p => p.IsExpanded, false)
            .Add(p => p.IsExpandedChanged, EventCallback.Factory.Create<bool>(this, v => newExpandedState = v)));

        // Act
        await cut.InvokeAsync(() => cut.Find(".h-accordion-header").Click());

        // Assert
        newExpandedState.Should().BeTrue();
    }

    [Fact]
    public async Task HorizontalAccordionPanel_NoCallback_DoesNotThrow()
    {
        // Arrange
        var cut = RenderComponent<HorizontalAccordionPanel>();

        // Act & Assert
        var action = async () =>
        {
            await cut.InvokeAsync(() => cut.Find(".h-accordion-header").Click());
        };

        await action.Should().NotThrowAsync();
    }

    #endregion

    #region Default Values Tests

    [Fact]
    public void HorizontalAccordionPanel_DefaultCollapsedWidth_Is60px()
    {
        // Act
        var cut = RenderComponent<HorizontalAccordionPanel>(parameters => parameters
            .Add(p => p.IsExpanded, false));

        // Assert
        cut.Find(".h-accordion-panel").GetAttribute("style").Should().Contain("60px");
    }

    [Fact]
    public void HorizontalAccordionPanel_DefaultExpandedFlex_Is1()
    {
        // Act
        var cut = RenderComponent<HorizontalAccordionPanel>(parameters => parameters
            .Add(p => p.IsExpanded, true));

        // Assert
        cut.Find(".h-accordion-panel").GetAttribute("style").Should().Contain("flex: 1 1 auto");
    }

    #endregion

    #region Parameter Update Tests

    [Fact]
    public void HorizontalAccordionPanel_UpdatingIsExpanded_ChangesState()
    {
        // Arrange
        var cut = RenderComponent<HorizontalAccordionPanel>(parameters => parameters
            .Add(p => p.IsExpanded, false));

        // Assert initial
        cut.Find(".h-accordion-panel").ClassList.Should().Contain("collapsed");

        // Act
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.IsExpanded, true));

        // Assert
        cut.Find(".h-accordion-panel").ClassList.Should().Contain("expanded");
    }

    [Fact]
    public void HorizontalAccordionPanel_UpdatingTitle_ChangesDisplay()
    {
        // Arrange
        var cut = RenderComponent<HorizontalAccordionPanel>(parameters => parameters
            .Add(p => p.Title, "Original"));

        // Assert initial
        cut.Find(".panel-title").TextContent.Should().Be("Original");

        // Act
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.Title, "Updated"));

        // Assert
        cut.Find(".panel-title").TextContent.Should().Be("Updated");
    }

    #endregion
}
