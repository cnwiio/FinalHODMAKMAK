using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;

namespace game
{
    public class BoxCollision : IEntity
    {
        public Vector2 Velocity;
        public IShapeF Bounds { get; }
        public BoxCollision(RectangleF rectangleF)
        {
            Bounds = rectangleF;
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawRectangle((RectangleF)Bounds, Color.Black, 3);
        }

        //public virtual void Update(GameTime gameTime)
        //{
        //}

        public void OnCollision(CollisionEventArgs collisionInfo)
        {
            // no logic yet
        }

    }
}
