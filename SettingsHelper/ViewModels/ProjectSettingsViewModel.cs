using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Input;
using Launcher.Config.Interfaces;
using Launcher.Helper;
using Launcher.Interfaces;
using Launcher.Models;
using Mvvm.Commands;
using Newtonsoft.Json;
using SettingsHelper.Service;
using SettingsHelper.ViewModels.Base;

namespace SettingsHelper.ViewModels
{
    //class ProjectSettingsViewModel : ConfigBase<IProjectSettings>, IProjectSettings
    //{
    //    #region Fields
    //    private string _exeLink;
    //    private Version _version;

    //    private readonly ActionArbiter _actionArbiter = new ActionArbiter();
    //    private readonly WebClient _webClient = new WebClient();
    //    private ErrorViewModel _exeStatus;
    //    private ServerViewModel _server;
    //    private IList<ServerViewModel> _serverList;
    //    private IList<ServerViewModel> _servers;

    //    #endregion

    //    #region Props

    //    public ErrorViewModel ExeStatus
    //    {
    //        get => _exeStatus;
    //        set => SetProperty(ref _exeStatus, value);
    //    }

    //    public ServerViewModel Server
    //    {
    //        get => _server;
    //        set => SetProperty(ref _server, value);
    //    }

    //    public IList<ServerViewModel> Servers
    //    {
    //        get { return _servers; }
    //        set { SetProperty(ref _servers, value); }
    //    }

    //    #endregion

    //    #region IProjectSettings

    //    public string ExeLink
    //    {
    //        get => _exeLink;
    //        set => SetProperty(ref _exeLink, value);
    //    }

    //    List<ServerInfo> IProjectSettings.Servers => Servers?.Select(x => x.ToServerInfo()).ToList();

    //    public Version Version
    //    {
    //        get => _version;
    //        set => SetProperty(ref _version, value);
    //    }

    //    #endregion

    //    #region Commands

    //    public ICommand CheckAndDownloadExe { get; private set; }

    //    public ICommand AddServer { get; private set; }
    //    public ICommand EditServer { get; private set; }
    //    public ICommand RemoveServer { get; private set; }

    //    #endregion

    //    public ProjectSettingsViewModel()
    //    {
    //        CheckAndDownloadExe = new DelegateCommand(OnDownload, () => CanDownload(_manager));
    //        RemoveServer = new DelegateCommand(OnDelete, () => Server != null);
    //        AddServer = new DelegateCommand(() => ShowDialog());
    //        EditServer = new DelegateCommand(() => ShowDialog(Server), () => Server != null);
    //    }

    //    #region Command handlers

    //    private async void OnDownload()
    //    {
    //        if (!CheckAndDownloadExe.CanExecute(null))
    //            return;

    //        _manager = new DownloadManager(ExeLink, interval: -1);
    //        await _manager.Download();
    //        _manager.DeleteFile();

    //        ExeStatus = new ErrorViewModel(null, _manager.IsError, !_manager.IsError);

    //        if (_manager.IsError)
    //            ExeStatus.Text = _manager.LastError.Message;
    //    }

    //    private void OnDelete()
    //    {
    //        if (!RemoveServer.CanExecute(null))
    //            return;

    //        var copy = Servers.ToList();
    //        copy.Remove(Server);
    //        Servers = copy;
    //    }
        
    //    #endregion

    //    private void ShowDialog(ServerViewModel server = null)
    //    {
    //        //todo Make Title

    //        if (server == null)
    //            server = new ServerViewModel();

    //        if (WindowService.ShowDialog(server) != true)
    //            return;

    //        var copy = Servers.ToList();

    //        if (copy.Contains(server))
    //            copy.Remove(server);

    //        copy.Add(server);
    //        Servers = copy;
    //    }
    //}
}
