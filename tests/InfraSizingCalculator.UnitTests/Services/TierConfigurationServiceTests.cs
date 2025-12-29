using FluentAssertions;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Services;
using InfraSizingCalculator.Services.Interfaces;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Services;

/// <summary>
/// Tests for TierConfigurationService which manages tier CPU/RAM configuration
/// across different technologies.
/// </summary>
public class TierConfigurationServiceTests
{
    private readonly ITierConfigurationService _service = new TierConfigurationService();

    #region GetTierCpu Tests

    [Theory]
    [InlineData("dotnet", "Small", 0.25)]
    [InlineData("dotnet", "Medium", 0.5)]
    [InlineData("dotnet", "Large", 1.0)]
    [InlineData("dotnet", "XLarge", 2.0)]
    [InlineData("java", "Small", 0.5)]
    [InlineData("java", "Medium", 1.0)]
    [InlineData("java", "Large", 2.0)]
    [InlineData("java", "XLarge", 4.0)]
    [InlineData("nodejs", "Small", 0.25)]
    [InlineData("nodejs", "Medium", 0.5)]
    [InlineData("nodejs", "Large", 1.0)]
    [InlineData("nodejs", "XLarge", 2.0)]
    [InlineData("python", "Small", 0.25)]
    [InlineData("python", "Medium", 0.5)]
    [InlineData("python", "Large", 1.0)]
    [InlineData("python", "XLarge", 2.0)]
    [InlineData("go", "Small", 0.125)]
    [InlineData("go", "Medium", 0.25)]
    [InlineData("go", "Large", 0.5)]
    [InlineData("go", "XLarge", 1.0)]
    [InlineData("mendix", "Small", 0.5)]
    [InlineData("mendix", "Medium", 1.0)]
    [InlineData("mendix", "Large", 2.0)]
    [InlineData("mendix", "XLarge", 4.0)]
    public void GetTierCpu_ReturnsDefaultValue_ForEachTechnology(string technology, string tier, double expectedCpu)
    {
        // Arrange
        var settings = new UICalculatorSettings();

        // Act
        var cpu = _service.GetTierCpu(settings, technology, tier);

        // Assert
        cpu.Should().Be(expectedCpu);
    }

    [Fact]
    public void GetTierCpu_IsCaseInsensitive()
    {
        // Arrange
        var settings = new UICalculatorSettings();

        // Act
        var cpuLower = _service.GetTierCpu(settings, "dotnet", "Small");
        var cpuUpper = _service.GetTierCpu(settings, "DOTNET", "Small");

        // Assert
        cpuLower.Should().Be(cpuUpper);
    }

    [Fact]
    public void GetTierCpu_ReturnsDefault_ForUnknownTechnology()
    {
        // Arrange
        var settings = new UICalculatorSettings();

        // Act
        var cpu = _service.GetTierCpu(settings, "unknown", "Small");

        // Assert
        cpu.Should().Be(0.5); // Default CPU
    }

    [Fact]
    public void GetTierCpu_ReturnsDefault_ForUnknownTier()
    {
        // Arrange
        var settings = new UICalculatorSettings();

        // Act
        var cpu = _service.GetTierCpu(settings, "dotnet", "Unknown");

        // Assert
        cpu.Should().Be(0.5); // Default CPU
    }

