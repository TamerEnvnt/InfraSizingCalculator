using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.Configuration;
using InfraSizingCalculator.Models.Enums;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components.Configuration;

/// <summary>
/// Tests for ClusterModeSelector component - Cluster mode selection sidebar
/// </summary>
public class ClusterModeSelectorTests : TestContext
{
    #region Rendering Tests

    [Fact]
    public void ClusterModeSelector_RendersContainer()
    {
        // Act
        var cut = RenderComponent<ClusterModeSelector>();

        // Assert
        cut.Find(".cluster-mode-sidebar").Should().NotBeNull();
    }

    [Fact]
    public void ClusterModeSelector_RendersHeader()
    {
        // Act
        var cut = RenderComponent<ClusterModeSelector>();

        // Assert
        cut.Find(".sidebar-header").TextContent.Should().Contain("Cluster Mode");
    }

    [Fact]
    public void ClusterModeSelector_RendersTwoModeOptions()
    {
        // Act
        var cut = RenderComponent<ClusterModeSelector>();

        // Assert
        cut.FindAll(".mode-option").Should().HaveCount(2);
    }

    [Fact]
    public void ClusterModeSelector_RendersMultiClusterOption()
    {
        // Act
        var cut = RenderComponent<ClusterModeSelector>();

        // Assert
        var options = cut.FindAll(".mode-option");
        options[0].TextContent.Should().Contain("Multi-Cluster");
        options[0].TextContent.Should().Contain("One cluster per env");
    }

    [Fact]
    public void ClusterModeSelector_RendersSingleClusterOption()
    {
        // Act
        var cut = RenderComponent<ClusterModeSelector>();

        // Assert
        var options = cut.FindAll(".mode-option");
        options[1].TextContent.Should().Contain("Single Cluster");
        options[1].TextContent.Should().Contain("One cluster total");
    }

    [Fact]
    public void ClusterModeSelector_HasInfoButton()
    {
        // Act
        var cut = RenderComponent<ClusterModeSelector>();

        // Assert
        cut.Find(".sidebar-header").InnerHtml.Should().NotBeEmpty();
    }

    [Fact]
    public void ClusterModeSelector_AppliesAdditionalClass()
    {
        // Act
        var cut = RenderComponent<ClusterModeSelector>(parameters => parameters
            .Add(p => p.AdditionalClass, "custom-class"));

        // Assert
        cut.Find(".cluster-mode-sidebar").ClassList.Should().Contain("custom-class");
    }

    #endregion

    #region Selection State Tests

    [Fact]
    public void ClusterModeSelector_MultiClusterSelectedByDefault()
    {
        // Act
        var cut = RenderComponent<ClusterModeSelector>();

        // Assert
        var options = cut.FindAll(".mode-option");
        options[0].ClassList.Should().Contain("selected");
        options[1].ClassList.Should().NotContain("selected");
    }

    [Fact]
    public void ClusterModeSelector_ShowsSharedClusterSelected()
    {
        // Act
        var cut = RenderComponent<ClusterModeSelector>(parameters => parameters
            .Add(p => p.SelectedMode, ClusterMode.SharedCluster));

        // Assert
        var options = cut.FindAll(".mode-option");
        options[0].ClassList.Should().NotContain("selected");
        options[1].ClassList.Should().Contain("selected");
    }

    [Fact]
    public void ClusterModeSelector_ShowsPerEnvironmentSelected()
    {
        // Act
        var cut = RenderComponent<ClusterModeSelector>(parameters => parameters
            .Add(p => p.SelectedMode, ClusterMode.PerEnvironment));

        // Assert
        var options = cut.FindAll(".mode-option");
        // PerEnvironment also shows Single Cluster as selected
        options[1].ClassList.Should().Contain("selected");
    }

    #endregion

    #region Single Cluster Scope Tests

    [Fact]
    public void ClusterModeSelector_ScopeSelector_HiddenForMultiCluster()
    {
        // Act
        var cut = RenderComponent<ClusterModeSelector>(parameters => parameters
            .Add(p => p.SelectedMode, ClusterMode.MultiCluster));

        // Assert
        cut.FindAll(".single-cluster-selector").Should().BeEmpty();
    }

    [Fact]
    public void ClusterModeSelector_ScopeSelector_ShownForSharedCluster()
    {
        // Act
        var cut = RenderComponent<ClusterModeSelector>(parameters => parameters
            .Add(p => p.SelectedMode, ClusterMode.SharedCluster));

        // Assert
        cut.Find(".single-cluster-selector").Should().NotBeNull();
    }

    [Fact]
    public void ClusterModeSelector_ScopeSelector_HasAllOptions()
    {
        // Act
        var cut = RenderComponent<ClusterModeSelector>(parameters => parameters
            .Add(p => p.SelectedMode, ClusterMode.SharedCluster));

        // Assert
        var options = cut.FindAll(".single-cluster-selector option");
        options.Should().HaveCount(6); // Shared, Dev, Test, Stage, Prod, DR
    }

    [Fact]
    public void ClusterModeSelector_ScopeSelector_ShowsSharedOption()
    {
        // Act
        var cut = RenderComponent<ClusterModeSelector>(parameters => parameters
            .Add(p => p.SelectedMode, ClusterMode.SharedCluster));

        // Assert
        var options = cut.FindAll(".single-cluster-selector option");
        options.Should().Contain(o => o.TextContent.Contains("Shared"));
    }

