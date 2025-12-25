using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.Wizard;

namespace InfraSizingCalculator.UnitTests.Components;

/// <summary>
/// Tests for WizardNavigation component
/// </summary>
public class WizardNavigationTests : TestContext
{
    [Fact]
    public void WizardNavigation_RendersBackButton_WhenShowBackButtonIsTrue()
    {
        // Act
        var cut = RenderComponent<WizardNavigation>(parameters => parameters
            .Add(p => p.ShowBackButton, true));

        // Assert
        cut.FindAll("button.back").Should().HaveCount(1);
        cut.Find("button.back").TextContent.Should().Contain("Back");
    }

    [Fact]
    public void WizardNavigation_HidesBackButton_WhenShowBackButtonIsFalse()
    {
        // Act
        var cut = RenderComponent<WizardNavigation>(parameters => parameters
            .Add(p => p.ShowBackButton, false));

        // Assert
        cut.FindAll("button.back").Should().BeEmpty();
    }

    [Fact]
    public void WizardNavigation_RendersNextButton_WhenNotLastStep()
    {
        // Act
        var cut = RenderComponent<WizardNavigation>(parameters => parameters
            .Add(p => p.IsLastStep, false));

        // Assert
        cut.Find("button.primary").TextContent.Should().Contain("Next");
    }

    [Fact]
    public void WizardNavigation_RendersCalculateButton_WhenIsLastStep()
    {
        // Act
        var cut = RenderComponent<WizardNavigation>(parameters => parameters
            .Add(p => p.IsLastStep, true));

        // Assert
        cut.Find("button.primary").TextContent.Should().Contain("Calculate Sizing");
    }

    [Fact]
    public void WizardNavigation_UsesCustomCalculateButtonText()
    {
        // Act
        var cut = RenderComponent<WizardNavigation>(parameters => parameters
            .Add(p => p.IsLastStep, true)
            .Add(p => p.CalculateButtonText, "Generate Results"));

        // Assert
        cut.Find("button.primary").TextContent.Should().Contain("Generate Results");
    }

    [Fact]
    public void WizardNavigation_DisablesBackButton_WhenCanGoBackIsFalse()
    {
        // Act
        var cut = RenderComponent<WizardNavigation>(parameters => parameters
            .Add(p => p.ShowBackButton, true)
            .Add(p => p.CanGoBack, false));

        // Assert
        cut.Find("button.back").HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void WizardNavigation_EnablesBackButton_WhenCanGoBackIsTrue()
    {
        // Act
        var cut = RenderComponent<WizardNavigation>(parameters => parameters
            .Add(p => p.ShowBackButton, true)
            .Add(p => p.CanGoBack, true));

        // Assert
        cut.Find("button.back").HasAttribute("disabled").Should().BeFalse();
    }

    [Fact]
    public void WizardNavigation_DisablesNextButton_WhenCanProceedIsFalse()
    {
        // Act
        var cut = RenderComponent<WizardNavigation>(parameters => parameters
            .Add(p => p.IsLastStep, false)
            .Add(p => p.CanProceed, false));

        // Assert
        cut.Find("button.primary").HasAttribute("disabled").Should().BeTrue();
    }

    [Fact]
    public void WizardNavigation_EnablesNextButton_WhenCanProceedIsTrue()
    {
        // Act
        var cut = RenderComponent<WizardNavigation>(parameters => parameters
            .Add(p => p.IsLastStep, false)
            .Add(p => p.CanProceed, true));

        // Assert
        cut.Find("button.primary").HasAttribute("disabled").Should().BeFalse();
    }

    [Fact]
    public void WizardNavigation_ShowsLoadingSpinner_WhenIsCalculating()
    {
        // Act
        var cut = RenderComponent<WizardNavigation>(parameters => parameters
            .Add(p => p.IsLastStep, true)
            .Add(p => p.IsCalculating, true));

        // Assert
        cut.FindAll(".btn-spinner").Should().HaveCount(1);
        cut.Find("button.primary").TextContent.Should().Contain("Calculating...");
    }

    [Fact]
    public void WizardNavigation_HidesLoadingSpinner_WhenNotCalculating()
    {
        // Act
        var cut = RenderComponent<WizardNavigation>(parameters => parameters
            .Add(p => p.IsLastStep, true)
            .Add(p => p.IsCalculating, false));

        // Assert
        cut.FindAll(".btn-spinner").Should().BeEmpty();
    }

    [Fact]
    public void WizardNavigation_InvokesOnBack_WhenBackButtonClicked()
    {
        // Arrange
        var clicked = false;
        var cut = RenderComponent<WizardNavigation>(parameters => parameters
            .Add(p => p.ShowBackButton, true)
            .Add(p => p.CanGoBack, true)
            .Add(p => p.OnBack, () => clicked = true));

        // Act
        cut.Find("button.back").Click();

        // Assert
        clicked.Should().BeTrue();
    }

    [Fact]
    public void WizardNavigation_InvokesOnNext_WhenNextButtonClicked()
    {
        // Arrange
        var clicked = false;
        var cut = RenderComponent<WizardNavigation>(parameters => parameters
            .Add(p => p.IsLastStep, false)
            .Add(p => p.CanProceed, true)
            .Add(p => p.OnNext, () => clicked = true));

        // Act
        cut.Find("button.primary").Click();

        // Assert
        clicked.Should().BeTrue();
    }

    [Fact]
    public void WizardNavigation_InvokesOnCalculate_WhenCalculateButtonClicked()
    {
        // Arrange
        var clicked = false;
        var cut = RenderComponent<WizardNavigation>(parameters => parameters
            .Add(p => p.IsLastStep, true)
            .Add(p => p.CanProceed, true)
            .Add(p => p.OnCalculate, () => clicked = true));

        // Act
        cut.Find("button.primary").Click();

        // Assert
        clicked.Should().BeTrue();
    }

    [Fact]
    public void WizardNavigation_AppliesAdditionalClass()
    {
        // Act
        var cut = RenderComponent<WizardNavigation>(parameters => parameters
            .Add(p => p.AdditionalClass, "custom-nav"));

        // Assert
        cut.Find(".wizard-navigation").ClassList.Should().Contain("custom-nav");
    }

    [Fact]
    public void WizardNavigation_HasCorrectLayout_WithBackAndNextButtons()
    {
        // Act
        var cut = RenderComponent<WizardNavigation>(parameters => parameters
            .Add(p => p.ShowBackButton, true)
            .Add(p => p.IsLastStep, false));

        // Assert - Should have 2 buttons
        cut.FindAll("button").Should().HaveCount(2);
    }

    [Fact]
    public void WizardNavigation_HasCorrectLayout_WithOnlyNextButton()
    {
        // Act
        var cut = RenderComponent<WizardNavigation>(parameters => parameters
            .Add(p => p.ShowBackButton, false)
            .Add(p => p.IsLastStep, false));

        // Assert - Should have 1 button (Next)
        cut.FindAll("button").Should().HaveCount(1);
    }
}
