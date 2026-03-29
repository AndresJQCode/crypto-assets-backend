---
name: dotnet-code-validation
description: Enforces code quality, naming conventions, editorconfig, and validation patterns for .NET. Use when reviewing or generating C# code, or when the user asks about conventions.
---

## CODE REVIEW CHECKLIST

When reviewing or generating code, Claude MUST verify ALL items below. C# style MUST follow the project `.editorconfig` (see **EDITORCONFIG / CODE STYLE** section).

### Domain Layer

```
✓ No dependencies on other layers
✓ No EF Core references
✓ All entities have private setters
✓ Methods use verb names (e.g., ReceiveInventory, not SetInventory)
✓ Invariants validated in domain methods
✓ Value Objects are immutable
✓ Aggregates raise domain events
✓ Factory methods for complex creation
✓ Domain exceptions for business rule violations
✓ URI/URL properties use System.Uri, not string (CA1054/CA1055/CA1056)
```

### Application Layer

```
✓ Commands and Queries separated
✓ Handlers are thin (orchestration only)
✓ No business rules in handlers
✓ Commands return Result<T> or Unit
✓ Queries return DTOs only
✓ Validators use FluentValidation
✓ No EF Core or Dapper in handlers directly
```

### Infrastructure Layer

```
✓ Repositories only for Aggregate Roots
✓ EF Core configurations in separate files
✓ Dapper queries return DTOs
✓ No business logic
✓ Proper connection disposal
✓ Indexes defined in configurations
```

### API Layer

```
✓ Minimal APIs only (no Controllers)
✓ Endpoints grouped logically
✓ Input validation only
✓ Maps to Commands/Queries
✓ Proper HTTP status codes
✓ No business logic
```

### API Assembly Visibility

**Because an application's API assembly is not typically referenced from outside the assembly, types can be made internal.**

- Prefer `internal` for types that are only used within the API project (constants, helpers, DTOs used only in endpoints).
- Use `public` only when a type must be consumed by another assembly (e.g. shared contracts, libraries).
- Apply to: static constant classes, endpoint helpers, API-specific DTOs, permission/authorization constants.

**✅ CORRECT**

```csharp
internal static class PermissionConstants
{
    internal static class Resources { ... }
    internal static class Actions { ... }
}
```

**❌ AVOID** (unnecessary public surface)

```csharp
public static class PermissionConstants { ... }  // Only used inside Api assembly
```

---

## EDITORCONFIG / CODE STYLE (aligned with .editorconfig)

**Reference:** Project root `.editorconfig`. When generating or reviewing C# code, Claude MUST follow these rules.

### Indentation and layout

- **Indent**: 4 spaces (no tabs). `indent_style = space`, `indent_size = 4`, `tab_width = 4`.
- **Line endings**: LF. `end_of_line = lf`.
- **Final newline**: Every file ends with a newline. `insert_final_newline = true`.
- **Usings**: `using` outside namespace. `csharp_using_directive_placement = outside_namespace`.
- **Namespaces**: File-scoped namespaces. `csharp_style_namespace_declarations = file_scoped`.

```csharp
using System;
using Domain.SeedWork;

namespace Api.Constants;

internal static class PermissionConstants { }
```

### Modifiers and accessibility

- **Explicit accessibility**: Use explicit modifiers for non-interface members. `dotnet_style_require_accessibility_modifiers = for_non_interface_members`.
- **Modifier order**: `public, private, protected, internal, static, extern, new, virtual, abstract, sealed, override, readonly, unsafe, volatile, async`. `csharp_preferred_modifier_order`.

```csharp
public sealed class Handler { }
private readonly List<int> _items;
internal static class Helpers { }
```

### No qualification with this/Me

- Do not use `this.` for events, fields, methods, or properties. `dotnet_style_qualification_for_* = false:error`.

**✅** `Name = value;`  
**❌** `this.Name = value;`

### Predefined types vs BCL

- Prefer language keywords for locals, parameters, members: `int`, `string`, `bool`, `object`, etc. `dotnet_style_predefined_type_for_locals_parameters_members = true:error`.
- Prefer keyword for member access when applicable. `dotnet_style_predefined_type_for_member_access = true:error`.

