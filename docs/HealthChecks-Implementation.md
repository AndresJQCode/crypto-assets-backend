# Health Checks Implementation

## Descripción General

Se ha implementado un sistema **simplificado** de Health Checks para monitorear la **disponibilidad** de la aplicación. Los health checks se enfocan en determinar si la aplicación está viva y lista para recibir tráfico, mientras que las **métricas detalladas** se manejan con **Prometheus**.

## 🎯 Filosofía: Health Checks Simplificados

### ¿Por qué simplificar?

Con la implementación de Prometheus, los health checks se han simplificado para:

1. **Separar responsabilidades**: Health checks = disponibilidad, Prometheus = observabilidad
2. **Mejorar rendimiento**: Health checks más rápidos (< 100ms)
3. **Reducir complejidad**: Solo verificar disponibilidad (Sí/No)
4. **Evitar duplicación**: Métricas detalladas van a Prometheus

### Health Checks vs Prometheus

| Aspecto | Health Checks | Prometheus |
|---------|---------------|------------|
| **Propósito** | ¿Está disponible? | ¿Cómo está funcionando? |
| **Respuesta** | Healthy/Unhealthy | Métricas numéricas |
| **Frecuencia** | 5-30 segundos | 15-60 segundos |
| **Uso** | K8s probes, LB | Dashboards, alertas |
| **Detalles** | Mínimos | Completos |
| **Duración** | < 100ms | Puede ser más largo |

## Componentes Implementados

**Ubicación en el código**: Registro de checks en `Api/Extensions/Extensions.cs` (`AddCustomHealthChecks`); endpoints en `Api/Extensions/CommonExtensions.cs` (`MapDefaultHealthChecks`). Con Prometheus, no se usa `MapHealthEndpoints` (/health/status y /health/ui fueron eliminados; el estado completo se obtiene en `/health`).

### 1. Health Checks Activos (Simplificados)

Todos los health checks han sido simplificados para solo verificar **disponibilidad** sin métricas detalladas. Las clases custom están en `Api/Infrastructure/HealthChecks/`.

#### 1. `self` - API Check
- **Tipo**: Función lambda
- **Propósito**: Verificar que la API está respondiendo
- **Timeout**: N/A (instantáneo)
- **Tags**: `api`, `ready`

#### 2. `database` - PostgreSQL Check  
- **Tipo**: Built-in NpgSql
- **Propósito**: Verificar conectividad a PostgreSQL
- **Timeout**: 5 segundos
- **Tags**: `db`, `ready`
- **Nota**: Solo verifica conectividad, sin métricas

#### 3. `cache` - Memory Cache Check
- **Ubicación**: `Api/Infrastructure/HealthChecks/MemoryCacheHealthCheck.cs`
- **Propósito**: Verificar que el caché funciona (write/read test)
- **Timeout**: 3 segundos
- **Tags**: `cache`, `ready`
- **Verificación**: Escribe y lee un valor de prueba

#### 4. `identity` - Identity Check
- **Ubicación**: `Api/Infrastructure/HealthChecks/IdentityHealthCheck.cs`
- **Propósito**: Verificar que Identity está accesible
- **Timeout**: 3 segundos
- **Tags**: `identity`, `ready`
- **Verificación**: Acceso a UserManager y RoleManager

#### 5. `email-config` - Email Service Check
- **Ubicación**: `Api/Infrastructure/HealthChecks/EmailServiceHealthCheck.cs`
- **Propósito**: Verificar configuración de email (Infobip)
- **Timeout**: 2 segundos
- **Tags**: `config`, `ready`
- **Verificación**: ApiKey y FromEmail configurados

**Nota**: No existe un health check de JWT porque la configuración JWT se valida al **inicio de la aplicación** mediante `JwtConfigurationValidator` y `ValidateOnStart()` en `Api/Extensions/Extensions.cs`. Si JWT está mal configurado, la aplicación no arranca.

### 2. Health Checks Eliminados

Los siguientes health checks fueron **eliminados** porque sus métricas ahora se capturan en **Prometheus**:

- ❌ `SystemHealthCheck` → CPU, memoria, threads en Prometheus
- ❌ `DiskSpaceHealthCheck` → Espacio en disco en Prometheus
- ❌ `DatabaseHealthCheck` (detallado) → Métricas de BD en Prometheus
- ❌ `PermissionCacheHealthCheck` (detallado) → Métricas de caché en Prometheus
- ❌ `ExternalServicesHealthCheck` → Ralentizaba health checks
- ❌ `JwtConfigurationHealthCheck` → Redundante: JWT se valida al inicio con `JwtConfigurationValidator` y `ValidateOnStart()`

