# API Organization Guide - Multi-Tenant Architecture

## Overview

Este proyecto implementa una arquitectura multi-tenant con dos niveles de acceso:
- **Super Admin**: Administrador del SaaS (gestiona todos los tenants)
- **Tenant User**: Usuario de un tenant específico

## API Structure

```
/auth/*                              # Autenticación (público)
/api/*                               # Tenant APIs (requiere tenant context)
/admin/*                             # Super Admin APIs (solo super admins)
```

## Endpoint Categories

### 1. Public Endpoints (`/auth/*`)

**Sin autenticación requerida:**
```
POST   /auth/login
POST   /auth/register
POST   /auth/forgot-password
GET    /auth/oauth/google
GET    /auth/oauth/microsoft
```

**Autenticación opcional:**
```
GET    /auth/me                      # Info del usuario actual
POST   /auth/refresh                 # Refresh token
POST   /auth/logout
```

### 2. Tenant Endpoints (`/api/*`)

**Características:**
- ✅ Requieren autenticación
- ✅ Requieren contexto de tenant (TenantId)
- ✅ Filtrado automático por TenantId
- ✅ Permiso: cualquier usuario autenticado del tenant

**Estructura:**
```
GET    /api/dashboard                # Dashboard del tenant
GET    /api/users                    # Usuarios del tenant
POST   /api/users                    # Crear usuario en el tenant
GET    /api/roles                    # Roles del tenant
GET    /api/connectors               # Conectores del tenant
POST   /api/connectors/shopify       # Conectar Shopify
GET    /api/orders                   # Órdenes del tenant
GET    /api/customers                # Clientes del tenant
GET    /api/products                 # Productos del tenant
```

### 3. Super Admin Endpoints (`/admin/*`)

**Características:**
- ✅ Requieren autenticación
- ✅ Requieren rol de SuperAdmin
- ✅ Acceso cross-tenant (ven todos los tenants)
- ✅ Operaciones de gestión del SaaS

**Estructura:**
```
# Gestión de Tenants
GET    /admin/tenants                # Listar todos los tenants
POST   /admin/tenants                # Crear nuevo tenant
GET    /admin/tenants/{id}           # Ver detalle de tenant
PUT    /admin/tenants/{id}           # Actualizar tenant
PATCH  /admin/tenants/{id}/suspend   # Suspender tenant
PATCH  /admin/tenants/{id}/activate  # Activar tenant
DELETE /admin/tenants/{id}           # Eliminar tenant

# Catálogo de Conectores
GET    /admin/connector-definitions  # Ver catálogo de conectores
POST   /admin/connector-definitions  # Agregar conector al catálogo
PUT    /admin/connector-definitions/{id}
DELETE /admin/connector-definitions/{id}
PATCH  /admin/connector-definitions/{id}/activate

# Configuración del Sistema
GET    /admin/system-config/{key}
PUT    /admin/system-config/{key}
POST   /admin/system-config/order-processing/enable
POST   /admin/system-config/order-processing/disable

# Permisos Globales
GET    /admin/permissions            # Ver todos los permisos del sistema
POST   /admin/permissions            # Crear nuevo permiso global
PUT    /admin/permissions/{id}
DELETE /admin/permissions/{id}

# Analytics y Reportes
GET    /admin/analytics/tenants      # Analytics de todos los tenants
GET    /admin/analytics/usage        # Uso del sistema por tenant
GET    /admin/analytics/revenue      # Revenue por tenant

# Billing (Futuro)
GET    /admin/billing/invoices       # Facturas de todos los tenants
POST   /admin/billing/invoice        # Generar factura
GET    /admin/billing/subscriptions  # Suscripciones activas
```

## Physical Folder Structure

