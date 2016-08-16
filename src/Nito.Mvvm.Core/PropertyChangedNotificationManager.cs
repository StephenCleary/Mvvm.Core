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
        private static PropertyChangedNotificationManager _singletonInstance;
        private readonly HashSet<PropertyChangedNotification> _propertiesRequiringNotification = new HashSet<PropertyChangedNotification>();
        private readonly Lazy<ResumeOnDispose> _resumeOnDispose;
        private int _referenceCount;

        private PropertyChangedNotificationManager()
        {
            _resumeOnDispose = new Lazy<ResumeOnDispose>(() => new ResumeOnDispose(this));
        }

        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static PropertyChangedNotificationManager Instance
        {
            get
            {
                if (_singletonInstance == null)
                    _singletonInstance = new PropertyChangedNotificationManager();
                return _singletonInstance;
            }
        }

        /// <summary>
        /// Defers <see cref="INotifyPropertyChanged.PropertyChanged"/> events until the returned disposable is disposed. Deferrals are reference counted, so they are safe to nest. Do not dispose the returned object more than once.
        /// </summary>
        public IDisposable DeferNotifications()
        {
            ++_referenceCount;
            return _resumeOnDispose.Value;
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
        /// <param name="raisePropertyChanged">An object capable of raising <see cref="INotifyPropertyChanged.PropertyChanged"/>. May not be <c>null</c>.</param>
        /// <param name="args">The event arguments to pass to <see cref="INotifyPropertyChanged.PropertyChanged"/>.</param>
        public void Register(IRaisePropertyChanged raisePropertyChanged, PropertyChangedEventArgs args)
        {
            if (raisePropertyChanged == null)
                throw new ArgumentNullException(nameof(raisePropertyChanged));

            if (_referenceCount == 0)
                raisePropertyChanged.RaisePropertyChanged(args);
            else
                _propertiesRequiringNotification.Add(new PropertyChangedNotification(raisePropertyChanged, args));
        }

        /// <summary>
        /// Registers a <see cref="INotifyPropertyChanged.PropertyChanged"/> event. If events are not deferred, then the event is raised immediately.
        /// </summary>
        /// <param name="raisePropertyChanged">An object capable of raising <see cref="INotifyPropertyChanged.PropertyChanged"/>. May not be <c>null</c>.</param>
        /// <param name="propertyName">The name of the property that changed.</param>
        public void Register(IRaisePropertyChanged raisePropertyChanged, string propertyName)
        {
            Register(raisePropertyChanged, PropertyChangedEventArgsCache.Instance.Get(propertyName));
        }

        private sealed class ResumeOnDispose : IDisposable
        {
            private readonly PropertyChangedNotificationManager _parent;

            public ResumeOnDispose(PropertyChangedNotificationManager parent)
            {
                _parent = parent;
            }

            public void Dispose()
            {
                _parent.ResumeNotifications();
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

            public int ReferenceCount => _value._referenceCount;

            public HashSet<PropertyChangedNotification> DeferredNotifications => _value._propertiesRequiringNotification;
        }
    }
}