using Bunit;
using FluentAssertions;
using InfraSizingCalculator.Components.Configuration;
using InfraSizingCalculator.Models.Enums;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components;

/// <summary>
/// Tests for SettingsPanel component
/// </summary>
public class SettingsPanelTests : TestContext
{
    private Dictionary<EnvironmentType, double> CreateDefaultHeadroom()
    {
        return new Dictionary<EnvironmentType, double>
        {
            { EnvironmentType.Dev, 15 },
            { EnvironmentType.Test, 15 },
            { EnvironmentType.Stage, 20 },
            { EnvironmentType.Prod, 25 },
            { EnvironmentType.DR, 30 }
        };
    }

    private Dictionary<EnvironmentType, int> CreateDefaultReplicas()
    {
        return new Dictionary<EnvironmentType, int>
        {
            { EnvironmentType.Dev, 1 },
            { EnvironmentType.Test, 1 },
            { EnvironmentType.Stage, 2 },
            { EnvironmentType.Prod, 3 },
            { EnvironmentType.DR, 3 }
        };
    }

    #region Overcommit Settings Tests

    [Fact]
    public void SettingsPanel_RendersOvercommitSection()
    {
        // Act
        var cut = RenderComponent<SettingsPanel>(parameters => parameters
            .Add(p => p.ProdCpuOvercommit, 1.0)
            .Add(p => p.ProdMemoryOvercommit, 1.0)
            .Add(p => p.NonProdCpuOvercommit, 1.5)
            .Add(p => p.NonProdMemoryOvercommit, 1.5));

        // Assert
        var overcommitSection = cut.FindAll(".settings-section")[0];
        overcommitSection.QuerySelector("h4")?.TextContent.Should().Contain("Overcommit Ratios");
    }

    [Fact]
    public void SettingsPanel_DisplaysProductionOvercommitSettings()
    {
        // Act
        var cut = RenderComponent<SettingsPanel>(parameters => parameters
            .Add(p => p.ProdCpuOvercommit, 1.2)
            .Add(p => p.ProdMemoryOvercommit, 1.1)
            .Add(p => p.NonProdCpuOvercommit, 1.5)
            .Add(p => p.NonProdMemoryOvercommit, 1.5));

        // Assert
        var prodGroup = cut.Find(".settings-group");
        prodGroup.QuerySelector(".group-label")?.TextContent.Should().Be("Production");

        var inputs = prodGroup.QuerySelectorAll("input[type='number']");
        inputs[0].GetAttribute("value").Should().Be("1.2"); // CPU
        inputs[1].GetAttribute("value").Should().Be("1.1"); // Memory
    }

    [Fact]
    public void SettingsPanel_DisplaysNonProdOvercommitSettings()
    {
        // Act
        var cut = RenderComponent<SettingsPanel>(parameters => parameters
            .Add(p => p.ProdCpuOvercommit, 1.0)
            .Add(p => p.ProdMemoryOvercommit, 1.0)
            .Add(p => p.NonProdCpuOvercommit, 2.0)
            .Add(p => p.NonProdMemoryOvercommit, 1.8));

        // Assert
        var groups = cut.FindAll(".settings-group");
        var nonprodGroup = groups[1];
        nonprodGroup.QuerySelector(".group-label")?.TextContent.Should().Be("Non-Production");

        var inputs = nonprodGroup.QuerySelectorAll("input[type='number']");
        inputs[0].GetAttribute("value").Should().Be("2"); // CPU
        inputs[1].GetAttribute("value").Should().Be("1.8"); // Memory
    }

    [Fact]
    public void SettingsPanel_OvercommitInputsHaveStepAttribute()
    {
        // Act
        var cut = RenderComponent<SettingsPanel>();

        // Assert
        var overcommitInputs = cut.FindAll(".settings-section")[0].QuerySelectorAll("input[type='number']");
        foreach (var input in overcommitInputs)
        {
            input.GetAttribute("step").Should().Be("0.1");
        }
    }

    [Fact]
    public async Task SettingsPanel_InvokesOnOvercommitChanged_ForProdCpu()
    {
        // Arrange
        (bool isProd, bool isCpu, double value)? changedValue = null;
        var cut = RenderComponent<SettingsPanel>(parameters => parameters
            .Add(p => p.OnOvercommitChanged, value => changedValue = value));

        // Act
        await cut.InvokeAsync(() =>
        {
            var prodGroup = cut.Find(".settings-group");
            var cpuInput = prodGroup.QuerySelectorAll("input[type='number']")[0];
            cpuInput.Change("1.5");
        });

        // Assert
        changedValue.Should().NotBeNull();
        changedValue.Value.isProd.Should().BeTrue();
        changedValue.Value.isCpu.Should().BeTrue();
        changedValue.Value.value.Should().Be(1.5);
    }

