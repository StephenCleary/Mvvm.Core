using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Nito.Mvvm
{
    /// <summary>
    /// Defers and consolidates <see cref="INotifyPropertyChanged.PropertyChanged"/> events. Events may be raised out of order when notifications are resumed. Instances of <see cref="IRaisePropertyChanged"/> are compared using reference equality during consolidation.
    /// </summary>
    [DebuggerTypeProxy(typeof(DebugView))]
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public sealed class PropertyChangedNotificationManager
    {
        [ThreadStatic]
        private static PropertyChangedNotificationManager SingletonInstance;
        private readonly HashSet<PropertyChangedNotification> _propertiesRequiringNotification = new HashSet<PropertyChangedNotification>();
        private int _referenceCount;

        private PropertyChangedNotificationManager()
        {
        }

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static PropertyChangedNotificationManager Instance
        {
            get
            {
                if (SingletonInstance == null)
                    SingletonInstance = new PropertyChangedNotificationManager();
                return SingletonInstance;
            }
        }

        /// <summary>
        /// Defers <see cref="INotifyPropertyChanged.PropertyChanged"/> events until the returned disposable is disposed. Deferrals are reference counted, so they are safe to nest. Do not dispose the returned object more than once.
        /// </summary>
        public IDisposable DeferNotifications()
        {
            ++_referenceCount;
            return ResumeOnDispose.Instance;
        }

        private void ResumeNotifications()
        {
            --_referenceCount;
            if (_referenceCount != 0)
                return;
            var properties = new PropertyChangedNotification[_propertiesRequiringNotification.Count];
            _propertiesRequiringNotification.CopyTo(properties);
            _propertiesRequiringNotification.Clear();
            foreach (var property in properties)
                property.RaisePropertyChanged.RaisePropertyChanged(property.EventArgs);
        }

        /// <summary>
        /// Registers a <see cref="INotifyPropertyChanged.PropertyChanged"/> event. If events are not deferred, then the event is raised immediately.
        /// </summary>
        /// <param name="raisePropertyChanged">An object capable of raising <see cref="INotifyPropertyChanged.PropertyChanged"/>.</param>
        /// <param name="args">The event arguments to pass to <see cref="INotifyPropertyChanged.PropertyChanged"/>.</param>
        public void Register(IRaisePropertyChanged raisePropertyChanged, PropertyChangedEventArgs e)
        {
            if (_referenceCount == 0)
                raisePropertyChanged.RaisePropertyChanged(e);
            else
                _propertiesRequiringNotification.Add(new PropertyChangedNotification(raisePropertyChanged, e));
        }

        /// <summary>
        /// Registers a <see cref="INotifyPropertyChanged.PropertyChanged"/> event. If events are not deferred, then the event is raised immediately.
        /// </summary>
        /// <param name="raisePropertyChanged">An object capable of raising <see cref="INotifyPropertyChanged.PropertyChanged"/>.</param>
        /// <param name="propertyName">The name of the property that changed.</param>
        public void Register(IRaisePropertyChanged raisePropertyChanged, string propertyName)
        {
            Register(raisePropertyChanged, PropertyChangedEventArgsCache.Instance.Get(propertyName));
        }

        private sealed class ResumeOnDispose : IDisposable
        {
            public static ResumeOnDispose Instance = new ResumeOnDispose();

            public void Dispose()
            {
                PropertyChangedNotificationManager.Instance.ResumeNotifications();
            }
        }

        private struct PropertyChangedNotification : IEquatable<PropertyChangedNotification>
        {
            public PropertyChangedNotification(IRaisePropertyChanged raisePropertyChanged, PropertyChangedEventArgs eventArgs)
            {
                RaisePropertyChanged = raisePropertyChanged;
                EventArgs = eventArgs;
            }

            public readonly IRaisePropertyChanged RaisePropertyChanged;
            public readonly PropertyChangedEventArgs EventArgs;

            public bool Equals(PropertyChangedNotification other)
            {
                return ReferenceEquals(RaisePropertyChanged, other.RaisePropertyChanged) && EventArgs.PropertyName == other.EventArgs.PropertyName;
            }

            public override int GetHashCode()
            {
                return RuntimeHelpers.GetHashCode(RaisePropertyChanged) ^ EventArgs.PropertyName.GetHashCode();
            }
        }

        [DebuggerNonUserCode]
        private string DebuggerDisplay
        {
            get
            {
                if (_referenceCount == 0)
                    return "Not deferred";
                return "Deferred; notification count: " + _propertiesRequiringNotification.Count + ", defer refcount: " + _referenceCount;
            }
        }

        [DebuggerNonUserCode]
        private sealed class DebugView
        {
            private readonly PropertyChangedNotificationManager _value;

            public DebugView(PropertyChangedNotificationManager value)
            {
                _value = value;
            }

            public int ReferenceCount { get { return _value._referenceCount; } }

            public HashSet<PropertyChangedNotification> DeferredNotifications { get { return _value._propertiesRequiringNotification; } }
        }
    }
}