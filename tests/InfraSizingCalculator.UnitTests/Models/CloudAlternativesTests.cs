using FluentAssertions;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Models.Pricing;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Models;

/// <summary>
/// Tests for CloudAlternative model and CloudAlternatives static factory.
/// </summary>
public class CloudAlternativesTests
{
    #region CloudAlternative Model Tests

    [Fact]
    public void CloudAlternative_DefaultProperties_HaveCorrectDefaults()
    {
        // Arrange & Act
        var alt = new CloudAlternative();

        // Assert
        alt.Name.Should().BeEmpty();
        alt.ServiceName.Should().BeEmpty();
        alt.Description.Should().BeEmpty();
        alt.IsDistributionSpecific.Should().BeFalse();
        alt.IsRecommended.Should().BeFalse();
        alt.SourceDistribution.Should().BeNull();
        alt.DocumentationUrl.Should().BeNull();
        alt.Features.Should().NotBeNull().And.BeEmpty();
        alt.Considerations.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void CloudAlternative_CanSetAllProperties()
    {
        // Arrange & Act
        var alt = new CloudAlternative
        {
            Provider = CloudProvider.AWS,
            Name = "EKS",
            ServiceName = "Amazon EKS",
            Description = "Managed K8s",
            IsDistributionSpecific = true,
            IsRecommended = true,
            SourceDistribution = Distribution.OpenShift,
            DocumentationUrl = "https://example.com",
            Features = new List<string> { "Feature 1", "Feature 2" },
            Considerations = new List<string> { "Consider 1" }
        };

        // Assert
        alt.Provider.Should().Be(CloudProvider.AWS);
        alt.Name.Should().Be("EKS");
        alt.ServiceName.Should().Be("Amazon EKS");
        alt.Description.Should().Be("Managed K8s");
        alt.IsDistributionSpecific.Should().BeTrue();
        alt.IsRecommended.Should().BeTrue();
        alt.SourceDistribution.Should().Be(Distribution.OpenShift);
        alt.DocumentationUrl.Should().Be("https://example.com");
        alt.Features.Should().HaveCount(2);
        alt.Considerations.Should().HaveCount(1);
    }

    #endregion

    #region GetGenericAlternatives Tests

    [Fact]
    public void GetGenericAlternatives_ReturnsNonEmptyList()
    {
        // Act
        var alternatives = CloudAlternatives.GetGenericAlternatives();

        // Assert
        alternatives.Should().NotBeEmpty();
    }

    [Fact]
    public void GetGenericAlternatives_ContainsAllMajorCloudProviders()
    {
        // Act
        var alternatives = CloudAlternatives.GetGenericAlternatives();

        // Assert
        alternatives.Should().Contain(a => a.Provider == CloudProvider.AWS);
        alternatives.Should().Contain(a => a.Provider == CloudProvider.Azure);
        alternatives.Should().Contain(a => a.Provider == CloudProvider.GCP);
        alternatives.Should().Contain(a => a.Provider == CloudProvider.OCI);
    }

    [Fact]
    public void GetGenericAlternatives_AllHaveNonEmptyNames()
    {
        // Act
        var alternatives = CloudAlternatives.GetGenericAlternatives();

        // Assert
        alternatives.Should().OnlyContain(a => !string.IsNullOrEmpty(a.Name));
    }

    [Fact]
    public void GetGenericAlternatives_AllHaveServiceNames()
    {
        // Act
        var alternatives = CloudAlternatives.GetGenericAlternatives();

        // Assert
        alternatives.Should().OnlyContain(a => !string.IsNullOrEmpty(a.ServiceName));
    }

    [Fact]
    public void GetGenericAlternatives_AllHaveDescriptions()
    {
        // Act
        var alternatives = CloudAlternatives.GetGenericAlternatives();

        // Assert
        alternatives.Should().OnlyContain(a => !string.IsNullOrEmpty(a.Description));
    }

    [Fact]
    public void GetGenericAlternatives_AllHaveDocumentationUrls()
    {
        // Act
        var alternatives = CloudAlternatives.GetGenericAlternatives();

        // Assert
        alternatives.Should().OnlyContain(a => !string.IsNullOrEmpty(a.DocumentationUrl));
    }

    [Fact]
    public void GetGenericAlternatives_NoneAreDistributionSpecific()
    {
        // Act
        var alternatives = CloudAlternatives.GetGenericAlternatives();

        // Assert
        alternatives.Should().OnlyContain(a => !a.IsDistributionSpecific);
    }

    [Fact]
    public void GetGenericAlternatives_AllHaveFeatures()
    {
        // Act
        var alternatives = CloudAlternatives.GetGenericAlternatives();

        // Assert
        alternatives.Should().OnlyContain(a => a.Features.Count > 0);
    }

    #endregion

    #region GetForDistribution Tests - OpenShift

    [Fact]
    public void GetForDistribution_OpenShift_ReturnsAlternatives()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.OpenShift);

