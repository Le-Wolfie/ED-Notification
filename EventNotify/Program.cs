using EventNotify.Data;
using EventNotify.EventBus;
using EventNotify.EventQueue;
using EventNotify.EventQueue.RabbitMQ;
using EventNotify.Events;
using EventNotify.Handlers;
using EventNotify.HostedServices;
using EventNotify.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Database: SQLite (using connection string from appsettings or environment)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=eventnotify.db";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// RabbitMQ Configuration
var rabbitMqSettings = builder.Configuration.GetSection("RabbitMQ").Get<RabbitMQSettings>()
    ?? new RabbitMQSettings();

builder.Services.AddSingleton(rabbitMqSettings);
builder.Services.AddSingleton<RabbitMQConnectionFactory>();
builder.Services.AddSingleton<IEventQueue, RabbitMQEventQueue>();

// Event Bus Architecture:
// 1. InMemoryEventBus: Executes handlers in parallel (scoped per request/event)
// 2. QueuedEventBus: Wraps IEventQueue for fire-and-forget from services
builder.Services.AddScoped<InMemoryEventBus>();
builder.Services.AddScoped<IEventBus, QueuedEventBus>();

// Background Service: Continuously processes queued events
// - Dequeues from RabbitMQ
// - Dispatches to InMemoryEventBus
// - Executes all handlers for each event asynchronously
builder.Services.AddHostedService<EventProcessorHostedService>();

// Register all event handlers for UserCreatedEvent
builder.Services.AddScoped<IEventHandler<UserCreatedEvent>, UserCreatedEmailHandler>();
builder.Services.AddScoped<IEventHandler<UserCreatedEvent>, UserCreatedLoggingHandler>();

// Register all event handlers for OrderCreatedEvent
builder.Services.AddScoped<IEventHandler<OrderCreatedEvent>, OrderCreatedEmailHandler>();
builder.Services.AddScoped<IEventHandler<OrderCreatedEvent>, OrderCreatedLoggingHandler>();

// Register all event handlers for OrderCompletedEvent
// (Add more handlers here as needed)

// Register all event handlers for PaymentProcessedEvent
builder.Services.AddScoped<IEventHandler<PaymentProcessedEvent>, PaymentProcessedNotificationHandler>();
builder.Services.AddScoped<IEventHandler<PaymentProcessedEvent>, PaymentProcessedLoggingHandler>();

// Register services
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<PaymentService>();

// Add controllers
builder.Services.AddControllers();

// Swagger/Swashbuckle
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Log startup
app.Logger.LogInformation("🐰 RabbitMQ event queue provider initialized");

// Initialize database (EnsureCreated creates DB if not exists)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
    app.Logger.LogInformation("Database ensured created");
}

// Configure HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "EventNotify API v1");
        c.RoutePrefix = ""; // Swagger at root: http://localhost:5103
    });
}
// For production, consider enabling HTTPS redirection and configuring proper certificates
// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
