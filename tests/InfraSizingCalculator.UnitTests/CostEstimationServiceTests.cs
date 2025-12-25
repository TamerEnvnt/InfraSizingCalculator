using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Models.Pricing;
using InfraSizingCalculator.Services;
using InfraSizingCalculator.Services.Interfaces;
using InfraSizingCalculator.Services.Pricing;
using NSubstitute;

namespace InfraSizingCalculator.UnitTests;

/// <summary>
/// Comprehensive tests for CostEstimationService
/// </summary>
public class CostEstimationServiceTests
{
    private readonly CostEstimationService _service;
    private readonly IPricingService _mockPricingService;

    public CostEstimationServiceTests()
    {
        _mockPricingService = Substitute.For<IPricingService>();
        _service = new CostEstimationService(_mockPricingService);
    }

    #region K8s Cost Estimation Tests

    [Fact]
    public async Task EstimateK8sCostAsync_ValidInput_ReturnsEstimate()
    {
        // Arrange
        var sizing = CreateK8sSizingResult();
        var pricing = DefaultPricingData.GetAWSPricing();
        _mockPricingService.GetPricingAsync(CloudProvider.AWS, "us-east-1", PricingType.OnDemand)
            .Returns(Task.FromResult(pricing));

        // Act
        var result = await _service.EstimateK8sCostAsync(sizing, CloudProvider.AWS, "us-east-1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(CloudProvider.AWS, result.Provider);
        Assert.Equal("us-east-1", result.Region);
        Assert.True(result.MonthlyTotal > 0);
    }

    [Fact]
    public async Task EstimateK8sCostAsync_IncludesComputeCosts()
    {
        // Arrange
        var sizing = CreateK8sSizingResult();
        var pricing = DefaultPricingData.GetAWSPricing();
        _mockPricingService.GetPricingAsync(CloudProvider.AWS, "us-east-1", PricingType.OnDemand)
            .Returns(Task.FromResult(pricing));

        // Act
        var result = await _service.EstimateK8sCostAsync(sizing, CloudProvider.AWS, "us-east-1");

        // Assert
        Assert.Contains(CostCategory.Compute, result.Breakdown.Keys);
        Assert.True(result.Breakdown[CostCategory.Compute].Monthly > 0);
    }

    [Fact]
    public async Task EstimateK8sCostAsync_IncludesStorageCosts_WhenEnabled()
    {
        // Arrange
        var sizing = CreateK8sSizingResult();
        var pricing = DefaultPricingData.GetAWSPricing();
        _mockPricingService.GetPricingAsync(CloudProvider.AWS, "us-east-1", PricingType.OnDemand)
            .Returns(Task.FromResult(pricing));

        var options = new CostEstimationOptions { IncludeStorage = true };

        // Act
        var result = await _service.EstimateK8sCostAsync(sizing, CloudProvider.AWS, "us-east-1", options);

        // Assert
        Assert.Contains(CostCategory.Storage, result.Breakdown.Keys);
    }

    [Fact]
    public async Task EstimateK8sCostAsync_ExcludesStorageCosts_WhenDisabled()
    {
        // Arrange
        var sizing = CreateK8sSizingResult();
        var pricing = DefaultPricingData.GetAWSPricing();
        _mockPricingService.GetPricingAsync(CloudProvider.AWS, "us-east-1", PricingType.OnDemand)
            .Returns(Task.FromResult(pricing));

        var options = new CostEstimationOptions { IncludeStorage = false };

        // Act
        var result = await _service.EstimateK8sCostAsync(sizing, CloudProvider.AWS, "us-east-1", options);

        // Assert
        Assert.DoesNotContain(CostCategory.Storage, result.Breakdown.Keys);
    }

    [Fact]
    public async Task EstimateK8sCostAsync_IncludesNetworkCosts_WhenEnabled()
    {
        // Arrange
        var sizing = CreateK8sSizingResult();
        var pricing = DefaultPricingData.GetAWSPricing();
        _mockPricingService.GetPricingAsync(CloudProvider.AWS, "us-east-1", PricingType.OnDemand)
            .Returns(Task.FromResult(pricing));

        var options = new CostEstimationOptions { IncludeNetwork = true };

        // Act
        var result = await _service.EstimateK8sCostAsync(sizing, CloudProvider.AWS, "us-east-1", options);

        // Assert
        Assert.Contains(CostCategory.Network, result.Breakdown.Keys);
    }

    [Fact]
    public async Task EstimateK8sCostAsync_IncludesLicenseCosts_ForOpenShift()
    {
        // Arrange
        var sizing = CreateK8sSizingResult();
        var pricing = DefaultPricingData.GetAWSPricing();
        _mockPricingService.GetPricingAsync(CloudProvider.AWS, "us-east-1", PricingType.OnDemand)
            .Returns(Task.FromResult(pricing));

        var options = new CostEstimationOptions
        {
            IncludeLicenses = true,
            Distribution = "OpenShift"
        };

        // Act
        var result = await _service.EstimateK8sCostAsync(sizing, CloudProvider.AWS, "us-east-1", options);

        // Assert
        Assert.Contains(CostCategory.License, result.Breakdown.Keys);
        Assert.True(result.Breakdown[CostCategory.License].Monthly > 0);
    }

    [Fact]
    public async Task EstimateK8sCostAsync_NoLicenseCosts_ForVanillaKubernetes()
    {
        // Arrange
        var sizing = CreateK8sSizingResult();
        var pricing = DefaultPricingData.GetAWSPricing();
        _mockPricingService.GetPricingAsync(CloudProvider.AWS, "us-east-1", PricingType.OnDemand)
            .Returns(Task.FromResult(pricing));

        var options = new CostEstimationOptions
        {
            IncludeLicenses = true,
            Distribution = "Kubernetes"
        };

        // Act
        var result = await _service.EstimateK8sCostAsync(sizing, CloudProvider.AWS, "us-east-1", options);

        // Assert
        Assert.DoesNotContain(CostCategory.License, result.Breakdown.Keys);
    }

    [Fact]
    public async Task EstimateK8sCostAsync_IncludesSupportCosts_WhenEnabled()
    {
        // Arrange
        var sizing = CreateK8sSizingResult();
        var pricing = DefaultPricingData.GetAWSPricing();
        _mockPricingService.GetPricingAsync(CloudProvider.AWS, "us-east-1", PricingType.OnDemand)
            .Returns(Task.FromResult(pricing));

        var options = new CostEstimationOptions
        {
            IncludeSupport = true,
            SupportTier = SupportTier.Business
        };

        // Act
        var result = await _service.EstimateK8sCostAsync(sizing, CloudProvider.AWS, "us-east-1", options);

        // Assert
        Assert.Contains(CostCategory.Support, result.Breakdown.Keys);
    }

    [Fact]
    public async Task EstimateK8sCostAsync_AppliesHeadroom()
    {
        // Arrange
        var sizing = CreateK8sSizingResult();
        var pricing = DefaultPricingData.GetAWSPricing();
        _mockPricingService.GetPricingAsync(CloudProvider.AWS, "us-east-1", PricingType.OnDemand)
            .Returns(Task.FromResult(pricing));

        var optionsNoHeadroom = new CostEstimationOptions { HeadroomPercent = 0 };
        var optionsWithHeadroom = new CostEstimationOptions { HeadroomPercent = 20 };

        // Act
        var resultNoHeadroom = await _service.EstimateK8sCostAsync(sizing, CloudProvider.AWS, "us-east-1", optionsNoHeadroom);
        var resultWithHeadroom = await _service.EstimateK8sCostAsync(sizing, CloudProvider.AWS, "us-east-1", optionsWithHeadroom);

        // Assert - With headroom should be more expensive
        Assert.True(resultWithHeadroom.MonthlyTotal >= resultNoHeadroom.MonthlyTotal);
    }

    [Fact]
    public async Task EstimateK8sCostAsync_CalculatesEnvironmentCosts()
    {
        // Arrange
        var sizing = CreateK8sSizingResult();
        var pricing = DefaultPricingData.GetAWSPricing();
        _mockPricingService.GetPricingAsync(CloudProvider.AWS, "us-east-1", PricingType.OnDemand)
            .Returns(Task.FromResult(pricing));

        // Act
        var result = await _service.EstimateK8sCostAsync(sizing, CloudProvider.AWS, "us-east-1");

        // Assert
        Assert.NotEmpty(result.EnvironmentCosts);
        foreach (var env in sizing.Environments)
        {
            Assert.Contains(env.Environment, result.EnvironmentCosts.Keys);
        }
    }

    [Fact]
    public async Task EstimateK8sCostAsync_CalculatesYearlyTotal()
    {
        // Arrange
        var sizing = CreateK8sSizingResult();
        var pricing = DefaultPricingData.GetAWSPricing();
        _mockPricingService.GetPricingAsync(CloudProvider.AWS, "us-east-1", PricingType.OnDemand)
            .Returns(Task.FromResult(pricing));

        // Act
        var result = await _service.EstimateK8sCostAsync(sizing, CloudProvider.AWS, "us-east-1");

        // Assert
        Assert.Equal(result.MonthlyTotal * 12, result.YearlyTotal);
    }

    [Fact]
    public async Task EstimateK8sCostAsync_CalculatesThreeYearTCO()
    {
        // Arrange
        var sizing = CreateK8sSizingResult();
        var pricing = DefaultPricingData.GetAWSPricing();
        _mockPricingService.GetPricingAsync(CloudProvider.AWS, "us-east-1", PricingType.OnDemand)
            .Returns(Task.FromResult(pricing));

        // Act
        var result = await _service.EstimateK8sCostAsync(sizing, CloudProvider.AWS, "us-east-1");

        // Assert
        Assert.Equal(result.YearlyTotal * 3, result.ThreeYearTCO);
    }

    [Fact]
    public async Task EstimateK8sCostAsync_IncludesManagedControlPlane_ForEKS()
    {
        // Arrange
        var sizing = CreateK8sSizingResult();
        var pricing = DefaultPricingData.GetAWSPricing();
        _mockPricingService.GetPricingAsync(CloudProvider.AWS, "us-east-1", PricingType.OnDemand)
            .Returns(Task.FromResult(pricing));

        var options = new CostEstimationOptions { IncludeManagedControlPlane = true };

        // Act
        var result = await _service.EstimateK8sCostAsync(sizing, CloudProvider.AWS, "us-east-1", options);

        // Assert
        var computeBreakdown = result.Breakdown[CostCategory.Compute];
        Assert.Contains(computeBreakdown.LineItems, item => item.Description.Contains("Control Plane"));
    }

    [Theory]
    [InlineData(CloudProvider.AWS)]
    [InlineData(CloudProvider.Azure)]
    [InlineData(CloudProvider.GCP)]
    [InlineData(CloudProvider.OCI)]
    public async Task EstimateK8sCostAsync_WorksWithDifferentProviders(CloudProvider provider)
    {
        // Arrange
        var sizing = CreateK8sSizingResult();
        var pricing = DefaultPricingData.GetDefaultPricing(provider);
        _mockPricingService.GetPricingAsync(provider, Arg.Any<string>(), PricingType.OnDemand)
            .Returns(Task.FromResult(pricing));

        // Act
        var result = await _service.EstimateK8sCostAsync(sizing, provider, "region");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(provider, result.Provider);
        Assert.True(result.MonthlyTotal > 0);
    }

    #endregion

    #region VM Cost Estimation Tests

    [Fact]
    public async Task EstimateVMCostAsync_ValidInput_ReturnsEstimate()
    {
        // Arrange
        var sizing = CreateVMSizingResult();
        var pricing = DefaultPricingData.GetAWSPricing();
        _mockPricingService.GetPricingAsync(CloudProvider.AWS, "us-east-1", PricingType.OnDemand)
            .Returns(Task.FromResult(pricing));

        // Act
        var result = await _service.EstimateVMCostAsync(sizing, CloudProvider.AWS, "us-east-1");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.MonthlyTotal > 0);
    }

