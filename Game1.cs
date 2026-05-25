using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using ChestLocker.Model.Core;
using ChestLocker.Controller;
using ChestLocker.View.Observers;
using ChestLocker.View.Rendering;
using System;

namespace ChestLocker
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private GameModel _gameModel;
        private InputController _inputController;

        private ProbeObserver _probeObserver;
        private GameModelObserver _gameModelObserver;
        private LockObserver[] _satelliteObservers;

        private OrbitalRenderer _orbitalRenderer;

        private KeyboardState _previousKeyboardState;
        private SpriteFont _defaultFont;

        // Звуковые эффекты
        private SoundEffect _openLockSound;
        private SoundEffect _errorSound;
        private SoundEffect _healSound;
        private SoundEffect _gameOverSound;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferWidth = 1024;
            _graphics.PreferredBackBufferHeight = 768;
            _graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            _gameModel = new GameModel(seed: new Random().Next());
            _inputController = new InputController(_gameModel);

            _probeObserver = new ProbeObserver();
            _gameModelObserver = new GameModelObserver();

            _satelliteObservers = new LockObserver[8];
            for (int i = 0; i < 8; i++)
            {
                _satelliteObservers[i] = new LockObserver(i);
                _gameModel.Satellites[i].Attach(_satelliteObservers[i]);
            }

            _gameModel.Attach(_gameModelObserver);
            _gameModel.Probe.Attach(_probeObserver);

            _probeObserver.OnModelChanged(nameof(Probe.Energy), _gameModel.Probe.Energy);

            // ПОДПИСКА НА ЗВУКИ
            _gameModel.OnLockOpened += () => _openLockSound?.Play();
            _gameModel.OnWrongLock += () => _errorSound?.Play();
            _gameModel.OnHealLockOpened += () => _healSound?.Play();
            _gameModel.OnGameOver += () => _gameOverSound?.Play();

            _previousKeyboardState = Keyboard.GetState();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _orbitalRenderer = new OrbitalRenderer(GraphicsDevice, _spriteBatch);

            // ЗАГРУЗКА ЗВУКОВ
            _openLockSound = Content.Load<SoundEffect>("open_lock");
            _errorSound = Content.Load<SoundEffect>("error");
            _healSound = Content.Load<SoundEffect>("heal");
            _gameOverSound = Content.Load<SoundEffect>("game_over");

            _orbitalRenderer.LoadContent(Content);

            try
            {
                _defaultFont = Content.Load<SpriteFont>("DefaultFont");
                _orbitalRenderer.SetFont(_defaultFont);
                System.Diagnostics.Debug.WriteLine("Шрифт успешно загружен");
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Ошибка загрузки шрифта: " + e.Message);
            }
        }

        protected override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            deltaTime = Math.Min(deltaTime, 0.033f);

            KeyboardState currentKeyboardState = Keyboard.GetState();

            // Активация (SPACE)
            if (currentKeyboardState.IsKeyDown(Keys.Space) && _previousKeyboardState.IsKeyUp(Keys.Space))
            {
                _inputController.HandleActivateCommand();
            }

            // Перезапуск (R)
            if (currentKeyboardState.IsKeyDown(Keys.R) && _previousKeyboardState.IsKeyUp(Keys.R))
            {
                _gameModel = new GameModel(seed: new Random().Next());
                _inputController = new InputController(_gameModel);

                _probeObserver = new ProbeObserver();
                _gameModelObserver = new GameModelObserver();

                _satelliteObservers = new LockObserver[8];
                for (int i = 0; i < 8; i++)
                {
                    _satelliteObservers[i] = new LockObserver(i);
                    _gameModel.Satellites[i].Attach(_satelliteObservers[i]);
                }

                _gameModel.Attach(_gameModelObserver);
                _gameModel.Probe.Attach(_probeObserver);

                _gameModel.OnLockOpened += () => _openLockSound?.Play();
                _gameModel.OnWrongLock += () => _errorSound?.Play();
                _gameModel.OnHealLockOpened += () => _healSound?.Play();
                _gameModel.OnGameOver += () => _gameOverSound?.Play();
            }

            // Выход (ESC)
            if (currentKeyboardState.IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            _previousKeyboardState = currentKeyboardState;

            if (!_gameModel.IsGameOver)
            {
                _inputController.Update(deltaTime);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _orbitalRenderer.Draw(_gameModel, _probeObserver, _satelliteObservers, _gameModelObserver, _defaultFont);

            base.Draw(gameTime);
        }
    }
}