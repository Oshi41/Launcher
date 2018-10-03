using Launcher.Config;
using Launcher.Interfaces;

namespace Launcher.Models
{
    public class Singleton
    {
        #region Singleton implementation
        private static Singleton _instance;
        public static Singleton Instance => _instance ?? (_instance = new Singleton());

        private Singleton()
        {
            Config = LauncherConfig.Default;
            //Config = new LauncherConfig();
        } 

        #endregion

        public LauncherConfig Config { get; private set; }

        public IPassword PasswordService { get; private set; }

        public string AssessToken { get; private set; }

        #region Methods

        public void Register(IPassword pass)
        {
            PasswordService = pass;
        }

        public void SetAssessToken(string token)
        {
            AssessToken = token;
        }

        #endregion
    }
}
