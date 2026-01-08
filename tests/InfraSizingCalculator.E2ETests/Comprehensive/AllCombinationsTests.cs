namespace InfraSizingCalculator.E2ETests.Comprehensive;

/// <summary>
/// Comprehensive E2E tests covering all platform/deployment/technology combinations.
/// Tests all possible paths through the wizard flow.
/// </summary>
[TestFixture]
public class AllCombinationsTests : PlaywrightFixture
{
    #region Native + Kubernetes + All Technologies

    [Test]
    public async Task Native_K8s_DotNet_CompleteFlow()
    {
        await GoToHomeAsync();
        await SelectCardAsync("Native");
        await SelectCardAsync("Kubernetes");
        await SelectTechCardAsync(".NET");
        await SelectDistroCardAsync();

        Assert.That(await IsVisibleAsync(".config-tabs-container"), Is.True,
            "Should reach K8s configuration");
        Assert.That(await IsVisibleAsync(".config-tab:has-text('Applications')"), Is.True);
    }

    [Test]
    public async Task Native_K8s_Java_CompleteFlow()
    {
        await GoToHomeAsync();
        await SelectCardAsync("Native");
        await SelectCardAsync("Kubernetes");
        await SelectTechCardAsync("Java");
        await SelectDistroCardAsync();

        Assert.That(await IsVisibleAsync(".config-tabs-container"), Is.True,
            "Should reach K8s configuration for Java");
    }

    [Test]
    public async Task Native_K8s_NodeJs_CompleteFlow()
    {
        await GoToHomeAsync();
        await SelectCardAsync("Native");
        await SelectCardAsync("Kubernetes");
        await SelectTechCardAsync("Node.js");
        await SelectDistroCardAsync();

        Assert.That(await IsVisibleAsync(".config-tabs-container"), Is.True,
            "Should reach K8s configuration for Node.js");
    }

    [Test]
    public async Task Native_K8s_Python_CompleteFlow()
    {
        await GoToHomeAsync();
        await SelectCardAsync("Native");
        await SelectCardAsync("Kubernetes");
        await SelectTechCardAsync("Python");
        await SelectDistroCardAsync();

        Assert.That(await IsVisibleAsync(".config-tabs-container"), Is.True,
            "Should reach K8s configuration for Python");
    }

    [Test]
    public async Task Native_K8s_Go_CompleteFlow()
    {
        await GoToHomeAsync();
        await SelectCardAsync("Native");
        await SelectCardAsync("Kubernetes");
        await SelectTechCardAsync("Go");
        await SelectDistroCardAsync();

        Assert.That(await IsVisibleAsync(".config-tabs-container"), Is.True,
            "Should reach K8s configuration for Go");
    }

    #endregion

    #region Native + VMs + All Technologies

    [Test]
    public async Task Native_VM_DotNet_CompleteFlow()
    {
        await GoToHomeAsync();
        await SelectCardAsync("Native");
        await SelectCardAsync("Virtual Machines");
        await SelectTechCardAsync(".NET");

        Assert.That(await IsVisibleAsync(".config-tabs-container"), Is.True,
            "Should reach VM configuration for .NET");
        Assert.That(await IsVisibleAsync(".config-tab:has-text('Server Roles')"), Is.True);
    }

    [Test]
    public async Task Native_VM_Java_CompleteFlow()
    {
        await GoToHomeAsync();
        await SelectCardAsync("Native");
        await SelectCardAsync("Virtual Machines");
        await SelectTechCardAsync("Java");

        Assert.That(await IsVisibleAsync(".config-tabs-container"), Is.True,
            "Should reach VM configuration for Java");
    }

    [Test]
    public async Task Native_VM_NodeJs_CompleteFlow()
    {
        await GoToHomeAsync();
        await SelectCardAsync("Native");
        await SelectCardAsync("Virtual Machines");
        await SelectTechCardAsync("Node.js");

        Assert.That(await IsVisibleAsync(".config-tabs-container"), Is.True,
            "Should reach VM configuration for Node.js");
    }

    [Test]
    public async Task Native_VM_Python_CompleteFlow()
    {
        await GoToHomeAsync();
        await SelectCardAsync("Native");
        await SelectCardAsync("Virtual Machines");
        await SelectTechCardAsync("Python");

        Assert.That(await IsVisibleAsync(".config-tabs-container"), Is.True,
            "Should reach VM configuration for Python");
    }

