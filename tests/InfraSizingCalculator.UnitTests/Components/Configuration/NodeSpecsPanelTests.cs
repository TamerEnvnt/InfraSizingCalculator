using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.Configuration;
using InfraSizingCalculator.Models;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components.Configuration;

/// <summary>
/// Tests for NodeSpecsPanel component - Node specifications for K8s configuration
/// </summary>
public class NodeSpecsPanelTests : TestContext
{
    #region Rendering Tests

    [Fact]
    public void NodeSpecsPanel_RendersContainer()
    {
        // Act
        var cut = RenderComponent<NodeSpecsPanel>();

        // Assert
        cut.Find(".node-specs-panel").Should().NotBeNull();
    }

    [Fact]
    public void NodeSpecsPanel_AppliesAdditionalClass()
    {
        // Act
        var cut = RenderComponent<NodeSpecsPanel>(parameters => parameters
            .Add(p => p.AdditionalClass, "custom-class"));

        // Assert
        cut.Find(".node-specs-panel").ClassList.Should().Contain("custom-class");
    }

    [Fact]
    public void NodeSpecsPanel_RendersControlPlaneSection()
    {
        // Act
        var cut = RenderComponent<NodeSpecsPanel>();

        // Assert
        var sections = cut.FindAll(".specs-section h4");
        sections.Should().Contain(h => h.TextContent.Contains("Control Plane"));
    }

    [Fact]
    public void NodeSpecsPanel_RendersWorkerNodesSection()
    {
        // Act
        var cut = RenderComponent<NodeSpecsPanel>();

        // Assert
        var sections = cut.FindAll(".specs-section h4");
        sections.Should().Contain(h => h.TextContent.Contains("Worker Nodes"));
    }

    [Fact]
    public void NodeSpecsPanel_RendersInfraSection_WhenShowInfraSpecsTrue()
    {
        // Act
        var cut = RenderComponent<NodeSpecsPanel>(parameters => parameters
            .Add(p => p.ShowInfraSpecs, true));

        // Assert
        var sections = cut.FindAll(".specs-section h4");
        sections.Should().Contain(h => h.TextContent.Contains("Infrastructure"));
    }

    [Fact]
    public void NodeSpecsPanel_HidesInfraSection_WhenShowInfraSpecsFalse()
    {
        // Act
        var cut = RenderComponent<NodeSpecsPanel>(parameters => parameters
            .Add(p => p.ShowInfraSpecs, false));

        // Assert
        var sections = cut.FindAll(".specs-section h4");
        sections.Should().NotContain(h => h.TextContent.Contains("Infrastructure"));
    }

    #endregion

    #region Spec Group Tests

    [Fact]
    public void NodeSpecsPanel_RendersProductionAndNonProductionGroups()
    {
        // Act
        var cut = RenderComponent<NodeSpecsPanel>();

        // Assert
        var groups = cut.FindAll(".specs-group label");
        groups.Should().Contain(l => l.TextContent.Contains("Production"));
        groups.Should().Contain(l => l.TextContent.Contains("Non-Production"));
    }

    [Fact]
    public void NodeSpecsPanel_RendersCpuRamDiskInputs()
    {
        // Act
        var cut = RenderComponent<NodeSpecsPanel>();

        // Assert
        var inlineSpecs = cut.Find(".inline-specs");
        var labels = inlineSpecs.QuerySelectorAll("label");
        labels.Should().Contain(l => l.TextContent == "CPU");
        labels.Should().Contain(l => l.TextContent == "RAM");
        labels.Should().Contain(l => l.TextContent == "Disk");
    }

    [Fact]
    public void NodeSpecsPanel_HasNumberInputsForEachSpec()
    {
        // Act
        var cut = RenderComponent<NodeSpecsPanel>();

        // Assert
        var inputs = cut.FindAll("input[type='number']");
        // 2 sections (control plane, workers) x 2 groups (prod, nonprod) x 3 specs (cpu, ram, disk) = 12
        // Plus infra section when shown: 6 more
        inputs.Should().HaveCountGreaterThanOrEqualTo(12);
    }

    #endregion

    #region Spec Values Tests

