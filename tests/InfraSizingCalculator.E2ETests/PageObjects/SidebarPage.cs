using Microsoft.Playwright;

namespace InfraSizingCalculator.E2ETests.PageObjects;

/// <summary>
/// Page object for sidebar navigation - wizard steps and result tabs.
/// Tests the navigation flow, step states, and tab interactions.
/// </summary>
public class SidebarPage
{
    private readonly IPage _page;
    private readonly int _defaultTimeout;

    public SidebarPage(IPage page, int defaultTimeout = 15000)
    {
        _page = page;
        _defaultTimeout = defaultTimeout;
    }

    #region Locators

    // Navigation sections
    private const string LeftSidebar = ".left-sidebar";
    private const string NavSection = ".nav-section";
    private const string NavSectionTitle = ".nav-section-title";

    // Wizard steps
    private const string NavStep = ".nav-step";
    private const string NavStepCurrent = ".nav-step.current";
    private const string NavStepCompleted = ".nav-step.completed";
    private const string NavStepDisabled = ".nav-step:not(.current):not(.completed)";
    private const string NavStepLabel = ".nav-step .step-label, .nav-step .step-title";
    private const string NavStepSelection = ".nav-step .step-selection, .nav-step .selection-display";

    // Result tabs
    private const string NavItem = "button.nav-item";
    private const string NavItemActive = "button.nav-item.active";
    private const string ResultsSection = ".results-section, .nav-section:has(button.nav-item)";

    // Specific result tabs
    private const string SizingDetailsTab = "button.nav-item:has-text('Sizing'), button.nav-item:has-text('Details')";
    private const string CostBreakdownTab = "button.nav-item:has-text('Cost')";
    private const string GrowthPlanningTab = "button.nav-item:has-text('Growth')";
    private const string InsightsTab = "button.nav-item:has-text('Insights')";

    #endregion

    #region Section Visibility

    public async Task<bool> IsSidebarVisibleAsync()
    {
        var element = await _page.QuerySelectorAsync(LeftSidebar);
        return element != null && await element.IsVisibleAsync();
    }

    public async Task<int> GetNavSectionCountAsync()
    {
        var sections = await _page.QuerySelectorAllAsync(NavSection);
        return sections.Count;
    }

    public async Task<IReadOnlyList<string>> GetSectionTitlesAsync()
    {
        var titles = new List<string>();
        var elements = await _page.QuerySelectorAllAsync(NavSectionTitle);

        foreach (var element in elements)
        {
            var text = await element.TextContentAsync();
            if (!string.IsNullOrEmpty(text))
            {
                titles.Add(text.Trim());
            }
        }

        return titles;
    }

    #endregion

    #region Wizard Steps

    public async Task<int> GetWizardStepCountAsync()
    {
        var steps = await _page.QuerySelectorAllAsync(NavStep);
        return steps.Count;
    }

    public async Task<int> GetCurrentStepIndexAsync()
    {
        var steps = await _page.QuerySelectorAllAsync(NavStep);
        for (int i = 0; i < steps.Count; i++)
        {
            var isCurrent = await steps[i].EvaluateAsync<bool>(
                "el => el.classList.contains('current')");
            if (isCurrent) return i;
        }
        return -1;
    }

    public async Task<int> GetCompletedStepCountAsync()
    {
        var steps = await _page.QuerySelectorAllAsync(NavStepCompleted);
        return steps.Count;
    }

    public async Task<bool> IsStepCurrentAsync(int stepIndex)
    {
        var steps = await _page.QuerySelectorAllAsync(NavStep);
        if (stepIndex >= steps.Count) return false;

        return await steps[stepIndex].EvaluateAsync<bool>(
            "el => el.classList.contains('current')");
    }

    public async Task<bool> IsStepCompletedAsync(int stepIndex)
    {
        var steps = await _page.QuerySelectorAllAsync(NavStep);
        if (stepIndex >= steps.Count) return false;

        return await steps[stepIndex].EvaluateAsync<bool>(
            "el => el.classList.contains('completed')");
    }

