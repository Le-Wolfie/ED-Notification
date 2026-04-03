# Event-Driven Notification System

A .NET development practice project demonstrating event-driven architecture patterns with ASP.NET Core, RabbitMQ, and background event processing.

> **Note**: This project showcases clean architecture and design patterns suitable for production use. However, it's built as an educational reference without hardened production requirements (authentication, authorization, production logging, monitoring, etc.). 

## Features

- ✅ **Event-Driven Architecture** - Loosely coupled services via domain events
- ✅ **Pub/Sub Pattern** - Multiple handlers respond to single event independently
- ✅ **Background Processing** - Events queued for async handling (API returns immediately)
- ✅ **Durable Messaging** - RabbitMQ persistence (events survive app restarts)
- ✅ **Horizontal Scaling** - Multiple EventProcessorHostedService instances load-balance events
- ✅ **Pluggable Queue Backends** - Switch between RabbitMQ and in-memory via configuration
- ✅ **Clean Architecture** - Separation of concerns (Controllers → Services → EventBus → Handlers)
- ✅ **Swagger/OpenAPI** - Built-in API documentation and testing UI
- ✅ **Structured Logging** - Console logging with emoji indicators for easy monitoring

## Technology Stack

- **.NET 9.0** - Modern C# framework
- **ASP.NET Core** - Web API framework
- **Entity Framework Core 9.0** - ORM with SQLite
- **RabbitMQ 3.13** - Message broker (via Docker)
- **Swagger/Swashbuckle** - API documentation
- **System.Threading.Channels** - In-memory queue alternative

## Project Structure

```
EventNotify/
├── Controllers/              # HTTP endpoints
│   ├── UsersController.cs
│   ├── OrdersController.cs
│   └── PaymentsController.cs
├── Services/                 # Business logic orchestration
│   ├── UserService.cs
│   ├── OrderService.cs
│   └── PaymentService.cs
├── Entities/                 # Database models
│   ├── User.cs
│   ├── Order.cs
│   └── Payment.cs
├── DTOs/                     # Data transfer objects
│   ├── CreateUserDto.cs
│   ├── CreateOrderDto.cs
│   └── CreatePaymentDto.cs
├── Events/                   # Domain events
│   ├── IEvent.cs            # Marker interface
│   ├── UserCreatedEvent.cs
│   ├── OrderCreatedEvent.cs
│   ├── OrderCompletedEvent.cs
│   └── PaymentProcessedEvent.cs
├── Handlers/                 # Event handlers (Pub/Sub subscribers)
│   ├── IEventHandler.cs     # Generic interface
│   ├── UserCreatedEmailHandler.cs
│   ├── UserCreatedLoggingHandler.cs
│   ├── OrderCreatedEmailHandler.cs
│   ├── PaymentProcessedNotificationHandler.cs
│   └── PaymentProcessedLoggingHandler.cs
├── EventBus/                 # Event publishing abstraction
│   ├── IEventBus.cs         # Interface
│   ├── InMemoryEventBus.cs  # Executes handlers in parallel
│   └── QueuedEventBus.cs    # Queues events for later processing
├── EventQueue/               # Event queueing implementations
│   ├── IEventQueue.cs       # Interface
│   ├── InMemoryEventQueue.cs
│   └── RabbitMQ/
│       ├── RabbitMQSettings.cs
│       ├── RabbitMQConnectionFactory.cs
│       └── RabbitMQEventQueue.cs
├── HostedServices/           # Background workers
│   └── EventProcessorHostedService.cs
├── Data/                     # Database context
│   └── AppDbContext.cs
├── Program.cs               # DI setup and app configuration
├── appsettings.json        # Configuration
└── docker-compose.yml      # RabbitMQ container
```

## Quick Start

### Prerequisites

- .NET 9.0 SDK
- Docker & Docker Compose
- SQLite (included with EF Core)

### Setup

1. **Clone repository**

   ```bash
   git clone https://github.com/yourusername/EventNotify.git
   cd EventNotify
   ```

2. **Start RabbitMQ** (default messaging)

   ```bash
   docker-compose up -d
   docker-compose ps  # Verify it's running
   ```

