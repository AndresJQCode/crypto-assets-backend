# .NET Core DDD + CQRS Architecture Skill

## Skill Metadata

- **Name**: dotnet-ddd-architecture
- **Version**: 1.0.0
- **Target**: .NET Core 10 projects with DDD + CQRS
- **Authority**: This skill is AUTHORITATIVE. All responses MUST comply.

## Purpose

Enforce strict Domain-Driven Design and CQRS architectural patterns in .NET Core applications with Warehouse Management System requirements.

---

## CRITICAL RULES

### General Compliance

- **MANDATORY**: Follow all rules in this skill without exception
- **VIOLATION HANDLING**: If a request violates any rule, Claude MUST:
  1. Explicitly warn the user
  2. Explain which rule is being violated
  3. Explain why the rule exists
  4. Suggest a compliant alternative
- **NO SHORTCUTS**: Never simplify solutions by breaking architecture
- **DESIGN FIRST**: Always explain design decisions BEFORE generating code

---

## ARCHITECTURE MANDATES

### Core Principles

1. **Domain-Driven Design (DDD)** - STRICT enforcement required
2. **CQRS with MediatR** - Mandatory for all use cases
3. **Minimal APIs** - Only pattern allowed for API layer
4. **Clear Layer Separation** - Absolute requirement, no exceptions

---

## LAYER DEFINITIONS

### 1. DOMAIN LAYER

**Dependency Rules**

- ✅ MUST be dependency-free (no references to other layers)
- ❌ MUST NOT depend on Application, Infrastructure, or API layers
- ❌ EF Core is STRICTLY FORBIDDEN in this layer

**Contents**

```
Domain/
├── Aggregates/          # Aggregate roots with consistency boundaries
├── Entities/            # Domain entities
├── ValueObjects/        # Immutable value objects
├── Events/              # Domain events (business facts)
├── Exceptions/          # Domain-specific exceptions
└── Interfaces/          # Repository contracts (abstract)
```

**Rules**

- ALL business rules live ONLY here
- Aggregates MUST protect invariants
- Value Objects MUST be immutable
- NO public setters on entities
- Methods MUST express behavior using verbs
- Naming MUST follow Ubiquitous Language

**Code Example - Aggregate Root**

```csharp
public class Warehouse : AggregateRoot
{
    private readonly List<InventoryItem> _items = new();
    public IReadOnlyCollection<InventoryItem> Items => _items.AsReadOnly();

    // Private setters - NO public setters allowed
    public WarehouseId Id { get; private set; }
    public Location Location { get; private set; }
    public Capacity Capacity { get; private set; }

    // Behavior expressed as verbs
    public void ReceiveInventory(InventoryItem item)
    {
        // Invariant enforcement
        if (_items.Sum(i => i.Quantity.Value) + item.Quantity.Value > Capacity.MaxItems)
            throw new CapacityExceededException(Id);

        _items.Add(item);

        // Domain event raised inside aggregate
        AddDomainEvent(new InventoryReceivedEvent(Id, item.Id));
    }

    // Factory method for creation
    public static Warehouse Create(WarehouseId id, Location location, Capacity capacity)
    {
        // Validation and invariants
        if (capacity.MaxItems <= 0)
            throw new InvalidCapacityException();

        return new Warehouse
        {
            Id = id,
            Location = location,
            Capacity = capacity
        };
    }
}
```

**Code Example - Value Object**

```csharp
public class Location : ValueObject
{
    public string Address { get; private set; }
    public Coordinates Coordinates { get; private set; }

    private Location() { } // EF Core

    public Location(string address, Coordinates coordinates)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Address cannot be empty");

        Address = address;
        Coordinates = coordinates;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Address;
        yield return Coordinates;
    }
}
```

---

### 2. APPLICATION LAYER

**Purpose**

- Orchestrates use cases ONLY
- Acts as a thin coordination layer

