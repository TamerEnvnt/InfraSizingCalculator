using FluentAssertions;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Models.Pricing;
using InfraSizingCalculator.Models.Pricing.Distributions;
using Xunit;

namespace InfraSizingCalculator.UnitTests;

/// <summary>
/// Comprehensive tests for all Kubernetes distribution licensing implementations.
/// Tests verify each distribution returns valid licensing data following the IDistributionLicensing interface.
/// </summary>
public class DistributionLicensingTests
{
    #region OpenShift Licensing Tests

    [Fact]
    public void OpenShiftLicensing_Distribution_ReturnsOpenShift()
    {
        var licensing = new OpenShiftLicensing();
        licensing.Distribution.Should().Be(Distribution.OpenShift);
    }

    [Fact]
    public void OpenShiftLicensing_RequiresLicense_ReturnsTrue()
    {
        var licensing = new OpenShiftLicensing();
        licensing.RequiresLicense.Should().BeTrue();
    }

    [Fact]
    public void OpenShiftLicensing_Vendor_IsRedHat()
    {
        var licensing = new OpenShiftLicensing();
        licensing.Vendor.Should().Contain("Red Hat");
    }

    [Fact]
    public void OpenShiftLicensing_DisplayName_IsNotEmpty()
    {
        var licensing = new OpenShiftLicensing();
        licensing.DisplayName.Should().NotBeNullOrEmpty();
        licensing.DisplayName.Should().Contain("OpenShift");
    }

    [Fact]
    public void OpenShiftLicensing_LicenseCostPerNodeYear_IsPositive()
    {
        var licensing = new OpenShiftLicensing();
        licensing.GetLicenseCostPerNodeYear().Should().BeGreaterThan(0);
    }

    [Fact]
    public void OpenShiftLicensing_HasSupportTiers()
    {
        var licensing = new OpenShiftLicensing();
        var tiers = licensing.GetSupportTiers();

        tiers.Should().NotBeEmpty();
        tiers.Should().Contain(t => t.Tier == SupportTier.Standard);
        tiers.Should().Contain(t => t.Tier == SupportTier.Premium);
    }

    [Fact]
    public void OpenShiftLicensing_CalculateLicensingCost_ReturnsValidCost()
    {
        var licensing = new OpenShiftLicensing();
        var input = new LicensingInput
        {
            NodeCount = 10,
            TotalCores = 40,
            SupportTier = SupportTier.Standard
        };

        var cost = licensing.CalculateLicensingCost(input);

        cost.Should().NotBeNull();
        cost.BaseLicensePerYear.Should().BeGreaterThan(0);
        cost.PerNodePerYear.Should().BeGreaterThan(0);
    }

    [Fact]
    public void OpenShiftLicensing_ROSA_IsManaged()
    {
        var licensing = new OpenShiftLicensing(isManagedVariant: true, cloudProvider: CloudProvider.ROSA);

        licensing.Distribution.Should().Be(Distribution.OpenShift);
        licensing.DisplayName.Should().Contain("ROSA");
    }

    [Fact]
    public void OpenShiftLicensing_ARO_IsManaged()
    {
        var licensing = new OpenShiftLicensing(isManagedVariant: true, cloudProvider: CloudProvider.ARO);

        licensing.DisplayName.Should().Contain("ARO");
    }

    #endregion

    #region Tanzu Licensing Tests

    [Fact]
    public void TanzuLicensing_Distribution_ReturnsTanzu()
    {
        var licensing = new TanzuLicensing();
        licensing.Distribution.Should().Be(Distribution.Tanzu);
    }

    [Fact]
    public void TanzuLicensing_RequiresLicense_ReturnsTrue()
    {
        var licensing = new TanzuLicensing();
        licensing.RequiresLicense.Should().BeTrue();
    }

    [Fact]
    public void TanzuLicensing_Vendor_IsVMware()
    {
        var licensing = new TanzuLicensing();
        licensing.Vendor.Should().Contain("VMware");
    }

    [Fact]
    public void TanzuLicensing_UsesPerCoreLicensing()
    {
        var licensing = new TanzuLicensing();
        licensing.GetLicenseCostPerCoreYear().Should().BeGreaterThan(0);
    }

    [Fact]
    public void TanzuLicensing_HasMinimumCoreRequirement()
    {
        var licensing = new TanzuLicensing();
        var input = new LicensingInput
        {
            NodeCount = 2,
            TotalCores = 4,  // Below minimum
            SupportTier = SupportTier.Standard
        };

        var cost = licensing.CalculateLicensingCost(input);

        // Should apply minimum core requirement
        cost.BaseLicensePerYear.Should().BeGreaterThan(0);
    }

    #endregion

