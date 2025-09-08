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

        // Hurtbox
        public PlayerHurtbox Hurtbox { get; private set; }

        // Track last movement direction for attack facing
        private Vector2 _lastDirection = new Vector2(0, 1); // default down

        // Expose stats
        public PlayerStats Stats => _stats;

        // Temporary list to store attack targets during update
        private List<IEntity> _attackTargets;

        public Player(AnimController texture, Vector2 startPosition)
        {
            _stats = new PlayerStats();
            _input = new PlayerInput();
            _movement = new PlayerMovement(startPosition, _stats);
            _animation = new PlayerAnimation(texture);

            // Initialize hurtbox (size matches player)
            Hurtbox = new PlayerHurtbox(this, 64, 96);
        }

        public void Update(GameTime gameTime, List<IEntity> attackTargets)
        {
            // Update input and movement
            _input.Update(gameTime);
            _movement.Update(gameTime, _input.Direction, _input.DashTriggered);

            // Update last direction if moving
            if (_movement.Direction != Vector2.Zero)
                _lastDirection = _movement.Direction;

            // Update hurtbox position
            Hurtbox.Update();

            // Reset isHit on monsters if attack is not active
            if (!_isAttacking && attackTargets != null)
            {
                foreach (var target in attackTargets)
                {
                    if (target is MonsterHurtbox monster)
                        monster.MonsterMelee.isHit = false; // ready to be hit again
                }
            }

            // Handle attack input
            if (_input.AttackTriggered && !_isAttacking)
            {
                StartAttack();
            }

            // Update attack timer
            if (_isAttacking)
            {
                _attackTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (_attackTimer <= 0f)
                    _isAttacking = false;
                else
                    CheckAttackHit(attackTargets);
            }

            // Update animation
            _animation.Update(gameTime, _movement.Direction, _movement.Position, _isAttacking);
        }

        private void StartAttack()
        {
            _isAttacking = true;
            _attackTimer = _attackDuration;

            // Trigger animation (if using AnimController)
            _animation.TriggerAttack();

            // Determine attack direction
            Vector2 attackDir = _movement.Direction != Vector2.Zero ? _movement.Direction : _lastDirection;

            if (attackDir != Vector2.Zero)
                attackDir.Normalize(); // Normalize diagonal attacks

            Vector2 attackOffset = attackDir * _attackRange;

            _attackHitbox = new RectangleF(
                _movement.Position + attackOffset - new Vector2(_attackRange / 2, _attackRange / 2),
                new SizeF(_attackRange, _attackRange)
            );
        }

        private void CheckAttackHit(List<IEntity> attackTargets)
        {
            if (attackTargets == null || attackTargets.Count == 0)
                return;

            // Create temporary hitbox entity for collision checks
            var playerAttack = new PlayerAttack(_attackHitbox);

            foreach (var target in attackTargets)
            {
                if (target is MonsterHurtbox monsterHurtbox)
                {
                    if (playerAttack.Bounds.Intersects(monsterHurtbox.Bounds))
                    {
                        if (!monsterHurtbox.MonsterMelee.isHit)
                        {
                            // Apply damage
                            monsterHurtbox.MonsterMelee.HP -= _stats.AttackDamage.Value;

                            // Mark monster as hit
                            monsterHurtbox.MonsterMelee.isHit = true;

                            // Debug
                            Debug.WriteLine($"Hit monster! Remaining HP: {monsterHurtbox.MonsterMelee.HP}");
                        }
                    }
                }
            }
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

            // Debug: draw hurtbox
            Hurtbox.Draw(spriteBatch);
        }
    }
}