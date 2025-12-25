using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.Shared;

namespace InfraSizingCalculator.UnitTests.Components;

/// <summary>
/// Tests for FilterButtons component
/// </summary>
public class FilterButtonsTests : TestContext
{
    private static readonly FilterButtons.FilterOption[] DefaultOptions = new[]
    {
        new FilterButtons.FilterOption("all", "All"),
        new FilterButtons.FilterOption("on-prem", "On-Premises"),
        new FilterButtons.FilterOption("cloud", "Cloud")
    };

    [Fact]
    public void FilterButtons_RendersAllOptions()
    {
        // Arrange & Act
        var cut = RenderComponent<FilterButtons>(parameters => parameters
            .Add(p => p.Options, DefaultOptions)
            .Add(p => p.SelectedValue, "all"));

        // Assert
        var buttons = cut.FindAll(".filter-btn");
        buttons.Should().HaveCount(3);
    }

    [Fact]
    public void FilterButtons_RendersLabel_WhenProvided()
    {
        // Arrange & Act
        var cut = RenderComponent<FilterButtons>(parameters => parameters
            .Add(p => p.Options, DefaultOptions)
            .Add(p => p.SelectedValue, "all")
            .Add(p => p.Label, "Filter:"));

        // Assert
        cut.Find(".filter-label").TextContent.Should().Be("Filter:");
    }

    [Fact]
    public void FilterButtons_DoesNotRenderLabel_WhenNotProvided()
    {
        // Arrange & Act
        var cut = RenderComponent<FilterButtons>(parameters => parameters
            .Add(p => p.Options, DefaultOptions)
            .Add(p => p.SelectedValue, "all"));

        // Assert
        cut.FindAll(".filter-label").Should().BeEmpty();
    }

    [Fact]
    public void FilterButtons_MarksSelectedOptionAsActive()
    {
        // Arrange & Act
        var cut = RenderComponent<FilterButtons>(parameters => parameters
            .Add(p => p.Options, DefaultOptions)
            .Add(p => p.SelectedValue, "on-prem"));

        // Assert
        var buttons = cut.FindAll(".filter-btn");
        buttons[0].ClassList.Should().NotContain("active"); // "all"
        buttons[1].ClassList.Should().Contain("active");    // "on-prem"
        buttons[2].ClassList.Should().NotContain("active"); // "cloud"
    }

    [Fact]
    public void FilterButtons_DisplaysCorrectLabels()
    {
        // Arrange & Act
        var cut = RenderComponent<FilterButtons>(parameters => parameters
            .Add(p => p.Options, DefaultOptions)
            .Add(p => p.SelectedValue, "all"));

        // Assert
        var buttons = cut.FindAll(".filter-btn");
        buttons[0].TextContent.Should().Be("All");
        buttons[1].TextContent.Should().Be("On-Premises");
        buttons[2].TextContent.Should().Be("Cloud");
    }

    [Fact]
    public void FilterButtons_InvokesOnValueChanged_WhenButtonClicked()
    {
        // Arrange
        string? selectedValue = null;
        var cut = RenderComponent<FilterButtons>(parameters => parameters
            .Add(p => p.Options, DefaultOptions)
            .Add(p => p.SelectedValue, "all")
            .Add(p => p.OnValueChanged, value => selectedValue = value));

        // Act
        cut.FindAll(".filter-btn")[2].Click(); // Click "Cloud"

        // Assert
        selectedValue.Should().Be("cloud");
    }

    [Fact]
    public void FilterButtons_AppliesAdditionalClass()
    {
        // Arrange & Act
        var cut = RenderComponent<FilterButtons>(parameters => parameters
            .Add(p => p.Options, DefaultOptions)
            .Add(p => p.SelectedValue, "all")
            .Add(p => p.AdditionalClass, "custom-filters"));

        // Assert
        cut.Find(".filter-buttons").ClassList.Should().Contain("custom-filters");
    }

    [Fact]
    public void FilterButtons_HandlesEmptyOptions()
    {
        // Arrange & Act
        var cut = RenderComponent<FilterButtons>(parameters => parameters
            .Add(p => p.Options, Array.Empty<FilterButtons.FilterOption>())
            .Add(p => p.SelectedValue, ""));

        // Assert
        cut.FindAll(".filter-btn").Should().BeEmpty();
    }

    [Fact]
    public void FilterButtons_HandlesSingleOption()
    {
        // Arrange
        var singleOption = new[] { new FilterButtons.FilterOption("only", "Only Option") };

        // Act
        var cut = RenderComponent<FilterButtons>(parameters => parameters
            .Add(p => p.Options, singleOption)
            .Add(p => p.SelectedValue, "only"));

        // Assert
        var buttons = cut.FindAll(".filter-btn");
        buttons.Should().HaveCount(1);
        buttons[0].ClassList.Should().Contain("active");
    }

    [Fact]
    public void FilterButtons_HandlesNoSelection()
    {
        // Arrange & Act
        var cut = RenderComponent<FilterButtons>(parameters => parameters
            .Add(p => p.Options, DefaultOptions)
            .Add(p => p.SelectedValue, ""));

        // Assert
        var buttons = cut.FindAll(".filter-btn");
        buttons.Should().AllSatisfy(btn => btn.ClassList.Should().NotContain("active"));
    }
}
