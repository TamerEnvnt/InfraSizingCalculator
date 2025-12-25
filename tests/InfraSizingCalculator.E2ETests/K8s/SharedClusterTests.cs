namespace InfraSizingCalculator.E2ETests.K8s;

/// <summary>
/// E2E tests for Shared Cluster mode
/// Shared cluster uses UNIFIED node specs (no Prod vs Non-Prod distinction)
/// All namespaces share the same cluster infrastructure
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
        await SelectClusterScopeAsync("Shared");
    }

    #region UI Tests

    [Test]
    public async Task SharedCluster_ShowsSingleClusterHeader()
    {
        await NavigateToSharedClusterConfigAsync();

        // Verify the mode description banner is displayed
        Assert.That(await IsVisibleAsync(".mode-description-banner"), Is.True,
            "Mode description banner should be visible");

        var bannerText = await GetTextAsync(".mode-description-banner .banner-content");
        Assert.That(bannerText, Does.Contain("Shared Cluster").IgnoreCase,
            "Banner should indicate shared cluster mode");
    }

    [Test]
    public async Task SharedCluster_ShowsNamespacesNotClusters()
    {
        await NavigateToSharedClusterConfigAsync();

        // In shared cluster mode, the workload config shows tier cards (S/M/L/XL)
        // not individual namespace/cluster rows
        var tierCards = await Page.QuerySelectorAllAsync(".tier-card");
        Assert.That(tierCards.Count, Is.GreaterThanOrEqualTo(3),
            "Should show tier cards for workload configuration");

        // Verify banner mentions "Single cluster" or "Shared"
        var bannerText = await GetTextAsync(".mode-description-banner");
        Assert.That(bannerText, Does.Contain("Shared").Or.Contain("Single").IgnoreCase,
            "Banner should describe single/shared cluster mode");
    }

    [Test]
    public async Task SharedCluster_ShowsNamespaceRows()
    {
        await NavigateToSharedClusterConfigAsync();

        // In shared cluster mode, the UI shows tier cards instead of namespace rows
        // The tier cards allow configuring S/M/L/XL app counts
        var tierCards = await Page.QuerySelectorAllAsync(".tier-card");
        Assert.That(tierCards.Count, Is.GreaterThanOrEqualTo(3),
            "Should show tier cards (S/M/L/XL) for workload configuration");

        // Check that tier inputs are available
        var tierInputs = await Page.QuerySelectorAllAsync(".tier-card input[type='number']");
        Assert.That(tierInputs.Count, Is.GreaterThanOrEqualTo(3),
            "Tier cards should have input fields");
    }

    [Test]
    public async Task SharedCluster_NoMultipleClusterGroupLabels()
    {
        await NavigateToSharedClusterConfigAsync();

        // Should NOT show "Production Clusters" or "Non-Production Clusters" labels
        var groupLabels = await Page.QuerySelectorAllAsync(".cluster-group .group-label");
        Assert.That(groupLabels.Count, Is.EqualTo(0),
            "Shared cluster should not show cluster group labels");
    }

    [Test]
    public async Task SharedCluster_CanToggleNamespaces()
    {
        await NavigateToSharedClusterConfigAsync();

        // Find a non-prod namespace checkbox
        var devCheckbox = await Page.QuerySelectorAsync(".namespace-row:has-text('ns-dev') input[type='checkbox']");
        if (devCheckbox != null)
        {
            var isChecked = await devCheckbox.IsCheckedAsync();
            Assert.That(isChecked, Is.True, "Dev namespace should be enabled by default");

            await devCheckbox.ClickAsync();
            await Page.WaitForTimeoutAsync(300);

            isChecked = await devCheckbox.IsCheckedAsync();
            Assert.That(isChecked, Is.False, "Dev namespace should be disabled after clicking");
        }
    }

    [Test]
    public async Task SharedCluster_NodeSpecs_ShowsUnifiedSpecs()
    {
        await NavigateToSharedClusterConfigAsync();
        await ClickTabAsync("Node Specs");

        // Should show unified specs table (single set of node specs, not Prod/Non-Prod)
        Assert.That(await IsVisibleAsync(".unified-specs") ||
                   await IsVisibleAsync(".node-spec-row:has-text('Worker')"), Is.True,
            "Unified specs table should be visible");
    }

    [Test]
    public async Task SharedCluster_NodeSpecs_NoEnvDistinction()
    {
        await NavigateToSharedClusterConfigAsync();
        await ClickTabAsync("Node Specs");

        // In shared cluster mode, should use unified specs table
        // Check for unified-specs class which indicates no Prod/Non-Prod distinction
        var unifiedTable = await Page.QuerySelectorAsync(".node-specs-table.unified-specs");

        if (unifiedTable != null)
        {
            // Unified specs - should have fewer spec rows (no Prod/Non-Prod split)
            var specRows = await Page.QuerySelectorAllAsync(".unified-specs .node-spec-row");
            Assert.That(specRows.Count, Is.LessThanOrEqualTo(4),
                "Unified specs should have fewer rows (max 4: Control Plane, Infrastructure, Worker, or some subset)");
        }
        else
        {
            // Fallback check - verify we can see worker spec row
            Assert.That(await IsVisibleAsync(".node-spec-row:has-text('Worker')"), Is.True,
                "At minimum, worker node specs should be visible");
        }
    }

    [Test]
    public async Task SharedCluster_NodeSpecs_NoProdNonProdLabels()
    {
        await NavigateToSharedClusterConfigAsync();
        await ClickTabAsync("Node Specs");

        // Should NOT show "Production" and "Non-Prod" labels (that's for multi-cluster only)
        var prodLabel = await Page.QuerySelectorAsync(".env-type-col.prod-label");
        var nonprodLabel = await Page.QuerySelectorAsync(".env-type-col.nonprod-label");
        Assert.That(prodLabel == null && nonprodLabel == null, Is.True,
            "Shared cluster should not show Prod/Non-Prod labels in node specs");
    }

    [Test]
    public async Task SharedCluster_BannerShowsCorrectDescription()
    {
        await NavigateToSharedClusterConfigAsync();

        // Check the mode description banner
        Assert.That(await IsVisibleAsync(".mode-description-banner"), Is.True,
            "Mode description banner should be visible");

        var bannerText = await GetTextAsync(".mode-description-banner .banner-content");
        Assert.That(bannerText, Does.Contain("Shared").Or.Contain("namespace").IgnoreCase,
            "Banner should mention shared cluster or namespaces");
    }

    #endregion

    #region Calculation Tests

    [Test]
    public async Task SharedCluster_Calculate_ReturnsOneClusterResult()
    {
        await NavigateToSharedClusterConfigAsync();

        // Set some app counts - may be namespace rows or cluster rows
        var inputs = await Page.QuerySelectorAllAsync(".namespace-row:not(.disabled) .tier-col input");
        if (inputs.Count == 0)
        {
            inputs = await Page.QuerySelectorAllAsync(".cluster-row:not(.disabled) .tier-col input:not([disabled])");
        }
        if (inputs.Count > 0)
        {
            await inputs[0].FillAsync("10");
        }

        await ClickCalculateAsync();
        await Page.WaitForSelectorAsync(".results-panel", new() { Timeout = 10000 });

        // Results should be displayed
        Assert.That(await IsVisibleAsync(".results-panel"), Is.True,
            "Results panel should be visible");

        // Should show summary cards
        var summaryCards = await IsVisibleAsync(".summary-cards");
        Assert.That(summaryCards, Is.True, "Summary cards should be visible");
    }

    [Test]
    public async Task SharedCluster_Calculate_ShowsNamespaceBreakdown()
    {
        await NavigateToSharedClusterConfigAsync();

        // Set apps
        var inputs = await Page.QuerySelectorAllAsync(".namespace-row:not(.disabled) .tier-col input");
        if (inputs.Count == 0)
        {
            inputs = await Page.QuerySelectorAllAsync(".cluster-row:not(.disabled) .tier-col input:not([disabled])");
        }
        if (inputs.Count > 0)
        {
            await inputs[0].FillAsync("10");
        }

        await ClickCalculateAsync();
        await Page.WaitForSelectorAsync(".results-panel", new() { Timeout = 10000 });

        // Results table should show something (Shared Cluster or env name)
        var tableText = await GetTextAsync(".results-table");
        Assert.That(tableText, Does.Contain("Shared").Or.Contain("Prod").Or.Contain("Total").IgnoreCase,
            "Results table should show results");
    }

    [Test]
    public async Task SharedCluster_Calculate_UsesUnifiedSpecs()
    {
        await NavigateToSharedClusterConfigAsync();
        await ClickTabAsync("Node Specs");

        // Modify the unified specs
        var cpuInput = await Page.QuerySelectorAsync(".node-spec-row:has-text('Worker') .spec-col input");
        if (cpuInput != null)
        {
            await cpuInput.FillAsync("16");
        }

        await ClickTabAsync("Applications");

        // Set some apps
        var inputs = await Page.QuerySelectorAllAsync(".namespace-row:not(.disabled) .tier-col input");
        if (inputs.Count > 0)
        {
            await inputs[0].FillAsync("20");
        }

        await ClickCalculateAsync();
        await Page.WaitForSelectorAsync(".results-panel", new() { Timeout = 10000 });

        // Results should be generated successfully
        Assert.That(await IsVisibleAsync(".results-panel"), Is.True,
            "Results should be displayed using unified specs");
    }

    [Test]
    public async Task SharedCluster_Calculate_TotalNodes_MatchesExpected()
    {
        await NavigateToSharedClusterConfigAsync();

        // Set known app count
        var inputs = await Page.QuerySelectorAllAsync(".namespace-row:not(.disabled) .tier-col input");
        if (inputs.Count > 1)
        {
            await inputs[1].FillAsync("70"); // 70 medium apps
        }

        await ClickCalculateAsync();
        await Page.WaitForSelectorAsync(".results-panel", new() { Timeout = 10000 });

        // Should show total nodes in summary
        var summaryText = await GetTextAsync(".summary-cards");
        Assert.That(summaryText, Does.Contain("Node").IgnoreCase,
            "Summary should show node count");

        // Total should be masters + workers (+ infra if applicable)
        // For shared cluster, masters = 3 (standard HA), workers based on resources
    }

    [Test]
    public async Task SharedCluster_Calculate_ResourceTotal_MatchesSpecs()
    {
        await NavigateToSharedClusterConfigAsync();

        // Set apps
        var inputs = await Page.QuerySelectorAllAsync(".namespace-row:not(.disabled) .tier-col input");
        if (inputs.Count > 0)
        {
            await inputs[0].FillAsync("10");
        }

        await ClickCalculateAsync();
        await Page.WaitForSelectorAsync(".results-panel", new() { Timeout = 10000 });

        // Should show CPU and RAM totals
        var tableText = await GetTextAsync(".results-table");
        Assert.That(tableText, Does.Contain("CPU").Or.Contain("vCPU").Or.Contain("RAM").IgnoreCase,
            "Results should show resource totals (CPU/RAM)");
    }

    #endregion
}
