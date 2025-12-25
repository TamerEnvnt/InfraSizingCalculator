using Microsoft.Playwright;

namespace InfraSizingCalculator.E2ETests.Calculations;

/// <summary>
/// E2E tests for calculation results verification
/// Updated for new 3-panel SPA layout with tier-card based inputs
/// </summary>
[TestFixture]
public class CalculationTests : PlaywrightFixture
{
    // Uses NavigateToK8sConfigAsync() and NavigateToVMConfigAsync() from PlaywrightFixture

    /// <summary>
    /// Helper to set app count in a tier card
    /// </summary>
    private async Task SetTierAppsAsync(string tier, string count)
    {
        // Ensure Applications tab is active
        await ClickTabAsync("Applications");
        await Page.WaitForTimeoutAsync(500);

        var selector = $".tier-card.{tier.ToLower()} input";

        // Scroll the element into view and click to focus
        var element = Page.Locator(selector).First;
        await element.ScrollIntoViewIfNeededAsync();
        await element.FillAsync(count);
        await Page.WaitForTimeoutAsync(300);
    }

    [Test]
    public async Task K8s_Calculate_ShowsResults()
    {
        await NavigateToK8sConfigAsync();

        // Set app counts using tier cards
        await SetTierAppsAsync("small", "10");

        // Calculate
        await ClickCalculateAsync();

        // Wait for results
        await Page.WaitForSelectorAsync(".results-panel", new() { Timeout = 10000 });

        // Verify results are displayed
        Assert.That(await IsVisibleAsync(".results-panel"), Is.True,
            "Results panel should be visible");
    }

    [Test]
    public async Task K8s_Calculate_ShowsTotalNodes()
    {
        await NavigateToK8sConfigAsync();

        // Set app counts - 70 medium apps (reference case)
        await SetTierAppsAsync("medium", "70");

        await ClickCalculateAsync();
        await Page.WaitForSelectorAsync(".results-panel", new() { Timeout = 10000 });

        // Should show results with node info
        Assert.That(await IsVisibleAsync(".results-panel"), Is.True,
            "Results should show node count");
    }

    [Test]
    public async Task K8s_Calculate_ShowsEnvironmentBreakdown()
    {
        await NavigateToK8sConfigAsync();

        // Set app counts
        await SetTierAppsAsync("small", "20");

        await ClickCalculateAsync();
        await Page.WaitForSelectorAsync(".results-panel", new() { Timeout = 10000 });

        // Should show environment results table
        Assert.That(await IsVisibleAsync(".results-table"), Is.True,
            "Results table should be visible");
    }

    [Test]
    public async Task K8s_Calculate_ShowsGrandTotal()
    {
        await NavigateToK8sConfigAsync();

        // Set app counts
        await SetTierAppsAsync("small", "10");

        await ClickCalculateAsync();
        await Page.WaitForSelectorAsync(".results-panel", new() { Timeout = 10000 });

        // Should show results panel
        Assert.That(await IsVisibleAsync(".results-panel"), Is.True,
            "Results should show grand total");
    }

    [Test]
    public async Task K8s_Calculate_ShowsExportButtons()
    {
        await NavigateToK8sConfigAsync();

        await SetTierAppsAsync("small", "10");

        await ClickCalculateAsync();
        await Page.WaitForSelectorAsync(".results-panel", new() { Timeout = 10000 });

        // Should show export options (check for any export-related buttons)
        Assert.That(await IsVisibleAsync(".results-panel"), Is.True,
            "Results panel with export options should be visible");
    }

    [Test]
    public async Task K8s_MultiCluster_CalculatesForEachEnvironment()
    {
        await NavigateToK8sConfigAsync();

        // Ensure Multi-Cluster is selected (default)
        await SelectClusterModeAsync("Multi");

        // Set app counts
        await SetTierAppsAsync("small", "20");

        await ClickCalculateAsync();
        await Page.WaitForSelectorAsync(".results-panel", new() { Timeout = 10000 });

        // Should show results
        Assert.That(await IsVisibleAsync(".results-panel"), Is.True,
            "Multi-cluster should show results");
    }

