using Microsoft.Data.SqlClient.Internal;
using TaskManagerApi.Model;
using TaskManagerApi.Events;

namespace TaskManagerApi.Repository;


public interface IOutboxRepository
{
    Task<IEnumerable<OutboxMessage>> FetchUnprocessedOutboxMessageAsync(CancellationToken ct);

    Task<int> UpdateOutboxMessageAsync(OutboxMessage outboxMessage, CancellationToken ct);
}