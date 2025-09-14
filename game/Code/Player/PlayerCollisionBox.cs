using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;

namespace game
{
    public class PlayerCollisionBox : IEntity
    {
        public IShapeF Bounds { get; private set; }
        public string LayerName { get; set; }
        public Player Player { get; private set; }

        private Vector2 _offset; // manual offset from player position
        private Vector2 _size;   // manual size of collision box

        public PlayerCollisionBox(Player player, Vector2 size, Vector2 offset)
        {
            Player = player;
            _size = size;
            _offset = offset;

            Bounds = new RectangleF(Player._movement.Position + _offset, _size);
        }

        public void Update()
        {
            if (Bounds is RectangleF rect)
            {
                rect.Position = Player._movement.Position + _offset;
                Bounds = rect;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawRectangle((RectangleF)Bounds, Color.Yellow, 2);
        }

        public void OnCollision(CollisionEventArgs collisionInfo)
        {
            if (collisionInfo.Other is Wall)
            {
                Bounds.Position -= collisionInfo.PenetrationVector;
                Player._movement.SetPosition(Bounds.Position - _offset);
            }

            if (collisionInfo.Other is MonsterCollision)
            {
                Bounds.Position -= collisionInfo.PenetrationVector;
                Player._movement.SetPosition(Bounds.Position - _offset);
            }
        }
    }
}
