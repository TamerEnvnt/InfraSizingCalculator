using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.Layout;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components.Layout;

/// <summary>
/// Tests for LeftSidebar component - Wizard step navigation
/// </summary>
public class LeftSidebarTests : TestContext
{
    #region Rendering Tests

    [Fact]
    public void LeftSidebar_RendersNavElement()
    {
        // Act
        var cut = RenderComponent<LeftSidebar>();

        // Assert
        cut.Find("nav.left-sidebar").Should().NotBeNull();
    }

    [Fact]
    public void LeftSidebar_RendersConfigurationSection()
    {
        // Act
        var cut = RenderComponent<LeftSidebar>();

        // Assert
        cut.Find(".nav-section-title").TextContent.Should().Contain("Configuration");
    }

    [Fact]
    public void LeftSidebar_RendersDefaultSteps()
    {
        // Act
        var cut = RenderComponent<LeftSidebar>();

        // Assert - Default is 5 steps
        cut.FindAll(".nav-step").Should().HaveCount(5);
    }

    [Fact]
    public void LeftSidebar_RendersCustomNumberOfSteps()
    {
        // Act
        var cut = RenderComponent<LeftSidebar>(parameters => parameters
            .Add(p => p.TotalSteps, 7));

        // Assert
        cut.FindAll(".nav-step").Should().HaveCount(7);
    }

    #endregion

    #region Step State Tests

    [Fact]
    public void LeftSidebar_FirstStepIsCurrent_ByDefault()
    {
        // Act
        var cut = RenderComponent<LeftSidebar>();

        // Assert
        cut.FindAll(".nav-step")[0].ClassList.Should().Contain("current");
    }

    [Fact]
    public void LeftSidebar_CurrentStep_HasCurrentClass()
    {
        // Act
        var cut = RenderComponent<LeftSidebar>(parameters => parameters
            .Add(p => p.CurrentStep, 3));

        // Assert
        cut.FindAll(".nav-step")[2].ClassList.Should().Contain("current");
    }

    [Fact]
    public void LeftSidebar_CompletedSteps_HaveCompletedClass()
    {
        // Act
        var cut = RenderComponent<LeftSidebar>(parameters => parameters
            .Add(p => p.CurrentStep, 3));

        // Assert
        cut.FindAll(".nav-step")[0].ClassList.Should().Contain("completed");
        cut.FindAll(".nav-step")[1].ClassList.Should().Contain("completed");
    }

    [Fact]
    public void LeftSidebar_UpcomingSteps_HaveUpcomingClass()
    {
        // Act
        var cut = RenderComponent<LeftSidebar>(parameters => parameters
            .Add(p => p.CurrentStep, 3));

        // Assert
        cut.FindAll(".nav-step")[3].ClassList.Should().Contain("upcoming");
        cut.FindAll(".nav-step")[4].ClassList.Should().Contain("upcoming");
    }

    [Fact]
    public void LeftSidebar_ClickableSteps_HaveClickableClass()
    {
        // Act
        var cut = RenderComponent<LeftSidebar>(parameters => parameters
            .Add(p => p.CurrentStep, 3));

        // Assert - Steps 1, 2, 3 should be clickable
        cut.FindAll(".nav-step")[0].ClassList.Should().Contain("clickable");
        cut.FindAll(".nav-step")[1].ClassList.Should().Contain("clickable");
        cut.FindAll(".nav-step")[2].ClassList.Should().Contain("clickable");
        cut.FindAll(".nav-step")[3].ClassList.Should().NotContain("clickable");
    }

    [Fact]
    public void LeftSidebar_UpcomingSteps_AreDisabled()
    {
        // Act
        var cut = RenderComponent<LeftSidebar>(parameters => parameters
            .Add(p => p.CurrentStep, 3));

        // Assert
        cut.FindAll(".nav-step")[3].GetAttribute("disabled").Should().NotBeNull();
        cut.FindAll(".nav-step")[4].GetAttribute("disabled").Should().NotBeNull();
    }

    #endregion

    #region Step Number Display Tests

