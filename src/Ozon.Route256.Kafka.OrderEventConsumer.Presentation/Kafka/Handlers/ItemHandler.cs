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
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.ValueObjects;
using Ozon.Route256.Kafka.OrderEventConsumer.Infrastructure.Kafka;
using Ozon.Route256.Kafka.OrderEventConsumer.Presentation.Contracts;
using IsolationLevel = Confluent.Kafka.IsolationLevel;

namespace Ozon.Route256.Kafka.OrderEventConsumer.Presentation.Kafka.Handlers;

public class ItemHandler : IHandler<Ignore, OrderEvent>
{
    private readonly ILogger<ItemHandler> _logger;
    private readonly IItemRepository _itemRepository;
    // TODO: move to other handler :D
    private readonly ISellerPaymentRepository _sellerPaymentRepository;

    public ItemHandler(
        ILogger<ItemHandler> logger,
        IItemRepository itemRepository,
        ISellerPaymentRepository sellerPaymentRepository)
    {
        _logger = logger;
        _itemRepository = itemRepository;
        _sellerPaymentRepository = sellerPaymentRepository;
    }

    public async Task Handle(IReadOnlyCollection<ConsumeResult<Ignore, OrderEvent>> messages, CancellationToken token)
    {
        _logger.LogInformation($"Read {messages.Count} messages! :D");

        var orderEvents = messages.Select(m => m.Message.Value);
        foreach (var orderEvent in orderEvents)
        {
            var status = orderEvent.Status;

            await UpdateItemsAccounting(orderEvent.Positions, status, token);

            if (status is OrderEvent.OrderStatus.Delivered)
            {
                await UpdateSellerPayment(orderEvent.Positions, token);
            }
        }
    }

    private async Task UpdateItemsAccounting(
        OrderEvent.OrderEventPosition[] orderEventPositions,
        OrderEvent.OrderStatus status,
        CancellationToken token)
    {
        foreach (var orderEventPosition in orderEventPositions)
        {
            var itemId = new ItemId(orderEventPosition.ItemId);
            var quantity = orderEventPosition.Quantity;
            var modifiedAt = DateTime.Now;

            var model = await _itemRepository.Get(itemId, token);
            if (model is null)
            {
                model = new ItemsAccountingV1
                {
                    ItemId = itemId,
                    Reserved = 0,
                    Sold = 0,
                    Canceled = 0,
                    ModifiedAt = modifiedAt
                };
                await _itemRepository.Add(model, token);
            }

            CalculateStats(
                model,
                status,
                quantity,
                out var reserved,
                out var sold,
                out var canceled);

            model = model with
            {
                Reserved = reserved,
                Sold = sold,
                Canceled = canceled,
            };

            await _itemRepository.Update(model, token);
        }
    }

    private void CalculateStats(
        ItemsAccountingV1 currentItemAccounting,
        OrderEvent.OrderStatus status,
        int quantity,
        out long reserved,
        out long sold,
        out long canceled)
    {
        reserved = currentItemAccounting.Reserved;
        sold = currentItemAccounting.Sold;
        canceled = currentItemAccounting.Canceled;

        switch (status)
        {
            case OrderEvent.OrderStatus.Created:
                reserved += quantity;
                break;
            case OrderEvent.OrderStatus.Delivered:
                sold += quantity;
                reserved -= quantity;
                break;
            case OrderEvent.OrderStatus.Canceled:
                canceled += quantity;
                reserved -= quantity;
                break;
        }

        if (reserved < 0)
        {
            reserved = 0;
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
