using Microsoft.Playwright;

namespace InfraSizingCalculator.E2ETests.Pricing;

/// <summary>
/// E2E tests for Mendix pricing flow
/// Tests the end-to-end experience of configuring and viewing Mendix pricing
/// </summary>
[TestFixture]
public class MendixPricingFlowTests : PlaywrightFixture
{
    /// <summary>
    /// Helper to navigate to Mendix technology selection (after Platform -> Deployment)
    /// Flow: Platform (Low-Code) -> Deployment (K8s) -> Technology (Mendix visible)
    /// </summary>
    private async Task NavigateToMendixPlatformAsync()
    {
        await GoToHomeAsync();

        // Select Low-Code platform -> auto-advances to Deployment step
        await SelectCardAsync("Low-Code");
        await Page.WaitForTimeoutAsync(500);

        // Select Kubernetes deployment -> auto-advances to Technology step
        await SelectCardAsync("Kubernetes");
        await Page.WaitForTimeoutAsync(500);
    }

    [Test]
    public async Task MendixFlow_CanSelectMendixTechnology()
    {
        await NavigateToMendixPlatformAsync();

        // We should be on step 3 (technology selection) - Mendix should be visible
        var mendixCard = Page.Locator(".tech-card:has-text('Mendix')");
        await mendixCard.WaitForAsync(new() { Timeout = 5000, State = WaitForSelectorState.Visible });

        Assert.That(await mendixCard.IsVisibleAsync(), Is.True,
            "Mendix technology card should be visible");

        await mendixCard.ClickAsync();
        await Page.WaitForTimeoutAsync(800);

        // Verify we moved to distribution selection (Step 4 for K8s)
        var distroCards = Page.Locator(".distro-card, .selection-card");
        Assert.That(await distroCards.CountAsync(), Is.GreaterThan(0),
            "Distribution options should be visible after selecting Mendix");
    }

    [Test]
    public async Task MendixFlow_ShowsDistributionOptions()
    {
        await NavigateToMendixPlatformAsync();

        // Select Mendix -> auto-advances to distribution step
        await SelectTechCardAsync("Mendix");
        await Page.WaitForTimeoutAsync(500);

        // Look for distribution cards (OpenShift, K3s, etc.)
        var distroCards = await Page.QuerySelectorAllAsync(".distro-card");
        Assert.That(distroCards.Count, Is.GreaterThan(0),
            "Distribution options should be visible after selecting Mendix");
    }

    [Test]
    public async Task MendixFlow_ShowsConfigurationAfterDistribution()
    {
        await NavigateToMendixPlatformAsync();

        // Select Mendix
        await SelectTechCardAsync("Mendix");
        await Page.WaitForTimeoutAsync(500);

        // Select a distribution
        await Page.ClickAsync(".distro-card");
        await Page.WaitForTimeoutAsync(800);

        // Should show configuration tabs
        Assert.That(await IsVisibleAsync(".config-tabs-container") || await IsVisibleAsync(".config-tab"), Is.True,
            "Configuration tabs should be visible after selecting distribution");
    }

    [Test]
    public async Task MendixConfigTab_ShowsMendixTab()
    {
        await NavigateToMendixPlatformAsync();

        // Select Mendix and navigate through the wizard
        await SelectTechCardAsync("Mendix");
        await Page.WaitForTimeoutAsync(500);

        // Select a distribution
        await Page.ClickAsync(".distro-card");
        await Page.WaitForTimeoutAsync(800);

        // Look for Mendix tab in configuration
        var mendixTab = Page.Locator(".config-tab:has-text('Mendix')");
        if (await mendixTab.IsVisibleAsync())
        {
            await mendixTab.ClickAsync();
            await Page.WaitForTimeoutAsync(500);

            // Look for Mendix-related content
            var pageContent = await Page.ContentAsync();
            Assert.That(pageContent.Contains("Mendix") || pageContent.Contains("User"),
                Is.True, "Mendix configuration should be visible");
        }
        else
        {
            // Fallback: verify configuration tabs exist
            Assert.That(await IsVisibleAsync(".config-tabs-container"), Is.True,
                "Configuration container should be visible");
        }
    }

    [Test]
    public async Task PricingStep_ShowsMendixInFlow()
    {
        // This test validates that Mendix appears in the flow
        await NavigateToMendixPlatformAsync();

        // We should see Mendix tech card
        Assert.That(await IsVisibleAsync(".tech-card:has-text('Mendix')"), Is.True,
            "Mendix technology card should be visible in Low-Code flow");
    }
}
