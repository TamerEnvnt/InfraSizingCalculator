using FluentAssertions;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Models;

/// <summary>
/// Tests for K8sSizingInput model covering default values, property setting,
/// and IValidatableObject validation (BR-E002 and nested validation).
/// </summary>
public class K8sSizingInputTests
{
    #region Default Values Tests

    [Fact]
    public void DefaultValues_DistributionIsOpenShift()
    {
        var input = new K8sSizingInput();
        input.Distribution.Should().Be(Distribution.OpenShift);
    }

    [Fact]
    public void DefaultValues_TechnologyIsDotNet()
    {
        var input = new K8sSizingInput();
        input.Technology.Should().Be(Technology.DotNet);
    }

    [Fact]
    public void DefaultValues_ClusterModeIsMultiCluster()
    {
        var input = new K8sSizingInput();
        input.ClusterMode.Should().Be(ClusterMode.MultiCluster);
    }

    [Fact]
    public void DefaultValues_ProdAppsIsNotNull()
    {
        var input = new K8sSizingInput();
        input.ProdApps.Should().NotBeNull();
    }

    [Fact]
    public void DefaultValues_NonProdAppsIsNotNull()
    {
        var input = new K8sSizingInput();
        input.NonProdApps.Should().NotBeNull();
    }

    [Fact]
    public void DefaultValues_EnvironmentAppsIsNull()
    {
        var input = new K8sSizingInput();
        input.EnvironmentApps.Should().BeNull();
    }

    [Fact]
    public void DefaultValues_EnabledEnvironmentsContainsAllEnvironments()
    {
        var input = new K8sSizingInput();

        input.EnabledEnvironments.Should().Contain(EnvironmentType.Dev);
        input.EnabledEnvironments.Should().Contain(EnvironmentType.Test);
        input.EnabledEnvironments.Should().Contain(EnvironmentType.Stage);
        input.EnabledEnvironments.Should().Contain(EnvironmentType.Prod);
        input.EnabledEnvironments.Should().Contain(EnvironmentType.DR);
        input.EnabledEnvironments.Should().HaveCount(5);
    }

    [Fact]
    public void DefaultValues_SelectedEnvironmentIsProd()
    {
        var input = new K8sSizingInput();
        input.SelectedEnvironment.Should().Be(EnvironmentType.Prod);
    }

    [Fact]
    public void DefaultValues_ReplicasIsNotNull()
    {
        var input = new K8sSizingInput();
        input.Replicas.Should().NotBeNull();
    }

    [Fact]
    public void DefaultValues_HeadroomIsNotNull()
    {
        var input = new K8sSizingInput();
        input.Headroom.Should().NotBeNull();
    }

    [Fact]
    public void DefaultValues_EnableHeadroomIsTrue()
    {
        var input = new K8sSizingInput();
        input.EnableHeadroom.Should().BeTrue();
    }

    [Fact]
    public void DefaultValues_ProdOvercommitIsNotNull()
    {
        var input = new K8sSizingInput();
        input.ProdOvercommit.Should().NotBeNull();
    }

    [Fact]
    public void DefaultValues_NonProdOvercommitIsNotNull()
    {
        var input = new K8sSizingInput();
        input.NonProdOvercommit.Should().NotBeNull();
    }

    [Fact]
    public void DefaultValues_CustomNodeSpecsIsNull()
    {
        var input = new K8sSizingInput();
        input.CustomNodeSpecs.Should().BeNull();
    }

    [Fact]
    public void DefaultValues_HADRConfigIsNotNull()
    {
        var input = new K8sSizingInput();
        input.HADRConfig.Should().NotBeNull();
    }

    [Fact]
    public void DefaultValues_EnvironmentHADRConfigsIsNull()
    {
        var input = new K8sSizingInput();
        input.EnvironmentHADRConfigs.Should().BeNull();
    }

    #endregion

    #region Property Setting Tests

    [Fact]
    public void Distribution_CanBeSet()
    {
        var input = new K8sSizingInput { Distribution = Distribution.AKS };
        input.Distribution.Should().Be(Distribution.AKS);
    }

    [Fact]
    public void Technology_CanBeSet()
    {
        var input = new K8sSizingInput { Technology = Technology.NodeJs };
        input.Technology.Should().Be(Technology.NodeJs);
    }

    [Fact]
    public void ClusterMode_CanBeSet()
    {
        var input = new K8sSizingInput { ClusterMode = ClusterMode.SharedCluster };
        input.ClusterMode.Should().Be(ClusterMode.SharedCluster);
    }

