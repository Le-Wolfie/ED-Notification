using System.Text;
using System.Text.Json;
using EventNotify.Events;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EventNotify.EventQueue.RabbitMQ;

/// <summary>
/// Event queue backed by RabbitMQ for distributed, persistent messaging.
/// Compatible with RabbitMQ.Client 6.8.1 (synchronous consumer pattern).
/// </summary>
public class RabbitMQEventQueue : IEventQueue, IDisposable
{
    private readonly RabbitMQConnectionFactory _connectionFactory;
    private readonly RabbitMQSettings _settings;
    private readonly ILogger<RabbitMQEventQueue> _logger;
    private IModel? _channel;
    private readonly Lock _lockObject = new();

    // Type registry for deserialization
    private static readonly Dictionary<string, Type> EventTypeRegistry = new()
    {
        { nameof(UserCreatedEvent), typeof(UserCreatedEvent) },
        { nameof(OrderCreatedEvent), typeof(OrderCreatedEvent) },
        { nameof(OrderCompletedEvent), typeof(OrderCompletedEvent) },
        { nameof(PaymentProcessedEvent), typeof(PaymentProcessedEvent) }
    };

    public RabbitMQEventQueue(
        RabbitMQConnectionFactory connectionFactory,
        RabbitMQSettings settings,
        ILogger<RabbitMQEventQueue> logger)
    {
        _connectionFactory = connectionFactory;
        _settings = settings;
        _logger = logger;
    }

    /// <summary>
    /// Lazy-initialize channel (thread-safe).
    /// </summary>
    private IModel GetChannel()
    {
        if (_channel is not null && _channel.IsOpen)
        {
            return _channel;
        }

        lock (_lockObject)
        {
            if (_channel is not null && _channel.IsOpen)
            {
                return _channel;
            }

            _channel = _connectionFactory.SetupQueue();
            return _channel;
        }
    }

    /// <summary>
    /// Enqueue an event by publishing to RabbitMQ exchange.
    /// </summary>
    public async Task EnqueueAsync<T>(T @event) where T : IEvent
    {
        if (@event == null)
            throw new ArgumentNullException(nameof(@event));

        try
        {
            var channel = GetChannel();
            var eventType = typeof(T).Name;

            // Serialize to JSON
            var json = JsonSerializer.Serialize(@event);
            var messageBody = Encoding.UTF8.GetBytes(json);

            // Create message properties
            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";
            properties.Headers = new Dictionary<string, object?>
            {
                { "EventType", eventType },
                { "Timestamp", DateTime.UtcNow.ToString("O") }  // ← Convert to string (ISO 8601 format)
            };

            _logger.LogInformation("📮 Publishing event to RabbitMQ: {EventType}", eventType);

            // Publish to exchange
            channel.BasicPublish(
                exchange: _settings.ExchangeName,
                routingKey: string.Empty,
                mandatory: false,
                basicProperties: properties,
                body: messageBody);

            _logger.LogInformation("✅ Event {EventType} published to RabbitMQ", eventType);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error enqueuing event to RabbitMQ");
            throw;
        }
    }

    /// <summary>
    /// Dequeue an event from RabbitMQ queue with timeout.
    /// </summary>
    public async Task<IEvent?> DequeueAsync(TimeSpan timeout)
    {
        try
        {
            var channel = GetChannel();

            // BasicGet: Synchronously retrieve one message (non-blocking, returns null if empty)
            var result = channel.BasicGet(_settings.EventQueueName, _settings.AutoAck);

            if (result == null)
            {
                await Task.Delay(100);  // Brief delay before next attempt
                return null;
            }

            // Deserialize message
            var json = Encoding.UTF8.GetString(result.Body.ToArray());
            var headers = result.BasicProperties?.Headers;

            // Headers in RabbitMQ are stored as byte[]. Decode properly.
            string? eventType = null;
            if (headers?.TryGetValue("EventType", out var eventTypeValue) == true)
            {
                eventType = eventTypeValue is byte[] typeBytes
                    ? Encoding.UTF8.GetString(typeBytes)
                    : eventTypeValue?.ToString();
            }

            if (string.IsNullOrEmpty(eventType) || !EventTypeRegistry.TryGetValue(eventType, out var type))
            {
                _logger.LogError("❌ Unknown event type in RabbitMQ message: {EventType}", eventType ?? "null");
                // Negative ack to return message to queue
                if (!_settings.AutoAck)
                {
                    channel.BasicNack(result.DeliveryTag, false, true);
                }
                return null;
            }

            var @event = JsonSerializer.Deserialize(json, type) as IEvent;

            _logger.LogInformation("⚙️  Dequeued event from RabbitMQ: {EventType}", eventType);

            // Positive ack (remove message from queue)
            if (!_settings.AutoAck)
            {
                channel.BasicAck(result.DeliveryTag, false);
            }

            return @event;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error dequeuing event from RabbitMQ");
            throw;
        }
    }

    /// <summary>
    /// Clean up resources.
    /// </summary>
    public void Dispose()
    {
        lock (_lockObject)
        {
            if (_channel?.IsOpen ?? false)
            {
                try
                {
                    _logger.LogInformation("🔌 Closing RabbitMQ channel...");
                    _channel.Close();
                    _channel.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error closing RabbitMQ channel");
                }
                finally
                {
                    _channel = null;
                }
            }
        }

        _connectionFactory.Dispose();
    }
}
