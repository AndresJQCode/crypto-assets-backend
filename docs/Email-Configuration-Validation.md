# Email Configuration Validation

## Overview

El sistema valida la configuración de emails al inicio de la aplicación (startup) para implementar el principio **fail-fast**: si la configuración es incorrecta, la aplicación no arranca y muestra un mensaje de error claro.

## Validation Flow

```
Application Startup
    ↓
Program.cs: builder.Services.AddEmailServices(configuration)
    ↓
EmailServicesExtensions.AddEmailServices()
    ↓
1. Validate EmailService section exists
    ↓
2. Validate EmailService required fields
    ↓
3. Validate provider-specific configuration (Infobip, SendGrid, etc.)
    ↓
✅ All valid → Continue startup
❌ Invalid → Throw exception with detailed error message
```

## Validated Configurations

### 1. EmailService Section

**Required Fields:**
- `Provider` - Email provider name ("Infobip", "SendGrid", "Smtp")
- `FromEmail` - Sender email address
- `FromName` - Sender display name
- `HeaderImage` - URL to email header logo

**Validation:**
```csharp
ValidateEmailServiceConfiguration(emailSettings);
```

### 2. Provider-Specific Sections

The validator checks the configuration section for the selected provider:

#### Infobip (when Provider = "Infobip")

**Required Fields:**
- `BasePath` - Infobip API URL
- `ApiKey` - Infobip API key

**Validation:**
```csharp
ValidateInfobipConfiguration(configuration);
```

#### SendGrid (when Provider = "SendGrid") - Future

**Required Fields:**
- `ApiKey` - SendGrid API key

#### SMTP (when Provider = "Smtp") - Future

**Required Fields:**
- `Host` - SMTP server hostname
- `Port` - SMTP server port
- `Username` - SMTP username
- `Password` - SMTP password
- `EnableSsl` - Whether to use SSL

## Configuration Examples

### ✅ Valid Configuration

```json
{
  "EmailService": {
    "Provider": "Infobip",
    "FromEmail": "noreply@lulocrm.com",
    "FromName": "Lulo CRM",
    "TestEmailTo": "test@example.com",
    "HeaderImage": "https://lulocrm.com/images/logo.png"
  },
  "Infobip": {
    "BasePath": "https://api.infobip.com",
    "ApiKey": "your-infobip-api-key-here"
  }
}
```

### ❌ Missing EmailService Section

**Configuration:**
```json
{
  // EmailService section missing!
  "Infobip": {
    "BasePath": "https://api.infobip.com",
    "ApiKey": "abc123"
  }
}
```

**Error Message:**
```
InvalidOperationException: EmailService configuration section is missing in appsettings.json.
Please add the 'EmailService' section with required properties:
Provider, FromEmail, FromName, HeaderImage
```

### ❌ Incomplete EmailService Configuration

**Configuration:**
```json
{
  "EmailService": {
    "Provider": "Infobip"
    // Missing: FromEmail, FromName, HeaderImage
  }
}
```

**Error Message:**
```
InvalidOperationException: EmailService configuration is incomplete:
  - EmailService.FromEmail is required
  - EmailService.FromName is required
  - EmailService.HeaderImage is required
```

### ❌ Missing Infobip Section

**Configuration:**
```json
{
  "EmailService": {
    "Provider": "Infobip",
    "FromEmail": "noreply@lulocrm.com",
    "FromName": "Lulo CRM",
    "HeaderImage": "https://lulocrm.com/logo.png"
  }
  // Infobip section missing!
}
```

**Error Message:**
```
InvalidOperationException: Infobip configuration section is missing in appsettings.json.
Since EmailService.Provider is set to 'Infobip', the 'Infobip' section is required
with properties: BasePath, ApiKey
```

### ❌ Incomplete Infobip Configuration

**Configuration:**
```json
{
  "EmailService": {
    "Provider": "Infobip",
    "FromEmail": "noreply@lulocrm.com",
    "FromName": "Lulo CRM",
    "HeaderImage": "https://lulocrm.com/logo.png"
  },
  "Infobip": {
    "BasePath": "https://api.infobip.com"
    // Missing: ApiKey
  }
}
```

**Error Message:**
```
InvalidOperationException: Infobip configuration is incomplete:
  - Infobip.ApiKey is required
```

### ❌ Unknown Provider

**Configuration:**
```json
{
  "EmailService": {
    "Provider": "Gmail",  // Not supported!
    "FromEmail": "noreply@lulocrm.com",
    "FromName": "Lulo CRM",
    "HeaderImage": "https://lulocrm.com/logo.png"
  }
}
```

**Error Message:**
```
InvalidOperationException: Unknown email provider 'Gmail'.
Supported providers: Infobip
```

## Environment Variables

You can also configure via environment variables (recommended for production):

