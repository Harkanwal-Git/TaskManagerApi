
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;


namespace TaskManagerApi.Repository;

public class CacheRedisRepository : ICacheRepository
{
    private readonly IDistributedCache _redisCache;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphoreLocks = new();
    public CacheRedisRepository(IDistributedCache distributedCache)
    {
        _redisCache = distributedCache;
    }

    public async Task InvalidateCacheKey(string key, CancellationToken ct)
    {
        await _redisCache.RemoveAsync(key, ct);
        _semaphoreLocks.TryRemove(key, out _);
    }

    public async Task<T> GetOrSet<T>(string key, Func<CancellationToken, Task<T>> factory, TimeSpan expiration, CancellationToken ct)
    {
        var byteFromCache = await _redisCache.GetAsync(key, ct);

        if (byteFromCache != null) return JsonSerializer.Deserialize<T>(byteFromCache);

        var semaphoreSlim = _semaphoreLocks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));

        await semaphoreSlim.WaitAsync(ct);
        try
        {

            byteFromCache = await _redisCache.GetAsync(key, ct);

            if (byteFromCache != null) return JsonSerializer.Deserialize<T>(byteFromCache);

            var cachedValue = await factory(ct);

            byte[] value = JsonSerializer.SerializeToUtf8Bytes(cachedValue);

            await _redisCache.SetAsync(key, value, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration,
                SlidingExpiration = TimeSpan.FromHours(1)
            });
            return cachedValue;
        }
        finally
        {
            semaphoreSlim.Release();
        }

    }
}