3. **Run the application**

   ```bash
   cd EventNotify/EventNotify
   dotnet run
   # App starts with RabbitMQ (distributed messaging)
   ```

4. **Access the API**
   ```
   Swagger: http://localhost:5103
   RabbitMQ Admin: http://localhost:15672 (guest/guest)
   ```

### Using In-Memory Queue (Optional)

To use in-memory queue instead of RabbitMQ (no Docker needed):

```bash
# Edit appsettings.json and change:
"EventQueueProvider": "InMemory"

# Run the application
cd EventNotify/EventNotify
dotnet run

# No RabbitMQ needed - uses fast in-memory channels
```

## Usage Examples

### Create a User

```bash
curl -X POST http://localhost:5103/api/users \
  -H "Content-Type: application/json" \
  -d '{"email":"alice@example.com"}'
```

**Response:** `201 Created`

- UserCreatedEmailHandler sends email (background)
- UserCreatedLoggingHandler logs to audit trail (background)

### Create an Order

```bash
curl -X POST http://localhost:5103/api/orders \
  -H "Content-Type: application/json" \
  -d '{"userId":1,"total":99.99}'
```

**Response:** `201 Created` (returns immediately)

- Event enqueued to RabbitMQ
- OrderCreatedEmailHandler & OrderCreatedLoggingHandler execute in background
- Client doesn't wait for handlers to finish

### Process Payment

```bash
curl -X POST http://localhost:5103/api/payments \
  -H "Content-Type: application/json" \
  -d '{"orderId":1,"amount":99.99}'
```

**Response:** `201 Created`

- PaymentProcessedEvent enqueued
- Handlers execute asynchronously

## Configuration

### `appsettings.json` (Default - InMemory)

```json
{
  "EventQueueProvider": "InMemory",
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=eventnotify.db"
  }
}
```

### `appsettings.Development.json` (RabbitMQ)

When `ASPNETCORE_ENVIRONMENT=Development`, this config is auto-merged:

```json
{
  "EventQueueProvider": "RabbitMQ",
  "RabbitMQ": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest",
    "EventQueueName": "eventnotify.events",
    "ExchangeName": "eventnotify.events",
    "ExchangeType": "fanout",
    "AutoAck": false,
    "DequeueTimeoutSeconds": 1,
    "PrefetchCount": 10
  }
}
```

### Queue Provider Options

| Provider               | Use Case                                   | Setup                                                         |
| ---------------------- | ------------------------------------------ | ------------------------------------------------------------- |
| **InMemory** (default) | Development, testing, single instance      | No setup required                                             |
| **RabbitMQ**           | Production, distributed, durable messaging | `docker-compose up -d` + `ASPNETCORE_ENVIRONMENT=Development` |

**To switch providers:**

1. **For development with RabbitMQ:**

   ```bash
   docker-compose up -d
   export ASPNETCORE_ENVIRONMENT=Development
   dotnet run
   ```

2. **For production:**
   - Set `ASPNETCORE_ENVIRONMENT=Production`
   - Update `appsettings.json` with your RabbitMQ host
   - Start your RabbitMQ broker


## Architecture Patterns

### Event-Driven Flow

```
1. Service publishes event
   ↓
2. QueuedEventBus enqueues to RabbitMQ/InMemory
   ↓
3. API returns 201 immediately
   ↓
4. EventProcessorHostedService (background) dequeues
   ↓
5. InMemoryEventBus dispatches to all handlers in parallel
   ↓
6. Handlers execute independently
```

### Handler Pattern

```csharp
public class OrderCreatedEmailHandler : IEventHandler<OrderCreatedEvent>
{
    public async Task Handle(OrderCreatedEvent @event)
    {
        // Send confirmation email
        // Execution is independent (doesn't affect other handlers)
    }
}
```

**Benefits:**