    #region Rancher Licensing Tests

    [Fact]
    public void RancherLicensing_Distribution_ReturnsRancher()
    {
        var licensing = new RancherLicensing();
        licensing.Distribution.Should().Be(Distribution.Rancher);
    }

    [Fact]
    public void RancherLicensing_Community_DoesNotRequireLicense()
    {
        var licensing = new RancherLicensing(RancherEdition.Community);
        licensing.RequiresLicense.Should().BeFalse();
    }

    [Fact]
    public void RancherLicensing_Prime_RequiresLicense()
    {
        var licensing = new RancherLicensing(RancherEdition.Prime);
        licensing.RequiresLicense.Should().BeTrue();
    }

    [Fact]
    public void RancherLicensing_Vendor_IsSuse()
    {
        var licensing = new RancherLicensing();
        licensing.Vendor.Should().Contain("SUSE");
    }

    [Fact]
    public void RancherLicensing_Community_FreeLicensing()
    {
        var licensing = new RancherLicensing(RancherEdition.Community);
        licensing.GetLicenseCostPerNodeYear().Should().Be(0);
    }

    [Fact]
    public void RancherLicensing_Prime_HasLicenseCost()
    {
        var licensing = new RancherLicensing(RancherEdition.Prime);
        licensing.GetLicenseCostPerNodeYear().Should().BeGreaterThan(0);
    }

    #endregion

    #region RKE2 Licensing Tests

    [Fact]
    public void Rke2Licensing_Distribution_ReturnsRKE2()
    {
        var licensing = new Rke2Licensing();
        licensing.Distribution.Should().Be(Distribution.RKE2);
    }

    [Fact]
    public void Rke2Licensing_Community_DoesNotRequireLicense()
    {
        var licensing = new Rke2Licensing(Rke2Edition.Community);
        licensing.RequiresLicense.Should().BeFalse();
    }

    [Fact]
    public void Rke2Licensing_Government_HasHigherCost()
    {
        var community = new Rke2Licensing(Rke2Edition.Community);
        var prime = new Rke2Licensing(Rke2Edition.Prime);
        var government = new Rke2Licensing(Rke2Edition.Government);

        community.GetLicenseCostPerNodeYear().Should().Be(0);
        prime.GetLicenseCostPerNodeYear().Should().BeGreaterThan(0);
        government.GetLicenseCostPerNodeYear().Should().BeGreaterThan(prime.GetLicenseCostPerNodeYear());
    }

    [Fact]
    public void Rke2Licensing_Vendor_IsSuse()
    {
        var licensing = new Rke2Licensing();
        licensing.Vendor.Should().Contain("SUSE");
    }

    #endregion

    #region K3s Licensing Tests

    [Fact]
    public void K3sLicensing_Distribution_ReturnsK3s()
    {
        var licensing = new K3sLicensing();
        licensing.Distribution.Should().Be(Distribution.K3s);
    }

    [Fact]
    public void K3sLicensing_DoesNotRequireLicense()
    {
        var licensing = new K3sLicensing();
        licensing.RequiresLicense.Should().BeFalse();
    }

    [Fact]
    public void K3sLicensing_FreeLicensing()
    {
        var licensing = new K3sLicensing();
        licensing.GetLicenseCostPerNodeYear().Should().Be(0);
    }

    [Fact]
    public void K3sLicensing_Vendor_IsSuse()
    {
        var licensing = new K3sLicensing();
        licensing.Vendor.Should().Contain("SUSE");
    }

    #endregion

    #region MicroK8s Licensing Tests

    [Fact]
    public void MicroK8sLicensing_Distribution_ReturnsMicroK8s()
    {
        var licensing = new MicroK8sLicensing();
        licensing.Distribution.Should().Be(Distribution.MicroK8s);
    }

    [Fact]
    public void MicroK8sLicensing_DoesNotRequireLicense()
    {
        var licensing = new MicroK8sLicensing();
        licensing.RequiresLicense.Should().BeFalse();
    }

    [Fact]
    public void MicroK8sLicensing_FreeLicensing()
    {
        var licensing = new MicroK8sLicensing();
        licensing.GetLicenseCostPerNodeYear().Should().Be(0);
    }

    [Fact]
    public void MicroK8sLicensing_Vendor_IsCanonical()
    {
        var licensing = new MicroK8sLicensing();
        licensing.Vendor.Should().Contain("Canonical");
    }

    #endregion

    #region Charmed Kubernetes Licensing Tests

    [Fact]
    public void CharmedLicensing_Distribution_ReturnsCharmed()
    {
        var licensing = new CharmedLicensing();
        licensing.Distribution.Should().Be(Distribution.Charmed);
    }

