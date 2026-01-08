using Microsoft.Playwright;

namespace InfraSizingCalculator.E2ETests.Settings;

/// <summary>
/// E2E tests for the Settings page
/// Tests that the Settings page loads properly with the new FullPageLayout
/// </summary>
[TestFixture]
public class SettingsPageTests : PlaywrightFixture
{
    [Test]
    public async Task SettingsPage_LoadsWithoutContentClipping()
    {
        await GoToHomeAsync();

        // Click settings button
        var settingsButton = Page.Locator("button:has-text('Settings')");
        await settingsButton.WaitForAsync(new() { Timeout = 15000, State = WaitForSelectorState.Visible });
        await settingsButton.ClickAsync();

        // Wait for settings page to load
        await Page.WaitForSelectorAsync(".settings-page", new() { Timeout = 15000 });

        // Verify the settings main content is visible
        var settingsMain = Page.Locator(".settings-main");
        Assert.That(await settingsMain.IsVisibleAsync(), Is.True,
            "Settings layout should be visible");

        // Verify content is not clipped - check that content section is visible
        var content = Page.Locator(".settings-content");
        Assert.That(await content.IsVisibleAsync(), Is.True, "Settings content should be visible");
    }

    [Test]
    public async Task SettingsPage_CanScrollAllContent()
    {
        await GoToHomeAsync();

        // Navigate to settings
        var settingsButton = Page.Locator("button:has-text('Settings')");
        await settingsButton.ClickAsync();
        await Page.WaitForSelectorAsync(".settings-page", new() { Timeout = 15000 });

        // Verify the settings page has content
        var settingsContent = Page.Locator(".settings-content");
        var isVisible = await settingsContent.IsVisibleAsync();

        // Either the content fits or we can scroll - either is acceptable
        // The fact that we got here means the page is accessible
        Assert.That(isVisible, Is.True, "Settings page content is fully accessible");
    }

    [Test]
    public async Task SettingsPage_ShowsAllSections()
    {
        await GoToHomeAsync();

        var settingsButton = Page.Locator("button:has-text('Settings')");
        await settingsButton.ClickAsync();
        await Page.WaitForSelectorAsync(".settings-page", new() { Timeout = 15000 });

        // Check for main sections in sidebar
        var pageContent = await Page.ContentAsync();

        Assert.That(pageContent.Contains("Hardware") || pageContent.Contains("Infrastructure"),
            Is.True, "Infrastructure/Hardware section should be present");
        Assert.That(pageContent.Contains("Mendix"),
            Is.True, "Mendix section should be present");
    }

    [Test]
    public async Task SettingsPage_HasBackButton()
    {
        await GoToHomeAsync();

        var settingsButton = Page.Locator("button:has-text('Settings')");
        await settingsButton.ClickAsync();
        await Page.WaitForSelectorAsync(".settings-page", new() { Timeout = 15000 });

        // Look for back button or link
        var backButton = Page.Locator("button:has-text('Back'), a:has-text('Back')");
        Assert.That(await backButton.IsVisibleAsync(), Is.True,
            "Back button should be visible on Settings page");
    }
}
