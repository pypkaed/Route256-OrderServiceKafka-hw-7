using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ozon.Route256.Kafka.OrderEventConsumer.Infrastructure.Kafka;
using Ozon.Route256.Kafka.OrderEventConsumer.Presentation.Contracts;
using Ozon.Route256.Kafka.OrderEventConsumer.Presentation.Kafka.Handlers;
using Ozon.Route256.Kafka.OrderEventConsumer.Presentation.Kafka.Serializers;

namespace Ozon.Route256.Kafka.OrderEventConsumer.Presentation.Kafka;

public class KafkaBackgroundService : BackgroundService
{
    private readonly KafkaAsyncConsumer<Ignore, OrderEvent> _consumer;
    private readonly ILogger<KafkaBackgroundService> _logger;

    public KafkaBackgroundService(IServiceProvider serviceProvider, ILogger<KafkaBackgroundService> logger)
    {
        // TODO: IOptions
        // TODO: KafkaServiceExtensions: services.AddKafkaHandler<TKey, TValue, THandler<TKey, TValue>>(serializers, topic, groupId);
        _logger = logger;
        var handler = serviceProvider.GetRequiredService<ItemHandler>();
        _consumer = new KafkaAsyncConsumer<Ignore, OrderEvent>(
            handler,
            "kafka:9092",
            "group_id",
            "order_events",
            keyDeserializer: null,
            valueDeserializer: new SystemTextJsonDeserializer<OrderEvent>(),
            serviceProvider.GetRequiredService<ILogger<KafkaAsyncConsumer<Ignore, OrderEvent>>>());
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _consumer.Dispose();

        return Task.CompletedTask;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await _consumer.Consume(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occured");
        }
    }
}
