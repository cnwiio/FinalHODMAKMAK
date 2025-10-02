using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
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

        public HealPickup(Vector2 position, Texture2D texture, Player player, CollisionComponent collisionComponent, int healAmount = 25)
        {
            _texture = texture;
            _player = player;
            _healAmount = healAmount;
            _collisionComponent = collisionComponent;

            // Make rectangle centered on position
            Bounds = new RectangleF(position - new Vector2(texture.Width / 2f, texture.Height / 2f),
                                    new SizeF(texture.Width, texture.Height));

            // Insert into collision system
            _collisionComponent.Insert(this);
        }

        public void OnCollected()
        {
            _player.Stats.HP.AddModifier(_healAmount);

            if (_player.Stats.HP.Value > _player.Stats.HP.BaseValue)
            {
                int excess = _player.Stats.HP.Value - _player.Stats.HP.BaseValue;
                _player.Stats.HP.RemoveModifier(excess);
            }

            Debug.WriteLine($"[HealPickup] Collected! Player HP: {_player.Stats.HP.Value}");

            // Remove from collision system after collected
            _collisionComponent.Remove(this);
        }

        public void OnCollision(CollisionEventArgs collisionInfo)
        {
            if (collisionInfo.Other == _player)
            {
                OnCollected();
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, ((RectangleF)Bounds).Position, Color.White);
        }
    }

}
