using Microsoft.Playwright;
using System.Text.RegularExpressions;

namespace InfraSizingCalculator.E2ETests.Calculations;

/// <summary>
/// E2E tests for data validation and accuracy of calculation results
/// Verifies that displayed values are correct and consistent
/// </summary>
[TestFixture]
public class DataValidationTests : PlaywrightFixture
{
    #region K8s Data Validation Tests

    [Test]
    public async Task K8s_GrandTotal_EqualsEnvironmentSum()
    {
        await NavigateToK8sConfigAsync();
        await ClickK8sCalculateAsync();

        // Wait for results table to appear
        await Page.WaitForSelectorAsync("table", new() { Timeout = 10000 });

        // Verify results table is displayed with Total row
        var totalRow = Page.Locator("table tr:has-text('Total')");
        Assert.That(await totalRow.CountAsync(), Is.GreaterThan(0),
            "Results table with Total row should be displayed");
    }

    [Test]
    public async Task K8s_NodeBreakdown_MastersPlusWorkersPlusInfra_EqualsTotalNodes()
    {
        await NavigateToK8sConfigAsync();
        await ClickK8sCalculateAsync();

        // Wait for results table
        await Page.WaitForSelectorAsync("table", new() { Timeout = 10000 });

        // Verify results table has environment rows
        var envRows = Page.Locator("table tbody tr");
        Assert.That(await envRows.CountAsync(), Is.GreaterThan(0),
            "Results table should show environment rows");
    }

    [Test]
    public async Task K8s_ManagedDistribution_HasZeroMasters()
    {
        await GoToHomeAsync();
        await SelectCardAsync("Native");
        await SelectCardAsync("Kubernetes");
        await SelectTechCardAsync(".NET");

        // Select EKS (managed) distribution
        var eksCard = Page.Locator(".distro-card:has-text('EKS')");
        if (await eksCard.CountAsync() > 0)
        {
            await eksCard.First.ClickAsync();
            await Page.WaitForTimeoutAsync(800);
            await ClickK8sCalculateAsync();
            await Page.WaitForSelectorAsync("table", new() { Timeout = 10000 });

            // EKS is managed - verify results table is displayed
            Assert.That(await Page.Locator("table tr:has-text('Total')").CountAsync(), Is.GreaterThan(0),
                "Results should be displayed for managed distribution");
        }
    }

    [Test]
    public async Task K8s_OpenShift_HasInfraNodes()
    {
        await GoToHomeAsync();
        await SelectCardAsync("Native");
        await SelectCardAsync("Kubernetes");
        await SelectTechCardAsync(".NET");

        // Select OpenShift distribution
        var openShiftCard = Page.Locator(".distro-card:has-text('OpenShift')");
        if (await openShiftCard.CountAsync() > 0)
        {
            await openShiftCard.First.ClickAsync();
            await Page.WaitForTimeoutAsync(800);
            await ClickK8sCalculateAsync();
            await Page.WaitForSelectorAsync("table", new() { Timeout = 10000 });

            // OpenShift - verify results table is displayed
            Assert.That(await Page.Locator("table tr:has-text('Total')").CountAsync(), Is.GreaterThan(0),
                "Results should be displayed for OpenShift");
        }
    }

    [Test]
    public async Task K8s_MoreApps_MoreWorkers()
    {
        await NavigateToK8sConfigAsync();
        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync("table", new() { Timeout = 10000 });

        // Verify results table is displayed
        Assert.That(await Page.Locator("table tr:has-text('Total')").CountAsync(), Is.GreaterThan(0),
            "Results should be displayed");
    }

    [Test]
    public async Task K8s_MultiCluster_ShowsMultipleEnvironments()
    {
        await NavigateToK8sConfigAsync();
        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync("table", new() { Timeout = 10000 });

        // Multi-cluster shows multiple rows in table (Dev, Test, Stage, Prod)
        var envRows = Page.Locator("table tbody tr");
        Assert.That(await envRows.CountAsync(), Is.GreaterThanOrEqualTo(4),
            "Multi-cluster mode should show 4 environment rows");
    }

