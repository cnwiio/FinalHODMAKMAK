using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Screens.Transitions;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Timers;
using MonoGame.Extended.ViewportAdapters;

namespace game
{
    public class ScenePrologue : GameScreen
    {
        // Tile Map
        private TileMaper _tileMaper;
        private List<IYsort> _ysort = new List<IYsort>();

        // Monster
        private List<IMonster> _monster = new List<IMonster>();
        private List<IEntity> _attackTargets = new List<IEntity>();

        // Collision & Layer
        private List<IEntity> _collision = new List<IEntity>();
        private CollisionComponent _collisionComponent;
        private PreventMonster _preventMonster;
        private List<GameObject> _gameObject = new List<GameObject>();
        private List<GameObject> _shadow = new List<GameObject>();

        // Player
        private AnimController _playerTexture;
        private Player _player;

        // Camera
        private GlobalCamera camera;
        private OrthographicCamera _camera;

        // Particle
        private HitParticle hitParticle;
        private DeadParticle deadParticle;
        private FireParticle fireParticleLight;
        private FireParticle fireParticleDark;

        // Audio
        private AudioController _audioController;

        // Other Setting
        private Game1 game1;
        private SpriteBatch _spriteBatch;
        private KeyboardState _ks, _oldKs; // keyboard
        private Texture2D _healTexture; // tempo
        private bool isDebug = false;

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

            // Audio
            _audioController = game1.audioController;

            // Particle
            hitParticle = new HitParticle(game1);
            deadParticle = new DeadParticle(game1);
            fireParticleLight = new FireParticle(game1);
            fireParticleDark = new FireParticle(game1);

