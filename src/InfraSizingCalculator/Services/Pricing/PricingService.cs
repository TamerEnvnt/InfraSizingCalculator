using InfraSizingCalculator.Models.Pricing;
using InfraSizingCalculator.Services.Interfaces;
using Microsoft.JSInterop;
using System.Text.Json;

namespace InfraSizingCalculator.Services.Pricing;

/// <summary>
/// Main pricing service that coordinates pricing providers and caching
/// </summary>
public class PricingService : IPricingService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly Dictionary<string, PricingModel> _pricingCache = new();
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromHours(24);
    private OnPremPricing _onPremPricing = new();

    private const string CacheKeyPrefix = "pricing-cache-";
    private const string OnPremCacheKey = "pricing-onprem";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public PricingService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <inheritdoc />
    public async Task<PricingModel> GetPricingAsync(CloudProvider provider, string region, PricingType pricingType = PricingType.OnDemand)
    {
        var cacheKey = GetCacheKey(provider, region, pricingType);

        // Check memory cache first
        if (_pricingCache.TryGetValue(cacheKey, out var cached) && !IsCacheStale(cached.LastUpdated))
        {
            return cached;
        }

        // Try to load from localStorage
        var storedPricing = await LoadFromStorageAsync(cacheKey);
        if (storedPricing != null && !IsCacheStale(storedPricing.LastUpdated))
        {
            _pricingCache[cacheKey] = storedPricing;
            return storedPricing;
        }

        // Try to fetch live pricing (if we implement API calls later)
        var livePricing = await TryFetchLivePricingAsync(provider, region, pricingType);
        if (livePricing != null)
        {
            livePricing.IsLive = true;
            await SaveToStorageAsync(cacheKey, livePricing);
            _pricingCache[cacheKey] = livePricing;
            return livePricing;
        }

        // Fall back to default pricing
        var defaultPricing = GetDefaultPricing(provider, region);
        _pricingCache[cacheKey] = defaultPricing;
        return defaultPricing;
    }

    /// <inheritdoc />
    public List<RegionInfo> GetRegions(CloudProvider provider)
    {
        return CloudRegions.GetRegions(provider);
    }

    /// <inheritdoc />
    public PricingModel GetDefaultPricing(CloudProvider provider)
    {
        return GetDefaultPricing(provider, null);
    }

    private PricingModel GetDefaultPricing(CloudProvider provider, string? region)
    {
        return DefaultPricingData.GetDefaultPricing(provider, region);
    }

    /// <inheritdoc />
    public OnPremPricing GetOnPremPricing()
    {
        return _onPremPricing;
    }

    /// <inheritdoc />
    public void UpdateOnPremPricing(OnPremPricing pricing)
    {
        _onPremPricing = pricing;
        // Could save to localStorage if needed
    }

    /// <inheritdoc />
    public bool IsPricingStale(CloudProvider provider, string region)
    {
        var cacheKey = GetCacheKey(provider, region, PricingType.OnDemand);
        if (_pricingCache.TryGetValue(cacheKey, out var cached))
        {
            return IsCacheStale(cached.LastUpdated);
        }
        return true;
    }

    /// <inheritdoc />
    public DateTime? GetLastPricingUpdate(CloudProvider provider, string region)
    {
        var cacheKey = GetCacheKey(provider, region, PricingType.OnDemand);
        if (_pricingCache.TryGetValue(cacheKey, out var cached))
        {
            return cached.LastUpdated;
        }
        return null;
    }

    /// <inheritdoc />
    public async Task<PricingModel?> RefreshPricingAsync(CloudProvider provider, string region)
    {
        var pricing = await TryFetchLivePricingAsync(provider, region, PricingType.OnDemand);
        if (pricing != null)
        {
            var cacheKey = GetCacheKey(provider, region, PricingType.OnDemand);
            pricing.IsLive = true;
            pricing.LastUpdated = DateTime.UtcNow;
            await SaveToStorageAsync(cacheKey, pricing);
            _pricingCache[cacheKey] = pricing;
        }
        return pricing;
    }

    private string GetCacheKey(CloudProvider provider, string region, PricingType pricingType)
    {
        return $"{CacheKeyPrefix}{provider}-{region}-{pricingType}".ToLowerInvariant();
    }

    private bool IsCacheStale(DateTime lastUpdated)
    {
        return DateTime.UtcNow - lastUpdated > _cacheExpiry;
    }

    private async Task<PricingModel?> LoadFromStorageAsync(string key)
    {
        try
        {
            var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
            if (!string.IsNullOrEmpty(json))
            {
                return JsonSerializer.Deserialize<PricingModel>(json, JsonOptions);
            }
        }
        catch
        {
            // Storage not available or parse error
        }
        return null;
    }

    private async Task SaveToStorageAsync(string key, PricingModel pricing)
    {
        try
        {
            var json = JsonSerializer.Serialize(pricing, JsonOptions);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, json);
        }
        catch
        {
            // Storage not available
        }
    }

    /// <summary>
    /// Attempt to fetch live pricing from cloud APIs
    /// Currently returns null - implementation can be added later for real API integration
    /// </summary>
    private async Task<PricingModel?> TryFetchLivePricingAsync(CloudProvider provider, string region, PricingType pricingType)
    {
        // Future implementation could call:
        // - AWS Price List API
        // - Azure Retail Prices API
        // - GCP Cloud Billing API
        // - Oracle OCI Cost Management API

        // For now, return null to use default pricing
        await Task.CompletedTask;
        return null;
    }
}
