using FluentAssertions;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Models.Pricing;
using InfraSizingCalculator.Services;
using InfraSizingCalculator.Services.Interfaces;
using NSubstitute;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Services;

public class HomePageDistributionServiceTests
{
    private readonly IDistributionService _distributionService;
    private readonly HomePageDistributionService _sut;

    public HomePageDistributionServiceTests()
    {
        _distributionService = Substitute.For<IDistributionService>();
        _sut = new HomePageDistributionService(_distributionService);

        // Setup default distribution configs
        SetupDistributionConfigs();
    }

    private void SetupDistributionConfigs()
    {
        // On-prem distributions
        _distributionService.GetConfig(Distribution.OpenShift).Returns(CreateConfig(Distribution.OpenShift, "OpenShift (On-Prem)", false));
        _distributionService.GetConfig(Distribution.Kubernetes).Returns(CreateConfig(Distribution.Kubernetes, "Vanilla Kubernetes", false));
        _distributionService.GetConfig(Distribution.Rancher).Returns(CreateConfig(Distribution.Rancher, "Rancher (On-Prem)", false));
        _distributionService.GetConfig(Distribution.RKE2).Returns(CreateConfig(Distribution.RKE2, "RKE2", false));
        _distributionService.GetConfig(Distribution.K3s).Returns(CreateConfig(Distribution.K3s, "K3s", false));
        _distributionService.GetConfig(Distribution.MicroK8s).Returns(CreateConfig(Distribution.MicroK8s, "MicroK8s", false));
        _distributionService.GetConfig(Distribution.Charmed).Returns(CreateConfig(Distribution.Charmed, "Charmed Kubernetes", false));
        _distributionService.GetConfig(Distribution.Tanzu).Returns(CreateConfig(Distribution.Tanzu, "VMware Tanzu (On-Prem)", false));

        // Cloud/managed distributions
        _distributionService.GetConfig(Distribution.EKS).Returns(CreateConfig(Distribution.EKS, "Amazon EKS", true));
        _distributionService.GetConfig(Distribution.AKS).Returns(CreateConfig(Distribution.AKS, "Azure AKS", true));
        _distributionService.GetConfig(Distribution.GKE).Returns(CreateConfig(Distribution.GKE, "Google GKE", true));

        // OpenShift cloud variants
        _distributionService.GetConfig(Distribution.OpenShiftROSA).Returns(CreateConfig(Distribution.OpenShiftROSA, "OpenShift ROSA", true));
        _distributionService.GetConfig(Distribution.OpenShiftARO).Returns(CreateConfig(Distribution.OpenShiftARO, "OpenShift ARO", true));
        _distributionService.GetConfig(Distribution.OpenShiftDedicated).Returns(CreateConfig(Distribution.OpenShiftDedicated, "OpenShift Dedicated", true));
        _distributionService.GetConfig(Distribution.OpenShiftIBM).Returns(CreateConfig(Distribution.OpenShiftIBM, "OpenShift on IBM", true));
    }

    private static DistributionConfig CreateConfig(Distribution distribution, string name, bool hasManagedControlPlane)
    {
        return new DistributionConfig
        {
            Distribution = distribution,
            Name = name,
            Vendor = "Test Vendor",
            Icon = "test",
            BrandColor = "#000000",
            Tags = hasManagedControlPlane ? new[] { "cloud", "managed" } : new[] { "on-prem" },
            HasManagedControlPlane = hasManagedControlPlane,
            HasInfraNodes = distribution.ToString().Contains("OpenShift"),
            ProdControlPlane = hasManagedControlPlane ? NodeSpecs.Zero : new NodeSpecs(4, 16, 100),
            NonProdControlPlane = hasManagedControlPlane ? NodeSpecs.Zero : new NodeSpecs(2, 8, 50),
            ProdWorker = new NodeSpecs(8, 32, 100),
            NonProdWorker = new NodeSpecs(4, 16, 50)
        };
    }

    #region GetDistributionName Tests

    [Fact]
    public void GetDistributionName_WithNullDistribution_ReturnsUnknown()
    {
        var result = _sut.GetDistributionName(null);

        result.Should().Be("Unknown");
    }

    [Theory]
    [InlineData(Distribution.OpenShift, "OpenShift (On-Prem)")]
    [InlineData(Distribution.EKS, "Amazon EKS")]
    [InlineData(Distribution.AKS, "Azure AKS")]
    [InlineData(Distribution.GKE, "Google GKE")]
    [InlineData(Distribution.Kubernetes, "Vanilla Kubernetes")]
    public void GetDistributionName_WithValidDistribution_ReturnsConfigName(Distribution distro, string expectedName)
    {
        var result = _sut.GetDistributionName(distro);

        result.Should().Be(expectedName);
    }

    #endregion

    #region GetTechnologyName Tests

    [Fact]
    public void GetTechnologyName_WithNullTechnology_ReturnsUnknown()
    {
        var result = _sut.GetTechnologyName(null);

        result.Should().Be("Unknown");
    }