    [Fact]
    public void CharmedLicensing_Vendor_IsCanonical()
    {
        var licensing = new CharmedLicensing();
        licensing.Vendor.Should().Contain("Canonical");
    }

    [Fact]
    public void CharmedLicensing_HasSupportTiers()
    {
        var licensing = new CharmedLicensing();
        var tiers = licensing.GetSupportTiers();

        tiers.Should().NotBeEmpty();
    }

    #endregion

    #region Vanilla Kubernetes Licensing Tests

    [Fact]
    public void VanillaK8sLicensing_Distribution_ReturnsKubernetes()
    {
        var licensing = new VanillaK8sLicensing();
        licensing.Distribution.Should().Be(Distribution.Kubernetes);
    }

    [Fact]
    public void VanillaK8sLicensing_DoesNotRequireLicense()
    {
        var licensing = new VanillaK8sLicensing();
        licensing.RequiresLicense.Should().BeFalse();
    }

    [Fact]
    public void VanillaK8sLicensing_FreeLicensing()
    {
        var licensing = new VanillaK8sLicensing();
        licensing.GetLicenseCostPerNodeYear().Should().Be(0);
    }

    [Fact]
    public void VanillaK8sLicensing_Vendor_IsCNCF()
    {
        var licensing = new VanillaK8sLicensing();
        licensing.Vendor.Should().Contain("CNCF");
    }

    [Fact]
    public void VanillaK8sLicensing_HasCommunitySupportTier()
    {
        var licensing = new VanillaK8sLicensing();
        var tiers = licensing.GetSupportTiers();

        tiers.Should().Contain(t => t.Tier == SupportTier.Community);
    }

    #endregion

    #region Managed K8s Licensing Tests

    [Theory]
    [InlineData(Distribution.EKS, CloudProvider.AWS, "Amazon")]
    [InlineData(Distribution.AKS, CloudProvider.Azure, "Microsoft")]
    [InlineData(Distribution.GKE, CloudProvider.GCP, "Google")]
    [InlineData(Distribution.OKE, CloudProvider.OCI, "Oracle")]
    [InlineData(Distribution.DOKS, CloudProvider.DigitalOcean, "DigitalOcean")]
    public void ManagedK8sLicensing_HasCorrectVendor(Distribution distribution, CloudProvider provider, string expectedVendor)
    {
        var licensing = new ManagedK8sLicensing(distribution, provider);

        licensing.Vendor.Should().Contain(expectedVendor);
    }

    [Theory]
    [InlineData(Distribution.EKS, CloudProvider.AWS)]
    [InlineData(Distribution.AKS, CloudProvider.Azure)]
    [InlineData(Distribution.GKE, CloudProvider.GCP)]
    public void ManagedK8sLicensing_DoesNotRequireLicense(Distribution distribution, CloudProvider provider)
    {
        var licensing = new ManagedK8sLicensing(distribution, provider);

        licensing.RequiresLicense.Should().BeFalse();
        licensing.GetLicenseCostPerNodeYear().Should().Be(0);
    }

