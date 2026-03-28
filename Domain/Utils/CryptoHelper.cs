using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;

namespace Domain.Utils
{
    public static class CryptoHelper
    {
        [SuppressMessage("Security", "CA5401:Use a non-default IV", Justification = "IV is generated uniquely for each encryption operation using GenerateIV()")]
        public static string EncryptString(string plainText, string key)
        {
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Convert.FromBase64String(key);
                // Generar un IV único para cada operación de cifrado
                aes.GenerateIV();

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    // Escribir el IV al inicio del stream
                    memoryStream.Write(aes.IV, 0, aes.IV.Length);

                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }

        public static string DecryptString(string cipherText, string key)
        {
            byte[] buffer = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Convert.FromBase64String(key);

                // Leer el IV del inicio del buffer
                byte[] iv = new byte[16];
                Array.Copy(buffer, 0, iv, 0, 16);
                aes.IV = iv;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                // Crear un nuevo buffer sin el IV (los primeros 16 bytes)
                byte[] dataBuffer = new byte[buffer.Length - 16];
                Array.Copy(buffer, 16, dataBuffer, 0, dataBuffer.Length);

                using (MemoryStream memoryStream = new MemoryStream(dataBuffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
