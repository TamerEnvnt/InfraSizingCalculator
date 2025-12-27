using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.Configuration;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components.Configuration;

/// <summary>
/// Tests for AppCountsPanel component - Application counts input for K8s configuration
/// </summary>
public class AppCountsPanelTests : TestContext
{
    private static readonly Dictionary<EnvironmentType, AppConfig> DefaultEnvApps = new()
    {
        { EnvironmentType.Prod, new AppConfig { Small = 2, Medium = 2, Large = 1, XLarge = 0 } },
        { EnvironmentType.Dev, new AppConfig { Small = 3, Medium = 1, Large = 0, XLarge = 0 } }
    };

    private static readonly HashSet<EnvironmentType> DefaultEnabled = new()
    {
        EnvironmentType.Prod,
        EnvironmentType.Dev,
        EnvironmentType.Test
    };

    #region Rendering Tests

    [Fact]
    public void AppCountsPanel_RendersContainer()
    {
        // Act
        var cut = RenderComponent<AppCountsPanel>();

        // Assert
        cut.Find(".app-counts-panel").Should().NotBeNull();
    }

    [Fact]
    public void AppCountsPanel_RendersModeDescriptionBanner()
    {
        // Act
        var cut = RenderComponent<AppCountsPanel>();

        // Assert
        cut.Find(".mode-description-banner").Should().NotBeNull();
    }

