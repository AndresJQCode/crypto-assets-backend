# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a .NET 10 DDD (Domain-Driven Design) template implementing Clean Architecture with CQRS, featuring JWT authentication with OAuth providers, comprehensive permission-based authorization, and full observability stack.

## Build and Development Commands

### Build and Run

```bash
# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run the API (from root directory)
dotnet run --project Api

# Watch mode (auto-rebuild on file changes)
dotnet watch --project Api
```

### Database Management

```bash
# Create a new migration (run from root directory)
dotnet ef migrations add <MigrationName> --project Infrastructure --startup-project Api

# Apply migrations
dotnet ef database update --project Infrastructure --startup-project Api

# Drop database
dotnet ef database drop --project Infrastructure --startup-project Api

# IMPORTANT: Do NOT create migrations manually per .cursorrules
```

### Testing

```bash
# Run all tests
dotnet test

# Run with code coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Docker

```bash
# Build and run API
docker build -t template-qcode-backend .
docker run -p 8080:80 template-qcode-backend

# Run with Loki + Grafana monitoring stack
docker-compose -f docker-compose.loki.yml up -d
# Grafana: http://localhost:3000 (admin/admin)
```

## Architecture

### Three-Layer Clean Architecture

```
Domain/             → Core business logic (entities, interfaces, exceptions)
Infrastructure/     → Data access, external services, EF Core configurations
Api/                → Presentation layer with Minimal APIs, Application logic (CQRS)
```

**Key Design Decision**: The `Api` project contains the Application layer (Commands, Queries, DTOs, Validators, Behaviors) instead of having a separate Application project. This is intentional for simplicity in this template.

### Domain Layer (Domain/)

**Purpose**: Contains enterprise business rules and domain entities. Zero dependencies on other layers.

**Key Patterns**:

- **Aggregates**: Entities grouped by consistency boundaries (`UserAggregate/`, `RoleAggregate/`, `PermissionAggregate/`)
- **Entity Base Class**: `Entity<TId>` with built-in soft delete, audit trail, and domain events support
- **Domain Events**: Infrastructure exists (`AddDomainEvent()`, `MediatorExtension.cs` dispatcher) but concrete events need implementation
- **Interfaces**: `IRepository<T>`, `IUnitOfWork`, `IJwtTokenService`, `IEmailSender` (follow Dependency Inversion Principle)
- **Value Objects**: Base class `ValueObject.cs` exists but not yet used (candidate for Email, Password)

**Important Files**:

- `SeedWork/Entity.cs` - Base entity with soft delete, audit fields, domain events
- `SeedWork/IRepository.cs` - Generic repository interface
- `SeedWork/IAggregateRoot.cs` - Marker interface for aggregate roots
- `Interfaces/` - Service interfaces (JWT, Email, etc.) for dependency inversion

### Infrastructure Layer (Infrastructure/)

**Purpose**: Implements data persistence, external service integrations, and cross-cutting concerns.

**Key Components**:

- **EF Core DbContext**: `ApiContext.cs` with PostgreSQL, command timeout 30s
- **Repository Pattern**: `Repository<T>` with built-in:
  - Automatic soft delete filtering
  - Audit field population (CreatedBy, LastModifiedBy)
  - AsNoTracking() by default for queries
  - Prometheus metrics for all operations
- **Services**:
  - Auth: `JwtTokenService`, `GoogleOAuthService`, `MicrosoftOAuthService`
  - Email: `InfobipEmailSender`, `SimpleEmailTemplateService`
  - Caching: `PermissionCacheService` with 15min expiration
  - Circuit Breaker: `PermissionCircuitBreakerService` for resilience
- **Entity Configurations**: Fluent API configurations in `EntityConfigurations/`
- **Metrics**: `InfrastructureMetrics.cs` for Prometheus instrumentation

**Database Strategy**:

- **Soft Delete**: All entities inherit soft delete from `Entity<T>`. Repository automatically excludes deleted records.
- **Audit Trail**: CreatedOn, CreatedBy, LastModifiedOn, LastModifiedBy auto-populated
- **Unit of Work**: `SaveEntitiesAsync()` dispatches domain events and saves changes

### Api Layer (Api/)

**Purpose**: HTTP presentation layer + Application logic (Commands/Queries).

**Structure**:

```
Api/
├── Apis/                    → Minimal API endpoint groups (AuthEndpoints, UsersEndpoints, etc.)
├── Application/
│   ├── Commands/           → CQRS Commands (Create, Update, Delete operations)
│   ├── Queries/            → CQRS Queries (Read operations)
│   ├── Dtos/               → Data Transfer Objects
│   ├── Validators/         → FluentValidation validators
│   ├── Behaviors/          → MediatR pipeline behaviors
│   └── Services/           → Application services
├── Infrastructure/
│   ├── Middlewares/        → Custom middleware (PermissionAuthorizationMiddleware, ErrorHandler)
│   └── Services/           → API-specific services (PermissionService, RecaptchaService)
└── Extensions/             → Service registration, configuration
```

**CQRS with MediatR**:

- **Commands**: Mutating operations (Create, Update, Delete)
  - Example: `LoginCommand` → `LoginCommandHandler` → returns `LoginResponseDto`
- **Queries**: Read-only operations
  - Example: `GetAllUsersQuery` → `GetAllUsersQueryHandler` → returns `List<UserDto>`
- **Pipeline Behaviors** (executed in order):
  1. `LoggingBehavior` - Structured logging with Serilog
  2. `ValidatorBehavior` - FluentValidation (throws `DomainException` on failure)
  3. `TransactionBehavior` - Wraps Commands in database transactions

**Minimal APIs**:

- Endpoints organized by feature (AuthApi, UsersApi, RolesApi, etc.)
- Use `.WithTags()` for Swagger grouping
- Example: `app.MapAuthEndpoints().WithTags("Auth")`

### Authorization Architecture

**Permission-Based System** (not role-based):

1. **Entities**: `User` → `UserRole` → `Role` → `PermissionRole` → `Permission`
2. **Permission Structure**: Resource + Action (e.g., "Users.Create", "Roles.Update")
3. **Middleware**: `PermissionAuthorizationMiddleware` checks permissions before endpoint execution
4. **Attribute**: `[RequirePermission("Resource", "Action")]` on endpoints
5. **Caching**: User permissions cached for 15min in `PermissionCacheService`
6. **Resilience**: Circuit breaker pattern for permission checks (fallback: deny access)

**Excluded Paths** (configured in `AppSettings.PermissionMiddleware.ExcludedPaths`):

- `/auth/*`, `/health/*`, `/metrics`, `/swagger`, `/scalar`, etc.

### Authentication Flow

**JWT + Refresh Token**:

1. User logs in → `LoginCommandHandler` validates credentials
2. Generate access token (JWT) + refresh token (GUID)
3. Store tokens in ASP.NET Core Identity's `UserTokens` table
4. Access token contains claims: NameIdentifier, Jti, Iat
5. Refresh token rotation: old token invalidated on refresh

**OAuth Providers**:

- Google OAuth: `GoogleOAuthService`
- Microsoft OAuth: `MicrosoftOAuthService`

**Important**: JWT tokens do NOT contain roles/permissions (design choice). Permissions are queried per request and cached.

## Code Style and Conventions

### C# Language Features (.cursorrules)

**CRITICAL RULES**:

1. **Primary Constructors**: Always use primary constructor syntax (C# 12+)

   ```csharp
   // ✅ Correct
   public class MyService(ILogger<MyService> logger, IRepository<User> repository) { }

   // ❌ Avoid
   public class MyService {
       private readonly ILogger<MyService> _logger;
       public MyService(ILogger<MyService> logger) { _logger = logger; }
   }
   ```

2. **File-Scoped Namespaces**: Always use file-scoped namespaces

   ```csharp
   // ✅ Correct
   namespace Domain.AggregatesModel.UserAggregate;

   public class User : Entity { }

   // ❌ Avoid
   namespace Domain.AggregatesModel.UserAggregate {
       public class User : Entity { }
   }
   ```

3. **Minimal APIs**: Always use Minimal APIs (NOT controllers)

   ```csharp
   // ✅ Correct
   public static class AuthApi {
       public static RouteGroupBuilder MapAuthEndpoints(this IEndpointRouteBuilder app) {
           var group = app.MapGroup("/auth");
           group.MapPost("/login", async (LoginCommand cmd, IMediator mediator) => ...);
       }
   }
   ```

4. **FluentValidation**: Use FluentValidation for all input validation (NOT Data Annotations)

### Architecture Principles

- **SOLID**: Follow SOLID principles rigorously
- **DDD**: Use domain-driven design patterns (Aggregates, Entities, Value Objects)
- **Dependency Inversion**: Interfaces in Domain, implementations in Infrastructure
- **MediatR**: All business logic goes through MediatR Commands/Queries
- **No Manual Migrations**: Let EF Core generate migrations from entity changes

### Naming Conventions

- **Commands**: `{Action}{Entity}Command` (e.g., `CreateUserCommand`)
- **Handlers**: `{Command/Query}Handler` (e.g., `CreateUserCommandHandler`)
- **DTOs**: `{Entity}{Purpose}Dto` (e.g., `UserResponseDto`, `LoginRequestDto`)
- **Validators**: `{Command/Query}Validator` (e.g., `CreateUserCommandValidator`)

## Configuration and Secrets

### appsettings.json Structure

```json
{
  "ConnectionStrings": { "DefaultConnection": "..." },
  "JwtSettings": { "SecretKey": "...", "Issuer": "...", "Audience": "..." },
  "OAuth": { "Google": {...}, "Microsoft": {...} },
  "Recaptcha": { "Enabled": true, "SecretKey": "..." },
  "PermissionMiddleware": {
    "Enabled": true,
    "ExcludedPaths": ["/auth/*", "/health/*", ...],
    "EnableAuditLogging": true,
    "EnablePerformanceMetrics": true
  }
}
```

**Security**:

## Observability

### Logging (Serilog + Grafana Loki)

- **Structured Logging**: Use Serilog with semantic properties
- **Correlation ID**: `CorrelationIdMiddleware` adds `X-Correlation-ID` to all logs
- **Loki Integration**: Logs shipped to Grafana Loki (see `docker-compose.loki.yml`)

### Metrics (Prometheus)

- **Endpoint**: `/metrics` (exposed automatically)
- **Metrics**:
  - `database_queries_total` - DB operations by type and status
  - `authentication_attempts_total` - Auth attempts by method and result
  - `permission_checks_total` - Permission checks by resource/action
  - Custom metrics in `InfrastructureMetrics.cs` and `ApiMetrics.cs`

### Health Checks

- `/health` - Overall health
- `/health/live` - Liveness probe
- `/health/ready` - Readiness probe
- `/health/db` - Database health

**Existing Checks**: Database, Cache, Identity, Email, JWT
**Missing**: OAuth providers (Google, Microsoft) - see MEJORAS_CHECKLIST.md

## Common Workflows

### Adding a New Entity

1. Create entity in `Domain/AggregatesModel/{EntityName}Aggregate/`
   - Inherit from `Entity` or `Entity<TId>`
   - Use primary constructor for dependencies
   - Add domain methods (SetActive, Delete, CanBeDeleted, etc.)

2. Create repository interface in Domain: `I{Entity}Repository`

3. Create repository in `Infrastructure/Repositories/` (if custom queries needed)
   - Most entities only need `IRepository<T>` from `Repository<T>`

4. Create EF Core configuration in `Infrastructure/EntityConfigurations/`
   - Configure table name, indexes, relationships

5. Add DbSet to `ApiContext.cs`

6. Generate migration (from root):
   ```bash
   dotnet ef migrations add Add{Entity} --project Infrastructure --startup-project Api
   ```

### Adding a New Command/Query

1. Create Command/Query in `Api/Application/Commands/` or `Queries/`

   ```csharp
   public record CreateUserCommand(string Email, string Password) : IRequest<UserResponseDto>;
   ```

2. Create Handler in same directory

   ```csharp
   public class CreateUserCommandHandler(
       IRepository<User> repository)
       : IRequestHandler<CreateUserCommand, UserResponseDto>
   {
       public async Task<UserResponseDto> Handle(CreateUserCommand request, CancellationToken ct)
       {
           // Business logic
           await repository.Create(user, ct);
           await repository.UnitOfWork.SaveEntitiesAsync(ct);
           return dto;
       }
   }
   ```

3. Create Validator in `Api/Application/Validators/`

   ```csharp
   public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
   {
       public CreateUserCommandValidator()
       {
           RuleFor(x => x.Email).NotEmpty().EmailAddress();
       }
   }
   ```

4. Create endpoint in `Api/Apis/{EntityName}Endpoints/`
   ```csharp
   group.MapPost("/", async (CreateUserCommand cmd, IMediator mediator) =>
   {
       var result = await mediator.Send(cmd);
       return Results.Ok(result);
   })
   .RequirePermission("Users", "Create")
   .WithName("CreateUser")
   .Produces<UserResponseDto>(201);
   ```

### Adding a Permission-Protected Endpoint

1. Use `.RequirePermission(resource, action)` extension method:

   ```csharp
   group.MapPost("/users", handler)
       .RequirePermission("Users", "Create");
   ```

2. The permission string format is `{Resource}.{Action}`
   - Resource: Entity name (Users, Roles, Permissions)
   - Action: Operation (Create, Read, Update, Delete, List)

3. `PermissionAuthorizationMiddleware` will:
   - Check authentication
   - Query user permissions (with caching)
   - Allow/deny based on permission check
   - Record metrics

### Running Migrations Locally

1. Ensure connection string in `appsettings.json` points to your local DB
2. Run: `dotnet ef database update --project Infrastructure --startup-project Api`
3. Seed data runs automatically on app startup (see `Program.cs:109-114`)

## Known Issues and Improvements

See `MEJORAS_CHECKLIST.md` for a comprehensive list of 32 pending improvements prioritized by impact.

**High Priority Items**:

- Move secrets to Azure Key Vault (packages referenced but not configured)
- Implement health checks for OAuth providers
- Use Result Pattern instead of exceptions for business logic
- Fix file naming typos: `CurentUserDto.cs` → `CurrentUserDto.cs`, `ForgotPasswordResponse.Dto.cs` → `ForgotPasswordResponseDto.cs`

**DDD Enhancements**:

- Implement concrete Domain Events (`UserRegisteredEvent`, `PasswordChangedEvent`)
- Create Value Objects for Email and Password (base class exists)
- Move business validation from handlers to entities

## API Documentation

- **Swagger UI**: Not used
- **Scalar**: `/scalar/v1` - Modern API documentation UI
- **OpenAPI**: `/openapi/v1.json` - OpenAPI specification

## Related Documentation

See `docs/` directory for detailed implementation guides:

- `Prometheus-Implementation.md` - Metrics setup
- `Serilog-Loki-Implementation.md` - Logging setup
- `HealthChecks-Implementation.md` - Health checks
- `CircuitBreaker-Implementation.md` - Resilience patterns
- `PermissionMiddleware-Configuration.md` - Authorization system