**✅** `int count;` `string name;`  
**❌** `Int32 count;` `String name;`

### URI properties (CA1055)

- Properties, parameters, and return values that represent URIs MUST use `System.Uri` instead of `string`.
- This applies to any member whose name contains: `Uri`, `Url`, `Urn`, `Link`, `Href`, `Endpoint`, `IconUrl`, `LogoUrl`, `ImageUrl`, `BaseUrl`, `WebhookUrl`, `CallbackUrl`, `RedirectUrl`, or similar URL-related names.
- This rule enforces **CA1055** (URI-like return values should not be strings), **CA1054** (URI parameters should not be strings), and **CA1056** (URI properties should not be strings).

**✅ CORRECT**

```csharp
public Uri IconUrl { get; private set; }
public Uri? WebhookUrl { get; set; }
public Uri BaseAddress { get; init; }

public void SetCallback(Uri callbackUrl) { }
public Uri GetRedirectUrl() => new("https://example.com");
```

**❌ AVOID**

```csharp
public string IconUrl { get; private set; }   // CA1056 - Use Uri
public string? WebhookUrl { get; set; }        // CA1056 - Use Uri
public string BaseAddress { get; init; }       // CA1056 - Use Uri

public void SetCallback(string callbackUrl) { } // CA1054 - Use Uri
public string GetRedirectUrl() => "https://..."; // CA1055 - Use Uri
```

**EF Core Configuration for Uri properties**

```csharp
// Convert Uri to string for database storage
builder.Property(e => e.IconUrl)
    .HasConversion(
        uri => uri.ToString(),
        str => new Uri(str))
    .HasMaxLength(2048);

// Nullable Uri
builder.Property(e => e.WebhookUrl)
    .HasConversion(
        uri => uri != null ? uri.ToString() : null,
        str => str != null ? new Uri(str) : null)
    .HasMaxLength(2048);
```

**JSON Serialization**: `System.Uri` serializes/deserializes automatically as a string in `System.Text.Json` and `Newtonsoft.Json`.

### var

- **No var** for built-in types; use explicit type. `csharp_style_var_for_built_in_types = false:error`.
- **No var** elsewhere by default. `csharp_style_var_elsewhere = false:error`.
- **Use var** when type is apparent (e.g. right-hand side). `csharp_style_var_when_type_is_apparent = true:error`.

**✅** `List<int> list = new();` `string s = GetName();`  
**✅** `var list = new List<int>();` (type apparent)

### Expression-bodied members

- Prefer expression-bodied for: accessors, constructors, indexers, lambdas, local functions, methods, operators, properties. All set to `true:error`.

**✅** `public string Name { get; set; } = "";`  
**✅** `public int Count => _items.Count;`  
**✅** `public void Clear() => _items.Clear();`

### Braces and blocks

- Always use braces for control flow. `csharp_prefer_braces = true:error`.
- Prefer simple `using` (no block when not needed). `csharp_prefer_simple_using_statement = true:error`.

**✅** `if (x) { Do(); }`  
**❌** `if (x) Do();`

### Parentheses

- No unnecessary parentheses in arithmetic, relational, and other binary operators. `dotnet_style_parentheses_* = never_if_unnecessary:error`.

### Expression and assignment preferences

- Coalesce: `x ?? y`. `dotnet_style_coalesce_expression = true:error`.
- Collection/object initializers. `dotnet_style_collection_initializer = true:error`, `dotnet_style_object_initializer = true:error`.
- Compound assignment: `x += 1` not `x = x + 1`. `dotnet_style_prefer_compound_assignment = true:error`.
- Conditional expression over assignment/return. `dotnet_style_prefer_conditional_expression_over_assignment/return = true:error`.
- Inferred tuple and anonymous type member names. `dotnet_style_prefer_inferred_tuple_names = true:error`, etc.
- Null check: `is null` / `is not null` instead of `== null` / `ReferenceEquals`. `dotnet_style_prefer_is_null_check_over_reference_equality_method = true:error`.
- Null propagation: prefer `?.` when applicable. `dotnet_style_null_propagation = true:error`.
- Prefer auto-properties when no extra logic. `dotnet_style_prefer_auto_properties = true:warning`.
- Readonly fields when never reassigned. `dotnet_style_readonly_field = true:error`.
- Unused parameters: mark or remove. `dotnet_code_quality_unused_parameters = all:error`.
- Unused value assignment/expression: prefer discard `_`. `csharp_style_unused_value_*_preference = discard_variable`.
- Conditional delegate call: `action?.Invoke()` not `if (action != null) action()`. `csharp_style_conditional_delegate_call = true:error`.

