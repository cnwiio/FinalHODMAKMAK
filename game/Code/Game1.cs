using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Collisions.Layers;
using MonoGame.Extended.Screens;
using MonoGame.Extended.ViewportAdapters;

namespace game
{
    public class Game1 : Game
    {   
        // monster Branch
        private GraphicsDeviceManager _graphics;
        public ScreenManager screenManager;

        // Audio
        public AudioController audioController;

        public GlobalCamera camera;
        public short MapWidth, MapHeight;
        public short ScreenWidth = 1280, ScreenHeight = 720;

        // Collision 
        public CollisionComponent CollisionComponent { get; set; }
        public List<IEntity> Collision { get; set; } = new List<IEntity>();

        // Player and monsters
        private AnimController _playerTexture;
        public Player Player;
        public PreventMonster PreventMonster;
        private List<IEntity> _monsters;
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            audioController = new AudioController();

            screenManager = new ScreenManager();
            Components.Add(screenManager);

            MapWidth = 64 * 70;
            MapHeight = 64 * 70;
            CollisionComponent = new CollisionComponent(new RectangleF(0 , 0, MapWidth, MapHeight));
        }

        protected override void Initialize()
        {
            var viewportAdapter = new BoxingViewportAdapter(Window, GraphicsDevice, ScreenWidth, ScreenHeight);
            camera = new GlobalCamera(viewportAdapter);

            _graphics.PreferredBackBufferWidth = ScreenWidth;
            _graphics.PreferredBackBufferHeight = ScreenHeight;
            //_graphics.IsFullScreen = true;
            _graphics.ApplyChanges();
                
            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Player
            _playerTexture = new AnimController(new Vector2(400, 400));
            _playerTexture.LoadFrame(Content, "Walk", "Player_Walk", 128, 128);

            // Walk Animations
            _playerTexture.CreateAnimation("Walk", "down", true, 98, 0, 8);
            _playerTexture.CreateAnimation("Walk", "right", true, 98, 8, 8);
            _playerTexture.CreateAnimation("Walk", "left", true, 98, 16, 8);
            _playerTexture.CreateAnimation("Walk", "up", true, 98, 24, 8);

            // Idle animation (4 directions, 6 frames per row)
            _playerTexture.LoadFrame(Content, "Idle", "Player_Idle", 128, 128);

            _playerTexture.CreateAnimation("Idle", "down", true, 200, 0, 6);   // row 0
            _playerTexture.CreateAnimation("Idle", "right", true, 200, 6, 6);  // row 1
            _playerTexture.CreateAnimation("Idle", "left", true, 200, 12, 6);  // row 2
            _playerTexture.CreateAnimation("Idle", "up", true, 200, 18, 6);    // row 3

            // Attack animation (4 directions, 6 frames per row)
            _playerTexture.LoadFrame(Content, "Attack", "Player_Attack", 288, 240);

            _playerTexture.CreateAnimation("Attack", "down", false, 25, 0, 8);   // row 0
            _playerTexture.CreateAnimation("Attack", "left", false, 25, 8, 8);  // row 1
            _playerTexture.CreateAnimation("Attack", "right", false, 25, 16, 8);  // row 2
            _playerTexture.CreateAnimation("Attack", "up", false, 25, 24, 8);    // row 3

            Player = new Player(_playerTexture, new Vector2(0));

            Player.SetWorldReferences(Collision, CollisionComponent);
            PreventMonster = new PreventMonster(new Vector2(400, 400), 350f);

            screenManager.LoadScreen(new SceneMenu(this)); 
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
