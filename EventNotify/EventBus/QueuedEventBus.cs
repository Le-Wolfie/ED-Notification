using EventNotify.EventQueue;
using EventNotify.Events;
using Microsoft.Extensions.Logging;

namespace EventNotify.EventBus;

/// <summary>
/// Event bus that queues events for background processing instead of executing handlers immediately.
/// API calls return instantly; handlers process asynchronously from a queue.
/// </summary>
public class QueuedEventBus : IEventBus
{
    private readonly IEventQueue _eventQueue;
    private readonly ILogger<QueuedEventBus> _logger;

    public QueuedEventBus(IEventQueue eventQueue, ILogger<QueuedEventBus> logger)
    {
        _eventQueue = eventQueue;
        _logger = logger;
    }

    public async Task Publish<T>(T @event) where T : IEvent
    {
        var eventType = typeof(T).Name;
        _logger.LogInformation("📮 Queueing event: {EventType}", eventType);

        try
        {
            await _eventQueue.EnqueueAsync(@event);
            _logger.LogInformation("✅ Event {EventType} enqueued successfully", eventType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error queueing event {EventType}", eventType);
            throw;
        }
    }
}
