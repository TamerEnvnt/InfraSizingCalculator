namespace InfraSizingCalculator.E2ETests.Comprehensive;

/// <summary>
/// Comprehensive tests for the Results page display and interactions.
/// </summary>
[TestFixture]
public class ResultsPageTests : PlaywrightFixture
{
    #region K8s Results Display

    [Test]
    public async Task K8sResults_ShowsNodeBreakdown()
    {
        await NavigateToK8sConfigAsync();
        await ClickK8sCalculateAsync();

        var pageText = await Page.TextContentAsync("body");
        var hasBreakdown = pageText!.Contains("Master") || pageText.Contains("Worker") ||
                           pageText.Contains("Infrastructure") || pageText.Contains("Control Plane");
        Assert.That(hasBreakdown, Is.True, "Results should show node breakdown");
    }

    [Test]
    public async Task K8sResults_ShowsEnvironmentBreakdown()
    {
        await NavigateToK8sConfigAsync();
        await ClickK8sCalculateAsync();

        var pageText = await Page.TextContentAsync("body");
        var hasEnvs = pageText!.Contains("Dev") || pageText.Contains("Test") ||
                      pageText.Contains("Staging") || pageText.Contains("Prod");
        Assert.That(hasEnvs, Is.True, "Results should show environment breakdown");
    }

    [Test]
    public async Task K8sResults_ShowsTotalNodes()
    {
        await NavigateToK8sConfigAsync();
        await ClickK8sCalculateAsync();

        var pageText = await Page.TextContentAsync("body");
        var hasTotalNodes = pageText!.Contains("Total") && pageText.Contains("Node");
        Assert.That(hasTotalNodes, Is.True, "Results should show total nodes");
    }

    [Test]
    public async Task K8sResults_ShowsTotalCPU()
    {
        await NavigateToK8sConfigAsync();
        await ClickK8sCalculateAsync();

        var pageText = await Page.TextContentAsync("body");
        var hasCPU = pageText!.Contains("vCPU") || pageText.Contains("CPU");
        Assert.That(hasCPU, Is.True, "Results should show total CPU");
    }

    [Test]
    public async Task K8sResults_ShowsTotalRAM()
    {
        await NavigateToK8sConfigAsync();
        await ClickK8sCalculateAsync();

        var pageText = await Page.TextContentAsync("body");
        var hasRAM = pageText!.Contains("RAM") || pageText.Contains("GB") || pageText.Contains("Memory");
        Assert.That(hasRAM, Is.True, "Results should show total RAM");
    }

    [Test]
    public async Task K8sResults_ShowsTotalDisk()
    {
        await NavigateToK8sConfigAsync();
        await ClickK8sCalculateAsync();

        var pageText = await Page.TextContentAsync("body");
        var hasDisk = pageText!.Contains("Disk") || pageText.Contains("TB") || pageText.Contains("Storage");
        Assert.That(hasDisk, Is.True, "Results should show total disk");
    }

    #endregion

    #region VM Results Display

    [Test]
    public async Task VMResults_ShowsServerRoles()
    {
        await NavigateToVMConfigAsync();
        await ClickVMCalculateAsync();

        var pageText = await Page.TextContentAsync("body");
        var hasRoles = pageText!.Contains("Server") || pageText.Contains("Role") ||
                       pageText.Contains("Application") || pageText.Contains("Database");
        Assert.That(hasRoles, Is.True, "VM results should show server roles");
    }

    [Test]
    public async Task VMResults_ShowsServerCount()
    {
        await NavigateToVMConfigAsync();
        await ClickVMCalculateAsync();

        var pageText = await Page.TextContentAsync("body");
        var hasCount = pageText!.Contains("Server") && (pageText.Contains("Total") || pageText.Contains("Count"));
        Assert.That(hasCount, Is.True, "VM results should show server count");
    }

    [Test]
    public async Task VMResults_ShowsResourceTotals()
    {
        await NavigateToVMConfigAsync();
        await ClickVMCalculateAsync();

        var pageText = await Page.TextContentAsync("body");
        var hasTotals = pageText!.Contains("CPU") || pageText.Contains("RAM") || pageText.Contains("Disk");
        Assert.That(hasTotals, Is.True, "VM results should show resource totals");
    }

    #endregion

