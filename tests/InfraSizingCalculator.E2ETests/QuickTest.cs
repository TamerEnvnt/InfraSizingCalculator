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

        // Find and click Defaults button
        var defaultsBtn = Page.Locator("button.header-btn").Filter(new() { HasText = "Defaults" });
        await defaultsBtn.WaitForAsync(new LocatorWaitForOptions { Timeout = 5000 });

        Console.WriteLine($"Defaults button visible: {await defaultsBtn.IsVisibleAsync()}");
        Console.WriteLine($"Defaults button enabled: {await defaultsBtn.IsEnabledAsync()}");

        await defaultsBtn.ClickAsync();
        await Task.Delay(1000);

        // Check if modal opened - modals use @if so we just look for .modal-overlay
        var modal = Page.Locator(".modal-overlay");
        var modalVisible = await modal.IsVisibleAsync();
        Console.WriteLine($"Modal visible after click: {modalVisible}");

        // Also check for modal content
        var modalContent = Page.Locator(".modal-content");
        var contentVisible = await modalContent.IsVisibleAsync();
        Console.WriteLine($"Modal content visible: {contentVisible}");

        Assert.That(modalVisible || contentVisible, Is.True, "Settings modal should be visible after clicking Defaults");
    }
}
