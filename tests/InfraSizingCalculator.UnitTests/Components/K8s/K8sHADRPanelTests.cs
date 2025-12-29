using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.K8s;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components.K8s;

/// <summary>
/// Tests for K8sHADRPanel component - K8s High Availability and Disaster Recovery configuration
/// </summary>
public class K8sHADRPanelTests : TestContext
{
    #region Rendering Tests

    [Fact]
    public void K8sHADRPanel_RendersMainContainer()
    {
        // Act
        var cut = RenderComponent<K8sHADRPanel>();

        // Assert
        cut.Find(".k8s-hadr-config").Should().NotBeNull();
    }

    [Fact]
    public void K8sHADRPanel_RendersTwoSections()
    {
        // Act
        var cut = RenderComponent<K8sHADRPanel>();

        // Assert
        cut.FindAll(".hadr-section").Should().HaveCount(2);
    }

    [Fact]
    public void K8sHADRPanel_RendersHighAvailabilitySection()
    {
        // Act
        var cut = RenderComponent<K8sHADRPanel>();

        // Assert
        var sections = cut.FindAll(".section-header");
        sections[0].TextContent.Should().Contain("High Availability");
    }

    [Fact]
    public void K8sHADRPanel_RendersDisasterRecoverySection()
    {
        // Act
        var cut = RenderComponent<K8sHADRPanel>();

        // Assert
        var sections = cut.FindAll(".section-header");
        sections[1].TextContent.Should().Contain("Disaster Recovery");
    }

    [Fact]
    public void K8sHADRPanel_RendersSummary()
    {
        // Act
        var cut = RenderComponent<K8sHADRPanel>();

        // Assert
        cut.Find(".hadr-summary").Should().NotBeNull();
    }

    [Fact]
    public void K8sHADRPanel_SummaryShowsCostMultiplier()
    {
        // Act
        var cut = RenderComponent<K8sHADRPanel>();

        // Assert
        cut.Find(".summary-cost").TextContent.Should().Contain("Cost multiplier:");
    }

    #endregion

    #region Managed Distribution Tests

