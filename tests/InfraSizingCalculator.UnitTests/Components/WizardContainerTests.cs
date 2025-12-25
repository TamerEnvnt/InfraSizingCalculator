using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.Wizard;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components;

/// <summary>
/// Tests for WizardContainer component
/// </summary>
public class WizardContainerTests : TestContext
{
    private static readonly string[] TestStepLabels = { "Platform", "Deploy", "Tech", "Config", "Results" };

    [Fact]
    public void WizardContainer_RendersWithDefaultTitle()
    {
        // Act
        var cut = RenderComponent<WizardContainer>(parameters => parameters
            .Add(p => p.TotalSteps, 5)
            .Add(p => p.CurrentStep, 1)
            .Add(p => p.StepLabels, TestStepLabels));

        // Assert
        var header = cut.Find(".wizard-header h1");
        header.TextContent.Should().Be("Infrastructure Sizing Calculator");
    }

    [Fact]
    public void WizardContainer_RendersWithCustomTitle()
    {
        // Arrange
        var customTitle = "Custom Wizard Title";

        // Act
        var cut = RenderComponent<WizardContainer>(parameters => parameters
            .Add(p => p.Title, customTitle)
            .Add(p => p.TotalSteps, 5)
            .Add(p => p.CurrentStep, 1)
            .Add(p => p.StepLabels, TestStepLabels));

        // Assert
        var header = cut.Find(".wizard-header h1");
        header.TextContent.Should().Be(customTitle);
    }

    [Fact]
    public void WizardContainer_AppliesAdditionalClass()
    {
        // Act
        var cut = RenderComponent<WizardContainer>(parameters => parameters
            .Add(p => p.AdditionalClass, "custom-wizard")
            .Add(p => p.TotalSteps, 5)
            .Add(p => p.CurrentStep, 1)
            .Add(p => p.StepLabels, TestStepLabels));

        // Assert
        cut.Find(".wizard-container").ClassList.Should().Contain("custom-wizard");
    }

    [Fact]
    public void WizardContainer_RendersHeaderActions()
    {
        // Act
        var cut = RenderComponent<WizardContainer>(parameters => parameters
            .Add(p => p.TotalSteps, 5)
            .Add(p => p.CurrentStep, 1)
            .Add(p => p.StepLabels, TestStepLabels)
            .Add(p => p.HeaderActions, builder =>
            {
                builder.OpenElement(0, "button");
                builder.AddAttribute(1, "class", "test-action-btn");
                builder.AddContent(2, "Test Action");
                builder.CloseElement();
            }));

        // Assert
        var headerActions = cut.Find(".header-actions");
        headerActions.Should().NotBeNull();
        cut.Find(".test-action-btn").TextContent.Should().Be("Test Action");
    }

    [Fact]
    public void WizardContainer_DoesNotRenderHeaderActions_WhenNull()
    {
        // Act
        var cut = RenderComponent<WizardContainer>(parameters => parameters
            .Add(p => p.TotalSteps, 5)
            .Add(p => p.CurrentStep, 1)
            .Add(p => p.StepLabels, TestStepLabels));

        // Assert
        cut.FindAll(".header-actions").Should().BeEmpty();
    }

    [Fact]
    public void WizardContainer_RendersChildContent()
    {
        // Act
        var cut = RenderComponent<WizardContainer>(parameters => parameters
            .Add(p => p.TotalSteps, 5)
            .Add(p => p.CurrentStep, 1)
            .Add(p => p.StepLabels, TestStepLabels)
            .AddChildContent("<div class='test-content'>Test Content</div>"));

        // Assert
        var content = cut.Find(".wizard-content .test-content");
        content.TextContent.Should().Be("Test Content");
    }

    [Fact]
    public void WizardContainer_RendersStepper()
    {
        // Act
        var cut = RenderComponent<WizardContainer>(parameters => parameters
            .Add(p => p.TotalSteps, 5)
            .Add(p => p.CurrentStep, 3)
            .Add(p => p.StepLabels, TestStepLabels));

        // Assert
        cut.FindComponent<WizardStepper>().Should().NotBeNull();
        cut.FindAll(".stepper-step").Should().HaveCount(5);
    }

    [Fact]
    public void WizardContainer_PassesCorrectPropsToStepper()
    {
        // Act
        var cut = RenderComponent<WizardContainer>(parameters => parameters
            .Add(p => p.TotalSteps, 5)
            .Add(p => p.CurrentStep, 3)
            .Add(p => p.StepLabels, TestStepLabels));

        // Assert
        var stepper = cut.FindComponent<WizardStepper>();
        stepper.Instance.TotalSteps.Should().Be(5);
        stepper.Instance.CurrentStep.Should().Be(3);
        stepper.Instance.StepLabels.Should().BeEquivalentTo(TestStepLabels);
    }

