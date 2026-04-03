using EventNotify.EventQueue.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;

namespace EventNotify.EventQueue.DependencyInjection;

/// <summary>
/// Extension methods for registering RabbitMQ event queue in dependency injection.
/// Handles both setup and teardown of RabbitMQ infrastructure.
/// </summary>
public static class RabbitMQDependencyInjectionExtensions
{
    /// <summary>
    /// Register RabbitMQ event queue as the implementation of IEventQueue.
    /// 
    /// Usage in Program.cs:
    ///   // Option 1: Use configuration from appsettings.json
    ///   services.AddRabbitMQEventQueue(builder.Configuration);
    ///   
    ///   // Option 2: Provide settings directly
    ///   var settings = new RabbitMQSettings { HostName = "rabbitmq.example.com" };
    ///   services.AddRabbitMQEventQueue(settings);
    /// </summary>
    public static IServiceCollection AddRabbitMQEventQueue(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var settings = configuration.GetSection("RabbitMQ").Get<RabbitMQSettings>()
            ?? new RabbitMQSettings();

        return AddRabbitMQEventQueue(services, settings);
    }

    /// <summary>
    /// Register RabbitMQ event queue with explicit settings.
    /// </summary>
    public static IServiceCollection AddRabbitMQEventQueue(
        this IServiceCollection services,
        RabbitMQSettings settings)
    {
        // Register settings as singleton (configuration is immutable)
        services.AddSingleton(settings);

        // Register connection factory as singleton (maintains persistent connection)
        services.AddSingleton<RabbitMQConnectionFactory>();

        // Register event queue as singleton (manages single connection + channel)
        services.AddSingleton<IEventQueue, RabbitMQEventQueue>();

        // Optional: Register as IAsyncDisposable for proper cleanup
        services.AddSingleton(sp => (IAsyncDisposable)sp.GetRequiredService<IEventQueue>());

        return services;
    }
}
