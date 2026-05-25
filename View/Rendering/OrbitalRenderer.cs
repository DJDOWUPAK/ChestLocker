using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ChestLocker.Model.Core;
using ChestLocker.View.Observers;
using System;

namespace ChestLocker.View.Rendering
{
    public class OrbitalRenderer
    {
        private GraphicsDevice _graphicsDevice;
        private SpriteBatch _spriteBatch;

        private Vector2 _cameraOffset;
        private float _scale = 40f;

        // Спрайты
        private Texture2D _templeSprite;
        private Texture2D _keySprite;
        private Texture2D _LockClosedSprite;
        private Texture2D _LockOpenSprite;
        private Texture2D _heartFullSprite;
        private Texture2D _heartEmptySprite;
        private Texture2D _backgroundSprite;
        private Texture2D _healingLockClosedSprite;
        private Texture2D _healingLockOpenSprite;

        // Для отрисовки линий и кругов
        private Texture2D _pixelTexture;
        private SpriteFont _defaultFont;

        // Для анимации
        private float _pulseTimer = 0;

        public OrbitalRenderer(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            _graphicsDevice = graphicsDevice;
            _spriteBatch = spriteBatch;
            _cameraOffset = new Vector2(512, 384);

            _pixelTexture = new Texture2D(_graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
        }

        public void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            try
            {
                _backgroundSprite = content.Load<Texture2D>("background");
                _templeSprite = content.Load<Texture2D>("temple");
                _keySprite = content.Load<Texture2D>("key");
                _LockClosedSprite = content.Load<Texture2D>("Lock_closed");
                _LockOpenSprite = content.Load<Texture2D>("Lock_open");
                _heartFullSprite = content.Load<Texture2D>("heart_full");
                _heartEmptySprite = content.Load<Texture2D>("heart_empty");
                _healingLockClosedSprite = content.Load<Texture2D>("healing_Lock_closed");
                _healingLockOpenSprite = content.Load<Texture2D>("healing_Lock_open");
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Ошибка загрузки спрайтов: " + e.Message);
            }
        }

        public void SetFont(SpriteFont font)
        {
            _defaultFont = font;
        }

        private Vector2 WorldToScreen(Vector2D worldPos)
        {
            return new Vector2(
                (float)(worldPos.X * _scale + _cameraOffset.X),
                (float)(-worldPos.Y * _scale + _cameraOffset.Y)
            );
        }

        public void Draw(GameModel model, ProbeObserver probeObserver, LockObserver[] satelliteObservers, GameModelObserver gameModelObserver, SpriteFont font)
        {
            if (font != null) _defaultFont = font;

            _pulseTimer += 0.1f;
            if (_pulseTimer > Math.PI * 2) _pulseTimer -= (float)(Math.PI * 2);

            _spriteBatch.Begin();

            // Фон
            if (_backgroundSprite != null)
            {
                _spriteBatch.Draw(_backgroundSprite, Vector2.Zero, Color.White);
            }

            // Орбита
            DrawOrbit();

            // Храм
            if (_templeSprite != null)
            {
                Vector2 templePos = _cameraOffset - new Vector2(_templeSprite.Width / 2, _templeSprite.Height / 2);
                _spriteBatch.Draw(_templeSprite, templePos, Color.White);
            }

            // Замки
            for (int i = 0; i < model.Satellites.Length; i++)
            {
                DrawLock(model.Satellites[i], satelliteObservers[i], model);
            }

            // Ключ
            if (_keySprite != null)
            {
                Vector2 screenPos = WorldToScreen(probeObserver.LastPosition);
                _spriteBatch.Draw(_keySprite, screenPos - new Vector2(_keySprite.Width / 2, _keySprite.Height / 2), Color.White);
            }

            // UI
            DrawUI(model, probeObserver, gameModelObserver);

            _spriteBatch.End();
        }

        private void DrawOrbit()
        {
            float radius = 5f * _scale;
            Vector2 center = _cameraOffset;

            const int segments = 120;
            for (int i = 0; i < segments; i++)
            {
                float angle1 = i * MathHelper.TwoPi / segments;
                float angle2 = (i + 1) * MathHelper.TwoPi / segments;

                Vector2 point1 = center + new Vector2((float)Math.Cos(angle1) * radius, (float)Math.Sin(angle1) * radius);
                Vector2 point2 = center + new Vector2((float)Math.Cos(angle2) * radius, (float)Math.Sin(angle2) * radius);

                DrawLine(point1, point2, Color.Gold);
            }
        }

