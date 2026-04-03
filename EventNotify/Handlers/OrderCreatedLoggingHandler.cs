using EventNotify.Events;
using Microsoft.Extensions.Logging;

namespace EventNotify.Handlers;

public class OrderCreatedLoggingHandler : IEventHandler<OrderCreatedEvent>
{
    private readonly ILogger<OrderCreatedLoggingHandler> _logger;

    public OrderCreatedLoggingHandler(ILogger<OrderCreatedLoggingHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(OrderCreatedEvent @event)
    {
        _logger.LogInformation("📝 [Order Logging Handler START] Audit: Order created - OrderId={OrderId}, UserId={UserId}, Total={Total}",
            @event.OrderId, @event.UserId, @event.Total);

        // Simulate logging to audit database
        await Task.Delay(100);

        _logger.LogInformation("📝 [Order Logging Handler END] Audit logged for OrderId={OrderId}", @event.OrderId);
    }
}
