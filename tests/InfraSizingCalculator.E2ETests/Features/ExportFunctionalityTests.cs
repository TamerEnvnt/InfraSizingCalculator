namespace InfraSizingCalculator.E2ETests.Features;

/// <summary>
/// E2E tests for Export functionality
/// Export is a dropdown button in right sidebar
/// </summary>
[TestFixture]
public class ExportFunctionalityTests : PlaywrightFixture
{
    [Test]
    public async Task ExportDropdown_VisibleAfterCalculation()
    {
        await NavigateToK8sConfigAsync();
        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync("table", new() { Timeout = 10000 });

        // Export dropdown button in right sidebar
        var exportBtn = Page.Locator("button:has-text('Export')");
        Assert.That(await exportBtn.CountAsync(), Is.GreaterThan(0),
            "Export dropdown button should be visible after calculation");
    }

    [Test]
    public async Task ExportDropdown_ClickOpensMenu()
    {
        await NavigateToK8sConfigAsync();
        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync("table", new() { Timeout = 10000 });

        // Click Export button to open menu
        var exportBtn = Page.Locator("button:has-text('Export')").First;
        await exportBtn.ClickAsync();
        await Page.WaitForTimeoutAsync(300);

        // Export menu should appear with format options
        var exportMenu = Page.Locator(".export-menu, .export-dropdown");
        Assert.That(await exportMenu.CountAsync(), Is.GreaterThan(0),
            "Export menu should open after clicking Export button");
    }

    [Test]
    public async Task ExportMenu_HasCSVOption()
    {
        await NavigateToK8sConfigAsync();
        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync("table", new() { Timeout = 10000 });

        // Click Export to open menu
        var exportBtn = Page.Locator("button:has-text('Export')").First;
        await exportBtn.ClickAsync();
        await Page.WaitForTimeoutAsync(300);

        // Check for CSV option
        var csvBtn = Page.Locator("button:has-text('CSV')");
        Assert.That(await csvBtn.CountAsync(), Is.GreaterThan(0),
            "Export menu should have CSV option");
    }

    [Test]
    public async Task ExportMenu_HasJSONOption()
    {
        await NavigateToK8sConfigAsync();
        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync("table", new() { Timeout = 10000 });

        // Click Export to open menu
        var exportBtn = Page.Locator("button:has-text('Export')").First;
        await exportBtn.ClickAsync();
        await Page.WaitForTimeoutAsync(300);

        // Check for JSON option
        var jsonBtn = Page.Locator("button:has-text('JSON')");
        Assert.That(await jsonBtn.CountAsync(), Is.GreaterThan(0),
            "Export menu should have JSON option");
    }

    [Test]
    public async Task ExportMenu_HasExcelOption()
    {
        await NavigateToK8sConfigAsync();
        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync("table", new() { Timeout = 10000 });

        // Click Export to open menu
        var exportBtn = Page.Locator("button:has-text('Export')").First;
        await exportBtn.ClickAsync();
        await Page.WaitForTimeoutAsync(300);

        // Check for Excel option
        var excelBtn = Page.Locator("button:has-text('Excel')");
        Assert.That(await excelBtn.CountAsync(), Is.GreaterThan(0),
            "Export menu should have Excel option");
    }

    [Test]
    public async Task ExportMenu_HasDiagramOption()
    {
        await NavigateToK8sConfigAsync();
        await ClickK8sCalculateAsync();
        await Page.WaitForSelectorAsync("table", new() { Timeout = 10000 });

        // Click Export to open menu
        var exportBtn = Page.Locator("button:has-text('Export')").First;
        await exportBtn.ClickAsync();
        await Page.WaitForTimeoutAsync(300);

        // Check for Diagram option
        var diagramBtn = Page.Locator("button:has-text('Diagram')");
        Assert.That(await diagramBtn.CountAsync(), Is.GreaterThan(0),
            "Export menu should have Diagram option");
    }

    [Test]
    public async Task VM_ExportDropdown_VisibleAfterCalculation()
    {
        await NavigateToVMConfigAsync();
        await ClickVMCalculateAsync();
        await Page.WaitForSelectorAsync("table", new() { Timeout = 10000 });

        var exportBtn = Page.Locator("button:has-text('Export')");
        Assert.That(await exportBtn.CountAsync(), Is.GreaterThan(0),
            "Export button should be visible after VM calculation");
    }
}
