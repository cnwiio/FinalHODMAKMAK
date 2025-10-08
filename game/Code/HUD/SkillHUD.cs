using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace game
{
    public class SkillHUD
    {
        private Player _player;

        private Vector2 _skill1Position;
        private Vector2 _skill2Position;

        private Texture2D _skill1ReadyTexture;
        private Texture2D _skill1CooldownTexture;
        private Texture2D _skill2ReadyTexture;
        private Texture2D _skill2CooldownTexture;

        private const int IconSize = 128; // as per your assets

        public SkillHUD(Player player, GraphicsDevice graphicsDevice)
        {
            _player = player;

            _skill1Position = new Vector2((graphicsDevice.Viewport.Width/2) - 64 - 120, graphicsDevice.Viewport.Height - 240);
            _skill2Position = new Vector2((graphicsDevice.Viewport.Width/2) - 64 + 120, graphicsDevice.Viewport.Height - 240);
        }

        public void LoadContent(ContentManager content)
        {
            _skill1ReadyTexture = content.Load<Texture2D>("HUD/Skill1Icon_1");
            _skill1CooldownTexture = content.Load<Texture2D>("HUD/Skill1Icon_2");

            _skill2ReadyTexture = content.Load<Texture2D>("HUD/Skill2Icon_1");
            _skill2CooldownTexture = content.Load<Texture2D>("HUD/Skill2Icon_2");
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Skill 1
            Texture2D tex1 = _player._skill1CooldownTimer > 0 ? _skill1CooldownTexture : _skill1ReadyTexture;
            spriteBatch.Draw(tex1, new Rectangle((int)_skill1Position.X, (int)_skill1Position.Y, IconSize, IconSize), Color.White);

            // Skill 2
            Texture2D tex2 = _player._skill2CooldownTimer > 0 ? _skill2CooldownTexture : _skill2ReadyTexture;
            spriteBatch.Draw(tex2, new Rectangle((int)_skill2Position.X, (int)_skill2Position.Y, IconSize, IconSize), Color.White);
        }
    }
}
