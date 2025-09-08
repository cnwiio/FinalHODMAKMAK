using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        private List<IEntity> _attackTargets = new List<IEntity>();

        // Collision & Layer
        private List<IEntity> _collision = new List<IEntity>();
        private CollisionComponent _collisionComponent;
        private PreventMonster _preventMonster;

        // Player
        private AnimController _playerTexture;
        private Player _player;

        // Camera
        private GlobalCamera camera;
        private OrthographicCamera _camera;

        // Other Settings
        private Game1 game1;
        private SpriteBatch _spriteBatch;
        private KeyboardState _ks;
        private Texture2D _dropTexture; // temporary

        public ScenePrologue(Game game) : base(game)
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            game1 = (Game1)Game;

            // Collision
            _collision = game1.Collision;
            _collisionComponent = game1.CollisionComponent;

            // Tile Map
            _tileMaper = new TileMaper(Game);
        }


        public override void LoadContent()
        {
            // Load temporary drop texture
            _dropTexture = Content.Load<Texture2D>("Texture/Health");

            // Camera setup
            camera = game1.camera;
            _camera = camera.Cam;

            // Player setup
            _playerTexture = new AnimController(new Vector2(400, 400));
            _playerTexture.LoadFrame(Content, "Walk", "Player_Walk", 64, 96);

            // Walk Animations
            _playerTexture.CreateAnimation("Walk", "left", true, 12, 0, 4);
            _playerTexture.CreateAnimation("Walk", "right", true, 12, 4, 4);
            _playerTexture.CreateAnimation("Walk", "down", true, 12, 8, 4);
            _playerTexture.CreateAnimation("Walk", "up", true, 12, 12, 4);
            _playerTexture.CreateAnimation("Walk", "attack", true, 12, 8, 4);

            // Idle animation (4 directions, 6 frames per row)
            _playerTexture.LoadFrame(Content, "Idle", "Player_Idle", 100, 112);

            _playerTexture.CreateAnimation("Idle", "down", true, 6, 0, 6);   // row 0
            _playerTexture.CreateAnimation("Idle", "right", true, 6, 6, 6);  // row 1
            _playerTexture.CreateAnimation("Idle", "left", true, 6, 12, 6);  // row 2
            _playerTexture.CreateAnimation("Idle", "up", true, 6, 18, 6);    // row 3


            _player = new Player(_playerTexture, new Vector2(400, 400));

            // Prevent monster zone
            _preventMonster = new PreventMonster(new Vector2(400, 400), 250f);
            _collisionComponent.Insert(_preventMonster);

            // Monster setup
            _monster.Add(new MonsterMelee(new Vector2(600, 200), _preventMonster));
            _monster.Add(new MonsterMelee(new Vector2(400, 200), _preventMonster));

            foreach (MonsterMelee monsterMelee in _monster.OfType<MonsterMelee>().ToList())
            {
                monsterMelee.LoadAnim("Walk", "Player_Walk", monsterMelee.Position, 64, 96, Content);
                monsterMelee.LoadAnim("Idle", "Player_Idle", monsterMelee.Position, 48, 53, Content);
                monsterMelee.CreateAnimation();
                monsterMelee.SetProperty(speed: 100f, sreachRadius: 500f, hp: 3);

                _collision.Add(monsterMelee.HurtBox);
            }

            // Fill attack targets list
            _attackTargets.Clear();
            foreach (var monster in _monster)
            {
                _attackTargets.Add(monster.HurtBox);
            }

            // Insert collision entities
            foreach (IEntity entity in _collision)
            {
                _collisionComponent.Insert(entity);
            }

            // Tile map
            _tileMaper.LoadMap(Content, "ScenePrologue");
            _tileMaper.LoadCollision(_collisionComponent, _collision, "Collision");

            base.LoadContent();
        }


        public override void Update(GameTime gameTime)
        {
            // Keyboard input
            _ks = Keyboard.GetState();
            if (_ks.IsKeyDown(Keys.Enter))
            {
                // logic here
            }

            // Player
            _player.Update(gameTime, _attackTargets);

            // Prevent monster follows player
            _preventMonster.UpdatePosition(_player._movement.Position);

            // Camera
            camera.Update(_player._movement.Position - new Vector2(game1.MapWidth / 2, game1.MapHeight / 2));
            camera.AdjustZoom();

            // Monster update
            foreach (MonsterMelee monster in _monster)
            {
                monster.UpdateState(gameTime, _collision, _collisionComponent, _player._movement.Position);

                if (monster.ShakeViewport)
                {
                    camera.ShakeCamera(gameTime);
                    monster.ShakeViewport = camera.ShakeViewport;
                }

                // Monster death handling
                if (monster.IsDead)
                {
                    monster.DropHeal(_collision, _collisionComponent, _dropTexture, _player);
                    monster.DeleteHitBox(1f, _collision, _collisionComponent);
                    monster.RemoveMonster();
                    _monster.Remove(monster);
                    break; // avoid modifying collection during iteration
                }
            }

            // Update collision component and map
            _collisionComponent.Update(gameTime);
            _tileMaper.UpdateMap(gameTime);

            // Debug FPS
            int instantFps = (int)(1.0 / gameTime.ElapsedGameTime.TotalSeconds);
            game1.Window.Title = $"FPS: {instantFps}";
        }


        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _tileMaper.DrawMap(_camera);

            _spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                transformMatrix: _camera.GetViewMatrix()
            );

            // Player
            _player.Draw(_spriteBatch);

            // Prevent monster zone
            _preventMonster.Draw(_spriteBatch);

            // Monster
            foreach (MonsterMelee monster in _monster)
            {
                monster.DrawMonster(_spriteBatch);
                _spriteBatch.DrawCircle(new CircleF(monster.SpawnPosition, monster.AwaySpawnRadius), 16, Color.DarkViolet, 2);
                _spriteBatch.DrawCircle(new CircleF(monster.Position, monster.SreachRadius), 16, Color.RoyalBlue, 2);
                _spriteBatch.DrawCircle(new CircleF(monster.Position, monster.ActiveRadius), 16, Color.DeepSkyBlue, 2);
            }

            // Draw hitboxes
            foreach (IEntity item in _collision)
            {
                item.Draw(_spriteBatch);
            }

            _spriteBatch.End();
        }


        public override void UnloadContent()
        {
            //_tileMaper.UnloadMap();
            //_collisionComponent.Clear();
            //_playerTexture.UnloadContent();

            foreach (MonsterMelee monster in _monster)
            {
                monster.UnLoad(); // actually calls UnLoad on each monster
            }

            _monster.Clear();
            _collision.Clear();
            Content.Unload();

            base.UnloadContent();
        }
    }
}
