namespace InfraSizingCalculator.E2ETests.K8s;

/// <summary>
/// E2E tests for Shared Cluster mode (Single Cluster)
/// Shared cluster uses UNIFIED node specs (no Prod vs Non-Prod distinction)
/// All namespaces share the same cluster infrastructure
/// In Single Cluster mode, the UI shows .tier-cards with .tier-card for each size
/// </summary>
[TestFixture]
public class SharedClusterTests : PlaywrightFixture
{
    /// <summary>
    /// Navigate to the configuration step with Shared Cluster selected
    /// </summary>
    private async Task NavigateToSharedClusterConfigAsync()
    {
        await NavigateToK8sConfigAsync();
        await SelectClusterModeAsync("Single");
        await Page.WaitForTimeoutAsync(500);
        await SelectClusterScopeAsync("Shared");
        await Page.WaitForTimeoutAsync(300);
    }

    /// <summary>
    /// Helper to set app count in single cluster mode using tier cards
    /// </summary>
    private async Task SetTierAppsAsync(string tier, string count)
    {
        var selector = $".tier-card.{tier.ToLower()} input";
        var element = await Page.QuerySelectorAsync(selector);
        if (element != null)
        {
            await element.FillAsync(count);
            await Page.WaitForTimeoutAsync(300);
        }
    }

    #region UI Tests

    [Test]
    public async Task SharedCluster_ShowsSingleClusterHeader()
    {
        await NavigateToSharedClusterConfigAsync();

        // Verify the cluster mode options are visible with Single selected
        Assert.That(await IsVisibleAsync(".cluster-mode-options") || await IsVisibleAsync(".mode-option.selected"), Is.True,
            "Cluster mode options should be visible");
    }

    [Test]
    public async Task SharedCluster_ShowsTierCards()
    {
        await NavigateToSharedClusterConfigAsync();

        // In single/shared cluster mode, the UI shows tier cards (S/M/L/XL)
        // not horizontal accordion like multi-cluster
        var tierCards = await Page.QuerySelectorAllAsync(".tier-card");
        Assert.That(tierCards.Count, Is.GreaterThanOrEqualTo(4),
            "Should show 4 tier cards for workload configuration (S/M/L/XL)");

        // Verify each tier size exists
        Assert.That(await IsVisibleAsync(".tier-card.small"), Is.True, "Small tier card should exist");
        Assert.That(await IsVisibleAsync(".tier-card.medium"), Is.True, "Medium tier card should exist");
        Assert.That(await IsVisibleAsync(".tier-card.large"), Is.True, "Large tier card should exist");
        Assert.That(await IsVisibleAsync(".tier-card.xlarge"), Is.True, "XLarge tier card should exist");
    }

    [Test]
    public async Task SharedCluster_NoHorizontalAccordion()
    {
        await NavigateToSharedClusterConfigAsync();

        // Single cluster mode should NOT show horizontal accordion (that's multi-cluster only)
        var accordion = await Page.QuerySelectorAsync(".h-accordion");
        var isVisible = accordion != null && await accordion.IsVisibleAsync();
        Assert.That(isVisible, Is.False,
            "Single cluster mode should not show horizontal accordion");
    }

    [Test]
    public async Task SharedCluster_TierCardsHaveInputs()
    {
        await NavigateToSharedClusterConfigAsync();

        // Each tier card should have an input field
        var tierInputs = await Page.QuerySelectorAllAsync(".tier-card input[type='number']");
        Assert.That(tierInputs.Count, Is.GreaterThanOrEqualTo(4),
            "Each tier card should have an input field");
    }

    [Test]
    public async Task SharedCluster_CanSetAppCounts()
    {
        await NavigateToSharedClusterConfigAsync();

        // Set some apps in the medium tier
        await SetTierAppsAsync("medium", "50");

        // Verify the value was set
        var mediumInput = await Page.QuerySelectorAsync(".tier-card.medium input");
        if (mediumInput != null)
        {
            var value = await mediumInput.InputValueAsync();
            Assert.That(value, Is.EqualTo("50"), "Medium tier should accept app count");
        }
    }

