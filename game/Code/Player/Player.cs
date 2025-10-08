using Assimp;
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
        private float _attackDuration = 0.25f;
        private float _attackTimer = 0f;
        private float _attackRange = 50f;
        private RectangleF _attackHitbox;
        private Vector2 _attackPosition;
        private Vector2 _lastDirection = new Vector2(0, 1);
        public Vector2 LastDirection => _lastDirection;

        // Skill1 (PillarOfLight) state
        public PillarOfLight Skill1 { get; private set; }
        private bool _isUsingSkill1 = false;
        private float _skill1Timer = 0f;
        private float _skill1Duration = 0.6f; // match PillarOfLight.Duration
        public  float Skill1Cooldown = 5f; // in seconds
        public float _skill1CooldownTimer = 0f;

        // Skill2 (ArclightCross) state
        public ArclightCross Skill2 { get; private set; }
        private bool _isUsingSkill2 = false;
        private float _skill2Timer = 0f;
        private float _skill2Duration = 0.5f; // match ArclightCross.Duration
        public float Skill2Cooldown = 5f; // in seconds
        public float _skill2CooldownTimer = 0f;


        public PlayerHurtbox Hurtbox { get; private set; }
        public PlayerCollisionBox Collision { get; private set; }

        public List<IEntity> _entities;
        internal List<PlayerAttackHitbox> _activeHitboxes = new List<PlayerAttackHitbox>();
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

            Skill1 = new PillarOfLight(this, _collisionComponent);
            Skill2 = new ArclightCross(this, _collisionComponent);

        }

        public void Update(GameTime gameTime, OrthographicCamera sceneCamera)
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
                _movement.Update(gameTime, _input.Direction, _input.DashTriggered, _animation);

                if (_movement.Direction != Vector2.Zero)
                    _lastDirection = SnapDirection(_movement.Direction);
            }

            // Handle Skill 1 (PillarOfLight)
            if (_input.Skill1Triggered && !_isUsingSkill1 && _skill1CooldownTimer <= 0f && sceneCamera != null)
            {
                StartSkill1(sceneCamera);
            }

            // If using skill, reduce timer and restore movement when done
            if (_isUsingSkill1)
            {
                _skill1Timer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_skill1Timer <= 0f)
                {
                    _isUsingSkill1 = false;
                    _movement.SetCanMove(true); // allow movement again
                }
            }

            if (_skill1CooldownTimer > 0f) // Reduce skill cooldown timers

            {
                _skill1CooldownTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_skill1CooldownTimer < 0f) _skill1CooldownTimer = 0f;
            }

            // Handle Skill 2 ArclightCross)

            if (_input.Skill2Triggered && !_isUsingSkill2 && _skill2CooldownTimer <= 0f)
            {
                StartSkill2();
            }

            if (_isUsingSkill2)
            {
                _skill2Timer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_skill2Timer <= 0f)
                {
                    _isUsingSkill2 = false;
                    _movement.SetCanMove(true);
                }
            }

            if (_skill2CooldownTimer > 0f)
            {
                _skill2CooldownTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_skill2CooldownTimer < 0f) _skill2CooldownTimer = 0f;
            }


            foreach (var hitbox in _activeHitboxes.ToList())
                hitbox.Update(gameTime);

            Hurtbox.Update(gameTime);
            Collision.Update();

            _animation.Update(gameTime, _movement.Direction, _movement.Position, _isAttacking, _movement.IsDashing);
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
            float horizontalWidth = 148;
            float horizontalHeight = 72f;
            float verticalWidth = 155f;
            float verticalHeight = 65f;

            SizeF hitboxSize = attackDir.X != 0
                ? new SizeF(horizontalWidth, horizontalHeight)
                : new SizeF(verticalWidth, verticalHeight);

            RectangleF attackBounds = new RectangleF(
                _attackPosition + attackDir * _attackRange - new Vector2(hitboxSize.Width / 2, hitboxSize.Height / 2),
                hitboxSize
            );

            var attackEntity = new PlayerAttackHitbox(this, attackBounds, _attackDuration, _collisionComponent);
            _activeHitboxes.Add(attackEntity);

            if (_entities != null)
            {
                _entities.Add(attackEntity);
                _collisionComponent.Insert(attackEntity);
            }
        }
        private void StartSkill1(OrthographicCamera sceneCamera)
        {
            _isUsingSkill1 = true;
            _skill1Timer = _skill1Duration;
            _skill1CooldownTimer = Skill1Cooldown; // start cooldown

            _movement.SetCanMove(false); // stop player from moving

            // Get mouse position in world space
            var mouseState = Microsoft.Xna.Framework.Input.Mouse.GetState();
            Vector2 mouseScreen = new Vector2(mouseState.X, mouseState.Y);
            Vector2 mouseWorldPos = ScreenToWorld(sceneCamera, mouseScreen);

            // Spawn PillarOfLight hitbox
            Skill1.Use(mouseWorldPos);
        }
        private void StartSkill2()
        {
            _isUsingSkill2 = true;
            _skill2Timer = _skill2Duration;
            _skill2CooldownTimer = Skill2Cooldown; // start cooldown

            _movement.SetCanMove(false);

            Vector2 dir = SnapDirection(_movement.Direction != Vector2.Zero ? _movement.Direction : _lastDirection);

            Skill2.Use();
        }


        public void RemoveAttackHitbox(PlayerAttackHitbox hitbox)
        {
            _activeHitboxes.Remove(hitbox);
            _entities.Remove(hitbox);
            _collisionComponent.Remove(hitbox);
        }

        private Vector2 SnapDirection(Vector2 dir)
        {
            if (dir == Vector2.Zero) return _lastDirection;

            return Math.Abs(dir.X) >= Math.Abs(dir.Y)
                ? new Vector2(Math.Sign(dir.X), 0)
                : new Vector2(0, Math.Sign(dir.Y));
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _animation.Draw(spriteBatch);
        }

        // Helper method to convert screen coordinates to world coordinates
        private static Vector2 ScreenToWorld(OrthographicCamera camera, Vector2 screenPos)
        {
            return Vector2.Transform(screenPos, Matrix.Invert(camera.GetViewMatrix()));
        }
    }
}