    [Fact]
    public void NodeSpecsPanel_DisplaysControlPlaneSpecs()
    {
        // Arrange
        var specs = new NodeSpecsConfig
        {
            ProdMasterCpu = 8,
            ProdMasterRam = 32,
            ProdMasterDisk = 200
        };

        // Act
        var cut = RenderComponent<NodeSpecsPanel>(parameters => parameters
            .Add(p => p.NodeSpecs, specs));

        // Assert
        var inputs = cut.FindAll("input[type='number']");
        inputs[0].GetAttribute("value").Should().Be("8");
        inputs[1].GetAttribute("value").Should().Be("32");
        inputs[2].GetAttribute("value").Should().Be("200");
    }

    [Fact]
    public void NodeSpecsPanel_DisplaysNonProdControlPlaneSpecs()
    {
        // Arrange
        var specs = new NodeSpecsConfig
        {
            NonProdMasterCpu = 4,
            NonProdMasterRam = 16,
            NonProdMasterDisk = 100
        };

        // Act
        var cut = RenderComponent<NodeSpecsPanel>(parameters => parameters
            .Add(p => p.NodeSpecs, specs));

        // Assert
        var inputs = cut.FindAll("input[type='number']");
        inputs[3].GetAttribute("value").Should().Be("4");
        inputs[4].GetAttribute("value").Should().Be("16");
        inputs[5].GetAttribute("value").Should().Be("100");
    }

    [Fact]
    public void NodeSpecsPanel_DisplaysWorkerSpecs()
    {
        // Arrange
        var specs = new NodeSpecsConfig
        {
            ProdWorkerCpu = 16,
            ProdWorkerRam = 64,
            ProdWorkerDisk = 500
        };

        // Act
        var cut = RenderComponent<NodeSpecsPanel>(parameters => parameters
            .Add(p => p.NodeSpecs, specs)
            .Add(p => p.ShowInfraSpecs, false));

        // Assert - Worker section is after control plane
        var inputs = cut.FindAll("input[type='number']");
        // Control plane: 6 inputs (0-5), Workers: 6 inputs (6-11)
        inputs[6].GetAttribute("value").Should().Be("16");
        inputs[7].GetAttribute("value").Should().Be("64");
        inputs[8].GetAttribute("value").Should().Be("500");
    }

    [Fact]
    public void NodeSpecsPanel_DisplaysInfraSpecs_WhenShown()
    {
        // Arrange
        var specs = new NodeSpecsConfig
        {
            ProdInfraCpu = 12,
            ProdInfraRam = 48,
            ProdInfraDisk = 300
        };

        // Act
        var cut = RenderComponent<NodeSpecsPanel>(parameters => parameters
            .Add(p => p.NodeSpecs, specs)
            .Add(p => p.ShowInfraSpecs, true));

        // Assert - Infra section is between control plane and workers
        var inputs = cut.FindAll("input[type='number']");
        // Control plane: 6 inputs (0-5), Infra: 6 inputs (6-11), Workers: 6 inputs (12-17)
        inputs[6].GetAttribute("value").Should().Be("12");
        inputs[7].GetAttribute("value").Should().Be("48");
        inputs[8].GetAttribute("value").Should().Be("300");
    }

    #endregion

    #region Callback Tests

    [Fact]
    public async Task NodeSpecsPanel_ChangingSpec_InvokesOnSpecChangedCallback()
    {
        // Arrange
        (string property, int value)? changedSpec = null;
        var specs = new NodeSpecsConfig();
        var cut = RenderComponent<NodeSpecsPanel>(parameters => parameters
            .Add(p => p.NodeSpecs, specs)
            .Add(p => p.OnSpecChanged, EventCallback.Factory.Create<(string, int)>(this, s => changedSpec = s)));

        // Act
        var cpuInput = cut.FindAll("input[type='number']")[0];
        await cut.InvokeAsync(() => cpuInput.Change(16));

        // Assert
        changedSpec.Should().NotBeNull();
        changedSpec!.Value.property.Should().Be("ProdMasterCpu");
        changedSpec!.Value.value.Should().Be(16);
    }

    [Fact]
    public async Task NodeSpecsPanel_ChangingNonProdSpec_PassesCorrectProperty()
    {
        // Arrange
        (string property, int value)? changedSpec = null;
        var specs = new NodeSpecsConfig();
        var cut = RenderComponent<NodeSpecsPanel>(parameters => parameters
            .Add(p => p.NodeSpecs, specs)
            .Add(p => p.OnSpecChanged, EventCallback.Factory.Create<(string, int)>(this, s => changedSpec = s)));

        // Act - Change NonProd Master CPU (index 3)
        var cpuInput = cut.FindAll("input[type='number']")[3];
        await cut.InvokeAsync(() => cpuInput.Change(8));

        // Assert
        changedSpec.Should().NotBeNull();
        changedSpec!.Value.property.Should().Be("NonProdMasterCpu");
        changedSpec!.Value.value.Should().Be(8);
    }

