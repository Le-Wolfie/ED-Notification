using EventNotify.Data;
using EventNotify.DTOs;
using EventNotify.Entities;
using EventNotify.EventBus;
using EventNotify.Events;
using Microsoft.Extensions.Logging;

namespace EventNotify.Services;

public class OrderService
{
    private readonly AppDbContext _dbContext;
    private readonly IEventBus _eventBus;
    private readonly ILogger<OrderService> _logger;

    public OrderService(AppDbContext dbContext, IEventBus eventBus, ILogger<OrderService> logger)
    {
        _dbContext = dbContext;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<Order> CreateOrder(CreateOrderDto dto)
    {
        _logger.LogInformation("Creating order for UserId={UserId}, Total={Total}", dto.UserId, dto.Total);

        // Create and persist order
        var order = new Order
        {
            UserId = dto.UserId,
            Total = dto.Total,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Order created successfully with ID: {OrderId}", order.Id);

        // Publish event
        try
        {
            var orderCreatedEvent = new OrderCreatedEvent(order.Id, order.UserId, order.Total);
            await _eventBus.Publish(orderCreatedEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing OrderCreatedEvent for OrderId: {OrderId}", order.Id);
            throw;
        }

        return order;
    }

    public async Task<Order> CompleteOrder(int orderId)
    {
        _logger.LogInformation("Completing order with ID: {OrderId}", orderId);

        var order = await _dbContext.Orders.FindAsync(orderId) ?? throw new InvalidOperationException($"Order {orderId} not found");
        order.Status = "Completed";
        _dbContext.Orders.Update(order);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Order completed successfully with ID: {OrderId}", order.Id);

        // Publish event
        try
        {
            var orderCompletedEvent = new OrderCompletedEvent(order.Id, order.UserId, order.Total);
            await _eventBus.Publish(orderCompletedEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing OrderCompletedEvent for OrderId: {OrderId}", order.Id);
            throw;
        }

        return order;
    }
}
