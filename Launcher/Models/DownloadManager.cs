using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Launcher.Models
{
    /// <summary>
    ///     Класс наблюдающий за скоростью скачивания.
    ///     <para>
    ///         Использование: при скачивании файла клиентом подписываемся на DownloadProgresChanged.
    ///         Передаём туда кол-во загруженных байт. Арбитр рассчитывает кол-во полученных бит за
    ///         заданное время
    ///     </para>
    /// </summary>
    public class DownloadManager
    {
        #region Fields
        private readonly WebClient _client;
        private readonly string _uri;
        private readonly string _file;
        private readonly int _interval;

        private long _previouse;
        private long _current;
        private DateTime _prevData;
        private int _percantage;

        //private Timer _timer;

        #endregion

        /// <summary>
        /// </summary>
        /// <param name="file">Куда скачиваем</param>
        /// <param name="uri">Что скачиваем</param>
        /// <param name="interval">Интервал замера скорости скачивания</param>
        public DownloadManager(string uri, string file = null, int interval = 40)
        {
            _client = new WebClient();

            _uri = uri;
            _file = file ?? Path.GetTempFileName();
            _interval = interval;

            // если интервал задан меньше 0, отключаем проверку скорости
            if (_interval > 0)
                _client.DownloadProgressChanged += OnProgressChanged;

            _client.DownloadFileCompleted += OnDownloadComplited;

            //_timer = new Timer(interval);
            //_timer.Elapsed += (sender, args) =>
            //{
            //    var bytes = _current - _previouse;
            //    _previouse = _current;

            //    var now = DateTime.Now;
            //    var mls = (now - _prevData).TotalMilliseconds;
            //    _prevData = now;

            //    var multiply = 1024.0 / 1000;

            //    var kbInSec = bytes / mls * multiply;

            //    Speed = (int) Math.Floor(kbInSec);

            //    ProgressChanged?.Invoke(this, EventArgs.Empty);
            //};
        }
        
        #region Properties

        /// <summary>
        /// Скорость скачивания
        /// </summary>
        public int Speed { get; private set; }

        /// <summary>
        /// Процентаж выполнения
        /// </summary>
        public int Percantage
        {
            get => _percantage;
            private set
            {
                if (_percantage != value) return;

                _percantage = value;
                ProgressChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool IsDownloading => _client.IsBusy;

        public bool IsError { get; set; }

        public Exception LastError { get; private set; }

        #endregion

        #region Events

        public event EventHandler<string> DownloadComplited;
        public event EventHandler DownloadStarted;
        public event EventHandler ProgressChanged;

        #endregion

        #region Puclic methods

        /// <summary>
        /// Скачивает файл и возвращает его путь
        /// </summary>
        /// <returns></returns>
        public async Task<string> Download()
        {
            _prevData = DateTime.Now;

            try
            {
                var task = _client.DownloadFileTaskAsync(_uri, _file);
                DownloadStarted?.Invoke(this, EventArgs.Empty);
                //_timer.Start();

                await task;
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
            }

            return _file;
        }

        public void Cancel()
        {
            if (IsDownloading)
                _client.CancelAsync();

        }

        public bool DeleteFile()
        {
            try
            {
                if (!File.Exists(_file))
                    throw new FileNotFoundException($"Can't find file - {_file}");

                File.Delete(_file);
                return true;
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                LastError = e;
                return false;
            }
        }

        #endregion

        #region Methods

        private void OnProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            _current = e.BytesReceived;
            Percantage = e.ProgressPercentage;

            var now = DateTime.Now;
            var mls = (now - _prevData).TotalMilliseconds;

            if (mls < _interval)
                return;

            _prevData = now;

            var bytes = _current - _previouse;
            _previouse = _current;

            var kbInSec = bytes / mls * 1024.0 / 1000;

            Speed = (int) Math.Floor(kbInSec);
            ProgressChanged?.Invoke(this, EventArgs.Empty);
        }

        private void OnDownloadComplited(object sender, AsyncCompletedEventArgs e)
        {
            DownloadComplited?.Invoke(sender, _file);
            IsError = e.Cancelled || e.Error != null;
            LastError = e.Error;
            //_timer.Stop();
        }

        #endregion
    }
}
