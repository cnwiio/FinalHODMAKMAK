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
            spritebatch.DrawCircle((CircleF)Bounds, 16, Color.Coral, 3);
        }
        public void OnCollision(CollisionEventArgs collisionInfo)
        {
            if (collisionInfo.Other is MonsterHurtbox hurtbox && hurtbox.Monster is MonsterMelee)
            {
                var monster = hurtbox.Monster as MonsterMelee;
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
