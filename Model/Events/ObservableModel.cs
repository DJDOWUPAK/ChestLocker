using System;
using System.Collections.Generic;

namespace ChestLocker.Model.Events
{
    public abstract class ObservableModel
    {
        private readonly List<IModelObserver> _observers = new List<IModelObserver>();

        public void Attach(IModelObserver observer)
        {
            if (!_observers.Contains(observer))
                _observers.Add(observer);
        }

        public void Detach(IModelObserver observer)
        {
            _observers.Remove(observer);
        }

        protected void Notify(string propertyName, object newValue)
        {
            foreach (var observer in _observers)
            {
                observer.OnModelChanged(propertyName, newValue);
            }
        }
    }
}