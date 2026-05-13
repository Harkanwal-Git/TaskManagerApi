
using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;

namespace TaskManagerApi.Repository;

public class CacheInMemoryRepository : ICacheRepository
{
    private readonly IMemoryCache _memoryCache;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphoreLocks = new();

    private readonly ConcurrentDictionary<string, Task<object?>> _inflight = new();
    private readonly ConcurrentDictionary<string, Lazy<Task<object?>>> _inflightLazy = new();
    public CacheInMemoryRepository(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    #region  without-lazy-Get
    public async Task<T?> Get_JustTask<T>(string key, Func<CancellationToken, Task<T>> factory, CancellationToken ct)
    {
        if (_memoryCache.TryGetValue(key, out var result) && result is T cachedValue) return cachedValue;

        var task = _inflight.GetOrAdd(key, _ => loadFromDB());

        var value = await task;
        return (T?)value;

        async Task<object?> loadFromDB()
        {
            try
            {
                // Double-check after winning race
                if (_memoryCache.TryGetValue(key, out var secondResult) &&
                    secondResult is T secondCached)
                {
                    return secondCached;
                }

                var created = await factory(ct);
                return created;
            }
            finally
            {
                _inflight.TryRemove(key, out _);
            }

        }
    }
    #endregion
    #region LazyGetSET
    public async Task<T?> GetLazyWay<T>(string key, Func<CancellationToken, Task<T>> factory)
    {
        if (_memoryCache.TryGetValue(key, out var result) && result is T cachedValue) return cachedValue;

        var task = _inflightLazy.GetOrAdd(key, _ => new Lazy<Task<object?>>(() => loadFromDB()));

        var value = await task.Value;
        return (T?)value;

        async Task<object?> loadFromDB()
        {
            try
            {
                // Double-check after winning race
                if (_memoryCache.TryGetValue(key, out var secondResult) &&
                    secondResult is T secondCached)
                {
                    return secondCached;
                }

                var created = await factory(CancellationToken.None);
                if (created != null) _memoryCache.Set(key, created, new MemoryCacheEntryOptions()
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24),

                });
                return created;
            }
            finally
            {
                _inflightLazy.TryRemove(key, out _);
            }

        }
    }
    #endregion

    public Task<T?> Get<T>(string key, CancellationToken ct)
    {
        _memoryCache.TryGetValue(key, out var value);
        return Task.FromResult((T?)value);
    }
    public Task Set<T>(string key, T TValue, TimeSpan expiration, CancellationToken ct)
    {
        _memoryCache.Set(key, TValue, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration
        });
        return Task.CompletedTask;
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