### Pattern matching and C# style

- Pattern matching over `as` with null check and over `is` with cast. `csharp_style_pattern_matching_over_* = true:error`.
- Prefer switch expression. `csharp_style_prefer_switch_expression = true:error`.
- Static local functions when possible. `csharp_prefer_static_local_function = true:error`.
- Simple default expression. `csharp_prefer_simple_default_expression = true:error`.
- Inlined variable declaration (e.g. out var). `csharp_style_inlined_variable_declaration = true:error`.
- Prefer pattern over anonymous function where applicable. `csharp_style_pattern_local_over_anonymous_function = true:error`.
- Prefer primary constructors when appropriate. `csharp_style_prefer_primary_constructors = true:suggestion`.
- Prefer `not` pattern. `csharp_style_prefer_not_pattern = true:suggestion`.
- Prefer simplified boolean expressions. `dotnet_style_prefer_simplified_boolean_expressions = true:suggestion`.
- Prefer simplified string interpolation. `dotnet_style_prefer_simplified_interpolation = true:suggestion`.
- Prefer collection expressions when types match. `dotnet_style_prefer_collection_expression = when_types_loosely_match:suggestion`.
- Namespace match folder. `dotnet_style_namespace_match_folder = true:suggestion`.
- Operator when wrapping: at beginning of line. `dotnet_style_operator_placement_when_wrapping = beginning_of_line`.

### Organize usings

- System directives first; no separate groups. `dotnet_sort_system_directives_first = true`, `dotnet_separate_import_directive_groups = false`.

### Naming (aligned with .editorconfig naming rules)

- **Interfaces**: PascalCase, prefix `I`. `dotnet_naming_rule.interface_should_be_begins_with_i`.
- **Types**: PascalCase. `dotnet_naming_rule.types_should_be_pascal_case`.
- **Non-field members** (properties, events, methods): PascalCase. `dotnet_naming_rule.non_field_members_should_be_pascal_case`.

**✅** `public interface IRepository { }`  
**✅** `public class UserService { }`  
**✅** `public string Name { get; }`

### Formatting (from .editorconfig)

**New lines:**
- New line before `catch`, `else`, `finally`. `csharp_new_line_before_catch/else/finally = true`.
- New line before open brace for all. `csharp_new_line_before_open_brace = all`.
- New line between query expression clauses. `csharp_new_line_between_query_expression_clauses = true`.
- No new line before members in anonymous types / object initializers (single-line style allowed).

**Indentation:**
- Indent block contents; do not indent braces. `csharp_indent_block_contents = true`, `csharp_indent_braces = false`.
- Indent case contents and switch labels. `csharp_indent_case_contents = true`, `csharp_indent_switch_labels = true`.

**Spaces:**
- After comma; around binary operators; after keywords in control flow; after semicolon in for.
- No space: after cast, before/after dot, inside parentheses/square brackets, between method name and `(`, between empty `()`.
- Before comma/colon in inheritance: no space before comma; space before/after colon in inheritance clause.

**Wrapping:**
- Preserve single-line blocks. `csharp_preserve_single_line_blocks = true`.
- Do not preserve single-line statements (break when needed). `csharp_preserve_single_line_statements = false`.

### Exceptions in .editorconfig (do not enforce in skill)

- Migrations: `generated_code = true`, analyzers disabled.
- StyleCop/CA/IDE diagnostics explicitly set to `none` in .editorconfig (e.g. SA1600, CA1031, CA1062, CA1848, CA2007) remain team choice; follow project defaults.

---

