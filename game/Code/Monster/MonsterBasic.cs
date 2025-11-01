using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
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
        public float AwaySpawnRadius { get; set; } = 2000f;
        public int Damage { get; set; }
        public int AttackRange { get; set; }
        public float DashForce { get; set; }
        // ----------------------------------
        public Vector2 Origin { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 DesiredPosition { get; set; }
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
        protected Texture2D HealthUI {  get; set; }
        public IMonsterState CurrentState { get; set; } = new IdleState();
        public ElementType ElementType { get; set; }
        protected Player _player { get; set; }
        protected HitParticle _hitParticle;
        protected DeadParticle _deadParticle;
        public AudioController audioController;
        public SoundEffect hitSound;
        public SoundEffect deadSound;
        public SoundEffect parrySound;
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
        public virtual bool isHit // togle I-frame state; check if monster is attacked
        {
            get => _isHit;
            set
            {
                if (value && !IsDead)
                {
                    _isHit = true;
                    ShakeViewport = true;
                    _hitTimer = 0.25f;
                    //ApplyDamage(50);
                    ApplyKnockback(250f);
                    if (HP > 0)
                    {
                        var r = new Random();
                        var pitch = r.NextSingle(0.75f);
                        audioController.PlaySoundEffect(hitSound, 1, pitch, 0, false);
                        _hitParticle.Trigger(Position, -DirectionToPlayer);
                    }
                    else if (HP <= 0)
                    {
                        _deadTimer = 1.2f;
                        _hitTimer = 5f;
                    }
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
                else
                {
                    _attackCD = 1f;
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
        public void LoadUI(ContentManager content, string name)
        {
            HealthUI = content.Load<Texture2D>("Texture/" + name);
        }

        public virtual void LoadSound(AudioController controller, SoundEffect hitSfx, SoundEffect deadSfx)
        {
            audioController = controller;
            hitSound = hitSfx;
            deadSound = deadSfx;
        }

        private float _HPScale = 1;
        private float _followUpUI = 1;
        private float _frameCount = 0;

        public void UpdateUI()
        {
            // UI เลือด
            var percent = (float)HP / (float)MAXHP; // เปอร์เซ็นเลือด
            if (_HPScale < percent - 0.05)
            {
                _HPScale += 0.025f;
            }
            else if (_HPScale > percent + 0.05)
            {
                _HPScale -= 0.025f;
            }
            else
            {
                if (_HPScale != percent)
                {
                    _HPScale = percent;
                    _frameCount = 0;
                }
                if (_frameCount >= 30)
                {
                    if (_followUpUI < _HPScale - 0.05)
                    {
                        _followUpUI += 0.025f;
                    }
                    else if (_followUpUI > _HPScale + 0.05)
                    {
                        _followUpUI -= 0.025f;
                    }
                    else
                    {
                        _followUpUI = _HPScale;
                        _frameCount = 0;
                    }
                }
                _frameCount += 1;
            }
        }
        public void DrawUI(SpriteBatch spriteBatch)
        {
            var scale = new Vector2(1f, 1);
            var offset = new Vector2(HealthUI.Width / 2 * scale.X, Height / 1.5f);
            spriteBatch.Draw(HealthUI, Position - offset, new Rectangle(0, HealthUI.Height / 2, (int)(HealthUI.Width * _followUpUI), HealthUI.Height / 2), Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
            spriteBatch.Draw(HealthUI, Position - offset, new Rectangle(0, HealthUI.Height / 2, (int)(HealthUI.Width * _HPScale), HealthUI.Height / 2), Color.Crimson, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
            spriteBatch.Draw(HealthUI, Position - offset, new Rectangle(0, 0, HealthUI.Width, HealthUI.Height / 2), Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
        }
        /*
         IMPORTANT NOTE: Need to change in future
         Based on the animation sprite sheet
        */
        public virtual void MoveTo(float deltaTime, Vector2 position)
        {
            Vector2 direction = position - Position;
            direction.Normalize();
            if (Speed == 0) Speed = 1f;
            Vector2 movement = direction * Speed * deltaTime;
            DesiredPosition = Position + movement;
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
                DesiredPosition += _knockBackDirection * _knockBackForce * deltaTime;
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
        public void ApplyKnockback(float knockbackForce, Vector2 knockbackDirection, float knockbackTimer)
        {
            if (knockbackDirection.LengthSquared() == 0)
            {
                return;
            }
            knockbackDirection.Normalize();
            _knockBackTimer = knockbackTimer;
            _knockBackDirection = knockbackDirection;
            _knockBackForce = knockbackForce;
        }
        public void DropHeal(Texture2D texture, Player player, CollisionComponent collisionComponent, Vector2 position, List<IEntity> scenePendingRemove, int healAmount = 25)
        {
            Random r = new Random();
            if (r.Next(1, 101) <= 100) // 100% chance, tweak if needed
            {
                var healPickup = new HealPickup(position, texture, player, collisionComponent, scenePendingRemove, healAmount);

                // Add to entity list so it can be drawn/updated
                player._entities.Add(healPickup);

                // ❌ Do NOT call collisionComponent.Insert here (already done in HealPickup)
                Debug.WriteLine("HealPickup spawned at: " + position);
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
        public void ApplyDamage(int Value)
        {
            if (_player.CurrentElement == ElementType)
            {
                Value /= 2; // ลดลง 50%
                //Debug.WriteLine("same element");
            }
            else
            {
                Value *= 2; 
                //Debug.WriteLine("dif element");
            }
            HP -= Value;
            Debug.WriteLine($"Monster took {Value} damage (after element modifier). Remaining HP: {HP}");
        }
        public void PlayeParrySound(MonsterAttackHitbox hitbox)
        {
            if (hitbox != null)
            {
                if (hitbox.PlaySound && parrySound != null)
                {
                    audioController.PlaySoundEffect(parrySound);
                    hitbox.PlaySound = false;
                } 
            }
        }
        public virtual void ChangeState(IMonsterState newState) { }
        public virtual void Attack() { }
    }
}
