# Guía de Instrumentación de Servicios con Prometheus

> 🎉 **INSTRUMENTACIÓN COMPLETA**: Todos los servicios principales han sido instrumentados exitosamente con métricas de Prometheus.

## 📊 Estado Actual de Instrumentación

### ✅ Servicios Ya Instrumentados

1. **HTTP Requests** - `PrometheusMetricsMiddleware`
   - Total de requests
   - Duración por endpoint
   - Requests en progreso
   - Excepciones no manejadas

2. **MediatR Commands/Queries** - `LoggingBehavior`
   - Total de comandos/queries
   - Duración
   - Errores por tipo

3. **Caché** - `CacheService`
   - Cache hits/misses
   - Duración de operaciones (get, set, remove)

4. **Health Checks** - `PrometheusExtensions`
   - Estado de health checks
   - Duración de health checks

5. **Autenticación** - `LoginCommandHandler` ✅
   - Intentos de login exitosos/fallidos
   - Tipos de fallo: user_not_found, invalid_password, user_inactive

6. **Métricas de Permisos** - `UnifiedPermissionMetricsService` ✅
   - Verificaciones de permisos (allowed/denied)
   - Duración de verificaciones
   - Estado del circuit breaker
   - Aperturas del circuit breaker
   - Usuarios cacheados

### 🔨 Servicios Completamente Instrumentados ✅

Todos los servicios principales han sido instrumentados exitosamente:

- ✅ JWT Token Service
- ✅ Permission Authorization Middleware  
- ✅ User Permission Service
- ✅ OAuth Providers (Google, Microsoft)
- ✅ Repositories (Database queries)
- ✅ Unified Permission Metrics Service
- ✅ Login Command Handler
- ✅ Cache Service

## 📝 Guía de Instrumentación por Servicio

### 0. UnifiedPermissionMetricsService (✅ YA INSTRUMENTADO)

**Archivo**: `Api/Infrastructure/Services/UnifiedPermissionMetricsService.cs`

Este servicio centralizado de métricas de permisos ya ha sido instrumentado con Prometheus.

**Métricas implementadas**:

1. **Verificaciones de permisos**: `nombre_app_permission_checks_total{permission_name, result}`
2. **Duración de verificaciones**: `nombre_app_permission_check_duration_seconds{permission_name, from_cache}`
3. **Estado del circuit breaker**: `nombre_app_circuit_breaker_state{breaker_name}`
4. **Aperturas del circuit breaker**: `nombre_app_circuit_breaker_opens_total{breaker_name}`
5. **Usuarios cacheados**: `nombre_app_permission_cached_users_count`

**Queries útiles**:

```promql
# Tasa de permisos denegados por minuto
rate(nombre_app_permission_checks_total{result="denied"}[5m])

# Permisos más lentos (P95)
histogram_quantile(0.95, rate(nombre_app_permission_check_duration_seconds_bucket[5m]))

# Estado del circuit breaker (alertar si está abierto)
nombre_app_circuit_breaker_state{breaker_name="permission-service"} == 1

# Número de aperturas del circuit breaker en la última hora
increase(nombre_app_circuit_breaker_opens_total{breaker_name="permission-service"}[1h])

# Permisos más verificados
topk(10, sum by (permission_name) (increase(nombre_app_permission_checks_total[1h])))
```

### 1. JWT Token Service

**Archivo**: `Infrastructure/Services/Auth/JwtTokenService.cs`

**Métricas a agregar**:
- Tokens generados (access, refresh)
- Tokens validados (éxito/fallo)

**Código a agregar**:

