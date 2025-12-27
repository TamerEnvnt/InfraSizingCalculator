using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.Wizard.Steps;
using InfraSizingCalculator.Components.Shared;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components.Wizard;

/// <summary>
/// Tests for TechnologyStep component - Step 3 of the wizard
/// </summary>
public class TechnologyStepTests : TestContext
{
    private static readonly Dictionary<AppTier, TierSpecs> DefaultTiers = new()
    {
        { AppTier.Small, new TierSpecs(0.5, 1) },
        { AppTier.Medium, new TierSpecs(1, 2) },
        { AppTier.Large, new TierSpecs(2, 4) }
    };

    private static readonly List<TechnologyConfig> SampleTechnologies = new()
    {
        new TechnologyConfig
        {
            Technology = Technology.DotNet,
            Name = ".NET",
            Vendor = "Microsoft",
            Icon = "🔷",
            BrandColor = "#512BD4",
            Description = "Cross-platform development framework",
            PlatformType = PlatformType.Native,
            Tiers = DefaultTiers
        },
        new TechnologyConfig
        {
            Technology = Technology.Java,
            Name = "Java",
            Vendor = "Oracle",
            Icon = "☕",
            BrandColor = "#007396",
            Description = "Enterprise-grade platform",
            PlatformType = PlatformType.Native,
            Tiers = DefaultTiers
        },
        new TechnologyConfig
        {
            Technology = Technology.Mendix,
            Name = "Mendix",
            Vendor = "Siemens",
            Icon = "🟠",
            BrandColor = "#0CABF8",
            Description = "Low-code development platform",
            PlatformType = PlatformType.LowCode,
            Tiers = DefaultTiers
        }
    };

    #region Rendering Tests

    [Fact]
    public void TechnologyStep_RendersWithDefaultState()
    {
        // Act
        var cut = RenderComponent<TechnologyStep>();

        // Assert
        cut.FindComponent<SectionHeader>().Should().NotBeNull();
        cut.FindAll(".tech-grid").Should().HaveCount(1);
    }

    [Fact]
    public void TechnologyStep_RendersEmptyGridWhenNoTechnologies()
    {
        // Act
        var cut = RenderComponent<TechnologyStep>(parameters => parameters
            .Add(p => p.Technologies, Enumerable.Empty<TechnologyConfig>()));

        // Assert
        cut.FindAll(".tech-card").Should().BeEmpty();
    }

    [Fact]
    public void TechnologyStep_RendersTechnologyCards()
    {
        // Act
        var cut = RenderComponent<TechnologyStep>(parameters => parameters
            .Add(p => p.Technologies, SampleTechnologies));

        // Assert
        cut.FindAll(".tech-card").Should().HaveCount(3);
    }

    [Fact]
    public void TechnologyStep_RendersSectionHeaderWithInfoButton()
    {
        // Act
        var cut = RenderComponent<TechnologyStep>();

        // Assert
        var header = cut.FindComponent<SectionHeader>();
        header.Instance.Title.Should().Be("Select Technology");
        header.Instance.ShowInfoButton.Should().BeTrue();
    }

    [Fact]
    public void TechnologyStep_DisplaysTechnologyDetails()
    {
        // Act
        var cut = RenderComponent<TechnologyStep>(parameters => parameters
            .Add(p => p.Technologies, SampleTechnologies));

        // Assert
        var cards = cut.FindAll(".tech-card");

        // Check first card (.NET)
        cards[0].QuerySelector(".tech-name")!.TextContent.Should().Be(".NET");
        cards[0].QuerySelector(".tech-vendor")!.TextContent.Should().Be("Microsoft");
        cards[0].QuerySelector(".tech-desc")!.TextContent.Should().Be("Cross-platform development framework");
    }

    [Fact]
    public void TechnologyStep_ShowsLowCodeTag_ForLowCodeTechnologies()
    {
        // Act
        var cut = RenderComponent<TechnologyStep>(parameters => parameters
            .Add(p => p.Technologies, SampleTechnologies));

        // Assert
        var cards = cut.FindAll(".tech-card");

        // Native technologies should have "Native" tag
        cards[0].QuerySelector(".tech-tag.native")!.TextContent.Should().Be("Native");
        cards[1].QuerySelector(".tech-tag.native")!.TextContent.Should().Be("Native");

        // Low-code technology should have "Low-Code" tag
        cards[2].QuerySelector(".tech-tag.lowcode")!.TextContent.Should().Be("Low-Code");
    }

