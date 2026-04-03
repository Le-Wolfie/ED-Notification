using EventNotify.Data;
using EventNotify.DTOs;
using EventNotify.Entities;
using EventNotify.EventBus;
using EventNotify.Events;
using Microsoft.Extensions.Logging;

namespace EventNotify.Services;

/// UserService orchestrates user creation and publishes domain events.
/// Errors in event publishing propagate to caller (fail-fast).
public class UserService
{
    private readonly AppDbContext _dbContext;
    private readonly IEventBus _eventBus;
    private readonly ILogger<UserService> _logger;

    public UserService(AppDbContext dbContext, IEventBus eventBus, ILogger<UserService> logger)
    {
        _dbContext = dbContext;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<User> CreateUser(CreateUserDto dto)
    {
        _logger.LogInformation("Creating user with email: {Email}", dto.Email);

        // Create and persist user
        var user = new User
        {
            Email = dto.Email,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("User created successfully with ID: {UserId}", user.Id);

        // Publish event; if any handler fails, exception propagates (no swallowing)
        try
        {
            var userCreatedEvent = new UserCreatedEvent(user.Id, user.Email);
            await _eventBus.Publish(userCreatedEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing UserCreatedEvent for UserId: {UserId}", user.Id);
            throw; // Fail-fast: propagate to caller
        }

        return user;
    }
}
