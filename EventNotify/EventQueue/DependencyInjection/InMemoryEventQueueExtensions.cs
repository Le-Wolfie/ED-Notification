using EventNotify.EventQueue;
using Microsoft.Extensions.DependencyInjection;

namespace EventNotify.EventQueue.DependencyInjection;

/// <summary>
/// Extension methods for registering in-memory event queue in dependency injection.
/// Use this for development environments or single-instance deployments.
/// </summary>
public static class InMemoryEventQueueExtensions
{
    /// <summary>
    /// Register in-memory event queue as the implementation of IEventQueue.
    /// 
    /// Usage in Program.cs:
    ///   services.AddInMemoryEventQueue();
    /// </summary>
    public static IServiceCollection AddInMemoryEventQueue(this IServiceCollection services)
    {
        // Register queue as singleton (single queue instance for entire app)
        services.AddSingleton<IEventQueue, InMemoryEventQueue>();
        return services;
    }
}
