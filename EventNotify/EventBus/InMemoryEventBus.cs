using EventNotify.Events;
using EventNotify.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EventNotify.EventBus;

/// In-memory event bus that dispatches events to registered handlers in parallel.
/// Handlers execute concurrently via Task.WhenAll(); if any handler throws, propagates exception (fail-fast).
public class InMemoryEventBus : IEventBus
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<InMemoryEventBus> _logger;

    public InMemoryEventBus(IServiceProvider serviceProvider, ILogger<InMemoryEventBus> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task Publish<T>(T @event) where T : IEvent
    {
        var eventType = typeof(T).Name;
        _logger.LogInformation("📢 Publishing event: {EventType}", eventType);

        var handlers = _serviceProvider.GetRequiredService<IEnumerable<IEventHandler<T>>>();

        try
        {
            // Execute all handlers in parallel; if any fails, fail-fast (propagate exception)
            var tasks = handlers
                .Select(handler =>
                {
                    _logger.LogInformation("▶️  Executing handler: {HandlerType} for event {EventType}",
                        handler.GetType().Name, eventType);
                    return handler.Handle(@event);
                })
                .ToList();

            await Task.WhenAll(tasks);

            _logger.LogInformation("✅ Event {EventType} published successfully with {HandlerCount} handler(s)",
                eventType, handlers.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error publishing event {EventType}", eventType);
            throw; // Fail-fast: propagate exception
        }
    }
}
