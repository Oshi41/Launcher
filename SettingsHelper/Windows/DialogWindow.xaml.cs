using System.Windows;

namespace SettingsHelper.Windows
{
    /// <summary>
    /// Interaction logic for DialogWindow.xaml
    /// </summary>
    public partial class DialogWindow : Window
    {
        public DialogWindow()
        {
            InitializeComponent();
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SetResult(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