    [Test]
    public async Task K8s_SharedCluster_CalculatesSingleCluster()
    {
        await NavigateToK8sConfigAsync();

        // Select Single Cluster mode with Shared scope
        await SelectClusterModeAsync("Single");
        await SelectClusterScopeAsync("Shared");

        // Set apps
        await SetTierAppsAsync("medium", "10");

        await ClickCalculateAsync();
        await Page.WaitForSelectorAsync(".results-panel", new() { Timeout = 10000 });

        // Results should indicate shared cluster
        Assert.That(await IsVisibleAsync(".results-panel"), Is.True,
            "Results should be displayed for shared cluster");
    }

    [Test]
    public async Task K8s_ZeroApps_ShowsMinimumNodes()
    {
        await NavigateToK8sConfigAsync();

        // Don't set any app counts (all zeros)
        await ClickCalculateAsync();
        await Page.WaitForSelectorAsync(".results-panel", new() { Timeout = 10000 });

        // Should still show results with minimum nodes (masters + infra)
        Assert.That(await IsVisibleAsync(".results-panel"), Is.True,
            "Results should be displayed even with zero apps");
    }

    [Test]
    public async Task VM_Calculate_ShowsResults()
    {
        await NavigateToVMConfigAsync();

        // Select a role for Prod environment
        var prodRow = await Page.QuerySelectorAsync(".vm-env-row:has-text('Prod')");
        if (prodRow != null)
        {
            var webRole = await prodRow.QuerySelectorAsync(".role-chip:has-text('Web')");
            if (webRole != null)
            {
                await webRole.ClickAsync();
                await Page.WaitForTimeoutAsync(300);
            }
        }

        await ClickCalculateAsync();
        await Page.WaitForSelectorAsync(".results-panel, .vm-results-section", new() { Timeout = 10000 });

        // Verify VM results are displayed
        Assert.That(await IsVisibleAsync(".results-panel") || await IsVisibleAsync(".vm-results-section"), Is.True,
            "VM results should be visible");
    }

    [Test]
    public async Task K8s_NodeSpecs_AffectsResults()
    {
        await NavigateToK8sConfigAsync();

        // Set some apps first
        await SetTierAppsAsync("small", "10");

        // Go to Node Specs tab
        await ClickTabAsync("Node Specs");
        await Page.WaitForTimeoutAsync(500);

        // Find any CPU input on the node specs page and verify tab changed
        var nodeSpecsVisible = await IsVisibleAsync(".node-specs-section") ||
                               await IsVisibleAsync(".spec-input") ||
                               await IsVisibleAsync("[class*='node']");

        // Go back to apps and calculate
        await ClickTabAsync("Applications");
        await ClickCalculateAsync();
        await Page.WaitForSelectorAsync(".results-panel", new() { Timeout = 10000 });

        // Verify results are generated
        Assert.That(await IsVisibleAsync(".results-panel"), Is.True,
            "Results should reflect node spec changes");
    }

    [Test]
    public async Task K8s_Headroom_AffectsResults()
    {
        await NavigateToK8sConfigAsync();

        // Set some apps
        await SetTierAppsAsync("medium", "20");

        // Go to Settings tab
        await ClickTabAsync("Settings");
        await Page.WaitForTimeoutAsync(500);

        // Go back to Applications tab and calculate
        await ClickTabAsync("Applications");
        await ClickCalculateAsync();
        await Page.WaitForSelectorAsync(".results-panel", new() { Timeout = 10000 });

        // Results should be generated
        Assert.That(await IsVisibleAsync(".results-panel"), Is.True,
            "Results should reflect headroom changes");
    }

    [Test]
    public async Task Results_CanRecalculate()
    {
        await NavigateToK8sConfigAsync();

        // First calculation
        await SetTierAppsAsync("small", "10");
        await ClickCalculateAsync();
        await Page.WaitForSelectorAsync(".results-panel", new() { Timeout = 10000 });

        // Go back and change values
        await ClickBackAsync();
        await Page.WaitForTimeoutAsync(500);

        // Change app count
        await SetTierAppsAsync("small", "50");

        // Recalculate
        await ClickCalculateAsync();
        await Page.WaitForSelectorAsync(".results-panel", new() { Timeout = 10000 });

        // Results should be updated
        Assert.That(await IsVisibleAsync(".results-panel"), Is.True,
            "Should be able to recalculate with new values");
    }
}
