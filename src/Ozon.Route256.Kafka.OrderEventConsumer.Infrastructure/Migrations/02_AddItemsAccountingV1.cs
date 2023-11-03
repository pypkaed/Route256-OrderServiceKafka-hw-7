using System;
using FluentMigrator;
using Ozon.Route256.Kafka.OrderEventConsumer.Infrastructure.Common;

namespace Ozon.Route256.Postgres.Persistence.Migrations;

[Migration(2, "Add ItemsAccountingV1 migration")]
public class AddItemsAccountingV1 : SqlMigration
{
    protected override string GetUpSql(IServiceProvider services) => @"
DO $$
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'item_id') THEN
            CREATE TYPE item_id AS (
                value              bigint
            );
        END IF;

        IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'items_accounting_v1') THEN
            CREATE TYPE items_accounting_v1 as (
                item_id             item_id,
                reserved            bigint,
                sold                bigint,
                canceled            bigint,
                deleted_at          timestamp with time zone
            );
        END IF;
    END
$$;";
}
