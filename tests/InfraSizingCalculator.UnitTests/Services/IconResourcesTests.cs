using FluentAssertions;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Services;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Services;

/// <summary>
/// Tests for IconResources static helper class
/// </summary>
public class IconResourcesTests
{
    #region Environment Icon Tests

    [Theory]
    [InlineData(EnvironmentType.Dev, "env-icon dev")]
    [InlineData(EnvironmentType.Test, "env-icon test")]
    [InlineData(EnvironmentType.Stage, "env-icon stage")]
    [InlineData(EnvironmentType.Prod, "env-icon prod")]
    [InlineData(EnvironmentType.DR, "env-icon dr")]
    public void GetEnvIconClass_ReturnsCorrectClass(EnvironmentType env, string expected)
    {
        // Act
        var result = IconResources.GetEnvIconClass(env);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(EnvironmentType.Dev, "Development")]
    [InlineData(EnvironmentType.Test, "Testing")]
    [InlineData(EnvironmentType.Stage, "Staging")]
    [InlineData(EnvironmentType.Prod, "Production")]
    [InlineData(EnvironmentType.DR, "Disaster Recovery")]
    public void GetEnvDisplayName_ReturnsCorrectName(EnvironmentType env, string expected)
    {
        // Act
        var result = IconResources.GetEnvDisplayName(env);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(EnvironmentType.Dev, "DEV")]
    [InlineData(EnvironmentType.Test, "TEST")]
    [InlineData(EnvironmentType.Stage, "STG")]
    [InlineData(EnvironmentType.Prod, "PROD")]
    [InlineData(EnvironmentType.DR, "DR")]
    public void GetEnvShortLabel_ReturnsCorrectLabel(EnvironmentType env, string expected)
    {
        // Act
        var result = IconResources.GetEnvShortLabel(env);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region Server Role Icon Tests

    [Theory]
    [InlineData(ServerRole.Web, "role-icon web")]
    [InlineData(ServerRole.App, "role-icon app")]
    [InlineData(ServerRole.Database, "role-icon database")]
    [InlineData(ServerRole.Cache, "role-icon cache")]
    [InlineData(ServerRole.MessageQueue, "role-icon mq")]
    [InlineData(ServerRole.Search, "role-icon search")]
    [InlineData(ServerRole.Storage, "role-icon storage")]
    [InlineData(ServerRole.Monitoring, "role-icon monitoring")]
    [InlineData(ServerRole.Bastion, "role-icon bastion")]
    public void GetRoleIconClass_ReturnsCorrectClass(ServerRole role, string expected)
    {
        // Act
        var result = IconResources.GetRoleIconClass(role);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("web", "role-icon web")]
    [InlineData("WEB", "role-icon web")]
    [InlineData("app", "role-icon app")]
    [InlineData("db", "role-icon database")]
    [InlineData("database", "role-icon database")]
    [InlineData("cache", "role-icon cache")]
    [InlineData("mq", "role-icon mq")]
    [InlineData("queue", "role-icon mq")]
    [InlineData("search", "role-icon search")]
    [InlineData("storage", "role-icon storage")]
    [InlineData("monitoring", "role-icon monitoring")]
    [InlineData("mon", "role-icon monitoring")]
    [InlineData("bastion", "role-icon bastion")]
    [InlineData(null, "role-icon")]
    [InlineData("unknown", "role-icon")]
    public void GetRoleIconClassFromString_ReturnsCorrectClass(string? icon, string expected)
    {
        // Act
        var result = IconResources.GetRoleIconClassFromString(icon);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(ServerRole.Web, "Web Server")]
    [InlineData(ServerRole.App, "Application Server")]
    [InlineData(ServerRole.Database, "Database Server")]
    [InlineData(ServerRole.Cache, "Cache Server")]
    [InlineData(ServerRole.MessageQueue, "Message Queue")]
    [InlineData(ServerRole.Search, "Search Server")]
    [InlineData(ServerRole.Storage, "Storage Server")]
    [InlineData(ServerRole.Monitoring, "Monitoring")]
    [InlineData(ServerRole.Bastion, "Bastion Host")]
    public void GetRoleDisplayName_ReturnsCorrectName(ServerRole role, string expected)
    {
        // Act
        var result = IconResources.GetRoleDisplayName(role);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region K8s Node Icon Tests

    [Theory]
    [InlineData("control", "node-icon control")]
    [InlineData("controlplane", "node-icon control")]
    [InlineData("cp", "node-icon control")]
    [InlineData("infra", "node-icon infra")]
    [InlineData("infrastructure", "node-icon infra")]
    [InlineData("worker", "node-icon worker")]
    [InlineData("w", "node-icon worker")]
    [InlineData("CONTROL", "node-icon control")]
    [InlineData(null, "node-icon")]
    [InlineData("unknown", "node-icon")]
    public void GetNodeIconClass_ReturnsCorrectClass(string? nodeType, string expected)
    {
        // Act
        var result = IconResources.GetNodeIconClass(nodeType);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("control", "Control Plane")]
    [InlineData("controlplane", "Control Plane")]
    [InlineData("cp", "Control Plane")]
    [InlineData("infra", "Infrastructure")]
    [InlineData("infrastructure", "Infrastructure")]
    [InlineData("worker", "Worker")]
    [InlineData("w", "Worker")]
    [InlineData(null, "Unknown")]
    [InlineData("custom", "custom")]
    public void GetNodeDisplayName_ReturnsCorrectName(string? nodeType, string expected)
    {
        // Act
        var result = IconResources.GetNodeDisplayName(nodeType);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region Utility Icon Tests

    [Theory]
    [InlineData("users", "util-icon users")]
    [InlineData("deployment", "util-icon deployment")]
    [InlineData("cost", "util-icon cost")]
    [InlineData(null, "util-icon")]
    [InlineData("unknown", "util-icon")]
    public void GetUtilIconClass_ReturnsCorrectClass(string? utilType, string expected)
    {
        // Act
        var result = IconResources.GetUtilIconClass(utilType);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region Size Class Tests

    [Theory]
    [InlineData("sm", "icon-sm")]
    [InlineData("small", "icon-sm")]
    [InlineData("md", "icon-md")]
    [InlineData("medium", "icon-md")]
    [InlineData("lg", "icon-lg")]
    [InlineData("large", "icon-lg")]
    [InlineData(null, "")]
    [InlineData("unknown", "")]
    public void GetSizeClass_ReturnsCorrectClass(string? size, string expected)
    {
        // Act
        var result = IconResources.GetSizeClass(size);

        // Assert
        result.Should().Be(expected);
    }

    #endregion
}
