using System;
using FluentMigrator;

using Ozon.Route256.Kafka.OrderEventConsumer.Infrastructure.Common;

namespace Ozon.Route256.Postgres.Persistence.Migrations;

[Migration(1, "Initial migration")]
public sealed class Initial : SqlMigration
{
    protected override string GetUpSql(IServiceProvider services) => @"
CREATE TABLE IF NOT EXISTS items_accounting (
      item_id       bigint
    , reserved      bigint
    , sold          bigint
    , canceled      bigint
    , modified_at   timestamp with time zone
);";
}