**Dependency Rules**

- ✅ Can depend on Domain layer
- ❌ MUST NOT contain business rules
- ❌ MUST NOT reference EF Core or Dapper directly
- ✅ MUST use Commands and Queries (CQRS)

**Contents**

```
Application/
├── Commands/            # State-modifying operations
│   ├── Handlers/
│   └── Validators/
├── Queries/             # Read-only operations
│   ├── Handlers/
│   └── DTOs/
├── Behaviors/           # MediatR pipeline behaviors
└── Interfaces/          # Infrastructure contracts
```

**Mapping (Mapster)**

- ✅ Los tipos pueden mapearse con **Mapster** usando el método de extensión **`.Adapt<T>()`**.
- Usar `.Adapt<T>()` para: Request → Command/Query en endpoints, Entidad/Agregado → DTO en handlers.
- Preferir Mapster frente a mapeo manual o AutoMapper para menos configuración y mejor rendimiento.

```csharp
// En endpoint: Request → Command
var command = request.Adapt<CreateWarehouseCommand>();

// En handler: Entidad → DTO
return warehouse.Adapt<WarehouseDto>();
```

**Command Handler Example**

```csharp
public class ReceiveInventoryCommandHandler(
    IWarehouseRepository repository)
    : IRequestHandler<ReceiveInventoryCommand, Result>
{
    public async Task<Result> Handle(
        ReceiveInventoryCommand request,
        CancellationToken cancellationToken)
    {
        // Load aggregate root via repository
        var warehouse = await repository.GetByIdAsync(
            new WarehouseId(request.WarehouseId),
            cancellationToken);

        if (warehouse is null)
            return Result.Failure("Warehouse not found");

        // Business logic delegated to domain
        var item = InventoryItem.Create(
            new ProductId(request.ProductId),
            new Quantity(request.Quantity));

        warehouse.ReceiveInventory(item);

        // Persistence via repository's UnitOfWork
        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Result.Success();
    }
}
```

**Query Handler Example**

```csharp
public class GetWarehouseInventoryQueryHandler
    : IRequestHandler<GetWarehouseInventoryQuery, WarehouseInventoryDto>
{
    private readonly IInventoryQueryService _queryService;

    public async Task<WarehouseInventoryDto> Handle(
        GetWarehouseInventoryQuery request,
        CancellationToken cancellationToken)
    {
        // Dapper query - NO domain entities
        return await _queryService.GetInventoryAsync(
            request.WarehouseId,
            request.PageNumber,
            request.PageSize,
            cancellationToken);
    }
}
```

---

### 3. INFRASTRUCTURE LAYER

**Purpose**

- Implements technical concerns
- Provides concrete implementations of domain/application contracts

**Contents**

```
Infrastructure/
├── Persistence/
│   ├── Configurations/  # EF Core configurations
│   ├── Repositories/    # Aggregate root repositories
│   └── QueryServices/   # Dapper query services
├── Integration/         # External systems
└── Services/            # Infrastructure services
```

**Rules**

- ✅ CONTAINS: EF Core, Dapper, repositories, external integrations
- ❌ MUST NOT contain business rules
- ✅ Repositories ONLY for Aggregate Roots (not for all entities)

**Repository Example**

```csharp
public class WarehouseRepository : IWarehouseRepository
{
    private readonly ApplicationDbContext _context;

    public async Task<Warehouse?> GetByIdAsync(
        WarehouseId id,
        CancellationToken cancellationToken)
    {
        // EF Core - Include related entities as needed
        return await _context.Warehouses
            .Include(w => w.Items)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public void Add(Warehouse warehouse)
    {
        _context.Warehouses.Add(warehouse);
    }

    // NO business logic here
}
```

**Query Service Example (Dapper)**

