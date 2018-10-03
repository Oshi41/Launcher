using System.Windows;
using Launcher.Models;

namespace Launcher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Singleton.Instance.Config.ReadAndDownload();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            Singleton.Instance.Config.Save();
        }
    }
}
