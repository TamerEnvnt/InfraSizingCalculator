using FluentAssertions;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Models.Pricing;

namespace InfraSizingCalculator.UnitTests.Models;

/// <summary>
/// Tests for K8sHADRConfig model including cost multiplier calculations.
/// Verifies business rules BR-HADR-001 through BR-HADR-020.
/// </summary>
public class K8sHADRConfigTests
{
    #region Default Values Tests

    [Fact]
    public void DefaultConstructor_SetsCorrectDefaults()
    {
        var config = new K8sHADRConfig();

        config.ControlPlaneHA.Should().Be(K8sControlPlaneHA.Managed);
        config.ControlPlaneNodes.Should().Be(3);
        config.NodeDistribution.Should().Be(K8sNodeDistribution.MultiAZ);
        config.AvailabilityZones.Should().Be(3);
        config.DRPattern.Should().Be(K8sDRPattern.None);
        config.BackupStrategy.Should().Be(K8sBackupStrategy.None);
        config.BackupFrequencyHours.Should().Be(24);
        config.BackupRetentionDays.Should().Be(30);
        config.EnablePodDisruptionBudgets.Should().BeTrue();
        config.EnableTopologySpread.Should().BeTrue();
    }

    #endregion

    #region Control Plane HA Cost Multiplier Tests (BR-HADR-001 to BR-HADR-004)

    [Fact]
    public void GetCostMultiplier_ManagedControlPlane_NoExtraCost()
    {
        var config = new K8sHADRConfig
        {
            ControlPlaneHA = K8sControlPlaneHA.Managed,
            NodeDistribution = K8sNodeDistribution.SingleAZ
        };

        config.GetCostMultiplier().Should().Be(1.0m);
    }

    [Fact]
    public void GetCostMultiplier_SingleControlPlane_NoExtraCost()
    {
        var config = new K8sHADRConfig
        {
            ControlPlaneHA = K8sControlPlaneHA.Single,
            NodeDistribution = K8sNodeDistribution.SingleAZ
        };

        config.GetCostMultiplier().Should().Be(1.0m);
    }

    [Theory]
    [InlineData(3, 0.20)] // 3 nodes = +0.20 (0.10 * 2)
    [InlineData(5, 0.40)] // 5 nodes = +0.40 (0.10 * 4)
    [InlineData(7, 0.60)] // 7 nodes = +0.60 (0.10 * 6)
    public void GetCostMultiplier_StackedHA_ScalesWithNodes(int nodes, decimal expectedExtra)
    {
        var config = new K8sHADRConfig
        {
            ControlPlaneHA = K8sControlPlaneHA.StackedHA,
            ControlPlaneNodes = nodes,
            NodeDistribution = K8sNodeDistribution.SingleAZ
        };

        config.GetCostMultiplier().Should().Be(1.0m + expectedExtra);
    }

    [Theory]
    [InlineData(3, 0.39)] // 3 nodes = +0.24 (0.12 * 2) + 0.15 = +0.39
    [InlineData(5, 0.63)] // 5 nodes = +0.48 (0.12 * 4) + 0.15 = +0.63
    [InlineData(7, 0.87)] // 7 nodes = +0.72 (0.12 * 6) + 0.15 = +0.87
    public void GetCostMultiplier_ExternalEtcd_IncludesEtcdClusterCost(int nodes, decimal expectedExtra)
    {
        var config = new K8sHADRConfig
        {
            ControlPlaneHA = K8sControlPlaneHA.ExternalEtcd,
            ControlPlaneNodes = nodes,
            NodeDistribution = K8sNodeDistribution.SingleAZ
        };

        config.GetCostMultiplier().Should().Be(1.0m + expectedExtra);
    }

    #endregion

    #region Node Distribution Cost Multiplier Tests (BR-HADR-005 to BR-HADR-009)

    [Fact]
    public void GetCostMultiplier_SingleAZ_NoCrossAZCost()
    {
        var config = new K8sHADRConfig
        {
            NodeDistribution = K8sNodeDistribution.SingleAZ
        };

        config.GetCostMultiplier().Should().Be(1.0m);
    }

    [Fact]
    public void GetCostMultiplier_DualAZ_Adds2Percent()
    {
        var config = new K8sHADRConfig
        {
            NodeDistribution = K8sNodeDistribution.DualAZ
        };

        config.GetCostMultiplier().Should().Be(1.02m);
    }

    [Fact]
    public void GetCostMultiplier_MultiAZ_Adds3Percent()
    {
        var config = new K8sHADRConfig
        {
            NodeDistribution = K8sNodeDistribution.MultiAZ
        };

        config.GetCostMultiplier().Should().Be(1.03m);
    }

