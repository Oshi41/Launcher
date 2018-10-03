using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using Launcher.Helper;
using Launcher.Models;
using Mvvm;
using Mvvm.Commands;

namespace SettingsHelper.ViewModels.Base
{
    public abstract class SettingsViewModelBase<T> : BindableBase
    {
        #region Fields

        protected DownloadManager _manager;

        #endregion

        #region Command

        public ICommand SaveCommand { get; private set; }

        #endregion

        #region Command Handler

        private void OnSave()
        {
            var dlg = new SaveFileDialog();
            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                var item = ToModel();
                var json = JsonHelper.Serialize<T>(item);
                File.WriteAllText(dlg.FileName, json);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
            }
        }

        #endregion

        protected SettingsViewModelBase()
        {
            SaveCommand = new DelegateCommand(OnSave);
        }

        #region Abstract

        protected abstract T ToModel();
        protected abstract void Refresh(T model);

        #endregion

        #region Protected

        protected bool IsDownloading()
        {
            return _manager?.IsDownloading == true;
        }

        #endregion
    }
}
