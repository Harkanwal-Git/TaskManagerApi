namespace TaskManagerApi.Repository;

public interface ICacheRepository
{
    Task InvalidateCacheKey(string key, CancellationToken ct);
    // T? Get<T>(string key);
    Task<T?> Get<T>(string key, CancellationToken ct);
    // Task<T?> GetLazyWay<T>(string key, Func<CancellationToken, Task<T>> factory);
    Task Set<T>(string key, T value, TimeSpan expiration, CancellationToken ct);

    Task<T> GetOrSet<T>(string key, Func<CancellationToken, Task<T>> factory, TimeSpan expiration, CancellationToken ct);
}