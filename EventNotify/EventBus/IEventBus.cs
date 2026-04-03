using EventNotify.Events;

namespace EventNotify.EventBus;

/// Event bus abstraction for publishing domain events to registered handlers.
public interface IEventBus
{
    Task Publish<T>(T @event) where T : IEvent;
}
