using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using Universal_Launcher.Models;
using Universal_Launcher.MVVM;

namespace Universal_Launcher.ViewModels
{
    public class DownloadingViewModel : ViewModelBase
    {
        /// <summary>
        ///     Вид-Модель для скачивания файла
        /// </summary>
        /// <param name="uri">Ссылка на скачивание</param>
        /// <param name="filename">Куда скачиваем</param>
        public DownloadingViewModel(string uri, string filename)
        {
            _uri = uri;
            _filename = filename;
            _client = new WebClient();
            _client.DownloadProgressChanged += ProgressChanged;
            _client.DownloadFileCompleted += OnComplited;
            _arbiter = new DownloadingArbiter();
            Cancel = new RelayCommand(() => _client.CancelAsync(), () => _client.IsBusy);
        }

        // Command
        public ICommand Cancel { get; set; }

        #region Fields

        private ManualResetEvent _reset;
        private readonly string _uri;
        private readonly string _filename;
        private int _speed;
        private int _progress;
        private readonly WebClient _client;
        private readonly DownloadingArbiter _arbiter;

        #endregion

        #region Properties

        public int Speed
        {
            get => _speed;
            set => Set(ref _speed, value);
        }

        public int Progress
        {
            get => _progress;
            set => Set(ref _progress, value);
        }

        public bool Success { get; set; }

        #endregion

        #region Methods

        private void OnComplited(object sender, AsyncCompletedEventArgs e)
        {
            Success = e.Error == null;
            _arbiter.Stop();
            _reset.Set();
        }

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Progress = e.ProgressPercentage;
            _arbiter.UpdateData(e.BytesReceived);
        }


        public void Start()
        {
            if (File.Exists(_filename))
                File.Delete(_filename);

            _client.DownloadFileAsync(new Uri(_uri), _filename);
            _arbiter.BeginObserve();
            _arbiter.OnSpeedChanged += (sender, i) => Speed = i;

            _reset = new ManualResetEvent(false);
            _reset.WaitOne();
        }

        #endregion
    }
}