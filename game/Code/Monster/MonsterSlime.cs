using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Animations;
using MonoGame.Extended.Collections;
using MonoGame.Extended.Collisions;

namespace game
{
    public class MonsterSlime : MonsterBasic, IMonster, IYsort
    {
        // -------------Property-------------
        public float SortY { get => Position.Y + Height / 2; }
        public float SortX { get => Position.X; }
        // ----------------------------------
        public override int HP
        {
            get => _HP;
            set
            {
                _HP = value;
                if (_HP <= 0)
                {
                    if (!IsDead)
                    {
                        _HP = 0;
                        _hitTimer += 5f;
                        _deadParticle.Trigger(Position, -Vector2.UnitY, (float)Math.PI);
                        audioController.PlaySoundEffect(deadSound);
                        if (_placeHolderDirection == Vector2.Zero) _placeHolderDirection = DirectionToPlayer;
                        animation.SetAnimation("Die", GetDirection(_placeHolderDirection), OnAnimationEvent);
                    }
                }
            }
        }
        public MonsterSlime(Vector2 position, PreventMonster preventMonster, Player player, HitParticle particle, DeadParticle deadParticle, ElementType element)
        {
            Position = position;
            DesiredPosition = position;
            SpawnPosition = position;
            PreventMonster = preventMonster;
            _player = player;
            _hitParticle = particle;
            _deadParticle = deadParticle;
            ElementType = element;
        }
        /*
         IMPORTANT NOTE: Need to change in future
         Based on the animation sprite sheet
        */
        public void CreateAnimation()
        {
            animation.CreateAnimation("Idle", "right", true, 200, 0, 4);
            animation.CreateAnimation("Idle", "left", true, 200, 0, 4);

            animation.CreateAnimation("Walk", "left", false, 150, 0, 9);
            animation.CreateAnimation("Walk", "right", false, 150, 0, 9);

            animation.CreateAnimation("Charge", "right", false, 200, 0, 3);
            animation.CreateAnimation("Charge", "left", false, 200, 0, 3);

            animation.CreateAnimation("Attack", "left", false, 150, 0, 9);
            animation.CreateAnimation("Attack", "right", false, 150, 0, 9);

            animation.CreateAnimation("Die", "right", false, 100, 0, 8);
            animation.CreateAnimation("Die", "left", false, 100, 0, 8);
        }
        // Need Change in future
        public void SetProperty(float speed, float sreachRadius, int hp, int damage, int attackRange, int activeRadius, float dashForce)
        {
            SetProperty(
                speed,
                sreachRadius,
                new MonsterHurtbox(
                    animation.AnimSprite["Idle"].GetBoundingRectangle(new Transform2(animation.Position, 0f, new Vector2(1, 1))),
                    this),
                new MonsterCollision(
                    new RectangleF(0, 0, 20, 20),
                    this),
                hp,
                damage,
                attackRange,
                activeRadius,
                dashForce
                );
        }
        public void SetProperty(float speed, float sreachRadius, IEntity hurtBox, IEntity collision, int hp, int dammage, int attackRange, int activeRadius, float dashForce)
        {
            Speed = speed;
            SreachRadius = sreachRadius;
            HurtBox = hurtBox;
            Collision = collision;
            HP = hp;
            Damage = dammage;
            MAXHP = hp;
            AttackRange = attackRange;
            ActiveRadius = activeRadius;
            DashForce = dashForce;
        }
        public void UpdateState(GameTime gameTime, List<IEntity> collisions, CollisionComponent collisionComponents, Vector2 targetPosition)
        {
            if (_collisions == null || _collisionComponents == null)
            {
                _collisions = collisions;
                _collisionComponents = collisionComponents;
            }

            var hurtBox = HurtBox as MonsterHurtbox;
            var col = Collision as MonsterCollision;
            TargetPos = targetPosition;
            float deltaTime = gameTime.GetElapsedSeconds();
            if (DesiredPosition != Vector2.Zero)
                Position = DesiredPosition;

            if (animation != null)
            {
                if (HP > 0)
                {
                    StateChecking(deltaTime);
                    CurrentState.Update(this, deltaTime);
                }
                DeleteHitBox(deltaTime, collisions, collisionComponents);
                UpdateHitTimer(deltaTime);
                hurtBox.Update(Position);
                col.Update(DesiredPosition);

                if (Hitbox != null)
                {
                    var rect = (RectangleF)Hitbox.Bounds;
                    rect.Position = Position - (rect.Size / 2f);
                    Hitbox.Bounds = rect;
                }

                animation.UpdateFrame(gameTime, Position); // Draw  
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {

            if (animation != null)
            {
                if (HP <= 0)
                {
                    Color tint = _deadTimer > 0.16 ? _deadTimer <= 0.6 ? Color.White * _deadTimer : Color.White : Color.White * 0.05f;
                    if (animation.CurrentAnimation == "left")
                    {
                        animation.DrawFrame(spriteBatch, true, tint);
                    }
                    else
                    {
                        animation.DrawFrame(spriteBatch, false, tint);
                    }
                }
                else
                {
                    bool shouldFlash = _isHit && (_blinkTimer < _blinkInterval);
                    Color tint = shouldFlash ? Color.Red : Color.White; // transparent and normal
                    if (animation.CurrentAnimation == "left")
                    {
                        animation.DrawFrame(spriteBatch, true, tint);
                    }
                    else
                    {
                        animation.DrawFrame(spriteBatch, false, tint);
                    }
                    // UI เลือด
                    DrawUI(spriteBatch);
                }
            }
        }
        //private SoundEffect jumpSound;
        //public override void LoadSound(ContentManager content, AudioController controller, string hitSfxName, string deadSfxName, string? jumpSfxName = null)
        //{
        //    base.LoadSound(content, controller, hitSfxName, deadSfxName);
        //    jumpSound = content.Load<SoundEffect>("Audio/" + jumpSfxName);
        //}
        public void CreateHitbox(List<IEntity> collisions, CollisionComponent collisionComponents)
        {
            const float ttl = 0.3f; // ms
            var bounds = HurtBox.Bounds.BoundingRectangle;
            var center = bounds.Center;
            var topleft = bounds.TopLeft;
            SizeF size = new SizeF(bounds.Width * 0.75f, bounds.Height * 0.75f); // Hitbox size; 
            if (Hitbox == null)
            {
                Hitbox = new MonsterAttackHitbox(
                                new RectangleF(Position - (size / 2f),
                                size), ttl, this);
            }
            Hitbox.TimeToLiveSeconds = ttl;
            Hitbox.Bounds.Position = Position - (size / 2f);
            collisions.Add(Hitbox);
            collisionComponents.Insert(Hitbox);
        }
        public void StateChecking(float deltaTime)
        {
            var bounds = HurtBox.Bounds.BoundingRectangle;
            var HurtboxWidth = (int)bounds.Width;
            var HurtboxHeight = (int)bounds.Height;
            preventMonsterEdge = Vector2.Distance(Position, PreventMonster.Position) - PreventMonster.Radius;
            isInAttackList = PreventMonster.ActiveAttacker.Contains(this);
            isAwayHome = Vector2.Distance(Position, SpawnPosition) > AwaySpawnRadius;
            isInRange = Vector2.Distance(Position, TargetPos) <= SreachRadius;
            //isInAttack = Vector2.Distance(Position, TargetPos) <= AttackRange;
            isInWander = Vector2.Distance(Position, TargetPos) > SreachRadius && Vector2.Distance(Position, SpawnPosition) > Width;
            isInActiveRadius = Vector2.Distance(Position, TargetPos) <= ActiveRadius;
            DirectionToPlayer = TargetPos - Position;
            if (DirectionToPlayer != Vector2.Zero)
                DirectionToPlayer.Normalize();
            if (preventMonsterEdge >= 1f || IgnorePlayer)
            {
                if (isInAttackList)
                    PreventMonster.RemoveMonster(this);
            }
        }

        public override void MoveTo(float deltaTime, Vector2 position)
        {
            Vector2 direction = position - Position;
            direction.Normalize();
            if (Speed == 0) Speed = 1f;
            if (animation.CurrentSpriteSheet == "Idle")
            {
                _placeHolderDirection = direction;
                animation.SetAnimation("Charge", GetDirection(direction), OnAnimationEvent);
            }
        }

        public void DeleteHitBox(float deltaTime, List<IEntity> entities, CollisionComponent collisionComponent)
        {
            if (Hitbox != null)
            {
                if (Hitbox.TimeToLiveSeconds > 0f)
                {
                    Hitbox.TimeToLiveSeconds -= deltaTime;
                    if (Hitbox.TimeToLiveSeconds <= 0f)
                    {
                        entities.Remove(Hitbox);
                        collisionComponent.Remove(Hitbox);
                    }
                }
            }
        }
        private Vector2 _placeHolderDirection;
        public void OnAnimationEvent(IAnimationController sender, AnimationEventTrigger trigger)
        {
            if (animation.CurrentSpriteSheet == "Die" && trigger == AnimationEventTrigger.AnimationCompleted)
            {
                IsDead = true;
            }
            if (animation.CurrentSpriteSheet == "Attack" && trigger == AnimationEventTrigger.AnimationCompleted)
            {
                animation.SetAnimation("Idle", GetDirection(_placeHolderDirection));
                if (isInRange && !IgnorePlayer)
                {
                    ChangeState(new IdleState());
                }
            }
            if (animation.CurrentSpriteSheet == "Charge" && trigger == AnimationEventTrigger.AnimationCompleted)
            {
                ApplyKnockback(DashForce, _placeHolderDirection);
                CreateHitbox(_collisions, _collisionComponents);
                animation.SetAnimation("Attack", GetDirection(_placeHolderDirection), OnAnimationEvent);
            }
            

        }
        public void UnLoad()
        {
            RemoveMonster();
        }
        public void RemoveMonster()
        {
            PreventMonster.RemoveMonster(this);
            if (_collisions != null || _collisionComponents != null)
            {
                _collisions.Remove(HurtBox);
                _collisionComponents.Remove(HurtBox);
                _collisions.Remove(Collision);
                _collisionComponents.Remove(Collision); 
            }
            animation.Unload(OnAnimationEvent);
            HurtBox = null;
            Collision = null;
            Hitbox = null;
            animation = null;
            HealthUI = null;
            _hitParticle = null;
            _deadParticle = null;
        }
        public override void ChangeState(IMonsterState newState)
        {
            if (CurrentState.GetType() == newState.GetType()) return;
            CurrentState.Exit(this);
            CurrentState = newState;
            CurrentState.Enter(this);
        }
    }
}
