using System.ComponentModel;
using System.Runtime.CompilerServices;
using Universal_Launcher.Properties;

namespace Universal_Launcher.MVVM
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        protected bool Set<T>(ref T field, T newValue, [CallerMemberName] string name = null)
        {
            if (ReferenceEquals(field, newValue))
                return false;

            field = newValue;
            OnPropertyChanged(name);
            return true;
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}