    [Fact]
    public async Task SettingsPanel_InvokesOnOvercommitChanged_ForProdMemory()
    {
        // Arrange
        (bool isProd, bool isCpu, double value)? changedValue = null;
        var cut = RenderComponent<SettingsPanel>(parameters => parameters
            .Add(p => p.OnOvercommitChanged, value => changedValue = value));

        // Act
        await cut.InvokeAsync(() =>
        {
            var prodGroup = cut.Find(".settings-group");
            var memoryInput = prodGroup.QuerySelectorAll("input[type='number']")[1];
            memoryInput.Change("1.3");
        });

        // Assert
        changedValue.Should().NotBeNull();
        changedValue.Value.isProd.Should().BeTrue();
        changedValue.Value.isCpu.Should().BeFalse();
        changedValue.Value.value.Should().Be(1.3);
    }

    [Fact]
    public async Task SettingsPanel_InvokesOnOvercommitChanged_ForNonProdCpu()
    {
        // Arrange
        (bool isProd, bool isCpu, double value)? changedValue = null;
        var cut = RenderComponent<SettingsPanel>(parameters => parameters
            .Add(p => p.OnOvercommitChanged, value => changedValue = value));

        // Act
        await cut.InvokeAsync(() =>
        {
            var nonprodGroup = cut.FindAll(".settings-group")[1];
            var cpuInput = nonprodGroup.QuerySelectorAll("input[type='number']")[0];
            cpuInput.Change("2.0");
        });

        // Assert
        changedValue.Should().NotBeNull();
        changedValue.Value.isProd.Should().BeFalse();
        changedValue.Value.isCpu.Should().BeTrue();
        changedValue.Value.value.Should().Be(2.0);
    }

    [Fact]
    public async Task SettingsPanel_InvokesOnOvercommitChanged_ForNonProdMemory()
    {
        // Arrange
        (bool isProd, bool isCpu, double value)? changedValue = null;
        var cut = RenderComponent<SettingsPanel>(parameters => parameters
            .Add(p => p.OnOvercommitChanged, value => changedValue = value));

        // Act
        await cut.InvokeAsync(() =>
        {
            var nonprodGroup = cut.FindAll(".settings-group")[1];
            var memoryInput = nonprodGroup.QuerySelectorAll("input[type='number']")[1];
            memoryInput.Change("1.7");
        });

        // Assert
        changedValue.Should().NotBeNull();
        changedValue.Value.isProd.Should().BeFalse();
        changedValue.Value.isCpu.Should().BeFalse();
        changedValue.Value.value.Should().Be(1.7);
    }

    #endregion

    #region Headroom Settings Tests

    [Fact]
    public void SettingsPanel_RendersHeadroomSection()
    {
        // Act
        var cut = RenderComponent<SettingsPanel>(parameters => parameters
            .Add(p => p.Headroom, CreateDefaultHeadroom()));

        // Assert
        var headroomSection = cut.FindAll(".settings-section")[1];
        headroomSection.QuerySelector("h4")?.TextContent.Should().Contain("Headroom Percentages");
    }

    [Fact]
    public void SettingsPanel_DisplaysAllEnvironmentHeadroomInputs()
    {
        // Act
        var cut = RenderComponent<SettingsPanel>(parameters => parameters
            .Add(p => p.Headroom, CreateDefaultHeadroom()));

        // Assert
        var headroomSection = cut.FindAll(".settings-section")[1];
        var inputs = headroomSection.QuerySelectorAll("input[type='number']");
        inputs.Should().HaveCount(5); // Dev, Test, Stage, Prod, DR
    }

    [Fact]
    public void SettingsPanel_DisplaysCorrectHeadroomValues()
    {
        // Act
        var cut = RenderComponent<SettingsPanel>(parameters => parameters
            .Add(p => p.Headroom, CreateDefaultHeadroom()));

        // Assert
        var headroomSection = cut.FindAll(".settings-section")[1];
        var inputs = headroomSection.QuerySelectorAll("input[type='number']");

        inputs[0].GetAttribute("value").Should().Be("15"); // Dev
        inputs[1].GetAttribute("value").Should().Be("15"); // Test
        inputs[2].GetAttribute("value").Should().Be("20"); // Stage
        inputs[3].GetAttribute("value").Should().Be("25"); // Prod
        inputs[4].GetAttribute("value").Should().Be("30"); // DR
    }