    [Fact]
    public async Task EstimateVMCostAsync_IncludesComputeCosts()
    {
        // Arrange
        var sizing = CreateVMSizingResult();
        var pricing = DefaultPricingData.GetAWSPricing();
        _mockPricingService.GetPricingAsync(CloudProvider.AWS, "us-east-1", PricingType.OnDemand)
            .Returns(Task.FromResult(pricing));

        // Act
        var result = await _service.EstimateVMCostAsync(sizing, CloudProvider.AWS, "us-east-1");

        // Assert
        Assert.Contains(CostCategory.Compute, result.Breakdown.Keys);
        Assert.True(result.Breakdown[CostCategory.Compute].Monthly > 0);
    }

    [Fact]
    public async Task EstimateVMCostAsync_CalculatesEnvironmentCosts()
    {
        // Arrange
        var sizing = CreateVMSizingResult();
        var pricing = DefaultPricingData.GetAWSPricing();
        _mockPricingService.GetPricingAsync(CloudProvider.AWS, "us-east-1", PricingType.OnDemand)
            .Returns(Task.FromResult(pricing));

        // Act
        var result = await _service.EstimateVMCostAsync(sizing, CloudProvider.AWS, "us-east-1");

        // Assert
        Assert.NotEmpty(result.EnvironmentCosts);
    }

