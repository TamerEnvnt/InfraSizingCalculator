using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.Configuration;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components;

/// <summary>
/// Tests for AppCountsPanel component
/// </summary>
public class AppCountsPanelTests : TestContext
{
    private Dictionary<EnvironmentType, AppConfig> CreateDefaultEnvApps()
    {
        return new Dictionary<EnvironmentType, AppConfig>
        {
            { EnvironmentType.Dev, new AppConfig { Small = 5, Medium = 3, Large = 1, XLarge = 0 } },
            { EnvironmentType.Test, new AppConfig { Small = 4, Medium = 2, Large = 1, XLarge = 0 } },
            { EnvironmentType.Stage, new AppConfig { Small = 3, Medium = 2, Large = 1, XLarge = 1 } },
            { EnvironmentType.Prod, new AppConfig { Small = 10, Medium = 5, Large = 3, XLarge = 2 } },
            { EnvironmentType.DR, new AppConfig { Small = 10, Medium = 5, Large = 3, XLarge = 2 } }
        };
    }

    private HashSet<EnvironmentType> CreateDefaultEnabledEnvironments()
    {
        return new HashSet<EnvironmentType>
        {
            EnvironmentType.Dev,
            EnvironmentType.Test,
            EnvironmentType.Stage,
            EnvironmentType.Prod
        };
    }

    #region Multi-Cluster Mode Tests

