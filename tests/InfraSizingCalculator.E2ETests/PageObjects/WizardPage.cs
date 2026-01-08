using Microsoft.Playwright;

namespace InfraSizingCalculator.E2ETests.PageObjects;

/// <summary>
/// Page object for wizard navigation and selection cards.
/// All wizard step interactions are centralized here.
/// </summary>
public class WizardPage
{
    private readonly IPage _page;
    private readonly int _defaultTimeout;
    private readonly bool _screenshotEveryStep;
    private readonly string _screenshotDir;
    private int _stepCounter;
    private string _testName;

    public WizardPage(IPage page, int defaultTimeout = 15000)
    {
        _page = page;
        _defaultTimeout = defaultTimeout;
        _screenshotEveryStep = Environment.GetEnvironmentVariable("SCREENSHOT_EVERY_STEP") == "true";
        _screenshotDir = Path.Combine(Directory.GetCurrentDirectory(), "Screenshots");
        _stepCounter = 0;
        _testName = "Wizard";
        Directory.CreateDirectory(_screenshotDir);
    }

    private async Task TakeStepScreenshotAsync(string stepName)
    {
        if (!_screenshotEveryStep) return;
        _stepCounter++;
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var fileName = $"{timestamp}_{_testName}_Step{_stepCounter:D2}_{stepName}.png";
        var filePath = Path.Combine(_screenshotDir, fileName);
        await _page.ScreenshotAsync(new PageScreenshotOptions { Path = filePath, FullPage = true });
        Console.WriteLine($"Screenshot saved: {filePath}");
    }

    #region Navigation

    public async Task GoToHomeAsync()
    {
        await _page.GotoAsync("http://localhost:5105", new PageGotoOptions { Timeout = 30000 });
        await _page.WaitForSelectorAsync(Locators.Layout.MainContent, new() { Timeout = 30000 });
        await WaitForStabilityAsync();
        await TakeStepScreenshotAsync("Home");
    }

    public async Task ClickNextAsync()
    {
        // Wait for button to be visible and enabled
        var nextButton = _page.Locator("button.btn-nav.primary:not([disabled])");
        await nextButton.WaitForAsync(new() { Timeout = _defaultTimeout, State = WaitForSelectorState.Visible });

        // Get current step before click
        var stepBefore = await _page.Locator(".step-indicator").TextContentAsync();
        Console.WriteLine($"Step before click: {stepBefore}");

        // Try multiple click approaches
        try
        {
            // First try: standard click with force
            await nextButton.ClickAsync(new() { Force = true });
        }
        catch
        {
            // Fallback: focus and Enter key
            await nextButton.FocusAsync();
            await _page.Keyboard.PressAsync("Enter");
        }

        await _page.WaitForTimeoutAsync(2000); // Wait for Blazor navigation

        // Verify step changed
        var stepAfter = await _page.Locator(".step-indicator").TextContentAsync();
        Console.WriteLine($"Step after click: {stepAfter}");

        await WaitForStabilityAsync();
        await TakeStepScreenshotAsync("AfterNext");
    }

    public async Task ClickBackAsync()
    {
        await _page.ClickAsync(Locators.Wizard.BackButton);
        await WaitForStabilityAsync();
    }

    public async Task ClickCalculateAsync()
    {
        var calcButton = _page.Locator(Locators.Wizard.CalculateButton);
        await calcButton.WaitForAsync(new() { Timeout = _defaultTimeout, State = WaitForSelectorState.Visible });
        await calcButton.ClickAsync();
        await _page.WaitForTimeoutAsync(1000); // Wait for calculation
        await TakeStepScreenshotAsync("AfterCalculate");
    }

    public async Task ClickNewCalculationAsync()
    {
        await _page.ClickAsync(Locators.Wizard.NewCalculationButton);
        await WaitForStabilityAsync();
    }

    #endregion

    #region Platform Selection (Step 1)

    public async Task SelectPlatformAsync(string platform)
    {
        await WaitForStabilityAsync();
        var selector = platform.ToLower() switch
        {
            "native" => Locators.SelectionCards.NativeCard,
            "low-code" or "lowcode" => Locators.SelectionCards.LowCodeCard,
            _ => $".selection-card:has-text('{platform}')"
        };
        await ClickCardAsync(selector);
        await TakeStepScreenshotAsync($"Platform_{platform}");
    }

