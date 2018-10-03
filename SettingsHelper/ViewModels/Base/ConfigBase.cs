using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using Launcher.Config;
using Launcher.Helper;
using Launcher.Models;
using Mvvm;
using Mvvm.Commands;

namespace SettingsHelper.ViewModels.Base
{
    public abstract class ConfigBase<T> : BindableBase
    {
        #region FIelds

        protected DownloadManager _manager;

        #endregion

        #region Commands

        private ICommand SaveCommand { get; }

        #endregion

        #region CommandHandlers

        private void OnSave()
        {
            var dlg = new SaveFileDialog();
            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                var json = ToJson();
                File.WriteAllText(dlg.FileName, json);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
            }
        }

        protected bool CanDownload(DownloadManager manager)
        {
            return manager?.IsDownloading != true;
        }

        #endregion

        protected ConfigBase()
        {
            SaveCommand = new DelegateCommand(OnSave);
        }

        protected virtual string ToJson()
        {
            return JsonHelper.Serialize<T>(this);
        }

        protected void SafeExecute(Action action)
        {
            if (action == null)
                return;

            Dispatcher.CurrentDispatcher.Invoke(action, DispatcherPriority.ApplicationIdle);
        }
    }
}
