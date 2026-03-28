# Servicios de Autenticación con Proveedores Externos

## Descripción

Este módulo implementa el patrón Strategy para manejar diferentes proveedores de autenticación externos (Google y Microsoft) en el endpoint `Auth/exchangeCode`. La implementación incluye integración real con las APIs de OAuth de Google y Microsoft Graph.

## Arquitectura

### Patrón Strategy Implementado

- **IAuthProviderService**: Interfaz base que define el contrato para todos los proveedores
- **GoogleAuthProviderService**: Implementación específica para Google OAuth con integración real
- **MicrosoftAuthProviderService**: Implementación específica para Microsoft Graph con integración real
- **IAuthProviderFactory**: Factory para obtener el proveedor correcto
- **AuthProviderFactory**: Implementación del factory que maneja la selección del proveedor

### Servicios de Infraestructura

- **IGoogleOAuthService**: Servicio para intercambiar códigos y obtener información de Google OAuth
- **IMicrosoftOAuthService**: Servicio para intercambiar códigos y obtener información de Microsoft Graph
- **IJwtTokenService**: Servicio para generar y validar tokens JWT

### Flujo de Autenticación

1. El cliente envía una petición POST a `/Auth/exchangeCode` con:

   - `code`: Código de autorización del proveedor
   - `provider`: Nombre del proveedor ("Google" o "Microsoft")

2. El endpoint utiliza MediatR para procesar el comando `ExchangeCodeCommand`

3. El handler `ExchangeCodeCommandHandler` utiliza el factory para obtener el servicio del proveedor correcto

4. El servicio del proveedor específico:

   - **Intercambia el código por un token de acceso** usando el servicio de infraestructura correspondiente
   - **Obtiene información del usuario** desde la API del proveedor (Google OAuth o Microsoft Graph)
   - **Busca o crea el usuario** en la base de datos usando ASP.NET Core Identity
   - **Genera un JWT token** para la aplicación usando el servicio JWT

5. Retorna la respuesta con el token JWT y la información del usuario

## Endpoints

### POST /Auth/exchangeCode

**Request:**

```json
{
  "code": "authorization_code_from_provider",
  "provider": "Google" // o "Microsoft"
}
```

**Response:**

```json
{
  "id": 1,
  "accessToken": "jwt_token",
  "role": "User",
  "email": "user@example.com",
  "fullName": "Usuario Ejemplo",
  "provider": "Google"
}
```

## Configuración

### 1. Configurar Google OAuth

1. Ve a [Google Cloud Console](https://console.cloud.google.com/)
2. Crea un nuevo proyecto o selecciona uno existente
3. Habilita la API de Google+ y Google OAuth2
4. Crea credenciales OAuth 2.0
5. Configura las URIs de redirección autorizadas

### 2. Configurar Microsoft OAuth

1. Ve a [Azure Portal](https://portal.azure.com/)
2. Registra una nueva aplicación en Azure Active Directory
3. Configura los permisos necesarios (User.Read, openid, profile, email)
4. Genera un secreto de cliente
5. Configura las URIs de redirección

### 3. Configuración en appsettings.json

```json
{
  "JwtSettings": {
    "SecretKey": "tu-clave-secreta-jwt",
    "Issuer": "tu-dominio.com",
    "Audience": "tu-dominio.com",
    "ExpirationMinutes": "60"
  },
  "Authentication": {
    "Google": {
      "ClientId": "tu-google-client-id.apps.googleusercontent.com",
      "ClientSecret": "tu-google-client-secret",
      "RedirectUri": "https://tu-dominio.com/Auth/exchangeCode"
    },
    "Microsoft": {
      "ClientId": "tu-microsoft-client-id",
      "ClientSecret": "tu-microsoft-client-secret",
      "RedirectUri": "https://tu-dominio.com/Auth/exchangeCode",
      "TenantId": "common"
    }
  }
}
```

## Proveedores Soportados

### Google OAuth 2.0

- **Endpoint**: `https://oauth2.googleapis.com/token`
- **User Info**: `https://www.googleapis.com/oauth2/v2/userinfo`
- **Scopes**: `openid profile email`

### Microsoft Graph

- **Endpoint**: `https://login.microsoftonline.com/{tenant}/oauth2/v2.0/token`
- **User Info**: `https://graph.microsoft.com/v1.0/me`
- **Scopes**: `openid profile email`

## Extensibilidad

Para agregar un nuevo proveedor:

1. Crear un servicio de infraestructura que implemente la lógica HTTP específica
2. Crear una nueva clase que implemente `IAuthProviderService`
3. Implementar el método `ExchangeCodeAsync` con la lógica específica del proveedor
4. Registrar el servicio en `AuthExtensions.cs`
5. El factory automáticamente detectará y registrará el nuevo proveedor

## Características Implementadas

✅ **Integración real con Google OAuth 2.0**
✅ **Integración real con Microsoft Graph**
✅ **Generación de tokens JWT**
✅ **Gestión de usuarios con ASP.NET Core Identity**
✅ **Manejo de errores y logging**
✅ **Configuración flexible por ambiente**
✅ **Patrón Strategy para extensibilidad**

## Documentación Adicional

- **[Sistema de Permisos](./SISTEMA_PERMISOS.md)**: Documentación completa del sistema de permisos dinámico
- **[Permisos de API](./PERMISOS_API.md)**: Documentación detallada de todos los permisos requeridos por endpoint
- **[Resumen de Implementación](./RESUMEN_IMPLEMENTACION.md)**: Resumen general de la arquitectura del sistema

## Notas de Seguridad

- Los client secrets deben almacenarse de forma segura (Azure Key Vault, AWS Secrets Manager, etc.)
- Los tokens JWT tienen expiración configurable
- Se recomienda usar HTTPS en producción
- Los redirect URIs deben coincidir exactamente con los configurados en los proveedores
