using System;
using System.Collections.Generic;
using System.Linq;
using ChestLocker.Model.Events;
using System.IO;

namespace ChestLocker.Model.Core
{
    public class GameModel : ObservableModel
    {
        private Probe _probe;
        private Lock[] _satellites;
        private int[] _activationOrder;
        private int _currentActivationIndex;
        private bool _isGameOver;
        private static int _globalHighScore = 0;
        private int _localHighScore = 0;

        // Параметры круговой орбиты
        private double _orbitAngle = 0;
        private double _orbitRadius = 5.0;
        private double _orbitSpeed = 1.5;
        private int _orbitDirection = 1;

        // Бесконечный режим
        public event Action OnSlowdown;
        private int _score = 0;
        private int _totalOpened = 0;
        private Random _random;
        private int _highScore = 0;
        private int _combo = 0;
        private int _bestCombo = 0;
        private int _lastPointsEarned = 0;
        public event Action OnLockOpened;
        public event Action OnWrongLock;
        public event Action OnHealLockOpened;
        public event Action OnGameOver;



        public Probe Probe => _probe;
        public int[] ActivationOrder => _activationOrder;
        public int HighScore => _highScore;
        public Lock[] Satellites => _satellites;
        public int Score => _score;
        public int Combo => _combo;
        public int BestCombo => _bestCombo;
        public int ComboMultiplier => Math.Min(5, 1 + (_combo / 4));
        public int LastPointsEarned => _lastPointsEarned;
        public int CurrentActivationIndex => _currentActivationIndex;

        public bool IsGameOver
        {
            get => _isGameOver;
            set
            {
                _isGameOver = value;
                Notify(nameof(IsGameOver), value);
            }
        }
        public GameModel(int seed = 42)
        {
            _random = new Random(seed);
            InitializeSatellites();
            InitializeProbe();
            GenerateNewOrder();
            _currentActivationIndex = 0;
            _isGameOver = false;
            _score = 0;
            _totalOpened = 0;
            _combo = 0;
            _bestCombo = 0;
            _lastPointsEarned = 0;
            _orbitSpeed = 1.5;
            _orbitDirection = 1;
            LoadHighScore();
        }

        private void InitializeSatellites()
        {
            _satellites = new Lock[8];
            for (int i = 0; i < 8; i++)
            {
                _satellites[i] = new Lock(i, i);
                _satellites[i].IsActivated = false;
            }
        }

        private void InitializeProbe()
        {
            Vector2D startPos = new Vector2D(5, 0);
            Vector2D startVel = new Vector2D(0, 2.236);
            _probe = new Probe(startPos, startVel);
            _orbitAngle = 0;
            _orbitSpeed = 1.5;
            _orbitDirection = 1;
        }

        private void GenerateNewOrder()
        {
            var order = new List<int>();
            for (int i = 0; i < 8; i++)
            {
                order.Add(i);
            }

            for (int i = order.Count - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                int temp = order[i];
                order[i] = order[j];
                order[j] = temp;
            }

            _activationOrder = order.ToArray();
            _currentActivationIndex = 0;

            System.Diagnostics.Debug.WriteLine("Новый порядок активации: " + string.Join(", ", _activationOrder));
            AssignHealingLock();
        }

        public void UpdatePhysics(double deltaTime)
        {
            if (_isGameOver) return;

            _orbitAngle += _orbitSpeed * deltaTime * _orbitDirection;
            if (_orbitAngle > Math.PI * 2) _orbitAngle -= (float)(Math.PI * 2);
            if (_orbitAngle < 0) _orbitAngle += (float)(Math.PI * 2);

            double x = Math.Cos(_orbitAngle) * _orbitRadius;
            double y = Math.Sin(_orbitAngle) * _orbitRadius;
            _probe.Position = new Vector2D(x, y);

            double vx = -Math.Sin(_orbitAngle) * _orbitSpeed * _orbitRadius;
            double vy = Math.Cos(_orbitAngle) * _orbitSpeed * _orbitRadius;
            _probe.Velocity = new Vector2D(vx, vy);

            CheckSatelliteProximity();
        }

        private void CheckSatelliteProximity()
        {
            for (int i = 0; i < _satellites.Length; i++)
            {
                double angle = i * 2 * Math.PI / 8;
                Vector2D satPos = new Vector2D(Math.Cos(angle) * _orbitRadius, Math.Sin(angle) * _orbitRadius);

                double dx = _probe.Position.X - satPos.X;
                double dy = _probe.Position.Y - satPos.Y;
                double distance = Math.Sqrt(dx * dx + dy * dy);

                if (distance < 0.5)
                {
                    _probe.CurrentSatelliteIndex = i;
                    return;
                }
            }

            _probe.CurrentSatelliteIndex = -1;
        }

        private void ResetCombo()
        {
            _combo = 0;
            _lastPointsEarned = 0;
        }

