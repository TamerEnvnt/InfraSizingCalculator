using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.Shared;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components;

/// <summary>
/// Tests for InfoButton component
/// </summary>
public class InfoButtonTests : TestContext
{
    [Fact]
    public void InfoButton_RendersButton()
    {
        // Act
        var cut = RenderComponent<InfoButton>();

        // Assert
        cut.FindAll(".info-btn").Should().HaveCount(1);
    }

    [Fact]
    public void InfoButton_DisplaysQuestionMark()
    {
        // Act
        var cut = RenderComponent<InfoButton>();

        // Assert - Text includes question mark (wrapped in aria-hidden span)
        cut.Find(".info-btn").TextContent.Should().Contain("?");
    }

    [Fact]
    public void InfoButton_HasDefaultAriaLabel()
    {
        // Act
        var cut = RenderComponent<InfoButton>();

        // Assert - Uses aria-label for accessibility instead of title
        cut.Find(".info-btn").GetAttribute("aria-label").Should().Be("More information");
    }

    [Fact]
    public void InfoButton_UsesCustomTitle()
    {
        // Act
        var cut = RenderComponent<InfoButton>(parameters => parameters
            .Add(p => p.Title, "Click for help"));

        // Assert - Title is reflected in aria-label for accessibility
        cut.Find(".info-btn").GetAttribute("aria-label").Should().Be("Click for help");
    }

    [Fact]
    public void InfoButton_AppliesSmallSize()
    {
        // Act
        var cut = RenderComponent<InfoButton>(parameters => parameters
            .Add(p => p.Size, "small"));

        // Assert
        cut.Find(".info-btn").ClassList.Should().Contain("small");
    }

    [Fact]
    public void InfoButton_AppliesAdditionalClass()
    {
        // Act
        var cut = RenderComponent<InfoButton>(parameters => parameters
            .Add(p => p.AdditionalClass, "custom-info"));

        // Assert
        cut.Find(".info-btn").ClassList.Should().Contain("custom-info");
    }

    [Fact]
    public void InfoButton_InvokesOnClick_WhenClicked()
    {
        // Arrange
        var clicked = false;
        var cut = RenderComponent<InfoButton>(parameters => parameters
            .Add(p => p.OnClick, () => clicked = true));

        // Act
        cut.Find(".info-btn").Click();

        // Assert
        clicked.Should().BeTrue();
    }

    [Fact]
    public void InfoButton_DefaultSize_HasNoSizeClass()
    {
        // Act
        var cut = RenderComponent<InfoButton>();

        // Assert
        cut.Find(".info-btn").ClassList.Should().NotContain("small");
        cut.Find(".info-btn").ClassList.Should().NotContain("large");
    }

    [Theory]
    [InlineData("")]
    [InlineData("small")]
    public void InfoButton_AcceptsValidSizes(string size)
    {
        // Act
        var cut = RenderComponent<InfoButton>(parameters => parameters
            .Add(p => p.Size, size));

        // Assert
        if (!string.IsNullOrEmpty(size))
        {
            cut.Find(".info-btn").ClassList.Should().Contain(size);
        }
    }

    [Fact]
    public void InfoButton_IsButton_Element()
    {
        // Act
        var cut = RenderComponent<InfoButton>();

        // Assert
        cut.Find(".info-btn").TagName.Should().Be("BUTTON");
    }
}
