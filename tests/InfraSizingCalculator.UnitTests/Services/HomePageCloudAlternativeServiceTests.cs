using FluentAssertions;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Services;
using InfraSizingCalculator.Services.Interfaces;
using NSubstitute;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Services;

public class HomePageCloudAlternativeServiceTests
{
    private readonly HomePageCloudAlternativeService _sut;
    private readonly IHomePageCostService _costService;
    private readonly IHomePageDistributionService _distributionService;

    public HomePageCloudAlternativeServiceTests()
    {
        _sut = new HomePageCloudAlternativeService();
        _costService = Substitute.For<IHomePageCostService>();
        _distributionService = Substitute.For<IHomePageDistributionService>();
    }

    private static K8sSizingResult CreateSizingResult(int workers, int totalCpu, int totalRam)
    {
        return new K8sSizingResult
        {
            Environments = new List<EnvironmentResult>(),
            Configuration = new K8sSizingInput
            {
                Technology = Technology.DotNet,
                Distribution = Distribution.Kubernetes,
                ClusterMode = ClusterMode.MultiCluster,
                EnabledEnvironments = new HashSet<EnvironmentType> { EnvironmentType.Prod },
                EnvironmentApps = new Dictionary<EnvironmentType, AppConfig>
                {
                    { EnvironmentType.Prod, new AppConfig { Medium = 10 } }
                }
            },
            GrandTotal = new GrandTotal
            {
                TotalWorkers = workers,
                TotalCpu = totalCpu,
                TotalRam = totalRam
            }
        };
    }

    #region GetInstanceType Tests

    [Theory]
    [InlineData("aws", 4, 16, "m6i.xlarge")]
    [InlineData("aws", 8, 32, "m6i.2xlarge")]
    [InlineData("aws", 16, 64, "m6i.4xlarge")]
    [InlineData("aws", 32, 128, "m6i.4xlarge")] // Max
    public void GetInstanceType_AWS_ReturnsCorrectType(string provider, decimal cpu, decimal ram, string expected)
    {
        var result = _sut.GetInstanceType(provider, cpu, ram);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("azure", 4, 16, "Standard_D4s_v5")]
    [InlineData("azure", 8, 32, "Standard_D8s_v5")]
    [InlineData("azure", 16, 64, "Standard_D16s_v5")]
    public void GetInstanceType_Azure_ReturnsCorrectType(string provider, decimal cpu, decimal ram, string expected)
    {
        var result = _sut.GetInstanceType(provider, cpu, ram);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("gcp", 4, 16, "n2-standard-4")]
    [InlineData("gcp", 8, 32, "n2-standard-8")]
    [InlineData("gcp", 16, 64, "n2-standard-16")]
    public void GetInstanceType_GCP_ReturnsCorrectType(string provider, decimal cpu, decimal ram, string expected)
    {
        var result = _sut.GetInstanceType(provider, cpu, ram);

        result.Should().Be(expected);
    }

    [Fact]
    public void GetInstanceType_UnknownProvider_ReturnsStandard()
    {
        var result = _sut.GetInstanceType("unknown", 8, 32);

        result.Should().Be("standard");
    }

    [Fact]
    public void GetInstanceType_IsCaseInsensitive()
    {
        var result1 = _sut.GetInstanceType("AWS", 8, 32);
        var result2 = _sut.GetInstanceType("aws", 8, 32);
        var result3 = _sut.GetInstanceType("Aws", 8, 32);

        result1.Should().Be(result2).And.Be(result3);
    }

    #endregion

    #region GetCloudAlternativesForBreakdown Tests