            //Tile Map
            _tileMaper.LoadMap(Content, "ScenePrologue");
            _tileMaper.LoadCollision(_collisionComponent, _collision, "Collision");
            // Game Object
            var objectLayer = _tileMaper.GetObjectLayer("Object");
            foreach (var item in objectLayer.Objects)
            {
                _gameObject.Add(new GameObject(item.Position, Content.Load<Texture2D>("TileMap/" + item.Type)));
                _ysort.Add(_gameObject.Last());
            }
            var shadowLayer = _tileMaper.GetObjectLayer("Shadow");
            foreach (var item in shadowLayer.Objects)
            {
                _shadow.Add(new GameObject(item.Position, Content.Load<Texture2D>("TileMap/" + item.Type)));
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

            // Attack animation (4 directions, 6 frames per row)
            _playerTexture.LoadFrame(Content, "Attack", "Player_Attack", 288, 240);

            _playerTexture.CreateAnimation("Attack", "down", false, 25, 0, 8);   // row 0
            _playerTexture.CreateAnimation("Attack", "left", false, 25, 8, 8);  // row 1
            _playerTexture.CreateAnimation("Attack", "right", false, 25, 16, 8);  // row 2
            _playerTexture.CreateAnimation("Attack", "up", false, 25, 24, 8);    // row 3


            var spawnPoint = _tileMaper.GetObjectLayer("SpawnPoint");
            foreach (var obj in spawnPoint.Objects)
            {
                if (obj.Name == "Player")
                {
                    _player = new Player(_playerTexture, new Vector2(obj.Position.X, obj.Position.Y));
                    break;
                }
            }
            //_player = new Player(_playerTexture, new Vector2(802, 2603));
            //_player = new Player(_playerTexture, new Vector2(2600, 1603));

            // **Set world references for collision / pickups**
            _player.SetWorldReferences(_collision, _collisionComponent);
            _ysort.Add(_player);

            // Prevent monster zone
            _preventMonster = new PreventMonster(new Vector2(400, 400), 350f);
            _collision.Add(_preventMonster);

            // Monster
            LoadMonster();

            // Fill attack targets list ไม่ต้องใช้ละ ลบได้
            //_attackTargets.Clear();
            //foreach (var monster in _monster)
            //{
            //    _attackTargets.Add(monster.HurtBox);
            //}

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
            if (!_ks.IsKeyDown(Keys.Enter) && _oldKs.IsKeyDown(Keys.Enter))
            {
                ScreenManager.LoadScreen(new SceneMenu(game1));
            }

            // Player
            _player.Update(gameTime, _attackTargets);

            // Prevent monster follows player
            _preventMonster.UpdatePosition(_player._movement.Position);

            // Camera
            camera.Update(_player._movement.Position - new Vector2(game1.ScreenWidth / 2, game1.ScreenHeight / 2));
            camera.AdjustZoom();
            //Debug.WriteLine(_camera.Zoom);

            // Particle
            if (hitParticle != null && deadParticle != null && fireParticleLight != null && fireParticleDark != null)
            {
                hitParticle.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
                deadParticle.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
                fireParticleLight.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
                fireParticleDark.Update((float)gameTime.ElapsedGameTime.TotalSeconds); 
            }

            // Monster
            UpdateMonster(gameTime);
            // Ysort
            _ysort.Sort((a, b) => 
            {
                // เปรียบเทียบ SortY ก่อน
                int yComparison = a.SortY.CompareTo(b.SortY);
                if (yComparison != 0)
                    return yComparison;

                // ถ้า SortY เท่ากัน ใช้ SortX เป็นเงื่อนไขรอง
                return b.SortX.CompareTo(a.SortX);
            });

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

            // วาดสกิลของบอส
            if (_monster.Exists(x => x is MonsterBoss)) {
                var Boss = (MonsterBoss)_monster.Find(x => x.GetType() == typeof(MonsterBoss));
                if (!Boss.IsDead)
                {
                    Boss.DrawSkill(_spriteBatch);
                }
            }
            
            // Shadow
            foreach (GameObject item in _shadow)
            {
                item.Draw(_spriteBatch);
            }

            // Object
            foreach (var item in _ysort)
            {
                item.Draw(_spriteBatch);    
            }

            // Draw hitboxes
            if (isDebug)
            {
                _spriteBatch.DrawRectangle(new RectangleF(camera.Position,
                    new SizeF(5, 5)), Color.Red, 5, 0);
                foreach (IEntity item in _collision)
                {
                    item.Draw(_spriteBatch);
                }
                // Prevent monster zone
                _preventMonster.Draw(_spriteBatch);
                // Monster
                foreach (MonsterMelee monster in _monster.OfType<MonsterMelee>().ToList())
                {
                    //monster.Draw(_spriteBatch);
                    _spriteBatch.DrawCircle(new CircleF(monster.SpawnPosition, monster.AwaySpawnRadius), 16, Color.DarkViolet, 2);
                    _spriteBatch.DrawCircle(new CircleF(monster.Position, monster.SreachRadius), 16, Color.RoyalBlue, 2);
                    _spriteBatch.DrawCircle(new CircleF(monster.Position, monster.ActiveRadius), 16, Color.DeepSkyBlue, 2);
                    _spriteBatch.DrawCircle(new CircleF(monster.Position, monster.AttackRange), 16, Color.Aqua, 2);
                }
                foreach (MonsterRange monster in _monster.OfType<MonsterRange>().ToList())
                {
                    //monster.Draw(_spriteBatch);
                    _spriteBatch.DrawCircle(new CircleF(monster.SpawnPosition, monster.AwaySpawnRadius), 16, Color.DarkViolet, 2);
                    _spriteBatch.DrawCircle(new CircleF(monster.Position, monster.SreachRadius), 16, Color.RoyalBlue, 2);
                    _spriteBatch.DrawCircle(new CircleF(monster.Position, monster.AttackRange), 16, Color.Aqua, 2);
                }
                foreach (MonsterBoss monster in _monster.OfType<MonsterBoss>().ToList())
                {
                    if (!monster.IsDead)
                    {
                        _spriteBatch.DrawCircle(new CircleF(monster.SpawnPosition, monster.AwaySpawnRadius), 16, Color.DarkViolet, 2);
                        _spriteBatch.DrawCircle(new CircleF(monster.Position, monster.SreachRadius), 16, Color.RoyalBlue, 2);
                        _spriteBatch.DrawCircle(new CircleF(monster.Position, monster.ActiveRadius), 16, Color.DeepSkyBlue, 2);
                        _spriteBatch.DrawCircle(new CircleF(monster.Position, monster.AttackRange), 16, Color.Aqua, 2);
                    }
                }
                foreach (MonsterSlime monster in _monster.OfType<MonsterSlime>().ToList())
                {
                    _spriteBatch.DrawCircle(new CircleF(monster.SpawnPosition, monster.AwaySpawnRadius), 16, Color.DarkViolet, 2);
                    _spriteBatch.DrawCircle(new CircleF(monster.Position, monster.SreachRadius), 16, Color.RoyalBlue, 2);
                    _spriteBatch.DrawCircle(new CircleF(monster.Position, monster.ActiveRadius), 16, Color.DeepSkyBlue, 2);
                    _spriteBatch.DrawCircle(new CircleF(monster.Position, monster.AttackRange), 16, Color.Aqua, 2);
                }
            }

            // Particle
            hitParticle.Draw(_spriteBatch);
            deadParticle.Draw(_spriteBatch);
            fireParticleLight.Draw(_spriteBatch);
            fireParticleDark.Draw(_spriteBatch);
            _spriteBatch.End();

            // UI
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, samplerState: SamplerState.PointClamp);
            foreach (IYsort item in _ysort)
            {
                if (item is MonsterBoss)
                {
                    var boss = (MonsterBoss)item;
                    var pos = new Vector2(game1.ScreenWidth / 2 , 50); // 320 = ครึ่งนีงของความยาว UI
                    if (!boss.IsDead && boss.isInActiveRadius) boss.DrawUI(_spriteBatch, pos);
                }
            }
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

