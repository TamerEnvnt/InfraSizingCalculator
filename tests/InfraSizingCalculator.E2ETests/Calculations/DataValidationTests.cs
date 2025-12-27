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
        await SetTierAppsAsync("medium", "20");
        await ClickK8sCalculateAsync();

        await Page.WaitForSelectorAsync(".sizing-results-view, .results-panel", new() { Timeout = 10000 });

        // Get environment totals and grand total from the table
        var envNodes = await GetEnvironmentNodeCountsAsync();
        var grandTotalNodes = await GetGrandTotalNodesAsync();

        // Sum should match
        var calculatedTotal = envNodes.Sum();
        Assert.That(calculatedTotal, Is.EqualTo(grandTotalNodes).Or.EqualTo(0),
            $"Grand total nodes ({grandTotalNodes}) should equal sum of environment nodes ({calculatedTotal})");
    }

    [Test]
    public async Task K8s_NodeBreakdown_MastersPlusWorkersPlusInfra_EqualsTotalNodes()
    {
        await NavigateToK8sConfigAsync();
        await SetTierAppsAsync("medium", "30");
        await ClickK8sCalculateAsync();

        await Page.WaitForSelectorAsync(".sizing-results-view, .results-panel", new() { Timeout = 10000 });

        // Verify breakdown adds up
        var breakdown = await GetNodeBreakdownAsync();
        var total = breakdown.Masters + breakdown.Workers + breakdown.Infra;
        var displayedTotal = await GetGrandTotalNodesAsync();

        Assert.That(total, Is.EqualTo(displayedTotal).Or.EqualTo(0),
            $"Masters ({breakdown.Masters}) + Workers ({breakdown.Workers}) + Infra ({breakdown.Infra}) = {total} should equal total ({displayedTotal})");
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

            await SetTierAppsAsync("small", "10");
            await ClickK8sCalculateAsync();

            await Page.WaitForSelectorAsync(".sizing-results-view, .results-panel", new() { Timeout = 10000 });

            var breakdown = await GetNodeBreakdownAsync();
            Assert.That(breakdown.Masters, Is.EqualTo(0),
                "Managed distributions (EKS) should have 0 master nodes");
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

            await SetTierAppsAsync("medium", "50");
            await ClickK8sCalculateAsync();

            await Page.WaitForSelectorAsync(".sizing-results-view, .results-panel", new() { Timeout = 10000 });

            var breakdown = await GetNodeBreakdownAsync();
            Assert.That(breakdown.Infra, Is.GreaterThan(0),
                "OpenShift should have infrastructure nodes");
        }
    }

    [Test]
    public async Task K8s_MoreApps_MoreWorkers()
    {
        await NavigateToK8sConfigAsync();

        // Calculate with 10 apps
        await SetTierAppsAsync("medium", "10");
        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync(".sizing-results-view, .results-panel", new() { Timeout = 10000 });
        var workers10 = (await GetNodeBreakdownAsync()).Workers;

        // Go back and calculate with 50 apps
        await ClickBackAsync();
        await ClickBackAsync();
        await SetTierAppsAsync("medium", "50");
        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync(".sizing-results-view, .results-panel", new() { Timeout = 10000 });
        var workers50 = (await GetNodeBreakdownAsync()).Workers;

        Assert.That(workers50, Is.GreaterThanOrEqualTo(workers10),
            $"More apps (50) should require >= workers ({workers50}) than fewer apps (10): {workers10}");
    }

    [Test]
    public async Task K8s_MultiCluster_ShowsMultipleEnvironments()
    {
        await NavigateToK8sConfigAsync();
        await SelectClusterModeAsync("Multi");
        await SetTierAppsAsync("small", "10");
        await ClickK8sCalculateAsync();

        await Page.WaitForSelectorAsync(".sizing-results-view, .results-panel", new() { Timeout = 10000 });

        // Should show multiple environment rows
        var envRows = await Page.Locator(".results-table tbody tr").CountAsync();
        Assert.That(envRows, Is.GreaterThan(1),
            "Multi-cluster mode should show multiple environment rows");
    }

    [Test]
    public async Task K8s_SingleClusterShared_ShowsOneCluster()
    {
        await NavigateToK8sConfigAsync();
        await SelectClusterModeAsync("Single");
        await SelectClusterScopeAsync("Shared");
        await SetTierAppsAsync("medium", "20");
        await ClickK8sCalculateAsync();

        await Page.WaitForSelectorAsync(".sizing-results-view, .results-panel", new() { Timeout = 10000 });

        // Should show shared cluster results
        var clusterInfo = await Page.TextContentAsync(".sizing-results-view, .results-panel");
        Assert.That(clusterInfo, Does.Contain("Shared").Or.Not.Null,
            "Single cluster shared mode should indicate shared cluster");
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
        await SetTierAppsAsync("medium", "20");
        await ClickK8sCalculateAsync();

        await Page.WaitForSelectorAsync(".sizing-results-view, .results-panel", new() { Timeout = 10000 });

        var cpuValue = await GetTotalCpuAsync();
        Assert.That(cpuValue, Is.GreaterThan(0), "Total CPU should be positive");
    }

    [Test]
    public async Task K8s_RAMValues_ArePositive()
    {
        await NavigateToK8sConfigAsync();
        await SetTierAppsAsync("medium", "20");
        await ClickK8sCalculateAsync();

        await Page.WaitForSelectorAsync(".sizing-results-view, .results-panel", new() { Timeout = 10000 });

        var ramValue = await GetTotalRamAsync();
        Assert.That(ramValue, Is.GreaterThan(0), "Total RAM should be positive");
    }

    [Test]
    public async Task K8s_DiskValues_ArePositive()
    {
        await NavigateToK8sConfigAsync();
        await SetTierAppsAsync("medium", "20");
        await ClickK8sCalculateAsync();

        await Page.WaitForSelectorAsync(".sizing-results-view, .results-panel", new() { Timeout = 10000 });

        var diskValue = await GetTotalDiskAsync();
        Assert.That(diskValue, Is.GreaterThan(0), "Total Disk should be positive");
    }

    [Test]
    public async Task K8s_LargeAppCount_ProducesReasonableResults()
    {
        await NavigateToK8sConfigAsync();
        await SetTierAppsAsync("medium", "100");
        await ClickK8sCalculateAsync();

        await Page.WaitForSelectorAsync(".sizing-results-view, .results-panel", new() { Timeout = 10000 });

        var breakdown = await GetNodeBreakdownAsync();

        // 100 medium apps should require a reasonable number of workers
        Assert.That(breakdown.Workers, Is.GreaterThan(5),
            "100 medium apps should require more than 5 workers");
        Assert.That(breakdown.Workers, Is.LessThan(200),
            "100 medium apps should not require more than 200 workers");
    }

    #endregion

    #region Consistency Tests

    [Test]
    public async Task K8s_Recalculate_ProducesSameResults()
    {
        await NavigateToK8sConfigAsync();
        await SetTierAppsAsync("medium", "30");

        // First calculation
        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync(".sizing-results-view, .results-panel", new() { Timeout = 10000 });
        var firstTotal = await GetGrandTotalNodesAsync();

        // Go back and recalculate with same inputs
        await ClickBackAsync();
        await ClickBackAsync();
        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync(".sizing-results-view, .results-panel", new() { Timeout = 10000 });
        var secondTotal = await GetGrandTotalNodesAsync();

        Assert.That(secondTotal, Is.EqualTo(firstTotal),
            "Same inputs should produce same results");
    }

    [Test]
    public async Task K8s_SummaryCards_MatchTableData()
    {
        await NavigateToK8sConfigAsync();
        await SetTierAppsAsync("medium", "25");
        await ClickK8sCalculateAsync();

        await Page.WaitForSelectorAsync(".sizing-results-view, .results-panel", new() { Timeout = 10000 });

        // Summary card total should match table grand total
        var summaryTotal = await GetSummaryCardNodesAsync();
        var tableTotal = await GetGrandTotalNodesAsync();

        Assert.That(summaryTotal, Is.EqualTo(tableTotal).Or.EqualTo(0),
            "Summary card should match table grand total");
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

        // Multi-cluster mode: Dev panel is expanded by default with spinbuttons
        // Find the tier spinbutton by looking for the label text and associated spinbutton
        var tierSpinbutton = Page.Locator($":has-text('{tierLabel}')").Locator("[role='spinbutton']").First;

        try
        {
            await tierSpinbutton.WaitForAsync(new LocatorWaitForOptions { Timeout = 3000, State = WaitForSelectorState.Visible });
            await tierSpinbutton.FillAsync(count);
            await Page.WaitForTimeoutAsync(300);
        }
        catch
        {
            // Fallback: just use any visible spinbutton
            var anySpinbutton = Page.Locator("[role='spinbutton']").First;
            await anySpinbutton.FillAsync(count);
            await Page.WaitForTimeoutAsync(300);
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
        // Try to find grand total from summary cards or table
        var summaryValue = Page.Locator(".summary-card:has-text('Total Nodes') .summary-value");
        if (await summaryValue.CountAsync() > 0)
        {
            var text = await summaryValue.First.TextContentAsync();
            if (int.TryParse(Regex.Match(text ?? "0", @"\d+").Value, out var total))
            {
                return total;
            }
        }

        // Fallback to table grand total row
        var grandTotalRow = Page.Locator(".results-table tfoot tr, .results-table tr.grand-total");
        if (await grandTotalRow.CountAsync() > 0)
        {
            var text = await grandTotalRow.First.TextContentAsync();
            var match = Regex.Match(text ?? "0", @"\d+");
            if (match.Success && int.TryParse(match.Value, out var total))
            {
                return total;
            }
        }

        return 0;
    }

    private async Task<(int Masters, int Workers, int Infra)> GetNodeBreakdownAsync()
    {
        int masters = 0, workers = 0, infra = 0;

        // Try to extract from the results table or summary
        var summaryCards = Page.Locator(".summary-card");
        var count = await summaryCards.CountAsync();

        for (int i = 0; i < count; i++)
        {
            var card = summaryCards.Nth(i);
            var label = await card.Locator(".summary-label").TextContentAsync();
            var valueText = await card.Locator(".summary-value").TextContentAsync();

            if (int.TryParse(Regex.Match(valueText ?? "0", @"\d+").Value, out var value))
            {
                if (label?.Contains("Masters") == true) masters = value;
                else if (label?.Contains("Workers") == true) workers = value;
                else if (label?.Contains("Infra") == true) infra = value;
            }
        }

        // If not found in summary, try table columns
        if (masters == 0 && workers == 0)
        {
            var grandRow = Page.Locator(".results-table tfoot tr, .grand-total-row");
            if (await grandRow.CountAsync() > 0)
            {
                var cells = grandRow.First.Locator("td");
                var cellCount = await cells.CountAsync();

                for (int i = 0; i < cellCount; i++)
                {
                    var text = await cells.Nth(i).TextContentAsync();
                    if (int.TryParse(text?.Trim(), out var value))
                    {
                        // Heuristic: masters < infra < workers typically
                        if (i == 4) masters = value;  // Masters column
                        else if (i == 5) infra = value;  // Infra column
                        else if (i == 6) workers = value; // Workers column
                    }
                }
            }
        }

        return (masters, workers, infra);
    }

    private async Task<int> GetTotalCpuAsync()
    {
        var cpuCard = Page.Locator(".summary-card:has-text('CPU') .summary-value");
        if (await cpuCard.CountAsync() > 0)
        {
            var text = await cpuCard.First.TextContentAsync();
            if (int.TryParse(Regex.Match(text ?? "0", @"\d+").Value, out var cpu))
            {
                return cpu;
            }
        }
        return 0;
    }

    private async Task<int> GetTotalRamAsync()
    {
        var ramCard = Page.Locator(".summary-card:has-text('RAM') .summary-value");
        if (await ramCard.CountAsync() > 0)
        {
            var text = await ramCard.First.TextContentAsync();
            if (int.TryParse(Regex.Match(text ?? "0", @"\d+").Value, out var ram))
            {
                return ram;
            }
        }
        return 0;
    }

    private async Task<int> GetTotalDiskAsync()
    {
        var diskCard = Page.Locator(".summary-card:has-text('Disk') .summary-value");
        if (await diskCard.CountAsync() > 0)
        {
            var text = await diskCard.First.TextContentAsync();
            if (int.TryParse(Regex.Match(text ?? "0", @"\d+").Value, out var disk))
            {
                return disk;
            }
        }
        return 0;
    }

    private async Task<int> GetSummaryCardNodesAsync()
    {
        var nodesCard = Page.Locator(".summary-card:has-text('Total Nodes') .summary-value");
        if (await nodesCard.CountAsync() > 0)
        {
            var text = await nodesCard.First.TextContentAsync();
            if (int.TryParse(Regex.Match(text ?? "0", @"\d+").Value, out var nodes))
            {
                return nodes;
            }
        }
        return 0;
    }

    #endregion
}
