using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Security;

/// <summary>
/// AES encryption service for encrypting/decrypting sensitive data.
/// Uses AES-256-CBC with a key from environment variable (ENCRYPTION_KEY).
/// </summary>
[SuppressMessage("Security", "CA5401:Do not use CreateEncryptor with non-default IV", Justification = "A new random IV is generated for each encryption operation, which is the recommended practice")]
public class AesEncryptionService(IConfiguration configuration, ILogger<AesEncryptionService> logger) : IEncryptionService
{
    private const string EncryptionKeyEnvironmentVariable = "ENCRYPTION_KEY";

    public async Task<string> EncryptAsync(string plainText)
    {
        if (string.IsNullOrWhiteSpace(plainText))
            throw new ArgumentException("Plain text cannot be empty", nameof(plainText));

        try
        {
            var key = GetEncryptionKey();

            using var aes = Aes.Create();
            aes.Key = key;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            // Generate a new IV for each encryption
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var msEncrypt = new MemoryStream();

            // Prepend IV to the ciphertext
            await msEncrypt.WriteAsync(aes.IV);

            await using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            await using (var swEncrypt = new StreamWriter(csEncrypt))
            {
                await swEncrypt.WriteAsync(plainText);
            }

            var encrypted = msEncrypt.ToArray();
            return Convert.ToBase64String(encrypted);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to encrypt data");
            throw new InvalidOperationException("Encryption failed", ex);
        }
    }

    public async Task<string> DecryptAsync(string encryptedText)
    {
        if (string.IsNullOrWhiteSpace(encryptedText))
            throw new ArgumentException("Encrypted text cannot be empty", nameof(encryptedText));

        try
        {
            var key = GetEncryptionKey();
            var cipherBytes = Convert.FromBase64String(encryptedText);

            using var aes = Aes.Create();
            aes.Key = key;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var msDecrypt = new MemoryStream(cipherBytes);

            // Extract IV from the beginning of the ciphertext
            var iv = new byte[aes.BlockSize / 8];
            await msDecrypt.ReadAsync(iv.AsMemory(0, iv.Length));
            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            await using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);

            return await srDecrypt.ReadToEndAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to decrypt data");
            throw new InvalidOperationException("Decryption failed", ex);
        }
    }

    private byte[] GetEncryptionKey()
    {
        var base64Key = configuration[EncryptionKeyEnvironmentVariable]
                       ?? Environment.GetEnvironmentVariable(EncryptionKeyEnvironmentVariable);

        if (string.IsNullOrWhiteSpace(base64Key))
        {
            throw new InvalidOperationException(
                $"Encryption key not found. Please set the {EncryptionKeyEnvironmentVariable} environment variable.");
        }

        try
        {
            var key = Convert.FromBase64String(base64Key);

            if (key.Length != 32) // AES-256 requires 32-byte key
            {
                throw new InvalidOperationException(
                    $"Invalid encryption key length. Expected 32 bytes (256 bits), got {key.Length} bytes.");
            }

            return key;
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException(
                $"Invalid encryption key format. The {EncryptionKeyEnvironmentVariable} must be a valid Base64 string.", ex);
        }
    }
}