    #endregion

    #region On-Prem Cost Estimation Tests

    [Fact]
    public void EstimateOnPremCost_ValidInput_ReturnsEstimate()
    {
        // Arrange
        var sizing = CreateK8sSizingResult();
        var pricing = new OnPremPricing();

        // Act
        var result = _service.EstimateOnPremCost(sizing, pricing);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(CloudProvider.OnPrem, result.Provider);
        Assert.True(result.MonthlyTotal > 0);
    }

    [Fact]
    public void EstimateOnPremCost_IncludesComputeCosts()
    {
        // Arrange
        var sizing = CreateK8sSizingResult();
        var pricing = new OnPremPricing();

        // Act
        var result = _service.EstimateOnPremCost(sizing, pricing);

        // Assert
        Assert.Contains(CostCategory.Compute, result.Breakdown.Keys);
    }

    [Fact]
    public void EstimateOnPremCost_IncludesDataCenterCosts()
    {
        // Arrange
        var sizing = CreateK8sSizingResult();
        var pricing = new OnPremPricing();

        // Act
        var result = _service.EstimateOnPremCost(sizing, pricing);

        // Assert
        Assert.Contains(CostCategory.DataCenter, result.Breakdown.Keys);
    }

    [Fact]
    public void EstimateOnPremCost_IncludesLaborCosts()
    {
        // Arrange
        var sizing = CreateK8sSizingResult();
        var pricing = new OnPremPricing();

        // Act
        var result = _service.EstimateOnPremCost(sizing, pricing);

        // Assert
        Assert.Contains(CostCategory.Labor, result.Breakdown.Keys);
    }

