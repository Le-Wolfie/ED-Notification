using EventNotify.EventBus;
using EventNotify.EventQueue;
using EventNotify.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EventNotify.HostedServices;

/// <summary>
/// Background service that continuously processes queued events.
/// Dequeues events and dispatches them to handlers via the actual EventBus.
/// </summary>
public class EventProcessorHostedService : BackgroundService
{
    private readonly IEventQueue _eventQueue;
    private readonly ILogger<EventProcessorHostedService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public EventProcessorHostedService(
        IEventQueue eventQueue,
        ILogger<EventProcessorHostedService> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _eventQueue = eventQueue;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🚀 Event processor hosted service starting...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Create a scope for this event processing cycle
                using var scope = _serviceScopeFactory.CreateScope();
                var processingBus = scope.ServiceProvider.GetRequiredService<InMemoryEventBus>();

                // Dequeue any event (not type-specific)
                var @event = await _eventQueue.DequeueAsync(TimeSpan.FromSeconds(1));
                if (@event != null)
                {
                    var eventType = @event.GetType().Name;
                    _logger.LogInformation("⚙️  Processing dequeued event: {EventType}", eventType);

                    // Use reflection to call PublishAsync with the correct type
                    var publishMethod = typeof(IEventBus).GetMethod("Publish")!
                        .MakeGenericMethod(@event.GetType());
                    var task = (Task)publishMethod.Invoke(processingBus, new[] { @event })!;
                    await task;
                    continue;
                }

                // No events dequeued, wait a bit before checking again
                await Task.Delay(100, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break; // Cancellation requested
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error processing queued event");
                // Continue processing despite errors
            }
        }

        _logger.LogInformation("🛑 Event processor hosted service stopping...");
    }
}
