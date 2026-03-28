# Specification Pattern - Ejemplos de Uso

## Introducción

El Specification Pattern está implementado en el repositorio para encapsular queries complejas y reutilizables. Esto mejora la mantenibilidad y permite componer queries de forma declarativa.

## Estructura

- **ISpecification<T>**: Interfaz que define el contrato
- **BaseSpecification<T>**: Clase base abstracta para implementar especificaciones
- **Repository.GetBySpecification()**: Métodos para ejecutar especificaciones

## Ejemplo 1: Especificación Simple

```csharp
using Domain.AggregatesModel.UserAggregate;
using Domain.SeedWork;

namespace Infrastructure.Specifications;

/// <summary>
/// Especificación para obtener usuarios activos con sus roles
/// </summary>
public class ActiveUsersWithRolesSpec : BaseSpecification<User>
{
    public ActiveUsersWithRolesSpec()
        : base(u => u.IsActive)
    {
        AddInclude(u => u.UserRoles!);
        ApplyOrderBy(u => u.Name);
    }
}

// Uso en un Query Handler
public class GetActiveUsersQueryHandler(IRepository<User> repository)
    : IRequestHandler<GetActiveUsersQuery, List<UserDto>>
{
    public async Task<List<UserDto>> Handle(
        GetActiveUsersQuery request,
        CancellationToken cancellationToken)
    {
        var spec = new ActiveUsersWithRolesSpec();
        var users = await repository.GetBySpecification(spec, cancellationToken);

        return users.Select(u => new UserDto
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email
        }).ToList();
    }
}
```

## Ejemplo 2: Especificación con Parámetros

```csharp
/// <summary>
/// Especificación para buscar usuarios por email con sus permisos
/// </summary>
public class UserByEmailSpec : BaseSpecification<User>
{
    public UserByEmailSpec(string email)
        : base(u => u.Email == email)
    {
        AddInclude(u => u.UserRoles!);
        AddInclude(u => u.Permissions!);
    }
}

// Uso
var spec = new UserByEmailSpec("user@example.com");
var user = await repository.GetFirstBySpecification(spec);
```

## Ejemplo 3: Especificación con Paginación

```csharp
/// <summary>
/// Especificación para usuarios creados recientemente, paginados
/// </summary>
public class RecentUsersPaginatedSpec : BaseSpecification<User>
{
    public RecentUsersPaginatedSpec(int page = 1, int pageSize = 10)
        : base(u => u.CreatedOn > DateTimeOffset.UtcNow.AddDays(-30))
    {
        ApplyOrderByDescending(u => u.CreatedOn);
        ApplyPaging(page, pageSize);
    }
}

// Uso
var spec = new RecentUsersPaginatedSpec(page: 2, pageSize: 20);
var pagedResult = await repository.GetBySpecificationPaginated(spec);

Console.WriteLine($"Total: {pagedResult.TotalCount}");
Console.WriteLine($"Items: {pagedResult.Items.Count}");
```

## Ejemplo 4: Especificación con Tracking

```csharp
/// <summary>
/// Especificación para obtener usuario con tracking (para actualización)
/// </summary>
public class UserForUpdateSpec : BaseSpecification<User>
{
    public UserForUpdateSpec(Guid userId)
        : base(u => u.Id == userId)
    {
        ApplyTracking(); // Habilita change tracking
    }
}

// Uso en un Command Handler
var spec = new UserForUpdateSpec(userId);
var user = await repository.GetFirstBySpecification(spec);

if (user != null)
{
    user.UpdateName("New Name");
    repository.Update(user);
    await repository.UnitOfWork.SaveEntitiesAsync();
}
```

## Ejemplo 5: Especificación Compleja

```csharp
/// <summary>
/// Especificación para reportes de usuarios con múltiples filtros
/// </summary>
public class UserReportSpec : BaseSpecification<User>
{
    public UserReportSpec(
        string? searchTerm = null,
        bool? isActive = null,
        DateTimeOffset? createdAfter = null,
        int page = 1,
        int pageSize = 50)
    {
        // Construir criterios dinámicamente
        Expression<Func<User, bool>>? criteria = null;

        if (!string.IsNullOrEmpty(searchTerm))
        {
            var search = searchTerm.ToLower();
            criteria = u => u.Name.ToLower().Contains(search) ||
                           u.Email.ToLower().Contains(search);
        }

        if (isActive.HasValue)
        {
            var activeFilter = isActive.Value;
            criteria = criteria == null
                ? u => u.IsActive == activeFilter
                : CombineExpressions(criteria, u => u.IsActive == activeFilter);
        }

        if (createdAfter.HasValue)
        {
            var date = createdAfter.Value;
            criteria = criteria == null
                ? u => u.CreatedOn >= date
                : CombineExpressions(criteria, u => u.CreatedOn >= date);
        }

        // Aplicar criterio final (usar constructor base si existe)
        if (criteria != null)
        {
            // Nota: necesitarías agregar un método protected SetCriteria en BaseSpecification
            // o pasar criteria al constructor base si tu diseño lo permite
        }

        AddInclude(u => u.UserRoles!);
        ApplyOrderByDescending(u => u.CreatedOn);
        ApplyPaging(page, pageSize);
    }

    private static Expression<Func<User, bool>> CombineExpressions(
        Expression<Func<User, bool>> first,
        Expression<Func<User, bool>> second)
    {
        var parameter = Expression.Parameter(typeof(User));

        var combined = Expression.AndAlso(
            Expression.Invoke(first, parameter),
            Expression.Invoke(second, parameter));

        return Expression.Lambda<Func<User, bool>>(combined, parameter);
    }
}
```

## Métodos Disponibles en BaseSpecification

```csharp
protected void AddInclude(Expression<Func<T, object>> includeExpression)
protected void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
protected void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescExpression)
protected void ApplyTracking()
protected void ApplyPaging(int page, int pageSize = 10)
```

## Métodos Disponibles en IRepository

```csharp
// Obtener lista
Task<List<T>> GetBySpecification(
    ISpecification<T> specification,
    CancellationToken cancellationToken = default);

// Obtener primero
Task<T?> GetFirstBySpecification(
    ISpecification<T> specification,
    CancellationToken cancellationToken = default);

// Contar
Task<int> GetCountBySpecification(
    ISpecification<T> specification,
    CancellationToken cancellationToken = default);

// Paginado
Task<PagedResult<T>> GetBySpecificationPaginated(
    ISpecification<T> specification,
    CancellationToken cancellationToken = default);
```

## Ventajas

1. **Reutilización**: Las especificaciones pueden reutilizarse en múltiples handlers
2. **Testabilidad**: Fácil de probar en aislamiento
3. **Composición**: Se pueden combinar especificaciones (con implementación adicional)
4. **Claridad**: El nombre de la clase documenta la intención de la query
5. **Mantenibilidad**: Cambios centralizados en un solo lugar
6. **Métricas**: Las especificaciones se registran automáticamente en Prometheus con el nombre de la clase

## Logging y Métricas

Las operaciones con especificaciones generan automáticamente:

```
[Debug] Executing specification ActiveUsersWithRolesSpec on User
[Debug] Repository operation specification_select on User completed successfully in 45.2ms
```

Métricas en Prometheus:
- `database_queries_total{operation="specification_select", entity="User", status="success"}`
- `database_query_duration{operation="specification_select", entity="User"}`

## Referencias

- Patrón original: [Specification Pattern - Martin Fowler](https://martinfowler.com/apsupp/spec.pdf)
- EF Core: [Query Filters](https://learn.microsoft.com/en-us/ef/core/querying/)
