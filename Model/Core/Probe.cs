using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChestLocker.Model.Events;

namespace ChestLocker.Model.Core
{
    public class Probe : ObservableModel
    {
        private Vector2D _position;
        private Vector2D _velocity;
        private int _energy;
        private int _currentSatelliteIndex;

        public Vector2D Position
        {
            get => _position;
            set
            {
                _position = value;
                Notify(nameof(Position), value);
            }
        }

        public Vector2D Velocity
        {
            get => _velocity;
            set
            {
                _velocity = value;
                Notify(nameof(Velocity), value);
            }
        }

        public int Energy
        {
            get => _energy;
            set
            {
                if (_energy != value)
                {
                    _energy = value;
                    Notify(nameof(Energy), value);
                }
            }
        }

        public int CurrentSatelliteIndex
        {
            get => _currentSatelliteIndex;
            set
            {
                _currentSatelliteIndex = value;
                Notify(nameof(CurrentSatelliteIndex), value);
            }
        }

        public Probe(Vector2D startPosition, Vector2D startVelocity)
        {
            _position = startPosition;
            _velocity = startVelocity;
            _energy = 3;
            _currentSatelliteIndex = -1;
        }

        public void LoseEnergy(int amount = 1)
        {
            Energy -= amount;
        }
    }
}