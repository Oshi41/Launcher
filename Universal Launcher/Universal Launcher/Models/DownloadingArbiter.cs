using System;
using System.Timers;

namespace Universal_Launcher.Models
{
    /// <summary>
    ///     Класс наблюдающий за скоростью скачивания.
    ///     <para>
    ///         Использование: при скачивании файла клиентом подписываемся на DownloadProgresChanged.
    ///         Передаём туда кол-во загруженных байт. Арбитр рассчитывает кол-во полученных бит за
    ///         заданное время
    ///     </para>
    /// </summary>
    public class DownloadingArbiter
    {
        /// <summary>
        /// </summary>
        /// <param name="mls">Период обновления наблюдений. Дефолт - 1 сек</param>
        public DownloadingArbiter(int mls = 1000)
        {
            _timer = new Timer
            {
                AutoReset = true,
                Interval = mls
            };
            _timer.Elapsed += Calculate;
        }

        /// <summary>
        ///     Скорость скачивания кб/сек
        /// </summary>
        public int Speed { get; set; }

        public event EventHandler<int> OnSpeedChanged;

        #region Fields

        private long _previouse;
        private long _current;
        private readonly Timer _timer;

        #endregion

        #region Methods

        /// <summary>
        ///     Обновляем значения за момент наблюдения
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Calculate(object sender, ElapsedEventArgs e)
        {
            var difference = _current - _previouse;
            Speed = (int) (difference / _timer.Interval);
            _previouse = _current;

            OnSpeedChanged?.Invoke(this, Speed);
        }

        /// <summary>
        ///     Начинаем наблюдения
        /// </summary>
        public void BeginObserve()
        {
            _previouse = 0;
            _current = 0;
            Speed = 0;
            _timer.Start();
        }

        /// <summary>
        ///     Обновляем кол-во полученных байт
        /// </summary>
        /// <param name="bytesRecieved">Общее кол-во скачанных байт файла</param>
        public void UpdateData(long bytesRecieved)
        {
            _current = bytesRecieved;
        }

        /// <summary>
        ///     Останавливаем наблюдения
        /// </summary>
        public void Stop()
        {
            _timer.Stop();
        }

        #endregion
    }
}