using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace game
{
    public partial class Player : IYsort
    {
        private PlayerStats _stats;
        private PlayerInput _input;
        private PlayerAnimation _animation;
        public PlayerMovement _movement;

        private bool _isAttacking = false;
        private float _attackDuration = 0.2f;
        private float _attackTimer = 0f;
        private float _attackRange = 50f;
        private RectangleF _attackHitbox;
        private Vector2 _attackPosition;
        private Vector2 _lastDirection = new Vector2(0, 1);

        public PlayerHurtbox Hurtbox { get; private set; }
        public PlayerCollisionBox Collision { get; private set; }

        private List<IEntity> _entities;
        private CollisionComponent _collisionComponent;

        public PlayerStats Stats => _stats;
        public float SortY => _movement.Position.Y + 48;

        public Player(AnimController texture, Vector2 startPosition)
        {
            _stats = new PlayerStats();
            _input = new PlayerInput();
            _movement = new PlayerMovement(startPosition, _stats);
            _animation = new PlayerAnimation(texture);

            Hurtbox = new PlayerHurtbox(this, 64, 96);

            // Manual collision size and offset
            Vector2 collisionSize = new Vector2(40, 21); // width, height
            Vector2 collisionOffset = new Vector2(-20, 35); // offset from top-left of sprite
            Collision = new PlayerCollisionBox(this, collisionSize, collisionOffset);
        }

        public void SetWorldReferences(List<IEntity> entities, CollisionComponent collisionComponent)
        {
            _entities = entities ?? new List<IEntity>();
            _collisionComponent = collisionComponent;

            if (!_entities.Contains(Hurtbox)) _entities.Add(Hurtbox);
            _collisionComponent?.Insert(Hurtbox);

            if (!_entities.Contains(Collision)) _entities.Add(Collision);
            _collisionComponent?.Insert(Collision);
        }

        public void Update(GameTime gameTime, List<IEntity> attackTargets)
        {
            _input.Update(gameTime);

            // Handle Element Toggle
            if (_input.ElementToggleTriggered)
                ToggleElement();

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
                    _lastDirection = SnapDirection(_movement.Direction);

                attackTargets?.OfType<MonsterHurtbox>().ToList().ForEach(m => m.Monster.isHit = false);
            }

            Hurtbox.Update();
            Collision.Update();

            _animation.Update(gameTime, _movement.Direction, _movement.Position, _isAttacking);
        }

        private void StartAttack()
        {
            _isAttacking = true;
            _attackTimer = _attackDuration;
            _movement.SetCanMove(false);
            _attackPosition = _movement.Position;
            _animation.TriggerAttack();

            Vector2 attackDir = SnapDirection(_movement.Direction != Vector2.Zero ? _movement.Direction : _lastDirection);

            // rectangle shape logic as before
            SizeF hitboxSize = (attackDir.X != 0)
                ? new SizeF(70, 110)
                : new SizeF(110, 70);

            _attackHitbox = new RectangleF(
                _attackPosition + attackDir * _attackRange - new Vector2(hitboxSize.Width / 2, hitboxSize.Height / 2),
                hitboxSize
            );

            // Insert into collision system
            var attackEntity = new PlayerAttackHitbox(this, _attackHitbox);
            _entities.Add(attackEntity);
            _collisionComponent?.Insert(attackEntity);
        }



        private Vector2 SnapDirection(Vector2 dir)
        {
            if (dir == Vector2.Zero) return _lastDirection;

            return Math.Abs(dir.X) >= Math.Abs(dir.Y)
                ? new Vector2(Math.Sign(dir.X), 0)   // Left or Right
                : new Vector2(0, Math.Sign(dir.Y)); // Up or Down
        }

        private void CheckAttackHit(List<IEntity> attackTargets)
        {
            if (attackTargets == null) return;
            var playerAttack = new PlayerAttack(_attackHitbox);

            foreach (var target in attackTargets.OfType<MonsterHurtbox>())
            {
                if (!target.Monster.isHit && playerAttack.Bounds.Intersects(target.Bounds) && target.Monster.HP > 0)
                {
                    target.Monster.HP -= _stats.AttackDamage.Value;
                    target.Monster.isHit = true;
                    Debug.WriteLine($"Hit monster! Remaining HP: {target.Monster.HP}");
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _animation.Draw(spriteBatch);

            if (_isAttacking) spriteBatch.DrawRectangle(_attackHitbox, Color.Red, 2);
            Hurtbox.Draw(spriteBatch);
            Collision.Draw(spriteBatch); // Yellow debug box
        }
    }
}
