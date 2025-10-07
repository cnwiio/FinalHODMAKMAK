using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;

namespace game
{
    public class PillarOfLight
    {
        private Player _player;
        private CollisionComponent _collisionComponent;

        // Adjustable parameters
        public float Width { get; set; } = 200f;
        public float Height { get; set; } = 200f;
        public float Duration { get; set; } = 0.6f;

        // Damage is calculated from player's AttackPower
        public int Damage => (int)(_player.Stats.AttackPower.Value * 0.5f);

        public PillarOfLight(Player player, CollisionComponent collisionComponent)
        {
            _player = player;
            _collisionComponent = collisionComponent;
        }

        public void Use(Vector2 worldPosition)
        {
            RectangleF skillArea = new RectangleF(
                worldPosition.X - Width / 2,
                worldPosition.Y - Height / 2,
                Width,
                Height
            );

            // Hitbox with 3 hits per monster, 0.3s delay between hits
            var skillHitbox = new PlayerAttackHitbox(
                _player,
                skillArea,
                Duration,
                _collisionComponent,
                Damage,
                maxHitsPerMonster: 3,
                hitDelay: 0.2f
            );

            _player._activeHitboxes.Add(skillHitbox);
            _player._entities?.Add(skillHitbox);
            _collisionComponent?.Insert(skillHitbox);
        }

    }
}
