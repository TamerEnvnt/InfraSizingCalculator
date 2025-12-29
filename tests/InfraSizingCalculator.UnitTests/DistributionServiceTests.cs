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

    #region GetByDeploymentType Tests

    [Fact]
    public void GetByDeploymentType_OnPrem_ReturnsOnPremDistributions()
    {
        var onPremDistros = _service.GetByDeploymentType("on-prem").ToList();

        Assert.NotEmpty(onPremDistros);
        Assert.All(onPremDistros, d => Assert.Contains("on-prem", d.Tags));
    }

    [Fact]
    public void GetByDeploymentType_Cloud_ReturnsCloudDistributions()
    {
        var cloudDistros = _service.GetByDeploymentType("cloud").ToList();

        Assert.NotEmpty(cloudDistros);
        Assert.All(cloudDistros, d => Assert.Contains("cloud", d.Tags));
    }

    [Fact]
    public void GetByDeploymentType_All_ReturnsAllDistributions()
    {
        var allDistros = _service.GetByDeploymentType("all").ToList();
        var expected = _service.GetAll().Count();

        Assert.Equal(expected, allDistros.Count);
    }

    [Fact]
    public void GetByDeploymentType_Null_ReturnsAllDistributions()
    {
        var allDistros = _service.GetByDeploymentType(null!).ToList();
        var expected = _service.GetAll().Count();

        Assert.Equal(expected, allDistros.Count);
    }

    [Fact]
    public void GetCountByDeploymentType_OnPrem_ReturnsCorrectCount()
    {
        var count = _service.GetCountByDeploymentType("on-prem");
        var expected = _service.GetByDeploymentType("on-prem").Count();

        Assert.Equal(expected, count);
        Assert.True(count > 0);
    }

    [Fact]
    public void GetCountByDeploymentType_Cloud_ReturnsCorrectCount()
    {
        var count = _service.GetCountByDeploymentType("cloud");
        var expected = _service.GetByDeploymentType("cloud").Count();

        Assert.Equal(expected, count);
        Assert.True(count > 0);
    }

    #endregion

    #region GetCloudByCategory Tests

    [Fact]
    public void GetCloudByCategory_All_ReturnsAllCloudDistributions()
    {
        var allCloud = _service.GetCloudByCategory("all").ToList();
        var expectedCloud = _service.GetByTag("cloud").ToList();

        Assert.Equal(expectedCloud.Count, allCloud.Count);
    }

    [Theory]
    [InlineData(Distribution.EKS, "major")]
    [InlineData(Distribution.AKS, "major")]
    [InlineData(Distribution.GKE, "major")]
    [InlineData(Distribution.OKE, "major")]
    [InlineData(Distribution.IKS, "major")]
    [InlineData(Distribution.ACK, "major")]
    [InlineData(Distribution.TKE, "major")]
    [InlineData(Distribution.CCE, "major")]
    public void GetCloudByCategory_Major_IncludesMajorCloudProviders(Distribution distro, string category)
    {
        var categoryDistros = _service.GetCloudByCategory(category);

        Assert.Contains(categoryDistros, d => d.Distribution == distro);
    }

    [Theory]
    [InlineData(Distribution.OpenShiftROSA, "openshift")]
    [InlineData(Distribution.OpenShiftARO, "openshift")]
    [InlineData(Distribution.OpenShiftDedicated, "openshift")]
    [InlineData(Distribution.OpenShiftIBM, "openshift")]
    public void GetCloudByCategory_OpenShift_IncludesOpenShiftCloudVariants(Distribution distro, string category)
    {
        var categoryDistros = _service.GetCloudByCategory(category);

        Assert.Contains(categoryDistros, d => d.Distribution == distro);
    }

    [Theory]
    [InlineData(Distribution.RancherHosted, "rancher")]
    [InlineData(Distribution.RancherEKS, "rancher")]
    [InlineData(Distribution.RancherAKS, "rancher")]
    [InlineData(Distribution.RancherGKE, "rancher")]
    public void GetCloudByCategory_Rancher_IncludesRancherFamily(Distribution distro, string category)
    {
        var categoryDistros = _service.GetCloudByCategory(category);

        Assert.Contains(categoryDistros, d => d.Distribution == distro);
    }

    [Theory]
    [InlineData(Distribution.TanzuCloud, "tanzu")]
    [InlineData(Distribution.TanzuAWS, "tanzu")]
    [InlineData(Distribution.TanzuAzure, "tanzu")]
    [InlineData(Distribution.TanzuGCP, "tanzu")]
    public void GetCloudByCategory_Tanzu_IncludesTanzuFamily(Distribution distro, string category)
    {
        var categoryDistros = _service.GetCloudByCategory(category);

        Assert.Contains(categoryDistros, d => d.Distribution == distro);
    }

    [Theory]
    [InlineData(Distribution.DOKS, "developer")]
    [InlineData(Distribution.LKE, "developer")]
    [InlineData(Distribution.VKE, "developer")]
    [InlineData(Distribution.HetznerK8s, "developer")]
    [InlineData(Distribution.OVHKubernetes, "developer")]
    [InlineData(Distribution.ScalewayKapsule, "developer")]
    public void GetCloudByCategory_Developer_IncludesDeveloperFocused(Distribution distro, string category)
    {
        var categoryDistros = _service.GetCloudByCategory(category);

        Assert.Contains(categoryDistros, d => d.Distribution == distro);
    }

    [Fact]
    public void GetCloudByCategory_Invalid_ReturnsEmpty()
    {
        var result = _service.GetCloudByCategory("invalid-category");

        Assert.Empty(result);
    }

    [Theory]
    [InlineData("major")]
    [InlineData("openshift")]
    [InlineData("rancher")]
    [InlineData("tanzu")]
    [InlineData("developer")]
    public void GetCountByCloudCategory_ReturnsCorrectCount(string category)
    {
        var count = _service.GetCountByCloudCategory(category);
        var expected = _service.GetCloudByCategory(category).Count();

        Assert.Equal(expected, count);
        Assert.True(count > 0);
    }

    #endregion

    #region GetFiltered Tests

    [Fact]
    public void GetFiltered_OnPrem_ReturnsOnPremDistributions()
    {
        var filtered = _service.GetFiltered("on-prem", null, null).ToList();

        Assert.NotEmpty(filtered);
        Assert.All(filtered, d => Assert.Contains("on-prem", d.Tags));
    }

    [Fact]
    public void GetFiltered_CloudWithCategory_ReturnsCategoryDistributions()
    {
        var filtered = _service.GetFiltered("cloud", "major", null).ToList();

        Assert.NotEmpty(filtered);
        Assert.All(filtered, d => Assert.Contains("cloud", d.Tags));
    }

    [Fact]
    public void GetFiltered_WithSearchText_FiltersResults()
    {
        var filtered = _service.GetFiltered(null, null, "AWS").ToList();

        Assert.NotEmpty(filtered);
        Assert.All(filtered, d => Assert.True(
            d.Name.Contains("AWS", StringComparison.OrdinalIgnoreCase) ||
            d.Vendor.Contains("AWS", StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public void GetFiltered_WithSearchText_CaseInsensitive()
    {
        var upperCase = _service.GetFiltered(null, null, "AWS").ToList();
        var lowerCase = _service.GetFiltered(null, null, "aws").ToList();
        var mixedCase = _service.GetFiltered(null, null, "Aws").ToList();

        Assert.Equal(upperCase.Count, lowerCase.Count);
        Assert.Equal(upperCase.Count, mixedCase.Count);
    }

    [Fact]
    public void GetFiltered_CloudWithSearchText_CombinesFilters()
    {
        var filtered = _service.GetFiltered("cloud", null, "Red Hat").ToList();

        Assert.NotEmpty(filtered);
        Assert.All(filtered, d =>
        {
            Assert.Contains("cloud", d.Tags);
            Assert.True(
                d.Name.Contains("Red Hat", StringComparison.OrdinalIgnoreCase) ||
                d.Vendor.Contains("Red Hat", StringComparison.OrdinalIgnoreCase));
        });
    }

    #endregion

    #region GetCloudCategory Tests

    [Theory]
    [InlineData(Distribution.EKS, "major")]
    [InlineData(Distribution.AKS, "major")]
    [InlineData(Distribution.GKE, "major")]
    [InlineData(Distribution.OpenShiftROSA, "openshift")]
    [InlineData(Distribution.RancherHosted, "rancher")]
    [InlineData(Distribution.TanzuCloud, "tanzu")]
    [InlineData(Distribution.DOKS, "developer")]
    public void GetCloudCategory_ReturnsCorrectCategory(Distribution distro, string expectedCategory)
    {
        var category = _service.GetCloudCategory(distro);

        Assert.Equal(expectedCategory, category);
    }

    [Theory]
    [InlineData(Distribution.OpenShift)]
    [InlineData(Distribution.Kubernetes)]
    [InlineData(Distribution.Rancher)]
    [InlineData(Distribution.K3s)]
    public void GetCloudCategory_OnPremDistributions_ReturnsNull(Distribution distro)
    {
        var category = _service.GetCloudCategory(distro);

        Assert.Null(category);
    }

    #endregion
}
