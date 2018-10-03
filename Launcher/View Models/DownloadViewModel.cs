using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Launcher.Models;
using MaterialDesignThemes.Wpf;
using Mvvm;
using Mvvm.Commands;

namespace Launcher.View_Models
{
    /// <summary>
    /// Ориентирована на показ в DialogHost
    /// </summary>
    class DownloadViewModel : BindableBase
    {
        private readonly bool _shouldAutoClose;
        private readonly DownloadManager _manager;
        private int _speed;
        private int _percantage;

        #region Properies

        public string Uri { get; }
        public string File { get; private set; }

        public int Speed
        {
            get => _speed;
            set => SetProperty(ref _speed, value);
        }

        public int Percantage
        {
            get => _percantage;
            set => SetProperty(ref _percantage, value);
        }

        public bool IsError
        {
            get => _isError;
            set => SetProperty(ref _isError, value);
        }

        #endregion

        #region Command

        public ICommand CancelDownloadCommand;
        private bool _isError;
        public ICommand CloseCommand => DialogHost.CloseDialogCommand;

        #endregion

        public DownloadViewModel(string uri, string file = null, bool shouldAutoClose = true)
        {
            _shouldAutoClose = shouldAutoClose;
            Uri = uri;
            if (file != null)
                File = file;

            _manager = new DownloadManager(uri, File);

            _manager.ProgressChanged += OnProgressChanged;
            _manager.DownloadComplited += OnComplited;

            CancelDownloadCommand = new DelegateCommand(() => _manager.Cancel(), () => _manager.IsDownloading);
        }

        public async Task<string> Start()
        {
            var dialogTask = DialogHost.Show(this);
            var downloadTask = _manager.Download();

            await Task.WhenAll(downloadTask, downloadTask);

            return downloadTask.Result;
        }

        #region Private methods

        private void OnProgressChanged(object sender, EventArgs e)
        {
            Speed = _manager.Speed;
            Percantage = _manager.Percantage;
        }

        private void OnComplited(object sender, string e)
        {
            if (_shouldAutoClose)
                CloseCommand.Execute(this);

            IsError = _manager.IsError;
        }

        #endregion

    }
}
