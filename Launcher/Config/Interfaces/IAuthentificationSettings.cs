using Launcher.AuthModules;
using Newtonsoft.Json;

namespace Launcher.Config.Interfaces
{
    public interface IAuthentificationSettings : IConfig
    {
        Modules? Module { get; set; }
        string Name { get; set; }
        bool NeedToSave { get; set; }
        string Password { get; set; }
    }

    /// <summary>
    /// Настройки аутентификации
    /// </summary>
    class AuthentificationSettings : IAuthentificationSettings
    {
        /// <summary>
        /// Логин
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Пароль (шифрованный)
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Нужно ли сохранять пароль
        /// </summary>
        public bool NeedToSave { get; set; }

        /// <summary>
        /// Используем ли авторизацию Ely.ua
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Modules? Module { get; set; }

        #region Equals

        public override bool Equals(object obj)
        {
            if (obj is AuthentificationSettings settings)
                return Equals(settings);

            return false;
        }

        protected bool Equals(AuthentificationSettings other)
        {
            return string.Equals(Name, other.Name) && string.Equals(Password, other.Password) &&
                   NeedToSave == other.NeedToSave && Module == other.Module;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Password != null ? Password.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ NeedToSave.GetHashCode();
                hashCode = (hashCode * 397) ^ Module.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(AuthentificationSettings left, AuthentificationSettings right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(AuthentificationSettings left, AuthentificationSettings right)
        {
            return !Equals(left, right);
        }


        #endregion
    }
}