            foreach (var item in _collision)
            {
                _collisionComponent.Remove(item);
            }
            _collision.Clear();
            _monster.Clear();
            _ysort.Clear();
            _gameObject.Clear();
            _shadow.Clear();
            hitParticle = null;
            deadParticle = null;
            fireParticleDark = null;
            fireParticleLight = null;
            //particle.Dispose();
            //Content.Unload();

            base.UnloadContent();
        }
        // {------------------------------ Monster ------------------------------------------- } //
        private void LoadMonster()
        {
            var spawnPoint = _tileMaper.GetObjectLayer("SpawnPoint");
            foreach (var obj in spawnPoint.Objects)
            {
                if (obj.Name == "Melee")
                {
                    if (obj.Type == "Light")
                    {
                        _monster.Add(new MonsterMelee(obj.Position, _preventMonster, _player, hitParticle, deadParticle, ElementType.Light));
                    } 
                    else
                    {
                        _monster.Add(new MonsterMelee(obj.Position, _preventMonster, _player, hitParticle, deadParticle, ElementType.Dark));
                    }
                    LoadMonsterMelee((MonsterMelee)_monster.Last());
                }
                else if (obj.Name == "Range")
                {
                    if (obj.Type == "Light")
                    {
                        _monster.Add(new MonsterRange(obj.Position, _preventMonster, _player, hitParticle, deadParticle, fireParticleLight, ElementType.Light)); 
                    }
                    else
                    {
                        _monster.Add(new MonsterRange(obj.Position, _preventMonster, _player, hitParticle, deadParticle, fireParticleDark, ElementType.Dark));
                    }
                    LoadMonsterRange((MonsterRange)_monster.Last());
                }
                else if (obj.Name == "Boss")
                {
                    _monster.Add(new MonsterBoss(obj.Position, _preventMonster, _player, hitParticle, deadParticle, fireParticleLight, ElementType.Light));
                    LoadMonsterBoss((MonsterBoss)_monster.Last());
                }
                else if (obj.Name == "Slime")
                {
                    if (obj.Type == "Light")
                    {
                        _monster.Add(new MonsterSlime(obj.Position, _preventMonster, _player, hitParticle, deadParticle, ElementType.Light));
                    }
                    //else
                    //{
                    //    _monster.Add(new MonsterSlime(obj.Position, _preventMonster, _player, hitParticle, deadParticle, ElementType.Dark));
                    //}
                    LoadMonsterSlime((MonsterSlime)_monster.Last());
                }
            }
            //_monster.Add(new MonsterRange(new Vector2(2500, 2603), _preventMonster, _player, particle, Element.light));
            //_monster.Add(new MonsterMelee(new Vector2(2500, 2603), _preventMonster, _player, particle, Element.light));
        }