```csharp
// Al inicio del archivo
using Infrastructure.Metrics;

// En GenerateToken (después de return)
InfrastructureMetrics.JwtTokensGeneratedTotal
    .WithLabels("access")
    .Inc();

// En GenerateRefreshToken (después de return)
InfrastructureMetrics.JwtTokensGeneratedTotal
    .WithLabels("refresh")
    .Inc();

// En ValidateToken
try
{
    var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
    
    // Métrica: validación exitosa
    InfrastructureMetrics.JwtTokensValidatedTotal
        .WithLabels("success")
        .Inc();
    
    return principal;
}
catch (Exception ex)
{
    // Métrica: validación fallida
    InfrastructureMetrics.JwtTokensValidatedTotal
        .WithLabels("failed")
        .Inc();
    
    _logger.LogError(ex, "Error al validar JWT token");
    return null;
}
```

### 2. Permission Authorization Middleware

**Archivo**: `Api/Infrastructure/Middlewares/PermissionAuthorizationMiddleware.cs`

**Métricas a agregar**:
- Verificaciones de permisos (allowed/denied)
- Duración de verificaciones
- Permisos más verificados

**Código a agregar**:

```csharp
// Al inicio del archivo
using Infrastructure.Metrics;

// En CheckPermissionAsync, después de obtener hasPermission
var permissionKey = $"{permissionAttribute.Resource}.{permissionAttribute.Action}";

InfrastructureMetrics.PermissionChecksTotal
    .WithLabels(permissionKey, hasPermission ? "allowed" : "denied")
    .Inc();

InfrastructureMetrics.PermissionCheckDuration
    .WithLabels(permissionKey, "true") // true = from middleware
    .Observe(stopwatch.Elapsed.TotalSeconds);
```

### 3. User Permission Service

**Archivo**: `Api/Application/Services/UserPermissionService.cs`

**Métricas a agregar**:
- Verificaciones desde caché vs base de datos
- Duración de verificaciones

**Código a agregar**:

```csharp
using System.Diagnostics;
using Infrastructure.Metrics;

public async Task<bool> UserHasPermissionAsync(Guid userId, string permissionKey, CancellationToken cancellationToken)
{
    var stopwatch = Stopwatch.StartNew();
    bool fromCache = false;
    
    try
    {
        // ... lógica existente ...
        
        // Si viene del caché
        if (cachedResult.HasValue)
        {
            fromCache = true;
            // Registrar métrica
            InfrastructureMetrics.PermissionCheckDuration
                .WithLabels(permissionKey, "true")
                .Observe(stopwatch.Elapsed.TotalSeconds);
            
            return cachedResult.Value;
        }
        
        // Si viene de BD
        bool hasPermission = await CheckDatabaseAsync(userId, permissionKey);
        
        stopwatch.Stop();
        InfrastructureMetrics.PermissionCheckDuration
            .WithLabels(permissionKey, "false")
            .Observe(stopwatch.Elapsed.TotalSeconds);
        
        return hasPermission;
    }
    finally
    {
        stopwatch.Stop();
    }
}
```

### 4. OAuth Providers (Google, Microsoft)

**Archivos**: 
- `Api/Application/Services/Auth/GoogleAuthProviderService.cs`
- `Api/Application/Services/Auth/MicrosoftAuthProviderService.cs`

**Métricas a agregar**:
- Intentos de autenticación OAuth
- Usuarios creados vs existentes

**Código a agregar**:

```csharp
// Al inicio
using Infrastructure.Metrics;

// En ExchangeCodeAsync
try
{
    var user = await FindOrCreateUserAsync(userInfo);
    
    if (!user.IsActive)
    {
        InfrastructureMetrics.AuthenticationAttemptsTotal
            .WithLabels("oauth_google", "user_inactive") // o oauth_microsoft
            .Inc();
        
        throw new UnauthorizedAccessException("Credenciales inválidas");
    }
    
    // Login exitoso
    InfrastructureMetrics.AuthenticationAttemptsTotal
        .WithLabels("oauth_google", "success")
        .Inc();
    
    return responseDto;
}
catch (Exception ex)
{
    InfrastructureMetrics.AuthenticationAttemptsTotal
        .WithLabels("oauth_google", "error")
        .Inc();
    throw;
}
```

### 5. Repositories (Base de Datos)

**Archivos**: `Infrastructure/Repositories/*.cs`