    [Fact]
    public void GetCloudAlternativesForBreakdown_WithNullResults_ReturnsDefaultAlternatives()
    {
        _costService.GetComputeCost(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
            .Returns(500m);
        _distributionService.IsOpenShiftDistribution(Arg.Any<Distribution?>())
            .Returns(false);

        var result = _sut.GetCloudAlternativesForBreakdown(null, Distribution.Kubernetes, _costService, _distributionService);

        result.Should().HaveCount(3);
        result.Should().Contain(a => a.Key == "eks");
        result.Should().Contain(a => a.Key == "aks");
        result.Should().Contain(a => a.Key == "gke");
    }

    [Fact]
    public void GetCloudAlternativesForBreakdown_WithResults_CalculatesCorrectly()
    {
        var results = CreateSizingResult(6, 96, 384);

        _costService.GetComputeCost("aws", "m6i.4xlarge", 6).Returns(1000m);
        _costService.GetComputeCost("azure", "Standard_D16s_v5", 6).Returns(950m);
        _costService.GetComputeCost("gcp", "n2-standard-16", 6).Returns(900m);
        _distributionService.IsOpenShiftDistribution(Arg.Any<Distribution?>()).Returns(false);

        var result = _sut.GetCloudAlternativesForBreakdown(results, Distribution.Kubernetes, _costService, _distributionService);

        result.Should().HaveCount(3);

        var eks = result.First(a => a.Key == "eks");
        eks.InstanceType.Should().Be("m6i.4xlarge");
        eks.ComputeCost.Should().Be(1000m);

        var aks = result.First(a => a.Key == "aks");
        aks.InstanceType.Should().Be("Standard_D16s_v5");
        aks.ControlPlaneCost.Should().Be(0); // AKS has free control plane
    }

    [Fact]
    public void GetCloudAlternativesForBreakdown_ForOpenShift_IncludesManagedOptions()
    {
        var results = CreateSizingResult(3, 48, 192);

        _costService.GetComputeCost(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
            .Returns(500m);
        _distributionService.IsOpenShiftDistribution(Distribution.OpenShift)
            .Returns(true);

        var result = _sut.GetCloudAlternativesForBreakdown(results, Distribution.OpenShift, _costService, _distributionService);

        result.Should().HaveCount(6); // EKS, AKS, GKE + ROSA, ARO, OSD
        result.Should().Contain(a => a.Key == "rosa");
        result.Should().Contain(a => a.Key == "aro");
        result.Should().Contain(a => a.Key == "osd");
    }

    [Fact]
    public void GetCloudAlternativesForBreakdown_OpenShiftManaged_HasLicenseCost()
    {
        var results = CreateSizingResult(3, 48, 192);

        _costService.GetComputeCost(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
            .Returns(500m);
        _distributionService.IsOpenShiftDistribution(Distribution.OpenShift)
            .Returns(true);

        var result = _sut.GetCloudAlternativesForBreakdown(results, Distribution.OpenShift, _costService, _distributionService);

        var rosa = result.First(a => a.Key == "rosa");
        rosa.IsOpenShiftManaged.Should().BeTrue();
        rosa.LicenseCost.Should().BeGreaterThan(0);
        rosa.TargetDistribution.Should().Be(Distribution.OpenShiftROSA);
    }

    [Fact]
    public void GetCloudAlternativesForBreakdown_VanillaK8s_NoOpenShiftOptions()
    {
        var results = CreateSizingResult(3, 48, 192);

        _costService.GetComputeCost(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
            .Returns(500m);
        _distributionService.IsOpenShiftDistribution(Distribution.Kubernetes)
            .Returns(false);

        var result = _sut.GetCloudAlternativesForBreakdown(results, Distribution.Kubernetes, _costService, _distributionService);

        result.Should().HaveCount(3);
        result.Should().NotContain(a => a.Key == "rosa");
        result.Should().NotContain(a => a.Key == "aro");
        result.Should().NotContain(a => a.Key == "osd");
    }

    [Fact]
    public void GetCloudAlternativesForBreakdown_CalculatesStorageCostCorrectly()
    {
        var results = CreateSizingResult(10, 160, 640);

        _costService.GetComputeCost(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
            .Returns(2000m);
        _distributionService.IsOpenShiftDistribution(Arg.Any<Distribution?>())
            .Returns(false);

        var result = _sut.GetCloudAlternativesForBreakdown(results, Distribution.EKS, _costService, _distributionService);

        var eks = result.First(a => a.Key == "eks");
        // 10 workers * 50GB * $0.10/GB = $50
        eks.StorageCost.Should().Be(50m);

        var aks = result.First(a => a.Key == "aks");
        // 10 workers * 50GB * $0.08/GB = $40
        aks.StorageCost.Should().Be(40m);
    }

    [Fact]
    public void GetCloudAlternativesForBreakdown_SetsCorrectTargetDistributions()
    {
        _costService.GetComputeCost(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
            .Returns(500m);
        _distributionService.IsOpenShiftDistribution(Arg.Any<Distribution?>())
            .Returns(false);

        var result = _sut.GetCloudAlternativesForBreakdown(null, Distribution.Kubernetes, _costService, _distributionService);

        result.First(a => a.Key == "eks").TargetDistribution.Should().Be(Distribution.EKS);
        result.First(a => a.Key == "aks").TargetDistribution.Should().Be(Distribution.AKS);
        result.First(a => a.Key == "gke").TargetDistribution.Should().Be(Distribution.GKE);
    }

    [Fact]
    public void GetCloudAlternativesForBreakdown_CalculatesTotalMonthlyCostCorrectly()
    {
        var results = CreateSizingResult(5, 80, 320);

        _costService.GetComputeCost("aws", Arg.Any<string>(), 5).Returns(1000m);
        _costService.GetComputeCost("azure", Arg.Any<string>(), 5).Returns(900m);
        _costService.GetComputeCost("gcp", Arg.Any<string>(), 5).Returns(950m);
        _distributionService.IsOpenShiftDistribution(Arg.Any<Distribution?>()).Returns(false);

        var result = _sut.GetCloudAlternativesForBreakdown(results, Distribution.Kubernetes, _costService, _distributionService);

        var eks = result.First(a => a.Key == "eks");
        // 1000 compute + 73 control plane + (5 * 50 * 0.10) storage = 1098
        eks.MonthlyCost.Should().Be(1098m);

        var aks = result.First(a => a.Key == "aks");
        // 900 compute + 0 control plane + (5 * 50 * 0.08) storage = 920
        aks.MonthlyCost.Should().Be(920m);

        var gke = result.First(a => a.Key == "gke");
        // 950 compute + 73 control plane + (5 * 50 * 0.08) storage = 1043
        gke.MonthlyCost.Should().Be(1043m);
    }

    [Fact]
    public void GetCloudAlternativesForBreakdown_HandlesMinimumWorkerCount()
    {
        var results = CreateSizingResult(0, 0, 0);

        _costService.GetComputeCost(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
            .Returns(100m);
        _distributionService.IsOpenShiftDistribution(Arg.Any<Distribution?>())
            .Returns(false);

        // Should not throw and should use default values
        var result = _sut.GetCloudAlternativesForBreakdown(results, Distribution.Kubernetes, _costService, _distributionService);

        result.Should().HaveCount(3);
    }

    #endregion

    #region Integration Scenarios

    [Fact]
    public void Scenario_LargeEnterpriseDeployment_CalculatesCorrectly()
    {
        var results = CreateSizingResult(20, 320, 1280);

        _costService.GetComputeCost("aws", "m6i.4xlarge", 20).Returns(5000m);
        _costService.GetComputeCost("azure", "Standard_D16s_v5", 20).Returns(4800m);
        _costService.GetComputeCost("gcp", "n2-standard-16", 20).Returns(4600m);
        _distributionService.IsOpenShiftDistribution(Distribution.OpenShift).Returns(true);

        var result = _sut.GetCloudAlternativesForBreakdown(results, Distribution.OpenShift, _costService, _distributionService);

        // All three major cloud providers should be included
        result.Should().Contain(a => a.Key == "eks");
        result.Should().Contain(a => a.Key == "aks");
        result.Should().Contain(a => a.Key == "gke");

        // OpenShift managed options should be included
        result.Should().Contain(a => a.Key == "rosa");
        result.Should().Contain(a => a.Key == "aro");
        result.Should().Contain(a => a.Key == "osd");

        // Each alternative should have reasonable costs
        foreach (var alt in result)
        {
            alt.MonthlyCost.Should().BeGreaterThan(0);
            alt.ComputeCost.Should().BeGreaterThan(0);
        }
    }

    [Fact]
    public void Scenario_SmallStartupDeployment_UsesSmallInstanceTypes()
    {
        var results = CreateSizingResult(3, 12, 48);

        _costService.GetComputeCost(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
            .Returns(200m);
        _distributionService.IsOpenShiftDistribution(Arg.Any<Distribution?>())
            .Returns(false);

        var result = _sut.GetCloudAlternativesForBreakdown(results, Distribution.Kubernetes, _costService, _distributionService);

        // With 4 vCPU per worker, should recommend small instance types
        var eks = result.First(a => a.Key == "eks");
        eks.InstanceType.Should().Be("m6i.xlarge"); // 4 vCPU

        var aks = result.First(a => a.Key == "aks");
        aks.InstanceType.Should().Be("Standard_D4s_v5"); // 4 vCPU

        var gke = result.First(a => a.Key == "gke");
        gke.InstanceType.Should().Be("n2-standard-4"); // 4 vCPU
    }

    #endregion
}