### Valid Environment Variable Configuration

```bash
# EmailService section
export EmailService__Provider=Infobip
export EmailService__FromEmail=noreply@lulocrm.com
export EmailService__FromName="Lulo CRM"
export EmailService__HeaderImage=https://lulocrm.com/logo.png

# Infobip section
export Infobip__BasePath=https://api.infobip.com
export Infobip__ApiKey=your-secret-api-key
```

### Docker Example

```dockerfile
# Dockerfile
ENV EmailService__Provider=Infobip
ENV EmailService__FromEmail=noreply@lulocrm.com
ENV EmailService__FromName="Lulo CRM"
ENV EmailService__HeaderImage=https://lulocrm.com/logo.png
ENV Infobip__ApiKey=${INFOBIP_API_KEY}
```

```bash
# docker-compose.yml
services:
  api:
    environment:
      - EmailService__Provider=Infobip
      - EmailService__FromEmail=noreply@lulocrm.com
      - EmailService__FromName=Lulo CRM
      - EmailService__HeaderImage=https://lulocrm.com/logo.png
      - Infobip__ApiKey=${INFOBIP_API_KEY}
```

## Validation Code Location

**File:** `Api/Extensions/EmailServicesExtensions.cs`

### Main Validation Entry Point

```csharp
public static IServiceCollection AddEmailServices(
    this IServiceCollection services,
    IConfiguration configuration)
{
    // 1. Validate EmailService section exists
    var emailSettings = configuration.GetSection("EmailService").Get<AppSettings.EmailSettings>();
    if (emailSettings == null)
        throw new InvalidOperationException("EmailService configuration section is missing...");

    // 2. Validate EmailService required fields
    ValidateEmailServiceConfiguration(emailSettings);

    // 3. Validate provider-specific configuration
    switch (provider)
    {
        case "INFOBIP":
            ValidateInfobipConfiguration(configuration);
            break;
        // ... other providers
    }

    // 4. Register services
    // ...
}
```

### EmailService Validation

```csharp
private static void ValidateEmailServiceConfiguration(AppSettings.EmailSettings emailSettings)
{
    var errors = new List<string>();

    if (string.IsNullOrWhiteSpace(emailSettings.FromEmail))
        errors.Add("EmailService.FromEmail is required");

    if (string.IsNullOrWhiteSpace(emailSettings.FromName))
        errors.Add("EmailService.FromName is required");

    if (string.IsNullOrWhiteSpace(emailSettings.HeaderImage))
        errors.Add("EmailService.HeaderImage is required");

    if (string.IsNullOrWhiteSpace(emailSettings.Provider))
        errors.Add("EmailService.Provider is required");

    if (errors.Count > 0)
        throw new InvalidOperationException(
            $"EmailService configuration is incomplete:{Environment.NewLine}" +
            string.Join(Environment.NewLine, errors.Select(e => $"  - {e}")));
}
```

### Infobip Validation

```csharp
private static void ValidateInfobipConfiguration(IConfiguration configuration)
{
    var infobipSettings = configuration.GetSection("Infobip").Get<AppSettings.InfobipSettings>();

    if (infobipSettings == null)
        throw new InvalidOperationException(
            "Infobip configuration section is missing in appsettings.json...");

    var errors = new List<string>();

    if (string.IsNullOrWhiteSpace(infobipSettings.BasePath))
        errors.Add("Infobip.BasePath is required");

    if (string.IsNullOrWhiteSpace(infobipSettings.ApiKey))
        errors.Add("Infobip.ApiKey is required");

    if (errors.Count > 0)
        throw new InvalidOperationException(
            $"Infobip configuration is incomplete:{Environment.NewLine}" +
            string.Join(Environment.NewLine, errors.Select(e => $"  - {e}")));
}
```

## Adding Validation for New Providers

When adding a new email provider, create a validation method:

### Example: SendGrid Validation

```csharp
private static void ValidateSendGridConfiguration(IConfiguration configuration)
{
    var sendGridSettings = configuration.GetSection("SendGrid").Get<AppSettings.SendGridSettings>();

    if (sendGridSettings == null)
    {
        throw new InvalidOperationException(
            "SendGrid configuration section is missing in appsettings.json. " +
            "Since EmailService.Provider is set to 'SendGrid', the 'SendGrid' section is required " +
            "with properties: ApiKey");
    }

    var errors = new List<string>();

    if (string.IsNullOrWhiteSpace(sendGridSettings.ApiKey))
        errors.Add("SendGrid.ApiKey is required");

    // Add more validations as needed
    if (string.IsNullOrWhiteSpace(sendGridSettings.FromEmail))
        errors.Add("SendGrid.FromEmail is required");

    if (errors.Count > 0)
    {
        throw new InvalidOperationException(
            $"SendGrid configuration is incomplete:{Environment.NewLine}" +
            string.Join(Environment.NewLine, errors.Select(e => $"  - {e}")));
    }
}
```

