namespace InfraSizingCalculator.E2ETests.K8s;

/// <summary>
/// E2E tests for Per-Environment (Single Cluster - specific environment) mode
/// Calculates sizing for a single environment at a time
/// </summary>
[TestFixture]
public class PerEnvironmentTests : PlaywrightFixture
{
    /// <summary>
    /// Navigate to Per-Environment mode with specific environment selected
    /// </summary>
    private async Task NavigateToPerEnvironmentConfigAsync(string environment = "Prod")
    {
        await NavigateToK8sConfigAsync();
        await SelectClusterModeAsync("Single");
        await SelectClusterScopeAsync(environment);
    }

    #region UI Tests

    [Test]
    public async Task PerEnvironment_ShowsEnvironmentSelector()
    {
        await NavigateToK8sConfigAsync();
        await SelectClusterModeAsync("Single");

        // Should show environment selector dropdown
        Assert.That(await IsVisibleAsync(".single-cluster-selector select"), Is.True,
            "Environment selector dropdown should be visible");
    }

    [Test]
    public async Task PerEnvironment_CanSelectProdEnvironment()
    {
        await NavigateToPerEnvironmentConfigAsync("Prod");

        // Verify Prod is selected
        var selectedValue = await Page.InputValueAsync(".single-cluster-selector select");
        Assert.That(selectedValue, Is.EqualTo("Prod"),
            "Prod environment should be selected");
    }

    [Test]
    public async Task PerEnvironment_CanSelectDevEnvironment()
    {
        await NavigateToPerEnvironmentConfigAsync("Dev");

        // Verify Dev is selected
        var selectedValue = await Page.InputValueAsync(".single-cluster-selector select");
        Assert.That(selectedValue, Is.EqualTo("Dev"),
            "Dev environment should be selected");
    }

    [Test]
    public async Task PerEnvironment_CanSelectTestEnvironment()
    {
        await NavigateToPerEnvironmentConfigAsync("Test");

        // Verify Test is selected
        var selectedValue = await Page.InputValueAsync(".single-cluster-selector select");
        Assert.That(selectedValue, Is.EqualTo("Test"),
            "Test environment should be selected");
    }

    [Test]
    public async Task PerEnvironment_CanSelectStageEnvironment()
    {
        await NavigateToPerEnvironmentConfigAsync("Stage");

        // Verify Stage is selected
        var selectedValue = await Page.InputValueAsync(".single-cluster-selector select");
        Assert.That(selectedValue, Is.EqualTo("Stage"),
            "Stage environment should be selected");
    }

    [Test]
    public async Task PerEnvironment_ShowsSingleEnvironmentRows()
    {
        await NavigateToPerEnvironmentConfigAsync("Prod");

        // Per-Environment mode uses .single-env-config with .tier-input-group
        // Not the cluster-row table structure
        var enabledInputs = await Page.QuerySelectorAllAsync(".single-env-config .tier-input-group input");
        if (enabledInputs.Count == 0)
        {
            // Try .app-tiers-grid which contains the tier input groups
            enabledInputs = await Page.QuerySelectorAllAsync(".app-tiers-grid input");
        }
        if (enabledInputs.Count == 0)
        {
            // Try cluster rows (may still be used in some modes)
            enabledInputs = await Page.QuerySelectorAllAsync(".cluster-row .tier-col input");
        }
        if (enabledInputs.Count == 0)
        {
            // Final fallback - any number input in the config area
            enabledInputs = await Page.QuerySelectorAllAsync(".config-tab-content input[type='number']");
        }
        Assert.That(enabledInputs.Count, Is.GreaterThan(0),
            "Should have inputs for the selected environment");
    }

    [Test]
    public async Task PerEnvironment_NodeSpecs_ShowsUnifiedSpecs()
    {
        await NavigateToPerEnvironmentConfigAsync("Prod");
        await ClickTabAsync("Node Specs");

        // Per-Environment is a SINGLE cluster - should show unified node specs table
        Assert.That(await IsVisibleAsync(".unified-specs") ||
                   await IsVisibleAsync(".node-spec-row:has-text('Worker')"), Is.True,
            "Worker node specs should be visible");

        // Should NOT show "Production" and "Non-Prod" distinction labels (that's for multi-cluster)
        var prodLabel = await Page.QuerySelectorAsync(".env-type-col.prod-label");
        var nonprodLabel = await Page.QuerySelectorAsync(".env-type-col.nonprod-label");
        Assert.That(prodLabel == null && nonprodLabel == null, Is.True,
            "Single cluster should not show Prod/Non-Prod labels in node specs");
    }

    #endregion

    #region Calculation Tests

    [Test]
    public async Task PerEnvironment_Calculate_ReturnsSingleEnvironment()
    {
        await NavigateToPerEnvironmentConfigAsync("Prod");

        // Set apps using single-env-config (per-environment mode UI)
        var inputs = await Page.QuerySelectorAllAsync(".single-env-config .tier-input-group input");
        if (inputs.Count == 0)
        {
            inputs = await Page.QuerySelectorAllAsync(".app-tiers-grid input");
        }
        if (inputs.Count > 0)
        {
            await inputs[0].FillAsync("20");
        }

        await ClickCalculateAsync();
        await Page.WaitForSelectorAsync(".results-panel", new() { Timeout = 10000 });

        // Should show single environment result
        var tableRows = await Page.QuerySelectorAllAsync(".results-table tbody tr");
        Assert.That(tableRows.Count, Is.EqualTo(1).Or.EqualTo(2), // 1 data row + possibly 1 total row
            "Per-environment should show single environment result");
    }

