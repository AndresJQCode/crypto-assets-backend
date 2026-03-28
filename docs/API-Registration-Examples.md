# API Registration Examples

## Overview

Este documento muestra ejemplos prácticos de cómo registrar endpoints usando los extension methods de `AdminExtensions` y `EndpointMappingExtensions`.

## Extension Methods Disponibles

### 1. MapAdminGroup
Crea un grupo de endpoints admin con:
- ✅ Prefijo `/admin`
- ✅ Autenticación requerida
- ✅ Validación de SuperAdmin role

### 2. MapTenantGroup
Crea un grupo de endpoints tenant con:
- ✅ Prefijo `/api`
- ✅ Autenticación requerida
- ✅ Tenant context (via middleware)

### 3. MapPublicGroup
Crea un grupo de endpoints públicos:
- ✅ Sin autenticación
- ✅ Prefijo personalizable

## Registro en Program.cs

### ✅ Forma Correcta (Usando Extension Methods)

```csharp
// ============================================
// 1. ADMIN ENDPOINTS
// ============================================
var adminGroup = app.MapAdminGroup("/admin");

// Cada endpoint recibe el adminGroup como parámetro
adminGroup.MapSystemConfigurationEndpoints();
adminGroup.MapConnectorDefinitionsEndpoints();
adminGroup.MapTenantManagementEndpoints();
adminGroup.MapGlobalPermissionsEndpoints();

// ============================================
// 2. TENANT ENDPOINTS
// ============================================
var tenantGroup = app.MapTenantGroup("/api");

tenantGroup.MapDashboardEndpoints();
tenantGroup.MapUsersEndpoints();
tenantGroup.MapRolesEndpoints();
tenantGroup.MapConnectorInstancesEndpoints();
tenantGroup.MapOrdersEndpoints();
tenantGroup.MapCustomersEndpoints();

// ============================================
// 3. PUBLIC ENDPOINTS
// ============================================
var publicGroup = app.MapPublicGroup("/auth");
publicGroup.MapAuthEndpoints();

// Health checks (también públicos)
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");
```

### ❌ Forma Incorrecta (Repetitiva)

```csharp
// ❌ NO HACER ESTO - Repetir auth y SuperAdmin en cada endpoint
app.MapGroup("/admin/system-config")
    .RequireAuthorization()
    .RequireSuperAdminRole()
    .MapSystemConfigurationEndpoints();

app.MapGroup("/admin/tenants")
    .RequireAuthorization()
    .RequireSuperAdminRole()
    .MapTenantManagementEndpoints();

// Esto es repetitivo y propenso a errores
```

## Implementación de Endpoints

### Admin Endpoint (Correcto)

```csharp
// Api/Apis/Admin/SystemConfigurationEndpoints/SystemConfigurationApi.cs
namespace Api.Apis.Admin.SystemConfigurationEndpoints;

public static class SystemConfigurationApi
{
    // ✅ Recibe el adminGroup como parámetro
    public static RouteGroupBuilder MapSystemConfigurationEndpoints(
        this RouteGroupBuilder adminGroup)
    {
        // ✅ Solo define el sub-path, el /admin ya viene del grupo
        var group = adminGroup.MapGroup("/system-config")
            .WithTags("Admin - System Configuration");

        group.MapGet("/{key}", GetConfigByKey)
            .WithName("GetSystemConfigurationByKey");

        group.MapPut("/{key}", UpdateConfig)
            .WithName("UpdateSystemConfiguration");

        return group;
    }

    private static async Task<IResult> GetConfigByKey(
        string key,
        IMediator mediator,
        CancellationToken ct)
    {
        // Implementation...
    }
}
```

### Tenant Endpoint (Correcto)

