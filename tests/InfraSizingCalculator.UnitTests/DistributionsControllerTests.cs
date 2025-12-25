using FluentAssertions;
using InfraSizingCalculator.Controllers.Api;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace InfraSizingCalculator.UnitTests;

public class DistributionsControllerTests
{
    private readonly IDistributionService _mockDistributionService;
    private readonly DistributionsController _controller;

    public DistributionsControllerTests()
    {
        _mockDistributionService = Substitute.For<IDistributionService>();
        _controller = new DistributionsController(_mockDistributionService);

        // Setup default mock behavior
        SetupDefaultMockBehavior();
    }

    private void SetupDefaultMockBehavior()
    {
        foreach (var dist in Enum.GetValues<Distribution>())
        {
            _mockDistributionService.GetConfig(dist).Returns(CreateDistributionConfig(dist));
        }
    }

    #region GetAll Tests

    [Fact]
    public void GetAll_ReturnsOkResult()
    {
        // Act
        var result = _controller.GetAll();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void GetAll_ReturnsAllDistributions()
    {
        // Act
        var result = _controller.GetAll();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var distributions = okResult.Value.Should().BeAssignableTo<IEnumerable<DistributionInfo>>().Subject;
        distributions.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void GetAll_CallsServiceForEachDistribution()
    {
        // Act
        _controller.GetAll();

        // Assert
        foreach (var dist in Enum.GetValues<Distribution>())
        {
            _mockDistributionService.Received().GetConfig(dist);
        }
    }

    [Fact]
    public void GetAll_DistributionsHaveName()
    {
        // Act
        var result = _controller.GetAll();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var distributions = okResult.Value.Should().BeAssignableTo<IEnumerable<DistributionInfo>>().Subject;
        distributions.Should().AllSatisfy(d => d.Name.Should().NotBeNullOrEmpty());
    }

    [Fact]
    public void GetAll_DistributionsHaveCategory()
    {
        // Act
        var result = _controller.GetAll();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var distributions = okResult.Value.Should().BeAssignableTo<IEnumerable<DistributionInfo>>().Subject;
        distributions.Should().AllSatisfy(d => d.Category.Should().NotBeNullOrEmpty());
    }

    [Fact]
    public void GetAll_ContainsOpenShift()
    {
        // Act
        var result = _controller.GetAll();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var distributions = okResult.Value.Should().BeAssignableTo<IEnumerable<DistributionInfo>>().Subject;
        distributions.Should().Contain(d => d.Distribution == Distribution.OpenShift);
    }

    [Fact]
    public void GetAll_ContainsEKS()
    {
        // Act
        var result = _controller.GetAll();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var distributions = okResult.Value.Should().BeAssignableTo<IEnumerable<DistributionInfo>>().Subject;
        distributions.Should().Contain(d => d.Distribution == Distribution.EKS);
    }

    [Fact]
    public void GetAll_ContainsKubernetes()
    {
        // Act
        var result = _controller.GetAll();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var distributions = okResult.Value.Should().BeAssignableTo<IEnumerable<DistributionInfo>>().Subject;
        distributions.Should().Contain(d => d.Distribution == Distribution.Kubernetes);
    }

    [Fact]
    public void GetAll_OpenShiftHasEnterpriseCategory()
    {
        // Act
        var result = _controller.GetAll();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var distributions = okResult.Value.Should().BeAssignableTo<IEnumerable<DistributionInfo>>().Subject;
        var openshift = distributions.FirstOrDefault(d => d.Distribution == Distribution.OpenShift);
        openshift.Should().NotBeNull();
        openshift!.Category.Should().Be("enterprise");
    }

    [Fact]
    public void GetAll_EKSHasManagedCategory()
    {
        // Act
        var result = _controller.GetAll();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var distributions = okResult.Value.Should().BeAssignableTo<IEnumerable<DistributionInfo>>().Subject;
        var eks = distributions.FirstOrDefault(d => d.Distribution == Distribution.EKS);
        eks.Should().NotBeNull();
        eks!.Category.Should().Be("managed");
    }

    [Fact]
    public void GetAll_K3sHasLightweightCategory()
    {
        // Act
        var result = _controller.GetAll();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var distributions = okResult.Value.Should().BeAssignableTo<IEnumerable<DistributionInfo>>().Subject;
        var k3s = distributions.FirstOrDefault(d => d.Distribution == Distribution.K3s);
        k3s.Should().NotBeNull();
        k3s!.Category.Should().Be("lightweight");
    }

    #endregion

    #region Get Tests

    [Theory]
    [InlineData(Distribution.OpenShift)]
    [InlineData(Distribution.Kubernetes)]
    [InlineData(Distribution.Rancher)]
    [InlineData(Distribution.EKS)]
    [InlineData(Distribution.AKS)]
    [InlineData(Distribution.GKE)]
    [InlineData(Distribution.K3s)]
    public void Get_ValidDistribution_ReturnsOk(Distribution distribution)
    {
        // Act
        var result = _controller.Get(distribution);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void Get_ValidDistribution_ReturnsDistributionInfo()
    {
        // Act
        var result = _controller.Get(Distribution.OpenShift);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var distInfo = okResult.Value.Should().BeOfType<DistributionInfo>().Subject;
        distInfo.Distribution.Should().Be(Distribution.OpenShift);
    }

    [Fact]
    public void Get_ValidDistribution_CallsService()
    {
        // Act
        _controller.Get(Distribution.EKS);

        // Assert
        _mockDistributionService.Received(1).GetConfig(Distribution.EKS);
    }

    [Fact]
    public void Get_ServiceThrowsArgumentException_ReturnsNotFound()
    {
        // Arrange
        _mockDistributionService.GetConfig(Arg.Any<Distribution>())
            .Returns(x => throw new ArgumentException("Not found"));

        // Act
        var result = _controller.Get(Distribution.OpenShift);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public void Get_OpenShift_ReturnsCorrectName()
    {
        // Act
        var result = _controller.Get(Distribution.OpenShift);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var distInfo = okResult.Value.Should().BeOfType<DistributionInfo>().Subject;
        distInfo.Name.Should().Be("Red Hat OpenShift");
    }

    [Fact]
    public void Get_Kubernetes_ReturnsCorrectName()
    {
        // Act
        var result = _controller.Get(Distribution.Kubernetes);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var distInfo = okResult.Value.Should().BeOfType<DistributionInfo>().Subject;
        distInfo.Name.Should().Be("Vanilla Kubernetes");
    }

    [Fact]
    public void Get_EKS_ReturnsCorrectName()
    {
        // Act
        var result = _controller.Get(Distribution.EKS);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var distInfo = okResult.Value.Should().BeOfType<DistributionInfo>().Subject;
        distInfo.Name.Should().Be("Amazon EKS");
    }

    [Fact]
    public void Get_Distribution_IncludesWorkerSpecs()
    {
        // Act
        var result = _controller.Get(Distribution.OpenShift);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var distInfo = okResult.Value.Should().BeOfType<DistributionInfo>().Subject;
        distInfo.ProdWorkerSpecs.Should().NotBeNull();
        distInfo.NonProdWorkerSpecs.Should().NotBeNull();
    }

    [Fact]
    public void Get_Distribution_IncludesControlPlaneInfo()
    {
        // Act
        var result = _controller.Get(Distribution.EKS);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var distInfo = okResult.Value.Should().BeOfType<DistributionInfo>().Subject;
        // Just verify property is accessible - managed services have managed control plane
        distInfo.HasManagedControlPlane.Should().BeTrue();
    }

    [Fact]
    public void Get_OpenShift_HasInfraNodes()
    {
        // Act
        var result = _controller.Get(Distribution.OpenShift);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var distInfo = okResult.Value.Should().BeOfType<DistributionInfo>().Subject;
        distInfo.HasInfraNodes.Should().BeTrue();
    }

    #endregion

    #region DistributionInfo Tests

    [Fact]
    public void DistributionInfo_DefaultValuesAreCorrect()
    {
        // Arrange & Act
        var info = new DistributionInfo();

        // Assert
        info.Name.Should().BeEmpty();
        info.Category.Should().BeEmpty();
        info.HasManagedControlPlane.Should().BeFalse();
        info.HasInfraNodes.Should().BeFalse();
    }

    #endregion

    #region Category Tests

    [Theory]
    [InlineData(Distribution.OpenShift, "enterprise")]
    [InlineData(Distribution.Tanzu, "enterprise")]
    [InlineData(Distribution.EKS, "managed")]
    [InlineData(Distribution.AKS, "managed")]
    [InlineData(Distribution.GKE, "managed")]
    [InlineData(Distribution.K3s, "lightweight")]
    [InlineData(Distribution.MicroK8s, "lightweight")]
    [InlineData(Distribution.Kubernetes, "standard")]
    [InlineData(Distribution.Rancher, "standard")]
    public void GetAll_DistributionHasCorrectCategory(Distribution distribution, string expectedCategory)
    {
        // Act
        var result = _controller.GetAll();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var distributions = okResult.Value.Should().BeAssignableTo<IEnumerable<DistributionInfo>>().Subject;
        var dist = distributions.FirstOrDefault(d => d.Distribution == distribution);
        dist.Should().NotBeNull();
        dist!.Category.Should().Be(expectedCategory);
    }

    #endregion

    #region Helper Methods

    private DistributionConfig CreateDistributionConfig(Distribution distribution)
    {
        var hasManagedControlPlane = distribution switch
        {
            Distribution.EKS or Distribution.AKS or Distribution.GKE or Distribution.OKE => true,
            _ => false
        };

        var hasInfraNodes = distribution switch
        {
            Distribution.OpenShift or Distribution.Tanzu => true,
            _ => false
        };

        var name = distribution switch
        {
            Distribution.OpenShift => "Red Hat OpenShift",
            Distribution.Kubernetes => "Vanilla Kubernetes",
            Distribution.EKS => "Amazon EKS",
            Distribution.AKS => "Azure AKS",
            Distribution.GKE => "Google GKE",
            Distribution.Rancher => "Rancher Kubernetes",
            Distribution.Tanzu => "VMware Tanzu",
            Distribution.K3s => "K3s",
            Distribution.MicroK8s => "MicroK8s",
            Distribution.OKE => "Oracle OKE",
            _ => distribution.ToString()
        };

        var vendor = distribution switch
        {
            Distribution.OpenShift => "Red Hat",
            Distribution.EKS => "AWS",
            Distribution.AKS => "Azure",
            Distribution.GKE => "Google Cloud",
            Distribution.Rancher => "SUSE",
            Distribution.Tanzu => "VMware",
            Distribution.K3s => "SUSE",
            Distribution.MicroK8s => "Canonical",
            Distribution.OKE => "Oracle",
            _ => "CNCF"
        };

        return new DistributionConfig
        {
            Distribution = distribution,
            Name = name,
            Vendor = vendor,
            HasManagedControlPlane = hasManagedControlPlane,
            HasInfraNodes = hasInfraNodes,
            ProdControlPlane = new NodeSpecs(4, 16, 100),
            NonProdControlPlane = new NodeSpecs(2, 8, 50),
            ProdWorker = new NodeSpecs(8, 32, 100),
            NonProdWorker = new NodeSpecs(4, 16, 50),
            ProdInfra = hasInfraNodes ? new NodeSpecs(4, 16, 100) : NodeSpecs.Zero,
            NonProdInfra = hasInfraNodes ? new NodeSpecs(2, 8, 50) : NodeSpecs.Zero
        };
    }

    #endregion
}
