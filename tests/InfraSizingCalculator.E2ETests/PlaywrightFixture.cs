using Microsoft.Playwright;

namespace InfraSizingCalculator.E2ETests;

/// <summary>
/// Base fixture for Playwright tests - handles browser setup and app URL
/// </summary>
public class PlaywrightFixture
{
    protected const string BaseUrl = "http://localhost:5062";

    protected IPlaywright Playwright { get; private set; } = null!;
    protected IBrowser Browser { get; private set; } = null!;
    protected IPage Page { get; private set; } = null!;

    [SetUp]
    public async Task SetUp()
    {
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
        Page = await Browser.NewPageAsync();
        // Set default timeout to 5 seconds
        Page.SetDefaultTimeout(5000);
    }

    [TearDown]
    public async Task TearDown()
    {
        await Page.CloseAsync();
        await Browser.CloseAsync();
        Playwright.Dispose();
    }

    /// <summary>
    /// Navigate to home page and wait for it to load
    /// </summary>
    protected async Task GoToHomeAsync()
    {
        await Page.GotoAsync(BaseUrl);
        // Wait for main content to be visible (new 3-panel layout)
        await Page.WaitForSelectorAsync(".main-content");
    }

    /// <summary>
    /// Click a selection card by its title. Note: Some cards auto-advance to next step on click.
    /// </summary>
    protected async Task SelectCardAsync(string title)
    {
        // Wait for page to be stable after any previous operations
        await Page.WaitForTimeoutAsync(500);

        // Find and click the card
        var cardSelector = $".selection-card:has-text('{title}')";
        await Page.WaitForSelectorAsync(cardSelector, new() { Timeout = 5000, State = WaitForSelectorState.Visible });
        await Page.ClickAsync(cardSelector);

        // Wait for Blazor to process the click (may auto-advance to next step)
        await Page.WaitForTimeoutAsync(800);
    }

    /// <summary>
    /// Click a technology card by its name
    /// </summary>
    protected async Task SelectTechCardAsync(string name)
    {
        await Page.WaitForTimeoutAsync(500);
        var cardSelector = $".tech-card:has-text('{name}')";
        await Page.WaitForSelectorAsync(cardSelector, new() { Timeout = 5000, State = WaitForSelectorState.Visible });
        await Page.ClickAsync(cardSelector);
        await Page.WaitForTimeoutAsync(800);
    }

    /// <summary>
    /// Click a distribution card
    /// </summary>
    protected async Task SelectDistroCardAsync()
    {
        await Page.WaitForTimeoutAsync(500);
        await Page.WaitForSelectorAsync(".distro-card", new() { Timeout = 5000, State = WaitForSelectorState.Visible });
        await Page.ClickAsync(".distro-card");
        await Page.WaitForTimeoutAsync(800);
    }

    /// <summary>
    /// Navigate to K8s configuration step (Platform -> Deployment -> Technology -> Distribution -> Config)
    /// By default lands in Multi-Cluster mode
    /// </summary>
    protected async Task NavigateToK8sConfigAsync()
    {
        await GoToHomeAsync();
        await SelectCardAsync("Native"); // -> Step 2
        await SelectCardAsync("Kubernetes"); // -> Step 3
        await SelectTechCardAsync(".NET"); // -> Step 4
        await SelectDistroCardAsync(); // -> Step 5 (Config)
        // Wait for config page to fully load
        await Page.WaitForSelectorAsync(".k8s-apps-config, .config-tabs-container", new() { Timeout = 10000 });
    }

    /// <summary>
    /// Navigate to K8s configuration step in Single Cluster (Shared) mode
    /// </summary>
    protected async Task NavigateToK8sConfigSingleClusterAsync()
    {
        await NavigateToK8sConfigAsync();
        await SelectClusterModeAsync("Single");
        await Page.WaitForSelectorAsync(".tier-card", new() { Timeout = 5000 });
    }

    /// <summary>
    /// Navigate to VM configuration step (Platform -> Deployment -> Technology -> Config)
    /// Lands on Step 4 (Configure) where roles can be selected
    /// </summary>
    protected async Task NavigateToVMConfigAsync()
    {
        await GoToHomeAsync();
        await SelectCardAsync("Native"); // -> Step 2
        await SelectCardAsync("Virtual Machines"); // -> Step 3
        await SelectTechCardAsync(".NET"); // -> Step 4 (Config for VMs)
        // Wait for VM config to load
        await Page.WaitForSelectorAsync(".config-tabs-container, .h-accordion-panel", new() { Timeout = 10000 });
    }

