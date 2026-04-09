using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Security.Claims;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace ApiOAuthEmpleados.Helpers
{
    public class HelperCryptography
    {
        public static string GetUserToken(ClaimsPrincipal user, IConfiguration configuration)
        {
            Claim claim = user.Claims.FirstOrDefault(c => c.Type == "UserData");
            if (claim == null) return null;
            return DecryptString(claim.Value, configuration.GetValue<string>("ApiOAuthToken:ClaveEncriptacion"));
        }

        /// <summary>
        /// Cifra una cadena de texto utilizando AES-256.
        /// </summary>
        public static string EncryptString(string plainText, string password)
        {
            if (string.IsNullOrEmpty(plainText)) return plainText;

            // Asegurar que la clave tenga 32 bytes (256 bits)
            byte[] key = Encoding.UTF8.GetBytes(password.PadRight(32).Substring(0, 32));

            using Aes aes = Aes.Create();
            aes.Key = key;
            aes.GenerateIV();

            using MemoryStream memoryStream = new MemoryStream();
            // Guardamos el Vector de Inicialización (IV) al principio para usarlo al descifrar
            memoryStream.Write(aes.IV, 0, aes.IV.Length);

            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
            using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
            {
                streamWriter.Write(plainText);
            }

            return Convert.ToBase64String(memoryStream.ToArray());
        }

        /// <summary>
        /// Descifra una cadena de texto cifrada previamente con AES-256.
        /// </summary>
        public static string DecryptString(string cipherText, string password)
        {
            if (string.IsNullOrEmpty(cipherText)) return cipherText;

            byte[] fullCipher = Convert.FromBase64String(cipherText);
            byte[] key = Encoding.UTF8.GetBytes(password.PadRight(32).Substring(0, 32));

            using Aes aes = Aes.Create();
            aes.Key = key;

            // Extraemos el IV del principio del arreglo
            byte[] iv = new byte[aes.BlockSize / 8];
            Array.Copy(fullCipher, 0, iv, 0, iv.Length);
            aes.IV = iv;

            using MemoryStream memoryStream = new MemoryStream(fullCipher, iv.Length, fullCipher.Length - iv.Length);
            using CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using StreamReader streamReader = new StreamReader(cryptoStream);

            return streamReader.ReadToEnd();
        }
    }
}
