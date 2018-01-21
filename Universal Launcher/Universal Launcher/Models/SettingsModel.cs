using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Serialization;
using Universal_Launcher.Settings;
using Universal_Launcher.Singleton;

namespace Universal_Launcher.Models
{
    public class SettingsModel
    {
        public SettingsModel()
        {
            _folderService = IoCContainer.Instanse.Resolve<IFolderService>();
        }

        #region Props

        public Settings.Settings Settings { get; set; }

        #endregion

        #region Field

        private readonly IFolderService _folderService;


        #endregion

        #region Methods

        /// <summary>
        ///     Читаю настрйоки или создаю дефолтные
        /// </summary>
        public void ReadOrCreate()
        {
            if (File.Exists(_folderService.SettingsPath))
            {
                ReadSettings();
                DownloadInfo();
                WriteSettings();
            }
            else
            {
                Settings = new Settings.Settings();
                DownloadInfo();
                WriteSettings();
            }
        }

        private void ReadSettings()
        {
            try
            {
                try
                {
                    var serializer = new XmlSerializer(typeof(Settings.Settings));

                    using (var reader = new StreamReader(_folderService.SettingsPath))
                    {
                        Settings = (Settings.Settings) serializer.Deserialize(reader);
                    }
                }
                catch
                {
                    Settings = new Settings.Settings();
                    WriteSettings();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void WriteSettings()
        {
            try
            {
                using (var writer = new StreamWriter(_folderService.SettingsPath, false))
                {
                    var serializer = new XmlSerializer(typeof(Settings.Settings));
                    serializer.Serialize(writer, Settings);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        ///     Скачиваем информацию о проекте
        /// </summary>
        /// <param name="link">Ссылка на скачивание</param>
        public void DownloadInfo()
        {
            var tempName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                _folderService.RootFolder, "temp.xml");

            if (File.Exists(tempName))
                File.Delete(tempName);

            if (!Directory.Exists(Path.GetDirectoryName(tempName)))
                Directory.CreateDirectory(Path.GetDirectoryName(tempName));

            try
            {
                using (var client = new WebClient())
                {
                    client.DownloadFile(new Uri(Properties.Resources.LauncherSettingsLink), tempName);
                    client.Dispose();
                }

                using (var reader = XmlReader.Create(new StreamReader(tempName)))
                {
                    var serializer = new XmlSerializer(typeof(LauncherSettings));

                    Settings.LauncherSettings = serializer.CanDeserialize(reader)
                        ? (LauncherSettings) serializer.Deserialize(reader)
                        : new LauncherSettings();
                }
            }
            catch
            {
                // ignored
            }
        }

        #endregion
    }
}