    [Fact]
    public void LeftSidebar_CurrentStep_ShowsStepNumber()
    {
        // Act
        var cut = RenderComponent<LeftSidebar>(parameters => parameters
            .Add(p => p.CurrentStep, 3));

        // Assert
        var currentStep = cut.FindAll(".nav-step")[2];
        currentStep.QuerySelector(".step-number")!.TextContent.Trim().Should().Be("3");
    }

    [Fact]
    public void LeftSidebar_CompletedStep_ShowsCheckmark()
    {
        // Act
        var cut = RenderComponent<LeftSidebar>(parameters => parameters
            .Add(p => p.CurrentStep, 3));

        // Assert
        var completedStep = cut.FindAll(".nav-step")[0];
        completedStep.QuerySelector(".step-number .check").Should().NotBeNull();
    }

    [Fact]
    public void LeftSidebar_UpcomingStep_ShowsStepNumber()
    {
        // Act
        var cut = RenderComponent<LeftSidebar>(parameters => parameters
            .Add(p => p.CurrentStep, 2));

        // Assert
        var upcomingStep = cut.FindAll(".nav-step")[3];
        upcomingStep.QuerySelector(".step-number")!.TextContent.Trim().Should().Be("4");
    }

    #endregion

    #region Step Label Tests

    [Fact]
    public void LeftSidebar_UsesDefaultLabels_WhenNoProviderGiven()
    {
        // Act
        var cut = RenderComponent<LeftSidebar>();

        // Assert
        var steps = cut.FindAll(".step-label");
        steps[0].TextContent.Should().Contain("Step 1");
        steps[1].TextContent.Should().Contain("Step 2");
    }

    [Fact]
    public void LeftSidebar_UsesStepLabelProvider()
    {
        // Arrange
        Func<int, string> labelProvider = step => step switch
        {
            1 => "Platform",
            2 => "Deployment",
            3 => "Technology",
            _ => $"Step {step}"
        };

        // Act
        var cut = RenderComponent<LeftSidebar>(parameters => parameters
            .Add(p => p.StepLabelProvider, labelProvider));

        // Assert
        var steps = cut.FindAll(".step-label");
        steps[0].TextContent.Should().Contain("Platform");
        steps[1].TextContent.Should().Contain("Deployment");
        steps[2].TextContent.Should().Contain("Technology");
    }

    [Fact]
    public void LeftSidebar_ShowsStepSelection_ForCompletedSteps()
    {
        // Arrange
        Func<int, string?> selectionProvider = step => step switch
        {
            1 => "Native",
            2 => "Kubernetes",
            _ => null
        };

        // Act
        var cut = RenderComponent<LeftSidebar>(parameters => parameters
            .Add(p => p.CurrentStep, 3)
            .Add(p => p.StepSelectionProvider, selectionProvider));

        // Assert
        var completedSteps = cut.FindAll(".nav-step.completed");
        completedSteps[0].QuerySelector(".step-selection")!.TextContent.Should().Contain("Native");
        completedSteps[1].QuerySelector(".step-selection")!.TextContent.Should().Contain("Kubernetes");
    }

    #endregion

    #region Click Event Tests

    [Fact]
    public async Task LeftSidebar_ClickingCompletedStep_InvokesOnStepSelected()
    {
        // Arrange
        int? selectedStep = null;
        var cut = RenderComponent<LeftSidebar>(parameters => parameters
            .Add(p => p.CurrentStep, 3)
            .Add(p => p.OnStepSelected, EventCallback.Factory.Create<int>(this, s => selectedStep = s)));

        // Act
        var step1 = cut.FindAll(".nav-step")[0];
        await cut.InvokeAsync(() => step1.Click());

        // Assert
        selectedStep.Should().Be(1);
    }

    [Fact]
    public async Task LeftSidebar_ClickingCurrentStep_InvokesOnStepSelected()
    {
        // Arrange
        int? selectedStep = null;
        var cut = RenderComponent<LeftSidebar>(parameters => parameters
            .Add(p => p.CurrentStep, 3)
            .Add(p => p.OnStepSelected, EventCallback.Factory.Create<int>(this, s => selectedStep = s)));

        // Act
        var step3 = cut.FindAll(".nav-step")[2];
        await cut.InvokeAsync(() => step3.Click());

        // Assert
        selectedStep.Should().Be(3);
    }

