using System.Threading;
using System.Threading.Tasks;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.Models;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.ValueObjects;

namespace Ozon.Route256.Kafka.OrderEventConsumer.Domain;

public interface IItemRepository
{
    Task Add(ItemsAccountingV1 model, CancellationToken cancellationToken);

    Task Update(ItemsAccountingV1 model, CancellationToken cancellationToken);
    Task<ItemsAccountingV1?> Get(ItemId itemId, CancellationToken cancellationToken);
}
