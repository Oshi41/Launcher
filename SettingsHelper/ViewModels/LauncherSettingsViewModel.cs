using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows.Forms;
using Mvvm;
using System.Windows.Input;
using Launcher.Config.Interfaces;
using Launcher.Helper;
using Launcher.Models;
using Mvvm.Commands;
using Newtonsoft.Json;
using SettingsHelper.ViewModels.Base;

namespace SettingsHelper.ViewModels
{
    class LauncherSettingsViewModel : ConfigBase<ILauncherSettings>, ILauncherSettings
    {
        #region Fields

        private string _baseFolderPath;
        private string _configUri;
        private string _registrationPage;
        private ErrorViewModel _baseFolderErrorStatus;
        private ErrorViewModel _configUriErrorStatus;
        private ErrorViewModel _registrationPageErrorStatus;

        private readonly WebClient _webClient = new WebClient();
        private readonly Ping _ping = new Ping();
        private readonly ActionArbiter _configArbiter = new ActionArbiter();
        private readonly ActionArbiter _pingArbiter = new ActionArbiter();

        #endregion

        #region Props

        public ErrorViewModel BaseFolderErrorStatus
        {
            get => _baseFolderErrorStatus;
            set => SetProperty(ref _baseFolderErrorStatus, value);
        }

        public ErrorViewModel ConfigUriErrorStatus
        {
            get => _configUriErrorStatus;
            set => SetProperty(ref _configUriErrorStatus, value);
        }

        public ErrorViewModel RegistrationPageErrorStatus
        {
            get => _registrationPageErrorStatus;
            set => SetProperty(ref _registrationPageErrorStatus, value);
        }

        #endregion

        #region Commands

        public ICommand OpenFolder { get; private set; }
        public ICommand CheckConfigUri { get; private set; }
        public ICommand PingSite { get; private set; }


        #endregion

        public LauncherSettingsViewModel()
        {
            OpenFolder = new DelegateCommand(OnOpenCommand);
            CheckConfigUri = new DelegateCommand(OnCheckConfig, () => CanDownload(_manager));
            PingSite = new DelegateCommand(OnPing, () => !_pingArbiter.IsBusy());
        }

        #region Command handlers

        private void OnPing()
        {
            if (_pingArbiter.IsBusy())
                return;

            RegistrationPageErrorStatus = new ErrorViewModel();

            async void Action()
            {
                try
                {
                    var result = await _ping.SendPingAsync(RegistrationPage);
                    var isSuccess = result.Status == IPStatus.Success;
                    var text = isSuccess
                        ? null
                        : "Ошибка в доступе к сайту, - " + result.Status;
                    SafeExecute(() => RegistrationPageErrorStatus = new ErrorViewModel(text, !isSuccess, isSuccess));
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e.Message);
                    SafeExecute(() => RegistrationPageErrorStatus = ErrorViewModel.GetError(e.Message));
                }
            }

            _pingArbiter.Do(Action);
        }

        private async void OnCheckConfig()
        {
            if (!CheckConfigUri.CanExecute(null))
                return;

            _manager = new DownloadManager(ConfigUri);
            var path = await _manager.Download();

            try
            {
                var json = File.ReadAllText(path);
                JsonConvert.DeserializeObject<ProjectSettings>(json);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                ConfigUriErrorStatus = ErrorViewModel.GetError(e.Message);
            }

            _manager.DeleteFile();

            ConfigUriErrorStatus = new ErrorViewModel(null, _manager.IsError, !_manager.IsError);

            if (_manager.IsError)
                ConfigUriErrorStatus.Text = _manager.LastError.Message;
        }

        private void OnOpenCommand()
        {
            BaseFolderErrorStatus = new ErrorViewModel();

            var dlg = new FolderBrowserDialog { ShowNewFolderButton = true };

            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            var path = dlg.SelectedPath;

            if (!Directory.Exists(path))
            {
                BaseFolderErrorStatus = ErrorViewModel.GetSuccess();
                return;
            }

            try
            {
                var info = new DirectoryInfo(path);
                var isSuccess = info.GetFiles().Length == 0;
                var text = isSuccess
                    ? null
                    : "Выберите пустую папку";

                BaseFolderErrorStatus = new ErrorViewModel(text, !isSuccess, isSuccess);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                BaseFolderErrorStatus = new ErrorViewModel("Ошибка", true);
            }
        } 
        #endregion

        #region ILauncherSettings

        public string BaseFolderPath
        {
            get => _baseFolderPath;
            set => SetProperty(ref _baseFolderPath, value);
        }

        public string ConfigUri
        {
            get => _configUri;
            set => SetProperty(ref _configUri, value);
        }

        public string RegistrationPage
        {
            get => _registrationPage;
            set => SetProperty(ref _registrationPage, value);
        }

        #endregion
    }

    public class ErrorViewModel : BindableBase
    {
        private string _text;
        private bool _isError;
        private bool _isSuccess;

        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }

        public bool IsError
        {
            get => _isError;
            set => SetProperty(ref _isError, value);
        }

        public bool IsSuccess
        {
            get => _isSuccess;
            set => SetProperty(ref _isSuccess, value);
        }

        public ErrorViewModel(string text = null, bool isError = false, bool isSuccess = false)
        {
            _text = text;
            _isError = isError;
            _isSuccess = isSuccess;
        }

        public static ErrorViewModel GetSuccess(string text = null)
        {
            return new ErrorViewModel(text, isSuccess: true);
        }

        public static ErrorViewModel GetError(string text = null)
        {
            return new ErrorViewModel(text, true);
        }
    }

    public class ActionArbiter
    {
        private bool _isActing;

        public bool IsBusy()
        {
            return _isActing;
        }

        public async Task Do(Action action)
        {
            if (IsBusy())
                return;

            _isActing = true;

            try
            {
                await Task.Run(action);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                throw;
            }
            finally
            {
                _isActing = false;
            }
        }
    }

}
