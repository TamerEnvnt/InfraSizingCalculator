using FluentAssertions;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Models.Pricing;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Models.Pricing;

/// <summary>
/// Tests for CloudAlternatives static factory and CloudAlternative model
/// </summary>
public class CloudAlternativesTests
{
    #region CloudAlternative Model Tests

    [Fact]
    public void CloudAlternative_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var alt = new CloudAlternative();

        // Assert
        alt.Name.Should().BeEmpty();
        alt.ServiceName.Should().BeEmpty();
        alt.Description.Should().BeEmpty();
        alt.IsDistributionSpecific.Should().BeFalse();
        alt.SourceDistribution.Should().BeNull();
        alt.IsRecommended.Should().BeFalse();
        alt.DocumentationUrl.Should().BeNull();
        alt.Features.Should().BeEmpty();
        alt.Considerations.Should().BeEmpty();
    }

    [Fact]
    public void CloudAlternative_CanSetAllProperties()
    {
        // Arrange & Act
        var alt = new CloudAlternative
        {
            Provider = CloudProvider.AWS,
            Name = "ROSA",
            ServiceName = "Red Hat OpenShift Service on AWS",
            Description = "Managed OpenShift",
            IsDistributionSpecific = true,
            SourceDistribution = Distribution.OpenShift,
            IsRecommended = true,
            DocumentationUrl = "https://example.com",
            Features = new List<string> { "Feature1", "Feature2" },
            Considerations = new List<string> { "Consideration1" }
        };

        // Assert
        alt.Provider.Should().Be(CloudProvider.AWS);
        alt.Name.Should().Be("ROSA");
        alt.ServiceName.Should().Be("Red Hat OpenShift Service on AWS");
        alt.Description.Should().Be("Managed OpenShift");
        alt.IsDistributionSpecific.Should().BeTrue();
        alt.SourceDistribution.Should().Be(Distribution.OpenShift);
        alt.IsRecommended.Should().BeTrue();
        alt.DocumentationUrl.Should().Be("https://example.com");
        alt.Features.Should().HaveCount(2);
        alt.Considerations.Should().HaveCount(1);
    }

    #endregion

    #region GetForDistribution Tests

    [Theory]
    [InlineData(Distribution.OpenShift)]
    [InlineData(Distribution.Rancher)]
    [InlineData(Distribution.RKE2)]
    [InlineData(Distribution.K3s)]
    [InlineData(Distribution.Tanzu)]
    [InlineData(Distribution.Charmed)]
    [InlineData(Distribution.Kubernetes)]
    [InlineData(Distribution.MicroK8s)]
    public void GetForDistribution_ReturnsNonEmptyList(Distribution distribution)
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(distribution);

        // Assert
        alternatives.Should().NotBeEmpty();
    }

    [Fact]
    public void GetForDistribution_UnknownDistribution_ReturnsGenericAlternatives()
    {
        // Arrange - Use a distribution that doesn't have specific handling
        var distribution = Distribution.CCE; // Huawei Cloud - returns generic alternatives

        // Act
        var alternatives = CloudAlternatives.GetForDistribution(distribution);

        // Assert
        alternatives.Should().NotBeEmpty();
        // Should return generic alternatives (all non-distribution-specific)
        alternatives.Should().OnlyContain(a => !a.IsDistributionSpecific || a.SourceDistribution == null);
    }

    #endregion

    #region OpenShift Alternatives Tests

    [Fact]
    public void GetForDistribution_OpenShift_ReturnsROSA()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.OpenShift);

        // Assert
        alternatives.Should().Contain(a => a.Name == "ROSA");
        var rosa = alternatives.First(a => a.Name == "ROSA");
        rosa.Provider.Should().Be(CloudProvider.AWS);
        rosa.IsDistributionSpecific.Should().BeTrue();
        rosa.SourceDistribution.Should().Be(Distribution.OpenShift);
        rosa.IsRecommended.Should().BeTrue();
    }

    [Fact]
    public void GetForDistribution_OpenShift_ReturnsARO()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.OpenShift);

        // Assert
        alternatives.Should().Contain(a => a.Name == "ARO");
        var aro = alternatives.First(a => a.Name == "ARO");
        aro.Provider.Should().Be(CloudProvider.Azure);
        aro.IsDistributionSpecific.Should().BeTrue();
        aro.IsRecommended.Should().BeTrue();
    }

    [Fact]
    public void GetForDistribution_OpenShift_ReturnsOpenShiftDedicated()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.OpenShift);

        // Assert
        alternatives.Should().Contain(a => a.Name == "OpenShift Dedicated");
        var osd = alternatives.First(a => a.Name == "OpenShift Dedicated");
        osd.Provider.Should().Be(CloudProvider.GCP);
    }

    [Fact]
    public void GetForDistribution_OpenShift_ReturnsROKS()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.OpenShift);

        // Assert
        alternatives.Should().Contain(a => a.Name == "ROKS");
        var roks = alternatives.First(a => a.Name == "ROKS");
        roks.Provider.Should().Be(CloudProvider.IBM);
    }

    [Fact]
    public void GetForDistribution_OpenShift_AllHaveFeatures()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.OpenShift);

        // Assert
        alternatives.Should().OnlyContain(a => a.Features.Count > 0);
    }

    #endregion

    #region Rancher Alternatives Tests

    [Fact]
    public void GetForDistribution_Rancher_ReturnsEKSWithRancher()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.Rancher);

        // Assert
        alternatives.Should().Contain(a => a.Name.Contains("EKS") && a.Name.Contains("Rancher"));
    }

    [Fact]
    public void GetForDistribution_Rancher_ReturnsAKSWithRancher()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.Rancher);

        // Assert
        alternatives.Should().Contain(a => a.Name.Contains("AKS") && a.Name.Contains("Rancher"));
    }

    [Fact]
    public void GetForDistribution_Rancher_ReturnsGKEWithRancher()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.Rancher);

        // Assert
        alternatives.Should().Contain(a => a.Name.Contains("GKE") && a.Name.Contains("Rancher"));
    }

    [Fact]
    public void GetForDistribution_Rancher_AllAreDistributionSpecific()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.Rancher);

        // Assert
        alternatives.Should().OnlyContain(a => a.IsDistributionSpecific);
    }

    #endregion

    #region K3s Alternatives Tests

    [Fact]
    public void GetForDistribution_K3s_IncludesDigitalOcean()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.K3s);

        // Assert
        alternatives.Should().Contain(a => a.Provider == CloudProvider.DigitalOcean);
        var doks = alternatives.First(a => a.Provider == CloudProvider.DigitalOcean);
        doks.Name.Should().Be("DOKS");
        doks.IsRecommended.Should().BeTrue();
    }

    [Fact]
    public void GetForDistribution_K3s_IncludesLinode()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.K3s);

        // Assert
        alternatives.Should().Contain(a => a.Provider == CloudProvider.Linode);
        var lke = alternatives.First(a => a.Provider == CloudProvider.Linode);
        lke.Name.Should().Be("LKE");
        lke.IsRecommended.Should().BeTrue();
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
    public void GetForDistribution_K3s_AlsoIncludesGenericOptions()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.K3s);

        // Assert - Should include EKS, AKS, GKE as well
        alternatives.Should().Contain(a => a.Name == "EKS");
        alternatives.Should().Contain(a => a.Name == "AKS");
        alternatives.Should().Contain(a => a.Name == "GKE");
    }

    #endregion

    #region Tanzu Alternatives Tests

    [Fact]
    public void GetForDistribution_Tanzu_IncludesTanzuOnAWS()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.Tanzu);

        // Assert
        alternatives.Should().Contain(a => a.Name.Contains("Tanzu") && a.Provider == CloudProvider.AWS);
    }

    [Fact]
    public void GetForDistribution_Tanzu_IncludesTanzuOnAzure()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.Tanzu);

        // Assert
        alternatives.Should().Contain(a => a.Name.Contains("Tanzu") && a.Provider == CloudProvider.Azure);
    }

    [Fact]
    public void GetForDistribution_Tanzu_IncludesTanzuOnGCP()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.Tanzu);

        // Assert
        alternatives.Should().Contain(a => a.Name.Contains("Tanzu") && a.Provider == CloudProvider.GCP);
    }

    [Fact]
    public void GetForDistribution_Tanzu_AllAreDistributionSpecific()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.Tanzu);

        // Assert
        alternatives.Should().OnlyContain(a => a.IsDistributionSpecific);
    }

    #endregion

    #region RKE2 Alternatives Tests

    [Fact]
    public void GetForDistribution_RKE2_ReturnsGenericWithConsiderations()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.RKE2);

        // Assert
        alternatives.Should().NotBeEmpty();
        // RKE2 returns generic alternatives with considerations added
        alternatives.Should().OnlyContain(a =>
            a.SourceDistribution == Distribution.RKE2 || a.SourceDistribution == null);
    }

    [Fact]
    public void GetForDistribution_RKE2_HasConsiderations()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.RKE2);

        // Assert
        alternatives.Should().Contain(a => a.Considerations.Any(c => c.Contains("RKE2")));
    }

    #endregion

    #region Charmed Alternatives Tests

    [Fact]
    public void GetForDistribution_Charmed_ReturnsGenericAlternatives()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.Charmed);

        // Assert
        alternatives.Should().NotBeEmpty();
        // Should have generic cloud options
        alternatives.Should().Contain(a => a.Name == "EKS" || a.Name == "AKS" || a.Name == "GKE");
    }

    [Fact]
    public void GetForDistribution_Charmed_SetsSourceDistribution()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.Charmed);

        // Assert
        alternatives.Should().OnlyContain(a => a.SourceDistribution == Distribution.Charmed);
    }

    #endregion

    #region MicroK8s Alternatives Tests

    [Fact]
    public void GetForDistribution_MicroK8s_IncludesLightweightOptions()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.MicroK8s);

        // Assert - Should be similar to K3s
        alternatives.Should().Contain(a => a.Provider == CloudProvider.DigitalOcean);
        alternatives.Should().Contain(a => a.Provider == CloudProvider.Linode);
    }

    [Fact]
    public void GetForDistribution_MicroK8s_SetsSourceDistribution()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.MicroK8s);

        // Assert
        alternatives.Should().OnlyContain(a => a.SourceDistribution == Distribution.MicroK8s);
    }

    #endregion

    #region Vanilla Kubernetes Tests

    [Fact]
    public void GetForDistribution_Kubernetes_ReturnsGenericAlternatives()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.Kubernetes);

        // Assert
        var generic = CloudAlternatives.GetGenericAlternatives();
        alternatives.Should().HaveCount(generic.Count);
    }

    #endregion

    #region GetGenericAlternatives Tests

    [Fact]
    public void GetGenericAlternatives_IncludesEKS()
    {
        // Act
        var alternatives = CloudAlternatives.GetGenericAlternatives();

        // Assert
        var eks = alternatives.FirstOrDefault(a => a.Name == "EKS");
        eks.Should().NotBeNull();
        eks!.Provider.Should().Be(CloudProvider.AWS);
        eks.ServiceName.Should().Contain("Elastic Kubernetes");
        eks.IsDistributionSpecific.Should().BeFalse();
        eks.DocumentationUrl.Should().NotBeNullOrEmpty();
        eks.Features.Should().NotBeEmpty();
    }

    [Fact]
    public void GetGenericAlternatives_IncludesAKS()
    {
        // Act
        var alternatives = CloudAlternatives.GetGenericAlternatives();

        // Assert
        var aks = alternatives.FirstOrDefault(a => a.Name == "AKS");
        aks.Should().NotBeNull();
        aks!.Provider.Should().Be(CloudProvider.Azure);
        aks.ServiceName.Should().Contain("Azure Kubernetes");
        aks.IsDistributionSpecific.Should().BeFalse();
        aks.DocumentationUrl.Should().NotBeNullOrEmpty();
        aks.Features.Should().NotBeEmpty();
    }

    [Fact]
    public void GetGenericAlternatives_IncludesGKE()
    {
        // Act
        var alternatives = CloudAlternatives.GetGenericAlternatives();

        // Assert
        var gke = alternatives.FirstOrDefault(a => a.Name == "GKE");
        gke.Should().NotBeNull();
        gke!.Provider.Should().Be(CloudProvider.GCP);
        gke.ServiceName.Should().Contain("Google Kubernetes");
        gke.IsDistributionSpecific.Should().BeFalse();
        gke.DocumentationUrl.Should().NotBeNullOrEmpty();
        gke.Features.Should().NotBeEmpty();
    }

    [Fact]
    public void GetGenericAlternatives_IncludesOKE()
    {
        // Act
        var alternatives = CloudAlternatives.GetGenericAlternatives();

        // Assert
        var oke = alternatives.FirstOrDefault(a => a.Name == "OKE");
        oke.Should().NotBeNull();
        oke!.Provider.Should().Be(CloudProvider.OCI);
        oke.ServiceName.Should().Contain("Oracle");
        oke.IsDistributionSpecific.Should().BeFalse();
    }

    [Fact]
    public void GetGenericAlternatives_NoneAreRecommended()
    {
        // Act
        var alternatives = CloudAlternatives.GetGenericAlternatives();

        // Assert - Generic alternatives aren't marked as recommended
        alternatives.Should().OnlyContain(a => !a.IsRecommended);
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
    public void GetGenericAlternatives_AllHaveFeatures()
    {
        // Act
        var alternatives = CloudAlternatives.GetGenericAlternatives();

        // Assert
        alternatives.Should().OnlyContain(a => a.Features.Count > 0);
    }

    [Fact]
    public void GetGenericAlternatives_AllHaveDocumentation()
    {
        // Act
        var alternatives = CloudAlternatives.GetGenericAlternatives();

        // Assert
        alternatives.Should().OnlyContain(a => !string.IsNullOrEmpty(a.DocumentationUrl));
    }

    #endregion

    #region Feature Validation Tests

    [Fact]
    public void AllAlternatives_HaveValidProvider()
    {
        // Arrange
        var distributions = new[] { Distribution.OpenShift, Distribution.Rancher, Distribution.K3s };

        foreach (var dist in distributions)
        {
            // Act
            var alternatives = CloudAlternatives.GetForDistribution(dist);

            // Assert
            alternatives.Should().OnlyContain(a => Enum.IsDefined(typeof(CloudProvider), a.Provider));
        }
    }

    [Fact]
    public void AllAlternatives_HaveNonEmptyName()
    {
        // Arrange
        var distributions = Enum.GetValues<Distribution>().Take(10);

        foreach (var dist in distributions)
        {
            // Act
            var alternatives = CloudAlternatives.GetForDistribution(dist);

            // Assert
            alternatives.Should().OnlyContain(a => !string.IsNullOrEmpty(a.Name));
        }
    }

    [Fact]
    public void AllAlternatives_HaveNonEmptyServiceName()
    {
        // Arrange
        var distributions = Enum.GetValues<Distribution>().Take(10);

        foreach (var dist in distributions)
        {
            // Act
            var alternatives = CloudAlternatives.GetForDistribution(dist);

            // Assert
            alternatives.Should().OnlyContain(a => !string.IsNullOrEmpty(a.ServiceName));
        }
    }

    #endregion
}
