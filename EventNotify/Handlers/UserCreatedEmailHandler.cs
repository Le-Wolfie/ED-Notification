using EventNotify.Events;
using Microsoft.Extensions.Logging;

namespace EventNotify.Handlers;

/// Handles UserCreatedEvent by sending a confirmation email.
/// Simulates I/O with 100ms delay to demonstrate parallel handler execution.
public class UserCreatedEmailHandler : IEventHandler<UserCreatedEvent>
{
    private readonly ILogger<UserCreatedEmailHandler> _logger;

    public UserCreatedEmailHandler(ILogger<UserCreatedEmailHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(UserCreatedEvent @event)
    {
        _logger.LogInformation("📧 [Email Handler START] Sending confirmation email to: {Email}", @event.Email);

        // Simulate I/O delay (network call to email service)
        await Task.Delay(100);

        _logger.LogInformation("📧 [Email Handler END] Email sent successfully to: {Email}", @event.Email);
    }
}
