using System;

namespace Universal_Launcher.Settings
{
    [Serializable]
    public class SystemSettings
    {
        public int Memory { get; set; }
        public bool UseJavaArgs { get; set; }
        public bool DebugMode { get; set; }

        //public SystemSettings()
        //{
        //    Memory = 2048;
        //    UseJavaArgs = false;
        //    DebugMode = false;
        //}

        //public SystemSettings(SystemSettings settings)
        //{
        //    if (settings == null)
        //        return;

        //    Memory = settings.Memory;
        //    UseJavaArgs = settings.UseJavaArgs;
        //    DebugMode = settings.DebugMode;
        //}
    }
}