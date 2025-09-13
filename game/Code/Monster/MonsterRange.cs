using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Animations;
using MonoGame.Extended.Collisions;

namespace game
{
    //note : The Createhitbox, CreateAnim, SetProperty method need changed based on situation.
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
    public class MonsterRange : MonsterBasic, IMonster, IYsort
    {
        // -------------Property-------------
        public float BulletSpeed { get; set; } = 350;
        public float SortY {  get => Position.Y + Height / 2; }
        // ----------------------------------
        public Texture2D bullet { get; set; }
        // ----------------Bool----------------
        public bool BulletVisible = false;
        // ----------------------------------
        public override int HP
        {
            get => _HP;
            set
            {
                _HP = value;
                if (_HP <= 0)
                {
                    _HP = 0;
                    _hitTimer += 2f;
                    animation.SetAnimation("Die", GetDirection(_placeHolderDirection), OnAnimationEvent);
                }
            }
        }


        public MonsterRange(Vector2 position, PreventMonster preventMonster, Player player, Particle particle)
        {
            Position = position;
            SpawnPosition = position;
            PreventMonster = preventMonster;
            _player = player;
            _particle = particle;
        }
        public void loadBullet(ContentManager content, string textureName)
        {
            bullet = content.Load<Texture2D>("texture/" + textureName);
        }
        /*
         IMPORTANT NOTE: Need to change in future
         Based on the animation sprite sheet
        */
        public void CreateAnimation()
        {
            animation.CreateAnimation("Idle", "right", true, 200, 0, 4);
            animation.CreateAnimation("Idle", "left", true, 200, 0, 4);

            animation.CreateAnimation("Walk", "right", true, 200, 0, 4);
            animation.CreateAnimation("Walk", "left", true, 200, 0, 4);

            animation.CreateAnimation("Charge", "right", false, 200, 0, 8);
            animation.CreateAnimation("Charge", "left", false, 200, 0, 8);

            animation.CreateAnimation("Attack", "right", false, 100, 0, 4);
            animation.CreateAnimation("Attack", "left", false, 100, 0, 4);

            animation.CreateAnimation("Die", "right", false, 100, 0, 12);
            animation.CreateAnimation("Die", "left", false, 100, 0, 12);
        }
        // Need Change in future
        public void SetProperty(float speed, float sreachRadius, int hp, int damage, Element element, int attackRange, float dashForce)
        {
            SetProperty(
                speed,
                sreachRadius,
                new MonsterHurtbox(
                    animation.AnimSprite["Walk"].GetBoundingRectangle(new Transform2(animation.Position, 0f, Vector2.One)),
                this), 
                new MonsterCollision(
                    new RectangleF(0, 0, 50, 30),
                    this),
                hp,
                damage,
                element,
                attackRange,
                dashForce
                );
        }
        public void SetProperty(float speed, float sreachRadius, IEntity hurtBox, IEntity collision, int hp, int dammage, Element element, int attackRange, float dashForce)
        {
            Speed = speed;
            SreachRadius = sreachRadius;
            HurtBox = hurtBox;
            Collision = collision;
            HP = hp;
            Damage = dammage;
            MAXHP = hp;
            ElementType = element;
            AttackRange = attackRange;
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

            if (animation != null)
            {
                if (HP > 0)
                {
                    StateChecking(deltaTime);
                    CurrentState.Update(this, deltaTime);
                }
                DeleteHitBox(deltaTime);
                hurtBox.Update(Position);
                col.Update(Position);
                UpdateHitTimer(deltaTime);

                if (BulletVisible)
                {
                    UpdateHitbox(deltaTime);
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
                    if (animation.CurrentAnimation == "right")
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
                    if (animation.CurrentAnimation == "right")
                    {
                        animation.DrawFrame(spriteBatch, true, tint);
                    }
                    else
                    {
                        animation.DrawFrame(spriteBatch, false, tint);
                    }
                }
                _particle?.Draw(spriteBatch);
            }
            if (BulletVisible)
            {
                spriteBatch.Draw(bullet, Hitbox.Bounds.BoundingRectangle.Position, Color.White);
            }
        }
        public void MoveToDirection(float deltaTime, Vector2 direction)
        {
            if (direction.LengthSquared() != 0)
                direction.Normalize();
            if (Speed == 0) Speed = 1f;
            Vector2 movement = direction * Speed * deltaTime;
            Position += movement;
            animation.SetAnimation("Walk", GetDirection(-direction));
        }
        public void CreateHitbox(List<IEntity> collisions, CollisionComponent collisionComponents)
        {
            const float ttl = 2f; // ms
            var bounds = bullet.Bounds;
            SizeF size = new SizeF(bounds.Width, bounds.Height); // Hitbox size; 
            if (Hitbox == null)
            {
                Hitbox = new MonsterAttackHitbox(
                                new RectangleF(Position - (size / 2f),
                                size), ttl, this);
            }
            Hitbox.TimeToLiveSeconds = ttl;
            Hitbox.Bounds.Position = Position - (size / 2f);
            BulletVisible = true;
            collisions.Add(Hitbox);
            collisionComponents.Insert(Hitbox);
        }
        public void StateChecking(float deltaTime)
        {
            var bounds = HurtBox.Bounds.BoundingRectangle;
            var HurtboxWidth = (int)bounds.Width;
            var HurtboxHeight = (int)bounds.Height;
            preventMonsterEdge = Vector2.Distance(Position, PreventMonster.Position) - PreventMonster.Radius;
            isAwayHome = Vector2.Distance(Position, SpawnPosition) > AwaySpawnRadius;
            isInRange = Vector2.Distance(Position, TargetPos) <= SreachRadius;
            isInAttack = Vector2.Distance(Position, TargetPos) <= AttackRange;
            isInWander = Vector2.Distance(Position, TargetPos) > SreachRadius && Vector2.Distance(Position, SpawnPosition) > Width;
            isInActiveRadius = Vector2.Distance(Position, TargetPos) <= ActiveRadius;
            isInAttackList = isInRange;
            DirectionToPlayer = TargetPos - Position;
            if (DirectionToPlayer != Vector2.Zero)
                DirectionToPlayer.Normalize();
        }
        public void DeleteHitBox(float deltaTime)
        {
            if (Hitbox != null)
            {
                if (Hitbox.TimeToLiveSeconds > 0f)
                {
                    Hitbox.TimeToLiveSeconds -= deltaTime;
                    if (Hitbox.TimeToLiveSeconds <= 0f || !BulletVisible)
                    {
                        BulletVisible = false;
                        _collisions.Remove(Hitbox);
                        _collisionComponents.Remove(Hitbox);
                    }
                }
            }
        }
        public void UpdateHitbox(float deltaTime)
        {
            if (_placeHolderDirection.LengthSquared() != 0)
                _placeHolderDirection.Normalize();
            var direction = _placeHolderDirection;
            Hitbox.Bounds.Position += direction * BulletSpeed * deltaTime;
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
                _attackCD = 2f;
                ChangeState(new IdleState());
            }
            if (animation.CurrentSpriteSheet == "Charge" && trigger == AnimationEventTrigger.AnimationCompleted)
            {
                ApplyKnockback(300f);
                CreateHitbox(_collisions, _collisionComponents);
                animation.SetAnimation("Attack", GetDirection(_placeHolderDirection), OnAnimationEvent);
            }

        }
        public void RemoveMonster()
        {
            _collisions.Remove(HurtBox);
            _collisionComponents.Remove(HurtBox);
            animation.Unload(OnAnimationEvent);
            animation = null;
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
            animation.Unload(OnAnimationEvent);
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