### Register Validation in Switch

```csharp
switch (provider)
{
    case "INFOBIP":
        ValidateInfobipConfiguration(configuration);
        services.AddSingleton<IEmailProvider, InfobipEmailProvider>();
        break;

    case "SENDGRID":
        ValidateSendGridConfiguration(configuration);  // Add here!
        services.AddSingleton<IEmailProvider, SendGridEmailProvider>();
        break;

    // ... more providers
}
```

## Testing Validation

### Manual Testing

1. **Remove EmailService section** from appsettings.json
2. **Run application:**
   ```bash
   dotnet run --project Api
   ```
3. **Expected result:** Application fails to start with clear error message

### Unit Testing

```csharp
[Fact]
public void AddEmailServices_ShouldThrow_WhenEmailServiceSectionMissing()
{
    // Arrange
    var services = new ServiceCollection();
    var configuration = new ConfigurationBuilder().Build(); // Empty config

    // Act & Assert
    var exception = Assert.Throws<InvalidOperationException>(() =>
        services.AddEmailServices(configuration));

    Assert.Contains("EmailService configuration section is missing", exception.Message);
}

[Fact]
public void AddEmailServices_ShouldThrow_WhenProviderSectionMissing()
{
    // Arrange
    var services = new ServiceCollection();
    var configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string>
        {
            ["EmailService:Provider"] = "Infobip",
            ["EmailService:FromEmail"] = "test@test.com",
            ["EmailService:FromName"] = "Test",
            ["EmailService:HeaderImage"] = "http://test.com/logo.png"
            // Infobip section missing!
        })
        .Build();

    // Act & Assert
    var exception = Assert.Throws<InvalidOperationException>(() =>
        services.AddEmailServices(configuration));

    Assert.Contains("Infobip configuration section is missing", exception.Message);
}
```

## Benefits

### 1. Fail-Fast Principle
- Application doesn't start with invalid configuration
- Errors caught during deployment, not at runtime
- Clear error messages guide developers to fix issues

### 2. Developer Experience
- Detailed error messages show exactly what's missing
- No guessing about configuration structure
- Easy to diagnose configuration issues

### 3. Production Safety
- Prevents silent failures (emails not sending)
- Configuration issues caught in CI/CD pipeline
- Environment variable validation ensures secrets are set

### 4. Multi-Environment Support
- Same validation logic for dev, staging, prod
- Environment variables override appsettings.json
- Docker/Kubernetes deployment validation

## Troubleshooting

### Application Won't Start

**Symptom:** Application crashes on startup with `InvalidOperationException`

**Solution:**
1. Read the error message carefully
2. Check the specific configuration key mentioned
3. Verify spelling and casing (case-sensitive!)
4. Ensure environment variables are set (if used)

### "Configuration section is missing"

**Cause:** Missing entire section in appsettings.json or environment variables

**Solution:** Add the required section with all properties

### "Configuration is incomplete"

**Cause:** Section exists but missing required fields

**Solution:** Add the missing fields listed in the error message

### Environment Variables Not Working

**Cause:** Incorrect naming convention

**Solution:** Use double underscore `__` as separator:
- ✅ `EmailService__Provider`
- ❌ `EmailService:Provider` (colon doesn't work in env vars)
- ❌ `EmailService.Provider` (dot doesn't work in env vars)

## Best Practices

### 1. Use Environment Variables for Secrets

```json
// appsettings.json (committed to git)
{
  "EmailService": {
    "Provider": "Infobip",
    "FromEmail": "noreply@lulocrm.com"
  },
  "Infobip": {
    "BasePath": "https://api.infobip.com"
    // ApiKey from environment variable
  }
}
```

```bash
# .env file (NOT committed to git)
Infobip__ApiKey=secret-api-key-here
```

### 2. Different Providers per Environment

```json
// appsettings.Development.json
{
  "EmailService": {
    "Provider": "Smtp"  // Use SMTP in development
  }
}

// appsettings.Production.json
{
  "EmailService": {
    "Provider": "Infobip"  // Use Infobip in production
  }
}
```

### 3. Validate in CI/CD

```yaml
# .github/workflows/deploy.yml
- name: Validate Configuration
  run: |
    dotnet build
    # Build will fail if configuration is invalid
```

### 4. Document Required Configuration

Keep this file and `Email-Strategy-Pattern.md` updated when adding new providers.

## Related Documentation

- [Email-Strategy-Pattern.md](./Email-Strategy-Pattern.md) - Complete email system architecture
- [AppSettings.cs](../Infrastructure/AppSettings.cs) - Configuration model classes
- [EmailServicesExtensions.cs](../Api/Extensions/EmailServicesExtensions.cs) - Validation implementation
