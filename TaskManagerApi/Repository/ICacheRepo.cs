namespace TaskManagerApi.Repository;

public interface ICacheRepository
{
    Task InvalidateCacheKey(string key, CancellationToken ct);
    // T? Get<T>(string key);
    // void Set<T>(string key, T value, TimeSpan timeSpan);

    Task<T> GetOrSet<T>(string key, Func<CancellationToken, Task<T>> factory, TimeSpan expiration, CancellationToken ct);
}