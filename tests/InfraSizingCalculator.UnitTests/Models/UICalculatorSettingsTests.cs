using FluentAssertions;
using InfraSizingCalculator.Models;
using Xunit;

namespace InfraSizingCalculator.UnitTests.Models;

/// <summary>
/// Tests for UICalculatorSettings model that holds user-configurable
/// calculator settings including infrastructure thresholds and tier specifications.
/// </summary>
public class UICalculatorSettingsTests
{
    #region Default Value Tests

    [Fact]
    public void Constructor_SetsInfrastructureDefaults()
    {
        // Act
        var settings = new UICalculatorSettings();

        // Assert
        settings.LargeDeploymentThreshold.Should().Be(50);
        settings.LargeClusterThreshold.Should().Be(100);
        settings.AppsPerInfraNode.Should().Be(25);
        settings.MinProdInfraLarge.Should().Be(5);
        settings.MinProdInfraSmall.Should().Be(3);
        settings.MaxInfraNodes.Should().Be(10);
    }

    [Fact]
    public void Constructor_SetsProdKubernetesDefaults()
    {
        // Act
        var settings = new UICalculatorSettings();

        // Assert
        settings.ProdCpuOvercommit.Should().Be(1.0);
        settings.ProdMemoryOvercommit.Should().Be(1.0);
        settings.ProdResourceBuffer.Should().Be(30);
    }

    [Fact]
    public void Constructor_SetsNonProdKubernetesDefaults()
    {
        // Act
        var settings = new UICalculatorSettings();

        // Assert
        settings.NonProdCpuOvercommit.Should().Be(1.0);
        settings.NonProdMemoryOvercommit.Should().Be(1.0);
        settings.NonProdResourceBuffer.Should().Be(15);
    }

    [Fact]
    public void Constructor_SetsSystemDefaults()
    {
        // Act
        var settings = new UICalculatorSettings();

        // Assert
        settings.NodeSystemReserve.Should().Be(15);
        settings.MinWorkerNodes.Should().Be(3);
    }

    [Fact]
    public void Constructor_SetsHeadroomDefaults()
    {
        // Act
        var settings = new UICalculatorSettings();

        // Assert
        settings.HeadroomDev.Should().Be(33);
        settings.HeadroomTest.Should().Be(33);
        settings.HeadroomStage.Should().Be(0);
        settings.HeadroomProd.Should().Be(37.5);
        settings.HeadroomDR.Should().Be(37.5);
    }

    [Fact]
    public void Constructor_SetsReplicaDefaults()
    {
        // Act
        var settings = new UICalculatorSettings();

        // Assert
        settings.ReplicasDev.Should().Be(1);
        settings.ReplicasTest.Should().Be(1);
        settings.ReplicasStage.Should().Be(2);
        settings.ReplicasProd.Should().Be(3);
        settings.ReplicasDR.Should().Be(3);
    }

    [Fact]
    public void Constructor_SetsProdNodeSpecDefaults()
    {
        // Act
        var settings = new UICalculatorSettings();

        // Assert - Control Plane
        settings.ProdMasterCpu.Should().Be(8);
        settings.ProdMasterRam.Should().Be(32);
        settings.ProdMasterDisk.Should().Be(200);

        // Assert - Infrastructure
        settings.ProdInfraCpu.Should().Be(8);
        settings.ProdInfraRam.Should().Be(32);
        settings.ProdInfraDisk.Should().Be(500);

        // Assert - Worker
        settings.ProdWorkerCpu.Should().Be(16);
        settings.ProdWorkerRam.Should().Be(64);
        settings.ProdWorkerDisk.Should().Be(200);
    }

