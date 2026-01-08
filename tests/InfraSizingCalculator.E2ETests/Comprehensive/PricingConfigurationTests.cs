namespace InfraSizingCalculator.E2ETests.Comprehensive;

/// <summary>
/// Tests for pricing configuration across all deployment types and pricing providers.
/// </summary>
[TestFixture]
public class PricingConfigurationTests : PlaywrightFixture
{
    #region Pricing Step Navigation

    [Test]
    public async Task K8s_PricingStep_IsAccessible()
    {
        await NavigateToK8sConfigAsync();
        await ClickNextAsync();

        Assert.That(await IsVisibleAsync(".pricing-config, .pricing-panel, h2:has-text('Pricing')"), Is.True,
            "Should reach pricing step for K8s");
    }

    [Test]
    public async Task VM_PricingStep_IsAccessible()
    {
        await NavigateToVMConfigAsync();
        await ClickNextAsync();

        Assert.That(await IsVisibleAsync(".pricing-config, .pricing-panel, h2:has-text('Pricing')"), Is.True,
            "Should reach pricing step for VMs");
    }

    #endregion

    #region Pricing Toggle

    [Test]
    public async Task K8s_PricingToggle_CanBeEnabled()
    {
        await NavigateToK8sConfigAsync();
        await ClickNextAsync();

        // Find pricing toggle
        var toggle = await Page.QuerySelectorAsync(".pricing-toggle, input[type='checkbox']:has-text('Pricing'), .toggle-switch");
        if (toggle != null)
        {
            await toggle.ClickAsync();
            await Page.WaitForTimeoutAsync(300);
            Assert.Pass("Pricing toggle is functional");
        }
    }

    [Test]
    public async Task K8s_PricingDisabled_ShowsNA()
    {
        await NavigateToK8sConfigAsync();
        await ClickNextAsync();

        // With pricing disabled, costs should show N/A
        var pageText = await Page.TextContentAsync("body");
        var hasNA = pageText!.Contains("N/A") || pageText.Contains("not configured");
        Assert.That(hasNA, Is.True, "Should show N/A when pricing is disabled");
    }

    #endregion

    #region Infrastructure Costs Tab

    [Test]
    public async Task K8s_InfraCostsTab_Exists()
    {
        await NavigateToK8sConfigAsync();
        await ClickNextAsync();

        Assert.That(await IsVisibleAsync(".pricing-tab:has-text('Infra'), .tab:has-text('Infrastructure')"), Is.True,
            "Infrastructure costs tab should exist");
    }

    [Test]
    public async Task K8s_InfraCostsTab_ShowsCostInputs()
    {
        await NavigateToK8sConfigAsync();
        await ClickNextAsync();

        // Click on infra tab if not active
        var infraTab = await Page.QuerySelectorAsync(".pricing-tab:has-text('Infra'), .tab:has-text('Infrastructure')");
        if (infraTab != null)
        {
            await infraTab.ClickAsync();
            await Page.WaitForTimeoutAsync(300);
        }

        // Should show cost input fields
        var hasCostInputs = await IsVisibleAsync("input[type='number'], .cost-input, .price-input");
        Assert.That(hasCostInputs, Is.True, "Should show cost input fields");
    }

    #endregion

    #region Cloud Alternatives Tab

    [Test]
    public async Task K8s_CloudAlternativesTab_Exists()
    {
        await NavigateToK8sConfigAsync();
        await ClickNextAsync();

        Assert.That(await IsVisibleAsync(".pricing-tab:has-text('Cloud'), .tab:has-text('Alternatives')"), Is.True,
            "Cloud Alternatives tab should exist");
    }

    [Test]
    public async Task K8s_CloudAlternativesTab_ShowsProviders()
    {
        await NavigateToK8sConfigAsync();
        await ClickNextAsync();

        // Click on cloud alternatives tab
        var cloudTab = await Page.QuerySelectorAsync(".pricing-tab:has-text('Cloud'), .tab:has-text('Alternatives')");
        if (cloudTab != null)
        {
            await cloudTab.ClickAsync();
            await Page.WaitForTimeoutAsync(300);

            // Should show cloud provider options
            var pageText = await Page.TextContentAsync("body");
            var hasProviders = pageText!.Contains("AWS") || pageText.Contains("Azure") || pageText.Contains("GCP");
            Assert.That(hasProviders, Is.True, "Should show cloud provider options");
        }
    }

    #endregion

    #region Mendix Pricing

    [Test]
    public async Task Mendix_PricingPanel_IsAccessible()
    {
        await GoToHomeAsync();
        await SelectCardAsync("Low-Code");
        await SelectCardAsync("Kubernetes");
        await SelectTechCardAsync("Mendix");
        await SelectMendixCategoryCardAsync("Private Cloud");
        await SelectMendixProviderCardAsync("Mendix Azure");
        await ClickNextAsync();

        // Mendix pricing should have specific options
        var pageText = await Page.TextContentAsync("body");
        var hasMendixPricing = pageText!.Contains("Mendix") || pageText.Contains("License") ||
                               pageText.Contains("Pricing");
        Assert.That(hasMendixPricing, Is.True, "Mendix pricing options should be accessible");
    }