```
Api/Apis/
│
├── AuthEndpoints/                   # /auth/*
│   └── AuthApi.cs
│
├── Admin/                           # /admin/* (Super Admin only)
│   ├── TenantManagementEndpoints/
│   │   └── TenantManagementApi.cs
│   ├── ConnectorDefinitionsEndpoints/
│   │   └── ConnectorDefinitionsApi.cs
│   ├── SystemConfigurationEndpoints/
│   │   └── SystemConfigurationApi.cs
│   ├── GlobalPermissionsEndpoints/
│   │   └── GlobalPermissionsApi.cs
│   └── AnalyticsEndpoints/
│       └── AnalyticsApi.cs
│
└── [Tenant Endpoints]/              # /api/* (Tenant context)
    ├── DashboardEndpoints/
    ├── UsersEndpoints/
    ├── RolesEndpoints/
    ├── ConnectorInstancesEndpoints/
    ├── OrdersEndpoints/
    ├── CustomersEndpoints/
    └── ProductsEndpoints/
```

## Migration Plan

### Endpoints to Move to `/admin/*`

1. **TenantsEndpoints** → `Admin/TenantManagementEndpoints`
   - Cambiar ruta de `/tenants` a `/admin/tenants`

2. **ConnectorDefinitionsEndpoints** → `Admin/ConnectorDefinitionsEndpoints`
   - Cambiar ruta de `/connector-definitions` a `/admin/connector-definitions`

3. **SystemConfigurationEndpoints** → `Admin/SystemConfigurationEndpoints`
   - Cambiar ruta de `/system-config` a `/admin/system-config`

4. **PermissionsEndpoints** → `Admin/GlobalPermissionsEndpoints`
   - Cambiar ruta de `/permissions` a `/admin/permissions`
   - Solo para gestión global de permisos

### Endpoints to Keep in Root (with `/api` prefix)

1. **ConnectorInstancesEndpoints** → `/api/connectors`
2. **DashboardEndpoints** → `/api/dashboard`
3. **UsersEndpoints** → `/api/users`
4. **RolesEndpoints** → `/api/roles`
5. **PermissionRolesEndpoints** → `/api/permission-roles`

## Implementation Example

### Before (Current)

```csharp
// Api/Apis/TenantsEndpoints/TenantsApi.cs
public static RouteGroupBuilder MapTenantsEndpoints(this IEndpointRouteBuilder app)
{
    var group = app.MapGroup("/tenants")  // ❌ No prefix, confusing
        .WithTags("Tenants");

    group.MapGet("/", GetAllTenants)
        .RequirePermission("Tenants", "Read");

    return group;
}
```

### After (Recommended)

```csharp
// Api/Apis/Admin/TenantManagementEndpoints/TenantManagementApi.cs
public static RouteGroupBuilder MapTenantManagementEndpoints(this IEndpointRouteBuilder app)
{
    var group = app.MapGroup("/admin/tenants")  // ✅ Clear admin prefix
        .WithTags("Admin - Tenant Management")
        .RequireAuthorization()
        .RequireSuperAdminRole();  // ✅ Global super admin check

    group.MapGet("/", GetAllTenants)
        .WithName("GetAllTenants")
        .Produces<List<TenantDto>>();

    group.MapPost("/", CreateTenant)
        .WithName("CreateTenant")
        .Produces<TenantDto>(201);

    group.MapPatch("/{id:guid}/suspend", SuspendTenant)
        .WithName("SuspendTenant")
        .Produces(200);

    return group;
}
```

## Middleware Strategy

### Super Admin Validation Middleware

```csharp
// Api/Extensions/AdminExtensions.cs
public static class AdminExtensions
{
    /// <summary>
    /// Requires the user to be a Super Admin
    /// </summary>
    public static RouteHandlerBuilder RequireSuperAdminRole(
        this RouteHandlerBuilder builder)
    {
        return builder.AddEndpointFilter(async (context, next) =>
        {
            var httpContext = context.HttpContext;
            var user = httpContext.User;

            // Check if user has SuperAdmin role
            if (!user.IsInRole("SuperAdmin"))
            {
                return Results.Forbid();
            }

            return await next(context);
        });
    }

    /// <summary>
    /// Extension for RouteGroupBuilder
    /// </summary>
    public static RouteGroupBuilder RequireSuperAdminRole(
        this RouteGroupBuilder group)
    {
        return group.AddEndpointFilter(async (context, next) =>
        {
            var httpContext = context.HttpContext;
            var user = httpContext.User;

            if (!user.IsInRole("SuperAdmin"))
            {
                return Results.Forbid();
            }

            return await next(context);
        });
    }
}
```

