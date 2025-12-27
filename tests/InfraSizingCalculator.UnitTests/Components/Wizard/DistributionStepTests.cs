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
/// Tests for DistributionStep component - Step 4 of the wizard (K8s Distribution Selection)
/// </summary>
public class DistributionStepTests : TestContext
{
    private static readonly NodeSpecs DefaultNodeSpecs = new(8, 32, 100);

    private static readonly List<DistributionConfig> SampleDistributions = new()
    {
        new DistributionConfig
        {
            Distribution = Distribution.OpenShift,
            Name = "OpenShift",
            Vendor = "Red Hat",
            Icon = "🔴",
            BrandColor = "#EE0000",
            Tags = new[] { "Enterprise", "Self-Managed" },
            HasManagedControlPlane = false,
            ProdControlPlane = DefaultNodeSpecs,
            NonProdControlPlane = DefaultNodeSpecs,
            ProdWorker = DefaultNodeSpecs,
            NonProdWorker = DefaultNodeSpecs
        },
        new DistributionConfig
        {
            Distribution = Distribution.EKS,
            Name = "Amazon EKS",
            Vendor = "AWS",
            Icon = "🟠",
            BrandColor = "#FF9900",
            Tags = new[] { "Cloud", "Managed" },
            HasManagedControlPlane = true,
            ProdControlPlane = DefaultNodeSpecs,
            NonProdControlPlane = DefaultNodeSpecs,
            ProdWorker = DefaultNodeSpecs,
            NonProdWorker = DefaultNodeSpecs
        },
        new DistributionConfig
        {
            Distribution = Distribution.AKS,
            Name = "Azure AKS",
            Vendor = "Microsoft",
            Icon = "🔵",
            BrandColor = "#0078D4",
            Tags = new[] { "Cloud", "Managed" },
            HasManagedControlPlane = true,
            ProdControlPlane = DefaultNodeSpecs,
            NonProdControlPlane = DefaultNodeSpecs,
            ProdWorker = DefaultNodeSpecs,
            NonProdWorker = DefaultNodeSpecs
        },
        new DistributionConfig
        {
            Distribution = Distribution.Rancher,
            Name = "Rancher",
            Vendor = "SUSE",
            Icon = "🐄",
            BrandColor = "#0075A8",
            Tags = new[] { "Multi-Cloud", "Self-Managed" },
            HasManagedControlPlane = false,
            ProdControlPlane = DefaultNodeSpecs,
            NonProdControlPlane = DefaultNodeSpecs,
            ProdWorker = DefaultNodeSpecs,
            NonProdWorker = DefaultNodeSpecs
        }
    };

    #region Rendering Tests

    [Fact]
    public void DistributionStep_RendersWithDefaultState()
    {
        // Act
        var cut = RenderComponent<DistributionStep>(parameters => parameters
            .Add(p => p.Distributions, SampleDistributions));

        // Assert
        cut.FindComponent<SectionHeader>().Should().NotBeNull();
        cut.FindComponent<FilterButtons>().Should().NotBeNull();
        cut.FindAll(".distro-grid").Should().HaveCount(1);
    }

    [Fact]
    public void DistributionStep_RendersDistributionCards()
    {
        // Act
        var cut = RenderComponent<DistributionStep>(parameters => parameters
            .Add(p => p.Distributions, SampleDistributions));

        // Assert
        cut.FindAll(".distro-card").Should().HaveCount(4);
    }

    [Fact]
    public void DistributionStep_RendersEmptyGridWhenNoDistributions()
    {
        // Act
        var cut = RenderComponent<DistributionStep>(parameters => parameters
            .Add(p => p.Distributions, Enumerable.Empty<DistributionConfig>()));

        // Assert
        cut.FindAll(".distro-card").Should().BeEmpty();
    }

    [Fact]
    public void DistributionStep_RendersSectionHeader()
    {
        // Act
        var cut = RenderComponent<DistributionStep>(parameters => parameters
            .Add(p => p.Distributions, SampleDistributions));

        // Assert
        var header = cut.FindComponent<SectionHeader>();
        header.Instance.Title.Should().Be("Select Kubernetes Distribution");
        header.Instance.ShowInfoButton.Should().BeTrue();
    }

