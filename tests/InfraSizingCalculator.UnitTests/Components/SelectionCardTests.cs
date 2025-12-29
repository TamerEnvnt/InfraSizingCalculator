using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace InfraSizingCalculator.UnitTests.Components;

/// <summary>
/// Tests for SelectionCard component
/// </summary>
public class SelectionCardTests : TestContext
{
    [Fact]
    public void SelectionCard_RendersTitle()
    {
        // Arrange & Act
        var cut = RenderComponent<SelectionCard>(parameters => parameters
            .Add(p => p.Title, "Test Title"));

        // Assert
        cut.Find(".card-title").TextContent.Should().Be("Test Title");
    }

    [Fact]
    public void SelectionCard_RendersIcon_WhenProvided()
    {
        // Arrange & Act
        var cut = RenderComponent<SelectionCard>(parameters => parameters
            .Add(p => p.Title, "Test")
            .Add(p => p.Icon, "🚀"));

        // Assert
        cut.Find(".card-icon").TextContent.Should().Be("🚀");
    }

    [Fact]
    public void SelectionCard_DoesNotRenderIcon_WhenNotProvided()
    {
        // Arrange & Act
        var cut = RenderComponent<SelectionCard>(parameters => parameters
            .Add(p => p.Title, "Test"));

        // Assert
        cut.FindAll(".card-icon").Should().BeEmpty();
    }

    [Fact]
    public void SelectionCard_RendersSubtitle_WhenProvided()
    {
        // Arrange & Act
        var cut = RenderComponent<SelectionCard>(parameters => parameters
            .Add(p => p.Title, "Test")
            .Add(p => p.Subtitle, "Test Subtitle"));

        // Assert
        cut.Find(".card-subtitle").TextContent.Should().Be("Test Subtitle");
    }

    [Fact]
    public void SelectionCard_RendersDescription_WhenProvided()
    {
        // Arrange & Act
        var cut = RenderComponent<SelectionCard>(parameters => parameters
            .Add(p => p.Title, "Test")
            .Add(p => p.Description, "Test Description"));

        // Assert
        cut.Find(".card-desc").TextContent.Should().Be("Test Description");
    }

    [Fact]
    public void SelectionCard_HasSelectedClass_WhenSelected()
    {
        // Arrange & Act
        var cut = RenderComponent<SelectionCard>(parameters => parameters
            .Add(p => p.Title, "Test")
            .Add(p => p.IsSelected, true));

        // Assert
        cut.Find(".selection-card").ClassList.Should().Contain("selected");
    }

    [Fact]
    public void SelectionCard_DoesNotHaveSelectedClass_WhenNotSelected()
    {
        // Arrange & Act
        var cut = RenderComponent<SelectionCard>(parameters => parameters
            .Add(p => p.Title, "Test")
            .Add(p => p.IsSelected, false));

        // Assert
        cut.Find(".selection-card").ClassList.Should().NotContain("selected");
    }

    [Fact]
    public void SelectionCard_ShowsCheckMark_WhenSelected()
    {
        // Arrange & Act
        var cut = RenderComponent<SelectionCard>(parameters => parameters
            .Add(p => p.Title, "Test")
            .Add(p => p.IsSelected, true));

        // Assert - Check mark uses CSS icon class
        var checkMark = cut.Find(".check-mark");
        checkMark.ClassList.Should().Contain("icon-check");
    }

    [Fact]
    public void SelectionCard_HidesCheckMark_WhenNotSelected()
    {
        // Arrange & Act
        var cut = RenderComponent<SelectionCard>(parameters => parameters
            .Add(p => p.Title, "Test")
            .Add(p => p.IsSelected, false));

        // Assert
        cut.FindAll(".check-mark").Should().BeEmpty();
    }

    [Fact]
    public void SelectionCard_InvokesOnClick_WhenClicked()
    {
        // Arrange
        var clicked = false;
        var cut = RenderComponent<SelectionCard>(parameters => parameters
            .Add(p => p.Title, "Test")
            .Add(p => p.OnClick, () => clicked = true));

        // Act
        cut.Find(".selection-card").Click();

        // Assert
        clicked.Should().BeTrue();
    }

    [Fact]
    public void SelectionCard_AppliesBrandColor_WhenProvided()
    {
        // Arrange & Act
        var cut = RenderComponent<SelectionCard>(parameters => parameters
            .Add(p => p.Title, "Test")
            .Add(p => p.BrandColor, "#FF0000"));

        // Assert
        var style = cut.Find(".selection-card").GetAttribute("style");
        style.Should().Contain("--brand-color: #FF0000");
    }

    [Fact]
    public void SelectionCard_AppliesAdditionalClass_WhenProvided()
    {
        // Arrange & Act
        var cut = RenderComponent<SelectionCard>(parameters => parameters
            .Add(p => p.Title, "Test")
            .Add(p => p.AdditionalClass, "custom-class"));

        // Assert
        cut.Find(".selection-card").ClassList.Should().Contain("custom-class");
    }

    [Fact]
    public void SelectionCard_RendersTags_WhenProvided()
    {
        // Arrange
        var tags = new List<SelectionCard.TagInfo>
        {
            new("tag-primary", "Primary"),
            new("tag-secondary", "Secondary")
        };

        // Act
        var cut = RenderComponent<SelectionCard>(parameters => parameters
            .Add(p => p.Title, "Test")
            .Add(p => p.Tags, tags));

        // Assert
        var tagElements = cut.FindAll(".tag");
        tagElements.Should().HaveCount(2);
        tagElements[0].TextContent.Should().Be("Primary");
        tagElements[0].ClassList.Should().Contain("tag-primary");
        tagElements[1].TextContent.Should().Be("Secondary");
        tagElements[1].ClassList.Should().Contain("tag-secondary");
    }