    [Fact]
    public void GetCostMultiplier_MultiRegion_Adds20Percent()
    {
        var config = new K8sHADRConfig
        {
            NodeDistribution = K8sNodeDistribution.MultiRegion
        };

        config.GetCostMultiplier().Should().Be(1.20m);
    }

    #endregion

    #region DR Pattern Cost Multiplier Tests (BR-HADR-010 to BR-HADR-013)

    [Fact]
    public void GetCostMultiplier_DRNone_NoDRCost()
    {
        var config = new K8sHADRConfig
        {
            DRPattern = K8sDRPattern.None,
            NodeDistribution = K8sNodeDistribution.SingleAZ
        };

        config.GetCostMultiplier().Should().Be(1.0m);
    }

    [Fact]
    public void GetCostMultiplier_BackupRestore_Adds8Percent()
    {
        var config = new K8sHADRConfig
        {
            DRPattern = K8sDRPattern.BackupRestore,
            NodeDistribution = K8sNodeDistribution.SingleAZ
        };

        config.GetCostMultiplier().Should().Be(1.08m);
    }

    [Fact]
    public void GetCostMultiplier_WarmStandby_Adds40Percent()
    {
        var config = new K8sHADRConfig
        {
            DRPattern = K8sDRPattern.WarmStandby,
            NodeDistribution = K8sNodeDistribution.SingleAZ
        };

        config.GetCostMultiplier().Should().Be(1.40m);
    }

    [Fact]
    public void GetCostMultiplier_HotStandby_Adds90Percent()
    {
        var config = new K8sHADRConfig
        {
            DRPattern = K8sDRPattern.HotStandby,
            NodeDistribution = K8sNodeDistribution.SingleAZ
        };

        config.GetCostMultiplier().Should().Be(1.90m);
    }

    [Fact]
    public void GetCostMultiplier_ActiveActive_Adds110Percent()
    {
        var config = new K8sHADRConfig
        {
            DRPattern = K8sDRPattern.ActiveActive,
            NodeDistribution = K8sNodeDistribution.SingleAZ
        };

        config.GetCostMultiplier().Should().Be(2.10m);
    }

    #endregion

    #region Backup Strategy Cost Multiplier Tests (BR-HADR-017 to BR-HADR-020)

    [Fact]
    public void GetCostMultiplier_BackupNone_NoBackupCost()
    {
        var config = new K8sHADRConfig
        {
            DRPattern = K8sDRPattern.None,
            BackupStrategy = K8sBackupStrategy.None,
            NodeDistribution = K8sNodeDistribution.SingleAZ
        };

        config.GetCostMultiplier().Should().Be(1.0m);
    }

    [Fact]
    public void GetCostMultiplier_Velero_Adds2Percent()
    {
        var config = new K8sHADRConfig
        {
            DRPattern = K8sDRPattern.None,
            BackupStrategy = K8sBackupStrategy.Velero,
            NodeDistribution = K8sNodeDistribution.SingleAZ
        };

        config.GetCostMultiplier().Should().Be(1.02m);
    }

    [Fact]
    public void GetCostMultiplier_Kasten_Adds5Percent()
    {
        var config = new K8sHADRConfig
        {
            DRPattern = K8sDRPattern.None,
            BackupStrategy = K8sBackupStrategy.Kasten,
            NodeDistribution = K8sNodeDistribution.SingleAZ
        };

        config.GetCostMultiplier().Should().Be(1.05m);
    }

    [Fact]
    public void GetCostMultiplier_Portworx_Adds8Percent()
    {
        var config = new K8sHADRConfig
        {
            DRPattern = K8sDRPattern.None,
            BackupStrategy = K8sBackupStrategy.Portworx,
            NodeDistribution = K8sNodeDistribution.SingleAZ
        };

        config.GetCostMultiplier().Should().Be(1.08m);
    }

    [Fact]
    public void GetCostMultiplier_CloudNative_Adds3Percent()
    {
        var config = new K8sHADRConfig
        {
            DRPattern = K8sDRPattern.None,
            BackupStrategy = K8sBackupStrategy.CloudNative,
            NodeDistribution = K8sNodeDistribution.SingleAZ
        };

        config.GetCostMultiplier().Should().Be(1.03m);
    }

    [Fact]
    public void GetCostMultiplier_BackupWithDRPattern_OnlyDRCostApplies()
    {
        // When DR pattern is set, backup strategy cost should NOT be added separately
        var config = new K8sHADRConfig
        {
            DRPattern = K8sDRPattern.BackupRestore,
            BackupStrategy = K8sBackupStrategy.Velero,
            NodeDistribution = K8sNodeDistribution.SingleAZ
        };

        // Should only be 1.08 (DR cost), not 1.10 (DR + backup)
        config.GetCostMultiplier().Should().Be(1.08m);
    }

    #endregion

