using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ozon.Route256.Kafka.OrderEventConsumer.Presentation.Contracts;

namespace Ozon.Route256.Kafka.OrderEventConsumer.Presentation.Kafka.Serializers;

public class OrderStatusConverter : JsonConverter<OrderEvent.OrderStatus>
{
    public override OrderEvent.OrderStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException();
        }

        string? enumString = reader.GetString();
        if (Enum.TryParse<OrderEvent.OrderStatus>(enumString, true, out OrderEvent.OrderStatus status))
        {
            return status;
        }

        throw new JsonException($"Couldn't to convert '{enumString}' to {typeof(OrderEvent.OrderStatus)}.");
    }

    public override void Write(Utf8JsonWriter writer, OrderEvent.OrderStatus value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
