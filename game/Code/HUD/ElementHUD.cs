using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace game
{
    public class ElementHUD
    {
        private AnimatedTexture _elementAnim;
        private Player _player;

        private Vector2 _position; // Center above health bar

        private const int FrameCount = 8;       // 8 frames in your sprite sheet
        private const int FrameWidth = 168;     // 1344 / 8
        private const int FrameHeight = 188;

        private bool _isSwitching = false;
        private int _targetFrame;               // Desired frame (0 for Light, 7 for Dark)
        private float _timer = 0f;
        private float totalDuration = 0.1f; // half a second to switch completely
        private float _frameDelay;

        public ElementHUD(Player player, GraphicsDevice graphicsDevice)
        {
            _player = player;
            _position = new Vector2(graphicsDevice.Viewport.Width / 2, graphicsDevice.Viewport.Height - 202);

            _elementAnim = new AnimatedTexture(
                origin: new Vector2(FrameWidth / 2f, FrameHeight / 2f),
                rotation: 0f,
                scale: 1f,
                depth: 0f
            );

            _frameDelay = totalDuration / (FrameCount - 1); // Seconds per frame when switching

        }

        public void LoadContent(ContentManager content)
        {
            _elementAnim.Load(content, "HUD/Elements", frameCount: FrameCount, frameRow: 1, framesPerSec: 60);
            _elementAnim.Pause();             // Stop automatic frame update
            _elementAnim.Reset();
            _elementAnim.SetFrame(0);         // Start with Light element
        }

        public void Update(GameTime gameTime)
        {
            // Determine target frame based on player element
            _targetFrame = _player.CurrentElement == ElementType.Light ? 0 : FrameCount - 1;

            if (_elementAnim.CurrentFrame != _targetFrame)
                _isSwitching = true;

            if (_isSwitching)
            {
                _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (_timer >= _frameDelay)
                {
                    _timer = 0f;
                    int current = _elementAnim.CurrentFrame;

                    if (current < _targetFrame)
                        _elementAnim.SetFrame(current + 1);
                    else if (current > _targetFrame)
                        _elementAnim.SetFrame(current - 1);

                    // Stop switching when target reached
                    if (_elementAnim.CurrentFrame == _targetFrame)
                        _isSwitching = false;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _elementAnim.DrawFrame(spriteBatch, _position);
        }
    }
}
