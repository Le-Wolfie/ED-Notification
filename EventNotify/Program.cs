using EventNotify.Data;
using EventNotify.EventBus;
using EventNotify.EventQueue;
using EventNotify.EventQueue.DependencyInjection;
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

// Event Queue and Bus Configuration (choose provider: InMemory or RabbitMQ)
var queueProvider = builder.Configuration.GetValue("EventQueueProvider", "InMemory");

if (queueProvider.Equals("RabbitMQ", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddRabbitMQEventQueue(builder.Configuration);
}
else
{
    builder.Services.AddInMemoryEventQueue();
}

// Register the actual event bus used for handler execution (by background service)
builder.Services.AddScoped<InMemoryEventBus>();

// Register the queueing event bus used by services (returns immediately)
builder.Services.AddScoped<IEventBus, QueuedEventBus>();

// Register background service that processes queued events
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

// Log which event queue provider is being used
var providerLog = queueProvider.Equals("RabbitMQ", StringComparison.OrdinalIgnoreCase)
    ? "🐰 RabbitMQ event queue provider registered"
    : "💾 In-Memory event queue provider registered";
app.Logger.LogInformation(providerLog);

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
