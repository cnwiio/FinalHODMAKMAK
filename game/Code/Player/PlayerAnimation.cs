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

        public bool _isPlayingElementToggle = false;
        public bool _isFainting = false;
        public bool IsPlayingElementToggle => _isPlayingElementToggle;
        private double _elementToggleTimer = 0;
        public bool IsFainting => _isFainting;

        public float TextureWidth { get; private set; }
        public float TextureHeight { get; private set; }

        public PlayerAnimation(AnimController animController)
        {
            _animController = animController;
        }

        public void Update(GameTime gameTime, Vector2 direction, Vector2 position, bool isAttacking, bool isDashing)
        {
            // Faint animation overrides everything
            if (_isFainting)
            {
                _animController.UpdateFrame(gameTime, position);
                TextureWidth = _animController.TextureWidth;
                TextureHeight = _animController.TextureHeight;
                return;
            }

            if (direction != Vector2.Zero) _lastDirection = direction;

            if (Math.Abs(_lastDirection.X) >= Math.Abs(_lastDirection.Y))
                _row = _lastDirection.X < 0 ? 1 : 2;
            else
                _row = _lastDirection.Y < 0 ? 4 : 3;

            string dirName = _row switch { 1 => "left", 2 => "right", 3 => "down", 4 => "up", _ => "down" };

            // Update Elemental Toggle Timer
            if (_isPlayingElementToggle)
            {
                _elementToggleTimer += gameTime.ElapsedGameTime.TotalMilliseconds;
                if (_elementToggleTimer >= 400) // total animation length
                {
                    _isPlayingElementToggle = false;
                }
            }

            // Only play walk/idle if element toggle is not playing
            if (!_isPlayingElementToggle)
            {
                if (direction != Vector2.Zero && !isAttacking && !isDashing)
                    _animController.SetAnimation("Walk", dirName);
                else if (!isAttacking && !isDashing)
                    _animController.SetAnimation("Idle", dirName);
            }

            _animController.UpdateFrame(gameTime, position);

            TextureWidth = _animController.TextureWidth;
            TextureHeight = _animController.TextureHeight;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _animController.DrawFrame(spriteBatch);
        }
        public void Draw(SpriteBatch spriteBatch, Color tint)
        {
            _animController.DrawFrame(spriteBatch, false, tint);
        }
        public void PlayElementToggleAnimation(ElementType element)
        {
            _isPlayingElementToggle = true;
            _elementToggleTimer = 0; // reset timer

            string animName = element == ElementType.Light ? "ElementalShiftDark" : "ElementalShiftLight";
            _animController.SetAnimation("ElementalShift", animName);
        }

        public void TriggerAttack(ElementType ele)
        {
            string elt = ele == ElementType.Light ? "Light" : "Dark";
            _animController.SetAnimation("Attack" + elt, _row switch { 1 => "left", 2 => "right", 3 => "down", 4 => "up", _ => "down" });
        }

        public void Dashing(ElementType ele)
        {
            string elt = ele == ElementType.Light ? "Light" : "Dark";
            _animController.SetAnimation("Dash" + elt, _row switch { 1 => "left", 2 => "right", 3 => "down", 4 => "up", _ => "down" });
        }
        public void TriggerFaint()
        {
            _isFainting = true;
            _animController.SetAnimation("Faint", "no"); // "no" is the row for non-directional faint animation
        }
        public void StopFaint() => _isFainting = false; // optional in case you wanna recover)
    }
}
