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
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Timers;
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
        private List<GameObject> _gameObject = new List<GameObject>();
        // Player
        private AnimController _playerTexture;
        private Player _player;

        // Camera
        private GlobalCamera camera;
        private OrthographicCamera _camera;
        // Particle
        private Particle particle;
        // Other Setting
        private Game1 game1;
        private SpriteBatch _spriteBatch;
        private KeyboardState _ks; // keyboard
        private Texture2D _healTexture; // tempo
        private List<IYsort> _ysort = new List<IYsort>();

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
            _healTexture = Content.Load<Texture2D>("Texture/Health");

            // Camera setup
            camera = game1.camera;
            _camera = camera.Cam;
            //Tile Map
            _tileMaper.LoadMap(Content, "ScenePrologue");
            _tileMaper.LoadCollision(_collisionComponent, _collision, "Collision");
            // Particle
            particle = new Particle(game1);
            // Game Object
            var objectLayer = _tileMaper.GetObjectLayer("Object");
            foreach (var item in objectLayer.Objects)
            {
                _gameObject.Add(new GameObject(item.Position, Content.Load<Texture2D>("TileMap/" + item.Type)));
                _ysort.Add(_gameObject.Last());
            }
            // Player
            _playerTexture = new AnimController(new Vector2(400, 400));
            _playerTexture.LoadFrame(Content, "Walk", "Player_Walk", 64, 96);

            // Walk Animations
            _playerTexture.CreateAnimation("Walk", "left", true, 200, 0, 4);
            _playerTexture.CreateAnimation("Walk", "right", true, 200, 4, 4);
            _playerTexture.CreateAnimation("Walk", "down", true, 200, 8, 4);
            _playerTexture.CreateAnimation("Walk", "up", true, 200, 12, 4);
            _playerTexture.CreateAnimation("Walk", "attack", true, 12, 8, 4);

            // Idle animation (4 directions, 6 frames per row)
            _playerTexture.LoadFrame(Content, "Idle", "Player_Idle", 100, 112);

            _playerTexture.CreateAnimation("Idle", "down", true, 200, 0, 6);   // row 0
            _playerTexture.CreateAnimation("Idle", "right", true, 200, 6, 6);  // row 1
            _playerTexture.CreateAnimation("Idle", "left", true, 200, 12, 6);  // row 2
            _playerTexture.CreateAnimation("Idle", "up", true, 200, 18, 6);    // row 3


            _player = new Player(_playerTexture, new Vector2(400, 400));

            // **Set world references for collision / pickups**
            _player.SetWorldReferences(_collision, _collisionComponent);
            _ysort.Add(_player);

            // Prevent monster zone
            _preventMonster = new PreventMonster(new Vector2(400, 400), 250f);
            _collisionComponent.Insert(_preventMonster);



            // Monster
            LoadMonster();
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
            // Particle
            particle.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            // Monster
            UpdateMonster(gameTime);
            // Ysort
            _ysort.Sort((a, b) => a.SortY.CompareTo(b.SortY));
            // Collision
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
            //_player.Draw(_spriteBatch);

            // Prevent monster zone
            _preventMonster.Draw(_spriteBatch);

            // Monster
            foreach (MonsterMelee monster in _monster.OfType<MonsterMelee>().ToList())  
            {
                //monster.Draw(_spriteBatch);
                _spriteBatch.DrawCircle(new CircleF(monster.SpawnPosition, monster.AwaySpawnRadius), 16, Color.DarkViolet, 2);
                _spriteBatch.DrawCircle(new CircleF(monster.Position, monster.SreachRadius), 16, Color.RoyalBlue, 2);
                _spriteBatch.DrawCircle(new CircleF(monster.Position, monster.ActiveRadius), 16, Color.DeepSkyBlue, 2);
            }
            foreach (MonsterRange monster in _monster.OfType<MonsterRange>().ToList())  
            {
                //monster.Draw(_spriteBatch);
                _spriteBatch.DrawCircle(new CircleF(monster.SpawnPosition, monster.AwaySpawnRadius), 16, Color.DarkViolet, 2);
                _spriteBatch.DrawCircle(new CircleF(monster.Position, monster.SreachRadius), 16, Color.RoyalBlue, 2);
                _spriteBatch.DrawCircle(new CircleF(monster.Position, monster.ActiveRadius), 16, Color.DeepSkyBlue, 2);
                _spriteBatch.DrawCircle(new CircleF(monster.Position, monster.AttackRange), 16, Color.Aqua, 2);
            }
            // Object
            foreach (var item in _ysort)
            {
                item.Draw(_spriteBatch);    
            }

            // Draw hitboxes
            foreach (IEntity item in _collision)
            {
                item.Draw(_spriteBatch);
            }
            // Particle
            particle.Draw(_spriteBatch);
            _spriteBatch.End();
        }


        public override void UnloadContent()
        {
            //_tileMaper.UnloadMap();
            //_collisionComponent.Clear();
            //_playerTexture.UnloadContent();

            foreach (IMonster monster in _monster)
            {
                monster.UnLoad(); // actually calls UnLoad on each monster
            }

            _monster.Clear();
            _collision.Clear();
            Content.Unload();

            base.UnloadContent();
        }
        // {------------------------------ Monster ------------------------------------------- } //
        private void LoadMonster()
        {
            var spawnPoint = _tileMaper.GetObjectLayer("SpawnPoint");
            foreach (var obj in spawnPoint.Objects)
            {
                if (obj.Type == "Melee")
                    _monster.Add(new MonsterMelee(obj.Position, _preventMonster, _player, particle));
                if (obj.Type == "Range")
                    _monster.Add(new MonsterRange(obj.Position, _preventMonster, _player, particle));
            }
            foreach (MonsterMelee monster in _monster.OfType<MonsterMelee>().ToList())
            {
                monster.LoadAnim("Walk", "LightGoonWalk", monster.Position, 128, 128, Content);
                monster.LoadAnim("Idle", "LightGoonIdle", monster.Position, 128, 128, Content);
                monster.LoadAnim("Attack", "LightGoonAttack", monster.Position, 128, 128, Content);
                monster.LoadAnim("Charge", "LightGoonCharge", monster.Position, 128, 128, Content);
                monster.LoadAnim("Die", "LightGoonFuckingDie-Sheet", monster.Position, 128, 128, Content);
                monster.CreateAnimation();
                monster.SetProperty(
                    speed: 100f,
                    sreachRadius: 500f,
                    hp: 200,
                    damage: 10,
                    element: Element.light,
                    attackRange: (int)(monster.Width * 1.5),
                    dashForce: monster.Width * 7
                );
                _ysort.Add(monster);
                _collision.Add(monster.HurtBox);
                _collision.Add(monster.Collision);
            }
            foreach (MonsterRange monster in _monster.OfType<MonsterRange>().ToList())
            {
                monster.loadBullet(Content, "Health");
                monster.LoadAnim("Walk", "LightRegimogusIdle", monster.Position, 128, 128, Content);
                monster.LoadAnim("Attack", "LightRegimogusAttack", monster.Position, 128, 128, Content);
                monster.LoadAnim("Charge", "LightRegimogusCharge", monster.Position, 128, 128, Content);
                monster.LoadAnim("Idle", "LightRegimogusIdle", monster.Position, 128, 128, Content);
                monster.LoadAnim("Die", "LightRegimogusFuckingDie", monster.Position, 128, 128, Content);
                monster.CreateAnimation();
                monster.SetProperty(
                    speed: 100f,
                    sreachRadius: 500f,
                    hp: 200,
                    damage: 10,
                    element: Element.light,
                    attackRange: (int)(monster.Width * 1.5),
                    dashForce: monster.Width * 7
                );
                _ysort.Add(monster);
                _collision.Add(monster.HurtBox);
                _collision.Add(monster.Collision);
            }
        }
        private void UpdateMonster(GameTime gameTime)
        {
            foreach (MonsterMelee monster in _monster.OfType<MonsterMelee>().ToList())
            {
                monster.UpdateState(gameTime, _collision, _collisionComponent, _player._movement.Position);
                if (monster.ShakeViewport)
                {
                    camera.ShakeCamera(gameTime);
                    monster.ShakeViewport = camera.ShakeViewport;
                }
                // Temporary
                // Will make additional method for monster dead and drop
                // ps. make a new global class and make a drop heal there, then call it in remove monster(maybe)
                if (monster.IsDead)
                {
                    monster.DropHeal(_collision, _collisionComponent, _healTexture, _player);
                    monster.DeleteHitBox(1f, _collision, _collisionComponent);
                    monster.RemoveMonster();
                    _monster.Remove(monster);
                    break; // Exit the loop to avoid modifying the collection while iterating; list bug prevented
                }
                //--------------
            }
            foreach (MonsterRange monster in _monster.OfType<MonsterRange>().ToList())
            {
                monster.UpdateState(gameTime, _collision, _collisionComponent, _player._movement.Position);
                if (monster.ShakeViewport)
                {
                    camera.ShakeCamera(gameTime);
                    monster.ShakeViewport = camera.ShakeViewport;
                }
                // Temporary
                // Will make additional method for monster dead and drop
                // ps. make a new global class and make a drop heal there, then call it in remove monster(maybe)
                if (monster.IsDead)
                {
                    monster.DropHeal(_collision, _collisionComponent, _healTexture, _player);
                    monster.DeleteHitBox(1f);
                    monster.RemoveMonster();
                    _monster.Remove(monster);
                    break; // Exit the loop to avoid modifying the collection while iterating; list bug prevented
                }
                //--------------
            }
        }
        // {-------------------------- End of Monster ---------------------------------------- } //
    }
}
