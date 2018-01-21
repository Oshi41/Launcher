using System.Windows;
using System.Windows.Threading;

namespace Universal_Launcher
{
    /// <summary>
    ///     Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string ProjectName = "Titan Project";

        public App()
        {
            DispatcherUnhandledException += Excep;
        }

        private void Excep(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message);
        }
    }
}