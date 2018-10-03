using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mvvm;

namespace SettingsHelper.ViewModels
{
    public enum ErrorStatus
    {
        None,
        Error,
        Success,
        Warning,
    }

    public class InfoBoxViewModel : BindableBase
    {
        private string _text;
        private ErrorStatus _status;
        private string _error;

        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }

        public ErrorStatus Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public string Error
        {
            get => _error;
            set => SetProperty(ref _error, value);
        }

        public void Clear()
        {
            Status = ErrorStatus.None;
        }

        public void SetError(string message)
        {
            Error = message;
            Status = ErrorStatus.Error;
        }

        public void SetSuccess(string message = null)
        {
            Error = message;
            Status = ErrorStatus.Success;
        }
    }
}
