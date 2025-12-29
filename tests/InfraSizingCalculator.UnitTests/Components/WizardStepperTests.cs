using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.Wizard;

namespace InfraSizingCalculator.UnitTests.Components;

/// <summary>
/// Tests for WizardStepper component
/// </summary>
public class WizardStepperTests : TestContext
{
    private static readonly string[] DefaultLabels = { "Step 1", "Step 2", "Step 3", "Step 4", "Step 5" };

    [Fact]
    public void WizardStepper_RendersCorrectNumberOfSteps()
    {
        // Arrange & Act
        var cut = RenderComponent<WizardStepper>(parameters => parameters
            .Add(p => p.TotalSteps, 5)
            .Add(p => p.CurrentStep, 1)
            .Add(p => p.StepLabels, DefaultLabels));

        // Assert
        cut.FindAll(".stepper-step").Should().HaveCount(5);
    }

    [Fact]
    public void WizardStepper_RendersCorrectNumberOfLines()
    {
        // Arrange & Act
        var cut = RenderComponent<WizardStepper>(parameters => parameters
            .Add(p => p.TotalSteps, 5)
            .Add(p => p.CurrentStep, 1)
            .Add(p => p.StepLabels, DefaultLabels));

        // Assert - Lines should be TotalSteps - 1
        cut.FindAll(".stepper-line").Should().HaveCount(4);
    }

    [Fact]
    public void WizardStepper_MarksCurrentStepAsActive()
    {
        // Arrange & Act
        var cut = RenderComponent<WizardStepper>(parameters => parameters
            .Add(p => p.TotalSteps, 5)
            .Add(p => p.CurrentStep, 3)
            .Add(p => p.StepLabels, DefaultLabels));

        // Assert
        var steps = cut.FindAll(".stepper-step");
        steps[2].ClassList.Should().Contain("active");
    }

    [Fact]
    public void WizardStepper_MarksPreviousStepsAsCompleted()
    {
        // Arrange & Act
        var cut = RenderComponent<WizardStepper>(parameters => parameters
            .Add(p => p.TotalSteps, 5)
            .Add(p => p.CurrentStep, 3)
            .Add(p => p.StepLabels, DefaultLabels));

        // Assert
        var steps = cut.FindAll(".stepper-step");
        steps[0].ClassList.Should().Contain("completed");
        steps[1].ClassList.Should().Contain("completed");
        steps[2].ClassList.Should().NotContain("completed"); // Current step
    }

    [Fact]
    public void WizardStepper_MarksFutureStepsAsDisabled()
    {
        // Arrange & Act
        var cut = RenderComponent<WizardStepper>(parameters => parameters
            .Add(p => p.TotalSteps, 5)
            .Add(p => p.CurrentStep, 2)
            .Add(p => p.StepLabels, DefaultLabels));

        // Assert
        var steps = cut.FindAll(".stepper-step");
        steps[2].ClassList.Should().Contain("disabled"); // Step 3
        steps[3].ClassList.Should().Contain("disabled"); // Step 4
        steps[4].ClassList.Should().Contain("disabled"); // Step 5
    }

    [Fact]
    public void WizardStepper_ShowsCheckmarkForCompletedSteps()
    {
        // Arrange & Act
        var cut = RenderComponent<WizardStepper>(parameters => parameters
            .Add(p => p.TotalSteps, 5)
            .Add(p => p.CurrentStep, 3)
            .Add(p => p.StepLabels, DefaultLabels));

        // Assert - Completed steps should have check icon, current step shows number
        var stepNumbers = cut.FindAll(".stepper-number");
        stepNumbers[0].QuerySelector(".icon.icon-check").Should().NotBeNull();
        stepNumbers[1].QuerySelector(".icon.icon-check").Should().NotBeNull();
        stepNumbers[2].TextContent.Should().Contain("3"); // Current step shows number
    }

