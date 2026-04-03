using EventNotify.Events;

namespace EventNotify.EventQueue;

/// <summary>
/// Abstraction for event queuing. Allows events to be enqueued and dequeued asynchronously.
/// </summary>
public interface IEventQueue
{
    /// <summary>
    /// Enqueue an event for processing by background workers.
    /// </summary>
    Task EnqueueAsync<T>(T @event) where T : IEvent;

    /// <summary>
    /// Dequeue any event with a timeout. Returns null if timeout occurs.
    /// </summary>
    Task<IEvent?> DequeueAsync(TimeSpan timeout);
}
