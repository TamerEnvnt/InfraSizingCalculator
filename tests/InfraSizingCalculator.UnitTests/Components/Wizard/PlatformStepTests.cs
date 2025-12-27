using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.Wizard.Steps;
using InfraSizingCalculator.Components.Shared;
using InfraSizingCalculator.Models.Enums;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components.Wizard;

/// <summary>
/// Tests for PlatformStep component - Step 1 of the wizard
/// </summary>
public class PlatformStepTests : TestContext
{
    #region Rendering Tests

    [Fact]
    public void PlatformStep_RendersWithDefaultState()
    {
        // Act
        var cut = RenderComponent<PlatformStep>();

        // Assert
        cut.FindComponent<SectionHeader>().Should().NotBeNull();
        cut.FindAll(".selection-grid").Should().HaveCount(1);
        cut.FindComponents<SelectionCard>().Should().HaveCount(2);
    }

    [Fact]
    public void PlatformStep_RendersNativeApplicationCard()
    {
        // Act
        var cut = RenderComponent<PlatformStep>();

        // Assert
        var cards = cut.FindComponents<SelectionCard>();
        cards[0].Instance.Title.Should().Be("Native Applications");
        cards[0].Instance.Description.Should().Be(".NET, Java, Node.js, Python, Go");
        cards[0].Instance.Icon.Should().Be("Code");
    }

    [Fact]
    public void PlatformStep_RendersLowCodePlatformCard()
    {
        // Act
        var cut = RenderComponent<PlatformStep>();

        // Assert
        var cards = cut.FindComponents<SelectionCard>();
        cards[1].Instance.Title.Should().Be("Low-Code Platform");
        cards[1].Instance.Description.Should().Be("Mendix, OutSystems");
        cards[1].Instance.Icon.Should().Be("Low");
    }

    [Fact]
    public void PlatformStep_RendersSectionHeaderWithInfoButton()
    {
        // Act
        var cut = RenderComponent<PlatformStep>();

        // Assert
        var header = cut.FindComponent<SectionHeader>();
        header.Instance.Title.Should().Be("Select Platform Type");
        header.Instance.ShowInfoButton.Should().BeTrue();
    }

    #endregion

    #region Selection State Tests

    [Fact]
    public void PlatformStep_NoSelectionByDefault()
    {
        // Act
        var cut = RenderComponent<PlatformStep>();

        // Assert
        var cards = cut.FindComponents<SelectionCard>();
        cards[0].Instance.IsSelected.Should().BeFalse();
        cards[1].Instance.IsSelected.Should().BeFalse();
    }

    [Theory]
    [InlineData(PlatformType.Native, 0)]
    [InlineData(PlatformType.LowCode, 1)]
    public void PlatformStep_ShowsCorrectCardAsSelected(PlatformType platform, int expectedIndex)
    {
        // Act
        var cut = RenderComponent<PlatformStep>(parameters => parameters
            .Add(p => p.SelectedPlatform, platform));

        // Assert
        var cards = cut.FindComponents<SelectionCard>();
        cards[expectedIndex].Instance.IsSelected.Should().BeTrue();
        cards[1 - expectedIndex].Instance.IsSelected.Should().BeFalse();
    }

    [Fact]
    public void PlatformStep_NullSelectedPlatform_ShowsNoSelection()
    {
        // Act
        var cut = RenderComponent<PlatformStep>(parameters => parameters
            .Add(p => p.SelectedPlatform, (PlatformType?)null));

        // Assert
        var cards = cut.FindComponents<SelectionCard>();
        cards.Should().OnlyContain(c => !c.Instance.IsSelected);
    }

    #endregion

    #region Event Callback Tests

    [Fact]
    public async Task PlatformStep_SelectingNative_InvokesOnPlatformChange()
    {
        // Arrange
        PlatformType? selectedPlatform = null;
        var cut = RenderComponent<PlatformStep>(parameters => parameters
            .Add(p => p.OnPlatformChange, EventCallback.Factory.Create<PlatformType>(this,
                p => selectedPlatform = p)));

        // Act
        var nativeCard = cut.FindComponents<SelectionCard>()[0];
        await cut.InvokeAsync(() => nativeCard.Instance.OnClick.InvokeAsync());

        // Assert
        selectedPlatform.Should().Be(PlatformType.Native);
    }