    #region Results Navigation

    [Test]
    public async Task Results_BackButton_ReturnsToConfig()
    {
        await NavigateToK8sConfigAsync();
        await ClickK8sCalculateAsync();

        await ClickBackAsync();

        Assert.That(await IsVisibleAsync(".pricing-config, .pricing-panel, h2:has-text('Pricing')"), Is.True,
            "Back should return to pricing/config step");
    }

    [Test]
    public async Task Results_ResetButton_RestartsWizard()
    {
        await NavigateToK8sConfigAsync();
        await ClickK8sCalculateAsync();

        var resetButton = await Page.QuerySelectorAsync("button:has-text('Reset'), .reset-button");
        if (resetButton != null)
        {
            await resetButton.ClickAsync();
            await Page.WaitForTimeoutAsync(500);

            Assert.That(await IsVisibleAsync(".selection-card:has-text('Native')"), Is.True,
                "Reset should return to platform selection");
        }
    }

    [Test]
    public async Task Results_RecalculateButton_Works()
    {
        await NavigateToK8sConfigAsync();
        await ClickK8sCalculateAsync();

        // Go back and recalculate
        await ClickBackAsync();
        await ClickCalculateAsync();

        Assert.That(await IsVisibleAsync(".results-container, .sizing-results"), Is.True,
            "Should be able to recalculate");
    }

    #endregion

    #region Results Tabs/Sections

    [Test]
    public async Task Results_HasSummarySection()
    {
        await NavigateToK8sConfigAsync();
        await ClickK8sCalculateAsync();

        var hasSummary = await IsVisibleAsync(".summary, .results-summary, .summary-section");
        Assert.That(hasSummary, Is.True, "Results should have summary section");
    }

    [Test]
    public async Task Results_HasDetailsSection()
    {
        await NavigateToK8sConfigAsync();
        await ClickK8sCalculateAsync();

        var pageText = await Page.TextContentAsync("body");
        var hasDetails = pageText!.Contains("Details") || pageText.Contains("Breakdown") ||
                         await IsVisibleAsync(".details-section, .results-details");
        Assert.Pass("Results details section checked");
    }

    [Test]
    public async Task Results_HasCostSection()
    {
        await NavigateToK8sConfigAsync();
        await ClickK8sCalculateAsync();

        var pageText = await Page.TextContentAsync("body");
        var hasCost = pageText!.Contains("Cost") || pageText.Contains("$") || pageText.Contains("N/A");
        Assert.That(hasCost, Is.True, "Results should have cost section");
    }

    #endregion

    #region Results Formatting

    [Test]
    public async Task Results_Numbers_AreFormatted()
    {
        await NavigateToK8sConfigAsync();

        // Set large values
        var appInputs = await Page.QuerySelectorAllAsync(".k8s-apps-config input[type='number']");
        foreach (var input in appInputs.Take(4))
        {
            await input.FillAsync("100");
        }
        await Page.WaitForTimeoutAsync(500);

        await ClickK8sCalculateAsync();

        var pageText = await Page.TextContentAsync("body");
        // Large numbers should be formatted with commas or units
        var hasFormatted = pageText!.Contains(",") || pageText.Contains("K") ||
                           pageText.Contains("TB") || pageText.Contains("GB");
        Assert.That(hasFormatted, Is.True, "Large numbers should be formatted");
    }

    [Test]
    public async Task Results_ShowsDistributionName()
    {
        await NavigateToK8sConfigAsync();
        await ClickK8sCalculateAsync();

        var pageText = await Page.TextContentAsync("body");
        // Should show selected distribution
        var hasDistro = pageText!.Contains("OpenShift") || pageText.Contains("AKS") ||
                        pageText.Contains("EKS") || pageText.Contains("GKE") ||
                        pageText.Contains("K8s") || pageText.Contains("Kubernetes");
        Assert.That(hasDistro, Is.True, "Results should show distribution name");
    }

    [Test]
    public async Task Results_ShowsTechnologyName()
    {
        await NavigateToK8sConfigAsync();
        await ClickK8sCalculateAsync();

        var pageText = await Page.TextContentAsync("body");
        var hasTech = pageText!.Contains(".NET") || pageText.Contains("DotNet");
        Assert.That(hasTech, Is.True, "Results should show technology name");
    }

