using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Npgsql;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.Models;

namespace Ozon.Route256.Kafka.OrderEventConsumer.Infrastructure;

public class SellerPaymentRepository : ISellerPaymentRepository
{
    private readonly string _connectionString;

    public SellerPaymentRepository(string connectionString) => _connectionString = connectionString;

    public async Task Add(SellerPaymentV1 model, CancellationToken cancellationToken)
    {
        const string sqlQuery = @"
insert into seller_payment (seller_id, rub, kzt)
values (@SellerId, @Rub, @Kzt);
";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await connection.QueryAsync(
            new CommandDefinition(
                sqlQuery,
                new
                {
                    @SellerId = model.SellerId,
                    @Rub = model.Rub,
                    @Kzt = model.Kzt
                },
                cancellationToken: cancellationToken));
    }

    public async Task Update(SellerPaymentV1 model, CancellationToken cancellationToken) {
        const string sqlQuery = @"
update seller_payment
   set rub = @Rub
     , kzt = @Kzt
 where seller_id = @SellerId
";
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await connection.QueryAsync(
            new CommandDefinition(
                sqlQuery,
                new
                {
                    @SellerId = model.SellerId,
                    @Rub = model.Rub,
                    @Kzt = model.Kzt
                },
                cancellationToken: cancellationToken));
    }

    public async Task<SellerPaymentV1?> Get(long sellerId, CancellationToken cancellationToken) {
        const string sqlQuery = @"
select seller_id, rub, kzt
from seller_payment
where seller_id = @SellerId
";
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        return await connection.QueryFirstOrDefaultAsync<SellerPaymentV1>(
            new CommandDefinition(
                sqlQuery,
                new
                {
                    @SellerId = sellerId
                },
                cancellationToken: cancellationToken));
    }
}
