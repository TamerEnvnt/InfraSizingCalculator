using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.Shared;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components;

/// <summary>
/// Tests for SectionHeader component
/// </summary>
public class SectionHeaderTests : TestContext
{
    [Fact]
    public void SectionHeader_RendersTitle()
    {
        // Act
        var cut = RenderComponent<SectionHeader>(parameters => parameters
            .Add(p => p.Title, "Test Section"));

        // Assert
        cut.Find(".section-title").TextContent.Should().Be("Test Section");
    }

    [Fact]
    public void SectionHeader_RendersEmptyTitle_WhenNotProvided()
    {
        // Act
        var cut = RenderComponent<SectionHeader>();

        // Assert
        cut.Find(".section-title").TextContent.Should().BeEmpty();
    }

    [Fact]
    public void SectionHeader_RendersInfoButton_WhenShowInfoButtonIsTrue()
    {
        // Act
        var cut = RenderComponent<SectionHeader>(parameters => parameters
            .Add(p => p.Title, "Test")
            .Add(p => p.ShowInfoButton, true));

        // Assert
        cut.FindAll(".info-btn").Should().HaveCount(1);
        cut.Find(".info-btn").TextContent.Should().Be("?");
    }

    [Fact]
    public void SectionHeader_HidesInfoButton_WhenShowInfoButtonIsFalse()
    {
        // Act
        var cut = RenderComponent<SectionHeader>(parameters => parameters
            .Add(p => p.Title, "Test")
            .Add(p => p.ShowInfoButton, false));

        // Assert
        cut.FindAll(".info-btn").Should().BeEmpty();
    }

    [Fact]
    public void SectionHeader_InvokesOnInfoClick_WhenInfoButtonClicked()
    {
        // Arrange
        var clicked = false;
        var cut = RenderComponent<SectionHeader>(parameters => parameters
            .Add(p => p.Title, "Test")
            .Add(p => p.ShowInfoButton, true)
            .Add(p => p.OnInfoClick, () => clicked = true));

        // Act
        cut.Find(".info-btn").Click();

        // Assert
        clicked.Should().BeTrue();
    }

    [Fact]
    public void SectionHeader_AppliesAdditionalClass()
    {
        // Act
        var cut = RenderComponent<SectionHeader>(parameters => parameters
            .Add(p => p.Title, "Test")
            .Add(p => p.AdditionalClass, "custom-header"));

        // Assert
        cut.Find(".section-header").ClassList.Should().Contain("custom-header");
    }

    [Fact]
    public void SectionHeader_RendersChildContent()
    {
        // Act
        var cut = RenderComponent<SectionHeader>(parameters => parameters
            .Add(p => p.Title, "Test")
            .AddChildContent("<span class='custom-child'>Child Content</span>"));

        // Assert
        cut.Find(".custom-child").TextContent.Should().Be("Child Content");
    }

    [Fact]
    public void SectionHeader_HasSectionHeaderContainer()
    {
        // Act
        var cut = RenderComponent<SectionHeader>(parameters => parameters
            .Add(p => p.Title, "Test"));

        // Assert
        cut.FindAll(".section-header").Should().HaveCount(1);
    }

    [Theory]
    [InlineData("Configuration Settings")]
    [InlineData("Result Summary")]
    [InlineData("Advanced Options")]
    [InlineData("")]
    public void SectionHeader_AcceptsVariousTitles(string title)
    {
        // Act
        var cut = RenderComponent<SectionHeader>(parameters => parameters
            .Add(p => p.Title, title));

        // Assert
        cut.Find(".section-title").TextContent.Should().Be(title);
    }

    [Fact]
    public void SectionHeader_RendersWithAllOptions()
    {
        // Arrange
        var clicked = false;

        // Act
        var cut = RenderComponent<SectionHeader>(parameters => parameters
            .Add(p => p.Title, "Complete Section")
            .Add(p => p.ShowInfoButton, true)
            .Add(p => p.OnInfoClick, () => clicked = true)
            .Add(p => p.AdditionalClass, "full-featured")
            .AddChildContent("<button>Action</button>"));

        // Assert
        cut.Find(".section-title").TextContent.Should().Be("Complete Section");
        cut.FindAll(".info-btn").Should().HaveCount(1);
        cut.Find(".section-header").ClassList.Should().Contain("full-featured");
        cut.FindAll("button").Should().HaveCount(2); // info-btn + child button
    }
}