    [Fact]
    public void Constructor_SetsNonProdNodeSpecDefaults()
    {
        // Act
        var settings = new UICalculatorSettings();

        // Assert - Control Plane
        settings.NonProdMasterCpu.Should().Be(8);
        settings.NonProdMasterRam.Should().Be(32);
        settings.NonProdMasterDisk.Should().Be(100);

        // Assert - Infrastructure
        settings.NonProdInfraCpu.Should().Be(8);
        settings.NonProdInfraRam.Should().Be(32);
        settings.NonProdInfraDisk.Should().Be(200);

        // Assert - Worker
        settings.NonProdWorkerCpu.Should().Be(8);
        settings.NonProdWorkerRam.Should().Be(32);
        settings.NonProdWorkerDisk.Should().Be(100);
    }

    #endregion

    #region Technology Tier Default Tests

    [Fact]
    public void Constructor_SetsDotNetTierDefaults()
    {
        // Act
        var settings = new UICalculatorSettings();

        // Assert
        settings.DotNetSmallCpu.Should().Be(0.25);
        settings.DotNetSmallRam.Should().Be(0.5);
        settings.DotNetMediumCpu.Should().Be(0.5);
        settings.DotNetMediumRam.Should().Be(1);
        settings.DotNetLargeCpu.Should().Be(1);
        settings.DotNetLargeRam.Should().Be(2);
        settings.DotNetXLargeCpu.Should().Be(2);
        settings.DotNetXLargeRam.Should().Be(4);
    }

    [Fact]
    public void Constructor_SetsJavaTierDefaults()
    {
        // Act
        var settings = new UICalculatorSettings();

        // Assert - Java requires more resources than .NET
        settings.JavaSmallCpu.Should().Be(0.5);
        settings.JavaSmallRam.Should().Be(1);
        settings.JavaMediumCpu.Should().Be(1);
        settings.JavaMediumRam.Should().Be(2);
        settings.JavaLargeCpu.Should().Be(2);
        settings.JavaLargeRam.Should().Be(4);
        settings.JavaXLargeCpu.Should().Be(4);
        settings.JavaXLargeRam.Should().Be(8);
    }

    [Fact]
    public void Constructor_SetsNodeJsTierDefaults()
    {
        // Act
        var settings = new UICalculatorSettings();

        // Assert - Node.js Small RAM is 1GB (V8 heap management)
        settings.NodeJsSmallCpu.Should().Be(0.25);
        settings.NodeJsSmallRam.Should().Be(1);
        settings.NodeJsMediumCpu.Should().Be(0.5);
        settings.NodeJsMediumRam.Should().Be(1);
        settings.NodeJsLargeCpu.Should().Be(1);
        settings.NodeJsLargeRam.Should().Be(2);
        settings.NodeJsXLargeCpu.Should().Be(2);
        settings.NodeJsXLargeRam.Should().Be(4);
    }

    [Fact]
    public void Constructor_SetsPythonTierDefaults()
    {
        // Act
        var settings = new UICalculatorSettings();

        // Assert - Python Small RAM is 1GB (WSGI/Django overhead)
        settings.PythonSmallCpu.Should().Be(0.25);
        settings.PythonSmallRam.Should().Be(1);
        settings.PythonMediumCpu.Should().Be(0.5);
        settings.PythonMediumRam.Should().Be(1);
        settings.PythonLargeCpu.Should().Be(1);
        settings.PythonLargeRam.Should().Be(2);
        settings.PythonXLargeCpu.Should().Be(2);
        settings.PythonXLargeRam.Should().Be(4);
    }

    [Fact]
    public void Constructor_SetsGoTierDefaults()
    {
        // Act
        var settings = new UICalculatorSettings();

        // Assert - Go is the most resource-efficient
        settings.GoSmallCpu.Should().Be(0.125);
        settings.GoSmallRam.Should().Be(0.25);
        settings.GoMediumCpu.Should().Be(0.25);
        settings.GoMediumRam.Should().Be(0.5);
        settings.GoLargeCpu.Should().Be(0.5);
        settings.GoLargeRam.Should().Be(1);
        settings.GoXLargeCpu.Should().Be(1);
        settings.GoXLargeRam.Should().Be(2);
    }