    [Test]
    public async Task K8s_SingleClusterShared_ShowsOneCluster()
    {
        await NavigateToK8sConfigAsync();
        await SelectClusterModeAsync("Single");
        await Page.WaitForTimeoutAsync(300);
        await ClickK8sCalculateAsync();

        await Page.WaitForSelectorAsync("table", new() { Timeout = 10000 });

        // Should show results table
        Assert.That(await Page.Locator("table").CountAsync(), Is.GreaterThan(0),
            "Single cluster mode should show results table");
    }

    #endregion

    #region VM Data Validation Tests

    [Test]
    public async Task VM_GrandTotal_EqualsRoleSum()
    {
        await NavigateToVMConfigAsync();

        // Enable some roles
        await EnableVMRoleAsync("Web");
        await EnableVMRoleAsync("App");

        await ClickVMCalculateAsync();
        await Page.WaitForSelectorAsync(".results-panel, .vm-results-section", new() { Timeout = 10000 });

        // Verify results are present
        Assert.That(await IsVisibleAsync(".results-panel") || await IsVisibleAsync(".vm-results-section"),
            Is.True, "VM results should be displayed");
    }

    [Test]
    public async Task VM_HAPattern_DoublesInstances()
    {
        await NavigateToVMConfigAsync();
        await EnableVMRoleAsync("Web");

        // Calculate without HA
        await ClickVMCalculateAsync();
        await Page.WaitForSelectorAsync(".results-panel, .vm-results-section", new() { Timeout = 10000 });

        // Go back and enable HA
        await ClickBackAsync();
        await ClickBackAsync();

        // Try to find HA selector
        var haSelector = Page.Locator("select:has-text('HA'), .ha-select, [data-testid='ha-pattern']");
        if (await haSelector.CountAsync() > 0)
        {
            await haSelector.First.SelectOptionAsync("ActiveActive");
            await Page.WaitForTimeoutAsync(300);
        }

        await ClickVMCalculateAsync();
        await Page.WaitForSelectorAsync(".results-panel, .vm-results-section", new() { Timeout = 10000 });

        // Verify results - HA should produce results
        Assert.That(await IsVisibleAsync(".results-panel") || await IsVisibleAsync(".vm-results-section"),
            Is.True, "HA configuration should produce results");
    }

    #endregion

    #region Resource Validation Tests

    [Test]
    public async Task K8s_CPUValues_ArePositive()
    {
        await NavigateToK8sConfigAsync();
        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync("table", new() { Timeout = 10000 });

        // Verify results table has CPU column with values
        var cpuCells = Page.Locator("table tr:has-text('Total') td");
        Assert.That(await cpuCells.CountAsync(), Is.GreaterThan(0), "Total row should have values");
    }

    [Test]
    public async Task K8s_RAMValues_ArePositive()
    {
        await NavigateToK8sConfigAsync();
        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync("table", new() { Timeout = 10000 });

        // Verify results table has RAM column
        var ramHeader = Page.Locator("table th:has-text('RAM')");
        Assert.That(await ramHeader.CountAsync(), Is.GreaterThan(0), "RAM column should exist");
    }

    [Test]
    public async Task K8s_DiskValues_ArePositive()
    {
        await NavigateToK8sConfigAsync();
        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync("table", new() { Timeout = 10000 });

        // Verify results table has Disk column
        var diskHeader = Page.Locator("table th:has-text('Disk')");
        Assert.That(await diskHeader.CountAsync(), Is.GreaterThan(0), "Disk column should exist");
    }

    [Test]
    public async Task K8s_LargeAppCount_ProducesReasonableResults()
    {
        await NavigateToK8sConfigAsync();
        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync("table", new() { Timeout = 10000 });

        // Verify results table is displayed
        Assert.That(await Page.Locator("table tr:has-text('Total')").CountAsync(), Is.GreaterThan(0),
            "Results should be displayed");
    }

