using Microsoft.Playwright;

namespace InfraSizingCalculator.E2ETests;

/// <summary>
/// Debug tests to diagnose Playwright issues
/// </summary>
[TestFixture]
[Explicit] // Only run when explicitly requested
public class DebugTests : PlaywrightFixture
{
    [Test]
    public async Task Debug_ClickNativeCard_DirectClick()
    {
        await GoToHomeAsync();

        // Wait for the page to be fully loaded
        await Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
        await Page.WaitForTimeoutAsync(2000); // Give Blazor time to hydrate

        // Log initial state
        var initialCards = await Page.QuerySelectorAllAsync(".selection-card .card-title");
        Console.WriteLine($"Initial card count: {initialCards.Count}");
        foreach (var card in initialCards)
        {
            var titleText = await card.TextContentAsync();
            Console.WriteLine($"Initial card: {titleText}");
        }

        // Click the Native Applications card
        Console.WriteLine("Clicking Native Applications...");
        await Page.ClickAsync(".selection-card:has-text('Native')");
        await Page.WaitForTimeoutAsync(1000);

        // Check what cards are visible now
        var cardsAfterClick = await Page.QuerySelectorAllAsync(".selection-card .card-title");
        Console.WriteLine($"Cards after click: {cardsAfterClick.Count}");
        foreach (var card in cardsAfterClick)
        {
            var titleText = await card.TextContentAsync();
            Console.WriteLine($"Card after click: {titleText}");
        }

        // The click WORKED if we see Kubernetes/VMs cards (Step 2) instead of Native/Low-Code (Step 1)
        var kubernetesCard = await Page.QuerySelectorAsync(".selection-card:has-text('Kubernetes')");
        var vmCard = await Page.QuerySelectorAsync(".selection-card:has-text('Virtual Machines')");

        Console.WriteLine($"Kubernetes card visible: {kubernetesCard != null}");
        Console.WriteLine($"VM card visible: {vmCard != null}");

        // SUCCESS: If we see Step 2 options (Kubernetes/VMs), the click worked!
        Assert.That(kubernetesCard != null || vmCard != null, Is.True,
            "Click should have worked - should see Deployment options (Kubernetes/VMs)");
    }

    [Test]
    public async Task Debug_ClickNativeCard_PlaywrightClick()
    {
        await GoToHomeAsync();

        // Wait for Blazor
        await Page.WaitForFunctionAsync("window.Blazor !== undefined", new PageWaitForFunctionOptions { Timeout = 10000 });
        await Page.WaitForTimeoutAsync(1000);

        // Click using Playwright with simpler selector
        var card = Page.Locator(".selection-card").First;
        await card.ClickAsync();
        await Page.WaitForTimeoutAsync(1000);

        // Check if it got selected
        var selectedCount = await Page.Locator(".selection-card.selected").CountAsync();
        Console.WriteLine($"Selected cards after Playwright click: {selectedCount}");
        Assert.That(selectedCount, Is.GreaterThan(0), "Should have selected a card");
    }

    [Test]
    public async Task Debug_CheckBlazorInitialized()
    {
        await GoToHomeAsync();

        // Check if Blazor is defined
        var blazorDefined = await Page.EvaluateAsync<bool>("typeof window.Blazor !== 'undefined'");
        Console.WriteLine($"Blazor defined: {blazorDefined}");
        Assert.That(blazorDefined, Is.True, "Blazor should be defined");

        // Wait a bit and check again
        await Page.WaitForTimeoutAsync(2000);
        blazorDefined = await Page.EvaluateAsync<bool>("typeof window.Blazor !== 'undefined'");
        Console.WriteLine($"Blazor defined after 2s: {blazorDefined}");
    }

    [Test]
    public async Task Debug_CheckCardCount()
    {
        await GoToHomeAsync();
        await Page.WaitForTimeoutAsync(1000);

        var cardCount = await Page.Locator(".selection-card").CountAsync();
        Console.WriteLine($"Card count: {cardCount}");
        Assert.That(cardCount, Is.EqualTo(2), "Should have 2 platform cards");
    }