        private void DrawLock(Lock Lock, LockObserver observer, GameModel model)
        {
            double angle = Lock.OrbitPosition * 2 * Math.PI / 8;
            Vector2D pos = new Vector2D(Math.Cos(angle) * 5, Math.Sin(angle) * 5);
            Vector2 screenPos = WorldToScreen(pos);

            bool isActivated = Lock.IsActivated;
            bool isHealing = Lock.IsHealingLock;

            // Выбор спрайта в зависимости от типа замка
            Texture2D sprite = null;
            if (isHealing)
            {
                sprite = isActivated ? _healingLockOpenSprite : _healingLockClosedSprite;
            }
            else
            {
                sprite = isActivated ? _LockOpenSprite : _LockClosedSprite;
            }

            float scale = 1.0f;

            int nextLock = -1;
            if (model.CurrentActivationIndex < model.ActivationOrder.Length)
            {
                nextLock = model.ActivationOrder[model.CurrentActivationIndex];
            }

            // Подсвечиваем следующий замок
            if (!isActivated && Lock.Id == nextLock && nextLock != -1)
            {
                // Пульсация
                float pulse = (float)(Math.Sin(_pulseTimer * 3) * 0.15f + 1.15f);
                scale = pulse;

                // Свечение
                Color glowColor = isHealing ? Color.Magenta : Color.Gold;
                float alpha = (float)(Math.Sin(_pulseTimer * 4) * 0.3f + 0.7f);
                DrawCircle(screenPos, 28f, glowColor * alpha, 3);
                DrawCircle(screenPos, 35f, (isHealing ? Color.Pink : Color.Orange) * (alpha * 0.4f), 2);

                // Парящие частицы
                for (int i = 0; i < 8; i++)
                {
                    float angle2 = (float)(_pulseTimer * 2 + i * Math.PI * 2 / 8);
                    float radius2 = 22f + (float)Math.Sin(_pulseTimer * 5 + i) * 5f;
                    float x = screenPos.X + (float)Math.Cos(angle2) * radius2;
                    float y = screenPos.Y + (float)Math.Sin(angle2) * radius2;
                    float alpha2 = (float)(Math.Sin(_pulseTimer * 6 + i) * 0.5f + 0.5f);
                    DrawFilledCircle(new Vector2(x, y), 2f, glowColor * alpha2);
                }
            }

            // Постоянное свечение для открытых замков
            if (isActivated)
            {
                Color glowColor = isHealing ? Color.Magenta : Color.Gold;
                DrawCircle(screenPos, 22f, glowColor * 0.4f, 2);
            }

            if (sprite == null)
            {
                return;
            }

            Vector2 origin = new Vector2(sprite.Width / 2, sprite.Height / 2);
            _spriteBatch.Draw(sprite, screenPos, null, Color.White, 0f, origin, scale, SpriteEffects.None, 0);
        }

