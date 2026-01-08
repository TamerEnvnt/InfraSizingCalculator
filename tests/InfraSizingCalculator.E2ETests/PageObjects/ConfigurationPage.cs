using Microsoft.Playwright;

namespace InfraSizingCalculator.E2ETests.PageObjects;

/// <summary>
/// Page object for K8s and VM configuration pages.
/// Handles cluster modes, app counts, node specs, HA/DR settings, and all config tabs.
/// </summary>
public class ConfigurationPage
{
    private readonly IPage _page;
    private readonly int _defaultTimeout;
    private readonly bool _screenshotEveryStep;
    private readonly string _screenshotDir;
    private int _stepCounter;
    private string _testName;

    public ConfigurationPage(IPage page, int defaultTimeout = 15000)
    {
        _page = page;
        _defaultTimeout = defaultTimeout;
        _screenshotEveryStep = Environment.GetEnvironmentVariable("SCREENSHOT_EVERY_STEP") == "true";
        _screenshotDir = Path.Combine(Directory.GetCurrentDirectory(), "Screenshots");
        _stepCounter = 0;
        _testName = "Config";
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

    #region Tab Navigation

    public async Task ClickApplicationsTabAsync()
    {
        await _page.ClickAsync(Locators.K8sConfig.ApplicationsTab);
        await WaitForStabilityAsync();
        await TakeStepScreenshotAsync("ApplicationsTab");
    }

    public async Task ClickNodeSpecsTabAsync()
    {
        await _page.ClickAsync(Locators.K8sConfig.NodeSpecsTab);
        await WaitForStabilityAsync();
        await TakeStepScreenshotAsync("NodeSpecsTab");
    }

    public async Task ClickSettingsTabAsync()
    {
        await _page.ClickAsync(Locators.K8sConfig.SettingsTab);
        await WaitForStabilityAsync();
        await TakeStepScreenshotAsync("SettingsTab");
    }

    public async Task ClickServerRolesTabAsync()
    {
        await _page.ClickAsync(Locators.VMConfig.ServerRolesTab);
        await WaitForStabilityAsync();
        await TakeStepScreenshotAsync("ServerRolesTab");
    }

    public async Task ClickHADRTabAsync()
    {
        // Use JavaScript pointer events to trigger Blazor's @onclick handler
        // Standard Playwright clicks may not trigger Blazor Server's SignalR-based events
        var hadrTab = _page.Locator(Locators.VMConfig.HADRTab).First;
        if (await hadrTab.IsVisibleAsync())
        {
            await hadrTab.ScrollIntoViewIfNeededAsync();
            await _page.WaitForTimeoutAsync(200);

            // Dispatch pointer events through JavaScript
            await hadrTab.EvaluateAsync(@"element => {
                const rect = element.getBoundingClientRect();
                const x = rect.left + rect.width / 2;
                const y = rect.top + rect.height / 2;

                // Dispatch pointerdown
                element.dispatchEvent(new PointerEvent('pointerdown', {
                    bubbles: true, cancelable: true, pointerId: 1, pointerType: 'mouse',
                    clientX: x, clientY: y, button: 0, buttons: 1, view: window
                }));

                // Dispatch pointerup
                element.dispatchEvent(new PointerEvent('pointerup', {
                    bubbles: true, cancelable: true, pointerId: 1, pointerType: 'mouse',
                    clientX: x, clientY: y, button: 0, buttons: 0, view: window
                }));

                // Dispatch click
                element.dispatchEvent(new MouseEvent('click', {
                    bubbles: true, cancelable: true, clientX: x, clientY: y, button: 0, view: window
                }));

                console.log('Dispatched pointer events to HA/DR tab');
            }");
        }
        await WaitForStabilityAsync();
        await TakeStepScreenshotAsync("HADRTab");
    }

    public async Task<bool> IsTabVisibleAsync(string tabName)
    {
        var selector = $".config-tab:has-text('{tabName}')";
        var element = await _page.QuerySelectorAsync(selector);
        return element != null && await element.IsVisibleAsync();
    }

    #endregion

    #region K8s Cluster Mode Selection

    public async Task SelectMultiClusterModeAsync()
    {
        await _page.ClickAsync(Locators.K8sConfig.MultiClusterMode);
        await WaitForStabilityAsync();
        await TakeStepScreenshotAsync("MultiClusterMode");
    }

    public async Task SelectSingleClusterModeAsync()
    {
        await _page.ClickAsync(Locators.K8sConfig.SingleClusterMode);
        await WaitForStabilityAsync();
        await TakeStepScreenshotAsync("SingleClusterMode");
    }

    public async Task SelectPerEnvironmentModeAsync()
    {
        await _page.ClickAsync(Locators.K8sConfig.PerEnvMode);
        await WaitForStabilityAsync();
        await TakeStepScreenshotAsync("PerEnvMode");
    }

    public async Task<string?> GetActiveModeAsync()
    {
        var activeMode = await _page.QuerySelectorAsync(Locators.K8sConfig.ActiveMode);
        return activeMode != null ? await activeMode.TextContentAsync() : null;
    }

    public async Task<bool> IsModeVisibleAsync()
    {
        var element = await _page.QuerySelectorAsync(Locators.K8sConfig.ClusterModeSelector);
        return element != null && await element.IsVisibleAsync();
    }

    #endregion

    #region Single Cluster - Tier Selection

    public async Task SelectTierAsync(string tierName)
    {
        var selector = $".tier-card:has-text('{tierName}')";
        await _page.WaitForSelectorAsync(selector, new() { Timeout = _defaultTimeout });
        await _page.ClickAsync(selector);
        await WaitForStabilityAsync();
        await TakeStepScreenshotAsync($"SelectTier_{tierName}");
    }

    public async Task SelectScopeAsync(string scopeValue)
    {
        await _page.SelectOptionAsync(Locators.K8sConfig.ScopeDropdown, new SelectOptionValue { Value = scopeValue });
        await WaitForStabilityAsync();
        await TakeStepScreenshotAsync($"SelectScope_{scopeValue}");
    }

    public async Task<bool> AreTierCardsVisibleAsync()
    {
        var element = await _page.QuerySelectorAsync(Locators.K8sConfig.TierCard);
        return element != null && await element.IsVisibleAsync();
    }

    public async Task<int> GetTierCardCountAsync()
    {
        var cards = await _page.QuerySelectorAllAsync(Locators.K8sConfig.TierCard);
        return cards.Count;
    }

    #endregion

    #region Environment App Configuration

    /// <summary>
    /// Sets app count for an environment in multi-cluster mode (accordion panels).
    /// Each environment has tier panels with tier inputs.
    /// </summary>
    public async Task SetAppCountForEnvironmentAsync(string environment, int count, string tier = "Standard")
    {
        // Multi-cluster mode uses accordion panels with environment-specific classes
        // Format: .h-accordion-panel.env-dev, .h-accordion-panel.env-test, etc.
        var envClass = environment.ToLowerInvariant();
        var panelSelector = $".h-accordion-panel.env-{envClass}";

        // First try environment-specific panel
        var panel = await _page.QuerySelectorAsync(panelSelector);
        if (panel == null)
        {
            // Fallback to text-based selection
            panelSelector = $".h-accordion-panel:has-text('{environment}')";
            panel = await _page.QuerySelectorAsync(panelSelector);
        }

        if (panel != null)
        {
            // Find the tier input within the panel
            // The input element itself has class 'tier-input', not a parent container
            var tierInput = await panel.QuerySelectorAsync($".tier-panel:has-text('{tier}') input.tier-input, input.tier-input");
            if (tierInput != null)
            {
                await tierInput.FillAsync(count.ToString());
                await WaitForStabilityAsync();
                await TakeStepScreenshotAsync($"AppCount_{environment}_{tier}_{count}");
            }
        }
    }

    /// <summary>
    /// Sets app count for a specific tier in single-cluster mode.
    /// In single-cluster: .tier-card has an input element inside
    /// In multi-cluster panels: .tier-panel has input.tier-input
    /// </summary>
    public async Task SetAppCountForTierAsync(string tier, int count)
    {
        var selector = $".tier-card:has-text('{tier}') input, .tier-panel:has-text('{tier}') input.tier-input";
        await _page.FillAsync(selector, count.ToString());
        await WaitForStabilityAsync();
        await TakeStepScreenshotAsync($"TierAppCount_{tier}_{count}");
    }

    public async Task SetPodsPerAppAsync(string environment, int pods)
    {
        var envClass = environment.ToLowerInvariant();
        var selector = $".h-accordion-panel.env-{envClass} .pods-per-app input, .environment-app-card:has-text('{environment}') .pods-per-app input";
        var input = await _page.QuerySelectorAsync(selector);
        if (input != null)
        {
            await _page.FillAsync(selector, pods.ToString());
            await WaitForStabilityAsync();
            await TakeStepScreenshotAsync($"PodsPerApp_{environment}_{pods}");
        }
    }

    public async Task<string> GetAppCountForEnvironmentAsync(string environment, string tier = "Standard")
    {
        var envClass = environment.ToLowerInvariant();
        var panelSelector = $".h-accordion-panel.env-{envClass}";

        var panel = await _page.QuerySelectorAsync(panelSelector);
        if (panel == null)
        {
            panelSelector = $".h-accordion-panel:has-text('{environment}')";
            panel = await _page.QuerySelectorAsync(panelSelector);
        }

        if (panel != null)
        {
            // The input element itself has class 'tier-input', not a parent container
            var tierInput = await panel.QuerySelectorAsync($".tier-panel:has-text('{tier}') input.tier-input, input.tier-input");
            if (tierInput != null)
            {
                return await tierInput.InputValueAsync() ?? "0";
            }
        }
        return "0";
    }

    /// <summary>
    /// Gets count of environment panels (accordion panels in multi-cluster mode).
    /// </summary>
    public async Task<int> GetEnvironmentCardCountAsync()
    {
        // Try accordion panels first (multi-cluster mode)
        var panels = await _page.QuerySelectorAllAsync(Locators.K8sConfig.EnvAccordionPanel);
        if (panels.Count > 0) return panels.Count;

        // Fallback to tier cards (single-cluster mode)
        var cards = await _page.QuerySelectorAllAsync(Locators.K8sConfig.TierCard);
        if (cards.Count > 0) return cards.Count;

        // Last fallback to generic selector
        var generic = await _page.QuerySelectorAllAsync(Locators.K8sConfig.EnvironmentAppCard);
        return generic.Count;
    }

    /// <summary>
    /// Gets all tier input values from a specific environment panel.
    /// </summary>
    public async Task<Dictionary<string, string>> GetTierInputsForEnvironmentAsync(string environment)
    {
        var result = new Dictionary<string, string>();
        var envClass = environment.ToLowerInvariant();
        var panelSelector = $".h-accordion-panel.env-{envClass}, .h-accordion-panel:has-text('{environment}')";

        var panel = await _page.QuerySelectorAsync(panelSelector);
        if (panel != null)
        {
            var tierPanels = await panel.QuerySelectorAllAsync(".tier-panel");
            foreach (var tierPanel in tierPanels)
            {
                var tierName = await tierPanel.QuerySelectorAsync(".tier-name")
                    ?? await tierPanel.QuerySelectorAsync("label");
                // The input element itself has class 'tier-input'
                var input = await tierPanel.QuerySelectorAsync("input.tier-input");

                if (tierName != null && input != null)
                {
                    var name = await tierName.TextContentAsync() ?? "Unknown";
                    var value = await input.InputValueAsync() ?? "0";
                    result[name.Trim()] = value;
                }
            }
        }
        return result;
    }

    #endregion

    #region Environment Sliders

    public async Task SetSliderValueAsync(string environment, int value)
    {
        var selector = $".environment-slider:has-text('{environment}') input[type='range']";
        await _page.EvaluateAsync($"document.querySelector(\"{selector}\").value = {value}");
        await _page.DispatchEventAsync(selector, "input");
        await _page.DispatchEventAsync(selector, "change");
        await WaitForStabilityAsync();
        await TakeStepScreenshotAsync($"Slider_{environment}_{value}");
    }

    public async Task<string?> GetSliderValueAsync(string environment)
    {
        var selector = $".environment-slider:has-text('{environment}') .slider-value";
        var element = await _page.QuerySelectorAsync(selector);
        return element != null ? await element.TextContentAsync() : null;
    }

    #endregion

    #region Node Specs Configuration

    public async Task SetCpuAsync(string value)
    {
        await _page.FillAsync(Locators.K8sConfig.CpuInput, value);
        await WaitForStabilityAsync();
        await TakeStepScreenshotAsync($"SetCpu_{value}");
    }

    public async Task SetRamAsync(string value)
    {
        await _page.FillAsync(Locators.K8sConfig.RamInput, value);
        await WaitForStabilityAsync();
        await TakeStepScreenshotAsync($"SetRam_{value}");
    }

    public async Task SetDiskAsync(string value)
    {
        await _page.FillAsync(Locators.K8sConfig.DiskInput, value);
        await WaitForStabilityAsync();
        await TakeStepScreenshotAsync($"SetDisk_{value}");
    }

    public async Task<string> GetCpuAsync()
    {
        return await _page.InputValueAsync(Locators.K8sConfig.CpuInput);
    }

    public async Task<string> GetRamAsync()
    {
        return await _page.InputValueAsync(Locators.K8sConfig.RamInput);
    }

    public async Task<string> GetDiskAsync()
    {
        return await _page.InputValueAsync(Locators.K8sConfig.DiskInput);
    }

    public async Task<bool> IsNodeSpecsPanelVisibleAsync()
    {
        var element = await _page.QuerySelectorAsync(Locators.K8sConfig.NodeSpecsPanel);
        return element != null && await element.IsVisibleAsync();
    }

    #endregion

    #region HA/DR Settings

    public async Task ToggleHAAsync()
    {
        await _page.ClickAsync(Locators.K8sConfig.HAToggle);
        await WaitForStabilityAsync();
        await TakeStepScreenshotAsync("ToggleHA");
    }

    public async Task ToggleDRAsync()
    {
        await _page.ClickAsync(Locators.K8sConfig.DRToggle);
        await WaitForStabilityAsync();
        await TakeStepScreenshotAsync("ToggleDR");
    }

    public async Task<bool> IsHAEnabledAsync()
    {
        var checkbox = await _page.QuerySelectorAsync(Locators.K8sConfig.HAToggle);
        return checkbox != null && await checkbox.IsCheckedAsync();
    }

    public async Task<bool> IsDREnabledAsync()
    {
        var checkbox = await _page.QuerySelectorAsync(Locators.K8sConfig.DRToggle);
        return checkbox != null && await checkbox.IsCheckedAsync();
    }

    public async Task<bool> IsHADRPanelVisibleAsync()
    {
        var element = await _page.QuerySelectorAsync(Locators.K8sConfig.HADRPanel);
        return element != null && await element.IsVisibleAsync();
    }

    #endregion

    #region VM Server Roles

    public async Task ToggleServerRoleAsync(string roleName)
    {
        var selector = $".role-card:has-text('{roleName}') input[type='checkbox']";
        await _page.ClickAsync(selector);
        await WaitForStabilityAsync();
        await TakeStepScreenshotAsync($"ToggleRole_{roleName}");
    }

    public async Task SetServerRoleCountAsync(string roleName, int count)
    {
        var selector = $".role-card:has-text('{roleName}') .role-count input";
        await _page.FillAsync(selector, count.ToString());
        await WaitForStabilityAsync();
        await TakeStepScreenshotAsync($"RoleCount_{roleName}_{count}");
    }

    public async Task<bool> IsServerRoleEnabledAsync(string roleName)
    {
        var selector = $".role-card:has-text('{roleName}') input[type='checkbox']";
        var checkbox = await _page.QuerySelectorAsync(selector);
        return checkbox != null && await checkbox.IsCheckedAsync();
    }

    public async Task<int> GetServerRoleCardCountAsync()
    {
        var cards = await _page.QuerySelectorAllAsync(Locators.VMConfig.RoleCard);
        return cards.Count;
    }

    public async Task<bool> IsServerRolesPanelVisibleAsync()
    {
        var element = await _page.QuerySelectorAsync(Locators.VMConfig.ServerRolesPanel);
        return element != null && await element.IsVisibleAsync();
    }

    #endregion

    #region Accordion Interactions

    public async Task ExpandAccordionPanelAsync(string panelTitle)
    {
        var selector = $".h-accordion-panel:has(.accordion-header:has-text('{panelTitle}'))";
        var panel = await _page.QuerySelectorAsync(selector);
        if (panel != null)
        {
            var header = await panel.QuerySelectorAsync(".accordion-header");
            if (header != null)
            {
                await header.ClickAsync();
                await WaitForStabilityAsync();
                await TakeStepScreenshotAsync($"Accordion_{panelTitle.Replace(" ", "")}");
            }
        }
    }

    public async Task<bool> IsAccordionPanelExpandedAsync(string panelTitle)
    {
        var selector = $".h-accordion-panel.expanded:has(.accordion-header:has-text('{panelTitle}'))";
        var element = await _page.QuerySelectorAsync(selector);
        return element != null;
    }

    public async Task<int> GetAccordionPanelCountAsync()
    {
        var panels = await _page.QuerySelectorAllAsync(Locators.Shared.AccordionPanel);
        return panels.Count;
    }

    #endregion

    #region Info Buttons

    public async Task ClickInfoButtonAsync(string nearText)
    {
        var selector = $":has-text('{nearText}') ~ .info-button, :has-text('{nearText}') .info-button";
        await _page.ClickAsync(selector);
        await WaitForStabilityAsync();
    }

    public async Task<bool> IsTooltipVisibleAsync()
    {
        var element = await _page.QuerySelectorAsync(Locators.Shared.Tooltip);
        return element != null && await element.IsVisibleAsync();
    }

    #endregion

    #region Verification

    public async Task<bool> IsK8sConfigPageAsync()
    {
        var element = await _page.QuerySelectorAsync(Locators.K8sConfig.ConfigTabsContainer);
        return element != null && await element.IsVisibleAsync();
    }

    public async Task<bool> IsVMConfigPageAsync()
    {
        var serverRoles = await _page.QuerySelectorAsync(Locators.VMConfig.ServerRolesPanel);
        var configTabs = await _page.QuerySelectorAsync(Locators.K8sConfig.ConfigTabsContainer);
        return (serverRoles != null || configTabs != null);
    }

    #endregion

    #region Helpers

    private async Task WaitForStabilityAsync()
    {
        await _page.WaitForTimeoutAsync(500);
    }

    #endregion
}
