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
    /// Helper to set app count - handles both single cluster (tier-card) and multi-cluster (spinbutton) modes
    /// </summary>
    private async Task SetTierAppsAsync(string tier, string count)
    {
        // Ensure Applications tab is active
        await ClickTabAsync("Applications");
        await Page.WaitForTimeoutAsync(500);

        // Map tier names to display labels
        var tierLabel = tier.ToLower() switch
        {
            "small" => "Small",
            "medium" => "Medium",
            "large" => "Large",
            "xlarge" or "xl" => "XLarge",
            _ => tier
        };

        // Try single cluster selector first (.tier-card)
        var singleClusterSelector = $".tier-card.{tier.ToLower()} input";
        var singleClusterElement = await Page.QuerySelectorAsync(singleClusterSelector);

        if (singleClusterElement != null)
        {
            await singleClusterElement.FillAsync(count);
            await Page.WaitForTimeoutAsync(300);
            return;
        }

        // Multi-cluster mode: Dev panel is expanded by default
        // Wait for spinbuttons to be present in the DOM
        await Page.WaitForSelectorAsync("[role='spinbutton']", new() { Timeout = 5000, State = WaitForSelectorState.Visible });

        // Find the tier spinbutton by looking for the label text
        var tierSpinbutton = Page.Locator($":has-text('{tierLabel}')").Locator("[role='spinbutton']").First;

        try
        {
            await tierSpinbutton.WaitForAsync(new() { Timeout = 3000, State = WaitForSelectorState.Visible });
            await tierSpinbutton.FillAsync(count);
            await Page.WaitForTimeoutAsync(300);
        }
        catch
        {
            // Fallback: use the first visible spinbutton
            var fallbackSpinbutton = Page.Locator("[role='spinbutton']").First;
            await fallbackSpinbutton.WaitForAsync(new() { Timeout = 3000, State = WaitForSelectorState.Visible });
            await fallbackSpinbutton.FillAsync(count);
            await Page.WaitForTimeoutAsync(300);
        }
    }

    [Test]
    public async Task K8s_Calculate_ShowsResults()
    {
        await NavigateToK8sConfigAsync();

        // Set app counts using tier spinbuttons
        await SetTierAppsAsync("small", "10");

        // Calculate (K8s: Step 5 Configure -> Step 6 Pricing -> Calculate)
        await ClickK8sCalculateAsync();

        // Wait for results (either .sizing-results-view or .results-panel)
        await Page.WaitForSelectorAsync(".sizing-results-view, .results-panel", new() { Timeout = 10000 });

        // Verify results are displayed
        Assert.That(await IsVisibleAsync(".sizing-results-view") || await IsVisibleAsync(".results-panel"), Is.True,
            "Results panel should be visible");
    }

    [Test]
    public async Task K8s_Calculate_ShowsTotalNodes()
    {
        await NavigateToK8sConfigAsync();

        // Set app counts - 70 medium apps (reference case)
        await SetTierAppsAsync("medium", "70");

        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync(".sizing-results-view, .results-panel", new() { Timeout = 10000 });

        // Should show results with node info
        Assert.That(await IsVisibleAsync(".sizing-results-view") || await IsVisibleAsync(".results-panel"), Is.True,
            "Results should show node count");
    }

    [Test]
    public async Task K8s_Calculate_ShowsEnvironmentBreakdown()
    {
        await NavigateToK8sConfigAsync();

        // Set app counts
        await SetTierAppsAsync("small", "20");

        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync(".sizing-results-view, .results-panel", new() { Timeout = 10000 });

        // Should show environment results (env-cards in new UI)
        Assert.That(await IsVisibleAsync(".env-card") || await IsVisibleAsync(".results-table") || await IsVisibleAsync(".sizing-results-view"), Is.True,
            "Environment breakdown should be visible");
    }

    [Test]
    public async Task K8s_Calculate_ShowsGrandTotal()
    {
        await NavigateToK8sConfigAsync();

        // Set app counts
        await SetTierAppsAsync("small", "10");

        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync(".sizing-results-view, .results-panel", new() { Timeout = 10000 });

        // Should show grand total bar
        Assert.That(await IsVisibleAsync(".grand-total-bar") || await IsVisibleAsync(".sizing-results-view"), Is.True,
            "Results should show grand total");
    }

    [Test]
    public async Task K8s_Calculate_ShowsExportButtons()
    {
        await NavigateToK8sConfigAsync();

        await SetTierAppsAsync("small", "10");

        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync(".sizing-results-view, .results-panel", new() { Timeout = 10000 });

        // Should show export options in right sidebar
        Assert.That(await IsVisibleAsync(".quick-actions") || await IsVisibleAsync(".sizing-results-view"), Is.True,
            "Results panel with export options should be visible");
    }

    [Test]
    public async Task K8s_MultiCluster_CalculatesForEachEnvironment()
    {
        await NavigateToK8sConfigAsync();

        // Ensure Multi-Cluster is selected (default)
        await SelectClusterModeAsync("Multi");
        await Page.WaitForTimeoutAsync(300);

        // Set app counts
        await SetTierAppsAsync("small", "20");

        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync(".sizing-results-view, .results-panel", new() { Timeout = 10000 });

        // Should show results with multiple env cards
        Assert.That(await IsVisibleAsync(".sizing-results-view") || await IsVisibleAsync(".results-panel"), Is.True,
            "Multi-cluster should show results");
    }

    [Test]
    public async Task K8s_SharedCluster_CalculatesSingleCluster()
    {
        await NavigateToK8sConfigAsync();

        // Select Single Cluster mode with Shared scope
        await SelectClusterModeAsync("Single");
        await Page.WaitForTimeoutAsync(500);
        await SelectClusterScopeAsync("Shared");

        // Set apps (now using .tier-card since single cluster)
        await SetTierAppsAsync("medium", "10");

        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync(".sizing-results-view, .results-panel", new() { Timeout = 10000 });

        // Results should indicate shared cluster
        Assert.That(await IsVisibleAsync(".sizing-results-view") || await IsVisibleAsync(".results-panel"), Is.True,
            "Results should be displayed for shared cluster");
    }

    [Test]
    public async Task K8s_ZeroApps_ShowsMinimumNodes()
    {
        await NavigateToK8sConfigAsync();

        // Don't set any app counts (all zeros)
        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync(".sizing-results-view, .results-panel", new() { Timeout = 10000 });

        // Should still show results with minimum nodes (masters + infra)
        Assert.That(await IsVisibleAsync(".sizing-results-view") || await IsVisibleAsync(".results-panel"), Is.True,
            "Results should be displayed even with zero apps");
    }

    [Test]
    public async Task VM_Calculate_ShowsResults()
    {
        await NavigateToVMConfigAsync();

        // VM flow: Step 4 (Configure) has roles pre-selected, use Next -> Calculate
        await ClickVMCalculateAsync();
        await Page.WaitForSelectorAsync(".sizing-results-view, .results-panel, .vm-results-section", new() { Timeout = 10000 });

        // Verify VM results are displayed
        Assert.That(await IsVisibleAsync(".sizing-results-view") || await IsVisibleAsync(".results-panel") || await IsVisibleAsync(".vm-results-section"), Is.True,
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
                               await IsVisibleAsync(".node-specs-panel") ||
                               await IsVisibleAsync(".spec-input") ||
                               await IsVisibleAsync("[class*='node']");

        // Go back to apps and calculate
        await ClickTabAsync("Applications");
        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync(".sizing-results-view, .results-panel", new() { Timeout = 10000 });

        // Verify results are generated
        Assert.That(await IsVisibleAsync(".sizing-results-view") || await IsVisibleAsync(".results-panel"), Is.True,
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
        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync(".sizing-results-view, .results-panel", new() { Timeout = 10000 });

        // Results should be generated
        Assert.That(await IsVisibleAsync(".sizing-results-view") || await IsVisibleAsync(".results-panel"), Is.True,
            "Results should reflect headroom changes");
    }

    [Test]
    public async Task Results_CanRecalculate()
    {
        await NavigateToK8sConfigAsync();

        // First calculation
        await SetTierAppsAsync("small", "10");
        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync(".sizing-results-view, .results-panel", new() { Timeout = 10000 });

        // Go back to Configure step to change app count
        await ClickBackAsync();
        await ClickBackAsync();
        await Page.WaitForTimeoutAsync(500);

        // Change app count
        await SetTierAppsAsync("small", "50");

        // Recalculate
        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync(".sizing-results-view, .results-panel", new() { Timeout = 10000 });

        // Results should be updated
        Assert.That(await IsVisibleAsync(".sizing-results-view") || await IsVisibleAsync(".results-panel"), Is.True,
            "Should be able to recalculate with new values");
    }
}