    [Fact]
    public void EstimateOnPremCost_IncludesLicenseCosts_ForOpenShift()
    {
        // Arrange
        var sizing = CreateK8sSizingResult();
        var pricing = new OnPremPricing();
        var options = new CostEstimationOptions
        {
            IncludeLicenses = true,
            Distribution = "OpenShift"
        };

        // Act
        var result = _service.EstimateOnPremCost(sizing, pricing, options);

        // Assert
        Assert.Contains(CostCategory.License, result.Breakdown.Keys);
    }

    [Fact]
    public void EstimateOnPremVMCost_ValidInput_ReturnsEstimate()
    {
        // Arrange
        var sizing = CreateVMSizingResult();
        var pricing = new OnPremPricing();

        // Act
        var result = _service.EstimateOnPremVMCost(sizing, pricing);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(CloudProvider.OnPrem, result.Provider);
        Assert.True(result.MonthlyTotal > 0);
    }

    #endregion

    #region Cost Comparison Tests

    [Fact]
    public void Compare_TwoEstimates_IdentifiesCheapest()
    {
        // Arrange
        var estimate1 = new CostEstimate
        {
            Provider = CloudProvider.AWS,
            Region = "us-east-1",
            MonthlyTotal = 10000m
        };
        estimate1.Breakdown[CostCategory.Compute] = new CostBreakdown { Monthly = 10000m };

        var estimate2 = new CostEstimate
        {
            Provider = CloudProvider.OCI,
            Region = "us-ashburn-1",
            MonthlyTotal = 5000m
        };
        estimate2.Breakdown[CostCategory.Compute] = new CostBreakdown { Monthly = 5000m };

        // Act
        var comparison = _service.Compare(estimate1, estimate2);

        // Assert
        Assert.Equal(estimate2, comparison.CheapestOption);
        Assert.Equal(estimate1, comparison.MostExpensiveOption);
    }