## NAMING CONVENTIONS

### Ubiquitous Language

All names MUST come from the business domain:

**✅ CORRECT**

```csharp
public class Warehouse { }
public class InventoryItem { }
public class ShipmentOrder { }
public void ReceiveInventory() { }
public void DispatchShipment() { }
```

**❌ INCORRECT**

```csharp
public class WarehouseData { }
public class ItemInfo { }
public class OrderRecord { }
public void UpdateInventory() { } // Too technical
public void ProcessShipment() { } // Too generic
```

### File Organization

```
Domain/
├── Warehouses/
│   ├── Warehouse.cs              # Aggregate Root
│   ├── InventoryItem.cs          # Entity
│   ├── Location.cs               # Value Object
│   ├── WarehouseId.cs            # Strongly-typed ID
│   ├── Events/
│   │   └── InventoryReceivedEvent.cs
│   └── Exceptions/
│       └── CapacityExceededException.cs
```

### Command/Query Naming

```csharp
// Commands: Verb + Noun + "Command"
public record CreateWarehouseCommand(...) : IRequest<Result>;
public record ReceiveInventoryCommand(...) : IRequest<Result>;
public record DispatchShipmentCommand(...) : IRequest<Result>;

// Queries: Get/List + Noun + "Query"
public record GetWarehouseQuery(...) : IRequest<WarehouseDto>;
public record ListWarehousesQuery(...) : IRequest<PagedResult<WarehouseListDto>>;
public record SearchInventoryQuery(...) : IRequest<List<InventoryDto>>;
```

### Event Naming

```csharp
// Domain Events: Past tense (fact that occurred)
public record InventoryReceivedEvent(...);
public record ShipmentDispatchedEvent(...);
public record WarehouseCreatedEvent(...);

// Integration Events: Past tense + "IntegrationEvent"
public record InventoryReceivedIntegrationEvent(...);
```

---

## COMMON PATTERNS

### Result Pattern (Error Handling)

**Implementation**

```csharp
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; }

    protected Result(bool isSuccess, string error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, string.Empty);
    public static Result Failure(string error) => new(false, error);
}

public class Result<T> : Result
{
    public T Value { get; }

    private Result(T value, bool isSuccess, string error)
        : base(isSuccess, error)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(value, true, string.Empty);
    public static new Result<T> Failure(string error) => new(default!, false, error);
}
```

**Usage in Commands**

```csharp
public async Task<Result<Guid>> Handle(
    CreateWarehouseCommand request,
    CancellationToken cancellationToken)
{
    try
    {
        var warehouse = Warehouse.Create(...);
        warehouseRepository.Add(warehouse);
        // NEVER inject IUnitOfWork directly, use warehouseRepository.UnitOfWork
        await warehouseRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Result<Guid>.Success(warehouse.Id.Value);
    }
    catch (DomainException ex)
    {
        return Result<Guid>.Failure(ex.Message);
    }
}
```

### Strongly-Typed IDs

**Pattern**

```csharp
public record WarehouseId(Guid Value)
{
    public static WarehouseId New() => new(Guid.NewGuid());
    public static WarehouseId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}

// EF Core Configuration
builder.Property(w => w.Id)
    .HasConversion(
        id => id.Value,
        value => new WarehouseId(value))
    .ValueGeneratedNever();
```

**Benefits**