    [Fact]
    public void WizardStepper_ShowsNumberForCurrentAndFutureSteps()
    {
        // Arrange & Act
        var cut = RenderComponent<WizardStepper>(parameters => parameters
            .Add(p => p.TotalSteps, 5)
            .Add(p => p.CurrentStep, 2)
            .Add(p => p.StepLabels, DefaultLabels));

        // Assert
        var stepNumbers = cut.FindAll(".stepper-number");
        stepNumbers[1].TextContent.Should().Contain("2"); // Current
        stepNumbers[2].TextContent.Should().Contain("3"); // Future
        stepNumbers[3].TextContent.Should().Contain("4"); // Future
    }

    [Fact]
    public void WizardStepper_DisplaysStepLabels()
    {
        // Arrange
        var labels = new[] { "Platform", "Deploy", "Tech", "Config", "Results" };

        // Act
        var cut = RenderComponent<WizardStepper>(parameters => parameters
            .Add(p => p.TotalSteps, 5)
            .Add(p => p.CurrentStep, 1)
            .Add(p => p.StepLabels, labels));

        // Assert
        var stepLabels = cut.FindAll(".stepper-label");
        stepLabels[0].TextContent.Should().Contain("Platform");
        stepLabels[1].TextContent.Should().Contain("Deploy");
        stepLabels[2].TextContent.Should().Contain("Tech");
        stepLabels[3].TextContent.Should().Contain("Config");
        stepLabels[4].TextContent.Should().Contain("Results");
    }

    [Fact]
    public void WizardStepper_UsesDefaultLabels_WhenNoneProvided()
    {
        // Arrange & Act
        var cut = RenderComponent<WizardStepper>(parameters => parameters
            .Add(p => p.TotalSteps, 3)
            .Add(p => p.CurrentStep, 1)
            .Add(p => p.StepLabels, Array.Empty<string>()));

        // Assert
        var stepLabels = cut.FindAll(".stepper-label");
        stepLabels[0].TextContent.Should().Contain("Step 1");
        stepLabels[1].TextContent.Should().Contain("Step 2");
        stepLabels[2].TextContent.Should().Contain("Step 3");
    }

    [Fact]
    public void WizardStepper_InvokesOnStepSelected_WhenCompletedStepClicked()
    {
        // Arrange
        int? selectedStep = null;
        var cut = RenderComponent<WizardStepper>(parameters => parameters
            .Add(p => p.TotalSteps, 5)
            .Add(p => p.CurrentStep, 3)
            .Add(p => p.StepLabels, DefaultLabels)
            .Add(p => p.OnStepSelected, step => selectedStep = step));

        // Act - Click on step 1 (completed)
        cut.FindAll(".stepper-step")[0].Click();

        // Assert
        selectedStep.Should().Be(1);
    }

    [Fact]
    public void WizardStepper_DoesNotInvokeOnStepSelected_WhenCurrentStepClicked()
    {
        // Arrange
        int? selectedStep = null;
        var cut = RenderComponent<WizardStepper>(parameters => parameters
            .Add(p => p.TotalSteps, 5)
            .Add(p => p.CurrentStep, 3)
            .Add(p => p.StepLabels, DefaultLabels)
            .Add(p => p.OnStepSelected, step => selectedStep = step));

        // Act - Click on current step (3)
        cut.FindAll(".stepper-step")[2].Click();

        // Assert
        selectedStep.Should().BeNull();
    }

    [Fact]
    public void WizardStepper_DoesNotInvokeOnStepSelected_WhenFutureStepClicked()
    {
        // Arrange
        int? selectedStep = null;
        var cut = RenderComponent<WizardStepper>(parameters => parameters
            .Add(p => p.TotalSteps, 5)
            .Add(p => p.CurrentStep, 2)
            .Add(p => p.StepLabels, DefaultLabels)
            .Add(p => p.OnStepSelected, step => selectedStep = step));

        // Act - Click on step 4 (future/disabled)
        cut.FindAll(".stepper-step")[3].Click();

        // Assert
        selectedStep.Should().BeNull();
    }