    [Fact]
    public void DistributionStep_DisplaysDistributionDetails()
    {
        // Act
        var cut = RenderComponent<DistributionStep>(parameters => parameters
            .Add(p => p.Distributions, SampleDistributions));

        // Assert
        var firstCard = cut.FindAll(".distro-card")[0];
        firstCard.QuerySelector(".distro-name")!.TextContent.Should().Be("OpenShift");
        firstCard.QuerySelector(".distro-vendor")!.TextContent.Should().Be("Red Hat");
    }

    [Fact]
    public void DistributionStep_DisplaysTags()
    {
        // Act
        var cut = RenderComponent<DistributionStep>(parameters => parameters
            .Add(p => p.Distributions, SampleDistributions));

        // Assert
        var firstCard = cut.FindAll(".distro-card")[0];
        var tags = firstCard.QuerySelectorAll(".distro-tag");
        tags.Should().HaveCount(2);
        tags[0].TextContent.Should().Be("Enterprise");
        tags[1].TextContent.Should().Be("Self-Managed");
    }

    [Fact]
    public void DistributionStep_AppliesBrandColor()
    {
        // Act
        var cut = RenderComponent<DistributionStep>(parameters => parameters
            .Add(p => p.Distributions, SampleDistributions));

        // Assert
        var firstCard = cut.FindAll(".distro-card")[0];
        firstCard.GetAttribute("style").Should().Contain("--brand-color: #EE0000");
    }

    #endregion

    #region Filter Tests

    [Fact]
    public void DistributionStep_RendersFilterButtons()
    {
        // Act
        var cut = RenderComponent<DistributionStep>(parameters => parameters
            .Add(p => p.Distributions, SampleDistributions));

        // Assert
        var filterButtons = cut.FindComponent<FilterButtons>();
        filterButtons.Should().NotBeNull();
        filterButtons.Instance.Label.Should().Be("Filter:");
    }

    [Fact]
    public void DistributionStep_DefaultFilterShowsAllDistributions()
    {
        // Act
        var cut = RenderComponent<DistributionStep>(parameters => parameters
            .Add(p => p.Distributions, SampleDistributions)
            .Add(p => p.SelectedFilter, "all"));

        // Assert
        cut.FindAll(".distro-card").Should().HaveCount(4);
    }

    [Fact]
    public void DistributionStep_OnPremFilterShowsOnlyOnPremDistributions()
    {
        // Act
        var cut = RenderComponent<DistributionStep>(parameters => parameters
            .Add(p => p.Distributions, SampleDistributions)
            .Add(p => p.SelectedFilter, "on-prem"));

        // Assert - Should show OpenShift and Rancher (HasManagedControlPlane = false)
        var cards = cut.FindAll(".distro-card");
        cards.Should().HaveCount(2);
    }

    [Fact]
    public void DistributionStep_CloudFilterShowsOnlyCloudDistributions()
    {
        // Act
        var cut = RenderComponent<DistributionStep>(parameters => parameters
            .Add(p => p.Distributions, SampleDistributions)
            .Add(p => p.SelectedFilter, "cloud"));

        // Assert - Should show EKS and AKS (HasManagedControlPlane = true)
        var cards = cut.FindAll(".distro-card");
        cards.Should().HaveCount(2);
    }

    [Fact]
    public async Task DistributionStep_FilterChange_InvokesOnFilterChanged()
    {
        // Arrange
        string? selectedFilter = null;
        var cut = RenderComponent<DistributionStep>(parameters => parameters
            .Add(p => p.Distributions, SampleDistributions)
            .Add(p => p.OnFilterChanged, EventCallback.Factory.Create<string>(this,
                f => selectedFilter = f)));

        // Act
        var filterButtons = cut.FindComponent<FilterButtons>();
        await cut.InvokeAsync(() => filterButtons.Instance.OnValueChanged.InvokeAsync("cloud"));

        // Assert
        selectedFilter.Should().Be("cloud");
    }

    #endregion

    #region Selection State Tests

    [Fact]
    public void DistributionStep_NoSelectionByDefault()
    {
        // Act
        var cut = RenderComponent<DistributionStep>(parameters => parameters
            .Add(p => p.Distributions, SampleDistributions));

        // Assert
        cut.FindAll(".distro-card.selected").Should().BeEmpty();
    }

    [Fact]
    public void DistributionStep_ShowsSelectedDistribution()
    {
        // Act
        var cut = RenderComponent<DistributionStep>(parameters => parameters
            .Add(p => p.Distributions, SampleDistributions)
            .Add(p => p.SelectedDistribution, Distribution.OpenShift));

        // Assert
        var selectedCards = cut.FindAll(".distro-card.selected");
        selectedCards.Should().HaveCount(1);
        selectedCards[0].QuerySelector(".distro-name")!.TextContent.Should().Be("OpenShift");
    }