    #endregion

    #region Deployment Selection (Step 2)

    public async Task SelectDeploymentAsync(string deployment)
    {
        await WaitForStabilityAsync();
        var selector = deployment.ToLower() switch
        {
            "kubernetes" or "k8s" => Locators.SelectionCards.KubernetesCard,
            "virtual machines" or "vm" or "vms" => Locators.SelectionCards.VirtualMachinesCard,
            _ => $".selection-card:has-text('{deployment}')"
        };
        await ClickCardAsync(selector);
        await TakeStepScreenshotAsync($"Deployment_{deployment.Replace(" ", "")}");
    }

    #endregion

    #region Technology Selection (Step 3)

    public async Task SelectTechnologyAsync(string technology)
    {
        await WaitForStabilityAsync();
        var selector = technology.ToLower() switch
        {
            ".net" or "dotnet" => Locators.SelectionCards.DotNetCard,
            "java" => Locators.SelectionCards.JavaCard,
            "node.js" or "nodejs" or "node" => Locators.SelectionCards.NodeJsCard,
            "python" => Locators.SelectionCards.PythonCard,
            "go" or "golang" => Locators.SelectionCards.GoCard,
            "mendix" => Locators.SelectionCards.MendixCard,
            "outsystems" => Locators.SelectionCards.OutSystemsCard,
            _ => $".tech-card:has-text('{technology}')"
        };
        await _page.WaitForSelectorAsync(selector, new() { Timeout = _defaultTimeout, State = WaitForSelectorState.Visible });
        await _page.ClickAsync(selector);
        await WaitForStabilityAsync();
        await TakeStepScreenshotAsync($"Technology_{technology.Replace(".", "").Replace(" ", "")}");
    }

    #endregion

    #region Distribution Selection (Step 4 for K8s)

    public async Task SelectDistributionAsync(string? distroName = null)
    {
        await WaitForStabilityAsync();
        await _page.WaitForSelectorAsync(Locators.SelectionCards.DistroCard, new() { Timeout = _defaultTimeout, State = WaitForSelectorState.Visible });

        if (string.IsNullOrEmpty(distroName))
        {
            // Click first available distro
            await _page.ClickAsync(Locators.SelectionCards.DistroCard);
        }
        else
        {
            await _page.ClickAsync($".distro-card:has-text('{distroName}')");
        }
        await WaitForStabilityAsync();
        await TakeStepScreenshotAsync($"Distribution_{distroName ?? "Default"}");
    }

    #endregion

    #region Mendix Specific Selections

    public async Task SelectMendixCategoryAsync(string category)
    {
        await WaitForStabilityAsync();
        var selector = category.ToLower() switch
        {
            "private cloud" => Locators.SelectionCards.MendixPrivateCloud,
            "mendix cloud" => Locators.SelectionCards.MendixCloud,
            "other kubernetes" or "other k8s" => Locators.SelectionCards.MendixOtherK8s,
            "server" => Locators.SelectionCards.MendixServer,
            "stackit" => Locators.SelectionCards.MendixStackIT,
            "sap btp" => Locators.SelectionCards.MendixSapBtp,
            _ => $".mendix-category-card:has-text('{category}')"
        };
        await _page.WaitForSelectorAsync(selector, new() { Timeout = _defaultTimeout, State = WaitForSelectorState.Visible });
        await _page.ClickAsync(selector);
        await WaitForStabilityAsync();
    }

    public async Task SelectMendixProviderAsync(string provider)
    {
        await WaitForStabilityAsync();
        var selector = provider.ToLower() switch
        {
            "mendix azure" or "azure" => Locators.SelectionCards.MendixAzure,
            "amazon eks" or "eks" => Locators.SelectionCards.AmazonEKS,
            "azure aks" or "aks" => Locators.SelectionCards.AzureAKS,
            "google gke" or "gke" => Locators.SelectionCards.GoogleGKE,
            "openshift" => Locators.SelectionCards.OpenShift,
            "rancher" => Locators.SelectionCards.Rancher,
            "k3s" => Locators.SelectionCards.K3s,
            "generic k8s" or "generic" => Locators.SelectionCards.GenericK8s,
            _ => $".mendix-option-card:has-text('{provider}')"
        };
        await _page.WaitForSelectorAsync(selector, new() { Timeout = _defaultTimeout, State = WaitForSelectorState.Visible });
        await _page.ClickAsync(selector);
        await WaitForStabilityAsync();
    }

