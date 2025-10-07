using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assimp;
using game;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Screens.Transitions;

namespace game
{
    public class SceneMenu : GameScreen
    {
        private SpriteBatch _spriteBatch;
        private KeyboardState _ks, _oldKs;
        private MouseState _ms, _oms;
        private Texture2D BG, SP, CR;
        private SoundEffect LOGOSFX;
        private const float SCREENTIME = 3;
        private float timer = 0;
        private float alpha = 1f;
        private ScreenScene scene = ScreenScene.Splash;
        private Game1 game1;
        enum ScreenScene
        {
            Splash,
            Credit,
            Menu
        }
        public SceneMenu(Game game, bool skipCutScene = false) : base(game)
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            scene = skipCutScene == false ? ScreenScene.Splash : ScreenScene.Menu;
            game1 = (Game1)game;
        }

        public override void LoadContent()
        {
            //ScreenManager.LoadScreen(new ScenePrologue(Game), new FadeTransition(GraphicsDevice, Color.Black, 1f));
            BG = Content.Load<Texture2D>("Texture/BG_art");
            SP = Content.Load<Texture2D>("Texture/splash");
            CR = Content.Load<Texture2D>("Texture/Credit");
            LOGOSFX = Content.Load<SoundEffect>("Audio/WoodHit");
            timer = SCREENTIME;
            base.LoadContent();
        }
        public override void Update(GameTime gameTime)
        {
            float dt = gameTime.GetElapsedSeconds();
            if (timer > 0)
            {
                timer -= dt;
                if (timer <= 1f)
                {
                    alpha -= dt;
                }
                if (timer <= 0)
                {
                    switch (scene)
                    {
                        case ScreenScene.Menu:
                            timer = 0;
                            break;
                        case ScreenScene.Splash:
                            timer = SCREENTIME;
                            alpha = 1f;
                            scene = ScreenScene.Credit;
                            game1.audioController.PlaySoundEffect(LOGOSFX);
                            break;
                        case ScreenScene.Credit:
                            scene = ScreenScene.Menu;
                            break;
                    }
                }
            }

            _oldKs = _ks;
            _ks = Keyboard.GetState();
            _oms = _ms;
            _ms = Mouse.GetState();
            if (scene == ScreenScene.Menu)
            {
                var checkKs = _ks.GetPressedKeyCount() > 0 && _oldKs.GetPressedKeyCount() == 0;
                var checkMS = (_ms.LeftButton == ButtonState.Pressed && _oms.LeftButton != ButtonState.Pressed) ||
                    (_ms.RightButton == ButtonState.Pressed && _oms.RightButton != ButtonState.Pressed);
                if (checkKs || checkMS)
                {
                    ScreenManager.LoadScreen(new ScenePrologue(Game), new FadeTransition(GraphicsDevice, Color.Black, 1f));
                } 
            }
            if (_ks.IsKeyDown(Keys.Enter) && !_oldKs.IsKeyDown(Keys.Enter)) scene = ScreenScene.Menu;
        }
        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin();
            switch (scene)
            {
                case ScreenScene.Menu:
                    _spriteBatch.Draw(BG, new Rectangle(0, 0, 1280, 720), Color.White);
                    break;
                case ScreenScene.Splash:
                    _spriteBatch.Draw(SP, new Rectangle(0, 0, 1280, 720), Color.White * alpha);
                    break;
                case ScreenScene.Credit:
                    _spriteBatch.Draw(CR, new Rectangle(0, 0, 1280, 720), Color.White * alpha);
                    break;
            }
            _spriteBatch.End();
        }
        public override void UnloadContent()
        {
            BG = null;
            SP = null;
            CR = null;
            LOGOSFX = null;
            base.UnloadContent();
        }
    }
}
