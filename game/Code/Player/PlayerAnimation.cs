
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Animations;
using System;

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

        public void Update(GameTime gameTime, Vector2 direction, Vector2 position, bool isAttacking)
        {
            // Update last direction if moving
            if (direction != Vector2.Zero)
                _lastDirection = direction;

            if (_lastDirection != Vector2.Zero)
            {
                if (Math.Abs(_lastDirection.X) >= Math.Abs(_lastDirection.Y))
                {
                    // Horizontal dominant
                    _row = _lastDirection.X < 0 ? 1 : 2; // Left : Right
                }
                else
                {
                    // Vertical dominant
                    _row = _lastDirection.Y < 0 ? 4 : 3; // Up : Down
                }
            }

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
                if (direction != Vector2.Zero && !isAttacking)
                {
                    // Walking animation
                    _animController.SetAnimation("Walk", directionName);
                }
                else if (direction == Vector2.Zero && !isAttacking)
                {
                    // Idle animation
                    _animController.SetAnimation("Idle", "down"); // use down row for idle
                }

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
