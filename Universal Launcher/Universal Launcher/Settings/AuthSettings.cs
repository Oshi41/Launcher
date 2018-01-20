using System;

namespace Universal_Launcher.Settings
{
    [Serializable]
    public class AuthSettings
    {
        public string Login { get; set; }
        public string EnctyptPassword { get; set; }
        public string Key { get; set; }
        public string Salt { get; set; }
        public bool RememberPassword { get; set; }

        //public AuthSettings()
        //{
        //    Login = string.Empty;
        //    EnctyptPassword = string.Empty;
        //    Key = string.Empty;
        //    Salt = string.Empty;
        //    RememberPassword = true;
        //}

        //public AuthSettings(AuthSettings settings)
        //{
        //    if (settings == null)
        //        return;

        //    Login = settings.Login;
        //    EnctyptPassword = settings.EnctyptPassword;
        //    Key = settings.Key;
        //    Salt = settings.Salt;
        //    RememberPassword = settings.RememberPassword;
        //}
    }
}