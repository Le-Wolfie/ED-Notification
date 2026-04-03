using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace EventNotify.EventQueue.RabbitMQ;

/// <summary>
/// Factory for creating and managing RabbitMQ connections.
/// Encapsulates connection logic, channel creation, and exchange/queue setup.
/// Compatible with RabbitMQ.Client 6.8.1 (stable synchronous API).
/// </summary>
public class RabbitMQConnectionFactory
{
    private readonly RabbitMQSettings _settings;
    private readonly ILogger<RabbitMQConnectionFactory> _logger;
    private IConnection? _connection;
    private readonly Lock _lockObject = new();

    public RabbitMQConnectionFactory(RabbitMQSettings settings, ILogger<RabbitMQConnectionFactory> logger)
    {
        _settings = settings;
        _logger = logger;
    }

    /// <summary>
    /// Gets or creates the RabbitMQ connection (singleton pattern).
    /// Connection is reused across the application lifetime.
    /// </summary>
    public IConnection GetConnection()
    {
        if (_connection is not null && _connection.IsOpen)
        {
            return _connection;
        }
        
        lock (_lockObject) 
        {
            // Double-check inside lock
            if (_connection is not null && _connection.IsOpen)
            {
                return _connection;
            }

            _logger.LogInformation("🔗 Establishing RabbitMQ connection to {HostName}:{Port}...",
                _settings.HostName, _settings.Port);

            var factory = new ConnectionFactory
            {
                HostName = _settings.HostName,
                Port = _settings.Port,
                UserName = _settings.UserName,
                Password = _settings.Password,
                VirtualHost = _settings.VirtualHost,
                DispatchConsumersAsync = true,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
                RequestedHeartbeat = TimeSpan.FromSeconds(60)
            };

            _connection = factory.CreateConnection();
            _logger.LogInformation("✅ RabbitMQ connection established");

            return _connection;
        }
    }

    /// <summary>
    /// Creates a channel from the connection and sets up the exchange and queue.
    /// </summary>
    public IModel SetupQueue()
    {
        var connection = GetConnection();
        var channel = connection.CreateModel();

        _logger.LogInformation("📢 Setting up RabbitMQ exchange: {ExchangeName} (type: {ExchangeType})",
            _settings.ExchangeName, _settings.ExchangeType);

        channel.ExchangeDeclare(
            exchange: _settings.ExchangeName,
            type: _settings.ExchangeType,
            durable: true,
            autoDelete: false);

        _logger.LogInformation("📭 Setting up RabbitMQ queue: {QueueName}", _settings.EventQueueName);

        channel.QueueDeclare(
            queue: _settings.EventQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false);

        _logger.LogInformation("🔗 Binding queue to exchange...");

        channel.QueueBind(
            queue: _settings.EventQueueName,
            exchange: _settings.ExchangeName,
            routingKey: string.Empty); // fanout exchange ignores routing key

        channel.BasicQos(0, _settings.PrefetchCount, false);

        _logger.LogInformation("✅ RabbitMQ topology configured");

        return channel;
    }

    /// <summary>
    /// Close the connection cleanly.
    /// </summary>
    public void Dispose()
    {
        lock (_lockObject)
        {
            if (_connection?.IsOpen ?? false)
            {
                _logger.LogInformation("🔌 Closing RabbitMQ connection...");
                _connection.Close();
                _connection?.Dispose();
                _connection = null;
            }
        }
    }
}