    [Fact]
    public void SettingsPanel_HeadroomInputsHaveCorrectConstraints()
    {
        // Act
        var cut = RenderComponent<SettingsPanel>(parameters => parameters
            .Add(p => p.Headroom, CreateDefaultHeadroom()));

        // Assert
        var headroomSection = cut.FindAll(".settings-section")[1];
        var inputs = headroomSection.QuerySelectorAll("input[type='number']");

        foreach (var input in inputs)
        {
            input.GetAttribute("min").Should().Be("0");
            input.GetAttribute("max").Should().Be("100");
        }
    }

    [Fact]
    public void SettingsPanel_UsesDefaultHeadroom_WhenNotInDictionary()
    {
        // Arrange - Empty headroom dictionary
        var emptyHeadroom = new Dictionary<EnvironmentType, double>();

        // Act
        var cut = RenderComponent<SettingsPanel>(parameters => parameters
            .Add(p => p.Headroom, emptyHeadroom));

        // Assert - All should show default value of 20
        var headroomSection = cut.FindAll(".settings-section")[1];
        var inputs = headroomSection.QuerySelectorAll("input[type='number']");

        foreach (var input in inputs)
        {
            input.GetAttribute("value").Should().Be("20");
        }
    }

    [Fact]
    public async Task SettingsPanel_InvokesOnHeadroomChanged()
    {
        // Arrange
        (EnvironmentType env, double value)? changedValue = null;
        var cut = RenderComponent<SettingsPanel>(parameters => parameters
            .Add(p => p.Headroom, CreateDefaultHeadroom())
            .Add(p => p.OnHeadroomChanged, value => changedValue = value));

        // Act - Change Prod headroom
        await cut.InvokeAsync(() =>
        {
            var headroomSection = cut.FindAll(".settings-section")[1];
            var prodInput = headroomSection.QuerySelectorAll("input[type='number']")[3]; // Prod is 4th
            prodInput.Change("35");
        });

        // Assert
        changedValue.Should().NotBeNull();
        changedValue.Value.env.Should().Be(EnvironmentType.Prod);
        changedValue.Value.value.Should().Be(35);
    }

    #endregion

    #region Replica Settings Tests

    [Fact]
    public void SettingsPanel_RendersReplicaSection()
    {
        // Act
        var cut = RenderComponent<SettingsPanel>(parameters => parameters
            .Add(p => p.Replicas, CreateDefaultReplicas()));

        // Assert
        var replicaSection = cut.FindAll(".settings-section")[2];
        replicaSection.QuerySelector("h4")?.TextContent.Should().Contain("Default Replicas");
    }

    [Fact]
    public void SettingsPanel_DisplaysAllEnvironmentReplicaInputs()
    {
        // Act
        var cut = RenderComponent<SettingsPanel>(parameters => parameters
            .Add(p => p.Replicas, CreateDefaultReplicas()));

        // Assert
        var replicaSection = cut.FindAll(".settings-section")[2];
        var inputs = replicaSection.QuerySelectorAll("input[type='number']");
        inputs.Should().HaveCount(5); // Dev, Test, Stage, Prod, DR
    }

    [Fact]
    public void SettingsPanel_DisplaysCorrectReplicaValues()
    {
        // Act
        var cut = RenderComponent<SettingsPanel>(parameters => parameters
            .Add(p => p.Replicas, CreateDefaultReplicas()));

        // Assert
        var replicaSection = cut.FindAll(".settings-section")[2];
        var inputs = replicaSection.QuerySelectorAll("input[type='number']");

        inputs[0].GetAttribute("value").Should().Be("1"); // Dev
        inputs[1].GetAttribute("value").Should().Be("1"); // Test
        inputs[2].GetAttribute("value").Should().Be("2"); // Stage
        inputs[3].GetAttribute("value").Should().Be("3"); // Prod
        inputs[4].GetAttribute("value").Should().Be("3"); // DR
    }

