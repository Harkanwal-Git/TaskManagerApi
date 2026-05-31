using System.Text.Json;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using taskmanager.events;
using TaskManagerApi.Producers.Events;

namespace TaskManagerApi.Producers;

public class TaskProducer : ITaskProducer, IDisposable
{
    private readonly ILogger<TaskProducer> _logger;

    private readonly IProducer<string, TaskCreatedEvent> _producer;
    public TaskProducer(IConfiguration configuration, ILogger<TaskProducer> logger, ISchemaRegistryClient schemaRegistryClient)
    {
        _logger = logger;
        var config = new ProducerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"]
        };

        _producer = new ProducerBuilder<string, TaskCreatedEvent>(config)
        .SetValueSerializer(new AvroSerializer<TaskCreatedEvent>(schemaRegistryClient))
        .Build();
    }



    public async Task PublishTaskCreatedAsync(TaskCreatedEvent taskCreatedEvent, CancellationToken ct)
    {
        var message = new Message<string, TaskCreatedEvent>
        {
            Key = taskCreatedEvent.taskId.ToString(),
            Value = taskCreatedEvent
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