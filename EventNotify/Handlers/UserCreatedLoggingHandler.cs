using EventNotify.Events;
using Microsoft.Extensions.Logging;

namespace EventNotify.Handlers;

/// Handles UserCreatedEvent by logging user creation to audit log.
/// Simulates I/O with 100ms delay to demonstrate parallel handler execution.
public class UserCreatedLoggingHandler : IEventHandler<UserCreatedEvent>
{
    private readonly ILogger<UserCreatedLoggingHandler> _logger;

    public UserCreatedLoggingHandler(ILogger<UserCreatedLoggingHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(UserCreatedEvent @event)
    {
        _logger.LogInformation("📝 [Logging Handler START] Audit log: User created - UserId={UserId}, Email={Email}",
            @event.UserId, @event.Email);

        // Simulate I/O delay (write to audit log database)
        await Task.Delay(100);

        _logger.LogInformation("📝 [Logging Handler END] Audit log written for UserId={UserId}", @event.UserId);
    }
}