        public bool TryActivateSatellite(int satelliteId)
        {
            if (_isGameOver) return false;
            if (satelliteId < 0 || satelliteId >= _satellites.Length) return false;
            if (_satellites[satelliteId].IsActivated) return false;
            if (_probe.CurrentSatelliteIndex != satelliteId) return false;

            bool isCorrectOrder = (_activationOrder[_currentActivationIndex] == satelliteId);

            if (!isCorrectOrder)
            {
                _probe.LoseEnergy(1);
                ResetCombo();
                OnWrongLock?.Invoke();
                _orbitSpeed *= 0.8f;
                if (_orbitSpeed < 0.5f) _orbitSpeed = 0.5f;

                OnSlowdown?.Invoke();

                System.Diagnostics.Debug.WriteLine($"Неправильный замок! Энергия: {_probe.Energy}, Скорость: {_orbitSpeed:F2}");

                if (_probe.Energy <= 0)
                {
                    IsGameOver = true;
                    OnGameOver?.Invoke();
                }
                return false;
            }

            // ===== ЛЕЧЕБНЫЙ ЗАМОК =====
            if (_satellites[satelliteId].IsHealingLock && _probe.Energy < 3)
            {
                _probe.Energy++;
                OnHealLockOpened?.Invoke();
                System.Diagnostics.Debug.WriteLine($"Лечебный замок! +1 жизнь. Теперь жизней: {_probe.Energy}");
            }
            else if (_satellites[satelliteId].IsHealingLock && _probe.Energy >= 3)
            {
                System.Diagnostics.Debug.WriteLine($"Лечебный замок, но жизней уже максимум (3)");
            }

            // АКТИВАЦИЯ ЗАМКА
            _satellites[satelliteId].IsActivated = true;
            _currentActivationIndex++;
            _combo++;
            if (_combo > _bestCombo) _bestCombo = _combo;
            _lastPointsEarned = 10 * ComboMultiplier;
            _score += _lastPointsEarned;
            _totalOpened++;

            OnLockOpened?.Invoke();

            System.Diagnostics.Debug.WriteLine($"Добавлено {_lastPointsEarned} очков. Комбо: {_combo}, множитель: x{ComboMultiplier}. Текущий счёт: {_score}, Рекорд до проверки: {_highScore}");

            // ===== ПРОВЕРКА И СОХРАНЕНИЕ РЕКОРДА =====
            if (_score > _highScore)
            {
                _highScore = _score;     
                SaveHighScore();          
            }

            System.Diagnostics.Debug.WriteLine($"Замок {satelliteId} открыт! Счёт: {_score}, прогресс: {_currentActivationIndex}/8");
            _orbitSpeed *= 1.08f;
            if (_orbitSpeed > 5.0f) _orbitSpeed = 5.0f;
            _orbitDirection *= -1;

            if (_currentActivationIndex >= _activationOrder.Length)
            {
                System.Diagnostics.Debug.WriteLine("=== ВСЕ ЗАМКИ ВОССТАНОВЛЕНЫ! НАЧИНАЕМ НОВЫЙ ЦИКЛ ===");

                for (int i = 0; i < _satellites.Length; i++)
                {
                    if (_satellites[i].IsActivated)
                    {
                        _satellites[i].ResetLock();
                    }
                }

                GenerateNewOrder();
            }

            return true;
        }

        public void PunishWrongActivation()
        {
            if (_isGameOver) return;

            _probe.LoseEnergy(1);
            ResetCombo();
            OnWrongLock?.Invoke();
            _orbitSpeed *= 0.8f; 
            if (_orbitSpeed < 0.5f) _orbitSpeed = 0.5f; 

            OnSlowdown?.Invoke();

            System.Diagnostics.Debug.WriteLine($"Нажатие в пустоту! Энергия: {_probe.Energy}, Скорость ключа: {_orbitSpeed:F2}");

            if (_probe.Energy <= 0)
            {
                IsGameOver = true;
                OnGameOver?.Invoke();
            }
        }

        public double GetCurrentSpeed()
        {
            return _orbitSpeed / 1.5;
        }

        public int GetActiveLocksCount()
        {
            int count = 0;
            for (int i = 0; i < _satellites.Length; i++)
            {
                if (_satellites[i].IsActivated) count++;
            }
            return count;
        }

        public void ResetGame()
        {
            InitializeSatellites();
            InitializeProbe();
            GenerateNewOrder();
            _currentActivationIndex = 0;
            _isGameOver = false;
            _score = 0;
            _totalOpened = 0;
            _combo = 0;
            _bestCombo = 0;
            _lastPointsEarned = 0;
            _orbitSpeed = 1.5;
            _orbitDirection = 1;
            System.Diagnostics.Debug.WriteLine($"Игра перезапущена. Текущий рекорд: {_highScore}");
        }
        private void AssignHealingLock()
        {
            for (int i = 0; i < _satellites.Length; i++)
            {
                _satellites[i].IsHealingLock = false;
            }

            int healingCount = _random.Next(1, 3);

            for (int i = 0; i < healingCount; i++)
            {
                int LockIndex = _random.Next(_satellites.Length);
                while (_satellites[LockIndex].IsHealingLock)
                {
                    LockIndex = _random.Next(_satellites.Length);
                }
                _satellites[LockIndex].IsHealingLock = true;
                System.Diagnostics.Debug.WriteLine($"Лечебный замок назначен: {LockIndex}");
            }
        }
        private const string HighScoreKey = "HighScore";

        private void LoadHighScore()
        {
            try
            {
                string filePath = "highscore.txt";
                if (File.Exists(filePath))
                {
                    string content = File.ReadAllText(filePath);
                    _highScore = int.Parse(content);
                }
                else
                {
                    _highScore = 0;
                }
            }
            catch
            {
                _highScore = 0;
            }
        }

        private void SaveHighScore()
        {
            try
            {
                string filePath = "highscore.txt";
                File.WriteAllText(filePath, _highScore.ToString());
            }
            catch
            {
            }
        }
    }
}