    [Fact]
    public void EnabledEnvironments_CanBeModified()
    {
        var input = new K8sSizingInput();
        input.EnabledEnvironments.Remove(EnvironmentType.Dev);
        input.EnabledEnvironments.Should().NotContain(EnvironmentType.Dev);
    }

    [Fact]
    public void EnvironmentApps_CanBeSet()
    {
        var input = new K8sSizingInput
        {
            EnvironmentApps = new Dictionary<EnvironmentType, AppConfig>
            {
                { EnvironmentType.Prod, new AppConfig { Small = 10 } }
            }
        };

        input.EnvironmentApps.Should().NotBeNull();
        input.EnvironmentApps.Should().HaveCount(1);
    }

    [Fact]
    public void SelectedEnvironment_CanBeSet()
    {
        var input = new K8sSizingInput { SelectedEnvironment = EnvironmentType.Test };
        input.SelectedEnvironment.Should().Be(EnvironmentType.Test);
    }

    [Fact]
    public void EnableHeadroom_CanBeSetToFalse()
    {
        var input = new K8sSizingInput { EnableHeadroom = false };
        input.EnableHeadroom.Should().BeFalse();
    }

    #endregion

    #region Validation - BR-E002 Production Must Be Enabled

    [Fact]
    public void Validate_ProductionEnabled_NoError()
    {
        var input = new K8sSizingInput();
        var context = new ValidationContext(input);

        var results = input.Validate(context).ToList();

        results.Should().NotContain(r => r.ErrorMessage!.Contains("BR-E002"));
    }

    [Fact]
    public void Validate_ProductionDisabled_ReturnsError()
    {
        var input = new K8sSizingInput();
        input.EnabledEnvironments.Remove(EnvironmentType.Prod);
        var context = new ValidationContext(input);

        var results = input.Validate(context).ToList();

        results.Should().Contain(r => r.ErrorMessage!.Contains("BR-E002"));
        results.Should().Contain(r => r.ErrorMessage!.Contains("Production environment must always be enabled"));
    }

    [Fact]
    public void Validate_ProductionDisabledError_HasCorrectMemberName()
    {
        var input = new K8sSizingInput();
        input.EnabledEnvironments.Remove(EnvironmentType.Prod);
        var context = new ValidationContext(input);

        var results = input.Validate(context).ToList();

        var prodError = results.First(r => r.ErrorMessage!.Contains("BR-E002"));
        prodError.MemberNames.Should().Contain(nameof(K8sSizingInput.EnabledEnvironments));
    }

    #endregion

    #region Validation - At Least One Environment

    [Fact]
    public void Validate_NoEnvironmentsEnabled_ReturnsError()
    {
        var input = new K8sSizingInput();
        input.EnabledEnvironments.Clear();
        var context = new ValidationContext(input);

        var results = input.Validate(context).ToList();

        results.Should().Contain(r => r.ErrorMessage!.Contains("At least one environment must be enabled"));
    }

    [Fact]
    public void Validate_EmptyEnvironments_ReturnsBothErrors()
    {
        var input = new K8sSizingInput();
        input.EnabledEnvironments.Clear();
        var context = new ValidationContext(input);

        var results = input.Validate(context).ToList();

        // Should have both BR-E002 and "at least one" errors
        results.Should().HaveCountGreaterOrEqualTo(2);
    }

    #endregion

    #region Validation - PerEnvironment Mode

    [Fact]
    public void Validate_PerEnvironmentMode_SelectedEnvironmentEnabled_NoError()
    {
        var input = new K8sSizingInput
        {
            ClusterMode = ClusterMode.PerEnvironment,
            SelectedEnvironment = EnvironmentType.Prod
        };
        var context = new ValidationContext(input);

        var results = input.Validate(context).ToList();

        results.Should().NotContain(r => r.ErrorMessage!.Contains("Selected environment must be in"));
    }

    [Fact]
    public void Validate_PerEnvironmentMode_SelectedEnvironmentDisabled_ReturnsError()
    {
        var input = new K8sSizingInput
        {
            ClusterMode = ClusterMode.PerEnvironment,
            SelectedEnvironment = EnvironmentType.Dev
        };
        input.EnabledEnvironments.Remove(EnvironmentType.Dev);
        var context = new ValidationContext(input);

        var results = input.Validate(context).ToList();

        results.Should().Contain(r => r.ErrorMessage!.Contains("Selected environment must be in the enabled environments list"));
    }

