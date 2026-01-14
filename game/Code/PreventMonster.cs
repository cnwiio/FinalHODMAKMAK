using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using System.Collections.Generic;

namespace game
{
    public class PreventMonster : IEntity
    {
        public IShapeF Bounds { get; set; }
        public Vector2 Position { get; set; }
        public float Radius { get; set; }
        public string LayerName { get; set; }
        public Game1 Game { get; set; }
        public List<IMonster> ActiveAttacker { get; private set; } = new List<IMonster>();
        public const short MAXATTACKER = 2;
        public bool AlwaysDraw => true;
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
            //spritebatch.DrawCircle((CircleF)Bounds, 16, Color.Coral, 3);
        }
        public void OnCollision(CollisionEventArgs collisionInfo)
        {
            if (collisionInfo.Other is MonsterHurtbox hurtbox && (hurtbox.Monster is MonsterMelee || hurtbox.Monster is MonsterSlime))
            {
                var monster = hurtbox.Monster;
                if (!ActiveAttacker.Contains(monster) && ActiveAttacker.Count < MAXATTACKER)
                {
                    ActiveAttacker.Add(monster);
                }
            }
        }

        public void RemoveMonster(IMonster monster)
        {
            if (ActiveAttacker.Contains(monster))
            {
                ActiveAttacker.Remove(monster); 
            }
        }
    }
}