    [Fact]
    public async Task NodeSpecsPanel_NoCallback_DoesNotThrow()
    {
        // Arrange
        var cut = RenderComponent<NodeSpecsPanel>();

        // Act & Assert
        var action = async () =>
        {
            var cpuInput = cut.FindAll("input[type='number']")[0];
            await cut.InvokeAsync(() => cpuInput.Change(16));
        };

        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task NodeSpecsPanel_InvalidValue_ParsesAsZero()
    {
        // Arrange
        (string property, int value)? changedSpec = null;
        var specs = new NodeSpecsConfig();
        var cut = RenderComponent<NodeSpecsPanel>(parameters => parameters
            .Add(p => p.NodeSpecs, specs)
            .Add(p => p.OnSpecChanged, EventCallback.Factory.Create<(string, int)>(this, s => changedSpec = s)));

        // Act
        var cpuInput = cut.FindAll("input[type='number']")[0];
        await cut.InvokeAsync(() => cpuInput.Change("invalid"));

        // Assert
        changedSpec.Should().NotBeNull();
        changedSpec!.Value.value.Should().Be(0);
    }

    #endregion

    #region Parameter Update Tests

    [Fact]
    public void NodeSpecsPanel_UpdatesDisplay_WhenSpecsChange()
    {
        // Arrange
        var specs = new NodeSpecsConfig { ProdMasterCpu = 4 };
        var cut = RenderComponent<NodeSpecsPanel>(parameters => parameters
            .Add(p => p.NodeSpecs, specs));

        // Assert initial
        var input = cut.FindAll("input[type='number']")[0];
        input.GetAttribute("value").Should().Be("4");

        // Act
        specs.ProdMasterCpu = 16;
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.NodeSpecs, specs));

