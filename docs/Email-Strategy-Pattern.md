# Email Strategy Pattern

## Overview

El sistema de envío de emails implementa el **patrón Strategy** para permitir el uso de múltiples proveedores de email (Infobip, SendGrid, SMTP, etc.) sin modificar la lógica de negocio.

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Application Layer                        │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  TenantRegisteredEventHandler                        │  │
│  │  ForgotPasswordCommandHandler                        │  │
│  │  RegisterCommandHandler                              │  │
│  └──────────────────────────────────────────────────────┘  │
│                            │                                │
│                            │ uses                           │
│                            ▼                                │
│  ┌──────────────────────────────────────────────────────┐  │
│  │             IEmailService (Interface)                │  │
│  │  - SendConfirmationLinkAsync()                       │  │
│  │  - SendPasswordResetLinkAsync()                      │  │
│  │  - SendCustomEmailAsync()                            │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
                             │
                             │ implemented by
                             ▼
┌─────────────────────────────────────────────────────────────┐
│                 Infrastructure Layer                        │
│  ┌──────────────────────────────────────────────────────┐  │
│  │          EmailService (Implementation)               │  │
│  │  - Business logic (templates, company name, etc.)    │  │
│  └──────────────────────────────────────────────────────┘  │
│                            │                                │
│                            │ uses (Strategy Pattern)        │
│                            ▼                                │
│  ┌──────────────────────────────────────────────────────┐  │
│  │         IEmailProvider (Strategy Interface)          │  │
│  │  - SendEmailAsync()                                  │  │
│  └──────────────────────────────────────────────────────┘  │
│                            │                                │
│              ┌─────────────┼─────────────┐                 │
│              ▼             ▼             ▼                 │
│  ┌────────────────┐  ┌──────────┐  ┌─────────┐           │
│  │ InfobipEmail   │  │SendGrid  │  │ SMTP    │           │
│  │   Provider     │  │ Provider │  │Provider │           │
│  └────────────────┘  └──────────┘  └─────────┘           │
│    (Implemented)       (Future)      (Future)             │
└─────────────────────────────────────────────────────────────┘
                             │
                             │ also integrates with
                             ▼
┌─────────────────────────────────────────────────────────────┐
│               ASP.NET Core Identity                         │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  IdentityEmailSenderAdapter<User>                    │  │
│  │  (Bridges IEmailSender<User> with IEmailService)     │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

## Components

### 1. IEmailService (Domain/Interfaces)
Interfaz principal para la lógica de negocio de emails.

**Responsabilidades:**
- Definir operaciones de alto nivel (confirmación, reset password, emails personalizados)
- Abstraer la lógica de negocio del proveedor técnico

**Métodos:**
- `SendConfirmationLinkAsync()` - Email de confirmación de cuenta
- `SendPasswordResetLinkAsync()` - Email de reset de contraseña
- `SendPasswordResetCodeAsync()` - Email con código de reset
- `SendCustomEmailAsync()` - Emails personalizados (notificaciones, alertas)

### 2. IEmailProvider (Domain/Interfaces)
Interfaz Strategy para proveedores de email.

**Responsabilidades:**
- Definir contrato para proveedores específicos
- Permitir intercambio transparente de proveedores

**Métodos:**
- `SendEmailAsync()` - Envío técnico del email
- `ProviderName { get; }` - Identificador del proveedor

### 3. EmailService (Infrastructure/Services/Email)
Implementación de `IEmailService` que contiene la lógica de negocio.

**Responsabilidades:**
- Renderizar templates usando `IEmailTemplateService`
- Aplicar configuración (FromEmail, CompanyName, HeaderImage)
- Delegar envío técnico a `IEmailProvider`
- Logging y manejo de errores

**Dependencies:**
- `IEmailProvider` (Strategy - inyectado según configuración)
- `IEmailTemplateService` (para renderizar HTML)
- `AppSettings` (configuración)

### 4. InfobipEmailProvider (Infrastructure/Services/Email/Providers)
Implementación concreta para Infobip.

**Responsabilidades:**
- Configurar cliente Infobip API
- Implementar envío usando SDK de Infobip
- Logging específico del proveedor

### 5. IdentityEmailSenderAdapter (Infrastructure/Services/Email)
Adapter para integrar con ASP.NET Core Identity.

**Responsabilidades:**
- Implementar `IEmailSender<User>` de Identity
- Delegar a `IEmailService` para envíos reales
- Extraer datos del usuario (nombre completo)

## Configuration

> **⚠️ Important:** Configuration is validated at startup. See [Email-Configuration-Validation.md](./Email-Configuration-Validation.md) for details on validation errors and troubleshooting.

### appsettings.json

