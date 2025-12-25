using FluentAssertions;
using InfraSizingCalculator.Models;
using InfraSizingCalculator.Models.Enums;
using InfraSizingCalculator.Services;
using Microsoft.JSInterop;
using NSubstitute;
using Xunit;

namespace InfraSizingCalculator.UnitTests;

public class ConfigurationSharingServiceTests
{
    private readonly IJSRuntime _mockJsRuntime;
    private readonly ConfigurationSharingService _service;

    public ConfigurationSharingServiceTests()
    {
        _mockJsRuntime = Substitute.For<IJSRuntime>();
        _service = new ConfigurationSharingService(_mockJsRuntime);
    }

    #region GenerateShareUrlAsync K8s Tests

    [Fact]
    public async Task GenerateShareUrlAsync_K8sInput_GeneratesValidUrl()
    {
        // Arrange
        var input = CreateK8sSizingInput();
        _mockJsRuntime.InvokeAsync<string>("getBaseUrl", Arg.Any<object[]>())
            .Returns(new ValueTask<string>("https://example.com"));

        // Act
        var url = await _service.GenerateShareUrlAsync(input);

        // Assert
        url.Should().StartWith("https://example.com?config=");
        url.Should().Contain("config=");
    }

    [Fact]
    public async Task GenerateShareUrlAsync_K8sInput_GeneratesUrlSafeBase64()
    {
        // Arrange
        var input = CreateK8sSizingInput();
        _mockJsRuntime.InvokeAsync<string>("getBaseUrl", Arg.Any<object[]>())
            .Returns(new ValueTask<string>("https://example.com"));

        // Act
        var url = await _service.GenerateShareUrlAsync(input);

        // Assert
        // URL-safe base64 should not contain + or /
        var configParam = url.Split("config=")[1];
        configParam.Should().NotContain("+");
        configParam.Should().NotContain("/");
    }

    #endregion

    #region GenerateShareUrlAsync VM Tests

    [Fact]
    public async Task GenerateShareUrlAsync_VMInput_GeneratesValidUrl()
    {
        // Arrange
        var input = CreateVMSizingInput();
        _mockJsRuntime.InvokeAsync<string>("getBaseUrl", Arg.Any<object[]>())
            .Returns(new ValueTask<string>("https://example.com"));

        // Act
        var url = await _service.GenerateShareUrlAsync(input);

        // Assert
        url.Should().StartWith("https://example.com?config=");
    }

    #endregion

    #region GenerateConfigParamAsync Tests

    [Fact]
    public async Task GenerateConfigParamAsync_K8sInput_GeneratesCompressedConfig()
    {
        // Arrange
        var input = CreateK8sSizingInput();

        // Act
        var configParam = await _service.GenerateConfigParamAsync(input);

        // Assert
        configParam.Should().NotBeNullOrEmpty();
        // Should be significantly shorter than raw JSON
        configParam.Length.Should().BeGreaterThan(10);
    }

