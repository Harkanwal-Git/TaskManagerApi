using TaskManagerApi.Middleware;
using TaskManagerApi.Model;

namespace TaskManagerApi.Repository;


public interface IIdempotencyRecordRepository
{
    Task<IdempotencyRecord?> GetByKey(Guid IdempotencyKey, CancellationToken ct);

    Task Add(IdempotencyRecord idempotencyRecord, CancellationToken ct);
}