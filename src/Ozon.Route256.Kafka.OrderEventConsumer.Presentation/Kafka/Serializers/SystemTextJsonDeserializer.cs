using System;
using System.Text;
using System.Text.Json;
using Confluent.Kafka;

namespace Ozon.Route256.Kafka.OrderEventConsumer.Presentation.Kafka.Serializers;

internal sealed class SystemTextJsonDeserializer<T> : IDeserializer<T>
{
    private readonly JsonSerializerOptions? _jsonSerializerOptions;

    public SystemTextJsonDeserializer(JsonSerializerOptions? jsonSerializerOptions = null)
    {
        _jsonSerializerOptions = jsonSerializerOptions;
    }

    public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
    {
        var zhopa = Encoding.UTF8.GetString(data);
        return isNull
            ? throw new ArgumentNullException($"Null data encountered deserializing {typeof(T).Name} value.")
            : JsonSerializer.Deserialize<T>(data, _jsonSerializerOptions)!;
    }
}
