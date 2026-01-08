using Microsoft.Playwright;

namespace InfraSizingCalculator.E2ETests.PageObjects;

/// <summary>
/// Page object for header controls - Settings, Saved, Reset, Theme Toggle.
/// </summary>
public class HeaderPage
{
    private readonly IPage _page;
    private readonly int _defaultTimeout;

    public HeaderPage(IPage page, int defaultTimeout = 15000)
    {
        _page = page;
        _defaultTimeout = defaultTimeout;
    }

    #region Locators

    // Header container
    private const string HeaderBar = ".header-bar";
    private const string HeaderLeft = ".header-left";
    private const string HeaderActions = ".header-actions";
    private const string HeaderRight = ".header-right";

    // Header elements
    private const string LogoIcon = ".logo-icon";
    private const string AppTitle = ".header-left h1";
    private const string SettingsButton = ".settings-btn, button:has-text('Settings')";
    private const string SavedButton = "button:has-text('Saved')";
    private const string ResetButton = ".header-actions button:has-text('Reset')";
    private const string ThemeToggle = ".theme-toggle";
    private const string ThemeIcon = ".theme-toggle .theme-icon";

    #endregion

    #region Visibility Checks

    public async Task<bool> IsHeaderVisibleAsync()
    {
        var element = await _page.QuerySelectorAsync(HeaderBar);
        return element != null && await element.IsVisibleAsync();
    }

    public async Task<bool> IsLogoVisibleAsync()
    {
        var element = await _page.QuerySelectorAsync(LogoIcon);
        return element != null && await element.IsVisibleAsync();
    }

    public async Task<bool> IsSettingsButtonVisibleAsync()
    {
        var element = await _page.QuerySelectorAsync(SettingsButton);
        return element != null && await element.IsVisibleAsync();
    }

    public async Task<bool> IsSavedButtonVisibleAsync()
    {
        var element = await _page.QuerySelectorAsync(SavedButton);
        return element != null && await element.IsVisibleAsync();
    }

    public async Task<bool> IsResetButtonVisibleAsync()
    {
        var element = await _page.QuerySelectorAsync(ResetButton);
        return element != null && await element.IsVisibleAsync();
    }

    public async Task<bool> IsThemeToggleVisibleAsync()
    {
        var element = await _page.QuerySelectorAsync(ThemeToggle);
        return element != null && await element.IsVisibleAsync();
    }

    public async Task<bool> AreAllHeaderElementsVisibleAsync()
    {
        return await IsHeaderVisibleAsync() &&
               await IsLogoVisibleAsync() &&
               await IsSettingsButtonVisibleAsync() &&
               await IsSavedButtonVisibleAsync() &&
               await IsResetButtonVisibleAsync() &&
               await IsThemeToggleVisibleAsync();
    }

    #endregion

    #region Content Verification

    public async Task<string?> GetAppTitleTextAsync()
    {
        var element = await _page.QuerySelectorAsync(AppTitle);
        return element != null ? await element.TextContentAsync() : null;
    }

    public async Task<string?> GetCurrentThemeAsync()
    {
        // Check the data-theme attribute on html or body
        var theme = await _page.EvaluateAsync<string>("() => document.documentElement.getAttribute('data-theme') || 'dark'");
        return theme;
    }

    public async Task<bool> IsDarkModeAsync()
    {
        var theme = await GetCurrentThemeAsync();
        return theme == "dark";
    }

    public async Task<bool> IsLightModeAsync()
    {
        var theme = await GetCurrentThemeAsync();
        return theme == "light";
    }

    #endregion

    #region Click Actions

    public async Task ClickSettingsButtonAsync()
    {
        await _page.ClickAsync(SettingsButton);
        await WaitForStabilityAsync();
    }

    public async Task ClickSavedButtonAsync()
    {
        await _page.ClickAsync(SavedButton);
        await WaitForStabilityAsync();
    }

    public async Task ClickResetButtonAsync()
    {
        await _page.ClickAsync(ResetButton);
        await WaitForStabilityAsync();
    }

    public async Task ClickThemeToggleAsync()
    {
        await _page.ClickAsync(ThemeToggle);
        await WaitForStabilityAsync();
    }

    #endregion

    #region Navigation Verification

    public async Task<bool> IsOnSettingsPageAsync()
    {
        var url = _page.Url;
        return url.Contains("/settings");
    }

    public async Task<bool> IsOnScenariosPageAsync()
    {
        var url = _page.Url;
        return url.Contains("/scenarios");
    }

    public async Task NavigateToSettingsAsync()
    {
        await ClickSettingsButtonAsync();
        // Wait for navigation
        await _page.WaitForURLAsync("**/settings", new() { Timeout = _defaultTimeout });
    }

    #endregion

    #region Theme Actions

    public async Task SwitchToLightModeAsync()
    {
        if (await IsDarkModeAsync())
        {
            await ClickThemeToggleAsync();
            // Wait for theme to apply
            await _page.WaitForFunctionAsync("() => document.documentElement.getAttribute('data-theme') === 'light'",
                new PageWaitForFunctionOptions { Timeout = 5000 });
        }
    }

    public async Task SwitchToDarkModeAsync()
    {
        if (await IsLightModeAsync())
        {
            await ClickThemeToggleAsync();
            // Wait for theme to apply
            await _page.WaitForFunctionAsync("() => document.documentElement.getAttribute('data-theme') === 'dark'",
                new PageWaitForFunctionOptions { Timeout = 5000 });
        }
    }

    public async Task ToggleThemeAsync()
    {
        var wasDark = await IsDarkModeAsync();
        await ClickThemeToggleAsync();

        // Wait for theme to toggle
        var expectedTheme = wasDark ? "light" : "dark";
        await _page.WaitForFunctionAsync($"() => document.documentElement.getAttribute('data-theme') === '{expectedTheme}'",
            new PageWaitForFunctionOptions { Timeout = 5000 });
    }

    #endregion

    #region Reset Verification

    public async Task<bool> IsWizardResetAsync()
    {
        // After reset, should be on step 1 with no selections
        var platformCards = await _page.QuerySelectorAllAsync(".selection-card");
        var activeCards = await _page.QuerySelectorAllAsync(".selection-card.selected, .selection-card.active");
        return platformCards.Count > 0 && activeCards.Count == 0;
    }

    public async Task ResetAndVerifyAsync()
    {
        await ClickResetButtonAsync();
        // Wait for reset to complete
        await _page.WaitForTimeoutAsync(500);
    }

    #endregion

    #region Structure Verification

    public async Task<bool> HasThreeSectionsAsync()
    {
        var leftSection = await _page.QuerySelectorAsync(HeaderLeft);
        var actionsSection = await _page.QuerySelectorAsync(HeaderActions);
        var rightSection = await _page.QuerySelectorAsync(HeaderRight);

        return leftSection != null && actionsSection != null && rightSection != null;
    }

    public async Task<int> GetHeaderButtonCountAsync()
    {
        // Count buttons in header-actions
        var buttons = await _page.QuerySelectorAllAsync($"{HeaderActions} button");
        return buttons.Count;
    }

    #endregion

    #region Helpers

    private async Task WaitForStabilityAsync()
    {
        await _page.WaitForTimeoutAsync(300);
    }

    #endregion
}
