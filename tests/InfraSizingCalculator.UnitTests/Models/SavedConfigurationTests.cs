using FluentAssertions;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Models;

/// <summary>
/// Tests for SavedConfiguration and ConfigurationSummary models.
/// </summary>
public class SavedConfigurationTests
{
    #region Default Values Tests

    [Fact]
    public void DefaultValues_IdIsNotEmpty()
    {
        var config = new SavedConfiguration();
        config.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void DefaultValues_NameIsEmpty()
    {
        var config = new SavedConfiguration();
        config.Name.Should().BeEmpty();
    }

    [Fact]
    public void DefaultValues_DescriptionIsEmpty()
    {
        var config = new SavedConfiguration();
        config.Description.Should().BeEmpty();
    }

    [Fact]
    public void DefaultValues_SavedAtIsRecentUtcTime()
    {
        var before = DateTime.UtcNow;
        var config = new SavedConfiguration();
        var after = DateTime.UtcNow;

        config.SavedAt.Should().BeOnOrAfter(before);
        config.SavedAt.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void DefaultValues_NullablePropertiesAreNull()
    {
        var config = new SavedConfiguration();

        config.Distribution.Should().BeNull();
        config.K8sInput.Should().BeNull();
        config.VMInput.Should().BeNull();
        config.Summary.Should().BeNull();
    }

    #endregion

    #region GenerateDescription Tests - K8s Mode

    [Fact]
    public void GenerateDescription_K8sWithDistributionAndSummary_ReturnsCorrectFormat()
    {
        var config = new SavedConfiguration
        {
            Technology = Technology.DotNet,
            DeploymentModel = DeploymentModel.Kubernetes,
            Distribution = Distribution.OpenShift,
            Summary = new ConfigurationSummary
            {
                TotalApps = 10,
                TotalNodes = 15
            }
        };

        var description = SavedConfiguration.GenerateDescription(config);

        description.Should().Be("DotNet | OpenShift | 10 apps | 15 nodes");
    }

    [Fact]
    public void GenerateDescription_K8sWithoutSummary_ReturnsBaseFormat()
    {
        var config = new SavedConfiguration
        {
            Technology = Technology.Mendix,
            DeploymentModel = DeploymentModel.Kubernetes,
            Distribution = Distribution.EKS
        };

        var description = SavedConfiguration.GenerateDescription(config);

        description.Should().Be("Mendix | EKS");
    }

    [Fact]
    public void GenerateDescription_K8sWithoutDistribution_AddsVMs()
    {
        var config = new SavedConfiguration
        {
            Technology = Technology.Java,
            DeploymentModel = DeploymentModel.Kubernetes,
            Distribution = null
        };

        var description = SavedConfiguration.GenerateDescription(config);

        // When K8s but no distribution, it falls through to VMs case
        description.Should().Be("Java | VMs");
    }

    #endregion

    #region GenerateDescription Tests - VM Mode

    [Fact]
    public void GenerateDescription_VMWithSummary_ReturnsCorrectFormat()
    {
        var config = new SavedConfiguration
        {
            Technology = Technology.NodeJs,
            DeploymentModel = DeploymentModel.VMs,
            Summary = new ConfigurationSummary
            {
                TotalApps = 5,
                TotalVMs = 8
            }
        };

        var description = SavedConfiguration.GenerateDescription(config);

        description.Should().Be("NodeJs | VMs | 5 apps | 8 VMs");
    }

    [Fact]
    public void GenerateDescription_VMWithoutSummary_ReturnsBaseFormat()
    {
        var config = new SavedConfiguration
        {
            Technology = Technology.Python,
            DeploymentModel = DeploymentModel.VMs
        };

        var description = SavedConfiguration.GenerateDescription(config);

        description.Should().Be("Python | VMs");
    }

    #endregion

    #region GenerateDescription Tests - All Technologies

    [Theory]
    [InlineData(Technology.DotNet)]
    [InlineData(Technology.Java)]
    [InlineData(Technology.NodeJs)]
    [InlineData(Technology.Python)]
    [InlineData(Technology.Go)]
    [InlineData(Technology.Mendix)]
    [InlineData(Technology.OutSystems)]
    public void GenerateDescription_AllTechnologies_IncludesTechnologyName(Technology tech)
    {
        var config = new SavedConfiguration
        {
            Technology = tech,
            DeploymentModel = DeploymentModel.VMs
        };

        var description = SavedConfiguration.GenerateDescription(config);

        description.Should().StartWith(tech.ToString());
    }

    #endregion

    #region GenerateDescription Tests - Various Distributions

    [Theory]
    [InlineData(Distribution.Kubernetes)]
    [InlineData(Distribution.OpenShift)]
    [InlineData(Distribution.EKS)]
    [InlineData(Distribution.AKS)]
    [InlineData(Distribution.GKE)]
    [InlineData(Distribution.Rancher)]
    [InlineData(Distribution.K3s)]
    public void GenerateDescription_VariousDistributions_IncludesDistributionName(Distribution distro)
    {
        var config = new SavedConfiguration
        {
            Technology = Technology.DotNet,
            DeploymentModel = DeploymentModel.Kubernetes,
            Distribution = distro
        };

        var description = SavedConfiguration.GenerateDescription(config);

        description.Should().Contain(distro.ToString());
    }

    #endregion

    #region Property Assignment Tests

    [Fact]
    public void Id_CanBeSetAndRetrieved()
    {
        var id = Guid.NewGuid();
        var config = new SavedConfiguration { Id = id };
        config.Id.Should().Be(id);
    }

    [Fact]
    public void Name_CanBeSetAndRetrieved()
    {
        var config = new SavedConfiguration { Name = "My Config" };
        config.Name.Should().Be("My Config");
    }

    [Fact]
    public void Description_CanBeSetAndRetrieved()
    {
        var config = new SavedConfiguration { Description = "Test description" };
        config.Description.Should().Be("Test description");
    }

    [Fact]
    public void SavedAt_CanBeSetAndRetrieved()
    {
        var date = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        var config = new SavedConfiguration { SavedAt = date };
        config.SavedAt.Should().Be(date);
    }

    [Fact]
    public void DeploymentModel_CanBeSetAndRetrieved()
    {
        var config = new SavedConfiguration { DeploymentModel = DeploymentModel.Kubernetes };
        config.DeploymentModel.Should().Be(DeploymentModel.Kubernetes);
    }

    [Fact]
    public void Technology_CanBeSetAndRetrieved()
    {
        var config = new SavedConfiguration { Technology = Technology.Mendix };
        config.Technology.Should().Be(Technology.Mendix);
    }

    [Fact]
    public void Distribution_CanBeSetAndRetrieved()
    {
        var config = new SavedConfiguration { Distribution = Distribution.OpenShift };
        config.Distribution.Should().Be(Distribution.OpenShift);
    }

    [Fact]
    public void K8sInput_CanBeSetAndRetrieved()
    {
        var input = new K8sSizingInput();
        var config = new SavedConfiguration { K8sInput = input };
        config.K8sInput.Should().BeSameAs(input);
    }

    [Fact]
    public void VMInput_CanBeSetAndRetrieved()
    {
        var input = new VMSizingInput();
        var config = new SavedConfiguration { VMInput = input };
        config.VMInput.Should().BeSameAs(input);
    }

    [Fact]
    public void Summary_CanBeSetAndRetrieved()
    {
        var summary = new ConfigurationSummary { TotalApps = 25 };
        var config = new SavedConfiguration { Summary = summary };
        config.Summary.Should().BeSameAs(summary);
    }

    #endregion

    #region ConfigurationSummary Tests

    [Fact]
    public void ConfigurationSummary_DefaultValues()
    {
        var summary = new ConfigurationSummary();

        summary.TotalApps.Should().Be(0);
        summary.TotalNodes.Should().Be(0);
        summary.TotalVMs.Should().Be(0);
        summary.TotalCpu.Should().Be(0);
        summary.TotalRam.Should().Be(0);
        summary.TotalDisk.Should().Be(0);
        summary.EnvironmentCount.Should().Be(0);
    }

    [Fact]
    public void ConfigurationSummary_AllPropertiesCanBeSet()
    {
        var summary = new ConfigurationSummary
        {
            TotalApps = 10,
            TotalNodes = 20,
            TotalVMs = 5,
            TotalCpu = 100,
            TotalRam = 256,
            TotalDisk = 1000,
            EnvironmentCount = 4
        };

        summary.TotalApps.Should().Be(10);
        summary.TotalNodes.Should().Be(20);
        summary.TotalVMs.Should().Be(5);
        summary.TotalCpu.Should().Be(100);
        summary.TotalRam.Should().Be(256);
        summary.TotalDisk.Should().Be(1000);
        summary.EnvironmentCount.Should().Be(4);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void NewInstances_HaveUniqueIds()
    {
        var config1 = new SavedConfiguration();
        var config2 = new SavedConfiguration();

        config1.Id.Should().NotBe(config2.Id);
    }

    [Fact]
    public void GenerateDescription_WithZeroAppCount_StillShowsApps()
    {
        var config = new SavedConfiguration
        {
            Technology = Technology.DotNet,
            DeploymentModel = DeploymentModel.Kubernetes,
            Distribution = Distribution.Kubernetes,
            Summary = new ConfigurationSummary
            {
                TotalApps = 0,
                TotalNodes = 3
            }
        };

        var description = SavedConfiguration.GenerateDescription(config);

        description.Should().Contain("0 apps");
    }

    [Fact]
    public void GenerateDescription_WithLargeNumbers_FormatsCorrectly()
    {
        var config = new SavedConfiguration
        {
            Technology = Technology.Java,
            DeploymentModel = DeploymentModel.Kubernetes,
            Distribution = Distribution.EKS,
            Summary = new ConfigurationSummary
            {
                TotalApps = 1000,
                TotalNodes = 500
            }
        };

        var description = SavedConfiguration.GenerateDescription(config);

        description.Should().Contain("1000 apps");
        description.Should().Contain("500 nodes");
    }

    #endregion
}