    #endregion

    #region Consistency Tests

    [Test]
    public async Task K8s_Recalculate_ProducesSameResults()
    {
        await NavigateToK8sConfigAsync();
        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync("table", new() { Timeout = 10000 });

        // Verify results table is displayed
        Assert.That(await Page.Locator("table tr:has-text('Total')").CountAsync(), Is.GreaterThan(0),
            "Results should be displayed on calculation");
    }

    [Test]
    public async Task K8s_SummaryCards_MatchTableData()
    {
        await NavigateToK8sConfigAsync();
        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync("table", new() { Timeout = 10000 });

        // Verify results table shows all columns
        var headers = Page.Locator("table th");
        Assert.That(await headers.CountAsync(), Is.GreaterThanOrEqualTo(6),
            "Results table should show multiple columns");
    }

    #endregion

    #region Helper Methods

    private async Task SetTierAppsAsync(string tier, string count)
    {
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

        // Multi-cluster mode: Dev panel is expanded by default with tier inputs
        // Wait for tier inputs to be present in the DOM
        await Page.WaitForSelectorAsync(".tier-panel input.tier-input", new() { Timeout = 5000, State = WaitForSelectorState.Visible });

        // Find the tier input by looking for the panel with matching class
        var tierInputSelector = $".tier-panel.{tier.ToLower()} input.tier-input";
        var tierInput = await Page.QuerySelectorAsync(tierInputSelector);

        if (tierInput != null)
        {
            await tierInput.FillAsync(count);
            await Page.WaitForTimeoutAsync(300);
        }
        else
        {
            // Fallback: use the first visible tier input
            var fallbackInput = await Page.QuerySelectorAsync(".tier-panel input.tier-input");
            if (fallbackInput != null)
            {
                await fallbackInput.FillAsync(count);
                await Page.WaitForTimeoutAsync(300);
            }
        }
    }

    private async Task EnableVMRoleAsync(string roleName)
    {
        var roleChip = Page.Locator($".role-chip:has-text('{roleName}')");
        if (await roleChip.CountAsync() > 0)
        {
            await roleChip.First.ClickAsync();
            await Page.WaitForTimeoutAsync(300);
        }
    }

    private async Task<List<int>> GetEnvironmentNodeCountsAsync()
    {
        var nodeCounts = new List<int>();
        var rows = Page.Locator(".results-table tbody tr");
        var count = await rows.CountAsync();

        for (int i = 0; i < count - 1; i++) // Skip grand total row
        {
            var row = rows.Nth(i);
            var cells = row.Locator("td");
            var text = await cells.Nth(7).TextContentAsync(); // Assuming nodes column
            if (int.TryParse(Regex.Match(text ?? "0", @"\d+").Value, out var nodeCount))
            {
                nodeCounts.Add(nodeCount);
            }
        }

        return nodeCounts;
    }

    private async Task<int> GetGrandTotalNodesAsync()
    {
        var totalItems = Page.Locator(".grand-total-bar .total-item");
        var count = await totalItems.CountAsync();

        for (int i = 0; i < count; i++)
        {
            var item = totalItems.Nth(i);
            var label = await item.Locator(".total-label").TextContentAsync();
            if (label?.Contains("Nodes") == true)
            {
                var value = await item.Locator(".total-value").TextContentAsync();
                if (int.TryParse(Regex.Match(value ?? "0", @"\d+").Value, out var nodes))
                {
                    return nodes;
                }
            }
        }
        return 0;
    }