    [Fact]
    public void DistributionStep_OnlyOneCardSelectedAtATime()
    {
        // Act
        var cut = RenderComponent<DistributionStep>(parameters => parameters
            .Add(p => p.Distributions, SampleDistributions)
            .Add(p => p.SelectedDistribution, Distribution.EKS));

        // Assert
        cut.FindAll(".distro-card.selected").Should().HaveCount(1);
        cut.FindAll(".distro-card").Should().HaveCount(4);
    }

    [Fact]
    public void DistributionStep_NullSelectedDistribution_ShowsNoSelection()
    {
        // Act
        var cut = RenderComponent<DistributionStep>(parameters => parameters
            .Add(p => p.Distributions, SampleDistributions)
            .Add(p => p.SelectedDistribution, (Distribution?)null));

        // Assert
        cut.FindAll(".distro-card.selected").Should().BeEmpty();
    }

    [Fact]
    public void DistributionStep_SelectedDistributionVisibleWithFilter()
    {
        // Arrange - Select an on-prem distribution
        var cut = RenderComponent<DistributionStep>(parameters => parameters
            .Add(p => p.Distributions, SampleDistributions)
            .Add(p => p.SelectedDistribution, Distribution.OpenShift)
            .Add(p => p.SelectedFilter, "on-prem"));

        // Assert - Should still be visible and selected
        var selectedCards = cut.FindAll(".distro-card.selected");
        selectedCards.Should().HaveCount(1);
    }

    [Fact]
    public void DistributionStep_SelectedDistributionHiddenByFilter()
    {
        // Arrange - Select an on-prem distribution but filter to cloud
        var cut = RenderComponent<DistributionStep>(parameters => parameters
            .Add(p => p.Distributions, SampleDistributions)
            .Add(p => p.SelectedDistribution, Distribution.OpenShift)
            .Add(p => p.SelectedFilter, "cloud"));

        // Assert - OpenShift should not be visible (filtered out)
        var cards = cut.FindAll(".distro-card");
        cards.Should().HaveCount(2);
        cut.FindAll(".distro-card.selected").Should().BeEmpty();
    }

    #endregion

    #region Event Callback Tests

    [Fact]
    public async Task DistributionStep_ClickingCard_InvokesOnDistributionChange()
    {
        // Arrange
        Distribution? selectedDistro = null;
        var cut = RenderComponent<DistributionStep>(parameters => parameters
            .Add(p => p.Distributions, SampleDistributions)
            .Add(p => p.OnDistributionChange, EventCallback.Factory.Create<Distribution>(this,
                d => selectedDistro = d)));

        // Act
        var firstCard = cut.FindAll(".distro-card")[0];
        await cut.InvokeAsync(() => firstCard.Click());

        // Assert
        selectedDistro.Should().Be(Distribution.OpenShift);
    }

    [Fact]
    public async Task DistributionStep_ClickingDifferentCard_InvokesWithCorrectDistribution()
    {
        // Arrange
        Distribution? selectedDistro = null;
        var cut = RenderComponent<DistributionStep>(parameters => parameters
            .Add(p => p.Distributions, SampleDistributions)
            .Add(p => p.OnDistributionChange, EventCallback.Factory.Create<Distribution>(this,
                d => selectedDistro = d)));

        // Act - Click EKS (second card)
        var eksCard = cut.FindAll(".distro-card")[1];
        await cut.InvokeAsync(() => eksCard.Click());

        // Assert
        selectedDistro.Should().Be(Distribution.EKS);
    }

    [Fact]
    public async Task DistributionStep_ClickingInfoButton_InvokesOnInfoClick()
    {
        // Arrange
        bool infoClicked = false;
        var cut = RenderComponent<DistributionStep>(parameters => parameters
            .Add(p => p.Distributions, SampleDistributions)
            .Add(p => p.OnInfoClick, EventCallback.Factory.Create(this, () => infoClicked = true)));

        // Act
        var header = cut.FindComponent<SectionHeader>();
        await cut.InvokeAsync(() => header.Instance.OnInfoClick.InvokeAsync());

        // Assert
        infoClicked.Should().BeTrue();
    }

