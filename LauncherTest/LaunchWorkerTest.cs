using Launcher.Launch;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LauncherTest
{
    [TestClass]
    public class LaunchWorkerTest
    {
        [TestMethod]
        public void TestLibsArgs()
        {
            var worker = new ForgeLaunchWorker(@"H:\Users\Tom\AppData\Roaming\.minecraft");
            var args = worker.GetLauncArgs();

            Assert.IsTrue(true);
        }
    }
}
