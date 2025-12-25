using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Services;

namespace InfraSizingCalculator.UnitTests;

/// <summary>
/// Tests for DistributionService
/// </summary>
public class DistributionServiceTests
{
    private readonly DistributionService _service;

    public DistributionServiceTests()
    {
        _service = new DistributionService();
    }

    /// <summary>
    /// Verify all distributions are available
    /// </summary>
    [Fact]
    public void GetAll_Returns34Distributions()
    {
        var distributions = _service.GetAll();
        Assert.Equal(34, distributions.Count());
    }

    /// <summary>
    /// Verify each distribution can be retrieved
    /// </summary>
    [Theory]
    [InlineData(Distribution.OpenShift)]
    [InlineData(Distribution.Kubernetes)]
    [InlineData(Distribution.EKS)]
    [InlineData(Distribution.AKS)]
    [InlineData(Distribution.GKE)]
    [InlineData(Distribution.OKE)]
    [InlineData(Distribution.Rancher)]
    [InlineData(Distribution.K3s)]
    [InlineData(Distribution.MicroK8s)]
    [InlineData(Distribution.Charmed)]
    [InlineData(Distribution.Tanzu)]
    public void GetConfig_ReturnsDistributionConfig(Distribution distro)
    {
        var config = _service.GetConfig(distro);

        Assert.NotNull(config);
        Assert.Equal(distro, config.Distribution);
        Assert.NotNull(config.Name);
        Assert.NotNull(config.Vendor);
    }

    /// <summary>
    /// Verify managed distributions have managed control plane
    /// </summary>
    [Theory]
    [InlineData(Distribution.EKS)]
    [InlineData(Distribution.AKS)]
    [InlineData(Distribution.GKE)]
    public void ManagedDistributions_HaveManagedControlPlane(Distribution distro)
    {
        var config = _service.GetConfig(distro);
        Assert.True(config.HasManagedControlPlane);
    }

    /// <summary>
    /// Verify self-managed distributions do not have managed control plane
    /// </summary>
    [Theory]
    [InlineData(Distribution.OpenShift)]
    [InlineData(Distribution.Kubernetes)]
    [InlineData(Distribution.Rancher)]
    [InlineData(Distribution.K3s)]
    [InlineData(Distribution.MicroK8s)]
    [InlineData(Distribution.Charmed)]
    [InlineData(Distribution.Tanzu)]
    public void SelfManagedDistributions_DoNotHaveManagedControlPlane(Distribution distro)
    {
        var config = _service.GetConfig(distro);
        Assert.False(config.HasManagedControlPlane);
    }

    /// <summary>
    /// Verify OpenShift variants have infra nodes
    /// </summary>
    [Fact]
    public void OpenShiftVariants_HaveInfraNodes()
    {
        var all = _service.GetAll();
        var withInfra = all.Where(d => d.HasInfraNodes).ToList();

        // OpenShift, OpenShiftROSA, OpenShiftARO, OpenShiftDedicated, OpenShiftIBM have infra nodes
        Assert.Equal(5, withInfra.Count);
        Assert.All(withInfra, c => Assert.Contains("OpenShift", c.Name));
    }

    /// <summary>
    /// Verify OpenShift has correct production control plane specs
    /// </summary>
    [Fact]
    public void OpenShift_HasCorrectProdControlPlaneSpecs()
    {
        var config = _service.GetConfig(Distribution.OpenShift);

        Assert.Equal(8, config.ProdControlPlane.Cpu);
        Assert.Equal(32, config.ProdControlPlane.Ram);
        Assert.Equal(200, config.ProdControlPlane.Disk);
    }

    /// <summary>
    /// Verify OpenShift has correct production worker specs
    /// </summary>
    [Fact]
    public void OpenShift_HasCorrectProdWorkerSpecs()
    {
        var config = _service.GetConfig(Distribution.OpenShift);

        Assert.Equal(16, config.ProdWorker.Cpu);
        Assert.Equal(64, config.ProdWorker.Ram);
        Assert.Equal(200, config.ProdWorker.Disk);
    }

    /// <summary>
    /// Verify OpenShift has correct production infra specs
    /// </summary>
    [Fact]
    public void OpenShift_HasCorrectProdInfraSpecs()
    {
        var config = _service.GetConfig(Distribution.OpenShift);

        Assert.Equal(8, config.ProdInfra.Cpu);
        Assert.Equal(32, config.ProdInfra.Ram);
        Assert.Equal(500, config.ProdInfra.Disk);
    }

    /// <summary>
    /// Verify managed distributions have zero control plane specs
    /// </summary>
    [Theory]
    [InlineData(Distribution.EKS)]
    [InlineData(Distribution.AKS)]
    [InlineData(Distribution.GKE)]
    public void ManagedDistributions_HaveZeroControlPlaneSpecs(Distribution distro)
    {
        var config = _service.GetConfig(distro);

        Assert.Equal(0, config.ProdControlPlane.Cpu);
        Assert.Equal(0, config.ProdControlPlane.Ram);
        Assert.Equal(0, config.ProdControlPlane.Disk);
    }

    /// <summary>
    /// Verify distributions can be filtered by HasManagedControlPlane
    /// </summary>
    [Fact]
    public void GetAll_FilterOnPremises_Returns9()
    {
        var onPrem = _service.GetAll().Where(d => !d.HasManagedControlPlane);
        Assert.Equal(9, onPrem.Count());
    }

    [Fact]
    public void GetAll_FilterCloudManaged_Returns25()
    {
        var cloud = _service.GetAll().Where(d => d.HasManagedControlPlane);
        Assert.Equal(25, cloud.Count());
    }

    /// <summary>
    /// Verify distribution vendors
    /// </summary>
    [Theory]
    [InlineData(Distribution.OpenShift, "Red Hat")]
    [InlineData(Distribution.EKS, "AWS")]
    [InlineData(Distribution.AKS, "Microsoft")]
    [InlineData(Distribution.GKE, "Google")]
    [InlineData(Distribution.Rancher, "SUSE")]
    [InlineData(Distribution.K3s, "SUSE")]
    public void Distribution_HasCorrectVendor(Distribution distro, string expectedVendor)
    {
        var config = _service.GetConfig(distro);
        Assert.Equal(expectedVendor, config.Vendor);
    }

    /// <summary>
    /// Verify distribution names
    /// </summary>
    [Theory]
    [InlineData(Distribution.OpenShift, "OpenShift (On-Prem)")]
    [InlineData(Distribution.Kubernetes, "Vanilla Kubernetes")]
    [InlineData(Distribution.EKS, "Amazon EKS")]
    [InlineData(Distribution.AKS, "Azure AKS")]
    [InlineData(Distribution.GKE, "Google GKE")]
    public void Distribution_HasCorrectName(Distribution distro, string expectedName)
    {
        var config = _service.GetConfig(distro);
        Assert.Equal(expectedName, config.Name);
    }
}
