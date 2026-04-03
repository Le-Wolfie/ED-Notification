using EventNotify.Events;

namespace EventNotify.Handlers;

/// Generic event handler abstraction. Handlers implement this to react to specific event types.
public interface IEventHandler<T> where T : IEvent
{
    Task Handle(T @event); /// Handle the event asynchronously. Implementations define the reaction logic.
}
