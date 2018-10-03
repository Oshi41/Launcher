using System.Security.Cryptography;
using Launcher.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LauncherTest
{
    [TestClass]
    public class CryptoWorkertTest
    {
        [TestMethod]
        public void TestCryptoWorker()
        {
            var text = "Once upon a time there was a little boy...";
            var pass = "12345678";
            var worker = new CryptoWorker();
            var salt = worker.GetUniqueSalt();
            var encrypted = worker.Encrypt<RijndaelManaged>(text, pass, salt);

            Assert.AreEqual(salt, worker.GetUniqueSalt());

            Assert.AreEqual(text, worker.Decrypt<RijndaelManaged>(encrypted, pass, salt));
        }
    }
}
