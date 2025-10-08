using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using game;

namespace game
{
    public class HealthBarHUD
    {
        private Texture2D _bgTexture;
        private Texture2D _barTexture;
        private Texture2D _frameTexture;
        private PlayerStats _playerStats;

        private Vector2 _position; // Position of the health bar center
        private Rectangle _bgRect;
        private Rectangle _barRect;
        private Rectangle _frameRect;

        private int _width = 1288;
        private int _height = 72;

        private float _healthPercent;
        public HealthBarHUD(PlayerStats playerStats, GraphicsDevice graphicsDevice)
        {
            _playerStats = playerStats;

            // Default position: bottom center of screen
            _position = new Vector2(graphicsDevice.Viewport.Width / 2, graphicsDevice.Viewport.Height - 75);

            // Rectangles are centered based on _position
            _bgRect = new Rectangle((int)(_position.X - _width / 2), (int)(_position.Y - _height / 2), _width, _height);
            _barRect = new Rectangle(_bgRect.X, _bgRect.Y, _width, _height);
            _frameRect = new Rectangle(_bgRect.X, _bgRect.Y, _width, _height);
        }

        public void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            _bgTexture = content.Load<Texture2D>("HUD/HealthBar_BG");
            _barTexture = content.Load<Texture2D>("HUD/HealthBar_Bar");
            _frameTexture = content.Load<Texture2D>("HUD/HealthBar_Frame");
        }

        public void Update()
        {
            // Just calculate the percentage, not resize anything
            _healthPercent = (float)_playerStats.CurrentHP / _playerStats.HP.Value;
            _healthPercent = MathHelper.Clamp(_healthPercent, 0f, 1f);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw order: BG -> Bar -> Frame
            spriteBatch.Draw(_bgTexture, _bgRect, Color.White);

            // Create a source rectangle to "cut" the texture instead of scaling it
            int barWidth = (int)(_width * _healthPercent);
            Rectangle sourceRect = new Rectangle(0, 0, barWidth, _height);

            // Draw only the visible part of the bar
            spriteBatch.Draw(
                _barTexture,
                destinationRectangle: new Rectangle(_bgRect.X, _bgRect.Y, barWidth, _height),
                sourceRectangle: sourceRect,
                color: Color.White
            );

            spriteBatch.Draw(_frameTexture, _frameRect, Color.White);
        }
    }
}
