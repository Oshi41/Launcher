using Newtonsoft.Json;

namespace Launcher.Config.Interfaces
{
   public interface ILauncherSettings : IConfig
    {
        string BaseFolderPath { get; set; }
        string ConfigUri { get; set; }
        string RegistrationPage { get; set; }
    }

    class LauncherSettings : ILauncherSettings
    {
        /// <summary>
        /// Путь к конфигу
        /// </summary>
        public string ConfigUri { get; set; }

        /// <summary>
        /// Путь к базовой папке проекта
        /// </summary>
        public string BaseFolderPath { get; set; }

        /// <summary>
        /// Если есть - сайт для регистрации
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string RegistrationPage { get; set; }

        public LauncherSettings()
        {
            ConfigUri = LauncherConfig.Folder;
        }

        #region Equals members

        public override bool Equals(object obj)
        {
            if (obj is LauncherSettings settings)
                return Equals(settings);

            return false;
        }

        protected bool Equals(LauncherSettings other)
        {
            return string.Equals(ConfigUri, other.ConfigUri)
                   && string.Equals(BaseFolderPath, other.BaseFolderPath)
                   && string.Equals(RegistrationPage, other.RegistrationPage);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (ConfigUri != null ? ConfigUri.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (BaseFolderPath != null ? BaseFolderPath.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (RegistrationPage != null ? RegistrationPage.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(LauncherSettings left, LauncherSettings right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(LauncherSettings left, LauncherSettings right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}