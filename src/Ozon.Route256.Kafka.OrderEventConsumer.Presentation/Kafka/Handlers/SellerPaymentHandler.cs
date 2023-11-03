using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.Models;
using Ozon.Route256.Kafka.OrderEventConsumer.Infrastructure.Kafka;
using Ozon.Route256.Kafka.OrderEventConsumer.Presentation.Contracts;

namespace Ozon.Route256.Kafka.OrderEventConsumer.Presentation.Kafka.Handlers;

public class SellerPaymentHandler : IHandler<Ignore, OrderEvent>
{
    private readonly ILogger<ItemHandler> _logger;
    private readonly ISellerPaymentRepository _sellerPaymentRepository;

    public SellerPaymentHandler(
        ISellerPaymentRepository sellerPaymentRepository,
        ILogger<ItemHandler> logger)
    {
        _sellerPaymentRepository = sellerPaymentRepository;
        _logger = logger;
    }

    public async Task Handle(IReadOnlyCollection<ConsumeResult<Ignore, OrderEvent>> messages, CancellationToken token)
    {
        _logger.LogInformation($"Seller Payment Handler read {messages.Count} messages! :D");

        var orderEvents = messages.Select(m => m.Message.Value);
        foreach (var orderEvent in orderEvents)
        {
            var status = orderEvent.Status;
            if (status is not OrderEvent.OrderStatus.Delivered)
            {
                continue;
            }

            await UpdateSellerPayment(orderEvent.Positions, token);
        }
    }


    private async Task UpdateSellerPayment(
        OrderEvent.OrderEventPosition[] orderEventPositions,
        CancellationToken token)
    {
        foreach (var orderEventPosition in orderEventPositions)
        {
            // TODO: magic numbers
            var sellerId = orderEventPosition.ItemId / 1000000;

            using var transaction = CreateTransactionScope();

            var model = await _sellerPaymentRepository.Get(sellerId, token);
            if (model is null)
            {
                model = new SellerPaymentV1()
                {
                    SellerId = sellerId,
                    Rub = 0,
                    Kzt = 0
                };
                await _sellerPaymentRepository.Add(model, token);
            }

            decimal units = orderEventPosition.Price.Units;
            decimal nanos = orderEventPosition.Price.Nanos / 1_000_000_000m;

            var payment = orderEventPosition.Quantity * (units + nanos);

            var currency = orderEventPosition.Price.Currency;

            model = currency switch
            {
                "RUB" => model with { Rub = payment + model.Rub },
                "KZT" => model with { Kzt = payment + model.Kzt },
                _ => throw new Exception("Unsupported currency type.")
            };

            await _sellerPaymentRepository.Update(model, token);

            transaction.Complete();
        }
    }

    private TransactionScope CreateTransactionScope(
        System.Transactions.IsolationLevel level = System.Transactions.IsolationLevel.ReadCommitted)
    {
        return new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions
            {
                IsolationLevel = level,
                Timeout = TimeSpan.FromSeconds(5)
            },
            TransactionScopeAsyncFlowOption.Enabled);
    }
}