- Type safety (can't mix WarehouseId with ProductId)
- Self-documenting code
- Refactoring support

### Specification Pattern

**Use Case**: Complex domain queries

**Implementation**

```csharp
public abstract class Specification<T>
{
    public abstract Expression<Func<T, bool>> ToExpression();

    public bool IsSatisfiedBy(T entity)
    {
        var predicate = ToExpression().Compile();
        return predicate(entity);
    }
}

public class AvailableWarehouseSpecification : Specification<Warehouse>
{
    public override Expression<Func<Warehouse, bool>> ToExpression()
    {
        return w => w.Status == WarehouseStatus.Active
                 && w.CurrentCapacity < w.MaxCapacity;
    }
}

// Usage in Domain
public class Warehouse
{
    public bool IsAvailableForInventory()
    {
        var spec = new AvailableWarehouseSpecification();
        return spec.IsSatisfiedBy(this);
    }
}
```

### Unit of Work Pattern

**Interface**

```csharp
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

**Implementation**

```csharp
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly IDomainEventDispatcher _eventDispatcher;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        // Dispatch domain events before saving
        await _eventDispatcher.DispatchEventsAsync(_context, cancellationToken);

        return await _context.SaveChangesAsync(cancellationToken);
    }
}
```

---

## VALIDATION PATTERNS

### FluentValidation for Commands

```csharp
public class CreateWarehouseCommandValidator
    : AbstractValidator<CreateWarehouseCommand>
{
    public CreateWarehouseCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.MaxCapacity)
            .GreaterThan(0)
            .LessThanOrEqualTo(100000);

        RuleFor(x => x.Address)
            .NotEmpty()
            .MaximumLength(500);
    }
}

// MediatR Pipeline Behavior
public class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);
        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(result => result.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Any())
            throw new ValidationException(failures);

        return await next();
    }
}
```

### Domain Validation

```csharp
public class Warehouse : AggregateRoot
{
    public void ReceiveInventory(InventoryItem item)
    {
        // Guard clauses for domain invariants
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        if (Status != WarehouseStatus.Active)
            throw new WarehouseNotActiveException(Id);

        if (!CanAccommodate(item.Quantity))
            throw new InsufficientCapacityException(Id, item.Quantity);

        // Business logic
        _items.Add(item);
        AddDomainEvent(new InventoryReceivedEvent(Id, item.Id));
    }

    private bool CanAccommodate(Quantity quantity)
    {
        var currentTotal = _items.Sum(i => i.Quantity.Value);
        return currentTotal + quantity.Value <= Capacity.MaxItems;
    }
}
```

---

## PERFORMANCE PATTERNS

### Pagination Helper

```csharp
public record PagedResult<T>
{
    public List<T> Items { get; init; }
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public PagedResult(List<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}

// Extension method for IQueryable
public static class QueryableExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>(items, totalCount, pageNumber, pageSize);
    }
}
```

### Query Object Pattern (Dapper)

```csharp
public class GetWarehouseInventoryQuery
{
    private readonly IDbConnection _connection;

    public async Task<PagedResult<InventoryItemDto>> ExecuteAsync(
        Guid warehouseId,
        int pageNumber,
        int pageSize,
        string? searchTerm,
        CancellationToken cancellationToken)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@WarehouseId", warehouseId);
        parameters.Add("@Offset", (pageNumber - 1) * pageSize);
        parameters.Add("@PageSize", pageSize);

        var whereClauses = new List<string> { "w.Id = @WarehouseId" };

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            whereClauses.Add("(i.ProductName LIKE @SearchTerm OR i.SKU LIKE @SearchTerm)");
            parameters.Add("@SearchTerm", $"%{searchTerm}%");
        }

        var whereClause = string.Join(" AND ", whereClauses);

        var countSql = $@"
            SELECT COUNT(*)
            FROM InventoryItems i
            INNER JOIN Warehouses w ON i.WarehouseId = w.Id
            WHERE {whereClause}";

        var dataSql = $@"
            SELECT
                i.Id,
                i.ProductName,
                i.SKU,
                i.Quantity,
                i.LastUpdated
            FROM InventoryItems i
            INNER JOIN Warehouses w ON i.WarehouseId = w.Id
            WHERE {whereClause}
            ORDER BY i.LastUpdated DESC
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY";

        var totalCount = await _connection.ExecuteScalarAsync<int>(
            countSql, parameters);

        var items = await _connection.QueryAsync<InventoryItemDto>(
            dataSql, parameters);

        return new PagedResult<InventoryItemDto>(
            items.ToList(),
            totalCount,
            pageNumber,
            pageSize);
    }
}
```

### Bulk Operations Pattern

```csharp
// For scenarios with many items
public class BulkReceiveInventoryCommand : IRequest<Result>
{
    public Guid WarehouseId { get; init; }
    public List<InventoryItemRequest> Items { get; init; }
}

