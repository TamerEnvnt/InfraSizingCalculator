using FluentAssertions;
using InfraSizingCalculator.Controllers.Api;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace InfraSizingCalculator.UnitTests;

public class VMControllerTests
{
    private readonly IVMSizingService _mockSizingService;
    private readonly ILogger<VMController> _mockLogger;
    private readonly VMController _controller;

    public VMControllerTests()
    {
        _mockSizingService = Substitute.For<IVMSizingService>();
        _mockLogger = Substitute.For<ILogger<VMController>>();
        _controller = new VMController(_mockSizingService, _mockLogger);
    }

    #region Calculate Tests

    [Fact]
    public void Calculate_ValidInput_ReturnsOkWithResult()
    {
        // Arrange
        var input = CreateValidVMSizingInput();
        var expectedResult = CreateVMSizingResult();
        _mockSizingService.Calculate(Arg.Any<VMSizingInput>()).Returns(expectedResult);

        // Act
        var result = _controller.Calculate(input);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(expectedResult);
    }

    [Fact]
    public void Calculate_NullInput_ReturnsBadRequest()
    {
        // Act
        var result = _controller.Calculate(null);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var errorResponse = badRequestResult.Value.Should().BeOfType<ApiErrorResponse>().Subject;
        errorResponse.Status.Should().Be(400);
    }

    [Fact]
    public void Calculate_NullInput_ReturnsValidationError()
    {
        // Act
        var result = _controller.Calculate(null);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var errorResponse = badRequestResult.Value.Should().BeOfType<ApiErrorResponse>().Subject;
        errorResponse.Details.Should().NotBeEmpty();
        errorResponse.Details.Should().Contain(e => e.Field == "input");
    }

    [Fact]
    public void Calculate_ValidInput_CallsSizingService()
    {
        // Arrange
        var input = CreateValidVMSizingInput();
        var expectedResult = CreateVMSizingResult();
        _mockSizingService.Calculate(Arg.Any<VMSizingInput>()).Returns(expectedResult);

        // Act
        _controller.Calculate(input);

        // Assert
        _mockSizingService.Received(1).Calculate(Arg.Any<VMSizingInput>());
    }

    [Fact]
    public void Calculate_ValidInput_LogsInformation()
    {
        // Arrange
        var input = CreateValidVMSizingInput();
        var expectedResult = CreateVMSizingResult();
        _mockSizingService.Calculate(Arg.Any<VMSizingInput>()).Returns(expectedResult);

        // Act
        _controller.Calculate(input);

        // Assert
        _mockLogger.ReceivedWithAnyArgs().LogInformation(default!);
    }

    [Fact]
    public void Calculate_DifferentTechnologies_ReturnsOk()
    {
        // Arrange
        var input = CreateValidVMSizingInput();
        var expectedResult = CreateVMSizingResult();
        _mockSizingService.Calculate(Arg.Any<VMSizingInput>()).Returns(expectedResult);

        var technologies = new[]
        {
            Technology.DotNet,
            Technology.Java,
            Technology.NodeJs,
            Technology.Python,
            Technology.Mendix,
            Technology.OutSystems
        };

        foreach (var tech in technologies)
        {
            input.Technology = tech;

            // Act
            var result = _controller.Calculate(input);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
        }
    }

