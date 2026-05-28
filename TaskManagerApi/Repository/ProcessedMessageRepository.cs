using Microsoft.EntityFrameworkCore;
using TaskManagerApi.Data;
using TaskManagerApi.Model;

namespace TaskManagerApi.Repository;

public class ProcessedMessageRepository : IProcessedMessageRepository
{
    private readonly AppDbContext _dbContext;
    public ProcessedMessageRepository(AppDbContext appDbContext)
    {
        _dbContext = appDbContext;
    }
    public Task AddProcessedMessage(ProcessedMessage processedMessage, CancellationToken ct)
    {
        _dbContext.ProcessedMessages.Add(processedMessage);
        return Task.CompletedTask;

    }

    public async Task<bool> ExistsAsync(Guid taskId, string ConsumerGroupId, CancellationToken ct)
    {
        return await _dbContext.ProcessedMessages.AnyAsync(pm => pm.TaskId == taskId && pm.ConsumerGroup == ConsumerGroupId, ct);
    }
}