using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assimp;
using game;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Screens.Transitions;

namespace game
{
    public class SceneVictory : GameScreen
    {
        private SpriteBatch _spriteBatch;
        private KeyboardState _ks, _oldKs;
        private MouseState _ms, _oms;
        private Texture2D BG;
        private Button Button;
        private SpriteFont font;
        private Game1 game;
        public SceneVictory(Game game) : base(game)
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            this.game = (Game1)game;
        }

        public override void LoadContent()
        {
            BG = Content.Load<Texture2D>("Texture/Victory");
            Button = new Button(new Vector2(1280 / 2, 720 - 96 * 2), 384, 96, Content.Load<Texture2D>("Texture/Button2"));
            font = Content.Load<SpriteFont>("Fonts/Pixeltype");

            game.Player.Stats.Heal(99999);
            game.Player.potion.Amout = 1;
            game.Player.DestinationPos = Vector2.Zero;
            game.Player.CurrentScene = null;
            base.LoadContent();
        }
        public override void Update(GameTime gameTime)
        {
            float dt = gameTime.GetElapsedSeconds();

            Button.Update();
            if (Button.Active)
            {
                game.Player.DestinationPos = Vector2.Zero;
                ScreenManager.LoadScreen(new SceneMenu(Game), new FadeTransition(GraphicsDevice, Color.Black, 5f));
                return;
            }
        }
        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin();
            _spriteBatch.Draw(BG, new Rectangle(0, 0, 1280, 720), Color.White); // BG

            var str = "Congratulation YOU WIN!";
            _spriteBatch.DrawString(font, str, new Vector2(1280 / 2 - 250, 100), Color.Black, 
                0, Vector2.Zero, Vector2.One * 2, SpriteEffects.None, 0);

            Button.Draw(_spriteBatch, 1);
            _spriteBatch.End();
        }
        public override void UnloadContent()
        {
            BG = null;
            Button = null;
            base.UnloadContent();
        }
    }
}
