using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Screens;
using MonoGame.Extended.ViewportAdapters;

namespace game
{
    public class Game1 : Game
    {   
        // monster Branch
        private GraphicsDeviceManager _graphics;
        public ScreenManager screenManager;

        public GlobalCamera camera;
        public short MapWidth, MapHeight;
        public short ScreenWidth = 1280, ScreenHeight = 720;

        // Collision 
        public CollisionComponent CollisionComponent { get; set; }
        public List<IEntity> Collision { get; set; } = new List<IEntity>();

        // Player and monsters
        private Player _player;
        private List<IEntity> _monsters;
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            screenManager = new ScreenManager();
            Components.Add(screenManager);

            MapWidth = 64 * 70;
            MapHeight = 64 * 50;
            CollisionComponent = new CollisionComponent(new RectangleF(0 , 0, MapWidth, MapHeight));
        }

        protected override void Initialize()
        {
            var viewportAdapter = new BoxingViewportAdapter(Window, GraphicsDevice, ScreenWidth, ScreenHeight);
            camera = new GlobalCamera(viewportAdapter);

            _graphics.PreferredBackBufferWidth = ScreenWidth;
            _graphics.PreferredBackBufferHeight = ScreenHeight;
            _graphics.IsFullScreen = true;
            _graphics.ApplyChanges();
                
            base.Initialize();
        }

        protected override void LoadContent()
        {
            screenManager.LoadScreen(new ScenePrologue(this)); // temporary
            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
    }
}
