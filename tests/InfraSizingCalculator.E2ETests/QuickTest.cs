using Microsoft.Playwright;
using NUnit.Framework;

namespace InfraSizingCalculator.E2ETests;

[TestFixture]
public class QuickTest : PlaywrightFixture
{
    [Test]
    public async Task ClickDefaultsButton_OpensModal()
    {
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Wait for Blazor to initialize
        await Page.WaitForFunctionAsync("() => window.Blazor !== undefined", new PageWaitForFunctionOptions { Timeout = 10000 });
        await Task.Delay(1000);

        // Find and click Settings button (navigates to /settings page)
        var settingsBtn = Page.Locator("button.header-btn.settings-btn");
        await settingsBtn.WaitForAsync(new LocatorWaitForOptions { Timeout = 15000 });

        Console.WriteLine($"Settings button visible: {await settingsBtn.IsVisibleAsync()}");
        Console.WriteLine($"Settings button enabled: {await settingsBtn.IsEnabledAsync()}");

        await settingsBtn.ClickAsync();
        await Task.Delay(1000);

        // Settings button navigates to /settings page, check for settings page content
        var settingsPage = Page.Locator(".settings-page, .settings-main").First;
        var settingsVisible = await settingsPage.IsVisibleAsync();
        Console.WriteLine($"Settings page visible after click: {settingsVisible}");

        Assert.That(settingsVisible, Is.True, "Settings page should be visible after clicking Settings button");
    }
}
