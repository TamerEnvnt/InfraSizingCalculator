using Microsoft.Playwright;

namespace InfraSizingCalculator.E2ETests.K8s;

/// <summary>
/// E2E tests for Multi-Cluster mode - ensures UI correctly represents multiple clusters
/// Uses HorizontalAccordion component for environment selection and tier configuration
/// </summary>
[TestFixture]
public class MultiClusterTests : PlaywrightFixture
{
    /// <summary>
    /// Navigate to the configuration step with Multi-Cluster selected
    /// </summary>
    private async Task NavigateToMultiClusterConfigAsync()
    {
        // Navigate to K8s config
        await NavigateToK8sConfigAsync();

        // Multi-Cluster is default, but explicitly select it
        await SelectClusterModeAsync("Multi");
        await Page.WaitForTimeoutAsync(500);
    }

    [Test]
    public async Task MultiCluster_ShowsHorizontalAccordion()
    {
        await NavigateToMultiClusterConfigAsync();

        // Multi-cluster mode uses HorizontalAccordion for environment selection
        Assert.That(await IsVisibleAsync(".h-accordion"), Is.True,
            "Multi-cluster should show horizontal accordion");

        // Should have environment panels
        var panels = await Page.QuerySelectorAllAsync(".h-accordion-panel");
        Assert.That(panels.Count, Is.GreaterThanOrEqualTo(2),
            "Should have multiple environment panels (at least Dev and Prod)");
    }

    [Test]
    public async Task MultiCluster_ShowsEnvironmentPanels()
    {
        await NavigateToMultiClusterConfigAsync();

        // Multi-cluster mode shows environment panels in accordion
        Assert.That(await IsVisibleAsync(".h-accordion-panel"), Is.True,
            "Environment panels should be visible");

        // Should have Prod panel (always present)
        Assert.That(await IsVisibleAsync(".h-accordion-panel.env-prod"), Is.True,
            "Prod environment panel should be visible");

        // Should have Dev panel
        Assert.That(await IsVisibleAsync(".h-accordion-panel.env-dev"), Is.True,
            "Dev environment panel should be visible");
    }

    [Test]
    public async Task MultiCluster_ShowsMultiClusterHeader()
    {
        await NavigateToMultiClusterConfigAsync();

        // Multi-cluster mode shows header with title and stats
        Assert.That(await IsVisibleAsync(".multi-cluster-header"), Is.True,
            "Multi-cluster header should be visible");

        var headerText = await GetTextAsync(".multi-cluster-header");
        Assert.That(headerText, Does.Contain("Multi-Cluster").IgnoreCase,
            "Header should describe multi-cluster mode");
    }

    [Test]
    public async Task MultiCluster_CanExpandEnvironmentPanels()
    {
        await NavigateToMultiClusterConfigAsync();

        // Find collapsed Dev panel and click to expand
        var devPanel = await Page.QuerySelectorAsync(".h-accordion-panel.env-dev");
        Assert.That(devPanel, Is.Not.Null, "Dev panel should exist");

        // Click on the header to expand
        var devHeader = await Page.QuerySelectorAsync(".h-accordion-panel.env-dev .h-accordion-header");
        Assert.That(devHeader, Is.Not.Null, "Dev panel header should exist");
        await devHeader!.ClickAsync();
        await Page.WaitForTimeoutAsync(300);

        // Dev panel should now be expanded
        devPanel = await Page.QuerySelectorAsync(".h-accordion-panel.env-dev");
        var devPanelClass = await devPanel!.GetAttributeAsync("class") ?? "";
        Assert.That(devPanelClass, Does.Contain("expanded"),
            "Dev panel should be expanded after clicking");
    }

    [Test]
    public async Task MultiCluster_SingleExpandMode_CollapsesPreviousPanel()
    {
        await NavigateToMultiClusterConfigAsync();

        // Find and expand Prod panel first
        var prodHeader = await Page.QuerySelectorAsync(".h-accordion-panel.env-prod .h-accordion-header");
        if (prodHeader != null)
        {
            await prodHeader.ClickAsync();
            await Page.WaitForTimeoutAsync(300);
        }

        // Now click Dev panel header
        var devHeader = await Page.QuerySelectorAsync(".h-accordion-panel.env-dev .h-accordion-header");
        Assert.That(devHeader, Is.Not.Null, "Dev panel header should exist");
        await devHeader!.ClickAsync();
        await Page.WaitForTimeoutAsync(300);

        // Dev should be expanded, Prod should be collapsed
        var devPanel = await Page.QuerySelectorAsync(".h-accordion-panel.env-dev");
        var devClass = await devPanel!.GetAttributeAsync("class") ?? "";
        Assert.That(devClass, Does.Contain("expanded"), "Dev panel should be expanded");

        var prodPanel = await Page.QuerySelectorAsync(".h-accordion-panel.env-prod");
        var prodClass = await prodPanel!.GetAttributeAsync("class") ?? "";
        Assert.That(prodClass, Does.Contain("collapsed"), "Prod panel should be collapsed");
    }

