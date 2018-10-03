using System;
using System.Collections.Generic;
using Launcher.Helper;
using Launcher.Interfaces;
using Newtonsoft.Json;

namespace Launcher.Config.Interfaces
{
    public interface IProjectSettings : IConfig
    {
        string ExeLink { get;  }
        List<ServerInfo> Servers { get;  }
        Version Version { get;  }
    }

    /// <summary>
    /// Сервера проекта. Предназначен для скачивания
    /// </summary>
    public class ProjectSettings : IProjectSettings
    {
        /// <summary>
        /// Версия лаунчера по ссылке внизу
        /// </summary>
        [JsonConverter(typeof(VersionConverter))]
        public Version Version { get; set; } = new Version();

        /// <summary>
        /// Ссылка для скачивания
        /// </summary>
        public string ExeLink { get; set; }

        /// <summary>
        /// Список серверов проекта
        /// </summary>
        public List<ServerInfo> Servers { get; set; } = new List<ServerInfo>();

        #region Nested class

        public class VersionConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                if (value == null)
                {
                    writer.WriteNull();
                }
                else
                {
                    var toWrite = value.ToString();
                    writer.WriteValue(toWrite);
                }
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                if (Version.TryParse(reader?.Value?.ToString(), out var version))
                    return version;

                return new Version();
            }

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(Version);
            }
        }

        #endregion

        #region Equals

        public override bool Equals(object obj)
        {
            if (obj is ProjectSettings settings)
                return Equals(settings);

            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Version != null ? Version.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ExeLink != null ? ExeLink.GetHashCode() : 0);
                return hashCode;
            }
        }

        protected bool Equals(ProjectSettings other)
        {
            return Servers.IsTermwiseEquals(other.Servers)
                   && Equals(Version, other.Version)
                   && string.Equals(ExeLink, other.ExeLink);
        }

        public static bool operator ==(ProjectSettings left, ProjectSettings right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ProjectSettings left, ProjectSettings right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}