    [Fact]
    public void SettingsPanel_ReplicaInputsHaveCorrectConstraints()
    {
        // Act
        var cut = RenderComponent<SettingsPanel>(parameters => parameters
            .Add(p => p.Replicas, CreateDefaultReplicas()));

        // Assert
        var replicaSection = cut.FindAll(".settings-section")[2];
        var inputs = replicaSection.QuerySelectorAll("input[type='number']");

        foreach (var input in inputs)
        {
            input.GetAttribute("min").Should().Be("1");
            input.GetAttribute("max").Should().Be("10");
        }
    }

    [Fact]
    public void SettingsPanel_UsesDefaultReplicas_WhenNotInDictionary()
    {
        // Arrange - Empty replicas dictionary
        var emptyReplicas = new Dictionary<EnvironmentType, int>();

        // Act
        var cut = RenderComponent<SettingsPanel>(parameters => parameters
            .Add(p => p.Replicas, emptyReplicas));

        // Assert - All should show default value of 1
        var replicaSection = cut.FindAll(".settings-section")[2];
        var inputs = replicaSection.QuerySelectorAll("input[type='number']");

        foreach (var input in inputs)
        {
            input.GetAttribute("value").Should().Be("1");
        }
    }

    [Fact]
    public async Task SettingsPanel_InvokesOnReplicasChanged()
    {
        // Arrange
        (EnvironmentType env, int value)? changedValue = null;
        var cut = RenderComponent<SettingsPanel>(parameters => parameters
            .Add(p => p.Replicas, CreateDefaultReplicas())
            .Add(p => p.OnReplicasChanged, value => changedValue = value));

        // Act - Change Stage replicas
        await cut.InvokeAsync(() =>
        {
            var replicaSection = cut.FindAll(".settings-section")[2];
            var stageInput = replicaSection.QuerySelectorAll("input[type='number']")[2]; // Stage is 3rd
            stageInput.Change("4");
        });

        // Assert
        changedValue.Should().NotBeNull();
        changedValue.Value.env.Should().Be(EnvironmentType.Stage);
        changedValue.Value.value.Should().Be(4);
    }

    #endregion

    #region Common Tests

    [Fact]
    public void SettingsPanel_AppliesAdditionalClass()
    {
        // Act
        var cut = RenderComponent<SettingsPanel>(parameters => parameters
            .Add(p => p.AdditionalClass, "custom-settings"));

        // Assert
        cut.Find(".settings-panel").ClassList.Should().Contain("custom-settings");
    }

    [Fact]
    public void SettingsPanel_HasThreeSections()
    {
        // Act
        var cut = RenderComponent<SettingsPanel>();

        // Assert
        var sections = cut.FindAll(".settings-section");
        sections.Should().HaveCount(3); // Overcommit, Headroom, Replicas
    }

    [Fact]
    public void SettingsPanel_AllSectionsHaveDescriptions()
    {
        // Act
        var cut = RenderComponent<SettingsPanel>();

        // Assert
        var sections = cut.FindAll(".settings-section");
        foreach (var section in sections)
        {
            section.QuerySelector(".settings-desc").Should().NotBeNull("each section should have a description");
        }
    }

    [Fact]
    public void SettingsPanel_AllLabelsHavePercentageSymbol_ForHeadroom()
    {
        // Act
        var cut = RenderComponent<SettingsPanel>(parameters => parameters
            .Add(p => p.Headroom, CreateDefaultHeadroom()));

        // Assert
        var headroomSection = cut.FindAll(".settings-section")[1];
        var labels = headroomSection.QuerySelectorAll("label");

        foreach (var label in labels)
        {
            label.TextContent.Should().Contain("(%)");
        }
    }

    [Fact]
    public async Task SettingsPanel_ParsesInvalidDoubleAsZero()
    {
        // Arrange
        (bool isProd, bool isCpu, double value)? changedValue = null;
        var cut = RenderComponent<SettingsPanel>(parameters => parameters
            .Add(p => p.OnOvercommitChanged, value => changedValue = value));

        // Act
        await cut.InvokeAsync(() =>
        {
            var prodGroup = cut.Find(".settings-group");
            var cpuInput = prodGroup.QuerySelectorAll("input[type='number']")[0];
            cpuInput.Change("invalid");
        });

        // Assert
        changedValue.Should().NotBeNull();
        changedValue.Value.value.Should().Be(0);
    }