    [Fact]
    public async Task PlatformStep_SelectingLowCode_InvokesOnPlatformChange()
    {
        // Arrange
        PlatformType? selectedPlatform = null;
        var cut = RenderComponent<PlatformStep>(parameters => parameters
            .Add(p => p.OnPlatformChange, EventCallback.Factory.Create<PlatformType>(this,
                p => selectedPlatform = p)));

        // Act
        var lowCodeCard = cut.FindComponents<SelectionCard>()[1];
        await cut.InvokeAsync(() => lowCodeCard.Instance.OnClick.InvokeAsync());

        // Assert
        selectedPlatform.Should().Be(PlatformType.LowCode);
    }

    [Fact]
    public async Task PlatformStep_ClickingInfoButton_InvokesOnInfoClick()
    {
        // Arrange
        bool infoClicked = false;
        var cut = RenderComponent<PlatformStep>(parameters => parameters
            .Add(p => p.OnInfoClick, EventCallback.Factory.Create(this, () => infoClicked = true)));

        // Act
        var header = cut.FindComponent<SectionHeader>();
        await cut.InvokeAsync(() => header.Instance.OnInfoClick.InvokeAsync());

        // Assert
        infoClicked.Should().BeTrue();
    }

    [Fact]
    public async Task PlatformStep_NoDelegate_DoesNotThrowOnPlatformSelect()
    {
        // Arrange
        var cut = RenderComponent<PlatformStep>();

        // Act & Assert - Should not throw
        var action = async () =>
        {
            var card = cut.FindComponents<SelectionCard>()[0];
            await cut.InvokeAsync(() => card.Instance.OnClick.InvokeAsync());
        };

        await action.Should().NotThrowAsync();
    }

    #endregion

    #region Parameter Update Tests

    [Fact]
    public void PlatformStep_UpdatesSelectionWhenParameterChanges()
    {
        // Arrange
        var cut = RenderComponent<PlatformStep>(parameters => parameters
            .Add(p => p.SelectedPlatform, PlatformType.Native));

        // Assert initial state
        var cards = cut.FindComponents<SelectionCard>();
        cards[0].Instance.IsSelected.Should().BeTrue();

        // Act - Change to LowCode
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.SelectedPlatform, PlatformType.LowCode));

        // Assert
        cards = cut.FindComponents<SelectionCard>();
        cards[0].Instance.IsSelected.Should().BeFalse();
        cards[1].Instance.IsSelected.Should().BeTrue();
    }

    [Fact]
    public void PlatformStep_ClearsSelectionWhenSetToNull()
    {
        // Arrange
        var cut = RenderComponent<PlatformStep>(parameters => parameters
            .Add(p => p.SelectedPlatform, PlatformType.Native));

        // Act - Clear selection
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.SelectedPlatform, (PlatformType?)null));

        // Assert
        var cards = cut.FindComponents<SelectionCard>();
        cards.Should().OnlyContain(c => !c.Instance.IsSelected);
    }

    #endregion

    #region Accessibility Tests

    [Fact]
    public void PlatformStep_CardsHaveDescriptiveContent()
    {
        // Act
        var cut = RenderComponent<PlatformStep>();

        // Assert - Verify cards have titles and descriptions
        var cards = cut.FindComponents<SelectionCard>();
        cards.Should().AllSatisfy(card =>
        {
            card.Instance.Title.Should().NotBeNullOrEmpty();
            card.Instance.Description.Should().NotBeNullOrEmpty();
        });
    }

    [Fact]
    public void PlatformStep_HasSelectionGrid()
    {
        // Act
        var cut = RenderComponent<PlatformStep>();

        // Assert
        var grid = cut.Find(".selection-grid");
        grid.Should().NotBeNull();
        grid.Children.Should().HaveCount(2);
    }

    #endregion
}
