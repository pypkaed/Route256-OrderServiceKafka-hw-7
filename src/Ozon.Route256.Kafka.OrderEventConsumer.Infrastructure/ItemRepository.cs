using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Npgsql;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.Models;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.ValueObjects;

namespace Ozon.Route256.Kafka.OrderEventConsumer.Infrastructure;

public sealed class ItemRepository : IItemRepository
{
    private readonly string _connectionString;

    public ItemRepository(string connectionString) => _connectionString = connectionString;

    public async Task Add(ItemsAccountingV1 model, CancellationToken cancellationToken)
    {
        const string sqlQuery = @"
insert into items_accounting (item_id, reserved, sold, canceled, modified_at)
values (@ItemId, @Reserved, @Sold, @Canceled, @ModifiedAt);
";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await connection.QueryAsync(
            new CommandDefinition(
                sqlQuery,
                new
                {
                    @ItemId = model.ItemId,
                    @Reserved = model.Reserved,
                    @Sold = model.Sold,
                    @Canceled = model.Canceled,
                    @ModifiedAt = model.ModifiedAt
                },
                cancellationToken: cancellationToken));

    }

    public async Task Update(ItemsAccountingV1 model, CancellationToken cancellationToken)
    {
        const string sqlQuery = @"
update items_accounting
   set reserved = @Reserved
     , sold = @Sold
     , canceled = @Canceled
     , modified_at = @ModifiedAt
 where item_id = @ItemId
";
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await connection.QueryAsync(
            new CommandDefinition(
                sqlQuery,
                new
                {
                    @ItemId = model.ItemId,
                    @Reserved = model.Reserved,
                    @Sold = model.Sold,
                    @Canceled = model.Canceled,
                    @ModifiedAt = model.ModifiedAt,
                },
                cancellationToken: cancellationToken));
    }

    public async Task<ItemsAccountingV1?> Get(ItemId itemId, CancellationToken cancellationToken)
    {
        const string sqlQuery = @"
select item_id, reserved, sold, canceled, modified_at
from items_accounting
where item_id = @ItemId;
";
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        return await connection.QueryFirstOrDefaultAsync<ItemsAccountingV1>(
            new CommandDefinition(
                sqlQuery,
                new
                {
                    ItemId = itemId
                },
                cancellationToken: cancellationToken));
    }
}
