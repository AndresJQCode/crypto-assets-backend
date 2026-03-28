# Encryption Setup Guide

## Overview

El sistema utiliza **AES-256-CBC** para encriptar datos sensibles como:
- Access tokens de conectores (Shopify, WooCommerce, etc.)
- Credenciales OAuth
- Cualquier dato sensible en la base de datos

## Generar Llave de Encriptación

### Opción 1: Script PowerShell (Windows)

```powershell
.\scripts\generate-encryption-key.ps1
```

### Opción 2: Script Bash (Linux/Mac)

```bash
chmod +x scripts/generate-encryption-key.sh
./scripts/generate-encryption-key.sh
```

### Opción 3: Manual con OpenSSL

```bash
openssl rand -base64 32
```

### Opción 4: Manual con C#

```csharp
using System.Security.Cryptography;

var bytes = new byte[32];
using var rng = RandomNumberGenerator.Create();
rng.GetBytes(bytes);
var key = Convert.ToBase64String(bytes);
Console.WriteLine(key);
```

## Configuración

### 1. Desarrollo Local (appsettings.Development.json)

```json
{
  "EncryptionKey": "YOUR-BASE64-KEY-HERE"
}
```

**IMPORTANTE:**
- ✅ Agregar `appsettings.Development.json` al `.gitignore`
- ❌ NUNCA commitear la llave al repositorio

### 2. Variables de Entorno (Recomendado para Producción)

```bash
# Linux/Mac
export EncryptionKey="YOUR-BASE64-KEY-HERE"

# Windows PowerShell
$env:EncryptionKey = "YOUR-BASE64-KEY-HERE"

# Windows CMD
set EncryptionKey=YOUR-BASE64-KEY-HERE
```

### 3. Azure App Service (Application Settings)

```bash
az webapp config appsettings set \
  --resource-group myResourceGroup \
  --name myAppName \
  --settings EncryptionKey="YOUR-BASE64-KEY-HERE"
```

### 4. Docker / Kubernetes

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: lulo-crm-secrets
type: Opaque
data:
  encryption-key: WU9VUi1CQVNFNjQtS0VZLUhFUkU=  # Base64 encoded
```

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: lulo-crm-api
spec:
  template:
    spec:
      containers:
      - name: api
        env:
        - name: EncryptionKey
          valueFrom:
            secretKeyRef:
              name: lulo-crm-secrets
              key: encryption-key
```

### 5. Azure Key Vault (Mejor Práctica)

#### Setup

```bash
# Crear Key Vault
az keyvault create \
  --name lulo-crm-kv \
  --resource-group myResourceGroup \
  --location eastus

# Agregar secret
az keyvault secret set \
  --vault-name lulo-crm-kv \
  --name EncryptionKey \
  --value "YOUR-BASE64-KEY-HERE"

# Otorgar acceso a la App Service
az keyvault set-policy \
  --name lulo-crm-kv \
  --object-id <app-service-managed-identity-object-id> \
  --secret-permissions get list
```

#### Configuración en appsettings.json

```json
{
  "KeyVault": {
    "VaultUri": "https://lulo-crm-kv.vault.azure.net/"
  }
}
```

#### Program.cs

```csharp
var keyVaultEndpoint = builder.Configuration["KeyVault:VaultUri"];
if (!string.IsNullOrEmpty(keyVaultEndpoint))
{
    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultEndpoint),
        new DefaultAzureCredential());
}
```

## Uso en el Código

### Encriptar

```csharp
public class MyService(IEncryptionService encryptionService)
{
    public async Task SaveAccessToken(string token)
    {
        var encryptedToken = await encryptionService.EncryptAsync(token);
        // Guardar encryptedToken en la base de datos
    }
}
```

### Desencriptar

```csharp
public class MyService(IEncryptionService encryptionService)
{
    public async Task<string> GetAccessToken()
    {
        // Obtener encryptedToken de la base de datos
        var token = await encryptionService.DecryptAsync(encryptedToken);
        return token;
    }
}
```

## Rotación de Llaves

### Proceso de Rotación

1. **Generar nueva llave:**
   ```bash
   openssl rand -base64 32
   ```

2. **Implementar soporte multi-llave** (futuro):
   ```json
   {
     "EncryptionKeys": {
       "Current": "NEW-KEY",
       "Previous": ["OLD-KEY-1", "OLD-KEY-2"]
     }
   }
   ```

