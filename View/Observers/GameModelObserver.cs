using System;
using ChestLocker.Model.Events;
using ChestLocker.Model.Core;

namespace ChestLocker.View.Observers
{
    public class GameModelObserver : IModelObserver
    {
        public bool IsGameOver { get; private set; }

        public event Action<bool> OnGameOverChanged;

        public void OnModelChanged(string propertyName, object newValue)
        {
            switch (propertyName)
            {
                case nameof(GameModel.IsGameOver):
                    IsGameOver = (bool)newValue;
                    OnGameOverChanged?.Invoke(IsGameOver);
                    break;
            }
        }
    }
}