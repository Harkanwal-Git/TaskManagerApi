using System.Text.Json;
using Confluent.Kafka;
using TaskManagerApi.Producers.Events;

namespace TaskManagerApi.Producers;

public class TaskProducer : ITaskProducer, IDisposable
{
    private readonly ILogger<TaskProducer> _logger;

    private readonly IProducer<string, string> _producer;
    public TaskProducer(IConfiguration configuration, ILogger<TaskProducer> logger)
    {
        _logger = logger;
        var config = new ProducerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"]
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }



    public async Task PublishTaskCreatedAsync(TaskCreatedEvent taskCreatedEvent, CancellationToken ct)
    {
        var message = new Message<string, string>
        {
            Key = taskCreatedEvent.TaskId.ToString(),
            Value = JsonSerializer.Serialize(taskCreatedEvent)
        };

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(5));
        await _producer.ProduceAsync("task.created", message, cts.Token);
    }
    public void Dispose()
    {
        _producer.Dispose();
    }

}