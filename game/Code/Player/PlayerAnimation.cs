using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Animations;
using System;

namespace game
{
    public class PlayerAnimation
    {
        private AnimController _animController;
        private int _row = 3;
        private Vector2 _lastDirection = new Vector2(0, 1);

        public float TextureWidth { get; private set; }
        public float TextureHeight { get; private set; }

        public PlayerAnimation(AnimController animController)
        {
            _animController = animController;
        }

        public void Update(GameTime gameTime, Vector2 direction, Vector2 position, bool isAttacking)
        {
            if (direction != Vector2.Zero) _lastDirection = direction;

            if (Math.Abs(_lastDirection.X) >= Math.Abs(_lastDirection.Y))
                _row = _lastDirection.X < 0 ? 1 : 2;
            else
                _row = _lastDirection.Y < 0 ? 4 : 3;

            string dirName = _row switch { 1 => "left", 2 => "right", 3 => "down", 4 => "up", _ => "down" };

            if (direction != Vector2.Zero && !isAttacking)
                _animController.SetAnimation("Walk", dirName);
            else if (!isAttacking)
                _animController.SetAnimation("Idle", dirName);

            _animController.UpdateFrame(gameTime, position);

            TextureWidth = _animController.TextureWidth;
            TextureHeight = _animController.TextureHeight;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _animController.DrawFrame(spriteBatch);
        }

        public void TriggerAttack()
        {
            _animController.SetAnimation("Walk", _row switch { 1 => "left", 2 => "right", 3 => "down", 4 => "up", _ => "down" });
        }
    }
}
