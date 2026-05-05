
using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;

namespace TaskManagerApi.Repository;

public class CacheInMemoryRepository : ICacheRepository
{
    private readonly IMemoryCache _memoryCache;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphoreLocks = new();
    public CacheInMemoryRepository(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public Task InvalidateCacheKey(string key, CancellationToken ct)
    {
        _memoryCache.Remove(key);
        _semaphoreLocks.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    public async Task<T> GetOrSet<T>(string key, Func<CancellationToken, Task<T>> factory, TimeSpan expiration, CancellationToken ct)
    {
        if (_memoryCache.TryGetValue(key, out var obj) && obj is T cachedValue) return cachedValue;

        var semaphoreSlim = _semaphoreLocks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));

        await semaphoreSlim.WaitAsync(ct);
        try
        {
            if (_memoryCache.TryGetValue(key, out obj) && obj is T value) return value;
            value = await factory(ct);

            _memoryCache.Set(key, value, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration,
                SlidingExpiration = TimeSpan.FromHours(1)
            });
            return value;
        }
        finally
        {
            semaphoreSlim.Release();
        }

    }
}