using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assimp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Collisions.Layers;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Screens.Transitions;

namespace game
{
    public class SceneHome : GameScreen
    {
        private GlobalContext globalContext;

        // Collision & Layer
        private List<IEntity> _collision = new List<IEntity>();
        private CollisionComponent _collisionComponent;

        // Other Setting
        private Game1 game1;
        private SpriteBatch _spriteBatch;
        private KeyboardState _ks, _oldKs; // keyboard
        private Texture2D _healTexture; // tempo
        private bool isDebug = false;

        public SceneHome(Game game) : base(game)
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            game1 = (Game1)Game;

            // Collision
            _collision = game1.Collision;
            _collisionComponent = game1.CollisionComponent;

            globalContext = new GlobalContext(Game);
        }

        public override void LoadContent()
        {
            //globalContext.LoadCamera();
            //globalContext.LoadParticle();
            //globalContext.LoadTiledMap(Content, "SceneHome");
            //globalContext.LoadMonster(); // รอ Player
            globalContext.LoadAll(Content, "SceneHome");

            // Insert collision entities
            foreach (IEntity entity in _collision)
            {
                _collisionComponent.Insert(entity);
            }
            base.LoadContent();
        }
        public override void Update(GameTime gameTime)
        {
            // Keyboard input
            _oldKs = _ks;
            _ks = Keyboard.GetState();
            if (_ks.IsKeyDown(Keys.O) && !_oldKs.IsKeyDown(Keys.O))
            {
                isDebug = !isDebug;
            }
            if (_ks.IsKeyDown(Keys.Enter) && !_oldKs.IsKeyDown(Keys.Enter))
            {
                ScreenManager.LoadScreen(new ScenePrologue(Game), new FadeTransition(GraphicsDevice, Color.Black, 1f));
            }

            globalContext.UpdateCamera(Vector2.Zero);
            globalContext.UpdateParticle(gameTime);
            //globalContext.UpdateMonster(); // รอ player
            globalContext.UpdateTiledMaper(gameTime);
            globalContext.UpdateYsort();

            // Collision
            _collisionComponent.Update(gameTime);

            // Debug FPS
            int instantFps = (int)(1.0 / gameTime.ElapsedGameTime.TotalSeconds);
            game1.Window.Title = $"FPS: {instantFps}";
        }
        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                transformMatrix: globalContext._Camera.GetViewMatrix()
            );
            //globalContext.DrawTiledMaper();
            //globalContext.DrawObject(_spriteBatch);
            //globalContext.DrawParticle(_spriteBatch);
            globalContext.DrawAll(_spriteBatch);

            // Draw hitboxes
            if (isDebug)
            {
                _spriteBatch.DrawRectangle(new RectangleF(globalContext.Camera.Position,
                    new SizeF(5, 5)), Color.Red, 5, 0);
                foreach (IEntity item in _collision)
                {
                    item.Draw(_spriteBatch);
                }
            }
            _spriteBatch.End();
        }
        public override void UnloadContent()
        {
            globalContext.UnloadAll();
            globalContext = null;
            base.UnloadContent();
        }
    }
}