        private void LoadMonsterMelee(MonsterMelee monster)
        {
            if (monster.ElementType == ElementType.Light)
            {
                monster.LoadAnim("Walk", "LightGoonWalk", monster.Position, 128, 128, Content);
                monster.LoadAnim("Idle", "LightGoonIdle", monster.Position, 128, 128, Content);
                monster.LoadAnim("Attack", "LightGoonAttack", monster.Position, 128, 128, Content);
                monster.LoadAnim("Charge", "LightGoonCharge", monster.Position, 128, 128, Content);
                monster.LoadAnim("Die", "LightGoonFuckingDie-Sheet", monster.Position, 128, 128, Content);
            }
            else if (monster.ElementType == ElementType.Dark)
            {
                monster.LoadAnim("Walk", "DarkGoonWalk", monster.Position, 128, 128, Content);
                monster.LoadAnim("Idle", "DarkGoonIdle", monster.Position, 128, 128, Content);
                monster.LoadAnim("Attack", "DarkGoonAttack", monster.Position, 128, 128, Content);
                monster.LoadAnim("Charge", "DarkGoonCharge", monster.Position, 128, 128, Content);
                monster.LoadAnim("Die", "DarkGoonFuckingDie", monster.Position, 128, 128, Content);
            }
            monster.LoadUI(Content, "HealthBar5");
            monster.LoadSound(Content, _audioController, "WoodHit", "WoodDie");
            monster.CreateAnimation();
            monster.SetProperty(
                speed: 100f,
                sreachRadius: 500f,
                hp: 250,
                damage: 10,
                attackRange: (int)(monster.Width * 1.5),
                activeRadius: (int)(monster.Width * 2),
                dashForce: monster.Width * 7
            );
            _ysort.Add(monster);
            _collision.Add(monster.HurtBox);
            _collision.Add(monster.Collision);
        }

        private void LoadMonsterRange(MonsterRange monster)
        {
            if (monster.ElementType == ElementType.Light)
            {
                monster.loadBullet(Content, "LightBullet");
                monster.LoadAnim("Walk", "LightRegimogusIdle", monster.Position, 128, 128, Content);
                monster.LoadAnim("Attack", "LightRegimogusAttack", monster.Position, 128, 128, Content);
                monster.LoadAnim("Charge", "LightRegimogusCharge", monster.Position, 128, 128, Content);
                monster.LoadAnim("Idle", "LightRegimogusIdle", monster.Position, 128, 128, Content);
                monster.LoadAnim("Die", "LightRegimogusFuckingDie", monster.Position, 128, 128, Content);
            }
            else if (monster.ElementType == ElementType.Dark)
            {
                monster.loadBullet(Content, "DarkBullet");
                monster.LoadAnim("Walk", "DarkRegimogusIdle", monster.Position, 128, 128, Content);
                monster.LoadAnim("Attack", "DarkRegimogusAttack", monster.Position, 128, 128, Content);
                monster.LoadAnim("Charge", "DarkRegimogusCharge", monster.Position, 128, 128, Content);
                monster.LoadAnim("Idle", "DarkRegimogusIdle", monster.Position, 128, 128, Content);
                monster.LoadAnim("Die", "DarkRegimogusFuckingDie", monster.Position, 128, 128, Content);
            }
            monster.LoadUI(Content, "HealthBar5");
            monster.LoadSound(Content, _audioController, "StoneHit", "StoneDie");
            monster.CreateAnimation();
            monster.SetProperty(
                speed: 100f,
                sreachRadius: 500f,
                hp: 150,
                damage: 10,
                attackRange: (int)(monster.Width * 2.5f),
                dashForce: 300,
                bulletSpeed: 750
            );
            _ysort.Add(monster);
            _collision.Add(monster.HurtBox);
            _collision.Add(monster.Collision);
        }

