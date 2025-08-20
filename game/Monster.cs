using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Threading;
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
        // -------------Property-------------
        public float Speed { get; set; }
        public float SreachRadius { get; set; }
        public IEntity HurtBox { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public float ActiveRadius { get; set; } = 150f;
        public float AwaySpawnRadius { get; set; } = 500f;
        // ----------------------------------
        public Vector2 Origin { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 TargetPos { get; set; }
        public Vector2 SpawnPosition { get; set; }
        public float WanderTimer { get; set; } = 0f;
        private const float _blinkInterval = 0.1f;
        private float _blinkTimer = 0f;
        private float _knockBackTimer = 0f, _knockBackForce = 0f;
        private Vector2 _knockBackDirection = Vector2.Zero;
        private List<IEntity> _collisions;
        private CollisionComponent _collisionComponents;
        private PreventMonster _preventMonster;
        private AnimController monster;
        public bool ShakeViewport = false;

        // ----------------Bool----------------
        public bool WaitingToReturn { get; set; } = false;
        public bool IsReturning { get; set; } = false;

        public bool IgnorePlayer = false;
        private bool _isHit;
        private float _hitTimer = 0f;
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
                    ApplyKnockback(250f); 
                }
            }
        }
        private bool _isAttack;
        private float _attackCD;
        public bool isAttack // togle attack state; check if monster is attacking
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
        public bool IsDead => HP <= 0;
        private int _HP;
        public int HP
        {
            get => _HP;
            set
            {
                _HP = value;
                if (_HP <= 0)
                {
                    _HP = 0;
                }
            }
        }
        // ----------------------------------



        public MonsterMelee(Vector2 position, PreventMonster preventMonster)
        {
            Position = position;
            SpawnPosition = position;
            _preventMonster = preventMonster;
        }
        public void LoadAnim(string spriteSheetName, string textureName, Vector2 position, int width, int height, ContentManager content)
        {
            if (Width == 0 || Height == 0)
            {
                Width = width;
                Height = height;
            }
            if (monster == null)
            {
                monster = new AnimController(position);
            }
            monster.LoadFrame(content, spriteSheetName, textureName, width, height);
        }

        /*
         IMPORTANT NOTE: Need to change in future
         Based on the animation sprite sheet
        */
        public void CreateAnimation()
        {
            monster.CreateAnimation("Idle", "down", true, 12, 0, 8); // temporary
            monster.CreateAnimation("Idle", "up", true, 12, 0, 8); // temporary
            monster.CreateAnimation("Idle", "left", true, 12, 0, 8); // temporary
            monster.CreateAnimation("Idle", "right", true, 12, 0, 8); // temporary

            monster.CreateAnimation("Walk", "left", true, 12, 0, 4);
            monster.CreateAnimation("Walk", "right", true, 12, 4, 4);
            monster.CreateAnimation("Walk", "down", true, 12, 8, 4);
            monster.CreateAnimation("Walk", "up", true, 12, 12, 4);
            monster.CreateAnimation("Walk", "attack", false, 12, 12, 4);
        }
        // Need Change in future
        public void SetProperty(float speed, float sreachRadius, int hp)
        {
            SetProperty(
                speed,
                sreachRadius,
                new MonsterHurtbox(
                    /*new RectangleF(Position, new SizeF(Width, Height)*/
                    monster.AnimSprite["Walk"].GetBoundingRectangle(new Transform2(monster.Position, 0f, Vector2.One)),
                this),
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
            if (_collisions == null || _collisionComponents == null)
            {
                _collisions = collisions;
                _collisionComponents = collisionComponents;
            }

            var hurtBox = HurtBox as MonsterHurtbox;
            TargetPos = targetPosition;
            float deltaTime = gameTime.GetElapsedSeconds();

            if (monster != null)
            {
                hurtBox.Update(Position);
                StateController(deltaTime, collisions, collisionComponents);
                DeleteHitBox(deltaTime, collisions, collisionComponents);
                UpdateHitTimer(deltaTime);
                monster.UpdateFrame(gameTime, Position); // Draw  
            }
        }
        public void DrawMonster(SpriteBatch spriteBatch)
        {
            if (monster != null)
            {
                bool shouldFlash = _isHit && (_blinkTimer < _blinkInterval);
                Color tint = shouldFlash ? Color.Red : Color.White; // transparent and normal
                monster.DrawFrame(spriteBatch, tint);
            }
        }

        public void UnLoad()
        {
            monster.Unload(OnAnimationEvent);
        }
        public void MoveTo(float deltaTime, Vector2 position)
        {
            Vector2 direction = position - Position;
            direction.Normalize();
            if (Speed == 0) Speed = 1f;
            Vector2 movement = direction * Speed * deltaTime;
            Position += movement;
            monster.SetAnimation("Walk", GetDirection(direction));
        }

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
            var bounds = HurtBox.Bounds.BoundingRectangle;
            SizeF size = new SizeF(bounds.Width * 1.2f, bounds.Height * 1.5f); // Hitbox size; size of sprite * 1.2f and 1.5f

            //Vector2 topLeft = Position - new Vector2(Width / 2f, Height / 2f); // old method maybe useful in future 
            Vector2 topLeft = bounds.TopLeft; // Shift Position to topleft; Because old positon was based on topleft position but now position is center
            switch (direction)
            {
                /*
                    Calculate logic :
                        Up :
                            X: First calculate the center of the sprite (topLeft.X + Width / 2) 
                            and then subtract half of the hitbox width (size.Height / 2) (use size.Height instead of width because it rotated) 
                            Y: Subtract the hitbox height (size.Width) from the topLeft.Y
                        Down :  
                            for Down logic is reverse of Up logic or similar to Up logic
                        Right :
                            X: Add the Width to the topLeft.X to get the right edge of the sprite
                            Y: First calculate the center of the right side sprite (topLeft.Y + Height / 2)
                            and then subtract half of the hitbox height (size.Height / 2)
                        Left :
                            for eft logic is reverse of right logic or similar to right logic
                */
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

        /*
        IMPORTANT NOTE:
        not sure if this is the best way to handle state controller and code is too complicated and hard to read
        maybe need to optimize in future
        */
        public void StateController(float deltaTime, List<IEntity> collisions, CollisionComponent collisionComponents)
        {
            // ----------------bool varbles------------------
            var bounds = HurtBox.Bounds.BoundingRectangle;
            Width = (int)bounds.Width;
            Height = (int)bounds.Height;
            float preventMonsterEdge = Vector2.Distance(Position, _preventMonster.Position) - _preventMonster.Radius;
            bool inAttackList = _preventMonster.ActiveAttacker.Contains(this);
            bool isAwayHome = Vector2.Distance(Position, SpawnPosition) > AwaySpawnRadius;
            bool inDetect = Vector2.Distance(Position, TargetPos) <= SreachRadius;
            bool inAttack = !(Math.Abs(Position.X - TargetPos.X) > Width * 1.2f || Math.Abs(Position.Y - TargetPos.Y) > Height * 1.2f) && inDetect;
            bool inWander = Vector2.Distance(Position, TargetPos) > SreachRadius || Vector2.Distance(Position, SpawnPosition) > Width;
            Vector2 direction = TargetPos - Position;
            direction.Normalize();
            //bool isAttacking = monster.CurrentSpriteSheet == "Attack";
            // ------------------------------------------

            if (isAwayHome)
            {
                IgnorePlayer = true;
            }
            if (inDetect || inAttack && !IgnorePlayer)
            {
                WaitingToReturn = false;
                IsReturning = false;
            }
            if (preventMonsterEdge >= 1f) _preventMonster.RemoveMonster(this); // Remove from active attacker if out of preventMonsterEdge range


            if (IgnorePlayer) // - --------------------------------------------------------------------------- Ignore Player State
            {
                // ignore player and move to spawn position
                inDetect = false;
                IsReturning = true;
            }
            else if (inAttack && !isAttack) // ----------------------------------------------------------- Attack State
            {
                // attack player if in attack range 
                isAttack = true;
                monster.SetAnimation("Walk", "attack", OnAnimationEvent); // temporary attack animation 
            }
            else if (inDetect && !isAttack) // ----------------------------------------------------------- Chasing State
            {
                /*
                Move to player only when inAttackList 
                if not will check if near player enough will add to activeAttacker list
                if not will move to preventMonsterEdge instead
                if at preventMonsterEdge will Idle instead
                */
                if (inAttackList)
                {
                    MoveTo(deltaTime, TargetPos);
                }
                else if (Vector2.Distance(Position, TargetPos) <= ActiveRadius)
                {
                    if (!_preventMonster.ActiveAttacker.Contains(this))
                        _preventMonster.ActiveAttacker.Add(this);
                }
                else if (preventMonsterEdge >= 1f) MoveTo(deltaTime, TargetPos);
                else monster.SetAnimation("Idle", GetDirection(direction));
            }
            else if (inWander && !isAttack) // ----------------------------------------------------------- Wander State
            {
                /*
                will Idle and wait to return if not in detect range or in attack range
                if wait end will return to spawn position
                */
                if (!WaitingToReturn && !IsReturning)
                {
                    WaitingToReturn = true;
                    WanderTimer = 2f;
                }

                if (WaitingToReturn)
                {
                    monster.SetAnimation("Idle", GetDirection(direction)); 
                    WanderTimer -= deltaTime;
                    if (WanderTimer <= 0f)
                    {
                        WaitingToReturn = false;
                        IsReturning = true;
                    }
                }
            }

            // Return Logic
            if (IsReturning)
            {
                MoveTo(deltaTime, SpawnPosition);
                if (Vector2.Distance(Position, SpawnPosition) <= Width)
                {
                    IsReturning = false;
                    IgnorePlayer = false;
                    monster.SetAnimation("Walk", GetDirection(direction));
                }
            }
        }
        public void DeleteHitBox(float deltaTime, List<IEntity> entities, CollisionComponent collisionComponent)
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
                if (_blinkTimer >= _blinkInterval * 2) _blinkTimer = 0f;
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
            if (IsKnockBack())
            {
                _knockBackTimer -= deltaTime;
                _knockBackForce *= MathF.Pow(0.1f, deltaTime);
                Position += _knockBackDirection * _knockBackForce * deltaTime;
                //Debug.WriteLine($"Knockback Force:" + _knockBackDirection * _knockBackForce * deltaTime
                //    + "\n DeltaTime : " + deltaTime + "\n Force : " + _knockBackForce + "\n Direction : " + _knockBackDirection);
            }
        }
        public void OnAnimationEvent(IAnimationController sender, AnimationEventTrigger trigger)
        {
            if (monster.CurrentSpriteSheet == "Walk" && trigger == AnimationEventTrigger.AnimationCompleted)
            {
                Vector2 direction = TargetPos - Position;
                direction.Normalize();
                CreateHitbox(GetDirection(direction), _collisions, _collisionComponents);
                monster.SetAnimation("Idle", GetDirection(direction));
            }
        }
        public void RemoveMonster()
        {
            _preventMonster.RemoveMonster(this);
            _collisions.Remove(HurtBox);
            _collisionComponents.Remove(HurtBox);
            monster.Unload(OnAnimationEvent);
            monster = null;
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
        public void DropHeal(List<IEntity> entities, CollisionComponent collisionComponent, Texture2D texture, Player player)
        {
            Random r = new Random();
            if (r.Next(1, 101) > 100 - 75) // Percentage, Ex: 75 mean 75%
            {
                entities.Add(new HealDrops(
                                new RectangleF(
                                    monster.Position,
                                    new SizeF(texture.Width, texture.Height)
                                    ),
                                texture,
                                player
                            )); // Add drops
                collisionComponent.Insert(entities.Last());
            }
        }
    }
}





// old Method
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

//// Summary how to use : 
//    //PlayAnimaion("Walk" ,GetDirection(direction));
//public void PlayAnimaion(string spriteSheet,string direction)
//{
//    switch (direction)
//    {
//        case "up":
//            monster.SetAnimation(spriteSheet, "up");
//            break;
//        case "down":
//            monster.SetAnimation(spriteSheet, "down");
//            break;
//        case "right":
//            monster.SetAnimation(spriteSheet, "right");
//            break;
//        case "left":
//            monster.SetAnimation(spriteSheet, "left");
//            break;
//    }
//}
