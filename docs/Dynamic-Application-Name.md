# Configuración de Nombre de Aplicación Dinámico

## Descripción

El proyecto ahora soporta un nombre de aplicación configurable que se aplica automáticamente a todas las métricas de Prometheus y configuraciones relacionadas. Esto permite personalizar fácilmente el proyecto al iniciar un nuevo desarrollo sin necesidad de buscar y reemplazar múltiples referencias en el código.

## Uso

### Cambiar el Nombre de la Aplicación

Para cambiar el nombre de tu aplicación, simplemente edita el archivo `Api/appsettings.json`:

```json
{
  "ApplicationName": "tu-nuevo-nombre",
  "CompanyName": "QCode"
}
```

**Ejemplos:**
- Para un sistema POS: `"ApplicationName": "POS"`
- Para un CRM: `"ApplicationName": "CRM"`
- Para un inventario: `"ApplicationName": "Inventory"`

### Qué se Actualiza Automáticamente

Al cambiar el `ApplicationName` en appsettings.json, se actualizan automáticamente:

#### 1. **Métricas de Prometheus**

Todas las métricas de Prometheus usarán el nuevo nombre como prefijo:

#### 2. **Endpoint de Métricas en Texto Plano**

El endpoint `/metrics-text` mostrará el nombre de la aplicación en su encabezado.

Las métricas específicas:

- **Antes (con "template"):**
  - `template_http_requests_total`
  - `template_database_queries_total`
  - `template_permission_checks_total`

- **Después (con "POS"):**
  - `pos_http_requests_total`
  - `pos_database_queries_total`
  - `pos_permission_checks_total`

#### 2. **Transformación del Nombre**

El nombre se transforma automáticamente para cumplir con las convenciones de Prometheus:
- Se convierte a minúsculas
- Los guiones (`-`) se reemplazan por guiones bajos (`_`)
- Los espacios se reemplazan por guiones bajos (`_`)

**Ejemplos:**
- `"My-App"` → `my_app`
- `"POS System"` → `pos_system`
- `"backend-CRM"` → `backend_crm`

## Implementación Técnica

### Archivos Modificados

1. **Infrastructure/AppSettings.cs**
   - Agregada propiedad `ApplicationName` con valor por defecto "template"

2. **Api/Infrastructure/Metrics/ApiMetrics.cs**
   - Convertido a inicialización dinámica con método `Initialize(string applicationName)`
   - Todas las métricas ahora usan el prefijo configurado

3. **Infrastructure/Metrics/InfrastructureMetrics.cs**
   - Convertido a inicialización dinámica con método `Initialize(string applicationName)`
   - Todas las métricas ahora usan el prefijo configurado

4. **Api/Extensions/PrometheusExtensions.cs**
   - Actualizado `AddPrometheusMetrics` para leer ApplicationName de configuración
   - Inicializa ambas clases de métricas con el nombre configurado

5. **Api/Program.cs**
   - Actualizado para pasar configuración a `AddPrometheusMetrics()`

6. **Api/appsettings.json**
   - Actualizado `ApplicationName` a "template" (valor por defecto del template)
   - Actualizado label de Loki para consistencia

7. **Api/Extensions/PrometheusExtensions.cs** (endpoint /metrics-text)
   - Actualizado el encabezado del endpoint `/metrics-text` para usar `ApplicationName`
   - Obtiene dinámicamente el nombre desde la configuración

## Ejemplo de Uso Completo

### Escenario: Crear un Sistema POS desde el Template

1. **Clonar el template**
   ```bash
   git clone <repository-url> my-pos-system
   cd my-pos-system
   ```

2. **Editar appsettings.json**
   ```json
   {
     "ApplicationName": "POS",
     "CompanyName": "MiEmpresa"
   }
   ```

3. **Compilar y ejecutar**
   ```bash
   dotnet build
   dotnet run --project Api
   ```

4. **Verificar las métricas**
   - Navegar a: http://localhost:5000/metrics
   - Todas las métricas ahora tendrán el prefijo `pos_`:
     ```
     pos_http_requests_total{method="GET",endpoint="/api/users",status_code="200"} 42
     pos_database_queries_total{query_type="select",entity="User",status="success"} 156
     ```

## Notas Adicionales

### Documentación de Métricas

Los archivos de documentación en `docs/` aún contienen ejemplos con el prefijo `wms_` (del proyecto WMS original). Al personalizar tu aplicación, deberás actualizar manualmente estas referencias en:

- `docs/Instrumentacion-Servicios-Prometheus.md`
- Cualquier dashboard de Grafana o consultas de Prometheus guardadas

**Búsqueda y reemplazo recomendada:**
```bash
# Reemplazar en archivos de documentación
find docs -name "*.md" -exec sed -i 's/wms_/tu_app_/g' {} +
```

### Otras Referencias

Algunas referencias al nombre de la aplicación que **no** se actualizan automáticamente:

1. **README.md**: Contiene ejemplos de Docker con `template-qcode-backend`
2. **CLAUDE.md**: Referencias al proyecto como "template"
3. **azure-pipelines.yml**: Variable `backend-template-backend`
4. **Nombres de archivos y directorios del proyecto**

Estos deben actualizarse manualmente según sea necesario.

## Valores por Defecto

Si no se especifica `ApplicationName` en appsettings.json, se usa el valor por defecto:

```csharp
public string ApplicationName { get; set; } = "";
```

## Troubleshooting

### Las métricas no aparecen con el nuevo nombre

1. Verificar que el servidor se reinició después de cambiar appsettings.json
2. Verificar que no hay errores de compilación
3. Verificar el log de inicio para confirmar que las métricas se inicializaron correctamente

### Caracteres especiales en el nombre

Evita usar caracteres especiales además de guiones y espacios. Caracteres permitidos:
- Letras (A-Z, a-z)
- Números (0-9)
- Guiones (`-`)
- Espacios (` `)
- Guiones bajos (`_`)

Los guiones y espacios se convertirán automáticamente a guiones bajos.

## Beneficios

1. **Un solo punto de configuración**: Cambia el nombre en un solo lugar
2. **Métricas consistentes**: Todas las métricas usan el mismo prefijo automáticamente
3. **Reutilización del template**: Facilita crear nuevos proyectos desde el template
4. **Conformidad con Prometheus**: Los nombres se formatean automáticamente según convenciones
5. **Mantenibilidad**: No hay referencias hardcodeadas que actualizar