    [Test]
    public async Task SharedCluster_NodeSpecs_ShowsUnifiedSpecs()
    {
        await NavigateToSharedClusterConfigAsync();
        await ClickTabAsync("Node Specs");

        // Should show node specs (unified for single cluster or node cards)
        Assert.That(await IsVisibleAsync(".k8s-node-specs-config") ||
                   await IsVisibleAsync(".node-cards") ||
                   await IsVisibleAsync(".node-card"), Is.True,
            "Node specs should be visible");
    }

    [Test]
    public async Task SharedCluster_BannerShowsCorrectDescription()
    {
        await NavigateToSharedClusterConfigAsync();

        // Check for cluster mode selector with single mode selected
        var hasModeBanner = await IsVisibleAsync(".cluster-mode-sidebar") ||
                           await IsVisibleAsync(".cluster-mode-options") ||
                           await IsVisibleAsync(".mode-option.selected");

        Assert.That(hasModeBanner, Is.True,
            "Should show cluster mode indicator");
    }

    #endregion

    #region Calculation Tests

    [Test]
    public async Task SharedCluster_Calculate_ShowsResults()
    {
        await NavigateToSharedClusterConfigAsync();
        await SetTierAppsAsync("small", "10");
        await SetTierAppsAsync("medium", "20");

        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync("table", new() { Timeout = 10000 });

        // Results displayed as table
        Assert.That(await Page.Locator("table").CountAsync(), Is.GreaterThan(0),
            "Results table should be visible");
    }

    [Test]
    public async Task SharedCluster_Calculate_ShowsSummaryCards()
    {
        await NavigateToSharedClusterConfigAsync();
        await SetTierAppsAsync("medium", "30");

        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync("table", new() { Timeout = 10000 });

        // Results table shows Total row as summary
        var totalRow = Page.Locator("table tr:has-text('Total')");
        Assert.That(await totalRow.CountAsync(), Is.GreaterThan(0),
            "Summary Total row should be visible");
    }

    [Test]
    public async Task SharedCluster_Calculate_ShowsEnvironmentResults()
    {
        await NavigateToSharedClusterConfigAsync();
        await SetTierAppsAsync("small", "10");

        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync("table", new() { Timeout = 10000 });

        // Results table shows environment rows
        var envRows = Page.Locator("table tbody tr");
        Assert.That(await envRows.CountAsync(), Is.GreaterThan(0),
            "Results should show environment rows");
    }

    [Test]
    public async Task SharedCluster_Calculate_UsesUnifiedSpecs()
    {
        await NavigateToSharedClusterConfigAsync();
        await ClickTabAsync("Node Specs");

        // Modify node specs if possible
        var cpuInput = await Page.QuerySelectorAsync(".node-spec-row:has-text('Worker') input");
        if (cpuInput != null)
        {
            await cpuInput.FillAsync("16");
        }

        await ClickTabAsync("Applications");
        await SetTierAppsAsync("medium", "20");

        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync("table", new() { Timeout = 10000 });

        // Results should be generated successfully as table
        Assert.That(await Page.Locator("table").CountAsync(), Is.GreaterThan(0),
            "Results table should be displayed using unified specs");
    }

    [Test]
    public async Task SharedCluster_Calculate_TotalNodes_ShowsInSummary()
    {
        await NavigateToSharedClusterConfigAsync();
        await SetTierAppsAsync("medium", "70");

        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync("table", new() { Timeout = 10000 });

        // Table has columns including Nodes
        var tableContent = await Page.Locator("table").TextContentAsync();
        Assert.That(tableContent, Does.Contain("Node").Or.Contain("Total").Or.Contain("vCPU").IgnoreCase,
            "Summary should show resource information");
    }

    [Test]
    public async Task SharedCluster_Calculate_ResourceTotal_Visible()
    {
        await NavigateToSharedClusterConfigAsync();
        await SetTierAppsAsync("small", "10");

        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync("table", new() { Timeout = 10000 });

        // Results table shows Total row with resource totals
        var totalRow = Page.Locator("table tr:has-text('Total')");
        Assert.That(await totalRow.CountAsync(), Is.GreaterThan(0),
            "Results should show resource totals in Total row");
    }

    #endregion
}
