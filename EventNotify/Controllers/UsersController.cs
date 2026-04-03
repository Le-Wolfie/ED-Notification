using EventNotify.DTOs;
using EventNotify.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventNotify.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(UserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// Create a new user. Publishes UserCreatedEvent to all registered handlers.
    [HttpPost]
    [Produces("application/json")]
    public async Task<ActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            _logger.LogWarning("CreateUser validation failed: Email is empty");
            return BadRequest(new { error = "Email is required" });
        }

        try
        {
            var user = await _userService.CreateUser(dto);
            _logger.LogInformation("User created successfully: {UserId}", user.Id);

            return CreatedAtAction(nameof(CreateUser), new { id = user.Id }, user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "An error occurred while creating the user" });
        }
    }
}