```csharp
// Api/Apis/DashboardEndpoints/DashboardApi.cs
namespace Api.Apis.DashboardEndpoints;

public static class DashboardApi
{
    // ✅ Recibe el tenantGroup como parámetro
    public static RouteGroupBuilder MapDashboardEndpoints(
        this RouteGroupBuilder tenantGroup)
    {
        // ✅ Solo define el sub-path, el /api ya viene del grupo
        var group = tenantGroup.MapGroup("/dashboard")
            .WithTags("Tenant - Dashboard");

        group.MapGet("/", GetDashboard)
            .WithName("GetDashboard")
            .RequirePermission("Dashboard", "Read");

        return group;
    }

    private static async Task<IResult> GetDashboard(
        HttpContext httpContext,
        IMediator mediator,
        CancellationToken ct)
    {
        // ✅ TenantId disponible en httpContext.Items
        var tenantId = (Guid)httpContext.Items["TenantId"]!;

        var query = new GetDashboardQuery(tenantId);
        var result = await mediator.Send(query, ct);

        return Results.Ok(result);
    }
}
```

### Public Endpoint (Correcto)

```csharp
// Api/Apis/AuthEndpoints/AuthApi.cs
namespace Api.Apis.AuthEndpoints;

public static class AuthApi
{
    // ✅ Recibe el publicGroup como parámetro
    public static RouteGroupBuilder MapAuthEndpoints(
        this RouteGroupBuilder publicGroup)
    {
        // ✅ El /auth ya viene del grupo, aquí definimos los métodos
        publicGroup.MapPost("/login", Login)
            .WithName("Login")
            .WithTags("Authentication")
            .AllowAnonymous();

        publicGroup.MapPost("/register", Register)
            .WithName("Register")
            .WithTags("Authentication")
            .AllowAnonymous();

        return publicGroup;
    }

    private static async Task<IResult> Login(
        LoginCommand command,
        IMediator mediator,
        CancellationToken ct)
    {
        // Implementation...
    }
}
```

## Migración de Endpoints Existentes

### Ejemplo: Migrar ConnectorDefinitions a Admin

**Antes:**
```csharp
// Api/Apis/ConnectorDefinitionsEndpoints/ConnectorDefinitionsApi.cs
public static RouteGroupBuilder MapConnectorDefinitionsEndpoints(
    this IEndpointRouteBuilder app)
{
    var group = app.MapGroup("/connector-definitions")
        .WithTags("ConnectorDefinitions");

    group.MapGet("/", GetAll)
        .RequirePermission("ConnectorDefinitions", "Read");

    return group;
}
```

**Después:**
```csharp
// Api/Apis/Admin/ConnectorDefinitionsEndpoints/ConnectorDefinitionsApi.cs
public static RouteGroupBuilder MapConnectorDefinitionsEndpoints(
    this RouteGroupBuilder adminGroup)  // ✅ Cambio 1: Recibe adminGroup
{
    var group = adminGroup.MapGroup("/connector-definitions")  // ✅ Cambio 2: Usa adminGroup
        .WithTags("Admin - Connector Definitions");  // ✅ Cambio 3: Tag actualizado

    // ✅ Cambio 4: Removemos RequirePermission, el SuperAdmin ya tiene acceso
    group.MapGet("/", GetAll)
        .WithName("GetAllConnectorDefinitions");

    return group;
}
```

### Ejemplo: Migrar Dashboard a Tenant API

**Antes:**
```csharp
// Api/Apis/DashboardEndpoints/DashboardApi.cs
public static RouteGroupBuilder MapDashboardEndpoints(
    this IEndpointRouteBuilder app)
{
    var group = app.MapGroup("/dashboard")
        .WithTags("Dashboard");

    group.MapGet("/", GetDashboard)
        .RequirePermission("Dashboard", "Read");

    return group;
}
```

**Después:**
```csharp
// Api/Apis/DashboardEndpoints/DashboardApi.cs
public static RouteGroupBuilder MapDashboardEndpoints(
    this RouteGroupBuilder tenantGroup)  // ✅ Cambio 1: Recibe tenantGroup
{
    var group = tenantGroup.MapGroup("/dashboard")  // ✅ Cambio 2: Usa tenantGroup
        .WithTags("Tenant - Dashboard");  // ✅ Cambio 3: Tag actualizado

    group.MapGet("/", GetDashboard)
        .RequirePermission("Dashboard", "Read");

    return group;
}
```

