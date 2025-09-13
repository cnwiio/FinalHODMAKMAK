using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;

namespace game
{
    public class MonsterCollision : IEntity
    {
        public IShapeF Bounds { get; set; }
        public string LayerName { get; set; }
        public IMonster Monster { get; set; }
        public MonsterCollision(RectangleF bounds, IMonster monster)
        {
            Bounds = bounds;
            Monster = monster;
        }
        public void Update(Vector2 position)
        {
            var rect = (RectangleF)Bounds;
            var origin = rect.Size / 2;
            var offset = new Vector2(0, Monster.Height / 2 - origin.Height);
            rect.Position = position - origin + offset;
            Bounds = rect;
        }
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            var rect = (RectangleF)Bounds;
            spriteBatch.DrawRectangle(rect, Color.Red, 3);

            // Draw a small cross at the origin (center) 
            var center = rect.Center;
            float crossSize = 4f;
            spriteBatch.DrawLine(center - new Vector2(crossSize, 0), center + new Vector2(crossSize, 0), Color.BlueViolet, 2);
            spriteBatch.DrawLine(center - new Vector2(0, crossSize), center + new Vector2(0, crossSize), Color.BlueViolet, 2);
        }
        public void OnCollision(CollisionEventArgs collisionInfo)
        {
            var returnState = Monster.CurrentState is ReturnState;
            if (!returnState)
            {
                if (collisionInfo.Other is MonsterCollision friend)
                {
                    if (!Monster.isHit && !(friend.Monster.CurrentState is ReturnState))
                    {
                        Monster.Position -= collisionInfo.PenetrationVector;
                    }
                }
                if (collisionInfo.Other is Wall)
                {
                    Monster.Position -= collisionInfo.PenetrationVector;
                }
            }
        }
    }
}
