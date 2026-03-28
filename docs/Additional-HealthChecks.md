# Health Checks Adicionales Implementados

## 📋 Resumen

Se han implementado 5 health checks adicionales específicos para la aplicación, complementando los 4 health checks básicos existentes.

## 🆕 Nuevos Health Checks

### 1. **EmailServiceHealthCheck** ⭐
**Ubicación**: `Api/Infrastructure/HealthChecks/EmailServiceHealthCheck.cs`

**Propósito**: Verificar que el servicio de email (Infobip) esté correctamente configurado.

**Verifica**:
- ✅ ApiKey configurada
- ✅ FromEmail configurado
- ✅ FromName configurado
- ✅ Configuración completa presente

**Tags**: `email`, `config`, `ready`

**Por qué es importante**: El sistema envía emails críticos (reset de contraseña, confirmaciones). Si el servicio de email no está configurado, estas funcionalidades fallarán.

**Métricas devueltas**:
```json
{
  "fromEmail": "hola@info.qcode.app",
  "fromName": "Nombre de la aplicación",
  "configured": true
}
```

---

### 2. **JwtConfigurationHealthCheck** ⭐
**Ubicación**: `Api/Infrastructure/HealthChecks/JwtConfigurationHealthCheck.cs`

**Propósito**: Verificar que la configuración JWT esté correcta y sea segura.

**Verifica**:
- ✅ SecretKey presente y de longitud adecuada (mínimo 32 caracteres)
- ✅ Issuer configurado
- ✅ Audience configurado
- ✅ Tiempos de expiración válidos (ExpirationMinutes > 0)
- ✅ RefreshTokenExpirationDays válido (> 0)

**Tags**: `jwt`, `auth`, `config`, `ready`

**Por qué es importante**: Una configuración JWT incorrecta puede comprometer la seguridad de toda la aplicación o impedir que los usuarios se autentiquen.

**Métricas devueltas**:
```json
{
  "issuer": "qcode.co",
  "audience": "qcode.co",
  "expirationMinutes": 30,
  "refreshTokenExpirationDays": 10,
  "secretKeyLength": 44
}
```

**Estados**:
- ❌ **Unhealthy**: SecretKey faltante, muy corta, o configuración inválida
- ✅ **Healthy**: Todas las configuraciones correctas

---

### 3. **PermissionCacheHealthCheck** ⭐
**Ubicación**: `Api/Infrastructure/HealthChecks/PermissionCacheHealthCheck.cs`

**Propósito**: Verificar que el sistema de caché de permisos funcione correctamente.

**Verifica**:
- ✅ IPermissionCacheService disponible y operacional
- ✅ Tiempo de respuesta del caché
- ✅ Capacidad de leer del caché

**Tags**: `cache`, `permissions`, `ready`

**Por qué es importante**: Tu aplicación tiene un sistema complejo de permisos con caché. Si el caché falla, el rendimiento se degrada significativamente y cada request tiene que ir a la base de datos.

**Métricas devueltas**:
```json
{
  "responseTimeMs": 2.34,
  "status": "operational"
}
```

**Estados**:
- ❌ **Unhealthy**: Error al acceder al caché
- ⚠️ **Degraded**: Respuesta lenta (> 100ms)
- ✅ **Healthy**: Funcionando correctamente

---

### 4. **ExternalServicesHealthCheck**
**Ubicación**: `Api/Infrastructure/HealthChecks/ExternalServicesHealthCheck.cs`

**Propósito**: Verificar conectividad con servicios OAuth externos (Google, Microsoft).

**Verifica**:
- 🌐 Google OAuth endpoint (.well-known/openid-configuration)
- 🌐 Microsoft OAuth endpoint (.well-known/openid-configuration)

**Tags**: `external`, `oauth`

**Por qué es importante**: Los usuarios pueden autenticarse con Google o Microsoft. Si estos servicios no están accesibles, esas opciones de login fallarán.

**Métricas devueltas**:
```json
{
  "google_oauth": "reachable",
  "microsoft_oauth": "reachable"
}
```

**Estados**:
- ❌ **Unhealthy**: Error al verificar servicios
- ⚠️ **Degraded**: Algunos servicios no accesibles (pero la app puede funcionar)
- ✅ **Healthy**: Todos los servicios accesibles

**Nota**: Este health check tiene un timeout de 10 segundos (más largo) porque depende de servicios externos.

---

### 5. **DiskSpaceHealthCheck**
**Ubicación**: `Api/Infrastructure/HealthChecks/DiskSpaceHealthCheck.cs`

**Propósito**: Monitorear espacio disponible en disco para prevenir problemas de almacenamiento.

**Verifica**:
- 💾 Espacio libre en disco
- 💾 Porcentaje de uso del disco
- 💾 Comparación con umbrales configurables

**Tags**: `disk`, `resources`

**Por qué es importante**: Si el disco se llena, la aplicación puede fallar al escribir logs, guardar archivos temporales, o incluso al actualizar la base de datos.

