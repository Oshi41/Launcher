using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Universal_Launcher.Models;
using Universal_Launcher.Models.Installers;
using Universal_Launcher.Models.Singleton;
using Universal_Launcher.MVVM;
using Universal_Launcher.Singleton;

namespace Universal_Launcher.ViewModels
{
    public class MainViewModel : ViewModelBase, INameService
    {
        public MainViewModel()
        {
            // подписываюсь на событие при полной готовности к загрузке
            // иначе мы не получим IPasswordService
            Application.Current.Activated += Initialize;
        }

        private async void Initialize(object sender, EventArgs eventArgs)
        {
            //отписались от процесса инициализации
            Application.Current.Activated -= Initialize;

            RefreshServersCommand = new RelayCommandAsync(OnRefreshServers);
            StartCommand = new RelayCommandAsync(OnStart, OnCanStart);

            // полностью загружен только после Loaded события, 
            // поэтому инициализируем это только сейчас
            _passwordService = IoCContainer.Instanse.Resolve<IPasswordService>();
            _popupMessageService = IoCContainer.Instanse.Resolve<IShowMessage>();

            // читаем настройки
            SettingsViewModel = new SettingsViewModel();

            RefreshViewBySettings();
        }

        #region Fields

        private ServerViewModel _currentServer;
        private IPasswordService _passwordService;
        private IShowMessage _popupMessageService;
        private string _login;
        private bool _rememberPassword;

        #endregion

        #region Properties

        public string Login
        {
            get => _login;
            set => Set(ref _login, value);
        }

        public bool RememberPassword
        {
            get => _rememberPassword;
            set => Set(ref _rememberPassword, value);
        }

        public ObservableCollection<ServerViewModel> Servers { get; } = new ObservableCollection<ServerViewModel>();

        public SettingsViewModel SettingsViewModel { get; set; }

        #endregion

        #region Commands

        public ICommand RefreshServersCommand { get; set; }

        public ICommand StartCommand { get; set; }

        #endregion

        #region Command Handlers

        /// <summary>
        ///     Можем ли стартануть игру
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool OnCanStart(object obj)
        {
            var empty = _passwordService == null
                        || string.IsNullOrWhiteSpace(Login)
                        || _passwordService.IsNullOrEmpty;
            return !empty;
        }

        /// <summary>
        ///     Стартуем игру
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private async Task OnStart(object arg)
        {
            if (_passwordService == null || !await CheckInstall())
                return;

            // проверяем соединении аккаунт на Ely.by
            var checkAuth = new AuthorizationChecker(Login, _passwordService);
            var id = await checkAuth.GetID();
            var accessToken = await checkAuth.GetAccessToken(id);
            if (accessToken == null || id == Guid.Empty)
            {
                var message = id == Guid.Empty
                    ? "Такой пользователь не зарегистрирован"
                    : "Авторизация прошла неудачно";
                message += ",но вы всё равно можете играть в одиночке";

                // Выходим, как как отказались запускать
                if (!await _popupMessageService.ShowMessageAsync(message))
                    return;


                //var confirm = new ConfirmViewModel(message);
                //await _popupMessageService.ShowAsync(confirm);
                //if (!confirm.Result)
                //    return;

                accessToken = "null";
            }

            // запускаем игру
            using (var launcher = new LaunchWorker(RootFolder, SettingsViewModel.Java64Path,
                SettingsViewModel.Memory, Login, id.ToString(), accessToken, SettingsViewModel.UseJavaArgs,
                SettingsViewModel.DebugMode))
            {
                Application.Current?.MainWindow?.Hide();
                var errors = await launcher.Launch();
                Application.Current?.MainWindow?.Show();

                if (!string.IsNullOrEmpty(errors) && SettingsViewModel.DebugMode)
                    File.WriteAllText(Path.Combine(launcher.GetBaseFolder, Guid.NewGuid() + ".log"), errors);

                //var error = new ErrorViewModel(errors);
                await _popupMessageService.ShowMessageAsync(errors);
            }
        }

