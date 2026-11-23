using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;

namespace Prism.Commands
{
    /// <summary>
    /// Minimal drop-in replacement for Prism.Commands.DelegateCommand.
    /// </summary>
    public class DelegateCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public event EventHandler CanExecuteChanged;

        public DelegateCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;

        public void Execute(object parameter)
        {
            if (CanExecute(parameter))
                _execute();
        }

        public void RaiseCanExecuteChanged()
        {
            void Raise() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

            if (MainThread.IsMainThread)
                Raise();
            else
                MainThread.BeginInvokeOnMainThread(Raise);
        }
    }

    /// <summary>
    /// Generic variant kept for completeness.
    /// </summary>
    public class DelegateCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public event EventHandler CanExecuteChanged;

        public DelegateCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke(Cast(parameter)) ?? true;

        public void Execute(object parameter)
        {
            if (CanExecute(parameter))
                _execute(Cast(parameter));
        }

        public void RaiseCanExecuteChanged()
        {
            void Raise() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

            if (MainThread.IsMainThread)
                Raise();
            else
                MainThread.BeginInvokeOnMainThread(Raise);
        }

        private static T Cast(object parameter) => parameter is T t ? t : default;
    }
}