    [Fact]
    public void WizardStepper_MarksCompletedLinesCorrectly()
    {
        // Arrange & Act
        var cut = RenderComponent<WizardStepper>(parameters => parameters
            .Add(p => p.TotalSteps, 5)
            .Add(p => p.CurrentStep, 3)
            .Add(p => p.StepLabels, DefaultLabels));

        // Assert - Lines before current step should be completed
        var lines = cut.FindAll(".stepper-line");
        lines[0].ClassList.Should().Contain("completed"); // Between step 1 and 2
        lines[1].ClassList.Should().Contain("completed"); // Between step 2 and 3
        lines[2].ClassList.Should().NotContain("completed"); // Between step 3 and 4
    }

    [Fact]
    public void WizardStepper_AppliesAdditionalClass()
    {
        // Arrange & Act
        var cut = RenderComponent<WizardStepper>(parameters => parameters
            .Add(p => p.TotalSteps, 3)
            .Add(p => p.CurrentStep, 1)
            .Add(p => p.StepLabels, new[] { "A", "B", "C" })
            .Add(p => p.AdditionalClass, "custom-stepper"));

        // Assert
        cut.Find(".wizard-stepper").ClassList.Should().Contain("custom-stepper");
    }

    #region Edge Cases

    [Fact]
    public void WizardStepper_HandlesMinimumSteps()
    {
        // Arrange & Act - Only 1 step
        var cut = RenderComponent<WizardStepper>(parameters => parameters
            .Add(p => p.TotalSteps, 1)
            .Add(p => p.CurrentStep, 1)
            .Add(p => p.StepLabels, new[] { "Only Step" }));

        // Assert
        cut.FindAll(".stepper-step").Should().HaveCount(1);
        cut.FindAll(".stepper-line").Should().BeEmpty(); // No lines for single step
        cut.Find(".stepper-step").ClassList.Should().Contain("active");
    }

    [Fact]
    public void WizardStepper_HandlesManySteps()
    {
        // Arrange - 10 steps
        var labels = Enumerable.Range(1, 10).Select(i => $"Step {i}").ToArray();

        // Act
        var cut = RenderComponent<WizardStepper>(parameters => parameters
            .Add(p => p.TotalSteps, 10)
            .Add(p => p.CurrentStep, 5)
            .Add(p => p.StepLabels, labels));

        // Assert
        cut.FindAll(".stepper-step").Should().HaveCount(10);
        cut.FindAll(".stepper-line").Should().HaveCount(9);
        cut.FindAll(".stepper-step.completed").Should().HaveCount(4); // Steps 1-4
        cut.FindAll(".stepper-step.active").Should().HaveCount(1); // Step 5
    }

    [Fact]
    public void WizardStepper_HandlesCurrentStepAtBoundaries()
    {
        // Arrange & Act - Current step is 0 (edge case)
        var cut = RenderComponent<WizardStepper>(parameters => parameters
            .Add(p => p.TotalSteps, 5)
            .Add(p => p.CurrentStep, 0)
            .Add(p => p.StepLabels, DefaultLabels));

        // Assert - Should handle gracefully, no step should be active
        var activeSteps = cut.FindAll(".stepper-step.active");
        activeSteps.Should().BeEmpty();
    }

    [Fact]
    public void WizardStepper_HandlesCurrentStepBeyondTotal()
    {
        // Arrange & Act - Current step is beyond total
        var cut = RenderComponent<WizardStepper>(parameters => parameters
            .Add(p => p.TotalSteps, 5)
            .Add(p => p.CurrentStep, 10)
            .Add(p => p.StepLabels, DefaultLabels));

        // Assert - All steps should be completed
        cut.FindAll(".stepper-step.completed").Should().HaveCount(5);
        cut.FindAll(".stepper-step.active").Should().BeEmpty();
    }

