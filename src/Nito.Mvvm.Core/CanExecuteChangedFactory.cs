using System;
using System.Collections.Generic;

namespace Nito.Mvvm
{
    /// <summary>
    /// Global factory method for creating <see cref="ICanExecuteChanged"/> implementations.
    /// </summary>
    public static class CanExecuteChangedFactory
    {
        /// <summary>
        /// Factory method for creating weak implementations of <see cref="ICanExecuteChanged"/>.
        /// </summary>
        public static readonly Func<object, ICanExecuteChanged> CreateWeakCanExecuteChanged = sender => new WeakCanExecuteChanged(sender);

        /// <summary>
        /// Factory method for creating strong implementations of <see cref="ICanExecuteChanged"/>.
        /// </summary>
        public static readonly Func<object, ICanExecuteChanged> CreateStrongCanExecuteChanged = sender => new StrongCanExecuteChanged(sender);

        /// <summary>
        /// Default factory method for creating <see cref="ICanExecuteChanged"/> implementations.
        /// </summary>
        public static Func<object, ICanExecuteChanged> Create { get; set; } = CreateStrongCanExecuteChanged;
    }
}
