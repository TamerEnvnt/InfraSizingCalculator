using Bunit;
using InfraSizingCalculator.Components.K8s;
using InfraSizingCalculator.Models.Enums;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Components.K8s;

/// <summary>
/// Tests for K8sSettingsConfig Blazor component.
/// Tests headroom, replicas, and overcommit ratio settings.
/// </summary>
public class K8sSettingsConfigTests : TestContext
{
    #region Section Rendering Tests

    [Fact]
    public void RendersHeadroomSection()
    {
        // Arrange & Act
        var cut = RenderComponent<K8sSettingsConfig>(parameters =>
            parameters.Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod }));

        // Assert
        Assert.Contains("Headroom (Buffer %)", cut.Markup);
    }

    [Fact]
    public void RendersReplicasSection()
    {
        // Arrange & Act
        var cut = RenderComponent<K8sSettingsConfig>(parameters =>
            parameters.Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod }));

        // Assert
        Assert.Contains("Replicas (Pod Instances)", cut.Markup);
    }

    [Fact]
    public void RendersOvercommitSection()
    {
        // Arrange & Act
        var cut = RenderComponent<K8sSettingsConfig>(parameters =>
            parameters.Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod }));

        // Assert
        Assert.Contains("Resource Overcommit Ratios", cut.Markup);
        Assert.Contains("Production", cut.Markup);
        Assert.Contains("Non-Production", cut.Markup);
    }

    #endregion

    #region Environment Display Tests

    [Fact]
    public void ShowsAllEnvironments()
    {
        // Arrange & Act
        var cut = RenderComponent<K8sSettingsConfig>(parameters =>
            parameters.Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod }));

        // Assert - All environments shown regardless of enabled state
        Assert.Contains("Dev", cut.Markup);
        Assert.Contains("Test", cut.Markup);
        Assert.Contains("Stage", cut.Markup);
        Assert.Contains("Prod", cut.Markup);
    }

    [Fact]
    public void DisablesInputs_ForNonEnabledEnvironments()
    {
        // Arrange - Only Prod enabled
        var enabled = new HashSet<EnvironmentType> { EnvironmentType.Prod };

        // Act
        var cut = RenderComponent<K8sSettingsConfig>(parameters =>
            parameters.Add(p => p.EnabledEnvironments, enabled));

        // Assert - Should have disabled inputs for non-Prod environments
        var disabledInputs = cut.FindAll("input[disabled]");
        Assert.True(disabledInputs.Count >= 4); // Dev, Test, Stage, DR headroom + replicas
    }

    #endregion

    #region Overcommit Ratio Tests

    [Fact]
    public void DisplaysProdOvercommitRatios()
    {
        // Arrange & Act
        var cut = RenderComponent<K8sSettingsConfig>(parameters =>
            parameters.Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod })
                     .Add(p => p.ProdCpuOvercommit, 1.5)
                     .Add(p => p.ProdMemoryOvercommit, 1.2));

        // Assert
        Assert.Contains("CPU Ratio", cut.Markup);
        Assert.Contains("Memory Ratio", cut.Markup);
    }

    [Fact]
    public void DisplaysNonProdOvercommitRatios()
    {
        // Arrange & Act
        var cut = RenderComponent<K8sSettingsConfig>(parameters =>
            parameters.Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Dev })
                     .Add(p => p.NonProdCpuOvercommit, 2.0)
                     .Add(p => p.NonProdMemoryOvercommit, 1.5));

        // Assert - Overcommit groups present
        var overcommitGroups = cut.FindAll(".overcommit-group");
        Assert.Equal(2, overcommitGroups.Count); // Prod and Non-Prod
    }

    #endregion

    #region Default Values Tests

    [Fact]
    public void HeadroomDefaults_To20Percent()
    {
        // Arrange - Empty headroom dictionary should use default
        var cut = RenderComponent<K8sSettingsConfig>(parameters =>
            parameters.Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod })
                     .Add(p => p.Headroom, new Dictionary<EnvironmentType, double>()));

        // Assert - Default value is 20
        var inputs = cut.FindAll(".settings-section-compact input[type='number']");
        Assert.True(inputs.Count > 0);
    }

    [Fact]
    public void ReplicasDefaults_To1()
    {
        // Arrange - Empty replicas dictionary should use default
        var cut = RenderComponent<K8sSettingsConfig>(parameters =>
            parameters.Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod })
                     .Add(p => p.Replicas, new Dictionary<EnvironmentType, int>()));

        // Assert - Inputs rendered
        var inputs = cut.FindAll(".settings-section-compact input[type='number']");
        Assert.True(inputs.Count > 0);
    }

    #endregion

    #region Settings Hint Tests

    [Fact]
    public void ShowsOvercommitHint()
    {
        // Arrange & Act
        var cut = RenderComponent<K8sSettingsConfig>(parameters =>
            parameters.Add(p => p.EnabledEnvironments, new HashSet<EnvironmentType> { EnvironmentType.Prod }));

        // Assert
        Assert.Contains("1.0 = no overcommit", cut.Markup);
    }

    #endregion
}