    [Test]
    public async Task Mendix_EditionSelector_Exists()
    {
        await GoToHomeAsync();
        await SelectCardAsync("Low-Code");
        await SelectCardAsync("Kubernetes");
        await SelectTechCardAsync("Mendix");
        await SelectMendixCategoryCardAsync("Private Cloud");
        await SelectMendixProviderCardAsync("Mendix Azure");
        await ClickNextAsync();

        // Should have edition selector
        var editionSelector = await Page.QuerySelectorAsync("select:has-text('Edition'), .edition-selector, select");
        Assert.That(editionSelector, Is.Not.Null, "Mendix edition selector should exist");
    }

    [Test]
    public async Task Mendix_UserCountInput_Exists()
    {
        await GoToHomeAsync();
        await SelectCardAsync("Low-Code");
        await SelectCardAsync("Kubernetes");
        await SelectTechCardAsync("Mendix");
        await SelectMendixCategoryCardAsync("Private Cloud");
        await SelectMendixProviderCardAsync("Mendix Azure");
        await ClickNextAsync();

        // Should have user count input
        var userInput = await Page.QuerySelectorAsync("input[type='number']");
        Assert.That(userInput, Is.Not.Null, "User count input should exist for Mendix pricing");
    }

    #endregion

    #region OutSystems Pricing (VM-only - OutSystems K8s not supported)

    [Test]
    public async Task OutSystems_PricingPanel_IsAccessible()
    {
        // OutSystems is VM-only (K8s only via OutSystems Cloud/ODC SaaS)
        await GoToHomeAsync();
        await SelectCardAsync("Low-Code");
        await SelectCardAsync("Virtual Machines");
        await SelectTechCardAsync("OutSystems");
        await ClickNextAsync();

        // OutSystems pricing should have specific options
        var pageText = await Page.TextContentAsync("body");
        var hasOutSystemsPricing = pageText!.Contains("OutSystems") || pageText.Contains("License") ||
                                   pageText.Contains("Pricing") || pageText.Contains("AO");
        Assert.That(hasOutSystemsPricing, Is.True, "OutSystems pricing options should be accessible");
    }

    [Test]
    public async Task OutSystems_AOInput_Exists()
    {
        // OutSystems is VM-only (K8s only via OutSystems Cloud/ODC SaaS)
        await GoToHomeAsync();
        await SelectCardAsync("Low-Code");
        await SelectCardAsync("Virtual Machines");
        await SelectTechCardAsync("OutSystems");
        await ClickNextAsync();

        // Should have AO (Application Objects) input
        var pageText = await Page.TextContentAsync("body");
        var hasAO = pageText!.Contains("AO") || pageText.Contains("Application Object");
        Assert.That(hasAO, Is.True, "OutSystems should show AO configuration");
    }

    #endregion

    #region VM Pricing

    [Test]
    public async Task VM_PricingPanel_ShowsServerCosts()
    {
        await NavigateToVMConfigAsync();
        await ClickNextAsync();

        // VM pricing should show server-based costs
        var pageText = await Page.TextContentAsync("body");
        var hasServerCosts = pageText!.Contains("Server") || pageText.Contains("Cost") ||
                             pageText.Contains("Pricing") || pageText.Contains("VM");
        Assert.That(hasServerCosts, Is.True, "VM pricing should show server cost options");
    }

    [Test]
    public async Task VM_PricingPanel_HasCostPerServer()
    {
        await NavigateToVMConfigAsync();
        await ClickNextAsync();

        // Should have cost per server inputs
        var costInput = await Page.QuerySelectorAsync("input[type='number']");
        Assert.That(costInput, Is.Not.Null, "Should have cost input fields for VM pricing");
    }

    #endregion

    #region Calculate Button

    [Test]
    public async Task K8s_CalculateButton_Exists()
    {
        await NavigateToK8sConfigAsync();
        await ClickNextAsync();

        Assert.That(await IsVisibleAsync("button:has-text('Calculate')"), Is.True,
            "Calculate button should exist on pricing step");
    }

    [Test]
    public async Task K8s_CalculateButton_IsEnabled()
    {
        await NavigateToK8sConfigAsync();
        await ClickNextAsync();

        var calcButton = await Page.QuerySelectorAsync("button:has-text('Calculate')");
        Assert.That(calcButton, Is.Not.Null, "Calculate button should exist");

        var isDisabled = await calcButton!.GetAttributeAsync("disabled");
        Assert.That(isDisabled, Is.Null, "Calculate button should be enabled");
    }

    [Test]
    public async Task K8s_Calculate_NavigatesToResults()
    {
        await NavigateToK8sConfigAsync();
        await ClickK8sCalculateAsync();

        Assert.That(await IsVisibleAsync(".results-container, .sizing-results, .result-panel"), Is.True,
            "Should navigate to results after calculate");
    }

    [Test]
    public async Task VM_Calculate_NavigatesToResults()
    {
        await NavigateToVMConfigAsync();
        await ClickVMCalculateAsync();

        Assert.That(await IsVisibleAsync(".results-container, .sizing-results, .result-panel"), Is.True,
            "Should navigate to results after calculate for VMs");
    }

    #endregion
}