    [Fact]
    public void WizardStepper_HandlesInsufficientLabels()
    {
        // Arrange & Act - Fewer labels than steps
        var cut = RenderComponent<WizardStepper>(parameters => parameters
            .Add(p => p.TotalSteps, 5)
            .Add(p => p.CurrentStep, 3)
            .Add(p => p.StepLabels, new[] { "Step 1", "Step 2" })); // Only 2 labels for 5 steps

        // Assert - Should use default labels for missing ones
        var labels = cut.FindAll(".stepper-label");
        labels[0].TextContent.Should().Contain("Step 1");
        labels[1].TextContent.Should().Contain("Step 2");
        labels[2].TextContent.Should().Contain("Step 3"); // Default
        labels[3].TextContent.Should().Contain("Step 4"); // Default
        labels[4].TextContent.Should().Contain("Step 5"); // Default
    }

    [Fact]
    public void WizardStepper_HandlesExcessLabels()
    {
        // Arrange & Act - More labels than steps
        var manyLabels = new[] { "A", "B", "C", "D", "E", "F", "G", "H" };
        var cut = RenderComponent<WizardStepper>(parameters => parameters
            .Add(p => p.TotalSteps, 3)
            .Add(p => p.CurrentStep, 2)
            .Add(p => p.StepLabels, manyLabels));

        // Assert - Should only use first 3 labels
        var labels = cut.FindAll(".stepper-label");
        labels.Should().HaveCount(3);
        labels[0].TextContent.Should().Contain("A");
        labels[1].TextContent.Should().Contain("B");
        labels[2].TextContent.Should().Contain("C");
    }

    [Fact]
    public void WizardStepper_HandlesEmptyLabelsArray()
    {
        // Arrange & Act - Use empty array instead of null
        var cut = RenderComponent<WizardStepper>(parameters => parameters
            .Add(p => p.TotalSteps, 3)
            .Add(p => p.CurrentStep, 1)
            .Add(p => p.StepLabels, Array.Empty<string>()));

        // Assert - Should use default labels
        var labels = cut.FindAll(".stepper-label");
        labels[0].TextContent.Should().Contain("Step 1");
        labels[1].TextContent.Should().Contain("Step 2");
        labels[2].TextContent.Should().Contain("Step 3");
    }

    [Fact]
    public void WizardStepper_OnlyCompletedStepsAreClickable()
    {
        // Arrange
        int clickCount = 0;
        var cut = RenderComponent<WizardStepper>(parameters => parameters
            .Add(p => p.TotalSteps, 5)
            .Add(p => p.CurrentStep, 3)
            .Add(p => p.StepLabels, DefaultLabels)
            .Add(p => p.OnStepSelected, step => clickCount++));

        // Act & Assert - Completed steps are clickable
        cut.FindAll(".stepper-step")[0].Click(); // Step 1 (completed)
        clickCount.Should().Be(1);

        cut.FindAll(".stepper-step")[1].Click(); // Step 2 (completed)
        clickCount.Should().Be(2);

        // Current and future steps should not invoke callback
        cut.FindAll(".stepper-step")[2].Click(); // Step 3 (current)
        clickCount.Should().Be(2); // No change

        cut.FindAll(".stepper-step")[3].Click(); // Step 4 (future)
        clickCount.Should().Be(2); // No change
    }

    [Fact]
    public void WizardStepper_AllStepsHaveCorrectStructure()
    {
        // Arrange & Act
        var cut = RenderComponent<WizardStepper>(parameters => parameters
            .Add(p => p.TotalSteps, 3)
            .Add(p => p.CurrentStep, 2)
            .Add(p => p.StepLabels, new[] { "First", "Second", "Third" }));

        // Assert
        var steps = cut.FindAll(".stepper-step");
        foreach (var step in steps)
        {
            step.QuerySelector(".stepper-number").Should().NotBeNull("each step should have a number");
            step.QuerySelector(".stepper-label").Should().NotBeNull("each step should have a label");
        }
    }