    [Fact]
    public void SelectionCard_DoesNotRenderTags_WhenEmpty()
    {
        // Arrange & Act
        var cut = RenderComponent<SelectionCard>(parameters => parameters
            .Add(p => p.Title, "Test")
            .Add(p => p.Tags, Array.Empty<SelectionCard.TagInfo>()));

        // Assert
        cut.FindAll(".card-tags").Should().BeEmpty();
    }

    [Fact]
    public void SelectionCard_DoesNotRenderTags_WhenNull()
    {
        // Arrange & Act
        var cut = RenderComponent<SelectionCard>(parameters => parameters
            .Add(p => p.Title, "Test"));

        // Assert
        cut.FindAll(".card-tags").Should().BeEmpty();
    }

    #region Keyboard Accessibility Tests (WCAG 2.1)

    [Fact]
    public void SelectionCard_EnterKey_InvokesOnClick()
    {
        // Arrange
        var clicked = false;
        var cut = RenderComponent<SelectionCard>(parameters => parameters
            .Add(p => p.Title, "Test")
            .Add(p => p.OnClick, () => clicked = true));

        // Act
        cut.Find(".selection-card").KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "Enter" });

        // Assert
        clicked.Should().BeTrue();
    }

    [Fact]
    public void SelectionCard_SpaceKey_InvokesOnClick()
    {
        // Arrange
        var clicked = false;
        var cut = RenderComponent<SelectionCard>(parameters => parameters
            .Add(p => p.Title, "Test")
            .Add(p => p.OnClick, () => clicked = true));

        // Act
        cut.Find(".selection-card").KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = " " });

        // Assert
        clicked.Should().BeTrue();
    }

    [Theory]
    [InlineData("Tab")]
    [InlineData("Escape")]
    [InlineData("ArrowDown")]
    [InlineData("a")]
    public void SelectionCard_OtherKeys_DoNotInvokeOnClick(string key)
    {
        // Arrange
        var clicked = false;
        var cut = RenderComponent<SelectionCard>(parameters => parameters
            .Add(p => p.Title, "Test")
            .Add(p => p.OnClick, () => clicked = true));

        // Act
        cut.Find(".selection-card").KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = key });

        // Assert
        clicked.Should().BeFalse();
    }

    [Fact]
    public void SelectionCard_KeyDown_NoCallbackDoesNotError()
    {
        // Arrange - No OnClick callback provided
        var cut = RenderComponent<SelectionCard>(parameters => parameters
            .Add(p => p.Title, "Test"));

        // Act & Assert - Should not throw
        var action = () => cut.Find(".selection-card").KeyDown(
            new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "Enter" });
        action.Should().NotThrow();
    }

    #endregion

    #region Accessibility Attributes

    [Fact]
    public void SelectionCard_HasCorrectAriaSelected_WhenSelected()
    {
        // Arrange & Act
        var cut = RenderComponent<SelectionCard>(parameters => parameters
            .Add(p => p.Title, "Test")
            .Add(p => p.IsSelected, true));

        // Assert
        cut.Find(".selection-card").GetAttribute("aria-selected").Should().Be("true");
    }

    [Fact]
    public void SelectionCard_HasCorrectAriaSelected_WhenNotSelected()
    {
        // Arrange & Act
        var cut = RenderComponent<SelectionCard>(parameters => parameters
            .Add(p => p.Title, "Test")
            .Add(p => p.IsSelected, false));

        // Assert
        cut.Find(".selection-card").GetAttribute("aria-selected").Should().Be("false");
    }

    [Fact]
    public void SelectionCard_HasCorrectRole()
    {
        // Arrange & Act
        var cut = RenderComponent<SelectionCard>(parameters => parameters
            .Add(p => p.Title, "Test"));

        // Assert
        cut.Find(".selection-card").GetAttribute("role").Should().Be("option");
    }

    [Fact]
    public void SelectionCard_IsFocusable()
    {
        // Arrange & Act
        var cut = RenderComponent<SelectionCard>(parameters => parameters
            .Add(p => p.Title, "Test"));

        // Assert - tabindex="0" makes the element focusable
        cut.Find(".selection-card").GetAttribute("tabindex").Should().Be("0");
    }

    #endregion

    #region Conditional Content Rendering

    [Fact]
    public void SelectionCard_DoesNotRenderSubtitle_WhenNotProvided()
    {
        // Arrange & Act
        var cut = RenderComponent<SelectionCard>(parameters => parameters
            .Add(p => p.Title, "Test"));

        // Assert
        cut.FindAll(".card-subtitle").Should().BeEmpty();
    }

    [Fact]
    public void SelectionCard_DoesNotRenderDescription_WhenNotProvided()
    {
        // Arrange & Act
        var cut = RenderComponent<SelectionCard>(parameters => parameters
            .Add(p => p.Title, "Test"));

        // Assert
        cut.FindAll(".card-desc").Should().BeEmpty();
    }

    [Fact]
    public void SelectionCard_ShowsVisuallyHiddenSelectedText_WhenSelected()
    {
        // Arrange & Act
        var cut = RenderComponent<SelectionCard>(parameters => parameters
            .Add(p => p.Title, "Test")
            .Add(p => p.IsSelected, true));

        // Assert - Screen reader text
        cut.Find(".visually-hidden").TextContent.Should().Be("Selected");
    }

    [Fact]
    public void SelectionCard_NoBrandColorStyle_WhenNotProvided()
    {
        // Arrange & Act
        var cut = RenderComponent<SelectionCard>(parameters => parameters
            .Add(p => p.Title, "Test"));

        // Assert
        var style = cut.Find(".selection-card").GetAttribute("style");
        (style ?? "").Should().NotContain("--brand-color");
    }

    #endregion
}
