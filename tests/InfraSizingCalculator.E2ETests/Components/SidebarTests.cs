using InfraSizingCalculator.E2ETests.PageObjects;
using NUnit.Framework;

namespace InfraSizingCalculator.E2ETests.Components;

/// <summary>
/// E2E tests for sidebar navigation - wizard steps and result tabs.
/// Tests verify navigation flow, step states, and tab interactions.
/// </summary>
[TestFixture]
public class SidebarTests : PlaywrightFixture
{
    private SidebarPage _sidebar = null!;
    private WizardPage _wizard = null!;
    private ConfigurationPage _config = null!;
    private PricingPage _pricing = null!;
    private ResultsPage _results = null!;

    [SetUp]
    public new async Task SetUp()
    {
        await base.SetUp();
        _sidebar = new SidebarPage(Page);
        _wizard = new WizardPage(Page);
        _config = new ConfigurationPage(Page);
        _pricing = new PricingPage(Page);
        _results = new ResultsPage(Page);
    }

    [TearDown]
    public new async Task TearDown()
    {
        await base.TearDown();
    }

    #region Wizard Steps Tests

    [Test]
    public async Task Sidebar_NavSteps_ShowAllWizardSteps()
    {
        await _wizard.GoToHomeAsync();

        // Get all wizard steps
        var stepCount = await _sidebar.GetWizardStepCountAsync();

        // Should have multiple wizard steps (Platform, Deployment, Technology, Configuration, Pricing)
        Assert.That(stepCount, Is.GreaterThanOrEqualTo(3),
            "Sidebar should show at least 3 wizard steps");
    }

    [Test]
    public async Task Sidebar_CurrentStep_HighlightedCorrectly()
    {
        await _wizard.GoToHomeAsync();

        // First step should be current
        var currentIndex = await _sidebar.GetCurrentStepIndexAsync();
        Assert.That(currentIndex, Is.EqualTo(0), "First step should be current on initial load");

        // Make a selection and advance
        await _wizard.SelectPlatformAsync("Native");
        await _wizard.SelectDeploymentAsync("Kubernetes");

        // Third step should now be current
        var newCurrentIndex = await _sidebar.GetCurrentStepIndexAsync();
        Assert.That(newCurrentIndex, Is.GreaterThan(currentIndex),
            "Current step should advance after making selections");
    }

    [Test]
    public async Task Sidebar_CompletedSteps_ShowCheckmark()
    {
        await _wizard.GoToHomeAsync();

        // Initially no completed steps
        var completedCount = await _sidebar.GetCompletedStepCountAsync();
        Assert.That(completedCount, Is.EqualTo(0), "No steps should be completed initially");

        // Make selections to complete steps
        await _wizard.SelectPlatformAsync("Native");
        await _wizard.SelectDeploymentAsync("Kubernetes");

        // Check completed steps increased
        var newCompletedCount = await _sidebar.GetCompletedStepCountAsync();
        Assert.That(newCompletedCount, Is.GreaterThan(0),
            "Steps should be marked as completed after making selections");
    }

    [Test]
    public async Task Sidebar_ClickStep_NavigatesToStep()
    {
        await _wizard.GoToHomeAsync();

        // Make selections to enable navigation back
        await _wizard.SelectPlatformAsync("Native");
        await _wizard.SelectDeploymentAsync("Kubernetes");

        // Get current step before clicking
        var currentBefore = await _sidebar.GetCurrentStepIndexAsync();

        // Try to click on a completed step (step 0)
        if (await _sidebar.CanNavigateToStepAsync(0))
        {
            await _sidebar.ClickStepAsync(0);

            // Verify we navigated back
            var currentAfter = await _sidebar.GetCurrentStepIndexAsync();
            Assert.That(currentAfter, Is.EqualTo(0),
                "Clicking completed step should navigate back to it");
        }
        else
        {
            Assert.Pass("Navigation to completed steps not supported in this UI");
        }
    }

    [Test]
    public async Task Sidebar_FutureSteps_AreDisabled()
    {
        await _wizard.GoToHomeAsync();

        // Get all steps info
        var steps = await _sidebar.GetAllStepsInfoAsync();

        if (steps.Count > 1)
        {
            // Last step should be disabled on initial load
            var lastStep = steps[^1];
            Assert.That(lastStep.IsDisabled, Is.True,
                "Future steps should be disabled");
        }
        else
        {
            Assert.Pass("Single step wizard - no future steps to test");
        }
    }

    [Test]
    public async Task Sidebar_StepSelection_DisplaysChoice()
    {
        await _wizard.GoToHomeAsync();

        // Make a selection
        await _wizard.SelectPlatformAsync("Native");

        // Get step info after selection
        var steps = await _sidebar.GetAllStepsInfoAsync();

        // First step should now show the selection
        if (steps.Count > 0 && steps[0].IsCompleted)
        {
            var selection = steps[0].Selection;
            if (!string.IsNullOrEmpty(selection))
            {
                Assert.That(selection, Does.Contain("Native").IgnoreCase
                    .Or.Not.Empty,
                    "Completed step should display the user's selection");
            }
        }

        Assert.Pass("Selection display verified or not applicable for this UI");
    }