public class BulkReceiveInventoryCommandHandler(
    IWarehouseRepository warehouseRepository)
    : IRequestHandler<BulkReceiveInventoryCommand, Result>
{
    public async Task<Result> Handle(
        BulkReceiveInventoryCommand request,
        CancellationToken cancellationToken)
    {
        // Load aggregate once
        var warehouse = await warehouseRepository.GetByIdAsync(
            new WarehouseId(request.WarehouseId),
            cancellationToken);

        if (warehouse is null)
        {
            return Result.Failure("Warehouse not found");
        }

        // Validate capacity upfront
        int totalQuantity = request.Items.Sum(i => i.Quantity);
        if (!warehouse.CanAccommodate(new Quantity(totalQuantity)))
        {
            return Result.Failure("Insufficient capacity");
        }

        // Process in batch
        foreach (var itemRequest in request.Items)
        {
            var item = InventoryItem.Create(
                new ProductId(itemRequest.ProductId),
                new Quantity(itemRequest.Quantity));

            warehouse.ReceiveInventory(item);
        }

        // NEVER inject IUnitOfWork directly, use warehouseRepository.UnitOfWork
        await warehouseRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);

        return Result.Success();
    }
}
```

---

## EF CORE CONFIGURATION PATTERNS

### Aggregate Configuration

```csharp
public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        // Table name
        builder.ToTable("Warehouses");

        // Primary key with value object
        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id)
            .HasConversion(
                id => id.Value,
                value => new WarehouseId(value))
            .ValueGeneratedNever();

        // Value objects as owned types
        builder.OwnsOne(w => w.Location, locationBuilder =>
        {
            locationBuilder.Property(l => l.Address)
                .HasMaxLength(500)
                .IsRequired();

            locationBuilder.OwnsOne(l => l.Coordinates);
        });

        builder.OwnsOne(w => w.Capacity, capacityBuilder =>
        {
            capacityBuilder.Property(c => c.MaxItems)
                .HasColumnName("MaxCapacity");
        });

        // Collections
        builder.HasMany(w => w.Items)
            .WithOne()
            .HasForeignKey("WarehouseId")
            .OnDelete(DeleteBehavior.Cascade);

        // Enums
        builder.Property(w => w.Status)
            .HasConversion<string>()
            .HasMaxLength(50);

        // Indexes for performance
        builder.HasIndex(w => w.Status)
            .HasDatabaseName("IX_Warehouse_Status");

        builder.HasIndex("Location_Address")
            .HasDatabaseName("IX_Warehouse_Location");

        // Ignore domain events (not persisted)
        builder.Ignore(w => w.DomainEvents);
    }
}
```

### Value Object Configuration

```csharp
public class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.ToTable("InventoryItems");

        // Strongly-typed ID
        builder.Property(i => i.Id)
            .HasConversion(
                id => id.Value,
                value => new InventoryItemId(value));

        // Quantity as value object
        builder.OwnsOne(i => i.Quantity, quantityBuilder =>
        {
            quantityBuilder.Property(q => q.Value)
                .HasColumnName("Quantity")
                .IsRequired();
        });

        // Product reference (ID only, not full entity)
        builder.Property(i => i.ProductId)
            .HasConversion(
                id => id.Value,
                value => new ProductId(value));

        // Index for product lookups
        builder.HasIndex(i => i.ProductId)
            .HasDatabaseName("IX_InventoryItem_ProductId");
    }
}
```

---

## TESTING PATTERNS

### Domain Unit Tests

```csharp
public class WarehouseTests
{
    [Fact]
    public void ReceiveInventory_WithSufficientCapacity_Succeeds()
    {
        // Arrange
        var warehouse = Warehouse.Create(
            WarehouseId.New(),
            new Location("123 Main St", Coordinates.Default),
            new Capacity(100));

        var item = InventoryItem.Create(
            new ProductId(Guid.NewGuid()),
            new Quantity(10));

        // Act
        warehouse.ReceiveInventory(item);

        // Assert
        warehouse.Items.Should().HaveCount(1);
        warehouse.DomainEvents.Should().ContainSingle(
            e => e is InventoryReceivedEvent);
    }