    [Fact]
    public void Compare_CalculatesPotentialSavings()
    {
        // Arrange
        var estimate1 = new CostEstimate
        {
            Provider = CloudProvider.AWS,
            Region = "us-east-1",
            MonthlyTotal = 10000m
        };
        estimate1.Breakdown[CostCategory.Compute] = new CostBreakdown { Monthly = 10000m };

        var estimate2 = new CostEstimate
        {
            Provider = CloudProvider.OCI,
            Region = "us-ashburn-1",
            MonthlyTotal = 5000m
        };
        estimate2.Breakdown[CostCategory.Compute] = new CostBreakdown { Monthly = 5000m };

        // Act
        var comparison = _service.Compare(estimate1, estimate2);

        // Assert
        Assert.Contains("AWS-us-east-1", comparison.PotentialSavings.Keys);
        Assert.Equal(5000m, comparison.PotentialSavings["AWS-us-east-1"]);
    }

    [Fact]
    public void Compare_GeneratesInsights()
    {
        // Arrange
        var estimate1 = new CostEstimate
        {
            Provider = CloudProvider.AWS,
            Region = "us-east-1",
            MonthlyTotal = 10000m
        };
        estimate1.Breakdown[CostCategory.Compute] = new CostBreakdown { Monthly = 10000m };

        var estimate2 = new CostEstimate
        {
            Provider = CloudProvider.OCI,
            Region = "us-ashburn-1",
            MonthlyTotal = 5000m
        };
        estimate2.Breakdown[CostCategory.Compute] = new CostBreakdown { Monthly = 5000m };

        // Act
        var comparison = _service.Compare(estimate1, estimate2);

        // Assert
        Assert.NotEmpty(comparison.Insights);
    }

    [Fact]
    public void Compare_SingleEstimate_ReturnsComparison()
    {
        // Arrange
        var estimate = new CostEstimate
        {
            Provider = CloudProvider.AWS,
            Region = "us-east-1",
            MonthlyTotal = 10000m
        };
        estimate.Breakdown[CostCategory.Compute] = new CostBreakdown { Monthly = 10000m };

        // Act
        var comparison = _service.Compare(estimate);

        // Assert
        Assert.NotNull(comparison);
        Assert.Single(comparison.Estimates);
    }

    [Fact]
    public void Compare_EmptyEstimates_ReturnsEmptyComparison()
    {
        // Act
        var comparison = _service.Compare();

        // Assert
        Assert.NotNull(comparison);
        Assert.Empty(comparison.Estimates);
    }