    [Fact]
    public async Task DistributionStep_NoDelegate_DoesNotThrowOnClick()
    {
        // Arrange
        var cut = RenderComponent<DistributionStep>(parameters => parameters
            .Add(p => p.Distributions, SampleDistributions));

        // Act & Assert
        var action = async () =>
        {
            var card = cut.FindAll(".distro-card")[0];
            await cut.InvokeAsync(() => card.Click());
        };

        await action.Should().NotThrowAsync();
    }

    #endregion

    #region Parameter Update Tests

    [Fact]
    public void DistributionStep_UpdatesSelectionWhenParameterChanges()
    {
        // Arrange
        var cut = RenderComponent<DistributionStep>(parameters => parameters
            .Add(p => p.Distributions, SampleDistributions)
            .Add(p => p.SelectedDistribution, Distribution.OpenShift));

        // Assert initial state
        var initialSelected = cut.FindAll(".distro-card.selected");
        initialSelected[0].QuerySelector(".distro-name")!.TextContent.Should().Be("OpenShift");

        // Act - Change selection
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.SelectedDistribution, Distribution.EKS));

        // Assert
        var newSelected = cut.FindAll(".distro-card.selected");
        newSelected[0].QuerySelector(".distro-name")!.TextContent.Should().Be("Amazon EKS");
    }

    [Fact]
    public void DistributionStep_UpdatesFilteredList()
    {
        // Arrange
        var cut = RenderComponent<DistributionStep>(parameters => parameters
            .Add(p => p.Distributions, SampleDistributions)
            .Add(p => p.SelectedFilter, "all"));

        // Assert initial state
        cut.FindAll(".distro-card").Should().HaveCount(4);

        // Act - Change filter
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.SelectedFilter, "cloud"));

        // Assert
        cut.FindAll(".distro-card").Should().HaveCount(2);
    }

    [Fact]
    public void DistributionStep_UpdatesDistributionsList()
    {
        // Arrange
        var cut = RenderComponent<DistributionStep>(parameters => parameters
            .Add(p => p.Distributions, SampleDistributions.Take(2)));

        // Assert initial state
        cut.FindAll(".distro-card").Should().HaveCount(2);

        // Act - Update distributions
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.Distributions, SampleDistributions));

        // Assert
        cut.FindAll(".distro-card").Should().HaveCount(4);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void DistributionStep_HandlesEmptyDistributionsList()
    {
        // Act
        var cut = RenderComponent<DistributionStep>(parameters => parameters
            .Add(p => p.Distributions, new List<DistributionConfig>()));

        // Assert
        cut.FindAll(".distro-card").Should().BeEmpty();
        cut.FindAll(".distro-grid").Should().HaveCount(1);
    }

    [Fact]
    public void DistributionStep_SelectedDistributionNotInList_ShowsNoSelection()
    {
        // Arrange - Select a distribution that's not in the list
        var limitedDistros = SampleDistributions.Where(d => d.Distribution != Distribution.Rancher).ToList();

        // Act
        var cut = RenderComponent<DistributionStep>(parameters => parameters
            .Add(p => p.Distributions, limitedDistros)
            .Add(p => p.SelectedDistribution, Distribution.Rancher));

        // Assert
        cut.FindAll(".distro-card.selected").Should().BeEmpty();
    }

    [Fact]
    public void DistributionStep_AllCloudDistributions_EmptyOnPremFilter()
    {
        // Arrange - Only cloud distributions
        var cloudOnly = SampleDistributions.Where(d => d.HasManagedControlPlane).ToList();

        // Act
        var cut = RenderComponent<DistributionStep>(parameters => parameters
            .Add(p => p.Distributions, cloudOnly)
            .Add(p => p.SelectedFilter, "on-prem"));

        // Assert
        cut.FindAll(".distro-card").Should().BeEmpty();
    }

    [Fact]
    public void DistributionStep_AllOnPremDistributions_EmptyCloudFilter()
    {
        // Arrange - Only on-prem distributions
        var onPremOnly = SampleDistributions.Where(d => !d.HasManagedControlPlane).ToList();

        // Act
        var cut = RenderComponent<DistributionStep>(parameters => parameters
            .Add(p => p.Distributions, onPremOnly)
            .Add(p => p.SelectedFilter, "cloud"));

        // Assert
        cut.FindAll(".distro-card").Should().BeEmpty();
    }

    #endregion
}
