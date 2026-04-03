using EventNotify.Events;
using Microsoft.Extensions.Logging;

namespace EventNotify.Handlers;

/// Test handler that throws an exception to verify fail-fast behavior.
/// Remove from DI after testing error handling.
public class UserCreatedErrorHandler : IEventHandler<UserCreatedEvent>
{
    private readonly ILogger<UserCreatedErrorHandler> _logger;

    public UserCreatedErrorHandler(ILogger<UserCreatedErrorHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(UserCreatedEvent @event)
    {
        _logger.LogInformation("❌ [Error Handler] Intentionally throwing exception for testing");
        throw new InvalidOperationException("Test: Simulated handler error!");
    }
}