## Estructura de URLs Resultante

### Admin Endpoints
```
/admin/system-config/{key}              → GET, PUT, PATCH
/admin/system-config/order-processing/* → POST
/admin/tenants                          → GET, POST
/admin/tenants/{id}                     → GET, PUT, DELETE
/admin/tenants/{id}/suspend             → PATCH
/admin/connector-definitions            → GET, POST, PUT, DELETE
/admin/permissions                      → GET, POST, PUT, DELETE
```

### Tenant Endpoints
```
/api/dashboard                          → GET
/api/users                              → GET, POST, PUT, DELETE
/api/roles                              → GET, POST, PUT, DELETE
/api/connectors                         → GET
/api/connectors/shopify/connect         → POST
/api/orders                             → GET, POST
/api/customers                          → GET, POST
```

### Public Endpoints
```
/auth/login                             → POST
/auth/register                          → POST
/auth/refresh                           → POST
/auth/forgot-password                   → POST
/auth/oauth/google                      → GET
/auth/oauth/microsoft                   → GET
```

## Validación y Testing

### Test de Admin Endpoint

```csharp
[Fact]
public async Task AdminEndpoint_WithSuperAdminRole_ReturnsSuccess()
{
    // Arrange
    var client = _factory.CreateClient();
    var token = GenerateSuperAdminToken();
    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", token);

    // Act
    var response = await client.GetAsync("/admin/system-config/test-key");

    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
}

[Fact]
public async Task AdminEndpoint_WithoutSuperAdminRole_Returns403()
{
    // Arrange
    var client = _factory.CreateClient();
    var token = GenerateTenantUserToken();  // Regular user
    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", token);

    // Act
    var response = await client.GetAsync("/admin/system-config/test-key");

    // Assert
    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
}
```

### Test de Tenant Endpoint

```csharp
[Fact]
public async Task TenantEndpoint_WithTenantContext_ReturnsSuccess()
{
    // Arrange
    var client = _factory.CreateClient();
    var token = GenerateTenantUserToken(tenantId: Guid.NewGuid());
    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", token);

    // Act
    var response = await client.GetAsync("/api/dashboard");

    // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
}

[Fact]
public async Task TenantEndpoint_WithoutTenantId_Returns400()
{
    // Arrange
    var client = _factory.CreateClient();
    var token = GenerateTokenWithoutTenantId();
    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", token);

    // Act
    var response = await client.GetAsync("/api/dashboard");

    // Assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
}
```

## Checklist de Migración

- [ ] Crear carpeta `Admin/` para endpoints de super admin
- [ ] Actualizar signature de endpoints admin: `RouteGroupBuilder` en lugar de `IEndpointRouteBuilder`
- [ ] Cambiar `.MapGroup("/path")` a `.MapGroup("/sub-path")` (sin `/admin` prefix)
- [ ] Actualizar tags: agregar prefijo "Admin -" o "Tenant -"
- [ ] Actualizar Program.cs para usar `MapAdminGroup()` y `MapTenantGroup()`
- [ ] Remover `.RequirePermission()` de admin endpoints (SuperAdmin tiene acceso total)
- [ ] Verificar que TenantContextMiddleware esté registrado
- [ ] Agregar tests de autorización
- [ ] Actualizar documentación de Swagger/Scalar

## Beneficios

✅ **Menos Código**: No repetir autenticación y autorización
✅ **Más Seguro**: Imposible olvidar proteger un admin endpoint
✅ **Más Claro**: URL structure refleja la arquitectura
✅ **Más Mantenible**: Cambios centralizados en extension methods
✅ **Mejor Testing**: Tests más simples y claros
