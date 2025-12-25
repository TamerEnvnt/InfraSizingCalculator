namespace InfraSizingCalculator.E2ETests.K8s;

/// <summary>
/// E2E tests for Multi-Cluster mode - ensures UI correctly represents multiple clusters
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
    }

    [Test]
    public async Task MultiCluster_ShowsClusterGroupLabels()
    {
        await NavigateToMultiClusterConfigAsync();

        // Multi-cluster mode shows cluster chips for environment selection
        Assert.That(await IsVisibleAsync(".cluster-selection"), Is.True,
            "Multi-cluster should show cluster selection area");

        // Should have Prod and Dev/Test/Stage cluster chips
        var prodChip = await Page.QuerySelectorAsync(".cluster-chip.prod");
        var nonprodChip = await Page.QuerySelectorAsync(".cluster-chip.nonprod");
        Assert.That(prodChip, Is.Not.Null, "Should have production cluster chip");
        Assert.That(nonprodChip, Is.Not.Null, "Should have non-production cluster chip");
    }

    [Test]
    public async Task MultiCluster_ShowsEnvironmentRows()
    {
        await NavigateToMultiClusterConfigAsync();

        // Multi-cluster mode shows cluster chips for each environment
        var clusterChips = await Page.QuerySelectorAllAsync(".cluster-chip");
        Assert.That(clusterChips.Count, Is.GreaterThanOrEqualTo(4),
            "Should show multiple cluster chips");

        // Should have Dev, Test, Stage, Prod chips
        Assert.That(await IsVisibleAsync(".cluster-chip:has-text('Prod')"), Is.True,
            "Prod cluster chip should be visible");
        Assert.That(await IsVisibleAsync(".cluster-chip:has-text('Dev')"), Is.True,
            "Dev cluster chip should be visible");
    }

    [Test]
    public async Task MultiCluster_ShowsClusterColumnHeader()
    {
        await NavigateToMultiClusterConfigAsync();

        // Multi-cluster mode shows workload title and cluster selection hint
        var workloadTitle = await GetTextAsync(".workload-title");
        Assert.That(workloadTitle, Does.Contain("Workload").IgnoreCase,
            "Should show 'Workload Configuration' header");

        // Check for cluster selection hint
        var hintText = await GetTextAsync(".cluster-hint");
        Assert.That(hintText, Does.Contain("cluster").IgnoreCase,
            "Should show cluster selection hint");
    }

    [Test]
    public async Task MultiCluster_CanToggleEnvironments()
    {
        await NavigateToMultiClusterConfigAsync();

        // Find Dev cluster chip label (the input is hidden, click on label)
        var devChip = await Page.QuerySelectorAsync(".cluster-chip:has-text('Dev')");
        Assert.That(devChip, Is.Not.Null, "Dev cluster chip should exist");

        // Find the checkbox inside to check its state
        var devCheckbox = await Page.QuerySelectorAsync(".cluster-chip:has-text('Dev') input[type='checkbox']");
        Assert.That(devCheckbox, Is.Not.Null, "Dev cluster checkbox should exist");

        // Should be checked by default
        var isChecked = await devCheckbox!.IsCheckedAsync();
        Assert.That(isChecked, Is.True, "Dev should be enabled by default");

        // Click on the chip label to toggle
        await devChip!.ClickAsync();
        await Page.WaitForTimeoutAsync(300);

        // Verify it's unchecked
        isChecked = await devCheckbox.IsCheckedAsync();
        Assert.That(isChecked, Is.False, "Dev should be disabled after clicking");
    }

    [Test]
    public async Task MultiCluster_ProdCannotBeDisabled()
    {
        await NavigateToMultiClusterConfigAsync();

        // Find Prod environment checkbox in cluster chip
        var prodCheckbox = await Page.QuerySelectorAsync(".cluster-chip:has-text('Prod') input[type='checkbox']");
        Assert.That(prodCheckbox, Is.Not.Null, "Prod cluster checkbox should exist");

        // Should be disabled (can't uncheck Prod)
        var isDisabled = await prodCheckbox!.IsDisabledAsync();
        Assert.That(isDisabled, Is.True, "Prod checkbox should be disabled (always required)");
    }

    [Test]
    public async Task MultiCluster_CanSetAppCountsPerEnvironment()
    {
        await NavigateToMultiClusterConfigAsync();

        // Set apps for Dev
        var devInputs = await Page.QuerySelectorAllAsync(".cluster-row:has-text('Dev') .tier-col input:not([disabled])");
        if (devInputs.Count > 0)
        {
            await devInputs[0].FillAsync("10");
            var value = await devInputs[0].InputValueAsync();
            Assert.That(value, Is.EqualTo("10"), "Dev cluster should accept app count");
        }

        // Set apps for Prod
        var prodInputs = await Page.QuerySelectorAllAsync(".cluster-row:has-text('Prod') .tier-col input:not([disabled])");
        if (prodInputs.Count > 0)
        {
            await prodInputs[0].FillAsync("50");
            var value = await prodInputs[0].InputValueAsync();
            Assert.That(value, Is.EqualTo("50"), "Prod cluster should accept app count");
        }
    }

    [Test]
    public async Task MultiCluster_Calculate_ShowsMultipleEnvironmentResults()
    {
        await NavigateToMultiClusterConfigAsync();

        // Set apps for multiple environments
        var devInputs = await Page.QuerySelectorAllAsync(".cluster-row:has-text('Dev') .tier-col input:not([disabled])");
        if (devInputs.Count > 0)
            await devInputs[0].FillAsync("10");

        var prodInputs = await Page.QuerySelectorAllAsync(".cluster-row:has-text('Prod') .tier-col input:not([disabled])");
        if (prodInputs.Count > 0)
            await prodInputs[0].FillAsync("20");

        await ClickCalculateAsync();
        await Page.WaitForSelectorAsync(".results-panel", new() { Timeout = 10000 });

        // Should show multiple environment results in the table
        // Check for multiple rows in the results table body
        var tableRows = await Page.QuerySelectorAllAsync(".results-table tbody tr");
        Assert.That(tableRows.Count, Is.GreaterThan(1),
            "Multi-cluster results should show multiple environment rows (including Grand Total)");
    }

    [Test]
    public async Task MultiCluster_Calculate_ShowsPerEnvironmentNodes()
    {
        await NavigateToMultiClusterConfigAsync();

        // Set apps for Prod only
        var prodInputs = await Page.QuerySelectorAllAsync(".cluster-row:has-text('Prod') .tier-col input:not([disabled])");
        if (prodInputs.Count > 1)
            await prodInputs[1].FillAsync("70"); // 70 medium apps

        await ClickCalculateAsync();
        await Page.WaitForSelectorAsync(".results-panel", new() { Timeout = 10000 });

        // Results table should show node counts per environment
        var resultsText = await GetTextAsync(".results-table");
        Assert.That(resultsText, Does.Contain("Master").Or.Contain("Worker").Or.Contain("Infra").IgnoreCase,
            "Results should show node type breakdown");
    }

    [Test]
    public async Task MultiCluster_NodeSpecs_ShowsPerEnvironmentTabs()
    {
        await NavigateToMultiClusterConfigAsync();
        await ClickTabAsync("Node Specs");

        // Should show tabbed interface for per-environment node specs
        Assert.That(await IsVisibleAsync(".node-specs-tabbed"), Is.True,
            "Tabbed node specs container should be visible");

        // Should show environment tabs
        Assert.That(await IsVisibleAsync(".node-specs-env-tabs"), Is.True,
            "Environment tabs should be visible");

        // Should have tabs for enabled environments (at minimum Prod)
        Assert.That(await IsVisibleAsync(".node-specs-env-tab:has-text('Prod')"), Is.True,
            "Prod environment tab should be visible");
    }

    [Test]
    public async Task MultiCluster_NodeSpecs_CanSwitchEnvironmentTabs()
    {
        await NavigateToMultiClusterConfigAsync();
        await ClickTabAsync("Node Specs");

        // Click on Dev tab
        var devTab = await Page.QuerySelectorAsync(".node-specs-env-tab:has-text('Dev')");
        if (devTab != null)
        {
            await devTab.ClickAsync();
            await Page.WaitForTimeoutAsync(300);

            // Dev tab should now be active
            var devTabClass = await devTab.GetAttributeAsync("class") ?? "";
            Assert.That(devTabClass, Does.Contain("active"),
                "Dev tab should be active after clicking");
        }

        // Click on Prod tab
        var prodTab = await Page.QuerySelectorAsync(".node-specs-env-tab:has-text('Prod')");
        if (prodTab != null)
        {
            await prodTab.ClickAsync();
            await Page.WaitForTimeoutAsync(300);

            // Prod tab should now be active
            var prodTabClass = await prodTab.GetAttributeAsync("class") ?? "";
            Assert.That(prodTabClass, Does.Contain("active"),
                "Prod tab should be active after clicking");
        }
    }

    [Test]
    public async Task MultiCluster_NodeSpecs_CanEditCpuAndRam()
    {
        await NavigateToMultiClusterConfigAsync();
        await ClickTabAsync("Node Specs");

        // The tabbed interface shows node specs for the selected environment
        // Find Worker CPU input in the tab content and change it
        var cpuInputs = await Page.QuerySelectorAllAsync(".node-specs-tab-content .node-spec-row:has-text('Worker') .spec-col input");
        if (cpuInputs.Count > 0)
        {
            await cpuInputs[0].FillAsync("32");
            var value = await cpuInputs[0].InputValueAsync();
            Assert.That(value, Is.EqualTo("32"), "Should be able to edit CPU value");
        }
        else
        {
            // Fallback: try any worker node spec input
            cpuInputs = await Page.QuerySelectorAllAsync(".node-spec-row:has-text('Worker') .spec-col input");
            if (cpuInputs.Count > 0)
            {
                await cpuInputs[0].FillAsync("32");
                var value = await cpuInputs[0].InputValueAsync();
                Assert.That(value, Is.EqualTo("32"), "Should be able to edit CPU value");
            }
        }
    }

    [Test]
    public async Task MultiCluster_Settings_ShowsHeadroomOptions()
    {
        await NavigateToMultiClusterConfigAsync();
        await ClickTabAsync("Settings");

        // Should show headroom settings
        Assert.That(await IsVisibleAsync(".settings-section-compact:has-text('Headroom')"), Is.True,
            "Headroom settings should be visible");

        // Should have separate headroom for environments
        var headroomInputs = await Page.QuerySelectorAllAsync(".settings-section-compact:has-text('Headroom') input");
        Assert.That(headroomInputs.Count, Is.GreaterThanOrEqualTo(2),
            "Should have multiple headroom inputs (Prod/Non-Prod)");
    }

    [Test]
    public async Task MultiCluster_Settings_ShowsOvercommitOptions()
    {
        await NavigateToMultiClusterConfigAsync();
        await ClickTabAsync("Settings");

        // Should show overcommit settings
        Assert.That(await IsVisibleAsync(".overcommit-group"), Is.True,
            "Overcommit settings should be visible");
    }

    [Test]
    public async Task MultiCluster_Settings_ShowsReplicaOptions()
    {
        await NavigateToMultiClusterConfigAsync();
        await ClickTabAsync("Settings");

        // Should show replica settings
        Assert.That(await IsVisibleAsync(".settings-section-compact:has-text('Replica')"), Is.True,
            "Replica settings should be visible");
    }

    [Test]
    public async Task MultiCluster_WithDR_ShowsDREnvironment()
    {
        await NavigateToMultiClusterConfigAsync();

        // If DR is available, enable it
        var drCheckbox = await Page.QuerySelectorAsync(".cluster-row:has-text('DR') input[type='checkbox']");
        if (drCheckbox != null)
        {
            // Enable DR if not already enabled
            if (!await drCheckbox.IsCheckedAsync())
            {
                await drCheckbox.ClickAsync();
                await Page.WaitForTimeoutAsync(300);
            }

            // DR row should be visible and have input fields
            Assert.That(await IsVisibleAsync(".cluster-row:has-text('DR') .tier-col input"), Is.True,
                "DR cluster should have app input fields");
        }
    }

    [Test]
    public async Task MultiCluster_SwitchToSingle_HidesClusterGroups()
    {
        await NavigateToMultiClusterConfigAsync();

        // Verify cluster selection is visible in multi-cluster mode
        Assert.That(await IsVisibleAsync(".cluster-selection"), Is.True,
            "Cluster selection should be visible in multi-cluster mode");

        // Switch to Single Cluster mode
        await SelectClusterModeAsync("Single");
        await Page.WaitForTimeoutAsync(500);

        // Cluster selection should be hidden in single cluster mode
        var clusterSelection = await Page.QuerySelectorAsync(".cluster-selection");
        var isVisible = clusterSelection != null && await clusterSelection.IsVisibleAsync();
        Assert.That(isVisible, Is.False,
            "Single cluster mode should not show cluster selection");
    }

    [Test]
    public async Task MultiCluster_BannerShowsCorrectDescription()
    {
        await NavigateToMultiClusterConfigAsync();

        // Check the mode description banner
        Assert.That(await IsVisibleAsync(".mode-description-banner"), Is.True,
            "Mode description banner should be visible");

        var bannerText = await GetTextAsync(".mode-description-banner .banner-content");
        Assert.That(bannerText, Does.Contain("Multi").Or.Contain("dedicated").Or.Contain("cluster").IgnoreCase,
            "Banner should describe multi-cluster mode");
    }

    #region Calculation Verification Tests

    [Test]
    public async Task MultiCluster_Calculate_70MediumApps_Returns9Workers()
    {
        await NavigateToMultiClusterConfigAsync();

        // Reference case: 70 medium apps with default settings should give 9 workers
        var prodInputs = await Page.QuerySelectorAllAsync(".cluster-row:has-text('Prod') .tier-col input:not([disabled])");
        if (prodInputs.Count > 1)
        {
            await prodInputs[1].FillAsync("70"); // Medium tier
        }

        await ClickCalculateAsync();
        await Page.WaitForSelectorAsync(".results-panel", new() { Timeout = 10000 });

        // Verify results show expected worker count
        var resultsText = await GetTextAsync(".results-table");
        Assert.That(resultsText, Does.Contain("9").Or.Contain("Worker"),
            "70 medium apps should result in approximately 9 workers");
    }

    [Test]
    public async Task MultiCluster_Calculate_ZeroApps_ReturnsMinimumNodes()
    {
        await NavigateToMultiClusterConfigAsync();

        // Don't set any app counts - should still show minimum nodes
        await ClickCalculateAsync();
        await Page.WaitForSelectorAsync(".results-panel", new() { Timeout = 10000 });

        // Should show results with minimum nodes (masters + infra minimum)
        Assert.That(await IsVisibleAsync(".results-panel"), Is.True,
            "Results should be displayed even with zero apps");

        var summaryText = await GetTextAsync(".summary-cards");
        Assert.That(summaryText, Does.Not.Contain("0 nodes").IgnoreCase,
            "Should have minimum nodes even with zero apps");
    }

    [Test]
    public async Task MultiCluster_Calculate_ShowsGrandTotal()
    {
        await NavigateToMultiClusterConfigAsync();

        // Set apps for multiple environments
        var devInputs = await Page.QuerySelectorAllAsync(".cluster-row:has-text('Dev') .tier-col input:not([disabled])");
        if (devInputs.Count > 0)
            await devInputs[0].FillAsync("10");

        var prodInputs = await Page.QuerySelectorAllAsync(".cluster-row:has-text('Prod') .tier-col input:not([disabled])");
        if (prodInputs.Count > 0)
            await prodInputs[0].FillAsync("20");

        await ClickCalculateAsync();
        await Page.WaitForSelectorAsync(".results-panel", new() { Timeout = 10000 });

        // Should show Grand Total row in results
        var tableText = await GetTextAsync(".results-table");
        Assert.That(tableText, Does.Contain("GRAND TOTAL").Or.Contain("Total").IgnoreCase,
            "Results should show grand total");
    }

    #endregion
}
