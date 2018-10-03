using System.ComponentModel;
using System.Windows.Input;
using SettingsHelper.Windows;

namespace SettingsHelper.Service
{
    public interface IDialogWindowViewModel : INotifyPropertyChanged
    {
        ICommand CloseCommand { get; }
        ICommand OkCommand { get; }
    }

    public static class WindowService
    {
        public static bool? ShowDialog(IDialogWindowViewModel vm, string title = null)
        {
            var window = new DialogWindow
            {
                DataContext = vm,
                Content = vm,
                Title = title
            };

            return window.ShowDialog();
        }
    }
}