```json
{
  "EmailService": {
    "Provider": "Infobip",  // Options: "Infobip", "SendGrid", "Smtp"
    "FromEmail": "noreply@lulocrm.com",
    "FromName": "Lulo CRM",
    "HeaderImage": "https://example.com/logo.png"
  },
  "Infobip": {
    "BasePath": "https://api.infobip.com",
    "ApiKey": "your-api-key-here"
  }
}
```

### Environment Variables (Production)

```bash
EmailService__Provider=Infobip
Infobip__ApiKey=your-secret-api-key
```

## Dependency Injection

Registro en `Program.cs`:

```csharp
// Configurar servicios de email con patrón Strategy
builder.Services.AddEmailServices(builder.Configuration);
```

Internamente (`EmailServicesExtensions.cs`):

```csharp
// 1. Register email template service
services.AddTransient<IEmailTemplateService, SimpleEmailTemplateService>();

// 2. Register email provider based on configuration (Strategy pattern)
switch (emailSettings.Provider.ToUpperInvariant())
{
    case "INFOBIP":
        services.AddSingleton<IEmailProvider, InfobipEmailProvider>();
        break;
    case "SENDGRID":
        services.AddSingleton<IEmailProvider, SendGridEmailProvider>();
        break;
    // ... more providers
}

// 3. Register main email service
services.AddScoped<IEmailService, EmailService>();

// 4. Register Identity adapter
services.AddTransient<IEmailSender<User>, IdentityEmailSenderAdapter<User>>();
```

## Usage Examples

### 1. Sending Custom Email (Domain Events, Notifications)

```csharp
public class TenantRegisteredEventHandler(
    IEmailService emailService,
    ILogger<TenantRegisteredEventHandler> logger)
    : INotificationHandler<TenantRegisteredEvent>
{
    public async Task Handle(TenantRegisteredEvent notification, CancellationToken ct)
    {
        await emailService.SendCustomEmailAsync(
            from: "noreply@lulocrm.com",
            to: new[] { "admin@company.com" },
            subject: "New Tenant Registered",
            htmlBody: "<h1>New tenant: " + notification.TenantName + "</h1>",
            cancellationToken: ct);
    }
}
```

### 2. Identity Integration (Automatic)

```csharp
// ASP.NET Core Identity automatically uses IEmailSender<User>
// which is implemented by IdentityEmailSenderAdapter<User>
// No changes needed in Identity-related code!

public class RegisterCommandHandler(
    UserManager<User> userManager)  // UserManager uses IEmailSender<User> internally
{
    // Email confirmation is sent automatically by Identity
    // using our email service architecture behind the scenes
}
```

### 3. Manual Identity Emails

```csharp
public class ForgotPasswordCommandHandler(
    UserManager<User> userManager,
    IEmailSender<User> emailSender)  // Identity interface, our implementation
{
    public async Task Handle(ForgotPasswordCommand request, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
        var resetLink = $"https://app.com/reset?token={resetToken}";

        // Uses IdentityEmailSenderAdapter → EmailService → InfobipEmailProvider
        await emailSender.SendPasswordResetLinkAsync(user, request.Email, resetLink);
    }
}
```

## Adding a New Email Provider

### Step 1: Create Provider Implementation

Create `Infrastructure/Services/Email/Providers/SendGridEmailProvider.cs`:

```csharp
using Domain.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Email.Providers;

public class SendGridEmailProvider : IEmailProvider
{
    private readonly SendGridClient _client;
    private readonly ILogger<SendGridEmailProvider> _logger;

    public string ProviderName => "SendGrid";

    public SendGridEmailProvider(
        IOptionsMonitor<AppSettings> appSettings,
        ILogger<SendGridEmailProvider> logger)
    {
        _logger = logger;
        _client = new SendGridClient(appSettings.CurrentValue.SendGrid.ApiKey);
    }

    public async Task SendEmailAsync(
        string from,
        IReadOnlyCollection<string> to,
        string subject,
        string htmlBody,
        IReadOnlyCollection<string>? cc = null,
        IReadOnlyCollection<string>? bcc = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var msg = new SendGridMessage
            {
                From = new EmailAddress(from),
                Subject = subject,
                HtmlContent = htmlBody
            };

            foreach (var recipient in to)
                msg.AddTo(new EmailAddress(recipient));

            await _client.SendEmailAsync(msg, cancellationToken);

            _logger.LogInformation(
                "Email sent successfully via {Provider} to {Recipients}",
                ProviderName,
                string.Join(", ", to));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email via {Provider}", ProviderName);
            throw;
        }
    }
}
```

### Step 2: Add Configuration

Update `AppSettings.cs`:

```csharp
public class AppSettings
{
    // ... existing properties
    public SendGridSettings? SendGrid { get; set; }

    public class SendGridSettings
    {
        public required string ApiKey { get; set; }
    }
}
```

