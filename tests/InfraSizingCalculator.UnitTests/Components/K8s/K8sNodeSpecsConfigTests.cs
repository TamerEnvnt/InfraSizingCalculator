using Bunit;
using InfraSizingCalculator.Components.K8s;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components.K8s;

/// <summary>
/// Tests for K8sNodeSpecsConfig Blazor component.
/// Tests single/multi-cluster modes, managed control plane, infra nodes, and spec callbacks.
/// </summary>
public class K8sNodeSpecsConfigTests : TestContext
{
    #region Single Cluster Mode Tests

    [Fact]
    public void SingleCluster_RendersNodeSpecsTable()
    {
        // Arrange & Act
        var cut = RenderComponent<K8sNodeSpecsConfig>(parameters =>
            parameters.Add(p => p.IsSingleCluster, true)
                     .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod })
                     .Add(p => p.NodeSpecs, new NodeSpecsConfig()));

        // Assert
        Assert.Contains("unified-specs", cut.Markup);
        Assert.Contains("Control Plane", cut.Markup);
        Assert.Contains("Worker", cut.Markup);
    }

    [Fact]
    public void SingleCluster_HidesControlPlane_WhenManaged()
    {
        // Arrange & Act
        var cut = RenderComponent<K8sNodeSpecsConfig>(parameters =>
            parameters.Add(p => p.IsSingleCluster, true)
                     .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod })
                     .Add(p => p.NodeSpecs, new NodeSpecsConfig())
                     .Add(p => p.HasManagedControlPlane, true)
                     .Add(p => p.VendorName, "AWS EKS"));

        // Assert
        Assert.Contains("managed-notice", cut.Markup);
        Assert.Contains("AWS EKS", cut.Markup);
        Assert.DoesNotContain("Control Plane</div>", cut.Markup);
    }

    [Fact]
    public void SingleCluster_ShowsInfraNodes_WhenEnabled()
    {
        // Arrange & Act
        var cut = RenderComponent<K8sNodeSpecsConfig>(parameters =>
            parameters.Add(p => p.IsSingleCluster, true)
                     .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod })
                     .Add(p => p.NodeSpecs, new NodeSpecsConfig())
                     .Add(p => p.HasInfraNodes, true));

        // Assert
        Assert.Contains("Infrastructure", cut.Markup);
    }

    [Fact]
    public void SingleCluster_HidesInfraNodes_WhenDisabled()
    {
        // Arrange & Act
        var cut = RenderComponent<K8sNodeSpecsConfig>(parameters =>
            parameters.Add(p => p.IsSingleCluster, true)
                     .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod })
                     .Add(p => p.NodeSpecs, new NodeSpecsConfig())
                     .Add(p => p.HasInfraNodes, false));

        // Assert
        Assert.DoesNotContain("Infrastructure", cut.Markup);
    }

    #endregion

    #region Multi-Cluster Mode Tests

    [Fact]
    public void MultiCluster_RendersAccordion()
    {
        // Arrange & Act
        var cut = RenderComponent<K8sNodeSpecsConfig>(parameters =>
            parameters.Add(p => p.IsSingleCluster, false)
                     .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Dev, EnvironmentType.Prod })
                     .Add(p => p.NodeSpecs, new NodeSpecsConfig()));

        // Assert
        Assert.Contains("Node Specifications", cut.Markup);
        Assert.Contains("node-specs-accordion", cut.Markup);
    }

    [Fact]
    public void MultiCluster_ShowsNodeCards()
    {
        // Arrange & Act
        var cut = RenderComponent<K8sNodeSpecsConfig>(parameters =>
            parameters.Add(p => p.IsSingleCluster, false)
                     .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod })
                     .Add(p => p.NodeSpecs, new NodeSpecsConfig())
                     .Add(p => p.HasInfraNodes, true));

        // Assert
        var nodeCards = cut.FindAll(".node-card");
        Assert.True(nodeCards.Count >= 2); // At least control plane and worker
    }

    [Fact]
    public void MultiCluster_ShowsManagedNotice_WhenManagedControlPlane()
    {
        // Arrange & Act
        var cut = RenderComponent<K8sNodeSpecsConfig>(parameters =>
            parameters.Add(p => p.IsSingleCluster, false)
                     .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod })
                     .Add(p => p.NodeSpecs, new NodeSpecsConfig())
                     .Add(p => p.HasManagedControlPlane, true)
                     .Add(p => p.VendorName, "Azure AKS"));

        // Assert
        Assert.Contains("managed-notice", cut.Markup);
        Assert.Contains("Azure AKS", cut.Markup);
    }

    #endregion

    #region Environment Type Tests

    [Theory]
    [InlineData(EnvironmentType.Prod, "Production")]
    [InlineData(EnvironmentType.DR, "Production")]
    [InlineData(EnvironmentType.Dev, "Non-Production")]
    [InlineData(EnvironmentType.Test, "Non-Production")]
    [InlineData(EnvironmentType.Stage, "Non-Production")]
    public void IsProdEnv_ShowsCorrectLabel(EnvironmentType env, string expectedLabel)
    {
        // Arrange & Act
        var cut = RenderComponent<K8sNodeSpecsConfig>(parameters =>
            parameters.Add(p => p.IsSingleCluster, false)
                     .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { env })
                     .Add(p => p.NodeSpecs, new NodeSpecsConfig()));

        // Assert
        Assert.Contains(expectedLabel, cut.Markup);
    }

    [Theory]
    [InlineData(EnvironmentType.Dev, "env-dev")]
    [InlineData(EnvironmentType.Test, "env-test")]
    [InlineData(EnvironmentType.Stage, "env-stage")]
    [InlineData(EnvironmentType.Prod, "env-prod")]
    [InlineData(EnvironmentType.DR, "env-dr")]
    public void GetEnvClass_ReturnsCorrectCss(EnvironmentType env, string expectedClass)
    {
        // Arrange & Act
        var cut = RenderComponent<K8sNodeSpecsConfig>(parameters =>
            parameters.Add(p => p.IsSingleCluster, false)
                     .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { env })
                     .Add(p => p.NodeSpecs, new NodeSpecsConfig()));

        // Assert
        Assert.Contains(expectedClass, cut.Markup);
    }

    #endregion

    #region Spec Display Tests

    [Fact]
    public void DisplaysNodeSpecInputs()
    {
        // Arrange
        var nodeSpecs = new NodeSpecsConfig
        {
            ProdMasterCpu = 4,
            ProdMasterRam = 16,
            ProdMasterDisk = 100,
            ProdWorkerCpu = 8,
            ProdWorkerRam = 32,
            ProdWorkerDisk = 200
        };

        // Act
        var cut = RenderComponent<K8sNodeSpecsConfig>(parameters =>
            parameters.Add(p => p.IsSingleCluster, true)
                     .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod })
                     .Add(p => p.NodeSpecs, nodeSpecs));

        // Assert - inputs should have values
        var inputs = cut.FindAll("input[type='number']");
        Assert.True(inputs.Count >= 6); // 3 for control plane, 3 for worker
    }

    [Fact]
    public void DisplaysInfraSpecInputs_WhenHasInfraNodes()
    {
        // Arrange
        var nodeSpecs = new NodeSpecsConfig
        {
            ProdInfraCpu = 4,
            ProdInfraRam = 16,
            ProdInfraDisk = 100
        };

        // Act
        var cut = RenderComponent<K8sNodeSpecsConfig>(parameters =>
            parameters.Add(p => p.IsSingleCluster, true)
                     .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod })
                     .Add(p => p.NodeSpecs, nodeSpecs)
                     .Add(p => p.HasInfraNodes, true));

        // Assert - should have 9 inputs (3 node types x 3 specs)
        var inputs = cut.FindAll("input[type='number']");
        Assert.Equal(9, inputs.Count);
    }

    #endregion

    #region Column Headers Tests

    [Fact]
    public void SingleCluster_ShowsColumnHeaders()
    {
        // Arrange & Act
        var cut = RenderComponent<K8sNodeSpecsConfig>(parameters =>
            parameters.Add(p => p.IsSingleCluster, true)
                     .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod })
                     .Add(p => p.NodeSpecs, new NodeSpecsConfig()));

        // Assert
        Assert.Contains("CPU", cut.Markup);
        Assert.Contains("RAM (GB)", cut.Markup);
        Assert.Contains("Disk (GB)", cut.Markup);
    }

    #endregion

    #region Multiple Environments Tests

    [Fact]
    public void MultiCluster_ShowsAllEnvironments()
    {
        // Arrange
        var environments = new HashSet<EnvironmentType>
        {
            EnvironmentType.Dev,
            EnvironmentType.Test,
            EnvironmentType.Prod
        };

        // Act
        var cut = RenderComponent<K8sNodeSpecsConfig>(parameters =>
            parameters.Add(p => p.IsSingleCluster, false)
                     .Add(p => p.EnabledEnvironments, environments)
                     .Add(p => p.NodeSpecs, new NodeSpecsConfig()));

        // Assert - Should have accordion panels for each env
        Assert.Contains("env-dev", cut.Markup);
        Assert.Contains("env-test", cut.Markup);
        Assert.Contains("env-prod", cut.Markup);
    }

    #endregion
}
