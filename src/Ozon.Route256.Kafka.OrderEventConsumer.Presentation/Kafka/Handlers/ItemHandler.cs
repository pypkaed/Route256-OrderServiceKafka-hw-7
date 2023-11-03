using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.Models;
using Ozon.Route256.Kafka.OrderEventConsumer.Domain.ValueObjects;
using Ozon.Route256.Kafka.OrderEventConsumer.Infrastructure.Kafka;
using Ozon.Route256.Kafka.OrderEventConsumer.Presentation.Contracts;

namespace Ozon.Route256.Kafka.OrderEventConsumer.Presentation.Kafka.Handlers;

public class ItemHandler : IHandler<Ignore, OrderEvent>
{
    private readonly ILogger<ItemHandler> _logger;
    private readonly IItemRepository _repository;
    private readonly Random _random = new();

    public ItemHandler(
        ILogger<ItemHandler> logger,
        IItemRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    public async Task Handle(IReadOnlyCollection<ConsumeResult<Ignore, OrderEvent>> messages, CancellationToken token)
    {
        _logger.LogInformation($"Read {messages.Count} messages! :D");

        var orderEvents = messages.Select(m => m.Message.Value);
        foreach (var orderEvent in orderEvents)
        {
            var status = orderEvent.Status;

            await UpdateItemsAccounting(orderEvent.Positions, status, token);
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

            var currentItemAccounting = await _repository.Get(itemId, token);
            if (currentItemAccounting is null)
            {
                currentItemAccounting = new ItemsAccountingV1
                {
                    ItemId = itemId,
                    Reserved = 0,
                    Sold = 0,
                    Canceled = 0,
                    ModifiedAt = modifiedAt
                };
                await _repository.Add(currentItemAccounting, token);
            }

            CalculateStats(
                currentItemAccounting,
                status,
                quantity,
                out var reserved,
                out var sold,
                out var canceled);

            var model = new ItemsAccountingV1
            {
                ItemId = itemId,
                Reserved = reserved,
                Sold = sold,
                Canceled = canceled,
                ModifiedAt = modifiedAt
            };

            await _repository.Update(model, token);
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
    }
}
