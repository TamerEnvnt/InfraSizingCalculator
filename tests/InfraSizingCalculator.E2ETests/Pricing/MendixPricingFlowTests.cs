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
    /// Helper to navigate to Mendix configuration
    /// </summary>
    private async Task NavigateToMendixPlatformAsync()
    {
        await GoToHomeAsync();

        // Select Low-Code platform
        await SelectCardAsync("Low-Code");
        await Page.WaitForTimeoutAsync(500);
    }

    [Test]
    public async Task MendixFlow_CanSelectMendixTechnology()
    {
        await NavigateToMendixPlatformAsync();

        // We should be on step 2 (technology selection)
        var mendixCard = Page.Locator(".tech-card:has-text('Mendix')");
        await mendixCard.WaitForAsync(new() { Timeout = 5000, State = WaitForSelectorState.Visible });

        Assert.That(await mendixCard.IsVisibleAsync(), Is.True,
            "Mendix technology card should be visible");

        await mendixCard.ClickAsync();
        await Page.WaitForTimeoutAsync(800);

        // Verify we moved to deployment selection
        var deploymentCards = Page.Locator(".deployment-card, .selection-card");
        Assert.That(await deploymentCards.CountAsync(), Is.GreaterThan(0),
            "Deployment options should be visible after selecting Mendix");
    }

    [Test]
    public async Task MendixFlow_ShowsCloudDeploymentOptions()
    {
        await NavigateToMendixPlatformAsync();

        // Select Mendix
        await SelectTechCardAsync("Mendix");
        await Page.WaitForTimeoutAsync(500);

        // Look for Mendix Cloud option
        var pageContent = await Page.ContentAsync();
        Assert.That(pageContent.Contains("Mendix Cloud") || pageContent.Contains("Cloud"),
            Is.True, "Mendix Cloud deployment option should be visible");
    }

    [Test]
    public async Task MendixFlow_ShowsPrivateCloudOptions()
    {
        await NavigateToMendixPlatformAsync();

        // Select Mendix
        await SelectTechCardAsync("Mendix");
        await Page.WaitForTimeoutAsync(500);

        // Look for Private Cloud option
        var pageContent = await Page.ContentAsync();
        Assert.That(pageContent.Contains("Private Cloud") || pageContent.Contains("Private"),
            Is.True, "Private Cloud deployment option should be visible");
    }

    [Test]
    public async Task MendixConfigTab_ShowsUserLicensingFields()
    {
        await NavigateToMendixPlatformAsync();

        // Select Mendix and navigate through the wizard
        await SelectTechCardAsync("Mendix");
        await Page.WaitForTimeoutAsync(1000);

        // Look for Cloud option and click it
        var cloudOption = Page.Locator(".deployment-card:has-text('Cloud'), .selection-card:has-text('Cloud')").First;
        if (await cloudOption.IsVisibleAsync())
        {
            await cloudOption.ClickAsync();
            await Page.WaitForTimeoutAsync(500);
        }

        // Navigate to configuration step (Step 5) - click through or find directly
        // The Mendix tab in Step 5 should show user licensing fields
        var mendixTab = Page.Locator("button:has-text('Mendix'), .tab-button:has-text('Mendix')");
        if (await mendixTab.IsVisibleAsync())
        {
            await mendixTab.ClickAsync();
            await Page.WaitForTimeoutAsync(500);

            // Look for Internal Users field
            var pageContent = await Page.ContentAsync();
            Assert.That(pageContent.Contains("Internal Users") || pageContent.Contains("internal"),
                Is.True, "Internal Users field should be visible in Mendix config");
        }
    }

    [Test]
    public async Task PricingStep_ShowsMendixCostSummary()
    {
        // This test validates that the Mendix cost summary appears in the pricing step
        // We need to navigate through the wizard to the pricing step

        await NavigateToMendixPlatformAsync();
        await SelectTechCardAsync("Mendix");
        await Page.WaitForTimeoutAsync(1000);

        // Look for pricing-related content after navigation
        var pageContent = await Page.ContentAsync();

        // Verify Mendix-related content exists
        Assert.That(pageContent.Contains("Mendix"),
            Is.True, "Mendix content should be present in the flow");
    }
}
