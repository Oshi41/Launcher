using System;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using Universal_Launcher.MVVM;

namespace Universal_Launcher.ViewModels
{
    public class MessageViewModel : ViewModelBase
    {
        private string _text;
        private bool _isError;
        private bool _result;


        public string Text
        {
            get { return _text; }
            set {  Set(ref _text, value); }
        }

        public bool IsError
        {
            get { return _isError; }
            set { Set(ref _isError, value); }
        }

        public ICommand Command { get; set; }
        public bool Result => _result;


        public MessageViewModel(string text, bool isError)
        {
            _text = text;
            _isError = isError;

            Command = new RelayCommand(OnClick);
        }


        private void OnClick(object obj)
        {
            Boolean.TryParse(obj.ToString(), out _result);

            DialogHost.CloseDialogCommand.Execute(null, null);
        }
    }
}