    [Fact]
    public void AppCountsPanel_AppliesAdditionalClass()
    {
        // Act
        var cut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.AdditionalClass, "custom-class"));

        // Assert
        cut.Find(".app-counts-panel").ClassList.Should().Contain("custom-class");
    }

    #endregion

    #region Multi-Cluster Mode Tests

    [Fact]
    public void AppCountsPanel_MultiCluster_ShowsMultiClusterConfig()
    {
        // Act
        var cut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.MultiCluster));

        // Assert
        cut.Find(".multi-cluster-config").Should().NotBeNull();
    }

    [Fact]
    public void AppCountsPanel_MultiCluster_ShowsBanner()
    {
        // Act
        var cut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.MultiCluster));

        // Assert - Banner now shows text instead of emoji
        cut.Find(".mode-description-banner").ClassList.Should().Contain("multi");
        cut.Find(".banner-icon").TextContent.Should().Be("Multi");
    }

    [Fact]
    public void AppCountsPanel_MultiCluster_ShowsProductionGroup()
    {
        // Act
        var cut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.MultiCluster));

        // Assert
        cut.Find(".cluster-group.prod").Should().NotBeNull();
        cut.Find(".cluster-group.prod .group-label").TextContent.Should().Contain("Production");
    }

    [Fact]
    public void AppCountsPanel_MultiCluster_ShowsNonProductionGroup()
    {
        // Act
        var cut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.MultiCluster));

        // Assert
        cut.Find(".cluster-group.nonprod").Should().NotBeNull();
        cut.Find(".cluster-group.nonprod .group-label").TextContent.Should().Contain("Non-Production");
    }

    [Fact]
    public void AppCountsPanel_MultiCluster_ShowsTableHeader()
    {
        // Act
        var cut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.MultiCluster));

        // Assert
        var header = cut.Find(".apps-table-header");
        header.TextContent.Should().Contain("Small");
        header.TextContent.Should().Contain("Medium");
        header.TextContent.Should().Contain("Large");
        header.TextContent.Should().Contain("XLarge");
    }

    [Fact]
    public void AppCountsPanel_MultiCluster_ShowsClusterRows()
    {
        // Act
        var cut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.MultiCluster));

        // Assert - Should have 5 cluster rows (Prod, DR, Dev, Test, Stage)
        cut.FindAll(".cluster-row").Should().HaveCount(5);
    }

    [Fact]
    public void AppCountsPanel_MultiCluster_ProdRowNotDisabled()
    {
        // Act
        var cut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.MultiCluster)
            .Add(p => p.EnabledEnvironments, DefaultEnabled));

        // Assert - Prod checkbox is disabled (always enabled)
        var prodRow = cut.FindAll(".cluster-row").First();
        prodRow.QuerySelector("input[type='checkbox']")!.GetAttribute("disabled").Should().NotBeNull();
    }

    #endregion

    #region Shared Cluster Mode Tests

    [Fact]
    public void AppCountsPanel_SharedCluster_ShowsSharedConfig()
    {
        // Act
        var cut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.SharedCluster)
            .Add(p => p.SingleClusterScope, "Shared"));

        // Assert
        cut.Find(".shared-cluster-config").Should().NotBeNull();
    }

    [Fact]
    public void AppCountsPanel_SharedCluster_ShowsSharedBanner()
    {
        // Act
        var cut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.SharedCluster)
            .Add(p => p.SingleClusterScope, "Shared"));

        // Assert - Banner now shows text instead of emoji
        cut.Find(".mode-description-banner").ClassList.Should().Contain("shared");
        cut.Find(".banner-icon").TextContent.Should().Be("Shared");
    }

    [Fact]
    public void AppCountsPanel_SharedCluster_ShowsClusterHeader()
    {
        // Act
        var cut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.SharedCluster)
            .Add(p => p.SingleClusterScope, "Shared"));

        // Assert
        cut.Find(".shared-cluster-header").Should().NotBeNull();
        cut.Find(".cluster-title").TextContent.Should().Contain("Single Shared Cluster");
    }

    [Fact]
    public void AppCountsPanel_SharedCluster_ShowsNamespacesTable()
    {
        // Act
        var cut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.SharedCluster)
            .Add(p => p.SingleClusterScope, "Shared"));

        // Assert
        cut.Find(".namespaces-table").Should().NotBeNull();
    }

    [Fact]
    public void AppCountsPanel_SharedCluster_ShowsAllEnvironmentsAsNamespaces()
    {
        // Act
        var cut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.SharedCluster)
            .Add(p => p.SingleClusterScope, "Shared"));

        // Assert - 5 namespace rows for all environments
        cut.FindAll(".namespace-row").Should().HaveCount(5);
    }

    [Fact]
    public void AppCountsPanel_SharedCluster_UsesNamespacePrefix()
    {
        // Act
        var cut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.SharedCluster)
            .Add(p => p.SingleClusterScope, "Shared"));

        // Assert
        var rows = cut.FindAll(".namespace-row");
        rows.Should().Contain(r => r.TextContent.Contains("ns-dev"));
        rows.Should().Contain(r => r.TextContent.Contains("ns-prod"));
    }

    #endregion

    #region Single Environment Mode Tests

    [Fact]
    public void AppCountsPanel_SingleEnv_ShowsSingleEnvironment()
    {
        // Act
        var cut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.SharedCluster)
            .Add(p => p.SingleClusterScope, "Prod"));

        // Assert - Only one namespace row
        cut.FindAll(".namespace-row").Should().HaveCount(1);
    }

    [Fact]
    public void AppCountsPanel_SingleEnv_ShowsCorrectEnvironment()
    {
        // Act
        var cut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.SharedCluster)
            .Add(p => p.SingleClusterScope, "Dev"));

        // Assert
        cut.Find(".cluster-title").TextContent.Should().Contain("Dev Cluster");
    }

    [Fact]
    public void AppCountsPanel_SingleEnv_ShowsSingleBanner()
    {
        // Act
        var cut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.SharedCluster)
            .Add(p => p.SingleClusterScope, "Prod"));

        // Assert - Banner now shows text instead of emoji
        cut.Find(".mode-description-banner").ClassList.Should().Contain("single");
        cut.Find(".banner-icon").TextContent.Should().Be("Single");
    }

    #endregion

    #region Environment Toggle Tests

    [Fact]
    public void AppCountsPanel_EnabledEnvironments_NotDisabled()
    {
        // Act
        var cut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.MultiCluster)
            .Add(p => p.EnabledEnvironments, DefaultEnabled));

        // Assert
        var devRow = cut.FindAll(".cluster-row").First(r => r.TextContent.Contains("Dev"));
        devRow.ClassList.Should().NotContain("disabled");
    }

    [Fact]
    public void AppCountsPanel_DisabledEnvironments_HasDisabledClass()
    {
        // Act
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };
        var cut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.MultiCluster)
            .Add(p => p.EnabledEnvironments, enabledEnvs));

        // Assert
        var devRow = cut.FindAll(".cluster-row").First(r => r.TextContent.Contains("Dev"));
        devRow.ClassList.Should().Contain("disabled");
    }

    [Fact]
    public async Task AppCountsPanel_TogglingEnvironment_InvokesCallback()
    {
        // Arrange
        (EnvironmentType env, bool enabled)? toggled = null;
        var cut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.MultiCluster)
            .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod, EnvironmentType.Dev })
            .Add(p => p.OnEnvironmentToggled, EventCallback.Factory.Create<(EnvironmentType, bool)>(this, t => toggled = t)));

        // Act - Toggle Dev environment
        var devCheckbox = cut.FindAll(".cluster-row")
            .First(r => r.TextContent.Contains("Dev"))
            .QuerySelector("input[type='checkbox']");
        await cut.InvokeAsync(() => devCheckbox!.Change(false));

        // Assert
        toggled.Should().NotBeNull();
        toggled!.Value.env.Should().Be(EnvironmentType.Dev);
        toggled!.Value.enabled.Should().BeFalse();
    }

    #endregion

    #region App Count Input Tests

    [Fact]
    public void AppCountsPanel_ShowsAppCounts()
    {
        // Act
        var cut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.MultiCluster)
            .Add(p => p.EnvApps, DefaultEnvApps)
            .Add(p => p.EnabledEnvironments, DefaultEnabled));

        // Assert - Prod row should have Small=2, Medium=2, Large=1
        var prodRow = cut.FindAll(".cluster-row").First();
        var inputs = prodRow.QuerySelectorAll("input[type='number']");
        inputs[0].GetAttribute("value").Should().Be("2"); // Small
        inputs[1].GetAttribute("value").Should().Be("2"); // Medium
        inputs[2].GetAttribute("value").Should().Be("1"); // Large
    }

    [Fact]
    public void AppCountsPanel_DisabledEnv_HasDisabledInputs()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Prod };

        // Act
        var cut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.MultiCluster)
            .Add(p => p.EnabledEnvironments, enabledEnvs));

        // Assert - Dev inputs should be disabled
        var devRow = cut.FindAll(".cluster-row").First(r => r.TextContent.Contains("Dev"));
        var inputs = devRow.QuerySelectorAll("input[type='number']");
        inputs.Should().AllSatisfy(i => i.GetAttribute("disabled").Should().NotBeNull());
    }

    [Fact]
    public async Task AppCountsPanel_ChangingAppCount_InvokesCallback()
    {
        // Arrange
        (EnvironmentType env, AppTier tier, int count)? changed = null;
        var cut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.MultiCluster)
            .Add(p => p.EnabledEnvironments, DefaultEnabled)
            .Add(p => p.OnAppCountChanged, EventCallback.Factory.Create<(EnvironmentType, AppTier, int)>(this, c => changed = c)));

        // Act - Change Small app count for Prod
        var prodRow = cut.FindAll(".cluster-row").First();
        var smallInput = prodRow.QuerySelectorAll("input[type='number']")[0];
        await cut.InvokeAsync(() => smallInput.Change(5));

        // Assert
        changed.Should().NotBeNull();
        changed!.Value.env.Should().Be(EnvironmentType.Prod);
        changed!.Value.tier.Should().Be(AppTier.Small);
        changed!.Value.count.Should().Be(5);
    }

    #endregion

    #region Tier Columns Tests

    [Fact]
    public void AppCountsPanel_HasFourTierColumns()
    {
        // Act
        var cut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.MultiCluster));

        // Assert
        var header = cut.Find(".apps-table-header");
        header.QuerySelectorAll(".tier-col").Should().HaveCount(4);
    }

    [Fact]
    public void AppCountsPanel_EachRowHasFourInputs()
    {
        // Act
        var cut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.MultiCluster));

        // Assert
        var firstRow = cut.Find(".cluster-row");
        firstRow.QuerySelectorAll("input[type='number']").Should().HaveCount(4);
    }

    #endregion

    #region Parameter Update Tests

    [Fact]
    public void AppCountsPanel_UpdatesMode()
    {
        // Arrange
        var cut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.MultiCluster));

        // Assert initial
        cut.Find(".multi-cluster-config").Should().NotBeNull();

        // Act
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.SharedCluster)
            .Add(p => p.SingleClusterScope, "Shared"));

        // Assert
        cut.FindAll(".multi-cluster-config").Should().BeEmpty();
        cut.Find(".shared-cluster-config").Should().NotBeNull();
    }

    [Fact]
    public void AppCountsPanel_UpdatesSingleClusterScope()
    {
        // Arrange
        var cut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.SharedCluster)
            .Add(p => p.SingleClusterScope, "Shared"));

        // Assert initial
        cut.FindAll(".namespace-row").Should().HaveCount(5);

        // Act
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.SingleClusterScope, "Dev"));

        // Assert
        cut.FindAll(".namespace-row").Should().HaveCount(1);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void AppCountsPanel_EmptyEnvApps_ShowsZeros()
    {
        // Act
        var cut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.MultiCluster)
            .Add(p => p.EnvApps, new Dictionary<EnvironmentType, AppConfig>())
            .Add(p => p.EnabledEnvironments, DefaultEnabled));

        // Assert - All inputs should show 0
        var firstRow = cut.Find(".cluster-row");
        var inputs = firstRow.QuerySelectorAll("input[type='number']");
        inputs.Should().AllSatisfy(i => i.GetAttribute("value").Should().Be("0"));
    }

    [Fact]
    public async Task AppCountsPanel_NoCallbacks_DoesNotThrow()
    {
        // Arrange
        var cut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.MultiCluster)
            .Add(p => p.EnabledEnvironments, DefaultEnabled));

        // Act & Assert
        var action = async () =>
        {
            var devRow = cut.FindAll(".cluster-row").First(r => r.TextContent.Contains("Dev"));
            var checkbox = devRow.QuerySelector("input[type='checkbox']");
            await cut.InvokeAsync(() => checkbox!.Change(false));
        };

        await action.Should().NotThrowAsync();
    }

    #endregion
}
