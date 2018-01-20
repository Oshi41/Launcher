using System;
using Universal_Launcher.Models;
using Universal_Launcher.MVVM;
using Universal_Launcher.Settings;

namespace Universal_Launcher.ViewModels
{
    public class ServerViewModel : ViewModelBase
    {
        /// <summary>
        ///     Создаем вид-модель на основе модели. Основная инф-а берётся из неё,
        ///     к примеру ссылка на саму игру
        /// </summary>
        /// <param name="server">Модель сервера</param>
        public ServerViewModel(Server server)
        {
            if (server == null)
                return;

            Name = server.Name;
            Address = server.Address;
            DownloadLink = server.DownloadLink;
            MD5 = server.MD5;

            CheckStat(server.Address);
        }

        /// <summary>
        ///     Пингуем сервер
        /// </summary>
        /// <param name="link"></param>
        public async void CheckStat(string link)
        {
            if (string.IsNullOrWhiteSpace(link))
                return;

            try
            {
                var info = new MineStat(link);
                await info.GetInfoAsync();
                IsOn = info.ServerUp;

                Ping = (int) info.Delay;
                Online = Convert.ToInt32(info.CurrentPlayers);
                Max = Convert.ToInt32(info.MaximumPlayers);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        #region Fields

        private string _address;
        private bool _isOn;
        private int _max;
        private string _name;
        private int _online;
        private int _ping;

        #endregion

        #region Properties

        /// <summary>
        ///     Имя сервера
        /// </summary>
        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        /// <summary>
        ///     Адрес сервера
        /// </summary>
        public string Address
        {
            get => _address;
            set => Set(ref _address, value);
        }

        /// <summary>
        ///     Кол-во онлайн
        /// </summary>
        public int Online
        {
            get => _online;
            set => Set(ref _online, value);
        }

        /// <summary>
        ///     Кол-во слотов
        /// </summary>
        public int Max
        {
            get => _max;
            set => Set(ref _max, value);
        }

        /// <summary>
        ///     Время задержки сервера в млск
        /// </summary>
        public int Ping
        {
            get => _ping;
            set => Set(ref _ping, value);
        }

        /// <summary>
        ///     Включен ли сервер
        /// </summary>
        public bool IsOn
        {
            get => _isOn;
            set => Set(ref _isOn, value);
        }

        /// <summary>
        ///     Ссылка для скачивания игры на этом сервере
        /// </summary>
        public string DownloadLink { get; set; }

        /// <summary>
        ///     Хэш сумма для проверки
        /// </summary>
        public string MD5 { get; set; }

        #endregion
    }
}