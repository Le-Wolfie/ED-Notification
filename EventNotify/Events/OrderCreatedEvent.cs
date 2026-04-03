namespace EventNotify.Events;

public record OrderCreatedEvent(int OrderId, int UserId, decimal Total) : IEvent;
