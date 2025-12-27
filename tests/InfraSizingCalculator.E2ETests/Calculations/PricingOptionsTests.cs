using Microsoft.Playwright;

namespace InfraSizingCalculator.E2ETests.Calculations;

/// <summary>
/// E2E tests for pricing options functionality
/// Verifies that all cloud providers are available regardless of distribution
/// </summary>
[TestFixture]
public class PricingOptionsTests : PlaywrightFixture
{
    /// <summary>
    /// Helper to navigate to cost breakdown tab after calculation
    /// </summary>
    private async Task NavigateToCostBreakdownAsync()
    {
        await NavigateToK8sConfigAsync();

        // Wait for the configuration page to fully load
        await Page.WaitForTimeoutAsync(1000);

        // We're in Multi-Cluster mode by default - Dev panel is expanded
        // Use the spinbutton for Medium tier (has existing apps)
        var mediumSpinbutton = Page.Locator(":has-text('Medium')").Locator("[role='spinbutton']").First;
        await mediumSpinbutton.WaitForAsync(new() { Timeout = 5000, State = WaitForSelectorState.Visible });
        await mediumSpinbutton.FillAsync("10");
        await Page.WaitForTimeoutAsync(500);

        // Calculate (K8s: Step 5 Configure -> Step 6 Pricing -> Calculate)
        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync(".sizing-results-view, .results-panel", new() { Timeout = 15000 });

        // Click Cost Breakdown in sidebar
        var costBreakdownLink = Page.Locator(".left-sidebar").GetByText("Cost Breakdown");
        await costBreakdownLink.WaitForAsync(new() { Timeout = 5000, State = WaitForSelectorState.Visible });
        await costBreakdownLink.ClickAsync();
        await Page.WaitForTimeoutAsync(1000);
    }

    [Test]
    public async Task PricingOptions_ShowsAllCloudProviders()
    {
        await NavigateToCostBreakdownAsync();

        // Open Pricing Options section
        var pricingHeader = Page.Locator("text=Pricing Options").First;
        await pricingHeader.ClickAsync();
        await Page.WaitForTimeoutAsync(500);

        // Check that the provider dropdown exists
        var providerSelect = Page.Locator(".pricing-selector select").First;
        Assert.That(await providerSelect.IsVisibleAsync(), Is.True,
            "Provider dropdown should be visible");

        // Get all options
        var options = await providerSelect.Locator("option").AllTextContentsAsync();

        // Verify all providers are present
        Assert.That(options.Any(o => o.Contains("AWS")), Is.True, "AWS should be available");
        Assert.That(options.Any(o => o.Contains("Azure")), Is.True, "Azure should be available");
        Assert.That(options.Any(o => o.Contains("Google") || o.Contains("GCP")), Is.True, "GCP should be available");
        Assert.That(options.Any(o => o.Contains("Oracle") || o.Contains("OCI")), Is.True, "OCI should be available");
        Assert.That(options.Any(o => o.Contains("On-Premises")), Is.True, "On-Premises should be available");
    }

    [Test]
    public async Task PricingOptions_CanSelectDifferentProviders()
    {
        await NavigateToCostBreakdownAsync();

        // Open Pricing Options section
        await Page.ClickAsync("text=Pricing Options");
        await Page.WaitForTimeoutAsync(500);

        var providerSelect = Page.Locator(".pricing-selector select").First;

        // Select AWS
        await providerSelect.SelectOptionAsync(new SelectOptionValue { Label = "AWS (EKS / EC2)" });
        await Page.WaitForTimeoutAsync(300);

        // Verify region dropdown appears for cloud provider
        var regionSelect = Page.Locator(".pricing-selector select").Nth(1);
        Assert.That(await regionSelect.IsVisibleAsync(), Is.True,
            "Region dropdown should appear for cloud provider");

        // Select On-Premises
        await providerSelect.SelectOptionAsync(new SelectOptionValue { Label = "On-Premises (Self-Managed)" });
        await Page.WaitForTimeoutAsync(300);

        // Region should not be needed for on-prem (title changes to On-Premises Cost Estimation)
        var title = await Page.Locator(".pricing-selector h4").TextContentAsync();
        Assert.That(title, Does.Contain("On-Premises"),
            "Title should indicate On-Premises estimation");
    }

    [Test]
    public async Task PricingOptions_CalculateCostsButton_Works()
    {
        await NavigateToCostBreakdownAsync();

        // Open Pricing Options section
        await Page.ClickAsync("text=Pricing Options");
        await Page.WaitForTimeoutAsync(500);

        // Click Calculate Costs button
        var calculateBtn = Page.Locator("text=Calculate Costs").First;
        await calculateBtn.ClickAsync();
        await Page.WaitForTimeoutAsync(2000);

        // Verify cost summary is updated (check for cost amount display)
        var costSummary = Page.Locator(".cost-summary-bar, .cost-summary-item");
        Assert.That(await costSummary.CountAsync(), Is.GreaterThan(0),
            "Cost summary should be displayed after calculation");
    }

    [Test]
    public async Task PricingOptions_ShowsCompareNote()
    {
        await NavigateToCostBreakdownAsync();

        // Open Pricing Options section
        await Page.ClickAsync("text=Pricing Options");
        await Page.WaitForTimeoutAsync(500);

        // Check for the compare note
        var compareNote = Page.Locator(".cloud-alternatives-note");
        Assert.That(await compareNote.IsVisibleAsync(), Is.True,
            "Compare note should be visible");

        var noteText = await compareNote.TextContentAsync();
        Assert.That(noteText, Does.Contain("Compare"),
            "Note should mention comparing costs");
    }

    [Test]
    public async Task CostBreakdown_ShowsTwoColumnLayout()
    {
        await NavigateToCostBreakdownAsync();

        // Wait for cost data to load
        await Page.WaitForTimeoutAsync(1000);

        // Check for two-column layout
        var twoColumnLayout = Page.Locator(".cost-two-column");
        Assert.That(await twoColumnLayout.IsVisibleAsync(), Is.True,
            "Two-column layout should be visible");

        // Check for both columns
        var costColumns = Page.Locator(".cost-column");
        Assert.That(await costColumns.CountAsync(), Is.EqualTo(2),
            "Should have two cost columns");
    }

    [Test]
    public async Task CostBreakdown_ShowsCostSummaryBar()
    {
        await NavigateToCostBreakdownAsync();

        // Wait for cost data to load
        await Page.WaitForTimeoutAsync(1000);

        // Check for cost summary bar
        var summaryBar = Page.Locator(".cost-summary-bar");
        Assert.That(await summaryBar.IsVisibleAsync(), Is.True,
            "Cost summary bar should be visible");

        // Check for summary items (monthly, yearly, TCO)
        var summaryItems = Page.Locator(".cost-summary-item");
        Assert.That(await summaryItems.CountAsync(), Is.GreaterThanOrEqualTo(3),
            "Should have at least 3 summary items (monthly, yearly, TCO)");
    }
}
