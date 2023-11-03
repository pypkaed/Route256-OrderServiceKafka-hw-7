namespace Ozon.Route256.Kafka.OrderEventConsumer.Domain.Models;

public sealed record SellerPaymentV1
{
    public long SellerId { get; init; }
    public decimal Rub { get; init; }
    public decimal Kzt { get; init; }
}