        /// <summary>
        ///     Получаем список серверов
        /// </summary>
        /// <returns></returns>
        private async Task OnRefreshServers()
        {
            await SettingsViewModel.UpdateServers();
        }

        #endregion

        #region Methods

        #region Private

        /// <summary>
        ///     Обновляем вид из модели настроек
        /// </summary>
        private void RefreshViewBySettings()
        {
            if (SettingsViewModel?.Model?.Settings?.LauncherSettings == null
                || SettingsViewModel.Model.Settings.AuthSettings == null)
                return;

            Servers.Clear();

            foreach (var server in SettingsViewModel.Model.Settings.LauncherSettings.Servers)
                Servers.Add(new ServerViewModel(server));

            CurrentServer = Servers.FirstOrDefault();

            RememberPassword = SettingsViewModel.Model.Settings.AuthSettings.RememberPassword;
            Login = SettingsViewModel.Model.Settings.AuthSettings.Login;
            RememberPassword = SettingsViewModel.Model.Settings.AuthSettings.RememberPassword;
            if (RememberPassword)
                try
                {
                    var crypo = new CryptoWorker();
                    _passwordService.SetPassword(crypo.Decrypt<AesManaged>(
                        SettingsViewModel.Model.Settings.AuthSettings.EnctyptPassword,
                        SettingsViewModel.Model.Settings.AuthSettings.Key,
                        SettingsViewModel.Model.Settings.AuthSettings.Salt));
                }
                catch
                {
                    // ignored
                }
        }

        /// <summary>
        ///     Сетим настройки из вида
        /// </summary>
        private void RefreshSettingsByView()
        {
            SettingsViewModel.Model.Settings.AuthSettings.RememberPassword = RememberPassword;
            SettingsViewModel.Model.Settings.AuthSettings.Login = Login;
            if (RememberPassword)
            {
                var crypto = new CryptoWorker();
                var key = crypto.GetRandomSalt();
                var salt = crypto.GetRandomSalt();
                var password = crypto.Encrypt<AesManaged>(_passwordService.GetPassword(), key, salt);

                SettingsViewModel.Model.Settings.AuthSettings.Key = key;
                SettingsViewModel.Model.Settings.AuthSettings.Salt = salt;
                SettingsViewModel.Model.Settings.AuthSettings.EnctyptPassword = password;
            }
        }

        private async Task<bool> CheckInstall()
        {
            var md5Checker = new HashChecker();
            var current = await Task.Run(() => md5Checker.CreateMd5ForFolder(Path.Combine(RootFolder, "mods")));
            if (!current.Equals(CurrentServer.MD5))
                await _popupMessageService.ShowWorkerAsync("",
                    () => Directory.Delete(SettingsViewModel.GetBaseFolder, true));

            if (Directory.Exists(RootFolder))
                return true;

            var installer = new FolderDownloader(CurrentServer.DownloadLink, RootFolder, true);

            return await installer.Begin();
        }

        #endregion

        public void Save()
        {
            RefreshSettingsByView();

            SettingsViewModel.Save();
        }

        #endregion

        #region INameService

        public string RootFolder { get; private set; }

        public string SettingsPath => Path.Combine(RootFolder ?? string.Empty, "Settings.xml");

        public event EventHandler SwitchedFolder;

        public void ChangeFolder(string newPath)
        {
            try
            {
                var isValid = Path.GetFullPath(newPath);
                if (RootFolder == isValid)
                    return;

                RootFolder = isValid;
                SwitchedFolder?.Invoke(this, EventArgs.Empty);
            }
            catch
            {
                // ignored
            }
        }

        public string ProjectName => App.ProjectName;

        public ServerViewModel CurrentServer
        {
            get => _currentServer;
            set
            {
                if (Set(ref _currentServer, value)) ServerChanged?.Invoke(value, EventArgs.Empty);
            }
        }

        public event EventHandler ServerChanged;

        #endregion
    }
}