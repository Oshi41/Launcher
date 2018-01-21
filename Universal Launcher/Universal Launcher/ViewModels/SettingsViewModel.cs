using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using Universal_Launcher.Installers;
using Universal_Launcher.Models;
using Universal_Launcher.MVVM;
using Universal_Launcher.Properties;
using Universal_Launcher.Singleton;

namespace Universal_Launcher.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        public SettingsViewModel()
        {
            _show = IoCContainer.Instanse.Resolve<IShowMessage>();
            _nameService = IoCContainer.Instanse.Resolve<INameService>();

            OpenFolder = new RelayCommand(OnOpenFolder);
            DeleteFolder = new RelayCommandAsync(OnDeleteFolder, CanDeleteFolder);
            OpenSite = new RelayCommand(() => Process.Start(Resources.ElyLoginLink));
            UpdateLauncher = new RelayCommandAsync(OnUpdateLauncher);

            _localPath =
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    _nameService.ProjectName);

            Model = new SettingsModel();

            Version = Assembly.GetExecutingAssembly().GetName().Version;

            Init();
        }

        #region Fields

        private int _memory;
        private bool _useJavaArgs;
        private bool _debugMode;
        private readonly INameService _nameService;
        private readonly IShowMessage _show;

        // путь к локальным файлам, куда скачиваем временные файлы
        private readonly string _localPath;
        private bool _needToUpdate;

        #endregion

        #region Command handlers

        private async Task OnUpdateLauncher()
        {
            var updHelper = new LauncherInstaller();

            await updHelper.Begin();
        }

        /// <summary>
        ///     Удаляем папку
        /// </summary>
        /// <returns></returns>
        private async Task OnDeleteFolder()
        {
            if (!await _show.ShowMessageAsync("Подтвердить вменяемость"))
                return;
            //var confirm = new ConfirmViewModel("Подтвердить вменяемость");
            //await _show.ShowAsync(confirm);
            //if (!confirm.Result)
            //    return;

            try
            {
                await _show.ShowWorkerAsync("Очищаем папки", () =>
                {
                    Directory.Delete(GetBaseFolder, true);
                    Directory.Delete(_localPath, true);
                });

                //using (new ShowViewModel("Очищаем папки"))
                //{
                //    Directory.Delete(GetBaseFolder, true);
                //    Directory.Delete(_localPath, true);
                //}
            }
            catch (Exception e)
            {
                await _show.ShowMessageAsync(e.Message);
            }
        }

        /// <summary>
        ///     Если есть папка с временными файлами или с файлами выбранного сервера
        /// </summary>
        /// <returns></returns>
        private bool CanDeleteFolder()
        {
            var exist = Directory.Exists(GetBaseFolder) || Directory.Exists(_localPath);
            return exist;
        }

        /// <summary>
        ///     Открываем папку с игрой
        /// </summary>
        private void OnOpenFolder()
        {
            Process.Start("explorer", GetBaseFolder);
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Возвращает путь к файлам выбранного сервера
        /// </summary>
        public string GetBaseFolder
        {
            get
            {
                if (_nameService == null)
                    return string.Empty;

                return Path.Combine(_nameService.RootFolder,
                    _nameService.CurrentServer?.Name ?? string.Empty);
            }
        }

        /// <summary>
        ///     Модель чтения/записи настроек
        /// </summary>
        public SettingsModel Model { get; }

        /// <summary>
        ///     История изменений программы
        /// </summary>
        public List<string> History { get; } = new List<string>();

        /// <summary>
        ///     Версия программы
        /// </summary>
        public Version Version { get; }

        /// <summary>
        ///     Кол-во используемой памяти при старте
        /// </summary>
        public int Memory
        {   // ограничиваю мин. память на 1024
            get => Math.Max(_memory, 1024);
            set => Set(ref _memory, value);
        }

        /// <summary>
        ///     ИСпользуем ли эксп. аргументы
        /// </summary>
        public bool UseJavaArgs
        {
            get => _useJavaArgs;
            set => Set(ref _useJavaArgs, value);
        }

        /// <summary>
        ///     Находимся ли в режиме отладки
        /// </summary>
        public bool DebugMode
        {
            get => _debugMode;
            set => Set(ref _debugMode, value);
        }

        /// <summary>
        ///     Путь к java.exe 64 bit
        /// </summary>
        public string Java64Path { get; private set; }

        /// <summary>
        ///     Доступно ли обновления для лаунчера
        /// </summary>
        public bool NeedToUpdate
        {
            get => _needToUpdate;
            set => Set(ref _needToUpdate, value);
        }

        #endregion

        #region Commands

        public ICommand UpdateLauncher { get; set; }

        public ICommand OpenSite { get; set; }

        public ICommand DeleteFolder { get; set; }

        public ICommand OpenFolder { get; set; }

        #endregion

        #region Methods

        /// <summary>
        ///     Обновляем данные на виде ИЗ модели
        /// </summary>
        private void UpdateViewByModel()
        {
            History.Clear();
            History.AddRange(Model.Settings.LauncherSettings.History);

            Memory = Model.Settings.SystemSettings.Memory;
            UseJavaArgs = Model.Settings.SystemSettings.UseJavaArgs;
            DebugMode = Model.Settings.SystemSettings.DebugMode;
        }

        /// <summary>
        ///     Обновляем модель из данных НА виде
        /// </summary>
        private void UpdateModelByView()
        {
            // не обновляем историю, потому что она меняться не может

            Model.Settings.SystemSettings.Memory = Memory;
            Model.Settings.SystemSettings.UseJavaArgs = UseJavaArgs;
            Model.Settings.SystemSettings.DebugMode = DebugMode;
        }

        /// <summary>
        ///     Ищем путь к 64 bit Java
        /// </summary>
        private void FindJava64Path()
        {
            var base64 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                "Java").Replace(" (x86)", "");

            if (Directory.Exists(base64))
                foreach (var dir in Directory.GetDirectories(base64))
                {
                    var result = Path.Combine(dir, "bin", "java.exe");
                    if (File.Exists(result))
                        Java64Path = result;
                }
        }

        /// <summary>
        ///     Обновляем сервера по команде
        /// </summary>
        /// <returns></returns>
        public async Task UpdateServers()
        {
            await Task.Run(() => Model.DownloadInfo());
            UpdateViewByModel();
        }

        /// <summary>
        ///     Сохраняю настройки
        /// </summary>
        public void Save()
        {
            UpdateModelByView();

            Model.WriteSettings();
        }

        /// <summary>
        ///     Инициализирую модель
        ///     <para>Оставил из-за особенностей работы. Раньше инициализировал до события Loaded</para>
        /// </summary>
        private void Init()
        {
            if (!Directory.Exists(GetBaseFolder))
                Directory.CreateDirectory(GetBaseFolder);

            FindJava64Path();

            Model.ReadOrCreate();

            UpdateViewByModel();

            Version ver;
            if (Version.TryParse(Model.Settings.LauncherSettings.Version, out ver))
                NeedToUpdate = ver > Version;
        }

        #endregion
    }
}