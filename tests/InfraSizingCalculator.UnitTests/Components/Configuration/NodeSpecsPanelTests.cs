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
}
