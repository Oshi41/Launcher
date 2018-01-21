using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Universal_Launcher.Singleton;
using Universal_Launcher.ViewModels;

namespace Universal_Launcher.Installers
{
    internal class LauncherInstaller
    {
        private readonly INameService _nameService;
        private readonly IShowMessage _showMessageService;
        private readonly string _tempDir;

        private readonly string _tempLaunName = "temp.zip";
        private readonly string _tempUpdName = "upd.zip";

        private string _origin;

        public LauncherInstaller()
        {
            _nameService = IoCContainer.Instanse.Resolve<INameService>();
            _showMessageService = IoCContainer.Instanse.Resolve<IShowMessage>();

            _origin = AppDomain.CurrentDomain.FriendlyName;
            _tempDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                _nameService.ProjectName, "TempUpdate");

            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);

            Directory.CreateDirectory(_tempDir);
        }


        public async Task Begin()
        {
            var unpackedUpdName = Path.Combine(_tempDir, _tempUpdName);
            var unpackedLaunName = Path.Combine(_tempDir, _tempLaunName);

            // delete old files
            DeleteFiles(unpackedLaunName, unpackedUpdName);

            // download all needed
            DownloadingViewModel download;

            download = new DownloadingViewModel(Properties.Resources.UpdaterLink, unpackedUpdName);
            await _showMessageService.ShowWorkerAsync(download, () => download.Start());

            download = new DownloadingViewModel(Properties.Resources.LauncherLink, unpackedLaunName);
            await _showMessageService.ShowWorkerAsync(download, () => download.Start());

            // extract all of it
            string launName;
            string updName;

            using (var zip = ZipFile.Open(unpackedLaunName, ZipArchiveMode.Read))
            {
                launName = Path.Combine(
                    Path.GetDirectoryName(unpackedLaunName),
                    zip.Entries.FirstOrDefault().Name);
            }

            using (var zip = ZipFile.Open(unpackedUpdName, ZipArchiveMode.Read))
            {
                updName = Path.Combine(
                    Path.GetDirectoryName(unpackedUpdName),
                    zip.Entries.FirstOrDefault().Name);
            }

            // delete old exe's
            DeleteFiles(launName, updName);

            // unpack all arch
            ZipFile.ExtractToDirectory(unpackedLaunName, Path.GetDirectoryName(unpackedLaunName));
            ZipFile.ExtractToDirectory(unpackedUpdName, Path.GetDirectoryName(unpackedUpdName));

            // delete arch
            DeleteFiles(unpackedLaunName, unpackedUpdName);
            var curentName = "Universal Launcher.exe";

            // start update
            Process.Start(updName, $"\"{curentName}\" \"{launName}\"");
            Process.GetCurrentProcess().Kill();
            Application.Current.Shutdown(1);
        }

        private void DeleteFiles(params string[] files)
        {
            foreach (var file in files)
                if (File.Exists(file))
                    File.Delete(file);
        }
    }
}