    #endregion

    #region Complete Path Navigation

    /// <summary>
    /// Navigate complete path: Native -> K8s -> Technology -> Distribution -> Config
    /// </summary>
    public async Task NavigateToNativeK8sConfigAsync(string technology, string? distribution = null)
    {
        await GoToHomeAsync();
        await SelectPlatformAsync("Native");
        await SelectDeploymentAsync("Kubernetes");
        await SelectTechnologyAsync(technology);
        await SelectDistributionAsync(distribution);
        await WaitForConfigPageAsync();
    }

    /// <summary>
    /// Navigate complete path: Native -> VM -> Technology -> Config
    /// </summary>
    public async Task NavigateToNativeVMConfigAsync(string technology)
    {
        await GoToHomeAsync();
        await SelectPlatformAsync("Native");
        await SelectDeploymentAsync("Virtual Machines");
        await SelectTechnologyAsync(technology);
        await WaitForConfigPageAsync();
    }

    /// <summary>
    /// Navigate complete path: Low-Code -> K8s -> Mendix -> Category -> Provider -> Config
    /// </summary>
    public async Task NavigateToMendixK8sConfigAsync(string category, string provider)
    {
        await GoToHomeAsync();
        await SelectPlatformAsync("Low-Code");
        await SelectDeploymentAsync("Kubernetes");
        await SelectTechnologyAsync("Mendix");
        await SelectMendixCategoryAsync(category);
        await SelectMendixProviderAsync(provider);
        await WaitForConfigPageAsync();
    }

    /// <summary>
    /// Navigate complete path: Low-Code -> VM -> Mendix -> Category -> Config
    /// </summary>
    public async Task NavigateToMendixVMConfigAsync(string category)
    {
        await GoToHomeAsync();
        await SelectPlatformAsync("Low-Code");
        await SelectDeploymentAsync("Virtual Machines");
        await SelectTechnologyAsync("Mendix");
        await SelectMendixCategoryAsync(category);
        await WaitForConfigPageAsync();
    }

    /// <summary>
    /// Navigate complete path: Low-Code -> VM -> OutSystems -> Config
    /// </summary>
    public async Task NavigateToOutSystemsVMConfigAsync()
    {
        await GoToHomeAsync();
        await SelectPlatformAsync("Low-Code");
        await SelectDeploymentAsync("Virtual Machines");
        await SelectTechnologyAsync("OutSystems");
        await WaitForConfigPageAsync();
    }

    #endregion

    #region Helpers

    private async Task ClickCardAsync(string selector)
    {
        await _page.WaitForSelectorAsync(selector, new() { Timeout = _defaultTimeout, State = WaitForSelectorState.Visible });
        await _page.ClickAsync(selector);
        await WaitForStabilityAsync();
    }

    private async Task WaitForStabilityAsync()
    {
        await _page.WaitForTimeoutAsync(800); // Wait for Blazor to process
    }

    private async Task WaitForConfigPageAsync()
    {
        // Wait for any config UI element - K8s tabs, accordion, or VM config
        await _page.WaitForSelectorAsync(
            $"{Locators.K8sConfig.ConfigTabsContainer}, {Locators.Shared.Accordion}, {Locators.VMConfig.ServerRolesPanel}, .config-layout",
            new() { Timeout = _defaultTimeout });
        await TakeStepScreenshotAsync("ConfigPage");
    }

    #endregion

    #region Verification

    public async Task<bool> IsOnResultsPageAsync()
    {
        var element = await _page.QuerySelectorAsync(Locators.Results.ResultsContainer);
        return element != null && await element.IsVisibleAsync();
    }

    public async Task<int> GetCurrentStepAsync()
    {
        var currentStep = await _page.QuerySelectorAsync(Locators.Sidebar.NavStepCurrent);
        if (currentStep == null) return 0;

        var text = await currentStep.TextContentAsync();
        // Extract step number from text
        return int.TryParse(text?.Trim().Split(' ').FirstOrDefault(), out var step) ? step : 0;
    }

    #endregion
}
