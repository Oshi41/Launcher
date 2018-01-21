using System;
using System.Windows;
using Universal_Launcher.Singleton;

namespace Universal_Launcher.Views
{
    /// <summary>
    ///     Логика взаимодействия для MainView.xaml
    /// </summary>
    public partial class MainView : IPasswordService
    {
        public MainView()
        {
            InitializeComponent();
            IoCContainer.Instanse.RegisterSingleton((IPasswordService) this);
        }

        public string GetPassword()
        {
            return PasswordBox.Password;
        }

        public bool IsNullOrEmpty => PasswordBox.SecurePassword.Length == 0;

        public void SetPassword(string newPassword)
        {
            PasswordBox.Password = newPassword;
        }

        // не используется
        public event EventHandler<bool> CheckPassword;

        private void OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown(0);
        }
    }
}