    [Theory]
    [InlineData(Technology.DotNet, ".NET")]
    [InlineData(Technology.Java, "Java")]
    [InlineData(Technology.NodeJs, "Node.js")]
    [InlineData(Technology.Python, "Python")]
    [InlineData(Technology.Go, "Go")]
    [InlineData(Technology.Mendix, "Mendix")]
    [InlineData(Technology.OutSystems, "OutSystems")]
    public void GetTechnologyName_WithValidTechnology_ReturnsCorrectName(Technology tech, string expectedName)
    {
        var result = _sut.GetTechnologyName(tech);

        result.Should().Be(expectedName);
    }

    #endregion

    #region IsOnPremDistribution Tests

    [Theory]
    [InlineData(Distribution.OpenShift, true)]
    [InlineData(Distribution.Kubernetes, true)]
    [InlineData(Distribution.Rancher, true)]
    [InlineData(Distribution.RKE2, true)]
    [InlineData(Distribution.K3s, true)]
    [InlineData(Distribution.MicroK8s, true)]
    [InlineData(Distribution.Charmed, true)]
    [InlineData(Distribution.Tanzu, true)]
    [InlineData(Distribution.EKS, false)]
    [InlineData(Distribution.AKS, false)]
    [InlineData(Distribution.GKE, false)]
    public void IsOnPremDistribution_WithNonMendix_ReturnsCorrectValue(Distribution distro, bool expected)
    {
        var context = new HomePageDistributionContext
        {
            SelectedTechnology = Technology.DotNet,
            SelectedDistribution = distro
        };

        var result = _sut.IsOnPremDistribution(context);

        result.Should().Be(expected);
    }