- Single responsibility (each handler does one thing)
- Independent (failure doesn't cascade)
- Reusable (add/remove handlers without changing services)
- Testable (mock IEventBus in unit tests)

### Dependency Injection

All components are registered in `Program.cs`:

```csharp
// Database
builder.Services.AddDbContext<AppDbContext>();

// Event queue (configurable)
builder.Services.AddRabbitMQEventQueue(builder.Configuration);  // or AddInMemoryEventQueue

// Event bus
builder.Services.AddScoped<InMemoryEventBus>();
builder.Services.AddScoped<IEventBus, QueuedEventBus>();

// Background worker
builder.Services.AddHostedService<EventProcessorHostedService>();

// Handlers (scoped = new instance per event)
builder.Services.AddScoped<IEventHandler<UserCreatedEvent>, UserCreatedEmailHandler>();

// Services
builder.Services.AddScoped<UserService>();
```

## Monitoring

### RabbitMQ Management UI

```
http://localhost:15672
Username: guest
Password: guest
```

**View:**

- **Queues** → `eventnotify.events` (message count, ready, unacked)
- **Connections** → Active connections
- **Channels** → Message flow

### Application Logs

The app uses structured logging:

```
RabbitMQ event queue provider registered
Publishing event to RabbitMQ: OrderCreatedEvent
Event OrderCreatedEvent enqueued successfully
Processing dequeued event: OrderCreatedEvent
Executing handler: OrderCreatedEmailHandler
Event OrderCreatedEvent published successfully with 2 handler(s)
```

## Running Tests

### Manual Testing (via curl/Swagger)

1. Open `http://localhost:5103` in browser
2. Try operations in Swagger UI
3. Watch terminal for structured logs

### Database Verification

Events are stored in SQLite:

```bash
sqlite3 eventnotify.db

# View users
SELECT * FROM Users;

# View orders with users
SELECT o.Id, u.Email, o.Total, o.Status FROM Orders o
JOIN Users u ON o.UserId = u.Id;
```

## Scaling Considerations

### Single Instance

```bash
dotnet run
# Uses RabbitMQ or in-memory queue
# All requests processed sequentially
```

### Multiple Worker Instances

```bash
# Terminal 1: API + Worker
dotnet run

# Terminal 2: Additional worker
dotnet run

# RabbitMQ load-balances events between workers
# Each worker processes events independently
```

### Docker Deployment

```bash
docker build -t eventnotify .
docker run -e EventQueueProvider=RabbitMQ \
           -e RabbitMQ__HostName=rabbitmq \
           -p 5103:5103 \
           --network eventnotify-network eventnotify
```

## RabbitMQ Concepts

- **Exchange**: Routes messages (fanout = broadcast)
- **Queue**: Stores messages (durable = survives restart)
- **Binding**: Connects exchange to queue
- **Consumer**: Application that reads from queue
- **Publisher**: Application that sends to exchange
- **Message**: The event data (JSON serialized)
- **Acknowledgment**: Consumer confirms message processed

## Error Handling

### Service Errors

- Errors during event publication propagate to caller (fail-fast)
- API returns 500 Internal Server Error
- Error logged with full exception details

### Handler Errors

- If handler fails, exception logged but doesn't affect other handlers
- Failed message requeued to RabbitMQ (with backoff)
- Consider implementing Dead Letter Queue (DLQ) for permanent failures

### Connection Errors

- RabbitMQ connection failures logged
- Auto-recovery enabled (reconnects every 10 seconds)
- In-memory fallback available if needed

## Extending the System

### Add New Event Type

1. **Create event** (in `Events/`)

   ```csharp
   public record ShipmentCreatedEvent(int OrderId, string TrackingNumber) : IEvent;
   ```

2. **Register in type registry** (RabbitMQEventQueue.cs)

   ```csharp
   { nameof(ShipmentCreatedEvent), typeof(ShipmentCreatedEvent) }
   ```

3. **Create handler** (in `Handlers/`)

   ```csharp
   public class ShipmentCreatedNotificationHandler : IEventHandler<ShipmentCreatedEvent>
   {
       public async Task Handle(ShipmentCreatedEvent @event) { ... }
   }
   ```

4. **Register handler** (Program.cs)

   ```csharp
   builder.Services.AddScoped<IEventHandler<ShipmentCreatedEvent>, ShipmentCreatedNotificationHandler>();
   ```

5. **Publish from service**
   ```csharp
   await _eventBus.Publish(new ShipmentCreatedEvent(orderId, trackingNumber));
   ```


## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

