using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using Launcher.Interfaces;
using Launcher.Models;
using Mvvm;
using Mvvm.Commands;
using Newtonsoft.Json;
using SettingsHelper.ViewModels.Base;
using Ping = System.Net.NetworkInformation.Ping;

namespace SettingsHelper.ViewModels.New
{
    class ServerViewModelNew : SettingsViewModelBase<IServerInfo>
    {
        #region Fields
        private InfoBoxViewModel _address;
        private InfoBoxViewModel _name;
        private InfoBoxViewModel _downloadLink;
        private Dictionary<string, string> _md5;

        private readonly ActionArbiter _adressArbiter = new ActionArbiter();
        private readonly ActionArbiter _pingDownloadLinkArbiter = new ActionArbiter();
        private ObservableCollection<Md5Row> _hashedFolders = new ObservableCollection<Md5Row>();
        private Md5Row _selectedRow;

        #endregion

        #region Properties

        public InfoBoxViewModel Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }

        public InfoBoxViewModel Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public InfoBoxViewModel DownloadLink
        {
            get => _downloadLink;
            set => SetProperty(ref _downloadLink, value);
        }

        public ObservableCollection<Md5Row> HashedFolders
        {
            get => _hashedFolders;
            set => SetProperty(ref _hashedFolders, value);
        }

        public Md5Row SelectedRow
        {
            get => _selectedRow;
            set => SetProperty(ref _selectedRow, value);
        }

        #endregion

        #region Commands

        public ICommand CheckAddress { get; private set; }
        public ICommand CalculateMD5 { get; private set; }
        public ICommand TryToDownload { get; private set; }

        public ICommand AddRow { get; private set; }
        public ICommand DeleteRow { get; private set; }

        #endregion

        #region Command Handlers

        private void OnMD5()
        {
            var dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            var worker = new HashChecker();
            try
            {
                var text = worker.CreateMd5ForFolder(dlg.SelectedPath);
                var relativeFolder = FindRelativeFolder(dlg.SelectedPath);
                HashedFolders.Add(new Md5Row(relativeFolder, text));
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
            }
        }

        private async void OnTryDownload(bool? onlyPing)
        {
            if (onlyPing == true)
            {
                ManageStatus(_pingDownloadLinkArbiter, DownloadLink);
                return;
            }

            _manager = new DownloadManager(DownloadLink.Text, interval: -1);
            await _manager.Download();
            _manager.DeleteFile();

            if (_manager.IsError)
            {
                DownloadLink.SetError(_manager.LastError.Message);
            }
            else
            {
                DownloadLink.SetSuccess("Скачано успешно");
            }
        }

        private void OnAddRow()
        {
            var dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            string folder = dlg.SelectedPath;
            string hash = string.Empty;

            try
            {
                var worker = new HashChecker();
                var full = worker.CreateMd5ForFolder(dlg.SelectedPath);

                folder = FindRelativeFolder(full);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
            }

            HashedFolders.Add(new Md5Row(folder, hash));
        }
        #endregion

        public ServerViewModelNew()
        {
            Address = new InfoBoxViewModel();
            Name = new InfoBoxViewModel();
            DownloadLink = new InfoBoxViewModel();
            HashedFolders = new ObservableCollection<Md5Row>();

            CalculateMD5 = new DelegateCommand(OnMD5);
            CheckAddress = new DelegateCommand(() => ManageStatus(_adressArbiter, Address), () => !_adressArbiter.IsBusy());
            TryToDownload = new DelegateCommand<bool?>(OnTryDownload, b => b == true || !IsDownloading());
            AddRow = new DelegateCommand(OnAddRow);
            DeleteRow = new DelegateCommand(() => HashedFolders.Remove(SelectedRow), () => SelectedRow  != null);
        }

        #region Methods

        private static async void ManageStatus(ActionArbiter arbiter,
            InfoBoxViewModel vm)
        {
            if (arbiter == null || string.IsNullOrWhiteSpace(vm?.Text))
                return;

            try
            {
                var result = await TryPing(vm.Text, arbiter);
                if (result.HasValue && result.Value == IPStatus.Success)
                {
                    vm.SetSuccess();
                }
                else
                {
                    vm.SetError("Ошибка доступа ссылки - " + result);
                }

            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                vm.SetError(e.Message);
            }
        }

        /// <summary>
        /// <exception cref="TimeoutException"></exception>
        /// </summary>
        /// <param name="address"></param>
        /// <param name="arbiter"></param>
        /// <returns></returns>
        private static async Task<IPStatus?> TryPing(string address, ActionArbiter arbiter)
        {
            if (string.IsNullOrWhiteSpace(address) || arbiter == null)
                return null;

            IPStatus? result = null;

            async void Function()
            {
                var ping = new Ping();
                var pingErsult = await ping.SendPingAsync(address);
                result = pingErsult.Status;
            }

            await arbiter.Do(Function);
            return result;
        }

        private static string FindRelativeFolder(string folder)
        {
            // папка которая всегда лежит на вершине
            var rootFolder = "assets";

            try
            {
                var parent = new DirectoryInfo(folder);

                while (parent?.Exists == true)
                {
                    var folders = parent.GetDirectories(rootFolder);
                    if (folders.Any())
                    {
                        var baseUri = new Uri(parent.FullName);
                        var hasedFolder = new Uri(folder);
                        Uri relative = baseUri.MakeRelativeUri(hasedFolder);
                        return relative.OriginalString;
                    }
                }

                return folder;
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                return folder;
            }
        }

        #endregion

        #region Overriden

        protected override IServerInfo ToModel()
        {
            var info = new ServerInfo
            {
                Address = Address.Text,
                DownloadLink = DownloadLink.Text,
                MD5 = HashedFolders.ToDictionary(x => x.FolderName, x => x.Hash),
                Name = Name.Text,
            };

            return info;
        }

        protected override void Refresh(IServerInfo model)
        {
            if (model == null)
                return;

            Address.Clear();
            DownloadLink.Clear();
            Name.Clear();

            Address.Text = model.Address;
            DownloadLink.Text = model.DownloadLink;
            Name.Text = model.Name;

            HashedFolders = new ObservableCollection<Md5Row>(model.MD5.Select(x => new Md5Row(x)));
        }

        #endregion
    }

    class Md5Row : BindableBase
    {
        private string _folderName;
        private string _hash;
        private InfoBoxViewModel _boxViewModel;

        public string FolderName
        {
            get => _folderName;
            set => SetProperty(ref _folderName, value);
        }

        public string Hash
        {
            get => _hash;
            set => SetProperty(ref _hash, value);
        }

        public InfoBoxViewModel BoxViewModel
        {
            get => _boxViewModel;
            set => SetProperty(ref _boxViewModel, value);
        }

        public Md5Row(KeyValuePair<string, string> row)
            : this(row.Key, row.Value) { }

        public Md5Row(string folder, string hash)
        {
            _folderName = folder;
            _hash = hash;
            BoxViewModel = new InfoBoxViewModel();
        }
    }
}
