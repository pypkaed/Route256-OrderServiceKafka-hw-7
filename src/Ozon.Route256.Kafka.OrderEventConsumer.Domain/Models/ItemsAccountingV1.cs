using System;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.ValueObjects;

namespace Ozon.Route256.Kafka.OrderEventConsumer.Domain.Models;

public sealed record ItemsAccountingV1
{
    public ItemId ItemId { get; init; }
    public long Reserved { get; init; }
    public long Sold { get; init; }
    public long Canceled { get; init; }
    public DateTime ModifiedAt { get; init; }
}
