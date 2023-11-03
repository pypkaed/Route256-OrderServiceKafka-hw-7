using System;
using FluentMigrator;
using Ozon.Route256.Kafka.OrderEventConsumer.Infrastructure.Common;

namespace Ozon.Route256.Postgres.Persistence.Migrations;

[Migration(3, "Add seller payment migration")]
public class AddSellerPaymentV1 : SqlMigration
{
    protected override string GetUpSql(IServiceProvider services) => @"
DO $$
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'seller_payment_v1') THEN
            CREATE TYPE seller_payment_v1 as (
                seller_id             bigint,
                rub                   decimal(18,8),
                kzt                   decimal(18,8)
            );
        END IF;
    END
$$;
";
}
