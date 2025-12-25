using FluentAssertions;
using InfraSizingCalculator.Controllers.Api;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace InfraSizingCalculator.UnitTests;

public class TechnologiesControllerTests
{
    private readonly ITechnologyService _mockTechnologyService;
    private readonly TechnologiesController _controller;

    public TechnologiesControllerTests()
    {
        _mockTechnologyService = Substitute.For<ITechnologyService>();
        _controller = new TechnologiesController(_mockTechnologyService);

        // Setup default mock behavior
        SetupDefaultMockBehavior();
    }

    private void SetupDefaultMockBehavior()
    {
        foreach (var tech in Enum.GetValues<Technology>())
        {
            _mockTechnologyService.GetConfig(tech).Returns(CreateTechnologyConfig(tech));
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
    public void GetAll_ReturnsAllTechnologies()
    {
        // Act
        var result = _controller.GetAll();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var technologies = okResult.Value.Should().BeAssignableTo<IEnumerable<TechnologyInfo>>().Subject;
        technologies.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void GetAll_CallsServiceForEachTechnology()
    {
        // Act
        _controller.GetAll();

        // Assert
        foreach (var tech in Enum.GetValues<Technology>())
        {
            _mockTechnologyService.Received().GetConfig(tech);
        }
    }

    [Fact]
    public void GetAll_TechnologiesHaveName()
    {
        // Act
        var result = _controller.GetAll();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var technologies = okResult.Value.Should().BeAssignableTo<IEnumerable<TechnologyInfo>>().Subject;
        technologies.Should().AllSatisfy(t => t.Name.Should().NotBeNullOrEmpty());
    }

    [Fact]
    public void GetAll_TechnologiesHaveIcon()
    {
        // Act
        var result = _controller.GetAll();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var technologies = okResult.Value.Should().BeAssignableTo<IEnumerable<TechnologyInfo>>().Subject;
        technologies.Should().AllSatisfy(t => t.Icon.Should().NotBeNullOrEmpty());
    }

    [Fact]
    public void GetAll_ContainsDotNet()
    {
        // Act
        var result = _controller.GetAll();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var technologies = okResult.Value.Should().BeAssignableTo<IEnumerable<TechnologyInfo>>().Subject;
        technologies.Should().Contain(t => t.Technology == Technology.DotNet);
    }

    [Fact]
    public void GetAll_ContainsJava()
    {
        // Act
        var result = _controller.GetAll();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var technologies = okResult.Value.Should().BeAssignableTo<IEnumerable<TechnologyInfo>>().Subject;
        technologies.Should().Contain(t => t.Technology == Technology.Java);
    }

    [Fact]
    public void GetAll_MendixHasLowCodePlatformType()
    {
        // Act
        var result = _controller.GetAll();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var technologies = okResult.Value.Should().BeAssignableTo<IEnumerable<TechnologyInfo>>().Subject;
        var mendix = technologies.FirstOrDefault(t => t.Technology == Technology.Mendix);
        mendix.Should().NotBeNull();
        mendix!.PlatformType.Should().Be(PlatformType.LowCode);
    }

    [Fact]
    public void GetAll_NonMendixHaveNativePlatformType()
    {
        // Act
        var result = _controller.GetAll();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var technologies = okResult.Value.Should().BeAssignableTo<IEnumerable<TechnologyInfo>>().Subject;
        var nonMendix = technologies.Where(t => t.Technology != Technology.Mendix);
        nonMendix.Should().AllSatisfy(t => t.PlatformType.Should().Be(PlatformType.Native));
    }

    #endregion

    #region Get Tests

    [Theory]
    [InlineData(Technology.DotNet)]
    [InlineData(Technology.Java)]
    [InlineData(Technology.NodeJs)]
    [InlineData(Technology.Go)]
    [InlineData(Technology.Mendix)]
    public void Get_ValidTechnology_ReturnsOk(Technology technology)
    {
        // Act
        var result = _controller.Get(technology);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void Get_ValidTechnology_ReturnsTechnologyInfo()
    {
        // Act
        var result = _controller.Get(Technology.DotNet);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var techInfo = okResult.Value.Should().BeOfType<TechnologyInfo>().Subject;
        techInfo.Technology.Should().Be(Technology.DotNet);
    }

    [Fact]
    public void Get_ValidTechnology_CallsService()
    {
        // Act
        _controller.Get(Technology.Java);

        // Assert
        _mockTechnologyService.Received(1).GetConfig(Technology.Java);
    }

    [Fact]
    public void Get_ServiceThrowsArgumentException_ReturnsNotFound()
    {
        // Arrange
        _mockTechnologyService.GetConfig(Arg.Any<Technology>())
            .Returns(x => throw new ArgumentException("Not found"));

        // Act
        var result = _controller.Get(Technology.DotNet);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public void Get_DotNet_ReturnsCorrectName()
    {
        // Act
        var result = _controller.Get(Technology.DotNet);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var techInfo = okResult.Value.Should().BeOfType<TechnologyInfo>().Subject;
        techInfo.Name.Should().Be(".NET");
    }

    [Fact]
    public void Get_Java_ReturnsCorrectName()
    {
        // Act
        var result = _controller.Get(Technology.Java);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var techInfo = okResult.Value.Should().BeOfType<TechnologyInfo>().Subject;
        techInfo.Name.Should().Be("Java");
    }

    [Fact]
    public void Get_NodeJs_ReturnsCorrectName()
    {
        // Act
        var result = _controller.Get(Technology.NodeJs);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var techInfo = okResult.Value.Should().BeOfType<TechnologyInfo>().Subject;
        techInfo.Name.Should().Be("Node.js");
    }

    [Fact]
    public void Get_Technology_IncludesTierSpecs()
    {
        // Act
        var result = _controller.Get(Technology.DotNet);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var techInfo = okResult.Value.Should().BeOfType<TechnologyInfo>().Subject;
        techInfo.TierSpecs.Should().NotBeNull();
    }

    #endregion

    #region TechnologyInfo Tests

    [Fact]
    public void TechnologyInfo_DefaultValuesAreCorrect()
    {
        // Arrange & Act
        var info = new TechnologyInfo();

        // Assert
        info.Name.Should().BeEmpty();
        info.Icon.Should().BeEmpty();
        info.TierSpecs.Should().NotBeNull();
        info.TierSpecs.Should().BeEmpty();
    }

    #endregion

    #region Helper Methods

    private TechnologyConfig CreateTechnologyConfig(Technology technology)
    {
        return new TechnologyConfig
        {
            Technology = technology,
            Name = technology.ToString(),
            Tiers = new Dictionary<AppTier, TierSpecs>
            {
                [AppTier.Small] = new TierSpecs(1, 2),
                [AppTier.Medium] = new TierSpecs(2, 4),
                [AppTier.Large] = new TierSpecs(4, 8),
                [AppTier.XLarge] = new TierSpecs(8, 16)
            }
        };
    }

    #endregion
}