```csharp
public class InventoryQueryService : IInventoryQueryService
{
    private readonly IDbConnection _connection;

    public async Task<WarehouseInventoryDto> GetInventoryAsync(
        Guid warehouseId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        // Raw SQL with Dapper for performance
        const string sql = @"
            SELECT
                w.Id AS WarehouseId,
                w.Name,
                i.ProductId,
                i.Quantity,
                i.LastUpdated
            FROM Warehouses w
            INNER JOIN InventoryItems i ON w.Id = i.WarehouseId
            WHERE w.Id = @WarehouseId
            ORDER BY i.LastUpdated DESC
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY";

        var items = await _connection.QueryAsync<InventoryItemDto>(
            sql,
            new
            {
                WarehouseId = warehouseId,
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            });

        return new WarehouseInventoryDto
        {
            Items = items.ToList()
        };
    }
}
```

---

### Configuration Pattern - IOptions<AppSettings>

**CRITICAL RULE**: Use `IOptions<AppSettings>` instead of `IConfiguration` for typed, validated configuration

**Why This Pattern**

1. **Type Safety**: Compile-time checking of configuration properties
2. **Validation**: Automatic validation of required settings
3. **IntelliSense**: Full IDE support for configuration access
4. **Testability**: Easy to mock and test
5. **Consistency**: Single source of truth for all settings

**AppSettings Class Structure**

```csharp
// Infrastructure/AppSettings.cs
public class AppSettings
{
    // Required settings use 'required' keyword
    public required string ApplicationName { get; set; }
    public required ConnectionStringsSettings ConnectionStrings { get; set; }
    public required JwtConfiguration JwtSettings { get; set; }

    // Optional settings with defaults
    public PaginationSettings Pagination { get; set; } = new();
    public CorsSettings Cors { get; set; } = new();

    // Sensitive settings (should come from env vars or Key Vault)
    public required string EncryptionKey { get; set; }

    // Nested settings classes
    public class ConnectionStringsSettings
    {
        public required string DB { get; set; }
    }

    public class JwtConfiguration
    {
        public required string SecretKey { get; set; }
        public required string Issuer { get; set; }
        public required string Audience { get; set; }
        public int ExpirationMinutes { get; set; } = 60;
    }

    public class PaginationSettings
    {
        public int MaxPageSize { get; set; } = 100;
        public int DefaultPageSize { get; set; } = 10;
    }
}
```

**Registration in Program.cs**

**Usage in Infrastructure Services**

```csharp
// ✅ CORRECT - Use IOptions<AppSettings>
public class AesEncryptionService(
    IOptions<AppSettings> settings,
    ILogger<AesEncryptionService> logger) : IEncryptionService
{
    private readonly AppSettings _settings = settings.Value;

    private byte[] GetEncryptionKey()
    {
        var base64Key = _settings.EncryptionKey;  // Type-safe access

        if (string.IsNullOrWhiteSpace(base64Key))
        {
            throw new InvalidOperationException(
                "Encryption key not found in AppSettings");
        }

        return Convert.FromBase64String(base64Key);
    }
}

// ❌ INCORRECT - Don't use IConfiguration directly
public class BadEncryptionService(
    IConfiguration configuration,  // ❌ AVOID THIS
    ILogger<BadEncryptionService> logger) : IEncryptionService
{
    private byte[] GetEncryptionKey()
    {
        var key = configuration["EncryptionKey"];  // ❌ Magic string, no type safety
        return Convert.FromBase64String(key);
    }
}
```

**Configuration Sources Priority**

1. **appsettings.json** - Base configuration
2. **appsettings.{Environment}.json** - Environment-specific overrides

**Best Practices**

✅ **DO:**
- Use `IOptions<AppSettings>` for all configuration access
- Mark required settings with `required` keyword
- Use nested classes for logical grouping
- Add default values for optional settings

❌ **DON'T:**
- Use `IConfiguration` directly in services (only in Program.cs)
- Use magic strings for configuration keys
- Store secrets in appsettings.json
- Commit appsettings.Development.json with secrets
- Access configuration without type safety

