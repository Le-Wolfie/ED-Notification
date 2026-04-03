using System.Threading.Channels;
using EventNotify.Events;

namespace EventNotify.EventQueue;

/// <summary>
/// In-memory event queue using System.Threading.Channels.
/// Events are stored in an unbounded channel and processed by background workers.
/// Use for development or single-instance deployments.
/// For distributed/production scenarios, use RabbitMQEventQueue instead.
/// </summary>
public class InMemoryEventQueue : IEventQueue
{
    private readonly Channel<IEvent> _channel = Channel.CreateUnbounded<IEvent>();

    public async Task EnqueueAsync<T>(T @event) where T : IEvent
    {
        await _channel.Writer.WriteAsync(@event ?? throw new ArgumentNullException(nameof(@event)));
    }

    public async Task<IEvent?> DequeueAsync(TimeSpan timeout)
    {
        // We use a cancellation token because Channel does not support timeouts directly. If timeout occurs, we return null.
        // WaitToReadAsync will return false if the channel is completed, but since we never complete it, we rely on the cancellation token for timeout.
        // If the channel is empty, WaitToReadAsync will wait until an item is available or the timeout occurs.
        using var cts = new CancellationTokenSource(timeout);
        try
        {
            while (await _channel.Reader.WaitToReadAsync(cts.Token))
            {
                // For each available item, try to read it. If we successfully read an item, return it. 
                // If the channel is empty, WaitToReadAsync will wait until an item is available or the timeout occurs.
                if (_channel.Reader.TryRead(out var item))
                {
                    return item;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Timeout - return null
        }

        return default;  // Returns null for reference types
    }
}
