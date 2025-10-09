using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace game
{
    public class PotionHUD
    {
        private Player _player;
        private Vector2 _position;
        private Texture2D[] _potionTextures = new Texture2D[Potion.MAXAMOUT + 1];

        public PotionHUD(Player player, GraphicsDevice graphicsDevice)
        {
            _player = player;

            // Position above health bar (adjust Y offset as needed)
            _position = new Vector2(graphicsDevice.Viewport.Width + 332, graphicsDevice.Viewport.Height - 125);
        }

        public void LoadContent(ContentManager content)
        {
            for (int i = 0; i <= Potion.MAXAMOUT; i++)
            {
                _potionTextures[i] = content.Load<Texture2D>($"HUD/HealthPotion_{i}");
            }
        }

        public void Update(GameTime gameTime)
        {
            // Nothing needed here unless you want effects
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            int amount = _player.potion.Amout;

            // Clamp amount to valid range
            amount = MathHelper.Clamp(amount, 0, Potion.MAXAMOUT);

            // Draw the texture corresponding to current potion amount
            Texture2D tex = _potionTextures[amount];
            Vector2 origin = new Vector2(tex.Width / 2f, tex.Height / 2f);
            Color tint = _player.potion.isinCoolDown ? Color.Gray : Color.White;    
            spriteBatch.Draw(tex, _position, null, tint, 0f, origin, 1f, SpriteEffects.None, 0f);
        }
    }
}