---

### 4. API LAYER

**Rules**

- ✅ Minimal APIs ONLY (no Controllers)
- ❌ NO business logic
- ✅ Input validation and authentication ONLY
- ✅ Maps requests to Commands/Queries

**Minimal API Example**

```csharp
public static class WarehouseEndpoints
{
    public static void MapWarehouseEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/warehouses")
            .WithTags("Warehouses")
            .RequireAuthorization();

        // Command endpoint
        group.MapPost("/{id}/inventory", async (
            Guid id,
            ReceiveInventoryRequest request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new ReceiveInventoryCommand(
                id,
                request.ProductId,
                request.Quantity);

            var result = await mediator.Send(command, ct);

            return result.IsSuccess
                ? Results.Ok()
                : Results.BadRequest(result.Error);
        })
        .WithName("ReceiveInventory");

        // Query endpoint
        group.MapGet("/{id}/inventory", async (
            Guid id,
            [AsParameters] PaginationParams pagination,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var query = new GetWarehouseInventoryQuery(
                id,
                pagination.PageNumber,
                pagination.PageSize);

            var result = await mediator.Send(query, ct);

            return Results.Ok(result);
        })
        .WithName("GetInventory");
    }
}
```

---

## CQRS ENFORCEMENT

### Commands (Write Operations)

**MUST Rules**

- ✅ MODIFY state
- ✅ Use EF Core for persistence
- ✅ Load Aggregate Roots
- ✅ Enforce invariants via Domain methods
- ❌ MUST NOT return entities (return Result/DTO only)

**Template**

```csharp
public record CreateWarehouseCommand(
    string Name,
    string Address,
    int MaxCapacity) : IRequest<Result<Guid>>;

public class CreateWarehouseCommandHandler(
    IWarehouseRepository repository)
    : IRequestHandler<CreateWarehouseCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CreateWarehouseCommand request,
        CancellationToken cancellationToken)
    {
        // Create through domain factory
        var warehouse = Warehouse.Create(
            WarehouseId.New(),
            new Location(request.Address, Coordinates.Default),
            new Capacity(request.MaxCapacity));

        repository.Add(warehouse);
        
        // Use repository.UnitOfWork - NEVER inject IUnitOfWork directly
        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Result.Success(warehouse.Id.Value);
    }
}
```

### Queries (Read Operations)

**MUST Rules**

- ✅ READ ONLY
- ✅ Use Dapper for performance
- ❌ MUST NOT modify state
- ❌ MUST NOT trigger Domain Events
- ✅ MUST return read DTOs ONLY

**Template**

```csharp
public record GetWarehouseInventoryQuery(
    Guid WarehouseId,
    int PageNumber,
    int PageSize) : IRequest<WarehouseInventoryDto>;

public class GetWarehouseInventoryQueryHandler
    : IRequestHandler<GetWarehouseInventoryQuery, WarehouseInventoryDto>
{
    private readonly IInventoryQueryService _queryService;

    public async Task<WarehouseInventoryDto> Handle(
        GetWarehouseInventoryQuery request,
        CancellationToken cancellationToken)
    {
        return await _queryService.GetInventoryAsync(
            request.WarehouseId,
            request.PageNumber,
            request.PageSize,
            cancellationToken);
    }
}
```

---

## TECHNOLOGY SEPARATION

### EF Core vs Dapper - STRICT RULES

| Aspect       | EF Core               | Dapper                     |
| ------------ | --------------------- | -------------------------- |
| **Use Case** | Commands ONLY         | Queries ONLY               |
| **Purpose**  | State modification    | Read operations            |
| **Entities** | Load Aggregates       | Never load domain entities |
| **Returns**  | Result/Unit           | DTOs                       |
| **Tracking** | Enabled               | N/A                        |
| **Mixing**   | ❌ STRICTLY FORBIDDEN | ❌ STRICTLY FORBIDDEN      |