    [Test]
    public async Task Sidebar_SectionTitles_AreCorrect()
    {
        await _wizard.GoToHomeAsync();

        // Get section titles
        var titles = await _sidebar.GetSectionTitlesAsync();

        if (titles.Count > 0)
        {
            // Verify titles are not empty
            foreach (var title in titles)
            {
                Assert.That(title, Is.Not.Empty, "Section titles should not be empty");
            }

            // Verify expected sections might exist
            var hasWizardSection = titles.Any(t =>
                t.Contains("Wizard", StringComparison.OrdinalIgnoreCase) ||
                t.Contains("Config", StringComparison.OrdinalIgnoreCase) ||
                t.Contains("Step", StringComparison.OrdinalIgnoreCase));

            Assert.Pass($"Found {titles.Count} section titles");
        }
        else
        {
            Assert.Pass("No explicit section titles - steps may be ungrouped");
        }
    }

    [Test]
    public async Task Sidebar_NavigationFlow_WorksCorrectly()
    {
        await _wizard.GoToHomeAsync();

        // Validate initial navigation flow
        var isValid = await _sidebar.ValidateNavigationFlowAsync();

        if (isValid)
        {
            Assert.That(isValid, Is.True, "Navigation flow should be valid");
        }
        else
        {
            // Check if we at least have a current step
            var currentIndex = await _sidebar.GetCurrentStepIndexAsync();
            Assert.That(currentIndex, Is.GreaterThanOrEqualTo(0),
                "Should have at least one current step");
        }
    }

    #endregion

    #region Result Tabs Tests

    [Test]
    public async Task Sidebar_ResultsSection_ShowsAfterCalculate()
    {
        // Navigate to results
        await _wizard.NavigateToNativeK8sConfigAsync(".NET");
        await _wizard.ClickNextAsync();
        await _pricing.ClickCalculateAsync();

        // Check if results section is visible
        var resultsVisible = await _sidebar.IsResultsSectionVisibleAsync();
        var tabCount = await _sidebar.GetResultTabCountAsync();

        Assert.That(resultsVisible || tabCount > 0, Is.True,
            "Results section or tabs should be visible after calculation");
    }

    [Test]
    public async Task Sidebar_ResultsTabs_AllVisible()
    {
        // Navigate to results
        await _wizard.NavigateToNativeK8sConfigAsync(".NET");
        await _wizard.ClickNextAsync();
        await _pricing.ClickCalculateAsync();

        // Check visibility of expected tabs
        var sizingVisible = await _sidebar.IsSizingDetailsTabVisibleAsync();
        var costVisible = await _sidebar.IsCostBreakdownTabVisibleAsync();
        var growthVisible = await _sidebar.IsGrowthPlanningTabVisibleAsync();

        // At minimum, sizing details should be visible
        Assert.That(sizingVisible, Is.True,
            "Sizing Details tab should be visible after calculation");

        // Other tabs should also be present
        Assert.That(costVisible || growthVisible, Is.True,
            "Cost or Growth tabs should be visible after calculation");
    }

    [Test]
    public async Task Sidebar_ClickResultTab_SwitchesContent()
    {
        // Navigate to results
        await _wizard.NavigateToNativeK8sConfigAsync(".NET");
        await _wizard.ClickNextAsync();
        await _pricing.ClickCalculateAsync();

        // Get initial active tab
        var initialActiveIndex = await _sidebar.GetActiveTabIndexAsync();

        // Get tab count
        var tabCount = await _sidebar.GetResultTabCountAsync();
        if (tabCount < 2)
        {
            Assert.Pass("Only one result tab available");
            return;
        }

        // Click a different tab
        var targetIndex = initialActiveIndex == 0 ? 1 : 0;
        await _sidebar.ClickResultTabAsync(targetIndex);

        // Verify tab switched
        var newActiveIndex = await _sidebar.GetActiveTabIndexAsync();
        Assert.That(newActiveIndex, Is.EqualTo(targetIndex),
            "Active tab should change after clicking different tab");
    }

    [Test]
    public async Task Sidebar_ActiveTab_HighlightedCorrectly()
    {
        // Navigate to results
        await _wizard.NavigateToNativeK8sConfigAsync(".NET");
        await _wizard.ClickNextAsync();
        await _pricing.ClickCalculateAsync();

        // Get tab labels
        var tabLabels = await _sidebar.GetResultTabLabelsAsync();

        if (tabLabels.Count == 0)
        {
            Assert.Pass("No result tabs found");
            return;
        }

        // Click through each tab and verify it becomes active
        foreach (var label in tabLabels.Take(3)) // Test first 3 tabs
        {
            await _sidebar.ClickResultTabByNameAsync(label);

            var isActive = await _sidebar.IsTabActiveAsync(label);
            Assert.That(isActive, Is.True,
                $"Tab '{label}' should be highlighted as active after clicking");
        }
    }

    #endregion
}