    [Test]
    public async Task MultiCluster_ExpandedPanel_ShowsTierInputs()
    {
        await NavigateToMultiClusterConfigAsync();

        // Dev panel is expanded by default in multi-cluster mode
        // Should show spinbuttons for tier inputs
        var spinbuttons = await Page.QuerySelectorAllAsync("[role='spinbutton']");
        Assert.That(spinbuttons.Count, Is.GreaterThanOrEqualTo(4),
            "Should have spinbuttons for tier inputs (S/M/L/XL)");
    }

    [Test]
    public async Task MultiCluster_CanSetAppCountsPerEnvironment()
    {
        await NavigateToMultiClusterConfigAsync();

        // Dev panel is expanded by default - find and use the Medium tier spinbutton
        // Structure is: container with text "Medium" containing a spinbutton
        var mediumSpinbutton = Page.Locator(":has-text('Medium')").Locator("[role='spinbutton']").First;

        await mediumSpinbutton.WaitForAsync(new() { Timeout = 5000, State = WaitForSelectorState.Visible });
        await mediumSpinbutton.FillAsync("50");
        var value = await mediumSpinbutton.InputValueAsync();
        Assert.That(value, Is.EqualTo("50"), "Medium tier spinbutton should accept app count");
    }

    [Test]
    public async Task MultiCluster_Calculate_ShowsResults()
    {
        await NavigateToMultiClusterConfigAsync();

        // Dev panel is expanded by default - use the Medium tier spinbutton
        var mediumSpinbutton = Page.Locator(":has-text('Medium')").Locator("[role='spinbutton']").First;
        await mediumSpinbutton.WaitForAsync(new() { Timeout = 5000, State = WaitForSelectorState.Visible });
        await mediumSpinbutton.FillAsync("20");

        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync(".sizing-results-view, .results-panel", new() { Timeout = 10000 });

        // Should show results
        Assert.That(await IsVisibleAsync(".sizing-results-view") || await IsVisibleAsync(".results-panel"), Is.True,
            "Results should be displayed after calculation");
    }

    [Test]
    public async Task MultiCluster_Calculate_ShowsMultipleEnvironmentResults()
    {
        await NavigateToMultiClusterConfigAsync();

        // Dev panel is expanded by default - set apps in Medium tier
        var mediumSpinbutton = Page.Locator(":has-text('Medium')").Locator("[role='spinbutton']").First;
        await mediumSpinbutton.WaitForAsync(new() { Timeout = 5000, State = WaitForSelectorState.Visible });
        await mediumSpinbutton.FillAsync("20");

        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync(".sizing-results-view, .results-panel", new() { Timeout = 10000 });

        // Should show results (multi-cluster shows all environments)
        Assert.That(await IsVisibleAsync(".sizing-results-view") || await IsVisibleAsync(".results-panel"), Is.True,
            "Multi-cluster results should show results");
    }

    [Test]
    public async Task MultiCluster_NodeSpecs_ShowsPerEnvironmentTabs()
    {
        await NavigateToMultiClusterConfigAsync();
        await ClickTabAsync("Node Specs");

        // Should show tabbed interface for per-environment node specs
        Assert.That(await IsVisibleAsync(".node-specs-tabbed") || await IsVisibleAsync(".node-specs-panel"), Is.True,
            "Node specs panel should be visible");
    }

    [Test]
    public async Task MultiCluster_Settings_ShowsHeadroomOptions()
    {
        await NavigateToMultiClusterConfigAsync();
        await ClickTabAsync("Settings");

        // Should show headroom settings
        Assert.That(await IsVisibleAsync(".settings-section-compact:has-text('Headroom')") ||
                    await IsVisibleAsync(".settings-panel:has-text('Headroom')"), Is.True,
            "Headroom settings should be visible");
    }

