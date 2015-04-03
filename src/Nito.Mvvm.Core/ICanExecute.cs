using System;

namespace Nito.Mvvm
{
    /// <summary>
    /// An implementation of <c>ICommand.CanExecuteChanged</c> using weak events.
    /// </summary>
    public interface ICanExecute
    {
        /// <summary>
        /// Occurs when the return value of <c>ICommand.CanExecute</c> may have changed. This is a weak event.
        /// </summary>
        event EventHandler CanExecuteChanged;

        bool CanExecute(object parameter);
    }
}