    [Fact]
    public void ClusterModeSelector_ScopeSelector_ShowsEnvironmentOptions()
    {
        // Act
        var cut = RenderComponent<ClusterModeSelector>(parameters => parameters
            .Add(p => p.SelectedMode, ClusterMode.SharedCluster));

        // Assert
        var options = cut.FindAll(".single-cluster-selector option");
        options.Should().Contain(o => o.TextContent.Contains("Dev"));
        options.Should().Contain(o => o.TextContent.Contains("Test"));
        options.Should().Contain(o => o.TextContent.Contains("Stage"));
        options.Should().Contain(o => o.TextContent.Contains("Prod"));
        options.Should().Contain(o => o.TextContent.Contains("DR"));
    }

    #endregion

    #region Event Callback Tests

    [Fact]
    public async Task ClusterModeSelector_ClickingMultiCluster_InvokesCallback()
    {
        // Arrange
        ClusterMode? selectedMode = null;
        var cut = RenderComponent<ClusterModeSelector>(parameters => parameters
            .Add(p => p.SelectedMode, ClusterMode.SharedCluster)
            .Add(p => p.OnModeChanged, EventCallback.Factory.Create<ClusterMode>(this, m => selectedMode = m)));

        // Act
        var multiOption = cut.FindAll(".mode-option")[0];
        await cut.InvokeAsync(() => multiOption.Click());

        // Assert
        selectedMode.Should().Be(ClusterMode.MultiCluster);
    }

    [Fact]
    public async Task ClusterModeSelector_ClickingSingleCluster_InvokesCallback()
    {
        // Arrange
        ClusterMode? selectedMode = null;
        var cut = RenderComponent<ClusterModeSelector>(parameters => parameters
            .Add(p => p.SelectedMode, ClusterMode.MultiCluster)
            .Add(p => p.OnModeChanged, EventCallback.Factory.Create<ClusterMode>(this, m => selectedMode = m)));

        // Act
        var singleOption = cut.FindAll(".mode-option")[1];
        await cut.InvokeAsync(() => singleOption.Click());

        // Assert
        selectedMode.Should().Be(ClusterMode.SharedCluster);
    }

    [Fact]
    public async Task ClusterModeSelector_ChangingScope_InvokesCallback()
    {
        // Arrange
        string? selectedScope = null;
        var cut = RenderComponent<ClusterModeSelector>(parameters => parameters
            .Add(p => p.SelectedMode, ClusterMode.SharedCluster)
            .Add(p => p.OnScopeChanged, EventCallback.Factory.Create<string>(this, s => selectedScope = s)));

        // Act
        var select = cut.Find(".single-cluster-selector select");
        await cut.InvokeAsync(() => select.Change("Dev"));

        // Assert
        selectedScope.Should().Be("Dev");
    }

    [Fact]
    public async Task ClusterModeSelector_NoCallback_DoesNotThrow()
    {
        // Arrange
        var cut = RenderComponent<ClusterModeSelector>();

        // Act & Assert
        var action = async () =>
        {
            var option = cut.FindAll(".mode-option")[1];
            await cut.InvokeAsync(() => option.Click());
        };

        await action.Should().NotThrowAsync();
    }

    #endregion

    #region Mode Icons Tests

    [Fact]
    public void ClusterModeSelector_MultiCluster_HasCSSIcon()
    {
        // Act
        var cut = RenderComponent<ClusterModeSelector>();

        // Assert - Multi-Cluster now uses CSS icon class instead of emoji
        var multiOption = cut.FindAll(".mode-option")[0];
        multiOption.QuerySelector(".mode-icon")!.ClassList.Should().Contain("icon");
    }

    [Fact]
    public void ClusterModeSelector_SingleCluster_HasNumberIcon()
    {
        // Act
        var cut = RenderComponent<ClusterModeSelector>();

        // Assert - Single Cluster now uses text "1" instead of emoji
        var singleOption = cut.FindAll(".mode-option")[1];
        singleOption.QuerySelector(".mode-icon")!.TextContent.Should().Be("1");
    }

    #endregion

    #region Parameter Update Tests

    [Fact]
    public void ClusterModeSelector_UpdatesSelection_WhenModeChanges()
    {
        // Arrange
        var cut = RenderComponent<ClusterModeSelector>(parameters => parameters
            .Add(p => p.SelectedMode, ClusterMode.MultiCluster));

        // Assert initial
        cut.FindAll(".mode-option")[0].ClassList.Should().Contain("selected");

        // Act
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.SelectedMode, ClusterMode.SharedCluster));

        // Assert
        cut.FindAll(".mode-option")[0].ClassList.Should().NotContain("selected");
        cut.FindAll(".mode-option")[1].ClassList.Should().Contain("selected");
    }

    [Fact]
    public void ClusterModeSelector_ShowsScopeSelector_WhenSwitchingToSingleCluster()
    {
        // Arrange
        var cut = RenderComponent<ClusterModeSelector>(parameters => parameters
            .Add(p => p.SelectedMode, ClusterMode.MultiCluster));

        // Assert initial - no scope selector
        cut.FindAll(".single-cluster-selector").Should().BeEmpty();

        // Act
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.SelectedMode, ClusterMode.SharedCluster));

        // Assert - scope selector appears
        cut.Find(".single-cluster-selector").Should().NotBeNull();
    }

    #endregion
}
