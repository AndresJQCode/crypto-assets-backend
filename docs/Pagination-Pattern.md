# Pagination Pattern - Guía de Uso

## Overview

Este proyecto implementa un patrón de paginación automatizado usando **MediatR Pipeline Behaviors** y **Extension Methods** para eliminar código repetitivo en endpoints paginados.

## Arquitectura

```
┌─────────────────────────────────────────────────────────────┐
│                     Minimal API Endpoint                     │
│  MapGetPaginated("/", () => new GetAllQuery())              │
└───────────────────────────┬─────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│              MediatR Pipeline (en orden)                     │
│  1. LoggingBehavior                                         │
│  2. ValidatorBehavior                                       │
│  3. PaginationBehavior ◄── Inyecta params y agrega headers  │
│  4. TransactionBehavior                                     │
└───────────────────────────┬─────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────┐
│                     Query Handler                            │
│  Usa request.PaginationParameters ya inyectados             │
└─────────────────────────────────────────────────────────────┘
```

## Componentes

### 1. IPaginatedQuery<TResponse>

Interfaz marcadora que indica que una query soporta paginación.

```csharp
public interface IPaginatedQuery<TResponse>
{
    PaginationParameters PaginationParameters { get; set; }
}
```

### 2. PaginationBehavior<TRequest, TResponse>

Pipeline behavior de MediatR que:
- ✅ Extrae parámetros de paginación del query string (`?page=1&limit=20`)
- ✅ Inyecta los parámetros en la query automáticamente

### 3. PaginationExtensions

Extension methods para simplificar el mapeo de endpoints paginados.

## Uso

### Paso 1: Crear la Query

```csharp
using Api.Application.Dtos;
using Api.Application.Queries;
using MediatR;

namespace Api.Application.Queries.UserQueries;

// ✅ Implementa IPaginatedQuery
internal sealed class GetAllUsersQuery
    : IRequest<PaginationResponseDto<UserDto>>,
      IPaginatedQuery<PaginationResponseDto<UserDto>>
{
    // ✅ Esta propiedad será inyectada automáticamente por PaginationBehavior
    public PaginationParameters PaginationParameters { get; set; } = new();
}
```

### Paso 2: Crear el Handler

```csharp
using Api.Application.Dtos;
using Domain.AggregatesModel.UserAggregate;
using Domain.SeedWork;
using Mapster;
using MediatR;

namespace Api.Application.Queries.UserQueries;

internal sealed class GetAllUsersQueryHandler(IUserRepository repository)
    : IRequestHandler<GetAllUsersQuery, PaginationResponseDto<UserDto>>
{
    public async Task<PaginationResponseDto<UserDto>> Handle(
        GetAllUsersQuery request,
        CancellationToken cancellationToken)
    {
        // ✅ Los PaginationParameters ya están inyectados
        var p = request.PaginationParameters;

        // ✅ Usar el repository para obtener resultados paginados
        PagedResult<User> paged = await repository.GetByFilterPagination(
            filter: u => !u.IsDeleted,
            orderBy: q => q.OrderBy(u => u.UserName),
            page: p.Page,
            pageSize: p.Limit,
            cancellationToken: cancellationToken);

        // ✅ Mapear y retornar
        var data = paged.Items.Adapt<IReadOnlyList<UserDto>>();
        return new PaginationResponseDto<UserDto>
        {
            Data = data,
            TotalCount = paged.TotalCount,
            TotalPages = paged.TotalPages,
            Limit = p.Limit,
            Page = p.Page
        };
    }
}
```

### Paso 3: Mapear el Endpoint

#### ✅ ANTES (Código Repetitivo):

```csharp
group.MapGet("/", GetAll)
    .WithName("GetAllUsers")
    .Produces<PaginationResponseDto<UserDto>>();

private static async Task<PaginationResponseDto<UserDto>> GetAll(
    IMediator mediator,
    IHttpContextAccessor httpContextAccessor,
    CancellationToken ct)
{
    // ❌ Repetir este código en cada endpoint paginado
    var paginationParams = PaginationHelper.GetPaginationParametersFromQueryString(httpContextAccessor);
    var query = new GetAllUsersQuery { PaginationParameters = paginationParams };
    var result = await mediator.Send(query, ct);
    PaginationHelper.AddPaginationHeaders(httpContextAccessor, result.TotalCount, result.Limit, result.Page);
    return result;
}
```

#### ✅ DESPUÉS (Usando Extension Method):

```csharp
// ✅ Una sola línea - todo el resto es automático
group.MapGetPaginated<GetAllUsersQuery, UserDto>(
    "/",
    () => new GetAllUsersQuery())
    .WithName("GetAllUsers")
    .WithSummary("Listar usuarios")
    .WithDescription("Lista usuarios con paginación (query: ?page=1&limit=20)")
    .Produces<PaginationResponseDto<UserDto>>();
```

## Ejemplos Avanzados

