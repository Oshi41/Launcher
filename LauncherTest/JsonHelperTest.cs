using Launcher.Helper;
using Launcher.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using SettingsHelper.ViewModels;

namespace LauncherTest
{
    [TestClass]
    public class JsonHelperTest
    {
        //[TestMethod]
        //public void ShouldWriteOnlyTypeProps()
        //{
        //    var serverInfo = new ServerViewModel
        //    {
        //        Name = "123",
        //        DownloadLink = "654",
        //        Address = "sdfgdfg",
        //        MD5 = "ergfsrtfhsgfrghdytr"
        //    };

        //    var json = JsonHelper.Serialize<IServerInfo>(serverInfo);
            
        //    Assert.IsFalse(json.Contains("CheckPing"));
        //}

        [TestMethod]
        public void CheckAttributesSettings()
        {
            var serverInfo = new ServerInfo()
            {
                Name = "123",
                DownloadLink = "654",
                Address = "sdfgdfg",
            };

            var json = JsonHelper.Serialize<IServerInfo>(serverInfo);

            var copy = JsonConvert.DeserializeObject<ServerInfo>(json);

            Assert.IsNull(copy.MD5);
        }
    }
}
