using ChatClient.ViewModel;
using System;

namespace ChatClient.Core.Events
{
    class ConnectionEventArgs : EventArgs
    {
        public MainViewModel MainViewModelContext;
    }
}