    [Fact]
    public void GetTierCpu_ThrowsArgumentNullException_WhenSettingsNull()
    {
        // Act
        var act = () => _service.GetTierCpu(null!, "dotnet", "Small");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region GetTierRam Tests

    [Theory]
    [InlineData("dotnet", "Small", 0.5)]
    [InlineData("dotnet", "Medium", 1.0)]
    [InlineData("dotnet", "Large", 2.0)]
    [InlineData("dotnet", "XLarge", 4.0)]
    [InlineData("java", "Small", 1.0)]
    [InlineData("java", "Medium", 2.0)]
    [InlineData("java", "Large", 4.0)]
    [InlineData("java", "XLarge", 8.0)]
    [InlineData("nodejs", "Small", 1.0)]
    [InlineData("nodejs", "Medium", 1.0)]
    [InlineData("nodejs", "Large", 2.0)]
    [InlineData("nodejs", "XLarge", 4.0)]
    [InlineData("python", "Small", 1.0)]
    [InlineData("python", "Medium", 1.0)]
    [InlineData("python", "Large", 2.0)]
    [InlineData("python", "XLarge", 4.0)]
    [InlineData("go", "Small", 0.25)]
    [InlineData("go", "Medium", 0.5)]
    [InlineData("go", "Large", 1.0)]
    [InlineData("go", "XLarge", 2.0)]
    [InlineData("mendix", "Small", 1.0)]
    [InlineData("mendix", "Medium", 2.0)]
    [InlineData("mendix", "Large", 4.0)]
    [InlineData("mendix", "XLarge", 8.0)]
    public void GetTierRam_ReturnsDefaultValue_ForEachTechnology(string technology, string tier, double expectedRam)
    {
        // Arrange
        var settings = new UICalculatorSettings();

        // Act
        var ram = _service.GetTierRam(settings, technology, tier);

        // Assert
        ram.Should().Be(expectedRam);
    }

    [Fact]
    public void GetTierRam_ReturnsDefault_ForUnknownTechnology()
    {
        // Arrange
        var settings = new UICalculatorSettings();

        // Act
        var ram = _service.GetTierRam(settings, "rust", "Small");

        // Assert
        ram.Should().Be(1.0); // Default RAM
    }

    [Fact]
    public void GetTierRam_ThrowsArgumentNullException_WhenSettingsNull()
    {
        // Act
        var act = () => _service.GetTierRam(null!, "dotnet", "Small");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region SetTierCpu Tests

    [Theory]
    [InlineData("dotnet", "Small")]
    [InlineData("dotnet", "Medium")]
    [InlineData("dotnet", "Large")]
    [InlineData("dotnet", "XLarge")]
    [InlineData("java", "Small")]
    [InlineData("java", "Medium")]
    [InlineData("java", "Large")]
    [InlineData("java", "XLarge")]
    [InlineData("nodejs", "Small")]
    [InlineData("nodejs", "Medium")]
    [InlineData("nodejs", "Large")]
    [InlineData("nodejs", "XLarge")]
    [InlineData("python", "Small")]
    [InlineData("python", "Medium")]
    [InlineData("python", "Large")]
    [InlineData("python", "XLarge")]
    [InlineData("go", "Small")]
    [InlineData("go", "Medium")]
    [InlineData("go", "Large")]
    [InlineData("go", "XLarge")]
    [InlineData("mendix", "Small")]
    [InlineData("mendix", "Medium")]
    [InlineData("mendix", "Large")]
    [InlineData("mendix", "XLarge")]
    public void SetTierCpu_UpdatesValue_ForEachTechnology(string technology, string tier)
    {
        // Arrange
        var settings = new UICalculatorSettings();
        var newValue = 99.9;

        // Act
        _service.SetTierCpu(settings, technology, tier, newValue);
        var result = _service.GetTierCpu(settings, technology, tier);

        // Assert
        result.Should().Be(newValue);
    }

    [Fact]
    public void SetTierCpu_DoesNotThrow_ForUnknownTechnology()
    {
        // Arrange
        var settings = new UICalculatorSettings();

        // Act
        var act = () => _service.SetTierCpu(settings, "unknown", "Small", 1.0);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void SetTierCpu_ThrowsArgumentNullException_WhenSettingsNull()
    {
        // Act
        var act = () => _service.SetTierCpu(null!, "dotnet", "Small", 1.0);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region SetTierRam Tests

    [Theory]
    [InlineData("dotnet", "Small")]
    [InlineData("java", "Medium")]
    [InlineData("nodejs", "Large")]
    [InlineData("python", "XLarge")]
    [InlineData("go", "Small")]
    [InlineData("mendix", "Large")]
    public void SetTierRam_UpdatesValue_ForVariousTechnologies(string technology, string tier)
    {
        // Arrange
        var settings = new UICalculatorSettings();
        var newValue = 128.0;

        // Act
        _service.SetTierRam(settings, technology, tier, newValue);
        var result = _service.GetTierRam(settings, technology, tier);

        // Assert
        result.Should().Be(newValue);
    }

    [Fact]
    public void SetTierRam_ThrowsArgumentNullException_WhenSettingsNull()
    {
        // Act
        var act = () => _service.SetTierRam(null!, "dotnet", "Small", 1.0);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region GetSupportedTechnologies Tests

    [Fact]
    public void GetSupportedTechnologies_ReturnsAllTechnologies()
    {
        // Act
        var technologies = _service.GetSupportedTechnologies();

        // Assert
        technologies.Should().BeEquivalentTo(new[] { "dotnet", "java", "nodejs", "python", "go", "mendix" });
    }

    [Fact]
    public void GetSupportedTechnologies_ReturnsReadOnlyList()
    {
        // Act
        var technologies = _service.GetSupportedTechnologies();

        // Assert
        technologies.Should().BeAssignableTo<IReadOnlyList<string>>();
    }

    #endregion

    #region GetSupportedTiers Tests

    [Fact]
    public void GetSupportedTiers_ReturnsAllTiers()
    {
        // Act
        var tiers = _service.GetSupportedTiers();

        // Assert
        tiers.Should().BeEquivalentTo(new[] { "Small", "Medium", "Large", "XLarge" });
    }

    [Fact]
    public void GetSupportedTiers_ReturnsReadOnlyList()
    {
        // Act
        var tiers = _service.GetSupportedTiers();

        // Assert
        tiers.Should().BeAssignableTo<IReadOnlyList<string>>();
    }

    #endregion

    #region Round-Trip Tests

    [Fact]
    public void RoundTrip_SetAndGet_MaintainsValue()
    {
        // Arrange
        var settings = new UICalculatorSettings();
        var technologies = _service.GetSupportedTechnologies();
        var tiers = _service.GetSupportedTiers();

        // Act & Assert - Set custom values and verify retrieval
        var counter = 1.0;
        foreach (var tech in technologies)
        {
            foreach (var tier in tiers)
            {
                var cpuValue = counter;
                var ramValue = counter * 2;

                _service.SetTierCpu(settings, tech, tier, cpuValue);
                _service.SetTierRam(settings, tech, tier, ramValue);

                _service.GetTierCpu(settings, tech, tier).Should().Be(cpuValue);
                _service.GetTierRam(settings, tech, tier).Should().Be(ramValue);

                counter++;
            }
        }
    }

    [Fact]
    public void MultipleSettings_AreIndependent()
    {
        // Arrange
        var settings1 = new UICalculatorSettings();
        var settings2 = new UICalculatorSettings();

        // Act
        _service.SetTierCpu(settings1, "dotnet", "Small", 99.0);
        _service.SetTierRam(settings1, "dotnet", "Small", 88.0);

        // Assert - settings2 should have defaults
        _service.GetTierCpu(settings2, "dotnet", "Small").Should().Be(0.25);
        _service.GetTierRam(settings2, "dotnet", "Small").Should().Be(0.5);
    }

    #endregion
}
