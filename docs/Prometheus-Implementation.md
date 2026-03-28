# Implementación de Prometheus

## 📋 Descripción General

Se ha implementado un sistema completo de métricas de Prometheus para monitoreo detallado de la aplicación. Este sistema complementa los health checks simplificados y proporciona métricas de rendimiento, recursos y negocio en tiempo real.

## 🎯 Filosofía: Health Checks vs Prometheus

### Health Checks (Disponibilidad)
- **Propósito**: Determinar si la aplicación está viva y lista para recibir tráfico
- **Respuesta**: Binaria (Healthy/Unhealthy/Degraded)
- **Frecuencia**: Cada 5-30 segundos
- **Uso**: Kubernetes probes, load balancers
- **Complejidad**: Simple y rápido (< 100ms)

### Prometheus (Observabilidad)
- **Propósito**: Monitoreo detallado, histórico y alertas
- **Respuesta**: Métricas numéricas continuas
- **Frecuencia**: Cada 15-60 segundos
- **Uso**: Dashboards, análisis de tendencias, alertas
- **Complejidad**: Detallado y completo

## 📊 Métricas Implementadas

### 1. Métricas HTTP

#### `nombre_app_http_requests_total`
- **Tipo**: Counter
- **Labels**: `method`, `endpoint`, `status_code`
- **Descripción**: Total de requests HTTP recibidos
- **Uso**: Monitorear tráfico y errores HTTP

#### `nombre_app_http_request_duration_seconds`
- **Tipo**: Histogram
- **Labels**: `method`, `endpoint`
- **Descripción**: Duración de requests HTTP
- **Buckets**: 1ms a 16s (exponencial)
- **Uso**: Análisis de latencia por endpoint

#### `nombre_app_http_requests_in_progress`
- **Tipo**: Gauge
- **Labels**: `method`
- **Descripción**: Requests actualmente en progreso
- **Uso**: Monitorear carga en tiempo real

### 2. Métricas de MediatR (Commands/Queries)

#### `nombre_app_mediatr_requests_total`
- **Tipo**: Counter
- **Labels**: `request_type`, `request_name`, `status`
- **Descripción**: Total de comandos/queries ejecutados
- **Valores**: `request_type` = "command" | "query"

#### `nombre_app_mediatr_request_duration_seconds`
- **Tipo**: Histogram
- **Labels**: `request_type`, `request_name`
- **Descripción**: Duración de comandos/queries

#### `nombre_app_mediatr_request_errors_total`
- **Tipo**: Counter
- **Labels**: `request_type`, `request_name`, `error_type`
- **Descripción**: Errores en comandos/queries

### 3. Métricas de Caché

#### `nombre_app_cache_hits_total`
- **Tipo**: Counter
- **Labels**: `cache_name`, `key_type`
- **Descripción**: Total de cache hits
- **Uso**: Calcular hit rate del caché

#### `nombre_app_cache_misses_total`
- **Tipo**: Counter
- **Labels**: `cache_name`, `key_type`
- **Descripción**: Total de cache misses

#### `nombre_app_cache_operation_duration_seconds`
- **Tipo**: Histogram
- **Labels**: `operation`, `cache_name`
- **Descripción**: Duración de operaciones de caché
- **Operaciones**: get, set, remove

### 4. Métricas de Permisos

#### `nombre_app_permission_checks_total`
- **Tipo**: Counter
- **Labels**: `permission_name`, `result`
- **Descripción**: Total de verificaciones de permisos

#### `nombre_app_permission_check_duration_seconds`
- **Tipo**: Histogram
- **Labels**: `permission_name`, `from_cache`
- **Descripción**: Duración de verificaciones de permisos

#### `nombre_app_permission_cached_users_count`
- **Tipo**: Gauge
- **Descripción**: Usuarios con permisos en caché

### 5. Métricas de Base de Datos

#### `nombre_app_database_query_duration_seconds`
- **Tipo**: Histogram
- **Labels**: `query_type`, `entity`
- **Descripción**: Duración de queries

#### `nombre_app_database_queries_total`
- **Tipo**: Counter
- **Labels**: `query_type`, `entity`, `status`
- **Descripción**: Total de queries ejecutadas

#### `nombre_app_database_errors_total`
- **Tipo**: Counter
- **Labels**: `error_type`, `entity`
- **Descripción**: Errores de base de datos

### 6. Métricas de Autenticación

#### `nombre_app_authentication_attempts_total`
- **Tipo**: Counter
- **Labels**: `method`, `result`
- **Descripción**: Intentos de autenticación
- **Métodos**: login, oauth_google, oauth_microsoft

#### `nombre_app_jwt_tokens_generated_total`
- **Tipo**: Counter
- **Labels**: `token_type`
- **Descripción**: Tokens JWT generados

### 7. Métricas de Health Checks

#### `nombre_app_healthcheck_status`
- **Tipo**: Gauge
- **Labels**: `check_name`
- **Descripción**: Estado del health check
- **Valores**: 0 = unhealthy, 0.5 = degraded, 1 = healthy

