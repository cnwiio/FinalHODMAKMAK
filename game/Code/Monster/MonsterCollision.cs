using System.Diagnostics;
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
                if (collisionInfo.Other is Wall wall)
                {
                    var direction = collisionInfo.PenetrationVector;
                    direction.Normalize();
                    var rect = (RectangleF)wall.Bounds;
                    var pos = Monster.DesiredPosition;
                    var col = (RectangleF)Bounds;
                    if (direction.Y > 0) // colide from Top
                    {
                        pos = new Vector2(pos.X, rect.Top - Monster.Height / 2);
                    }
                    if (direction.Y < 0) // colide from Bottom
                    {
                        pos = new Vector2(pos.X, rect.Bottom - Monster.Height / 2 + col.Height);
                    }
                    if (direction.X < 0) // colide from Right
                    {
                        pos = new Vector2(rect.Right + col.Width /2 , pos.Y);
                    }
                    if (direction.X > 0) // colide from Left
                    {
                        pos = new Vector2(rect.Left - col.Width /2 , pos.Y);
                    }
                    Monster.DesiredPosition = pos;
                    //Debug.WriteLine("Snaped = " + Monster.DesiredPosition);
                }
                //if (collisionInfo.Other is MonsterCollision friend)
                //{
                //    if (!Monster.isHit && !(friend.Monster.CurrentState is ReturnState))
                //    {
                //        Monster.DesiredPosition -= collisionInfo.PenetrationVector;
                //    }
                //}
            }
        }
    }
}
