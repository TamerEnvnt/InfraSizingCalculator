using System.IO.Compression;
using System.Text;
using System.Text.Json;
using InfraSizingCalculator.Models;
using Microsoft.JSInterop;

namespace InfraSizingCalculator.Services;

/// <summary>
/// Service for sharing configurations via URL.
/// </summary>
public class ConfigurationSharingService
{
    private readonly IJSRuntime _jsRuntime;
    private const string ConfigParamName = "config";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public ConfigurationSharingService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// Generate a shareable URL for a K8s configuration.
    /// </summary>
    public async Task<string> GenerateShareUrlAsync(K8sSizingInput input)
    {
        var shareData = new ShareableConfig
        {
            Type = "k8s",
            K8sInput = input
        };

        return await GenerateShareUrlAsync(shareData);
    }

    /// <summary>
    /// Generate a shareable URL for a VM configuration.
    /// </summary>
    public async Task<string> GenerateShareUrlAsync(VMSizingInput input)
    {
        var shareData = new ShareableConfig
        {
            Type = "vm",
            VMInput = input
        };

        return await GenerateShareUrlAsync(shareData);
    }

    /// <summary>
    /// Generate just the config parameter value for a K8s configuration.
    /// </summary>
    public Task<string> GenerateConfigParamAsync(K8sSizingInput input)
    {
        var shareData = new ShareableConfig
        {
            Type = "k8s",
            K8sInput = input
        };

        return Task.FromResult(GenerateConfigParam(shareData));
    }

    /// <summary>
    /// Generate just the config parameter value for a VM configuration.
    /// </summary>
    public Task<string> GenerateConfigParamAsync(VMSizingInput input)
    {
        var shareData = new ShareableConfig
        {
            Type = "vm",
            VMInput = input
        };

        return Task.FromResult(GenerateConfigParam(shareData));
    }

    private string GenerateConfigParam(ShareableConfig config)
    {
        var json = JsonSerializer.Serialize(config, JsonOptions);
        var compressed = CompressString(json);
        return Base64UrlEncode(compressed);
    }

    private async Task<string> GenerateShareUrlAsync(ShareableConfig config)
    {
        var json = JsonSerializer.Serialize(config, JsonOptions);
        var compressed = CompressString(json);
        var encoded = Base64UrlEncode(compressed);

        var baseUrl = await _jsRuntime.InvokeAsync<string>("getBaseUrl");
        return $"{baseUrl}?{ConfigParamName}={encoded}";
    }

    /// <summary>
    /// Parse a shared configuration from URL parameter.
    /// </summary>
    public async Task<ShareableConfig?> ParseFromUrlAsync()
    {
        try
        {
            var encoded = await _jsRuntime.InvokeAsync<string?>("getUrlParameter", ConfigParamName);
            if (string.IsNullOrEmpty(encoded))
                return null;

            var compressed = Base64UrlDecode(encoded);
            var json = DecompressString(compressed);

            return JsonSerializer.Deserialize<ShareableConfig>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Copy share URL to clipboard.
    /// </summary>
    public async Task<bool> CopyShareUrlAsync(string url)
    {
        return await _jsRuntime.InvokeAsync<bool>("copyToClipboard", url);
    }

    /// <summary>
    /// Clear the config parameter from URL.
    /// </summary>
    public async Task ClearUrlParameterAsync()
    {
        await _jsRuntime.InvokeVoidAsync("setUrlParameter", ConfigParamName, null);
    }

    /// <summary>
    /// Update URL with current config (without full page reload).
    /// </summary>
    public async Task UpdateUrlWithConfigAsync(K8sSizingInput input)
    {
        var shareData = new ShareableConfig { Type = "k8s", K8sInput = input };
        var json = JsonSerializer.Serialize(shareData, JsonOptions);
        var compressed = CompressString(json);
        var encoded = Base64UrlEncode(compressed);

        await _jsRuntime.InvokeVoidAsync("setUrlParameter", ConfigParamName, encoded);
    }

    // Compression helpers to keep URL shorter
    private static byte[] CompressString(string text)
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        using var output = new MemoryStream();
        using (var gzip = new GZipStream(output, CompressionLevel.Optimal))
        {
            gzip.Write(bytes, 0, bytes.Length);
        }
        return output.ToArray();
    }

    private static string DecompressString(byte[] compressed)
    {
        using var input = new MemoryStream(compressed);
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();
        gzip.CopyTo(output);
        return Encoding.UTF8.GetString(output.ToArray());
    }

    // URL-safe Base64 encoding
    private static string Base64UrlEncode(byte[] input)
    {
        return Convert.ToBase64String(input)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    private static byte[] Base64UrlDecode(string input)
    {
        var base64 = input
            .Replace('-', '+')
            .Replace('_', '/');

        // Add padding if needed
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }

        return Convert.FromBase64String(base64);
    }
}

/// <summary>
/// Container for shareable configuration data.
/// </summary>
public class ShareableConfig
{
    /// <summary>
    /// Configuration type: "k8s" or "vm"
    /// </summary>
    public string Type { get; set; } = "k8s";

    /// <summary>
    /// K8s sizing input (if type is k8s)
    /// </summary>
    public K8sSizingInput? K8sInput { get; set; }

    /// <summary>
    /// VM sizing input (if type is vm)
    /// </summary>
    public VMSizingInput? VMInput { get; set; }

    /// <summary>
    /// Version for future compatibility
    /// </summary>
    public int Version { get; set; } = 1;
}