    /// <summary>
    /// For VM flows: Navigate from Config (Step 4) to Pricing (Step 5) and click Calculate
    /// VM flow: Step 4 has Next -> Step 5 has Calculate -> Step 6 Results
    /// </summary>
    protected async Task ClickVMCalculateAsync()
    {
        // Click Next to go from Step 4 (Configure) to Step 5 (Pricing)
        await ClickNextAsync();
        // Now click Calculate on the Pricing step
        await ClickCalculateAsync();
    }

    /// <summary>
    /// For K8s flows: Navigate from Configure (Step 5) to Pricing (Step 6) and click Calculate
    /// K8s flow: Step 5 (Configure) has Next -> Step 6 (Pricing) has Calculate -> Step 7 Results
    /// </summary>
    protected async Task ClickK8sCalculateAsync()
    {
        // Click Next to go from Step 5 (Configure) to Step 6 (Pricing)
        await ClickNextAsync();
        // Now click Calculate on the Pricing step
        await ClickCalculateAsync();
    }

    /// <summary>
    /// Click the Next button (waits for it to be enabled)
    /// </summary>
    protected async Task ClickNextAsync()
    {
        // Wait for the Next button to be enabled
        var nextButton = Page.Locator("button:has-text('Next'):not([disabled])");
        await nextButton.WaitForAsync(new() { Timeout = 5000, State = WaitForSelectorState.Visible });
        await nextButton.ClickAsync();
        await Page.WaitForTimeoutAsync(500); // Wait for step transition
    }

    /// <summary>
    /// Click the Back button
    /// </summary>
    protected async Task ClickBackAsync()
    {
        await Page.ClickAsync("button:has-text('Back')");
        await Page.WaitForTimeoutAsync(300);
    }

    /// <summary>
    /// Click Calculate button (waits for it to be enabled)
    /// </summary>
    protected async Task ClickCalculateAsync()
    {
        // Wait for the Calculate button to be enabled
        var calcButton = Page.Locator("button:has-text('Calculate'):not([disabled])");
        await calcButton.WaitForAsync(new() { Timeout = 5000, State = WaitForSelectorState.Visible });
        await calcButton.ClickAsync();
        await Page.WaitForTimeoutAsync(1000);
    }

    /// <summary>
    /// Select a cluster mode option
    /// </summary>
    protected async Task SelectClusterModeAsync(string mode)
    {
        await Page.ClickAsync($".mode-option:has-text('{mode}')");
        await Page.WaitForTimeoutAsync(300);
    }

    /// <summary>
    /// Select a scope from the dropdown (for single cluster mode)
    /// Use the value attribute (e.g., "Shared", "Prod", "Dev") not the display label
    /// </summary>
    protected async Task SelectClusterScopeAsync(string scopeValue)
    {
        await Page.SelectOptionAsync(".single-cluster-selector select", new SelectOptionValue { Value = scopeValue });
        await Page.WaitForTimeoutAsync(300);
    }

    /// <summary>
    /// Click on a tab
    /// </summary>
    protected async Task ClickTabAsync(string tabName)
    {
        await Page.ClickAsync($".config-tab:has-text('{tabName}')");
        await Page.WaitForTimeoutAsync(300);
    }

    /// <summary>
    /// Get text content of an element
    /// </summary>
    protected async Task<string> GetTextAsync(string selector)
    {
        var element = await Page.QuerySelectorAsync(selector);
        return element != null ? await element.TextContentAsync() ?? "" : "";
    }

    /// <summary>
    /// Check if an element is visible
    /// </summary>
    protected async Task<bool> IsVisibleAsync(string selector)
    {
        var element = await Page.QuerySelectorAsync(selector);
        return element != null && await element.IsVisibleAsync();
    }

    /// <summary>
    /// Set input value
    /// </summary>
    protected async Task SetInputValueAsync(string selector, string value)
    {
        await Page.FillAsync(selector, value);
    }

    /// <summary>
    /// Get input value
    /// </summary>
    protected async Task<string> GetInputValueAsync(string selector)
    {
        return await Page.InputValueAsync(selector);
    }
}
