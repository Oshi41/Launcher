using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Launcher.Models
{
    /// <summary>
    ///     Статистика свойств работы сервера
    /// </summary>
    public class ServerStat
    {
        /// <summary>
        /// </summary>
        /// <param name="address">Путь к серверу</param>
        public ServerStat(string address)
        {
            // Указан порт
            if (address.Contains(":"))
            {
                Address = address.Substring(0, address.IndexOf(':'));
                ushort port;
                Port = ushort.TryParse(address.Remove(0, address.IndexOf(':') + 1), out port)
                    ? port
                    : StandartPort;
            }
            // в иных случаях, например, когла путь сервера - обычный web путь
            else
            {
                Address = address;
                Port = StandartPort;
            }
        }

        #region Fields

        private const ushort DataSize = 512; // this will hopefully suffice since the MotD should be <=59 characters
        private const ushort NumFields = 6; // number of values expected from server
        private const ushort StandartPort = 25565; // standart mincraft port

        #endregion

        #region Methods

        /// <summary>
        /// Посылает запрос к серверу для получения статистики сервера
        /// </summary>
        /// <returns></returns>
        public async Task GetInfoAsync()
        {
            await Task.Run(() => GetInfo());
        }

        /// <summary>
        /// 
        /// </summary>
        private void GetInfo()
        {
            var rawServerData = new byte[DataSize];

            try
            {
                var client = new TcpClient();
                Delay = CheckConnect(client);

                if (Delay < 0)
                    throw new TimeoutException();

                var stream = client.GetStream();
                var payload = new byte[] { 0xFE, 0x01 };
                stream.Write(payload, 0, payload.Length);
                stream.Read(rawServerData, 0, DataSize);
                client.Close();

                if (rawServerData.Length == 0)
                    throw new IndexOutOfRangeException();
            }
            catch
            {
                ServerUp = false;
                return;
            }

            var serverData = Encoding.Unicode.GetString(rawServerData).Split("\u0000\u0000\u0000".ToCharArray());
            if (serverData.Length >= NumFields)
            {
                ServerUp = true;
                Version = serverData[2];
                Motd = serverData[3];
                CurrentPlayers = serverData[4];
                MaximumPlayers = serverData[5];
            }
            else
            {
                ServerUp = false;
            }
        }

        /// <summary>
        ///     Проверяет наличие соединения (не больше 5 сек, далее считается недоступным)
        /// </summary>
        /// <param name="client">Клиент для отправки сигнала</param>
        /// <returns>Задержка сигнала в млск. Возвращает -1, если сервер недоступен в течении 5 секунд</returns>
        private long CheckConnect(TcpClient client)
        {
            var stopwatch = new Stopwatch();
            // засекаем время
            stopwatch.Start();

            var task = client.BeginConnect(Address, Port, null, null);
            // ожидаем резльтата не долше 5 секунд
            var success = task.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(5));

            stopwatch.Stop();
            client.EndConnect(task);

            return success
                ? stopwatch.ElapsedMilliseconds
                : -1;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Адрес сервера
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        ///     Порт сервера
        /// </summary>
        public ushort Port { get; set; }

        public string Motd { get; set; }

        /// <summary>
        ///     Версия сервера
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        ///     Кол-во игроков онлайн
        /// </summary>
        public string CurrentPlayers { get; set; }

        /// <summary>
        ///     Макс. кол-во слотов
        /// </summary>
        public string MaximumPlayers { get; set; }

        /// <summary>
        ///     Включен ли сервер
        /// </summary>
        public bool ServerUp { get; set; }

        /// <summary>
        ///     Время задержки ответа (основная характеристика качества игры)
        /// </summary>
        public long Delay { get; set; }

        #endregion
    }
}
