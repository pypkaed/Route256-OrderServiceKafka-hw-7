using System;
using System.Data;
using Dapper;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.ValueObjects;

namespace Ozon.Route256.Kafka.OrderEventConsumer.Presentation.Kafka.Serializers;

public class ItemIdTypeHandler : SqlMapper.TypeHandler<ItemId>
{
    public override void SetValue(IDbDataParameter parameter, ItemId value)
    {
        parameter.Value = value.Value;
    }

    public override ItemId Parse(object value)
    {
        if (value is long actualValue)
        {
            return new ItemId(actualValue);
        }

        throw new Exception("Couldn't parse value to ItemId");
    }
}