    [Fact]
    public void Constructor_SetsMendixTierDefaults()
    {
        // Act
        var settings = new UICalculatorSettings();

        // Assert - Mendix similar to Java
        settings.MendixSmallCpu.Should().Be(0.5);
        settings.MendixSmallRam.Should().Be(1);
        settings.MendixMediumCpu.Should().Be(1);
        settings.MendixMediumRam.Should().Be(2);
        settings.MendixLargeCpu.Should().Be(2);
        settings.MendixLargeRam.Should().Be(4);
        settings.MendixXLargeCpu.Should().Be(4);
        settings.MendixXLargeRam.Should().Be(8);
    }

    [Fact]
    public void Constructor_SetsMendixOperatorDefaults()
    {
        // Act
        var settings = new UICalculatorSettings();

        // Assert
        settings.MendixOperatorReplicas.Should().Be(2);
    }

    #endregion

    #region Property Modification Tests

    [Fact]
    public void Properties_CanBeModified()
    {
        // Arrange
        var settings = new UICalculatorSettings();

        // Act
        settings.LargeDeploymentThreshold = 100;
        settings.ProdCpuOvercommit = 2.5;
        settings.HeadroomProd = 50.0;
        settings.DotNetSmallCpu = 0.5;

        // Assert
        settings.LargeDeploymentThreshold.Should().Be(100);
        settings.ProdCpuOvercommit.Should().Be(2.5);
        settings.HeadroomProd.Should().Be(50.0);
        settings.DotNetSmallCpu.Should().Be(0.5);
    }

    [Fact]
    public void MultipleInstances_AreIndependent()
    {
        // Arrange
        var settings1 = new UICalculatorSettings();
        var settings2 = new UICalculatorSettings();

        // Act
        settings1.ProdCpuOvercommit = 5.0;

        // Assert
        settings1.ProdCpuOvercommit.Should().Be(5.0);
        settings2.ProdCpuOvercommit.Should().Be(1.0);
    }

    #endregion

    #region Business Rule Verification Tests

    [Fact]
    public void GoHasLowestResourceRequirements()
    {
        // Arrange
        var settings = new UICalculatorSettings();

        // Assert - Go should have the lowest Small tier resources
        settings.GoSmallCpu.Should().BeLessThan(settings.DotNetSmallCpu);
        settings.GoSmallCpu.Should().BeLessThan(settings.JavaSmallCpu);
        settings.GoSmallCpu.Should().BeLessThan(settings.NodeJsSmallCpu);
        settings.GoSmallCpu.Should().BeLessThan(settings.PythonSmallCpu);
        settings.GoSmallCpu.Should().BeLessThan(settings.MendixSmallCpu);
    }

    [Fact]
    public void JavaHasHigherResourcesThanDotNet()
    {
        // Arrange
        var settings = new UICalculatorSettings();

        // Assert - Java (JVM) typically needs more resources
        settings.JavaSmallCpu.Should().BeGreaterThan(settings.DotNetSmallCpu);
        settings.JavaSmallRam.Should().BeGreaterThan(settings.DotNetSmallRam);
    }

    [Fact]
    public void ProdReplicasHigherThanNonProd()
    {
        // Arrange
        var settings = new UICalculatorSettings();

        // Assert - Production should have higher availability
        settings.ReplicasProd.Should().BeGreaterThan(settings.ReplicasDev);
        settings.ReplicasProd.Should().BeGreaterThan(settings.ReplicasTest);
        settings.ReplicasProd.Should().BeGreaterThanOrEqualTo(settings.ReplicasStage);
    }

    [Fact]
    public void ProdWorkerResourcesHigherThanNonProd()
    {
        // Arrange
        var settings = new UICalculatorSettings();

        // Assert - Production workers have more resources
        settings.ProdWorkerCpu.Should().BeGreaterThan(settings.NonProdWorkerCpu);
        settings.ProdWorkerRam.Should().BeGreaterThan(settings.NonProdWorkerRam);
        settings.ProdWorkerDisk.Should().BeGreaterThan(settings.NonProdWorkerDisk);
    }

    #endregion
}
