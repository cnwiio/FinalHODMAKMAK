
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using SharpDX;
using System.Collections.Generic;
using System.Diagnostics;

namespace game
{
    public class Player
    {
        private PlayerStats _stats;
        private PlayerInput _input;
        public PlayerMovement _movement;
        private PlayerAnimation _animation;

        // Attack properties
        private float _attackRange = 50f;      // Radius or size of attack
        private bool _isAttacking = false;
        private float _attackDuration = 0.2f;  // How long the attack hitbox stays active
        private float _attackTimer = 0f;
        private RectangleF _attackHitbox;

        // Track last movement direction for attack facing
        private Vector2 _lastDirection = new Vector2(0, 1); // default down


        public PlayerStats Stats => _stats;

        private List<IEntity> _attackTargets;


        public Player(AnimController texture, Vector2 startPosition/*, List<IEntity> attackTargets*/)
        {
            _stats = new PlayerStats();
            _input = new PlayerInput();
            _movement = new PlayerMovement(startPosition, _stats);
            _animation = new PlayerAnimation(texture);
            //_attackTargets = attackTargets;
        }

        public void Update(GameTime gameTime)
        {
            _input.Update(gameTime);
            _movement.Update(gameTime, _input.Direction, _input.DashTriggered);
            _animation.Update(gameTime, _movement.Direction, _movement.Position);

            // Update last direction if moving
            if (_movement.Direction != Vector2.Zero)
                _lastDirection = _movement.Direction;

            // Handle attack logic
            if (Mouse.GetState().LeftButton == ButtonState.Pressed && !_isAttacking)
            {
                StartAttack();
            }

            if (_isAttacking)
            {
                _attackTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_attackTimer <= 0f)
                {
                    _isAttacking = false;
                }
                else
                {
                    CheckAttackHit();
                }
            }
        }
        private void StartAttack()
        {
            _isAttacking = true;
            _attackTimer = _attackDuration;

            // Trigger animation (if using AnimController)
            _animation.TriggerAttack();

            Vector2 attackDir = _movement.Direction != Vector2.Zero ? _movement.Direction : _lastDirection; // Use last movement direction if idle
            if (attackDir != Vector2.Zero) attackDir.Normalize(); // Normalize to make diagonal attacks same distance as straight ones
            Vector2 attackOffset = attackDir * _attackRange;

            _attackHitbox = new RectangleF(_movement.Position + attackOffset - new Vector2(_attackRange / 2, _attackRange / 2), new SizeF(_attackRange, _attackRange));
        }
        private void CheckAttackHit()
        {
            //foreach (var target in _attackTargets)
            //{
            //    if (target is BoxCollision box && _attackHitbox.Intersects(box.Bounds))
            //    {
            //        // Apply damage here
            //        Debug.WriteLine("Hit enemy!");
            //    }
            //}
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw player animation
            _animation.Draw(spriteBatch);

            // Debug: draw attack hitbox
            if (_isAttacking)
            {
                spriteBatch.DrawRectangle(_attackHitbox, Color.Red, 2); // requires MonoGame.Extended
            }
        }
    }
}
