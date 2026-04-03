using EventNotify.DTOs;
using EventNotify.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventNotify.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly PaymentService _paymentService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(PaymentService paymentService, ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    /// <summary>
    /// Process a payment. Publishes PaymentProcessedEvent to all registered handlers.
    /// </summary>
    [HttpPost]
    [Produces("application/json")]
    public async Task<ActionResult> ProcessPayment([FromBody] CreatePaymentDto dto)
    {
        if (dto.OrderId <= 0)
        {
            _logger.LogWarning("ProcessPayment validation failed: OrderId is invalid");
            return BadRequest(new { error = "OrderId is required and must be positive" });
        }

        if (dto.Amount <= 0)
        {
            _logger.LogWarning("ProcessPayment validation failed: Amount is invalid");
            return BadRequest(new { error = "Amount is required and must be positive" });
        }

        try
        {
            var payment = await _paymentService.ProcessPayment(dto);
            _logger.LogInformation("Payment processed successfully: {PaymentId}", payment.Id);

            return CreatedAtAction(nameof(ProcessPayment), new { id = payment.Id }, payment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "An error occurred while processing the payment" });
        }
    }
}