        private void LoadMonsterBoss(MonsterBoss monster)
        {
            monster.LoadAnim("Walk", "LightGoonWalk", monster.Position, 128, 128, Content);
            monster.LoadAnim("Idle", "Light-VoidDevourer-Idle", monster.Position, 320, 384, Content);
            monster.LoadAnim("Attack", "LightGoonAttack", monster.Position, 128, 128, Content);
            monster.LoadAnim("Charge", "LightGoonCharge", monster.Position, 128, 128, Content);
            monster.LoadAnim("Casting", "Light-VoidDevourer-gooning", monster.Position, 320, 384, Content);
            monster.LoadAnim("Die", "LightGoonFuckingDie-Sheet", monster.Position, 128, 128, Content);
            monster.loadBullet(Content, "LightBullet", "DarkBullet");
            monster.LoadAssets(Content);
            monster.LoadUI(Content, "HealthBar7");
            monster.CreateAnimation();
            monster.SetProperty(
                speed: 100f,
                sreachRadius: 2000f,
                hp: 1000,
                damage: 10,
                attackRange: (int)(monster.Width * 7),
                activeRadius: (int)(monster.Width * 3),
                dashForce: monster.Width * 10,
                bulletSpeed: 750
            );
            _ysort.Add(monster);
            _collision.Add(monster.HurtBox);
            _collision.Add(monster.Collision);
        }
        private void LoadMonsterSlime(MonsterSlime monster)
        {
            if (monster.ElementType == ElementType.Light)
            {
                monster.LoadAnim("Idle", "LightSlimeIdle", monster.Position, 64, 64, Content);
                monster.LoadAnim("Walk", "LightSlimeAttack", monster.Position, 64, 192, Content);
                monster.LoadAnim("Attack", "LightSlimeAttack", monster.Position, 64, 192, Content);
                monster.LoadAnim("Charge", "LightSlimeCharge", monster.Position, 64, 64, Content);
                monster.LoadAnim("Die", "LightSlimeDie", monster.Position, 64, 64, Content);
            }
            //else if (monster.ElementType == ElementType.Dark)
            //{
            //    monster.LoadAnim("Walk", "DarkGoonWalk", monster.Position, 128, 128, Content);
            //    monster.LoadAnim("Idle", "DarkGoonIdle", monster.Position, 128, 128, Content);
            //    monster.LoadAnim("Attack", "DarkGoonAttack", monster.Position, 128, 128, Content);
            //    monster.LoadAnim("Charge", "DarkGoonCharge", monster.Position, 128, 128, Content);
            //    monster.LoadAnim("Die", "DarkGoonFuckingDie", monster.Position, 128, 128, Content);
            //}
            monster.LoadUI(Content, "HealthBar5");
            monster.LoadSound(Content, _audioController, "SlimeHit", "SlimeDie");
            monster.CreateAnimation();
            monster.SetProperty(
                speed: 100f,
                sreachRadius: 500f,
                hp: 250,
                damage: 10,
                attackRange: (int)(monster.Width * 1.5),
                activeRadius: (int)(monster.Width * 2),
                dashForce: monster.Width * 7
            );
            _ysort.Add(monster);
            _collision.Add(monster.HurtBox);
            _collision.Add(monster.Collision);
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
            foreach (MonsterBoss monster in _monster.OfType<MonsterBoss>().ToList())
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
            foreach (MonsterSlime monster in _monster.OfType<MonsterSlime>().ToList())
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
        }
        // {-------------------------- End of Monster ---------------------------------------- } //
    }
}
