using Microsoft.Playwright;

namespace InfraSizingCalculator.E2ETests;

/// <summary>
/// Base fixture for Playwright tests - handles browser setup and app URL
///
/// Tests run HEADED (visible browser) by default.
///
/// Environment Variables:
/// - PLAYWRIGHT_HEADLESS=true : Run browser in headless mode (for CI)
/// - PLAYWRIGHT_SLOW_MO=ms    : Add delay between operations (e.g., 500)
/// - SCREENSHOT_ON_FAILURE=true : Automatically capture screenshot on test failure
/// - SCREENSHOT_EVERY_STEP=true : Take screenshot after every navigation step
/// - PLAYWRIGHT_WINDOW_X=n    : X position for browser window (e.g., 2000 for external monitor)
/// - PLAYWRIGHT_WINDOW_Y=n    : Y position for browser window (default: 100)
/// </summary>
public class PlaywrightFixture
{
    protected const string BaseUrl = "http://localhost:5105";
    protected static readonly string ScreenshotDir = Path.Combine(
        Directory.GetCurrentDirectory(), "Screenshots");

    protected IPlaywright Playwright { get; private set; } = null!;
    protected IBrowser Browser { get; private set; } = null!;
    protected IPage Page { get; private set; } = null!;

    // Current test name for screenshot naming
    private string _currentTestName = "";
    private int _stepCounter = 0;
    private bool _screenshotEveryStep = false;

