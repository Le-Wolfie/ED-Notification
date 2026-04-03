namespace EventNotify.Events;

public record OrderCompletedEvent(int OrderId, int UserId, decimal Total) : IEvent;
