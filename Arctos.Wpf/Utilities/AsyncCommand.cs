using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Arctos.Wpf.Utilities
{
    public class AsyncCommand : ICommand
    {
        private readonly Func<object, Task> _execute;
        private readonly Predicate<object>? _canExecute;

        public AsyncCommand(Func<object, Task> execute, Predicate<object>? canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return _canExecute?.Invoke(parameter!) ?? true;
        }

        public async void Execute(object? parameter)
        {
            RaiseCanExecuteChanged();
            try
            {
                await _execute(parameter!);
            }
            finally
            {
                RaiseCanExecuteChanged();
            }
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
