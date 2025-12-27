using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.Configuration;
using InfraSizingCalculator.Models.Enums;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components;

/// <summary>
/// Tests for ClusterModeSelector component
/// </summary>
public class ClusterModeSelectorTests : TestContext
{
    [Fact]
    public void ClusterModeSelector_RendersWithDefaultMode()
    {
        // Act
        var cut = RenderComponent<ClusterModeSelector>();

        // Assert
        cut.Find(".cluster-mode-sidebar").Should().NotBeNull();
        cut.Find(".sidebar-header span").TextContent.Should().Be("Cluster Mode");
    }

    [Fact]
    public void ClusterModeSelector_AppliesAdditionalClass()
    {
        // Act
        var cut = RenderComponent<ClusterModeSelector>(parameters => parameters
            .Add(p => p.AdditionalClass, "custom-sidebar"));

        // Assert
        cut.Find(".cluster-mode-sidebar").ClassList.Should().Contain("custom-sidebar");
    }

    [Fact]
    public void ClusterModeSelector_DisplaysInfoButton()
    {
        // Act
        var cut = RenderComponent<ClusterModeSelector>();

        // Assert
        var infoButton = cut.FindComponent<InfraSizingCalculator.Components.Shared.InfoButton>();
        infoButton.Should().NotBeNull();
        infoButton.Instance.Size.Should().Be("small");
    }

    [Fact]
    public void ClusterModeSelector_ShowsBothModeOptions()
    {
        // Act
        var cut = RenderComponent<ClusterModeSelector>();

        // Assert
        var modeOptions = cut.FindAll(".mode-option");
        modeOptions.Should().HaveCount(2);

        // Verify Multi-Cluster option
        modeOptions[0].QuerySelector(".mode-name")?.TextContent.Should().Be("Multi-Cluster");
        modeOptions[0].QuerySelector(".mode-desc")?.TextContent.Should().Be("One cluster per env");

        // Verify Single Cluster option
        modeOptions[1].QuerySelector(".mode-name")?.TextContent.Should().Be("Single Cluster");
        modeOptions[1].QuerySelector(".mode-desc")?.TextContent.Should().Be("One cluster total");
    }

    [Fact]
    public void ClusterModeSelector_MultiClusterIsSelectedByDefault()
    {
        // Act
        var cut = RenderComponent<ClusterModeSelector>();

        // Assert
        var modeOptions = cut.FindAll(".mode-option");
        modeOptions[0].ClassList.Should().Contain("selected"); // Multi-Cluster
        modeOptions[1].ClassList.Should().NotContain("selected"); // Single Cluster
    }

    [Fact]
    public void ClusterModeSelector_SelectsSingleClusterMode()
    {
        // Act
        var cut = RenderComponent<ClusterModeSelector>(parameters => parameters
            .Add(p => p.SelectedMode, ClusterMode.SharedCluster));

        // Assert
        var modeOptions = cut.FindAll(".mode-option");
        modeOptions[0].ClassList.Should().NotContain("selected"); // Multi-Cluster
        modeOptions[1].ClassList.Should().Contain("selected"); // Single Cluster
    }

    [Fact]
    public void ClusterModeSelector_SelectsPerEnvironmentMode()
    {
        // Act
        var cut = RenderComponent<ClusterModeSelector>(parameters => parameters
            .Add(p => p.SelectedMode, ClusterMode.PerEnvironment));

        // Assert - PerEnvironment is treated as single cluster mode
        var modeOptions = cut.FindAll(".mode-option");
        modeOptions[1].ClassList.Should().Contain("selected");
    }

    [Fact]
    public void ClusterModeSelector_HidesScopeSelector_InMultiClusterMode()
    {
        // Act
        var cut = RenderComponent<ClusterModeSelector>(parameters => parameters
            .Add(p => p.SelectedMode, ClusterMode.MultiCluster));

        // Assert
        cut.FindAll(".single-cluster-selector").Should().BeEmpty();
    }

    [Fact]
    public void ClusterModeSelector_ShowsScopeSelector_InSingleClusterMode()
    {
        // Act
        var cut = RenderComponent<ClusterModeSelector>(parameters => parameters
            .Add(p => p.SelectedMode, ClusterMode.SharedCluster));

        // Assert
        cut.FindAll(".single-cluster-selector").Should().HaveCount(1);
        cut.Find(".single-cluster-selector label").TextContent.Should().Be("Cluster Scope:");
    }

