using System;

namespace Universal_Launcher.Settings
{
    [Serializable]
    public class Server
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string DownloadLink { get; set; }

        // ReSharper disable once InconsistentNaming
        public string MD5 { get; set; }

        //public Server()
        //{
        //    //Name = string.Empty;
        //    //Address = string.Empty;
        //    //DownloadLink = string.Empty;
        //}

        //public Server(Server source)
        //{
        //    if (source == null)
        //        return;

        //    Name = source.Name;
        //    Address = source.Address;
        //    DownloadLink = source.DownloadLink;
        //}
    }
}