### Tenant Context Middleware

```csharp
// Api/Middlewares/TenantContextMiddleware.cs
public class TenantContextMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";

        // Skip tenant resolution for admin and auth routes
        if (path.StartsWith("/admin") || path.StartsWith("/auth"))
        {
            await next(context);
            return;
        }

        // For /api/* routes, resolve tenant from user claims
        if (path.StartsWith("/api"))
        {
            var tenantId = context.User.FindFirst("TenantId")?.Value;

            if (string.IsNullOrEmpty(tenantId))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Tenant context required for this endpoint"
                });
                return;
            }

            // Store in HttpContext.Items for use in repositories
            context.Items["TenantId"] = Guid.Parse(tenantId);
        }

        await next(context);
    }
}
```

## Program.cs Registration

```csharp
// Register tenant context middleware
app.UseTenantContext();

// Auth endpoints (public)
app.MapAuthEndpoints().WithTags("Authentication");

// Tenant endpoints (/api/*)
app.MapGroup("/api")
    .MapDashboardEndpoints()
    .MapUsersEndpoints()
    .MapRolesEndpoints()
    .MapConnectorInstancesEndpoints()
    .RequireAuthorization();  // All /api/* require auth

// Admin endpoints (/admin/*)
app.MapGroup("/admin")
    .MapTenantManagementEndpoints()
    .MapConnectorDefinitionsEndpoints()
    .MapSystemConfigurationEndpoints()
    .MapGlobalPermissionsEndpoints()
    .RequireSuperAdminRole();  // All /admin/* require SuperAdmin
```

## Swagger/Scalar Organization

```csharp
// Configure Swagger groups
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.TagActionsBy(api =>
    {
        var path = api.RelativePath ?? "";

        if (path.StartsWith("admin/"))
            return new[] { $"Admin - {api.GroupName}" };

        if (path.StartsWith("api/"))
            return new[] { $"Tenant - {api.GroupName}" };

        return new[] { api.GroupName ?? "Default" };
    });

    options.DocInclusionPredicate((docName, api) => true);
});
```

## Security Considerations

### ✅ Best Practices

1. **Clear Separation**: Admin y Tenant APIs físicamente separadas
2. **Middleware Protection**: Validación automática de SuperAdmin en `/admin/*`
3. **Tenant Isolation**: TenantId automático en `/api/*`
4. **Audit Logging**: Log diferenciado para admin vs tenant operations
5. **Rate Limiting**: Límites diferentes para admin vs tenant

### 🔒 Permission Strategy

```
SuperAdmin Role:
  - Full access to /admin/*
  - Read-only access to /api/* (for support)
  - No TenantId in claims (cross-tenant)

Tenant Admin:
  - Full access to /api/* (within their tenant)
  - No access to /admin/*
  - TenantId in claims (single tenant)

Tenant User:
  - Limited access to /api/* based on permissions
  - No access to /admin/*
  - TenantId in claims (single tenant)
```

## Testing

### Integration Test Example

```csharp
[Fact]
public async Task AdminEndpoint_WithoutSuperAdminRole_Returns403()
{
    // Arrange
    var client = _factory.CreateClient();
    var token = GenerateTenantUserToken(); // Not SuperAdmin

    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", token);

    // Act
    var response = await client.GetAsync("/admin/tenants");

    // Assert
    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
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

## Checklist

- [ ] Crear carpeta `Api/Apis/Admin/`
- [ ] Mover endpoints de super admin a `/admin/*`
- [ ] Agregar prefijo `/api/*` a endpoints de tenant
- [ ] Implementar `RequireSuperAdminRole()` extension
- [ ] Implementar `TenantContextMiddleware`
- [ ] Actualizar Program.cs con nueva estructura
- [ ] Actualizar Swagger tags
- [ ] Agregar tests de autorización
- [ ] Actualizar documentación de API
- [ ] Migrar permisos a nueva estructura

## References

- [Multi-Tenant Architecture Best Practices](https://learn.microsoft.com/en-us/azure/architecture/guide/multitenant/overview)
- [ASP.NET Core Minimal APIs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis)
- [Route Groups](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/route-handlers#route-groups)
