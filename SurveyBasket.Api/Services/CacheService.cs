
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace SurveyBasket.Api.Services;

public class CacheService(IDistributedCache distributedCache) : ICacheService
{
    private readonly IDistributedCache _distributedCache = distributedCache;

    public async Task<T?> GetAsync<T>(string cacheKey, CancellationToken cancellationToken) where T : class
    {
        var result = await _distributedCache.GetAsync(cacheKey, cancellationToken);
        return result is null
            ? null
            : JsonSerializer.Deserialize<T>(result);
    }
    public async Task SetAsync<T>(string cacheKey, T value, CancellationToken cancellationToken) where T : class
    {
        await _distributedCache.SetStringAsync(cacheKey, JsonSerializer.Serialize(value), cancellationToken);
    }
    public async Task RemoveAsync(string cacheKey, CancellationToken cancellationToken)
    {
        await _distributedCache.RemoveAsync(cacheKey, cancellationToken);
    }

}
