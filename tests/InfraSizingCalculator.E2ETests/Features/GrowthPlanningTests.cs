namespace InfraSizingCalculator.E2ETests.Features;

/// <summary>
/// E2E tests for Growth Planning functionality
/// Tests the growth projection settings, calculation, and visualization
/// </summary>
[TestFixture]
public class GrowthPlanningTests : PlaywrightFixture
{
    private async Task NavigateToGrowthPlanningAsync()
    {
        await NavigateToK8sConfigAsync();
        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync("table", new() { Timeout = 10000 });

        // Click Growth Planning in sidebar
        var growthBtn = Page.Locator("button:has-text('Growth')");
        if (await growthBtn.CountAsync() > 0)
        {
            await growthBtn.ClickAsync();
            await Page.WaitForTimeoutAsync(500);
        }
    }

    [Test]
    public async Task GrowthPlanning_SidebarButton_Exists()
    {
        await NavigateToK8sConfigAsync();
        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync("table", new() { Timeout = 10000 });

        var growthBtn = Page.Locator("button:has-text('Growth')");
        Assert.That(await growthBtn.CountAsync(), Is.GreaterThan(0),
            "Growth Planning button should exist in sidebar");
    }

    [Test]
    public async Task GrowthPlanning_ShowsSettingsBar()
    {
        await NavigateToGrowthPlanningAsync();

        // Check for settings bar with Period, Growth, Pattern controls
        var settingsBar = Page.Locator(".settings-bar-compact, .settings-inline, select:has-text('Year')");
        Assert.That(await settingsBar.CountAsync(), Is.GreaterThan(0),
            "Growth planning should show settings bar");
    }

    [Test]
    public async Task GrowthPlanning_PeriodSelector_Exists()
    {
        await NavigateToGrowthPlanningAsync();

        // Period selector with 1, 3, 5 year options
        var periodSelect = Page.Locator("select:has(option:has-text('Year'))");
        Assert.That(await periodSelect.CountAsync(), Is.GreaterThan(0),
            "Period selector should exist");
    }

    [Test]
    public async Task GrowthPlanning_GrowthRateInput_Exists()
    {
        await NavigateToGrowthPlanningAsync();

        // Growth rate input field
        var growthInput = Page.Locator(".rate-input-sm input, input[type='number']");
        Assert.That(await growthInput.CountAsync(), Is.GreaterThan(0),
            "Growth rate input should exist");
    }

    [Test]
    public async Task GrowthPlanning_PatternSelector_Exists()
    {
        await NavigateToGrowthPlanningAsync();

        // Pattern selector with Linear, Exponential, S-Curve options
        var patternSelect = Page.Locator("select:has(option:has-text('Linear'))");
        Assert.That(await patternSelect.CountAsync(), Is.GreaterThan(0),
            "Pattern selector should exist");
    }

    [Test]
    public async Task GrowthPlanning_CalculateButton_Exists()
    {
        await NavigateToGrowthPlanningAsync();

        var calcButton = Page.Locator(".btn-calc-sm, button:has-text('Calculate')");
        Assert.That(await calcButton.CountAsync(), Is.GreaterThan(0),
            "Calculate button should exist");
    }

    [Test]
    public async Task GrowthPlanning_Calculate_ShowsProjection()
    {
        await NavigateToGrowthPlanningAsync();

        // Click calculate button
        var calcButton = Page.Locator(".btn-calc-sm, button:has-text('Calculate')").First;
        if (await calcButton.CountAsync() > 0)
        {
            await calcButton.ClickAsync();
            await Page.WaitForTimeoutAsync(1000);
        }

        // Should show projection results (hero strip or year cards)
        var results = Page.Locator(".hero-strip, .year-card-sm, .growth-dashboard");
        Assert.That(await results.CountAsync(), Is.GreaterThan(0),
            "Growth projection should show results after calculation");
    }

    [Test]
    public async Task GrowthPlanning_ShowsTabs()
    {
        await NavigateToGrowthPlanningAsync();

        // Calculate first
        var calcButton = Page.Locator(".btn-calc-sm, button:has-text('Calculate')").First;
        if (await calcButton.CountAsync() > 0)
        {
            await calcButton.ClickAsync();
            await Page.WaitForTimeoutAsync(1000);
        }

        // Check for Resources, Cost, Timeline tabs
        var resourcesTab = Page.Locator("button:has-text('Resources')");
        var timelineTab = Page.Locator("button:has-text('Timeline')");

        var hasTabs = await resourcesTab.CountAsync() > 0 || await timelineTab.CountAsync() > 0;
        Assert.That(hasTabs, Is.True,
            "Growth planning should show view tabs");
    }

    [Test]
    public async Task GrowthPlanning_ResourcesTab_ShowsYearCards()
    {
        await NavigateToGrowthPlanningAsync();

        // Calculate
        var calcButton = Page.Locator(".btn-calc-sm, button:has-text('Calculate')").First;
        if (await calcButton.CountAsync() > 0)
        {
            await calcButton.ClickAsync();
            await Page.WaitForTimeoutAsync(1000);
        }

        // Click Resources tab if available
        var resourcesTab = Page.Locator("button:has-text('Resources')");
        if (await resourcesTab.CountAsync() > 0)
        {
            await resourcesTab.ClickAsync();
            await Page.WaitForTimeoutAsync(300);
        }

        // Should show year cards or data table
        var yearContent = Page.Locator(".year-card-sm, .data-table-sm, table");
        Assert.That(await yearContent.CountAsync(), Is.GreaterThan(0),
            "Resources tab should show year breakdown");
    }

    [Test]
    public async Task GrowthPlanning_TimelineTab_ShowsNodes()
    {
        await NavigateToGrowthPlanningAsync();

        // Calculate
        var calcButton = Page.Locator(".btn-calc-sm, button:has-text('Calculate')").First;
        if (await calcButton.CountAsync() > 0)
        {
            await calcButton.ClickAsync();
            await Page.WaitForTimeoutAsync(1000);
        }

        // Click Timeline tab if available
        var timelineTab = Page.Locator("button:has-text('Timeline')");
        if (await timelineTab.CountAsync() > 0)
        {
            await timelineTab.ClickAsync();
            await Page.WaitForTimeoutAsync(300);
        }

        // Should show timeline visual
        var timeline = Page.Locator(".timeline-visual-compact, .tl-nodes, .tl-track");
        Assert.That(await timeline.CountAsync(), Is.GreaterThan(0),
            "Timeline tab should show visual timeline");
    }

    [Test]
    public async Task GrowthPlanning_CostToggle_Exists()
    {
        await NavigateToGrowthPlanningAsync();

        // Cost checkbox toggle
        var costToggle = Page.Locator(".toggle-sm input[type='checkbox'], input[type='checkbox']:near(:text('Cost'))");
        Assert.That(await costToggle.CountAsync(), Is.GreaterThan(0),
            "Cost projection toggle should exist");
    }
}
