
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Animations;

namespace game
{ 
    public class PlayerAnimation
    {
        private AnimController _animController;
        private AnimatedTexture _textureChar;
        private int _row = 3;
        private int overLoad;

        // Track last movement direction for animation facing
        private Vector2 _lastDirection = new Vector2(0, 1); // default down

        public PlayerAnimation(AnimController texture)
        {
            _animController = texture;
            overLoad = 1;
        }
        public PlayerAnimation(AnimatedTexture texture)
        {
            _textureChar = texture;
            overLoad = 2;
        }

        public void Update(GameTime gameTime, Vector2 direction, Vector2 position)
        {
            // Update last direction if moving
            if (direction != Vector2.Zero)
                _lastDirection = direction;

            // Map last direction to row for AnimatedTexture
            _row = _lastDirection.Y < 0 ? 4 :      // Up
                   _lastDirection.Y > 0 ? 3 :      // Down
                   _lastDirection.X < 0 ? 1 :      // Left
                   _lastDirection.X > 0 ? 2 : 3;   // Right / default Down

            if (overLoad == 1)
            {
                // Map last direction to animation name for AnimController
                string directionName = _row switch
                {
                    1 => "left",
                    2 => "right",
                    3 => "down",
                    4 => "up",
                    _ => "down"
                };

                // If moving, set walking animation
                if (direction != Vector2.Zero)
                    _animController.SetAnimation("Walk", directionName);

                _animController.UpdateFrame(gameTime, position);
            }
            else if (overLoad == 2)
            {
                // Update AnimatedTexture frame if moving
                if (direction != Vector2.Zero)
                    _textureChar.UpdateFrame((float)gameTime.ElapsedGameTime.TotalSeconds);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _animController.DrawFrame(spriteBatch);
        }
        public void Draw(SpriteBatch spriteBatch, Vector2 position)
        {
            _textureChar.DrawFrame(spriteBatch, position, _row);
        }

        public void TriggerAttack()
        {
            if (overLoad == 1 && _animController != null)
            {
                // Use the same walking animation for attack (there's no attack sheet rn)
                string directionName = _row switch
                {
                    1 => "left",
                    2 => "right",
                    3 => "down",
                    4 => "up",
                    _ => "down"
                };

                _animController.SetAnimation("Walk", directionName);
            }
            else if (overLoad == 2 && _textureChar != null)
            {
                // Pause at the row that matches last direction
                _textureChar.Pause(0, _row);
            }
        }

        public void OnAnimationEvent(IAnimationController sender, AnimationEventTrigger trigger)
        {
            if (overLoad == 1 && trigger == AnimationEventTrigger.AnimationCompleted)
            {
                if (overLoad == 1)
                {
                    _animController.SetAnimation("Walk", "up");
                }
            }
        }
    }
}