#### `nombre_app_healthcheck_duration_seconds`
- **Tipo**: Histogram
- **Labels**: `check_name`
- **Descripción**: Duración de health checks

### 8. Métricas de Aplicación

#### `nombre_app_unhandled_exceptions_total`
- **Tipo**: Counter
- **Labels**: `exception_type`, `endpoint`
- **Descripción**: Excepciones no manejadas

#### `nombre_app_application_uptime_seconds`
- **Tipo**: Gauge
- **Descripción**: Tiempo desde inicio de la aplicación

#### `nombre_app_application_info`
- **Tipo**: Gauge
- **Labels**: `version`, `environment`
- **Descripción**: Información de la aplicación

### 9. Métricas de Circuit Breaker

#### `nombre_app_circuit_breaker_state`
- **Tipo**: Gauge
- **Labels**: `breaker_name`
- **Descripción**: Estado del circuit breaker
- **Valores**: 0 = closed, 0.5 = half-open, 1 = open

#### `nombre_app_circuit_breaker_opens_total`
- **Tipo**: Counter
- **Labels**: `breaker_name`
- **Descripción**: Veces que se abrió el circuit breaker

## 🔧 Endpoints de Métricas

### `/metrics`
- **Método**: GET
- **Descripción**: Endpoint principal de métricas en formato Prometheus
- **Formato**: Texto plano (formato Prometheus)
- **Uso**: Configurar como target en Prometheus

**Ejemplo de configuración de Prometheus:**
```yaml
scrape_configs:
  - job_name: 'nombre-app-api'
    metrics_path: '/metrics'
    scrape_interval: 15s
    static_configs:
      - targets: ['localhost:5224']
```

### `/metrics-text`
- **Método**: GET
- **Descripción**: Información básica sobre métricas (debugging)
- **Formato**: Texto plano legible
- **Uso**: Verificación manual

## 📈 Queries Útiles de PromQL

### Tasa de Requests HTTP
```promql
rate(nombre_app_http_requests_total[5m])
```

### Tasa de Errores HTTP (5xx)
```promql
rate(nombre_app_http_requests_total{status_code=~"5.."}[5m])
```

### Latencia P95 de Requests HTTP
```promql
histogram_quantile(0.95, rate(nombre_app_http_request_duration_seconds_bucket[5m]))
```

### Latencia P99 por Endpoint
```promql
histogram_quantile(0.99, 
  rate(nombre_app_http_request_duration_seconds_bucket[5m]) by (endpoint, le)
)
```

### Cache Hit Rate
```promql
rate(nombre_app_cache_hits_total[5m]) / 
(rate(nombre_app_cache_hits_total[5m]) + rate(nombre_app_cache_misses_total[5m]))
```

### Tasa de Errores de MediatR
```promql
rate(nombre_app_mediatr_request_errors_total[5m])
```

### Comandos más Lentos (Top 5)
```promql
topk(5, 
  histogram_quantile(0.95, 
    rate(nombre_app_mediatr_request_duration_seconds_bucket[5m]) by (request_name, le)
  )
)
```

### Requests Activos en Progreso
```promql
sum(nombre_app_http_requests_in_progress)
```

### Uptime de la Aplicación (en horas)
```promql
nombre_app_application_uptime_seconds / 3600
```

### Health Checks en Estado Unhealthy
```promql
nombre_app_healthcheck_status{check_name!=""} == 0
```

### Duración de Queries de Base de Datos P95
```promql
histogram_quantile(0.95, 
  rate(nombre_app_database_query_duration_seconds_bucket[5m])
)
```

## 🎨 Dashboards Recomendados de Grafana

### Dashboard 1: Overview de la Aplicación
- Uptime
- Requests por segundo
- Tasa de errores
- Latencia P95/P99
- Requests activos

### Dashboard 2: Performance de API
- Requests por endpoint
- Latencia por endpoint
- Distribución de status codes
- Top endpoints más lentos

### Dashboard 3: MediatR Commands/Queries
- Total de comandos vs queries
- Latencia por comando/query
- Tasa de errores
- Comandos más ejecutados
- Comandos más lentos

### Dashboard 4: Caché y Permisos
- Cache hit rate
- Operaciones de caché por segundo
- Latencia de operaciones de caché
- Verificaciones de permisos por segundo
- Usuarios con permisos cacheados

### Dashboard 5: Base de Datos
- Queries por segundo
- Latencia de queries
- Errores de base de datos
- Queries por entidad

### Dashboard 6: Health Checks
- Estado de todos los health checks
- Duración de health checks
- Historial de fallos

## 🚀 Configuración de Prometheus

### Docker Compose con Prometheus y Grafana

