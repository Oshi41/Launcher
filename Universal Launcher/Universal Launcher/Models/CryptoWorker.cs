using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Universal_Launcher.Models
{
    public class CryptoWorker
    {
        /// <summary>
        ///     Шифруем пароль
        /// </summary>
        /// <typeparam name="T">
        ///     Любое симметричное шифрование,
        ///     я предпочитаю <see cref="AesManaged" />>
        /// </typeparam>
        /// <param name="value">Шифруемые данные</param>
        /// <param name="password">Ключ шифрования</param>
        /// <param name="salt">Зерно шифрования</param>
        /// <returns></returns>
        public string Encrypt<T>(string value, string password, string salt)
            where T : SymmetricAlgorithm, new()
        {
            DeriveBytes rgb = new Rfc2898DeriveBytes(password, Encoding.Unicode.GetBytes(salt));

            SymmetricAlgorithm algorithm = new T();

            var rgbKey = rgb.GetBytes(algorithm.KeySize >> 3);
            var rgbIV = rgb.GetBytes(algorithm.BlockSize >> 3);

            var transform = algorithm.CreateEncryptor(rgbKey, rgbIV);

            using (var buffer = new MemoryStream())
            {
                using (var stream = new CryptoStream(buffer, transform, CryptoStreamMode.Write))
                {
                    using (var writer = new StreamWriter(stream, Encoding.Unicode))
                    {
                        writer.Write(value);
                    }
                }

                return Convert.ToBase64String(buffer.ToArray());
            }
        }

        /// <summary>
        ///     Дешифруем пароль
        /// </summary>
        /// <typeparam name="T">
        ///     Любое симметричное шифрование,
        ///     я предпочитаю <see cref="AesManaged" />>
        /// </typeparam>
        /// <param name="text">Шифруемые данные</param>
        /// <param name="password">Ключ шифрования</param>
        /// <param name="salt">Зерно шифрования</param>
        /// <returns></returns>
        public string Decrypt<T>(string text, string password, string salt)
            where T : SymmetricAlgorithm, new()
        {
            DeriveBytes rgb = new Rfc2898DeriveBytes(password, Encoding.Unicode.GetBytes(salt));

            SymmetricAlgorithm algorithm = new T();

            var rgbKey = rgb.GetBytes(algorithm.KeySize >> 3);
            var rgbIV = rgb.GetBytes(algorithm.BlockSize >> 3);

            var transform = algorithm.CreateDecryptor(rgbKey, rgbIV);

            using (var buffer = new MemoryStream(Convert.FromBase64String(text)))
            {
                using (var stream = new CryptoStream(buffer, transform, CryptoStreamMode.Read))
                {
                    using (var reader = new StreamReader(stream, Encoding.Unicode))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }

        /// <summary>
        ///     Генерирует случайное зерно для шифрования
        /// </summary>
        /// <returns></returns>
        public string GetRandomSalt()
        {
            var rand = new Random();
            var length = rand.Next(8, 16);
            var buffer = new byte[length];
            rand.NextBytes(buffer);
            return Convert.ToBase64String(buffer);
        }
    }
}