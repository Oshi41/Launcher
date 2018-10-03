using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Launcher.Config.Interfaces;
using Launcher.Helper;
using Launcher.Interfaces;
using Launcher.Launch;
using Launcher.Models;
using Mvvm;
using Mvvm.Commands;

namespace Launcher.View_Models
{
    class ProjectViewModel : BindableBase
    {
        private ServerViewModel _choosenServer;
        private Process _process;

        #region Props

        public ObservableCollection<ServerViewModel> Servers { get; set; } = new ObservableCollection<ServerViewModel>();

        public ServerViewModel ChoosenServer
        {
            get => _choosenServer;
            set => SetProperty(ref _choosenServer, value);
        }

        #endregion

        #region Commands

        public ICommand LaunchCommand { get; }
        public ICommand RefreshCommand { get; }

        #endregion

        public ProjectViewModel()
        {
            RefreshCommand = new DelegateCommand(Refresh);
            LaunchCommand = new DelegateCommand(Launch, CanLaunch);

            Refresh();
        }

        #region Commang Handlers

        private void Refresh()
        {
            var conf = Singleton.Instance.Config.GetConfig<ProjectSettings>();
            Servers.Clear();

            foreach (var server in conf.Servers)
            {
                Servers.Add(new ServerViewModel(server));
            }
        }

        private bool CanLaunch()
        {
            return ChoosenServer?.CanConnect == true;
        }

        private async void Launch()
        {
            if (!CanLaunch())
                return;

            var conf = Singleton.Instance.Config.GetConfig<LauncherSettings>();
            var folder = Path.Combine(conf.BaseFolderPath, ChoosenServer.Name);

            if (!await CheckAndDownload(folder))
            {
                return;
            }

            if (!CheckMD5(folder))
            {
                // notify about redownload
                if (Directory.Exists(folder))
                    Directory.Delete(folder, true);

                return;
            }

            var worker = new ForgeLaunchWorker(folder);
            var args = worker.GetLauncArgs();

            var startInfo = new ProcessStartInfo
            {
                FileName = Singleton.Instance.Config.GetConfig<SystemSettings>().JavaPath,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                WorkingDirectory = folder,
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = args,
                UseShellExecute = false
            };

            _process = new Process { StartInfo = startInfo };
            _process.Exited += OnExit;

            _process.Start();

            App.Current.MainWindow.Hide();
        }

        private async Task<bool> CheckAndDownload(string folder)
        {
            if (Directory.Exists(folder))
                return true;

            var vm = new DownloadViewModel(ChoosenServer.DownloadLink, shouldAutoClose: false);

            var filePath = await vm.Start();
            if (!vm.IsError)
            {
                var archive = new CompressionManager(filePath, folder);
                if (await archive.Extract(true, true))
                {
                    vm.CloseCommand.Execute(null);
                    return true;
                }
            }

            return false;
        }

        private bool CheckMD5(string folder)
        {
            if (ChoosenServer.MD5.IsNullOrEmpty())
                return true;

            var worker = new HashChecker();

            foreach (var pair in ChoosenServer.MD5)
            {
                var checkingFolder = Path.Combine(folder, pair.Key);
                var md5 = worker.CreateMd5ForFolder(checkingFolder);

                if (!string.Equals(md5, pair.Value))
                    return false;
            }

            return true;
        }

        #endregion

        #region Methods

        private void OnExit(object sender, EventArgs e)
        {
            App.Current.MainWindow.Show();
        }

        #endregion

    }

    class ServerViewModel : BindableBase, IServerInfo
    {
        private string _address;
        private string _name;
        private string _downloadLink;
        private Dictionary<string, string> _md5;
        private ServerStat _serverStatistics;

        private bool _isWorking;

        #region Props

        public string Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string DownloadLink
        {
            get => _downloadLink;
            set => SetProperty(ref _downloadLink, value);
        }

        public Dictionary<string, string> MD5
        {
            get => _md5;
            set => SetProperty(ref _md5, value);
        }

        public ServerStat ServerStatistics
        {
            get => _serverStatistics;
            set => SetProperty(ref _serverStatistics, value);
        }

        public bool CanConnect => ServerStatistics?.ServerUp == true;

        #endregion

        #region Commands

        public ICommand RefreshCommand { get; set; }

        #endregion

        public ServerViewModel(IServerInfo info)
        {
            if (info == null)
                return;

            Name = info.Name;
            DownloadLink = DownloadLink;
            MD5 = info.MD5;
            Address = info.Address;

            RefreshCommand = new DelegateCommand(Refresh, () => _isWorking);
            RefreshCommand.Execute(null);
        }

        #region Methods

        private async void Refresh()
        {
            if (_isWorking)
                return;

            _isWorking = true;

            var stat = new ServerStat(Address);
            await stat.GetInfoAsync();

            ServerStatistics = stat;

            _isWorking = false;
        }

        #endregion
    }
}
