using System;
using System.Windows;
using System.Windows.Input;

namespace TopDownWpfClient.Models {
    public class DelegateCommand<T> : ICommand {
        private readonly Action<T> _execute;
        private readonly Predicate<T> _canExecute;

        public bool CanExecute(object parameter) {
            return _canExecute == null || _canExecute.Invoke((T) parameter);
        }

        public void Execute(object parameter) {
            try {
                _execute?.Invoke((T) parameter);
            } catch(Exception e) {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event EventHandler CanExecuteChanged {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public DelegateCommand(Action<T> execute) : this(execute, null) { }

        public DelegateCommand(Action<T> execute, Predicate<T> canExecute) {
            _execute = execute;
            _canExecute = canExecute;
        }
    }
}