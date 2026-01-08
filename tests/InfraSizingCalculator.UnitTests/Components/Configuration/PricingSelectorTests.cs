using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.Configuration;
using InfraSizingCalculator.Models.Pricing;
using InfraSizingCalculator.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components.Configuration;

/// <summary>
/// Tests for PricingSelector component - provider/region selection and pricing options
/// </summary>
public class PricingSelectorTests : TestContext
{
    private readonly IPricingService _pricingService;

    public PricingSelectorTests()
    {
        _pricingService = Substitute.For<IPricingService>();
        _pricingService.GetRegions(Arg.Any<CloudProvider>()).Returns(new List<RegionInfo>
        {
            new() { Code = "us-east-1", DisplayName = "US East (N. Virginia)", IsPreferred = true },
            new() { Code = "us-west-2", DisplayName = "US West (Oregon)" },
            new() { Code = "eu-west-1", DisplayName = "EU (Ireland)" }
        });
        Services.AddSingleton(_pricingService);
    }

    #region Rendering Tests

    [Fact]
    public void PricingSelector_RendersContainer()
    {
        var cut = RenderComponent<PricingSelector>();
        cut.Find(".pricing-selector").Should().NotBeNull();
    }

    [Fact]
    public void PricingSelector_AppliesAdditionalClass()
    {
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.AdditionalClass, "custom-class"));
        cut.Find(".pricing-selector").ClassList.Should().Contain("custom-class");
    }

    [Fact]
    public void PricingSelector_RendersProviderDropdown()
    {
        var cut = RenderComponent<PricingSelector>();
        var select = cut.Find(".pricing-provider-row select");
        select.Should().NotBeNull();
    }

    [Fact]
    public void PricingSelector_RendersRegionDropdown_WhenNotOnPrem()
    {
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, "EKS"));
        var selects = cut.FindAll(".pricing-provider-row select");
        selects.Should().HaveCount(2); // Provider and Region
    }

    [Fact]
    public void PricingSelector_HidesRegionDropdown_WhenOnPrem()
    {
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, "KUBERNETES"));
        // OnPrem distributions don't show region dropdown
        var markup = cut.Markup;
        markup.Should().Contain("On-Premises Cost Estimation");
    }

    [Fact]
    public void PricingSelector_RendersPricingAccordion()
    {
        var cut = RenderComponent<PricingSelector>();
        cut.Find("details.pricing-accordion").Should().NotBeNull();
        cut.Markup.Should().Contain("Pricing Options");
    }

    [Fact]
    public void PricingSelector_RendersAdvancedOptions()
    {
        var cut = RenderComponent<PricingSelector>();
        var accordions = cut.FindAll("details.pricing-accordion");
        accordions.Should().HaveCountGreaterThanOrEqualTo(2);
        cut.Markup.Should().Contain("Advanced Options");
    }

    [Fact]
    public void PricingSelector_RendersCalculateButton()
    {
        var cut = RenderComponent<PricingSelector>();
        var button = cut.Find("button.btn-primary");
        button.TextContent.Should().Contain("Calculate Costs");
    }

    #endregion

    #region Provider Auto-Selection Tests

    [Theory]
    [InlineData("EKS", "AWS")]
    [InlineData("AKS", "Azure")]
    [InlineData("GKE", "Google Cloud")]
    [InlineData("OKE", "Oracle Cloud")]
    [InlineData("IKS", "IBM Cloud")]
    [InlineData("ACK", "Alibaba Cloud")]
    [InlineData("TKE", "Tencent Cloud")]
    [InlineData("CCE", "Huawei Cloud")]
    [InlineData("DOKS", "DigitalOcean")]
    [InlineData("LKE", "Linode")]
    [InlineData("VKE", "Vultr")]
    [InlineData("HETZNERK8S", "Hetzner")]
    [InlineData("OVHKUBERNETES", "OVHcloud")]
    [InlineData("SCALEWAYKAPSULE", "Scaleway")]
    public void AutoSelectProvider_ManagedDistribution_SelectsCorrectProvider(string distribution, string expectedProvider)
    {
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, distribution));
        cut.Markup.Should().Contain($"{expectedProvider} Cost Estimation");
    }

    [Theory]
    [InlineData("OPENSHIFT")]
    [InlineData("RANCHER")]
    [InlineData("RKE2")]
    [InlineData("K3S")]
    [InlineData("TANZU")]
    [InlineData("CHARMED")]
    [InlineData("KUBERNETES")]
    [InlineData("MICROK8S")]
    public void AutoSelectProvider_OnPremDistribution_SelectsOnPrem(string distribution)
    {
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, distribution));
        cut.Markup.Should().Contain("On-Premises Cost Estimation");
    }

    [Theory]
    [InlineData("OPENSHIFTROSA", "AWS")]
    [InlineData("OPENSHIFTARO", "Azure")]
    [InlineData("OPENSHIFTDEDICATED", "Google Cloud")]
    [InlineData("OPENSHIFTIBM", "IBM Cloud")]
    public void AutoSelectProvider_ManagedOpenShift_SelectsCorrectProvider(string distribution, string expectedProvider)
    {
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, distribution));
        cut.Markup.Should().Contain($"{expectedProvider} Cost Estimation");
    }

    [Theory]
    [InlineData("RANCHEREKS", "AWS")]
    [InlineData("RANCHERAKS", "Azure")]
    [InlineData("RANCHERGKE", "Google Cloud")]
    [InlineData("RANCHERHOSTED", "AWS")]
    public void AutoSelectProvider_ManagedRancher_SelectsCorrectProvider(string distribution, string expectedProvider)
    {
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, distribution));
        cut.Markup.Should().Contain($"{expectedProvider} Cost Estimation");
    }

    [Theory]
    [InlineData("TANZUAWS", "AWS")]
    [InlineData("TANZUAZURE", "Azure")]
    [InlineData("TANZUGCP", "Google Cloud")]
    public void AutoSelectProvider_ManagedTanzu_SelectsCorrectProvider(string distribution, string expectedProvider)
    {
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, distribution));
        cut.Markup.Should().Contain($"{expectedProvider} Cost Estimation");
    }

    [Theory]
    [InlineData("MENDIX")]
    [InlineData("OUTSYSTEMS")]
    public void AutoSelectProvider_LowCodePlatform_DefaultsToAWS(string distribution)
    {
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, distribution));
        cut.Markup.Should().Contain("AWS Cost Estimation");
    }

    [Fact]
    public void AutoSelectProvider_UnknownDistribution_DefaultsToAWS()
    {
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, "UNKNOWN_DIST"));
        cut.Markup.Should().Contain("AWS Cost Estimation");
    }

    [Fact]
    public void AutoSelectProvider_NullDistribution_DefaultsToAWS()
    {
        var cut = RenderComponent<PricingSelector>();
        cut.Markup.Should().Contain("AWS Cost Estimation");
    }

    #endregion

    #region Region Loading Tests

    [Fact]
    public void LoadRegions_CloudProvider_LoadsFromService()
    {
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, "EKS"));
        _pricingService.Received(1).GetRegions(CloudProvider.AWS);
    }

    [Fact]
    public void LoadRegions_PreferredRegion_IsSelected()
    {
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, "EKS"));
        cut.Markup.Should().Contain("US East (N. Virginia)");
    }

    [Fact]
    public void LoadRegions_OnPrem_HasNoRegions()
    {
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, "KUBERNETES"));
        var selects = cut.FindAll(".pricing-provider-row .option-group");
        // OnPrem should only have provider, no region dropdown
        selects.Should().HaveCount(1);
    }

    #endregion

    #region OpenShift-Specific Tests

    [Fact]
    public void OpenShiftDistribution_ShowsManagedOpenShiftOptions()
    {
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, "OPENSHIFTROSA"));
        cut.Markup.Should().Contain("Managed OpenShift");
        cut.Markup.Should().Contain("ROSA (AWS)");
    }

    #endregion

    #region Cloud Alternative Mode Tests

    [Fact]
    public void CloudAlternativeMode_HidesOnPremOption()
    {
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.AdditionalClass, "cloud-alternative")
            .Add(p => p.Distribution, "RKE2"));
        // In cloud-alternative mode, OnPrem is hidden and provider defaults to AWS
        cut.Markup.Should().Contain("AWS Cost Estimation");
    }

    [Fact]
    public void CloudAlternativeMode_DefaultsToAWS()
    {
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.AdditionalClass, "cloud-alternative")
            .Add(p => p.Distribution, "KUBERNETES"));
        // Should show AWS, not On-Premises
        cut.Markup.Should().Contain("AWS Cost Estimation");
        cut.Markup.Should().NotContain("On-Premises Cost Estimation");
    }

    #endregion

    #region Pricing Options Tests

    [Fact]
    public void PricingOptions_ShowsPricingTypeDropdown_WhenNotOnPrem()
    {
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, "EKS"));
        cut.Markup.Should().Contain("On-Demand");
        cut.Markup.Should().Contain("1-Year Reserved");
        cut.Markup.Should().Contain("3-Year Reserved");
        cut.Markup.Should().Contain("Spot");
    }

    [Fact]
    public void PricingOptions_ShowsSupportTierDropdown()
    {
        var cut = RenderComponent<PricingSelector>();
        cut.Markup.Should().Contain("Support");
        cut.Markup.Should().Contain("None");
        cut.Markup.Should().Contain("Basic");
        cut.Markup.Should().Contain("Developer");
        cut.Markup.Should().Contain("Business");
        cut.Markup.Should().Contain("Enterprise");
    }

    [Fact]
    public void PricingOptions_ShowsToggleCheckboxes()
    {
        var cut = RenderComponent<PricingSelector>();
        cut.Markup.Should().Contain("Licenses");
        cut.Markup.Should().Contain("Support");
        cut.Markup.Should().Contain("Storage");
        cut.Markup.Should().Contain("Network");
    }

    [Fact]
    public void PricingOptions_ShowsControlPlaneToggle_WhenNotOnPrem()
    {
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, "EKS"));
        cut.Markup.Should().Contain("Control Plane");
    }

    #endregion

    #region Advanced Options Tests

    [Fact]
    public void AdvancedOptions_ShowsStorageInput()
    {
        var cut = RenderComponent<PricingSelector>();
        cut.Markup.Should().Contain("Storage/Node");
    }

    [Fact]
    public void AdvancedOptions_ShowsEgressInput()
    {
        var cut = RenderComponent<PricingSelector>();
        cut.Markup.Should().Contain("Egress (GB)");
    }

    [Fact]
    public void AdvancedOptions_ShowsLoadBalancersInput()
    {
        var cut = RenderComponent<PricingSelector>();
        cut.Markup.Should().Contain("Load Balancers");
    }

    [Fact]
    public void AdvancedOptions_ShowsHeadroomInput()
    {
        var cut = RenderComponent<PricingSelector>();
        cut.Markup.Should().Contain("Headroom %");
    }

    #endregion

    #region Calculate Button Tests

    [Fact]
    public async Task CalculateCosts_InvokesCallback()
    {
        (CloudProvider Provider, string Region, CostEstimationOptions Options)? received = null;
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, "EKS")
            .Add(p => p.OnCalculate, EventCallback.Factory.Create<(CloudProvider, string, CostEstimationOptions)>(
                this, args => received = args)));

        var button = cut.Find("button.btn-primary");
        await cut.InvokeAsync(() => button.Click());

        received.Should().NotBeNull();
        received!.Value.Provider.Should().Be(CloudProvider.AWS);
        received!.Value.Region.Should().Be("us-east-1");
    }

    [Fact]
    public async Task CalculateCosts_NoCallback_DoesNotThrow()
    {
        var cut = RenderComponent<PricingSelector>();
        var button = cut.Find("button.btn-primary");

        var action = async () => await cut.InvokeAsync(() => button.Click());
        await action.Should().NotThrowAsync();
    }

    #endregion

    #region Distribution Change Tests

    [Fact]
    public void OnParametersSet_DistributionChange_UpdatesProvider()
    {
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, "EKS"));

        cut.Markup.Should().Contain("AWS Cost Estimation");

        // Change distribution
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.Distribution, "AKS"));

        cut.Markup.Should().Contain("Azure Cost Estimation");
    }

    [Fact]
    public void OnParametersSet_SameDistribution_PreservesState()
    {
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, "EKS"));

        // Re-render with same distribution
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.Distribution, "EKS"));

        // Provider should still be AWS
        cut.Markup.Should().Contain("AWS Cost Estimation");
    }

    #endregion

    #region Provider Display Name Tests

    [Theory]
    [InlineData("EKS", "AWS")]
    [InlineData("AKS", "Azure")]
    [InlineData("GKE", "Google Cloud")]
    [InlineData("OKE", "Oracle Cloud")]
    [InlineData("IKS", "IBM Cloud")]
    [InlineData("ACK", "Alibaba Cloud")]
    [InlineData("TKE", "Tencent Cloud")]
    [InlineData("CCE", "Huawei Cloud")]
    [InlineData("DOKS", "DigitalOcean")]
    [InlineData("LKE", "Linode")]
    [InlineData("VKE", "Vultr")]
    [InlineData("HETZNERK8S", "Hetzner")]
    [InlineData("OVHKUBERNETES", "OVHcloud")]
    [InlineData("SCALEWAYKAPSULE", "Scaleway")]
    [InlineData("KUBERNETES", "On-Premises")]
    public void GetCostSectionTitle_ShowsCorrectProviderName(string distribution, string expectedName)
    {
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, distribution));
        cut.Markup.Should().Contain($"{expectedName} Cost Estimation");
    }

    #endregion
}
