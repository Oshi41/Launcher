using System;
using System.Windows;
using System.Windows.Controls;
using Launcher.Interfaces;
using Launcher.Models;

namespace Launcher.Windows
{
    /// <summary>
    /// Interaction logic for AuthView.xaml
    /// </summary>
    public partial class AuthView : UserControl, IPassword
    {
        public AuthView()
        {
            InitializeComponent();

            PasswordBox.PasswordChanged += (sender, args) => PassChanged?.Invoke(sender, args);

            Loaded += OnLoad;
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            Singleton.Instance.Register(this);
        }

        public string GetPassword()
        {
            return PasswordBox.Password;
        }

        public void SetPassword(string pass)
        {
            PasswordBox.Password = pass;
        }

        public event EventHandler PassChanged;
    }
}