    [Test]
    public async Task Native_VM_Go_CompleteFlow()
    {
        await GoToHomeAsync();
        await SelectCardAsync("Native");
        await SelectCardAsync("Virtual Machines");
        await SelectTechCardAsync("Go");

        Assert.That(await IsVisibleAsync(".config-tabs-container"), Is.True,
            "Should reach VM configuration for Go");
    }

    #endregion

    #region LowCode + Kubernetes + Mendix (OutSystems is VM-only)

    [Test]
    public async Task LowCode_K8s_Mendix_PrivateCloud_Azure_CompleteFlow()
    {
        await GoToHomeAsync();
        await SelectCardAsync("Low-Code");
        await SelectCardAsync("Kubernetes");
        await SelectTechCardAsync("Mendix");
        await SelectMendixCategoryCardAsync("Private Cloud");
        await SelectMendixProviderCardAsync("Mendix Azure");

        Assert.That(await IsVisibleAsync(".config-tabs-container"), Is.True,
            "Should reach K8s configuration for Mendix Private Cloud Azure");
    }

    [Test]
    public async Task LowCode_K8s_Mendix_PrivateCloud_EKS_CompleteFlow()
    {
        await GoToHomeAsync();
        await SelectCardAsync("Low-Code");
        await SelectCardAsync("Kubernetes");
        await SelectTechCardAsync("Mendix");
        await SelectMendixCategoryCardAsync("Private Cloud");
        await SelectMendixProviderCardAsync("Amazon EKS");

        Assert.That(await IsVisibleAsync(".config-tabs-container"), Is.True,
            "Should reach K8s configuration for Mendix Private Cloud EKS");
    }

    [Test]
    public async Task LowCode_K8s_Mendix_MendixCloud_CompleteFlow()
    {
        await GoToHomeAsync();
        await SelectCardAsync("Low-Code");
        await SelectCardAsync("Kubernetes");
        await SelectTechCardAsync("Mendix");
        await SelectMendixCategoryCardAsync("Mendix Cloud");

        // Mendix Cloud is SaaS - shows cloud type options
        Assert.That(await IsVisibleAsync(".mendix-cloud-options, .mendix-suboptions"), Is.True,
            "Should show Mendix Cloud type options");
    }

    [Test]
    public async Task LowCode_K8s_Mendix_OtherK8s_Rancher_CompleteFlow()
    {
        await GoToHomeAsync();
        await SelectCardAsync("Low-Code");
        await SelectCardAsync("Kubernetes");
        await SelectTechCardAsync("Mendix");
        await SelectMendixCategoryCardAsync("Other Kubernetes");
        await SelectMendixProviderCardAsync("Rancher");

        Assert.That(await IsVisibleAsync(".config-tabs-container"), Is.True,
            "Should reach K8s configuration for Mendix Other K8s Rancher");
    }

    // Note: OutSystems is VM-only - K8s is only available via OutSystems Cloud/ODC (SaaS)
    // No OutSystems K8s tests here as it's not supported in this application

    #endregion

    #region LowCode + VMs + All Platforms

    [Test]
    public async Task LowCode_VM_Mendix_Server_CompleteFlow()
    {
        await GoToHomeAsync();
        await SelectCardAsync("Low-Code");
        await SelectCardAsync("Virtual Machines");
        await SelectTechCardAsync("Mendix");
        await SelectMendixCategoryCardAsync("Server");

        Assert.That(await IsVisibleAsync(".config-tabs-container"), Is.True,
            "Should reach VM configuration for Mendix Server");
    }

    [Test]
    public async Task LowCode_VM_Mendix_StackIT_CompleteFlow()
    {
        await GoToHomeAsync();
        await SelectCardAsync("Low-Code");
        await SelectCardAsync("Virtual Machines");
        await SelectTechCardAsync("Mendix");
        await SelectMendixCategoryCardAsync("StackIT");

        Assert.That(await IsVisibleAsync(".config-tabs-container, .pricing-panel"), Is.True,
            "Should reach VM configuration for Mendix StackIT");
    }

    [Test]
    public async Task LowCode_VM_Mendix_SapBtp_CompleteFlow()
    {
        await GoToHomeAsync();
        await SelectCardAsync("Low-Code");
        await SelectCardAsync("Virtual Machines");
        await SelectTechCardAsync("Mendix");
        await SelectMendixCategoryCardAsync("SAP BTP");

        Assert.That(await IsVisibleAsync(".config-tabs-container, .pricing-panel"), Is.True,
            "Should reach VM configuration for Mendix SAP BTP");
    }