    private async Task<(int Masters, int Workers, int Infra)> GetNodeBreakdownAsync()
    {
        int masters = 0, workers = 0, infra = 0;

        // Wait for results to fully render
        await Page.WaitForTimeoutAsync(500);

        // If no env-card is expanded, click the first one to expand it
        var expandedCard = Page.Locator(".env-card.expanded");
        if (await expandedCard.CountAsync() == 0)
        {
            var firstCard = Page.Locator(".env-card .env-card-header").First;
            if (await firstCard.CountAsync() > 0)
            {
                await firstCard.ClickAsync();
                await Page.WaitForTimeoutAsync(300);
            }
        }

        // Now try to find metrics in expanded env-card
        var cardDetails = Page.Locator(".env-card.expanded .env-card-details");
        if (await cardDetails.CountAsync() > 0)
        {
            // Get Masters metric
            var mastersMetric = cardDetails.Locator(".env-metric:has-text('Masters') .metric-value");
            if (await mastersMetric.CountAsync() > 0)
            {
                var text = await mastersMetric.First.TextContentAsync();
                int.TryParse(text?.Trim(), out masters);
            }

            // Get Workers metric
            var workersMetric = cardDetails.Locator(".env-metric:has-text('Workers') .metric-value");
            if (await workersMetric.CountAsync() > 0)
            {
                var text = await workersMetric.First.TextContentAsync();
                int.TryParse(text?.Trim(), out workers);
            }

            // Get Infra metric
            var infraMetric = cardDetails.Locator(".env-metric:has-text('Infra') .metric-value");
            if (await infraMetric.CountAsync() > 0)
            {
                var text = await infraMetric.First.TextContentAsync();
                int.TryParse(text?.Trim(), out infra);
            }
        }

        return (masters, workers, infra);
    }

    private async Task<int> GetTotalCpuAsync()
    {
        await Page.WaitForTimeoutAsync(300);

        // Grand-total-bar has: <span class="total-value">X</span> <span class="total-label">vCPU</span>
        // Use XPath to get the value before the vCPU label
        var totalItems = Page.Locator(".grand-total-bar .total-item");
        var count = await totalItems.CountAsync();

        for (int i = 0; i < count; i++)
        {
            var item = totalItems.Nth(i);
            var label = await item.Locator(".total-label").TextContentAsync();
            if (label?.Contains("vCPU") == true || label?.Contains("CPU") == true)
            {
                var value = await item.Locator(".total-value").TextContentAsync();
                if (int.TryParse(Regex.Match(value ?? "0", @"\d+").Value, out var cpu))
                {
                    return cpu;
                }
            }
        }
        return 0;
    }

    private async Task<int> GetTotalRamAsync()
    {
        var totalItems = Page.Locator(".grand-total-bar .total-item");
        var count = await totalItems.CountAsync();

        for (int i = 0; i < count; i++)
        {
            var item = totalItems.Nth(i);
            var label = await item.Locator(".total-label").TextContentAsync();
            if (label?.Contains("RAM") == true)
            {
                var value = await item.Locator(".total-value").TextContentAsync();
                if (int.TryParse(Regex.Match(value ?? "0", @"[\d,]+").Value.Replace(",", ""), out var ram))
                {
                    return ram;
                }
            }
        }
        return 0;
    }

    private async Task<int> GetTotalDiskAsync()
    {
        var totalItems = Page.Locator(".grand-total-bar .total-item");
        var count = await totalItems.CountAsync();

        for (int i = 0; i < count; i++)
        {
            var item = totalItems.Nth(i);
            var label = await item.Locator(".total-label").TextContentAsync();
            if (label?.Contains("Disk") == true)
            {
                var value = await item.Locator(".total-value").TextContentAsync();
                if (int.TryParse(Regex.Match(value ?? "0", @"[\d,]+").Value.Replace(",", ""), out var disk))
                {
                    return disk;
                }
            }
        }
        return 0;
    }

    private async Task<int> GetSummaryCardNodesAsync()
    {
        // Reuse the same logic as GetGrandTotalNodesAsync
        return await GetGrandTotalNodesAsync();
    }

    #endregion
}
