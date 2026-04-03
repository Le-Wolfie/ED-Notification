using EventNotify.Events;
using Microsoft.Extensions.Logging;

namespace EventNotify.Handlers;

public class PaymentProcessedNotificationHandler : IEventHandler<PaymentProcessedEvent>
{
    private readonly ILogger<PaymentProcessedNotificationHandler> _logger;

    public PaymentProcessedNotificationHandler(ILogger<PaymentProcessedNotificationHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(PaymentProcessedEvent @event)
    {
        _logger.LogInformation("💳 [Payment Notification Handler START] Payment {Status} - PaymentId={PaymentId}, OrderId={OrderId}, Amount={Amount}",
            @event.Status, @event.PaymentId, @event.OrderId, @event.Amount);

        // Simulate sending notification
        await Task.Delay(100);

        _logger.LogInformation("💳 [Payment Notification Handler END] Notification sent for PaymentId={PaymentId}", @event.PaymentId);
    }
}