    [SetUp]
    public async Task SetUp()
    {
        // Always run headed (visible browser) by default
        // Set PLAYWRIGHT_HEADLESS=true to override and run headless
        var headless = Environment.GetEnvironmentVariable("PLAYWRIGHT_HEADLESS") == "true";
        var slowMo = int.TryParse(Environment.GetEnvironmentVariable("PLAYWRIGHT_SLOW_MO"), out var ms) ? ms : 0;

        // Window position for external monitors (default: position on external monitor at x=2000)
        var windowX = int.TryParse(Environment.GetEnvironmentVariable("PLAYWRIGHT_WINDOW_X"), out var x) ? x : 2000;
        var windowY = int.TryParse(Environment.GetEnvironmentVariable("PLAYWRIGHT_WINDOW_Y"), out var y) ? y : 100;

        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();

        // Build launch arguments for window positioning (always add for headed mode)
        var launchArgs = new List<string>();
        if (!headless)
        {
            launchArgs.Add($"--window-position={windowX},{windowY}");
            launchArgs.Add("--window-size=1920,1080");
        }

        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = headless,  // Default: false (headed)
            SlowMo = slowMo,
            Args = launchArgs.ToArray()
        });
        Page = await Browser.NewPageAsync(new BrowserNewPageOptions
        {
            ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
        });
        // Set default timeout to 15 seconds for CI/slow machines
        Page.SetDefaultTimeout(15000);

        // Store test name for screenshot naming
        _currentTestName = TestContext.CurrentContext.Test.Name;
        _stepCounter = 0;
        _screenshotEveryStep = Environment.GetEnvironmentVariable("SCREENSHOT_EVERY_STEP") == "true";

        // Ensure screenshot directory exists
        Directory.CreateDirectory(ScreenshotDir);
    }

    /// <summary>
    /// Take a screenshot after a navigation step (if SCREENSHOT_EVERY_STEP=true)
    /// </summary>
    private async Task AutoScreenshotAsync(string stepDescription)
    {
        if (_screenshotEveryStep)
        {
            _stepCounter++;
            await TakeScreenshotAsync($"{_currentTestName}_Step{_stepCounter:D2}_{stepDescription}");
        }
    }

    [TearDown]
    public async Task TearDown()
    {
        // Capture screenshot on failure if enabled
        if (Environment.GetEnvironmentVariable("SCREENSHOT_ON_FAILURE") == "true" &&
            TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Failed)
        {
            await TakeScreenshotAsync($"FAILED_{_currentTestName}");
        }

        await Page.CloseAsync();
        await Browser.CloseAsync();
        Playwright.Dispose();
    }

    /// <summary>
    /// Take a screenshot and save to Screenshots directory
    /// </summary>
    protected async Task<string> TakeScreenshotAsync(string name)
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var fileName = $"{timestamp}_{name.Replace(" ", "_")}.png";
        var filePath = Path.Combine(ScreenshotDir, fileName);

        await Page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = filePath,
            FullPage = true
        });

        TestContext.WriteLine($"Screenshot saved: {filePath}");
        return filePath;
    }

    /// <summary>
    /// Take a screenshot with optional description for visual validation
    /// Always takes screenshot since tests run headed by default
    /// </summary>
    protected async Task VisualCheckpointAsync(string stepName)
    {
        // Always take screenshot (tests run headed by default)
        await TakeScreenshotAsync($"{_currentTestName}_{stepName}");
    }

    /// <summary>
    /// Navigate to home page and wait for it to load
    /// </summary>
    protected async Task GoToHomeAsync()
    {
        await Page.GotoAsync(BaseUrl, new PageGotoOptions { Timeout = 30000 });
        // Wait for main content to be visible (new 3-panel layout)
        await Page.WaitForSelectorAsync(".main-content", new() { Timeout = 30000 });
        await AutoScreenshotAsync("Home");
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
        await Page.WaitForSelectorAsync(cardSelector, new() { Timeout = 15000, State = WaitForSelectorState.Visible });
        await Page.ClickAsync(cardSelector);

        // Wait for Blazor to process the click (may auto-advance to next step)
        await Page.WaitForTimeoutAsync(800);
        await AutoScreenshotAsync($"Selected_{title.Replace(" ", "")}");
    }

    /// <summary>
    /// Click a technology card by its name
    /// </summary>
    protected async Task SelectTechCardAsync(string name)
    {
        await Page.WaitForTimeoutAsync(500);
        var cardSelector = $".tech-card:has-text('{name}')";
        await Page.WaitForSelectorAsync(cardSelector, new() { Timeout = 15000, State = WaitForSelectorState.Visible });
        await Page.ClickAsync(cardSelector);
        await Page.WaitForTimeoutAsync(800);
        await AutoScreenshotAsync($"Tech_{name.Replace(".", "").Replace(" ", "")}");
    }

    /// <summary>
    /// Click a distribution card
    /// </summary>
    protected async Task SelectDistroCardAsync()
    {
        await Page.WaitForTimeoutAsync(500);
        await Page.WaitForSelectorAsync(".distro-card", new() { Timeout = 15000, State = WaitForSelectorState.Visible });
        await Page.ClickAsync(".distro-card");
        await Page.WaitForTimeoutAsync(800);
        await AutoScreenshotAsync("Distro_Selected");
    }

    /// <summary>
    /// Select a Mendix deployment category card (main categories).
    /// Options: "Mendix Cloud", "Private Cloud", "Other Kubernetes" (for K8s)
    /// Or: "Server", "StackIT", "SAP BTP" (for VM)
    /// </summary>
    protected async Task SelectMendixCategoryCardAsync(string categoryName)
    {
        await Page.WaitForTimeoutAsync(500);
        var cardSelector = $".mendix-category-card:has-text('{categoryName}')";
        await Page.WaitForSelectorAsync(cardSelector, new() { Timeout = 15000, State = WaitForSelectorState.Visible });
        await Page.ClickAsync(cardSelector);
        await Page.WaitForTimeoutAsync(800);
        await AutoScreenshotAsync($"MendixCategory_{categoryName.Replace(" ", "")}");
    }

    /// <summary>
    /// Select a Mendix provider option card (sub-options after selecting category).
    /// For Private Cloud: "Mendix Azure", "Amazon EKS", "Azure AKS", "Google GKE", "OpenShift"
    /// For Other K8s: "Rancher", "K3s", "Generic K8s"
    /// </summary>
    protected async Task SelectMendixProviderCardAsync(string providerName)
    {
        await Page.WaitForTimeoutAsync(500);
        var cardSelector = $".mendix-option-card:has-text('{providerName}')";
        await Page.WaitForSelectorAsync(cardSelector, new() { Timeout = 15000, State = WaitForSelectorState.Visible });
        await Page.ClickAsync(cardSelector);
        await Page.WaitForTimeoutAsync(800);
        await AutoScreenshotAsync($"MendixProvider_{providerName.Replace(" ", "")}");
    }

    /// <summary>
    /// Navigate to Mendix K8s configuration (Private Cloud -> Azure)
    /// Low-Code -> Kubernetes -> Mendix -> Private Cloud -> Azure -> Config
    /// </summary>
    protected async Task NavigateToMendixK8sConfigAsync()
    {
        await GoToHomeAsync();
        await SelectCardAsync("Low-Code");
        await SelectCardAsync("Kubernetes");
        await SelectTechCardAsync("Mendix");
        await SelectMendixCategoryCardAsync("Private Cloud");
        await SelectMendixProviderCardAsync("Mendix Azure");
        // Wait for config page to fully load
        await Page.WaitForSelectorAsync(".k8s-apps-config, .config-tabs-container", new() { Timeout = 10000 });
    }

    /// <summary>
    /// Navigate to Mendix VM configuration (with Server deployment type)
    /// Low-Code -> Virtual Machines -> Mendix -> Server -> Config
    /// </summary>
    protected async Task NavigateToMendixVMConfigAsync()
    {
        await GoToHomeAsync();
        await SelectCardAsync("Low-Code");
        await SelectCardAsync("Virtual Machines");
        await SelectTechCardAsync("Mendix");
        await SelectMendixCategoryCardAsync("Server");
        // Wait for config page to fully load
        await Page.WaitForSelectorAsync(".config-tabs-container", new() { Timeout = 10000 });
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
        await Page.WaitForSelectorAsync(".tier-card", new() { Timeout = 15000 });
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
        await nextButton.WaitForAsync(new() { Timeout = 15000, State = WaitForSelectorState.Visible });
        await nextButton.ClickAsync();
        await Page.WaitForTimeoutAsync(500); // Wait for step transition
        await AutoScreenshotAsync("AfterNext");
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
        await calcButton.WaitForAsync(new() { Timeout = 15000, State = WaitForSelectorState.Visible });
        await calcButton.ClickAsync();
        await Page.WaitForTimeoutAsync(1000);
        await AutoScreenshotAsync("AfterCalculate");
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
    /// Click on a configuration tab
    /// </summary>
    protected async Task ClickTabAsync(string tabName)
    {
        await Page.ClickAsync($".config-tab:has-text('{tabName}')");
        await Page.WaitForTimeoutAsync(300);
        await AutoScreenshotAsync($"Tab_{tabName.Replace(" ", "").Replace("&", "")}");
    }

    /// <summary>
    /// Click on a results tab (Sizing Details, Cost Breakdown, Growth Planning, Insights)
    /// Uses button.nav-item selector matching LeftSidebar.razor structure
    /// </summary>
    protected async Task ClickResultsTabAsync(string tabName)
    {
        var tabSelector = $"button.nav-item:has-text('{tabName}')";
        await Page.WaitForSelectorAsync(tabSelector, new() { Timeout = 5000, State = WaitForSelectorState.Visible });
        await Page.ClickAsync(tabSelector);
        await Page.WaitForTimeoutAsync(500);
        await AutoScreenshotAsync($"ResultsTab_{tabName.Replace(" ", "")}");
    }

    /// <summary>
    /// Check if results page is visible (use after Calculate)
    /// </summary>
    protected async Task<bool> IsResultsPageVisibleAsync()
    {
        return await IsVisibleAsync(".results-panel, .sizing-results-view, .results-table-container");
    }

    /// <summary>
    /// Navigate through all results tabs and take screenshots
    /// </summary>
    protected async Task VerifyAllResultsTabsAsync()
    {
        // Verify we're on results page
        Assert.That(await IsResultsPageVisibleAsync(), Is.True, "Should be on results page");

        // Click through each results tab
        var resultTabs = new[] { "Sizing Details", "Cost Breakdown", "Growth Planning", "Insights" };
        foreach (var tab in resultTabs)
        {
            try
            {
                await ClickResultsTabAsync(tab);
                await Page.WaitForTimeoutAsync(300);
            }
            catch
            {
                // Tab might not exist for this configuration - continue
            }
        }
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