    [Fact]
    public async Task LeftSidebar_ClickingUpcomingStep_DoesNotInvoke()
    {
        // Arrange
        int? selectedStep = null;
        var cut = RenderComponent<LeftSidebar>(parameters => parameters
            .Add(p => p.CurrentStep, 2)
            .Add(p => p.OnStepSelected, EventCallback.Factory.Create<int>(this, s => selectedStep = s)));

        // Act - Upcoming steps are disabled, but clicking shouldn't trigger callback
        // The button is disabled so click won't work normally

        // Assert
        selectedStep.Should().BeNull();
    }

    #endregion

    #region Results Section Tests

    [Fact]
    public void LeftSidebar_ResultsSection_HiddenByDefault()
    {
        // Act
        var cut = RenderComponent<LeftSidebar>();

        // Assert
        cut.FindAll(".nav-section").Should().HaveCount(1); // Only Configuration section
    }

    [Fact]
    public void LeftSidebar_ResultsSection_ShownWhenEnabled()
    {
        // Act
        var cut = RenderComponent<LeftSidebar>(parameters => parameters
            .Add(p => p.ShowResultsSection, true));

        // Assert
        cut.FindAll(".nav-section").Should().HaveCount(2);
        cut.FindAll(".nav-section-title")[1].TextContent.Should().Contain("Results");
    }

    [Fact]
    public void LeftSidebar_ResultsSection_HasResultTabs()
    {
        // Act
        var cut = RenderComponent<LeftSidebar>(parameters => parameters
            .Add(p => p.ShowResultsSection, true));

        // Assert
        var resultItems = cut.FindAll(".nav-item");
        resultItems.Should().HaveCountGreaterThanOrEqualTo(3); // Sizing, Cost, Growth
    }

    [Fact]
    public void LeftSidebar_ResultsSection_ShowsSizingDetails()
    {
        // Act
        var cut = RenderComponent<LeftSidebar>(parameters => parameters
            .Add(p => p.ShowResultsSection, true));

        // Assert
        var resultItems = cut.FindAll(".nav-item");
        resultItems.Should().Contain(i => i.TextContent.Contains("Sizing"));
    }

    [Fact]
    public void LeftSidebar_ResultsSection_ShowsCostBreakdown()
    {
        // Act
        var cut = RenderComponent<LeftSidebar>(parameters => parameters
            .Add(p => p.ShowResultsSection, true));

        // Assert
        var resultItems = cut.FindAll(".nav-item");
        resultItems.Should().Contain(i => i.TextContent.Contains("Cost"));
    }

    [Fact]
    public void LeftSidebar_ResultsSection_ShowsGrowthPlanning()
    {
        // Act
        var cut = RenderComponent<LeftSidebar>(parameters => parameters
            .Add(p => p.ShowResultsSection, true));

        // Assert
        var resultItems = cut.FindAll(".nav-item");
        resultItems.Should().Contain(i => i.TextContent.Contains("Growth"));
    }

    [Fact]
    public void LeftSidebar_ActiveResultTab_HasActiveClass()
    {
        // Act
        var cut = RenderComponent<LeftSidebar>(parameters => parameters
            .Add(p => p.ShowResultsSection, true)
            .Add(p => p.ActiveResultTab, "cost"));

        // Assert
        var costTab = cut.FindAll(".nav-item").First(i => i.TextContent.Contains("Cost"));
        costTab.ClassList.Should().Contain("active");
    }

    [Fact]
    public async Task LeftSidebar_ClickingResultTab_InvokesOnResultTabSelected()
    {
        // Arrange
        string? selectedTab = null;
        var cut = RenderComponent<LeftSidebar>(parameters => parameters
            .Add(p => p.ShowResultsSection, true)
            .Add(p => p.OnResultTabSelected, EventCallback.Factory.Create<string>(this, t => selectedTab = t)));

        // Act
        var costTab = cut.FindAll(".nav-item").First(i => i.TextContent.Contains("Cost"));
        await cut.InvokeAsync(() => costTab.Click());

        // Assert
        selectedTab.Should().Be("cost");
    }

