using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Launcher.Models
{
    public class HashChecker
    {
        /// <summary>
        ///     Рассчитывает хэш сумму всех файлов в указанном пути и во вложенных папках.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string CreateMd5ForFolder(string path)
        {
            if (!Directory.Exists(path))
                return string.Empty;

            // assuming you want to include nested folders
            var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).ToList();

            var md5 = MD5.Create();

            for (var i = 0; i < files.Count; i++)
            {
                var file = files[i];

                // hash path
                var relativePath = file.Substring(path.Length + 1);
                var pathBytes = Encoding.UTF8.GetBytes(relativePath.ToLower());
                md5.TransformBlock(pathBytes, 0, pathBytes.Length, pathBytes, 0);

                // hash contents
                var contentBytes = File.ReadAllBytes(file);
                if (i == files.Count - 1)
                    md5.TransformFinalBlock(contentBytes, 0, contentBytes.Length);
                else
                    md5.TransformBlock(contentBytes, 0, contentBytes.Length, contentBytes, 0);
            }

            return BitConverter.ToString(md5.Hash).Replace("-", "").ToLower();
        }
    }
}