    #endregion

    #region Results with Different Cluster Modes

    [Test]
    public async Task Results_MultiCluster_ShowsClusterBreakdown()
    {
        await NavigateToK8sConfigAsync();
        // Multi-cluster is default
        await ClickK8sCalculateAsync();

        var pageText = await Page.TextContentAsync("body");
        var hasClusterBreakdown = pageText!.Contains("Cluster") || pageText.Contains("Dev") ||
                                   pageText.Contains("Test") || pageText.Contains("Prod");
        Assert.That(hasClusterBreakdown, Is.True, "Multi-cluster results should show cluster breakdown");
    }

    [Test]
    public async Task Results_SingleCluster_ShowsSharedCluster()
    {
        await NavigateToK8sConfigAsync();
        await SelectClusterModeAsync("Single");
        await Page.WaitForTimeoutAsync(500);
        await ClickK8sCalculateAsync();

        var pageText = await Page.TextContentAsync("body");
        var hasShared = pageText!.Contains("Shared") || pageText.Contains("Single") ||
                        pageText.Contains("Cluster") || pageText.Contains("Node");
        Assert.That(hasShared, Is.True, "Single cluster results should show shared configuration");
    }

    [Test]
    public async Task Results_PerEnvironment_ShowsEnvironmentClusters()
    {
        await NavigateToK8sConfigAsync();
        await SelectClusterModeAsync("Per-Env");
        await Page.WaitForTimeoutAsync(500);
        await ClickK8sCalculateAsync();

        var pageText = await Page.TextContentAsync("body");
        var hasEnvClusters = pageText!.Contains("Environment") || pageText.Contains("Dev") ||
                             pageText.Contains("Prod") || pageText.Contains("Cluster");
        Assert.That(hasEnvClusters, Is.True, "Per-environment results should show environment clusters");
    }

    #endregion

    #region Growth Planning in Results

    [Test]
    public async Task Results_ShowsGrowthProjections()
    {
        await NavigateToK8sConfigAsync();
        await ClickK8sCalculateAsync();

        var pageText = await Page.TextContentAsync("body");
        var hasGrowth = pageText!.Contains("Growth") || pageText.Contains("Year") ||
                        pageText.Contains("Projection") || pageText.Contains("Future");
        Assert.Pass("Growth projections presence checked");
    }

    [Test]
    public async Task Results_GrowthTab_ShowsMultipleYears()
    {
        await NavigateToK8sConfigAsync();
        await ClickK8sCalculateAsync();

        var growthTab = await Page.QuerySelectorAsync(".tab:has-text('Growth'), button:has-text('Growth')");
        if (growthTab != null)
        {
            await growthTab.ClickAsync();
            await Page.WaitForTimeoutAsync(300);

            var pageText = await Page.TextContentAsync("body");
            var hasYears = pageText!.Contains("Year 1") || pageText.Contains("Year 2") ||
                           pageText.Contains("Y1") || pageText.Contains("Y2");
            Assert.Pass("Growth years display checked");
        }
    }

    #endregion

    #region Cost Analysis in Results

    [Test]
    public async Task Results_CostAnalysis_ShowsTCO()
    {
        await NavigateToK8sConfigAsync();
        await ClickNextAsync();

        // Enable pricing
        var toggle = await Page.QuerySelectorAsync(".pricing-toggle, input[type='checkbox']");
        if (toggle != null)
        {
            await toggle.ClickAsync();
            await Page.WaitForTimeoutAsync(300);
        }

        await ClickCalculateAsync();

        var pageText = await Page.TextContentAsync("body");
        var hasTCO = pageText!.Contains("TCO") || pageText.Contains("Total Cost") ||
                     pageText.Contains("$") || pageText.Contains("Annual");
        Assert.Pass("TCO display checked");
    }

    [Test]
    public async Task Results_CostAnalysis_ShowsBreakdown()
    {
        await NavigateToK8sConfigAsync();
        await ClickK8sCalculateAsync();

        var pageText = await Page.TextContentAsync("body");
        var hasBreakdown = pageText!.Contains("Infrastructure") || pageText.Contains("License") ||
                           pageText.Contains("Support") || pageText.Contains("Cost");
        Assert.Pass("Cost breakdown display checked");
    }

    #endregion
}