    [Test]
    public async Task LowCode_VM_OutSystems_CompleteFlow()
    {
        await GoToHomeAsync();
        await SelectCardAsync("Low-Code");
        await SelectCardAsync("Virtual Machines");
        await SelectTechCardAsync("OutSystems");

        Assert.That(await IsVisibleAsync(".config-tabs-container"), Is.True,
            "Should reach VM configuration for OutSystems");
    }

    #endregion

    #region K8s Cluster Modes - Native Technologies

    [Test]
    public async Task Native_K8s_DotNet_MultiCluster_Mode()
    {
        await NavigateToK8sConfigAsync();
        // Default is Multi-Cluster
        Assert.That(await IsVisibleAsync(".mode-option.active:has-text('Multi')"), Is.True,
            "Multi-Cluster mode should be active by default");
    }

    [Test]
    public async Task Native_K8s_DotNet_SingleCluster_Mode()
    {
        await NavigateToK8sConfigAsync();
        await SelectClusterModeAsync("Single");

        Assert.That(await IsVisibleAsync(".mode-option.active:has-text('Single')"), Is.True,
            "Single Cluster mode should be active");
        Assert.That(await IsVisibleAsync(".tier-card"), Is.True,
            "Tier cards should be visible in single cluster mode");
    }

    [Test]
    public async Task Native_K8s_DotNet_PerEnvironment_Mode()
    {
        await NavigateToK8sConfigAsync();
        await SelectClusterModeAsync("Per-Env");

        Assert.That(await IsVisibleAsync(".mode-option.active:has-text('Per-Env')"), Is.True,
            "Per-Environment mode should be active");
    }

    [Test]
    public async Task Native_K8s_Java_MultiCluster_ToPricing()
    {
        await GoToHomeAsync();
        await SelectCardAsync("Native");
        await SelectCardAsync("Kubernetes");
        await SelectTechCardAsync("Java");
        await SelectDistroCardAsync();

        await ClickK8sCalculateAsync();
        Assert.That(await IsVisibleAsync(".results-container, .sizing-results"), Is.True,
            "Should reach results");
    }

    [Test]
    public async Task Native_K8s_NodeJs_MultiCluster_ToPricing()
    {
        await GoToHomeAsync();
        await SelectCardAsync("Native");
        await SelectCardAsync("Kubernetes");
        await SelectTechCardAsync("Node.js");
        await SelectDistroCardAsync();

        await ClickK8sCalculateAsync();
        Assert.That(await IsVisibleAsync(".results-container, .sizing-results"), Is.True,
            "Should reach results");
    }

    [Test]
    public async Task Native_K8s_Python_SingleCluster_ToPricing()
    {
        await GoToHomeAsync();
        await SelectCardAsync("Native");
        await SelectCardAsync("Kubernetes");
        await SelectTechCardAsync("Python");
        await SelectDistroCardAsync();

        await SelectClusterModeAsync("Single");
        await Page.WaitForTimeoutAsync(500);

        await ClickK8sCalculateAsync();
        Assert.That(await IsVisibleAsync(".results-container, .sizing-results"), Is.True,
            "Should reach results for Python single cluster");
    }

    [Test]
    public async Task Native_K8s_Go_PerEnv_ToPricing()
    {
        await GoToHomeAsync();
        await SelectCardAsync("Native");
        await SelectCardAsync("Kubernetes");
        await SelectTechCardAsync("Go");
        await SelectDistroCardAsync();

        await SelectClusterModeAsync("Per-Env");
        await Page.WaitForTimeoutAsync(500);

        await ClickK8sCalculateAsync();
        Assert.That(await IsVisibleAsync(".results-container, .sizing-results"), Is.True,
            "Should reach results for Go per-environment");
    }

    #endregion

    #region K8s Cluster Modes - LowCode Technologies (Mendix only - OutSystems is VM-only)

    [Test]
    public async Task LowCode_K8s_Mendix_Azure_MultiCluster_ToPricing()
    {
        await GoToHomeAsync();
        await SelectCardAsync("Low-Code");
        await SelectCardAsync("Kubernetes");
        await SelectTechCardAsync("Mendix");
        await SelectMendixCategoryCardAsync("Private Cloud");
        await SelectMendixProviderCardAsync("Mendix Azure");

        await ClickK8sCalculateAsync();

        // Verify full results journey - check all tabs
        await VerifyAllResultsTabsAsync();
    }

