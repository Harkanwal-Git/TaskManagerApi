using System.Transactions;
using Dapper;
using TaskManagerApi.Data;
using TaskManagerApi.Events;
using TaskManagerApi.Model;

namespace TaskManagerApi.Repository;

public class OutboxRepository : IOutboxRepository
{
    private readonly IDataConnectionFactory _dataConnection;
    public OutboxRepository(IDataConnectionFactory dataConnection)
    {
        _dataConnection = dataConnection; ;
    }

    public async Task<IEnumerable<OutboxMessage>> FetchUnprocessedOutboxMessageAsync(CancellationToken ct)
    {
        var sql = @"UPDATE OutboxMessages SET ProcessedAt=GETUTCDATE(), RetryCount=RetryCount+1 
OUTPUT INSERTED.*  WHERE ID IN (Select top 10 Id from OutboxMessages WITH (UPDLOCK, READPAST,ROWLOCK) where IsPublished=0 AND IsStalled=0 AND (ProcessedAt IS NULL OR ProcessedAt<@threshold))";

        using var connection = _dataConnection.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            var result = await connection.QueryAsync<OutboxMessage>(new CommandDefinition(sql, new { threshold = DateTime.UtcNow.AddSeconds(-60) }, transaction, cancellationToken: ct));

            transaction.Commit();
            return result;
        }
        catch (Exception)
        {
            transaction.Rollback();
            throw;
        }

    }

    public async Task<int> UpdateOutboxMessageAsync(OutboxMessage outboxMessage, CancellationToken ct)
    {
        var sql = @"Update OutboxMessages SET IsPublished=@IsPublished, PublishedAt=@PublishedAt, RetryCount=@RetryCount, IsStalled=@IsStalled where Id=@Id;";

        using var connection = _dataConnection.CreateConnection();
        var result = await connection.ExecuteAsync(new CommandDefinition(sql, new { outboxMessage.IsPublished, outboxMessage.PublishedAt, outboxMessage.RetryCount, IsStalled = outboxMessage.IsStalled, outboxMessage.Id }, cancellationToken: ct));

        return result;
    }
}