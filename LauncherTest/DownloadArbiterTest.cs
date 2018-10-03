using System.Collections.Generic;
using System.IO;
using Launcher.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LauncherTest
{
    [TestClass]
    public class DownloadArbiterTest
    {
        [TestMethod]
        public void DownloadTest()
        {
            var path = Path.GetTempFileName();
            var manager = new DownloadManager(@"http://bit.ly/2MVjI0p", path);
            var list = new List<int>();

            manager.ProgressChanged += (sender, args) =>
            {
                list.Add(manager.Speed);
            };

            manager.Download().Wait();
        }
    }
}
