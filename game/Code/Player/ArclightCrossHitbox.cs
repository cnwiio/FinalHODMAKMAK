using Microsoft.Xna.Framework;
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
            int damage
        ) : base(player, new RectangleF(startPosition.X - width / 2, startPosition.Y - height / 2, width, height),
                lifetime, collisionComponent, damage, maxHitsPerMonster, hitDelay)
        {
            _direction = direction != Vector2.Zero ? Vector2.Normalize(direction) : Vector2.UnitY;
            _speed = speed;

            _rect = new RectangleF(startPosition.X - width / 2, startPosition.Y - height / 2, width, height);
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
    }
}