### 3. Endpoints de Health Check

#### `/health` - Health Check Completo
- **Método**: GET
- **Descripción**: Devuelve el estado de todos los componentes del sistema
- **Formato**: JSON (formato HealthChecks.UI Client: status, totalDuration, entries por nombre)
- **Códigos de estado**:
  - `200 OK`: Sistema saludable o degradado
  - `503 Service Unavailable`: Sistema no saludable

**Ejemplo de respuesta** (formato simplificado; los checks actuales no incluyen métricas detalladas en `data`):
```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.0234567",
  "entries": {
    "self": { "status": "Healthy", "description": "API funcionando", "duration": "00:00:00.0000123" },
    "database": { "status": "Healthy", "description": null, "duration": "00:00:00.0123456" },
    "cache": { "status": "Healthy", "description": "Caché disponible", "duration": "00:00:00.0012345" },
    "identity": { "status": "Healthy", "description": "Identity disponible", "duration": "00:00:00.0023456" },
    "email-config": { "status": "Healthy", "description": "Email configurado", "duration": "00:00:00.0001234" }
  }
}
```

#### `/health/live` - Liveness Probe
- **Método**: GET
- **Descripción**: Verifica si la API está viva (solo chequea el servicio básico)
- **Uso**: Kubernetes liveness probe
- **Formato**: JSON simple
- **Códigos de estado**: `200 OK` o `503 Service Unavailable`

**Ejemplo de respuesta**:
```json
{
  "status": "Healthy",
  "timestamp": "2025-10-21T10:30:00Z"
}
```

#### `/health/ready` - Readiness Probe
- **Método**: GET
- **Descripción**: Verifica si la API está lista para recibir tráfico
- **Uso**: Kubernetes readiness probe
- **Verifica**: Base de datos, caché, Identity
- **Códigos de estado**: 
  - `200 OK`: Listo para recibir tráfico
  - `503 Service Unavailable`: No listo

#### `/health/db` - Database Health Check
- **Método**: GET
- **Descripción**: Verifica solo el estado de la base de datos
- **Formato**: JSON detallado

#### Endpoints Legacy (Compatibilidad)
- `/hc`: Health check completo (formato legacy)
- `/liveness`: Liveness check simple (formato legacy)

## Configuración

### Tags de Health Checks

Los health checks están organizados por tags para facilitar el filtrado (definidos en `Api/Extensions/Extensions.cs`):

- `api`: Check básico de la API (self)
- `ready`: Checks para determinar si está listo para tráfico (todos los críticos)
- `db`: Base de datos (PostgreSQL)
- `cache`: Caché en memoria
- `identity`: Identity (UserManager/RoleManager)
- `config`: Configuración (email-config)

### Timeouts

Cada health check tiene un timeout configurado en `AddCustomHealthChecks`:

- `self`: instantáneo
- `database`: 5 segundos
- `cache`: 3 segundos
- `identity`: 3 segundos
- `email-config`: 2 segundos

### Estados de Health Check

1. **Healthy** (Saludable): 
   - Todo funciona correctamente
   - Código HTTP: 200 OK

2. **Degraded** (Degradado):
   - Funciona pero con rendimiento reducido
   - Ejemplo: Base de datos lenta (>1000ms)
   - Código HTTP: 200 OK

3. **Unhealthy** (No saludable):
   - Componente no funciona
   - Código HTTP: 503 Service Unavailable

## Uso con Kubernetes

### Liveness Probe
```yaml
livenessProbe:
  httpGet:
    path: /health/live
    port: 8080
  initialDelaySeconds: 30
  periodSeconds: 10
  timeoutSeconds: 5
  failureThreshold: 3
```

### Readiness Probe
```yaml
readinessProbe:
  httpGet:
    path: /health/ready
    port: 8080
  initialDelaySeconds: 10
  periodSeconds: 5
  timeoutSeconds: 5
  failureThreshold: 3
```

## Uso con Docker

### Docker Compose Healthcheck
```yaml
services:
  api:
    image: nombre-app-api
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health/live"]
      interval: 30s
      timeout: 5s
      retries: 3
      start_period: 40s
```

## Monitoreo y Alertas

### Prometheus (Opcional)
Las métricas de la aplicación (incluidas las de health checks cuando se publiquen) se exponen en `/metrics`. Configuración típica de scrape:

```yaml
- job_name: 'nombre-app-api'
  metrics_path: '/metrics'
  scrape_interval: 30s
  static_configs:
    - targets: ['localhost:8080']
```