    #region Combined Cost Multiplier Tests

    [Fact]
    public void GetCostMultiplier_FullHADRSetup_CombinesAllCosts()
    {
        var config = new K8sHADRConfig
        {
            ControlPlaneHA = K8sControlPlaneHA.StackedHA,
            ControlPlaneNodes = 5,
            NodeDistribution = K8sNodeDistribution.MultiAZ,
            DRPattern = K8sDRPattern.WarmStandby
        };

        // Base: 1.0
        // StackedHA with 5 nodes: +0.40 (0.10 * 4)
        // MultiAZ: +0.03
        // WarmStandby: +0.40
        // Total: 1.83
        config.GetCostMultiplier().Should().Be(1.83m);
    }

    [Fact]
    public void GetCostMultiplier_ExternalEtcdMultiRegionActiveActive_HighCost()
    {
        var config = new K8sHADRConfig
        {
            ControlPlaneHA = K8sControlPlaneHA.ExternalEtcd,
            ControlPlaneNodes = 7,
            NodeDistribution = K8sNodeDistribution.MultiRegion,
            DRPattern = K8sDRPattern.ActiveActive
        };

        // Base: 1.0
        // ExternalEtcd with 7 nodes: +0.87 (0.12 * 6 + 0.15)
        // MultiRegion: +0.20
        // ActiveActive: +1.10
        // Total: 3.17
        config.GetCostMultiplier().Should().Be(3.17m);
    }

    #endregion

    #region Provider-Aware Cost Multiplier Tests (BR-HADR-014 to BR-HADR-016)

    [Fact]
    public void GetCostMultiplier_AzureMultiAZ_FreeCrossAZ()
    {
        var config = new K8sHADRConfig
        {
            NodeDistribution = K8sNodeDistribution.MultiAZ,
            AvailabilityZones = 3
        };

        // Azure has FREE cross-AZ traffic
        var multiplier = config.GetCostMultiplier(Distribution.AKS);

        // Should be just 1.0 - no cross-AZ cost for Azure
        multiplier.Should().Be(1.0m);
    }

    [Fact]
    public void GetCostMultiplier_AWSMultiAZ_AddsCrossAZCost()
    {
        var config = new K8sHADRConfig
        {
            NodeDistribution = K8sNodeDistribution.MultiAZ,
            AvailabilityZones = 3
        };

        var multiplier = config.GetCostMultiplier(Distribution.EKS);

        // AWS adds ~3% for 3+ AZs
        multiplier.Should().Be(1.03m);
    }

    [Fact]
    public void GetCostMultiplier_GCPDualAZ_AddsCrossAZCost()
    {
        var config = new K8sHADRConfig
        {
            NodeDistribution = K8sNodeDistribution.DualAZ,
            AvailabilityZones = 2
        };

        var multiplier = config.GetCostMultiplier(Distribution.GKE);

        // GCP adds ~2% for 2 AZs
        multiplier.Should().Be(1.02m);
    }

    [Fact]
    public void GetCostMultiplier_OnPremMultiAZ_NoCrossAZCost()
    {
        var config = new K8sHADRConfig
        {
            NodeDistribution = K8sNodeDistribution.MultiAZ,
            AvailabilityZones = 3
        };

        var multiplier = config.GetCostMultiplier(Distribution.OpenShift);

        // On-prem has no cloud data transfer costs
        multiplier.Should().Be(1.0m);
    }

    [Fact]
    public void GetCostMultiplier_AzureMultiRegion_StillAddsCost()
    {
        var config = new K8sHADRConfig
        {
            NodeDistribution = K8sNodeDistribution.MultiRegion,
            AvailabilityZones = 3
        };

        var multiplier = config.GetCostMultiplier(Distribution.AKS);

        // Even Azure charges for multi-region
        // Cross-AZ = 0 (Azure free), Multi-region extra = +0.17
        multiplier.Should().Be(1.17m);
    }

    [Fact]
    public void GetCostMultiplier_AWSMultiRegionWithDR_CombinesAll()
    {
        var config = new K8sHADRConfig
        {
            ControlPlaneHA = K8sControlPlaneHA.StackedHA,
            ControlPlaneNodes = 3,
            NodeDistribution = K8sNodeDistribution.MultiRegion,
            AvailabilityZones = 3,
            DRPattern = K8sDRPattern.HotStandby
        };

        var multiplier = config.GetCostMultiplier(Distribution.EKS);

        // Base: 1.0
        // StackedHA 3 nodes: +0.20
        // Cross-AZ (3 zones): +0.03
        // Multi-region extra: +0.17
        // HotStandby: +0.90
        // Total: 2.30
        multiplier.Should().Be(2.30m);
    }

    #endregion

    #region GetSummary Tests