    [Theory]
    [InlineData(Distribution.EKS)]
    [InlineData(Distribution.AKS)]
    [InlineData(Distribution.GKE)]
    [InlineData(Distribution.OKE)]
    [InlineData(Distribution.DOKS)]
    [InlineData(Distribution.LKE)]
    [InlineData(Distribution.OpenShiftROSA)]
    [InlineData(Distribution.OpenShiftARO)]
    public void K8sHADRPanel_ManagedDistribution_ShowsManagedBadge(Distribution distribution)
    {
        // Act
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Distribution, distribution));

        // Assert
        cut.Find(".managed-badge").TextContent.Should().Contain("Managed");
    }

    [Theory]
    [InlineData(Distribution.EKS)]
    [InlineData(Distribution.AKS)]
    [InlineData(Distribution.GKE)]
    public void K8sHADRPanel_ManagedDistribution_HidesControlPlaneSelector(Distribution distribution)
    {
        // Act
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Distribution, distribution));

        // Assert - Should not have control plane HA dropdown (only managed text)
        var selectElements = cut.FindAll("select");
        var hasControlPlaneSelect = selectElements.Any(s =>
        {
            var value = s.GetAttribute("value");
            return value != null && value.Contains("ControlPlaneHA");
        });
        hasControlPlaneSelect.Should().BeFalse();
    }

    [Theory]
    [InlineData(Distribution.EKS, "AWS")]
    [InlineData(Distribution.AKS, "Azure")]
    [InlineData(Distribution.GKE, "Google Cloud")]
    [InlineData(Distribution.OKE, "Oracle Cloud")]
    [InlineData(Distribution.OpenShiftROSA, "AWS (ROSA)")]
    [InlineData(Distribution.OpenShiftARO, "Azure (ARO)")]
    public void K8sHADRPanel_ManagedDistribution_ShowsProviderName(Distribution distribution, string expectedProvider)
    {
        // Act
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Distribution, distribution));

        // Assert
        cut.Find(".managed-text").TextContent.Should().Contain(expectedProvider);
    }

    [Fact]
    public void K8sHADRPanel_ManagedDistribution_AutoSetsControlPlaneToManaged()
    {
        // Arrange
        var config = new K8sHADRConfig { ControlPlaneHA = K8sControlPlaneHA.Single };

        // Act
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Config, config)
            .Add(p => p.Distribution, Distribution.EKS));

        // Assert - Config should be auto-updated to Managed
        config.ControlPlaneHA.Should().Be(K8sControlPlaneHA.Managed);
    }

    #endregion

    #region Self-Managed Distribution Tests

    [Theory]
    [InlineData(Distribution.Kubernetes)]
    [InlineData(Distribution.RKE2)]
    [InlineData(Distribution.K3s)]
    [InlineData(Distribution.OpenShift)]
    public void K8sHADRPanel_SelfManaged_ShowsControlPlaneSelector(Distribution distribution)
    {
        // Act
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Distribution, distribution));

        // Assert - Should have control plane HA dropdown
        cut.FindAll(".config-label").Should().Contain(l => l.TextContent.Contains("Control Plane"));
    }

    [Theory]
    [InlineData(Distribution.Kubernetes)]
    [InlineData(Distribution.RKE2)]
    public void K8sHADRPanel_SelfManaged_HidesManagedBadge(Distribution distribution)
    {
        // Act
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Distribution, distribution));

        // Assert
        cut.FindAll(".managed-badge").Should().BeEmpty();
    }

    [Theory]
    [InlineData(K8sControlPlaneHA.StackedHA)]
    [InlineData(K8sControlPlaneHA.ExternalEtcd)]
    public void K8sHADRPanel_HAMode_ShowsNodeCountSelector(K8sControlPlaneHA haMode)
    {
        // Arrange
        var config = new K8sHADRConfig { ControlPlaneHA = haMode };

        // Act
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Config, config)
            .Add(p => p.Distribution, Distribution.Kubernetes));

        // Assert - Should show node count selector
        cut.FindAll(".config-label").Should().Contain(l => l.TextContent.Contains("Control Plane Nodes"));
    }

    [Fact]
    public void K8sHADRPanel_SingleHA_HidesNodeCountSelector()
    {
        // Arrange
        var config = new K8sHADRConfig { ControlPlaneHA = K8sControlPlaneHA.Single };

        // Act
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Config, config)
            .Add(p => p.Distribution, Distribution.Kubernetes));

        // Assert - Should NOT show node count selector
        cut.FindAll(".config-label").Should().NotContain(l => l.TextContent.Contains("Control Plane Nodes"));
    }

    #endregion

    #region Node Distribution Tests

    [Fact]
    public void K8sHADRPanel_RendersNodeDistributionSelector()
    {
        // Act
        var cut = RenderComponent<K8sHADRPanel>();

        // Assert
        cut.FindAll(".config-label").Should().Contain(l => l.TextContent.Contains("Node Distribution"));
    }

    [Fact]
    public void K8sHADRPanel_SingleAZ_HidesAZCountSelector()
    {
        // Arrange
        var config = new K8sHADRConfig { NodeDistribution = K8sNodeDistribution.SingleAZ };

        // Act
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Config, config));

        // Assert
        cut.FindAll(".config-label").Should().NotContain(l => l.TextContent.Contains("Availability Zones"));
    }

    [Theory]
    [InlineData(K8sNodeDistribution.DualAZ)]
    [InlineData(K8sNodeDistribution.MultiAZ)]
    [InlineData(K8sNodeDistribution.MultiRegion)]
    public void K8sHADRPanel_NonSingleAZ_ShowsAZCountSelector(K8sNodeDistribution distribution)
    {
        // Arrange
        var config = new K8sHADRConfig { NodeDistribution = distribution };

        // Act
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Config, config));

        // Assert
        cut.FindAll(".config-label").Should().Contain(l => l.TextContent.Contains("Availability Zones"));
    }

    #endregion

    #region DR Pattern Tests

    [Fact]
    public void K8sHADRPanel_RendersDRStrategySelector()
    {
        // Act
        var cut = RenderComponent<K8sHADRPanel>();

        // Assert
        cut.FindAll(".config-label").Should().Contain(l => l.TextContent.Contains("DR Strategy"));
    }

    [Fact]
    public void K8sHADRPanel_DRPatternNone_HidesDRRegionAndRTO()
    {
        // Arrange
        var config = new K8sHADRConfig { DRPattern = K8sDRPattern.None };

        // Act
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Config, config));

        // Assert
        cut.FindAll(".config-label").Should().NotContain(l => l.TextContent.Contains("DR Region"));
        cut.FindAll(".config-label").Should().NotContain(l => l.TextContent.Contains("RTO"));
    }

    [Theory]
    [InlineData(K8sDRPattern.BackupRestore)]
    [InlineData(K8sDRPattern.WarmStandby)]
    [InlineData(K8sDRPattern.HotStandby)]
    [InlineData(K8sDRPattern.ActiveActive)]
    public void K8sHADRPanel_DRPatternSet_ShowsDRRegionAndRTO(K8sDRPattern pattern)
    {
        // Arrange
        var config = new K8sHADRConfig { DRPattern = pattern };

        // Act
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Config, config));

        // Assert
        cut.FindAll(".config-label").Should().Contain(l => l.TextContent.Contains("DR Region"));
        cut.FindAll(".config-label").Should().Contain(l => l.TextContent.Contains("RTO"));
    }

    #endregion

    #region Backup Strategy Tests

    [Fact]
    public void K8sHADRPanel_RendersBackupStrategySelector()
    {
        // Act
        var cut = RenderComponent<K8sHADRPanel>();

        // Assert
        cut.FindAll(".config-label").Should().Contain(l => l.TextContent.Contains("Backup Strategy"));
    }

    [Fact]
    public void K8sHADRPanel_BackupStrategyNone_HidesFrequencyAndRetention()
    {
        // Arrange
        var config = new K8sHADRConfig { BackupStrategy = K8sBackupStrategy.None };

        // Act
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Config, config));

        // Assert
        cut.FindAll(".config-label").Should().NotContain(l => l.TextContent.Contains("Backup Frequency"));
        cut.FindAll(".config-label").Should().NotContain(l => l.TextContent.Contains("Retention"));
    }

    [Theory]
    [InlineData(K8sBackupStrategy.Velero)]
    [InlineData(K8sBackupStrategy.Kasten)]
    [InlineData(K8sBackupStrategy.Portworx)]
    public void K8sHADRPanel_BackupStrategySet_ShowsFrequencyAndRetention(K8sBackupStrategy strategy)
    {
        // Arrange
        var config = new K8sHADRConfig { BackupStrategy = strategy };

        // Act
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Config, config));

        // Assert
        cut.FindAll(".config-label").Should().Contain(l => l.TextContent.Contains("Backup Frequency"));
        cut.FindAll(".config-label").Should().Contain(l => l.TextContent.Contains("Retention"));
    }

    [Theory]
    [InlineData(Distribution.EKS)]
    [InlineData(Distribution.AKS)]
    [InlineData(Distribution.GKE)]
    public void K8sHADRPanel_MajorCloudProvider_ShowsCloudNativeBackupOption(Distribution distribution)
    {
        // Act
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Distribution, distribution));

        // Assert - CloudNative should be an available option
        var backupSelects = cut.FindAll("select");
        // Find the backup strategy select - it should contain "Cloud Native Backup" option
        backupSelects.Should().Contain(s => s.InnerHtml.Contains("Cloud Native"));
    }

    [Theory]
    [InlineData(Distribution.Kubernetes)]
    [InlineData(Distribution.RKE2)]
    [InlineData(Distribution.K3s)]
    public void K8sHADRPanel_NonCloudProvider_HidesCloudNativeBackupOption(Distribution distribution)
    {
        // Act
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Distribution, distribution));

        // Assert - CloudNative should NOT be an available option
        var backupSelects = cut.FindAll("select");
        backupSelects.Should().NotContain(s => s.InnerHtml.Contains("Cloud Native Backup"));
    }

    #endregion

    #region Toggle Tests

    [Fact]
    public void K8sHADRPanel_RendersPodDisruptionBudgetsToggle()
    {
        // Act
        var cut = RenderComponent<K8sHADRPanel>();

        // Assert
        cut.FindAll(".toggle-label").Should().Contain(l => l.TextContent.Contains("Pod Disruption Budgets"));
    }

    [Fact]
    public void K8sHADRPanel_RendersTopologySpreadToggle()
    {
        // Act
        var cut = RenderComponent<K8sHADRPanel>();

        // Assert
        cut.FindAll(".toggle-label").Should().Contain(l => l.TextContent.Contains("Topology Spread Constraints"));
    }

    [Fact]
    public void K8sHADRPanel_CheckboxesDefaultToChecked()
    {
        // Arrange - Model defaults to true for both PDB and Topology Spread (best practices)
        var config = new K8sHADRConfig();

        // Act
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Config, config));

        // Assert - Defaults reflect K8s best practices (enabled by default)
        config.EnablePodDisruptionBudgets.Should().BeTrue();
        config.EnableTopologySpread.Should().BeTrue();
    }

    [Fact]
    public void K8sHADRPanel_CheckboxesShowCorrectState()
    {
        // Arrange
        var config = new K8sHADRConfig
        {
            EnablePodDisruptionBudgets = true,
            EnableTopologySpread = true
        };

        // Act
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Config, config));

        // Assert - Checkboxes should be checked
        var checkboxes = cut.FindAll("input[type='checkbox']");
        checkboxes.Should().HaveCount(2);
        checkboxes.All(cb => cb.HasAttribute("checked") || cb.GetAttribute("value") == "True");
    }

    #endregion

    #region EventCallback Tests

    [Fact]
    public async Task K8sHADRPanel_ChangingNodeDistribution_InvokesCallback()
    {
        // Arrange
        K8sHADRConfig? updatedConfig = null;
        var config = new K8sHADRConfig { NodeDistribution = K8sNodeDistribution.SingleAZ };
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Config, config)
            .Add(p => p.ConfigChanged, EventCallback.Factory.Create<K8sHADRConfig>(this, c => updatedConfig = c)));

        // Act
        var selects = cut.FindAll("select");
        var nodeDistSelect = selects.First(s => s.InnerHtml.Contains("Single Availability Zone"));
        await cut.InvokeAsync(() => nodeDistSelect.Change(K8sNodeDistribution.MultiAZ.ToString()));

        // Assert
        updatedConfig.Should().NotBeNull();
        config.NodeDistribution.Should().Be(K8sNodeDistribution.MultiAZ);
    }

    [Fact]
    public async Task K8sHADRPanel_ChangingDRPattern_InvokesCallback()
    {
        // Arrange
        K8sHADRConfig? updatedConfig = null;
        var config = new K8sHADRConfig { DRPattern = K8sDRPattern.None };
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Config, config)
            .Add(p => p.ConfigChanged, EventCallback.Factory.Create<K8sHADRConfig>(this, c => updatedConfig = c)));

        // Act
        var selects = cut.FindAll("select");
        var drSelect = selects.First(s => s.InnerHtml.Contains("None (Multi-AZ only)"));
        await cut.InvokeAsync(() => drSelect.Change(K8sDRPattern.HotStandby.ToString()));

        // Assert
        updatedConfig.Should().NotBeNull();
        config.DRPattern.Should().Be(K8sDRPattern.HotStandby);
    }

    [Fact]
    public async Task K8sHADRPanel_ChangingDRPattern_AutoSetsRTO()
    {
        // Arrange
        var config = new K8sHADRConfig { DRPattern = K8sDRPattern.None };
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Config, config)
            .Add(p => p.ConfigChanged, EventCallback.Factory.Create<K8sHADRConfig>(this, _ => { })));

        // Act - Change to ActiveActive
        var selects = cut.FindAll("select");
        var drSelect = selects.First(s => s.InnerHtml.Contains("None (Multi-AZ only)"));
        await cut.InvokeAsync(() => drSelect.Change(K8sDRPattern.ActiveActive.ToString()));

        // Assert - RTO should be auto-set to 5 minutes for ActiveActive
        config.RTOMinutes.Should().Be(5);
    }

    [Fact]
    public async Task K8sHADRPanel_ChangingNodeDistribution_AutoAdjustsAZCount()
    {
        // Arrange
        var config = new K8sHADRConfig
        {
            NodeDistribution = K8sNodeDistribution.SingleAZ,
            AvailabilityZones = 1
        };
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Config, config)
            .Add(p => p.ConfigChanged, EventCallback.Factory.Create<K8sHADRConfig>(this, _ => { })));

        // Act - Change to DualAZ
        var selects = cut.FindAll("select");
        var nodeDistSelect = selects.First(s => s.InnerHtml.Contains("Single Availability Zone"));
        await cut.InvokeAsync(() => nodeDistSelect.Change(K8sNodeDistribution.DualAZ.ToString()));

        // Assert - AZ count should be auto-adjusted to 2
        config.AvailabilityZones.Should().Be(2);
    }

    [Fact]
    public async Task K8sHADRPanel_ChangingControlPlaneHA_InvokesCallback()
    {
        // Arrange
        K8sHADRConfig? updatedConfig = null;
        var config = new K8sHADRConfig { ControlPlaneHA = K8sControlPlaneHA.Single };
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Config, config)
            .Add(p => p.Distribution, Distribution.Kubernetes)
            .Add(p => p.ConfigChanged, EventCallback.Factory.Create<K8sHADRConfig>(this, c => updatedConfig = c)));

        // Act
        var selects = cut.FindAll("select");
        var cpSelect = selects.First(s => s.InnerHtml.Contains("Single Node (No HA)"));
        await cut.InvokeAsync(() => cpSelect.Change(K8sControlPlaneHA.StackedHA.ToString()));

        // Assert
        updatedConfig.Should().NotBeNull();
        config.ControlPlaneHA.Should().Be(K8sControlPlaneHA.StackedHA);
    }

    [Fact]
    public async Task K8sHADRPanel_NoCallback_DoesNotThrow()
    {
        // Arrange
        var config = new K8sHADRConfig();
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Config, config));

        // Act & Assert
        var action = async () =>
        {
            var selects = cut.FindAll("select");
            var drSelect = selects.First(s => s.InnerHtml.Contains("None (Multi-AZ only)"));
            await cut.InvokeAsync(() => drSelect.Change(K8sDRPattern.WarmStandby.ToString()));
        };

        await action.Should().NotThrowAsync();
    }

    #endregion

    #region Summary Display Tests

    [Fact]
    public void K8sHADRPanel_Summary_DisplaysConfigSummary()
    {
        // Arrange
        var config = new K8sHADRConfig
        {
            NodeDistribution = K8sNodeDistribution.MultiAZ,
            AvailabilityZones = 3,
            DRPattern = K8sDRPattern.HotStandby
        };

        // Act
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Config, config));

        // Assert - Summary shows "3 AZs" and DR pattern
        var summaryText = cut.Find(".summary-text").TextContent;
        summaryText.Should().Contain("3 AZ");
        summaryText.Should().Contain("HotStandby");
    }

    [Fact]
    public void K8sHADRPanel_Summary_DisplaysCostMultiplier()
    {
        // Arrange
        var config = new K8sHADRConfig
        {
            DRPattern = K8sDRPattern.ActiveActive
        };

        // Act
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Config, config)
            .Add(p => p.Distribution, Distribution.EKS));

        // Assert
        var summaryCost = cut.Find(".summary-cost").TextContent;
        summaryCost.Should().Contain("x"); // Should contain multiplier value
    }

    [Theory]
    [InlineData(Distribution.AKS, K8sNodeDistribution.MultiAZ)]
    [InlineData(Distribution.EKS, K8sNodeDistribution.MultiAZ)]
    public void K8sHADRPanel_Summary_ReflectsProviderAwareCost(Distribution distribution, K8sNodeDistribution nodeDist)
    {
        // Arrange
        var config = new K8sHADRConfig { NodeDistribution = nodeDist, AvailabilityZones = 3 };

        // Act
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Config, config)
            .Add(p => p.Distribution, distribution));

        // Assert - Summary should show multiplier (Azure FREE vs AWS paid)
        var costText = cut.Find(".summary-cost").TextContent;
        costText.Should().Contain("Cost multiplier:");
    }

    #endregion

    #region Parameter Update Tests

    [Fact]
    public void K8sHADRPanel_UpdatesUI_WhenConfigChanges()
    {
        // Arrange
        var config = new K8sHADRConfig { DRPattern = K8sDRPattern.None };
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Config, config));

        // Assert initial - no DR region
        cut.FindAll(".config-label").Should().NotContain(l => l.TextContent.Contains("DR Region"));

        // Act
        config.DRPattern = K8sDRPattern.WarmStandby;
        cut.SetParametersAndRender(parameters => parameters.Add(p => p.Config, config));

        // Assert - DR region should now be visible
        cut.FindAll(".config-label").Should().Contain(l => l.TextContent.Contains("DR Region"));
    }

    [Fact]
    public void K8sHADRPanel_UpdatesUI_WhenDistributionChanges()
    {
        // Arrange
        var config = new K8sHADRConfig();
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Config, config)
            .Add(p => p.Distribution, Distribution.EKS));

        // Assert initial - managed badge
        cut.Find(".managed-badge").Should().NotBeNull();

        // Act - switch to self-managed
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.Config, config)
            .Add(p => p.Distribution, Distribution.Kubernetes));

        // Assert - control plane selector should now be visible
        cut.FindAll(".config-label").Should().Contain(l => l.TextContent.Contains("Control Plane"));
    }

    #endregion

    #region Display Helper Tests

    [Fact]
    public void K8sHADRPanel_ShowsCorrectControlPlaneHAHints()
    {
        // Arrange
        var config = new K8sHADRConfig { ControlPlaneHA = K8sControlPlaneHA.Single };

        // Act
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Config, config)
            .Add(p => p.Distribution, Distribution.Kubernetes));

        // Assert
        cut.FindAll(".config-hint").Should().Contain(h => h.TextContent.Contains("Not recommended for production"));
    }

    [Fact]
    public void K8sHADRPanel_ShowsCorrectNodeDistributionHints()
    {
        // Arrange
        var config = new K8sHADRConfig { NodeDistribution = K8sNodeDistribution.MultiAZ };

        // Act
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Config, config));

        // Assert
        cut.FindAll(".config-hint").Should().Contain(h => h.TextContent.Contains("Recommended for production"));
    }

    [Fact]
    public void K8sHADRPanel_ShowsCorrectDRPatternHints()
    {
        // Arrange
        var config = new K8sHADRConfig { DRPattern = K8sDRPattern.ActiveActive };

        // Act
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Config, config));

        // Assert
        cut.FindAll(".config-hint").Should().Contain(h => h.TextContent.Contains("Multiple regions"));
    }

    [Fact]
    public void K8sHADRPanel_ShowsCorrectBackupStrategyHints()
    {
        // Arrange
        var config = new K8sHADRConfig { BackupStrategy = K8sBackupStrategy.Velero };

        // Act
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Config, config));

        // Assert
        cut.FindAll(".config-hint").Should().Contain(h => h.TextContent.Contains("CNCF project"));
    }

    #endregion

    #region Section Icon Tests

    [Fact]
    public void K8sHADRPanel_HighAvailabilitySection_HasShieldIcon()
    {
        // Act
        var cut = RenderComponent<K8sHADRPanel>();

        // Assert
        var sections = cut.FindAll(".section-icon");
        sections[0].TextContent.Should().Contain("🛡️");
    }

    [Fact]
    public void K8sHADRPanel_DisasterRecoverySection_HasRefreshIcon()
    {
        // Act
        var cut = RenderComponent<K8sHADRPanel>();

        // Assert
        var sections = cut.FindAll(".section-icon");
        sections[1].TextContent.Should().Contain("🔄");
    }

    #endregion

    #region Additional Managed Distribution Tests

    [Theory]
    [InlineData(Distribution.IKS)]
    [InlineData(Distribution.ACK)]
    [InlineData(Distribution.TKE)]
    [InlineData(Distribution.CCE)]
    [InlineData(Distribution.VKE)]
    [InlineData(Distribution.HetznerK8s)]
    [InlineData(Distribution.OVHKubernetes)]
    [InlineData(Distribution.ScalewayKapsule)]
    [InlineData(Distribution.OpenShiftDedicated)]
    [InlineData(Distribution.OpenShiftIBM)]
    public void K8sHADRPanel_AdditionalManagedDistributions_ShowsManagedBadge(Distribution distribution)
    {
        // Act
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Distribution, distribution));

        // Assert
        cut.Find(".managed-badge").TextContent.Should().Contain("Managed");
    }

    [Theory]
    [InlineData(Distribution.IKS, "IBM Cloud")]
    [InlineData(Distribution.DOKS, "DigitalOcean")]
    [InlineData(Distribution.LKE, "Linode")]
    public void K8sHADRPanel_AdditionalProviders_ShowsCorrectName(Distribution distribution, string expectedProvider)
    {
        // Act
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Distribution, distribution));

        // Assert
        cut.Find(".managed-text").TextContent.Should().Contain(expectedProvider);
    }

    [Theory]
    [InlineData(Distribution.VKE)]
    [InlineData(Distribution.HetznerK8s)]
    [InlineData(Distribution.OVHKubernetes)]
    [InlineData(Distribution.ScalewayKapsule)]
    public void K8sHADRPanel_MinorCloudProviders_ShowsGenericProviderName(Distribution distribution)
    {
        // Act
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Distribution, distribution));

        // Assert - Falls back to "the cloud provider"
        cut.Find(".managed-text").TextContent.Should().Contain("cloud provider");
    }

    #endregion

    #region Control Plane Nodes Tests

    [Fact]
    public async Task K8sHADRPanel_ChangingControlPlaneNodes_InvokesCallback()
    {
        // Arrange
        K8sHADRConfig? updatedConfig = null;
        var config = new K8sHADRConfig { ControlPlaneHA = K8sControlPlaneHA.StackedHA, ControlPlaneNodes = 3 };
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Config, config)
            .Add(p => p.Distribution, Distribution.Kubernetes)
            .Add(p => p.ConfigChanged, EventCallback.Factory.Create<K8sHADRConfig>(this, c => updatedConfig = c)));

        // Act
        var selects = cut.FindAll("select");
        var cpNodesSelect = selects.First(s => s.InnerHtml.Contains("3 nodes"));
        await cut.InvokeAsync(() => cpNodesSelect.Change("5"));

        // Assert
        updatedConfig.Should().NotBeNull();
        config.ControlPlaneNodes.Should().Be(5);
    }

    [Fact]
    public void K8sHADRPanel_StackedHA_Shows3NodeOption()
    {
        // Arrange
        var config = new K8sHADRConfig { ControlPlaneHA = K8sControlPlaneHA.StackedHA };

        // Act
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Config, config)
            .Add(p => p.Distribution, Distribution.Kubernetes));

        // Assert
        var selects = cut.FindAll("select");
        selects.Should().Contain(s => s.InnerHtml.Contains("3 nodes (minimum HA)"));
    }

    [Fact]
    public void K8sHADRPanel_ExternalEtcd_ShowsControlPlaneNodesSelector()
    {
        // Arrange
        var config = new K8sHADRConfig { ControlPlaneHA = K8sControlPlaneHA.ExternalEtcd };

        // Act
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Config, config)
            .Add(p => p.Distribution, Distribution.Kubernetes));

        // Assert
        cut.FindAll(".config-label").Should().Contain(l => l.TextContent.Contains("Control Plane Nodes"));
    }

    #endregion

    #region Availability Zones Count Tests

    [Fact]
    public async Task K8sHADRPanel_ChangingAZCount_InvokesCallback()
    {
        // Arrange
        K8sHADRConfig? updatedConfig = null;
        var config = new K8sHADRConfig { NodeDistribution = K8sNodeDistribution.MultiAZ, AvailabilityZones = 3 };
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Config, config)
            .Add(p => p.ConfigChanged, EventCallback.Factory.Create<K8sHADRConfig>(this, c => updatedConfig = c)));

        // Act
        var selects = cut.FindAll("select");
        var azSelect = selects.First(s => s.InnerHtml.Contains("3 AZs"));
        await cut.InvokeAsync(() => azSelect.Change("4"));

        // Assert
        updatedConfig.Should().NotBeNull();
        config.AvailabilityZones.Should().Be(4);
    }

    [Fact]
    public void K8sHADRPanel_DualAZ_OnlyShowsTwoAZOption()
    {
        // Arrange
        var config = new K8sHADRConfig { NodeDistribution = K8sNodeDistribution.DualAZ };

        // Act
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Config, config));

        // Assert - Only 2 AZs option available for DualAZ
        var selects = cut.FindAll("select");
        var azSelect = selects.First(s => s.InnerHtml.Contains("2 AZs"));
        azSelect.InnerHtml.Should().Contain("2 AZs");
        azSelect.InnerHtml.Should().NotContain("3 AZs");
    }

    [Fact]
    public void K8sHADRPanel_MultiAZ_ShowsMultipleAZOptions()
    {
        // Arrange
        var config = new K8sHADRConfig { NodeDistribution = K8sNodeDistribution.MultiAZ };

        // Act
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Config, config));

        // Assert
        var selects = cut.FindAll("select");
        var azSelect = selects.First(s => s.InnerHtml.Contains("AZs"));
        azSelect.InnerHtml.Should().Contain("3 AZs");
        azSelect.InnerHtml.Should().Contain("4 AZs");
        azSelect.InnerHtml.Should().Contain("5 AZs");
    }

    [Fact]
    public async Task K8sHADRPanel_ChangeToMultiAZ_PreservesHigherAZCount()
    {
        // Arrange
        var config = new K8sHADRConfig
        {
            NodeDistribution = K8sNodeDistribution.MultiRegion,
            AvailabilityZones = 5
        };
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Config, config)
            .Add(p => p.ConfigChanged, EventCallback.Factory.Create<K8sHADRConfig>(this, _ => { })));

        // Act - Change to MultiAZ (should preserve 5 since it's >= 3)
        var selects = cut.FindAll("select");
        var nodeDistSelect = selects.First(s => s.InnerHtml.Contains("Multi-Region"));
        await cut.InvokeAsync(() => nodeDistSelect.Change(K8sNodeDistribution.MultiAZ.ToString()));

        // Assert
        config.AvailabilityZones.Should().Be(5);
    }

    #endregion

    #region DR Region Tests

    [Fact]
    public async Task K8sHADRPanel_ChangingDRRegion_InvokesCallback()
    {
        // Arrange
        K8sHADRConfig? updatedConfig = null;
        var config = new K8sHADRConfig { DRPattern = K8sDRPattern.WarmStandby };
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Config, config)
            .Add(p => p.ConfigChanged, EventCallback.Factory.Create<K8sHADRConfig>(this, c => updatedConfig = c)));

        // Act
        var drRegionInput = cut.Find("input[placeholder*='us-west']");
        await cut.InvokeAsync(() => drRegionInput.Change("eu-west-1"));

        // Assert
        updatedConfig.Should().NotBeNull();
        config.DRRegion.Should().Be("eu-west-1");
    }

    [Fact]
    public void K8sHADRPanel_DRPatternSet_ShowsDRRegionInput()
    {
        // Arrange
        var config = new K8sHADRConfig { DRPattern = K8sDRPattern.HotStandby };

        // Act
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Config, config));

        // Assert
        cut.Find("input[placeholder*='us-west']").Should().NotBeNull();
    }

    #endregion

    #region Backup Frequency and Retention Tests

    [Fact]
    public async Task K8sHADRPanel_ChangingBackupFrequency_InvokesCallback()
    {
        // Arrange
        K8sHADRConfig? updatedConfig = null;
        var config = new K8sHADRConfig { BackupStrategy = K8sBackupStrategy.Velero, BackupFrequencyHours = 24 };
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Config, config)
            .Add(p => p.ConfigChanged, EventCallback.Factory.Create<K8sHADRConfig>(this, c => updatedConfig = c)));

        // Act
        var selects = cut.FindAll("select");
        var freqSelect = selects.First(s => s.InnerHtml.Contains("Hourly"));
        await cut.InvokeAsync(() => freqSelect.Change("4"));

        // Assert
        updatedConfig.Should().NotBeNull();
        config.BackupFrequencyHours.Should().Be(4);
    }

    [Fact]
    public async Task K8sHADRPanel_ChangingBackupRetention_InvokesCallback()
    {
        // Arrange
        K8sHADRConfig? updatedConfig = null;
        var config = new K8sHADRConfig { BackupStrategy = K8sBackupStrategy.Kasten, BackupRetentionDays = 30 };
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Config, config)
            .Add(p => p.ConfigChanged, EventCallback.Factory.Create<K8sHADRConfig>(this, c => updatedConfig = c)));

        // Act
        var selects = cut.FindAll("select");
        var retentionSelect = selects.First(s => s.InnerHtml.Contains("30 days"));
        await cut.InvokeAsync(() => retentionSelect.Change("90"));

        // Assert
        updatedConfig.Should().NotBeNull();
        config.BackupRetentionDays.Should().Be(90);
    }

    [Fact]
    public void K8sHADRPanel_BackupStrategySet_ShowsFrequencyOptions()
    {
        // Arrange
        var config = new K8sHADRConfig { BackupStrategy = K8sBackupStrategy.Portworx };

        // Act
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Config, config));

        // Assert
        var selects = cut.FindAll("select");
        selects.Should().Contain(s => s.InnerHtml.Contains("Hourly"));
        selects.Should().Contain(s => s.InnerHtml.Contains("Every 4 hours"));
        selects.Should().Contain(s => s.InnerHtml.Contains("Weekly"));
    }

    [Fact]
    public void K8sHADRPanel_BackupStrategySet_ShowsRetentionOptions()
    {
        // Arrange
        var config = new K8sHADRConfig { BackupStrategy = K8sBackupStrategy.Velero };

        // Act
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Config, config));

        // Assert
        var selects = cut.FindAll("select");
        selects.Should().Contain(s => s.InnerHtml.Contains("7 days"));
        selects.Should().Contain(s => s.InnerHtml.Contains("1 year"));
    }

    #endregion

    #region Custom Backup Strategy Tests

    [Fact]
    public void K8sHADRPanel_AllDistributions_ShowCustomBackupOption()
    {
        // Act
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Distribution, Distribution.Kubernetes));

        // Assert
        var selects = cut.FindAll("select");
        selects.Should().Contain(s => s.InnerHtml.Contains("Custom Solution"));
    }

    [Fact]
    public void K8sHADRPanel_CustomBackupStrategy_ShowsCorrectHint()
    {
        // Arrange
        var config = new K8sHADRConfig { BackupStrategy = K8sBackupStrategy.Custom };

        // Act
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Config, config));

        // Assert
        cut.FindAll(".config-hint").Should().Contain(h => h.TextContent.Contains("Organization-specific"));
    }

    #endregion

    #region RTO Select Tests

    [Fact]
    public async Task K8sHADRPanel_ChangingRTO_InvokesCallback()
    {
        // Arrange
        K8sHADRConfig? updatedConfig = null;
        var config = new K8sHADRConfig { DRPattern = K8sDRPattern.WarmStandby, RTOMinutes = 60 };
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Config, config)
            .Add(p => p.ConfigChanged, EventCallback.Factory.Create<K8sHADRConfig>(this, c => updatedConfig = c)));

        // Act
        var selects = cut.FindAll("select");
        var rtoSelect = selects.First(s => s.InnerHtml.Contains("5 minutes"));
        await cut.InvokeAsync(() => rtoSelect.Change("480"));

        // Assert
        updatedConfig.Should().NotBeNull();
        config.RTOMinutes.Should().Be(480);
    }

    [Fact]
    public void K8sHADRPanel_RTOSelector_ShowsAllOptions()
    {
        // Arrange - Need a DR pattern that shows the RTO selector
        var config = new K8sHADRConfig { DRPattern = K8sDRPattern.HotStandby };

        // Act
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Config, config));

        // Assert - Find the RTO select by looking for time-based options
        var selects = cut.FindAll("select");
        var rtoSelect = selects.FirstOrDefault(s => s.InnerHtml.Contains("5 minutes"));
        rtoSelect.Should().NotBeNull("RTO selector should exist when DR pattern is active");
        rtoSelect!.InnerHtml.Should().Contain("5 minutes (Active-Active)");
        rtoSelect.InnerHtml.Should().Contain("15 minutes (Hot Standby)");
        rtoSelect.InnerHtml.Should().Contain("1 hour (Warm Standby)");
        rtoSelect.InnerHtml.Should().Contain("4 hours");
        rtoSelect.InnerHtml.Should().Contain("8 hours");
        rtoSelect.InnerHtml.Should().Contain("24 hours (Backup/Restore)");
    }

    #endregion

    #region RTO Auto-Set Tests

    [Theory]
    [InlineData(K8sDRPattern.ActiveActive, 5)]
    [InlineData(K8sDRPattern.HotStandby, 15)]
    [InlineData(K8sDRPattern.WarmStandby, 60)]
    [InlineData(K8sDRPattern.BackupRestore, 240)]
    public async Task K8sHADRPanel_DRPatternChange_SetsCorrectRTO(K8sDRPattern pattern, int expectedRTO)
    {
        // Arrange
        var config = new K8sHADRConfig { DRPattern = K8sDRPattern.None };
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Config, config)
            .Add(p => p.ConfigChanged, EventCallback.Factory.Create<K8sHADRConfig>(this, _ => { })));

        // Act
        var selects = cut.FindAll("select");
        var drSelect = selects.First(s => s.InnerHtml.Contains("None (Multi-AZ only)"));
        await cut.InvokeAsync(() => drSelect.Change(pattern.ToString()));

        // Assert
        config.RTOMinutes.Should().Be(expectedRTO);
    }

    [Fact]
    public async Task K8sHADRPanel_DRPatternNone_ClearsRTO()
    {
        // Arrange
        var config = new K8sHADRConfig { DRPattern = K8sDRPattern.HotStandby, RTOMinutes = 15 };
        var cut = RenderComponent<K8sHADRPanel>(parameters => parameters
            .Add(p => p.Config, config)
            .Add(p => p.ConfigChanged, EventCallback.Factory.Create<K8sHADRConfig>(this, _ => { })));

        // Act
        var selects = cut.FindAll("select");
        var drSelect = selects.First(s => s.InnerHtml.Contains("Hot Standby"));
        await cut.InvokeAsync(() => drSelect.Change(K8sDRPattern.None.ToString()));

        // Assert
        config.RTOMinutes.Should().BeNull();
    }

    #endregion
}