**Configuración**:
```csharp
// Por defecto requiere mínimo 1GB libre
// Puedes configurar esto en el constructor
new DiskSpaceHealthCheck(logger, minimumFreeMegabytes: 2048) // 2GB
```

**Métricas devueltas**:
```json
{
  "driveName": "C:\\",
  "driveFormat": "NTFS",
  "totalSpaceMB": 500000,
  "freeSpaceMB": 50000,
  "usedSpaceMB": 450000,
  "percentUsed": 90.0
}
```

**Estados**:
- ❌ **Unhealthy**: Menos del espacio mínimo configurado disponible
- ⚠️ **Degraded**: Menos del doble del espacio mínimo O más del 85% usado
- ✅ **Healthy**: Espacio adecuado disponible

---

## 📊 Resumen de Todos los Health Checks

### Health Checks Básicos (Existentes)
1. ✅ **self** - API básica funcionando
2. ✅ **database** - PostgreSQL conectividad
3. ✅ **database-detailed** - PostgreSQL con métricas detalladas
4. ✅ **memory-cache** - Caché en memoria
5. ✅ **identity** - Sistema de usuarios y roles
6. ✅ **system** - Recursos del sistema (CPU, memoria, threads)

### Health Checks Nuevos
7. ⭐ **email-service** - Configuración de Email (Infobip)
8. ⭐ **jwt-config** - Configuración JWT
9. ⭐ **permission-cache** - Caché de permisos
10. 🌐 **external-services** - Servicios OAuth externos
11. 💾 **disk-space** - Espacio en disco

---

## 🏷️ Tags por Categoría

### Por Criticidad
- **ready**: Checks críticos para que la app reciba tráfico
  - `self`, `database`, `memory-cache`, `identity`, `email-service`, `jwt-config`, `permission-cache`

### Por Tipo
- **db**: `database`, `database-detailed`
- **cache**: `memory-cache`, `permission-cache`
- **auth**: `identity`, `jwt-config`
- **config**: `email-service`, `jwt-config`
- **resources**: `system`, `disk-space`
- **external**: `external-services`

---

## 📈 Endpoints Afectados

Todos estos health checks están disponibles en:

### Endpoints Públicos
- **`/health`** - Todos los checks
- **`/health/ready`** - Solo checks con tag `ready` (incluye nuevos checks críticos)
- **`/health/live`** - Solo check básico de API

---

## 🔧 Configuración Recomendada

### Para Producción
```csharp
// Habilitar todos excepto external-services si hay problemas de latencia
services.AddCustomHealthChecks(configuration)
    // Los checks ya están configurados automáticamente
```

### Para Desarrollo
Todos los checks están habilitados por defecto.

### Para Kubernetes

**Liveness Probe** (solo checks básicos):
```yaml
livenessProbe:
  httpGet:
    path: /health/live
    port: 8080
  initialDelaySeconds: 30
  periodSeconds: 10
```

**Readiness Probe** (incluye nuevos checks críticos):
```yaml
readinessProbe:
  httpGet:
    path: /health/ready
    port: 8080
  initialDelaySeconds: 10
  periodSeconds: 5
```

---

## 🎯 Recomendaciones de Uso

### Health Checks Críticos (Siempre Monitorear)
1. **database** - Sin BD la app no funciona
2. **jwt-config** - Sin JWT nadie puede autenticarse
3. **email-service** - Funciones críticas de usuario
4. **permission-cache** - Impacta rendimiento significativamente

### Health Checks Importantes (Monitorear en Producción)
1. **disk-space** - Prevenir fallas por disco lleno
2. **system** - Monitorear uso de recursos
3. **identity** - Sistema de usuarios

### Health Checks Opcionales (Nice to Have)
1. **external-services** - Puede degradar rendimiento del health check
   - Considerar deshabilitarlo si hay latencia de red
   - O aumentar el timeout si es necesario

---

## 🚀 Próximos Health Checks Sugeridos

Dependiendo de tus necesidades futuras, podrías agregar:

1. **Service Bus Health Check** - Si usas Azure Service Bus
2. **Key Vault Health Check** - Si usas Azure Key Vault
3. **Redis Health Check** - Si implementas Redis para caché distribuido
4. **File Storage Health Check** - Si guardas archivos en Azure Blob Storage o similar
5. **Background Jobs Health Check** - Para verificar servicios en background

---

## 📝 Testing

### Verificar un Check Específico

```bash
# Ver todos los checks
curl http://localhost:5224/health | jq

# Ver solo checks de configuración
curl http://localhost:5224/health/ready | jq

```

### Simular Fallos

Para probar que los health checks funcionan:

1. **Email Service**: Eliminar la configuración de EmailService en appsettings
2. **JWT Config**: Poner una SecretKey muy corta
3. **Disk Space**: Llenar el disco (no recomendado 😅)
4. **External Services**: Desconectar internet

---

## 📚 Referencias

- [ASP.NET Core Health Checks](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)
- [Health Checks in Microservices](https://microservices.io/patterns/observability/health-check-api.html)
- [Kubernetes Health Checks Best Practices](https://kubernetes.io/docs/tasks/configure-pod-container/configure-liveness-readiness-startup-probes/)