        // Assert
        alternatives.Should().NotBeEmpty();
    }

    [Fact]
    public void GetForDistribution_OpenShift_IncludesROSA()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.OpenShift);

        // Assert
        alternatives.Should().Contain(a => a.Name == "ROSA");
    }

    [Fact]
    public void GetForDistribution_OpenShift_IncludesARO()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.OpenShift);

        // Assert
        alternatives.Should().Contain(a => a.Name == "ARO");
    }

    [Fact]
    public void GetForDistribution_OpenShift_IncludesROKS()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.OpenShift);

        // Assert
        alternatives.Should().Contain(a => a.Name == "ROKS");
    }

    [Fact]
    public void GetForDistribution_OpenShift_HasRecommendedOptions()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.OpenShift);

        // Assert
        alternatives.Should().Contain(a => a.IsRecommended);
    }

    [Fact]
    public void GetForDistribution_OpenShift_AllAreDistributionSpecific()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.OpenShift);

        // Assert
        alternatives.Should().OnlyContain(a => a.IsDistributionSpecific);
    }

    [Fact]
    public void GetForDistribution_OpenShift_AllHaveOpenShiftAsSource()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.OpenShift);

        // Assert
        alternatives.Should().OnlyContain(a => a.SourceDistribution == Distribution.OpenShift);
    }

    [Fact]
    public void GetForDistribution_OpenShift_IncludesConsiderations()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.OpenShift);

        // Assert
        alternatives.Should().Contain(a => a.Considerations.Count > 0);
    }

    #endregion

    #region GetForDistribution Tests - Rancher

    [Fact]
    public void GetForDistribution_Rancher_ReturnsAlternatives()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.Rancher);

        // Assert
        alternatives.Should().NotBeEmpty();
    }

    [Fact]
    public void GetForDistribution_Rancher_IncludesEKSWithRancher()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.Rancher);

        // Assert
        alternatives.Should().Contain(a => a.Name.Contains("EKS"));
    }

    [Fact]
    public void GetForDistribution_Rancher_IncludesAKSWithRancher()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.Rancher);

        // Assert
        alternatives.Should().Contain(a => a.Name.Contains("AKS"));
    }

    [Fact]
    public void GetForDistribution_Rancher_IncludesGKEWithRancher()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.Rancher);

        // Assert
        alternatives.Should().Contain(a => a.Name.Contains("GKE"));
    }

    [Fact]
    public void GetForDistribution_Rancher_AllHaveRancherAsSource()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.Rancher);

        // Assert
        alternatives.Should().OnlyContain(a => a.SourceDistribution == Distribution.Rancher);
    }

    #endregion

    #region GetForDistribution Tests - RKE2

    [Fact]
    public void GetForDistribution_RKE2_ReturnsAlternatives()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.RKE2);

        // Assert
        alternatives.Should().NotBeEmpty();
    }

    [Fact]
    public void GetForDistribution_RKE2_BasedOnGenericAlternatives()
    {
        // Act
        var rke2Alts = CloudAlternatives.GetForDistribution(Distribution.RKE2);
        var genericAlts = CloudAlternatives.GetGenericAlternatives();

        // Assert - RKE2 alternatives should have at least as many as generic
        rke2Alts.Count.Should().BeGreaterThanOrEqualTo(genericAlts.Count);
    }

    [Fact]
    public void GetForDistribution_RKE2_HasRKE2Considerations()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.RKE2);

        // Assert - RKE2 alternatives should include adaptation consideration
        alternatives.Should().Contain(a =>
            a.Considerations.Any(c => c.Contains("RKE2")));
    }

    [Fact]
    public void GetForDistribution_RKE2_AllHaveRKE2AsSource()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.RKE2);

        // Assert
        alternatives.Should().OnlyContain(a => a.SourceDistribution == Distribution.RKE2);
    }

    #endregion

    #region GetForDistribution Tests - K3s

    [Fact]
    public void GetForDistribution_K3s_ReturnsAlternatives()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.K3s);

        // Assert
        alternatives.Should().NotBeEmpty();
    }

    [Fact]
    public void GetForDistribution_K3s_IncludesDigitalOcean()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.K3s);

        // Assert
        alternatives.Should().Contain(a => a.Provider == CloudProvider.DigitalOcean);
    }

    [Fact]
    public void GetForDistribution_K3s_IncludesLinode()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.K3s);

        // Assert
        alternatives.Should().Contain(a => a.Provider == CloudProvider.Linode);
    }

    [Fact]
    public void GetForDistribution_K3s_IncludesHetzner()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.K3s);

        // Assert
        alternatives.Should().Contain(a => a.Provider == CloudProvider.Hetzner);
    }

    [Fact]
    public void GetForDistribution_K3s_AlsoIncludesGenericProviders()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.K3s);

        // Assert - K3s should also include generic alternatives
        alternatives.Should().Contain(a => a.Provider == CloudProvider.AWS);
        alternatives.Should().Contain(a => a.Provider == CloudProvider.Azure);
        alternatives.Should().Contain(a => a.Provider == CloudProvider.GCP);
    }

    [Fact]
    public void GetForDistribution_K3s_HasLightweightOptions()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.K3s);

        // Assert - K3s alternatives should include cost-effective/simple options
        alternatives.Should().Contain(a =>
            a.Features.Any(f => f.Contains("cost") || f.Contains("simple", StringComparison.OrdinalIgnoreCase)));
    }

    #endregion

    #region GetForDistribution Tests - Tanzu

    [Fact]
    public void GetForDistribution_Tanzu_ReturnsAlternatives()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.Tanzu);

        // Assert
        alternatives.Should().NotBeEmpty();
    }

    [Fact]
    public void GetForDistribution_Tanzu_IncludesTanzuOnAWS()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.Tanzu);

        // Assert
        alternatives.Should().Contain(a => a.Name.Contains("AWS"));
    }

    [Fact]
    public void GetForDistribution_Tanzu_IncludesTanzuOnAzure()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.Tanzu);

        // Assert
        alternatives.Should().Contain(a => a.Name.Contains("Azure"));
    }

    [Fact]
    public void GetForDistribution_Tanzu_IncludesTanzuOnGCP()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.Tanzu);

        // Assert
        alternatives.Should().Contain(a => a.Name.Contains("GCP"));
    }

    [Fact]
    public void GetForDistribution_Tanzu_AllHaveVMwareIntegration()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.Tanzu);

        // Assert
        alternatives.Should().OnlyContain(a =>
            a.Features.Any(f => f.Contains("VMware", StringComparison.OrdinalIgnoreCase) ||
                               f.Contains("vSphere", StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public void GetForDistribution_Tanzu_AllHaveTanzuAsSource()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.Tanzu);

        // Assert
        alternatives.Should().OnlyContain(a => a.SourceDistribution == Distribution.Tanzu);
    }

    #endregion

    #region GetForDistribution Tests - Charmed

    [Fact]
    public void GetForDistribution_Charmed_ReturnsAlternatives()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.Charmed);

        // Assert
        alternatives.Should().NotBeEmpty();
    }

    [Fact]
    public void GetForDistribution_Charmed_AllHaveCharmedAsSource()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.Charmed);

        // Assert
        alternatives.Should().OnlyContain(a => a.SourceDistribution == Distribution.Charmed);
    }

    [Fact]
    public void GetForDistribution_Charmed_IncludesMajorCloudProviders()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.Charmed);

        // Assert - Should include major cloud options
        alternatives.Should().Contain(a => a.Provider == CloudProvider.AWS);
        alternatives.Should().Contain(a => a.Provider == CloudProvider.Azure);
        alternatives.Should().Contain(a => a.Provider == CloudProvider.GCP);
    }

    #endregion

    #region GetForDistribution Tests - Vanilla Kubernetes

    [Fact]
    public void GetForDistribution_Kubernetes_ReturnsAlternatives()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.Kubernetes);

        // Assert
        alternatives.Should().NotBeEmpty();
    }

    [Fact]
    public void GetForDistribution_Kubernetes_ReturnsGenericAlternatives()
    {
        // Act
        var k8sAlts = CloudAlternatives.GetForDistribution(Distribution.Kubernetes);
        var genericAlts = CloudAlternatives.GetGenericAlternatives();

        // Assert
        k8sAlts.Should().HaveCount(genericAlts.Count);
    }

    #endregion

    #region GetForDistribution Tests - MicroK8s

    [Fact]
    public void GetForDistribution_MicroK8s_ReturnsAlternatives()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.MicroK8s);

        // Assert
        alternatives.Should().NotBeEmpty();
    }

    [Fact]
    public void GetForDistribution_MicroK8s_SimilarToK3s()
    {
        // Act
        var microK8sAlts = CloudAlternatives.GetForDistribution(Distribution.MicroK8s);
        var k3sAlts = CloudAlternatives.GetForDistribution(Distribution.K3s);

        // Assert - MicroK8s and K3s should have same alternatives count
        microK8sAlts.Count.Should().Be(k3sAlts.Count);
    }

    [Fact]
    public void GetForDistribution_MicroK8s_AllHaveMicroK8sAsSource()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.MicroK8s);

        // Assert
        alternatives.Should().OnlyContain(a => a.SourceDistribution == Distribution.MicroK8s);
    }

    [Fact]
    public void GetForDistribution_MicroK8s_IncludesLightweightProviders()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.MicroK8s);

        // Assert
        alternatives.Should().Contain(a => a.Provider == CloudProvider.DigitalOcean);
        alternatives.Should().Contain(a => a.Provider == CloudProvider.Linode);
    }

    #endregion

    #region GetForDistribution Tests - Unknown Distribution

    [Fact]
    public void GetForDistribution_UnknownDistribution_ReturnsGenericAlternatives()
    {
        // Act - Use a distribution that doesn't have specific alternatives
        var alternatives = CloudAlternatives.GetForDistribution((Distribution)999);

        // Assert
        alternatives.Should().NotBeEmpty();
        alternatives.Should().HaveCount(CloudAlternatives.GetGenericAlternatives().Count);
    }

    #endregion

    #region All Distributions Coverage Tests

    [Theory]
    [InlineData(Distribution.OpenShift)]
    [InlineData(Distribution.Rancher)]
    [InlineData(Distribution.RKE2)]
    [InlineData(Distribution.K3s)]
    [InlineData(Distribution.Tanzu)]
    [InlineData(Distribution.Charmed)]
    [InlineData(Distribution.Kubernetes)]
    [InlineData(Distribution.MicroK8s)]
    public void GetForDistribution_AllDistributions_ReturnNonEmpty(Distribution distribution)
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(distribution);

        // Assert
        alternatives.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData(Distribution.OpenShift)]
    [InlineData(Distribution.Rancher)]
    [InlineData(Distribution.RKE2)]
    [InlineData(Distribution.K3s)]
    [InlineData(Distribution.Tanzu)]
    [InlineData(Distribution.Charmed)]
    [InlineData(Distribution.Kubernetes)]
    [InlineData(Distribution.MicroK8s)]
    public void GetForDistribution_AllDistributions_HaveValidProvider(Distribution distribution)
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(distribution);

        // Assert
        alternatives.Should().OnlyContain(a => Enum.IsDefined(typeof(CloudProvider), a.Provider));
    }

    [Theory]
    [InlineData(Distribution.OpenShift)]
    [InlineData(Distribution.Rancher)]
    [InlineData(Distribution.RKE2)]
    [InlineData(Distribution.K3s)]
    [InlineData(Distribution.Tanzu)]
    [InlineData(Distribution.Charmed)]
    [InlineData(Distribution.Kubernetes)]
    [InlineData(Distribution.MicroK8s)]
    public void GetForDistribution_AllDistributions_HaveNonEmptyNames(Distribution distribution)
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(distribution);

        // Assert
        alternatives.Should().OnlyContain(a => !string.IsNullOrEmpty(a.Name));
    }

    [Theory]
    [InlineData(Distribution.OpenShift)]
    [InlineData(Distribution.Rancher)]
    [InlineData(Distribution.RKE2)]
    [InlineData(Distribution.K3s)]
    [InlineData(Distribution.Tanzu)]
    [InlineData(Distribution.Charmed)]
    [InlineData(Distribution.Kubernetes)]
    [InlineData(Distribution.MicroK8s)]
    public void GetForDistribution_AllDistributions_HaveNonEmptyServiceNames(Distribution distribution)
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(distribution);

        // Assert
        alternatives.Should().OnlyContain(a => !string.IsNullOrEmpty(a.ServiceName));
    }

    [Theory]
    [InlineData(Distribution.OpenShift)]
    [InlineData(Distribution.Rancher)]
    [InlineData(Distribution.RKE2)]
    [InlineData(Distribution.K3s)]
    [InlineData(Distribution.Tanzu)]
    [InlineData(Distribution.Charmed)]
    [InlineData(Distribution.Kubernetes)]
    [InlineData(Distribution.MicroK8s)]
    public void GetForDistribution_AllDistributions_HaveNonEmptyDescriptions(Distribution distribution)
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(distribution);

        // Assert
        alternatives.Should().OnlyContain(a => !string.IsNullOrEmpty(a.Description));
    }

    #endregion
}