```yaml
version: '3.8'

services:
  nombre-app-api:
    image: nombre-app-qcode-api:latest
    ports:
      - "5224:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
  
  prometheus:
    image: prom/prometheus:latest
    ports:
      - "9090:9090"
    volumes:
      - ./prometheus.yml:/etc/prometheus/prometheus.yml
      - prometheus-data:/prometheus
    command:
      - '--config.file=/etc/prometheus/prometheus.yml'
      - '--storage.tsdb.path=/prometheus'
      - '--web.console.libraries=/usr/share/prometheus/console_libraries'
      - '--web.console.templates=/usr/share/prometheus/consoles'
  
  grafana:
    image: grafana/grafana:latest
    ports:
      - "3000:3000"
    volumes:
      - grafana-data:/var/lib/grafana
    environment:
      - GF_SECURITY_ADMIN_PASSWORD=admin
      - GF_USERS_ALLOW_SIGN_UP=false

volumes:
  prometheus-data:
  grafana-data:
```

### Archivo prometheus.yml

```yaml
global:
  scrape_interval: 15s
  evaluation_interval: 15s
  external_labels:
    cluster: 'nombre-app'
    environment: 'production'

# Reglas de alertas
rule_files:
  - 'alerts.yml'

# Configuración de scraping
scrape_configs:
  - job_name: 'nombre-app-api'
    metrics_path: '/metrics'
    scrape_interval: 15s
    scrape_timeout: 10s
    static_configs:
      - targets: ['nombre-app-api:8080']
        labels:
          service: 'api'
          team: 'backend'
```

### Ejemplo de Alertas (alerts.yml)

```yaml
groups:
  - name: nombre_app_qcode_alerts
    interval: 30s
    rules:
      # Alerta de alta tasa de errores
      - alert: HighErrorRate
        expr: |
          rate(nombre_app_http_requests_total{status_code=~"5.."}[5m]) > 0.05
        for: 5m
        labels:
          severity: critical
        annotations:
          summary: "Alta tasa de errores HTTP 5xx"
          description: "La tasa de errores es {{ $value | humanizePercentage }}"
      
      # Alerta de latencia alta
      - alert: HighLatency
        expr: |
          histogram_quantile(0.95, 
            rate(nombre_app_http_request_duration_seconds_bucket[5m])
          ) > 1
        for: 10m
        labels:
          severity: warning
        annotations:
          summary: "Latencia P95 alta"
          description: "P95 latency es {{ $value }}s"
      
      # Alerta de health check fallando
      - alert: HealthCheckFailing
        expr: nombre_app_healthcheck_status == 0
        for: 2m
        labels:
          severity: critical
        annotations:
          summary: "Health check {{ $labels.check_name }} está fallando"
      
      # Alerta de baja tasa de cache hits
      - alert: LowCacheHitRate
        expr: |
          rate(nombre_app_cache_hits_total[5m]) / 
          (rate(nombre_app_cache_hits_total[5m]) + rate(nombre_app_cache_misses_total[5m])) < 0.7
        for: 15m
        labels:
          severity: warning
        annotations:
          summary: "Baja tasa de cache hits"
          description: "Cache hit rate es {{ $value | humanizePercentage }}"
      
      # Alerta de errores en base de datos
      - alert: DatabaseErrors
        expr: rate(nombre_app_database_errors_total[5m]) > 0
        for: 5m
        labels:
          severity: critical
        annotations:
          summary: "Errores en base de datos"
          description: "Tasa de errores: {{ $value }}/s"
```

## 🔍 Troubleshooting

### Las métricas no aparecen en Prometheus

1. Verificar que el endpoint `/metrics` está accesible:
   ```bash
   curl http://localhost:5224/metrics
   ```

2. Revisar la configuración de scraping en Prometheus:
   ```bash
   # Ver targets en Prometheus UI
   http://localhost:9090/targets
   ```

3. Verificar logs de Prometheus:
   ```bash
   docker logs prometheus
   ```

### Métricas con cardinalidad alta

Si tienes demasiadas series de métricas (cardinalidad alta):

1. Revisa los labels - evita valores dinámicos como IDs de usuario
2. El middleware ya sanitiza paths con IDs a `{id}`
3. Considera agregar límites de cardinalidad en Prometheus

### Performance Impact

Las métricas de Prometheus tienen un impacto mínimo:
- Overhead por request: < 1ms
- Memoria adicional: ~50-100MB
- CPU adicional: < 5%

## 📚 Mejores Prácticas

1. **Nombres de Métricas**: Usa el prefijo `nombre_app_` para todas las métricas
2. **Labels**: Mantén cardinalidad baja (evita IDs únicos)
3. **Tipos de Métricas**:
   - Counter: Para valores que solo incrementan
   - Gauge: Para valores que suben y bajan
   - Histogram: Para distribuciones (latencia, tamaños)
4. **Scraping**: 15-60 segundos es suficiente para la mayoría de casos
5. **Retención**: Configura retención apropiada (15-30 días típicamente)
6. **Alertas**: Usa `for` clause para evitar alertas por picos momentáneos

## 🔗 Referencias

- [Prometheus Documentation](https://prometheus.io/docs/)
- [PromQL Basics](https://prometheus.io/docs/prometheus/latest/querying/basics/)
- [Grafana Dashboards](https://grafana.com/docs/grafana/latest/dashboards/)
- [prometheus-net Library](https://github.com/prometheus-net/prometheus-net)
- [Best Practices for Naming Metrics](https://prometheus.io/docs/practices/naming/)

