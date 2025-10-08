using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using System;

namespace game
{
    public class ArclightCrossHitbox : PlayerAttackHitbox
    {
        private Vector2 _direction;
        private float _speed;
        private RectangleF _rect; // Moving rectangle
        public Vector2 Pos => _rect.Position;

        private Texture2D _texture;
        private ElementType _element;
        private Rectangle _sourceRect; // Slice from texture

        private const int TextureWidth = 512;
        private const int TextureHeight = 256;
        private const int Columns = 4;
        private const int Rows = 2;

        public ArclightCrossHitbox(
            Player player,
            Vector2 startPosition,
            Vector2 direction,
            float speed,
            float width,
            float height,
            float lifetime,
            CollisionComponent collisionComponent,
            int maxHitsPerMonster,
            float hitDelay,
            int damage,
            Texture2D texture,
            ElementType element
        ) : base(player, new RectangleF(startPosition.X - width / 2, startPosition.Y - height / 2, width, height),
                lifetime, collisionComponent, damage, maxHitsPerMonster, hitDelay)
        {
            _direction = direction != Vector2.Zero ? Vector2.Normalize(direction) : Vector2.UnitY;
            _speed = speed;

            _rect = new RectangleF(startPosition.X - width / 2, startPosition.Y - height / 2, width, height);

            _texture = texture;
            _element = element;

            // Determine the column based on direction
            int col = direction.X < 0 ? 0 : direction.X > 0 ? 1 : direction.Y < 0 ? 2 : 3;
            int row = _element == ElementType.Dark ? 0 : 1;

            int frameWidth = TextureWidth / Columns;
            int frameHeight = TextureHeight / Rows;

            _sourceRect = new Rectangle(col * frameWidth, row * frameHeight, frameWidth, frameHeight);
        }

        // Override Bounds so the collision system uses our moving rectangle
        public override IShapeF Bounds => _rect;

        public override void Update(GameTime gameTime)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Move the hitbox
            _rect.Position += _direction * _speed * delta;

            // Call base for lifetime & damage handling
            base.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_texture != null)
            {
                spriteBatch.Draw(_texture, _rect.Position, _sourceRect, Color.White);
            }
        }
    }
}
