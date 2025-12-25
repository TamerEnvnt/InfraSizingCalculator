using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.Shared;

namespace InfraSizingCalculator.UnitTests.Components;

/// <summary>
/// Tests for LoadingSpinner component
/// </summary>
public class LoadingSpinnerTests : TestContext
{
    [Fact]
    public void LoadingSpinner_RendersSpinner()
    {
        // Arrange & Act
        var cut = RenderComponent<LoadingSpinner>();

        // Assert
        cut.FindAll(".loading-spinner").Should().HaveCount(1);
    }

    [Fact]
    public void LoadingSpinner_AppliesMediumSizeByDefault()
    {
        // Arrange & Act
        var cut = RenderComponent<LoadingSpinner>();

        // Assert
        cut.Find(".loading-spinner").ClassList.Should().Contain("medium");
    }

    [Fact]
    public void LoadingSpinner_AppliesSmallSize()
    {
        // Arrange & Act
        var cut = RenderComponent<LoadingSpinner>(parameters => parameters
            .Add(p => p.Size, "small"));

        // Assert
        cut.Find(".loading-spinner").ClassList.Should().Contain("small");
    }

    [Fact]
    public void LoadingSpinner_AppliesLargeSize()
    {
        // Arrange & Act
        var cut = RenderComponent<LoadingSpinner>(parameters => parameters
            .Add(p => p.Size, "large"));

        // Assert
        cut.Find(".loading-spinner").ClassList.Should().Contain("large");
    }

    [Fact]
    public void LoadingSpinner_RendersMessage_WhenProvided()
    {
        // Arrange & Act
        var cut = RenderComponent<LoadingSpinner>(parameters => parameters
            .Add(p => p.Message, "Loading data..."));

        // Assert
        cut.Find(".loading-message").TextContent.Should().Be("Loading data...");
    }

    [Fact]
    public void LoadingSpinner_DoesNotRenderMessage_WhenNotProvided()
    {
        // Arrange & Act
        var cut = RenderComponent<LoadingSpinner>();

        // Assert
        cut.FindAll(".loading-message").Should().BeEmpty();
    }

    [Fact]
    public void LoadingSpinner_AppliesAdditionalClass()
    {
        // Arrange & Act
        var cut = RenderComponent<LoadingSpinner>(parameters => parameters
            .Add(p => p.AdditionalClass, "custom-loader"));

        // Assert
        cut.Find(".loading-container").ClassList.Should().Contain("custom-loader");
    }

    [Theory]
    [InlineData("small")]
    [InlineData("medium")]
    [InlineData("large")]
    public void LoadingSpinner_AcceptsValidSizes(string size)
    {
        // Arrange & Act
        var cut = RenderComponent<LoadingSpinner>(parameters => parameters
            .Add(p => p.Size, size));

        // Assert
        cut.Find(".loading-spinner").ClassList.Should().Contain(size);
    }
}