Update `appsettings.json`:

```json
{
  "EmailService": {
    "Provider": "SendGrid"
  },
  "SendGrid": {
    "ApiKey": "SG.xxxxxxxxxxxxx"
  }
}
```

### Step 3: Register in DI

Update `EmailServicesExtensions.cs`:

```csharp
switch (provider)
{
    case "INFOBIP":
        services.AddSingleton<IEmailProvider, InfobipEmailProvider>();
        break;

    case "SENDGRID":
        services.AddSingleton<IEmailProvider, SendGridEmailProvider>();
        break;

    // ... more providers
}
```

### Step 4: Test

```bash
# Change provider in appsettings.json or environment variable
export EmailService__Provider=SendGrid

# Run application - emails will now use SendGrid
dotnet run --project Api
```

## Benefits

### 1. Open/Closed Principle
- Sistema abierto para extensión (nuevos proveedores)
- Cerrado para modificación (lógica de negocio no cambia)

### 2. Single Responsibility
- `IEmailService` → Lógica de negocio
- `IEmailProvider` → Integración técnica
- `EmailService` → Orquestación
- Providers → Implementación específica

### 3. Dependency Inversion
- Alto nivel (`EmailService`) no depende de bajo nivel (`InfobipEmailProvider`)
- Ambos dependen de abstracción (`IEmailProvider`)

### 4. Easy Testing
```csharp
// Mock del provider para tests
var mockProvider = new Mock<IEmailProvider>();
mockProvider.Setup(p => p.SendEmailAsync(...)).ReturnsAsync();

var emailService = new EmailService(
    mockProvider.Object,
    mockTemplateService,
    mockAppSettings,
    mockLogger);

// Test business logic without hitting real email API
await emailService.SendConfirmationLinkAsync(...);
```

### 5. Configuration-Based Switching
- Cambiar proveedor sin recompilar
- Diferentes proveedores por ambiente (dev, staging, prod)
- A/B testing de proveedores

### 6. Centralized Business Logic
- Templates, company name, header image en un solo lugar
- Cambios en formato de email no requieren modificar proveedores
- Consistencia en todos los emails

## Migration from Old Code

### Before (Coupled to Infobip)

```csharp
public class TenantRegisteredEventHandler(
    InfobipEmailSender<User> emailSender)  // Coupled!
{
    await emailSender.SendEmailAsync(from, to, subject, htmlBody);
}
```

### After (Strategy Pattern)

```csharp
public class TenantRegisteredEventHandler(
    IEmailService emailService)  // Abstraction!
{
    await emailService.SendCustomEmailAsync(from, to, subject, htmlBody);
}
```

### Benefits of Migration
- ✅ Can switch to SendGrid without changing handler
- ✅ Can mock IEmailService in tests
- ✅ Business logic separated from provider details
- ✅ Follows SOLID principles

## Troubleshooting

### Provider Not Found Error

```
InvalidOperationException: Unknown email provider 'XYZ'
```

**Solution:** Check `appsettings.json`:

```json
{
  "EmailService": {
    "Provider": "Infobip"  // Must be: Infobip, SendGrid, or Smtp
  }
}
```

### Email Not Sending

1. Check logs for provider-specific errors
2. Verify API keys in configuration
3. Check provider dashboard for quota/limits
4. Verify network connectivity

### Provider Configuration Missing

```
InvalidOperationException: EmailService configuration is required
```

**Solution:** Ensure `EmailService` section exists in `appsettings.json`

## Future Enhancements

### 1. Provider Fallback
Implement automatic fallback to secondary provider if primary fails:

```csharp
public class FallbackEmailProvider : IEmailProvider
{
    private readonly IEmailProvider _primary;
    private readonly IEmailProvider _fallback;

    public async Task SendEmailAsync(...)
    {
        try
        {
            await _primary.SendEmailAsync(...);
        }
        catch
        {
            await _fallback.SendEmailAsync(...);
        }
    }
}
```

### 2. Email Queue
Add queue for retry logic and rate limiting:

```csharp
public class QueuedEmailProvider : IEmailProvider
{
    private readonly IBackgroundTaskQueue _queue;

    public async Task SendEmailAsync(...)
    {
        await _queue.QueueBackgroundWorkItemAsync(async token =>
        {
            await _innerProvider.SendEmailAsync(...);
        });
    }
}
```

### 3. Template Versioning
Support multiple template versions for A/B testing.

### 4. Multi-Provider Support
Send same email through multiple providers simultaneously for redundancy.

## References

- [Strategy Pattern - Gang of Four](https://refactoring.guru/design-patterns/strategy)
- [SOLID Principles](https://en.wikipedia.org/wiki/SOLID)
- [Dependency Injection in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)
