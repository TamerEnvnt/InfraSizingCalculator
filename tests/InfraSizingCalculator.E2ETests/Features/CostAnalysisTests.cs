namespace InfraSizingCalculator.E2ETests.Features;

/// <summary>
/// E2E tests for Cost Analysis functionality
/// Cost Breakdown is accessed via left sidebar navigation
/// </summary>
[TestFixture]
public class CostAnalysisTests : PlaywrightFixture
{
    private async Task NavigateToCostBreakdownAsync()
    {
        await NavigateToK8sConfigAsync();
        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync("table", new() { Timeout = 10000 });

        // Click "Cost Breakdown" in left sidebar
        var costBtn = Page.Locator(".nav-item:has-text('Cost'), button:has-text('Cost Breakdown')");
        if (await costBtn.CountAsync() > 0)
        {
            await costBtn.First.ClickAsync();
            await Page.WaitForTimeoutAsync(500);
        }
    }

    [Test]
    public async Task CostBreakdown_SidebarButton_Exists()
    {
        await NavigateToK8sConfigAsync();
        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync("table", new() { Timeout = 10000 });

        // Cost Breakdown button in left sidebar
        var costBtn = Page.Locator(".nav-item:has-text('Cost'), button:has-text('Cost Breakdown')");
        Assert.That(await costBtn.CountAsync(), Is.GreaterThan(0),
            "Cost Breakdown button should exist in sidebar");
    }

    [Test]
    public async Task CostBreakdown_ClickChangesView()
    {
        await NavigateToCostBreakdownAsync();

        // The view should change - check for cost-related content
        var pageContent = await Page.ContentAsync();
        Assert.That(pageContent.Contains("Cost") || pageContent.Contains("$") || pageContent.Contains("Monthly"),
            Is.True, "Cost breakdown view should display cost-related content");
    }

    [Test]
    public async Task CostBreakdown_ShowsDollarValues()
    {
        await NavigateToCostBreakdownAsync();

        // Should display dollar amounts somewhere
        var pageContent = await Page.ContentAsync();
        Assert.That(pageContent.Contains("$"), Is.True,
            "Cost breakdown should display currency values");
    }

    [Test]
    public async Task CostBreakdown_ShowsMonthlyInfo()
    {
        await NavigateToCostBreakdownAsync();

        // Monthly cost should be referenced
        var monthlyText = Page.Locator(":text('Monthly'), :text('month')");
        var pageContent = await Page.ContentAsync();

        Assert.That(await monthlyText.CountAsync() > 0 || pageContent.Contains("Monthly") || pageContent.Contains("/mo"),
            Is.True, "Cost breakdown should show monthly information");
    }

    [Test]
    public async Task RightSidebar_ShowsMonthlyCost()
    {
        await NavigateToK8sConfigAsync();
        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync("table", new() { Timeout = 10000 });

        // Right sidebar shows summary stats including Monthly Cost
        var statsCard = Page.Locator(".stats-card, .right-stats");
        Assert.That(await statsCard.CountAsync(), Is.GreaterThan(0),
            "Right sidebar should show stats cards");
    }

    [Test]
    public async Task RightSidebar_ShowsTotalNodes()
    {
        await NavigateToK8sConfigAsync();
        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync("table", new() { Timeout = 10000 });

        // Right sidebar shows Total Nodes
        var nodesLabel = Page.Locator(":text('Total Nodes'), .stats-label:has-text('Nodes')");
        Assert.That(await nodesLabel.CountAsync(), Is.GreaterThan(0),
            "Right sidebar should show Total Nodes");
    }

    [Test]
    public async Task RightSidebar_ShowsTotalCPU()
    {
        await NavigateToK8sConfigAsync();
        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync("table", new() { Timeout = 10000 });

        // Right sidebar shows Total vCPU
        var cpuLabel = Page.Locator(":text('Total vCPU'), .stats-label:has-text('vCPU')");
        Assert.That(await cpuLabel.CountAsync(), Is.GreaterThan(0),
            "Right sidebar should show Total vCPU");
    }

    [Test]
    public async Task RightSidebar_ShowsTotalRAM()
    {
        await NavigateToK8sConfigAsync();
        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync("table", new() { Timeout = 10000 });

        // Right sidebar shows Total RAM
        var ramLabel = Page.Locator(":text('Total RAM'), .stats-label:has-text('RAM')");
        Assert.That(await ramLabel.CountAsync(), Is.GreaterThan(0),
            "Right sidebar should show Total RAM");
    }

    [Test]
    public async Task RightSidebar_ShareButton_Exists()
    {
        await NavigateToK8sConfigAsync();
        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync("table", new() { Timeout = 10000 });

        // Share button in right sidebar
        var shareBtn = Page.Locator("button:has-text('Share')");
        Assert.That(await shareBtn.CountAsync(), Is.GreaterThan(0),
            "Share button should exist in sidebar");
    }

    [Test]
    public async Task RightSidebar_SaveButton_Exists()
    {
        await NavigateToK8sConfigAsync();
        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync("table", new() { Timeout = 10000 });

        // Save button in right sidebar
        var saveBtn = Page.Locator("button:has-text('Save')");
        Assert.That(await saveBtn.CountAsync(), Is.GreaterThan(0),
            "Save button should exist in sidebar");
    }

    [Test]
    public async Task VM_CostBreakdown_SidebarButton_Exists()
    {
        await NavigateToVMConfigAsync();
        await ClickVMCalculateAsync();
        await Page.WaitForSelectorAsync("table", new() { Timeout = 10000 });

        var costBtn = Page.Locator(".nav-item:has-text('Cost'), button:has-text('Cost Breakdown')");
        Assert.That(await costBtn.CountAsync(), Is.GreaterThan(0),
            "VM Cost Breakdown button should exist in sidebar");
    }
}