    [Fact]
    public void Compare_ThreeEstimates_FindsCheapest()
    {
        // Arrange
        var estimates = new[]
        {
            new CostEstimate { Provider = CloudProvider.AWS, MonthlyTotal = 10000m },
            new CostEstimate { Provider = CloudProvider.Azure, MonthlyTotal = 8000m },
            new CostEstimate { Provider = CloudProvider.OCI, MonthlyTotal = 5000m }
        };
        foreach (var e in estimates)
            e.Breakdown[CostCategory.Compute] = new CostBreakdown { Monthly = e.MonthlyTotal };

        // Act
        var comparison = _service.Compare(estimates);

        // Assert
        Assert.Equal(CloudProvider.OCI, comparison.CheapestOption?.Provider);
        Assert.Equal(CloudProvider.AWS, comparison.MostExpensiveOption?.Provider);
    }

    #endregion

    #region TCO Calculation Tests

    [Fact]
    public void CalculateTCO_OneYear_ReturnsYearlyTotal()
    {
        // Arrange
        var estimate = new CostEstimate { MonthlyTotal = 1000m };
        estimate.Breakdown[CostCategory.Compute] = new CostBreakdown { Monthly = 1000m };

        // Act
        var tco = _service.CalculateTCO(estimate, 1);

        // Assert
        Assert.Equal(12000m, tco);
    }

    [Fact]
    public void CalculateTCO_ThreeYears_ReturnsThreeYearTotal()
    {
        // Arrange
        var estimate = new CostEstimate { MonthlyTotal = 1000m };
        estimate.Breakdown[CostCategory.Compute] = new CostBreakdown { Monthly = 1000m };

        // Act
        var tco = _service.CalculateTCO(estimate, 3);

        // Assert
        Assert.Equal(36000m, tco);
    }

    [Fact]
    public void CalculateTCO_FiveYears_ReturnsFiveYearTotal()
    {
        // Arrange
        var estimate = new CostEstimate { MonthlyTotal = 1000m };
        estimate.Breakdown[CostCategory.Compute] = new CostBreakdown { Monthly = 1000m };

        // Act
        var tco = _service.CalculateTCO(estimate, 5);

        // Assert
        Assert.Equal(60000m, tco);
    }

    #endregion

    #region License Cost Calculation Tests

    [Theory]
    [InlineData("OpenShift", 2500)]
    [InlineData("Red Hat OpenShift", 2500)]
    [InlineData("Tanzu", 1500)]
    [InlineData("VMware Tanzu", 1500)]
    [InlineData("Rancher", 1000)]
    [InlineData("Rancher Enterprise", 1000)]
    [InlineData("Charmed", 500)]
    [InlineData("Charmed Kubernetes", 500)]
    public async Task EstimateK8sCostAsync_CalculatesCorrectLicenseCost(string distribution, decimal expectedPerNode)
    {
        // Arrange
        var sizing = CreateK8sSizingResult();
        var pricing = DefaultPricingData.GetAWSPricing();
        _mockPricingService.GetPricingAsync(CloudProvider.AWS, "us-east-1", PricingType.OnDemand)
            .Returns(Task.FromResult(pricing));

        var options = new CostEstimationOptions
        {
            IncludeLicenses = true,
            Distribution = distribution
        };

        // Act
        var result = await _service.EstimateK8sCostAsync(sizing, CloudProvider.AWS, "us-east-1", options);

        // Assert
        Assert.Contains(CostCategory.License, result.Breakdown.Keys);
        // License cost should be proportional to nodes
        var expectedMonthly = (expectedPerNode * sizing.GrandTotal.TotalNodes) / 12;
        Assert.Equal(expectedMonthly, result.Breakdown[CostCategory.License].Monthly, precision: 2);
    }

    #endregion

    #region Support Tier Tests

