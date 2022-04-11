using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ChatClient.MVVM.Core
{
    public class BindableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual bool SetProperty<T>(ref T field, T value, [CallerMemberName] string callerName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }
            field = value;
            RaisePropertyChanged(callerName);
            return true;
        }
        protected virtual bool SetProperty<T>(ref T field, T value, Action onChanged, [CallerMemberName] string callerName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            onChanged?.Invoke();
            RaisePropertyChanged(callerName);
            return true;
        }
        protected void RaisePropertyChanged([CallerMemberName] string callerName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(callerName));
        }
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            PropertyChanged?.Invoke(this, args);
        }
    }
}
