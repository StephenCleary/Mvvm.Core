using System;

namespace Nito.Mvvm
{
    /// <summary>
    /// An explicit implementation of <c>ICommand.CanExecuteChanged</c>.
    /// </summary>
    public sealed class ExplicitCanExecute : ICanExecute
    {
        /// <summary>
        /// The weak event implementation.
        /// </summary>
        private readonly WeakCanExecuteChanged _canExecuteChanged;

        /// <summary>
        /// Whether the command can be executed.
        /// </summary>
        private bool _canExecute;

        /// <summary>
        /// Creates a new explicit implementation of <c>ICommand.CanExecuteChanged</c>.
        /// </summary>
        /// <param name="sender">The sender of the <c>ICommand.CanExecuteChanged</c> event.</param>
        public ExplicitCanExecute(object sender)
        {
            _canExecuteChanged = new WeakCanExecuteChanged(sender);
        }

        /// <summary>
        /// Whether the command can be executed. This property does not raise <see cref="System.ComponentModel.INotifyPropertyChanged.PropertyChanged"/>; it raises <see cref="ICanExecuteChanged.CanExecuteChanged"/> instead.
        /// </summary>
        public bool CanExecute
        {
            get { return _canExecute; }
            set
            {
                if (_canExecute == value)
                    return;
                _canExecute = value;
                _canExecuteChanged.OnCanExecuteChanged();
            }
        }

        event EventHandler ICanExecute.CanExecuteChanged
        {
            add { _canExecuteChanged.CanExecuteChanged += value; }
            remove { _canExecuteChanged.CanExecuteChanged -= value; }
        }

        bool ICanExecute.CanExecute(object parameter)
        {
            return CanExecute;
        }
    }
}