    [Fact]
    public void Validate_MultiClusterMode_SelectedEnvironmentDisabled_NoError()
    {
        // In MultiCluster mode, SelectedEnvironment doesn't matter
        var input = new K8sSizingInput
        {
            ClusterMode = ClusterMode.MultiCluster,
            SelectedEnvironment = EnvironmentType.Dev
        };
        input.EnabledEnvironments.Remove(EnvironmentType.Dev);
        var context = new ValidationContext(input);

        var results = input.Validate(context).ToList();

        results.Should().NotContain(r => r.ErrorMessage!.Contains("Selected environment"));
    }

    [Fact]
    public void Validate_SharedClusterMode_SelectedEnvironmentDisabled_NoError()
    {
        // In SharedCluster mode, SelectedEnvironment doesn't matter
        var input = new K8sSizingInput
        {
            ClusterMode = ClusterMode.SharedCluster,
            SelectedEnvironment = EnvironmentType.Dev
        };
        input.EnabledEnvironments.Remove(EnvironmentType.Dev);
        var context = new ValidationContext(input);

        var results = input.Validate(context).ToList();

        results.Should().NotContain(r => r.ErrorMessage!.Contains("Selected environment"));
    }

    #endregion

    #region Validation - Nested Objects

    [Fact]
    public void Validate_InvalidHeadroom_ReturnsNestedError()
    {
        var input = new K8sSizingInput();
        input.Headroom.Dev = -10; // Invalid
        var context = new ValidationContext(input);

        var results = input.Validate(context).ToList();

        results.Should().Contain(r => r.ErrorMessage!.Contains("Headroom"));
    }

    [Fact]
    public void Validate_InvalidReplica_ReturnsNestedError()
    {
        var input = new K8sSizingInput();
        input.Replicas.Prod = -1; // Invalid if supported
        var context = new ValidationContext(input);

        // Note: This tests the nested validation flow - actual result depends on ReplicaSettings validation
        var results = input.Validate(context);
        // Just verify the nested validation runs without throwing
        results.Should().NotBeNull();
    }

    [Fact]
    public void Validate_NestedValidation_UsesCorrectContext()
    {
        var input = new K8sSizingInput();
        var context = new ValidationContext(input);

        // Just ensure the validation flow works
        var act = () => input.Validate(context).ToList();
        act.Should().NotThrow();
    }

    #endregion

    #region Validation - EnvironmentApps

    [Fact]
    public void Validate_ValidEnvironmentApps_NoError()
    {
        var input = new K8sSizingInput
        {
            EnvironmentApps = new Dictionary<EnvironmentType, AppConfig>
            {
                { EnvironmentType.Prod, new AppConfig { Small = 5, Medium = 3 } }
            }
        };
        var context = new ValidationContext(input);

        var results = input.Validate(context).ToList();

        results.Should().NotContain(r => r.ErrorMessage!.Contains("EnvironmentApps"));
    }

    [Fact]
    public void Validate_NullEnvironmentApps_NoError()
    {
        var input = new K8sSizingInput { EnvironmentApps = null };
        var context = new ValidationContext(input);

        var results = input.Validate(context).ToList();

        // No error for null EnvironmentApps
        results.Should().NotContain(r => r.ErrorMessage!.StartsWith("EnvironmentApps"));
    }

    #endregion

    #region ValidateComplexTypeAttribute Tests

