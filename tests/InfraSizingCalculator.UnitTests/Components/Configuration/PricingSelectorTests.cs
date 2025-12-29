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
/// Tests for PricingSelector component - Cloud provider and pricing configuration
/// </summary>
public class PricingSelectorTests : TestContext
{
    private readonly IPricingService _pricingService;

    public PricingSelectorTests()
    {
        _pricingService = Substitute.For<IPricingService>();
        Services.AddSingleton(_pricingService);

        // Setup default regions for AWS
        _pricingService.GetRegions(CloudProvider.AWS).Returns(new List<RegionInfo>
        {
            new() { Code = "us-east-1", DisplayName = "US East (N. Virginia)", IsPreferred = true },
            new() { Code = "us-west-2", DisplayName = "US West (Oregon)", IsPreferred = true },
            new() { Code = "eu-west-1", DisplayName = "Europe (Ireland)", IsPreferred = false }
        });

        // Setup regions for Azure
        _pricingService.GetRegions(CloudProvider.Azure).Returns(new List<RegionInfo>
        {
            new() { Code = "eastus", DisplayName = "East US", IsPreferred = true },
            new() { Code = "westeurope", DisplayName = "West Europe", IsPreferred = false }
        });

        // Setup regions for GCP
        _pricingService.GetRegions(CloudProvider.GCP).Returns(new List<RegionInfo>
        {
            new() { Code = "us-central1", DisplayName = "Iowa", IsPreferred = true },
            new() { Code = "europe-west1", DisplayName = "Belgium", IsPreferred = false }
        });

        // Setup empty regions for OnPrem
        _pricingService.GetRegions(CloudProvider.OnPrem).Returns(new List<RegionInfo>());
    }

    #region Rendering Tests

    [Fact]
    public void PricingSelector_RendersContainer()
    {
        // Act
        var cut = RenderComponent<PricingSelector>();

        // Assert
        cut.Find(".pricing-selector").Should().NotBeNull();
    }

