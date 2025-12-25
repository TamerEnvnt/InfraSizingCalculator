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

public class K8sControllerTests
{
    private readonly IK8sSizingService _mockSizingService;
    private readonly ILogger<K8sController> _mockLogger;
    private readonly K8sController _controller;

    public K8sControllerTests()
    {
        _mockSizingService = Substitute.For<IK8sSizingService>();
        _mockLogger = Substitute.For<ILogger<K8sController>>();
        _controller = new K8sController(_mockSizingService, _mockLogger);
    }

    #region Calculate Tests

    [Fact]
    public void Calculate_ValidInput_ReturnsOkWithResult()
    {
        // Arrange
        var input = CreateValidK8sSizingInput();
        var expectedResult = CreateK8sSizingResult();
        _mockSizingService.Calculate(Arg.Any<K8sSizingInput>()).Returns(expectedResult);

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
    public void Calculate_InvalidInput_ReturnsBadRequest()
    {
        // Arrange
        var input = new K8sSizingInput
        {
            ProdApps = new AppConfig { Small = -1 } // Invalid negative apps
        };

        // Act
        var result = _controller.Calculate(input);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void Calculate_ValidInput_CallsSizingService()
    {
        // Arrange
        var input = CreateValidK8sSizingInput();
        var expectedResult = CreateK8sSizingResult();
        _mockSizingService.Calculate(Arg.Any<K8sSizingInput>()).Returns(expectedResult);

        // Act
        _controller.Calculate(input);

        // Assert
        _mockSizingService.Received(1).Calculate(Arg.Any<K8sSizingInput>());
    }

    [Fact]
    public void Calculate_ValidInput_LogsInformation()
    {
        // Arrange
        var input = CreateValidK8sSizingInput();
        var expectedResult = CreateK8sSizingResult();
        _mockSizingService.Calculate(Arg.Any<K8sSizingInput>()).Returns(expectedResult);

        // Act
        _controller.Calculate(input);

        // Assert
        _mockLogger.ReceivedWithAnyArgs().LogInformation(default!);
    }

    [Fact]
    public void Calculate_DifferentDistributions_ReturnsOk()
    {
        // Arrange
        var input = CreateValidK8sSizingInput();
        var expectedResult = CreateK8sSizingResult();
        _mockSizingService.Calculate(Arg.Any<K8sSizingInput>()).Returns(expectedResult);

        var distributions = new[]
        {
            Distribution.Kubernetes,
            Distribution.OpenShift,
            Distribution.EKS,
            Distribution.AKS,
            Distribution.GKE,
            Distribution.OKE,
            Distribution.Rancher,
            Distribution.K3s
        };

        foreach (var dist in distributions)
        {
            input.Distribution = dist;

            // Act
            var result = _controller.Calculate(input);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
        }
    }

    [Fact]
    public void Calculate_WithHeadroomSettings_ReturnsOk()
    {
        // Arrange
        var input = CreateValidK8sSizingInput();
        input.EnableHeadroom = true;
        input.Headroom = new HeadroomSettings
        {
            Prod = 40,
            DR = 30,
            Stage = 25,
            Test = 20,
            Dev = 15
        };
        var expectedResult = CreateK8sSizingResult();
        _mockSizingService.Calculate(Arg.Any<K8sSizingInput>()).Returns(expectedResult);

        // Act
        var result = _controller.Calculate(input);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void Calculate_WithOvercommitSettings_ReturnsOk()
    {
        // Arrange
        var input = CreateValidK8sSizingInput();
        input.ProdOvercommit = new OvercommitSettings { Cpu = 1.5, Memory = 1.2 };
        input.NonProdOvercommit = new OvercommitSettings { Cpu = 2.0, Memory = 1.5 };
        var expectedResult = CreateK8sSizingResult();
        _mockSizingService.Calculate(Arg.Any<K8sSizingInput>()).Returns(expectedResult);

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
        var input = CreateValidK8sSizingInput();

        // Act
        var result = _controller.Validate(input);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value;
        response.Should().NotBeNull();
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
    public void Validate_InvalidInput_ReturnsBadRequest()
    {
        // Arrange
        var input = new K8sSizingInput
        {
            ProdApps = new AppConfig { Small = -1 } // Invalid
        };

        // Act
        var result = _controller.Validate(input);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void Validate_ValidInput_DoesNotCallSizingService()
    {
        // Arrange
        var input = CreateValidK8sSizingInput();

        // Act
        _controller.Validate(input);

        // Assert
        _mockSizingService.DidNotReceive().Calculate(Arg.Any<K8sSizingInput>());
    }

    [Fact]
    public void Validate_ValidInput_ReturnsValidTrue()
    {
        // Arrange
        var input = CreateValidK8sSizingInput();

        // Act
        var result = _controller.Validate(input);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var responseJson = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
        responseJson.Should().Contain("\"valid\":true");
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
        errorResponse.Message.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region AppProfile Validation Tests

    [Fact]
    public void Calculate_ZeroApps_ReturnsOk()
    {
        // Arrange
        var input = CreateValidK8sSizingInput();
        input.ProdApps = new AppConfig { Small = 0, Medium = 0, Large = 0, XLarge = 0 };
        input.NonProdApps = new AppConfig { Small = 0, Medium = 0, Large = 0, XLarge = 0 };
        var expectedResult = CreateK8sSizingResult();
        _mockSizingService.Calculate(Arg.Any<K8sSizingInput>()).Returns(expectedResult);

        // Act
        var result = _controller.Calculate(input);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void Calculate_LargeAppCount_ReturnsOk()
    {
        // Arrange
        var input = CreateValidK8sSizingInput();
        input.ProdApps = new AppConfig { Small = 250, Medium = 250, Large = 250, XLarge = 250 };
        var expectedResult = CreateK8sSizingResult();
        _mockSizingService.Calculate(Arg.Any<K8sSizingInput>()).Returns(expectedResult);

        // Act
        var result = _controller.Calculate(input);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region Helper Methods

    private K8sSizingInput CreateValidK8sSizingInput()
    {
        return new K8sSizingInput
        {
            Distribution = Distribution.Kubernetes,
            ProdApps = new AppConfig
            {
                Small = 5,
                Medium = 10,
                Large = 5,
                XLarge = 0
            },
            NonProdApps = new AppConfig
            {
                Small = 5,
                Medium = 10,
                Large = 5,
                XLarge = 0
            },
            EnableHeadroom = true,
            Headroom = new HeadroomSettings
            {
                Prod = 30,
                DR = 30,
                Stage = 25,
                Test = 20,
                Dev = 15
            },
            ProdOvercommit = new OvercommitSettings { Cpu = 1.0, Memory = 1.0 },
            NonProdOvercommit = new OvercommitSettings { Cpu = 2.0, Memory = 1.5 }
        };
    }

    private K8sSizingResult CreateK8sSizingResult()
    {
        var input = CreateValidK8sSizingInput();
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
                    Workers = 10,
                    Infra = 3,
                    TotalNodes = 16,
                    TotalCpu = 160,
                    TotalRam = 320,
                    TotalDisk = 1000
                }
            },
            GrandTotal = new GrandTotal
            {
                TotalNodes = 20,
                TotalMasters = 3,
                TotalWorkers = 10,
                TotalInfra = 3,
                TotalCpu = 160,
                TotalRam = 320,
                TotalDisk = 1000
            },
            Configuration = input
        };
    }

    #endregion
}
