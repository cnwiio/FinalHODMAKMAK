using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;

namespace game
{
    public class HealPickup : IEntity, ICollisionActor
    {
        public IShapeF Bounds { get; private set; }
        private Texture2D _texture;
        private int _healAmount;
        private Player _player;

        public HealPickup(Vector2 position, Texture2D texture, Player player, int healAmount = 25)
        {
            _texture = texture;
            _player = player;
            _healAmount = healAmount;
            Bounds = new RectangleF(position, new SizeF(texture.Width, texture.Height));
        }

        public void OnCollected()
        {
            _player.Stats.HP.AddModifier(_healAmount);

            if (_player.Stats.HP.Value > _player.Stats.HP.BaseValue)
            {
                int excess = _player.Stats.HP.Value - _player.Stats.HP.BaseValue;
                _player.Stats.HP.RemoveModifier(excess);
            }
        }

        public void OnCollision(CollisionEventArgs collisionInfo)
        {
            // nothing here
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, ((RectangleF)Bounds).Position, Color.White);
        }
    }

}
