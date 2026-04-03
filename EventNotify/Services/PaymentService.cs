using EventNotify.Data;
using EventNotify.DTOs;
using EventNotify.Entities;
using EventNotify.EventBus;
using EventNotify.Events;
using Microsoft.Extensions.Logging;

namespace EventNotify.Services;

public class PaymentService
{
    private readonly AppDbContext _dbContext;
    private readonly IEventBus _eventBus;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(AppDbContext dbContext, IEventBus eventBus, ILogger<PaymentService> logger)
    {
        _dbContext = dbContext;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<Payment> ProcessPayment(CreatePaymentDto dto)
    {
        _logger.LogInformation("Processing payment for OrderId={OrderId}, Amount={Amount}", dto.OrderId, dto.Amount);

        // Create and persist payment
        var payment = new Payment
        {
            OrderId = dto.OrderId,
            Amount = dto.Amount,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Payments.Add(payment);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Payment created successfully with ID: {PaymentId}", payment.Id);

        // Simulate payment processing
        await Task.Delay(100);

        // Mark as successful
        payment.Status = "Success";
        _dbContext.Payments.Update(payment);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Payment processed successfully with ID: {PaymentId}", payment.Id);

        // Publish event
        try
        {
            var paymentProcessedEvent = new PaymentProcessedEvent(payment.Id, payment.OrderId, payment.Amount, payment.Status);
            await _eventBus.Publish(paymentProcessedEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing PaymentProcessedEvent for PaymentId: {PaymentId}", payment.Id);
            throw;
        }

        return payment;
    }
}
