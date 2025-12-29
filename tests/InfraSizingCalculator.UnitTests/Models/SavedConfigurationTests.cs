using FluentAssertions;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Models;

/// <summary>
/// Tests for SavedConfiguration model that stores saved sizing configurations
/// for quick recall and comparison.
/// </summary>
public class SavedConfigurationTests
{
    #region Default Value Tests

    [Fact]
    public void Constructor_SetsDefaultId()
    {
        // Act
        var config = new SavedConfiguration();

        // Assert
        config.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Constructor_GeneratesUniqueIds()
    {
        // Act
        var config1 = new SavedConfiguration();
        var config2 = new SavedConfiguration();

        // Assert
        config1.Id.Should().NotBe(config2.Id);
    }

    [Fact]
    public void Constructor_SetsDefaultName()
    {
        // Act
        var config = new SavedConfiguration();

        // Assert
        config.Name.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_SetsDefaultDescription()
    {
        // Act
        var config = new SavedConfiguration();

        // Assert
        config.Description.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_SetsSavedAtToCurrentTime()
    {
        // Arrange
        var before = DateTime.UtcNow.AddSeconds(-1);

        // Act
        var config = new SavedConfiguration();

        // Assert
        var after = DateTime.UtcNow.AddSeconds(1);
        config.SavedAt.Should().BeAfter(before).And.BeBefore(after);
    }

    [Fact]
    public void Constructor_SetsNullablePropertiesToNull()
    {
        // Act
        var config = new SavedConfiguration();

        // Assert
        config.Distribution.Should().BeNull();
        config.K8sInput.Should().BeNull();
        config.VMInput.Should().BeNull();
        config.Summary.Should().BeNull();
    }

    #endregion

    #region Property Assignment Tests

    [Fact]
    public void Properties_CanBeSet()
    {
        // Arrange
        var config = new SavedConfiguration();
        var id = Guid.NewGuid();
        var savedAt = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);

        // Act
        config.Id = id;
        config.Name = "Production Config";
        config.Description = "High availability setup";
        config.SavedAt = savedAt;
        config.DeploymentModel = DeploymentModel.Kubernetes;
        config.Technology = Technology.DotNet;
        config.Distribution = Distribution.OpenShift;

        // Assert
        config.Id.Should().Be(id);
        config.Name.Should().Be("Production Config");
        config.Description.Should().Be("High availability setup");
        config.SavedAt.Should().Be(savedAt);
        config.DeploymentModel.Should().Be(DeploymentModel.Kubernetes);
        config.Technology.Should().Be(Technology.DotNet);
        config.Distribution.Should().Be(Distribution.OpenShift);
    }

    [Fact]
    public void K8sInput_CanBeAssigned()
    {
        // Arrange
        var config = new SavedConfiguration();
        var input = new K8sSizingInput();

        // Act
        config.K8sInput = input;

        // Assert
        config.K8sInput.Should().BeSameAs(input);
    }

    [Fact]
    public void VMInput_CanBeAssigned()
    {
        // Arrange
        var config = new SavedConfiguration();
        var input = new VMSizingInput();

        // Act
        config.VMInput = input;

        // Assert
        config.VMInput.Should().BeSameAs(input);
    }

    [Fact]
    public void Summary_CanBeAssigned()
    {
        // Arrange
        var config = new SavedConfiguration();
        var summary = new ConfigurationSummary
        {
            TotalApps = 10,
            TotalNodes = 5,
            TotalVMs = 0,
            TotalCpu = 80,
            TotalRam = 320,
            TotalDisk = 2000,
            EnvironmentCount = 4
        };

        // Act
        config.Summary = summary;

        // Assert
        config.Summary.Should().BeSameAs(summary);
    }

    #endregion

    #region GenerateDescription Tests - Kubernetes Scenarios

    [Fact]
    public void GenerateDescription_K8sWithDistributionAndSummary_IncludesAllParts()
    {
        // Arrange
        var config = new SavedConfiguration
        {
            DeploymentModel = DeploymentModel.Kubernetes,
            Technology = Technology.Java,
            Distribution = Distribution.OpenShift,
            Summary = new ConfigurationSummary
            {
                TotalApps = 25,
                TotalNodes = 12
            }
        };

        // Act
        var description = SavedConfiguration.GenerateDescription(config);

        // Assert
        description.Should().Be("Java | OpenShift | 25 apps | 12 nodes");
    }

    [Fact]
    public void GenerateDescription_K8sWithDistributionWithoutSummary_ExcludesResourceInfo()
    {
        // Arrange
        var config = new SavedConfiguration
        {
            DeploymentModel = DeploymentModel.Kubernetes,
            Technology = Technology.DotNet,
            Distribution = Distribution.EKS,
            Summary = null
        };

        // Act
        var description = SavedConfiguration.GenerateDescription(config);

        // Assert
        description.Should().Be("DotNet | EKS");
    }

    [Theory]
    [InlineData(Distribution.OpenShift)]
    [InlineData(Distribution.EKS)]
    [InlineData(Distribution.AKS)]
    [InlineData(Distribution.GKE)]
    [InlineData(Distribution.K3s)]
    [InlineData(Distribution.RKE2)]
    public void GenerateDescription_K8sWithVariousDistributions_IncludesDistributionName(Distribution distribution)
    {
        // Arrange
        var config = new SavedConfiguration
        {
            DeploymentModel = DeploymentModel.Kubernetes,
            Technology = Technology.Mendix,
            Distribution = distribution
        };

        // Act
        var description = SavedConfiguration.GenerateDescription(config);

        // Assert
        description.Should().Contain(distribution.ToString());
    }

    [Fact]
    public void GenerateDescription_K8sWithNullDistribution_UsesVMs()
    {
        // Arrange - K8s deployment but distribution is null (edge case)
        var config = new SavedConfiguration
        {
            DeploymentModel = DeploymentModel.Kubernetes,
            Technology = Technology.NodeJs,
            Distribution = null
        };

        // Act
        var description = SavedConfiguration.GenerateDescription(config);

        // Assert
        description.Should().Be("NodeJs | VMs");
    }

    #endregion

    #region GenerateDescription Tests - VM Scenarios

    [Fact]
    public void GenerateDescription_VMWithSummary_IncludesVMCount()
    {
        // Arrange
        var config = new SavedConfiguration
        {
            DeploymentModel = DeploymentModel.VMs,
            Technology = Technology.Python,
            Summary = new ConfigurationSummary
            {
                TotalApps = 15,
                TotalVMs = 8
            }
        };

        // Act
        var description = SavedConfiguration.GenerateDescription(config);

        // Assert
        description.Should().Be("Python | VMs | 15 apps | 8 VMs");
    }

    [Fact]
    public void GenerateDescription_VMWithoutSummary_ExcludesResourceInfo()
    {
        // Arrange
        var config = new SavedConfiguration
        {
            DeploymentModel = DeploymentModel.VMs,
            Technology = Technology.Go
        };

        // Act
        var description = SavedConfiguration.GenerateDescription(config);

        // Assert
        description.Should().Be("Go | VMs");
    }

    [Fact]
    public void GenerateDescription_VMIgnoresDistribution()
    {
        // Arrange - VM deployment with distribution set (should be ignored)
        var config = new SavedConfiguration
        {
            DeploymentModel = DeploymentModel.VMs,
            Technology = Technology.Java,
            Distribution = Distribution.OpenShift // Should be ignored for VM
        };

        // Act
        var description = SavedConfiguration.GenerateDescription(config);

        // Assert
        description.Should().Be("Java | VMs");
        description.Should().NotContain("OpenShift");
    }

    #endregion

    #region GenerateDescription Tests - All Technologies

    [Theory]
    [InlineData(Technology.DotNet, "DotNet")]
    [InlineData(Technology.Java, "Java")]
    [InlineData(Technology.NodeJs, "NodeJs")]
    [InlineData(Technology.Python, "Python")]
    [InlineData(Technology.Go, "Go")]
    [InlineData(Technology.Mendix, "Mendix")]
    [InlineData(Technology.OutSystems, "OutSystems")]
    public void GenerateDescription_AllTechnologies_IncludeTechnologyName(Technology tech, string expectedName)
    {
        // Arrange
        var config = new SavedConfiguration
        {
            DeploymentModel = DeploymentModel.VMs,
            Technology = tech
        };

        // Act
        var description = SavedConfiguration.GenerateDescription(config);

        // Assert
        description.Should().StartWith(expectedName);
    }

    #endregion

    #region GenerateDescription Tests - Edge Cases

    [Fact]
    public void GenerateDescription_SummaryWithZeroApps_ShowsZero()
    {
        // Arrange
        var config = new SavedConfiguration
        {
            DeploymentModel = DeploymentModel.VMs,
            Technology = Technology.DotNet,
            Summary = new ConfigurationSummary { TotalApps = 0, TotalVMs = 0 }
        };

        // Act
        var description = SavedConfiguration.GenerateDescription(config);

        // Assert
        description.Should().Contain("0 apps");
        description.Should().Contain("0 VMs");
    }

    [Fact]
    public void GenerateDescription_LargeNumbers_FormatsCorrectly()
    {
        // Arrange
        var config = new SavedConfiguration
        {
            DeploymentModel = DeploymentModel.Kubernetes,
            Technology = Technology.Java,
            Distribution = Distribution.OpenShift,
            Summary = new ConfigurationSummary { TotalApps = 1000, TotalNodes = 500 }
        };

        // Act
        var description = SavedConfiguration.GenerateDescription(config);

        // Assert
        description.Should().Contain("1000 apps");
        description.Should().Contain("500 nodes");
    }

    #endregion

    #region ConfigurationSummary Tests

    [Fact]
    public void ConfigurationSummary_DefaultValues_AreZero()
    {
        // Act
        var summary = new ConfigurationSummary();

        // Assert
        summary.TotalApps.Should().Be(0);
        summary.TotalNodes.Should().Be(0);
        summary.TotalVMs.Should().Be(0);
        summary.TotalCpu.Should().Be(0);
        summary.TotalRam.Should().Be(0);
        summary.TotalDisk.Should().Be(0);
        summary.EnvironmentCount.Should().Be(0);
    }

    [Fact]
    public void ConfigurationSummary_AllProperties_CanBeSet()
    {
        // Arrange & Act
        var summary = new ConfigurationSummary
        {
            TotalApps = 50,
            TotalNodes = 20,
            TotalVMs = 15,
            TotalCpu = 160,
            TotalRam = 640,
            TotalDisk = 5000,
            EnvironmentCount = 5
        };

        // Assert
        summary.TotalApps.Should().Be(50);
        summary.TotalNodes.Should().Be(20);
        summary.TotalVMs.Should().Be(15);
        summary.TotalCpu.Should().Be(160);
        summary.TotalRam.Should().Be(640);
        summary.TotalDisk.Should().Be(5000);
        summary.EnvironmentCount.Should().Be(5);
    }

    #endregion
}
