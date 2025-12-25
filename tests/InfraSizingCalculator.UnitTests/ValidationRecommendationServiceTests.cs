using FluentAssertions;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Services;
using Xunit;

namespace InfraSizingCalculator.UnitTests;

public class ValidationRecommendationServiceTests
{
    private readonly ValidationRecommendationService _service;

    public ValidationRecommendationServiceTests()
    {
        _service = new ValidationRecommendationService();
    }

    #region K8s Analyze Tests

    [Fact]
    public void Analyze_K8s_ReturnsWarningsList()
    {
        // Arrange
        var result = CreateK8sSizingResult();
        var input = CreateK8sSizingInput();

        // Act
        var warnings = _service.Analyze(result, input);

        // Assert
        warnings.Should().NotBeNull();
    }

    [Fact]
    public void Analyze_K8s_WarningsAreSortedBySeverity()
    {
        // Arrange
        var result = CreateK8sSizingResult();
        var input = CreateK8sSizingInput();

        // Act
        var warnings = _service.Analyze(result, input);

        // Assert
        if (warnings.Count > 1)
        {
            for (int i = 1; i < warnings.Count; i++)
            {
                ((int)warnings[i].Severity).Should().BeLessThanOrEqualTo((int)warnings[i - 1].Severity);
            }
        }
    }

    [Fact]
    public void Analyze_K8s_GoodConfiguration_ReturnsSuccessWarning()
    {
        // Arrange
        var result = CreateGoodK8sSizingResult();
        var input = CreateGoodK8sSizingInput();

        // Act
        var warnings = _service.Analyze(result, input);

        // Assert
        warnings.Should().Contain(w =>
            w.Severity == WarningSeverity.Success &&
            w.Id == "config-good");
    }

    #endregion

    #region Environment Balance Tests

    [Fact]
    public void Analyze_K8s_OversizedDevEnvironment_GeneratesWarning()
    {
        // Arrange
        var input = new K8sSizingInput { Distribution = Distribution.Kubernetes, ProdApps = new AppConfig(), NonProdApps = new AppConfig() };
        var result = new K8sSizingResult
        {
            Environments = new List<EnvironmentResult>
            {
                new EnvironmentResult
                {
                    Environment = EnvironmentType.Prod,
                    IsProd = true,
                    TotalNodes = 10,
                    Masters = 3,
                    Workers = 5,
                    Infra = 2
                },
                new EnvironmentResult
                {
                    Environment = EnvironmentType.Dev,
                    IsProd = false,
                    TotalNodes = 8, // 80% of prod - oversized
                    Masters = 3,
                    Workers = 3,
                    Infra = 2
                }
            },
            GrandTotal = new GrandTotal { TotalNodes = 18 },
            Configuration = input
        };

        // Act
        var warnings = _service.Analyze(result, input);

        // Assert
        warnings.Should().Contain(w =>
            w.Id == "env-dev-oversized" &&
            w.Category == WarningCategory.Cost);
    }

    [Fact]
    public void Analyze_K8s_OversizedTestEnvironment_GeneratesWarning()
    {
        // Arrange
        var input = new K8sSizingInput { Distribution = Distribution.Kubernetes, ProdApps = new AppConfig(), NonProdApps = new AppConfig() };
        var result = new K8sSizingResult
        {
            Environments = new List<EnvironmentResult>
            {
                new EnvironmentResult
                {
                    Environment = EnvironmentType.Prod,
                    IsProd = true,
                    TotalNodes = 10,
                    Masters = 3,
                    Workers = 5,
                    Infra = 2
                },
                new EnvironmentResult
                {
                    Environment = EnvironmentType.Test,
                    IsProd = false,
                    TotalNodes = 8, // 80% of prod - oversized
                    Masters = 3,
                    Workers = 3,
                    Infra = 2
                }
            },
            GrandTotal = new GrandTotal { TotalNodes = 18 },
            Configuration = input
        };

        // Act
        var warnings = _service.Analyze(result, input);

        // Assert
        warnings.Should().Contain(w =>
            w.Id == "env-test-oversized" &&
            w.Category == WarningCategory.Cost);
    }

    #endregion

    #region Replica Analysis Tests

