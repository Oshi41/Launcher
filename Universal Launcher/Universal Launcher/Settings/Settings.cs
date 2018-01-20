using System;

namespace Universal_Launcher.Settings
{
    [Serializable]
    public class Settings
    {
        public Settings()
        {
            AuthSettings = new AuthSettings();
            SystemSettings = new SystemSettings();
            LauncherSettings = new LauncherSettings();
        }

        public AuthSettings AuthSettings { get; set; }
        public SystemSettings SystemSettings { get; set; }
        public LauncherSettings LauncherSettings { get; set; }

        //public Settings()
        //{
        //    AuthSettings = new AuthSettings();
        //    SystemSettings = new SystemSettings();
        //    LauncherSettings = new LauncherSettings();
        //}

        //public Settings(Settings settings)
        //{
        //    if (settings == null)
        //        return;

        //    AuthSettings = new AuthSettings(settings.AuthSettings);
        //    SystemSettings = new SystemSettings(settings.SystemSettings);
        //    LauncherSettings = new LauncherSettings(settings.LauncherSettings);
        //}
    }
}