    [Test]
    public async Task PerEnvironment_Prod_UsesUnifiedSpecs()
    {
        await NavigateToPerEnvironmentConfigAsync("Prod");
        await ClickTabAsync("Node Specs");

        // All single cluster modes use unified specs - should see unified-specs table
        Assert.That(await IsVisibleAsync(".unified-specs") ||
                   await IsVisibleAsync(".node-spec-row:has-text('Worker')"), Is.True,
            "Single cluster should show unified specs");

        await ClickTabAsync("Applications");

        // Set apps using the single-env-config inputs
        var inputs = await Page.QuerySelectorAllAsync(".single-env-config .tier-input-group input");
        if (inputs.Count == 0)
        {
            inputs = await Page.QuerySelectorAllAsync(".app-tiers-grid input");
        }
        if (inputs.Count > 0)
        {
            await inputs[0].FillAsync("10");
        }

        await ClickCalculateAsync();
        await Page.WaitForSelectorAsync(".results-panel", new() { Timeout = 10000 });

        // Results should show Prod environment
        var resultsText = await GetTextAsync(".results-table");
        Assert.That(resultsText, Does.Contain("Prod").IgnoreCase,
            "Results should show Production environment");
    }

    [Test]
    public async Task PerEnvironment_Dev_UsesUnifiedSpecs()
    {
        await NavigateToPerEnvironmentConfigAsync("Dev");

        // All single cluster modes use unified specs
        await ClickTabAsync("Node Specs");
        Assert.That(await IsVisibleAsync(".unified-specs") ||
                   await IsVisibleAsync(".node-spec-row:has-text('Worker')"), Is.True,
            "Single cluster should show unified specs even for Dev");

        await ClickTabAsync("Applications");

        // Set apps for Dev
        var inputs = await Page.QuerySelectorAllAsync(".single-env-config .tier-input-group input");
        if (inputs.Count == 0)
        {
            inputs = await Page.QuerySelectorAllAsync(".app-tiers-grid input");
        }
        if (inputs.Count > 0)
        {
            await inputs[0].FillAsync("10");
        }

        await ClickCalculateAsync();
        await Page.WaitForSelectorAsync(".results-panel", new() { Timeout = 10000 });

        // Results should show Dev environment
        var resultsText = await GetTextAsync(".results-table");
        Assert.That(resultsText, Does.Contain("Dev").Or.Contain("Development").IgnoreCase,
            "Results should show Dev/Development environment");
    }

    [Test]
    public async Task PerEnvironment_SwitchEnvironment_RecalculatesCorrectly()
    {
        await NavigateToPerEnvironmentConfigAsync("Prod");

        // Set apps for Prod using single-env-config UI
        var prodInputs = await Page.QuerySelectorAllAsync(".single-env-config .tier-input-group input");
        if (prodInputs.Count == 0)
        {
            prodInputs = await Page.QuerySelectorAllAsync(".app-tiers-grid input");
        }
        if (prodInputs.Count > 0)
        {
            await prodInputs[0].FillAsync("20");
        }

        await ClickCalculateAsync();
        await Page.WaitForSelectorAsync(".results-panel", new() { Timeout = 10000 });

        // Verify Prod result
        var prodResultText = await GetTextAsync(".results-table");
        Assert.That(prodResultText, Does.Contain("Prod").IgnoreCase,
            "First calculation should show Prod");

        // Go back and switch to Dev
        await ClickBackAsync();
        await SelectClusterScopeAsync("Dev");

        // Set apps for Dev using single-env-config UI
        var devInputs = await Page.QuerySelectorAllAsync(".single-env-config .tier-input-group input");
        if (devInputs.Count == 0)
        {
            devInputs = await Page.QuerySelectorAllAsync(".app-tiers-grid input");
        }
        if (devInputs.Count > 0)
        {
            await devInputs[0].FillAsync("10");
        }

        await ClickCalculateAsync();
        await Page.WaitForSelectorAsync(".results-panel", new() { Timeout = 10000 });

        // Verify Dev result
        var devResultText = await GetTextAsync(".results-table");
        Assert.That(devResultText, Does.Contain("Dev").Or.Contain("Development").IgnoreCase,
            "Second calculation should show Dev");
    }

    [Test]
    public async Task PerEnvironment_ShowsCorrectEnvironmentBadge()
    {
        await NavigateToPerEnvironmentConfigAsync("Stage");

        // Set apps using single-env-config UI
        var inputs = await Page.QuerySelectorAllAsync(".single-env-config .tier-input-group input");
        if (inputs.Count == 0)
        {
            inputs = await Page.QuerySelectorAllAsync(".app-tiers-grid input");
        }
        if (inputs.Count > 0)
        {
            await inputs[0].FillAsync("15");
        }

        await ClickCalculateAsync();
        await Page.WaitForSelectorAsync(".results-panel", new() { Timeout = 10000 });

        // Results should show Stage environment badge
        var resultsText = await GetTextAsync(".results-table");
        Assert.That(resultsText, Does.Contain("Stage").Or.Contain("Staging").IgnoreCase,
            "Results should show Stage/Staging environment");
    }

    #endregion
}
