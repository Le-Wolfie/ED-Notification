using EventNotify.DTOs;
using EventNotify.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventNotify.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(OrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new order. Publishes OrderCreatedEvent to all registered handlers.
    /// </summary>
    [HttpPost]
    [Produces("application/json")]
    public async Task<ActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        if (dto.UserId <= 0)
        {
            _logger.LogWarning("CreateOrder validation failed: UserId is invalid");
            return BadRequest(new { error = "UserId is required and must be positive" });
        }

        if (dto.Total <= 0)
        {
            _logger.LogWarning("CreateOrder validation failed: Total is invalid");
            return BadRequest(new { error = "Total is required and must be positive" });
        }

        try
        {
            var order = await _orderService.CreateOrder(dto);
            _logger.LogInformation("Order created successfully: {OrderId}", order.Id);

            return CreatedAtAction(nameof(CreateOrder), new { id = order.Id }, order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "An error occurred while creating the order" });
        }
    }

    /// <summary>
    /// Complete an order. Publishes OrderCompletedEvent to all registered handlers.
    /// </summary>
    [HttpPut("{id}/complete")]
    [Produces("application/json")]
    public async Task<ActionResult> CompleteOrder(int id)
    {
        try
        {
            var order = await _orderService.CompleteOrder(id);
            _logger.LogInformation("Order completed successfully: {OrderId}", order.Id);
            return Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Order not found: {OrderId}", id);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing order");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "An error occurred while completing the order" });
        }
    }
}