    [Fact]
    public void GetSummary_DefaultConfig_ReturnsBasicNoHADR()
    {
        var config = new K8sHADRConfig
        {
            ControlPlaneHA = K8sControlPlaneHA.Managed,
            NodeDistribution = K8sNodeDistribution.SingleAZ,
            DRPattern = K8sDRPattern.None,
            BackupStrategy = K8sBackupStrategy.None
        };

        config.GetSummary().Should().Be("Basic (no HA/DR)");
    }

    [Fact]
    public void GetSummary_StackedHAWithNodes_IncludesNodeCount()
    {
        var config = new K8sHADRConfig
        {
            ControlPlaneHA = K8sControlPlaneHA.StackedHA,
            ControlPlaneNodes = 5,
            NodeDistribution = K8sNodeDistribution.SingleAZ,
            DRPattern = K8sDRPattern.None,
            BackupStrategy = K8sBackupStrategy.None
        };

        config.GetSummary().Should().Contain("CP HA (5 nodes)");
    }

    [Fact]
    public void GetSummary_MultiAZ_IncludesAZCount()
    {
        var config = new K8sHADRConfig
        {
            ControlPlaneHA = K8sControlPlaneHA.Managed,
            NodeDistribution = K8sNodeDistribution.MultiAZ,
            AvailabilityZones = 3,
            DRPattern = K8sDRPattern.None,
            BackupStrategy = K8sBackupStrategy.None
        };

        config.GetSummary().Should().Contain("3 AZs");
    }

    [Fact]
    public void GetSummary_DRPattern_IncludesDRType()
    {
        var config = new K8sHADRConfig
        {
            ControlPlaneHA = K8sControlPlaneHA.Managed,
            NodeDistribution = K8sNodeDistribution.SingleAZ,
            DRPattern = K8sDRPattern.HotStandby,
            BackupStrategy = K8sBackupStrategy.None
        };

        config.GetSummary().Should().Contain("HotStandby");
    }

    [Fact]
    public void GetSummary_BackupStrategy_IncludesBackupTool()
    {
        var config = new K8sHADRConfig
        {
            ControlPlaneHA = K8sControlPlaneHA.Managed,
            NodeDistribution = K8sNodeDistribution.SingleAZ,
            DRPattern = K8sDRPattern.None,
            BackupStrategy = K8sBackupStrategy.Velero
        };

        config.GetSummary().Should().Contain("Backup (Velero)");
    }

    [Fact]
    public void GetSummary_FullConfig_CombinesAllParts()
    {
        var config = new K8sHADRConfig
        {
            ControlPlaneHA = K8sControlPlaneHA.ExternalEtcd,
            ControlPlaneNodes = 7,
            NodeDistribution = K8sNodeDistribution.MultiAZ,
            AvailabilityZones = 5,
            DRPattern = K8sDRPattern.ActiveActive,
            BackupStrategy = K8sBackupStrategy.Kasten
        };

        var summary = config.GetSummary();
        summary.Should().Contain("CP HA (7 nodes)");
        summary.Should().Contain("5 AZs");
        summary.Should().Contain("ActiveActive");
        summary.Should().Contain("Backup (Kasten)");
        summary.Should().Contain(" • ");
    }

    #endregion

    #region Edge Cases and Boundary Tests

    [Fact]
    public void GetCostMultiplier_CustomBackupStrategy_NoExtraCost()
    {
        var config = new K8sHADRConfig
        {
            DRPattern = K8sDRPattern.None,
            BackupStrategy = K8sBackupStrategy.Custom,
            NodeDistribution = K8sNodeDistribution.SingleAZ
        };

        // Custom backup strategy has no predefined cost
        config.GetCostMultiplier().Should().Be(1.0m);
    }

    [Fact]
    public void GetCostMultiplier_SingleAZWithOneAvailabilityZone_NoCrossAZCost()
    {
        var config = new K8sHADRConfig
        {
            NodeDistribution = K8sNodeDistribution.SingleAZ,
            AvailabilityZones = 1
        };

        config.GetCostMultiplier().Should().Be(1.0m);
    }

    [Fact]
    public void ProviderCostMultiplier_SingleAZ_AlwaysZeroCrossAZ()
    {
        var config = new K8sHADRConfig
        {
            NodeDistribution = K8sNodeDistribution.SingleAZ,
            AvailabilityZones = 1
        };

        // All providers should have 0 cross-AZ cost for SingleAZ
        config.GetCostMultiplier(Distribution.EKS).Should().Be(1.0m);
        config.GetCostMultiplier(Distribution.AKS).Should().Be(1.0m);
        config.GetCostMultiplier(Distribution.GKE).Should().Be(1.0m);
        config.GetCostMultiplier(Distribution.OpenShift).Should().Be(1.0m);
    }

    #endregion
}