    [Test]
    public async Task Debug_K8sConfigPage()
    {
        await NavigateToK8sConfigAsync();

        // Check available cluster rows
        var clusterRows = await Page.QuerySelectorAllAsync(".cluster-row");
        Console.WriteLine($"Cluster rows: {clusterRows.Count}");

        // Check namespace rows
        var namespaceRows = await Page.QuerySelectorAllAsync(".namespace-row");
        Console.WriteLine($"Namespace rows: {namespaceRows.Count}");

        // Check all inputs
        var allInputs = await Page.QuerySelectorAllAsync("input[type='number']");
        Console.WriteLine($"Number inputs: {allInputs.Count}");

        // Check tier-col inputs specifically
        var tierInputs = await Page.QuerySelectorAllAsync(".tier-col input");
        Console.WriteLine($"Tier inputs: {tierInputs.Count}");

        // Check button state
        var calcButton = await Page.QuerySelectorAsync("button:has-text('Calculate')");
        if (calcButton != null)
        {
            var isDisabled = await calcButton.IsDisabledAsync();
            Console.WriteLine($"Calculate button disabled: {isDisabled}");
        }

        // Dump page HTML around config area
        var html = await Page.InnerHTMLAsync(".config-tabs-container");
        Console.WriteLine($"Config tabs HTML (first 2000 chars): {html.Substring(0, Math.Min(2000, html.Length))}");
    }

    [Test]
    public async Task Debug_ResultsPage()
    {
        await NavigateToK8sConfigAsync();

        // Fill in app count
        var prodInputs = await Page.QuerySelectorAllAsync(".cluster-row:has-text('Prod') .tier-col input:not([disabled])");
        Console.WriteLine($"Prod inputs: {prodInputs.Count}");
        if (prodInputs.Count > 0)
        {
            await prodInputs[0].FillAsync("20");
        }

        // Click Calculate
        await ClickCalculateAsync();
        await Page.WaitForSelectorAsync(".results-panel", new() { Timeout = 10000 });

        // Check results structure
        var resultsHtml = await Page.InnerHTMLAsync(".results-panel");
        Console.WriteLine($"Results HTML (first 3000 chars): {resultsHtml.Substring(0, Math.Min(3000, resultsHtml.Length))}");

        // Check for env-badge
        var envBadges = await Page.QuerySelectorAllAsync(".env-badge");
        Console.WriteLine($"Env badges: {envBadges.Count}");
        foreach (var badge in envBadges)
        {
            var text = await badge.TextContentAsync();
            Console.WriteLine($"Badge text: {text}");
        }
    }

    [Test]
    public async Task Debug_SingleClusterMode()
    {
        await NavigateToK8sConfigAsync();

        // Switch to Single Cluster mode
        await SelectClusterModeAsync("Single");
        await Page.WaitForTimeoutAsync(500);

        // Check what selectors are available
        var namespaceRows = await Page.QuerySelectorAllAsync(".namespace-row");
        Console.WriteLine($"Namespace rows: {namespaceRows.Count}");

        var clusterRows = await Page.QuerySelectorAllAsync(".cluster-row");
        Console.WriteLine($"Cluster rows in single mode: {clusterRows.Count}");

        var tierInputs = await Page.QuerySelectorAllAsync(".tier-col input");
        Console.WriteLine($"Tier inputs: {tierInputs.Count}");

        // Try to get any inputs
        var allInputs = await Page.QuerySelectorAllAsync("input[type='number']");
        Console.WriteLine($"All number inputs: {allInputs.Count}");

        // Dump the config container HTML
        var html = await Page.InnerHTMLAsync(".config-tab-content");
        Console.WriteLine($"Config HTML (first 2500 chars): {html.Substring(0, Math.Min(2500, html.Length))}");
    }

    [Test]
    public async Task Debug_VMConfigPage()
    {
        await NavigateToVMConfigAsync();

        // Check what's on the VM config page
        var vmEnvRows = await Page.QuerySelectorAllAsync(".vm-env-row");
        Console.WriteLine($"VM env rows: {vmEnvRows.Count}");

        var roleChips = await Page.QuerySelectorAllAsync(".role-chip");
        Console.WriteLine($"Role chips: {roleChips.Count}");

        var calcButton = await Page.QuerySelectorAsync("button:has-text('Calculate')");
        if (calcButton != null)
        {
            var isDisabled = await calcButton.IsDisabledAsync();
            Console.WriteLine($"Calculate button disabled: {isDisabled}");
        }
        else
        {
            Console.WriteLine("Calculate button not found");
        }

        // Check current step
        var stepIndicator = await GetTextAsync(".step-indicator");
        Console.WriteLine($"Step indicator: {stepIndicator}");

        // Dump page HTML
        var html = await Page.InnerHTMLAsync(".wizard-content");
        Console.WriteLine($"Wizard content (first 2000 chars): {html.Substring(0, Math.Min(2000, html.Length))}");
    }
}
