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

        public List<IEntity> _entities;
        private List<PlayerAttackHitbox> _activeHitboxes = new List<PlayerAttackHitbox>();
        public CollisionComponent _collisionComponent;

        public PlayerStats Stats => _stats;
        public float SortY => _movement.Position.Y + 48;
        public float SortX => _movement.Position.X;
        public Vector2 DestinationPos { get; set; }
        public string CurrentScene { get; set; }
        public Potion potion { get; set; }

        public Player(AnimController texture, Vector2 startPosition)
        {
            _stats = new PlayerStats();
            _input = new PlayerInput();
            _movement = new PlayerMovement(startPosition, _stats);
            _animation = new PlayerAnimation(texture);

            Hurtbox = new PlayerHurtbox(this, 48, 72);

            // Manual collision size and offset
            Vector2 collisionSize = new Vector2(40, 27); // width, height
            Vector2 collisionOffset = new Vector2(-20, 26); // offset from top-left of sprite
            Collision = new PlayerCollisionBox(this, collisionSize, collisionOffset);

            // potion
            potion = new Potion(this);
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
        public void Update(GameTime gameTime)
        {
            _input.Update(gameTime);

            // Handle Element Toggle
            if (_input.ElementToggleTriggered)
                ToggleElement();

            if (_input.AttackTriggered && !_isAttacking && !_movement.IsDashing)
                StartAttack();

            if (_input.PotionTriggered && !_isAttacking && !_movement.IsDashing)
                potion.Use();

            if (_isAttacking)
            {
                _attackTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                //_movement.SetPosition(_attackPosition);
                //CheckAttackHit(attackTargets); // ไม่ต้องเช็คเองแล้ว เพราะไปใช้ของ Extended

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

                //attackTargets?.OfType<MonsterHurtbox>().ToList().ForEach(m => m.Monster.isHit = false); // ไม่ต้องใช้แล้ว
            }

            foreach (var hitbox in _activeHitboxes.ToList())
                hitbox.Update(gameTime);

            Hurtbox.Update(gameTime);
            Collision.Update();

            _animation.Update(gameTime, _movement.Direction, _movement.Position, _isAttacking);
            potion.Update(gameTime); // เอาไว้อัพเดท คูลดาว
        }

        private void StartAttack()
        {
            _isAttacking = true;
            _attackTimer = _attackDuration;
            _movement.SetCanMove(false);
            _attackPosition = _movement.Position;
            _animation.TriggerAttack();

            Vector2 attackDir = SnapDirection(_movement.Direction != Vector2.Zero ? _movement.Direction : _lastDirection);

            // Hitbox size
            float horizontalWidth = 70f;
            float horizontalHeight = 110f;
            float verticalWidth = 110f;
            float verticalHeight = 70f;

            SizeF hitboxSize = attackDir.X != 0
                ? new SizeF(horizontalWidth, horizontalHeight)
                : new SizeF(verticalWidth, verticalHeight);

            RectangleF attackBounds = new RectangleF(
                _attackPosition + attackDir * _attackRange - new Vector2(hitboxSize.Width / 2, hitboxSize.Height / 2),
                hitboxSize
            );

            var attackEntity = new PlayerAttackHitbox(this, attackBounds, _attackDuration, _collisionComponent); 
            // ให้เพิ่มเข้า List แค่ตรงนี้ เพราะจะได้เรียกแค่ที่เดียว
            _activeHitboxes.Add(attackEntity);
            if (_entities != null)
            {
                _entities.Add(attackEntity); // insert to entities list for update/draw
                _collisionComponent.Insert(attackEntity);
            }
        }


        public void RemoveAttackHitbox(PlayerAttackHitbox hitbox)
        {
            // ให้มันลบตรงนี้ที่เดียว จะได้ไม่ต้องไปปรับที่อื่น
            _activeHitboxes.Remove(hitbox);
            _entities.Remove(hitbox);
            _collisionComponent.Remove(hitbox);
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

            //// Draw active attack hitboxes (for debugging)
            //foreach (var hitbox in _activeHitboxes)
            //    hitbox.Draw(spriteBatch);

            //Hurtbox.Draw(spriteBatch);
            //Collision.Draw(spriteBatch); // Yellow debug box
        }
    }
}
