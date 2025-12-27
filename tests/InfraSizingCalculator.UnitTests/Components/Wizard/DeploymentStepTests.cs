using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.Wizard.Steps;
using InfraSizingCalculator.Components.Shared;
using InfraSizingCalculator.Models.Enums;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components.Wizard;

/// <summary>
/// Tests for DeploymentStep component - Step 2 of the wizard
/// </summary>
public class DeploymentStepTests : TestContext
{
    #region Rendering Tests

    [Fact]
    public void DeploymentStep_RendersWithDefaultState()
    {
        // Act
        var cut = RenderComponent<DeploymentStep>();

        // Assert
        cut.FindComponent<SectionHeader>().Should().NotBeNull();
        cut.FindAll(".selection-grid").Should().HaveCount(1);
        cut.FindComponents<SelectionCard>().Should().HaveCount(2);
    }

    [Fact]
    public void DeploymentStep_RendersKubernetesCard()
    {
        // Act
        var cut = RenderComponent<DeploymentStep>();

        // Assert
        var cards = cut.FindComponents<SelectionCard>();
        cards[0].Instance.Title.Should().Be("Kubernetes");
        cards[0].Instance.Description.Should().Be("Container orchestration with auto-scaling and self-healing");
        cards[0].Instance.Icon.Should().Be("K8s");
    }

    [Fact]
    public void DeploymentStep_RendersVirtualMachinesCard()
    {
        // Act
        var cut = RenderComponent<DeploymentStep>();

        // Assert
        var cards = cut.FindComponents<SelectionCard>();
        cards[1].Instance.Title.Should().Be("Virtual Machines");
        cards[1].Instance.Description.Should().Be("Traditional VM deployment with dedicated servers");
        cards[1].Instance.Icon.Should().Be("VM");
    }

    [Fact]
    public void DeploymentStep_RendersSectionHeaderWithInfoButton()
    {
        // Act
        var cut = RenderComponent<DeploymentStep>();

        // Assert
        var header = cut.FindComponent<SectionHeader>();
        header.Instance.Title.Should().Be("Select Deployment Model");
        header.Instance.ShowInfoButton.Should().BeTrue();
    }

    #endregion

    #region Selection State Tests

    [Fact]
    public void DeploymentStep_NoSelectionByDefault()
    {
        // Act
        var cut = RenderComponent<DeploymentStep>();

        // Assert
        var cards = cut.FindComponents<SelectionCard>();
        cards[0].Instance.IsSelected.Should().BeFalse();
        cards[1].Instance.IsSelected.Should().BeFalse();
    }

    [Theory]
    [InlineData(DeploymentModel.Kubernetes, 0)]
    [InlineData(DeploymentModel.VMs, 1)]
    public void DeploymentStep_ShowsCorrectCardAsSelected(DeploymentModel deployment, int expectedIndex)
    {
        // Act
        var cut = RenderComponent<DeploymentStep>(parameters => parameters
            .Add(p => p.SelectedDeployment, deployment));

        // Assert
        var cards = cut.FindComponents<SelectionCard>();
        cards[expectedIndex].Instance.IsSelected.Should().BeTrue();
        cards[1 - expectedIndex].Instance.IsSelected.Should().BeFalse();
    }

    [Fact]
    public void DeploymentStep_NullSelectedDeployment_ShowsNoSelection()
    {
        // Act
        var cut = RenderComponent<DeploymentStep>(parameters => parameters
            .Add(p => p.SelectedDeployment, (DeploymentModel?)null));

        // Assert
        var cards = cut.FindComponents<SelectionCard>();
        cards.Should().OnlyContain(c => !c.Instance.IsSelected);
    }

    #endregion

    #region Event Callback Tests

