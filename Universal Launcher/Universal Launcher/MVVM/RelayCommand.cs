using System;
using System.Windows.Input;

namespace Universal_Launcher.MVVM
{
    public class RelayCommand : ICommand
    {
        private readonly Predicate<object> _canExecute;
        private readonly Action<object> _execute;

        public RelayCommand(Action<object> onExecuteMethod, Predicate<object> onCanExecuteMethod = null)
        {
            _execute = onExecuteMethod;
            _canExecute = onCanExecuteMethod ?? (o => true);
        }

        public RelayCommand(Action onExecuteMethod, Func<bool> onCanExecuteMethod = null)
        {
            _execute = o => onExecuteMethod();
            _canExecute = o => onCanExecuteMethod?.Invoke() ?? true;
        }

        #region ICommand Members

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute.Invoke(parameter);
        }

        public void Execute(object parameter)
        {
            _execute.Invoke(parameter);
        }

        #endregion
    }
}