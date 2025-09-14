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
using MonoGame.Extended.Screens;
using MonoGame.Extended.Screens.Transitions;

namespace game
{
    public class SceneMenu : GameScreen
    {
        private SpriteBatch _spriteBatch;
        private KeyboardState _ks, _oldKs;
        public SceneMenu(Game game) : base(game)
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        public override void LoadContent()
        {
            base.LoadContent();
        }
        public override void Update(GameTime gameTime)
        {
            _oldKs = _ks;
            _ks = Keyboard.GetState();
            if (_ks.IsKeyDown(Keys.Enter) && !_oldKs.IsKeyDown(Keys.Enter))
            {
                Debug.WriteLine("sence menu");
                ScreenManager.LoadScreen(new ScenePrologue(Game), new FadeTransition(GraphicsDevice, Color.Black, 1f));
            }
        }
        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
        }
    }
}