    [Fact]
    public async Task DeploymentStep_SelectingKubernetes_InvokesOnDeploymentChange()
    {
        // Arrange
        DeploymentModel? selectedDeployment = null;
        var cut = RenderComponent<DeploymentStep>(parameters => parameters
            .Add(p => p.OnDeploymentChange, EventCallback.Factory.Create<DeploymentModel>(this,
                d => selectedDeployment = d)));

        // Act
        var k8sCard = cut.FindComponents<SelectionCard>()[0];
        await cut.InvokeAsync(() => k8sCard.Instance.OnClick.InvokeAsync());

        // Assert
        selectedDeployment.Should().Be(DeploymentModel.Kubernetes);
    }

    [Fact]
    public async Task DeploymentStep_SelectingVMs_InvokesOnDeploymentChange()
    {
        // Arrange
        DeploymentModel? selectedDeployment = null;
        var cut = RenderComponent<DeploymentStep>(parameters => parameters
            .Add(p => p.OnDeploymentChange, EventCallback.Factory.Create<DeploymentModel>(this,
                d => selectedDeployment = d)));

        // Act
        var vmCard = cut.FindComponents<SelectionCard>()[1];
        await cut.InvokeAsync(() => vmCard.Instance.OnClick.InvokeAsync());

        // Assert
        selectedDeployment.Should().Be(DeploymentModel.VMs);
    }

    [Fact]
    public async Task DeploymentStep_ClickingInfoButton_InvokesOnInfoClick()
    {
        // Arrange
        bool infoClicked = false;
        var cut = RenderComponent<DeploymentStep>(parameters => parameters
            .Add(p => p.OnInfoClick, EventCallback.Factory.Create(this, () => infoClicked = true)));

        // Act
        var header = cut.FindComponent<SectionHeader>();
        await cut.InvokeAsync(() => header.Instance.OnInfoClick.InvokeAsync());

        // Assert
        infoClicked.Should().BeTrue();
    }

    [Fact]
    public async Task DeploymentStep_NoDelegate_DoesNotThrowOnDeploymentSelect()
    {
        // Arrange
        var cut = RenderComponent<DeploymentStep>();

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
    public void DeploymentStep_UpdatesSelectionWhenParameterChanges()
    {
        // Arrange
        var cut = RenderComponent<DeploymentStep>(parameters => parameters
            .Add(p => p.SelectedDeployment, DeploymentModel.Kubernetes));

        // Assert initial state
        var cards = cut.FindComponents<SelectionCard>();
        cards[0].Instance.IsSelected.Should().BeTrue();

        // Act - Change to VMs
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.SelectedDeployment, DeploymentModel.VMs));

        // Assert
        cards = cut.FindComponents<SelectionCard>();
        cards[0].Instance.IsSelected.Should().BeFalse();
        cards[1].Instance.IsSelected.Should().BeTrue();
    }

    [Fact]
    public void DeploymentStep_ClearsSelectionWhenSetToNull()
    {
        // Arrange
        var cut = RenderComponent<DeploymentStep>(parameters => parameters
            .Add(p => p.SelectedDeployment, DeploymentModel.Kubernetes));

        // Act - Clear selection
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.SelectedDeployment, (DeploymentModel?)null));

        // Assert
        var cards = cut.FindComponents<SelectionCard>();
        cards.Should().OnlyContain(c => !c.Instance.IsSelected);
    }

    #endregion

    #region Accessibility Tests

    [Fact]
    public void DeploymentStep_CardsHaveDescriptiveContent()
    {
        // Act
        var cut = RenderComponent<DeploymentStep>();

        // Assert
        var cards = cut.FindComponents<SelectionCard>();
        cards.Should().AllSatisfy(card =>
        {
            card.Instance.Title.Should().NotBeNullOrEmpty();
            card.Instance.Description.Should().NotBeNullOrEmpty();
        });
    }

    [Fact]
    public void DeploymentStep_HasSelectionGrid()
    {
        // Act
        var cut = RenderComponent<DeploymentStep>();

        // Assert
        var grid = cut.Find(".selection-grid");
        grid.Should().NotBeNull();
        grid.Children.Should().HaveCount(2);
    }

    #endregion
}