### Endpoint con Parámetros de Ruta

```csharp
// Query con parámetro adicional
internal sealed class GetUsersByRoleQuery
    : IRequest<PaginationResponseDto<UserDto>>,
      IPaginatedQuery<PaginationResponseDto<UserDto>>
{
    public Guid RoleId { get; set; }
    public PaginationParameters PaginationParameters { get; set; } = new();
}

// Endpoint
group.MapGetPaginated<GetUsersByRoleQuery, UserDto, Guid>(
    "/{roleId:guid}/users",
    roleId => new GetUsersByRoleQuery { RoleId = roleId })
    .WithName("GetUsersByRole")
    .Produces<PaginationResponseDto<UserDto>>();
```

### Endpoint con Múltiples Parámetros

```csharp
// Query
internal sealed class SearchUsersQuery
    : IRequest<PaginationResponseDto<UserDto>>,
      IPaginatedQuery<PaginationResponseDto<UserDto>>
{
    public string SearchTerm { get; set; } = "";
    public bool IncludeInactive { get; set; }
    public PaginationParameters PaginationParameters { get; set; } = new();
}

// Endpoint con query parameters adicionales
group.MapGetPaginated<SearchUsersQuery, UserDto, string, bool>(
    "/search",
    (searchTerm, includeInactive) => new SearchUsersQuery
    {
        SearchTerm = searchTerm,
        IncludeInactive = includeInactive
    })
    .WithName("SearchUsers")
    .Produces<PaginationResponseDto<UserDto>>();
```

## Request/Response

### Request Query String

```http
GET /api/users?page=2&limit=10
```

### Response Body

La información de paginación se incluye directamente en el response body:

```json
{
  "data": [
    { "id": "...", "name": "User 11", ... },
    { "id": "...", "name": "User 12", ... },
    ...
  ],
  "totalCount": 156,
  "page": 2,
  "limit": 10,
  "totalPages": 16
}
```

**Nota**: La paginación se retorna en el body, no en headers HTTP.

## Valores Por Defecto

Si no se especifican parámetros en el query string:

- **page**: 1
- **limit**: 20 (configurable en `PaginationHelper`)

```http
GET /api/users
# Equivalente a: GET /api/users?page=1&limit=20
```

## Ventajas del Patrón

✅ **DRY (Don't Repeat Yourself)**: Elimina código repetitivo
✅ **Separación de Responsabilidades**: Lógica de paginación centralizada
✅ **Type-Safe**: Compile-time safety con genéricos
✅ **Testeable**: Fácil de probar en unit tests
✅ **Consistente**: Todos los endpoints paginados funcionan igual
✅ **Extensible**: Fácil agregar nuevos parámetros de paginación

## Testing

### Unit Test del Handler

```csharp
[Fact]
public async Task Handle_ReturnsPagedUsers()
{
    // Arrange
    var query = new GetAllUsersQuery
    {
        PaginationParameters = new PaginationParameters { Page = 1, Limit = 10 }
    };

    // Act
    var result = await _handler.Handle(query, CancellationToken.None);

    // Assert
    Assert.Equal(10, result.Data.Count);
    Assert.Equal(1, result.Page);
    Assert.Equal(10, result.Limit);
}
```

### Integration Test del Endpoint

```csharp
[Fact]
public async Task GetAllUsers_WithPagination_ReturnsPagedResults()
{
    // Arrange
    var client = _factory.CreateClient();
    var token = GenerateTenantUserToken();
    client.DefaultRequestHeaders.Authorization = new("Bearer", token);

    // Act
    var response = await client.GetAsync("/api/users?page=2&limit=5");

    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.Equal("5", response.Headers.GetValues("X-Limit").First());
    Assert.Equal("2", response.Headers.GetValues("X-Page").First());

    var content = await response.Content.ReadFromJsonAsync<PaginationResponseDto<UserDto>>();
    Assert.Equal(5, content.Limit);
    Assert.Equal(2, content.Page);
}
```

## Migración de Endpoints Existentes

Para migrar endpoints existentes al nuevo patrón:

1. ✅ Hacer que la query implemente `IPaginatedQuery<TResponse>`
2. ✅ Cambiar `MapGet()` por `MapGetPaginated()`
3. ✅ Eliminar el método privado que manejaba paginación manualmente
4. ✅ Limpiar imports innecesarios (IHttpContextAccessor, PaginationHelper)

**El PaginationBehavior se encargará automáticamente del resto** 🎉

## Checklist de Implementación

- [x] Crear `IPaginatedQuery<TResponse>`
- [x] Crear `PaginationBehavior<TRequest, TResponse>`
- [x] Registrar behavior en `Program.cs`
- [x] Crear `PaginationExtensions` con `MapGetPaginated()`
- [x] Actualizar queries existentes
- [x] Actualizar endpoints existentes
- [ ] Agregar tests unitarios
- [ ] Agregar tests de integración
- [ ] Documentar en CLAUDE.md
