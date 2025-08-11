using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Screens;
using MonoGame.Extended.ViewportAdapters;

namespace game
{
    public class ScenePrologue : GameScreen
    {
        // Tile Map
        private TileMaper _tileMaper;
        // Collision
        private CollisionComponent _collisionComponent;
        private List<IEntity> _collision;
        // Other Setting
        private Game1 game1;
        private SpriteBatch _spriteBatch;
        private KeyboardState _ks; // keyboard
        private OrthographicCamera _camera; // camera
        
        public ScenePrologue(Game game) : base(game)
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            game1 = (Game1)Game;
            //Collision
            _collision = game1.Collision;
            _collisionComponent = game1.CollisionComponent;
            _tileMaper = new TileMaper(Game); // Tile Map
        }
        public override void LoadContent()
        {
            //Camera
            var viewportAdapter = new BoxingViewportAdapter(Game.Window, GraphicsDevice, 1200, 800);
            _camera = new OrthographicCamera(viewportAdapter);
            //Tile Map
            _tileMaper.LoadMap(Content, "ScenePrologue");
            _tileMaper.LoadCollision(_collisionComponent, _collision, "Collision");
            base.LoadContent();
        }
        public override void Update(GameTime gameTime)
        {
            _ks = Keyboard.GetState();
            if (_ks.IsKeyDown(Keys.Enter) == true)
            {
                //logic
            }
            _tileMaper.UpdateMap(gameTime);
        }
        public override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin();
            _tileMaper.DrawMap(_camera);
            _spriteBatch.End();
        }
    }
}