    [Fact]
    public async Task GenerateConfigParamAsync_VMInput_GeneratesCompressedConfig()
    {
        // Arrange
        var input = CreateVMSizingInput();

        // Act
        var configParam = await _service.GenerateConfigParamAsync(input);

        // Assert
        configParam.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GenerateConfigParamAsync_DifferentInputs_GenerateDifferentParams()
    {
        // Arrange
        var k8sInput = CreateK8sSizingInput();
        var vmInput = CreateVMSizingInput();

        // Act
        var k8sParam = await _service.GenerateConfigParamAsync(k8sInput);
        var vmParam = await _service.GenerateConfigParamAsync(vmInput);

        // Assert
        k8sParam.Should().NotBe(vmParam);
    }

    #endregion

    #region ParseFromUrlAsync Tests

    [Fact]
    public async Task ParseFromUrlAsync_NoConfigParam_ReturnsNull()
    {
        // Arrange
        _mockJsRuntime.InvokeAsync<string?>("getUrlParameter", Arg.Any<object[]>())
            .Returns(new ValueTask<string?>((string?)null));

        // Act
        var config = await _service.ParseFromUrlAsync();

        // Assert
        config.Should().BeNull();
    }

    [Fact]
    public async Task ParseFromUrlAsync_EmptyConfigParam_ReturnsNull()
    {
        // Arrange
        _mockJsRuntime.InvokeAsync<string?>("getUrlParameter", Arg.Any<object[]>())
            .Returns(new ValueTask<string?>(""));

        // Act
        var config = await _service.ParseFromUrlAsync();

        // Assert
        config.Should().BeNull();
    }

    [Fact]
    public async Task ParseFromUrlAsync_InvalidConfigParam_ReturnsNull()
    {
        // Arrange
        _mockJsRuntime.InvokeAsync<string?>("getUrlParameter", Arg.Any<object[]>())
            .Returns(new ValueTask<string?>("invalidbase64data!@#$"));

        // Act
        var config = await _service.ParseFromUrlAsync();

        // Assert
        config.Should().BeNull();
    }

    [Fact]
    public async Task ParseFromUrlAsync_JSInteropFails_ReturnsNull()
    {
        // Arrange
        _mockJsRuntime.InvokeAsync<string?>("getUrlParameter", Arg.Any<object[]>())
            .Returns<string?>(_ => throw new InvalidOperationException("JS Interop not available"));

        // Act
        var config = await _service.ParseFromUrlAsync();

        // Assert
        config.Should().BeNull();
    }

    #endregion

    #region CopyShareUrlAsync Tests

    [Fact]
    public async Task CopyShareUrlAsync_Success_ReturnsTrue()
    {
        // Arrange
        _mockJsRuntime.InvokeAsync<bool>("copyToClipboard", Arg.Any<object[]>())
            .Returns(new ValueTask<bool>(true));

        // Act
        var result = await _service.CopyShareUrlAsync("https://example.com?config=abc");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CopyShareUrlAsync_Failure_ReturnsFalse()
    {
        // Arrange
        _mockJsRuntime.InvokeAsync<bool>("copyToClipboard", Arg.Any<object[]>())
            .Returns(new ValueTask<bool>(false));

        // Act
        var result = await _service.CopyShareUrlAsync("https://example.com?config=abc");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region ClearUrlParameterAsync Tests

    [Fact]
    public async Task ClearUrlParameterAsync_CallsJSInterop()
    {
        // Act
        await _service.ClearUrlParameterAsync();

        // Assert
        await _mockJsRuntime.Received(1)
            .InvokeVoidAsync("setUrlParameter", Arg.Is<object[]>(args =>
                args[0].ToString() == "config" && args[1] == null));
    }

    #endregion

    #region UpdateUrlWithConfigAsync Tests

    [Fact]
    public async Task UpdateUrlWithConfigAsync_CallsJSInteropWithEncodedConfig()
    {
        // Arrange
        var input = CreateK8sSizingInput();

        // Act
        await _service.UpdateUrlWithConfigAsync(input);

        // Assert
        await _mockJsRuntime.Received(1)
            .InvokeVoidAsync("setUrlParameter", Arg.Is<object[]>(args =>
                args[0].ToString() == "config" &&
                args[1] != null &&
                args[1].ToString()!.Length > 0));
    }

    #endregion

    #region ShareableConfig Tests

    [Fact]
    public void ShareableConfig_DefaultType_IsK8s()
    {
        // Arrange & Act
        var config = new ShareableConfig();

        // Assert
        config.Type.Should().Be("k8s");
    }

    [Fact]
    public void ShareableConfig_DefaultVersion_IsOne()
    {
        // Arrange & Act
        var config = new ShareableConfig();

        // Assert
        config.Version.Should().Be(1);
    }

    [Fact]
    public void ShareableConfig_CanHoldK8sInput()
    {
        // Arrange
        var input = CreateK8sSizingInput();

        // Act
        var config = new ShareableConfig
        {
            Type = "k8s",
            K8sInput = input
        };

        // Assert
        config.K8sInput.Should().NotBeNull();
        config.K8sInput.Should().Be(input);
    }

    [Fact]
    public void ShareableConfig_CanHoldVMInput()
    {
        // Arrange
        var input = CreateVMSizingInput();

        // Act
        var config = new ShareableConfig
        {
            Type = "vm",
            VMInput = input
        };

        // Assert
        config.VMInput.Should().NotBeNull();
        config.VMInput.Should().Be(input);
    }

    #endregion

    #region Roundtrip Tests

    [Fact]
    public async Task RoundTrip_K8sInput_PreservesData()
    {
        // Arrange
        var originalInput = CreateK8sSizingInput();
        originalInput.ProdApps.Small = 10;
        originalInput.ProdApps.Medium = 15;
        originalInput.ProdApps.Large = 12;
        originalInput.ProdApps.XLarge = 5;

        // Generate config param
        var configParam = await _service.GenerateConfigParamAsync(originalInput);

        // Mock the URL parameter return
        _mockJsRuntime.InvokeAsync<string?>("getUrlParameter", Arg.Any<object[]>())
            .Returns(new ValueTask<string?>(configParam));

        // Act
        var parsedConfig = await _service.ParseFromUrlAsync();

        // Assert
        parsedConfig.Should().NotBeNull();
        parsedConfig!.Type.Should().Be("k8s");
        parsedConfig.K8sInput.Should().NotBeNull();
        parsedConfig.K8sInput!.ProdApps.TotalApps.Should().Be(42);
    }

    [Fact]
    public async Task RoundTrip_VMInput_PreservesData()
    {
        // Arrange
        var originalInput = CreateVMSizingInput();
        originalInput.Technology = Technology.Java;

        // Generate config param
        var configParam = await _service.GenerateConfigParamAsync(originalInput);

        // Mock the URL parameter return
        _mockJsRuntime.InvokeAsync<string?>("getUrlParameter", Arg.Any<object[]>())
            .Returns(new ValueTask<string?>(configParam));

        // Act
        var parsedConfig = await _service.ParseFromUrlAsync();

        // Assert
        parsedConfig.Should().NotBeNull();
        parsedConfig!.Type.Should().Be("vm");
        parsedConfig.VMInput.Should().NotBeNull();
        parsedConfig.VMInput!.Technology.Should().Be(Technology.Java);
    }

    #endregion

    #region Compression Tests

    [Fact]
    public async Task GenerateConfigParamAsync_CompressesData()
    {
        // Arrange
        var input = CreateLargeK8sSizingInput();

        // Act
        var configParam = await _service.GenerateConfigParamAsync(input);

        // Assert
        // Compressed should be reasonably sized for URL sharing
        configParam.Length.Should().BeLessThan(2000); // Reasonable URL length limit
    }

    #endregion

    #region Helper Methods

    private K8sSizingInput CreateK8sSizingInput()
    {
        return new K8sSizingInput
        {
            Distribution = Distribution.Kubernetes,
            ProdApps = new AppConfig { Small = 10 },
            NonProdApps = new AppConfig { Small = 5 }
        };
    }

    private K8sSizingInput CreateLargeK8sSizingInput()
    {
        return new K8sSizingInput
        {
            Distribution = Distribution.OpenShift,
            ProdApps = new AppConfig
            {
                Small = 30,
                Medium = 40,
                Large = 20,
                XLarge = 10
            },
            NonProdApps = new AppConfig
            {
                Small = 30,
                Medium = 40,
                Large = 20,
                XLarge = 10
            },
            EnableHeadroom = true,
            Headroom = new HeadroomSettings
            {
                Prod = 40,
                DR = 30,
                Stage = 25,
                Test = 20,
                Dev = 15
            }
        };
    }

    private VMSizingInput CreateVMSizingInput()
    {
        return new VMSizingInput
        {
            Technology = Technology.DotNet,
            EnabledEnvironments = new HashSet<EnvironmentType>
            {
                EnvironmentType.Prod,
                EnvironmentType.Dev,
                EnvironmentType.Test
            }
        };
    }

    #endregion
}