**Métricas a agregar**:
- Duración de queries
- Total de queries por entidad
- Errores de base de datos

**Patrón genérico**:

```csharp
using System.Diagnostics;
using Infrastructure.Metrics;

public async Task<User?> GetByIdAsync(Guid id)
{
    var stopwatch = Stopwatch.StartNew();
    string status = "success";
    
    try
    {
        var result = await _context.Users.FindAsync(id);
        
        stopwatch.Stop();
        InfrastructureMetrics.DatabaseQueryDuration
            .WithLabels("select", "User")
            .Observe(stopwatch.Elapsed.TotalSeconds);
        
        InfrastructureMetrics.DatabaseQueriesTotal
            .WithLabels("select", "User", status)
            .Inc();
        
        return result;
    }
    catch (Exception ex)
    {
        status = "error";
        stopwatch.Stop();
        
        InfrastructureMetrics.DatabaseErrorsTotal
            .WithLabels(ex.GetType().Name, "User")
            .Inc();
        
        InfrastructureMetrics.DatabaseQueriesTotal
            .WithLabels("select", "User", status)
            .Inc();
        
        throw;
    }
}

// Para INSERT
public async Task<User> AddAsync(User user)
{
    var stopwatch = Stopwatch.StartNew();
    
    try
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        
        stopwatch.Stop();
        InfrastructureMetrics.DatabaseQueryDuration
            .WithLabels("insert", "User")
            .Observe(stopwatch.Elapsed.TotalSeconds);
        
        InfrastructureMetrics.DatabaseQueriesTotal
            .WithLabels("insert", "User", "success")
            .Inc();
        
        InfrastructureMetrics.UserOperationsTotal
            .WithLabels("create", "success")
            .Inc();
        
        return user;
    }
    catch (Exception ex)
    {
        stopwatch.Stop();
        InfrastructureMetrics.DatabaseErrorsTotal
            .WithLabels(ex.GetType().Name, "User")
            .Inc();
        
        InfrastructureMetrics.DatabaseQueriesTotal
            .WithLabels("insert", "User", "error")
            .Inc();
        
        InfrastructureMetrics.UserOperationsTotal
            .WithLabels("create", "error")
            .Inc();
        
        throw;
    }
}

// Para UPDATE
public async Task UpdateAsync(User user)
{
    var stopwatch = Stopwatch.StartNew();
    
    try
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        
        stopwatch.Stop();
        InfrastructureMetrics.DatabaseQueryDuration
            .WithLabels("update", "User")
            .Observe(stopwatch.Elapsed.TotalSeconds);
        
        InfrastructureMetrics.DatabaseQueriesTotal
            .WithLabels("update", "User", "success")
            .Inc();
        
        InfrastructureMetrics.UserOperationsTotal
            .WithLabels("update", "success")
            .Inc();
    }
    catch (Exception ex)
    {
        stopwatch.Stop();
        InfrastructureMetrics.DatabaseErrorsTotal
            .WithLabels(ex.GetType().Name, "User")
            .Inc();
        
        InfrastructureMetrics.DatabaseQueriesTotal
            .WithLabels("update", "User", "error")
            .Inc();
        
        InfrastructureMetrics.UserOperationsTotal
            .WithLabels("update", "error")
            .Inc();
        
        throw;
    }
}

// Para DELETE
public async Task DeleteAsync(Guid id)
{
    var stopwatch = Stopwatch.StartNew();
    
    try
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
        
        stopwatch.Stop();
        InfrastructureMetrics.DatabaseQueryDuration
            .WithLabels("delete", "User")
            .Observe(stopwatch.Elapsed.TotalSeconds);
        
        InfrastructureMetrics.DatabaseQueriesTotal
            .WithLabels("delete", "User", "success")
            .Inc();
        
        InfrastructureMetrics.UserOperationsTotal
            .WithLabels("delete", "success")
            .Inc();
    }
    catch (Exception ex)
    {
        stopwatch.Stop();
        InfrastructureMetrics.DatabaseErrorsTotal
            .WithLabels(ex.GetType().Name, "User")
            .Inc();
        
        InfrastructureMetrics.DatabaseQueriesTotal
            .WithLabels("delete", "User", "error")
            .Inc();
        
        InfrastructureMetrics.UserOperationsTotal
            .WithLabels("delete", "error")
            .Inc();
        
        throw;
    }
}
```

