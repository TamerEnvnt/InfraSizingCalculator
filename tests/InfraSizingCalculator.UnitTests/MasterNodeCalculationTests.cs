using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Services;

namespace InfraSizingCalculator.UnitTests;

/// <summary>
/// Tests for master node calculation rules BR-M001 through BR-M004
/// </summary>
public class MasterNodeCalculationTests
{
    private readonly K8sSizingService _service;

    public MasterNodeCalculationTests()
    {
        var distributionService = new DistributionService();
        var technologyService = new TechnologyService();
        _service = new K8sSizingService(distributionService, technologyService);
    }

    /// <summary>
    /// BR-M001: Managed control plane (EKS, AKS, GKE) = 0 masters
    /// </summary>
    [Theory]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(100)]
    [InlineData(500)]
    public void ManagedControlPlane_Returns0Masters(int workerCount)
    {
        var result = _service.CalculateMasterNodes(workerCount, isManagedControlPlane: true);
        Assert.Equal(0, result);
    }

    /// <summary>
    /// BR-M002, BR-M004: Standard HA quorum = 3 masters for clusters <= 100 workers
    /// </summary>
    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(50)]
    [InlineData(100)]
    public void SelfManaged_SmallCluster_Returns3Masters(int workerCount)
    {
        var result = _service.CalculateMasterNodes(workerCount, isManagedControlPlane: false);
        Assert.Equal(3, result);
    }

    /// <summary>
    /// BR-M003: Large clusters (> 100 workers) need 5 masters
    /// </summary>
    [Theory]
    [InlineData(101)]
    [InlineData(200)]
    [InlineData(500)]
    public void SelfManaged_LargeCluster_Returns5Masters(int workerCount)
    {
        var result = _service.CalculateMasterNodes(workerCount, isManagedControlPlane: false);
        Assert.Equal(5, result);
    }

    /// <summary>
    /// Verify managed distributions return 0 masters
    /// </summary>
    [Fact]
    public void EKS_Returns0Masters()
    {
        var distributionService = new DistributionService();
        var config = distributionService.GetConfig(Distribution.EKS);

        Assert.True(config.HasManagedControlPlane);
        Assert.Equal(0, config.ProdControlPlane.Cpu);
    }

    [Fact]
    public void AKS_Returns0Masters()
    {
        var distributionService = new DistributionService();
        var config = distributionService.GetConfig(Distribution.AKS);

        Assert.True(config.HasManagedControlPlane);
    }

    [Fact]
    public void GKE_Returns0Masters()
    {
        var distributionService = new DistributionService();
        var config = distributionService.GetConfig(Distribution.GKE);

        Assert.True(config.HasManagedControlPlane);
    }

    /// <summary>
    /// Verify self-managed distributions have control plane specs
    /// </summary>
    [Fact]
    public void OpenShift_HasControlPlaneSpecs()
    {
        var distributionService = new DistributionService();
        var config = distributionService.GetConfig(Distribution.OpenShift);

        Assert.False(config.HasManagedControlPlane);
        Assert.Equal(8, config.ProdControlPlane.Cpu);
        Assert.Equal(32, config.ProdControlPlane.Ram);
    }
}
