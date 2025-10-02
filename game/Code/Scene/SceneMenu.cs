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
        private MouseState _ms, _oms;
        private Texture2D BG;
        public SceneMenu(Game game) : base(game)
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        public override void LoadContent()
        {
            //ScreenManager.LoadScreen(new ScenePrologue(Game), new FadeTransition(GraphicsDevice, Color.Black, 1f));
            BG = Content.Load<Texture2D>("Texture/BG_art");
            base.LoadContent();
        }
        public override void Update(GameTime gameTime)
        { 
            _oldKs = _ks;
            _ks = Keyboard.GetState();
            _oms = _ms;
            _ms = Mouse.GetState();
            var checkKs = _ks.GetPressedKeyCount() > 0 && _oldKs.GetPressedKeyCount() == 0;
            var checkMS = (_ms.LeftButton == ButtonState.Pressed && _oms.LeftButton != ButtonState.Pressed) ||
                (_ms.RightButton == ButtonState.Pressed && _oms.RightButton != ButtonState.Pressed);
            if (checkKs || checkMS)
            {
                ScreenManager.LoadScreen(new ScenePrologue(Game), new FadeTransition(GraphicsDevice, Color.Black, 1f));
            }

        }
        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin();
            _spriteBatch.Draw(BG, new Rectangle(0, 0, 1280, 720), Color.White);
            _spriteBatch.End();
        }
        public override void UnloadContent()
        {
            BG = null;
            base.UnloadContent();
        }
    }
}