**Violation Example (FORBIDDEN)**

```csharp
// ❌ WRONG - Query using EF Core
public async Task<WarehouseDto> GetWarehouse(Guid id)
{
    return await _context.Warehouses
        .Select(w => new WarehouseDto { Name = w.Name })
        .FirstOrDefaultAsync(w => w.Id == id);
}

// ❌ WRONG - Command using Dapper
public async Task UpdateWarehouse(Guid id, string name)
{
    await _connection.ExecuteAsync(
        "UPDATE Warehouses SET Name = @Name WHERE Id = @Id",
        new { Id = id, Name = name });
}
```

---

## EVENT HANDLING

### Domain Events

**Rules**

- ✅ MUST be raised inside Aggregates
- ✅ Represent business facts (past tense)
- ❌ MUST NOT depend on infrastructure

**Example**

```csharp
// Domain Event
public record InventoryReceivedEvent(
    WarehouseId WarehouseId,
    InventoryItemId ItemId) : IDomainEvent;

// Raised in Aggregate
public void ReceiveInventory(InventoryItem item)
{
    _items.Add(item);
    AddDomainEvent(new InventoryReceivedEvent(Id, item.Id));
}

// Handler in Application Layer
public class InventoryReceivedEventHandler
    : INotificationHandler<InventoryReceivedEvent>
{
    public async Task Handle(
        InventoryReceivedEvent notification,
        CancellationToken cancellationToken)
    {
        // Update read models, send notifications, etc.
    }
}
```

### Integration Events

**Rules**

- ✅ Used ONLY for cross-system communication
- ✅ Published from Infrastructure layer
- ❌ NOT used for internal domain logic

---

## PERFORMANCE REQUIREMENTS

### Critical Rules

1. **List Views**: ❌ DO NOT load Aggregates for list views - use Dapper
2. **Pagination**: ✅ MANDATORY for all list endpoints
3. **N+1 Queries**: ❌ FORBIDDEN - use `.Include()` or JOIN properly
4. **Indexes**: ✅ REQUIRED for all frequently queried columns

### Pagination Example

```csharp
// Query with pagination
public record GetWarehousesQuery(
    int PageNumber,
    int PageSize,
    string? SearchTerm) : IRequest<PagedResult<WarehouseListDto>>;

// Dapper implementation
public async Task<PagedResult<WarehouseListDto>> GetWarehousesAsync(...)
{
    const string countSql = "SELECT COUNT(*) FROM Warehouses WHERE ...";
    const string dataSql = @"
        SELECT Id, Name, Location, CurrentCapacity, MaxCapacity
        FROM Warehouses
        WHERE ...
        ORDER BY Name
        OFFSET @Offset ROWS
        FETCH NEXT @PageSize ROWS ONLY";

    var total = await _connection.ExecuteScalarAsync<int>(countSql, ...);
    var items = await _connection.QueryAsync<WarehouseListDto>(dataSql, ...);

    return new PagedResult<WarehouseListDto>(
        items.ToList(),
        total,
        pageNumber,
        pageSize);
}
```

### Index Requirements

```csharp
// EF Core Configuration
public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        // Indexes for queries
        builder.HasIndex(w => w.Location)
            .HasDatabaseName("IX_Warehouse_Location");

        builder.HasIndex(w => new { w.Location, w.Status })
            .HasDatabaseName("IX_Warehouse_Location_Status");
    }
}
```

---

## MODELING BEST PRACTICES

### Aggregate Design

