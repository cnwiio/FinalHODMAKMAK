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
    public class SceneDead : GameScreen
    {
        private SpriteBatch _spriteBatch;
        private KeyboardState _ks, _oldKs;
        private MouseState _ms, _oms;
        private Texture2D BG;
        private Button Button;
        private Button Button2;
        private Game1 game;
        public SceneDead(Game game) : base(game)
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            this.game = (Game1)game;
        }

        public override void LoadContent()
        {
            BG = Content.Load<Texture2D>("Texture/DeadScene");
            Button = new Button(new Vector2(game.ScreenWidth / 2, game.ScreenHeight - 96 * 3), 384, 96, Content.Load<Texture2D>("Texture/Button2"));
            Button2 = new Button(new Vector2(game.ScreenWidth / 2, game.ScreenHeight - 96 * 2), 384, 96, Content.Load<Texture2D>("Texture/Button2"));

            var player = game.Player;
            var stats = player.Stats;
            while (stats.CurrentHP != game.SavedHP)
            {
                if (stats.CurrentHP < game.SavedHP)
                    stats.Heal(1);
                else if (stats.CurrentHP > game.SavedHP)
                    stats.TakeDamage(1);
            }
            player.potion.Amout = game.SavedPotion;
            //Debug.WriteLine("LOAD! Save HP : " + game.SavedHP + " / Player HP: " + game.Player.Stats.CurrentHP);

            base.LoadContent();
        }
        public override void Update(GameTime gameTime)
        {
            float dt = gameTime.GetElapsedSeconds();

            Button.Update();
            Button2.Update();
            if (Button.Active)
            {
                var scene = game.Player.CurrentScene;
                if (scene == "SceneHome")
                {
                    ScreenManager.LoadScreen(new SceneHome(game), new FadeTransition(game.GraphicsDevice, Color.Black, 1f));
                }
                else if (scene == "ScenePrologue")
                {
                    ScreenManager.LoadScreen(new ScenePrologue(game), new FadeTransition(game.GraphicsDevice, Color.Black, 1f));
                }
                else if (scene == "SceneSmallBrigde")
                {
                    ScreenManager.LoadScreen(new SceneSmallBrigde(game), new FadeTransition(game.GraphicsDevice, Color.Black, 1f));
                }
                else if (scene == "SceneHomeNorth")
                {
                    ScreenManager.LoadScreen(new SceneHomeNorth(game), new FadeTransition(game.GraphicsDevice, Color.Black, 1f));
                }
                else if (scene == "SceneFlowerHills")
                {
                    ScreenManager.LoadScreen(new SceneFlowerHills(game), new FadeTransition(game.GraphicsDevice, Color.Black, 1f));
                }
                else if (scene == "SceneWaterfall")
                {
                    ScreenManager.LoadScreen(new SceneWaterfall(game), new FadeTransition(game.GraphicsDevice, Color.Black, 1f));
                }
                else if (scene == "SceneUnderWaterfall")
                {
                    ScreenManager.LoadScreen(new SceneUnderWaterfall(game), new FadeTransition(game.GraphicsDevice, Color.Black, 1f));
                }
                else
                {
                    ScreenManager.LoadScreen(new ScenePrologue(Game), new FadeTransition(GraphicsDevice, Color.Black, 1f));
                }
                return;
            }
            else if (Button2.Active)
            {
                game.Player.DestinationPos = Vector2.Zero;
                game.Player.CurrentScene = null;
                ScreenManager.LoadScreen(new SceneMenu(Game, true));
                return;
            }
        }
        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin();
            _spriteBatch.Draw(BG, new Rectangle(0, 0, game.ScreenWidth, game.ScreenHeight), Color.White);
            Button.Draw(_spriteBatch);
            Button2.Draw(_spriteBatch, 1);
            _spriteBatch.End();
        }
        public override void UnloadContent()
        {
            BG = null;
            Button = null;
            Button2 = null;
            base.UnloadContent();
        }
    }
}