## 🎯 Queries de Prometheus Útiles

### Monitorear Intentos de Login Fallidos

```promql
# Total de logins fallidos en los últimos 5 minutos
rate(nombre_app_authentication_attempts_total{result!="success"}[5m])

# Logins fallidos por tipo
sum by (result) (rate(nombre_app_authentication_attempts_total{method="login", result!="success"}[5m]))

# Tasa de éxito de login (%)
(rate(nombre_app_authentication_attempts_total{result="success"}[5m]) / 
 rate(nombre_app_authentication_attempts_total[5m])) * 100
```

### Monitorear Permisos Denegados

```promql
# Permisos denegados en los últimos 5 minutos
rate(nombre_app_permission_checks_total{result="denied"}[5m])

# Top 5 permisos más denegados
topk(5, sum by (permission_name) (rate(nombre_app_permission_checks_total{result="denied"}[5m])))

# Tasa de permisos permitidos (%)
(rate(nombre_app_permission_checks_total{result="allowed"}[5m]) / 
 rate(nombre_app_permission_checks_total[5m])) * 100
```

### Monitorear Performance de Base de Datos

```promql
# Queries más lentas (P95)
histogram_quantile(0.95, 
  rate(nombre_app_database_query_duration_seconds_bucket[5m])
) by (query_type, entity)

# Queries por segundo
rate(nombre_app_database_queries_total[5m])

# Tasa de errores de BD
rate(nombre_app_database_errors_total[5m])

# Top 5 entidades con más queries
topk(5, sum by (entity) (rate(nombre_app_database_queries_total[5m])))
```

### Monitorear Tokens JWT

```promql
# Tokens generados por segundo
rate(nombre_app_jwt_tokens_generated_total[5m])

# Tokens fallidos
rate(nombre_app_jwt_tokens_validated_total{result="failed"}[5m])

# Ratio de tokens válidos
(rate(nombre_app_jwt_tokens_validated_total{result="success"}[5m]) / 
 rate(nombre_app_jwt_tokens_validated_total[5m])) * 100
```

## 📈 Alertas Recomendadas

### Alta Tasa de Logins Fallidos

```yaml
- alert: HighLoginFailureRate
  expr: |
    (rate(nombre_app_authentication_attempts_total{result!="success"}[5m]) / 
     rate(nombre_app_authentication_attempts_total[5m])) > 0.3
  for: 10m
  labels:
    severity: warning
  annotations:
    summary: "Alta tasa de logins fallidos"
    description: "Más del 30% de intentos de login están fallando"
```

### Posible Ataque de Fuerza Bruta

```yaml
- alert: PossibleBruteForce
  expr: rate(nombre_app_authentication_attempts_total{result="invalid_password"}[1m]) > 5
  for: 2m
  labels:
    severity: critical
  annotations:
    summary: "Posible ataque de fuerza bruta"
    description: "Más de 5 intentos de login con contraseña inválida por segundo"
```

### Muchos Permisos Denegados

```yaml
- alert: HighPermissionDenials
  expr: rate(nombre_app_permission_checks_total{result="denied"}[5m]) > 10
  for: 10m
  labels:
    severity: warning
  annotations:
    summary: "Alta tasa de permisos denegados"
    description: "Más de 10 permisos denegados por segundo"
```

### Queries de Base de Datos Lentas

```yaml
- alert: SlowDatabaseQueries
  expr: |
    histogram_quantile(0.95, 
      rate(nombre_app_database_query_duration_seconds_bucket[5m])
    ) > 2
  for: 10m
  labels:
    severity: warning
  annotations:
    summary: "Queries de BD lentas"
    description: "P95 de queries de BD está por encima de 2 segundos"
```