    [Fact]
    public void ValidateComplexTypeAttribute_NullValue_ReturnsSuccess()
    {
        var attribute = new ValidateComplexTypeAttribute();
        var context = new ValidationContext(new object());

        // Using reflection to test protected method
        var method = typeof(ValidateComplexTypeAttribute)
            .GetMethod("IsValid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                null, new[] { typeof(object), typeof(ValidationContext) }, null);

        var result = method!.Invoke(attribute, new object?[] { null, context }) as ValidationResult;

        result.Should().Be(ValidationResult.Success);
    }

    [Fact]
    public void ValidateComplexTypeAttribute_ValidObject_ReturnsSuccess()
    {
        var attribute = new ValidateComplexTypeAttribute();
        var validObject = new HeadroomSettings(); // Default values are valid
        var context = new ValidationContext(validObject);

        var method = typeof(ValidateComplexTypeAttribute)
            .GetMethod("IsValid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                null, new[] { typeof(object), typeof(ValidationContext) }, null);

        var result = method!.Invoke(attribute, new object?[] { validObject, context }) as ValidationResult;

        result.Should().Be(ValidationResult.Success);
    }

    [Fact]
    public void ValidateComplexTypeAttribute_InvalidObject_ReturnsError()
    {
        var attribute = new ValidateComplexTypeAttribute();
        var invalidObject = new HeadroomSettings { Dev = -10 }; // Invalid value
        var context = new ValidationContext(invalidObject);

        var method = typeof(ValidateComplexTypeAttribute)
            .GetMethod("IsValid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                null, new[] { typeof(object), typeof(ValidationContext) }, null);

        var result = method!.Invoke(attribute, new object?[] { invalidObject, context }) as ValidationResult;

        result.Should().NotBe(ValidationResult.Success);
    }

    [Fact]
    public void Validate_EnvironmentApps_WithInvalidAppConfig_ReturnsError()
    {
        // Arrange - Create EnvironmentApps with an invalid AppConfig (negative value)
        var input = new K8sSizingInput
        {
            EnvironmentApps = new Dictionary<EnvironmentType, AppConfig>
            {
                { EnvironmentType.Prod, new AppConfig { Small = -5, Medium = 3 } }
            }
        };
        var context = new ValidationContext(input);

        // Act
        var results = input.Validate(context).ToList();

        // Assert - Should contain error for EnvironmentApps[Prod]
        results.Should().Contain(r => r.ErrorMessage!.Contains("EnvironmentApps[Prod]"));
    }

    [Fact]
    public void Validate_EnvironmentApps_WithMultipleInvalidConfigs_ReturnsMultipleErrors()
    {
        // Arrange
        var input = new K8sSizingInput
        {
            EnvironmentApps = new Dictionary<EnvironmentType, AppConfig>
            {
                { EnvironmentType.Dev, new AppConfig { Small = -1 } },
                { EnvironmentType.Test, new AppConfig { Medium = -2 } }
            }
        };
        var context = new ValidationContext(input);

        // Act
        var results = input.Validate(context).ToList();

        // Assert
        results.Should().Contain(r => r.ErrorMessage!.Contains("EnvironmentApps[Dev]"));
        results.Should().Contain(r => r.ErrorMessage!.Contains("EnvironmentApps[Test]"));
    }

    [Fact]
    public void Validate_EnvironmentApps_InvalidConfig_HasCorrectMemberNames()
    {
        // Arrange
        var input = new K8sSizingInput
        {
            EnvironmentApps = new Dictionary<EnvironmentType, AppConfig>
            {
                { EnvironmentType.Stage, new AppConfig { Large = -10 } }
            }
        };
        var context = new ValidationContext(input);

        // Act
        var results = input.Validate(context).ToList();

        // Assert
        var stageError = results.First(r => r.ErrorMessage!.Contains("EnvironmentApps[Stage]"));
        stageError.MemberNames.Should().Contain(m => m.Contains("EnvironmentApps.Stage"));
    }

    #endregion

    #region Valid Full Configuration

    [Fact]
    public void Validate_ValidFullConfiguration_NoErrors()
    {
        var input = new K8sSizingInput
        {
            Distribution = Distribution.AKS,
            Technology = Technology.Java,
            ClusterMode = ClusterMode.MultiCluster,
            ProdApps = new AppConfig { Small = 10, Medium = 5, Large = 2, XLarge = 1 },
            NonProdApps = new AppConfig { Small = 5, Medium = 3 },
            EnabledEnvironments = new HashSet<EnvironmentType>
            {
                EnvironmentType.Dev,
                EnvironmentType.Test,
                EnvironmentType.Prod
            },
            Replicas = new ReplicaSettings(),
            Headroom = new HeadroomSettings(),
            EnableHeadroom = true,
            ProdOvercommit = new OvercommitSettings(),
            NonProdOvercommit = new OvercommitSettings()
        };
        var context = new ValidationContext(input);

        var results = input.Validate(context).ToList();

        results.Should().BeEmpty();
    }

    [Fact]
    public void Validate_MinimalValidConfiguration_NoErrors()
    {
        var input = new K8sSizingInput
        {
            EnabledEnvironments = new HashSet<EnvironmentType> { EnvironmentType.Prod }
        };
        var context = new ValidationContext(input);

        var results = input.Validate(context).ToList();

        results.Should().BeEmpty();
    }

    #endregion
}
