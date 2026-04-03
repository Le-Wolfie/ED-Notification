using EventNotify.Events;
using Microsoft.Extensions.Logging;

namespace EventNotify.Handlers;

public class OrderCreatedEmailHandler : IEventHandler<OrderCreatedEvent>
{
    private readonly ILogger<OrderCreatedEmailHandler> _logger;

    public OrderCreatedEmailHandler(ILogger<OrderCreatedEmailHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(OrderCreatedEvent @event)
    {
        _logger.LogInformation("📧 [Order Email Handler START] Sending order confirmation for Order={OrderId}, UserId={UserId}, Total={Total}",
            @event.OrderId, @event.UserId, @event.Total);

        // Simulate sending email
        await Task.Delay(100);

        _logger.LogInformation("📧 [Order Email Handler END] Order confirmation sent for Order={OrderId}", @event.OrderId);
    }
}
