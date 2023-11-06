using System;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.ValueObjects;

namespace Ozon.Route256.Kafka.OrderEventConsumer.Domain.Models;

public sealed record ItemsAccountingV1
{
    public required ItemId ItemId { get; init; }
    public required long Reserved { get; init; }
    public required long Sold { get; init; }
    public required long Canceled { get; init; }
    public required DateTime ModifiedAt { get; init; }
}