    [Fact]
    public void ClusterModeSelector_ScopeSelectorHasAllOptions()
    {
        // Act
        var cut = RenderComponent<ClusterModeSelector>(parameters => parameters
            .Add(p => p.SelectedMode, ClusterMode.SharedCluster));

        // Assert
        var options = cut.FindAll(".single-cluster-selector select option");
        options.Should().HaveCount(6);
        options[0].GetAttribute("value").Should().Be("Shared");
        options[1].GetAttribute("value").Should().Be("Dev");
        options[2].GetAttribute("value").Should().Be("Test");
        options[3].GetAttribute("value").Should().Be("Stage");
        options[4].GetAttribute("value").Should().Be("Prod");
        options[5].GetAttribute("value").Should().Be("DR");
    }

    [Fact]
    public void ClusterModeSelector_DisplaysCorrectScopeSelection()
    {
        // Act
        var cut = RenderComponent<ClusterModeSelector>(parameters => parameters
            .Add(p => p.SelectedMode, ClusterMode.SharedCluster)
            .Add(p => p.SingleClusterScope, "Prod"));

        // Assert
        var select = cut.Find(".single-cluster-selector select");
        select.GetAttribute("value").Should().Be("Prod");
    }

    [Fact]
    public async Task ClusterModeSelector_InvokesOnModeChanged_WhenMultiClusterClicked()
    {
        // Arrange
        ClusterMode? changedMode = null;
        var cut = RenderComponent<ClusterModeSelector>(parameters => parameters
            .Add(p => p.SelectedMode, ClusterMode.SharedCluster)
            .Add(p => p.OnModeChanged, mode => changedMode = mode));

        // Act
        await cut.InvokeAsync(() =>
        {
            cut.FindAll(".mode-option")[0].Click(); // Click Multi-Cluster
        });

        // Assert
        changedMode.Should().Be(ClusterMode.MultiCluster);
    }

    [Fact]
    public async Task ClusterModeSelector_InvokesOnModeChanged_WhenSingleClusterClicked()
    {
        // Arrange
        ClusterMode? changedMode = null;
        var cut = RenderComponent<ClusterModeSelector>(parameters => parameters
            .Add(p => p.SelectedMode, ClusterMode.MultiCluster)
            .Add(p => p.OnModeChanged, mode => changedMode = mode));

        // Act
        await cut.InvokeAsync(() =>
        {
            cut.FindAll(".mode-option")[1].Click(); // Click Single Cluster
        });

        // Assert
        changedMode.Should().Be(ClusterMode.SharedCluster);
    }

    [Fact]
    public async Task ClusterModeSelector_InvokesOnScopeChanged_WhenScopeChanged()
    {
        // Arrange
        string? changedScope = null;
        var cut = RenderComponent<ClusterModeSelector>(parameters => parameters
            .Add(p => p.SelectedMode, ClusterMode.SharedCluster)
            .Add(p => p.SingleClusterScope, "Shared")
            .Add(p => p.OnScopeChanged, scope => changedScope = scope));

        // Act
        await cut.InvokeAsync(() =>
        {
            var select = cut.Find(".single-cluster-selector select");
            select.Change("Prod");
        });

        // Assert
        changedScope.Should().Be("Prod");
    }

    [Fact]
    public async Task ClusterModeSelector_InvokesOnInfoClick_WhenInfoButtonClicked()
    {
        // Arrange
        bool infoClicked = false;
        var cut = RenderComponent<ClusterModeSelector>(parameters => parameters
            .Add(p => p.OnInfoClick, () => infoClicked = true));

        // Act
        await cut.InvokeAsync(() =>
        {
            cut.FindComponent<InfraSizingCalculator.Components.Shared.InfoButton>().Find("button").Click();
        });

        // Assert
        infoClicked.Should().BeTrue();
    }

    [Fact]
    public void ClusterModeSelector_AllScopeOptionsHaveDescriptiveText()
    {
        // Act
        var cut = RenderComponent<ClusterModeSelector>(parameters => parameters
            .Add(p => p.SelectedMode, ClusterMode.SharedCluster));

        // Assert - Scope options now use plain text instead of emojis
        var options = cut.FindAll(".single-cluster-selector select option");
        options[0].TextContent.Should().Contain("Shared");
        options[1].TextContent.Should().Contain("Dev");
        options[2].TextContent.Should().Contain("Test");
        options[3].TextContent.Should().Contain("Stage");
        options[4].TextContent.Should().Contain("Prod");
        options[5].TextContent.Should().Contain("DR");
    }

