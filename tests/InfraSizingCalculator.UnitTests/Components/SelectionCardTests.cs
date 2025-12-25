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
}