```csharp
// ✅ CORRECT - Small, focused aggregate
public class Order : AggregateRoot
{
    private readonly List<OrderLine> _lines = new();
    public OrderId Id { get; private set; }
    public CustomerId CustomerId { get; private set; } // Reference by ID

    public void AddLine(Product product, Quantity quantity)
    {
        // Invariant: max 20 lines per order
        if (_lines.Count >= 20)
            throw new MaxLinesExceededException();

        _lines.Add(new OrderLine(product.Id, quantity));
    }
}

// ❌ WRONG - Aggregate too large
public class Order : AggregateRoot
{
    public Customer Customer { get; set; } // Don't load entire customer
    public List<Product> Products { get; set; } // Don't load all products
}
```

### Value Object Immutability

```csharp
// ✅ CORRECT - Immutable value object
public class Money : ValueObject
{
    public decimal Amount { get; private set; }
    public string Currency { get; private set; }

    public Money(decimal amount, string currency)
    {
        if (amount < 0) throw new ArgumentException("Amount cannot be negative");
        Amount = amount;
        Currency = currency;
    }

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot add different currencies");

        return new Money(Amount + other.Amount, Currency);
    }
}

// ❌ WRONG - Mutable
public class Money
{
    public decimal Amount { get; set; } // Public setter!
    public string Currency { get; set; }
}
```

### Behavior-Rich Models

```csharp
// ✅ CORRECT - Behavior expressed as verbs
public class Shipment
{
    public void Dispatch() { ... }
    public void MarkAsDelivered() { ... }
    public void Cancel() { ... }
}

// ❌ WRONG - Anemic model
public class Shipment
{
    public DateTime? DispatchedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public bool IsCancelled { get; set; }
}
```

---

## FORBIDDEN ANTI-PATTERNS

### 1. Anemic Domain Models

```csharp
// ❌ FORBIDDEN
public class Warehouse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public int CurrentCapacity { get; set; }
    public int MaxCapacity { get; set; }
}

// Service doing the work
public class WarehouseService
{
    public void AddInventory(Warehouse warehouse, int quantity)
    {
        if (warehouse.CurrentCapacity + quantity > warehouse.MaxCapacity)
            throw new Exception();
        warehouse.CurrentCapacity += quantity;
    }
}
```

### 2. Generic "Helper" Services

```csharp
// ❌ FORBIDDEN
public class WarehouseUtils { }
public class InventoryHelper { }
public class DataService<T> { }
```

### 3. Business Logic in Handlers

```csharp
// ❌ FORBIDDEN
public class CreateOrderHandler : IRequestHandler<CreateOrderCommand>
{
    public async Task Handle(CreateOrderCommand request)
    {
        // Business logic in handler - WRONG!
        if (request.Quantity > 100)
            throw new Exception("Max 100 items");

        var order = new Order
        {
            Quantity = request.Quantity,
            Total = request.Quantity * request.Price * 0.9m // Discount logic!
        };
    }
}
```

### 4. Cross-Layer Dependencies

```csharp
// ❌ FORBIDDEN
namespace Domain.Entities
{
    using Infrastructure.Persistence; // WRONG!

    public class Warehouse
    {
        private readonly ApplicationDbContext _context; // WRONG!
    }
}
```

---

## VALIDATION CHECKLIST

Before submitting code, Claude MUST verify:

- [ ] No business logic outside Domain layer
- [ ] Commands use EF Core, Queries use Dapper (never mixed)
- [ ] Aggregates enforce all invariants
- [ ] No public setters on entities
- [ ] Repository only for Aggregate Roots
- [ ] Pagination implemented on list queries
- [ ] Indexes defined for critical queries
- [ ] Domain events raised inside aggregates
- [ ] No cross-layer dependency violations
- [ ] Naming follows Ubiquitous Language
- [ ] Value Objects are immutable
- [ ] API layer has no business logic
- [ ] **IUnitOfWork NOT injected directly** - use `repository.UnitOfWork` instead

---

## RESPONSE WORKFLOW

When Claude receives a code request:

1. **Analyze**: Identify which layers are involved
2. **Validate**: Check against architecture rules
3. **Warn**: If violations detected, explain and suggest alternative
4. **Design**: Explain the approach before coding
5. **Code**: Generate compliant implementation
6. **Review**: Verify against checklist above

