using System.Windows;
using System.Windows.Controls;

namespace Launcher.Windows
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
        }

        private void OnClose(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }

        private void Collapse(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);

            if (window != null)
                window.WindowState = WindowState.Minimized;
        }

        private void PerformFlip(object sender, RoutedEventArgs e)
        {
            FlipperControl.IsFlipped = !FlipperControl.IsFlipped;
        }
    }
}
