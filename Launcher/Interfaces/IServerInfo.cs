using System.Collections.Generic;
using Launcher.Helper;
using Newtonsoft.Json;

namespace Launcher.Interfaces
{
    public interface IServerInfo
    {
        /// <summary>
        /// Путь к серверу
        /// </summary>
        string Address { get;  }

        /// <summary>
        /// Имя сервера, отображающеся в лаунчере
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Путь к архиву с файлами клиента
        /// </summary>
        string DownloadLink { get; }

        /// <summary>
        /// Хэш для запуска лаунчера. 
        /// <para>Ключ - Имя папки в клиенте начиная с корня</para>
        /// <para>Значение - хэш</para>
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        Dictionary<string, string> MD5 { get; }
    }

    public class ServerInfo : IServerInfo
    {
        #region Properties

        public string Address { get; set; }
        public string Name { get; set; }
        public string DownloadLink { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> MD5 { get; set; } 

        #endregion

        #region Equals members

        public override bool Equals(object obj)
        {
            if (obj is IServerInfo server)
            {
                return Equals(server);
            }

            return false;
        }

        protected bool Equals(IServerInfo other)
        {
            return string.Equals(Address, other.Address)
                   && string.Equals(Name, other.Name)
                   && string.Equals(DownloadLink, other.DownloadLink)
                   && SafeEqualsDictionary(MD5, other.MD5);
        }

        private bool SafeEqualsDictionary(Dictionary<string, string> md5, Dictionary<string, string> otherMd5)
        {
            if (md5.IsNullOrEmpty() && otherMd5.IsNullOrEmpty())
                return true;

            if (md5.IsNullOrEmpty() || otherMd5.IsNullOrEmpty())
                return true;

            foreach (var pair in md5)
            {
                // такого ключа нет
                if (!otherMd5.ContainsKey(pair.Key))
                    return false;

                // значения разные
                if (!string.Equals(pair.Value, otherMd5[pair.Key]))
                    return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Address != null ? Address.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (DownloadLink != null ? DownloadLink.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(ServerInfo left, IServerInfo right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ServerInfo left, IServerInfo right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}