    [Fact]
    public void ClusterModeSelector_ModeOptionsHaveIcons()
    {
        // Act
        var cut = RenderComponent<ClusterModeSelector>();

        // Assert - Mode icons use CSS classes and text instead of emojis
        var modeIcons = cut.FindAll(".mode-icon");
        modeIcons[0].ClassList.Should().Contain("icon"); // Multi-Cluster uses CSS icon class
        modeIcons[1].TextContent.Should().Be("1"); // Single Cluster uses text "1"
    }

    [Fact]
    public async Task ClusterModeSelector_SwitchingFromMultiToSingle_ShowsScopeSelector()
    {
        // Arrange
        ClusterMode currentMode = ClusterMode.MultiCluster;
        var cut = RenderComponent<ClusterModeSelector>(parameters => parameters
            .Add(p => p.SelectedMode, currentMode)
            .Add(p => p.OnModeChanged, mode => currentMode = mode));

        // Assert - Initially no scope selector
        cut.FindAll(".single-cluster-selector").Should().BeEmpty();

        // Act - Switch to single cluster
        await cut.InvokeAsync(() =>
        {
            cut.SetParametersAndRender(parameters => parameters
                .Add(p => p.SelectedMode, ClusterMode.SharedCluster));
        });

        // Assert - Scope selector appears
        cut.FindAll(".single-cluster-selector").Should().HaveCount(1);
    }

    [Fact]
    public async Task ClusterModeSelector_SwitchingFromSingleToMulti_HidesScopeSelector()
    {
        // Arrange
        ClusterMode currentMode = ClusterMode.SharedCluster;
        var cut = RenderComponent<ClusterModeSelector>(parameters => parameters
            .Add(p => p.SelectedMode, currentMode)
            .Add(p => p.OnModeChanged, mode => currentMode = mode));

        // Assert - Initially has scope selector
        cut.FindAll(".single-cluster-selector").Should().HaveCount(1);

        // Act - Switch to multi cluster
        await cut.InvokeAsync(() =>
        {
            cut.SetParametersAndRender(parameters => parameters
                .Add(p => p.SelectedMode, ClusterMode.MultiCluster));
        });

        // Assert - Scope selector hidden
        cut.FindAll(".single-cluster-selector").Should().BeEmpty();
    }

    [Fact]
    public async Task ClusterModeSelector_DoesNotInvokeCallback_WhenSameModeClicked()
    {
        // Arrange
        int callCount = 0;
        var cut = RenderComponent<ClusterModeSelector>(parameters => parameters
            .Add(p => p.SelectedMode, ClusterMode.MultiCluster)
            .Add(p => p.OnModeChanged, mode => callCount++));

        // Act - Click already selected mode
        await cut.InvokeAsync(() =>
        {
            cut.FindAll(".mode-option")[0].Click(); // Click Multi-Cluster (already selected)
        });

        // Assert - Callback is still invoked (component doesn't prevent it)
        callCount.Should().Be(1);
    }

    [Fact]
    public async Task ClusterModeSelector_AllScopeValues_CanBeSelected()
    {
        // Arrange
        var scopes = new[] { "Shared", "Dev", "Test", "Stage", "Prod", "DR" };

        foreach (var scope in scopes)
        {
            string? selectedScope = null;
            var cut = RenderComponent<ClusterModeSelector>(parameters => parameters
                .Add(p => p.SelectedMode, ClusterMode.SharedCluster)
                .Add(p => p.SingleClusterScope, "Shared")
                .Add(p => p.OnScopeChanged, s => selectedScope = s));

            // Act
            await cut.InvokeAsync(() =>
            {
                cut.Find(".single-cluster-selector select").Change(scope);
            });

            // Assert
            selectedScope.Should().Be(scope, $"scope {scope} should be selectable");
        }
    }

    [Fact]
    public void ClusterModeSelector_NoCallbacksDoNotError()
    {
        // Arrange & Act - No callbacks provided
        var cut = RenderComponent<ClusterModeSelector>(parameters => parameters
            .Add(p => p.SelectedMode, ClusterMode.SharedCluster));

        // Assert - Clicking should not throw
        var actions = new Action[]
        {
            () => cut.FindAll(".mode-option")[0].Click(),
            () => cut.FindAll(".mode-option")[1].Click(),
            () => cut.Find(".single-cluster-selector select").Change("Prod")
        };

        foreach (var action in actions)
        {
            action.Should().NotThrow();
        }
    }
}
