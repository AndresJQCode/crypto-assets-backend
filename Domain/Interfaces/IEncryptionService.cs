namespace Domain.Interfaces;

/// <summary>
/// Service for encrypting and decrypting sensitive data (access tokens, credentials).
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// Encrypts plain text using AES encryption.
    /// </summary>
    /// <param name="plainText">The text to encrypt.</param>
    /// <returns>Base64-encoded encrypted string.</returns>
    Task<string> EncryptAsync(string plainText);

    /// <summary>
    /// Decrypts encrypted text using AES decryption.
    /// </summary>
    /// <param name="encryptedText">Base64-encoded encrypted string.</param>
    /// <returns>Decrypted plain text.</returns>
    Task<string> DecryptAsync(string encryptedText);
}
