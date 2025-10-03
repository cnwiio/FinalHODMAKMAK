using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Animations;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Timers;

namespace game
{
    public class MonsterBoss : MonsterBasic, IMonster, IYsort
    {
        // -------------Property-------------
        public float BulletSpeed { get; set; } = 350;
        public float SortY { get => Position.Y + Height / 2; }
        public float SortX { get => Position.X; }
        // ----------------------------------
        public Texture2D bulletLight { get; set; }
        public Texture2D bulletDark { get; set; }
        public MonsterAttackHitbox[] bulletHitbox;
        public Vector2[] bulletDirection;
        private float fireInterval;
        private FireParticle _fireParticle;
        private Texture2D _teleGraph;
        private const short MAXWAVES = 10;
        private short wave;
        private float waveTimer = 0;
        public Telegraph[] telegraph;
        public MonsterAttackHitbox[] telegraphHitbox;
        // ----------------Bool----------------
        public bool[] BulletVisible;
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
                        animation.SetAnimation("Die", GetDirection(_placeHolderDirection), OnAnimationEvent);
                    }
                }
            }
        }
        public override bool isHit // togle I-frame state; check if monster is attacked
        {
            get => _isHit;
            set
            {
                if (value && !IsDead)
                {
                    _isHit = true;
                    ShakeViewport = true;
                    _hitTimer = 0.25f;
                    _deadTimer = 1.2f;
                    if (HP > 0)
                    {
                        _hitParticle.Trigger(Position, -DirectionToPlayer);
                    }
                }
            }
        }

        #region Setting && Update
        public MonsterBoss(Vector2 position, PreventMonster preventMonster, Player player, HitParticle particle, DeadParticle deadParticle, FireParticle fireParticle, ElementType element)
        {
            Position = position;
            DesiredPosition = position;
            SpawnPosition = position;
            PreventMonster = preventMonster;
            _player = player;
            _hitParticle = particle;
            _deadParticle = deadParticle;
            _fireParticle = fireParticle;
            ElementType = element;

            wave = 0;
        }
        public void LoadAssets(ContentManager content)
        {
            _teleGraph = content.Load<Texture2D>("Texture/" + "Circle");
            telegraph = new Telegraph[MAXWAVES];
            telegraphHitbox = new MonsterAttackHitbox[MAXWAVES];
            for (int i = 0; i < MAXWAVES; i++)
            {
                var individualSkillTexture = new AnimController(Position);
                individualSkillTexture.LoadFrame(content, "Idle", "DarkGoonIdle", 128, 128);
                individualSkillTexture.LoadFrame(content, "Attack", "DarkGoonAttack", 128, 128);
                individualSkillTexture.CreateAnimation("Idle", "no", true, 50, 0, 4);
                individualSkillTexture.CreateAnimation("Attack", "yes", false, 50, 0, 9);
                telegraph[i] = new Telegraph(_teleGraph, individualSkillTexture);
            }
        }

        public void loadBullet(ContentManager content, string lightBullet, string darkBullet)
        {
            bulletHitbox = new MonsterAttackHitbox[10];
            BulletVisible = new bool[10];
            bulletDirection = new Vector2[10];
            bulletLight = content.Load<Texture2D>("texture/" + lightBullet);
            bulletDark = content.Load<Texture2D>("texture/" + darkBullet);
        }
        /*
         IMPORTANT NOTE: Need to change in future
         Based on the animation sprite sheet
        */
        public void CreateAnimation()
        {
            animation.CreateAnimation("Idle", "left", true, 200, 0, 8);
            animation.CreateAnimation("Idle", "right", true, 200, 0, 8);

            animation.CreateAnimation("Walk", "left", true, 200, 0, 8);
            animation.CreateAnimation("Walk", "right", true, 200, 0, 8);

            animation.CreateAnimation("Charge", "right", false, 200, 0, 6);
            animation.CreateAnimation("Charge", "left", false, 200, 0, 6);

            animation.CreateAnimation("Casting", "left", true, 100, 0, 4);
            animation.CreateAnimation("Casting", "right", true, 100, 0, 4);

            animation.CreateAnimation("Attack", "left", false, 100, 0, 9);
            animation.CreateAnimation("Attack", "right", false, 100, 0, 9);

            animation.CreateAnimation("Die", "right", false, 100, 0, 12);
            animation.CreateAnimation("Die", "left", false, 100, 0, 12);

            animation.CreateAnimation("ChargeFire", "left", false, 200, 0, 2);
            animation.CreateAnimation("ChargeFire", "right", false, 200, 0, 2);

            animation.CreateAnimation("EndFire", "left", false, 200, 10, 8);
            animation.CreateAnimation("EndFire", "right", false, 200, 10, 8);

            animation.CreateAnimation("Fire", "left", true, 200, 2, 8);    
            animation.CreateAnimation("Fire", "right", true, 200, 2, 8);
        }
        // Need Change in future
        public void SetProperty(float speed, float sreachRadius, int hp, int damage, int attackRange, int activeRadius, float dashForce, int bulletSpeed)
        {
            SetProperty(
                speed,
                sreachRadius,
                new MonsterHurtbox(
                    animation.AnimSprite["Idle"].GetBoundingRectangle(new Transform2(animation.Position, 0f, new Vector2(0.6f, 0.9f))),
                    this),
                new MonsterCollision(
                    new RectangleF(0, 0, 60, 30),
                    this),
                hp,
                damage,
                attackRange,
                activeRadius,
                dashForce,
                bulletSpeed
                );
        }
        public void SetProperty(float speed, float sreachRadius, IEntity hurtBox, IEntity collision, int hp, int dammage, int attackRange, int activeRadius, float dashForce, int bulletSpeed)
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
            BulletSpeed = bulletSpeed;
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
                UpdateSkillTimer(gameTime);
                UpdateHitbox(deltaTime);
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
            if (!isInActiveRadius && Vector2.Distance(Position, TargetPos) <= ActiveRadius)
            {
                isInActiveRadius = true;
            } else if (!isInRange) 
            {
                Reset();
                isInActiveRadius = false;
            }
            //isInActiveRadius = Vector2.Distance(Position, TargetPos) <= ActiveRadius;
            isInAttackList = isInRange;
            DirectionToPlayer = TargetPos - Position;
            if (DirectionToPlayer != Vector2.Zero)
                DirectionToPlayer.Normalize();
        }
        #endregion

        #region Draw
        public void Draw(SpriteBatch spriteBatch)
        {
            var elementColor = ElementType == ElementType.Light ? Color.Gold : Color.Violet;
            var offset = new Vector2(0, 128);
            if (animation != null)
            {
                var origin = new Vector2(_teleGraph.Width / 2, _teleGraph.Height / 2);
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
                    spriteBatch.Draw(_teleGraph, Position - offset, null, elementColor, 0, Vector2.Zero, Vector2.One * 5, SpriteEffects.None, 0);
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
                    spriteBatch.Draw(_teleGraph, Position - offset, null, elementColor, 0, origin, Vector2.One * 5, SpriteEffects.None, 0);
                    //DrawUI(spriteBatch);
                }

                for (int i = 0; i < 10 ; i++) 
                {
                    if (BulletVisible[i] == true)
                    {
                        var bullet = ElementType == ElementType.Light ? bulletLight : bulletDark;
                        var size = new SizeF(bullet.Width, bullet.Height);
                        spriteBatch.Draw(bullet, bulletHitbox[i].Bounds.Position - (size * 0.15f), Color.White); // 10 - คูณ hitbox / 2 เช่น 10 - 0.8f / 2
                    }
                }
            }
        }
        private float _HPScale = 1;
        private float _followUpUI = 1;
        private short _frameCount = 0;
        public void DrawUI(SpriteBatch spriteBatch, Vector2 position)
        {
            // UI เลือด
            var scale = new Vector2(16, 4);
            var percent = (float)HP / (float)MAXHP; // เปอร์เซ็นเลือด
            var offset = new Vector2(HealthUI.Width / 2, 0) * scale;
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
            spriteBatch.Draw(HealthUI, position - offset, new Rectangle(0, 0, HealthUI.Width, HealthUI.Height / 2), Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
            spriteBatch.Draw(HealthUI, position - offset, new Rectangle(0, HealthUI.Height / 2, (int)(HealthUI.Width * _followUpUI), HealthUI.Height / 2), Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
            spriteBatch.Draw(HealthUI, position - offset, new Rectangle(0, HealthUI.Height / 2, (int)(HealthUI.Width * _HPScale), HealthUI.Height / 2), Color.Crimson, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
        }

        public void DrawSkill(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < MAXWAVES; i++)
            {
                telegraph[i].Draw(spriteBatch); 
            }
        }
        #endregion

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

        public void CreateBulletHitbox(int i)
        {
            const float ttl = 2f; // ms
            var bullet = ElementType == ElementType.Light ? bulletLight : bulletDark;
            var bounds = bullet.Bounds;
            SizeF size = new SizeF(bounds.Width, bounds.Height) * 0.7f; // Hitbox size; 
            var offset = Vector2.Zero;
            if (GetDirection(DirectionToPlayer) == "left")
            {
                offset = new Vector2(-Width / 2 + 32, 32);
            } else
            {
                offset = new Vector2(Width / 2 - 32, 32);
            }
            if (bulletHitbox[i] == null)
            {
                bulletHitbox[i] = new MonsterAttackHitbox(
                                new RectangleF(Position - (size / 2f) + offset,
                                size), ttl, this);
            }
            bulletHitbox[i].TimeToLiveSeconds = ttl;
            bulletHitbox[i].Bounds.Position = Position - (size / 2f) + offset;
            BulletVisible[i] = true;
            bulletHitbox[i].bulletVisible = true;
            var dir = bulletHitbox[i].Bounds.Position - TargetPos;
            dir.Normalize();
            bulletDirection[i] = -dir;
            _collisions.Add(bulletHitbox[i]);
            _collisionComponents.Insert(bulletHitbox[i]);
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

            for (int i = 0; i < 10; i++)
            {
                if (bulletHitbox[i] != null)
                {
                    if (bulletHitbox[i].TimeToLiveSeconds > 0f)
                    {
                        bulletHitbox[i].TimeToLiveSeconds -= deltaTime;
                        BulletVisible[i] = bulletHitbox[i].bulletVisible;
                        if (bulletHitbox[i].TimeToLiveSeconds < 0f || !BulletVisible[i])
                        {
                            BulletVisible[i] = false;
                            bulletHitbox[i].TimeToLiveSeconds = 0;
                            var tint = ElementType == ElementType.Light ? new Color(255, 248, 174) : new Color(202, 174, 255);
                            _fireParticle.Trigger(bulletHitbox[i].Bounds.Position, tint);
                            _collisions.Remove(bulletHitbox[i]);
                            _collisionComponents.Remove(bulletHitbox[i]);
                        }
                    }
                } 
            }
        }
        private Vector2 _placeHolderDirection;
        private short dashCounter = 1;
        public void OnAnimationEvent(IAnimationController sender, AnimationEventTrigger trigger)
        {
            if (animation.CurrentSpriteSheet == "Die" && trigger == AnimationEventTrigger.AnimationCompleted)
            {
                IsDead = true;
            }
            if (animation.CurrentSpriteSheet == "Attack" && trigger == AnimationEventTrigger.AnimationCompleted)
            {
                if (currentBossAttack == 3)
                {
                    if (dashCounter < 3)
                    {
                        dashCounter++;
                        _placeHolderDirection = DirectionToPlayer;
                        ApplyKnockback(DashForce, _placeHolderDirection);
                        CreateHitbox(_collisions, _collisionComponents);
                        animation.SetAnimation("Charge", GetDirection(_placeHolderDirection), OnAnimationEvent);
                        animation.SetAnimation("Attack", GetDirection(_placeHolderDirection), OnAnimationEvent);
                        return;
                    }
                    else
                    {
                        dashCounter = 1;
                    } 
                }

                _attackCD = 1f;
                ChangeState(new IdleState());
            }
            if (animation.CurrentSpriteSheet == "Charge" && trigger == AnimationEventTrigger.AnimationCompleted)
            {
                if (currentBossAttack == 3)
                {
                    ApplyKnockback(DashForce, _placeHolderDirection);
                    CreateHitbox(_collisions, _collisionComponents);
                    animation.SetAnimation("Attack", GetDirection(_placeHolderDirection), OnAnimationEvent); 
                }
                else if (currentBossAttack == 5)
                {
                    // กูเจนมา จะอ่านต้องดำเอา

                    // Calculate three different directions with 45-degree angles
                    float angle45 = MathHelper.ToRadians(45);
                    float angleMinus45 = MathHelper.ToRadians(-45);
                    
                    // First bullet: DirectionToPlayer - 45 degrees
                    bulletDirection[0] = RotateVector(DirectionToPlayer, angleMinus45);
                    CreateBulletHitbox(0);
                    
                    // Second bullet: DirectionToPlayer (original direction)
                    bulletDirection[1] = DirectionToPlayer;
                    CreateBulletHitbox(1);
                    
                    // Third bullet: DirectionToPlayer + 45 degrees
                    bulletDirection[2] = RotateVector(DirectionToPlayer, angle45);
                    CreateBulletHitbox(2);
                    
                    animation.SetAnimation("Attack", GetDirection(_placeHolderDirection), OnAnimationEvent);
                }
            }
            if (animation.CurrentSpriteSheet == "ChargeFire" && trigger == AnimationEventTrigger.AnimationCompleted)
            {
                animation.SetAnimation("Fire", GetDirection(_placeHolderDirection));
            }

            if (animation.CurrentSpriteSheet == "EndFire" && trigger == AnimationEventTrigger.AnimationCompleted)
            {
                _attackCD = 0.5f;
                ChangeState(new IdleState());
            }
        }

        private short currentBossAttack;
        public override void Attack()
        {
            var r = new Random();
            if (!isAttack)
            {
                isAttack = true;
                currentBossAttack = (short)r.Next(1, 7);
                _placeHolderDirection = DirectionToPlayer;
                //currentBossAttack = 6;
                if (currentBossAttack == 3 || currentBossAttack == 5)
                {
                    animation.SetAnimation("Charge", GetDirection(_placeHolderDirection), OnAnimationEvent);
                }
                else if (currentBossAttack == 4)
                {
                    if (ElementType == ElementType.Light) ElementType = ElementType.Dark;
                    else ElementType = ElementType.Light;
                    _attackCD = 1f;
                    ChangeState(new IdleState());
                }
                else if (currentBossAttack == 6)
                {
                    animation.SetAnimation("ChargeFire", GetDirection(DirectionToPlayer), OnAnimationEvent);
                }
            }
            switch (currentBossAttack) 
            {
                case 1:
                    CreateTelegraph();
                    animation.SetAnimation("Casting", GetDirection(DirectionToPlayer));
                    break;
                case 2:
                    CreateTelegraph2();
                    animation.SetAnimation("Casting", GetDirection(DirectionToPlayer));
                    break;
                case 6:
                    if (animation.CurrentSpriteSheet != "ChargeFire" && animation.CurrentSpriteSheet != "EndFire")
                    {
                        animation.SetAnimation("Fire", GetDirection(DirectionToPlayer));
                        FireBullet();
                    }
                    break;
                default:
                    break;
            }
        }
        public void UnLoad()
        {
            RemoveMonster();
        }
        public void RemoveMonster()
        {
            _collisions.Remove(HurtBox);
            _collisionComponents.Remove(HurtBox);
            _collisions.Remove(Collision);
            _collisionComponents.Remove(Collision);
            animation.Unload(OnAnimationEvent);
            HurtBox = null;
            Collision = null;
            Hitbox = null;
            animation = null;
            HealthUI = null;
            _teleGraph = null;
            bulletLight = null;
            bulletDark = null;
            _hitParticle = null;
            _fireParticle = null;
            _deadParticle = null;
        }
        public override void ChangeState(IMonsterState newState)
        {
            if (CurrentState.GetType() == newState.GetType()) return;
            CurrentState.Exit(this);
            CurrentState = newState;
            CurrentState.Enter(this);
        }


        // ------------------------------------------------------------------------------------ //
        // -----------------------------   skill  --------------------------------------------- //
        // ------------------------------------------------------------------------------------ //

        public void UpdateSkillTimer(GameTime gameTime)
        {
            float deltaTime = gameTime.GetElapsedSeconds();
            for (int i = 0; i < MAXWAVES; i++)
            {
                telegraph[i].Update(gameTime);
                if (telegraph[i].isFinished())
                {
                    ShakeViewport = true;
                    CreateTelegraphHitbox(i);
                }
            }
            if (waveTimer > 0)
            {
                waveTimer -= deltaTime;
                if (waveTimer <= 0)
                {
                    waveTimer = 0f;
                }
            }
            DeleteTelegraphHitbox(deltaTime);

            // bullet
            if (fireInterval > 0)
            {
                fireInterval -= deltaTime;
                if (fireInterval <= 0)
                {
                    fireInterval = 0f;
                }
            }
        }

        public void UpdateHitbox(float deltaTime)
        {
            for (int i = 0; i < 10; i++)
            {
                if (bulletHitbox[i] != null && BulletVisible[i])
                {
                    var direction = bulletDirection[i];
                    if (direction.LengthSquared() != 0)
                        direction.Normalize();
                    bulletHitbox[i].Bounds.Position += direction * BulletSpeed * deltaTime;  
                }
            }
        }

        // ตีตามตัว
        public void CreateTelegraph()
        {
            var waveAmout = 10;
            if (wave < waveAmout && waveTimer == 0f && isAttack)
            {
                waveTimer = 0.3f;
                telegraph[wave].Create(
                    scale: 10,
                    alpha: 0.2f,
                    position: TargetPos,
                    lifeTime: 0.75f
                    );
                wave++;
            }
            else if (wave >= waveAmout && waveTimer == 0)
            {
                wave = 0;
                waveTimer = 2;
                _attackCD = 1f;
                ChangeState(new IdleState());
                isAttack = false;
            }
        }
        // ตีเป็นเส้น
        public void CreateTelegraph2()
        {
            var pos = wave == 0 ? Position : telegraph[wave-1].Position;
            var direction = DirectionToPlayer;
            if (direction != Vector2.Zero)
                direction.Normalize();
            if (wave < MAXWAVES && waveTimer == 0f && isAttack)
            {
                waveTimer = 0.1f;
                telegraph[wave].Create(
                    scale: 10,
                    alpha: 0.2f,
                    position: pos + direction * (_teleGraph.Bounds.Width * 10),
                    lifeTime: 0.75f
                    );
                wave++;
            }
            else if (wave >= MAXWAVES && waveTimer == 0)
            {
                wave = 0;
                waveTimer = 2;
                _attackCD = 1f;
                ChangeState(new IdleState());
                isAttack = false;
            }
        }

        // hitbox วงโจมตี
        public void CreateTelegraphHitbox(int i)
        {
            const float ttl = 0.1f; // ms
            var bounds = _teleGraph.Bounds;
            var center = bounds.Center;
            var pos = telegraph[i].Position;
            SizeF size = new SizeF(bounds.Width, bounds.Height) * 10f; // Hitbox size; 
            if (telegraphHitbox[i] == null)
            {
                telegraphHitbox[i] = new MonsterAttackHitbox(
                                new RectangleF(pos - (size / 2f),
                                size), ttl, this);
            }
            telegraphHitbox[i].TimeToLiveSeconds = ttl;
            telegraphHitbox[i].Bounds.Position = pos - (size / 2f);
            _collisions.Add(telegraphHitbox[i]);
            _collisionComponents.Insert(telegraphHitbox[i]);
        }

        public void DeleteTelegraphHitbox(float deltaTime)
        {
            for (int i = 0; i < MAXWAVES; i++)
            {
                if (telegraphHitbox[i] != null)
                {
                    if (telegraphHitbox[i].TimeToLiveSeconds > 0f)
                    {
                        telegraphHitbox[i].TimeToLiveSeconds -= deltaTime;
                        if (telegraphHitbox[i].TimeToLiveSeconds <= 0f)
                        {
                            _collisions.Remove(telegraphHitbox[i]);
                            _collisionComponents.Remove(telegraphHitbox[i]);
                        }
                    }
                } 
            }
        }

        // Helper method to rotate a vector by a given angle in radians
        private Vector2 RotateVector(Vector2 vector, float angle)
        {
            float cos = (float)Math.Cos(angle);
            float sin = (float)Math.Sin(angle);
            return new Vector2(
                vector.X * cos - vector.Y * sin,
                vector.X * sin + vector.Y * cos
            );
        }

        public void FireBullet()
        {
            var waveAmout = 10;
            if (wave < waveAmout && waveTimer == 0f && isAttack)
            {
                waveTimer = 0.35f;
                CreateBulletHitbox(wave);
                wave++;
            }
            else if (wave >= waveAmout && waveTimer == 0)
            {
                wave = 0;
                waveTimer = 2;
                animation.SetAnimation("EndFire", GetDirection(DirectionToPlayer), OnAnimationEvent);
            }
        }

        // ------------------------------------------------------------------------------------ //
        // -----------------------------   skill  --------------------------------------------- //
        // ------------------------------------------------------------------------------------ //
    }
}