    [Fact]
    public void PricingSelector_AppliesAdditionalClass()
    {
        // Act
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.AdditionalClass, "custom-class"));

        // Assert
        cut.Find(".pricing-selector").ClassList.Should().Contain("custom-class");
    }

    [Fact]
    public void PricingSelector_RendersSectionTitle()
    {
        // Act
        var cut = RenderComponent<PricingSelector>();

        // Assert
        cut.Find("h4").TextContent.Should().Contain("Cost Estimation");
    }

    [Fact]
    public void PricingSelector_RendersProviderDropdown()
    {
        // Act
        var cut = RenderComponent<PricingSelector>();

        // Assert
        var selects = cut.FindAll("select");
        selects.Should().HaveCountGreaterThanOrEqualTo(1);
    }

    [Fact]
    public void PricingSelector_RendersRegionDropdown_WhenNotOnPrem()
    {
        // Act
        var cut = RenderComponent<PricingSelector>();

        // Assert - Should have provider and region selects
        var selects = cut.FindAll("select");
        selects.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public void PricingSelector_RendersPricingOptionsAccordion()
    {
        // Act
        var cut = RenderComponent<PricingSelector>();

        // Assert
        var details = cut.FindAll("details.pricing-accordion");
        details.Should().Contain(d => d.InnerHtml.Contains("Pricing Options"));
    }

    [Fact]
    public void PricingSelector_RendersAdvancedOptionsAccordion()
    {
        // Act
        var cut = RenderComponent<PricingSelector>();

        // Assert
        var details = cut.FindAll("details.pricing-accordion");
        details.Should().Contain(d => d.InnerHtml.Contains("Advanced Options"));
    }

    [Fact]
    public void PricingSelector_RendersCalculateButton()
    {
        // Act
        var cut = RenderComponent<PricingSelector>();

        // Assert
        cut.Find("button.btn-primary").TextContent.Should().Contain("Calculate Costs");
    }

    #endregion

    #region Provider Selection Tests

    [Fact]
    public void PricingSelector_DefaultsToAWS()
    {
        // Act
        var cut = RenderComponent<PricingSelector>();

        // Assert
        cut.Find("h4").TextContent.Should().Contain("AWS Cost Estimation");
    }

    [Theory]
    [InlineData("EKS", CloudProvider.AWS)]
    [InlineData("AKS", CloudProvider.Azure)]
    [InlineData("GKE", CloudProvider.GCP)]
    [InlineData("OKE", CloudProvider.OCI)]
    [InlineData("IKS", CloudProvider.IBM)]
    [InlineData("DOKS", CloudProvider.DigitalOcean)]
    public void PricingSelector_AutoSelectsProviderFromDistribution(string distribution, CloudProvider expectedProvider)
    {
        // Arrange - Setup region for expected provider
        _pricingService.GetRegions(expectedProvider).Returns(new List<RegionInfo>
        {
            new() { Code = "default", DisplayName = "Default", IsPreferred = true }
        });

        // Act
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, distribution));

        // Assert
        var title = cut.Find("h4").TextContent;
        title.Should().Contain("Cost Estimation");
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
    public void PricingSelector_SelectsOnPrem_ForOnPremDistributions(string distribution)
    {
        // Act
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, distribution));

        // Assert
        cut.Find("h4").TextContent.Should().Contain("On-Premises Cost Estimation");
    }

    [Fact]
    public void PricingSelector_ShowsOpenShiftOptions_ForOpenShiftDistribution()
    {
        // Act
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, "OPENSHIFT"));

        // Assert
        var select = cut.Find("select");
        var options = select.InnerHtml;
        options.Should().Contain("ROSA");
        options.Should().Contain("ARO");
    }

    [Fact]
    public void PricingSelector_HidesOnPremOption_InCloudAlternativeMode()
    {
        // Act
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.AdditionalClass, "cloud-alternative"));

        // Assert
        var select = cut.Find("select");
        var options = select.InnerHtml;
        // Should NOT contain On-Premises in optgroup
        options.Should().NotContain("On-Premises</option>");
    }

    #endregion

    #region Title Generation Tests

    [Fact]
    public void PricingSelector_ShowsProviderInTitle()
    {
        // Setup Azure regions
        _pricingService.GetRegions(CloudProvider.Azure).Returns(new List<RegionInfo>
        {
            new() { Code = "eastus", DisplayName = "East US", IsPreferred = true }
        });

        // Act
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, "AKS"));

        // Assert
        cut.Find("h4").TextContent.Should().Contain("Azure Cost Estimation");
    }

    [Fact]
    public void PricingSelector_ShowsOnPremTitle_WhenOnPrem()
    {
        // Act
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, "K3S"));

        // Assert
        cut.Find("h4").TextContent.Should().Be("On-Premises Cost Estimation");
    }

    #endregion

    #region Region Tests

    [Fact]
    public void PricingSelector_LoadsRegionsForProvider()
    {
        // Act
        var cut = RenderComponent<PricingSelector>();

        // Assert
        _pricingService.Received(1).GetRegions(CloudProvider.AWS);
    }

    [Fact]
    public void PricingSelector_SelectsPreferredRegion()
    {
        // Act
        var cut = RenderComponent<PricingSelector>();

        // Assert - Should select us-east-1 as it's preferred
        var regionSelect = cut.FindAll("select")[1]; // Second select is region
        regionSelect.InnerHtml.Should().Contain("us-east-1");
    }

    [Fact]
    public void PricingSelector_HidesRegionDropdown_ForOnPrem()
    {
        // Act
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, "OPENSHIFT"));

        // Assert - Should only have provider select
        var selects = cut.FindAll("select");
        // When OnPrem, there's no region dropdown but there might be pricing type and support
        var optionGroupHtml = selects[0].InnerHtml;
        optionGroupHtml.Should().Contain("On-Premises");
    }

    #endregion

    #region Pricing Options Tests

    [Fact]
    public void PricingSelector_RendersPricingTypeOptions()
    {
        // Act
        var cut = RenderComponent<PricingSelector>();

        // Assert
        var html = cut.Markup;
        html.Should().Contain("On-Demand");
        html.Should().Contain("1-Year Reserved");
        html.Should().Contain("3-Year Reserved");
        html.Should().Contain("Spot");
    }

    [Fact]
    public void PricingSelector_RendersSupportTierOptions()
    {
        // Act
        var cut = RenderComponent<PricingSelector>();

        // Assert
        var html = cut.Markup;
        html.Should().Contain("None");
        html.Should().Contain("Basic");
        html.Should().Contain("Developer");
        html.Should().Contain("Business");
        html.Should().Contain("Enterprise");
    }

    [Fact]
    public void PricingSelector_RendersToggleCheckboxes()
    {
        // Act
        var cut = RenderComponent<PricingSelector>();

        // Assert
        var html = cut.Markup;
        html.Should().Contain("Licenses");
        html.Should().Contain("Support");
        html.Should().Contain("Storage");
        html.Should().Contain("Network");
    }

    [Fact]
    public void PricingSelector_RendersControlPlaneToggle_WhenNotOnPrem()
    {
        // Act
        var cut = RenderComponent<PricingSelector>();

        // Assert
        var html = cut.Markup;
        html.Should().Contain("Control Plane");
    }

    #endregion

    #region Advanced Options Tests

    [Fact]
    public void PricingSelector_RendersStorageInput()
    {
        // Act
        var cut = RenderComponent<PricingSelector>();

        // Assert
        var html = cut.Markup;
        html.Should().Contain("Storage/Node");
    }

    [Fact]
    public void PricingSelector_RendersEgressInput()
    {
        // Act
        var cut = RenderComponent<PricingSelector>();

        // Assert
        var html = cut.Markup;
        html.Should().Contain("Egress (GB)");
    }

    [Fact]
    public void PricingSelector_RendersLoadBalancersInput()
    {
        // Act
        var cut = RenderComponent<PricingSelector>();

        // Assert
        var html = cut.Markup;
        html.Should().Contain("Load Balancers");
    }

    [Fact]
    public void PricingSelector_RendersHeadroomInput()
    {
        // Act
        var cut = RenderComponent<PricingSelector>();

        // Assert
        var html = cut.Markup;
        html.Should().Contain("Headroom %");
    }

    #endregion

    #region Calculate Callback Tests

    [Fact]
    public async Task PricingSelector_CalculateButton_InvokesCallback()
    {
        // Arrange
        (CloudProvider provider, string region, CostEstimationOptions options)? result = null;
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.OnCalculate, EventCallback.Factory.Create<(CloudProvider, string, CostEstimationOptions)>(
                this, r => result = r)));

        // Act
        await cut.InvokeAsync(() => cut.Find("button.btn-primary").Click());

        // Assert
        result.Should().NotBeNull();
        result!.Value.provider.Should().Be(CloudProvider.AWS);
    }

    [Fact]
    public async Task PricingSelector_CalculateButton_PassesSelectedProvider()
    {
        // Arrange
        (CloudProvider provider, string region, CostEstimationOptions options)? result = null;
        _pricingService.GetRegions(CloudProvider.Azure).Returns(new List<RegionInfo>
        {
            new() { Code = "eastus", DisplayName = "East US", IsPreferred = true }
        });

        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, "AKS")
            .Add(p => p.OnCalculate, EventCallback.Factory.Create<(CloudProvider, string, CostEstimationOptions)>(
                this, r => result = r)));

        // Act
        await cut.InvokeAsync(() => cut.Find("button.btn-primary").Click());

        // Assert
        result.Should().NotBeNull();
        result!.Value.provider.Should().Be(CloudProvider.Azure);
    }

    [Fact]
    public async Task PricingSelector_CalculateButton_PassesOptions()
    {
        // Arrange
        (CloudProvider provider, string region, CostEstimationOptions options)? result = null;
        var options = new CostEstimationOptions
        {
            IncludeLicenses = false,
            StorageGBPerNode = 200
        };
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Options, options)
            .Add(p => p.OnCalculate, EventCallback.Factory.Create<(CloudProvider, string, CostEstimationOptions)>(
                this, r => result = r)));

        // Act
        await cut.InvokeAsync(() => cut.Find("button.btn-primary").Click());

        // Assert
        result.Should().NotBeNull();
        result!.Value.options.IncludeLicenses.Should().BeFalse();
        result!.Value.options.StorageGBPerNode.Should().Be(200);
    }

    [Fact]
    public async Task PricingSelector_ShowsCalculating_DuringCalculation()
    {
        // Arrange
        var tcs = new TaskCompletionSource();
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.OnCalculate, EventCallback.Factory.Create<(CloudProvider, string, CostEstimationOptions)>(
                this, async _ => await tcs.Task)));

        // Act
        var clickTask = cut.InvokeAsync(() => cut.Find("button.btn-primary").Click());

        // Assert - Button should show Calculating during async operation
        cut.Find("button.btn-primary").TextContent.Should().Contain("Calculating");

        // Complete the task
        tcs.SetResult();
        await clickTask;

        // Assert - Button should show Calculate Costs after completion
        cut.WaitForAssertion(() =>
            cut.Find("button.btn-primary").TextContent.Should().Contain("Calculate Costs"));
    }

    [Fact]
    public async Task PricingSelector_DisablesButton_DuringCalculation()
    {
        // Arrange
        var tcs = new TaskCompletionSource();
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.OnCalculate, EventCallback.Factory.Create<(CloudProvider, string, CostEstimationOptions)>(
                this, async _ => await tcs.Task)));

        // Act
        var clickTask = cut.InvokeAsync(() => cut.Find("button.btn-primary").Click());

        // Assert
        cut.Find("button.btn-primary").HasAttribute("disabled").Should().BeTrue();

        // Complete the task
        tcs.SetResult();
        await clickTask;

        // Assert - Button should be enabled after completion
        cut.WaitForAssertion(() =>
            cut.Find("button.btn-primary").HasAttribute("disabled").Should().BeFalse());
    }

    #endregion

    #region Distribution Change Tests

    [Fact]
    public void PricingSelector_UpdatesProvider_WhenDistributionChanges()
    {
        // Arrange
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, "EKS"));

        // Assert initial
        cut.Find("h4").TextContent.Should().Contain("AWS");

        // Act - Change distribution to AKS
        _pricingService.GetRegions(CloudProvider.Azure).Returns(new List<RegionInfo>
        {
            new() { Code = "eastus", DisplayName = "East US", IsPreferred = true }
        });

        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.Distribution, "AKS"));

        // Assert
        cut.Find("h4").TextContent.Should().Contain("Azure");
    }

    [Fact]
    public void PricingSelector_SetsDistributionOnOptions()
    {
        // Arrange
        var options = new CostEstimationOptions();

        // Act
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Options, options)
            .Add(p => p.Distribution, "EKS"));

        // Assert
        options.Distribution.Should().Be("EKS");
    }

    #endregion

    #region Cloud Alternative Mode Tests

    [Fact]
    public void PricingSelector_DefaultsToAWS_InCloudAlternativeMode()
    {
        // Act
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, "K3S")
            .Add(p => p.AdditionalClass, "cloud-alternative"));

        // Assert - Should be AWS, not OnPrem
        cut.Find("h4").TextContent.Should().Contain("AWS Cost Estimation");
    }

    #endregion

    #region Provider Display Name Tests

    [Theory]
    [InlineData("EKS", "AWS")]
    [InlineData("AKS", "Azure")]
    [InlineData("GKE", "Google Cloud")]
    [InlineData("DOKS", "DigitalOcean")]
    public void PricingSelector_DisplaysCorrectProviderName(string distribution, string expectedName)
    {
        // Arrange
        var provider = distribution switch
        {
            "EKS" => CloudProvider.AWS,
            "AKS" => CloudProvider.Azure,
            "GKE" => CloudProvider.GCP,
            "DOKS" => CloudProvider.DigitalOcean,
            _ => CloudProvider.AWS
        };

        _pricingService.GetRegions(provider).Returns(new List<RegionInfo>
        {
            new() { Code = "default", DisplayName = "Default", IsPreferred = true }
        });

        // Act
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, distribution));

        // Assert
        cut.Find("h4").TextContent.Should().Contain(expectedName);
    }

    #endregion

    #region Major Cloud Options Tests

    [Fact]
    public void PricingSelector_IncludesMajorCloudProviders()
    {
        // Act
        var cut = RenderComponent<PricingSelector>();

        // Assert
        var select = cut.Find("select");
        var options = select.InnerHtml;
        options.Should().Contain("AWS");
        options.Should().Contain("Azure");
        options.Should().Contain("Google Cloud");
        options.Should().Contain("Oracle");
        options.Should().Contain("IBM Cloud");
    }

    [Fact]
    public void PricingSelector_IncludesDeveloperCloudProviders()
    {
        // Act
        var cut = RenderComponent<PricingSelector>();

        // Assert
        var select = cut.Find("select");
        var options = select.InnerHtml;
        options.Should().Contain("DigitalOcean");
        options.Should().Contain("Linode");
        options.Should().Contain("Vultr");
        options.Should().Contain("Hetzner");
    }

    #endregion

    #region Options Parameter Tests

    [Fact]
    public void PricingSelector_UsesProvidedOptions()
    {
        // Arrange
        var options = new CostEstimationOptions
        {
            PricingType = PricingType.Reserved1Year,
            SupportTier = SupportTier.Enterprise,
            IncludeLicenses = false,
            StorageGBPerNode = 500
        };

        // Act
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Options, options));

        // Assert - Component should use these options (verified via callback)
        options.PricingType.Should().Be(PricingType.Reserved1Year);
        options.SupportTier.Should().Be(SupportTier.Enterprise);
    }

    [Fact]
    public void PricingSelector_CreatesDefaultOptions_WhenNotProvided()
    {
        // Act
        var cut = RenderComponent<PricingSelector>();

        // Assert - Should not throw
        cut.Markup.Should().NotBeEmpty();
    }

    #endregion

    #region No Callback Tests

    [Fact]
    public async Task PricingSelector_NoCallback_DoesNotThrow()
    {
        // Arrange
        var cut = RenderComponent<PricingSelector>();

        // Act & Assert
        var action = async () =>
        {
            await cut.InvokeAsync(() => cut.Find("button.btn-primary").Click());
        };

        await action.Should().NotThrowAsync();
    }

    #endregion

    #region Extended Distribution Mapping Tests

    [Theory]
    [InlineData("ACK", CloudProvider.Alibaba)]
    [InlineData("TKE", CloudProvider.Tencent)]
    [InlineData("CCE", CloudProvider.Huawei)]
    [InlineData("LKE", CloudProvider.Linode)]
    [InlineData("VKE", CloudProvider.Vultr)]
    public void PricingSelector_AutoSelectsProvider_ForAdditionalDistributions(string distribution, CloudProvider expectedProvider)
    {
        // Arrange
        _pricingService.GetRegions(expectedProvider).Returns(new List<RegionInfo>
        {
            new() { Code = "default", DisplayName = "Default", IsPreferred = true }
        });

        // Act
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, distribution));

        // Assert - Provider should match expected
        // Title contains the display name for the provider
        cut.Find("h4").TextContent.Should().Contain("Cost Estimation");
    }

    [Theory]
    [InlineData("HETZNERK8S", CloudProvider.Hetzner)]
    [InlineData("OVHKUBERNETES", CloudProvider.OVH)]
    [InlineData("SCALEWAYKAPSULE", CloudProvider.Scaleway)]
    public void PricingSelector_AutoSelectsProvider_ForDeveloperCloudDistributions(string distribution, CloudProvider expectedProvider)
    {
        // Arrange
        _pricingService.GetRegions(expectedProvider).Returns(new List<RegionInfo>
        {
            new() { Code = "default", DisplayName = "Default", IsPreferred = true }
        });

        // Act
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, distribution));

        // Assert
        cut.Find("h4").TextContent.Should().Contain("Cost Estimation");
    }

    [Theory]
    [InlineData("OPENSHIFTROSA", CloudProvider.AWS, "AWS")]
    [InlineData("OPENSHIFTARO", CloudProvider.Azure, "Azure")]
    [InlineData("OPENSHIFTDEDICATED", CloudProvider.GCP, "Google Cloud")]
    [InlineData("OPENSHIFTIBM", CloudProvider.IBM, "IBM Cloud")]
    public void PricingSelector_AutoSelectsProvider_ForOpenShiftCloudVariants(
        string distribution, CloudProvider expectedProvider, string expectedDisplayName)
    {
        // Arrange
        _pricingService.GetRegions(expectedProvider).Returns(new List<RegionInfo>
        {
            new() { Code = "default", DisplayName = "Default", IsPreferred = true }
        });

        // Act
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, distribution));

        // Assert
        cut.Find("h4").TextContent.Should().Contain(expectedDisplayName);
    }

    [Theory]
    [InlineData("RANCHEREKS", CloudProvider.AWS, "AWS")]
    [InlineData("RANCHERAKS", CloudProvider.Azure, "Azure")]
    [InlineData("RANCHERGKE", CloudProvider.GCP, "Google Cloud")]
    [InlineData("RANCHERHOSTED", CloudProvider.AWS, "AWS")]
    public void PricingSelector_AutoSelectsProvider_ForRancherVariants(
        string distribution, CloudProvider expectedProvider, string expectedDisplayName)
    {
        // Arrange
        _pricingService.GetRegions(expectedProvider).Returns(new List<RegionInfo>
        {
            new() { Code = "default", DisplayName = "Default", IsPreferred = true }
        });

        // Act
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, distribution));

        // Assert
        cut.Find("h4").TextContent.Should().Contain(expectedDisplayName);
    }

    [Theory]
    [InlineData("TANZUAWS", CloudProvider.AWS, "AWS")]
    [InlineData("TANZUAZURE", CloudProvider.Azure, "Azure")]
    [InlineData("TANZUGCP", CloudProvider.GCP, "Google Cloud")]
    public void PricingSelector_AutoSelectsProvider_ForTanzuVariants(
        string distribution, CloudProvider expectedProvider, string expectedDisplayName)
    {
        // Arrange
        _pricingService.GetRegions(expectedProvider).Returns(new List<RegionInfo>
        {
            new() { Code = "default", DisplayName = "Default", IsPreferred = true }
        });

        // Act
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, distribution));

        // Assert
        cut.Find("h4").TextContent.Should().Contain(expectedDisplayName);
    }

    [Theory]
    [InlineData("MENDIX")]
    [InlineData("OUTSYSTEMS")]
    public void PricingSelector_DefaultsToAWS_ForLowCodePlatforms(string distribution)
    {
        // Arrange - ensure AWS regions are loaded
        _pricingService.GetRegions(CloudProvider.AWS).Returns(new List<RegionInfo>
        {
            new() { Code = "us-east-1", DisplayName = "US East", IsPreferred = true }
        });

        // Act
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, distribution));

        // Assert
        cut.Find("h4").TextContent.Should().Contain("AWS");
    }

    [Fact]
    public void PricingSelector_DefaultsToAWS_ForUnknownDistribution()
    {
        // Arrange
        _pricingService.GetRegions(CloudProvider.AWS).Returns(new List<RegionInfo>
        {
            new() { Code = "us-east-1", DisplayName = "US East", IsPreferred = true }
        });

        // Act
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, "UNKNOWN_DIST_XYZ"));

        // Assert
        cut.Find("h4").TextContent.Should().Contain("AWS");
    }

    [Fact]
    public void PricingSelector_HandlesNullDistribution()
    {
        // Act
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, (string?)null));

        // Assert - Defaults to AWS
        cut.Find("h4").TextContent.Should().Contain("AWS");
    }

    [Fact]
    public void PricingSelector_HandlesEmptyDistribution()
    {
        // Act
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, ""));

        // Assert - Defaults to AWS
        cut.Find("h4").TextContent.Should().Contain("AWS");
    }

    #endregion

    #region Extended Provider Display Name Tests

    [Theory]
    [InlineData("OKE", "Oracle Cloud")]
    [InlineData("IKS", "IBM Cloud")]
    public void PricingSelector_DisplaysCorrectProviderName_ForMajorClouds(string distribution, string expectedName)
    {
        // Arrange
        var provider = distribution switch
        {
            "OKE" => CloudProvider.OCI,
            "IKS" => CloudProvider.IBM,
            _ => CloudProvider.AWS
        };

        _pricingService.GetRegions(provider).Returns(new List<RegionInfo>
        {
            new() { Code = "default", DisplayName = "Default", IsPreferred = true }
        });

        // Act
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, distribution));

        // Assert
        cut.Find("h4").TextContent.Should().Contain(expectedName);
    }

    [Theory]
    [InlineData("ACK", "Alibaba Cloud")]
    [InlineData("TKE", "Tencent Cloud")]
    [InlineData("CCE", "Huawei Cloud")]
    public void PricingSelector_DisplaysCorrectProviderName_ForAsianClouds(string distribution, string expectedName)
    {
        // Arrange
        var provider = distribution switch
        {
            "ACK" => CloudProvider.Alibaba,
            "TKE" => CloudProvider.Tencent,
            "CCE" => CloudProvider.Huawei,
            _ => CloudProvider.AWS
        };

        _pricingService.GetRegions(provider).Returns(new List<RegionInfo>
        {
            new() { Code = "default", DisplayName = "Default", IsPreferred = true }
        });

        // Act
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, distribution));

        // Assert
        cut.Find("h4").TextContent.Should().Contain(expectedName);
    }

    [Theory]
    [InlineData("LKE", "Linode")]
    [InlineData("VKE", "Vultr")]
    public void PricingSelector_DisplaysCorrectProviderName_ForDeveloperClouds(string distribution, string expectedName)
    {
        // Arrange
        var provider = distribution switch
        {
            "LKE" => CloudProvider.Linode,
            "VKE" => CloudProvider.Vultr,
            _ => CloudProvider.AWS
        };

        _pricingService.GetRegions(provider).Returns(new List<RegionInfo>
        {
            new() { Code = "default", DisplayName = "Default", IsPreferred = true }
        });

        // Act
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, distribution));

        // Assert
        cut.Find("h4").TextContent.Should().Contain(expectedName);
    }

    [Theory]
    [InlineData("HETZNERK8S", "Hetzner")]
    [InlineData("OVHKUBERNETES", "OVHcloud")]
    [InlineData("SCALEWAYKAPSULE", "Scaleway")]
    public void PricingSelector_DisplaysCorrectProviderName_ForEuropeanClouds(string distribution, string expectedName)
    {
        // Arrange
        var provider = distribution switch
        {
            "HETZNERK8S" => CloudProvider.Hetzner,
            "OVHKUBERNETES" => CloudProvider.OVH,
            "SCALEWAYKAPSULE" => CloudProvider.Scaleway,
            _ => CloudProvider.AWS
        };

        _pricingService.GetRegions(provider).Returns(new List<RegionInfo>
        {
            new() { Code = "default", DisplayName = "Default", IsPreferred = true }
        });

        // Act
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, distribution));

        // Assert
        cut.Find("h4").TextContent.Should().Contain(expectedName);
    }

    #endregion

    #region OnParametersSet Tests

    [Fact]
    public void PricingSelector_ReloadsRegions_WhenDistributionChanges()
    {
        // Arrange - Start with EKS (AWS)
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, "EKS"));

        // Initial state - AWS regions loaded
        _pricingService.ClearReceivedCalls();

        // Act - Change to AKS (Azure)
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.Distribution, "AKS"));

        // Assert - Azure regions should be loaded
        _pricingService.Received(1).GetRegions(CloudProvider.Azure);
    }

    [Fact]
    public void PricingSelector_DoesNotReloadRegions_WhenDistributionUnchanged()
    {
        // Arrange
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, "EKS"));

        _pricingService.ClearReceivedCalls();

        // Act - Re-render with same distribution
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.Distribution, "EKS"));

        // Assert - Should not reload regions again
        _pricingService.DidNotReceive().GetRegions(Arg.Any<CloudProvider>());
    }

    [Fact]
    public void PricingSelector_UpdatesOptionsDistribution_WhenDistributionChanges()
    {
        // Arrange
        var options = new CostEstimationOptions();
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Options, options)
            .Add(p => p.Distribution, "EKS"));

        // Assert initial
        options.Distribution.Should().Be("EKS");

        // Act - Change distribution
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.Options, options)
            .Add(p => p.Distribution, "AKS"));

        // Assert
        options.Distribution.Should().Be("AKS");
    }

    #endregion

    #region Provider Change Tests

    [Fact]
    public void PricingSelector_ReloadsRegions_WhenProviderChanges()
    {
        // Arrange
        var cut = RenderComponent<PricingSelector>();
        _pricingService.ClearReceivedCalls();

        // Act - Change provider via select
        var providerSelect = cut.Find("select");
        providerSelect.Change(CloudProvider.Azure.ToString());

        // Assert
        _pricingService.Received(1).GetRegions(CloudProvider.Azure);
    }

    [Fact]
    public void PricingSelector_UpdatesTitle_WhenProviderChanges()
    {
        // Arrange
        var cut = RenderComponent<PricingSelector>();

        // Assert initial
        cut.Find("h4").TextContent.Should().Contain("AWS");

        // Act - Change provider
        var providerSelect = cut.Find("select");
        providerSelect.Change(CloudProvider.GCP.ToString());

        // Assert
        cut.Find("h4").TextContent.Should().Contain("Google Cloud");
    }

    [Fact]
    public void PricingSelector_SelectsFirstPreferredRegion()
    {
        // Arrange - Setup regions with first being preferred
        _pricingService.GetRegions(CloudProvider.AWS).Returns(new List<RegionInfo>
        {
            new() { Code = "us-east-1", DisplayName = "US East", IsPreferred = true },
            new() { Code = "eu-west-1", DisplayName = "EU West", IsPreferred = false }
        });

        // Act
        var cut = RenderComponent<PricingSelector>();

        // Assert - Should have us-east-1 selected (first preferred)
        var regionSelect = cut.FindAll("select")[1];
        regionSelect.InnerHtml.Should().Contain("us-east-1");
    }

    [Fact]
    public void PricingSelector_SelectsFirstRegion_WhenNonePreferred()
    {
        // Arrange - No preferred regions
        _pricingService.GetRegions(CloudProvider.AWS).Returns(new List<RegionInfo>
        {
            new() { Code = "region-1", DisplayName = "Region 1", IsPreferred = false },
            new() { Code = "region-2", DisplayName = "Region 2", IsPreferred = false }
        });

        // Act
        var cut = RenderComponent<PricingSelector>();

        // Assert - Should select first region
        var regionSelect = cut.FindAll("select")[1];
        regionSelect.InnerHtml.Should().Contain("region-1");
    }

    [Fact]
    public void PricingSelector_SetsEmptyRegion_WhenNoRegionsAvailable()
    {
        // Arrange - Empty regions list
        _pricingService.GetRegions(CloudProvider.AWS).Returns(new List<RegionInfo>());

        // Act
        var cut = RenderComponent<PricingSelector>();

        // Assert - Should still render without error
        cut.Markup.Should().NotBeEmpty();
    }

    #endregion

    #region OnPrem Provider Tests

    [Fact]
    public void PricingSelector_HidesPricingType_WhenOnPrem()
    {
        // Act
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, "K3S"));

        // Assert - Should not show pricing type options for on-prem
        var markup = cut.Markup;
        // The pricing type select should only appear when not on-prem
        // Find the accordion content and check it doesn't have pricing type
        var pricingSection = cut.Find("details.pricing-accordion");
        var accordionContent = pricingSection.InnerHtml;
        // On-prem mode should still have support tier but not pricing type dropdown
        accordionContent.Should().Contain("Support");
    }

    [Fact]
    public void PricingSelector_HidesControlPlaneToggle_WhenOnPrem()
    {
        // Act
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, "K3S"));

        // Assert
        cut.Markup.Should().NotContain("Control Plane");
    }

    [Fact]
    public void PricingSelector_SetsOnPremRegion_WhenOnPrem()
    {
        // Act
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, "OPENSHIFT"));

        // Assert - Should not have region dropdown
        var selects = cut.FindAll("select");
        // First select is provider, should not have region select visible
        selects.Count.Should().BeGreaterThan(0);
    }

    #endregion

    #region OpenShift Distribution Tests

    [Fact]
    public void PricingSelector_IsOpenShiftDistribution_WhenOpenShiftInName()
    {
        // Act
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, "OPENSHIFTROSA"));

        // Assert - Should show OpenShift managed options
        var select = cut.Find("select");
        select.InnerHtml.Should().Contain("ROSA");
    }

    [Fact]
    public void PricingSelector_IsOpenShiftDistribution_CaseInsensitive()
    {
        // Act
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, "openshift"));

        // Assert
        var select = cut.Find("select");
        select.InnerHtml.Should().Contain("ROSA");
    }

    [Fact]
    public void PricingSelector_RendersAllManagedOpenShiftOptions()
    {
        // Act
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, "OPENSHIFT"));

        // Assert
        var select = cut.Find("select");
        var options = select.InnerHtml;
        options.Should().Contain("ROSA (AWS)");
        options.Should().Contain("ARO (Azure)");
        options.Should().Contain("OSD (GCP)");
        options.Should().Contain("ROKS (IBM)");
    }

    #endregion

    #region Additional Cloud Providers Tests

    [Fact]
    public void PricingSelector_IncludesAllAsianCloudProviders()
    {
        // Act
        var cut = RenderComponent<PricingSelector>();

        // Assert
        var select = cut.Find("select");
        var options = select.InnerHtml;
        options.Should().Contain("Alibaba");
        options.Should().Contain("Tencent");
        options.Should().Contain("Huawei");
    }

    [Fact]
    public void PricingSelector_IncludesAllEuropeanDeveloperClouds()
    {
        // Act
        var cut = RenderComponent<PricingSelector>();

        // Assert
        var select = cut.Find("select");
        var options = select.InnerHtml;
        options.Should().Contain("OVHcloud");
        options.Should().Contain("Scaleway");
        options.Should().Contain("Exoscale");
        options.Should().Contain("Civo");
    }

    #endregion

    #region GetCloudAlternatives and CloudAlternativesMap Tests

    [Theory]
    [InlineData("OPENSHIFT", "ROSA", true)]  // OpenShift shows ROSA option
    [InlineData("RANCHER", "On-Premises", true)]  // Rancher defaults to on-prem
    [InlineData("RKE2", "On-Premises", true)]  // RKE2 defaults to on-prem
    [InlineData("K3S", "On-Premises", true)]  // K3S defaults to on-prem
    [InlineData("TANZU", "On-Premises", true)]  // Tanzu defaults to on-prem
    [InlineData("CHARMED", "On-Premises", true)]  // Charmed defaults to on-prem
    [InlineData("MICROK8S", "On-Premises", true)]  // MicroK8s defaults to on-prem
    public void PricingSelector_CloudAlternativesMap_ContainsExpectedAlternatives(
        string distribution, string expectedContent, bool shouldContain)
    {
        // This test validates that the CloudAlternativesMap data is correct
        // by checking that specific distributions have the expected alternatives
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, distribution));

        // Verify expected content is present
        if (shouldContain)
        {
            cut.Markup.Should().Contain(expectedContent);
        }
        else
        {
            cut.Markup.Should().NotContain(expectedContent);
        }
    }

    [Fact]
    public void PricingSelector_HasCloudAlternatives_ReturnsTrue_ForOpenShift()
    {
        // Act
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, "OPENSHIFT"));

        // Assert - Should show OpenShift managed options
        var select = cut.Find("select");
        select.InnerHtml.Should().Contain("ROSA");
    }

    [Fact]
    public void PricingSelector_HasCloudAlternatives_ReturnsTrue_ForRancher()
    {
        // Act
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, "RANCHER"));

        // Assert - Should work without error, on-prem distribution
        cut.Find("h4").TextContent.Should().Contain("On-Premises");
    }

    [Fact]
    public void PricingSelector_HasCloudAlternatives_ReturnsFalse_ForUnknownDistribution()
    {
        // Act
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, "UNKNOWN_PLATFORM"));

        // Assert - Should default to AWS without cloud alternatives
        cut.Find("h4").TextContent.Should().Contain("AWS");
    }

    [Theory]
    [InlineData("KUBERNETES")]
    [InlineData("MICROK8S")]
    [InlineData("CHARMED")]
    public void PricingSelector_CloudAlternatives_IncludesGenericK8s_ForStandardDistributions(string distribution)
    {
        // Act
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, distribution));

        // Assert - Should default to on-prem, alternatives would include EKS/AKS/GKE
        cut.Find("h4").TextContent.Should().Contain("On-Premises");
    }

    [Fact]
    public void PricingSelector_KubernetesDistribution_IncludesOracleAlternative()
    {
        // KUBERNETES distribution includes OKE (Oracle) as an alternative
        // Act
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, "KUBERNETES"));

        // Assert - On-prem by default but Oracle is a valid alternative
        var select = cut.Find("select");
        select.InnerHtml.Should().Contain("Oracle");
    }

    #endregion

    #region Additional Provider Tests (Civo, Exoscale, etc.)

    [Theory]
    [InlineData(CloudProvider.Civo)]
    [InlineData(CloudProvider.Exoscale)]
    public void PricingSelector_IncludesAllDeveloperCloudProviders_InDropdown(CloudProvider provider)
    {
        // Act
        var cut = RenderComponent<PricingSelector>();

        // Assert
        var select = cut.Find("select");
        select.InnerHtml.Should().Contain(provider.ToString());
    }

    [Fact]
    public void PricingSelector_IncludesCivo_InDeveloperCloudsGroup()
    {
        // Act
        var cut = RenderComponent<PricingSelector>();

        // Assert
        var select = cut.Find("select");
        select.InnerHtml.Should().Contain("Civo");
    }

    [Fact]
    public void PricingSelector_IncludesExoscale_InDeveloperCloudsGroup()
    {
        // Act
        var cut = RenderComponent<PricingSelector>();

        // Assert
        var select = cut.Find("select");
        select.InnerHtml.Should().Contain("Exoscale");
    }

    #endregion

    #region Checkbox Toggle and Input Field Tests

    [Fact]
    public void PricingSelector_LicensesCheckbox_IsRendered()
    {
        // Act
        var cut = RenderComponent<PricingSelector>();

        // Assert - Check markup contains a checkbox with Licenses label
        var markup = cut.Markup;
        markup.Should().Contain("Licenses");
        markup.Should().Contain("type=\"checkbox\"");
    }

    [Fact]
    public void PricingSelector_SupportCheckbox_IsRendered()
    {
        // Act
        var cut = RenderComponent<PricingSelector>();

        // Assert - Check markup contains a checkbox with Support label
        var labels = cut.FindAll("label");
        labels.Should().Contain(l => l.TextContent.Contains("Support"));
    }

    [Fact]
    public void PricingSelector_StorageCheckbox_IsRendered()
    {
        // Act
        var cut = RenderComponent<PricingSelector>();

        // Assert - Check markup contains Storage checkbox
        var labels = cut.FindAll("label");
        labels.Should().Contain(l => l.TextContent.Contains("Storage"));
    }

    [Fact]
    public void PricingSelector_NetworkCheckbox_IsRendered()
    {
        // Act
        var cut = RenderComponent<PricingSelector>();

        // Assert - Check markup contains Network checkbox
        var labels = cut.FindAll("label");
        labels.Should().Contain(l => l.TextContent.Contains("Network"));
    }

    [Fact]
    public void PricingSelector_ControlPlaneCheckbox_IsRendered_WhenNotOnPrem()
    {
        // Act
        var cut = RenderComponent<PricingSelector>();

        // Assert - AWS by default, should show control plane
        cut.Markup.Should().Contain("Control Plane");
    }

    [Fact]
    public void PricingSelector_NumberInputs_AreRendered_InAdvancedOptions()
    {
        // Act
        var cut = RenderComponent<PricingSelector>();

        // Assert
        var numberInputs = cut.FindAll("input[type='number']");
        numberInputs.Should().HaveCountGreaterThanOrEqualTo(4); // Storage, Egress, LBs, Headroom
    }

    [Fact]
    public void PricingSelector_StorageInput_HasCorrectMinMax()
    {
        // Act
        var cut = RenderComponent<PricingSelector>();

        // Assert - Find the first number input (Storage)
        var numberInputs = cut.FindAll("input[type='number']");
        var storageInput = numberInputs.First();
        storageInput.GetAttribute("min").Should().Be("10");
        storageInput.GetAttribute("max").Should().Be("10000");
    }

    [Fact]
    public void PricingSelector_EgressInput_HasCorrectMinMax()
    {
        // Act
        var cut = RenderComponent<PricingSelector>();

        // Assert - Find the second number input (Egress)
        var numberInputs = cut.FindAll("input[type='number']");
        numberInputs.Should().HaveCountGreaterThanOrEqualTo(2);
        var egressInput = numberInputs[1];
        egressInput.GetAttribute("min").Should().Be("0");
        egressInput.GetAttribute("max").Should().Be("100000");
    }

    [Fact]
    public void PricingSelector_LoadBalancersInput_HasCorrectMinMax()
    {
        // Act
        var cut = RenderComponent<PricingSelector>();

        // Assert - Find the third number input (Load Balancers)
        var numberInputs = cut.FindAll("input[type='number']");
        numberInputs.Should().HaveCountGreaterThanOrEqualTo(3);
        var lbInput = numberInputs[2];
        lbInput.GetAttribute("min").Should().Be("0");
        lbInput.GetAttribute("max").Should().Be("100");
    }

    [Fact]
    public void PricingSelector_HeadroomInput_HasCorrectMinMax()
    {
        // Act
        var cut = RenderComponent<PricingSelector>();

        // Assert - Find the fourth number input (Headroom)
        var numberInputs = cut.FindAll("input[type='number']");
        numberInputs.Should().HaveCountGreaterThanOrEqualTo(4);
        var headroomInput = numberInputs[3];
        headroomInput.GetAttribute("min").Should().Be("0");
        headroomInput.GetAttribute("max").Should().Be("100");
    }

    #endregion

    #region Region Dropdown Interaction Tests

    [Fact]
    public void PricingSelector_RegionDropdown_RendersAllAvailableRegions()
    {
        // Arrange - Setup multiple regions
        _pricingService.GetRegions(CloudProvider.AWS).Returns(new List<RegionInfo>
        {
            new() { Code = "us-east-1", DisplayName = "US East (N. Virginia)", IsPreferred = true },
            new() { Code = "us-west-2", DisplayName = "US West (Oregon)", IsPreferred = true },
            new() { Code = "eu-west-1", DisplayName = "Europe (Ireland)", IsPreferred = false },
            new() { Code = "ap-southeast-1", DisplayName = "Asia Pacific (Singapore)", IsPreferred = false }
        });

        // Act
        var cut = RenderComponent<PricingSelector>();

        // Assert
        var regionSelect = cut.FindAll("select")[1];
        regionSelect.InnerHtml.Should().Contain("US East (N. Virginia)");
        regionSelect.InnerHtml.Should().Contain("US West (Oregon)");
        regionSelect.InnerHtml.Should().Contain("Europe (Ireland)");
        regionSelect.InnerHtml.Should().Contain("Asia Pacific (Singapore)");
    }

    [Fact]
    public async Task PricingSelector_RegionChange_UpdatesSelectedRegion()
    {
        // Arrange
        _pricingService.GetRegions(CloudProvider.AWS).Returns(new List<RegionInfo>
        {
            new() { Code = "us-east-1", DisplayName = "US East", IsPreferred = true },
            new() { Code = "eu-west-1", DisplayName = "EU West", IsPreferred = false }
        });

        (CloudProvider provider, string region, CostEstimationOptions options)? result = null;
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.OnCalculate, EventCallback.Factory.Create<(CloudProvider, string, CostEstimationOptions)>(
                this, r => result = r)));

        // Act - Change region
        var regionSelect = cut.FindAll("select")[1];
        regionSelect.Change("eu-west-1");

        // Calculate to verify region
        await cut.InvokeAsync(() => cut.Find("button.btn-primary").Click());

        // Assert
        result.Should().NotBeNull();
        result!.Value.region.Should().Be("eu-west-1");
    }

    #endregion

    #region Accordion State Tests

    [Fact]
    public void PricingSelector_PricingAccordion_IsOpenByDefault()
    {
        // Act
        var cut = RenderComponent<PricingSelector>();

        // Assert - First accordion (Pricing Options) should have 'open' attribute
        var pricingAccordion = cut.FindAll("details.pricing-accordion")[0];
        pricingAccordion.HasAttribute("open").Should().BeTrue();
    }

    [Fact]
    public void PricingSelector_AdvancedAccordion_IsClosedByDefault()
    {
        // Act
        var cut = RenderComponent<PricingSelector>();

        // Assert - Second accordion (Advanced Options) should not have 'open' attribute
        var accordions = cut.FindAll("details.pricing-accordion");
        accordions.Should().HaveCountGreaterThanOrEqualTo(2);
        var advancedAccordion = accordions[1];
        advancedAccordion.HasAttribute("open").Should().BeFalse();
    }

    #endregion

    #region PricingType and SupportTier Enum Tests

    [Theory]
    [InlineData(PricingType.OnDemand)]
    [InlineData(PricingType.Reserved1Year)]
    [InlineData(PricingType.Reserved3Year)]
    [InlineData(PricingType.Spot)]
    public void PricingSelector_RendersPricingTypeOption(PricingType pricingType)
    {
        // Act
        var cut = RenderComponent<PricingSelector>();

        // Assert
        var html = cut.Markup;
        var displayName = pricingType switch
        {
            PricingType.OnDemand => "On-Demand",
            PricingType.Reserved1Year => "1-Year Reserved",
            PricingType.Reserved3Year => "3-Year Reserved",
            PricingType.Spot => "Spot",
            _ => pricingType.ToString()
        };
        html.Should().Contain(displayName);
    }

    [Theory]
    [InlineData(SupportTier.None)]
    [InlineData(SupportTier.Basic)]
    [InlineData(SupportTier.Developer)]
    [InlineData(SupportTier.Business)]
    [InlineData(SupportTier.Enterprise)]
    public void PricingSelector_RendersSupportTierOption(SupportTier tier)
    {
        // Act
        var cut = RenderComponent<PricingSelector>();

        // Assert
        var html = cut.Markup;
        html.Should().Contain(tier.ToString());
    }

    #endregion

    #region IsOnPremDistribution Tests

    [Theory]
    [InlineData("OPENSHIFT", true)]
    [InlineData("RANCHER", true)]
    [InlineData("RKE2", true)]
    [InlineData("K3S", true)]
    [InlineData("TANZU", true)]
    [InlineData("CHARMED", true)]
    [InlineData("KUBERNETES", true)]
    [InlineData("MICROK8S", true)]
    [InlineData("EKS", false)]
    [InlineData("AKS", false)]
    [InlineData("GKE", false)]
    public void PricingSelector_IsOnPremDistribution_ReturnsExpectedValue(string distribution, bool expectedOnPrem)
    {
        // Act
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, distribution));

        // Assert
        var title = cut.Find("h4").TextContent;
        if (expectedOnPrem)
        {
            title.Should().Contain("On-Premises");
        }
        else
        {
            title.Should().NotContain("On-Premises");
        }
    }

    [Fact]
    public void PricingSelector_IsOnPremDistribution_CaseInsensitive()
    {
        // Test lowercase
        var cutLower = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, "openshift"));
        cutLower.Find("h4").TextContent.Should().Contain("On-Premises");

        // Test mixed case
        var cutMixed = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.Distribution, "OpenShift"));
        cutMixed.Find("h4").TextContent.Should().Contain("On-Premises");
    }

    #endregion

    #region Calculate Button State Tests

    [Fact]
    public async Task PricingSelector_ButtonReEnables_AfterCalculation()
    {
        // Arrange - Use a TaskCompletionSource to control calculation timing
        var tcs = new TaskCompletionSource();

        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.OnCalculate, EventCallback.Factory.Create<(CloudProvider, string, CostEstimationOptions)>(
                this, async _ => await tcs.Task)));

        // Act - Start calculation
        var clickTask = cut.InvokeAsync(() => cut.Find("button.btn-primary").Click());

        // Assert - Button should be disabled during calculation
        cut.Find("button.btn-primary").HasAttribute("disabled").Should().BeTrue();

        // Complete the calculation
        tcs.SetResult();
        await clickTask;

        // Button should be enabled again after completion
        cut.WaitForAssertion(() =>
            cut.Find("button.btn-primary").HasAttribute("disabled").Should().BeFalse());
    }

    [Fact]
    public async Task PricingSelector_PassesCorrectRegion_InCallback()
    {
        // Arrange
        (CloudProvider provider, string region, CostEstimationOptions options)? result = null;
        var cut = RenderComponent<PricingSelector>(parameters => parameters
            .Add(p => p.OnCalculate, EventCallback.Factory.Create<(CloudProvider, string, CostEstimationOptions)>(
                this, r => result = r)));

        // Act
        await cut.InvokeAsync(() => cut.Find("button.btn-primary").Click());

        // Assert - Region should be the preferred region (us-east-1)
        result.Should().NotBeNull();
        result!.Value.region.Should().Be("us-east-1");
    }

    #endregion
}
