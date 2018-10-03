using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Input;
using Launcher.Config.Interfaces;
using Launcher.Models;
using Mvvm;
using Mvvm.Commands;

namespace Launcher.View_Models
{
    class SettingsViewModel : BindableBase
    {
        private int _memory;
        private int _maxMemory;
        private bool _useExpJavaArgs;
        private string _site;
        private bool _needToUpdate = true;

        #region Props

        public Version Version { get; set; }

        public int Memory
        {
            get => _memory;
            set => SetProperty(ref _memory, value);
        }

        public int MaxMemory
        {
            get => _maxMemory;
            set => SetProperty(ref _maxMemory, value);
        }

        public int MinMemory => 512;

        public bool UseExpJavaArgs
        {
            get => _useExpJavaArgs;
            set => SetProperty(ref _useExpJavaArgs, value);
        }

        public string Site
        {
            get => _site;
            set => SetProperty(ref _site, value);
        }

        public bool NeedToUpdate
        {
            get => _needToUpdate;
            set => SetProperty(ref _needToUpdate, value);
        }

        #endregion

        #region Commands

        public ICommand OpenBaseFolderCommand { get; set; }
        public ICommand ClearFolderCommand { get; set; }
        public ICommand OpenSiteCommand { get; set; }
        public ICommand UpdateLauncherCommand { get; set; }
        public ICommand SaveCommand { get; set; }

        #endregion

        public SettingsViewModel()
        {
            Singleton.Instance.Config.ConfigChanged += (sender, args) => RefreshBySettings();

            RefreshBySettings();

            OpenSiteCommand = new DelegateCommand(OnOpenBaseFolder);
            OpenSiteCommand = new DelegateCommand(OnOpenSite, CanOpenSite);
            UpdateLauncherCommand = new DelegateCommand(OnUpdate, () => NeedToUpdate);
            ClearFolderCommand = new DelegateCommand(OnClearFolder);
            SaveCommand = new DelegateCommand(OnSaveCommand);
        }

        #region Command Handlres

        private void OnOpenBaseFolder()
        {
            var folder = Singleton.Instance.Config.GetConfig<LauncherSettings>().BaseFolderPath;
            Process.Start("explorer", folder);
        }

        private bool CanOpenSite()
        {
            return !string.IsNullOrWhiteSpace(Site);
        }

        private void OnOpenSite()
        {
            if (!CanOpenSite())
                return;

            Process.Start(Site);
        }

        private async void OnUpdate()
        {
            var conf = Singleton.Instance.Config.GetConfig<ProjectSettings>();
            var vm = new DownloadViewModel(conf.ExeLink, shouldAutoClose: true);

            var file = await vm.Start();
            var currentExeName = AppDomain.CurrentDomain.FriendlyName;
            var currentThreadName = Thread.CurrentThread.Name;

            var lConf = Singleton.Instance.Config.GetConfig<LauncherSettings>();
            var updatorExe = Path.Combine(lConf.BaseFolderPath, "update.exe");

            if (File.Exists(updatorExe))
                return;

            Process.Start(updatorExe, $"{file} {currentExeName} {currentThreadName}");
            Process.GetCurrentProcess().Kill();
        }

        private void OnClearFolder()
        {
            // show question

            var conf = Singleton.Instance.Config.GetConfig<LauncherSettings>();

            if (Directory.Exists(conf.BaseFolderPath))
                Directory.Delete(conf.BaseFolderPath, true);
        }

        private void OnSaveCommand()
        {
            var conf = Singleton.Instance.Config.GetConfig<SystemSettings>();

            conf.Memory = Memory;
            conf.UseSpecialJavaArgs = UseExpJavaArgs;

            Singleton.Instance.Config.Replace(conf);
            Singleton.Instance.Config.Save();
            RefreshBySettings();
        }

        #endregion

        public void RefreshBySettings()
        {
            var conf = Singleton.Instance.Config.GetConfig<SystemSettings>();

            _useExpJavaArgs = conf.UseSpecialJavaArgs;
            MaxMemory = conf.MaxMemory;

            Version = Assembly.GetExecutingAssembly().GetName().Version;

            var projectConf = Singleton.Instance.Config.GetConfig<LauncherSettings>();
            Site = projectConf.RegistrationPage;

            var downloadConf = Singleton.Instance.Config.GetConfig<ProjectSettings>();
            NeedToUpdate = Version < downloadConf.Version;
        }
    }
}
