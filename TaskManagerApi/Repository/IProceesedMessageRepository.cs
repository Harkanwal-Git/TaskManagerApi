using TaskManagerApi.Model;

namespace TaskManagerApi.Repository;

public interface IProcessedMessageRepository
{
    Task<bool> ExistsAsync(Guid TaskId, string ConsumerGroupId, CancellationToken ct);
    Task AddProcessedMessage(ProcessedMessage processedMessage, CancellationToken ct);
}