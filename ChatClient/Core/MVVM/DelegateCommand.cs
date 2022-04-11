using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ChatClient.MVVM.Core
{
    public class DelegateCommand : ICommand
    {
        private readonly Func<Task> _executeTask;
        private readonly Func<bool> _canExecute;
        private readonly Action _execute;


        public DelegateCommand(Action execute) : this(execute, null) { }
        public DelegateCommand(Func<Task> execute) : this(execute, canExecute: null) { }
        public DelegateCommand(Action execute, Func<bool> canExecute) 
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public DelegateCommand(Func<Task> execute, Func<bool> canExecute)
        {
            _executeTask = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute();
        }

        public async void Execute(object parameter)
        {
            if (_execute == null)
            {
                await _executeTask();
                return;
            }
            _execute();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
