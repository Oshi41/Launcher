using System;
using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;
using Microsoft.VisualBasic.Devices;

namespace Universal_Launcher.Views
{
    /// <summary>
    /// Логика взаимодействия для SettingsView.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            var info = new ComputerInfo();
            // получаем 80% от свободной памяти в МБ
            var maxMem = info.AvailablePhysicalMemory / 1024 / 1024 / 10 * 8;
            Slider.Maximum = Math.Max(maxMem, 2048);
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown(1);
        }

        private void CollapseExpander(object sender, RoutedEventArgs e)
        {
            if (Expander.IsExpanded)
            {
                Expander.IsExpanded = false;
            }
            else
            {
                Flipper.FlipCommand.Execute(null, null);
            }
        }
    }
}