    [Fact]
    public void ManagedK8sLicensing_EKS_HasControlPlaneCost()
    {
        var licensing = new ManagedK8sLicensing(Distribution.EKS, CloudProvider.AWS);
        var fixedCost = licensing.GetClusterFixedCostPerYear();

        fixedCost.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ManagedK8sLicensing_AKS_HasFreeControlPlane()
    {
        var licensing = new ManagedK8sLicensing(Distribution.AKS, CloudProvider.Azure);
        var fixedCost = licensing.GetClusterFixedCostPerYear();

        // AKS basic tier is free
        fixedCost.Should().Be(0);
    }

    [Fact]
    public void ManagedK8sLicensing_DOKS_HasFreeControlPlane()
    {
        var licensing = new ManagedK8sLicensing(Distribution.DOKS, CloudProvider.DigitalOcean);
        var fixedCost = licensing.GetClusterFixedCostPerYear();

        fixedCost.Should().Be(0);
    }

    [Theory]
    [InlineData(Distribution.EKS, CloudProvider.AWS)]
    [InlineData(Distribution.AKS, CloudProvider.Azure)]
    [InlineData(Distribution.GKE, CloudProvider.GCP)]
    [InlineData(Distribution.OKE, CloudProvider.OCI)]
    [InlineData(Distribution.DOKS, CloudProvider.DigitalOcean)]
    [InlineData(Distribution.LKE, CloudProvider.Linode)]
    [InlineData(Distribution.VKE, CloudProvider.Vultr)]
    public void ManagedK8sLicensing_CalculateLicensingCost_ReturnsValidCost(Distribution distribution, CloudProvider provider)
    {
        var licensing = new ManagedK8sLicensing(distribution, provider);
        var input = new LicensingInput
        {
            NodeCount = 5,
            TotalCores = 20,
            SupportTier = SupportTier.Standard
        };

        var cost = licensing.CalculateLicensingCost(input);

        cost.Should().NotBeNull();
        cost.BaseLicensePerYear.Should().Be(0); // Managed K8s has no separate license
        cost.LicensingModel.Should().Contain("Managed K8s");
    }

    #endregion

    #region Licensing Cost Calculation Tests

    [Fact]
    public void LicensingCost_CalculatesPerNodeCorrectly()
    {
        var licensing = new OpenShiftLicensing();
        var input10Nodes = new LicensingInput { NodeCount = 10, TotalCores = 40, SupportTier = SupportTier.Standard };
        var input20Nodes = new LicensingInput { NodeCount = 20, TotalCores = 80, SupportTier = SupportTier.Standard };

        var cost10 = licensing.CalculateLicensingCost(input10Nodes);
        var cost20 = licensing.CalculateLicensingCost(input20Nodes);

        // Total should roughly double with double nodes (some variance for volume discounts)
        cost20.BaseLicensePerYear.Should().BeGreaterThan(cost10.BaseLicensePerYear);
    }

    [Fact]
    public void LicensingCost_PremiumSupportCostsMore()
    {
        var licensing = new OpenShiftLicensing();
        var standardInput = new LicensingInput { NodeCount = 10, TotalCores = 40, SupportTier = SupportTier.Standard };
        var premiumInput = new LicensingInput { NodeCount = 10, TotalCores = 40, SupportTier = SupportTier.Premium };

        var standardCost = licensing.CalculateLicensingCost(standardInput);
        var premiumCost = licensing.CalculateLicensingCost(premiumInput);

        premiumCost.SupportCostPerYear.Should().BeGreaterThanOrEqualTo(standardCost.SupportCostPerYear);
    }

    #endregion

    #region Support Tier Tests

    [Fact]
    public void SupportTierInfo_HasRequiredProperties()
    {
        var licensing = new OpenShiftLicensing();
        var tiers = licensing.GetSupportTiers();

        foreach (var tier in tiers)
        {
            tier.Name.Should().NotBeNullOrEmpty();
            tier.Hours.Should().NotBeNullOrEmpty();
            tier.CostMultiplier.Should().BeGreaterThanOrEqualTo(0);
        }
    }

    [Fact]
    public void EnterpriseSupportTier_IncludesTAM()
    {
        var licensing = new OpenShiftLicensing();
        var tiers = licensing.GetSupportTiers();
        var enterprise = tiers.FirstOrDefault(t => t.Tier == SupportTier.Enterprise);

        enterprise.Should().NotBeNull();
        enterprise!.IncludesTAM.Should().BeTrue();
    }

    #endregion

    #region Commercial vs Open Source Tests

    [Theory]
    [InlineData(typeof(OpenShiftLicensing), true)]
    [InlineData(typeof(TanzuLicensing), true)]
    [InlineData(typeof(VanillaK8sLicensing), false)]
    [InlineData(typeof(K3sLicensing), false)]
    [InlineData(typeof(MicroK8sLicensing), false)]
    public void Licensing_RequiresLicense_MatchesExpectation(Type licensingType, bool expectedRequiresLicense)
    {
        var licensing = (IDistributionLicensing)Activator.CreateInstance(licensingType)!;

        licensing.RequiresLicense.Should().Be(expectedRequiresLicense);
    }

    [Fact]
    public void OpenSourceDistributions_HaveZeroLicenseCost()
    {
        var openSourceDistributions = new IDistributionLicensing[]
        {
            new VanillaK8sLicensing(),
            new K3sLicensing(),
            new MicroK8sLicensing(),
            new Rke2Licensing(Rke2Edition.Community),
            new RancherLicensing(RancherEdition.Community)
        };

        foreach (var dist in openSourceDistributions)
        {
            dist.GetLicenseCostPerNodeYear().Should().Be(0,
                because: $"{dist.DisplayName} is open source and should be free");
        }
    }

    [Fact]
    public void CommercialDistributions_HavePositiveLicenseCost()
    {
        var commercialDistributions = new IDistributionLicensing[]
        {
            new OpenShiftLicensing(),
            new TanzuLicensing(),
            new RancherLicensing(RancherEdition.Prime),
            new Rke2Licensing(Rke2Edition.Prime)
        };

        foreach (var dist in commercialDistributions)
        {
            dist.GetLicenseCostPerNodeYear().Should().BeGreaterThan(0,
                because: $"{dist.DisplayName} is commercial and should have a license cost");
        }
    }

    #endregion
}