    [Fact]
    public void TechnologyStep_AppliesBrandColorToCards()
    {
        // Act
        var cut = RenderComponent<TechnologyStep>(parameters => parameters
            .Add(p => p.Technologies, SampleTechnologies));

        // Assert
        var cards = cut.FindAll(".tech-card");
        cards[0].GetAttribute("style").Should().Contain("--brand-color: #512BD4");
    }

    #endregion

    #region Selection State Tests

    [Fact]
    public void TechnologyStep_NoSelectionByDefault()
    {
        // Act
        var cut = RenderComponent<TechnologyStep>(parameters => parameters
            .Add(p => p.Technologies, SampleTechnologies));

        // Assert
        cut.FindAll(".tech-card.selected").Should().BeEmpty();
    }

    [Fact]
    public void TechnologyStep_ShowsSelectedTechnology()
    {
        // Act
        var cut = RenderComponent<TechnologyStep>(parameters => parameters
            .Add(p => p.Technologies, SampleTechnologies)
            .Add(p => p.SelectedTechnology, Technology.DotNet));

        // Assert
        var selectedCards = cut.FindAll(".tech-card.selected");
        selectedCards.Should().HaveCount(1);
        selectedCards[0].QuerySelector(".tech-name")!.TextContent.Should().Be(".NET");
    }

    [Fact]
    public void TechnologyStep_OnlyOneCardSelectedAtATime()
    {
        // Act
        var cut = RenderComponent<TechnologyStep>(parameters => parameters
            .Add(p => p.Technologies, SampleTechnologies)
            .Add(p => p.SelectedTechnology, Technology.Java));

        // Assert
        var allCards = cut.FindAll(".tech-card");
        var selectedCards = cut.FindAll(".tech-card.selected");

        selectedCards.Should().HaveCount(1);
        allCards.Should().HaveCount(3);
    }

    [Fact]
    public void TechnologyStep_NullSelectedTechnology_ShowsNoSelection()
    {
        // Act
        var cut = RenderComponent<TechnologyStep>(parameters => parameters
            .Add(p => p.Technologies, SampleTechnologies)
            .Add(p => p.SelectedTechnology, (Technology?)null));

        // Assert
        cut.FindAll(".tech-card.selected").Should().BeEmpty();
    }

    #endregion

    #region Event Callback Tests

    [Fact]
    public async Task TechnologyStep_ClickingCard_InvokesOnTechnologyChange()
    {
        // Arrange
        Technology? selectedTech = null;
        var cut = RenderComponent<TechnologyStep>(parameters => parameters
            .Add(p => p.Technologies, SampleTechnologies)
            .Add(p => p.OnTechnologyChange, EventCallback.Factory.Create<Technology>(this,
                t => selectedTech = t)));

        // Act
        var firstCard = cut.FindAll(".tech-card")[0];
        await cut.InvokeAsync(() => firstCard.Click());

        // Assert
        selectedTech.Should().Be(Technology.DotNet);
    }

    [Fact]
    public async Task TechnologyStep_ClickingDifferentCard_InvokesWithCorrectTechnology()
    {
        // Arrange
        Technology? selectedTech = null;
        var cut = RenderComponent<TechnologyStep>(parameters => parameters
            .Add(p => p.Technologies, SampleTechnologies)
            .Add(p => p.OnTechnologyChange, EventCallback.Factory.Create<Technology>(this,
                t => selectedTech = t)));

        // Act
        var mendixCard = cut.FindAll(".tech-card")[2];
        await cut.InvokeAsync(() => mendixCard.Click());

        // Assert
        selectedTech.Should().Be(Technology.Mendix);
    }

