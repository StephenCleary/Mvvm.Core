using System;
using System.ComponentModel;

namespace Nito.Mvvm
{
    /// <summary>
    /// An object capable of raising <see cref="INotifyPropertyChanged.PropertyChanged"/>. This interface is typically implemented explicitly.
    /// </summary>
    public interface IRaisePropertyChanged
    {
        /// <summary>
        /// Raises <see cref="INotifyPropertyChanged.PropertyChanged"/> with the specified arguments.
        /// </summary>
        /// <param name="args">The event arguments to pass to <see cref="INotifyPropertyChanged.PropertyChanged"/>.</param>
        void RaisePropertyChanged(PropertyChangedEventArgs args);
    }
}