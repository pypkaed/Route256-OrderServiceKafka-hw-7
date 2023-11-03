using Dapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Npgsql.NameTranslation;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.Models;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.ValueObjects;
using Ozon.Route256.Kafka.OrderEventConsumer.Infrastructure;
using Ozon.Route256.Kafka.OrderEventConsumer.Infrastructure.Common;
using Ozon.Route256.Kafka.OrderEventConsumer.Presentation.Kafka;
using Ozon.Route256.Kafka.OrderEventConsumer.Presentation.Kafka.Handlers;
using Ozon.Route256.Kafka.OrderEventConsumer.Presentation.Kafka.Serializers;

namespace Ozon.Route256.Kafka.OrderEventConsumer.Presentation;

public sealed class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration) => _configuration = configuration;

    public void ConfigureServices(IServiceCollection services)
    {
        services
            .AddLogging();

        var connectionString = _configuration["ConnectionString"]!;

        services
            .AddFluentMigrator(
                connectionString,
                typeof(SqlMigration).Assembly);

        MapCompositeTypes();

        services.AddSingleton<IItemRepository, ItemRepository>(_ => new ItemRepository(connectionString));
        services.AddSingleton<ISellerPaymentRepository, SellerPaymentRepository>(_ => new SellerPaymentRepository(connectionString));
        services.AddSingleton<ItemHandler>();
        services.AddHostedService<KafkaBackgroundService>();
    }

    private static void MapCompositeTypes()
    {
        INpgsqlNameTranslator translator = new NpgsqlSnakeCaseNameTranslator();

#pragma warning disable CS0618 // Type or member is obsolete
        var mapper = NpgsqlConnection.GlobalTypeMapper;
#pragma warning restore CS0618 // Type or member is obsolete
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

        mapper.MapComposite<ItemId>("item_id", translator);
        mapper.MapComposite<ItemsAccountingV1>("items_accounting_v1", translator);

        SqlMapper.AddTypeHandler(new ItemIdTypeHandler());
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
    }
}
