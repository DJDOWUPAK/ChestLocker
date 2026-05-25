using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChestLocker.Model.Events;
using ChestLocker.Model.Core;

namespace ChestLocker.View.Observers
{
    public class LockObserver : IModelObserver
    {
        private int _satelliteId;


        public bool IsActivated { get; private set; }

        public event Action<int, bool> OnActivationChanged;

        public LockObserver(int satelliteId)
        {
            _satelliteId = satelliteId;
        }

        public void OnModelChanged(string propertyName, object newValue)
        {
            switch (propertyName)
            { 
                case nameof(Lock.IsActivated):
                    IsActivated = (bool)newValue;
                    OnActivationChanged?.Invoke(_satelliteId, IsActivated); 
                    break;
            }
        }
    }
}