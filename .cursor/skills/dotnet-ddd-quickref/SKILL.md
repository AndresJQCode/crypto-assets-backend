---
name: dotnet-ddd-quickref
description: Quick reference for .NET DDD patterns, templates, and CQRS decision tree. Use for fast lookup of correct patterns and violation detection.
---

## INSTANT VIOLATION DETECTOR

### ❌ Code Smells - Reject Immediately

```csharp
// ❌ SMELL #1: Public setters on entities
public class Warehouse
{
    public string Name { get; set; } // WRONG!
}

// ❌ SMELL #2: Business logic in handler
public class Handler : IRequestHandler<Command>
{
    public async Task Handle(Command cmd)
    {
        if (cmd.Price > 1000) // Business rule in handler!
            throw new Exception();
    }
}

// ❌ SMELL #3: EF Core in Domain
namespace Domain.Entities
{
    using Microsoft.EntityFrameworkCore; // FORBIDDEN!
}

// ❌ SMELL #4: Query with EF Core
public async Task<DTO> GetWarehouse(Guid id)
{
    return await _context.Warehouses // Should use Dapper!
        .Select(...)
        .FirstAsync();
}

// ❌ SMELL #5: Anemic model
public class Order
{
    public decimal Total { get; set; }
    public List<OrderLine> Lines { get; set; }
}
// Where's the behavior?!

// ❌ SMELL #6: Generic helper
public class WarehouseHelper { } // NO!
public class DataService<T> { }  // NO!
```

---

## ✅ CORRECT PATTERNS - Copy These

### Domain Entity Template
```csharp
public class Warehouse : AggregateRoot
{
    // Strongly-typed ID
    public WarehouseId Id { get; private set; }
    
    // Value objects
    public Location Location { get; private set; }
    public Capacity Capacity { get; private set; }
    
    // Collections
    private readonly List<InventoryItem> _items = new();
    public IReadOnlyCollection<InventoryItem> Items => _items.AsReadOnly();
    
    // Factory method
    public static Warehouse Create(WarehouseId id, Location location, Capacity capacity)
    {
        // Validation
        ArgumentNullException.ThrowIfNull(location);
        
        return new Warehouse { Id = id, Location = location, Capacity = capacity };
    }
    
    // Behavior (verb)
    public void ReceiveInventory(InventoryItem item)
    {
        // Guard clauses
        if (!CanAccommodate(item.Quantity))
            throw new InsufficientCapacityException(Id);
        
        // Business logic
        _items.Add(item);
        
        // Domain event
        AddDomainEvent(new InventoryReceivedEvent(Id, item.Id));
    }
    
    // Private invariant check
    private bool CanAccommodate(Quantity quantity) { ... }
}
```

### Command Template
```csharp
// Command record
public record CreateWarehouseCommand(
    string Name,
    string Address,
    int MaxCapacity) : IRequest<Result<Guid>>;

// Validator
public class CreateWarehouseCommandValidator 
    : AbstractValidator<CreateWarehouseCommand>
{
    public CreateWarehouseCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.MaxCapacity).GreaterThan(0);
    }
}

// Handler (use primary constructor, access UnitOfWork via repository)
public class CreateWarehouseCommandHandler(
    IWarehouseRepository warehouseRepository)
    : IRequestHandler<CreateWarehouseCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CreateWarehouseCommand request,
        CancellationToken ct)
    {
        // Domain factory
        var warehouse = Warehouse.Create(
            WarehouseId.New(),
            new Location(request.Address, Coordinates.Default),
            new Capacity(request.MaxCapacity));

        // Repository
        warehouseRepository.Add(warehouse);

        // Unit of Work (NEVER inject IUnitOfWork directly, use warehouseRepository.UnitOfWork)
        await warehouseRepository.UnitOfWork.SaveEntitiesAsync(ct);

        return Result<Guid>.Success(warehouse.Id.Value);
    }
}
```

### Query Template
```csharp
// Query record
public record GetWarehousesQuery(
    int PageNumber,
    int PageSize,
    string? SearchTerm) : IRequest<PagedResult<WarehouseDto>>;

// Handler
public class GetWarehousesQueryHandler 
    : IRequestHandler<GetWarehousesQuery, PagedResult<WarehouseDto>>
{
    private readonly IWarehouseQueryService _queryService;
    
    public async Task<PagedResult<WarehouseDto>> Handle(
        GetWarehousesQuery request,
        CancellationToken ct)
    {
        return await _queryService.GetWarehousesAsync(
            request.PageNumber,
            request.PageSize,
            request.SearchTerm,
            ct);
    }
}

// Query Service (Dapper)
public class WarehouseQueryService : IWarehouseQueryService
{
    private readonly IDbConnection _connection;
    
    public async Task<PagedResult<WarehouseDto>> GetWarehousesAsync(
        int pageNumber, int pageSize, string? searchTerm, CancellationToken ct)
    {
        var sql = @"
            SELECT Id, Name, Location, CurrentCapacity, MaxCapacity
            FROM Warehouses
            WHERE (@SearchTerm IS NULL OR Name LIKE '%' + @SearchTerm + '%')
            ORDER BY Name
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY";
        
        var countSql = "SELECT COUNT(*) FROM Warehouses WHERE ...";
        
        var total = await _connection.ExecuteScalarAsync<int>(countSql, ...);
        var items = await _connection.QueryAsync<WarehouseDto>(sql, ...);
        
        return new PagedResult<WarehouseDto>(items.ToList(), total, pageNumber, pageSize);
    }
}
```