    public async Task<bool> IsStepDisabledAsync(int stepIndex)
    {
        var steps = await _page.QuerySelectorAllAsync(NavStep);
        if (stepIndex >= steps.Count) return false;

        // A step is disabled if it's not current and not completed (future step)
        var isCurrent = await steps[stepIndex].EvaluateAsync<bool>(
            "el => el.classList.contains('current')");
        var isCompleted = await steps[stepIndex].EvaluateAsync<bool>(
            "el => el.classList.contains('completed')");

        return !isCurrent && !isCompleted;
    }

    public async Task ClickStepAsync(int stepIndex)
    {
        var steps = await _page.QuerySelectorAllAsync(NavStep);
        if (stepIndex < steps.Count)
        {
            await steps[stepIndex].ClickAsync();
            await WaitForStabilityAsync();
        }
    }

    public async Task<string?> GetStepLabelAsync(int stepIndex)
    {
        var labels = await _page.QuerySelectorAllAsync(NavStepLabel);
        if (stepIndex < labels.Count)
        {
            return await labels[stepIndex].TextContentAsync();
        }
        return null;
    }

    public async Task<string?> GetStepSelectionAsync(int stepIndex)
    {
        var steps = await _page.QuerySelectorAllAsync(NavStep);
        if (stepIndex >= steps.Count) return null;

        var selection = await steps[stepIndex].QuerySelectorAsync(NavStepSelection.Replace(".nav-step ", ""));
        return selection != null ? await selection.TextContentAsync() : null;
    }

    public async Task<IReadOnlyList<WizardStepInfo>> GetAllStepsInfoAsync()
    {
        var stepsInfo = new List<WizardStepInfo>();
        var steps = await _page.QuerySelectorAllAsync(NavStep);

        for (int i = 0; i < steps.Count; i++)
        {
            var step = steps[i];
            var isCurrent = await step.EvaluateAsync<bool>("el => el.classList.contains('current')");
            var isCompleted = await step.EvaluateAsync<bool>("el => el.classList.contains('completed')");

            var labelElement = await step.QuerySelectorAsync(".step-label, .step-title");
            var label = labelElement != null ? await labelElement.TextContentAsync() : $"Step {i + 1}";

            var selectionElement = await step.QuerySelectorAsync(".step-selection, .selection-display");
            var selection = selectionElement != null ? await selectionElement.TextContentAsync() : null;

            stepsInfo.Add(new WizardStepInfo
            {
                Index = i,
                Label = label?.Trim() ?? "",
                Selection = selection?.Trim(),
                IsCurrent = isCurrent,
                IsCompleted = isCompleted,
                IsDisabled = !isCurrent && !isCompleted
            });
        }

        return stepsInfo;
    }

    #endregion

    #region Result Tabs

    public async Task<bool> IsResultsSectionVisibleAsync()
    {
        var element = await _page.QuerySelectorAsync(ResultsSection);
        return element != null && await element.IsVisibleAsync();
    }

    public async Task<int> GetResultTabCountAsync()
    {
        var tabs = await _page.QuerySelectorAllAsync(NavItem);
        return tabs.Count;
    }

    public async Task<IReadOnlyList<string>> GetResultTabLabelsAsync()
    {
        var labels = new List<string>();
        var tabs = await _page.QuerySelectorAllAsync(NavItem);

        foreach (var tab in tabs)
        {
            var text = await tab.TextContentAsync();
            if (!string.IsNullOrEmpty(text))
            {
                labels.Add(text.Trim());
            }
        }

        return labels;
    }

    public async Task<int> GetActiveTabIndexAsync()
    {
        var tabs = await _page.QuerySelectorAllAsync(NavItem);
        for (int i = 0; i < tabs.Count; i++)
        {
            var isActive = await tabs[i].EvaluateAsync<bool>(
                "el => el.classList.contains('active')");
            if (isActive) return i;
        }
        return -1;
    }

