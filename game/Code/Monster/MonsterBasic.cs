using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;

namespace game
{
    public class MonsterBasic
    {
        // -------------Property-------------
        public float Speed { get; set; }
        public float SreachRadius { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public float ActiveRadius { get; set; } = 150f;
        public float AwaySpawnRadius { get; set; } = 500f;
        public int Damage { get; set; }
        public int AttackRange { get; set; }
        public float DashForce { get; set; }
        // ----------------------------------
        public Vector2 Origin { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 TargetPos { get; set; }
        public Vector2 SpawnPosition { get; set; }
        public Vector2 DirectionToPlayer { get; set; }
        public float WanderTimer { get; set; } = 0f;
        protected const float _blinkInterval = 0.1f;
        protected float _blinkTimer = 0f;
        protected float _knockBackTimer = 0f, _knockBackForce = 0f;
        protected Vector2 _knockBackDirection = Vector2.Zero;
        protected List<IEntity> _collisions;
        protected CollisionComponent _collisionComponents;
        public MonsterAttackHitbox Hitbox;
        public IEntity HurtBox { get; set; }
        public IEntity Collision { get; set; }
        public PreventMonster PreventMonster;
        public AnimController animation {  get; set; }
        public IMonsterState CurrentState { get; set; } = new IdleState();
        public Element ElementType { get; set; }
        protected Player _player { get; set; }
        protected Particle _particle;
        // ----------------Bool----------------
        public bool ShakeViewport = false;
        public bool WaitingToReturn { get; set; } = false;
        public bool IsReturning { get; set; } = false;
        public float preventMonsterEdge { get; set; }
        public bool isInAttackList { get; set; }
        public bool isAwayHome { get; set; }
        public bool isInRange { get; set; }
        public bool isInAttack { get; set; }
        public bool isInWander { get; set; }
        public bool isInActiveRadius { get; set; }
        public bool IgnorePlayer { get; set; } = false;
        protected bool _isHit;
        protected float _hitTimer = 0f;
        protected float _deadTimer = 0f;
        public bool isHit // togle I-frame state; check if monster is attacked
        {
            get => _isHit;
            set
            {
                if (value)
                {
                    _isHit = true;
                    ShakeViewport = true;
                    _hitTimer = 1f;
                    _deadTimer = 1.2f;
                    ApplyDamage();
                    ApplyKnockback(250f);
                    _particle.Trigger(Position, -DirectionToPlayer);
                }
            }
        }
        protected bool _isAttack;
        protected float _attackCD;
        public bool isAttack // togle attack state; check if monster is attacking
        {
            get => _isAttack;
            set
            {
                if (value)
                {
                    _isAttack = true;
                }
            }
        }
        public bool IsDead {  get; set; }
        public int MAXHP { get; set; }
        protected int _HP;
        public virtual int HP { get; set; }
        // ----------------------------------
        public void LoadAnim(string spriteSheetName, string textureName, Vector2 position, int width, int height, ContentManager content)
        {
            if (Width == 0 || Height == 0)
            {
                Width = width;
                Height = height;
            }
            if (animation == null)
            {
                animation = new AnimController(position);
            }
            animation.LoadFrame(content, spriteSheetName, textureName, width, height);
        }
        /*
         IMPORTANT NOTE: Need to change in future
         Based on the animation sprite sheet
        */
        public void MoveTo(float deltaTime, Vector2 position)
        {
            Vector2 direction = position - Position;
            direction.Normalize();
            if (Speed == 0) Speed = 1f;
            Vector2 movement = direction * Speed * deltaTime;
            Position += movement;
            animation.SetAnimation("Walk", GetDirection(direction));
        }

        public string GetDirection(Vector2 direction)
        {
            if (direction.LengthSquared() == 0)
                return null;


            if (direction.X >= 0)
                return "right";
            else if (direction.X < 0)
                return "left";
            else
                return "right";
        }
        public void UpdateHitTimer(float deltaTime)
        {
            if (_isHit)
            {
                _hitTimer -= deltaTime;
                _blinkTimer += deltaTime;
                if (_blinkTimer >= _blinkInterval * 2) _blinkTimer = 0f;
                if (_hitTimer <= 0f)
                {
                    _isHit = false;
                    _hitTimer = 0f;
                    _blinkTimer = 0f;
                }
            }
            if (_attackCD > 0f)
            {
                _attackCD -= deltaTime;
                if (_attackCD <= 0f)
                {
                    _isAttack = false;
                    _attackCD = 0f;
                }
            }
            if (IsKnockBack())
            {
                _knockBackTimer -= deltaTime;
                _knockBackForce *= MathF.Pow(0.1f, deltaTime);
                Position += _knockBackDirection * _knockBackForce * deltaTime;
                //Debug.WriteLine($"Knockback Force:" + _knockBackDirection * _knockBackForce * deltaTime
                //    + "\n DeltaTime : " + deltaTime + "\n Force : " + _knockBackForce + "\n Direction : " + _knockBackDirection);
            }

            if (WaitingToReturn)
            {

                if (CurrentState is IdleState)
                {

                    WanderTimer -= deltaTime;
                    if (WanderTimer <= 0f)
                    {
                        IsReturning = true;
                        ChangeState(new ReturnState());
                    }
                }
                else
                {
                    WaitingToReturn = false;
                }
            }

            if (_deadTimer > 0f)
            {
                _deadTimer -= deltaTime;
                if (_deadTimer <= 0f)
                {
                    _deadTimer = 0f;
                }
            }
        }
        private Vector2 _placeHolderDirection;
        public bool IsKnockBack()
        {
            return _knockBackTimer > 0f && _knockBackForce > 0.01f;
        }
        public void ApplyKnockback(float knockbackForce)
        {
            var knockbackDirection = -(TargetPos - Position);
            if (knockbackDirection.LengthSquared() == 0)
            {
                return;
            }
            knockbackDirection.Normalize();
            _knockBackTimer = 0.4f;
            _knockBackDirection = knockbackDirection;
            _knockBackForce = knockbackForce;
        }
        public void ApplyKnockback(float knockbackForce, Vector2 knockbackDirection)
        {
            if (knockbackDirection.LengthSquared() == 0)
            {
                return;
            }
            knockbackDirection.Normalize();
            _knockBackTimer = 0.7f;
            _knockBackDirection = knockbackDirection;
            _knockBackForce = knockbackForce;
        }
        public void DropHeal(List<IEntity> entities, CollisionComponent collisionComponent, Texture2D texture, Player player)
        {
            Random r = new Random();
            if (r.Next(1, 101) <= 75) // Percentage, Ex: 75 mean 75%
            {
                entities.Add(new HealPickup(
                                animation.Position,
                                texture,
                                player
                            )); // Add drops
                collisionComponent.Insert(entities.Last());
            }
        }
        public void Return(float deltaTime)
        {
            MoveTo(deltaTime, SpawnPosition);
            if (Vector2.Distance(Position, SpawnPosition) <= Width)
            {
                IsReturning = false;
                IgnorePlayer = false;
                Reset();
                ChangeState(new IdleState());
            }
        }
        public void Reset()
        {
            HP = MAXHP;
        }
        public void ApplyDamage()
        {
            var Damage = _player.Stats.AttackDamage.Value;
            HP -= Damage;
            //Debug.WriteLine(Damage);
        }
        public virtual void ChangeState(IMonsterState newState) { }
        public virtual void Attack() { }
    }
}