### Minimal API Template
```csharp
public static class WarehouseEndpoints
{
    public static void MapWarehouseEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/warehouses")
            .WithTags("Warehouses")
            .RequireAuthorization();
        
        // Command
        group.MapPost("/", async (
            CreateWarehouseRequest request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new CreateWarehouseCommand(
                request.Name,
                request.Address,
                request.MaxCapacity);
                
            var result = await mediator.Send(command, ct);
            
            return result.IsSuccess
                ? Results.Created($"/api/warehouses/{result.Value}", result.Value)
                : Results.BadRequest(result.Error);
        });
        
        // Query
        group.MapGet("/", async (
            [AsParameters] PaginationParams pagination,
            string? searchTerm,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var query = new GetWarehousesQuery(
                pagination.PageNumber,
                pagination.PageSize,
                searchTerm);
                
            var result = await mediator.Send(query, ct);
            return Results.Ok(result);
        });
    }
}
```

---

## LAYER CHEAT SHEET

> **NOTE**: In this project, the Api and Application layers are **merged into the `Api/` project**
> (`Api/Application/` contains Commands, Queries, DTOs, Validators, Behaviors).
> This is a deliberate design decision for simplicity. The logical separation still applies.

### Domain Layer
```
✓ Aggregates, Entities, Value Objects
✓ Domain Events
✓ Domain Exceptions
✓ Repository interfaces (contracts only)
✗ EF Core
✗ Dapper
✗ MediatR
✗ Any infrastructure
```

### Api Layer (includes Application concerns)
```
✓ Minimal API endpoints (Api/Apis/)
✓ Commands & Queries (Api/Application/Commands/, Api/Application/Queries/)
✓ Command & Query Handlers
✓ DTOs (Api/Application/Dtos/)
✓ Validators - FluentValidation (Api/Application/Validators/)
✓ MediatR Pipeline Behaviors (Api/Application/Behaviors/)
✓ Request/Response DTOs
✓ Authentication & Input validation
✗ Business logic
✗ Direct DB access (no EF Core DbContext in handlers)
```

### Infrastructure Layer
```
✓ EF Core DbContext
✓ EF Core Configurations
✓ Repository implementations
✓ External integrations
✗ Business rules
```

---

## CQRS DECISION TREE

```
Are you MODIFYING data?
├─ YES → COMMAND
│   ├─ Use EF Core
│   ├─ Load Aggregate
│   ├─ Call Domain method
│   ├─ Return Result<T>
│   └─ DO NOT return entity
│
└─ NO → QUERY
    ├─ Use Dapper
    ├─ Return DTO
    ├─ Implement pagination
    └─ DO NOT load aggregates
```

---

## VALUE OBJECT TEMPLATE

```csharp
public class Email : ValueObject
{
    public string Value { get; private set; }
    
    private Email() { } // EF Core
    
    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be empty");
            
        if (!IsValidEmail(value))
            throw new ArgumentException("Invalid email format");
            
        Value = value;
    }
    
    private static bool IsValidEmail(string email)
    {
        return email.Contains('@'); // Simplified
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
    
    public static implicit operator string(Email email) => email.Value;
}
```

---

## STRONGLY-TYPED ID TEMPLATE

```csharp
public record WarehouseId(Guid Value)
{
    public static WarehouseId New() => new(Guid.NewGuid());
    public static WarehouseId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}

// EF Core config
builder.Property(w => w.Id)
    .HasConversion(
        id => id.Value,
        value => new WarehouseId(value));
```

---

## PAGINATION TEMPLATE

```csharp
// Request params
public record PaginationParams
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

// Result wrapper
public record PagedResult<T>(
    List<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNext => PageNumber < TotalPages;
    public bool HasPrevious => PageNumber > 1;
}

// SQL Server pagination
OFFSET @Offset ROWS
FETCH NEXT @PageSize ROWS ONLY
```

---

## COMMON MISTAKES & FIXES

| Mistake | Fix |
|---------|-----|
| `public int Quantity { get; set; }` | `public Quantity Quantity { get; private set; }` |
| Handler has business logic | Move to Domain method |
| Query uses EF Core | Use Dapper |
| Command uses Dapper | Use EF Core |
| No pagination | Add PagedResult |
| Generic service | Use specific repositories |
| `UpdateWarehouse()` | Use `Rename()`, `ChangeLocation()` |
| Returning entity from command | Return `Result<Id>` |
| Domain depends on Infrastructure | Reverse dependency (DIP) |

---

## 5-SECOND ARCHITECTURE CHECK

Ask yourself:
1. **Business rule?** → Domain layer
2. **Orchestration?** → Api layer (Api/Application/ — Commands/Queries handlers)
3. **Data access?** → Infrastructure layer
4. **HTTP/API?** → Api layer (Api/Apis/ — Minimal API endpoints)
5. **Modifying state?** → Command + EF Core
6. **Reading data?** → Query

---

## RED FLAGS 🚩

If you see any of these, STOP and refactor:

- Public setters on entities
- Business logic in handlers
- EF Core in queries
- Dapper in commands
- Generic "helper" classes
- Cross-layer dependencies
- Missing pagination on lists
- No indexes on query columns
- Returning entities from commands
- Domain depending on Infrastructure

---

## DEPENDENCY GRAPH (Must Follow)

```
Api (endpoints + application logic) → Domain
         ↓
    Infrastructure
```

> **Note**: Api and Application are merged into the `Api/` project.
> The logical separation is maintained via folder structure (`Api/Apis/` for endpoints, `Api/Application/` for CQRS).

**Rule**: Arrows point INWARD. Domain has ZERO dependencies.

---

This quick reference should be your first stop for common patterns and violations.
