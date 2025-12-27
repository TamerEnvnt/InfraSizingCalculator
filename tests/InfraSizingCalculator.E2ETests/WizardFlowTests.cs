namespace InfraSizingCalculator.E2ETests;

/// <summary>
/// E2E tests for the wizard flow navigation.
/// Note: Card selections auto-advance to the next step, so ClickNextAsync() is not needed after selecting cards.
/// Flow: Platform -> Deployment -> Technology -> (Distribution for K8s) -> Configuration
/// </summary>
[TestFixture]
public class WizardFlowTests : PlaywrightFixture
{
    [Test]
    public async Task HomePage_LoadsSuccessfully()
    {
        await GoToHomeAsync();

        // Verify main content is present (new 3-panel layout)
        Assert.That(await IsVisibleAsync(".main-content"), Is.True,
            "Main content should be visible");

        // Verify step 1 is current (uses nav-step with .current class)
        Assert.That(await IsVisibleAsync(".nav-step.current"), Is.True,
            "First step should be current");
    }

    [Test]
    public async Task Step1_ShowsPlatformOptions()
    {
        await GoToHomeAsync();

        // Should show Native and Low-Code platform options
        Assert.That(await IsVisibleAsync(".selection-card:has-text('Native')"), Is.True,
            "Native Applications card should be visible");
        Assert.That(await IsVisibleAsync(".selection-card:has-text('Low-Code')"), Is.True,
            "Low-Code Platform card should be visible");
    }

    [Test]
    public async Task Step1_SelectNative_NavigatesToStep2()
    {
        await GoToHomeAsync();

        // Clicking a platform card auto-advances to Step 2
        await SelectCardAsync("Native");

        // Verify we're on step 2 (Deployment) by checking for deployment cards
        Assert.That(await IsVisibleAsync(".selection-card:has-text('Kubernetes')"), Is.True,
            "Should see Kubernetes option on Deployment step");
    }

    [Test]
    public async Task Step2_ShowsDeploymentOptions()
    {
        await GoToHomeAsync();
        await SelectCardAsync("Native"); // Auto-advances to Step 2

        // Should show deployment options
        Assert.That(await IsVisibleAsync(".selection-card:has-text('Kubernetes')"), Is.True,
            "Kubernetes option should be visible");
        Assert.That(await IsVisibleAsync(".selection-card:has-text('Virtual Machines')"), Is.True,
            "Virtual Machines option should be visible");
    }

    [Test]
    public async Task Step2_SelectK8s_NavigatesToStep3()
    {
        await GoToHomeAsync();
        await SelectCardAsync("Native"); // Auto-advances to Step 2
        await SelectCardAsync("Kubernetes"); // Auto-advances to Step 3 (Technology)

        // Should show technology options
        Assert.That(await IsVisibleAsync(".tech-card:has-text('.NET')") || await IsVisibleAsync(".selection-card:has-text('.NET')"), Is.True,
            ".NET option should be visible on Technology step");
    }

    [Test]
    public async Task Step3_ShowsTechnologyOptions_ForNative()
    {
        await GoToHomeAsync();
        await SelectCardAsync("Native"); // -> Step 2
        await SelectCardAsync("Kubernetes"); // -> Step 3

        // Should show technology options (using tech-card class)
        Assert.That(await IsVisibleAsync(".tech-card:has-text('.NET')"), Is.True,
            ".NET option should be visible");
        Assert.That(await IsVisibleAsync(".tech-card:has-text('Java')"), Is.True,
            "Java option should be visible");
    }

    [Test]
    public async Task Step3_ShowsTechnologyOptions_ForLowCode()
    {
        await GoToHomeAsync();
        await SelectCardAsync("Low-Code"); // -> Step 2
        await SelectCardAsync("Kubernetes"); // -> Step 3

        // Should show low-code platforms
        Assert.That(await IsVisibleAsync(".tech-card:has-text('Mendix')"), Is.True,
            "Mendix option should be visible");
    }