        private void DrawUI(GameModel model, ProbeObserver probeObserver, GameModelObserver gameModelObserver)
        {
            // Сердечки
            int mana = model.Probe.Energy;
            for (int i = 0; i < 3; i++)
            {
                int x = 20 + i * 30;
                int y = 20;
                Texture2D heart = (i < mana) ? _heartFullSprite : _heartEmptySprite;
                if (heart != null)
                {
                    _spriteBatch.Draw(heart, new Vector2(x, y), Color.White);
                }
            }

            // Логотип
            if (_defaultFont != null)
            {
                string logo = "ANCIENT TEMPLE";
                Vector2 logoSize = _defaultFont.MeasureString(logo);
                _spriteBatch.DrawString(_defaultFont, logo, new Vector2(1024 - logoSize.X - 10, 10), Color.Yellow);
            }

            // Подсказка о лечебных замках
            if (_defaultFont != null && !gameModelObserver.IsGameOver)
            {
                string healingHint = "HEART LOCK = +1 LIFE";
                Vector2 healingSize = _defaultFont.MeasureString(healingHint);
                _spriteBatch.DrawString(_defaultFont, healingHint,
                    new Vector2(1024 - healingSize.X - 10, 50), Color.Black);
            }

            // Скорость ключа
            if (_defaultFont != null && !gameModelObserver.IsGameOver && model.CurrentActivationIndex > 0)
            {
                double speed = model.GetCurrentSpeed();
                string speedText = $"KEY SPEED: {speed:F1}x";
                Vector2 speedSize = _defaultFont.MeasureString(speedText);
                _spriteBatch.DrawString(_defaultFont, speedText,
                    new Vector2(1024 - speedSize.X - 10, 80), Color.Orange);
            }

            // Комбо и множитель очков
            if (_defaultFont != null && !gameModelObserver.IsGameOver && model.Combo > 0)
            {
                string comboText = $"COMBO: {model.Combo}  x{model.ComboMultiplier}";
                Vector2 comboSize = _defaultFont.MeasureString(comboText);
                _spriteBatch.DrawString(_defaultFont, comboText,
                    new Vector2(1024 - comboSize.X - 10, 170), Color.LightGreen);

                string pointsText = $"LAST: +{model.LastPointsEarned}";
                Vector2 pointsSize = _defaultFont.MeasureString(pointsText);
                _spriteBatch.DrawString(_defaultFont, pointsText,
                    new Vector2(1024 - pointsSize.X - 10, 200), Color.LightGreen);
            }

            // ПОДСКАЗКА "PRESS SPACE"
            if (_defaultFont != null && !gameModelObserver.IsGameOver)
            {
                string nearText = "PRESS SPACE TO OPEN LOCK";
                Vector2 nearSize = _defaultFont.MeasureString(nearText);

                int bgWidth = (int)nearSize.X + 40;
                int bgHeight = 35;
                int bgX = 512 - bgWidth / 2;
                int bgY = 660;

                DrawFilledRectangle(new Rectangle(bgX, bgY, bgWidth, bgHeight), Color.Black * 0.7f);
                DrawRectangle(new Rectangle(bgX, bgY, bgWidth, bgHeight), Color.Yellow, 2);

                _spriteBatch.DrawString(_defaultFont, nearText,
                    new Vector2(512 - nearSize.X / 2, 668), Color.Yellow);
            }

            // Экран поражения
            if (gameModelObserver.IsGameOver && _defaultFont != null)
            {
                DrawFilledRectangle(new Rectangle(0, 0, 1024, 768), Color.Black * 0.85f);

                string loseLine1 = "GAME OVER";
                Vector2 size1 = _defaultFont.MeasureString(loseLine1);
                _spriteBatch.DrawString(_defaultFont, loseLine1,
                    new Vector2(512 - size1.X / 2, 250), Color.Red);

                string scoreText = $"YOUR SCORE: {model.Score}";
                Vector2 scoreSize = _defaultFont.MeasureString(scoreText);
                _spriteBatch.DrawString(_defaultFont, scoreText,
                    new Vector2(512 - scoreSize.X / 2, 300), Color.White);

                string bestComboText = $"BEST COMBO: {model.BestCombo}";
                Vector2 bestComboSize = _defaultFont.MeasureString(bestComboText);
                _spriteBatch.DrawString(_defaultFont, bestComboText,
                    new Vector2(512 - bestComboSize.X / 2, 340), Color.LightGreen);

                string highScoreText = $"HIGH SCORE: {model.HighScore}";
                Vector2 highScoreSize = _defaultFont.MeasureString(highScoreText);
                _spriteBatch.DrawString(_defaultFont, highScoreText,
                    new Vector2(1024 - highScoreSize.X - 10, 140), Color.Yellow);

                string loseLine2 = "Press R to try again";
                Vector2 size2 = _defaultFont.MeasureString(loseLine2);
                _spriteBatch.DrawString(_defaultFont, loseLine2,
                    new Vector2(512 - size2.X / 2, 400), Color.White);

                string controlsHint = "R - restart | ESC - exit";
                Vector2 hintSize = _defaultFont.MeasureString(controlsHint);
                _spriteBatch.DrawString(_defaultFont, controlsHint,
                    new Vector2(512 - hintSize.X / 2, 450), Color.LightGray);
            }
            if (_defaultFont != null && !gameModelObserver.IsGameOver)
            {
                string scoreText = $"SCORE: {model.Score}";
                Vector2 scoreSize = _defaultFont.MeasureString(scoreText);
                _spriteBatch.DrawString(_defaultFont, scoreText,
                    new Vector2(1024 - scoreSize.X - 10, 110), Color.Yellow);

                string highScoreText = $"HIGH SCORE: {model.HighScore}";
                Vector2 highScoreSize = _defaultFont.MeasureString(highScoreText);
                _spriteBatch.DrawString(_defaultFont, highScoreText,
                    new Vector2(1024 - highScoreSize.X - 10, 140), Color.Yellow);
            }
        }

        private void DrawLine(Vector2 start, Vector2 end, Color color)
        {
            Vector2 edge = end - start;
            float angle = (float)Math.Atan2(edge.Y, edge.X);
            float length = edge.Length();

            if (length > 0)
            {
                _spriteBatch.Draw(_pixelTexture, start, null, color, angle, Vector2.Zero, new Vector2(length, 1), SpriteEffects.None, 0);
            }
        }

        private void DrawCircle(Vector2 center, float radius, Color color, int thickness = 1)
        {
            const int segments = 36;
            for (int i = 0; i < segments; i++)
            {
                float angle1 = i * MathHelper.TwoPi / segments;
                float angle2 = (i + 1) * MathHelper.TwoPi / segments;

                Vector2 point1 = center + new Vector2((float)Math.Cos(angle1) * radius, (float)Math.Sin(angle1) * radius);
                Vector2 point2 = center + new Vector2((float)Math.Cos(angle2) * radius, (float)Math.Sin(angle2) * radius);

                DrawLine(point1, point2, color);
            }
        }

        private void DrawFilledCircle(Vector2 center, float radius, Color color)
        {
            int r = (int)radius;
            for (int x = -r; x <= r; x++)
            {
                for (int y = -r; y <= r; y++)
                {
                    if (x * x + y * y <= r * r)
                    {
                        _spriteBatch.Draw(_pixelTexture, center + new Vector2(x, y), color);
                    }
                }
            }
        }

        private void DrawFilledRectangle(Rectangle rect, Color color)
        {
            _spriteBatch.Draw(_pixelTexture, rect, color);
        }

        private void DrawRectangle(Rectangle rect, Color color, int thickness)
        {
            DrawFilledRectangle(new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
            DrawFilledRectangle(new Rectangle(rect.X, rect.Y + rect.Height - thickness, rect.Width, thickness), color);
            DrawFilledRectangle(new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
            DrawFilledRectangle(new Rectangle(rect.X + rect.Width - thickness, rect.Y, thickness, rect.Height), color);
        }
    }
}