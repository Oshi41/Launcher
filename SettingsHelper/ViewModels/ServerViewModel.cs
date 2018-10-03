using System;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using Launcher.Helper;
using Launcher.Interfaces;
using Launcher.Models;
using Mvvm;
using Mvvm.Commands;
using Newtonsoft.Json;
using SettingsHelper.Service;
using SettingsHelper.ViewModels.Base;

namespace SettingsHelper.ViewModels
{
    //public class ServerViewModel : ConfigBase<IServerInfo>, IServerInfo, IDialogWindowViewModel
    //{
    //    #region Fields
    //    private string _md5;
    //    private string _downloadLink;
    //    private string _address;
    //    private string _name;

    //    private DownloadManager _manager;
    //    private ErrorViewModel _linkStatus;
    //    private readonly ActionArbiter _actionArbiter = new ActionArbiter();
    //    private ErrorViewModel _addressStatus;

    //    #endregion

    //    #region Props

    //    public string Address
    //    {
    //        get => _address;
    //        set => SetProperty(ref _address, value);
    //    }

    //    public string Name
    //    {
    //        get => _name;
    //        set => SetProperty(ref _name, value);
    //    }

    //    /// <summary>
    //    /// Путь к архиву файла!!!!
    //    /// </summary>
    //    public string DownloadLink
    //    {
    //        get => _downloadLink;
    //        set => SetProperty(ref _downloadLink, value);
    //    }

    //    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    //    public string MD5
    //    {
    //        get => _md5;
    //        set => SetProperty(ref _md5, value);
    //    }

    //    public ErrorViewModel LinkStatus
    //    {
    //        get { return _linkStatus; }
    //        set { SetProperty(ref _linkStatus, value); }
    //    }

    //    public ErrorViewModel AddressStatus
    //    {
    //        get { return _addressStatus; }
    //        set { SetProperty(ref _addressStatus, value); }
    //    }

    //    #endregion

    //    #region Commands

    //    public ICommand CheckAddress { get; private set; }
    //    public ICommand CheckPing { get; private set; }
    //    public ICommand TryDownload { get; private set; }
    //    public ICommand CalculateMD5 { get; private set; }

    //    #endregion

    //    public ServerViewModel()
    //    {
    //        CheckAddress = new DelegateCommand(OnPingAddress, () => !_actionArbiter.IsBusy());

    //        CheckPing = new DelegateCommand(OnPing, () => !_actionArbiter.IsBusy());
    //        CalculateMD5 = new DelegateCommand(OnMD5);
    //        TryDownload = new DelegateCommand(OnTryDownload, () => _manager?.IsDownloading != true);
    //    }

    //    #region CommandHandlers

    //    private void OnPing()
    //    {
    //        async void Action()
    //        {
    //            LinkStatus = new ErrorViewModel();

    //            var ping = new Ping();
    //            try
    //            {
    //                var result = await ping.SendPingAsync(DownloadLink);
    //                var good = result.Status == IPStatus.Success;
    //                var text = good
    //                    ? null
    //                    : "Ошибка доступа ссылки - " + result.Status;

    //                LinkStatus = new ErrorViewModel(text, !good, good);
    //            }
    //            catch (Exception e)
    //            {
    //                Trace.WriteLine(e.Message);
    //                LinkStatus = ErrorViewModel.GetError(e.Message);
    //            }

    //        }

    //        _actionArbiter.Do(Action);
    //    }

    //    private void OnMD5()
    //    {
    //        var dlg = new FolderBrowserDialog();
    //        if (dlg.ShowDialog() != DialogResult.OK)
    //            return;

    //        var worker = new HashChecker();
    //        try
    //        {
    //            MD5 = worker.CreateMd5ForFolder(dlg.SelectedPath);
    //        }
    //        catch (Exception e)
    //        {
    //            Trace.WriteLine(e.Message);
    //            MD5 = string.Empty;
    //        }
    //    }

    //    private async void OnTryDownload()
    //    {
    //        LinkStatus = new ErrorViewModel();

    //        _manager = new DownloadManager(DownloadLink, interval: -1);
    //        await _manager.Download();
    //        _manager.DeleteFile();

    //        LinkStatus = !_manager.IsError
    //            ? ErrorViewModel.GetSuccess("Скачано успешно")
    //            : ErrorViewModel.GetError(_manager.LastError?.Message);
    //    }

    //    private async void OnPingAddress()
    //    {
    //        AddressStatus = new ErrorViewModel();

    //        var ping = new Ping();
    //        try
    //        {
    //            var result = await ping.SendPingAsync(Address);
    //            var good = result.Status == IPStatus.Success;
    //            var text = good
    //                ? null
    //                : "Ошибка доступа ссылки - " + result.Status;

    //            AddressStatus = new ErrorViewModel(text, !good, good);
    //        }
    //        catch (Exception e)
    //        {
    //            Trace.WriteLine(e.Message);
    //            LinkStatus = ErrorViewModel.GetError(e.Message);
    //        }
    //    }

    //    #endregion

    //    public ServerInfo ToServerInfo()
    //    {
    //        var json = JsonHelper.Serialize<IServerInfo>(this);
    //        return JsonConvert.DeserializeObject<ServerInfo>(json);
    //    }

    //    public ICommand CloseCommand { get; }
    //    public ICommand OkCommand { get; }
    //}
}
