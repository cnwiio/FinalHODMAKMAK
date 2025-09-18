using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Animations;
using MonoGame.Extended.Collisions;

namespace game
{
    // note : The Createhitbox, CreateAnim, SetProperty method need changed based on situation.
    /*
     Example How to use :
    1.
            private List<IMonster> _monster = new List<IMonster>();
    2.
            var spawnPoint = _tileMaper.GetObjectLayer("SpawnPoint");
            foreach(var obj in spawnPoint.Objects)
            {
                if (obj.Type == "Melee")
                    _monster.Add(new MonsterMelee(obj.Position, _preventMonster));
            }
            ///// or /////
            _monster.Add(new MonsterMelee(new Vector2(600, 200), _preventMonster));
            _monster.Add(new MonsterMelee(new Vector2(400, 200), _preventMonster));
    3.
            foreach (MonsterMelee monsterMelee in _monster.OfType<MonsterMelee>().ToList())
            {
                monsterMelee.LoadAnim("Walk", "GoonWalk-Sheet", monsterMelee.Position, 128, 128, Content);
                monsterMelee.LoadAnim("Idle", "GrootIdle-Sheet", monsterMelee.Position, 128, 128, Content);
                monsterMelee.CreateAnimation();
                monsterMelee.SetProperty(
                    speed: 100f,
                    sreachRadius: 500f,
                    hp: 50,
                    damage: 1,
                    element: Element.light,
                    attackRange: (int)(monsterMelee.Width * 1.5f),
                    dashForce: monsterMelee.Width * 7
                );
                _collision.Add(monsterMelee.HurtBox);
            }
    4.
            // Monster
            foreach (MonsterMelee monster in _monster)
            {
                monster.UpdateState(gameTime, _collision, _collisionComponent, _player._movement.Position);
                if (monster.ShakeViewport)
                {
                    camera.ShakeCamera(gameTime);
                    monster.ShakeViewport = camera.ShakeViewport;
                }
                // ----Temporary-----
                // Will make additional method for monster dead and drop
                // ps. make a new global class and make a drop heal there, then call it in remove monster(maybe)
                if (monster.IsDead)
                {
                    monster.DropHeal(_collision, _collisionComponent, _dropTexture, _player);
                    monster.DeleteHitBox(1f, _collision, _collisionComponent);
                    monster.RemoveMonster();
                    _monster.Remove(monster);
                    break; // Exit the loop to avoid modifying the collection while iterating; list bug prevented
                }
                //------Temporary--------
            }
    5.
            foreach (MonsterMelee monster in _monster)
            {
                monster.DrawMonster(_spriteBatch);
                _spriteBatch.DrawCircle(new CircleF(monster.Position, monster.SreachRadius), 16, Color.Red,2); // for debug only
            }
     */
    public class MonsterMelee : MonsterBasic, IMonster, IYsort
    {
        // -------------Property-------------
        public float SortY { get => Position.Y + Height / 2; }
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
                        animation.SetAnimation("Die", GetDirection(_placeHolderDirection), OnAnimationEvent); 
                    }
                }
            }
        }
        public MonsterMelee(Vector2 position, PreventMonster preventMonster, Player player, Particle particle, Element element)
        {
            Position = position;
            SpawnPosition = position;
            PreventMonster = preventMonster;
            _player = player;
            _particle = particle;
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

            animation.CreateAnimation("Walk", "left", true, 200, 0, 8);
            animation.CreateAnimation("Walk", "right", true, 200, 0, 8);

            animation.CreateAnimation("Charge", "right", false, 200, 0, 6);
            animation.CreateAnimation("Charge", "left", false, 200, 0, 6);

            animation.CreateAnimation("Attack", "left", false, 100, 0, 9);
            animation.CreateAnimation("Attack", "right", false, 100, 0, 9);

            animation.CreateAnimation("Die", "right", false, 100, 0, 12);
            animation.CreateAnimation("Die", "left", false, 100, 0, 12);
        }
        // Need Change in future
        public void SetProperty(float speed, float sreachRadius, int hp, int damage, int attackRange, int activeRadius, float dashForce)
        {
            SetProperty(
                speed,
                sreachRadius,
                new MonsterHurtbox(
                    animation.AnimSprite["Walk"].GetBoundingRectangle(new Transform2(animation.Position, 0f, new Vector2(0.6f,0.9f))),
                    this),
                new MonsterCollision(
                    new RectangleF(0, 0, 60, 30), 
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
                    var scale = new Vector2(0.1f, 0.2f);
                    var percent = (float)HP / (float)MAXHP; // เปอร์เซ็นเลือด
                    var offset = new Vector2(HealthUI.Width * 0.1f / 2, Height / 1.5f);
                    spriteBatch.Draw(HealthUI, Position - offset, new Rectangle(0, 0, HealthUI.Width, HealthUI.Height / 2), Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
                    spriteBatch.Draw(HealthUI, Position - offset + new Vector2(0.8f, 0), new Rectangle(0, HealthUI.Height / 2, (int)(HealthUI.Width * percent), HealthUI.Height / 2), Color.Red, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
                } 
            }
        }
        public void CreateHitbox(List<IEntity> collisions, CollisionComponent collisionComponents)
        {
            const float ttl = 0.7f; // ms
            var bounds = HurtBox.Bounds.BoundingRectangle;
            var center = bounds.Center;
            var topleft = bounds.TopLeft;
            SizeF size = new SizeF(bounds.Width * 0.65f, bounds.Height * 0.75f); // Hitbox size; 
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
            isInAttack = Vector2.Distance(Position, TargetPos) <= AttackRange;
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
                _attackCD = 1f;
                ChangeState(new IdleState());
            }
            if (animation.CurrentSpriteSheet == "Charge" && trigger == AnimationEventTrigger.AnimationCompleted)
            {
                ApplyKnockback(DashForce, _placeHolderDirection);
                CreateHitbox(_collisions, _collisionComponents);
                animation.SetAnimation("Attack", GetDirection(_placeHolderDirection), OnAnimationEvent);
            }

        }
        public override void Attack()
        {
            if (!isAttack)
            {
                isAttack = true;
                _placeHolderDirection = DirectionToPlayer;
                animation.SetAnimation("Charge", GetDirection(_placeHolderDirection), OnAnimationEvent);
            }
        }
        public void UnLoad()
        {
            RemoveMonster();
        }
        public void RemoveMonster()
        {
            PreventMonster.RemoveMonster(this);
            _collisions.Remove(HurtBox);
            _collisionComponents.Remove(HurtBox);
            _collisions.Remove(Collision);
            _collisionComponents.Remove(Collision);
            animation.Unload(OnAnimationEvent);
            animation = null;
            HealthUI = null;
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
