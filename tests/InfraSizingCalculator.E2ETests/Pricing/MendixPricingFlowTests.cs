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
        await mendixCard.WaitForAsync(new() { Timeout = 15000, State = WaitForSelectorState.Visible });

        Assert.That(await mendixCard.IsVisibleAsync(), Is.True,
            "Mendix technology card should be visible");

        await mendixCard.ClickAsync();

        // Wait for Mendix deployment categories (Mendix has a special flow, not .distro-card)
        await Page.WaitForSelectorAsync(".mendix-deployment-categories, .mendix-category-card", new() { Timeout = 15000 });

        // Verify we moved to Mendix deployment category selection (Step 4 for Mendix K8s)
        var categoryCards = Page.Locator(".mendix-category-card");
        Assert.That(await categoryCards.CountAsync(), Is.GreaterThan(0),
            "Mendix deployment categories should be visible after selecting Mendix");
    }

    [Test]
    public async Task MendixFlow_ShowsDeploymentCategories()
    {
        await NavigateToMendixPlatformAsync();

        // Select Mendix -> auto-advances to Mendix deployment category step
        await SelectTechCardAsync("Mendix");

        // Wait for Mendix deployment categories to load (Mendix has special flow)
        await Page.WaitForSelectorAsync(".mendix-deployment-categories, .mendix-category-card", new() { Timeout = 15000 });

        // Look for Mendix deployment category cards (Cloud, Private Cloud, Other)
        var categoryCards = await Page.QuerySelectorAllAsync(".mendix-category-card");
        Assert.That(categoryCards.Count, Is.GreaterThan(0),
            "Mendix deployment categories should be visible after selecting Mendix");
    }

    [Test]
    public async Task MendixFlow_ShowsConfigurationAfterDeploymentSelection()
    {
        await NavigateToMendixPlatformAsync();

        // Select Mendix
        await SelectTechCardAsync("Mendix");

        // Wait for Mendix deployment categories to load
        await Page.WaitForSelectorAsync(".mendix-deployment-categories, .mendix-category-card", new() { Timeout = 15000 });

        // Select "Other Kubernetes" category (leads to configuration)
        var otherK8sCard = Page.Locator(".mendix-category-card:has-text('Other Kubernetes')");
        await otherK8sCard.ClickAsync();
        await Page.WaitForTimeoutAsync(500);

        // Select a provider (e.g., K3s) from sub-options
        var k3sOption = Page.Locator(".mendix-option-card:has-text('K3s')");
        if (await k3sOption.CountAsync() > 0)
        {
            await k3sOption.ClickAsync();
            await Page.WaitForTimeoutAsync(500);
        }

        // Wait for configuration to appear
        await Page.WaitForSelectorAsync(".config-tabs-container, .config-tab, .k8s-apps-config, .cluster-mode-sidebar", new() { Timeout = 15000 });

        // Should show configuration
        Assert.That(await IsVisibleAsync(".config-tabs-container") || await IsVisibleAsync(".config-tab") ||
                    await IsVisibleAsync(".k8s-apps-config") || await IsVisibleAsync(".cluster-mode-sidebar"), Is.True,
            "Configuration should be visible after selecting Mendix deployment option");
    }

    [Test]
    public async Task MendixConfigTab_ShowsMendixConfiguration()
    {
        await NavigateToMendixPlatformAsync();

        // Select Mendix and navigate through the wizard
        await SelectTechCardAsync("Mendix");

        // Wait for Mendix deployment categories
        await Page.WaitForSelectorAsync(".mendix-deployment-categories, .mendix-category-card", new() { Timeout = 15000 });

        // Select "Other Kubernetes" category
        var otherK8sCard = Page.Locator(".mendix-category-card:has-text('Other Kubernetes')");
        await otherK8sCard.ClickAsync();
        await Page.WaitForTimeoutAsync(500);

        // Select K3s from sub-options
        var k3sOption = Page.Locator(".mendix-option-card:has-text('K3s')");
        if (await k3sOption.CountAsync() > 0)
        {
            await k3sOption.ClickAsync();
            await Page.WaitForTimeoutAsync(500);
        }

        // Wait for configuration to appear
        await Page.WaitForSelectorAsync(".config-tabs-container, .config-tab, .k8s-apps-config, .cluster-mode-sidebar", new() { Timeout = 15000 });

        // Verify Mendix-related content is visible (either in tabs or page content)
        var pageContent = await Page.ContentAsync();
        Assert.That(pageContent.Contains("Mendix") || pageContent.Contains("Apps") || pageContent.Contains("Multi-Cluster"),
            Is.True, "Mendix configuration or app configuration should be visible");
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
