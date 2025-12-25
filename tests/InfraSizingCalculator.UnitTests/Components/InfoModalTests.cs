using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.Modals;

namespace InfraSizingCalculator.UnitTests.Components;

/// <summary>
/// Tests for InfoModal component
/// </summary>
public class InfoModalTests : TestContext
{
    [Fact]
    public void InfoModal_DoesNotRender_WhenClosed()
    {
        // Arrange & Act
        var cut = RenderComponent<InfoModal>(parameters => parameters
            .Add(p => p.IsOpen, false)
            .Add(p => p.Title, "Info")
            .Add(p => p.Content, "Test content"));

        // Assert
        cut.FindAll(".modal-overlay").Should().BeEmpty();
    }

    [Fact]
    public void InfoModal_Renders_WhenOpen()
    {
        // Arrange & Act
        var cut = RenderComponent<InfoModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.Title, "Info")
            .Add(p => p.Content, "Test content"));

        // Assert
        cut.FindAll(".modal-overlay").Should().HaveCount(1);
    }

    [Fact]
    public void InfoModal_DisplaysTitle()
    {
        // Arrange & Act
        var cut = RenderComponent<InfoModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.Title, "Help Information")
            .Add(p => p.Content, "Test"));

        // Assert
        cut.Find(".modal-header h2").TextContent.Should().Be("Help Information");
    }

    [Fact]
    public void InfoModal_RendersHtmlContent()
    {
        // Arrange
        var htmlContent = "<h3>Section Title</h3><p>Paragraph content</p>";

        // Act
        var cut = RenderComponent<InfoModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.Title, "Info")
            .Add(p => p.Content, htmlContent));

        // Assert
        cut.Find(".info-content h3").TextContent.Should().Be("Section Title");
        cut.Find(".info-content p").TextContent.Should().Be("Paragraph content");
    }

    [Fact]
    public void InfoModal_HasCloseButton()
    {
        // Arrange & Act
        var cut = RenderComponent<InfoModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.Title, "Info")
            .Add(p => p.Content, "Test"));

        // Assert
        cut.FindAll("button").Should().Contain(b => b.TextContent == "Close");
    }

    [Fact]
    public void InfoModal_InvokesOnClose_WhenCloseButtonClicked()
    {
        // Arrange
        var closed = false;
        var cut = RenderComponent<InfoModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.Title, "Info")
            .Add(p => p.Content, "Test")
            .Add(p => p.OnClose, () => closed = true));

        // Act - Click the "Close" button in footer
        var closeButton = cut.FindAll("button").First(b => b.TextContent == "Close");
        closeButton.Click();

        // Assert
        closed.Should().BeTrue();
    }

    [Fact]
    public void InfoModal_InvokesOnClose_WhenXButtonClicked()
    {
        // Arrange
        var closed = false;
        var cut = RenderComponent<InfoModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.Title, "Info")
            .Add(p => p.Content, "Test")
            .Add(p => p.OnClose, () => closed = true));

        // Act - Click the X button
        cut.Find(".modal-close").Click();

        // Assert
        closed.Should().BeTrue();
    }

    [Fact]
    public void InfoModal_HasInfoContentClass()
    {
        // Arrange & Act
        var cut = RenderComponent<InfoModal>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.Title, "Info")
            .Add(p => p.Content, "Test"));

        // Assert
        cut.FindAll(".info-content").Should().HaveCount(1);
    }
}