    [Fact]
    public void AppCountsPanel_MultiCluster_RendersModeBanner()
    {
        // Act
        var cut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.MultiCluster)
            .Add(p => p.EnvApps, CreateDefaultEnvApps())
            .Add(p => p.EnabledEnvironments, CreateDefaultEnabledEnvironments()));

        // Assert
        var banner = cut.Find(".mode-description-banner");
        banner.Should().NotBeNull();
        banner.ClassList.Should().Contain("multi");
        banner.QuerySelector("strong")?.TextContent.Should().Be("Multi-Cluster Mode");
    }

    [Fact]
    public void AppCountsPanel_MultiCluster_DisplaysProductionClusters()
    {
        // Act
        var cut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.MultiCluster)
            .Add(p => p.EnvApps, CreateDefaultEnvApps())
            .Add(p => p.EnabledEnvironments, CreateDefaultEnabledEnvironments()));

        // Assert
        var prodGroup = cut.Find(".cluster-group.prod");
        prodGroup.Should().NotBeNull();
        prodGroup.QuerySelector(".group-label")?.TextContent.Should().Contain("Production Clusters");

        // Should have Prod and DR
        var prodRows = prodGroup.QuerySelectorAll(".cluster-row");
        prodRows.Should().HaveCount(2);
    }

    [Fact]
    public void AppCountsPanel_MultiCluster_DisplaysNonProductionClusters()
    {
        // Act
        var cut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.MultiCluster)
            .Add(p => p.EnvApps, CreateDefaultEnvApps())
            .Add(p => p.EnabledEnvironments, CreateDefaultEnabledEnvironments()));

        // Assert
        var nonprodGroup = cut.Find(".cluster-group.nonprod");
        nonprodGroup.Should().NotBeNull();
        nonprodGroup.QuerySelector(".group-label")?.TextContent.Should().Contain("Non-Production Clusters");

        // Should have Dev, Test, Stage
        var nonprodRows = nonprodGroup.QuerySelectorAll(".cluster-row");
        nonprodRows.Should().HaveCount(3);
    }

    [Fact]
    public void AppCountsPanel_MultiCluster_ShowsTableHeaders()
    {
        // Act
        var cut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.MultiCluster)
            .Add(p => p.EnvApps, CreateDefaultEnvApps())
            .Add(p => p.EnabledEnvironments, CreateDefaultEnabledEnvironments()));

        // Assert
        var headers = cut.FindAll(".apps-table-header .tier-col");
        headers.Should().HaveCount(4);
        headers[0].TextContent.Should().Be("Small");
        headers[1].TextContent.Should().Be("Medium");
        headers[2].TextContent.Should().Be("Large");
        headers[3].TextContent.Should().Be("XLarge");
    }

    [Fact]
    public void AppCountsPanel_MultiCluster_ProdCheckboxIsDisabled()
    {
        // Act
        var cut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.MultiCluster)
            .Add(p => p.EnvApps, CreateDefaultEnvApps())
            .Add(p => p.EnabledEnvironments, CreateDefaultEnabledEnvironments()));

        // Assert
        var prodCheckbox = cut.Find(".cluster-group.prod .cluster-row input[type='checkbox']");
        prodCheckbox.HasAttribute("disabled").Should().BeTrue();
        prodCheckbox.HasAttribute("checked").Should().BeTrue();
    }

    [Fact]
    public void AppCountsPanel_MultiCluster_NonProdCheckboxesAreEnabled()
    {
        // Act
        var cut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.MultiCluster)
            .Add(p => p.EnvApps, CreateDefaultEnvApps())
            .Add(p => p.EnabledEnvironments, CreateDefaultEnabledEnvironments()));

        // Assert
        var nonprodCheckboxes = cut.FindAll(".cluster-group.nonprod .cluster-row input[type='checkbox']");
        nonprodCheckboxes.Should().HaveCount(3); // Dev, Test, Stage

        foreach (var checkbox in nonprodCheckboxes)
        {
            checkbox.HasAttribute("disabled").Should().BeFalse();
        }
    }

    [Fact]
    public void AppCountsPanel_MultiCluster_DisplaysCorrectAppCounts()
    {
        // Act
        var cut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.MultiCluster)
            .Add(p => p.EnvApps, CreateDefaultEnvApps())
            .Add(p => p.EnabledEnvironments, CreateDefaultEnabledEnvironments()));

        // Assert - Check Prod cluster values
        var prodRow = cut.Find(".cluster-group.prod .cluster-row");
        var prodInputs = prodRow.QuerySelectorAll("input[type='number']");

        prodInputs[0].GetAttribute("value").Should().Be("10"); // Small
        prodInputs[1].GetAttribute("value").Should().Be("5");  // Medium
        prodInputs[2].GetAttribute("value").Should().Be("3");  // Large
        prodInputs[3].GetAttribute("value").Should().Be("2");  // XLarge
    }

    [Fact]
    public void AppCountsPanel_MultiCluster_DisabledEnvironmentHasDisabledInputs()
    {
        // Arrange - DR is not enabled
        var enabledEnvs = new HashSet<EnvironmentType>
        {
            EnvironmentType.Dev,
            EnvironmentType.Test,
            EnvironmentType.Stage,
            EnvironmentType.Prod
        };

        // Act
        var cut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.MultiCluster)
            .Add(p => p.EnvApps, CreateDefaultEnvApps())
            .Add(p => p.EnabledEnvironments, enabledEnvs));

        // Assert
        var rows = cut.FindAll(".cluster-group.prod .cluster-row");
        var drRow = rows[1]; // DR is second in prod group

        drRow.ClassList.Should().Contain("disabled");
        var drInputs = drRow.QuerySelectorAll("input[type='number']");
        foreach (var input in drInputs)
        {
            input.HasAttribute("disabled").Should().BeTrue();
        }
    }

    [Fact]
    public async Task AppCountsPanel_MultiCluster_InvokesOnEnvironmentToggled()
    {
        // Arrange
        (EnvironmentType env, bool enabled)? toggledValue = null;
        var cut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.MultiCluster)
            .Add(p => p.EnvApps, CreateDefaultEnvApps())
            .Add(p => p.EnabledEnvironments, CreateDefaultEnabledEnvironments())
            .Add(p => p.OnEnvironmentToggled, value => toggledValue = value));

        // Act - Toggle Dev environment
        await cut.InvokeAsync(() =>
        {
            var devCheckbox = cut.FindAll(".cluster-group.nonprod .cluster-row input[type='checkbox']")[0];
            devCheckbox.Change(false);
        });

        // Assert
        toggledValue.Should().NotBeNull();
        toggledValue.Value.env.Should().Be(EnvironmentType.Dev);
        toggledValue.Value.enabled.Should().BeFalse();
    }

    [Fact]
    public async Task AppCountsPanel_MultiCluster_InvokesOnAppCountChanged()
    {
        // Arrange
        (EnvironmentType env, AppTier tier, int count)? changedValue = null;
        var cut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.MultiCluster)
            .Add(p => p.EnvApps, CreateDefaultEnvApps())
            .Add(p => p.EnabledEnvironments, CreateDefaultEnabledEnvironments())
            .Add(p => p.OnAppCountChanged, value => changedValue = value));

        // Act - Change Prod Small apps to 15
        await cut.InvokeAsync(() =>
        {
            var prodRow = cut.Find(".cluster-group.prod .cluster-row");
            var smallInput = prodRow.QuerySelectorAll("input[type='number']")[0];
            smallInput.Change("15");
        });

        // Assert
        changedValue.Should().NotBeNull();
        changedValue.Value.env.Should().Be(EnvironmentType.Prod);
        changedValue.Value.tier.Should().Be(AppTier.Small);
        changedValue.Value.count.Should().Be(15);
    }

    #endregion

    #region Single Cluster / Shared Mode Tests

    [Fact]
    public void AppCountsPanel_SharedCluster_RendersSharedBanner()
    {
        // Act
        var cut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.SharedCluster)
            .Add(p => p.SingleClusterScope, "Shared")
            .Add(p => p.EnvApps, CreateDefaultEnvApps())
            .Add(p => p.EnabledEnvironments, CreateDefaultEnabledEnvironments()));

        // Assert
        var banner = cut.Find(".mode-description-banner");
        banner.ClassList.Should().Contain("shared");
        banner.QuerySelector("strong")?.TextContent.Should().Be("Shared Cluster");
    }

    [Fact]
    public void AppCountsPanel_SharedCluster_ShowsAllEnvironments()
    {
        // Act
        var cut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.SharedCluster)
            .Add(p => p.SingleClusterScope, "Shared")
            .Add(p => p.EnvApps, CreateDefaultEnvApps())
            .Add(p => p.EnabledEnvironments, CreateDefaultEnabledEnvironments()));

        // Assert
        var rows = cut.FindAll(".namespace-row");
        rows.Should().HaveCount(5); // Dev, Test, Stage, Prod, DR
    }

    [Fact]
    public void AppCountsPanel_SharedCluster_ShowsNamespaceNames()
    {
        // Act
        var cut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.SharedCluster)
            .Add(p => p.SingleClusterScope, "Shared")
            .Add(p => p.EnvApps, CreateDefaultEnvApps())
            .Add(p => p.EnabledEnvironments, CreateDefaultEnabledEnvironments()));

        // Assert
        var namespaceNames = cut.FindAll(".namespace-name");
        namespaceNames[0].TextContent.Should().Be("ns-dev");
        namespaceNames[1].TextContent.Should().Be("ns-test");
        namespaceNames[2].TextContent.Should().Be("ns-stage");
        namespaceNames[3].TextContent.Should().Be("ns-prod");
        namespaceNames[4].TextContent.Should().Be("ns-dr");
    }

    [Fact]
    public void AppCountsPanel_SharedCluster_ShowsSharedClusterHeader()
    {
        // Act
        var cut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.SharedCluster)
            .Add(p => p.SingleClusterScope, "Shared")
            .Add(p => p.EnvApps, CreateDefaultEnvApps())
            .Add(p => p.EnabledEnvironments, CreateDefaultEnabledEnvironments()));

        // Assert
        var header = cut.Find(".shared-cluster-header");
        header.QuerySelector(".cluster-title")?.TextContent.Should().Be("Single Shared Cluster");
        header.QuerySelector(".cluster-subtitle")?.TextContent.Should().Be("All environments as namespaces");
    }

    [Fact]
    public void AppCountsPanel_SingleEnvCluster_ShowsEnvironmentName()
    {
        // Act
        var cut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.SharedCluster)
            .Add(p => p.SingleClusterScope, "Prod")
            .Add(p => p.EnvApps, CreateDefaultEnvApps())
            .Add(p => p.EnabledEnvironments, CreateDefaultEnabledEnvironments()));

        // Assert
        var banner = cut.Find(".mode-description-banner");
        banner.ClassList.Should().Contain("single");
        banner.QuerySelector("strong")?.TextContent.Should().Be("Prod Cluster Only");

        var header = cut.Find(".shared-cluster-header");
        header.QuerySelector(".cluster-title")?.TextContent.Should().Be("Prod Cluster");
        cut.FindAll(".cluster-subtitle").Should().BeEmpty();
    }

    [Fact]
    public void AppCountsPanel_SingleEnvCluster_ShowsOnlyOneEnvironment()
    {
        // Act
        var cut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.SharedCluster)
            .Add(p => p.SingleClusterScope, "Dev")
            .Add(p => p.EnvApps, CreateDefaultEnvApps())
            .Add(p => p.EnabledEnvironments, CreateDefaultEnabledEnvironments()));

        // Assert
        var rows = cut.FindAll(".namespace-row");
        rows.Should().HaveCount(1);

        var envName = cut.Find(".namespace-name");
        envName.TextContent.Should().Be("Dev"); // Not namespace format for single env
    }

    [Fact]
    public void AppCountsPanel_SingleEnvCluster_AllScopesWork()
    {
        // Test all scopes
        var scopes = new[] { "Dev", "Test", "Stage", "Prod", "DR" };

        foreach (var scope in scopes)
        {
            // Act
            var cut = RenderComponent<AppCountsPanel>(parameters => parameters
                .Add(p => p.ClusterMode, ClusterMode.SharedCluster)
                .Add(p => p.SingleClusterScope, scope)
                .Add(p => p.EnvApps, CreateDefaultEnvApps())
                .Add(p => p.EnabledEnvironments, CreateDefaultEnabledEnvironments()));

            // Assert
            var rows = cut.FindAll(".namespace-row");
            rows.Should().HaveCount(1, $"scope {scope} should show one environment");

            var header = cut.Find(".shared-cluster-header .cluster-title");
            header.TextContent.Should().Be($"{scope} Cluster", $"scope {scope} should show correct title");
        }
    }

    [Fact]
    public void AppCountsPanel_SharedCluster_ProdNamespaceHasCorrectStyling()
    {
        // Act
        var cut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.SharedCluster)
            .Add(p => p.SingleClusterScope, "Shared")
            .Add(p => p.EnvApps, CreateDefaultEnvApps())
            .Add(p => p.EnabledEnvironments, CreateDefaultEnabledEnvironments()));

        // Assert
        var namespaceRows = cut.FindAll(".namespace-row");
        namespaceRows[3].ClassList.Should().Contain("prod-ns"); // Prod
        namespaceRows[4].ClassList.Should().Contain("prod-ns"); // DR
        namespaceRows[0].ClassList.Should().Contain("nonprod-ns"); // Dev
    }

    #endregion

    #region Common Tests

    [Fact]
    public void AppCountsPanel_AppliesAdditionalClass()
    {
        // Act
        var cut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.MultiCluster)
            .Add(p => p.AdditionalClass, "custom-panel")
            .Add(p => p.EnvApps, CreateDefaultEnvApps())
            .Add(p => p.EnabledEnvironments, CreateDefaultEnabledEnvironments()));

        // Assert
        cut.Find(".app-counts-panel").ClassList.Should().Contain("custom-panel");
    }

    [Fact]
    public void AppCountsPanel_HandlesEmptyEnvApps()
    {
        // Act
        var cut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.MultiCluster)
            .Add(p => p.EnvApps, new Dictionary<EnvironmentType, AppConfig>())
            .Add(p => p.EnabledEnvironments, CreateDefaultEnabledEnvironments()));

        // Assert - Should render without error
        cut.Find(".app-counts-panel").Should().NotBeNull();

        // All inputs should show 0
        var inputs = cut.FindAll("input[type='number']");
        foreach (var input in inputs)
        {
            input.GetAttribute("value").Should().Be("0");
        }
    }

    [Fact]
    public void AppCountsPanel_HandlesEmptyEnabledEnvironments()
    {
        // Act
        var cut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.MultiCluster)
            .Add(p => p.EnvApps, CreateDefaultEnvApps())
            .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType>()));

        // Assert - All rows should be disabled
        var rows = cut.FindAll(".cluster-row");
        foreach (var row in rows)
        {
            row.ClassList.Should().Contain("disabled");
        }
    }

    [Fact]
    public void AppCountsPanel_AllInputsHaveMinAttribute()
    {
        // Act
        var cut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.MultiCluster)
            .Add(p => p.EnvApps, CreateDefaultEnvApps())
            .Add(p => p.EnabledEnvironments, CreateDefaultEnabledEnvironments()));

        // Assert
        var inputs = cut.FindAll("input[type='number']");
        foreach (var input in inputs)
        {
            input.GetAttribute("min").Should().Be("0");
        }
    }

    [Fact]
    public async Task AppCountsPanel_ParsesInvalidInputAsZero()
    {
        // Arrange
        (EnvironmentType env, AppTier tier, int count)? changedValue = null;
        var cut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.MultiCluster)
            .Add(p => p.EnvApps, CreateDefaultEnvApps())
            .Add(p => p.EnabledEnvironments, CreateDefaultEnabledEnvironments())
            .Add(p => p.OnAppCountChanged, value => changedValue = value));

        // Act - Input invalid value
        await cut.InvokeAsync(() =>
        {
            var prodRow = cut.Find(".cluster-group.prod .cluster-row");
            var input = prodRow.QuerySelectorAll("input[type='number']")[0];
            input.Change("invalid");
        });

        // Assert
        changedValue.Should().NotBeNull();
        changedValue.Value.count.Should().Be(0);
    }

    [Fact]
    public async Task AppCountsPanel_HandlesNegativeInput()
    {
        // Arrange
        (EnvironmentType env, AppTier tier, int count)? changedValue = null;
        var cut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.MultiCluster)
            .Add(p => p.EnvApps, CreateDefaultEnvApps())
            .Add(p => p.EnabledEnvironments, CreateDefaultEnabledEnvironments())
            .Add(p => p.OnAppCountChanged, value => changedValue = value));

        // Act - Input negative value
        await cut.InvokeAsync(() =>
        {
            var prodRow = cut.Find(".cluster-group.prod .cluster-row");
            var input = prodRow.QuerySelectorAll("input[type='number']")[0];
            input.Change("-5");
        });

        // Assert - Component accepts it (HTML will enforce min=0)
        changedValue.Should().NotBeNull();
        changedValue.Value.count.Should().Be(-5);
    }

    [Fact]
    public void AppCountsPanel_ShowsCorrectBannerIcons()
    {
        // Multi-cluster
        var multiCut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.MultiCluster)
            .Add(p => p.EnvApps, CreateDefaultEnvApps())
            .Add(p => p.EnabledEnvironments, CreateDefaultEnabledEnvironments()));

        multiCut.Find(".banner-icon").TextContent.Should().Be("🌐");

        // Shared cluster
        var sharedCut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.SharedCluster)
            .Add(p => p.SingleClusterScope, "Shared")
            .Add(p => p.EnvApps, CreateDefaultEnvApps())
            .Add(p => p.EnabledEnvironments, CreateDefaultEnabledEnvironments()));

        sharedCut.Find(".banner-icon").TextContent.Should().Be("🔗");

        // Single environment cluster
        var singleCut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.SharedCluster)
            .Add(p => p.SingleClusterScope, "Prod")
            .Add(p => p.EnvApps, CreateDefaultEnvApps())
            .Add(p => p.EnabledEnvironments, CreateDefaultEnabledEnvironments()));

        singleCut.Find(".banner-icon").TextContent.Should().Be("🎯");
    }

    [Fact]
    public async Task AppCountsPanel_NoCallbacksDoNotError()
    {
        // Arrange & Act - No callbacks provided
        var cut = RenderComponent<AppCountsPanel>(parameters => parameters
            .Add(p => p.ClusterMode, ClusterMode.MultiCluster)
            .Add(p => p.EnvApps, CreateDefaultEnvApps())
            .Add(p => p.EnabledEnvironments, CreateDefaultEnabledEnvironments()));

        // Assert - Actions should not throw
        await cut.InvokeAsync(() =>
        {
            var devCheckbox = cut.FindAll(".cluster-group.nonprod .cluster-row input[type='checkbox']")[0];
            var action1 = () => devCheckbox.Change(false);
            action1.Should().NotThrow();

            var input = cut.Find("input[type='number']");
            var action2 = () => input.Change("10");
            action2.Should().NotThrow();
        });
    }

    #endregion
}
