using Launcher.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Launcher.Config.Interfaces;

namespace Launcher.Config
{
    public interface IConfig { };

    public class LauncherConfig
    {
        public static string Folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Universal Launcher", "config");

        public List<IConfig> Configs { get; set; } = new List<IConfig>();

        [JsonIgnore]
        public static LauncherConfig Default => new LauncherConfig
        {
            Configs = new List<IConfig>
                {
                    new SystemSettings(),
                    new AuthentificationSettings(),
                    new LauncherSettings(),
                    new ProjectSettings(),
                }
        };

        public event EventHandler ConfigChanged;

        #region Methods

        public void Save()
        {
            if (!Directory.Exists(Folder))
                Directory.CreateDirectory(Folder);

            foreach (var item in Configs)
            {
                try
                {
                    var json = JsonConvert.SerializeObject(item, Formatting.Indented);
                    File.WriteAllText(Path.Combine(Folder, item.GetType().Name), json);
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e.Message);
                }
            }
        }

        public void Read()
        {
            if (!Directory.Exists(Folder))
                Directory.CreateDirectory(Folder);

            var implemented = AppDomain
                .CurrentDomain
                .GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => typeof(IConfig).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
                .ToList();
            foreach (var type in implemented)
            {
                var name = Path.Combine(Folder, type.Name);
                if (!File.Exists(name))
                    continue;
                try
                {
                    var text = File.ReadAllText(name);
                    var deserialize = (IConfig)JsonConvert.DeserializeObject(text, type);
                    if (deserialize != null)
                        Replace(deserialize);
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e.Message);
                }
            }
        }

        public T GetConfig<T>() where T : class, IConfig, new()
        {
            return Configs.OfType<T>().FirstOrDefault() ?? new T();
        }

        public void Replace<T>(T source) 
            where T : IConfig
        {
            if (source == null)
                return;

            var type = source.GetType();
            var find = Configs.FirstOrDefault(x => x.GetType() == type);
            if (find != null)
                Configs.Remove(find);

            Configs.Add(source);
        }

        public void ReadAndDownload()
        {
            Read();

            DownloadConfig();
        }

        public async Task DownloadConfig()
        {
            var uri = GetConfig<LauncherSettings>().ConfigUri;

            var downloadManager = new DownloadManager(uri);

            var filePath = await downloadManager.Download();
            if (downloadManager.IsError)
                return;

            try
            {
                var json = File.ReadAllText(filePath);
                downloadManager.DeleteFile();
                var settings = JsonConvert.DeserializeObject<ProjectSettings>(json);
                Replace(settings);

                ConfigChanged?.Invoke(settings, EventArgs.Empty);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
            }
        }

        #endregion
    }
    
}
