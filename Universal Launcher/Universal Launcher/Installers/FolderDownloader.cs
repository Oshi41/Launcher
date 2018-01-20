using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Universal_Launcher.Singleton;
using Universal_Launcher.ViewModels;

namespace Universal_Launcher.Installers
{
    internal class FolderDownloader
    {
        private readonly bool _deleteArch;
        private readonly string _destinationFolder;
        private readonly string _link;
        private readonly bool _reinstall;

        /// <summary>
        /// </summary>
        /// <param name="link">Ссылка для скачивания</param>
        /// <param name="destinationFolder">Куда папку скачиваем</param>
        /// <param name="deleteArch">Удалить архив после распаковки</param>
        /// <param name="reinstall">Удалить папку, если она есть</param>
        public FolderDownloader(string link, string destinationFolder,
            bool deleteArch, bool reinstall = false)
        {
            _link = link;
            _destinationFolder = destinationFolder;
            _deleteArch = deleteArch;
            _reinstall = reinstall;
        }

        public async Task<bool> Begin()
        {
            var show = IoCContainer.Instanse.Resolve<IShowMessage>();

            if (Directory.Exists(_destinationFolder))
            {
                if (!_reinstall)
                    return false;

                await show.ShowWorkerAsync("Очищаем папку", () => Directory.Delete(_destinationFolder, true));

                //using (new ShowViewModel("Очищаем папку"))
                //{
                //    await Task.Run(() => Directory.Delete(_destinationFolder, true));
                //}
            }
            else
            {
                Directory.CreateDirectory(_destinationFolder);
            }

            var archName = Path.Combine(_destinationFolder, "temp.zip");

            var download = new DownloadingViewModel(_link, archName);

            await show.ShowWorkerAsync(download, () => download.Start());

            //await show.ShowAsync(download, false);

            if (!download.Success)
            {
                await show.ShowMessageAsync("Произошла ошибка");
                //var confirm = new ConfirmViewModel("Произошла ошибка");
                //await show.ShowAsync(confirm);
                return false;
            }

            await show.ShowWorkerAsync("Распаковываем архив",
                () => ZipFile.ExtractToDirectory(archName, Path.GetDirectoryName(archName)));

            //using (new ShowViewModel("Распаковываем архив"))
            //{
            //    await Task.Run(() => ZipFile.ExtractToDirectory(archName, Path.GetDirectoryName(archName)));
            //}

            if (_deleteArch)
                File.Delete(archName);

            return true;
        }
    }
}