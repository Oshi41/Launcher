using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Universal_Launcher.MVVM
{
    public class RelayCommandAsync : ICommand
    {
        private readonly Predicate<object> _canExecute;
        private readonly Func<object, Task> _execute;

        private long _isExecuting;

        public RelayCommandAsync(Func<object, Task> execute, Predicate<object> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute ?? (o => true);
        }

        public RelayCommandAsync(Func<Task> execute, Func<bool> canExecute = null)
        {
            _execute = o => execute();
            _canExecute = o => canExecute?.Invoke() ?? true;
        }


        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter)
        {
            return Interlocked.Read(ref _isExecuting) == 0 && _canExecute(parameter);
        }

        public async void Execute(object parameter)
        {
            Interlocked.Exchange(ref _isExecuting, 1);
            RaiseCanExecuteChanged();

            try
            {
                await _execute(parameter);
            }
            finally
            {
                Interlocked.Exchange(ref _isExecuting, 0);
                RaiseCanExecuteChanged();
            }
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}