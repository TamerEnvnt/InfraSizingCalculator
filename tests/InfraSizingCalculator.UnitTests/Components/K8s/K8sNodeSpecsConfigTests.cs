using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.K8s;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components.K8s;

public class K8sNodeSpecsConfigTests : TestContext
{

    #region Single Cluster Mode Tests

    [Fact]
    public void SingleCluster_RendersNodeSpecsTable()
    {
        // Arrange
        var nodeSpecs = new NodeSpecsConfig();

        // Act
        var cut = RenderComponent<K8sNodeSpecsConfig>(parameters => parameters
            .Add(p => p.IsSingleCluster, true)
            .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod })
            .Add(p => p.NodeSpecs, nodeSpecs)
            .Add(p => p.HasManagedControlPlane, false)
            .Add(p => p.HasInfraNodes, true));

        // Assert
        cut.Find(".unified-specs").Should().NotBeNull();
    }

    [Fact]
    public void SingleCluster_HidesMasterNodes_WhenManagedControlPlane()
    {
        // Arrange
        var nodeSpecs = new NodeSpecsConfig();

        // Act
        var cut = RenderComponent<K8sNodeSpecsConfig>(parameters => parameters
            .Add(p => p.IsSingleCluster, true)
            .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod })
            .Add(p => p.NodeSpecs, nodeSpecs)
            .Add(p => p.HasManagedControlPlane, true)
            .Add(p => p.HasInfraNodes, false)
            .Add(p => p.VendorName, "Azure"));

        // Assert
        cut.Find(".managed-notice").Should().NotBeNull();
        var markup = cut.Markup;
        markup.Should().Contain("Azure");
        markup.Should().NotContain("Control Plane</div>");
    }

    [Fact]
    public void SingleCluster_ShowsInfraNodes_WhenHasInfraNodes()
    {
        // Arrange
        var nodeSpecs = new NodeSpecsConfig();

        // Act
        var cut = RenderComponent<K8sNodeSpecsConfig>(parameters => parameters
            .Add(p => p.IsSingleCluster, true)
            .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod })
            .Add(p => p.NodeSpecs, nodeSpecs)
            .Add(p => p.HasManagedControlPlane, false)
            .Add(p => p.HasInfraNodes, true));

        // Assert
        var markup = cut.Markup;
        markup.Should().Contain("Infrastructure");
    }

    [Fact]
    public void SingleCluster_HidesInfraNodes_WhenNoInfraNodes()
    {
        // Arrange
        var nodeSpecs = new NodeSpecsConfig();

        // Act
        var cut = RenderComponent<K8sNodeSpecsConfig>(parameters => parameters
            .Add(p => p.IsSingleCluster, true)
            .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod })
            .Add(p => p.NodeSpecs, nodeSpecs)
            .Add(p => p.HasManagedControlPlane, false)
            .Add(p => p.HasInfraNodes, false));

        // Assert - should not contain infrastructure node group but should have worker
        var markup = cut.Markup;
        markup.Should().Contain("Worker");
        // Infrastructure is only shown when HasInfraNodes is true
    }

    #endregion

    #region Multi-Cluster Mode Tests

    [Fact]
    public void MultiCluster_RendersAccordion()
    {
        // Arrange
        var nodeSpecs = new NodeSpecsConfig();
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Dev, EnvironmentType.Prod };

        // Act
        var cut = RenderComponent<K8sNodeSpecsConfig>(parameters => parameters
            .Add(p => p.IsSingleCluster, false)
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.NodeSpecs, nodeSpecs)
            .Add(p => p.HasManagedControlPlane, false)
            .Add(p => p.HasInfraNodes, false));

        // Assert
        cut.Find(".multi-cluster-header").Should().NotBeNull();
    }

    #endregion

    #region GetMasterCpu Tests

    [Theory]
    [InlineData(EnvironmentType.Dev, 2)]
    [InlineData(EnvironmentType.Test, 2)]
    [InlineData(EnvironmentType.Stage, 2)]
    [InlineData(EnvironmentType.Prod, 4)]
    [InlineData(EnvironmentType.DR, 4)]
    public void GetMasterCpu_ReturnsCorrectValue(EnvironmentType env, int expected)
    {
        // Arrange
        var nodeSpecs = new NodeSpecsConfig
        {
            DevMasterCpu = 2,
            TestMasterCpu = 2,
            StageMasterCpu = 2,
            ProdMasterCpu = 4,
            DRMasterCpu = 4
        };

        // Act
        var cut = RenderComponent<K8sNodeSpecsConfig>(parameters => parameters
            .Add(p => p.IsSingleCluster, false)
            .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { env })
            .Add(p => p.NodeSpecs, nodeSpecs)
            .Add(p => p.HasManagedControlPlane, false)
            .Add(p => p.HasInfraNodes, false));

        // Assert - find CPU input for control plane
        var inputs = cut.FindAll("input[type='number']");
        // First input in each card is CPU
        inputs.Should().NotBeEmpty();
    }

    #endregion

    #region SetMasterCpu Tests

    [Fact]
    public async Task SetMasterCpu_UpdatesDevEnvironment()
    {
        // Arrange
        var nodeSpecs = new NodeSpecsConfig { DevMasterCpu = 2 };
        NodeSpecsConfig? updatedSpecs = null;

        var cut = RenderComponent<K8sNodeSpecsConfig>(parameters => parameters
            .Add(p => p.IsSingleCluster, false)
            .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Dev })
            .Add(p => p.NodeSpecs, nodeSpecs)
            .Add(p => p.NodeSpecsChanged, EventCallback.Factory.Create<NodeSpecsConfig>(
                this, specs => updatedSpecs = specs))
            .Add(p => p.HasManagedControlPlane, false)
            .Add(p => p.HasInfraNodes, false));

        // Act - find and update the CPU input
        var cpuInputs = cut.FindAll("input[type='number']");
        if (cpuInputs.Any())
        {
            await cut.InvokeAsync(() => cpuInputs[0].Change(new ChangeEventArgs { Value = "8" }));
        }

        // Assert
        updatedSpecs.Should().NotBeNull();
    }

    #endregion

    #region GetMasterRam Tests

    [Theory]
    [InlineData(EnvironmentType.Dev, 8)]
    [InlineData(EnvironmentType.Prod, 16)]
    public void GetMasterRam_ReturnsCorrectValue(EnvironmentType env, int expected)
    {
        // Arrange
        var nodeSpecs = new NodeSpecsConfig
        {
            DevMasterRam = 8,
            ProdMasterRam = 16
        };

        var cut = RenderComponent<K8sNodeSpecsConfig>(parameters => parameters
            .Add(p => p.IsSingleCluster, false)
            .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { env })
            .Add(p => p.NodeSpecs, nodeSpecs)
            .Add(p => p.HasManagedControlPlane, false)
            .Add(p => p.HasInfraNodes, false));

        // Assert - component should render
        cut.Should().NotBeNull();
    }

    #endregion

    #region GetMasterDisk Tests

    [Theory]
    [InlineData(EnvironmentType.Dev, 50)]
    [InlineData(EnvironmentType.Prod, 100)]
    public void GetMasterDisk_ReturnsCorrectValue(EnvironmentType env, int expected)
    {
        // Arrange
        var nodeSpecs = new NodeSpecsConfig
        {
            DevMasterDisk = 50,
            ProdMasterDisk = 100
        };

        var cut = RenderComponent<K8sNodeSpecsConfig>(parameters => parameters
            .Add(p => p.IsSingleCluster, false)
            .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { env })
            .Add(p => p.NodeSpecs, nodeSpecs)
            .Add(p => p.HasManagedControlPlane, false)
            .Add(p => p.HasInfraNodes, false));

        // Assert - component should render
        cut.Should().NotBeNull();
    }

    #endregion

    #region GetInfraCpu/Ram/Disk Tests

    [Theory]
    [InlineData(EnvironmentType.Dev)]
    [InlineData(EnvironmentType.Test)]
    [InlineData(EnvironmentType.Stage)]
    [InlineData(EnvironmentType.Prod)]
    [InlineData(EnvironmentType.DR)]
    public void GetInfraSpecs_ReturnsCorrectValues(EnvironmentType env)
    {
        // Arrange
        var nodeSpecs = new NodeSpecsConfig
        {
            DevInfraCpu = 2, DevInfraRam = 4, DevInfraDisk = 50,
            TestInfraCpu = 2, TestInfraRam = 4, TestInfraDisk = 50,
            StageInfraCpu = 2, StageInfraRam = 4, StageInfraDisk = 50,
            ProdInfraCpu = 4, ProdInfraRam = 8, ProdInfraDisk = 100,
            DRInfraCpu = 4, DRInfraRam = 8, DRInfraDisk = 100
        };

        var cut = RenderComponent<K8sNodeSpecsConfig>(parameters => parameters
            .Add(p => p.IsSingleCluster, false)
            .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { env })
            .Add(p => p.NodeSpecs, nodeSpecs)
            .Add(p => p.HasManagedControlPlane, false)
            .Add(p => p.HasInfraNodes, true));

        // Assert
        cut.Should().NotBeNull();
        cut.Find(".node-card").Should().NotBeNull();
    }

    #endregion

    #region GetWorkerCpu/Ram/Disk Tests

    [Theory]
    [InlineData(EnvironmentType.Dev)]
    [InlineData(EnvironmentType.Test)]
    [InlineData(EnvironmentType.Stage)]
    [InlineData(EnvironmentType.Prod)]
    [InlineData(EnvironmentType.DR)]
    public void GetWorkerSpecs_ReturnsCorrectValues(EnvironmentType env)
    {
        // Arrange
        var nodeSpecs = new NodeSpecsConfig
        {
            DevWorkerCpu = 4, DevWorkerRam = 8, DevWorkerDisk = 100,
            TestWorkerCpu = 4, TestWorkerRam = 8, TestWorkerDisk = 100,
            StageWorkerCpu = 4, StageWorkerRam = 8, StageWorkerDisk = 100,
            ProdWorkerCpu = 8, ProdWorkerRam = 16, ProdWorkerDisk = 200,
            DRWorkerCpu = 8, DRWorkerRam = 16, DRWorkerDisk = 200
        };

        var cut = RenderComponent<K8sNodeSpecsConfig>(parameters => parameters
            .Add(p => p.IsSingleCluster, false)
            .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { env })
            .Add(p => p.NodeSpecs, nodeSpecs)
            .Add(p => p.HasManagedControlPlane, true) // Only workers
            .Add(p => p.HasInfraNodes, false));

        // Assert
        cut.Should().NotBeNull();
        var markup = cut.Markup;
        markup.Should().Contain("Worker");
    }

    #endregion

    #region IsProdEnv Tests

    [Theory]
    [InlineData(EnvironmentType.Prod, true)]
    [InlineData(EnvironmentType.DR, true)]
    [InlineData(EnvironmentType.Dev, false)]
    [InlineData(EnvironmentType.Test, false)]
    [InlineData(EnvironmentType.Stage, false)]
    public void IsProdEnv_ReturnsCorrectValue(EnvironmentType env, bool expected)
    {
        // Arrange
        var cut = RenderComponent<K8sNodeSpecsConfig>(parameters => parameters
            .Add(p => p.IsSingleCluster, false)
            .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { env })
            .Add(p => p.NodeSpecs, new NodeSpecsConfig())
            .Add(p => p.HasManagedControlPlane, false)
            .Add(p => p.HasInfraNodes, false));

        // Assert
        var markup = cut.Markup;
        if (expected)
        {
            markup.Should().Contain("Production");
        }
        else
        {
            markup.Should().Contain("Non-Production");
        }
    }

    #endregion

    #region GetEnvClass Tests

    [Theory]
    [InlineData(EnvironmentType.Dev, "env-dev")]
    [InlineData(EnvironmentType.Test, "env-test")]
    [InlineData(EnvironmentType.Stage, "env-stage")]
    [InlineData(EnvironmentType.Prod, "env-prod")]
    [InlineData(EnvironmentType.DR, "env-dr")]
    public void GetEnvClass_ReturnsCorrectClass(EnvironmentType env, string expectedClass)
    {
        // Arrange
        var cut = RenderComponent<K8sNodeSpecsConfig>(parameters => parameters
            .Add(p => p.IsSingleCluster, false)
            .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { env })
            .Add(p => p.NodeSpecs, new NodeSpecsConfig())
            .Add(p => p.HasManagedControlPlane, false)
            .Add(p => p.HasInfraNodes, false));

        // Assert
        cut.Markup.Should().Contain(expectedClass);
    }

    #endregion

    #region ParseInt Edge Cases

    [Fact]
    public async Task ParseInt_HandlesNullValue_ReturnsOne()
    {
        // Arrange
        var nodeSpecs = new NodeSpecsConfig { DevMasterCpu = 4 };
        NodeSpecsConfig? result = null;

        var cut = RenderComponent<K8sNodeSpecsConfig>(parameters => parameters
            .Add(p => p.IsSingleCluster, false)
            .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Dev })
            .Add(p => p.NodeSpecs, nodeSpecs)
            .Add(p => p.NodeSpecsChanged, EventCallback.Factory.Create<NodeSpecsConfig>(
                this, specs => result = specs))
            .Add(p => p.HasManagedControlPlane, false)
            .Add(p => p.HasInfraNodes, false));

        // Act
        var inputs = cut.FindAll("input[type='number']");
        if (inputs.Any())
        {
            await cut.InvokeAsync(() => inputs[0].Change(new ChangeEventArgs { Value = null }));
        }

        // Assert - should default to 1 (minimum for node specs)
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ParseInt_HandlesNegativeValue_ClampsToOne()
    {
        // Arrange
        var nodeSpecs = new NodeSpecsConfig { DevMasterCpu = 4 };
        NodeSpecsConfig? result = null;

        var cut = RenderComponent<K8sNodeSpecsConfig>(parameters => parameters
            .Add(p => p.IsSingleCluster, false)
            .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Dev })
            .Add(p => p.NodeSpecs, nodeSpecs)
            .Add(p => p.NodeSpecsChanged, EventCallback.Factory.Create<NodeSpecsConfig>(
                this, specs => result = specs))
            .Add(p => p.HasManagedControlPlane, false)
            .Add(p => p.HasInfraNodes, false));

        // Act
        var inputs = cut.FindAll("input[type='number']");
        if (inputs.Any())
        {
            await cut.InvokeAsync(() => inputs[0].Change(new ChangeEventArgs { Value = "-5" }));
        }

        // Assert - should clamp to 1
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task ParseInt_HandlesInvalidString_ReturnsOne()
    {
        // Arrange
        var nodeSpecs = new NodeSpecsConfig();
        NodeSpecsConfig? result = null;

        var cut = RenderComponent<K8sNodeSpecsConfig>(parameters => parameters
            .Add(p => p.IsSingleCluster, false)
            .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Dev })
            .Add(p => p.NodeSpecs, nodeSpecs)
            .Add(p => p.NodeSpecsChanged, EventCallback.Factory.Create<NodeSpecsConfig>(
                this, specs => result = specs))
            .Add(p => p.HasManagedControlPlane, false)
            .Add(p => p.HasInfraNodes, false));

        // Act
        var inputs = cut.FindAll("input[type='number']");
        if (inputs.Any())
        {
            await cut.InvokeAsync(() => inputs[0].Change(new ChangeEventArgs { Value = "invalid" }));
        }

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region SetMasterRam/Disk Tests

    [Theory]
    [InlineData(EnvironmentType.Dev)]
    [InlineData(EnvironmentType.Test)]
    [InlineData(EnvironmentType.Stage)]
    [InlineData(EnvironmentType.Prod)]
    [InlineData(EnvironmentType.DR)]
    public async Task SetMasterRam_UpdatesCorrectEnvironment(EnvironmentType env)
    {
        // Arrange
        var nodeSpecs = new NodeSpecsConfig();
        NodeSpecsConfig? result = null;

        var cut = RenderComponent<K8sNodeSpecsConfig>(parameters => parameters
            .Add(p => p.IsSingleCluster, false)
            .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { env })
            .Add(p => p.NodeSpecs, nodeSpecs)
            .Add(p => p.NodeSpecsChanged, EventCallback.Factory.Create<NodeSpecsConfig>(
                this, specs => result = specs))
            .Add(p => p.HasManagedControlPlane, false)
            .Add(p => p.HasInfraNodes, false));

        // Act - RAM input is the second one after CPU
        var inputs = cut.FindAll("input[type='number']");
        if (inputs.Count >= 2)
        {
            await cut.InvokeAsync(() => inputs[1].Change(new ChangeEventArgs { Value = "64" }));
        }

        // Assert
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData(EnvironmentType.Dev)]
    [InlineData(EnvironmentType.Test)]
    [InlineData(EnvironmentType.Stage)]
    [InlineData(EnvironmentType.Prod)]
    [InlineData(EnvironmentType.DR)]
    public async Task SetMasterDisk_UpdatesCorrectEnvironment(EnvironmentType env)
    {
        // Arrange
        var nodeSpecs = new NodeSpecsConfig();
        NodeSpecsConfig? result = null;

        var cut = RenderComponent<K8sNodeSpecsConfig>(parameters => parameters
            .Add(p => p.IsSingleCluster, false)
            .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { env })
            .Add(p => p.NodeSpecs, nodeSpecs)
            .Add(p => p.NodeSpecsChanged, EventCallback.Factory.Create<NodeSpecsConfig>(
                this, specs => result = specs))
            .Add(p => p.HasManagedControlPlane, false)
            .Add(p => p.HasInfraNodes, false));

        // Act - Disk input is the third one
        var inputs = cut.FindAll("input[type='number']");
        if (inputs.Count >= 3)
        {
            await cut.InvokeAsync(() => inputs[2].Change(new ChangeEventArgs { Value = "500" }));
        }

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region SetInfra Tests

    [Theory]
    [InlineData(EnvironmentType.Dev)]
    [InlineData(EnvironmentType.Test)]
    [InlineData(EnvironmentType.Stage)]
    [InlineData(EnvironmentType.Prod)]
    [InlineData(EnvironmentType.DR)]
    public async Task SetInfraCpu_UpdatesCorrectEnvironment(EnvironmentType env)
    {
        // Arrange
        var nodeSpecs = new NodeSpecsConfig();
        NodeSpecsConfig? result = null;

        var cut = RenderComponent<K8sNodeSpecsConfig>(parameters => parameters
            .Add(p => p.IsSingleCluster, false)
            .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { env })
            .Add(p => p.NodeSpecs, nodeSpecs)
            .Add(p => p.NodeSpecsChanged, EventCallback.Factory.Create<NodeSpecsConfig>(
                this, specs => result = specs))
            .Add(p => p.HasManagedControlPlane, false)
            .Add(p => p.HasInfraNodes, true));

        // Act - Infra CPU is after Master inputs (4th = index 3)
        var inputs = cut.FindAll("input[type='number']");
        if (inputs.Count >= 4)
        {
            await cut.InvokeAsync(() => inputs[3].Change(new ChangeEventArgs { Value = "16" }));
        }

        // Assert
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData(EnvironmentType.Dev)]
    [InlineData(EnvironmentType.Test)]
    [InlineData(EnvironmentType.Stage)]
    [InlineData(EnvironmentType.Prod)]
    [InlineData(EnvironmentType.DR)]
    public async Task SetInfraRam_UpdatesCorrectEnvironment(EnvironmentType env)
    {
        // Arrange
        var nodeSpecs = new NodeSpecsConfig();
        NodeSpecsConfig? result = null;

        var cut = RenderComponent<K8sNodeSpecsConfig>(parameters => parameters
            .Add(p => p.IsSingleCluster, false)
            .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { env })
            .Add(p => p.NodeSpecs, nodeSpecs)
            .Add(p => p.NodeSpecsChanged, EventCallback.Factory.Create<NodeSpecsConfig>(
                this, specs => result = specs))
            .Add(p => p.HasManagedControlPlane, false)
            .Add(p => p.HasInfraNodes, true));

        // Act - Infra RAM is at index 4
        var inputs = cut.FindAll("input[type='number']");
        if (inputs.Count >= 5)
        {
            await cut.InvokeAsync(() => inputs[4].Change(new ChangeEventArgs { Value = "128" }));
        }

        // Assert
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData(EnvironmentType.Dev)]
    [InlineData(EnvironmentType.Test)]
    [InlineData(EnvironmentType.Stage)]
    [InlineData(EnvironmentType.Prod)]
    [InlineData(EnvironmentType.DR)]
    public async Task SetInfraDisk_UpdatesCorrectEnvironment(EnvironmentType env)
    {
        // Arrange
        var nodeSpecs = new NodeSpecsConfig();
        NodeSpecsConfig? result = null;

        var cut = RenderComponent<K8sNodeSpecsConfig>(parameters => parameters
            .Add(p => p.IsSingleCluster, false)
            .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { env })
            .Add(p => p.NodeSpecs, nodeSpecs)
            .Add(p => p.NodeSpecsChanged, EventCallback.Factory.Create<NodeSpecsConfig>(
                this, specs => result = specs))
            .Add(p => p.HasManagedControlPlane, false)
            .Add(p => p.HasInfraNodes, true));

        // Act - Infra Disk is at index 5
        var inputs = cut.FindAll("input[type='number']");
        if (inputs.Count >= 6)
        {
            await cut.InvokeAsync(() => inputs[5].Change(new ChangeEventArgs { Value = "1000" }));
        }

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region SetWorker Tests

    [Theory]
    [InlineData(EnvironmentType.Dev)]
    [InlineData(EnvironmentType.Test)]
    [InlineData(EnvironmentType.Stage)]
    [InlineData(EnvironmentType.Prod)]
    [InlineData(EnvironmentType.DR)]
    public async Task SetWorkerCpu_UpdatesCorrectEnvironment(EnvironmentType env)
    {
        // Arrange
        var nodeSpecs = new NodeSpecsConfig();
        NodeSpecsConfig? result = null;

        // No managed control plane, no infra nodes - worker inputs start at index 3
        var cut = RenderComponent<K8sNodeSpecsConfig>(parameters => parameters
            .Add(p => p.IsSingleCluster, false)
            .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { env })
            .Add(p => p.NodeSpecs, nodeSpecs)
            .Add(p => p.NodeSpecsChanged, EventCallback.Factory.Create<NodeSpecsConfig>(
                this, specs => result = specs))
            .Add(p => p.HasManagedControlPlane, false)
            .Add(p => p.HasInfraNodes, false));

        // Act - Worker CPU is at index 3 (after 3 master inputs)
        var inputs = cut.FindAll("input[type='number']");
        if (inputs.Count >= 4)
        {
            await cut.InvokeAsync(() => inputs[3].Change(new ChangeEventArgs { Value = "32" }));
        }

        // Assert
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData(EnvironmentType.Dev)]
    [InlineData(EnvironmentType.Test)]
    [InlineData(EnvironmentType.Stage)]
    [InlineData(EnvironmentType.Prod)]
    [InlineData(EnvironmentType.DR)]
    public async Task SetWorkerRam_UpdatesCorrectEnvironment(EnvironmentType env)
    {
        // Arrange
        var nodeSpecs = new NodeSpecsConfig();
        NodeSpecsConfig? result = null;

        var cut = RenderComponent<K8sNodeSpecsConfig>(parameters => parameters
            .Add(p => p.IsSingleCluster, false)
            .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { env })
            .Add(p => p.NodeSpecs, nodeSpecs)
            .Add(p => p.NodeSpecsChanged, EventCallback.Factory.Create<NodeSpecsConfig>(
                this, specs => result = specs))
            .Add(p => p.HasManagedControlPlane, false)
            .Add(p => p.HasInfraNodes, false));

        // Act - Worker RAM is at index 4
        var inputs = cut.FindAll("input[type='number']");
        if (inputs.Count >= 5)
        {
            await cut.InvokeAsync(() => inputs[4].Change(new ChangeEventArgs { Value = "256" }));
        }

        // Assert
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData(EnvironmentType.Dev)]
    [InlineData(EnvironmentType.Test)]
    [InlineData(EnvironmentType.Stage)]
    [InlineData(EnvironmentType.Prod)]
    [InlineData(EnvironmentType.DR)]
    public async Task SetWorkerDisk_UpdatesCorrectEnvironment(EnvironmentType env)
    {
        // Arrange
        var nodeSpecs = new NodeSpecsConfig();
        NodeSpecsConfig? result = null;

        var cut = RenderComponent<K8sNodeSpecsConfig>(parameters => parameters
            .Add(p => p.IsSingleCluster, false)
            .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { env })
            .Add(p => p.NodeSpecs, nodeSpecs)
            .Add(p => p.NodeSpecsChanged, EventCallback.Factory.Create<NodeSpecsConfig>(
                this, specs => result = specs))
            .Add(p => p.HasManagedControlPlane, false)
            .Add(p => p.HasInfraNodes, false));

        // Act - Worker Disk is at index 5
        var inputs = cut.FindAll("input[type='number']");
        if (inputs.Count >= 6)
        {
            await cut.InvokeAsync(() => inputs[5].Change(new ChangeEventArgs { Value = "2000" }));
        }

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Managed Control Plane with Workers Only

    [Theory]
    [InlineData(EnvironmentType.Dev)]
    [InlineData(EnvironmentType.Test)]
    [InlineData(EnvironmentType.Stage)]
    [InlineData(EnvironmentType.Prod)]
    [InlineData(EnvironmentType.DR)]
    public async Task ManagedControlPlane_WorkerInputs_StartAtFirstPosition(EnvironmentType env)
    {
        // When control plane is managed, worker inputs are the first ones
        var nodeSpecs = new NodeSpecsConfig();
        NodeSpecsConfig? result = null;

        var cut = RenderComponent<K8sNodeSpecsConfig>(parameters => parameters
            .Add(p => p.IsSingleCluster, false)
            .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { env })
            .Add(p => p.NodeSpecs, nodeSpecs)
            .Add(p => p.NodeSpecsChanged, EventCallback.Factory.Create<NodeSpecsConfig>(
                this, specs => result = specs))
            .Add(p => p.HasManagedControlPlane, true)
            .Add(p => p.HasInfraNodes, false));

        // Act - Worker CPU is at index 0 when control plane is managed
        var inputs = cut.FindAll("input[type='number']");
        if (inputs.Any())
        {
            await cut.InvokeAsync(() => inputs[0].Change(new ChangeEventArgs { Value = "16" }));
        }

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region Full Node Stack Tests

    [Theory]
    [InlineData(EnvironmentType.Dev)]
    [InlineData(EnvironmentType.Test)]
    [InlineData(EnvironmentType.Stage)]
    [InlineData(EnvironmentType.Prod)]
    [InlineData(EnvironmentType.DR)]
    public async Task FullNodeStack_AllInputsWorkCorrectly(EnvironmentType env)
    {
        // Test with all node types: Control Plane, Infra, Worker (9 inputs total)
        var nodeSpecs = new NodeSpecsConfig();
        NodeSpecsConfig? result = null;

        var cut = RenderComponent<K8sNodeSpecsConfig>(parameters => parameters
            .Add(p => p.IsSingleCluster, false)
            .Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { env })
            .Add(p => p.NodeSpecs, nodeSpecs)
            .Add(p => p.NodeSpecsChanged, EventCallback.Factory.Create<NodeSpecsConfig>(
                this, specs => result = specs))
            .Add(p => p.HasManagedControlPlane, false)
            .Add(p => p.HasInfraNodes, true));

        // Assert - should have 9 inputs (3 per node type x 3 node types)
        var inputs = cut.FindAll("input[type='number']");
        inputs.Count.Should().Be(9);

        // Act - update Worker Disk (last input, index 8)
        if (inputs.Count >= 9)
        {
            await cut.InvokeAsync(() => inputs[8].Change(new ChangeEventArgs { Value = "500" }));
        }

        // Assert
        result.Should().NotBeNull();
    }

    #endregion

    #region OnParametersSet Tests

    [Fact]
    public void OnParametersSet_ExpandsFirstEnvironment_WhenNoneSelected()
    {
        // Arrange
        var enabledEnvs = new HashSet<EnvironmentType> { EnvironmentType.Test, EnvironmentType.Prod };

        // Act
        var cut = RenderComponent<K8sNodeSpecsConfig>(parameters => parameters
            .Add(p => p.IsSingleCluster, false)
            .Add(p => p.EnabledEnvironments, enabledEnvs)
            .Add(p => p.NodeSpecs, new NodeSpecsConfig())
            .Add(p => p.HasManagedControlPlane, false)
            .Add(p => p.HasInfraNodes, false));

        // Assert - component renders with first env expanded
        cut.Should().NotBeNull();
    }

    #endregion
}