    public async Task<bool> IsTabActiveAsync(string tabName)
    {
        var selector = $"button.nav-item.active:has-text('{tabName}')";
        var element = await _page.QuerySelectorAsync(selector);
        return element != null;
    }

    public async Task ClickResultTabAsync(int tabIndex)
    {
        var tabs = await _page.QuerySelectorAllAsync(NavItem);
        if (tabIndex < tabs.Count)
        {
            await tabs[tabIndex].ClickAsync();
            await WaitForStabilityAsync();
        }
    }

    public async Task ClickResultTabByNameAsync(string tabName)
    {
        var selector = $"button.nav-item:has-text('{tabName}')";
        await _page.ClickAsync(selector);
        await WaitForStabilityAsync();
    }

    // Specific tab clicks
    public async Task ClickSizingDetailsAsync()
    {
        await _page.ClickAsync(SizingDetailsTab);
        await WaitForStabilityAsync();
    }

    public async Task ClickCostBreakdownAsync()
    {
        await _page.ClickAsync(CostBreakdownTab);
        await WaitForStabilityAsync();
    }

    public async Task ClickGrowthPlanningAsync()
    {
        await _page.ClickAsync(GrowthPlanningTab);
        await WaitForStabilityAsync();
    }

    public async Task ClickInsightsAsync()
    {
        await _page.ClickAsync(InsightsTab);
        await WaitForStabilityAsync();
    }

    public async Task<bool> IsSizingDetailsTabVisibleAsync()
    {
        var element = await _page.QuerySelectorAsync(SizingDetailsTab);
        return element != null && await element.IsVisibleAsync();
    }

    public async Task<bool> IsCostBreakdownTabVisibleAsync()
    {
        var element = await _page.QuerySelectorAsync(CostBreakdownTab);
        return element != null && await element.IsVisibleAsync();
    }

    public async Task<bool> IsGrowthPlanningTabVisibleAsync()
    {
        var element = await _page.QuerySelectorAsync(GrowthPlanningTab);
        return element != null && await element.IsVisibleAsync();
    }

    public async Task<bool> IsInsightsTabVisibleAsync()
    {
        var element = await _page.QuerySelectorAsync(InsightsTab);
        return element != null && await element.IsVisibleAsync();
    }

    #endregion

    #region Navigation Flow

    public async Task<bool> CanNavigateToStepAsync(int stepIndex)
    {
        // Can navigate to completed steps or current step
        var isCompleted = await IsStepCompletedAsync(stepIndex);
        var isCurrent = await IsStepCurrentAsync(stepIndex);
        return isCompleted || isCurrent;
    }

    public async Task<bool> ValidateNavigationFlowAsync()
    {
        var steps = await GetAllStepsInfoAsync();

        // First step should be current or completed
        if (steps.Count == 0) return false;

        // There should be exactly one current step
        var currentSteps = steps.Count(s => s.IsCurrent);
        if (currentSteps != 1) return false;

        // All steps before current should be completed
        var currentIndex = steps.ToList().FindIndex(s => s.IsCurrent);
        for (int i = 0; i < currentIndex; i++)
        {
            if (!steps[i].IsCompleted) return false;
        }

        // All steps after current should be disabled
        for (int i = currentIndex + 1; i < steps.Count; i++)
        {
            if (!steps[i].IsDisabled) return false;
        }

        return true;
    }

    #endregion

    #region Helpers

    private async Task WaitForStabilityAsync()
    {
        await _page.WaitForTimeoutAsync(300);
    }

    #endregion
}

/// <summary>
/// Information about a wizard step
/// </summary>
public class WizardStepInfo
{
    public int Index { get; set; }
    public string Label { get; set; } = "";
    public string? Selection { get; set; }
    public bool IsCurrent { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsDisabled { get; set; }
}
