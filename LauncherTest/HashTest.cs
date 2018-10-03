using Launcher.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LauncherTest
{
    [TestClass]
    public class HashTest
    {
        [TestMethod]
        public void TestHash()
        {
            var hashChecker = new HashChecker();
            var path = @"H:\Users\Tom\Documents\Minecraft\Mods";

            var hash = hashChecker.CreateMd5ForFolder(path);

            var hash2 = hashChecker.CreateMd5ForFolder(path);

            Assert.AreEqual(hash2, hash);
        }
    }
}