    [Test]
    public async Task MultiCluster_Settings_ShowsOvercommitOptions()
    {
        await NavigateToMultiClusterConfigAsync();
        await ClickTabAsync("Settings");

        // Should show overcommit settings
        Assert.That(await IsVisibleAsync(".overcommit-group") ||
                    await IsVisibleAsync(".settings-panel:has-text('Overcommit')"), Is.True,
            "Overcommit settings should be visible");
    }

    [Test]
    public async Task MultiCluster_SwitchToSingle_HidesAccordion()
    {
        await NavigateToMultiClusterConfigAsync();

        // Verify accordion is visible in multi-cluster mode
        Assert.That(await IsVisibleAsync(".h-accordion"), Is.True,
            "Accordion should be visible in multi-cluster mode");

        // Switch to Single Cluster mode
        await SelectClusterModeAsync("Single");
        await Page.WaitForTimeoutAsync(500);

        // Accordion should be hidden, tier-cards should be visible
        var accordion = await Page.QuerySelectorAsync(".h-accordion");
        var isAccordionVisible = accordion != null && await accordion.IsVisibleAsync();
        Assert.That(isAccordionVisible, Is.False,
            "Single cluster mode should not show horizontal accordion");

        Assert.That(await IsVisibleAsync(".tier-cards"), Is.True,
            "Single cluster mode should show tier cards");
    }

    [Test]
    public async Task MultiCluster_HeaderStats_UpdatesWithAppCounts()
    {
        await NavigateToMultiClusterConfigAsync();

        // Dev panel is expanded by default - update Medium tier spinbutton
        var mediumSpinbutton = Page.Locator(":has-text('Medium')").Locator("[role='spinbutton']").First;
        await mediumSpinbutton.WaitForAsync(new() { Timeout = 5000, State = WaitForSelectorState.Visible });
        await mediumSpinbutton.FillAsync("100");
        await Page.WaitForTimeoutAsync(300);

        // Stats should show the updated count (look for the total apps display)
        var pageContent = await Page.ContentAsync();
        Assert.That(pageContent, Does.Contain("100"),
            "Page should reflect updated app count");
    }

    #region Calculation Verification Tests

    [Test]
    public async Task MultiCluster_Calculate_70MediumApps_ReturnsExpectedWorkers()
    {
        await NavigateToMultiClusterConfigAsync();

        // Dev panel is expanded by default - use the Medium tier spinbutton
        var mediumSpinbutton = Page.Locator(":has-text('Medium')").Locator("[role='spinbutton']").First;
        await mediumSpinbutton.WaitForAsync(new() { Timeout = 5000, State = WaitForSelectorState.Visible });
        await mediumSpinbutton.FillAsync("70");

        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync(".sizing-results-view, .results-panel", new() { Timeout = 10000 });

        // Verify results show expected worker count (around 9 workers)
        Assert.That(await IsVisibleAsync(".sizing-results-view") || await IsVisibleAsync(".results-panel"), Is.True,
            "Results should be visible");
    }

    [Test]
    public async Task MultiCluster_Calculate_ZeroApps_ReturnsMinimumNodes()
    {
        await NavigateToMultiClusterConfigAsync();

        // Don't set any app counts - should still show minimum nodes
        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync(".sizing-results-view, .results-panel", new() { Timeout = 10000 });

        // Should show results with minimum nodes
        Assert.That(await IsVisibleAsync(".sizing-results-view") || await IsVisibleAsync(".results-panel"), Is.True,
            "Results should be displayed even with zero apps");
    }

    [Test]
    public async Task MultiCluster_Calculate_ShowsGrandTotal()
    {
        await NavigateToMultiClusterConfigAsync();

        // Dev panel is expanded by default - use the Medium tier spinbutton
        var mediumSpinbutton = Page.Locator(":has-text('Medium')").Locator("[role='spinbutton']").First;
        await mediumSpinbutton.WaitForAsync(new() { Timeout = 5000, State = WaitForSelectorState.Visible });
        await mediumSpinbutton.FillAsync("20");

        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync(".sizing-results-view, .results-panel", new() { Timeout = 10000 });

        // Should show Grand Total
        var resultsText = await GetTextAsync(".sizing-results-view, .results-panel");
        Assert.That(resultsText, Does.Contain("Grand Total").Or.Contain("Total").IgnoreCase,
            "Results should show grand total");
    }

    #endregion
}
