using EventNotify.Events;
using Microsoft.Extensions.Logging;

namespace EventNotify.Handlers;

public class PaymentProcessedLoggingHandler : IEventHandler<PaymentProcessedEvent>
{
    private readonly ILogger<PaymentProcessedLoggingHandler> _logger;

    public PaymentProcessedLoggingHandler(ILogger<PaymentProcessedLoggingHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(PaymentProcessedEvent @event)
    {
        _logger.LogInformation("📝 [Payment Logging Handler START] Audit: Payment {Status} - PaymentId={PaymentId}, OrderId={OrderId}, Amount={Amount}",
            @event.Status, @event.PaymentId, @event.OrderId, @event.Amount);

        // Simulate logging to audit database
        await Task.Delay(100);

        _logger.LogInformation("📝 [Payment Logging Handler END] Audit logged for PaymentId={PaymentId}", @event.PaymentId);
    }
}
