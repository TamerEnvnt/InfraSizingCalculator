using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.Pricing;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Models.Pricing;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components.Pricing;

/// <summary>
/// Tests for CloudAlternativesPanel component - displays cloud migration alternatives
/// </summary>
public class CloudAlternativesPanelTests : TestContext
{
    #region Rendering Tests

    [Fact]
    public void CloudAlternativesPanel_RendersMainContainer()
    {
        // Act
        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Distribution, Distribution.OpenShift));

        // Assert
        cut.Find(".cloud-alternatives-panel").Should().NotBeNull();
    }

    [Fact]
    public void CloudAlternativesPanel_RendersIntroText()
    {
        // Act
        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Distribution, Distribution.OpenShift));

        // Assert
        cut.Find(".intro-text").Should().NotBeNull();
        cut.Markup.Should().Contain("Compare your on-premises");
    }

    [Fact]
    public void CloudAlternativesPanel_RendersDistributionInIntro()
    {
        // Act
        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Distribution, Distribution.RKE2));

        // Assert
        cut.Markup.Should().Contain("RKE2");
    }

    #endregion

    #region Distribution-Specific Alternatives Tests

    [Fact]
    public void CloudAlternativesPanel_WithDistributionSpecificAlternatives_RendersPrimarySection()
    {
        // Arrange
        var alternatives = new List<CloudAlternative>
        {
            CreateDistributionSpecificAlternative("ROSA", CloudProvider.AWS)
        };

        // Act
        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Distribution, Distribution.OpenShift)
            .Add(p => p.Alternatives, alternatives));

        // Assert
        cut.FindAll(".alternatives-section.primary").Should().NotBeEmpty();
    }

    [Fact]
    public void CloudAlternativesPanel_DistributionSpecific_ShowsRecommendedHeader()
    {
        // Arrange
        var alternatives = new List<CloudAlternative>
        {
            CreateDistributionSpecificAlternative("ROSA", CloudProvider.AWS)
        };

        // Act
        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Distribution, Distribution.OpenShift)
            .Add(p => p.Alternatives, alternatives));

        // Assert
        cut.Markup.Should().Contain("Recommended for OpenShift");
    }

    [Fact]
    public void CloudAlternativesPanel_NoDistributionSpecific_HidesPrimarySection()
    {
        // Arrange
        var alternatives = new List<CloudAlternative>
        {
            CreateGenericAlternative("EKS", CloudProvider.AWS)
        };

        // Act
        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Distribution, Distribution.Kubernetes)
            .Add(p => p.Alternatives, alternatives));

        // Assert
        cut.FindAll(".alternatives-section.primary").Should().BeEmpty();
    }

    [Fact]
    public void CloudAlternativesPanel_RecommendedAlternative_ShowsBadge()
    {
        // Arrange
        var alternatives = new List<CloudAlternative>
        {
            CreateDistributionSpecificAlternative("ROSA", CloudProvider.AWS, isRecommended: true)
        };

        // Act
        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Distribution, Distribution.OpenShift)
            .Add(p => p.Alternatives, alternatives));

        // Assert
        cut.FindAll(".recommended-badge").Should().NotBeEmpty();
    }

    [Fact]
    public void CloudAlternativesPanel_RecommendedAlternative_HasRecommendedClass()
    {
        // Arrange
        var alternatives = new List<CloudAlternative>
        {
            CreateDistributionSpecificAlternative("ROSA", CloudProvider.AWS, isRecommended: true)
        };

        // Act
        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Distribution, Distribution.OpenShift)
            .Add(p => p.Alternatives, alternatives));

        // Assert
        cut.FindAll(".alternative-card.recommended").Should().NotBeEmpty();
    }

    #endregion

    #region Generic Alternatives Tests

    [Fact]
    public void CloudAlternativesPanel_AlwaysRendersGenericSection()
    {
        // Arrange
        var alternatives = new List<CloudAlternative>
        {
            CreateGenericAlternative("EKS", CloudProvider.AWS)
        };

        // Act
        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Alternatives, alternatives));

        // Assert
        cut.FindAll(".alternatives-section.secondary").Should().NotBeEmpty();
    }

    [Fact]
    public void CloudAlternativesPanel_GenericSection_ShowsOtherOptionsHeader()
    {
        // Act
        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Alternatives, new List<CloudAlternative>()));

        // Assert
        cut.Markup.Should().Contain("Other Cloud Options");
    }

    [Fact]
    public void CloudAlternativesPanel_MultipleGenericAlternatives_RendersAllCards()
    {
        // Arrange
        var alternatives = new List<CloudAlternative>
        {
            CreateGenericAlternative("EKS", CloudProvider.AWS),
            CreateGenericAlternative("AKS", CloudProvider.Azure),
            CreateGenericAlternative("GKE", CloudProvider.GCP)
        };

        // Act
        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Alternatives, alternatives));

        // Assert
        var cards = cut.FindAll(".alternative-card");
        cards.Should().HaveCount(3);
    }

    #endregion

    #region Card Content Tests

    [Fact]
    public void CloudAlternativesPanel_Card_DisplaysName()
    {
        // Arrange
        var alternatives = new List<CloudAlternative>
        {
            CreateGenericAlternative("EKS", CloudProvider.AWS)
        };

        // Act
        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Alternatives, alternatives));

        // Assert
        cut.Markup.Should().Contain("EKS");
    }

    [Fact]
    public void CloudAlternativesPanel_Card_DisplaysServiceName()
    {
        // Arrange
        var alt = CreateGenericAlternative("EKS", CloudProvider.AWS);
        alt.ServiceName = "Elastic Kubernetes Service";

        // Act
        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Alternatives, new List<CloudAlternative> { alt }));

        // Assert
        cut.Markup.Should().Contain("Elastic Kubernetes Service");
    }

    [Fact]
    public void CloudAlternativesPanel_Card_DisplaysDescription()
    {
        // Arrange
        var alt = CreateGenericAlternative("EKS", CloudProvider.AWS);
        alt.Description = "Managed Kubernetes on AWS";

        // Act
        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Alternatives, new List<CloudAlternative> { alt }));

        // Assert
        cut.Markup.Should().Contain("Managed Kubernetes on AWS");
    }

    [Fact]
    public void CloudAlternativesPanel_Card_DisplaysFeatures()
    {
        // Arrange
        var alt = CreateGenericAlternative("EKS", CloudProvider.AWS);
        alt.Features = new List<string> { "Feature 1", "Feature 2", "Feature 3" };

        // Act
        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Alternatives, new List<CloudAlternative> { alt }));

        // Assert
        cut.Markup.Should().Contain("Feature 1");
        cut.Markup.Should().Contain("Feature 2");
        cut.Markup.Should().Contain("Feature 3");
    }

    [Fact]
    public void CloudAlternativesPanel_Card_LimitsFeaturesToThree()
    {
        // Arrange
        var alt = CreateGenericAlternative("EKS", CloudProvider.AWS);
        alt.Features = new List<string> { "F1", "F2", "F3", "F4", "F5" };

        // Act
        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Alternatives, new List<CloudAlternative> { alt }));

        // Assert
        var features = cut.FindAll(".card-features li");
        features.Should().HaveCount(3);
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
    public void CloudAlternativesPanel_DisplaysCorrectProviderIcon(CloudProvider provider, string expectedIcon)
    {
        // Arrange
        var alt = CreateGenericAlternative("Test", provider);

        // Act
        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Alternatives, new List<CloudAlternative> { alt }));

        // Assert
        cut.Find(".provider-icon").TextContent.Should().Be(expectedIcon);
    }

    #endregion

    #region Card Selection Tests

    [Fact]
    public async Task CloudAlternativesPanel_ClickingCard_SelectsIt()
    {
        // Arrange
        var alternatives = new List<CloudAlternative>
        {
            CreateGenericAlternative("EKS", CloudProvider.AWS)
        };

        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Alternatives, alternatives));

        // Act
        var card = cut.Find(".alternative-card");
        await cut.InvokeAsync(() => card.Click());

        // Assert
        cut.FindAll(".alternative-card.selected").Should().NotBeEmpty();
    }

    [Fact]
    public async Task CloudAlternativesPanel_ClickingCard_InvokesCallback()
    {
        // Arrange
        CloudAlternative? selectedAlt = null;
        var alternatives = new List<CloudAlternative>
        {
            CreateGenericAlternative("EKS", CloudProvider.AWS)
        };

        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Alternatives, alternatives)
            .Add(p => p.OnAlternativeSelected,
                EventCallback.Factory.Create<CloudAlternative>(this, a => selectedAlt = a)));

        // Act
        var card = cut.Find(".alternative-card");
        await cut.InvokeAsync(() => card.Click());

        // Assert
        selectedAlt.Should().NotBeNull();
        selectedAlt!.Name.Should().Be("EKS");
    }

    #endregion

    #region Compare Button Tests

    [Fact]
    public void CloudAlternativesPanel_Card_HasCompareButton()
    {
        // Arrange
        var alternatives = new List<CloudAlternative>
        {
            CreateGenericAlternative("EKS", CloudProvider.AWS)
        };

        // Act
        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Alternatives, alternatives));

        // Assert
        cut.FindAll(".btn-compare").Should().NotBeEmpty();
        cut.Markup.Should().Contain("Compare Costs");
    }

    [Fact]
    public async Task CloudAlternativesPanel_ClickingCompare_ShowsComparisonPanel()
    {
        // Arrange
        var alternatives = new List<CloudAlternative>
        {
            CreateGenericAlternative("EKS", CloudProvider.AWS)
        };

        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Alternatives, alternatives));

        // Act
        var compareBtn = cut.Find(".btn-compare");
        await cut.InvokeAsync(() => compareBtn.Click());

        // Assert
        cut.FindAll(".comparison-panel").Should().NotBeEmpty();
    }

    [Fact]
    public async Task CloudAlternativesPanel_ComparisonPanel_ShowsSelectedAlternativeName()
    {
        // Arrange
        var alternatives = new List<CloudAlternative>
        {
            CreateGenericAlternative("EKS", CloudProvider.AWS)
        };

        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Alternatives, alternatives));

        // Act
        var compareBtn = cut.Find(".btn-compare");
        await cut.InvokeAsync(() => compareBtn.Click());

        // Assert
        cut.Find(".comparison-header").TextContent.Should().Contain("EKS");
    }

    #endregion

    #region Close Comparison Tests

    [Fact]
    public async Task CloudAlternativesPanel_ClosingComparison_HidesPanel()
    {
        // Arrange
        var alternatives = new List<CloudAlternative>
        {
            CreateGenericAlternative("EKS", CloudProvider.AWS)
        };

        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Alternatives, alternatives));

        // Open comparison first
        var compareBtn = cut.Find(".btn-compare");
        await cut.InvokeAsync(() => compareBtn.Click());

        // Assert panel is visible
        cut.FindAll(".comparison-panel").Should().NotBeEmpty();

        // Act - Close the panel
        var closeBtn = cut.Find(".close-btn");
        await cut.InvokeAsync(() => closeBtn.Click());

        // Assert
        cut.FindAll(".comparison-panel").Should().BeEmpty();
    }

    #endregion

    #region Documentation Link Tests

    [Fact]
    public void CloudAlternativesPanel_WithDocsUrl_ShowsDocsLink()
    {
        // Arrange
        var alt = CreateGenericAlternative("EKS", CloudProvider.AWS);
        alt.DocumentationUrl = "https://docs.aws.amazon.com/eks";

        // Act
        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Alternatives, new List<CloudAlternative> { alt }));

        // Assert
        cut.FindAll(".btn-docs").Should().NotBeEmpty();
        cut.Markup.Should().Contain("Docs ↗");
    }

    [Fact]
    public void CloudAlternativesPanel_WithDocsUrl_LinkHasCorrectHref()
    {
        // Arrange
        var alt = CreateGenericAlternative("EKS", CloudProvider.AWS);
        alt.DocumentationUrl = "https://docs.aws.amazon.com/eks";

        // Act
        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Alternatives, new List<CloudAlternative> { alt }));

        // Assert
        var docsLink = cut.Find(".btn-docs");
        docsLink.GetAttribute("href").Should().Be("https://docs.aws.amazon.com/eks");
        docsLink.GetAttribute("target").Should().Be("_blank");
    }

    [Fact]
    public void CloudAlternativesPanel_NoDocsUrl_HidesDocsLink()
    {
        // Arrange
        var alt = CreateGenericAlternative("EKS", CloudProvider.AWS);
        alt.DocumentationUrl = null;

        // Act
        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Alternatives, new List<CloudAlternative> { alt }));

        // Assert
        cut.FindAll(".btn-docs").Should().BeEmpty();
    }

    [Fact]
    public void CloudAlternativesPanel_EmptyDocsUrl_HidesDocsLink()
    {
        // Arrange
        var alt = CreateGenericAlternative("EKS", CloudProvider.AWS);
        alt.DocumentationUrl = "";

        // Act
        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Alternatives, new List<CloudAlternative> { alt }));

        // Assert
        cut.FindAll(".btn-docs").Should().BeEmpty();
    }

    #endregion

    #region Empty State Tests

    [Fact]
    public void CloudAlternativesPanel_NoAlternatives_RendersEmptyGrid()
    {
        // Act
        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Alternatives, new List<CloudAlternative>()));

        // Assert
        cut.FindAll(".alternative-card").Should().BeEmpty();
    }

    [Fact]
    public void CloudAlternativesPanel_NullDistribution_DoesNotThrow()
    {
        // Act
        var action = () => RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Distribution, null as Distribution?)
            .Add(p => p.Alternatives, new List<CloudAlternative>()));

        // Assert
        action.Should().NotThrow();
    }

    #endregion

    #region EventCallback Safety Tests

    [Fact]
    public async Task CloudAlternativesPanel_NoCallback_ClickDoesNotThrow()
    {
        // Arrange
        var alternatives = new List<CloudAlternative>
        {
            CreateGenericAlternative("EKS", CloudProvider.AWS)
        };

        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Alternatives, alternatives));
        // No OnAlternativeSelected callback provided

        // Act & Assert
        var action = async () =>
        {
            var card = cut.Find(".alternative-card");
            await cut.InvokeAsync(() => card.Click());
        };

        await action.Should().NotThrowAsync();
    }

    #endregion

    #region Feature Display Tests

    [Fact]
    public void CloudAlternativesPanel_NoFeatures_DoesNotRenderFeaturesList()
    {
        // Arrange
        var alt = CreateGenericAlternative("EKS", CloudProvider.AWS);
        alt.Features = new List<string>(); // Empty features

        // Act
        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Alternatives, new List<CloudAlternative> { alt }));

        // Assert
        cut.FindAll(".card-features").Should().BeEmpty();
    }

    [Fact]
    public void CloudAlternativesPanel_WithFeatures_RendersFeaturesList()
    {
        // Arrange
        var alt = CreateGenericAlternative("EKS", CloudProvider.AWS);
        alt.Features = new List<string> { "Auto-scaling", "Multi-AZ" };

        // Act
        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Alternatives, new List<CloudAlternative> { alt }));

        // Assert
        cut.FindAll(".card-features").Should().NotBeEmpty();
    }

    #endregion

    #region Mixed Alternatives Tests

    [Fact]
    public void CloudAlternativesPanel_MixedAlternatives_RendersInCorrectSections()
    {
        // Arrange
        var alternatives = new List<CloudAlternative>
        {
            CreateDistributionSpecificAlternative("ROSA", CloudProvider.AWS),
            CreateGenericAlternative("EKS", CloudProvider.AWS),
            CreateGenericAlternative("GKE", CloudProvider.GCP)
        };

        // Act
        var cut = RenderComponent<CloudAlternativesPanel>(parameters => parameters
            .Add(p => p.Distribution, Distribution.OpenShift)
            .Add(p => p.Alternatives, alternatives));

        // Assert
        // Primary section should have distribution-specific
        var primarySection = cut.Find(".alternatives-section.primary");
        primarySection.TextContent.Should().Contain("ROSA");

        // Secondary section should have generic
        var secondarySection = cut.Find(".alternatives-section.secondary");
        secondarySection.TextContent.Should().Contain("EKS");
        secondarySection.TextContent.Should().Contain("GKE");
    }

    #endregion

    #region Helper Methods

    private static CloudAlternative CreateGenericAlternative(string name, CloudProvider provider)
    {
        return new CloudAlternative
        {
            Name = name,
            Provider = provider,
            ServiceName = $"{name} Service",
            Description = $"Managed {name} service",
            IsDistributionSpecific = false,
            IsRecommended = false,
            Features = new List<string>(),
            DocumentationUrl = null
        };
    }

    private static CloudAlternative CreateDistributionSpecificAlternative(
        string name,
        CloudProvider provider,
        bool isRecommended = false)
    {
        return new CloudAlternative
        {
            Name = name,
            Provider = provider,
            ServiceName = $"{name} Service",
            Description = $"Managed {name} service",
            IsDistributionSpecific = true,
            IsRecommended = isRecommended,
            Features = new List<string>(),
            DocumentationUrl = null
        };
    }

    #endregion
}
