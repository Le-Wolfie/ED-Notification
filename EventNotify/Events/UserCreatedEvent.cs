namespace EventNotify.Events;

public record UserCreatedEvent(int UserId, string Email) : IEvent;