    [Fact]
    public void IsOnPremDistribution_WithNullDistribution_ReturnsFalse()
    {
        var context = new HomePageDistributionContext
        {
            SelectedTechnology = Technology.Java,
            SelectedDistribution = null
        };

        var result = _sut.IsOnPremDistribution(context);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsOnPremDistribution_MendixCloudSaaS_ReturnsFalse()
    {
        var context = new HomePageDistributionContext
        {
            SelectedTechnology = Technology.Mendix,
            SelectedDeployment = DeploymentModel.Kubernetes,
            MendixDeploymentCategory = MendixDeploymentCategory.Cloud
        };

        var result = _sut.IsOnPremDistribution(context);

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(MendixPrivateCloudProvider.Azure, false)]
    [InlineData(MendixPrivateCloudProvider.EKS, false)]
    [InlineData(MendixPrivateCloudProvider.AKS, false)]
    [InlineData(MendixPrivateCloudProvider.GKE, false)]
    [InlineData(MendixPrivateCloudProvider.OpenShift, true)]
    public void IsOnPremDistribution_MendixPrivateCloud_ReturnsCorrectValue(MendixPrivateCloudProvider provider, bool expected)
    {
        var context = new HomePageDistributionContext
        {
            SelectedTechnology = Technology.Mendix,
            SelectedDeployment = DeploymentModel.Kubernetes,
            MendixDeploymentCategory = MendixDeploymentCategory.PrivateCloud,
            MendixPrivateCloudProvider = provider
        };

        var result = _sut.IsOnPremDistribution(context);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(MendixPrivateCloudProvider.Rancher, true)]
    [InlineData(MendixPrivateCloudProvider.K3s, true)]
    [InlineData(MendixPrivateCloudProvider.GenericK8s, true)]
    public void IsOnPremDistribution_MendixOther_ReturnsTrue(MendixPrivateCloudProvider provider, bool expected)
    {
        var context = new HomePageDistributionContext
        {
            SelectedTechnology = Technology.Mendix,
            SelectedDeployment = DeploymentModel.Kubernetes,
            MendixDeploymentCategory = MendixDeploymentCategory.Other,
            MendixPrivateCloudProvider = provider
        };

        var result = _sut.IsOnPremDistribution(context);

        result.Should().Be(expected);
    }

    [Fact]
    public void IsOnPremDistribution_MendixOtherWithNoProvider_ReturnsTrue()
    {
        var context = new HomePageDistributionContext
        {
            SelectedTechnology = Technology.Mendix,
            SelectedDeployment = DeploymentModel.Kubernetes,
            MendixDeploymentCategory = MendixDeploymentCategory.Other,
            MendixPrivateCloudProvider = null
        };

        var result = _sut.IsOnPremDistribution(context);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsOnPremDistribution_MendixVMs_ReturnsFalse()
    {
        // Mendix VMs don't use the Mendix K8s path
        var context = new HomePageDistributionContext
        {
            SelectedTechnology = Technology.Mendix,
            SelectedDeployment = DeploymentModel.VMs,
            SelectedDistribution = null
        };

        var result = _sut.IsOnPremDistribution(context);

        result.Should().BeFalse();
    }

    #endregion

    #region IsOpenShiftDistribution Tests

    [Fact]
    public void IsOpenShiftDistribution_WithNullDistribution_ReturnsFalse()
    {
        var result = _sut.IsOpenShiftDistribution(null);

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(Distribution.OpenShift, true)]
    [InlineData(Distribution.OpenShiftROSA, true)]
    [InlineData(Distribution.OpenShiftARO, true)]
    [InlineData(Distribution.OpenShiftDedicated, true)]
    [InlineData(Distribution.OpenShiftIBM, true)]
    [InlineData(Distribution.EKS, false)]
    [InlineData(Distribution.AKS, false)]
    [InlineData(Distribution.Kubernetes, false)]
    [InlineData(Distribution.Rancher, false)]
    public void IsOpenShiftDistribution_ReturnsCorrectValue(Distribution distro, bool expected)
    {
        var result = _sut.IsOpenShiftDistribution(distro);

        result.Should().Be(expected);
    }

    #endregion

    #region IsManagedControlPlane Tests

    [Fact]
    public void IsManagedControlPlane_WithNullDistribution_ReturnsFalse()
    {
        var result = _sut.IsManagedControlPlane(null);

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(Distribution.EKS, true)]
    [InlineData(Distribution.AKS, true)]
    [InlineData(Distribution.GKE, true)]
    [InlineData(Distribution.OpenShift, false)]
    [InlineData(Distribution.Kubernetes, false)]
    [InlineData(Distribution.K3s, false)]
    public void IsManagedControlPlane_ReturnsCorrectValue(Distribution distro, bool expected)
    {
        var result = _sut.IsManagedControlPlane(distro);

        result.Should().Be(expected);
    }

    #endregion

    #region GetAllDistributions Tests

    [Fact]
    public void GetAllDistributions_DelegatesToDistributionService()
    {
        var expectedConfigs = new List<DistributionConfig>
        {
            CreateConfig(Distribution.EKS, "Amazon EKS", true),
            CreateConfig(Distribution.AKS, "Azure AKS", true)
        };
        _distributionService.GetAll().Returns(expectedConfigs);

        var result = _sut.GetAllDistributions();

        result.Should().BeEquivalentTo(expectedConfigs);
        _distributionService.Received(1).GetAll();
    }

    #endregion

    #region GetDistributionsByTag Tests

    [Fact]
    public void GetDistributionsByTag_DelegatesToDistributionService()
    {
        var tag = "cloud";
        var expectedConfigs = new List<DistributionConfig>
        {
            CreateConfig(Distribution.EKS, "Amazon EKS", true)
        };
        _distributionService.GetByTag(tag).Returns(expectedConfigs);

        var result = _sut.GetDistributionsByTag(tag);

        result.Should().BeEquivalentTo(expectedConfigs);
        _distributionService.Received(1).GetByTag(tag);
    }

    [Theory]
    [InlineData("on-prem")]
    [InlineData("cloud")]
    [InlineData("managed")]
    [InlineData("enterprise")]
    public void GetDistributionsByTag_WithDifferentTags_CallsServiceWithCorrectTag(string tag)
    {
        _distributionService.GetByTag(tag).Returns(Enumerable.Empty<DistributionConfig>());

        _sut.GetDistributionsByTag(tag);

        _distributionService.Received(1).GetByTag(tag);
    }

    #endregion

    #region Integration-like Scenarios

    [Fact]
    public void Scenario_MendixPrivateCloudOnOpenShift_IsOnPrem()
    {
        var context = new HomePageDistributionContext
        {
            SelectedTechnology = Technology.Mendix,
            SelectedDeployment = DeploymentModel.Kubernetes,
            MendixDeploymentCategory = MendixDeploymentCategory.PrivateCloud,
            MendixPrivateCloudProvider = MendixPrivateCloudProvider.OpenShift
        };

        var result = _sut.IsOnPremDistribution(context);

        result.Should().BeTrue("OpenShift is self-managed infrastructure");
    }

    [Fact]
    public void Scenario_MendixPrivateCloudOnEKS_IsNotOnPrem()
    {
        var context = new HomePageDistributionContext
        {
            SelectedTechnology = Technology.Mendix,
            SelectedDeployment = DeploymentModel.Kubernetes,
            MendixDeploymentCategory = MendixDeploymentCategory.PrivateCloud,
            MendixPrivateCloudProvider = MendixPrivateCloudProvider.EKS
        };

        var result = _sut.IsOnPremDistribution(context);

        result.Should().BeFalse("EKS is AWS-managed cloud infrastructure");
    }

    [Fact]
    public void Scenario_StandardDotNetOnOpenShift_IsOnPrem()
    {
        var context = new HomePageDistributionContext
        {
            SelectedTechnology = Technology.DotNet,
            SelectedDeployment = DeploymentModel.Kubernetes,
            SelectedDistribution = Distribution.OpenShift
        };

        var result = _sut.IsOnPremDistribution(context);

        result.Should().BeTrue();
    }

    [Fact]
    public void Scenario_StandardJavaOnEKS_IsNotOnPrem()
    {
        var context = new HomePageDistributionContext
        {
            SelectedTechnology = Technology.Java,
            SelectedDeployment = DeploymentModel.Kubernetes,
            SelectedDistribution = Distribution.EKS
        };

        var result = _sut.IsOnPremDistribution(context);

        result.Should().BeFalse();
    }

    #endregion
}
