
using System.Text.Json;
using Confluent.Kafka;
using TaskManagerApi.EventPublisher;
using TaskManagerApi.Producers;
using TaskManagerApi.Producers.Events;
using TaskManagerApi.Repository;

namespace TaskManagerApi.OutboxWorker;

public class OutboxWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ITaskProducer _taskProducer;
    private readonly ILogger<OutboxWorker> _logger;
    private readonly int MaxRetries;

    public OutboxWorker(IServiceScopeFactory serviceScopeFactory, ITaskProducer taskProducer, IConfiguration configuration, ILogger<OutboxWorker> logger)
    {
        _scopeFactory = serviceScopeFactory;
        _taskProducer = taskProducer;
        _logger = logger;
        MaxRetries = configuration.GetValue<int>("OutboxWorker:MaxRetries");

    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {


        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();


            var unPublishedRecords = await outboxRepository.FetchUnprocessedOutboxMessageAsync(stoppingToken);
            _logger.LogInformation("Fetched {unPublishedRecords} Unpublished Records for retry at {time}", unPublishedRecords.Count(), DateTime.UtcNow);
            if (unPublishedRecords.Any())
            {

                foreach (var message in unPublishedRecords)
                {

                    try
                    {
                        var publisher = scope.ServiceProvider.GetRequiredKeyedService<IEventPublisher>(message.EventType);
                        await publisher.PublishAsync(message.Payload, stoppingToken);
                        _logger.LogInformation("Successfully re-publsihed Unpublished Record: {unPublishedRecord} at retry: {retryCount} at {time}", message.Id, message.RetryCount, DateTime.UtcNow);
                        message.PublishedAt = DateTime.UtcNow;
                        message.IsPublished = true;

                        var updatedRows = await outboxRepository.UpdateOutboxMessageAsync(message, stoppingToken);
                    }
                    catch (KafkaException)
                    {
                        message.IsStalled = message.RetryCount > MaxRetries;
                        _logger.LogWarning("Unsuccessful re-publsih attempt : {retryCount} for Record: {unPublishedRecord} at {time}", message.RetryCount, message.Id, DateTime.UtcNow);
                        var updatedRows = await outboxRepository.UpdateOutboxMessageAsync(message, stoppingToken);
                    }
                    catch (OperationCanceledException) when (!stoppingToken.IsCancellationRequested)
                    {
                        // timeout - treat same as Kafka transient failure
                        message.IsStalled = message.RetryCount > MaxRetries;
                        _logger.LogWarning("Kafka publish timed out for message {Id}, retry {RetryCount}", message.Id, message.RetryCount);
                        await outboxRepository.UpdateOutboxMessageAsync(message, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        // permanent - stall immediately, don't waste retries
                        message.IsStalled = true;
                        _logger.LogError(ex, "Permanent failure processing outbox message {Id}", message.Id);
                        await outboxRepository.UpdateOutboxMessageAsync(message, stoppingToken);
                    }
                }
            }
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
