using Microsoft.EntityFrameworkCore;
using TaskManagerApi.Data;
using TaskManagerApi.Model;

namespace TaskManagerApi.Repository;

public class IdempotencyRecordRepository : IIdempotencyRecordRepository
{
    private readonly AppDbContext _dBContext;
    public IdempotencyRecordRepository(AppDbContext appDbContext)
    {
        _dBContext = appDbContext;
    }

    public async Task Add(IdempotencyRecord idempotencyRecord, CancellationToken ct)
    {
        _dBContext.IdempotencyRecords.Add(idempotencyRecord);

        await _dBContext.SaveChangesAsync(ct);

    }

    public async Task<IdempotencyRecord?> GetByKey(Guid IdempotencyKey, CancellationToken ct)
    {
        var result = await _dBContext.IdempotencyRecords.SingleOrDefaultAsync(i => i.IdempotencyKey == IdempotencyKey, ct);

        return result;
    }
}