    [Theory]
    [InlineData(SupportTier.None, 0)]
    [InlineData(SupportTier.Basic, 0)]
    [InlineData(SupportTier.Developer, 3)]
    [InlineData(SupportTier.Business, 10)]
    [InlineData(SupportTier.Enterprise, 15)]
    public async Task EstimateK8sCostAsync_AppliesCorrectSupportPercentage(SupportTier tier, decimal expectedPercent)
    {
        // Arrange
        var sizing = CreateK8sSizingResult();
        var pricing = DefaultPricingData.GetAWSPricing();
        _mockPricingService.GetPricingAsync(CloudProvider.AWS, "us-east-1", PricingType.OnDemand)
            .Returns(Task.FromResult(pricing));

        var options = new CostEstimationOptions
        {
            IncludeSupport = true,
            SupportTier = tier,
            IncludeStorage = false,
            IncludeNetwork = false,
            IncludeLicenses = false
        };

        // Act
        var result = await _service.EstimateK8sCostAsync(sizing, CloudProvider.AWS, "us-east-1", options);

        // Assert
        if (expectedPercent > 0)
        {
            Assert.Contains(CostCategory.Support, result.Breakdown.Keys);
        }
    }

    #endregion

    #region Helper Methods

    private static K8sSizingInput CreateK8sSizingInput()
    {
        return new K8sSizingInput
        {
            Distribution = Distribution.OpenShift,
            Technology = Technology.DotNet,
            ClusterMode = ClusterMode.MultiCluster,
            ProdApps = new AppConfig
            {
                Small = 2,
                Medium = 4,
                Large = 3,
                XLarge = 1
            },
            NonProdApps = new AppConfig
            {
                Small = 2,
                Medium = 4,
                Large = 3,
                XLarge = 1
            },
            EnabledEnvironments = new HashSet<EnvironmentType>
            {
                EnvironmentType.Dev,
                EnvironmentType.Test,
                EnvironmentType.Stage,
                EnvironmentType.Prod,
                EnvironmentType.DR
            },
            Replicas = new ReplicaSettings(),
            Headroom = new HeadroomSettings(),
            ProdOvercommit = new OvercommitSettings(),
            NonProdOvercommit = new OvercommitSettings()
        };
    }

    private static K8sSizingResult CreateK8sSizingResult()
    {
        return new K8sSizingResult
        {
            Configuration = CreateK8sSizingInput(),
            DistributionName = "OpenShift",
            TechnologyName = ".NET",
            Environments = new List<EnvironmentResult>
            {
                new()
                {
                    Environment = EnvironmentType.Dev,
                    EnvironmentName = "Development",
                    IsProd = false,
                    Apps = 10,
                    Replicas = 1,
                    Pods = 10,
                    Masters = 3,
                    Infra = 3,
                    Workers = 4,
                    TotalNodes = 10,
                    TotalCpu = 120,
                    TotalRam = 480,
                    TotalDisk = 1000
                },
                new()
                {
                    Environment = EnvironmentType.Prod,
                    EnvironmentName = "Production",
                    IsProd = true,
                    Apps = 10,
                    Replicas = 3,
                    Pods = 30,
                    Masters = 3,
                    Infra = 5,
                    Workers = 11,
                    TotalNodes = 19,
                    TotalCpu = 240,
                    TotalRam = 960,
                    TotalDisk = 2000
                }
            },
            GrandTotal = new GrandTotal
            {
                TotalNodes = 29,
                TotalMasters = 6,
                TotalInfra = 8,
                TotalWorkers = 15,
                TotalCpu = 360,
                TotalRam = 1440,
                TotalDisk = 3000
            }
        };
    }

    private static VMSizingResult CreateVMSizingResult()
    {
        return new VMSizingResult
        {
            TechnologyName = ".NET",
            Environments = new List<VMEnvironmentResult>
            {
                new()
                {
                    Environment = EnvironmentType.Prod,
                    EnvironmentName = "Production",
                    IsProd = true,
                    TotalVMs = 10,
                    TotalCpu = 80,
                    TotalRam = 320,
                    TotalDisk = 1000,
                    Roles = new List<VMRoleResult>()
                }
            },
            GrandTotal = new VMGrandTotal
            {
                TotalVMs = 10,
                TotalCpu = 80,
                TotalRam = 320,
                TotalDisk = 1000
            }
        };
    }

    #endregion
}
