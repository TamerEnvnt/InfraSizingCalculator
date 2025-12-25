using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.Modals;
using Microsoft.AspNetCore.Components;

namespace InfraSizingCalculator.UnitTests.Components;

/// <summary>
/// Tests for ModalBase component
/// </summary>
public class ModalBaseTests : TestContext
{
    [Fact]
    public void ModalBase_DoesNotRender_WhenClosed()
    {
        // Arrange & Act
        var cut = RenderComponent<ModalBase>(parameters => parameters
            .Add(p => p.IsOpen, false)
            .Add(p => p.Title, "Test Modal"));

        // Assert
        cut.FindAll(".modal-overlay").Should().BeEmpty();
    }

    [Fact]
    public void ModalBase_Renders_WhenOpen()
    {
        // Arrange & Act
        var cut = RenderComponent<ModalBase>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.Title, "Test Modal"));

        // Assert
        cut.FindAll(".modal-overlay").Should().HaveCount(1);
    }

    [Fact]
    public void ModalBase_DisplaysTitle()
    {
        // Arrange & Act
        var cut = RenderComponent<ModalBase>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.Title, "My Test Modal"));

        // Assert
        cut.Find(".modal-header h2").TextContent.Should().Be("My Test Modal");
    }

    [Fact]
    public void ModalBase_RendersChildContent()
    {
        // Arrange & Act
        var cut = RenderComponent<ModalBase>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.Title, "Test")
            .AddChildContent("<div class='test-content'>Hello World</div>"));

        // Assert
        cut.Find(".modal-body .test-content").TextContent.Should().Be("Hello World");
    }

    [Fact]
    public void ModalBase_RendersFooterContent_WhenProvided()
    {
        // Arrange & Act
        var cut = RenderComponent<ModalBase>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.Title, "Test")
            .Add(p => p.FooterContent, (Microsoft.AspNetCore.Components.RenderFragment)(builder =>
            {
                builder.AddMarkupContent(0, "<button class='test-btn'>OK</button>");
            })));

        // Assert
        cut.Find(".modal-footer .test-btn").TextContent.Should().Be("OK");
    }

    [Fact]
    public void ModalBase_DoesNotRenderFooter_WhenNoFooterContent()
    {
        // Arrange & Act
        var cut = RenderComponent<ModalBase>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.Title, "Test"));

        // Assert
        cut.FindAll(".modal-footer").Should().BeEmpty();
    }

    [Fact]
    public void ModalBase_InvokesOnClose_WhenCloseButtonClicked()
    {
        // Arrange
        var closed = false;
        var cut = RenderComponent<ModalBase>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.Title, "Test")
            .Add(p => p.OnClose, () => closed = true));

        // Act
        cut.Find(".modal-close").Click();

        // Assert
        closed.Should().BeTrue();
    }

    [Fact]
    public void ModalBase_InvokesOnClose_WhenOverlayClicked_AndCloseOnOverlayClickIsTrue()
    {
        // Arrange
        var closed = false;
        var cut = RenderComponent<ModalBase>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.Title, "Test")
            .Add(p => p.CloseOnOverlayClick, true)
            .Add(p => p.OnClose, () => closed = true));

        // Act
        cut.Find(".modal-overlay").Click();

        // Assert
        closed.Should().BeTrue();
    }

    [Fact]
    public void ModalBase_DoesNotInvokeOnClose_WhenOverlayClicked_AndCloseOnOverlayClickIsFalse()
    {
        // Arrange
        var closed = false;
        var cut = RenderComponent<ModalBase>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.Title, "Test")
            .Add(p => p.CloseOnOverlayClick, false)
            .Add(p => p.OnClose, () => closed = true));

        // Act
        cut.Find(".modal-overlay").Click();

        // Assert
        closed.Should().BeFalse();
    }

    [Fact]
    public void ModalBase_AppliesSmallSizeClass()
    {
        // Arrange & Act
        var cut = RenderComponent<ModalBase>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.Title, "Test")
            .Add(p => p.Size, "small"));

        // Assert
        cut.Find(".modal-content").ClassList.Should().Contain("small");
    }

    [Fact]
    public void ModalBase_AppliesLargeSizeClass()
    {
        // Arrange & Act
        var cut = RenderComponent<ModalBase>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.Title, "Test")
            .Add(p => p.Size, "large"));

        // Assert
        cut.Find(".modal-content").ClassList.Should().Contain("large");
    }

    [Fact]
    public void ModalBase_AppliesAdditionalClass()
    {
        // Arrange & Act
        var cut = RenderComponent<ModalBase>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.Title, "Test")
            .Add(p => p.AdditionalClass, "custom-modal"));

        // Assert
        cut.Find(".modal-content").ClassList.Should().Contain("custom-modal");
    }

    [Fact]
    public void ModalBase_AppliesBodyClass()
    {
        // Arrange & Act
        var cut = RenderComponent<ModalBase>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.Title, "Test")
            .Add(p => p.BodyClass, "scrollable-body"));

        // Assert
        cut.Find(".modal-body").ClassList.Should().Contain("scrollable-body");
    }

    [Fact]
    public void ModalBase_HasCloseButton()
    {
        // Arrange & Act
        var cut = RenderComponent<ModalBase>(parameters => parameters
            .Add(p => p.IsOpen, true)
            .Add(p => p.Title, "Test"));

        // Assert
        var closeBtn = cut.Find(".modal-close");
        closeBtn.TextContent.Should().Be("×");
    }
}
