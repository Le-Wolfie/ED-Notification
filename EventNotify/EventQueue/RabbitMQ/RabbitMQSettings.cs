namespace EventNotify.EventQueue.RabbitMQ;

/// <summary>
/// Configuration settings for RabbitMQ connection and queue behavior.
/// Typically bound from appsettings.json:RabbitMQ section.
/// </summary>
public class RabbitMQSettings
{
    /// <summary>
    /// Hostname or IP address of RabbitMQ server. Default: localhost
    /// </summary>
    public string HostName { get; set; } = "localhost";

    /// <summary>
    /// Port number for RabbitMQ AMQP protocol. Default: 5672
    /// </summary>
    public int Port { get; set; } = 5672;

    /// <summary>
    /// Username for authentication. Default: guest
    /// </summary>
    public string UserName { get; set; } = "guest";

    /// <summary>
    /// Password for authentication. Default: guest
    /// </summary>
    public string Password { get; set; } = "guest";

    /// <summary>
    /// Virtual host to connect to. Default: /
    /// </summary>
    public string VirtualHost { get; set; } = "/";

    /// <summary>
    /// Name of the RabbitMQ queue where events are stored.
    /// </summary>
    public string EventQueueName { get; set; } = "eventnotify.events";

    /// <summary>
    /// Name of the RabbitMQ exchange (pub/sub broker). 
    /// We use fanout for events (broadcasts to all interested queues).
    /// </summary>
    public string ExchangeName { get; set; } = "eventnotify.events";

    /// <summary>
    /// Exchange type: direct, topic, fanout, headers.
    /// fanout = broadcast to all bound queues (good for events)
    /// </summary>
    public string ExchangeType { get; set; } = "fanout";

    /// <summary>
    /// Auto-acknowledge messages? If true, RabbitMQ assumes message handled immediately.
    /// If false, handler must explicitly acknowledge (safer for production).
    /// </summary>
    public bool AutoAck { get; set; } = false;

    /// <summary>
    /// Default timeout for dequeueing (in seconds).
    /// </summary>
    public int DequeueTimeoutSeconds { get; set; } = 1;

    /// <summary>
    /// Maximum number of messages to prefetch from broker.
    /// Higher = better throughput, lower = better fairness across consumers.
    /// </summary>
    public ushort PrefetchCount { get; set; } = 10;
}
