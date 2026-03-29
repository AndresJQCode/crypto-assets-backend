# Digital Assets - .NET 10

Template de proyecto basado en Domain-Driven Design (DDD) y Clean Architecture para .NET 9.

## 🚀 Características

- **Arquitectura DDD**: Implementación completa de Domain-Driven Design
- **Clean Architecture**: Separación clara de responsabilidades por capas
- **CQRS**: Patrón Command Query Responsibility Segregation
- **MediatR**: Implementación de Mediator Pattern
- **Entity Framework Core**: ORM con Code First approach
- **JWT Authentication**: Autenticación basada en tokens
- **Docker Support**: Contenedores para desarrollo y producción
- **Swagger/OpenAPI**: Documentación automática de la API

## 📁 Estructura del Proyecto

```
├── Domain/           # Capa de Dominio (Lógica de Negocio)
├── Infrastructure/   # Capa de Infraestructura (Datos, Servicios)
├── Api/             # Capa de Aplicación (API, Endpoints)
├── docs/            # Documentación del Proyecto
└── deployment/      # Scripts y configuraciones de despliegue
```

## 🛠️ Tecnologías Utilizadas

- **.NET 9.0** - Framework principal
- **ASP.NET Core** - Web API
- **Entity Framework Core** - ORM
- **MediatR** - Mediator Pattern
- **FluentValidation** - Validación de datos
- **Mapster** - Mapeo de objetos
- **JWT Bearer** - Autenticación
- **Swagger/OpenAPI** - Documentación de API
- **Serilog** - Logging estructurado
- **Grafana Loki** - Agregación y consulta de logs
- **Prometheus** - Métricas y monitoreo
- **Docker** - Contenedores

## 📚 Documentación

Toda la documentación del proyecto está organizada en la carpeta [`docs/`](./docs/):

- **[Documentación de la API](./docs/README.md)** - Endpoints, autenticación y ejemplos
- **[Implementación de Prometheus](./docs/Prometheus-Implementation.md)** - Métricas y monitoreo
- **[Implementación de Serilog + Loki](./docs/Serilog-Loki-Implementation.md)** - Logging centralizado con Grafana Loki
- **[Health Checks](./docs/HealthChecks-Implementation.md)** - Sistema de health checks
- **[Circuit Breaker](./docs/CircuitBreaker-Implementation.md)** - Implementación de circuit breaker
- **[Middleware de Permisos](./docs/PermissionMiddleware-Configuration.md)** - Sistema de autorización por permisos

## 🚀 Inicio Rápido

### Prerrequisitos

- .NET 9.0 SDK
- SQL Server LocalDB o SQL Server
- Visual Studio 2022 o Visual Studio Code

### Configuración

1. **Clonar el repositorio**

   ```bash
   git clone https://github.com/your-org/template-ddd.git
   cd template-ddd
   ```

2. **Restaurar dependencias**

   ```bash
   dotnet restore
   ```

3. **Configurar base de datos**

   ```bash
   dotnet ef database update --project Infrastructure --startup-project Api
   ```

4. **Ejecutar la aplicación**

   ```bash
   dotnet run --project Api
   ```

5. **Acceder a la aplicación**
   - API: `https://localhost:5001`
   - Swagger: `https://localhost:5001/swagger`

## 🐳 Docker

### Desarrollo con Docker

```bash
# Construir y ejecutar con Docker Compose
docker-compose up --build
```

### Stack de Monitoreo (Loki + Grafana)

```bash
# Iniciar Loki y Grafana para logging
docker-compose -f docker-compose.loki.yml up -d

# Acceder a Grafana: http://localhost:3000
# Usuario: admin / Contraseña: admin
```

### Solo la API

```bash
# Construir imagen
docker build -t template-qcode-backend .

# Ejecutar contenedor
docker run -p 8080:80 template-qcode-backend
```

## 🧪 Testing

```bash
# Ejecutar todos los tests
dotnet test

# Tests con cobertura
dotnet test --collect:"XPlat Code Coverage"
```

## 📋 Endpoints Principales

- **Autenticación**: `/auth/*`
- **Usuarios**: `/users/*`
- **Roles**: `/roles/*`
- **Permisos**: `/permissions/*`
- **Dashboard**: `/dashboard/*`
- **Health Check**: `/health`, `/health/live`, `/health/ready`, `/health/db`
- **Métricas Prometheus**: `/metrics`
- **Documentación API**: `/scalar/v1`

## 🤝 Contribuir

1. Fork el proyecto
2. Crear una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abrir un Pull Request

Para más detalles, consulta la [Guía de Desarrollo](./docs/development/README.md).

## 📄 Licencia

Este proyecto está bajo la Licencia MIT. Ver el archivo [LICENSE](LICENSE) para más detalles.

## 🙏 Agradecimientos

- [Microsoft eShopOnContainers](https://github.com/dotnet/eShopOnContainers) - Inspiración para la arquitectura
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html) - Principios de arquitectura
- [Domain-Driven Design](https://martinfowler.com/bliki/DomainDrivenDesign.html) - Patrones de diseño
