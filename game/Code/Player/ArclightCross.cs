using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using System;

namespace game
{
    public class ArclightCross
    {
        private Player _player;
        private CollisionComponent _collisionComponent;

        // Adjustable parameters
        public float HorizontalWidth { get; set; } = 60f;
        public float HorizontalHeight { get; set; } = 120f;
        public float VerticalWidth { get; set; } = 120f;
        public float VerticalHeight { get; set; } = 60f;
        public float Speed { get; set; } = 3000f;
        public float Duration { get; set; } = 0.125f;
        public float HitDelay { get; set; } = 0.2f;

        // Damage is calculated from player's AttackPower
        public int Damage => (int)(_player.Stats.AttackPower.Value * 2f);
        public ArclightCross(Player player, CollisionComponent collisionComponent)
        {
            _player = player;
            _collisionComponent = collisionComponent;
        }

        public void Use()
        {
            Vector2 dir = _player._movement.Direction != Vector2.Zero
                ? _player._movement.Direction
                : _player.LastDirection; // default down

            Vector2 startPos = _player._movement.Position;

            float width = dir.X != 0 ? HorizontalWidth : VerticalWidth;
            float height = dir.X != 0 ? HorizontalHeight : VerticalHeight;

            var hitbox = new ArclightCrossHitbox(
                _player,
                startPos,
                dir,
                Speed,
                width,
                height,
                Duration,
                _collisionComponent,
                maxHitsPerMonster: 1,
                HitDelay,
                Damage
            );

            // Add to player’s active hitboxes and world entities
            _player._activeHitboxes.Add(hitbox);
            _player._entities?.Add(hitbox);
            _collisionComponent?.Insert(hitbox);
        }
    }
}
