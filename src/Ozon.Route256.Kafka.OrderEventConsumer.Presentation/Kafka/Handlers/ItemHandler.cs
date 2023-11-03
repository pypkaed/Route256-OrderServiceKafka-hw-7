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
        _logger.LogInformation("zhiopaa");

        var orderEvents = messages.Select(m => m.Message.Value);
        foreach (var orderEvent in orderEvents)
        {
            var status = orderEvent.Status;

            foreach (var orderEventPosition in orderEvent.Positions)
            {
                var itemId = new ItemId(orderEventPosition.ItemId);
                var quantity = orderEventPosition.Quantity;
                var modifiedAt = DateTime.Now;

                var currentItemAccounting = await _repository.Get(itemId, token);
                if (currentItemAccounting is null)
                {
                    await _repository.Add(
                            new ItemsAccountingV1
                            {
                                ItemId = itemId,
                                Reserved = 0,
                                Sold = 0,
                                Canceled = 0,
                                ModifiedAt = modifiedAt
                            },
                        token);
                }

                long reserved = currentItemAccounting?.Reserved ?? 0;
                long sold = currentItemAccounting?.Sold ?? 0;
                long canceled = currentItemAccounting?.Canceled ?? 0;

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
    }
}
