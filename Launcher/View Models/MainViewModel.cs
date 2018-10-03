using System.Windows.Input;
using Mvvm;
using Mvvm.Commands;

namespace Launcher.View_Models
{
    class MainViewModel : BindableBase
    {
        #region Props

        public AuthViewModel AuthViewModel { get; set; }
        public ProjectViewModel ProjectViewModel { get; set; }
        public SettingsViewModel SettingsViewModel { get; set; }

        public ICommand StartCommand { get; private set; }
        public ICommand SwitchCommand { get; private set; }

        #endregion

        public MainViewModel()
        {
            AuthViewModel = new AuthViewModel();
            ProjectViewModel = new ProjectViewModel();
            SettingsViewModel = new SettingsViewModel();

            StartCommand = new DelegateCommand(OnStart, CanStart);
            SwitchCommand = new DelegateCommand<bool?>(OnChangeView);
        }

        #region Command Hanlers

        private bool CanStart()
        {
            return AuthViewModel.StartCommand.CanExecute(null)
                   && ProjectViewModel.LaunchCommand.CanExecute(null);
        }

        private void OnStart()
        {
            AuthViewModel.StartCommand.Execute(null);

            if (CanStart())
                ProjectViewModel.LaunchCommand.Execute(null);
        }

        private void OnChangeView(bool? isSettings)
        {
            if (isSettings == true)
            {
                SettingsViewModel.RefreshBySettings();
            }

            if (isSettings == false)
            {
                SettingsViewModel.SaveCommand.Execute(null);
            }
        }

        #endregion
    }
}
