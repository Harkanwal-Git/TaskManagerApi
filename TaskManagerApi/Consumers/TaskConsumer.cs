
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Serilog.Context;
using TaskManagerApi.Data;
using TaskManagerApi.Model;
using TaskManagerApi.Producers.Events;
using TaskManagerApi.Repository;

namespace TaskManagerApi.Consumers;

public class TaskConsumer : BackgroundService
{
    private readonly ILogger<TaskConsumer> _logger;
    private readonly ConsumerConfig _consumerConfig;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ICacheRepository _cacheRepository;

    private readonly ProducerConfig _producerConfig;


    private readonly int _concurrentConsumption;

    public TaskConsumer(ILogger<TaskConsumer> logger, IConfiguration configuration, IServiceScopeFactory serviceScopeFactory, ICacheRepository cacheRepository)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _consumerConfig = new ConsumerConfig()
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"],
            GroupId = "Task-Notification-Group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        _producerConfig = new ProducerConfig()
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"],

        };
        _cacheRepository = cacheRepository;
        // _processedMessageRepo = processedMessageRepository;

        _concurrentConsumption = configuration.GetValue<int>("Kafka:concurrencyPool");
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using (LogContext.PushProperty("CorrelationId", Guid.NewGuid()))
            try
            {
                var tasks = new List<Task>(_concurrentConsumption);
                for (int i = 0; i < _concurrentConsumption; i++)
                {
                    tasks.Add(Task.Run(async () =>
                     {

                         using var consumer = new ConsumerBuilder<string, string>(_consumerConfig).Build();
                         consumer.Subscribe("task.created");
                         var dltProducer = new ProducerBuilder<string, string>(_producerConfig).Build();
                         try
                         {
                             while (!stoppingToken.IsCancellationRequested)
                             {
                                 var result = consumer.Consume(stoppingToken);

                                 using var scope = _serviceScopeFactory.CreateScope();
                                 var processedMessageRepo = scope.ServiceProvider.GetRequiredService<IProcessedMessageRepository>();
                                 var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                                 try
                                 {
                                     var message = JsonSerializer.Deserialize<TaskCreatedEvent>(result.Message.Value);
                                     var taskId = message!.TaskId;
                                     var alreadyProcessedEvent = await _cacheRepository.Get<bool?>($"TaskCreatedEvent:{taskId}", stoppingToken) ?? false;
                                     if (alreadyProcessedEvent) continue;
                                     else
                                     {
                                         alreadyProcessedEvent = await processedMessageRepo.ExistsAsync(taskId, _consumerConfig.GroupId, stoppingToken);
                                         if (alreadyProcessedEvent)
                                         {
                                             await _cacheRepository.Set<bool>($"TaskCreatedEvent:{taskId}", true, TimeSpan.FromHours(4), stoppingToken);
                                             continue;
                                         }
                                     }
                                     int retryCount = 0;
                                     int maxRetry = 3;
                                     while (retryCount < maxRetry)
                                     {
                                         try
                                         {
                                             await ProcessMessageAsync(processedMessageRepo, dbContext, consumer, result, taskId, stoppingToken);
                                             break;
                                         }
                                         catch (DbUpdateException ex)
                                         {
                                             _logger.LogInformation("Error occured while processing message from Kafka Topic: {topic}, message: {message}, RetryCount: {retryCount}, Error: {stackTrace}", "task.created", result.Message.Value, retryCount, ex.StackTrace);
                                             retryCount++;
                                             if (retryCount >= maxRetry)
                                             {
                                                 await dltProducer.ProduceAsync("DLT.task.created", message: result.Message, stoppingToken);
                                                 consumer.Commit(result);
                                                 break;
                                             }

                                             var delay = TimeSpan.FromSeconds(Math.Pow(2, retryCount)) + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1000));
                                             await Task.Delay(delay, stoppingToken);
                                             // await transaction.RollbackAsync(stoppingToken);
                                         }
                                     }
                                 }
                                 catch (JsonException jex)
                                 {
                                     _logger.LogError("Kafka message deserialization exception");
                                     await dltProducer.ProduceAsync("DLT.task.created", message: result.Message, stoppingToken);
                                     consumer.Commit(result);
                                 }
                                 catch (ConsumeException ex)
                                 {
                                     _logger.LogError("Error while reading message from Kafka:Error: {error} StackTrace: {stackTracke}", ex.Message, ex.StackTrace);
                                 }
                             }
                         }
                         catch (OperationCanceledException)
                         {
                             _logger.LogInformation("Consumer shutting down gracefully");
                         }
                     }));
                }
                await Task.WhenAll(tasks);

            }
            catch (Exception ex)
            {
                _logger.LogError("Error in Kafka Task Consumer:Error: {error} StackTrace: {stackTracke}", ex.Message, ex.StackTrace);
            }
    }

    private async Task ProcessMessageAsync(IProcessedMessageRepository processedMessageRepo, AppDbContext dbContext, IConsumer<string, string> consumer, ConsumeResult<string, string> result, Guid taskId, CancellationToken stoppingToken)
    {

        await using var transaction = await dbContext.Database.BeginTransactionAsync(stoppingToken);
        try
        {
            _logger.LogInformation("Received message from Kafka Topic: {topic}, message: {message}", "task.created", result.Message.Value);
            await processedMessageRepo.AddProcessedMessage(new ProcessedMessage() { TaskId = taskId, ConsumerGroup = _consumerConfig.GroupId, ProcessedAt = DateTime.UtcNow }, stoppingToken);
            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync(stoppingToken);
            consumer.Commit(result);
            await _cacheRepository.Set($"TaskCreatedEvent:{taskId}", true, TimeSpan.FromHours(4), stoppingToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number == 2627)
        {
            _logger.LogInformation("Received duplciate message from Kafka Topic: {topic}, message: {message}", "task.created", result.Message.Value);
            // await transaction.RollbackAsync(stoppingToken);

        }
        catch (KafkaException)
        {
            _logger.LogWarning("Kafka exception will commiting offset, Kafka Topic: {topic}, message: {message}", "task.created", result.Message.Value);

        }

    }


}