        // Assert
        input = cut.FindAll("input[type='number']")[0];
        input.GetAttribute("value").Should().Be("16");
    }

    [Fact]
    public void NodeSpecsPanel_TogglingInfraSpecs_UpdatesSections()
    {
        // Arrange
        var cut = RenderComponent<NodeSpecsPanel>(parameters => parameters
            .Add(p => p.ShowInfraSpecs, true));

        // Assert initial
        cut.FindAll(".specs-section h4").Should().HaveCount(3);

        // Act
        cut.SetParametersAndRender(parameters => parameters
            .Add(p => p.ShowInfraSpecs, false));

        // Assert
        cut.FindAll(".specs-section h4").Should().HaveCount(2);
    }

    #endregion

    #region Extended Input Change Tests

    [Fact]
    public async Task NodeSpecsPanel_ChangingProdMasterRam_PassesCorrectProperty()
    {
        // Arrange
        (string property, int value)? changedSpec = null;
        var cut = RenderComponent<NodeSpecsPanel>(parameters => parameters
            .Add(p => p.NodeSpecs, new NodeSpecsConfig())
            .Add(p => p.OnSpecChanged, EventCallback.Factory.Create<(string, int)>(this, s => changedSpec = s)));

        // Act - ProdMasterRam is index 1
        var ramInput = cut.FindAll("input[type='number']")[1];
        await cut.InvokeAsync(() => ramInput.Change(64));

        // Assert
        changedSpec.Should().NotBeNull();
        changedSpec!.Value.property.Should().Be("ProdMasterRam");
        changedSpec!.Value.value.Should().Be(64);
    }

    [Fact]
    public async Task NodeSpecsPanel_ChangingProdMasterDisk_PassesCorrectProperty()
    {
        // Arrange
        (string property, int value)? changedSpec = null;
        var cut = RenderComponent<NodeSpecsPanel>(parameters => parameters
            .Add(p => p.NodeSpecs, new NodeSpecsConfig())
            .Add(p => p.OnSpecChanged, EventCallback.Factory.Create<(string, int)>(this, s => changedSpec = s)));

        // Act - ProdMasterDisk is index 2
        var diskInput = cut.FindAll("input[type='number']")[2];
        await cut.InvokeAsync(() => diskInput.Change(500));

        // Assert
        changedSpec.Should().NotBeNull();
        changedSpec!.Value.property.Should().Be("ProdMasterDisk");
        changedSpec!.Value.value.Should().Be(500);
    }

    [Fact]
    public async Task NodeSpecsPanel_ChangingNonProdMasterRam_PassesCorrectProperty()
    {
        // Arrange
        (string property, int value)? changedSpec = null;
        var cut = RenderComponent<NodeSpecsPanel>(parameters => parameters
            .Add(p => p.NodeSpecs, new NodeSpecsConfig())
            .Add(p => p.OnSpecChanged, EventCallback.Factory.Create<(string, int)>(this, s => changedSpec = s)));

        // Act - NonProdMasterRam is index 4
        var ramInput = cut.FindAll("input[type='number']")[4];
        await cut.InvokeAsync(() => ramInput.Change(32));

        // Assert
        changedSpec.Should().NotBeNull();
        changedSpec!.Value.property.Should().Be("NonProdMasterRam");
        changedSpec!.Value.value.Should().Be(32);
    }

    [Fact]
    public async Task NodeSpecsPanel_ChangingNonProdMasterDisk_PassesCorrectProperty()
    {
        // Arrange
        (string property, int value)? changedSpec = null;
        var cut = RenderComponent<NodeSpecsPanel>(parameters => parameters
            .Add(p => p.NodeSpecs, new NodeSpecsConfig())
            .Add(p => p.OnSpecChanged, EventCallback.Factory.Create<(string, int)>(this, s => changedSpec = s)));

        // Act - NonProdMasterDisk is index 5
        var diskInput = cut.FindAll("input[type='number']")[5];
        await cut.InvokeAsync(() => diskInput.Change(200));

        // Assert
        changedSpec.Should().NotBeNull();
        changedSpec!.Value.property.Should().Be("NonProdMasterDisk");
        changedSpec!.Value.value.Should().Be(200);
    }

    #endregion

    #region Infra Spec Change Tests

    [Fact]
    public async Task NodeSpecsPanel_ChangingProdInfraCpu_PassesCorrectProperty()
    {
        // Arrange
        (string property, int value)? changedSpec = null;
        var cut = RenderComponent<NodeSpecsPanel>(parameters => parameters
            .Add(p => p.NodeSpecs, new NodeSpecsConfig())
            .Add(p => p.ShowInfraSpecs, true)
            .Add(p => p.OnSpecChanged, EventCallback.Factory.Create<(string, int)>(this, s => changedSpec = s)));

        // Act - ProdInfraCpu is index 6 (after control plane)
        var cpuInput = cut.FindAll("input[type='number']")[6];
        await cut.InvokeAsync(() => cpuInput.Change(12));

        // Assert
        changedSpec.Should().NotBeNull();
        changedSpec!.Value.property.Should().Be("ProdInfraCpu");
        changedSpec!.Value.value.Should().Be(12);
    }

    [Fact]
    public async Task NodeSpecsPanel_ChangingProdInfraRam_PassesCorrectProperty()
    {
        // Arrange
        (string property, int value)? changedSpec = null;
        var cut = RenderComponent<NodeSpecsPanel>(parameters => parameters
            .Add(p => p.NodeSpecs, new NodeSpecsConfig())
            .Add(p => p.ShowInfraSpecs, true)
            .Add(p => p.OnSpecChanged, EventCallback.Factory.Create<(string, int)>(this, s => changedSpec = s)));

        // Act - ProdInfraRam is index 7
        var ramInput = cut.FindAll("input[type='number']")[7];
        await cut.InvokeAsync(() => ramInput.Change(48));

        // Assert
        changedSpec.Should().NotBeNull();
        changedSpec!.Value.property.Should().Be("ProdInfraRam");
        changedSpec!.Value.value.Should().Be(48);
    }

    [Fact]
    public async Task NodeSpecsPanel_ChangingProdInfraDisk_PassesCorrectProperty()
    {
        // Arrange
        (string property, int value)? changedSpec = null;
        var cut = RenderComponent<NodeSpecsPanel>(parameters => parameters
            .Add(p => p.NodeSpecs, new NodeSpecsConfig())
            .Add(p => p.ShowInfraSpecs, true)
            .Add(p => p.OnSpecChanged, EventCallback.Factory.Create<(string, int)>(this, s => changedSpec = s)));

        // Act - ProdInfraDisk is index 8
        var diskInput = cut.FindAll("input[type='number']")[8];
        await cut.InvokeAsync(() => diskInput.Change(300));

        // Assert
        changedSpec.Should().NotBeNull();
        changedSpec!.Value.property.Should().Be("ProdInfraDisk");
        changedSpec!.Value.value.Should().Be(300);
    }

    [Fact]
    public async Task NodeSpecsPanel_ChangingNonProdInfraCpu_PassesCorrectProperty()
    {
        // Arrange
        (string property, int value)? changedSpec = null;
        var cut = RenderComponent<NodeSpecsPanel>(parameters => parameters
            .Add(p => p.NodeSpecs, new NodeSpecsConfig())
            .Add(p => p.ShowInfraSpecs, true)
            .Add(p => p.OnSpecChanged, EventCallback.Factory.Create<(string, int)>(this, s => changedSpec = s)));

        // Act - NonProdInfraCpu is index 9
        var cpuInput = cut.FindAll("input[type='number']")[9];
        await cut.InvokeAsync(() => cpuInput.Change(6));

        // Assert
        changedSpec.Should().NotBeNull();
        changedSpec!.Value.property.Should().Be("NonProdInfraCpu");
        changedSpec!.Value.value.Should().Be(6);
    }

    [Fact]
    public async Task NodeSpecsPanel_ChangingNonProdInfraRam_PassesCorrectProperty()
    {
        // Arrange
        (string property, int value)? changedSpec = null;
        var cut = RenderComponent<NodeSpecsPanel>(parameters => parameters
            .Add(p => p.NodeSpecs, new NodeSpecsConfig())
            .Add(p => p.ShowInfraSpecs, true)
            .Add(p => p.OnSpecChanged, EventCallback.Factory.Create<(string, int)>(this, s => changedSpec = s)));

        // Act - NonProdInfraRam is index 10
        var ramInput = cut.FindAll("input[type='number']")[10];
        await cut.InvokeAsync(() => ramInput.Change(24));

        // Assert
        changedSpec.Should().NotBeNull();
        changedSpec!.Value.property.Should().Be("NonProdInfraRam");
        changedSpec!.Value.value.Should().Be(24);
    }

    [Fact]
    public async Task NodeSpecsPanel_ChangingNonProdInfraDisk_PassesCorrectProperty()
    {
        // Arrange
        (string property, int value)? changedSpec = null;
        var cut = RenderComponent<NodeSpecsPanel>(parameters => parameters
            .Add(p => p.NodeSpecs, new NodeSpecsConfig())
            .Add(p => p.ShowInfraSpecs, true)
            .Add(p => p.OnSpecChanged, EventCallback.Factory.Create<(string, int)>(this, s => changedSpec = s)));

        // Act - NonProdInfraDisk is index 11
        var diskInput = cut.FindAll("input[type='number']")[11];
        await cut.InvokeAsync(() => diskInput.Change(150));

        // Assert
        changedSpec.Should().NotBeNull();
        changedSpec!.Value.property.Should().Be("NonProdInfraDisk");
        changedSpec!.Value.value.Should().Be(150);
    }

    #endregion

    #region Worker Spec Change Tests

    [Fact]
    public async Task NodeSpecsPanel_ChangingProdWorkerCpu_PassesCorrectProperty()
    {
        // Arrange
        (string property, int value)? changedSpec = null;
        var cut = RenderComponent<NodeSpecsPanel>(parameters => parameters
            .Add(p => p.NodeSpecs, new NodeSpecsConfig())
            .Add(p => p.ShowInfraSpecs, false)
            .Add(p => p.OnSpecChanged, EventCallback.Factory.Create<(string, int)>(this, s => changedSpec = s)));

        // Act - ProdWorkerCpu is index 6 when infra hidden
        var cpuInput = cut.FindAll("input[type='number']")[6];
        await cut.InvokeAsync(() => cpuInput.Change(32));

        // Assert
        changedSpec.Should().NotBeNull();
        changedSpec!.Value.property.Should().Be("ProdWorkerCpu");
        changedSpec!.Value.value.Should().Be(32);
    }

    [Fact]
    public async Task NodeSpecsPanel_ChangingProdWorkerRam_PassesCorrectProperty()
    {
        // Arrange
        (string property, int value)? changedSpec = null;
        var cut = RenderComponent<NodeSpecsPanel>(parameters => parameters
            .Add(p => p.NodeSpecs, new NodeSpecsConfig())
            .Add(p => p.ShowInfraSpecs, false)
            .Add(p => p.OnSpecChanged, EventCallback.Factory.Create<(string, int)>(this, s => changedSpec = s)));

        // Act - ProdWorkerRam is index 7 when infra hidden
        var ramInput = cut.FindAll("input[type='number']")[7];
        await cut.InvokeAsync(() => ramInput.Change(128));

        // Assert
        changedSpec.Should().NotBeNull();
        changedSpec!.Value.property.Should().Be("ProdWorkerRam");
        changedSpec!.Value.value.Should().Be(128);
    }

    [Fact]
    public async Task NodeSpecsPanel_ChangingProdWorkerDisk_PassesCorrectProperty()
    {
        // Arrange
        (string property, int value)? changedSpec = null;
        var cut = RenderComponent<NodeSpecsPanel>(parameters => parameters
            .Add(p => p.NodeSpecs, new NodeSpecsConfig())
            .Add(p => p.ShowInfraSpecs, false)
            .Add(p => p.OnSpecChanged, EventCallback.Factory.Create<(string, int)>(this, s => changedSpec = s)));

        // Act - ProdWorkerDisk is index 8 when infra hidden
        var diskInput = cut.FindAll("input[type='number']")[8];
        await cut.InvokeAsync(() => diskInput.Change(1000));

        // Assert
        changedSpec.Should().NotBeNull();
        changedSpec!.Value.property.Should().Be("ProdWorkerDisk");
        changedSpec!.Value.value.Should().Be(1000);
    }

    [Fact]
    public async Task NodeSpecsPanel_ChangingNonProdWorkerCpu_PassesCorrectProperty()
    {
        // Arrange
        (string property, int value)? changedSpec = null;
        var cut = RenderComponent<NodeSpecsPanel>(parameters => parameters
            .Add(p => p.NodeSpecs, new NodeSpecsConfig())
            .Add(p => p.ShowInfraSpecs, false)
            .Add(p => p.OnSpecChanged, EventCallback.Factory.Create<(string, int)>(this, s => changedSpec = s)));

        // Act - NonProdWorkerCpu is index 9 when infra hidden
        var cpuInput = cut.FindAll("input[type='number']")[9];
        await cut.InvokeAsync(() => cpuInput.Change(8));

        // Assert
        changedSpec.Should().NotBeNull();
        changedSpec!.Value.property.Should().Be("NonProdWorkerCpu");
        changedSpec!.Value.value.Should().Be(8);
    }

    [Fact]
    public async Task NodeSpecsPanel_ChangingNonProdWorkerRam_PassesCorrectProperty()
    {
        // Arrange
        (string property, int value)? changedSpec = null;
        var cut = RenderComponent<NodeSpecsPanel>(parameters => parameters
            .Add(p => p.NodeSpecs, new NodeSpecsConfig())
            .Add(p => p.ShowInfraSpecs, false)
            .Add(p => p.OnSpecChanged, EventCallback.Factory.Create<(string, int)>(this, s => changedSpec = s)));

        // Act - NonProdWorkerRam is index 10 when infra hidden
        var ramInput = cut.FindAll("input[type='number']")[10];
        await cut.InvokeAsync(() => ramInput.Change(32));

        // Assert
        changedSpec.Should().NotBeNull();
        changedSpec!.Value.property.Should().Be("NonProdWorkerRam");
        changedSpec!.Value.value.Should().Be(32);
    }

    [Fact]
    public async Task NodeSpecsPanel_ChangingNonProdWorkerDisk_PassesCorrectProperty()
    {
        // Arrange
        (string property, int value)? changedSpec = null;
        var cut = RenderComponent<NodeSpecsPanel>(parameters => parameters
            .Add(p => p.NodeSpecs, new NodeSpecsConfig())
            .Add(p => p.ShowInfraSpecs, false)
            .Add(p => p.OnSpecChanged, EventCallback.Factory.Create<(string, int)>(this, s => changedSpec = s)));

        // Act - NonProdWorkerDisk is index 11 when infra hidden
        var diskInput = cut.FindAll("input[type='number']")[11];
        await cut.InvokeAsync(() => diskInput.Change(250));

        // Assert
        changedSpec.Should().NotBeNull();
        changedSpec!.Value.property.Should().Be("NonProdWorkerDisk");
        changedSpec!.Value.value.Should().Be(250);
    }

    #endregion

    #region ParseInt Edge Case Tests

    [Fact]
    public async Task NodeSpecsPanel_EmptyStringValue_ParsesAsZero()
    {
        // Arrange
        (string property, int value)? changedSpec = null;
        var cut = RenderComponent<NodeSpecsPanel>(parameters => parameters
            .Add(p => p.NodeSpecs, new NodeSpecsConfig())
            .Add(p => p.OnSpecChanged, EventCallback.Factory.Create<(string, int)>(this, s => changedSpec = s)));

        // Act
        var cpuInput = cut.FindAll("input[type='number']")[0];
        await cut.InvokeAsync(() => cpuInput.Change(""));

        // Assert
        changedSpec.Should().NotBeNull();
        changedSpec!.Value.value.Should().Be(0);
    }

    [Fact]
    public async Task NodeSpecsPanel_NullValue_ParsesAsZero()
    {
        // Arrange
        (string property, int value)? changedSpec = null;
        var cut = RenderComponent<NodeSpecsPanel>(parameters => parameters
            .Add(p => p.NodeSpecs, new NodeSpecsConfig())
            .Add(p => p.OnSpecChanged, EventCallback.Factory.Create<(string, int)>(this, s => changedSpec = s)));

        // Act
        var cpuInput = cut.FindAll("input[type='number']")[0];
        await cut.InvokeAsync(() => cpuInput.Change((string?)null));

        // Assert
        changedSpec.Should().NotBeNull();
        changedSpec!.Value.value.Should().Be(0);
    }

    [Fact]
    public async Task NodeSpecsPanel_NegativeValue_ParsesCorrectly()
    {
        // Arrange
        (string property, int value)? changedSpec = null;
        var cut = RenderComponent<NodeSpecsPanel>(parameters => parameters
            .Add(p => p.NodeSpecs, new NodeSpecsConfig())
            .Add(p => p.OnSpecChanged, EventCallback.Factory.Create<(string, int)>(this, s => changedSpec = s)));

        // Act
        var cpuInput = cut.FindAll("input[type='number']")[0];
        await cut.InvokeAsync(() => cpuInput.Change(-5));

        // Assert
        changedSpec.Should().NotBeNull();
        changedSpec!.Value.value.Should().Be(-5);
    }

    [Fact]
    public async Task NodeSpecsPanel_LargeValue_ParsesCorrectly()
    {
        // Arrange
        (string property, int value)? changedSpec = null;
        var cut = RenderComponent<NodeSpecsPanel>(parameters => parameters
            .Add(p => p.NodeSpecs, new NodeSpecsConfig())
            .Add(p => p.OnSpecChanged, EventCallback.Factory.Create<(string, int)>(this, s => changedSpec = s)));

        // Act
        var cpuInput = cut.FindAll("input[type='number']")[0];
        await cut.InvokeAsync(() => cpuInput.Change(999999));

        // Assert
        changedSpec.Should().NotBeNull();
        changedSpec!.Value.value.Should().Be(999999);
    }

    [Fact]
    public async Task NodeSpecsPanel_DecimalValue_TruncatesToInt()
    {
        // Arrange
        (string property, int value)? changedSpec = null;
        var cut = RenderComponent<NodeSpecsPanel>(parameters => parameters
            .Add(p => p.NodeSpecs, new NodeSpecsConfig())
            .Add(p => p.OnSpecChanged, EventCallback.Factory.Create<(string, int)>(this, s => changedSpec = s)));

        // Act
        var cpuInput = cut.FindAll("input[type='number']")[0];
        await cut.InvokeAsync(() => cpuInput.Change("16.5"));

        // Assert - int.TryParse will fail for decimal, so it should be 0
        changedSpec.Should().NotBeNull();
        changedSpec!.Value.value.Should().Be(0);
    }

    [Fact]
    public async Task NodeSpecsPanel_WhitespaceValue_ParsesAsZero()
    {
        // Arrange
        (string property, int value)? changedSpec = null;
        var cut = RenderComponent<NodeSpecsPanel>(parameters => parameters
            .Add(p => p.NodeSpecs, new NodeSpecsConfig())
            .Add(p => p.OnSpecChanged, EventCallback.Factory.Create<(string, int)>(this, s => changedSpec = s)));

        // Act
        var cpuInput = cut.FindAll("input[type='number']")[0];
        await cut.InvokeAsync(() => cpuInput.Change("   "));

        // Assert
        changedSpec.Should().NotBeNull();
        changedSpec!.Value.value.Should().Be(0);
    }

    #endregion

    #region Worker Specs With Infra Shown Tests

    [Fact]
    public async Task NodeSpecsPanel_ChangingProdWorkerCpu_WithInfraShown_PassesCorrectProperty()
    {
        // Arrange
        (string property, int value)? changedSpec = null;
        var cut = RenderComponent<NodeSpecsPanel>(parameters => parameters
            .Add(p => p.NodeSpecs, new NodeSpecsConfig())
            .Add(p => p.ShowInfraSpecs, true)
            .Add(p => p.OnSpecChanged, EventCallback.Factory.Create<(string, int)>(this, s => changedSpec = s)));

        // Act - ProdWorkerCpu is index 12 when infra is shown
        var cpuInput = cut.FindAll("input[type='number']")[12];
        await cut.InvokeAsync(() => cpuInput.Change(32));

        // Assert
        changedSpec.Should().NotBeNull();
        changedSpec!.Value.property.Should().Be("ProdWorkerCpu");
        changedSpec!.Value.value.Should().Be(32);
    }

    [Fact]
    public async Task NodeSpecsPanel_ChangingNonProdWorkerDisk_WithInfraShown_PassesCorrectProperty()
    {
        // Arrange
        (string property, int value)? changedSpec = null;
        var cut = RenderComponent<NodeSpecsPanel>(parameters => parameters
            .Add(p => p.NodeSpecs, new NodeSpecsConfig())
            .Add(p => p.ShowInfraSpecs, true)
            .Add(p => p.OnSpecChanged, EventCallback.Factory.Create<(string, int)>(this, s => changedSpec = s)));

        // Act - NonProdWorkerDisk is index 17 when infra is shown
        var diskInput = cut.FindAll("input[type='number']")[17];
        await cut.InvokeAsync(() => diskInput.Change(250));

        // Assert
        changedSpec.Should().NotBeNull();
        changedSpec!.Value.property.Should().Be("NonProdWorkerDisk");
        changedSpec!.Value.value.Should().Be(250);
    }

    #endregion

    #region Display Tests for All Spec Types

    [Fact]
    public void NodeSpecsPanel_DisplaysAllNonProdSpecs()
    {
        // Arrange
        var specs = new NodeSpecsConfig
        {
            NonProdMasterCpu = 4,
            NonProdMasterRam = 16,
            NonProdMasterDisk = 100,
            NonProdInfraCpu = 2,
            NonProdInfraRam = 8,
            NonProdInfraDisk = 50,
            NonProdWorkerCpu = 8,
            NonProdWorkerRam = 32,
            NonProdWorkerDisk = 200
        };

        // Act
        var cut = RenderComponent<NodeSpecsPanel>(parameters => parameters
            .Add(p => p.NodeSpecs, specs)
            .Add(p => p.ShowInfraSpecs, true));

        // Assert - Check non-prod master specs
        var inputs = cut.FindAll("input[type='number']");
        inputs[3].GetAttribute("value").Should().Be("4");
        inputs[4].GetAttribute("value").Should().Be("16");
        inputs[5].GetAttribute("value").Should().Be("100");

        // Non-prod infra specs
        inputs[9].GetAttribute("value").Should().Be("2");
        inputs[10].GetAttribute("value").Should().Be("8");
        inputs[11].GetAttribute("value").Should().Be("50");

        // Non-prod worker specs
        inputs[15].GetAttribute("value").Should().Be("8");
        inputs[16].GetAttribute("value").Should().Be("32");
        inputs[17].GetAttribute("value").Should().Be("200");
    }

    [Fact]
    public void NodeSpecsPanel_DisplaysNonProdWorkerSpecs_WhenInfraHidden()
    {
        // Arrange
        var specs = new NodeSpecsConfig
        {
            NonProdWorkerCpu = 8,
            NonProdWorkerRam = 32,
            NonProdWorkerDisk = 200
        };

        // Act
        var cut = RenderComponent<NodeSpecsPanel>(parameters => parameters
            .Add(p => p.NodeSpecs, specs)
            .Add(p => p.ShowInfraSpecs, false));

        // Assert - Non-prod worker specs are at indices 9-11 when infra hidden
        var inputs = cut.FindAll("input[type='number']");
        inputs[9].GetAttribute("value").Should().Be("8");
        inputs[10].GetAttribute("value").Should().Be("32");
        inputs[11].GetAttribute("value").Should().Be("200");
    }

    [Fact]
    public void NodeSpecsPanel_DefaultsToShowInfraSpecsTrue()
    {
        // Act
        var cut = RenderComponent<NodeSpecsPanel>();

        // Assert - Should show all 3 sections (control plane, infra, workers)
        cut.FindAll(".specs-section h4").Should().HaveCount(3);
    }

    #endregion
}
