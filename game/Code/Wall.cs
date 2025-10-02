using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;

namespace game
{
    public class Wall : IEntity
    {
        public IShapeF Bounds { get; set; }
        public string LayerName { get; set; }
        public bool AlwaysDraw => true;
        public Wall(RectangleF bounds)
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
}