    [Fact]
    public async Task TechnologyStep_ClickingInfoButton_InvokesOnInfoClick()
    {
        // Arrange
        bool infoClicked = false;
        var cut = RenderComponent<TechnologyStep>(parameters => parameters
            .Add(p => p.Technologies, SampleTechnologies)
            .Add(p => p.OnInfoClick, EventCallback.Factory.Create(this, () => infoClicked = true)));

        // Act
        var header = cut.FindComponent<SectionHeader>();
        await cut.InvokeAsync(() => header.Instance.OnInfoClick.InvokeAsync());

        // Assert
        infoClicked.Should().BeTrue();
    }

    [Fact]
    public async Task TechnologyStep_NoDelegate_DoesNotThrowOnClick()
    {
        // Arrange
        var cut = RenderComponent<TechnologyStep>(parameters => parameters
            .Add(p => p.Technologies, SampleTechnologies));

        // Act & Assert
        var action = async () =>
        {
            var card = cut.FindAll(".tech-card")[0];
            await cut.InvokeAsync(() => card.Click());
        };

        await action.Should().NotThrowAsync();
    }

    #endregion

    #region Parameter Update Tests

    [Fact]
    public void TechnologyStep_UpdatesSelectionWhenParameterChanges()
    {
        // Arrange
        var cut = RenderComponent<TechnologyStep>(parameters => parameters
            .Add(p => p.Technologies, SampleTechnologies)
            .Add(p => p.SelectedTechnology, Technology.DotNet));

        // Assert initial state
        cut.FindAll(".tech-card.selected").Should().HaveCount(1);

        // Act - Change selection
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.SelectedTechnology, Technology.Java));

        // Assert
        var selectedCards = cut.FindAll(".tech-card.selected");
        selectedCards.Should().HaveCount(1);
        selectedCards[0].QuerySelector(".tech-name")!.TextContent.Should().Be("Java");
    }

    [Fact]
    public void TechnologyStep_UpdatesTechnologiesList()
    {
        // Arrange
        var cut = RenderComponent<TechnologyStep>(parameters => parameters
            .Add(p => p.Technologies, SampleTechnologies.Take(1)));

        // Assert initial state
        cut.FindAll(".tech-card").Should().HaveCount(1);

        // Act - Update technologies
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.Technologies, SampleTechnologies));

        // Assert
        cut.FindAll(".tech-card").Should().HaveCount(3);
    }

    [Fact]
    public void TechnologyStep_ClearsSelectionWhenSetToNull()
    {
        // Arrange
        var cut = RenderComponent<TechnologyStep>(parameters => parameters
            .Add(p => p.Technologies, SampleTechnologies)
            .Add(p => p.SelectedTechnology, Technology.DotNet));

        // Act
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.SelectedTechnology, (Technology?)null));

        // Assert
        cut.FindAll(".tech-card.selected").Should().BeEmpty();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void TechnologyStep_HandlesEmptyTechnologiesList()
    {
        // Act
        var cut = RenderComponent<TechnologyStep>(parameters => parameters
            .Add(p => p.Technologies, new List<TechnologyConfig>()));

        // Assert
        cut.FindAll(".tech-card").Should().BeEmpty();
        cut.FindAll(".tech-grid").Should().HaveCount(1);
    }

    [Fact]
    public void TechnologyStep_SelectedTechnologyNotInList_ShowsNoSelection()
    {
        // Arrange - Select a technology that's not in the list
        var limitedTechs = SampleTechnologies.Where(t => t.Technology != Technology.Mendix).ToList();

        // Act
        var cut = RenderComponent<TechnologyStep>(parameters => parameters
            .Add(p => p.Technologies, limitedTechs)
            .Add(p => p.SelectedTechnology, Technology.Mendix));

        // Assert
        cut.FindAll(".tech-card.selected").Should().BeEmpty();
    }

    [Fact]
    public void TechnologyStep_SingleTechnology_RendersCorrectly()
    {
        // Act
        var cut = RenderComponent<TechnologyStep>(parameters => parameters
            .Add(p => p.Technologies, SampleTechnologies.Take(1)));

        // Assert
        cut.FindAll(".tech-card").Should().HaveCount(1);
    }

    #endregion
}