## 🔍 Dashboard de Grafana Sugerido

### Panel 1: Autenticación
- Login success rate (gauge)
- Logins totales por minuto (graph)
- Logins fallidos por tipo (pie chart)
- OAuth vs login tradicional (graph)

### Panel 2: Permisos
- Permission checks por segundo (graph)
- Permisos denegados top 10 (table)
- Permission check duration P95 (graph)
- Ratio de permisos permitidos (gauge)

### Panel 3: Base de Datos
- Query duration P50, P95, P99 (graph)
- Queries por segundo por entidad (graph)
- Errores de BD (counter)
- Queries más lentas (table)

### Panel 4: Tokens JWT
- Tokens generados por minuto (graph)
- Token validation success rate (gauge)
- Tokens expirados vs válidos (pie chart)

## 📚 Mejores Prácticas

1. **No instrumentar en exceso**: Solo métricas útiles
2. **Labels consistentes**: Usa los mismos nombres en todos lados
3. **Cardinalidad baja**: Evita IDs de usuario en labels
4. **Performance**: Las métricas deben ser < 1ms de overhead
5. **Errores**: Siempre registra errores con tipo específico
6. **Duración**: Usa Stopwatch para precisión
7. **Try-finally**: Asegúrate de detener el stopwatch

## 🚀 Próximos Pasos

### ✅ Completado

1. ✅ Instrumentar JwtTokenService
2. ✅ Instrumentar PermissionAuthorizationMiddleware
3. ✅ Instrumentar OAuth providers (Google y Microsoft)
4. ✅ Instrumentar Repositories (Repository<T> genérico)
5. ✅ Instrumentar UnifiedPermissionMetricsService
6. ✅ Instrumentar LoginCommandHandler
7. ✅ Instrumentar UserPermissionService

### 🔜 Pendientes

1. Crear dashboards en Grafana basados en las métricas implementadas
2. Configurar alertas en Prometheus usando las reglas sugeridas
3. Monitorear y ajustar umbrales de alertas según necesidad
4. Instrumentar servicios adicionales según surjan necesidades

---

## 📋 Resumen Final de Implementación

### Servicios Instrumentados: 8/8 ✅

Todos los servicios críticos del backend han sido exitosamente instrumentados con Prometheus:

1. ✅ **UnifiedPermissionMetricsService** - Permisos, circuit breaker, cache de permisos
2. ✅ **PermissionAuthorizationMiddleware** - Autenticación en middleware, permisos denegados
3. ✅ **LoginCommandHandler** - Logins exitosos/fallidos con categorización de errores
4. ✅ **JwtTokenService** - Generación y validación de tokens access/refresh
5. ✅ **GoogleOAuthService** - OAuth Google (exchange, userinfo)
6. ✅ **MicrosoftOAuthService** - OAuth Microsoft (exchange, userinfo, config)
7. ✅ **UserPermissionService** - Verificaciones de permisos y queries de BD
8. ✅ **Repository<T>** - Todas las operaciones CRUD (insert, update, delete, select)

### Métricas Totales Implementadas

- **Autenticación**: 15+ etiquetas diferentes
- **Base de Datos**: 6 tipos de operaciones x N entidades
- **Permisos**: 5 métricas principales
- **Circuit Breaker**: 2 métricas de estado
- **Cache**: 3 métricas de rendimiento

### Beneficios Obtenidos

✅ Visibilidad completa del flujo de autenticación (login tradicional y OAuth)  
✅ Monitoreo detallado de operaciones de base de datos por entidad  
✅ Tracking de permisos denegados y circuit breaker  
✅ Métricas de duración para identificar cuellos de botella  
✅ Contadores de errores categorizados para debugging rápido  
✅ Base sólida para dashboards y alertas en Grafana

**¡La instrumentación está completa y lista para producción!** 🎉