    [Fact]
    public void ReceiveInventory_ExceedingCapacity_ThrowsException()
    {
        // Arrange
        var warehouse = Warehouse.Create(
            WarehouseId.New(),
            new Location("123 Main St", Coordinates.Default),
            new Capacity(10));

        var item = InventoryItem.Create(
            new ProductId(Guid.NewGuid()),
            new Quantity(20));

        // Act & Assert
        var act = () => warehouse.ReceiveInventory(item);
        act.Should().Throw<InsufficientCapacityException>();
    }
}
```

### Command Handler Integration Tests

```csharp
public class CreateWarehouseCommandHandlerTests : IClassFixture<DatabaseFixture>
{
    private readonly ApplicationDbContext _context;
    private readonly IWarehouseRepository _repository;

    [Fact]
    public async Task Handle_ValidCommand_CreatesWarehouse()
    {
        // Arrange
        var command = new CreateWarehouseCommand(
            "Test Warehouse",
            "123 Test St",
            1000);

        var handler = new CreateWarehouseCommandHandler(
            _repository,
            new UnitOfWork(_context, ...));

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(w => w.Id == new WarehouseId(result.Value));

        warehouse.Should().NotBeNull();
        warehouse.Location.Address.Should().Be("123 Test St");
    }
}
```

---

## ERROR HANDLING PATTERNS

### Domain Exceptions

```csharp
// Base domain exception
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
}

// Specific domain exceptions
public class WarehouseNotFoundException : DomainException
{
    public WarehouseNotFoundException(WarehouseId id)
        : base($"Warehouse with ID {id} was not found") { }
}

public class InsufficientCapacityException : DomainException
{
    public InsufficientCapacityException(WarehouseId warehouseId, Quantity requested)
        : base($"Warehouse {warehouseId} cannot accommodate {requested.Value} items") { }
}
```

### Global Exception Handling

```csharp
// Minimal API exception handler
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

        var (statusCode, message) = exception switch
        {
            DomainException domainEx => (400, domainEx.Message),
            ValidationException => (400, "Validation failed"),
            NotFoundException => (404, "Resource not found"),
            _ => (500, "An error occurred")
        };

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(new { error = message });
    });
});
```

---

## DEPENDENCY INJECTION SETUP

### Program.cs Organization

```csharp
var builder = WebApplication.CreateBuilder(args);

// Layer registration
builder.Services.AddDomain();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApi();

var app = builder.Build();

// Configure pipeline
app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapWarehouseEndpoints();
app.MapInventoryEndpoints();
app.MapShipmentEndpoints();

app.Run();
```

### Layer-Specific Registration

```csharp
// Application layer
public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        return services;
    }
}

// Infrastructure layer
public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IWarehouseRepository, WarehouseRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Dapper connection
        services.AddScoped<IDbConnection>(sp =>
            new SqlConnection(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IInventoryQueryService, InventoryQueryService>();

        return services;
    }
}
```

---

## QUICK REFERENCE

### When to Use What

| Scenario       | Use                              | Don't Use             |
| -------------- | -------------------------------- | --------------------- |
| Modify state   | Command + EF Core                | Query + Dapper        |
| Read data      | Query + Dapper                   | Command + EF Core     |
| List view      | Dapper DTO                       | EF Core entities      |
| Business rule  | Domain method                    | Handler/Service       |
| Validation     | FluentValidation + Domain guards | Attributes            |
| ID type        | Strongly-typed (WarehouseId)     | Guid/int              |
| Error handling | Result pattern + exceptions      | Throwing everywhere   |
| Events         | Domain events in aggregate       | Infrastructure events |

### Architecture Decision Tree

```
Is this a business rule?
├─ Yes → Domain layer (Aggregate/Entity/Value Object)
└─ No
   ├─ Is it orchestration? → Application layer (Handler)
   ├─ Is it data access? → Infrastructure layer (Repository/Query Service)
   └─ Is it HTTP concern? → API layer (Minimal API endpoint)
```

---

This skill complements the main architecture skill with practical patterns, validation rules, and code examples. Use both skills together for complete .NET DDD + CQRS guidance.