3. **Re-encriptar datos existentes:**
   ```csharp
   // Script de migración
   var allRecords = await repository.GetAllEncryptedRecords();

   foreach (var record in allRecords)
   {
       // Desencriptar con llave vieja
       var decrypted = await oldEncryptionService.DecryptAsync(record.EncryptedData);

       // Re-encriptar con llave nueva
       record.EncryptedData = await newEncryptionService.EncryptAsync(decrypted);

       await repository.UpdateAsync(record);
   }
   ```

4. **Remover llave vieja** después de confirmar que todo funciona.

### Frecuencia Recomendada

- **Desarrollo:** No es necesario rotar
- **Staging:** Cada 6 meses
- **Producción:** Cada 90-180 días

## Validación

### Test de Encriptación

```csharp
[Fact]
public async Task EncryptDecrypt_ShouldReturnOriginalText()
{
    // Arrange
    var service = new AesEncryptionService(options, logger);
    var plainText = "sensitive-data-123";

    // Act
    var encrypted = await service.EncryptAsync(plainText);
    var decrypted = await service.DecryptAsync(encrypted);

    // Assert
    Assert.NotEqual(plainText, encrypted);
    Assert.Equal(plainText, decrypted);
}
```

### Verificar Configuración

```bash
# Verificar que la llave esté configurada
dotnet run --project Api -- --verify-encryption

# O manualmente
curl https://your-api.com/health/encryption
```

## Troubleshooting

### Error: "Encryption key not found"

**Causa:** La llave no está configurada en appsettings.json o variables de entorno.

**Solución:**
1. Generar llave: `openssl rand -base64 32`
2. Agregar a appsettings.Development.json o variable de entorno

### Error: "Invalid encryption key length"

**Causa:** La llave no tiene exactamente 32 bytes (256 bits).

**Solución:**
- Asegurar que la llave Base64 decodificada tenga 32 bytes
- Regenerar llave usando los scripts proporcionados

### Error: "Invalid encryption key format"

**Causa:** La llave no es un string Base64 válido.

**Solución:**
- Verificar que no haya espacios o caracteres especiales
- Regenerar llave

### Error: "Decryption failed"

**Posibles causas:**
1. Datos encriptados con una llave diferente
2. Datos corruptos en la base de datos
3. Llave cambiada sin re-encriptar datos existentes

**Solución:**
- Verificar que la llave sea la correcta
- Si cambió la llave, re-encriptar los datos existentes

## Seguridad

### Best Practices

✅ **DO:**
- Usar llaves diferentes para cada entorno (dev, staging, prod)
- Almacenar llaves en servicios seguros (Azure Key Vault, AWS Secrets Manager)
- Rotar llaves periódicamente
- Auditar acceso a las llaves
- Usar HTTPS para toda comunicación
- Implementar backup de llaves encriptadas

❌ **DON'T:**
- Commitear llaves al repositorio
- Compartir llaves por email/Slack
- Usar la misma llave en múltiples entornos
- Hard-codear llaves en el código
- Almacenar llaves en texto plano
- Loggear llaves o datos desencriptados

### Checklist de Seguridad

- [ ] Llave generada con RNG criptográficamente seguro
- [ ] Llave de 256 bits (32 bytes)
- [ ] Llave almacenada en Key Vault o equivalente
- [ ] Llaves diferentes por entorno
- [ ] `.gitignore` incluye archivos de configuración local
- [ ] Acceso a Key Vault restringido
- [ ] Logging NO incluye datos sensibles
- [ ] Plan de rotación de llaves documentado
- [ ] Backup de llaves implementado

## Compliance

### GDPR / CCPA

- Datos encriptados en reposo (base de datos)
- Datos encriptados en tránsito (HTTPS)
- Capacidad de eliminar datos (derecho al olvido)

### PCI DSS (si aplica)

- AES-256 cumple con requisitos PCI DSS
- Rotación de llaves cada 90 días
- Auditoría de acceso a llaves
- Segregación de ambientes

## Referencias

- [NIST Cryptographic Standards](https://csrc.nist.gov/publications/detail/sp/800-175b/rev-1/final)
- [OWASP Cryptographic Storage Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Cryptographic_Storage_Cheat_Sheet.html)
- [Azure Key Vault Best Practices](https://learn.microsoft.com/en-us/azure/key-vault/general/best-practices)
- [.NET Cryptography Overview](https://learn.microsoft.com/en-us/dotnet/standard/security/cryptography-model)
