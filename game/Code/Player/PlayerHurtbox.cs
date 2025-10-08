using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Collisions.Layers;
using SharpDX;
using System.Collections.Generic;
using System.Diagnostics;

namespace game
{
    public class PlayerHurtbox : IEntity
    {
        public IShapeF Bounds { get; set; }
        public string LayerName { get; set; }
        public Player _player { get; set; }
        public bool AlwaysDraw => true;
        private float _invincibleTimer = 0f;
        private float _invincibleDuration = 0.3f; // 0.3 seconds i-frame
        private bool canPlaySound = false;
        public PlayerHurtbox(Player player, float width, float height)
        {
            _player = player;
            // Centered on player
            Bounds = new RectangleF(_player._movement.Position - new Vector2(width / 2, height / 2), new SizeF(width, height));
        }

        // Update hurtbox position to match player
        public void Update(GameTime gameTime)
        {
            var rect = (RectangleF)Bounds;
            rect.Position = _player._movement.Position - (rect.Size / 2f); // center on player
            Bounds = rect;

            if (_invincibleTimer > 0)
                _invincibleTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        }


        public void Draw(SpriteBatch spriteBatch)
        {
            var rect = (RectangleF)Bounds;
            spriteBatch.DrawRectangle(rect, Color.Blue, 2); // Draw in blue for hurtbox
        }

        public void OnCollision(CollisionEventArgs collisionInfo)
        {
            if (_player._movement.IsDashing || _invincibleTimer > 0) return; // still invincible or dashing

            if (collisionInfo.Other is MonsterAttackHitbox monster)
            {
                // Reduce CurrentHP, not Stat.Value
                _player.Stats.TakeDamage(monster.Monster.Damage);

                // Start i-frames
                _invincibleTimer = _invincibleDuration;
                canPlaySound = true;

                Debug.WriteLine($"Player took damage! HP: {_player.Stats.CurrentHP}");
            }
        }

        public bool PlaySound()
        {
            if (canPlaySound)
            {
                canPlaySound = false;
                return true;
            }
            return false;
        }
    }

}
