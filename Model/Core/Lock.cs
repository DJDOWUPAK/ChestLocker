using ChestLocker.Model.Events;

namespace ChestLocker.Model.Core
{
    public class Lock : ObservableModel
    {
        private bool _isActivated;
        private bool _isHealingLock;

        public int OrbitPosition { get; set; }
        public int Id { get; set; }

       

        public bool IsActivated
        {
            get => _isActivated;
            set
            {
                if (_isActivated != value)
                {
                    _isActivated = value;
                    Notify(nameof(IsActivated), value);
                }
            }
        }

        public bool IsHealingLock
        {
            get => _isHealingLock;
            set
            {
                if (_isHealingLock != value)
                {
                    _isHealingLock = value;
                    Notify(nameof(IsHealingLock), value);
                }
            }
        }
        public Lock(int id, int orbitPosition)
        {
            Id = id;
            OrbitPosition = orbitPosition;
            _isActivated = false;
            _isHealingLock = false;
        }

        public void ResetLock()
        {
            _isActivated = false;
            Notify(nameof(IsActivated), false);
        }
    }
}