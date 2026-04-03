namespace EventNotify.Events;

public record PaymentProcessedEvent(int PaymentId, int OrderId, decimal Amount, string Status) : IEvent;
