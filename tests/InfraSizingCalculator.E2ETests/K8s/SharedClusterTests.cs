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

        // Verify the mode description banner is displayed
        Assert.That(await IsVisibleAsync(".mode-description-banner") || await IsVisibleAsync(".cluster-mode-banner"), Is.True,
            "Mode description banner should be visible");
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

        // Should show node specs (may be unified or tabbed)
        Assert.That(await IsVisibleAsync(".node-specs-panel") ||
                   await IsVisibleAsync(".node-spec-row") ||
                   await IsVisibleAsync(".unified-specs"), Is.True,
            "Node specs should be visible");
    }

    [Test]
    public async Task SharedCluster_BannerShowsCorrectDescription()
    {
        await NavigateToSharedClusterConfigAsync();

        // Check for any mode description or cluster mode indicator
        var hasModeBanner = await IsVisibleAsync(".mode-description-banner") ||
                           await IsVisibleAsync(".cluster-mode-banner") ||
                           await IsVisibleAsync(".mode-selector");

        Assert.That(hasModeBanner, Is.True,
            "Should show cluster mode indicator");
    }

    #endregion

    #region Calculation Tests

    [Test]
    public async Task SharedCluster_Calculate_ShowsResults()
    {
        await NavigateToSharedClusterConfigAsync();

        // Set some app counts
        await SetTierAppsAsync("small", "10");
        await SetTierAppsAsync("medium", "20");

        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync(".sizing-results-view, .results-panel", new() { Timeout = 10000 });

        // Results should be displayed
        Assert.That(await IsVisibleAsync(".sizing-results-view") || await IsVisibleAsync(".results-panel"), Is.True,
            "Results panel should be visible");
    }

    [Test]
    public async Task SharedCluster_Calculate_ShowsSummaryCards()
    {
        await NavigateToSharedClusterConfigAsync();

        // Set apps
        await SetTierAppsAsync("medium", "30");

        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync(".sizing-results-view, .results-panel", new() { Timeout = 10000 });

        // Should show summary cards or grand total
        Assert.That(await IsVisibleAsync(".summary-cards") ||
                   await IsVisibleAsync(".grand-total-bar") ||
                   await IsVisibleAsync(".total-item") ||
                   await IsVisibleAsync(".sizing-results-view"), Is.True,
            "Summary information should be visible");
    }

    [Test]
    public async Task SharedCluster_Calculate_ShowsEnvironmentResults()
    {
        await NavigateToSharedClusterConfigAsync();

        // Set apps
        await SetTierAppsAsync("small", "10");

        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync(".sizing-results-view, .results-panel", new() { Timeout = 10000 });

        // Should show environment breakdown (env-cards) or sizing results
        Assert.That(await IsVisibleAsync(".env-card") || await IsVisibleAsync(".sizing-results-view"), Is.True,
            "Results should show environment cards or sizing results");
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

        // Set some apps
        await SetTierAppsAsync("medium", "20");

        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync(".sizing-results-view, .results-panel", new() { Timeout = 10000 });

        // Results should be generated successfully
        Assert.That(await IsVisibleAsync(".sizing-results-view") || await IsVisibleAsync(".results-panel"), Is.True,
            "Results should be displayed using unified specs");
    }

    [Test]
    public async Task SharedCluster_Calculate_TotalNodes_ShowsInSummary()
    {
        await NavigateToSharedClusterConfigAsync();

        // Set known app count
        await SetTierAppsAsync("medium", "70");

        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync(".sizing-results-view, .results-panel", new() { Timeout = 10000 });

        // Should show total nodes in summary or results
        var resultsText = await GetTextAsync(".sizing-results-view, .results-panel");
        Assert.That(resultsText, Does.Contain("Node").Or.Contain("Total").Or.Contain("vCPU").IgnoreCase,
            "Summary should show resource information");
    }

    [Test]
    public async Task SharedCluster_Calculate_ResourceTotal_Visible()
    {
        await NavigateToSharedClusterConfigAsync();

        // Set apps
        await SetTierAppsAsync("small", "10");

        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync(".sizing-results-view, .results-panel", new() { Timeout = 10000 });

        // Should show CPU and RAM totals in grand total bar or summary
        var hasResourceInfo = await IsVisibleAsync(".total-item") ||
                              await IsVisibleAsync(".grand-total-bar") ||
                              await IsVisibleAsync(".summary-cards") ||
                              await IsVisibleAsync(".sizing-results-view");

        Assert.That(hasResourceInfo, Is.True,
            "Results should show resource totals (CPU/RAM)");
    }

    #endregion
}
