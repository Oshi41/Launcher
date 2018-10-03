using System.Linq;
using Launcher.Config;
using Launcher.Config.Interfaces;
using Launcher.Helper;
using Launcher.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LauncherTest
{
    [TestClass]
    public class ConfigTest
    {
        [TestMethod]
        public void WriteAndReadTests()
        {
            var config = LauncherConfig.Default;

            var project = config.GetConfig<ProjectSettings>();
            project.Servers.Add(new ServerInfo
            {
                Address = "MC.FRINEMINE.RU",
                Name = "Some Server"

            });

            var cofs = config.Configs.ToList();

            config.Save();

            config.Read();

            var confs1 = config.Configs.ToList();

            Assert.IsTrue(cofs.IsTermwiseEquals(confs1));
        }

    }
}
