using Microsoft.Playwright;

namespace InfraSizingCalculator.E2ETests.PageObjects;

/// <summary>
/// Page object for pricing configuration step.
/// Handles infrastructure costs, cloud alternatives, Mendix/OutSystems pricing.
/// </summary>
public class PricingPage
{
    private readonly IPage _page;
    private readonly int _defaultTimeout;
    private readonly bool _screenshotEveryStep;
    private readonly string _screenshotDir;
    private int _stepCounter;
    private string _testName;

    public PricingPage(IPage page, int defaultTimeout = 15000)
    {
        _page = page;
        _defaultTimeout = defaultTimeout;
        _screenshotEveryStep = Environment.GetEnvironmentVariable("SCREENSHOT_EVERY_STEP") == "true";
        _screenshotDir = Path.Combine(Directory.GetCurrentDirectory(), "Screenshots");
        _stepCounter = 0;
        _testName = "Pricing";
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

    #region Pricing Toggle

    public async Task TogglePricingAsync()
    {
        var toggle = await _page.QuerySelectorAsync(Locators.Pricing.PricingToggle);
        if (toggle != null)
        {
            await toggle.ClickAsync();
            await WaitForStabilityAsync();
            var isEnabled = await toggle.IsCheckedAsync();
            await TakeStepScreenshotAsync($"PricingToggle_{(isEnabled ? "Enabled" : "Disabled")}");
        }
    }

    public async Task<bool> IsPricingEnabledAsync()
    {
        var toggle = await _page.QuerySelectorAsync(Locators.Pricing.PricingToggle);
        return toggle != null && await toggle.IsCheckedAsync();
    }

    #endregion

    #region Pricing Tabs

    public async Task ClickInfraTabAsync()
    {
        await _page.ClickAsync(Locators.Pricing.InfraTab);
        await WaitForStabilityAsync();
        await TakeStepScreenshotAsync("InfraTab");
    }

    public async Task ClickCloudTabAsync()
    {
        await _page.ClickAsync(Locators.Pricing.CloudTab);
        await WaitForStabilityAsync();
        await TakeStepScreenshotAsync("CloudTab");
    }

    public async Task<bool> IsInfraTabVisibleAsync()
    {
        var element = await _page.QuerySelectorAsync(Locators.Pricing.InfraTab);
        return element != null && await element.IsVisibleAsync();
    }

    public async Task<bool> IsCloudTabVisibleAsync()
    {
        var element = await _page.QuerySelectorAsync(Locators.Pricing.CloudTab);
        return element != null && await element.IsVisibleAsync();
    }

    #endregion

    #region Cloud Provider Selection

    public async Task SelectCloudProviderAsync(string provider)
    {
        var selector = provider.ToUpper() switch
        {
            "AWS" => Locators.Pricing.AWSOption,
            "AZURE" => Locators.Pricing.AzureOption,
            "GCP" => Locators.Pricing.GCPOption,
            "ONPREM" => Locators.Pricing.OnPremOption,
            _ => $"[value='{provider}']"
        };
        await _page.ClickAsync(selector);
        await WaitForStabilityAsync();
        await TakeStepScreenshotAsync($"Provider_{provider}");
    }

    public async Task<bool> IsProviderSelectorVisibleAsync()
    {
        var element = await _page.QuerySelectorAsync(Locators.Pricing.ProviderSelector);
        return element != null && await element.IsVisibleAsync();
    }

    #endregion

    #region Region Selection

    public async Task SelectRegionAsync(string regionValue)
    {
        await _page.SelectOptionAsync(Locators.Pricing.RegionDropdown, new SelectOptionValue { Value = regionValue });
        await WaitForStabilityAsync();
        await TakeStepScreenshotAsync($"Region_{regionValue}");
    }

    public async Task<bool> IsRegionDropdownVisibleAsync()
    {
        var element = await _page.QuerySelectorAsync(Locators.Pricing.RegionDropdown);
        return element != null && await element.IsVisibleAsync();
    }

    #endregion

    #region Cost Inputs

    public async Task SetMonthlyServerCostAsync(string value)
    {
        await _page.FillAsync(Locators.Pricing.MonthlyServerCost, value);
        await WaitForStabilityAsync();
        await TakeStepScreenshotAsync($"MonthlyCost_{value}");
    }

    public async Task SetCostInputAsync(string value)
    {
        await _page.FillAsync(Locators.Pricing.CostInput, value);
        await WaitForStabilityAsync();
        await TakeStepScreenshotAsync($"CostInput_{value}");
    }

    public async Task<string> GetCostInputValueAsync()
    {
        var input = await _page.QuerySelectorAsync(Locators.Pricing.CostInput);
        return input != null ? await _page.InputValueAsync(Locators.Pricing.CostInput) : "";
    }

    #endregion

    #region Mendix Pricing

    public async Task SelectMendixEditionAsync(string edition)
    {
        await _page.SelectOptionAsync(Locators.Pricing.MendixEditionSelector, new SelectOptionValue { Label = edition });
        await WaitForStabilityAsync();
        await TakeStepScreenshotAsync($"MendixEdition_{edition}");
    }

    public async Task SetMendixUserCountAsync(string count)
    {
        await _page.FillAsync(Locators.Pricing.MendixUserCountInput, count);
        await WaitForStabilityAsync();
        await TakeStepScreenshotAsync($"MendixUsers_{count}");
    }

    public async Task<bool> IsMendixEditionSelectorVisibleAsync()
    {
        var element = await _page.QuerySelectorAsync(Locators.Pricing.MendixEditionSelector);
        return element != null && await element.IsVisibleAsync();
    }

    public async Task<bool> IsMendixUserCountInputVisibleAsync()
    {
        var element = await _page.QuerySelectorAsync(Locators.Pricing.MendixUserCountInput);
        return element != null && await element.IsVisibleAsync();
    }

    #endregion

    #region OutSystems Pricing

    public async Task SetOutSystemsAOAsync(string ao)
    {
        await _page.FillAsync(Locators.Pricing.OutSystemsAOInput, ao);
        await WaitForStabilityAsync();
        await TakeStepScreenshotAsync($"OutSystemsAO_{ao}");
    }

    public async Task<bool> IsOutSystemsAOInputVisibleAsync()
    {
        var element = await _page.QuerySelectorAsync(Locators.Pricing.OutSystemsAOInput);
        return element != null && await element.IsVisibleAsync();
    }

    #endregion

    #region Calculate Button

    public async Task ClickCalculateAsync()
    {
        await TakeStepScreenshotAsync("BeforeCalculate");
        var calcButton = _page.Locator(Locators.Wizard.CalculateButton);
        await calcButton.WaitForAsync(new() { Timeout = _defaultTimeout, State = WaitForSelectorState.Visible });
        await calcButton.ClickAsync();

        // Wait for the results panel to appear (step 7 transition)
        // The results panel only shows when currentStep >= GetResultsStep() && results != null
        await _page.WaitForSelectorAsync(Locators.Results.ResultsContainer, new() { Timeout = 10000, State = WaitForSelectorState.Visible });
        await TakeStepScreenshotAsync("AfterCalculate_ResultsVisible");
    }

    public async Task<bool> IsCalculateButtonEnabledAsync()
    {
        var button = await _page.QuerySelectorAsync(Locators.Wizard.CalculateButton);
        if (button == null) return false;
        var disabled = await button.GetAttributeAsync("disabled");
        return disabled == null;
    }

    public async Task<bool> IsCalculateButtonVisibleAsync()
    {
        var element = await _page.QuerySelectorAsync("button:has-text('Calculate')");
        return element != null && await element.IsVisibleAsync();
    }

    #endregion

    #region Pricing Panel Verification

    public async Task<bool> IsPricingPanelVisibleAsync()
    {
        var element = await _page.QuerySelectorAsync(Locators.Pricing.PricingPanel);
        return element != null && await element.IsVisibleAsync();
    }

    public async Task<bool> ShowsNAWhenDisabledAsync()
    {
        var pageText = await _page.TextContentAsync("body");
        return pageText!.Contains("N/A") || pageText.Contains("not configured");
    }

    public async Task<bool> HasCostInputFieldsAsync()
    {
        var inputs = await _page.QuerySelectorAllAsync("input[type='number']");
        return inputs.Count > 0;
    }

    public async Task<bool> ShowsCloudProvidersAsync()
    {
        var pageText = await _page.TextContentAsync("body");
        return pageText!.Contains("AWS") || pageText.Contains("Azure") || pageText.Contains("GCP");
    }

    #endregion

    #region Helpers

    private async Task WaitForStabilityAsync()
    {
        await _page.WaitForTimeoutAsync(500);
    }

    #endregion
}
