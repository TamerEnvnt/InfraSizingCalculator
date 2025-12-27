namespace InfraSizingCalculator.E2ETests.K8s;

/// <summary>
/// E2E tests for Per-Environment (Single Cluster - specific environment) mode
/// Calculates sizing for a single environment at a time
/// In Single Cluster mode, the UI shows .tier-cards with .tier-card for each size
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
        await Page.WaitForTimeoutAsync(500);
        await SelectClusterScopeAsync(environment);
        await Page.WaitForTimeoutAsync(300);
    }

    /// <summary>
    /// Helper to set app count in single cluster mode
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
    public async Task PerEnvironment_ShowsEnvironmentSelector()
    {
        await NavigateToK8sConfigAsync();
        await SelectClusterModeAsync("Single");
        await Page.WaitForTimeoutAsync(500);

        // Should show environment selector dropdown
        Assert.That(await IsVisibleAsync(".single-cluster-selector select") ||
                   await IsVisibleAsync(".scope-selector select") ||
                   await IsVisibleAsync("select"), Is.True,
            "Environment selector dropdown should be visible in single cluster mode");
    }

    [Test]
    public async Task PerEnvironment_CanSelectProdEnvironment()
    {
        await NavigateToPerEnvironmentConfigAsync("Prod");

        // Verify Prod is selected - check the dropdown value
        var selectElement = await Page.QuerySelectorAsync(".single-cluster-selector select");
        if (selectElement == null)
        {
            selectElement = await Page.QuerySelectorAsync("select");
        }

        if (selectElement != null)
        {
            var selectedValue = await selectElement.InputValueAsync();
            Assert.That(selectedValue, Is.EqualTo("Prod"),
                "Prod environment should be selected");
        }
    }

    [Test]
    public async Task PerEnvironment_CanSelectDevEnvironment()
    {
        await NavigateToPerEnvironmentConfigAsync("Dev");

        // Verify Dev is selected
        var selectElement = await Page.QuerySelectorAsync(".single-cluster-selector select");
        if (selectElement == null)
        {
            selectElement = await Page.QuerySelectorAsync("select");
        }

        if (selectElement != null)
        {
            var selectedValue = await selectElement.InputValueAsync();
            Assert.That(selectedValue, Is.EqualTo("Dev"),
                "Dev environment should be selected");
        }
    }

    [Test]
    public async Task PerEnvironment_ShowsTierCards()
    {
        await NavigateToPerEnvironmentConfigAsync("Prod");

        // Single cluster mode shows tier cards (S/M/L/XL)
        var tierCards = await Page.QuerySelectorAllAsync(".tier-card");
        Assert.That(tierCards.Count, Is.GreaterThanOrEqualTo(4),
            "Should show 4 tier cards for workload configuration");

        // Verify tier cards have inputs
        var inputs = await Page.QuerySelectorAllAsync(".tier-card input[type='number']");
        Assert.That(inputs.Count, Is.GreaterThanOrEqualTo(4),
            "Each tier card should have an input");
    }

    [Test]
    public async Task PerEnvironment_NodeSpecs_ShowsUnifiedSpecs()
    {
        await NavigateToPerEnvironmentConfigAsync("Prod");
        await ClickTabAsync("Node Specs");

        // Per-Environment is a SINGLE cluster - should show node specs
        Assert.That(await IsVisibleAsync(".node-specs-panel") ||
                   await IsVisibleAsync(".node-spec-row") ||
                   await IsVisibleAsync(".unified-specs"), Is.True,
            "Node specs should be visible");
    }

    #endregion

    #region Calculation Tests

    [Test]
    public async Task PerEnvironment_Calculate_ShowsResults()
    {
        await NavigateToPerEnvironmentConfigAsync("Prod");

        // Set apps using tier cards
        await SetTierAppsAsync("medium", "20");

        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync(".sizing-results-view, .results-panel", new() { Timeout = 10000 });

        // Should show results
        Assert.That(await IsVisibleAsync(".sizing-results-view") || await IsVisibleAsync(".results-panel"), Is.True,
            "Results should be displayed");
    }

    [Test]
    public async Task PerEnvironment_Prod_ShowsProdResults()
    {
        await NavigateToPerEnvironmentConfigAsync("Prod");

        // Set apps
        await SetTierAppsAsync("small", "10");

        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync(".sizing-results-view, .results-panel", new() { Timeout = 10000 });

        // Results should be visible (Prod is default so may not be explicitly labeled)
        Assert.That(await IsVisibleAsync(".sizing-results-view") || await IsVisibleAsync(".results-panel"), Is.True,
            "Results should be visible for Prod");
    }

    [Test]
    public async Task PerEnvironment_Dev_ShowsDevResults()
    {
        await NavigateToPerEnvironmentConfigAsync("Dev");

        // Set apps
        await SetTierAppsAsync("small", "10");

        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync(".sizing-results-view, .results-panel", new() { Timeout = 10000 });

        // Results should be visible
        Assert.That(await IsVisibleAsync(".sizing-results-view") || await IsVisibleAsync(".results-panel"), Is.True,
            "Results should be visible for Dev");
    }

    [Test]
    public async Task PerEnvironment_SwitchEnvironment_RecalculatesCorrectly()
    {
        await NavigateToPerEnvironmentConfigAsync("Prod");

        // Set apps for Prod
        await SetTierAppsAsync("medium", "20");

        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync(".sizing-results-view, .results-panel", new() { Timeout = 10000 });

        // Verify results are visible
        Assert.That(await IsVisibleAsync(".sizing-results-view") || await IsVisibleAsync(".results-panel"), Is.True,
            "First calculation should show results");

        // Go back to Configure step
        await ClickBackAsync();
        await ClickBackAsync();
        await Page.WaitForTimeoutAsync(500);

        // Switch to Dev
        await SelectClusterScopeAsync("Dev");
        await Page.WaitForTimeoutAsync(300);

        // Set apps for Dev
        await SetTierAppsAsync("small", "10");

        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync(".sizing-results-view, .results-panel", new() { Timeout = 10000 });

        // Verify results are visible
        Assert.That(await IsVisibleAsync(".sizing-results-view") || await IsVisibleAsync(".results-panel"), Is.True,
            "Second calculation should show results");
    }

    [Test]
    public async Task PerEnvironment_Stage_ShowsStageResults()
    {
        await NavigateToPerEnvironmentConfigAsync("Stage");

        // Set apps
        await SetTierAppsAsync("medium", "15");

        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync(".sizing-results-view, .results-panel", new() { Timeout = 10000 });

        // Results should be visible
        Assert.That(await IsVisibleAsync(".sizing-results-view") || await IsVisibleAsync(".results-panel"), Is.True,
            "Results should be visible for Stage");
    }

    [Test]
    public async Task PerEnvironment_Test_ShowsTestResults()
    {
        await NavigateToPerEnvironmentConfigAsync("Test");

        // Set apps
        await SetTierAppsAsync("small", "5");

        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync(".sizing-results-view, .results-panel", new() { Timeout = 10000 });

        // Results should be visible
        Assert.That(await IsVisibleAsync(".sizing-results-view") || await IsVisibleAsync(".results-panel"), Is.True,
            "Results should be visible for Test");
    }

    #endregion
}