    [Fact]
    public void Analyze_K8s_HighReplicasInNonProd_GeneratesInfo()
    {
        // Arrange
        var input = new K8sSizingInput { Distribution = Distribution.Kubernetes, ProdApps = new AppConfig(), NonProdApps = new AppConfig() };
        var result = new K8sSizingResult
        {
            Environments = new List<EnvironmentResult>
            {
                new EnvironmentResult
                {
                    Environment = EnvironmentType.Dev,
                    IsProd = false,
                    Replicas = 5 // High for non-prod
                }
            },
            GrandTotal = new GrandTotal { TotalNodes = 10 },
            Configuration = input
        };

        // Act
        var warnings = _service.Analyze(result, input);

        // Assert
        warnings.Should().Contain(w =>
            w.Id.StartsWith("replicas-high-") &&
            w.Category == WarningCategory.Cost);
    }

    [Fact]
    public void Analyze_K8s_LowReplicasInProd_GeneratesWarning()
    {
        // Arrange
        var input = new K8sSizingInput { Distribution = Distribution.Kubernetes, ProdApps = new AppConfig(), NonProdApps = new AppConfig() };
        var result = new K8sSizingResult
        {
            Environments = new List<EnvironmentResult>
            {
                new EnvironmentResult
                {
                    Environment = EnvironmentType.Prod,
                    IsProd = true,
                    Replicas = 1 // Too low for prod
                }
            },
            GrandTotal = new GrandTotal { TotalNodes = 10 },
            Configuration = input
        };

        // Act
        var warnings = _service.Analyze(result, input);

        // Assert
        warnings.Should().Contain(w =>
            w.Id == "replicas-low-prod" &&
            w.Category == WarningCategory.HighAvailability);
    }

    #endregion

    #region Distribution Choice Tests

    [Fact]
    public void Analyze_K8s_OpenShiftForSmallDeployment_GeneratesInfo()
    {
        // Arrange
        var input = new K8sSizingInput { Distribution = Distribution.OpenShift, ProdApps = new AppConfig(), NonProdApps = new AppConfig() };
        var result = new K8sSizingResult
        {
            Environments = new List<EnvironmentResult>(),
            GrandTotal = new GrandTotal { TotalNodes = 10 }, // Small deployment
            Configuration = input
        };

        // Act
        var warnings = _service.Analyze(result, input);

        // Assert
        warnings.Should().Contain(w =>
            w.Id == "dist-openshift-small" &&
            w.Category == WarningCategory.Distribution);
    }

    [Fact]
    public void Analyze_K8s_VanillaK8sForLargeDeployment_SuggestionManagedService()
    {
        // Arrange
        var input = new K8sSizingInput { Distribution = Distribution.Kubernetes, ProdApps = new AppConfig(), NonProdApps = new AppConfig() };
        var result = new K8sSizingResult
        {
            Environments = new List<EnvironmentResult>(),
            GrandTotal = new GrandTotal { TotalNodes = 60 }, // Large deployment
            Configuration = input
        };

        // Act
        var warnings = _service.Analyze(result, input);

        // Assert
        warnings.Should().Contain(w =>
            w.Id == "dist-managed-suggestion" &&
            w.Category == WarningCategory.Distribution);
    }

    [Fact]
    public void Analyze_K8s_LargeDeploymentWithoutInfraNodes_SuggestsInfraNodes()
    {
        // Arrange
        var input = new K8sSizingInput { Distribution = Distribution.Kubernetes, ProdApps = new AppConfig(), NonProdApps = new AppConfig() };
        var result = new K8sSizingResult
        {
            Environments = new List<EnvironmentResult>(),
            GrandTotal = new GrandTotal
            {
                TotalNodes = 40,
                TotalInfra = 0 // No infra nodes
            },
            Configuration = input
        };

        // Act
        var warnings = _service.Analyze(result, input);

        // Assert
        warnings.Should().Contain(w =>
            w.Id == "dist-infra-suggestion" &&
            w.Category == WarningCategory.BestPractice);
    }

    #endregion

    #region Headroom Analysis Tests

    [Fact]
    public void Analyze_K8s_HeadroomDisabled_GeneratesWarning()
    {
        // Arrange
        var result = CreateK8sSizingResult();
        var input = new K8sSizingInput
        {
            Distribution = Distribution.Kubernetes,
            EnableHeadroom = false
        };

        // Act
        var warnings = _service.Analyze(result, input);

        // Assert
        warnings.Should().Contain(w =>
            w.Id == "headroom-disabled" &&
            w.Category == WarningCategory.Sizing);
    }

