using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace game
{
    public class Player
    {
        private PlayerStats _stats;
        private PlayerInput _input;
        private PlayerAnimation _animation;
        public PlayerMovement _movement;

        // Attack properties
        private float _attackRange = 50f;
        private bool _isAttacking = false;
        private float _attackDuration = 0.2f;
        private float _attackTimer = 0f;
        private RectangleF _attackHitbox;
        private Vector2 _attackPosition;

        // Hurtbox
        public PlayerHurtbox Hurtbox { get; private set; }

        // Track last movement direction for attack facing
        private Vector2 _lastDirection = new Vector2(0, 1); // default down

        // Expose stats
        public PlayerStats Stats => _stats;

        // World references for collisions and entities
        private List<IEntity> _entities;
        private CollisionComponent _collisionComponent;

        public Player(AnimController texture, Vector2 startPosition)
        {
            _stats = new PlayerStats();
            _input = new PlayerInput();
            _movement = new PlayerMovement(startPosition, _stats);
            _animation = new PlayerAnimation(texture);

            // Initialize hurtbox
            Hurtbox = new PlayerHurtbox(this, 64, 96);
        }

        // Call this from your Game class after creating player
        public void SetWorldReferences(List<IEntity> entities, CollisionComponent collisionComponent)
        {
            _entities = entities;
            _collisionComponent = collisionComponent;
        }

        public void Update(GameTime gameTime, List<IEntity> attackTargets)
        {
            _input.Update(gameTime);

            // Handle attack input
            if (_input.AttackTriggered && !_isAttacking && !_movement.IsDashing)
                StartAttack();

            if (_isAttacking)
            {
                _attackTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                _movement.SetPosition(_attackPosition);
                CheckAttackHit(attackTargets);

                if (_attackTimer <= 0f)
                {
                    _isAttacking = false;
                    _movement.SetCanMove(true);
                }
            }
            else
            {
                _movement.Update(gameTime, _input.Direction, _input.DashTriggered);

                if (_movement.Direction != Vector2.Zero)
                    _lastDirection = _movement.Direction;

                if (attackTargets != null)
                {
                    foreach (var target in attackTargets)
                    {
                        if (target is MonsterHurtbox monster)
                            monster.Monster.isHit = false;
                    }
                }
            }

            // Collect heal pickups
            if (_entities != null && _collisionComponent != null)
            {
                foreach (var heal in _entities.OfType<HealPickup>().ToList())
                {
                    if (heal.Bounds.Intersects(Hurtbox.Bounds))
                    {
                        heal.OnCollected();
                        _entities.Remove(heal);
                        _collisionComponent.Remove(heal);
                    }
                }

            }

            Hurtbox.Update();
            _animation.Update(gameTime, _movement.Direction, _movement.Position, _isAttacking);
        }

        private void StartAttack()
        {
            _isAttacking = true;
            _attackTimer = _attackDuration;
            _movement.SetCanMove(false);
            _attackPosition = _movement.Position;
            _animation.TriggerAttack();

            Vector2 attackDir = _movement.Direction != Vector2.Zero ? _movement.Direction : _lastDirection;
            if (attackDir != Vector2.Zero) attackDir.Normalize();
            Vector2 attackOffset = attackDir * _attackRange;

            _attackHitbox = new RectangleF(
                _attackPosition + attackOffset - new Vector2(_attackRange / 2, _attackRange / 2),
                new SizeF(_attackRange, _attackRange)
            );
        }

        private void CheckAttackHit(List<IEntity> attackTargets)
        {
            if (attackTargets == null || attackTargets.Count == 0) return;

            var playerAttack = new PlayerAttack(_attackHitbox);

            foreach (var target in attackTargets)
            {
                if (target is MonsterHurtbox monsterHurtbox)
                {
                    if (playerAttack.Bounds.Intersects(monsterHurtbox.Bounds) &&
                        !monsterHurtbox.Monster.isHit)
                    {
                        monsterHurtbox.Monster.HP -= _stats.AttackDamage.Value;
                        monsterHurtbox.Monster.isHit = true;
                        Debug.WriteLine($"Hit monster! Remaining HP: {monsterHurtbox.Monster.HP}");
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _animation.Draw(spriteBatch);

            if (_isAttacking)
                spriteBatch.DrawRectangle(_attackHitbox, Color.Red, 2);

            Hurtbox.Draw(spriteBatch);
        }
    }
}
