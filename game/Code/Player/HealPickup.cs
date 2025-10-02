using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using System.Collections.Generic;
using System.Diagnostics;

namespace game
{
    public class HealPickup : IEntity
    {
        public IShapeF Bounds { get; private set; }
        private Texture2D _texture;
        private int _healAmount;
        private Player _player;
        public string LayerName { get; set; }
        private CollisionComponent _collisionComponent;
        private bool IsActive = true;
        private List<IEntity> _scenePendingRemove;

        public HealPickup(Vector2 position, Texture2D texture, Player player,
            CollisionComponent collisionComponent, List<IEntity> scenePendingRemove, int healAmount = 25)
        {
            _texture = texture;
            _player = player;
            _healAmount = healAmount;
            _collisionComponent = collisionComponent;
            _scenePendingRemove = scenePendingRemove;

            Bounds = new RectangleF(
                position.X - texture.Width / 2f,
                position.Y - texture.Height / 2f,
                texture.Width,
                texture.Height
            );

            _collisionComponent.Insert(this);
        }


        private void Collect()
        {
            if (!IsActive) return;

            // Heal the player
            _player.Stats.HP.AddModifier(_healAmount);

            if (_player.Stats.HP.Value > _player.Stats.HP.BaseValue)
            {
                int excess = _player.Stats.HP.Value - _player.Stats.HP.BaseValue;
                _player.Stats.HP.RemoveModifier(excess);
            }

            Debug.WriteLine($"[HealPickup] Collected! Player HP: {_player.Stats.HP.Value}");

            IsActive = false;

            // Tell the scene to remove me later
            _scenePendingRemove.Add(this);
        }

        public void OnCollision(CollisionEventArgs collisionInfo)
        {
            if (!IsActive) return;

            // Player collides with Heal
            if (collisionInfo.Other is PlayerHurtbox)
            {
                Collect();
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!IsActive) return;

            spriteBatch.Draw(_texture, ((RectangleF)Bounds).Position, Color.White);

            // Debug outline (optional)
            spriteBatch.DrawRectangle((RectangleF)Bounds, Color.Yellow, 2);
        }
    }
}
