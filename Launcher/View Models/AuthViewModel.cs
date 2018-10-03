using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Input;
using Launcher.AuthModules;
using Launcher.Config.Interfaces;
using Launcher.Interfaces;
using Launcher.Models;
using Mvvm;
using Mvvm.Commands;

namespace Launcher.View_Models
{
    class AuthViewModel : BindableBase
    {
        #region Fields

        private readonly AuthModel _model = new AuthModel();

        private AuthModulBase _module;
        private string _login;
        private bool _remember;
        private bool _useModuleAuth;
        private bool _passAuth;
        private Modules _moduleType;
        private bool _useAuthModule;

        #endregion

        #region Props

        public string Login
        {
            get => _login;
            set => SetProperty(ref _login, value);
        }

        public bool Remember
        {
            get => _remember;
            set => SetProperty(ref _remember, value);
        }

        public bool PassAuth
        {
            get => _passAuth;
            private set => SetProperty(ref _passAuth, value);
        }

        public bool UseAuthModule
        {
            get => _useAuthModule;
            set => SetProperty(ref _useAuthModule, value);
        }

        public Modules ModuleType
        {
            get => _moduleType;
            set => SetProperty(ref _moduleType, value);
        }

        #endregion

        public ICommand StartCommand { get; private set; }

        public AuthViewModel()
        {
            Refresh();

            StartCommand = new DelegateCommand(OnStart, CanStart);
        }

        #region Methods

        public void Refresh()
        {
            var conf = Singleton.Instance.Config.GetConfig<AuthentificationSettings>();

            Login = conf.Name;
            Singleton.Instance.PasswordService?.SetPassword(_model.DecryptPass(conf.Password));
            Remember = conf.NeedToSave;
            UseAuthModule = conf.Module.HasValue;

            if (conf.Module.HasValue)
                ModuleType = conf.Module.Value;
        }

        public void Save()
        {
            var conf = Singleton.Instance.Config.GetConfig<AuthentificationSettings>();

            conf.Name = Login;
            conf.Password = _model.EncryptPass(Singleton.Instance.PasswordService);
            conf.NeedToSave = Remember;

            if (UseAuthModule)
            {
                conf.Module = ModuleType;
            }
            else
            {
                conf.Module = null;
            }

            Singleton.Instance.Config.Replace(conf);
        }

        private async Task<bool> TryAuth()
        {
            InitModule();

            // Если модуль не инициализирован, считаем, что аутентификации нет
            if (_model == null)
                return true;

            var result = await _module.TryAuthentificate();
            return result.IsSuccessfull();
        }

        /// <summary>
        /// Так как пока что у меня поддержка только одного модуля, оставляю метод на будущее
        /// </summary>
        private void InitModule()
        {
            if (!UseAuthModule)
            {
                _module = null;
                return;
            }

            switch (ModuleType)
            {
                case Modules.Ely:
                    _module = new ElyAuthModule(Login);
                    break;

                default:
                    _module = null;
                    break;
            }
        }

        #endregion

        #region Command Handlers

        private bool CanStart()
        {
            return !string.IsNullOrWhiteSpace(Login);
        }

        private async void OnStart()
        {
            if (!await TryAuth())
            {
                // todo show auth error
                PassAuth = false;
                return;
            }

            PassAuth = true;
            Save();
        }

        #endregion
    }

    class AuthModel
    {
        public string EncryptPass(IGetPassword pass)
        {
            if (pass == null)
                return String.Empty;

            var worker = new CryptoWorker();
            var id = worker.GetUniqueSalt();
            return worker.Encrypt<RijndaelManaged>(pass.GetPassword(), id, id);

        }

        public string DecryptPass(string pass)
        {
            if (string.IsNullOrEmpty(pass))
                return String.Empty;

            var worker = new CryptoWorker();
            var id = worker.GetUniqueSalt();
            return worker.Decrypt<RijndaelManaged>(pass, id, id);
        }
    }
}