    [Fact]
    public async Task SettingsPanel_ParsesInvalidIntAsZero()
    {
        // Arrange
        (EnvironmentType env, int value)? changedValue = null;
        var cut = RenderComponent<SettingsPanel>(parameters => parameters
            .Add(p => p.Replicas, CreateDefaultReplicas())
            .Add(p => p.OnReplicasChanged, value => changedValue = value));

        // Act
        await cut.InvokeAsync(() =>
        {
            var replicaSection = cut.FindAll(".settings-section")[2];
            var input = replicaSection.QuerySelectorAll("input[type='number']")[0];
            input.Change("abc");
        });

        // Assert
        changedValue.Should().NotBeNull();
        changedValue.Value.value.Should().Be(0);
    }

    [Fact]
    public async Task SettingsPanel_NoCallbacksDoNotError()
    {
        // Arrange & Act - No callbacks provided
        var cut = RenderComponent<SettingsPanel>(parameters => parameters
            .Add(p => p.Headroom, CreateDefaultHeadroom())
            .Add(p => p.Replicas, CreateDefaultReplicas()));

        // Assert - All actions should not throw
        await cut.InvokeAsync(() =>
        {
            var actions = new Action[]
            {
                () => cut.Find(".settings-group input[type='number']").Change("1.5"),
                () => cut.FindAll(".settings-section")[1].QuerySelector("input[type='number']")!.Change("25"),
                () => cut.FindAll(".settings-section")[2].QuerySelector("input[type='number']")!.Change("2")
            };

            foreach (var action in actions)
            {
                action.Should().NotThrow();
            }
        });
    }

    [Fact]
    public void SettingsPanel_EnvironmentOrderIsConsistent()
    {
        // Act
        var cut = RenderComponent<SettingsPanel>(parameters => parameters
            .Add(p => p.Headroom, CreateDefaultHeadroom())
            .Add(p => p.Replicas, CreateDefaultReplicas()));

        // Assert - Check headroom section labels
        var headroomLabels = cut.FindAll(".settings-section")[1].QuerySelectorAll("label");
        headroomLabels[0].TextContent.Should().Contain("Dev");
        headroomLabels[1].TextContent.Should().Contain("Test");
        headroomLabels[2].TextContent.Should().Contain("Stage");
        headroomLabels[3].TextContent.Should().Contain("Prod");
        headroomLabels[4].TextContent.Should().Contain("DR");

        // Check replica section labels
        var replicaLabels = cut.FindAll(".settings-section")[2].QuerySelectorAll("label");
        replicaLabels[0].TextContent.Should().Contain("Dev");
        replicaLabels[1].TextContent.Should().Contain("Test");
        replicaLabels[2].TextContent.Should().Contain("Stage");
        replicaLabels[3].TextContent.Should().Contain("Prod");
        replicaLabels[4].TextContent.Should().Contain("DR");
    }

    [Fact]
    public async Task SettingsPanel_CanUpdateAllSettings()
    {
        // Arrange - Track all changes
        var overcommitChanges = new List<(bool isProd, bool isCpu, double value)>();
        var headroomChanges = new List<(EnvironmentType env, double value)>();
        var replicaChanges = new List<(EnvironmentType env, int value)>();

        var cut = RenderComponent<SettingsPanel>(parameters => parameters
            .Add(p => p.Headroom, CreateDefaultHeadroom())
            .Add(p => p.Replicas, CreateDefaultReplicas())
            .Add(p => p.OnOvercommitChanged, v => overcommitChanges.Add(v))
            .Add(p => p.OnHeadroomChanged, v => headroomChanges.Add(v))
            .Add(p => p.OnReplicasChanged, v => replicaChanges.Add(v)));

        // Act - Change one of each type
        await cut.InvokeAsync(() =>
        {
            // Change prod CPU overcommit
            cut.Find(".settings-group input[type='number']").Change("1.8");

            // Change Dev headroom
            cut.FindAll(".settings-section")[1].QuerySelectorAll("input[type='number']")[0].Change("30");

            // Change Test replicas
            cut.FindAll(".settings-section")[2].QuerySelectorAll("input[type='number']")[1].Change("2");
        });

        // Assert
        overcommitChanges.Should().HaveCount(1);
        overcommitChanges[0].isProd.Should().BeTrue();
        overcommitChanges[0].isCpu.Should().BeTrue();
        overcommitChanges[0].value.Should().Be(1.8);

        headroomChanges.Should().HaveCount(1);
        headroomChanges[0].env.Should().Be(EnvironmentType.Dev);
        headroomChanges[0].value.Should().Be(30);

        replicaChanges.Should().HaveCount(1);
        replicaChanges[0].env.Should().Be(EnvironmentType.Test);
        replicaChanges[0].value.Should().Be(2);
    }

    #endregion
}