La aplicación define métricas de health en `ApiMetrics.HealthCheckStatus` y `ApiMetrics.HealthCheckDuration`; la publicación desde el reporte de health se hace mediante `PrometheusExtensions.PublishHealthCheckMetrics` (integrar en pipeline de health si se desea exponer estado en Prometheus).

### Integración con Load Balancers

Los endpoints `/health/ready` y `/health/live` son ideales para:
- Azure Application Gateway
- AWS Application Load Balancer
- NGINX
- HAProxy

Ejemplo de configuración NGINX:
```nginx
upstream api_backend {
    server api1:8080 max_fails=3 fail_timeout=30s;
    server api2:8080 max_fails=3 fail_timeout=30s;
}

server {
    location / {
        proxy_pass http://api_backend;
        health_check uri=/health/ready interval=10s;
    }
}
```

## Testing

### Pruebas Locales

1. **Health check completo**:
   ```bash
   curl http://localhost:5224/health
   ```

2. **Liveness check**:
   ```bash
   curl http://localhost:5224/health/live
   ```

3. **Readiness check**:
   ```bash
   curl http://localhost:5224/health/ready
   ```

### Pruebas Automatizadas

```bash
# Script de prueba
#!/bin/bash

# Verificar liveness
if curl -f http://localhost:5224/health/live; then
    echo "✅ API está viva"
else
    echo "❌ API no responde"
    exit 1
fi

# Verificar readiness
if curl -f http://localhost:5224/health/ready; then
    echo "✅ API está lista"
else
    echo "❌ API no está lista"
    exit 1
fi
```

## Extensión

### Agregar un Nuevo Health Check

1. Crear una nueva clase en `Api/Infrastructure/HealthChecks/`:

```csharp
public class CustomHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Implementar lógica de verificación
            
            return HealthCheckResult.Healthy("Componente funcionando");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Error", exception: ex);
        }
    }
}
```

2. Registrar en `Api/Extensions/Extensions.cs`, dentro del método `AddCustomHealthChecks`:

```csharp
.AddCheck<CustomHealthCheck>(
    name: "custom",
    tags: new[] { "custom", "ready" },
    timeout: TimeSpan.FromSeconds(5))
```

## Mejores Prácticas (Health Checks Simplificados)

1. **Mantenerlos simples**: Solo verificar disponibilidad (Sí/No)
   - ❌ NO: Contar registros, medir tiempos, estadísticas
   - ✅ SÍ: ¿Puedo conectarme? ¿Funciona?

2. **Rápidos**: Deben completarse en < 100ms total
   - Health checks lentos ralentizan los probes de Kubernetes
   - Pueden causar reinicios innecesarios del pod

3. **Sin métricas detalladas**: Las métricas van a Prometheus
   - ❌ NO: Tiempo de respuesta, conteo de items
   - ✅ SÍ: Solo estado (Healthy/Unhealthy)

4. **Timeouts cortos**: 2-5 segundos máximo
   - Componentes lentos probablemente están caídos de todos modos

5. **Tags apropiados**: Usa el tag `ready` para checks críticos
   - Kubernetes readiness probe usa checks con tag `ready`

6. **No incluir información sensible**: Evitar exponer credenciales

7. **Logging mínimo**: Solo logear fallos, no éxitos

## Troubleshooting

### Health Check Falla

1. Revisar logs de la aplicación
2. Verificar conectividad de base de datos
3. Verificar recursos del sistema (memoria, CPU)
4. Revisar el endpoint `/health` para el estado en JSON

### Health Check Lento

1. Revisar métricas en `/health` para identificar el componente lento
2. Verificar timeouts de base de datos
3. Verificar carga del sistema

## 📊 Integración con Prometheus

Para métricas detalladas de health checks y otros aspectos de la aplicación, consulta:

**[Prometheus Implementation Guide](./Prometheus-Implementation.md)**

Prometheus proporciona:
- ✅ Métricas de estado de health checks (`nombre_app_healthcheck_status`)
- ✅ Duración de health checks (`nombre_app_healthcheck_duration_seconds`)
- ✅ Métricas de recursos del sistema (CPU, memoria, threads)
- ✅ Métricas de rendimiento de base de datos
- ✅ Métricas de caché (hits, misses, latencia)
- ✅ Métricas de permisos
- ✅ Y mucho más...

## Referencias

- [ASP.NET Core Health Checks](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)
- [Kubernetes Liveness and Readiness Probes](https://kubernetes.io/docs/tasks/configure-pod-container/configure-liveness-readiness-startup-probes/)
- [Health Checks UI Client](https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks)
- [Prometheus Implementation](./Prometheus-Implementation.md)

