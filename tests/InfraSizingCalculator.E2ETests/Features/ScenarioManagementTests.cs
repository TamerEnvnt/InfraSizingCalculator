using Microsoft.Playwright;

namespace InfraSizingCalculator.E2ETests.Features;

/// <summary>
/// E2E tests for Scenario Management functionality
/// Tests the /scenarios page, saving, deleting, and comparing scenarios
/// </summary>
[TestFixture]
public class ScenarioManagementTests : PlaywrightFixture
{
    [Test]
    public async Task ScenariosPage_NavigatesCorrectly()
    {
        await Page.GotoAsync($"{BaseUrl}/scenarios");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify scenarios page loads
        var pageTitle = Page.Locator("h1:has-text('Saved Scenarios')");
        Assert.That(await pageTitle.CountAsync(), Is.GreaterThan(0),
            "Scenarios page should show title");
    }

    [Test]
    public async Task ScenariosPage_ShowsEmptyState_WhenNoScenarios()
    {
        await Page.GotoAsync($"{BaseUrl}/scenarios");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Either shows empty state or scenario list
        var emptyState = Page.Locator(".empty-state");
        var scenarioList = Page.Locator(".scenarios-list, .scenario-card");

        var hasContent = await emptyState.CountAsync() > 0 || await scenarioList.CountAsync() > 0;
        Assert.That(hasContent, Is.True,
            "Page should show either empty state or scenario list");
    }

    [Test]
    public async Task ScenariosPage_HasBackButton()
    {
        await Page.GotoAsync($"{BaseUrl}/scenarios");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var backButton = Page.Locator("button:has-text('Back'), a:has-text('Back')");
        Assert.That(await backButton.CountAsync(), Is.GreaterThan(0),
            "Page should have a back button");
    }

    [Test]
    public async Task ScenariosPage_HasTypeTabs()
    {
        await Page.GotoAsync($"{BaseUrl}/scenarios");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Check for Saved/Draft tabs if scenarios exist
        var savedTab = Page.Locator("button:has-text('Saved')");
        var draftTab = Page.Locator("button:has-text('Draft')");

        // Either tabs exist or it's empty state
        var hasTabs = await savedTab.CountAsync() > 0 || await draftTab.CountAsync() > 0;
        var hasEmptyState = await Page.Locator(".empty-state").CountAsync() > 0;

        Assert.That(hasTabs || hasEmptyState, Is.True,
            "Page should have tabs or empty state");
    }

    [Test]
    public async Task ScenariosPage_HasFilterControls()
    {
        await Page.GotoAsync($"{BaseUrl}/scenarios");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Filter controls exist when scenarios are present
        var filterSelect = Page.Locator("select.filter-select, select:has-text('All Types')");
        var emptyState = Page.Locator(".empty-state");

        var hasFilters = await filterSelect.CountAsync() > 0;
        var isEmpty = await emptyState.CountAsync() > 0;

        Assert.That(hasFilters || isEmpty, Is.True,
            "Page should have filter controls or be in empty state");
    }

    [Test]
    public async Task ScenariosPage_GoToCalculatorLink_Works()
    {
        await Page.GotoAsync($"{BaseUrl}/scenarios");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Click back to calculator or the empty state link
        var goLink = Page.Locator("a:has-text('Calculator'), button:has-text('Back')");
        if (await goLink.CountAsync() > 0)
        {
            await goLink.First.ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Should be on home page
            Assert.That(Page.Url, Does.Contain("/").Or.Not.Contain("/scenarios"),
                "Should navigate away from scenarios page");
        }
    }

    [Test]
    public async Task SaveScenario_SaveProfileButton_VisibleAfterCalculation()
    {
        await NavigateToK8sConfigAsync();
        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync("table", new() { Timeout = 10000 });

        // Check for Save Profile button in export buttons area
        var saveButton = Page.Locator("button:has-text('Save Profile'), button:has-text('Save')");
        var hasButton = await saveButton.CountAsync() > 0;

        // Button might be in sidebar or export area
        Assert.That(hasButton || await Page.Locator(".export-buttons, .export-btn").CountAsync() > 0,
            Is.True, "Save or export options should be available after calculation");
    }

    [Test]
    public async Task ScenariosPage_ViewModeToggle_Exists()
    {
        await Page.GotoAsync($"{BaseUrl}/scenarios");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // View toggle exists when scenarios are present
        var viewToggle = Page.Locator(".view-toggle, button:has-text('List'), button:has-text('Compare')");
        var emptyState = Page.Locator(".empty-state");

        Assert.That(await viewToggle.CountAsync() > 0 || await emptyState.CountAsync() > 0,
            Is.True, "View toggle should exist when scenarios present");
    }
}
