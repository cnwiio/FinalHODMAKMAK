using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Animations;
using System;
using MonoGame.Extended.Input.InputListeners;
using System.Diagnostics;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using System.Collections.Generic;

namespace game
{
    public class MonsterAttackHitbox : IEntity
    {
        public IShapeF Bounds { get; set; }
        public string LayerName { get; set; }
        public float TimeToLiveSeconds { get; set; }
        public MonsterAttackHitbox(RectangleF bounds, float timeToLiveSeconds)
        {
            Bounds = bounds;
            TimeToLiveSeconds = timeToLiveSeconds;
        }
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawRectangle((RectangleF)Bounds, Color.Blue, 3);
        }
        public void OnCollision(CollisionEventArgs collisionInfo)
        {

        }
    }
    public class MonsterHurtbox : IEntity {
        public IShapeF Bounds { get; set; }
        public string LayerName { get; set; }
        public MonsterMelee MonsterMelee { get; set; }
        public MonsterHurtbox(RectangleF bounds, MonsterMelee monsterMelee)
        {
            Bounds = bounds;
            MonsterMelee = monsterMelee;
        }
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawRectangle((RectangleF)Bounds, Color.Red, 3);
        }
        public void OnCollision(CollisionEventArgs collisionInfo)
        {
            if (collisionInfo.Other is PlayerAttack)
            {
                if (!MonsterMelee.isHit)
                {
                    MonsterMelee.Position -= collisionInfo.PenetrationVector * 5f;
                    MonsterMelee.isHit = true;
                    MonsterMelee.HP -= 1;
                    Debug.WriteLine(MonsterMelee.HP);
                }
            }
            if (collisionInfo.Other is MonsterHurtbox)
            {
                if (!MonsterMelee.isHit)
                {
                    MonsterMelee.Position -= collisionInfo.PenetrationVector;
                }
            }
        }
    }
    public class PlayerAttack : IEntity
    {
        public IShapeF Bounds { get; set; }
        public string LayerName { get; set; }
        public PlayerAttack(RectangleF bounds)
        {
            Bounds = bounds;
        }
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawRectangle((RectangleF)Bounds, Color.Red, 3);
        }
        public void OnCollision(CollisionEventArgs collisionInfo)
        {
        }
    }
    public class PreventMonster : IEntity
    {
        public IShapeF Bounds { get; set; }
        public Vector2 Position { get; set; }
        public float Radius { get; set; }
        public string LayerName { get; set; }
        public Game1 Game { get; set; }
        public List<MonsterMelee> ActiveAttacker { get; private set; } = new List<MonsterMelee>();
        public const int MAXATTACKER = 1;
        public PreventMonster(Vector2 position, float radius)
        {
            Position = position;
            Radius = radius;
            Bounds = new CircleF(position, radius);
        }
        public void UpdatePosition(Vector2 position)
        {
            Position = position;
            Bounds.Position = Position;
        }
        public virtual void Draw(SpriteBatch spritebatch)
        {
            spritebatch.DrawCircle((CircleF)Bounds, 16, Color.Blue, 3);
        }
        public void OnCollision(CollisionEventArgs collisionInfo)
        {
            if (collisionInfo.Other is MonsterHurtbox hurtbox)
            {
                var monster = hurtbox.MonsterMelee;
                if (!ActiveAttacker.Contains(monster) && ActiveAttacker.Count < MAXATTACKER)
                {
                    ActiveAttacker.Add(monster);
                }
            }
        }

        public void RemoveMonster(MonsterMelee monster)
        {
            ActiveAttacker.Remove(monster); 
        }
    }
}