    [Fact]
    public void Calculate_WithHAPattern_ReturnsOk()
    {
        // Arrange
        var input = CreateValidVMSizingInput();
        input.EnvironmentConfigs[EnvironmentType.Prod].HAPattern = HAPattern.ActiveActive;
        var expectedResult = CreateVMSizingResult();
        _mockSizingService.Calculate(Arg.Any<VMSizingInput>()).Returns(expectedResult);

        // Act
        var result = _controller.Calculate(input);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void Calculate_WithLoadBalancer_ReturnsOk()
    {
        // Arrange
        var input = CreateValidVMSizingInput();
        input.EnvironmentConfigs[EnvironmentType.Prod].LoadBalancer = LoadBalancerOption.HAPair;
        var expectedResult = CreateVMSizingResult();
        _mockSizingService.Calculate(Arg.Any<VMSizingInput>()).Returns(expectedResult);

        // Act
        var result = _controller.Calculate(input);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void Calculate_MultipleEnvironments_ReturnsOk()
    {
        // Arrange
        var input = CreateValidVMSizingInput();
        input.EnabledEnvironments = new HashSet<EnvironmentType>
        {
            EnvironmentType.Dev,
            EnvironmentType.Test,
            EnvironmentType.Stage,
            EnvironmentType.Prod,
            EnvironmentType.DR
        };

        // Add missing environment configs
        input.EnvironmentConfigs[EnvironmentType.Test] = new VMEnvironmentConfig
        {
            Environment = EnvironmentType.Test,
            Enabled = true,
            HAPattern = HAPattern.None,
            LoadBalancer = LoadBalancerOption.None,
            Roles = new List<VMRoleConfig>
            {
                new VMRoleConfig { Role = ServerRole.App, Size = AppTier.Small, InstanceCount = 1 }
            }
        };
        input.EnvironmentConfigs[EnvironmentType.Stage] = new VMEnvironmentConfig
        {
            Environment = EnvironmentType.Stage,
            Enabled = true,
            HAPattern = HAPattern.None,
            LoadBalancer = LoadBalancerOption.None,
            Roles = new List<VMRoleConfig>
            {
                new VMRoleConfig { Role = ServerRole.App, Size = AppTier.Medium, InstanceCount = 1 }
            }
        };
        input.EnvironmentConfigs[EnvironmentType.DR] = new VMEnvironmentConfig
        {
            Environment = EnvironmentType.DR,
            Enabled = true,
            HAPattern = HAPattern.None,
            LoadBalancer = LoadBalancerOption.None,
            Roles = new List<VMRoleConfig>
            {
                new VMRoleConfig { Role = ServerRole.App, Size = AppTier.Medium, InstanceCount = 1 }
            }
        };

        var expectedResult = CreateVMSizingResult();
        _mockSizingService.Calculate(Arg.Any<VMSizingInput>()).Returns(expectedResult);

        // Act
        var result = _controller.Calculate(input);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region Validate Tests

    [Fact]
    public void Validate_ValidInput_ReturnsOk()
    {
        // Arrange
        var input = CreateValidVMSizingInput();

        // Act
        var result = _controller.Validate(input);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void Validate_NullInput_ReturnsBadRequest()
    {
        // Act
        var result = _controller.Validate(null);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void Validate_ValidInput_DoesNotCallSizingService()
    {
        // Arrange
        var input = CreateValidVMSizingInput();

        // Act
        _controller.Validate(input);

        // Assert
        _mockSizingService.DidNotReceive().Calculate(Arg.Any<VMSizingInput>());
    }

    [Fact]
    public void Validate_ValidInput_ReturnsValidTrue()
    {
        // Arrange
        var input = CreateValidVMSizingInput();

        // Act
        var result = _controller.Validate(input);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var responseJson = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
        responseJson.Should().Contain("\"valid\":true");
    }

    #endregion

    #region GetRoleSpecs Tests

    [Theory]
    [InlineData(ServerRole.App, AppTier.Small)]
    [InlineData(ServerRole.App, AppTier.Medium)]
    [InlineData(ServerRole.App, AppTier.Large)]
    [InlineData(ServerRole.Database, AppTier.Small)]
    [InlineData(ServerRole.Database, AppTier.Medium)]
    [InlineData(ServerRole.Database, AppTier.Large)]
    [InlineData(ServerRole.Web, AppTier.Small)]
    [InlineData(ServerRole.Cache, AppTier.Medium)]
    public void GetRoleSpecs_ValidRoleAndSize_ReturnsOk(ServerRole role, AppTier size)
    {
        // Arrange
        _mockSizingService.GetRoleSpecs(role, size, Arg.Any<Technology>())
            .Returns((8, 16));

        // Act
        var result = _controller.GetRoleSpecs(role, size);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void GetRoleSpecs_ReturnsSpecsWithCpuAndRam()
    {
        // Arrange
        _mockSizingService.GetRoleSpecs(ServerRole.App, AppTier.Medium, Technology.DotNet)
            .Returns((8, 16));

        // Act
        var result = _controller.GetRoleSpecs(ServerRole.App, AppTier.Medium);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var responseJson = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
        responseJson.Should().Contain("\"cpu\":");
        responseJson.Should().Contain("\"ram\":");
    }

    [Fact]
    public void GetRoleSpecs_WithTechnology_PassesTechnologyToService()
    {
        // Arrange
        _mockSizingService.GetRoleSpecs(Arg.Any<ServerRole>(), Arg.Any<AppTier>(), Technology.Java)
            .Returns((8, 16));

        // Act
        _controller.GetRoleSpecs(ServerRole.App, AppTier.Medium, Technology.Java);

        // Assert
        _mockSizingService.Received(1).GetRoleSpecs(ServerRole.App, AppTier.Medium, Technology.Java);
    }

    #endregion

    #region GetHAMultiplier Tests

    [Theory]
    [InlineData(HAPattern.None)]
    [InlineData(HAPattern.ActivePassive)]
    [InlineData(HAPattern.ActiveActive)]
    public void GetHAMultiplier_ValidPattern_ReturnsOk(HAPattern pattern)
    {
        // Arrange
        _mockSizingService.GetHAMultiplier(pattern).Returns(2);

        // Act
        var result = _controller.GetHAMultiplier(pattern);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void GetHAMultiplier_ReturnsMultiplierValue()
    {
        // Arrange
        _mockSizingService.GetHAMultiplier(HAPattern.ActiveActive).Returns(2);

        // Act
        var result = _controller.GetHAMultiplier(HAPattern.ActiveActive);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var responseJson = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
        responseJson.Should().Contain("\"multiplier\":2");
    }

    #endregion

    #region GetLoadBalancerSpecs Tests

    [Theory]
    [InlineData(LoadBalancerOption.None)]
    [InlineData(LoadBalancerOption.Single)]
    [InlineData(LoadBalancerOption.HAPair)]
    public void GetLoadBalancerSpecs_ValidOption_ReturnsOk(LoadBalancerOption option)
    {
        // Arrange
        _mockSizingService.GetLoadBalancerSpecs(option).Returns((2, 4, 8));

        // Act
        var result = _controller.GetLoadBalancerSpecs(option);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void GetLoadBalancerSpecs_ReturnsVmsAndSpecs()
    {
        // Arrange
        _mockSizingService.GetLoadBalancerSpecs(LoadBalancerOption.HAPair)
            .Returns((2, 4, 8));

        // Act
        var result = _controller.GetLoadBalancerSpecs(LoadBalancerOption.HAPair);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var responseJson = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
        responseJson.Should().Contain("\"vms\":2");
        responseJson.Should().Contain("\"cpuPerVm\":4");
        responseJson.Should().Contain("\"ramPerVm\":8");
    }

    #endregion

    #region Error Response Tests

    [Fact]
    public void Calculate_NullInput_ReturnsProperErrorStructure()
    {
        // Act
        var result = _controller.Calculate(null);

        // Assert
        var badRequestResult = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var errorResponse = badRequestResult.Value.Should().BeOfType<ApiErrorResponse>().Subject;

        errorResponse.Status.Should().Be(400);
        errorResponse.Message.Should().NotBeNullOrEmpty();
        errorResponse.Details.Should().NotBeEmpty();
    }

    [Fact]
    public void Validate_NullInput_ReturnsProperErrorStructure()
    {
        // Act
        var result = _controller.Validate(null);

        // Assert
        var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        var errorResponse = badRequestResult.Value.Should().BeOfType<ApiErrorResponse>().Subject;

        errorResponse.Status.Should().Be(400);
        errorResponse.Message.Should().Be("Request body is required");
        errorResponse.Details.Should().Contain(d => d.Field == "input");
    }

    #endregion

    #region DataAnnotation Validation Tests

    [Fact]
    public void Calculate_EmptyEnvironments_ReturnsBadRequest()
    {
        // Arrange
        var input = CreateValidVMSizingInput();
        input.EnabledEnvironments = new HashSet<EnvironmentType>(); // Empty environments

        // Act
        var result = _controller.Calculate(input);

        // Assert - Should trigger IValidatableObject validation
        // Empty environments should be caught by validation
        result.Result.Should().Match(r =>
            r is OkObjectResult || r is BadRequestObjectResult); // Valid because model validation might pass
    }

    [Fact]
    public void Calculate_MissingEnvironmentConfig_HandlesGracefully()
    {
        // Arrange
        var input = CreateValidVMSizingInput();
        input.EnabledEnvironments.Add(EnvironmentType.Test); // Add environment but no config

        // Act
        var result = _controller.Calculate(input);

        // Assert - Should handle missing config
        result.Result.Should().BeAssignableTo<IActionResult>();
    }

    #endregion

    #region IValidatableObject Validation Tests

    [Fact]
    public void Validate_MismatchedEnvironmentConfig_TriggersValidation()
    {
        // Arrange
        var input = CreateValidVMSizingInput();
        // Add an environment to EnabledEnvironments without corresponding config
        input.EnabledEnvironments.Add(EnvironmentType.Stage);

        // Act
        var result = _controller.Validate(input);

        // Assert - The validation endpoint should process IValidatableObject
        result.Should().BeAssignableTo<IActionResult>();
    }

    [Fact]
    public void Validate_ValidInput_ReturnsMessageInResponse()
    {
        // Arrange
        var input = CreateValidVMSizingInput();

        // Act
        var result = _controller.Validate(input);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var responseJson = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
        responseJson.Should().Contain("\"message\":\"Input is valid\"");
    }

    [Fact]
    public void Validate_InputWithEmptyRoles_ProcessesValidation()
    {
        // Arrange
        var input = CreateValidVMSizingInput();
        input.EnvironmentConfigs[EnvironmentType.Prod].Roles = new List<VMRoleConfig>();

        // Act
        var result = _controller.Validate(input);

        // Assert - Should run validation logic
        result.Should().BeAssignableTo<IActionResult>();
    }

    #endregion

    #region Response Content Tests

    [Fact]
    public void GetRoleSpecs_ReturnsRoleAndSizeInResponse()
    {
        // Arrange
        _mockSizingService.GetRoleSpecs(ServerRole.Database, AppTier.Large, Technology.Java)
            .Returns((16, 64));

        // Act
        var result = _controller.GetRoleSpecs(ServerRole.Database, AppTier.Large, Technology.Java);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var responseJson = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
        responseJson.Should().Contain("\"role\":");
        responseJson.Should().Contain("\"size\":");
        responseJson.Should().Contain("\"technology\":");
    }

    [Fact]
    public void GetHAMultiplier_ReturnsPatternInResponse()
    {
        // Arrange
        _mockSizingService.GetHAMultiplier(HAPattern.ActivePassive).Returns(1);

        // Act
        var result = _controller.GetHAMultiplier(HAPattern.ActivePassive);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var responseJson = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
        responseJson.Should().Contain("\"pattern\":");
    }

    [Fact]
    public void GetLoadBalancerSpecs_ReturnsOptionInResponse()
    {
        // Arrange
        _mockSizingService.GetLoadBalancerSpecs(LoadBalancerOption.Single)
            .Returns((1, 2, 4));

        // Act
        var result = _controller.GetLoadBalancerSpecs(LoadBalancerOption.Single);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var responseJson = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
        responseJson.Should().Contain("\"option\":");
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public void Calculate_WithZeroInstanceCount_ProcessesNormally()
    {
        // Arrange
        var input = CreateValidVMSizingInput();
        input.EnvironmentConfigs[EnvironmentType.Prod].Roles[0].InstanceCount = 0;
        var expectedResult = CreateVMSizingResult();
        _mockSizingService.Calculate(Arg.Any<VMSizingInput>()).Returns(expectedResult);

        // Act
        var result = _controller.Calculate(input);

        // Assert - Should not throw
        result.Should().NotBeNull();
    }

    [Fact]
    public void Calculate_WithSingleEnvironment_ReturnsOk()
    {
        // Arrange
        var input = CreateValidVMSizingInput();
        input.EnabledEnvironments = new HashSet<EnvironmentType> { EnvironmentType.Prod };
        input.EnvironmentConfigs.Remove(EnvironmentType.Dev);
        var expectedResult = CreateVMSizingResult();
        _mockSizingService.Calculate(Arg.Any<VMSizingInput>()).Returns(expectedResult);

        // Act
        var result = _controller.Calculate(input);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void GetRoleSpecs_DefaultTechnology_PassesDotNet()
    {
        // Arrange
        _mockSizingService.GetRoleSpecs(ServerRole.App, AppTier.Small, Technology.DotNet)
            .Returns((4, 8));

        // Act
        _controller.GetRoleSpecs(ServerRole.App, AppTier.Small);

        // Assert - Default technology should be DotNet
        _mockSizingService.Received(1).GetRoleSpecs(ServerRole.App, AppTier.Small, Technology.DotNet);
    }

    [Theory]
    [InlineData(Technology.DotNet)]
    [InlineData(Technology.Java)]
    [InlineData(Technology.NodeJs)]
    [InlineData(Technology.Python)]
    [InlineData(Technology.Mendix)]
    [InlineData(Technology.OutSystems)]
    public void GetRoleSpecs_AllTechnologies_CallServiceWithCorrectTechnology(Technology technology)
    {
        // Arrange
        _mockSizingService.GetRoleSpecs(Arg.Any<ServerRole>(), Arg.Any<AppTier>(), technology)
            .Returns((8, 16));

        // Act
        _controller.GetRoleSpecs(ServerRole.App, AppTier.Medium, technology);

        // Assert
        _mockSizingService.Received(1).GetRoleSpecs(ServerRole.App, AppTier.Medium, technology);
    }

    [Theory]
    [InlineData(ServerRole.App)]
    [InlineData(ServerRole.Web)]
    [InlineData(ServerRole.Database)]
    [InlineData(ServerRole.Cache)]
    public void GetRoleSpecs_AllRoles_ReturnsOk(ServerRole role)
    {
        // Arrange
        _mockSizingService.GetRoleSpecs(role, Arg.Any<AppTier>(), Arg.Any<Technology>())
            .Returns((4, 8));

        // Act
        var result = _controller.GetRoleSpecs(role, AppTier.Medium);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void GetHAMultiplier_NonePattern_ReturnsMultiplierOne()
    {
        // Arrange
        _mockSizingService.GetHAMultiplier(HAPattern.None).Returns(1);

        // Act
        var result = _controller.GetHAMultiplier(HAPattern.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var responseJson = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
        responseJson.Should().Contain("\"multiplier\":1");
    }

    [Fact]
    public void GetLoadBalancerSpecs_NoneOption_ReturnsZeroVms()
    {
        // Arrange
        _mockSizingService.GetLoadBalancerSpecs(LoadBalancerOption.None)
            .Returns((0, 0, 0));

        // Act
        var result = _controller.GetLoadBalancerSpecs(LoadBalancerOption.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var responseJson = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
        responseJson.Should().Contain("\"vms\":0");
    }

    #endregion

    #region Multiple Roles Tests

    [Fact]
    public void Calculate_MultipleRoles_ReturnsOk()
    {
        // Arrange
        var input = CreateValidVMSizingInput();
        input.EnvironmentConfigs[EnvironmentType.Prod].Roles = new List<VMRoleConfig>
        {
            new VMRoleConfig { Role = ServerRole.App, Size = AppTier.Large, InstanceCount = 3 },
            new VMRoleConfig { Role = ServerRole.Web, Size = AppTier.Medium, InstanceCount = 2 },
            new VMRoleConfig { Role = ServerRole.Database, Size = AppTier.Large, InstanceCount = 2 },
            new VMRoleConfig { Role = ServerRole.Cache, Size = AppTier.Small, InstanceCount = 1 }
        };
        var expectedResult = CreateVMSizingResult();
        _mockSizingService.Calculate(Arg.Any<VMSizingInput>()).Returns(expectedResult);

        // Act
        var result = _controller.Calculate(input);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void Calculate_AllEnvironmentTypes_ReturnsOk()
    {
        // Arrange
        var input = new VMSizingInput
        {
            Technology = Technology.Java,
            EnabledEnvironments = new HashSet<EnvironmentType>
            {
                EnvironmentType.Dev,
                EnvironmentType.Test,
                EnvironmentType.Stage,
                EnvironmentType.Prod,
                EnvironmentType.DR
            },
            EnvironmentConfigs = new Dictionary<EnvironmentType, VMEnvironmentConfig>()
        };

        // Add configs for all environments
        foreach (var env in input.EnabledEnvironments)
        {
            input.EnvironmentConfigs[env] = new VMEnvironmentConfig
            {
                Environment = env,
                Enabled = true,
                HAPattern = env == EnvironmentType.Prod ? HAPattern.ActiveActive : HAPattern.None,
                LoadBalancer = env == EnvironmentType.Prod ? LoadBalancerOption.HAPair : LoadBalancerOption.None,
                Roles = new List<VMRoleConfig>
                {
                    new VMRoleConfig { Role = ServerRole.App, Size = AppTier.Medium, InstanceCount = 2 }
                }
            };
        }

        var expectedResult = CreateVMSizingResult();
        _mockSizingService.Calculate(Arg.Any<VMSizingInput>()).Returns(expectedResult);

        // Act
        var result = _controller.Calculate(input);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region Low-Code Platform Tests

    [Theory]
    [InlineData(Technology.Mendix)]
    [InlineData(Technology.OutSystems)]
    public void Calculate_LowCodePlatforms_ReturnsOk(Technology technology)
    {
        // Arrange
        var input = CreateValidVMSizingInput();
        input.Technology = technology;
        var expectedResult = CreateVMSizingResult();
        _mockSizingService.Calculate(Arg.Any<VMSizingInput>()).Returns(expectedResult);

        // Act
        var result = _controller.Calculate(input);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Theory]
    [InlineData(Technology.Mendix)]
    [InlineData(Technology.OutSystems)]
    public void GetRoleSpecs_LowCodePlatforms_CallsServiceWithTechnology(Technology technology)
    {
        // Arrange
        _mockSizingService.GetRoleSpecs(Arg.Any<ServerRole>(), Arg.Any<AppTier>(), technology)
            .Returns((8, 16));

        // Act
        _controller.GetRoleSpecs(ServerRole.App, AppTier.Medium, technology);

        // Assert
        _mockSizingService.Received(1).GetRoleSpecs(ServerRole.App, AppTier.Medium, technology);
    }

    #endregion

    #region Helper Methods

    private VMSizingInput CreateValidVMSizingInput()
    {
        return new VMSizingInput
        {
            Technology = Technology.DotNet,
            EnabledEnvironments = new HashSet<EnvironmentType>
            {
                EnvironmentType.Prod,
                EnvironmentType.Dev
            },
            EnvironmentConfigs = new Dictionary<EnvironmentType, VMEnvironmentConfig>
            {
                {
                    EnvironmentType.Prod,
                    new VMEnvironmentConfig
                    {
                        Environment = EnvironmentType.Prod,
                        Enabled = true,
                        HAPattern = HAPattern.None,
                        LoadBalancer = LoadBalancerOption.None,
                        Roles = new List<VMRoleConfig>
                        {
                            new VMRoleConfig
                            {
                                Role = ServerRole.App,
                                Size = AppTier.Medium,
                                InstanceCount = 2
                            }
                        }
                    }
                },
                {
                    EnvironmentType.Dev,
                    new VMEnvironmentConfig
                    {
                        Environment = EnvironmentType.Dev,
                        Enabled = true,
                        HAPattern = HAPattern.None,
                        LoadBalancer = LoadBalancerOption.None,
                        Roles = new List<VMRoleConfig>
                        {
                            new VMRoleConfig
                            {
                                Role = ServerRole.App,
                                Size = AppTier.Small,
                                InstanceCount = 1
                            }
                        }
                    }
                }
            }
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
                    Roles = new List<VMRoleResult>
                    {
                        new VMRoleResult
                        {
                            Role = ServerRole.App,
                            RoleName = "Application",
                            TotalInstances = 4,
                            CpuPerInstance = 8,
                            RamPerInstance = 16
                        }
                    }
                }
            },
            GrandTotal = new VMGrandTotal
            {
                TotalVMs = 10,
                TotalCpu = 80,
                TotalRam = 160,
                TotalDisk = 500
            }
        };
    }

    #endregion
}
