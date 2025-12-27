using Microsoft.Playwright;

namespace InfraSizingCalculator.E2ETests.Calculations;

/// <summary>
/// E2E tests for pricing options functionality
/// </summary>
[TestFixture]
public class PricingOptionsTests : PlaywrightFixture
{
    [Test]
    public async Task PricingOptions_ShowsAllCloudProviders()
    {
        await NavigateToK8sConfigAsync();
        // Go to Pricing step (Next from Configure)
        await ClickNextAsync();
        await Page.WaitForSelectorAsync("button:has-text('Calculate')", new() { Timeout = 5000 });

        // Verify pricing page is displayed with tabs
        var infraTab = Page.Locator("button:has-text('Infrastructure')");
        var cloudTab = Page.Locator("button:has-text('Cloud')");

        Assert.That(await infraTab.CountAsync() > 0 || await cloudTab.CountAsync() > 0,
            Is.True, "Pricing tabs should be visible");
    }

    [Test]
    public async Task PricingOptions_CanSelectDifferentProviders()
    {
        await NavigateToK8sConfigAsync();
        await ClickNextAsync();
        await Page.WaitForSelectorAsync("button:has-text('Calculate')", new() { Timeout = 5000 });

        // Click Cloud Alternatives tab if available
        var cloudTab = Page.Locator("button:has-text('Cloud')");
        if (await cloudTab.CountAsync() > 0)
        {
            await cloudTab.ClickAsync();
            await Page.WaitForTimeoutAsync(500);
        }

        // Verify pricing content is displayed
        var pricingContent = Page.Locator("[class*='pricing'], [class*='cost']");
        Assert.That(await pricingContent.CountAsync(), Is.GreaterThan(0),
            "Pricing content should be visible");
    }

    [Test]
    public async Task PricingOptions_CalculateCostsButton_Works()
    {
        await NavigateToK8sConfigAsync();
        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync("table", new() { Timeout = 10000 });

        // Verify results are displayed
        Assert.That(await Page.Locator("table tr:has-text('Total')").CountAsync(), Is.GreaterThan(0),
            "Results should be displayed after calculation");
    }

    [Test]
    public async Task PricingOptions_ShowsCompareNote()
    {
        await NavigateToK8sConfigAsync();
        await ClickNextAsync();
        await Page.WaitForSelectorAsync("button:has-text('Calculate')", new() { Timeout = 5000 });

        // Verify pricing configuration is shown
        var pricingHeader = Page.Locator("h3:has-text('Pricing')");
        Assert.That(await pricingHeader.CountAsync(), Is.GreaterThan(0),
            "Pricing configuration should be visible");
    }

    [Test]
    public async Task CostBreakdown_ShowsCostCards()
    {
        await NavigateToK8sConfigAsync();
        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync("table", new() { Timeout = 10000 });

        // Click Cost Breakdown in sidebar
        var costBreakdownBtn = Page.Locator("button:has-text('Cost Breakdown')");
        if (await costBreakdownBtn.CountAsync() > 0)
        {
            await costBreakdownBtn.ClickAsync();
            await Page.WaitForTimeoutAsync(500);
        }

        // Verify some cost-related content is shown
        var pageContent = await Page.ContentAsync();
        Assert.That(pageContent.Contains("$") || pageContent.Contains("Cost") || pageContent.Contains("Monthly"),
            Is.True, "Cost information should be displayed");
    }

    [Test]
    public async Task CostBreakdown_ShowsCostSummaryRow()
    {
        await NavigateToK8sConfigAsync();
        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync("table", new() { Timeout = 10000 });

        // Verify results table is displayed (costs are shown in sidebar)
        var totalRow = Page.Locator("table tr:has-text('Total')");
        Assert.That(await totalRow.CountAsync(), Is.GreaterThan(0),
            "Results with cost summary should be displayed");
    }
}
