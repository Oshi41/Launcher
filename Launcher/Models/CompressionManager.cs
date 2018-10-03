using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Launcher.Models
{
    class CompressionManager
    {
        #region Fields
        private readonly string _zipName;
        private readonly string _folderName;

        #endregion

        public CompressionManager(string zipName, string folderName)
        {
            _zipName = zipName;
            _folderName = folderName;
        }

        #region Events

        public event EventHandler Complited;

        #endregion

        #region Methods

        public async Task<bool> Extract(bool needToDeleteArch = false, bool needToClearFolder = false)
        {
            try
            {
                if (!File.Exists(_zipName))
                    throw new FileNotFoundException($"Cannot find file - {_zipName}");

                // очищаем папку
                if (Directory.Exists(_folderName) && needToClearFolder)
                    Directory.Delete(_folderName, true);

                if (!Directory.Exists(_folderName))
                    Directory.CreateDirectory(_folderName);

                await Task.Run(() => ZipFile.ExtractToDirectory(_zipName, _folderName));

                if (needToDeleteArch)
                    File.Delete(_zipName);

                return true;
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);

                return false;
            }
            finally
            {
                Complited?.Invoke(this, EventArgs.Empty);
            }

        }

        public async Task<bool> Compress()
        {
            try
            {
                if (File.Exists(_zipName))
                    File.Delete(_zipName);

                if (!Directory.Exists(_folderName))
                    throw new FileNotFoundException($"Cannot find folder - {_folderName}");

                await Task.Run(() => ZipFile.CreateFromDirectory(_zipName, _folderName));

                return true;
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                return false;
            }
            finally
            {
                Complited?.Invoke(this, EventArgs.Empty);
            }
        }

        #endregion
    }
}