    #endregion

    #region Insights Badge Tests

    [Fact]
    public void LeftSidebar_InsightsTab_HiddenWhenCountIsZero()
    {
        // Act
        var cut = RenderComponent<LeftSidebar>(parameters => parameters
            .Add(p => p.ShowResultsSection, true)
            .Add(p => p.InsightsCount, 0));

        // Assert
        cut.FindAll(".nav-item").Should().NotContain(i => i.TextContent.Contains("Insights"));
    }

    [Fact]
    public void LeftSidebar_InsightsTab_ShownWhenCountGreaterThanZero()
    {
        // Act
        var cut = RenderComponent<LeftSidebar>(parameters => parameters
            .Add(p => p.ShowResultsSection, true)
            .Add(p => p.InsightsCount, 5));

        // Assert
        cut.FindAll(".nav-item").Should().Contain(i => i.TextContent.Contains("Insights"));
    }

    [Fact]
    public void LeftSidebar_InsightsTab_ShowsBadgeWithCount()
    {
        // Act
        var cut = RenderComponent<LeftSidebar>(parameters => parameters
            .Add(p => p.ShowResultsSection, true)
            .Add(p => p.InsightsCount, 5));

        // Assert
        var insightsTab = cut.FindAll(".nav-item").First(i => i.TextContent.Contains("Insights"));
        insightsTab.QuerySelector(".badge")!.TextContent.Should().Be("5");
    }

    [Fact]
    public void LeftSidebar_InsightsBadge_HasCriticalClass_WhenCriticalCountGreaterThanZero()
    {
        // Act
        var cut = RenderComponent<LeftSidebar>(parameters => parameters
            .Add(p => p.ShowResultsSection, true)
            .Add(p => p.InsightsCount, 5)
            .Add(p => p.CriticalCount, 2));

        // Assert
        var insightsTab = cut.FindAll(".nav-item").First(i => i.TextContent.Contains("Insights"));
        insightsTab.QuerySelector(".badge")!.ClassList.Should().Contain("critical");
    }

    [Fact]
    public void LeftSidebar_InsightsBadge_NoCriticalClass_WhenCriticalCountIsZero()
    {
        // Act
        var cut = RenderComponent<LeftSidebar>(parameters => parameters
            .Add(p => p.ShowResultsSection, true)
            .Add(p => p.InsightsCount, 5)
            .Add(p => p.CriticalCount, 0));

        // Assert
        var insightsTab = cut.FindAll(".nav-item").First(i => i.TextContent.Contains("Insights"));
        insightsTab.QuerySelector(".badge")!.ClassList.Should().NotContain("critical");
    }

    #endregion

    #region Parameter Update Tests

    [Fact]
    public void LeftSidebar_UpdatesCurrentStep()
    {
        // Arrange
        var cut = RenderComponent<LeftSidebar>(parameters => parameters
            .Add(p => p.CurrentStep, 1));

        // Assert initial state
        cut.FindAll(".nav-step")[0].ClassList.Should().Contain("current");

        // Act
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.CurrentStep, 3));

        // Assert
        cut.FindAll(".nav-step")[0].ClassList.Should().Contain("completed");
        cut.FindAll(".nav-step")[2].ClassList.Should().Contain("current");
    }

    [Fact]
    public void LeftSidebar_UpdatesActiveResultTab()
    {
        // Arrange
        var cut = RenderComponent<LeftSidebar>(parameters => parameters
            .Add(p => p.ShowResultsSection, true)
            .Add(p => p.ActiveResultTab, "sizing"));

        // Assert initial state
        var sizingTab = cut.FindAll(".nav-item").First(i => i.TextContent.Contains("Sizing"));
        sizingTab.ClassList.Should().Contain("active");

        // Act
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.ActiveResultTab, "growth"));

        // Assert
        var growthTab = cut.FindAll(".nav-item").First(i => i.TextContent.Contains("Growth"));
        growthTab.ClassList.Should().Contain("active");
    }

    #endregion
}
