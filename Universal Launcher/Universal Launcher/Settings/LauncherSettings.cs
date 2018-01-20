using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Universal_Launcher.Settings
{
    [Serializable]
    public class LauncherSettings
    {
        [XmlArray] public List<Server> Servers { get; set; }

        [XmlArray] public List<string> History { get; set; }

        public string LauncherUpdatePath { get; set; }
        public string Version { get; set; }


        //public LauncherSettings()
        //{
        //    //Servers = new List<Server>();
        //    //History = new List<string>();
        //    //Version = string.Empty;
        //    //LauncherUpdatePath = string.Empty;
        //}
        //public LauncherSettings(LauncherSettings settings)
        //{
        //    if (settings == null)
        //        return;

        //    Version = settings.Version;
        //    LauncherUpdatePath = settings.LauncherUpdatePath;
        //    History = settings.History.Select(x => x.ToString()).ToList();
        //    Servers = settings.Servers.Select(x => new Server(x)).ToList();
        //}
    }
}