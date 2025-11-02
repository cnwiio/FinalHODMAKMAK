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
        public float Duration { get; set; } = 0.2f;
        public float HitDelay { get; set; } = 0.2f;

        // Damage is calculated from player's AttackPower
        public int Damage => (int)(_player.Stats.AttackPower.Value + 40);
        public ArclightCross(Player player, CollisionComponent collisionComponent)
        {
            _player = player;
            _collisionComponent = collisionComponent;
        }

        public void Use()
        {
            // Determine direction (snap to horizontal or vertical)
            Vector2 rawDir = _player._movement.Direction != Vector2.Zero
                ? _player._movement.Direction
                : _player.LastDirection;

            Vector2 dir = SnapDirection(rawDir);

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
                Damage,
                _player.Skill2Texture,   // Texture2D
                _player.CurrentElement
            );

            // Add to player’s active hitboxes and world entities
            _player._activeHitboxes.Add(hitbox);
            _player._entities?.Add(hitbox);
            _collisionComponent?.Insert(hitbox);
        }

        // Helper to snap diagonal directions to pure horizontal or vertical
        private Vector2 SnapDirection(Vector2 dir)
        {
            if (Math.Abs(dir.X) >= Math.Abs(dir.Y))
                return new Vector2(Math.Sign(dir.X), 0); // horizontal
            else
                return new Vector2(0, Math.Sign(dir.Y)); // vertical
        }

    }
}