    [Test]
    public async Task Step4_ShowsDistributionOptions()
    {
        await GoToHomeAsync();
        await SelectCardAsync("Native"); // -> Step 2
        await SelectCardAsync("Kubernetes"); // -> Step 3
        await Page.ClickAsync(".tech-card:has-text('.NET')"); // -> Step 4 (Distribution)
        await Page.WaitForTimeoutAsync(800);

        // Should show distribution options
        Assert.That(await IsVisibleAsync(".distro-card"), Is.True,
            "Distribution cards should be visible");
    }

    [Test]
    public async Task BackButton_NavigatesToPreviousStep()
    {
        await GoToHomeAsync();
        await SelectCardAsync("Native"); // Auto-advances to Step 2

        // Now on step 2, go back
        await ClickBackAsync();

        // Verify we're back on step 1
        Assert.That(await IsVisibleAsync(".selection-card:has-text('Native')"), Is.True,
            "Should be back on Platform step");
    }

    [Test]
    public async Task FullWizardFlow_K8s_ReachesConfigStep()
    {
        await GoToHomeAsync();

        // Step 1: Platform -> auto-advances to Step 2
        await SelectCardAsync("Native");

        // Step 2: Deployment -> auto-advances to Step 3
        await SelectCardAsync("Kubernetes");

        // Step 3: Technology -> auto-advances to Step 4
        await Page.ClickAsync(".tech-card:has-text('.NET')");
        await Page.WaitForTimeoutAsync(800);

        // Step 4: Distribution -> click a distro card
        await Page.ClickAsync(".distro-card");
        await Page.WaitForTimeoutAsync(800);

        // Step 5: Configuration
        Assert.That(await IsVisibleAsync(".config-tabs-container"), Is.True,
            "Configuration tabs should be visible");
        Assert.That(await IsVisibleAsync(".config-tab:has-text('Applications')"), Is.True,
            "Applications tab should be visible");
    }

    [Test]
    public async Task FullWizardFlow_VM_ReachesConfigStep()
    {
        await GoToHomeAsync();

        // Step 1: Platform -> auto-advances to Step 2
        await SelectCardAsync("Native");

        // Step 2: Deployment - select VMs -> auto-advances to Step 3
        await SelectCardAsync("Virtual Machines");

        // Step 3: Technology -> auto-advances to VM Config (no Distribution step for VMs)
        await Page.ClickAsync(".tech-card:has-text('.NET')");
        await Page.WaitForTimeoutAsync(800);

        // Step 4: VM Configuration
        Assert.That(await IsVisibleAsync(".config-tabs-container"), Is.True,
            "Configuration tabs should be visible");
        Assert.That(await IsVisibleAsync(".config-tab:has-text('Server Roles')"), Is.True,
            "Server Roles tab should be visible for VMs");
    }

    [Test]
    public async Task NextButton_DisabledWithoutSelection()
    {
        await GoToHomeAsync();

        // Without selecting a platform, Next should be disabled
        var nextButton = await Page.QuerySelectorAsync("button:has-text('Next')");
        Assert.That(nextButton, Is.Not.Null, "Next button should exist");

        var isDisabled = await nextButton!.GetAttributeAsync("disabled");
        Assert.That(isDisabled, Is.Not.Null, "Next button should be disabled without selection");
    }

    [Test]
    public async Task StepIndicator_ShowsCorrectProgress()
    {
        await GoToHomeAsync();

        // Step 1 should be current (nav-step uses .current class)
        // Use :nth-of-type to get buttons, not the h3 title
        var navSteps = await Page.QuerySelectorAllAsync(".nav-section .nav-step");
        Assert.That(navSteps.Count, Is.GreaterThan(0), "Nav steps should exist");

        var step1 = navSteps[0];
        Assert.That(await step1.GetAttributeAsync("class"), Does.Contain("current"),
            "Step 1 should be current");

        // Navigate to step 2 (auto-advances on selection)
        await SelectCardAsync("Native");

        // Step 1 should be completed, Step 2 should be current
        navSteps = await Page.QuerySelectorAllAsync(".nav-section .nav-step");
        step1 = navSteps[0];
        Assert.That(await step1.GetAttributeAsync("class"), Does.Contain("completed"),
            "Step 1 should be completed");

        var step2 = navSteps[1];
        Assert.That(await step2.GetAttributeAsync("class"), Does.Contain("current"),
            "Step 2 should be current");
    }
}
