using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        // Monster
        private List<IMonster> _monster = new List<IMonster>();
        // Collision & Layer
        private List<IEntity> _collision = new List<IEntity>();
        private CollisionComponent _collisionComponent;
        private PreventMonster _preventMonster; // Tempo
        // Player
        private AnimController _playerTexture;
        private Player _player;
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
            var viewportAdapter = new BoxingViewportAdapter(Game.Window, GraphicsDevice, game1.MapWidth, game1.MapHeight);
            _camera = new OrthographicCamera(viewportAdapter);
            AdjustZoom(); 
            // Player
            _playerTexture = new AnimController("Char01", new Vector2(400, 400), 32, 48);
            _playerTexture.LoadFrame(Content);
            _playerTexture.CreateAnimation("down", true, 12, 0, 3);
            _playerTexture.CreateAnimation("left", true, 12, 4, 7);
            _playerTexture.CreateAnimation("right", true, 12, 8, 11);
            _playerTexture.CreateAnimation("up", true, 12, 12, 15);
            _player = new Player(_playerTexture, _playerTexture.Position + new Vector2(_playerTexture.TextureWidth / 2, _playerTexture.TextureHeight / 2)); // Tempo Position
            _preventMonster = new PreventMonster(_playerTexture.Position, 100f); // Tempo
            _collisionComponent.Insert(_preventMonster); // Tempo
            // ชั่วคราว
            _collision.Add(new PlayerAttack(new RectangleF(
                _player._movement.Position - new Vector2(_playerTexture.TextureWidth / 2, _playerTexture.TextureHeight / 2), // Tempo Position
                new SizeF(32, 48)
                )));
            // Monster
            _monster.Add(new MonsterMelee(new Vector2(600, 200), _preventMonster));
            _monster.Add(new MonsterMelee(new Vector2(400, 200), _preventMonster));
            foreach (MonsterMelee monsterMelee in _monster.OfType<MonsterMelee>().ToList())
            {
                monsterMelee.LoadAnim("Char01", monsterMelee.Position, 32, 48, Content);
                monsterMelee.CreateAnimation();
                monsterMelee.SetProperty(
                    speed: 100f,
                    sreachRadius: 300f,
                    hp: 3
                );
                _collision.Add(monsterMelee.HurtBox);
            }
            //Collision
            foreach (IEntity entity in _collision)
            {
                _collisionComponent.Insert(entity);
            }
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

            // Player
            _player.Update(gameTime);
            _collision.Find(x => x.GetType() == typeof(PlayerAttack)).Bounds.Position = _player._movement.Position - new Vector2(_playerTexture.TextureWidth / 2, _playerTexture.TextureHeight / 2); // ชั่วคราว
            _preventMonster.UpdatePosition(_player._movement.Position); // TEMPO position
            // Monster
            foreach (MonsterMelee monster in _monster)
            {
                monster.UpdateState(gameTime, _collision, _collisionComponent, _player._movement.Position);
            }
            // Collision
            _collisionComponent.Update(gameTime);
            //_camera.Position = _player._movement.Position - new Vector2(
            //    (game1.MapWidth / 2) - (_playerTexture.TextureWidth / 2),
            //    (game1.MapHeight / 2) - (_playerTexture.TextureHeight / 2)
            //    ); // Temporary
            AdjustZoom();
            _tileMaper.UpdateMap(gameTime);
        }
        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _tileMaper.DrawMap(_camera);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, transformMatrix: _camera.GetViewMatrix());
            // Player
            _player.Draw(_spriteBatch);
            _preventMonster.Draw(_spriteBatch);
            // Monster
            foreach (MonsterMelee monster in _monster)
            {
                monster.DrawMonster(_spriteBatch);
                _spriteBatch.DrawCircle(new CircleF(monster.Position, monster.SreachRadius), 16, Color.Red, 2);
                _spriteBatch.DrawCircle(new CircleF(monster.SpawnPosition, 500f), 16, Color.Black, 2);
            }
            // Hitbox
            foreach (IEntity item in _collision)
            {
                item.Draw(_spriteBatch);
            }
            _spriteBatch.End();
        }
        public void AdjustZoom()
        {
            float zoomPerTick = 0.1f;
            if (_ks.IsKeyDown(Keys.Z) == true)
            {
                _camera.ZoomIn(zoomPerTick);
            }
            if (_ks.IsKeyDown(Keys.X) == true)
            {
                _camera.ZoomOut(zoomPerTick);
            }
            if (_ks.IsKeyDown(Keys.C) == true)
            {
                _camera.ZoomIn(zoomPerTick * 0.1f);
            }
            if (_ks.IsKeyDown(Keys.V) == true)
            {
                _camera.ZoomOut(zoomPerTick * 0.1f);
            }
        }
    }
}