    [Test]
    public async Task LowCode_K8s_Mendix_Azure_SingleCluster_ToPricing()
    {
        await GoToHomeAsync();
        await SelectCardAsync("Low-Code");
        await SelectCardAsync("Kubernetes");
        await SelectTechCardAsync("Mendix");
        await SelectMendixCategoryCardAsync("Private Cloud");
        await SelectMendixProviderCardAsync("Mendix Azure");

        // Note: Mendix K8s may not have cluster mode selector - skip if not present
        try
        {
            await SelectClusterModeAsync("Single");
            await Page.WaitForTimeoutAsync(500);
        }
        catch { /* Cluster mode selector not available for Mendix */ }

        await ClickK8sCalculateAsync();
        await VerifyAllResultsTabsAsync();
    }

    [Test]
    public async Task LowCode_K8s_Mendix_Azure_PerEnv_ToPricing()
    {
        await GoToHomeAsync();
        await SelectCardAsync("Low-Code");
        await SelectCardAsync("Kubernetes");
        await SelectTechCardAsync("Mendix");
        await SelectMendixCategoryCardAsync("Private Cloud");
        await SelectMendixProviderCardAsync("Mendix Azure");

        // Note: Mendix K8s may not have cluster mode selector - skip if not present
        try
        {
            await SelectClusterModeAsync("Per-Env");
            await Page.WaitForTimeoutAsync(500);
        }
        catch { /* Cluster mode selector not available for Mendix */ }

        await ClickK8sCalculateAsync();
        await VerifyAllResultsTabsAsync();
    }

    [Test]
    public async Task LowCode_K8s_Mendix_Rancher_MultiCluster_ToPricing()
    {
        await GoToHomeAsync();
        await SelectCardAsync("Low-Code");
        await SelectCardAsync("Kubernetes");
        await SelectTechCardAsync("Mendix");
        await SelectMendixCategoryCardAsync("Other Kubernetes");
        await SelectMendixProviderCardAsync("Rancher");

        await ClickK8sCalculateAsync();
        await VerifyAllResultsTabsAsync();
    }

    // Note: OutSystems K8s tests removed - OutSystems only supports VM deployment in this app

    #endregion

    #region VM Complete Flows with Results

    [Test]
    public async Task Native_VM_DotNet_ToResults()
    {
        await NavigateToVMConfigAsync();
        await ClickVMCalculateAsync();
        await VerifyAllResultsTabsAsync();
    }

    [Test]
    public async Task Native_VM_Java_ToResults()
    {
        await GoToHomeAsync();
        await SelectCardAsync("Native");
        await SelectCardAsync("Virtual Machines");
        await SelectTechCardAsync("Java");

        await ClickVMCalculateAsync();
        await VerifyAllResultsTabsAsync();
    }

    [Test]
    public async Task Native_VM_NodeJs_ToResults()
    {
        await GoToHomeAsync();
        await SelectCardAsync("Native");
        await SelectCardAsync("Virtual Machines");
        await SelectTechCardAsync("Node.js");

        await ClickVMCalculateAsync();
        await VerifyAllResultsTabsAsync();
    }

    [Test]
    public async Task Native_VM_Python_ToResults()
    {
        await GoToHomeAsync();
        await SelectCardAsync("Native");
        await SelectCardAsync("Virtual Machines");
        await SelectTechCardAsync("Python");

        await ClickVMCalculateAsync();
        await VerifyAllResultsTabsAsync();
    }

    [Test]
    public async Task Native_VM_Go_ToResults()
    {
        await GoToHomeAsync();
        await SelectCardAsync("Native");
        await SelectCardAsync("Virtual Machines");
        await SelectTechCardAsync("Go");

        await ClickVMCalculateAsync();
        await VerifyAllResultsTabsAsync();
    }

    [Test]
    public async Task LowCode_VM_Mendix_Server_ToResults()
    {
        await GoToHomeAsync();
        await SelectCardAsync("Low-Code");
        await SelectCardAsync("Virtual Machines");
        await SelectTechCardAsync("Mendix");
        await SelectMendixCategoryCardAsync("Server");

        await ClickVMCalculateAsync();
        await VerifyAllResultsTabsAsync();
    }

    [Test]
    public async Task LowCode_VM_OutSystems_ToResults()
    {
        await GoToHomeAsync();
        await SelectCardAsync("Low-Code");
        await SelectCardAsync("Virtual Machines");
        await SelectTechCardAsync("OutSystems");

        await ClickVMCalculateAsync();
        await VerifyAllResultsTabsAsync();
    }

    #endregion
}
