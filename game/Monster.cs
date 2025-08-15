using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Animations;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Collisions.Layers;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Input.InputListeners;
using MonoGame.Extended.Timers;

namespace game
{

    public interface IMonster
    {
        Vector2 Position { get; set; }
        Vector2 TargetPos { get; set; }
        Vector2 SpawnPosition { get; set; }
        float WanderTimer { get; set; }
        bool WaitingToReturn { get; set; }
        bool IsReturning { get; set; }
        float Speed { get; set; }
        float SreachRadius { get; set; }
        int Width { get; set; }
        int Height { get; set; }

        void MoveTo(float deltaTime, Vector2 position);
    }

    public class MonsterMelee : IMonster
    {
        // note : The Createhitbox, CreateAnim, SetProperty method need changed based on situation.
        /*
         Example How to use :
        1.
                private List<IMonster> _monster = new List<IMonster>();
        2.
                _monster.Add(new MonsterMelee(new Vector2(600, 200), _preventMonster));
                _monster.Add(new MonsterMelee(new Vector2(400, 200), _preventMonster));
        3.
                foreach (MonsterMelee monsterMelee in _monster.OfType<MonsterMelee>().ToList())
            {
                monsterMelee.LoadAnim("Char01", monsterMelee.Position, 32, 48, Content);
                monsterMelee.CreateAnimation();
                monsterMelee.SetProperty(
                    speed: 100f,
                    sreachRadius: 300f,
                    hp: 3
                );
                _collision.Add(monsterMelee.HurtBox);
            }
        4.
                foreach (MonsterMelee monster in _monster)
                {
                    monster.UpdateState(gameTime, _collision, _collisionComponent, _player._movement.Position);
                }
        5.
                foreach (MonsterMelee monster in _monster)
                {
                    monster.DrawMonster(_spriteBatch);
                    _spriteBatch.DrawCircle(new CircleF(monster.Position, monster.SreachRadius), 16, Color.Red,2); // for debug only
                }
         */
        public Vector2 Center {  get; set; }
        public Vector2 Position { get; set; }
        public Vector2 TargetPos { get; set; }
        public Vector2 SpawnPosition { get; set; }
        public float WanderTimer { get; set; } = 0f;
        public bool WaitingToReturn { get; set; } = false;
        public bool IsReturning { get; set; } = false;
        private PreventMonster _preventMonster;
        // -------------Property-------------
        public float Speed { get; set; }
        public float SreachRadius {  get; set; }
        public IEntity HurtBox { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        // ----------------------------------
        public bool IgnorePlayer = false;
        private bool _isHit;
        private float _hitTimer = 0f;
        public bool isHit
        {
            get => _isHit;
            set
            {
                if (value)
                {
                    _isHit = true;
                    _hitTimer = 1f; // start 1 second timer
                }
            }
        }
        private bool _isAttack;
        private float _attackCD;
        public bool isAttack 
        {
            get => _isAttack;
            set 
            {
                if (value)
                {
                    _isAttack = true;
                    _attackCD = 2f;
                }
            }
        }
        private int _HP;
        public int HP { 
            get => _HP;
            set 
            {
                _HP = value;
                if (_HP <= 0)
                {
                    Debug.WriteLine("Dead");
                }
            } 
        }


        private AnimController monster;

        public MonsterMelee(Vector2 position, PreventMonster preventMonster)
        {
            Position = position;
            SpawnPosition = Position;
            _preventMonster = preventMonster;
        }
        public void LoadAnim(string textureName, Vector2 position, int width, int height, ContentManager content)
        {
            Width = width;
            Height = height;
            monster = new AnimController(textureName, position, width, height);
            monster.LoadFrame(content);
            Center = new Vector2(monster.TextureWidth /2, monster.TextureHeight/2); // Centerize: maybe tempo, not sure
        }
        public void LoadAnim(AnimController animController, ContentManager content)
        {
            LoadAnim(animController.TextureName, animController.Position, animController.TextureWidth, animController.TextureHeight, content);
        }
        public void CreateAnimation()
        {
            monster.CreateAnimation("idle", true, 1, 12, 15); // temporary
            monster.CreateAnimation("attack", false, 12, 0, 4); // temporary
            monster.CreateAnimation("Walk/down", true, 1, 0, 3);
            monster.CreateAnimation("Walk/left", true, 1, 4, 7);
            monster.CreateAnimation("Walk/right", true, 1, 8, 11);
            monster.CreateAnimation("Walk/up", true, 1, 12, 15);
            // Update In future
            //monster.CreateAnimation("Idle/down", true, 0.5f, 0, 3);
            //monster.CreateAnimation("Idle/left", true, 0.5f, 4, 7);
            //monster.CreateAnimation("Idle/right", true, 0.5f, 8, 11);
            //monster.CreateAnimation("Idle/up", true, 0.5f, 12, 15);
            //monster.CreateAnimation("Attack/down", true, 0.5f, 0, 3);
            //monster.CreateAnimation("Attack/left", true, 0.5f, 4, 7);
            //monster.CreateAnimation("Attack/right", true, 0.5f, 8, 11);
            //monster.CreateAnimation("Attack/up", true, 0.5f, 12, 15); 
        }
        // Need Change in future
        public void SetProperty(float speed, float sreachRadius, int hp)
        {
            SetProperty(
                speed, 
                sreachRadius, 
                new MonsterHurtbox(new RectangleF(Position, new SizeF(Width, Height)), this),
                hp
                );
        }
        public void SetProperty(float speed, float sreachRadius, IEntity hurtBox, int hp)
        {
            Speed = speed;
            SreachRadius = sreachRadius;
            HurtBox = hurtBox;
            HP = hp;
        }
        public void UpdateState(GameTime gameTime, List<IEntity> collisions, CollisionComponent collisionComponents, Vector2 targetPosition)
        {
            HurtBox.Bounds.Position = Position - Center; // Centerize: maybe tempo, not sure
            TargetPos = targetPosition;
            float deltaTime = gameTime.GetElapsedSeconds();

            StateController(deltaTime, collisions, collisionComponents);
            DeleteHitBox(deltaTime, collisions, collisionComponents);
            UpdateHitTimer(deltaTime);
            monster.UpdateFrame(gameTime, Position - Center); // Draw // Centerize: maybe tempo, not sure
        }
        private float _blinkTimer = 0f; // timer สำหรับ blink
        private const float BlinkInterval = 0.1f; // กระพริบทุก 0.1 วินาที (ปรับได้)
        public void DrawMonster(SpriteBatch spriteBatch)
        {
            bool shouldFlash = _isHit && (_blinkTimer < BlinkInterval);
            Color tint = shouldFlash ? Color.White * 0.5f : Color.White; // transparent and normal
            monster.DrawFrame(spriteBatch, tint);
            //monster.DrawFrame(spriteBatch);
        }
        public void MoveTo(float deltaTime, Vector2 position)
        {
            Vector2 direction = position - Position;
            direction.Normalize();
            if (Speed == 0) Speed = 1f;
            Vector2 movement = direction * Speed * deltaTime;
            Position += movement;
            PlayAnimaion("Walk" ,GetDirection(direction));
        }
        //public void BackToSpawn(float deltaTime)
        //{
        //    Vector2 direction = SpawnPosition - Position;
        //    direction.Normalize();
        //    Position += direction * Speed * deltaTime;
        //    PlayAnimaion("Walk", GetDirection(direction));
        //}
        //public string GetDirection(Vector2 direction)
        //{
        //    var angle = Math.Acos(direction.X) * (180/Math.PI); 
        //    Debug.WriteLine(angle);

        //    if (direction.X >= -0.7 && direction.X <= 0.7 && direction.Y <= 0.7)
        //        return "up";
        //    else if (direction.X >= -0.7 && direction.X <= 0.7 && direction.Y > -0.7)
        //        return "down";
        //    else if (direction.Y < 0.7 && direction.Y >= -0.7 && direction.X >= 0.7)
        //        return "right";
        //    else if (direction.Y <= 0.7 && direction.Y >= -0.7 && direction.X <= -0.7)
        //        return "left";
        //    return null;
        //}
        public string GetDirection(Vector2 direction)
        {
            if (direction.LengthSquared() == 0)
                return null;

            float angle = MathF.Atan2(direction.Y, direction.X) * (180f / MathF.PI);
            if (angle < 0) angle += 360f;

            if (angle >= 45 && angle < 135) return "down";
            if (angle >= 135 && angle < 225) return "left";
            if (angle >= 225 && angle < 315) return "up";
            return "right";
        }

        public void CreateHitbox(string direction, List<IEntity> collisions, CollisionComponent collisionComponents)
        {
            const float ttl = 0.1f; // 100 ms
            SizeF size = new SizeF(Width * 1.2f, Height * 1.5f);

            // Shift so Position is treated as center
            Vector2 topLeft = Position - new Vector2(Width / 2f, Height / 2f);

            switch (direction)
            {
                case "up":
                    var hb = new MonsterAttackHitbox(
                        new RectangleF(new Vector2((topLeft.X + Width / 2) - size.Height / 2, topLeft.Y - size.Width),
                        new SizeF(size.Height, size.Width)), ttl);
                    collisions.Add(hb);
                    collisionComponents.Insert(hb);
                    break;

                case "down":
                    hb = new MonsterAttackHitbox(
                        new RectangleF(new Vector2((topLeft.X + Width / 2) - size.Height / 2, topLeft.Y + Height),
                        new SizeF(size.Height, size.Width)), ttl);
                    collisions.Add(hb);
                    collisionComponents.Insert(hb);
                    break;

                case "right":
                    hb = new MonsterAttackHitbox(
                        new RectangleF(new Vector2(topLeft.X + Width, (topLeft.Y + Height / 2) - size.Height / 2),
                        size), ttl);
                    collisions.Add(hb);
                    collisionComponents.Insert(hb);
                    break;

                case "left":
                    hb = new MonsterAttackHitbox(
                        new RectangleF(new Vector2(topLeft.X - size.Width, (topLeft.Y + Height / 2) - size.Height / 2),
                        size), ttl);
                    collisions.Add(hb);
                    collisionComponents.Insert(hb);
                    break;
            }
        }

        public void StateController(float deltaTime, List<IEntity> collisions, CollisionComponent collisionComponents)
        {
            AnimatedSprite animSprite = monster.AnimSprite;
            float preventMonsterEdge = Vector2.Distance(Position,_preventMonster.Position) - _preventMonster.Radius;
            bool inAttackList = _preventMonster.ActiveAttacker.Contains(this); 
            bool isAwayHome = Vector2.Distance(Position, SpawnPosition) > 500f;
            bool inDetect = Vector2.Distance(Position, TargetPos) <= SreachRadius;
            bool inAttack = !(Math.Abs(Position.X - TargetPos.X) > Width * 1.2f || Math.Abs(Position.Y - TargetPos.Y) > Height * 1.2f) && inDetect;
            bool inWander = Vector2.Distance(Position, TargetPos) > SreachRadius && Vector2.Distance(Position, SpawnPosition) > Width;
            bool isAttacking = animSprite.CurrentAnimation == "attack";

            if (isAwayHome)
            {
                IgnorePlayer = true;
            }            
            if (inDetect || inAttack && !IgnorePlayer) // if player is in detect range or in attack range and not ignoring player will wait to return
            {
                WaitingToReturn = false;
                IsReturning = false;
            }
            if (preventMonsterEdge >= 1f) _preventMonster.ActiveAttacker.Remove(this); // Remove from active attacker if out of preventMonsterEdge range
            

            if (IgnorePlayer) // ---------------------------------------------------------------------------- Ignore Player State
            {
                inDetect = false;
                IsReturning = true;
            }
            else if (inAttack && !isAttacking) // ----------------------------------------------------------- Attack State
            {
                if (!isAttack)
                {
                    _preventMonster.ActiveAttacker.Add(this); // Add to active attacker when in Attack State
                    isAttack = true;
                    animSprite.SetAnimation("attack").OnAnimationEvent += (sender, trigger) =>
                    {
                        if (trigger == AnimationEventTrigger.AnimationCompleted)
                        {
                            Vector2 direction = TargetPos - Position;
                            direction.Normalize();
                            CreateHitbox(GetDirection(direction), collisions, collisionComponents);
                            animSprite.SetAnimation("idle");
                        }
                    };
                }
            }
            else if (inDetect && !isAttacking) // ----------------------------------------------------------- Chasing State
            {
                if (!isAttack && inAttackList)
                {
                    MoveTo(deltaTime, TargetPos);
                }
                else if (!isAttack &&  preventMonsterEdge >= 1f) MoveTo(deltaTime, TargetPos);
                else animSprite.SetAnimation("idle");
            }
            else if (inWander && !isAttacking) // ----------------------------------------------------------- Wander State
            {
                if (!WaitingToReturn && !IsReturning)
                {
                    WaitingToReturn = true;
                    WanderTimer = 2f;
                }

                if (WaitingToReturn)
                {
                    if (animSprite.CurrentAnimation != "idle")
                        animSprite.SetAnimation("idle");
                    WanderTimer -= deltaTime;
                    if (WanderTimer <= 0f)
                    {
                        WaitingToReturn = false;
                        IsReturning = true;

                    }
                }
            }

            if (IsReturning)
            {
                MoveTo(deltaTime, SpawnPosition);
                Debug.WriteLine("Returning to Spawn Position");
                Debug.WriteLine(inWander);
                if (!inWander)
                {
                    IsReturning = false;
                    IgnorePlayer = false;
                    animSprite.SetAnimation("idle");
                }
            }
        }
        public void DeleteHitBox(float deltaTime,List<IEntity> entities, CollisionComponent collisionComponent)
        {
            foreach (var hb in entities.OfType<MonsterAttackHitbox>().ToList())
            {
                hb.TimeToLiveSeconds -= deltaTime;
                if (hb.TimeToLiveSeconds <= 0f)
                {
                    entities.Remove(hb);
                    collisionComponent.Remove(hb);
                }
            }
        }
        public void UpdateHitTimer(float deltaTime)
        {
            if (_isHit)
            {
                _hitTimer -= deltaTime;
                _blinkTimer += deltaTime; 
                if (_blinkTimer >= BlinkInterval * 2) _blinkTimer = 0f; 
                if (_hitTimer <= 0f)
                {
                    _isHit = false;
                    _hitTimer = 0f;
                    _blinkTimer = 0f;
                }
            }
            if (_isAttack)
            {
                _attackCD -= deltaTime;
                if (_attackCD <= 0f)
                {
                    _isAttack = false;
                    _attackCD = 0f;
                }
            }
        }

        // Summary how to use : 
            //PlayAnimaion("Walk" ,GetDirection(direction));
        public void PlayAnimaion(string animationType,string direction)
        {
            switch (direction)
            {
                case "up":
                    monster.ChangeAnim(animationType + "/up");
                    break;
                case "down":
                    monster.ChangeAnim(animationType + "/down");
                    break;
                case "right":
                    monster.ChangeAnim(animationType + "/right");
                    break;
                case "left":
                    monster.ChangeAnim(animationType + "/left");
                    break;
            }
        }
    }
}
