using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.Pricing;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Models.Pricing;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components.Pricing;

/// <summary>
/// Tests for CloudAlternativesPanel component - displays cloud alternative options for on-prem distributions
/// </summary>
public class CloudAlternativesPanelTests : TestContext
{
    #region Rendering Tests

    [Fact]
    public void CloudAlternativesPanel_RendersMainContainer()
    {
        // Arrange & Act
        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Distribution, Distribution.OpenShift)
            .Add(p => p.Alternatives, CreateSampleAlternatives()));

        // Assert
        cut.Find(".cloud-alternatives-panel").Should().NotBeNull();
    }

    [Fact]
    public void CloudAlternativesPanel_RendersIntroText()
    {
        // Arrange & Act
        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Distribution, Distribution.OpenShift)
            .Add(p => p.Alternatives, CreateSampleAlternatives()));

        // Assert
        cut.Find(".panel-intro").Should().NotBeNull();
        cut.Find(".intro-text").TextContent.Should().Contain("Compare");
    }

    [Fact]
    public void CloudAlternativesPanel_ShowsDistributionNameInIntro()
    {
        // Arrange & Act
        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Distribution, Distribution.OpenShift)
            .Add(p => p.Alternatives, CreateSampleAlternatives()));

        // Assert
        cut.Find(".intro-text").TextContent.Should().Contain("OpenShift");
    }

    #endregion

    #region Distribution-Specific Alternatives Tests

    [Fact]
    public void CloudAlternativesPanel_HasDistributionSpecific_ShowsPrimarySection()
    {
        // Arrange
        var alternatives = new List<CloudAlternative>
        {
            new()
            {
                Provider = CloudProvider.AWS,
                Name = "ROSA",
                ServiceName = "Red Hat OpenShift Service on AWS",
                Description = "Managed OpenShift on AWS",
                IsDistributionSpecific = true,
                IsRecommended = true,
                Features = new List<string> { "Feature 1", "Feature 2" }
            }
        };

        // Act
        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Distribution, Distribution.OpenShift)
            .Add(p => p.Alternatives, alternatives));

        // Assert
        cut.FindAll(".alternatives-section.primary").Should().NotBeEmpty();
    }

    [Fact]
    public void CloudAlternativesPanel_NoDistributionSpecific_HidesPrimarySection()
    {
        // Arrange - Only generic alternatives
        var alternatives = new List<CloudAlternative>
        {
            new()
            {
                Provider = CloudProvider.AWS,
                Name = "EKS",
                ServiceName = "Amazon EKS",
                Description = "Managed K8s",
                IsDistributionSpecific = false,
                Features = new List<string>()
            }
        };

        // Act
        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Distribution, Distribution.Kubernetes)
            .Add(p => p.Alternatives, alternatives));

        // Assert
        cut.FindAll(".alternatives-section.primary").Should().BeEmpty();
    }

    [Fact]
    public void CloudAlternativesPanel_ShowsRecommendedForDistribution()
    {
        // Arrange
        var alternatives = new List<CloudAlternative>
        {
            new()
            {
                Provider = CloudProvider.AWS,
                Name = "ROSA",
                ServiceName = "Managed OpenShift",
                Description = "Test",
                IsDistributionSpecific = true,
                IsRecommended = true,
                Features = new List<string>()
            }
        };

        // Act
        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Distribution, Distribution.OpenShift)
            .Add(p => p.Alternatives, alternatives));

        // Assert
        cut.Find(".section-header").TextContent.Should().Contain("Recommended");
    }

    #endregion

    #region Generic Alternatives Tests

    [Fact]
    public void CloudAlternativesPanel_ShowsSecondarySection()
    {
        // Arrange
        var alternatives = new List<CloudAlternative>
        {
            new()
            {
                Provider = CloudProvider.AWS,
                Name = "EKS",
                ServiceName = "Amazon EKS",
                Description = "Managed K8s",
                IsDistributionSpecific = false,
                Features = new List<string>()
            }
        };

        // Act
        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Distribution, Distribution.Kubernetes)
            .Add(p => p.Alternatives, alternatives));

        // Assert
        cut.FindAll(".alternatives-section.secondary").Should().NotBeEmpty();
    }

    [Fact]
    public void CloudAlternativesPanel_SecondarySection_HasCorrectHeader()
    {
        // Arrange
        var alternatives = CreateSampleAlternatives();

        // Act
        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Distribution, Distribution.OpenShift)
            .Add(p => p.Alternatives, alternatives));

        // Assert
        cut.Find(".alternatives-section.secondary .section-header").TextContent.Should().Contain("Other Cloud Options");
    }

    #endregion

    #region Alternative Card Tests

    [Fact]
    public void CloudAlternativesPanel_RendersAlternativeCards()
    {
        // Arrange
        var alternatives = CreateSampleAlternatives();

        // Act
        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Distribution, Distribution.OpenShift)
            .Add(p => p.Alternatives, alternatives));

        // Assert
        var cards = cut.FindAll(".alternative-card");
        cards.Should().HaveCount(alternatives.Count);
    }

    [Fact]
    public void CloudAlternativesPanel_CardShowsName()
    {
        // Arrange
        var alternatives = new List<CloudAlternative>
        {
            new()
            {
                Provider = CloudProvider.AWS,
                Name = "ROSA",
                ServiceName = "Red Hat OpenShift Service on AWS",
                Description = "Test",
                IsDistributionSpecific = true,
                Features = new List<string>()
            }
        };

        // Act
        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Distribution, Distribution.OpenShift)
            .Add(p => p.Alternatives, alternatives));

        // Assert
        cut.Find(".card-name").TextContent.Should().Be("ROSA");
    }

    [Fact]
    public void CloudAlternativesPanel_CardShowsServiceName()
    {
        // Arrange
        var alternatives = new List<CloudAlternative>
        {
            new()
            {
                Provider = CloudProvider.AWS,
                Name = "ROSA",
                ServiceName = "Red Hat OpenShift Service on AWS",
                Description = "Test",
                IsDistributionSpecific = true,
                Features = new List<string>()
            }
        };

        // Act
        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Distribution, Distribution.OpenShift)
            .Add(p => p.Alternatives, alternatives));

        // Assert
        cut.Find(".card-service").TextContent.Should().Contain("Red Hat OpenShift");
    }

    [Fact]
    public void CloudAlternativesPanel_CardShowsDescription()
    {
        // Arrange
        var alternatives = new List<CloudAlternative>
        {
            new()
            {
                Provider = CloudProvider.AWS,
                Name = "EKS",
                ServiceName = "Amazon EKS",
                Description = "Fully managed Kubernetes service",
                IsDistributionSpecific = false,
                Features = new List<string>()
            }
        };

        // Act
        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Distribution, Distribution.Kubernetes)
            .Add(p => p.Alternatives, alternatives));

        // Assert
        cut.Find(".card-description").TextContent.Should().Contain("Fully managed Kubernetes");
    }

    [Fact]
    public void CloudAlternativesPanel_CardShowsFeatures()
    {
        // Arrange
        var alternatives = new List<CloudAlternative>
        {
            new()
            {
                Provider = CloudProvider.AWS,
                Name = "EKS",
                ServiceName = "Amazon EKS",
                Description = "Test",
                IsDistributionSpecific = false,
                Features = new List<string> { "Feature A", "Feature B", "Feature C" }
            }
        };

        // Act
        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Distribution, Distribution.Kubernetes)
            .Add(p => p.Alternatives, alternatives));

        // Assert
        var features = cut.FindAll(".card-features li");
        features.Should().HaveCount(3);
        features[0].TextContent.Should().Contain("Feature A");
    }

    [Fact]
    public void CloudAlternativesPanel_CardShowsMaxThreeFeatures()
    {
        // Arrange
        var alternatives = new List<CloudAlternative>
        {
            new()
            {
                Provider = CloudProvider.AWS,
                Name = "EKS",
                ServiceName = "Amazon EKS",
                Description = "Test",
                IsDistributionSpecific = false,
                Features = new List<string> { "Feature 1", "Feature 2", "Feature 3", "Feature 4", "Feature 5" }
            }
        };

        // Act
        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Distribution, Distribution.Kubernetes)
            .Add(p => p.Alternatives, alternatives));

        // Assert - Only first 3 features shown
        var features = cut.FindAll(".card-features li");
        features.Should().HaveCount(3);
    }

    [Fact]
    public void CloudAlternativesPanel_NoFeatures_HidesFeaturesSection()
    {
        // Arrange
        var alternatives = new List<CloudAlternative>
        {
            new()
            {
                Provider = CloudProvider.AWS,
                Name = "EKS",
                ServiceName = "Amazon EKS",
                Description = "Test",
                IsDistributionSpecific = false,
                Features = new List<string>() // Empty
            }
        };

        // Act
        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Distribution, Distribution.Kubernetes)
            .Add(p => p.Alternatives, alternatives));

        // Assert
        cut.FindAll(".card-features").Should().BeEmpty();
    }

    #endregion

    #region Provider Icon Tests

    [Theory]
    [InlineData(CloudProvider.AWS, "AWS")]
    [InlineData(CloudProvider.Azure, "Azure")]
    [InlineData(CloudProvider.GCP, "GCP")]
    [InlineData(CloudProvider.OCI, "OCI")]
    [InlineData(CloudProvider.IBM, "IBM")]
    [InlineData(CloudProvider.Alibaba, "Ali")]
    [InlineData(CloudProvider.DigitalOcean, "DO")]
    [InlineData(CloudProvider.Linode, "Lin")]
    [InlineData(CloudProvider.Vultr, "Vul")]
    [InlineData(CloudProvider.Hetzner, "Htz")]
    public void CloudAlternativesPanel_ShowsCorrectProviderIcon(CloudProvider provider, string expectedIcon)
    {
        // Arrange
        var alternatives = new List<CloudAlternative>
        {
            new()
            {
                Provider = provider,
                Name = "Test",
                ServiceName = "Test Service",
                Description = "Test",
                IsDistributionSpecific = false,
                Features = new List<string>()
            }
        };

        // Act
        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Distribution, Distribution.Kubernetes)
            .Add(p => p.Alternatives, alternatives));

        // Assert
        cut.Find(".provider-icon").TextContent.Should().Be(expectedIcon);
    }

    #endregion

    #region Recommended Badge Tests

    [Fact]
    public void CloudAlternativesPanel_RecommendedCard_ShowsBadge()
    {
        // Arrange
        var alternatives = new List<CloudAlternative>
        {
            new()
            {
                Provider = CloudProvider.AWS,
                Name = "ROSA",
                ServiceName = "Test",
                Description = "Test",
                IsDistributionSpecific = true,
                IsRecommended = true,
                Features = new List<string>()
            }
        };

        // Act
        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Distribution, Distribution.OpenShift)
            .Add(p => p.Alternatives, alternatives));

        // Assert
        cut.Find(".recommended-badge").TextContent.Should().Contain("Recommended");
    }

    [Fact]
    public void CloudAlternativesPanel_RecommendedCard_HasRecommendedClass()
    {
        // Arrange
        var alternatives = new List<CloudAlternative>
        {
            new()
            {
                Provider = CloudProvider.AWS,
                Name = "ROSA",
                ServiceName = "Test",
                Description = "Test",
                IsDistributionSpecific = true,
                IsRecommended = true,
                Features = new List<string>()
            }
        };

        // Act
        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Distribution, Distribution.OpenShift)
            .Add(p => p.Alternatives, alternatives));

        // Assert
        cut.FindAll(".alternative-card.recommended").Should().NotBeEmpty();
    }

    [Fact]
    public void CloudAlternativesPanel_NotRecommended_NoBadge()
    {
        // Arrange
        var alternatives = new List<CloudAlternative>
        {
            new()
            {
                Provider = CloudProvider.AWS,
                Name = "EKS",
                ServiceName = "Test",
                Description = "Test",
                IsDistributionSpecific = false,
                IsRecommended = false,
                Features = new List<string>()
            }
        };

        // Act
        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Distribution, Distribution.Kubernetes)
            .Add(p => p.Alternatives, alternatives));

        // Assert
        cut.FindAll(".recommended-badge").Should().BeEmpty();
    }

    #endregion

    #region Card Actions Tests

    [Fact]
    public void CloudAlternativesPanel_ShowsCompareButton()
    {
        // Arrange
        var alternatives = CreateSampleAlternatives();

        // Act
        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Distribution, Distribution.OpenShift)
            .Add(p => p.Alternatives, alternatives));

        // Assert
        cut.FindAll(".btn-compare").Should().NotBeEmpty();
        cut.Find(".btn-compare").TextContent.Should().Contain("Compare");
    }

    [Fact]
    public void CloudAlternativesPanel_WithDocUrl_ShowsDocsLink()
    {
        // Arrange
        var alternatives = new List<CloudAlternative>
        {
            new()
            {
                Provider = CloudProvider.AWS,
                Name = "EKS",
                ServiceName = "Test",
                Description = "Test",
                IsDistributionSpecific = false,
                DocumentationUrl = "https://aws.amazon.com/eks/",
                Features = new List<string>()
            }
        };

        // Act
        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Distribution, Distribution.Kubernetes)
            .Add(p => p.Alternatives, alternatives));

        // Assert
        var docsLink = cut.Find(".btn-docs");
        docsLink.TextContent.Should().Contain("Docs");
        docsLink.GetAttribute("href").Should().Be("https://aws.amazon.com/eks/");
        docsLink.GetAttribute("target").Should().Be("_blank");
    }

    [Fact]
    public void CloudAlternativesPanel_NoDocUrl_HidesDocsLink()
    {
        // Arrange
        var alternatives = new List<CloudAlternative>
        {
            new()
            {
                Provider = CloudProvider.AWS,
                Name = "EKS",
                ServiceName = "Test",
                Description = "Test",
                IsDistributionSpecific = false,
                DocumentationUrl = null,
                Features = new List<string>()
            }
        };

        // Act
        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Distribution, Distribution.Kubernetes)
            .Add(p => p.Alternatives, alternatives));

        // Assert
        cut.FindAll(".btn-docs").Should().BeEmpty();
    }

    #endregion

    #region Selection Tests

    [Fact]
    public async Task CloudAlternativesPanel_ClickCard_SelectsAlternative()
    {
        // Arrange
        CloudAlternative? selectedAlt = null;
        var alternatives = CreateSampleAlternatives();

        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Distribution, Distribution.OpenShift)
            .Add(p => p.Alternatives, alternatives)
            .Add(p => p.OnAlternativeSelected,
                EventCallback.Factory.Create<CloudAlternative>(this, a => selectedAlt = a)));

        // Act
        var card = cut.Find(".alternative-card");
        await cut.InvokeAsync(() => card.Click());

        // Assert
        selectedAlt.Should().NotBeNull();
    }

    [Fact]
    public async Task CloudAlternativesPanel_SelectedCard_HasSelectedClass()
    {
        // Arrange
        var alternatives = CreateSampleAlternatives();

        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Distribution, Distribution.OpenShift)
            .Add(p => p.Alternatives, alternatives));

        // Act
        var card = cut.Find(".alternative-card");
        await cut.InvokeAsync(() => card.Click());

        // Assert
        cut.FindAll(".alternative-card.selected").Should().NotBeEmpty();
    }

    #endregion

    #region Comparison Panel Tests

    [Fact]
    public async Task CloudAlternativesPanel_ClickCompare_ShowsComparisonPanel()
    {
        // Arrange
        var alternatives = CreateSampleAlternatives();

        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Distribution, Distribution.OpenShift)
            .Add(p => p.Alternatives, alternatives));

        // Act
        var compareBtn = cut.Find(".btn-compare");
        await cut.InvokeAsync(() => compareBtn.Click());

        // Assert
        cut.FindAll(".comparison-panel").Should().NotBeEmpty();
    }

    [Fact]
    public async Task CloudAlternativesPanel_ComparisonPanel_ShowsSelectedName()
    {
        // Arrange
        var alternatives = new List<CloudAlternative>
        {
            new()
            {
                Provider = CloudProvider.AWS,
                Name = "ROSA",
                ServiceName = "Test",
                Description = "Test",
                IsDistributionSpecific = true,
                Features = new List<string>()
            }
        };

        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Distribution, Distribution.OpenShift)
            .Add(p => p.Alternatives, alternatives));

        // Act
        var compareBtn = cut.Find(".btn-compare");
        await cut.InvokeAsync(() => compareBtn.Click());

        // Assert
        cut.Find(".comparison-header h4").TextContent.Should().Contain("ROSA");
    }

    [Fact]
    public async Task CloudAlternativesPanel_CloseComparison_HidesPanel()
    {
        // Arrange
        var alternatives = CreateSampleAlternatives();

        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Distribution, Distribution.OpenShift)
            .Add(p => p.Alternatives, alternatives));

        // Open comparison panel
        var compareBtn = cut.Find(".btn-compare");
        await cut.InvokeAsync(() => compareBtn.Click());
        cut.FindAll(".comparison-panel").Should().NotBeEmpty();

        // Act - Close it
        var closeBtn = cut.Find(".close-btn");
        await cut.InvokeAsync(() => closeBtn.Click());

        // Assert
        cut.FindAll(".comparison-panel").Should().BeEmpty();
    }

    [Fact]
    public async Task CloudAlternativesPanel_ComparisonPanel_ShowsNote()
    {
        // Arrange
        var alternatives = CreateSampleAlternatives();

        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Distribution, Distribution.OpenShift)
            .Add(p => p.Alternatives, alternatives));

        // Act
        var compareBtn = cut.Find(".btn-compare");
        await cut.InvokeAsync(() => compareBtn.Click());

        // Assert
        cut.Find(".comparison-note").TextContent.Should().Contain("cost comparison");
    }

    #endregion

    #region Empty State Tests

    [Fact]
    public void CloudAlternativesPanel_EmptyAlternatives_RendersWithoutError()
    {
        // Arrange & Act
        var action = () => RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Distribution, Distribution.Kubernetes)
            .Add(p => p.Alternatives, new List<CloudAlternative>()));

        // Assert
        action.Should().NotThrow();
    }

    [Fact]
    public void CloudAlternativesPanel_NullDistribution_RendersWithoutError()
    {
        // Arrange & Act
        var action = () => RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Distribution, (Distribution?)null)
            .Add(p => p.Alternatives, CreateSampleAlternatives()));

        // Assert
        action.Should().NotThrow();
    }

    #endregion

    #region Distribution-Specific Factory Tests

    [Theory]
    [InlineData(Distribution.OpenShift)]
    [InlineData(Distribution.Rancher)]
    [InlineData(Distribution.RKE2)]
    [InlineData(Distribution.K3s)]
    [InlineData(Distribution.Tanzu)]
    [InlineData(Distribution.Charmed)]
    [InlineData(Distribution.Kubernetes)]
    [InlineData(Distribution.MicroK8s)]
    public void CloudAlternatives_GetForDistribution_ReturnsAlternatives(Distribution distribution)
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(distribution);

        // Assert
        alternatives.Should().NotBeEmpty();
    }

    [Fact]
    public void CloudAlternatives_GetGenericAlternatives_ReturnsAllMajorProviders()
    {
        // Act
        var alternatives = CloudAlternatives.GetGenericAlternatives();

        // Assert
        alternatives.Should().Contain(a => a.Provider == CloudProvider.AWS);
        alternatives.Should().Contain(a => a.Provider == CloudProvider.Azure);
        alternatives.Should().Contain(a => a.Provider == CloudProvider.GCP);
    }

    [Fact]
    public void CloudAlternatives_OpenShift_IncludesROSAAndARO()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.OpenShift);

        // Assert
        alternatives.Should().Contain(a => a.Name == "ROSA");
        alternatives.Should().Contain(a => a.Name == "ARO");
    }

    [Fact]
    public void CloudAlternatives_K3s_IncludesLightweightOptions()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.K3s);

        // Assert
        alternatives.Should().Contain(a => a.Provider == CloudProvider.DigitalOcean);
        alternatives.Should().Contain(a => a.Provider == CloudProvider.Linode);
    }

    [Fact]
    public void CloudAlternatives_Tanzu_IncludesVMwareCloudOptions()
    {
        // Act
        var alternatives = CloudAlternatives.GetForDistribution(Distribution.Tanzu);

        // Assert
        alternatives.Should().Contain(a => a.Name.Contains("Tanzu"));
    }

    #endregion

    #region Helper Methods

    private static List<CloudAlternative> CreateSampleAlternatives()
    {
        return new List<CloudAlternative>
        {
            new()
            {
                Provider = CloudProvider.AWS,
                Name = "ROSA",
                ServiceName = "Red Hat OpenShift Service on AWS",
                Description = "Managed OpenShift on AWS with joint support",
                IsDistributionSpecific = true,
                IsRecommended = true,
                DocumentationUrl = "https://www.redhat.com/openshift/aws",
                Features = new List<string> { "Native AWS integration", "Joint support", "PrivateLink" }
            },
            new()
            {
                Provider = CloudProvider.Azure,
                Name = "ARO",
                ServiceName = "Azure Red Hat OpenShift",
                Description = "Managed OpenShift on Azure",
                IsDistributionSpecific = true,
                IsRecommended = true,
                Features = new List<string> { "Azure AD integration", "Joint support" }
            },
            new()
            {
                Provider = CloudProvider.AWS,
                Name = "EKS",
                ServiceName = "Amazon Elastic Kubernetes Service",
                Description = "Fully managed Kubernetes on AWS",
                IsDistributionSpecific = false,
                IsRecommended = false,
                DocumentationUrl = "https://aws.amazon.com/eks/",
                Features = new List<string> { "Managed control plane", "Fargate support" }
            }
        };
    }

    #endregion
}