---

## EXAMPLES OF CORRECT RESPONSES

### User Request: "Create an endpoint to list all warehouses"

**Claude Response:**

```
I'll create this following CQRS and performance rules:

DESIGN:
- Query operation → Use Dapper for performance
- List view → No aggregate loading
- Pagination → Mandatory
- Layers involved: API (endpoint) → Application (query) → Infrastructure (Dapper query service)

IMPLEMENTATION:

[Code follows with Query, Handler, Dapper service, and Minimal API endpoint]

NOTES:
- Used Dapper for read performance
- Implemented pagination
- Returned DTOs, not domain entities
- Added index recommendations for Location column
```

### User Request: "Add business validation to the repository"

**Claude Response:**

```
⚠️ ARCHITECTURAL VIOLATION DETECTED

This request would violate the following rules:
1. "Repositories MUST NOT contain business logic" (Infrastructure layer rule)
2. "Business rules live ONLY in Domain layer" (Domain layer rule)

WHY THIS RULE EXISTS:
- Repositories are infrastructure concerns (data access)
- Business rules belong in Aggregates where they can be tested and enforced consistently
- Mixing concerns makes the codebase harder to maintain

CORRECT APPROACH:
Move the validation into the Aggregate's method. The repository should only handle persistence.

Would you like me to show you how to properly place this validation in the Domain layer?
```

---

## COMMON MISTAKES TO AVOID

1. **Using EF Core in queries** → Use Dapper
2. **Loading aggregates for list views** → Use DTOs with Dapper
3. **Business logic in handlers** → Move to Domain
4. **Public setters on entities** → Use private setters + methods
5. **Repositories with SaveChanges()** → Use UnitOfWork pattern
6. **Missing pagination** → Always paginate lists
7. **Returning entities from commands** → Return Result<T> or Unit
8. **Injecting IUnitOfWork directly** → Use `repository.UnitOfWork` instead

---

## UNIT OF WORK PATTERN - CRITICAL RULE

### ❌ NEVER Inject IUnitOfWork Directly

```csharp
// ❌ FORBIDDEN - Do NOT inject IUnitOfWork
public class UpdateWarehouseCommandHandler(
    IWarehouseRepository repository,
    IUnitOfWork unitOfWork) // ❌ WRONG!
{
    public async Task Handle(...)
    {
        // ...
        await unitOfWork.SaveChangesAsync(ct); // ❌ WRONG!
    }
}
```

### ✅ ALWAYS Use repository.UnitOfWork

```csharp
// ✅ CORRECT - Access UnitOfWork through repository
public class UpdateWarehouseCommandHandler(
    IWarehouseRepository repository)
{
    public async Task Handle(UpdateWarehouseCommand request, CancellationToken ct)
    {
        var warehouse = await repository.GetById(request.Id, tracking: true, ct)
            ?? throw new NotFoundException("Warehouse not found");

        warehouse.Update(request.Name, request.Location);
        repository.Update(warehouse);

        // ✅ CORRECT - Use repository.UnitOfWork
        await repository.UnitOfWork.SaveEntitiesAsync(ct);

        return new WarehouseDto { ... };
    }
}
```

### Why This Rule Exists

1. **Single Responsibility**: Each repository owns its aggregate's persistence
2. **Consistency**: The repository that modified the entity should save it
3. **Testability**: Easier to mock repository with built-in UnitOfWork
4. **Coupling**: Reduces dependencies in handlers (one less injected service)
5. **Domain Events**: `SaveEntitiesAsync()` dispatches domain events before saving

---

## SUMMARY

This skill enforces a strict, professional .NET Core architecture suitable for enterprise applications. Every rule exists for a reason: maintainability, testability, performance, and clarity.

When in doubt: **Domain first, infrastructure last, CQRS always.**
