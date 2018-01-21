using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using Universal_Launcher.MVVM;

namespace Universal_Launcher.ViewModels
{
    public class MessageViewModel : ViewModelBase
    {
        private bool _isError;
        private bool _result;
        private string _text;


        public MessageViewModel(string text, bool isError)
        {
            _text = text;
            _isError = isError;

            Command = new RelayCommand(OnClick);
        }


        public string Text
        {
            get => _text;
            set => Set(ref _text, value);
        }

        public bool IsError
        {
            get => _isError;
            set => Set(ref _isError, value);
        }

        public ICommand Command { get; set; }
        public bool Result => _result;


        private void OnClick(object obj)
        {
            bool.TryParse(obj.ToString(), out _result);

            DialogHost.CloseDialogCommand.Execute(null, null);
        }
    }
}