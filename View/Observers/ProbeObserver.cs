using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChestLocker.Model.Events;
using ChestLocker.Model.Core;

namespace ChestLocker.View.Observers
{
    public class ProbeObserver : IModelObserver
    {
        public Vector2D LastPosition { get; private set; }
        public Vector2D LastVelocity { get; private set; }
        public int LastEnergy { get; private set; }
        public int LastSatelliteIndex { get; private set; }

        public event Action<Vector2D> OnPositionChanged;
        public event Action<Vector2D> OnVelocityChanged;
        public event Action<int> OnEnergyChanged;
        public event Action<int> OnSatelliteIndexChanged;

        public void OnModelChanged(string propertyName, object newValue)
        {
            switch (propertyName)
            {
                case nameof(Probe.Position):
                    LastPosition = (Vector2D)newValue;
                    OnPositionChanged?.Invoke(LastPosition);
                    break;
                case nameof(Probe.Velocity):
                    LastVelocity = (Vector2D)newValue;
                    OnVelocityChanged?.Invoke(LastVelocity);
                    break;
                case nameof(Probe.Energy):
                    LastEnergy = (int)newValue;
                    OnEnergyChanged?.Invoke(LastEnergy);
                    break;
                case nameof(Probe.CurrentSatelliteIndex):
                    LastSatelliteIndex = (int)newValue;
                    OnSatelliteIndexChanged?.Invoke(LastSatelliteIndex);
                    break;
            }
        }
    }
}