    [Fact]
    public void WizardContainer_ShowsNavigation_ByDefault()
    {
        // Act
        var cut = RenderComponent<WizardContainer>(parameters => parameters
            .Add(p => p.TotalSteps, 5)
            .Add(p => p.CurrentStep, 1)
            .Add(p => p.StepLabels, TestStepLabels));

        // Assert
        cut.FindComponent<WizardNavigation>().Should().NotBeNull();
    }

    [Fact]
    public void WizardContainer_HidesNavigation_WhenShowNavigationIsFalse()
    {
        // Act
        var cut = RenderComponent<WizardContainer>(parameters => parameters
            .Add(p => p.TotalSteps, 5)
            .Add(p => p.CurrentStep, 1)
            .Add(p => p.StepLabels, TestStepLabels)
            .Add(p => p.ShowNavigation, false));

        // Assert
        cut.FindComponents<WizardNavigation>().Should().BeEmpty();
    }

    [Fact]
    public void WizardContainer_NavigationShowsBackButton_OnlyAfterFirstStep()
    {
        // Arrange & Act - First step
        var cut = RenderComponent<WizardContainer>(parameters => parameters
            .Add(p => p.TotalSteps, 5)
            .Add(p => p.CurrentStep, 1)
            .Add(p => p.StepLabels, TestStepLabels));

        // Assert - Back button should not be shown
        var nav = cut.FindComponent<WizardNavigation>();
        nav.Instance.ShowBackButton.Should().BeFalse();
        nav.Instance.CanGoBack.Should().BeFalse();

        // Act - Move to step 2
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.CurrentStep, 2));

        // Assert - Back button should be shown
        nav.Instance.ShowBackButton.Should().BeTrue();
        nav.Instance.CanGoBack.Should().BeTrue();
    }

    [Fact]
    public void WizardContainer_NavigationShowsCalculateButton_OnLastStep()
    {
        // Arrange & Act - Second to last step
        var cut = RenderComponent<WizardContainer>(parameters => parameters
            .Add(p => p.TotalSteps, 5)
            .Add(p => p.CurrentStep, 4)
            .Add(p => p.StepLabels, TestStepLabels));

        // Assert
        var nav = cut.FindComponent<WizardNavigation>();
        nav.Instance.IsLastStep.Should().BeFalse();

        // Act - Move to last step
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.CurrentStep, 5));

        // Assert
        nav.Instance.IsLastStep.Should().BeTrue();
    }

    [Fact]
    public void WizardContainer_PassesCanProceedToNavigation()
    {
        // Act
        var cut = RenderComponent<WizardContainer>(parameters => parameters
            .Add(p => p.TotalSteps, 5)
            .Add(p => p.CurrentStep, 1)
            .Add(p => p.StepLabels, TestStepLabels)
            .Add(p => p.CanProceed, true));

        // Assert
        var nav = cut.FindComponent<WizardNavigation>();
        nav.Instance.CanProceed.Should().BeTrue();

        // Act - Change to false
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.CanProceed, false));

        // Assert
        nav.Instance.CanProceed.Should().BeFalse();
    }

    [Fact]
    public void WizardContainer_PassesIsCalculatingToNavigation()
    {
        // Act
        var cut = RenderComponent<WizardContainer>(parameters => parameters
            .Add(p => p.TotalSteps, 5)
            .Add(p => p.CurrentStep, 5)
            .Add(p => p.StepLabels, TestStepLabels)
            .Add(p => p.IsCalculating, false));

        // Assert
        var nav = cut.FindComponent<WizardNavigation>();
        nav.Instance.IsCalculating.Should().BeFalse();

        // Act - Set calculating
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.IsCalculating, true));

        // Assert
        nav.Instance.IsCalculating.Should().BeTrue();
    }

    [Fact]
    public void WizardContainer_PassesCustomCalculateButtonText()
    {
        // Arrange
        var customText = "Run Calculation";

        // Act
        var cut = RenderComponent<WizardContainer>(parameters => parameters
            .Add(p => p.TotalSteps, 5)
            .Add(p => p.CurrentStep, 5)
            .Add(p => p.StepLabels, TestStepLabels)
            .Add(p => p.CalculateButtonText, customText));

        // Assert
        var nav = cut.FindComponent<WizardNavigation>();
        nav.Instance.CalculateButtonText.Should().Be(customText);
    }

    [Fact]
    public void WizardContainer_UsesDefaultCalculateButtonText()
    {
        // Act
        var cut = RenderComponent<WizardContainer>(parameters => parameters
            .Add(p => p.TotalSteps, 5)
            .Add(p => p.CurrentStep, 5)
            .Add(p => p.StepLabels, TestStepLabels));

        // Assert
        var nav = cut.FindComponent<WizardNavigation>();
        nav.Instance.CalculateButtonText.Should().Be("Calculate Sizing");
    }

    [Fact]
    public async Task WizardContainer_InvokesOnStepSelected_WhenStepClicked()
    {
        // Arrange
        int? selectedStep = null;
        var cut = RenderComponent<WizardContainer>(parameters => parameters
            .Add(p => p.TotalSteps, 5)
            .Add(p => p.CurrentStep, 3)
            .Add(p => p.StepLabels, TestStepLabels)
            .Add(p => p.OnStepSelected, EventCallback.Factory.Create<int>(this, step => selectedStep = step)));

        // Act - Click on completed step
        await cut.InvokeAsync(() =>
        {
            cut.FindAll(".stepper-step")[0].Click();
        });

        // Assert
        selectedStep.Should().Be(1);
    }

    [Fact]
    public async Task WizardContainer_InvokesOnBack_WhenBackClicked()
    {
        // Arrange
        bool backCalled = false;
        var cut = RenderComponent<WizardContainer>(parameters => parameters
            .Add(p => p.TotalSteps, 5)
            .Add(p => p.CurrentStep, 2)
            .Add(p => p.StepLabels, TestStepLabels)
            .Add(p => p.OnBack, EventCallback.Factory.Create(this, () => backCalled = true)));

        // Act
        await cut.InvokeAsync(() =>
        {
            cut.Find(".wizard-btn.back").Click();
        });

        // Assert
        backCalled.Should().BeTrue();
    }

    [Fact]
    public async Task WizardContainer_InvokesOnNext_WhenNextClicked()
    {
        // Arrange
        bool nextCalled = false;
        var cut = RenderComponent<WizardContainer>(parameters => parameters
            .Add(p => p.TotalSteps, 5)
            .Add(p => p.CurrentStep, 1)
            .Add(p => p.CanProceed, true)
            .Add(p => p.StepLabels, TestStepLabels)
            .Add(p => p.OnNext, EventCallback.Factory.Create(this, () => nextCalled = true)));

        // Act - Find the primary button (Next button on non-last step)
        await cut.InvokeAsync(() =>
        {
            cut.Find(".wizard-btn.primary").Click();
        });

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task WizardContainer_InvokesOnCalculate_WhenCalculateClicked()
    {
        // Arrange
        bool calculateCalled = false;
        var cut = RenderComponent<WizardContainer>(parameters => parameters
            .Add(p => p.TotalSteps, 5)
            .Add(p => p.CurrentStep, 5)
            .Add(p => p.CanProceed, true)
            .Add(p => p.StepLabels, TestStepLabels)
            .Add(p => p.OnCalculate, EventCallback.Factory.Create(this, () => calculateCalled = true)));

        // Act - Find the primary button (Calculate button on last step)
        await cut.InvokeAsync(() =>
        {
            cut.Find(".wizard-btn.primary").Click();
        });

        // Assert
        calculateCalled.Should().BeTrue();
    }

    [Fact]
    public void WizardContainer_HandlesEmptyStepLabels()
    {
        // Act
        var cut = RenderComponent<WizardContainer>(parameters => parameters
            .Add(p => p.TotalSteps, 3)
            .Add(p => p.CurrentStep, 1)
            .Add(p => p.StepLabels, Array.Empty<string>()));

        // Assert - Should render without error
        cut.FindComponent<WizardStepper>().Should().NotBeNull();
        cut.FindAll(".stepper-step").Should().HaveCount(3);
    }

    [Fact]
    public void WizardContainer_UpdatesStepperWhenCurrentStepChanges()
    {
        // Arrange
        var cut = RenderComponent<WizardContainer>(parameters => parameters
            .Add(p => p.TotalSteps, 5)
            .Add(p => p.CurrentStep, 1)
            .Add(p => p.StepLabels, TestStepLabels));

        // Assert - Step 1 is active
        var steps = cut.FindAll(".stepper-step");
        steps[0].ClassList.Should().Contain("active");

        // Act - Change to step 3
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.CurrentStep, 3));

        // Assert - Step 3 is active
        steps = cut.FindAll(".stepper-step");
        steps[2].ClassList.Should().Contain("active");
        steps[0].ClassList.Should().NotContain("active");
        steps[0].ClassList.Should().Contain("completed");
    }
}