    [Fact]
    public void Analyze_K8s_HighHeadroomInNonProd_GeneratesInfo()
    {
        // Arrange
        var result = CreateK8sSizingResult();
        var input = new K8sSizingInput
        {
            Distribution = Distribution.Kubernetes,
            EnableHeadroom = true,
            Headroom = new HeadroomSettings
            {
                Dev = 60, // Very high
                Test = 60
            }
        };

        // Act
        var warnings = _service.Analyze(result, input);

        // Assert
        warnings.Should().Contain(w =>
            w.Id == "headroom-high-nonprod" &&
            w.Category == WarningCategory.Cost);
    }

    #endregion

    #region Overcommit Analysis Tests

    [Fact]
    public void Analyze_K8s_NoOvercommitInProd_GeneratesInfo()
    {
        // Arrange
        var result = CreateK8sSizingResult();
        var input = new K8sSizingInput
        {
            Distribution = Distribution.Kubernetes,
            ProdOvercommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 }
        };

        // Act
        var warnings = _service.Analyze(result, input);

        // Assert
        warnings.Should().Contain(w =>
            w.Id == "overcommit-none-prod" &&
            w.Category == WarningCategory.Cost);
    }

    [Fact]
    public void Analyze_K8s_HighCpuOvercommit_GeneratesWarning()
    {
        // Arrange
        var result = CreateK8sSizingResult();
        var input = new K8sSizingInput
        {
            Distribution = Distribution.Kubernetes,
            ProdOvercommit = new OvercommitSettings { Cpu = 5.0, Memory = 1.0 } // Too high
        };

        // Act
        var warnings = _service.Analyze(result, input);

        // Assert
        warnings.Should().Contain(w =>
            w.Id == "overcommit-high-cpu" &&
            w.Category == WarningCategory.Sizing);
    }

    [Fact]
    public void Analyze_K8s_HighMemoryOvercommit_GeneratesWarning()
    {
        // Arrange
        var result = CreateK8sSizingResult();
        var input = new K8sSizingInput
        {
            Distribution = Distribution.Kubernetes,
            ProdOvercommit = new OvercommitSettings { Cpu = 1.0, Memory = 2.5 } // Too high
        };

        // Act
        var warnings = _service.Analyze(result, input);

        // Assert
        warnings.Should().Contain(w =>
            w.Id == "overcommit-high-memory" &&
            w.Category == WarningCategory.Sizing);
    }

    #endregion

    #region Cluster Size Analysis Tests

    [Fact]
    public void Analyze_K8s_VeryLargeCluster_GeneratesWarning()
    {
        // Arrange
        var input = new K8sSizingInput { Distribution = Distribution.Kubernetes, ProdApps = new AppConfig(), NonProdApps = new AppConfig() };
        var result = new K8sSizingResult
        {
            Environments = new List<EnvironmentResult>
            {
                new EnvironmentResult
                {
                    Environment = EnvironmentType.Prod,
                    IsProd = true,
                    Workers = 250 // Very large
                }
            },
            GrandTotal = new GrandTotal { TotalNodes = 260 },
            Configuration = input
        };

        // Act
        var warnings = _service.Analyze(result, input);

        // Assert
        warnings.Should().Contain(w =>
            w.Id.StartsWith("cluster-large-") &&
            w.Category == WarningCategory.BestPractice);
    }

    [Fact]
    public void Analyze_K8s_SmallProdCluster_GeneratesWarning()
    {
        // Arrange
        var input = new K8sSizingInput { Distribution = Distribution.Kubernetes, ProdApps = new AppConfig(), NonProdApps = new AppConfig() };
        var result = new K8sSizingResult
        {
            Environments = new List<EnvironmentResult>
            {
                new EnvironmentResult
                {
                    Environment = EnvironmentType.Prod,
                    IsProd = true,
                    Workers = 2 // Too small for HA
                }
            },
            GrandTotal = new GrandTotal { TotalNodes = 5 },
            Configuration = input
        };

        // Act
        var warnings = _service.Analyze(result, input);

        // Assert
        warnings.Should().Contain(w =>
            w.Id.StartsWith("cluster-small-") &&
            w.Category == WarningCategory.HighAvailability);
    }

    #endregion

    #region Positive Feedback Tests

    [Fact]
    public void Analyze_K8s_WithDREnvironment_GeneratesSuccessMessage()
    {
        // Arrange
        var input = CreateGoodK8sSizingInput();
        var result = new K8sSizingResult
        {
            Environments = new List<EnvironmentResult>
            {
                new EnvironmentResult
                {
                    Environment = EnvironmentType.Prod,
                    IsProd = true,
                    Workers = 5,
                    Replicas = 3
                },
                new EnvironmentResult
                {
                    Environment = EnvironmentType.DR,
                    IsProd = true
                }
            },
            GrandTotal = new GrandTotal { TotalNodes = 15 },
            Configuration = input
        };

        // Act
        var warnings = _service.Analyze(result, input);

        // Assert
        warnings.Should().Contain(w =>
            w.Id == "dr-configured" &&
            w.Category == WarningCategory.HighAvailability &&
            w.Severity == WarningSeverity.Success);
    }

    #endregion

    #region VM Analyze Tests

    [Fact]
    public void Analyze_VM_ReturnsWarningsList()
    {
        // Arrange
        var result = CreateVMSizingResult();
        var input = CreateVMSizingInput();

        // Act
        var warnings = _service.Analyze(result, input);

        // Assert
        warnings.Should().NotBeNull();
    }

    [Fact]
    public void Analyze_VM_NoHAInProd_GeneratesWarning()
    {
        // Arrange
        var result = new VMSizingResult
        {
            Environments = new List<VMEnvironmentResult>
            {
                new VMEnvironmentResult
                {
                    Environment = EnvironmentType.Prod,
                    EnvironmentName = "Production",
                    IsProd = true,
                    HAPattern = HAPattern.None
                }
            },
            GrandTotal = new VMGrandTotal()
        };
        var input = CreateVMSizingInput();

        // Act
        var warnings = _service.Analyze(result, input);

        // Assert
        warnings.Should().Contain(w =>
            w.Id.StartsWith("ha-none-") &&
            w.Category == WarningCategory.HighAvailability);
    }

    [Fact]
    public void Analyze_VM_NoLoadBalancerInProd_GeneratesInfo()
    {
        // Arrange
        var result = new VMSizingResult
        {
            Environments = new List<VMEnvironmentResult>
            {
                new VMEnvironmentResult
                {
                    Environment = EnvironmentType.Prod,
                    EnvironmentName = "Production",
                    IsProd = true,
                    HAPattern = HAPattern.ActiveActive,
                    LoadBalancer = LoadBalancerOption.None
                }
            },
            GrandTotal = new VMGrandTotal()
        };
        var input = CreateVMSizingInput();

        // Act
        var warnings = _service.Analyze(result, input);

        // Assert
        warnings.Should().Contain(w =>
            w.Id.StartsWith("lb-none-") &&
            w.Category == WarningCategory.HighAvailability);
    }

    [Fact]
    public void Analyze_VM_SingleLoadBalancerInProd_GeneratesWarning()
    {
        // Arrange
        var result = new VMSizingResult
        {
            Environments = new List<VMEnvironmentResult>
            {
                new VMEnvironmentResult
                {
                    Environment = EnvironmentType.Prod,
                    EnvironmentName = "Production",
                    IsProd = true,
                    HAPattern = HAPattern.ActiveActive,
                    LoadBalancer = LoadBalancerOption.Single
                }
            },
            GrandTotal = new VMGrandTotal()
        };
        var input = CreateVMSizingInput();

        // Act
        var warnings = _service.Analyze(result, input);

        // Assert
        warnings.Should().Contain(w =>
            w.Id.StartsWith("lb-single-") &&
            w.Category == WarningCategory.HighAvailability);
    }

    [Fact]
    public void Analyze_VM_SingleInstanceCriticalRole_GeneratesWarning()
    {
        // Arrange
        var result = new VMSizingResult
        {
            Environments = new List<VMEnvironmentResult>
            {
                new VMEnvironmentResult
                {
                    Environment = EnvironmentType.Prod,
                    EnvironmentName = "Production",
                    IsProd = true,
                    HAPattern = HAPattern.ActiveActive,
                    LoadBalancer = LoadBalancerOption.HAPair,
                    Roles = new List<VMRoleResult>
                    {
                        new VMRoleResult
                        {
                            Role = ServerRole.Database,
                            RoleName = "Database",
                            TotalInstances = 1 // Single instance
                        }
                    }
                }
            },
            GrandTotal = new VMGrandTotal()
        };
        var input = CreateVMSizingInput();

        // Act
        var warnings = _service.Analyze(result, input);

        // Assert
        warnings.Should().Contain(w =>
            w.Id.Contains("role-single-") &&
            w.Category == WarningCategory.HighAvailability);
    }

    #endregion

    #region Helper Methods

    private K8sSizingResult CreateK8sSizingResult()
    {
        var input = CreateK8sSizingInput();
        return new K8sSizingResult
        {
            Environments = new List<EnvironmentResult>
            {
                new EnvironmentResult
                {
                    Environment = EnvironmentType.Prod,
                    EnvironmentName = "Production",
                    IsProd = true,
                    Masters = 3,
                    Workers = 5,
                    Infra = 2,
                    TotalNodes = 10
                }
            },
            GrandTotal = new GrandTotal { TotalNodes = 10 },
            Configuration = input
        };
    }

    private K8sSizingResult CreateGoodK8sSizingResult()
    {
        var input = CreateGoodK8sSizingInput();
        return new K8sSizingResult
        {
            Environments = new List<EnvironmentResult>
            {
                new EnvironmentResult
                {
                    Environment = EnvironmentType.Prod,
                    EnvironmentName = "Production",
                    IsProd = true,
                    Masters = 3,
                    Workers = 5,
                    Infra = 3,
                    TotalNodes = 11,
                    Replicas = 3
                },
                new EnvironmentResult
                {
                    Environment = EnvironmentType.Dev,
                    EnvironmentName = "Development",
                    IsProd = false,
                    Masters = 1,
                    Workers = 2,
                    Infra = 0,
                    TotalNodes = 3,
                    Replicas = 1
                }
            },
            GrandTotal = new GrandTotal { TotalNodes = 14, TotalInfra = 3 },
            Configuration = input
        };
    }

    private K8sSizingInput CreateK8sSizingInput()
    {
        return new K8sSizingInput
        {
            Distribution = Distribution.Kubernetes,
            ProdApps = new AppConfig { Small = 5, Medium = 10, Large = 5 },
            NonProdApps = new AppConfig { Small = 5, Medium = 10, Large = 5 },
            EnableHeadroom = true,
            Headroom = new HeadroomSettings
            {
                Prod = 30,
                Dev = 20,
                Test = 20
            },
            ProdOvercommit = new OvercommitSettings { Cpu = 1.5, Memory = 1.0 }
        };
    }

    private K8sSizingInput CreateGoodK8sSizingInput()
    {
        return new K8sSizingInput
        {
            Distribution = Distribution.Kubernetes,
            ProdApps = new AppConfig { Small = 5, Medium = 10, Large = 5 },
            NonProdApps = new AppConfig { Small = 5, Medium = 10, Large = 5 },
            EnableHeadroom = true,
            Headroom = new HeadroomSettings
            {
                Prod = 30,
                Dev = 20,
                Test = 20,
                Stage = 25
            },
            ProdOvercommit = new OvercommitSettings { Cpu = 1.5, Memory = 1.0 }
        };
    }

    private VMSizingResult CreateVMSizingResult()
    {
        return new VMSizingResult
        {
            Environments = new List<VMEnvironmentResult>
            {
                new VMEnvironmentResult
                {
                    Environment = EnvironmentType.Prod,
                    EnvironmentName = "Production",
                    IsProd = true,
                    HAPattern = HAPattern.ActiveActive,
                    LoadBalancer = LoadBalancerOption.HAPair
                }
            },
            GrandTotal = new VMGrandTotal { TotalVMs = 10 }
        };
    }

    private VMSizingInput CreateVMSizingInput()
    {
        return new VMSizingInput
        {
            Technology = Technology.DotNet,
            EnabledEnvironments = new HashSet<EnvironmentType> { EnvironmentType.Prod }
        };
    }

    #endregion
}
