using ChestLocker.Model.Core;

namespace ChestLocker.Controller
{
    public class InputController
    {
        private GameModel _gameModel;

        public InputController(GameModel gameModel)
        {
            _gameModel = gameModel;
        }

        public void HandleActivateCommand()
        {
            if (_gameModel.IsGameOver) return;

            int currentSatellite = _gameModel.Probe.CurrentSatelliteIndex;

            if (currentSatellite >= 0)
            {
                _gameModel.TryActivateSatellite(currentSatellite);
            }
            else
            {
                _gameModel.PunishWrongActivation();
            }
        }

        public void HandleResetCommand()
        {
            _gameModel.ResetGame();
        }

        public void Update(double deltaTime)
        {
            _gameModel.UpdatePhysics(deltaTime);
        }
    }
}