    [Fact]
    public void WizardStepper_TransitionsBetweenStatesCorrectly()
    {
        // Arrange
        var cut = RenderComponent<WizardStepper>(parameters => parameters
            .Add(p => p.TotalSteps, 5)
            .Add(p => p.CurrentStep, 1)
            .Add(p => p.StepLabels, DefaultLabels));

        var steps = cut.FindAll(".stepper-step");

        // Assert initial state - Step 1 active
        steps[0].ClassList.Should().Contain("active");
        steps[0].ClassList.Should().NotContain("completed");
        steps[1].ClassList.Should().Contain("disabled");

        // Act - Move to step 2
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.CurrentStep, 2));

        steps = cut.FindAll(".stepper-step");

        // Assert - Step 1 completed, Step 2 active
        steps[0].ClassList.Should().Contain("completed");
        steps[0].ClassList.Should().NotContain("active");
        steps[1].ClassList.Should().Contain("active");
        steps[1].ClassList.Should().NotContain("completed");

        // Act - Move to step 3
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.CurrentStep, 3));

        steps = cut.FindAll(".stepper-step");

        // Assert - Steps 1-2 completed, Step 3 active
        steps[0].ClassList.Should().Contain("completed");
        steps[1].ClassList.Should().Contain("completed");
        steps[2].ClassList.Should().Contain("active");
    }

    [Fact]
    public void WizardStepper_CanMoveBackwards()
    {
        // Arrange
        var cut = RenderComponent<WizardStepper>(parameters => parameters
            .Add(p => p.TotalSteps, 5)
            .Add(p => p.CurrentStep, 4)
            .Add(p => p.StepLabels, DefaultLabels));

        var steps = cut.FindAll(".stepper-step");

        // Assert - Step 4 is active, 1-3 completed
        steps[3].ClassList.Should().Contain("active");
        steps[0].ClassList.Should().Contain("completed");
        steps[1].ClassList.Should().Contain("completed");
        steps[2].ClassList.Should().Contain("completed");

        // Act - Move back to step 2
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.CurrentStep, 2));

        steps = cut.FindAll(".stepper-step");

        // Assert - Only step 1 completed, step 2 active, 3-5 disabled
        steps[0].ClassList.Should().Contain("completed");
        steps[1].ClassList.Should().Contain("active");
        steps[2].ClassList.Should().Contain("disabled");
        steps[3].ClassList.Should().Contain("disabled");
    }

    [Fact]
    public void WizardStepper_NoCallbackDoesNotError()
    {
        // Arrange & Act - No OnStepSelected callback provided
        var cut = RenderComponent<WizardStepper>(parameters => parameters
            .Add(p => p.TotalSteps, 5)
            .Add(p => p.CurrentStep, 3)
            .Add(p => p.StepLabels, DefaultLabels));

        // Assert - Clicking should not throw
        var action = () => cut.FindAll(".stepper-step")[0].Click();
        action.Should().NotThrow();
    }

    #endregion

    #region Keyboard Accessibility Tests (WCAG 2.1)

    [Fact]
    public void WizardStepper_EnterKey_ActivatesCompletedStep()
    {
        // Arrange
        int? selectedStep = null;
        var cut = RenderComponent<WizardStepper>(parameters => parameters
            .Add(p => p.TotalSteps, 5)
            .Add(p => p.CurrentStep, 3)
            .Add(p => p.StepLabels, DefaultLabels)
            .Add(p => p.OnStepSelected, step => selectedStep = step));

        // Act - Press Enter on completed step 1
        cut.FindAll(".stepper-step")[0].KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "Enter" });

        // Assert
        selectedStep.Should().Be(1);
    }

    [Fact]
    public void WizardStepper_SpaceKey_ActivatesCompletedStep()
    {
        // Arrange
        int? selectedStep = null;
        var cut = RenderComponent<WizardStepper>(parameters => parameters
            .Add(p => p.TotalSteps, 5)
            .Add(p => p.CurrentStep, 3)
            .Add(p => p.StepLabels, DefaultLabels)
            .Add(p => p.OnStepSelected, step => selectedStep = step));

        // Act - Press Space on completed step 2
        cut.FindAll(".stepper-step")[1].KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = " " });

        // Assert
        selectedStep.Should().Be(2);
    }

    [Fact]
    public void WizardStepper_EnterKey_DoesNotActivateCurrentStep()
    {
        // Arrange
        int? selectedStep = null;
        var cut = RenderComponent<WizardStepper>(parameters => parameters
            .Add(p => p.TotalSteps, 5)
            .Add(p => p.CurrentStep, 3)
            .Add(p => p.StepLabels, DefaultLabels)
            .Add(p => p.OnStepSelected, step => selectedStep = step));

        // Act - Press Enter on current step 3
        cut.FindAll(".stepper-step")[2].KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "Enter" });

        // Assert
        selectedStep.Should().BeNull();
    }

    [Fact]
    public void WizardStepper_SpaceKey_DoesNotActivateFutureStep()
    {
        // Arrange
        int? selectedStep = null;
        var cut = RenderComponent<WizardStepper>(parameters => parameters
            .Add(p => p.TotalSteps, 5)
            .Add(p => p.CurrentStep, 2)
            .Add(p => p.StepLabels, DefaultLabels)
            .Add(p => p.OnStepSelected, step => selectedStep = step));

        // Act - Press Space on future step 4
        cut.FindAll(".stepper-step")[3].KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = " " });

        // Assert
        selectedStep.Should().BeNull();
    }

    [Theory]
    [InlineData("Tab")]
    [InlineData("Escape")]
    [InlineData("ArrowDown")]
    [InlineData("ArrowUp")]
    [InlineData("a")]
    public void WizardStepper_OtherKeys_DoNotActivateStep(string key)
    {
        // Arrange
        int? selectedStep = null;
        var cut = RenderComponent<WizardStepper>(parameters => parameters
            .Add(p => p.TotalSteps, 5)
            .Add(p => p.CurrentStep, 3)
            .Add(p => p.StepLabels, DefaultLabels)
            .Add(p => p.OnStepSelected, step => selectedStep = step));

        // Act - Press non-activation key on completed step
        cut.FindAll(".stepper-step")[0].KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = key });

        // Assert
        selectedStep.Should().BeNull();
    }

    [Fact]
    public void WizardStepper_EnterKey_NoCallbackDoesNotError()
    {
        // Arrange & Act - No OnStepSelected callback provided
        var cut = RenderComponent<WizardStepper>(parameters => parameters
            .Add(p => p.TotalSteps, 5)
            .Add(p => p.CurrentStep, 3)
            .Add(p => p.StepLabels, DefaultLabels));

        // Assert - Pressing Enter should not throw
        var action = () => cut.FindAll(".stepper-step")[0].KeyDown(
            new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "Enter" });
        action.Should().NotThrow();
    }

    [Fact]
    public void WizardStepper_KeyboardActivation_WorksForAllCompletedSteps()
    {
        // Arrange
        int keyboardCallCount = 0;
        var cut = RenderComponent<WizardStepper>(parameters => parameters
            .Add(p => p.TotalSteps, 5)
            .Add(p => p.CurrentStep, 4)
            .Add(p => p.StepLabels, DefaultLabels)
            .Add(p => p.OnStepSelected, step => keyboardCallCount++));

        // Act - Use keyboard to activate all completed steps
        cut.FindAll(".stepper-step")[0].KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "Enter" });
        cut.FindAll(".stepper-step")[1].KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = " " });
        cut.FindAll(".stepper-step")[2].KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "Enter" });

        // Assert - All 3 completed steps should respond
        keyboardCallCount.Should().Be(3);
    }

    #endregion
}
