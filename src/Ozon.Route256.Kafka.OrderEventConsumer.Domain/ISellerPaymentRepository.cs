using System.Threading;
using System.Threading.Tasks;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.Models;

namespace Ozon.Route256.Kafka.OrderEventConsumer.Domain;

public interface ISellerPaymentRepository
{
    Task Add(SellerPaymentV1 model, CancellationToken cancellationToken);

    Task Update(SellerPaymentV1 model, CancellationToken cancellationToken);
    Task<SellerPaymentV1?> Get(long sellerId, CancellationToken cancellationToken);
}
