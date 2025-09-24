using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Assimp;
using Assimp.Unmanaged;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Collisions.Layers;
using MonoGame.Extended.Timers;

namespace game
{
    public class GlobalContext
    {
        // Tile Map
        public TileMaper TileMaper;
        public List<IYsort> Ysort = new List<IYsort>();

        // Monster
        public List<IMonster> Monsters = new List<IMonster>();

        // Collision & Layer
        public List<IEntity> Collisions = new List<IEntity>();
        public CollisionComponent CollisionComponents;
        public List<GameObject> GameObjects = new List<GameObject>();

        // Camera
        public GlobalCamera Camera;
        public OrthographicCamera _Camera;

        // Particle
        public HitParticle HitParticle;
        public DeadParticle DeadParticle;
        public FireParticle FireParticleLight;
        public FireParticle FireParticleDark;

        // Other Setting
        public Game1 _Game1;
        public SpriteBatch SpriteBatch;
        public KeyboardState Ks, OldKs; // keyboard

        public GlobalContext(Game game) {
            SpriteBatch = new SpriteBatch(game.GraphicsDevice);
            _Game1 = (Game1)game;

            // Collision
            Collisions = _Game1.Collision;
            CollisionComponents = _Game1.CollisionComponent;

            // Tile Map
            TileMaper = new TileMaper(game);
        }

        // ----------------------------------------------------------------------------------------------------- //
        //                                          LOAD                                                         //
        // ----------------------------------------------------------------------------------------------------- //

        public void LoadCamera()
        {
            Camera = _Game1.camera;
            _Camera = Camera.Cam;
        }

        public void LoadParticle()
        {
            // Particle
            HitParticle = new HitParticle(_Game1);
            DeadParticle = new DeadParticle(_Game1);
            FireParticleLight = new FireParticle(_Game1);
            FireParticleDark = new FireParticle(_Game1);
        }

        public void LoadTiledMap(ContentManager Content, string sceneName)
        {
            //Tile Map
            TileMaper.LoadMap(Content, sceneName);
            TileMaper.LoadCollision(CollisionComponents, Collisions, "Collision");

            // Game Object
            var objectLayer = TileMaper.GetObjectLayer("Object");
            foreach (var item in objectLayer.Objects)
            {
                GameObjects.Add(new GameObject(item.Position, Content.Load<Texture2D>("TileMap/" + item.Type)));
                Ysort.Add(GameObjects.Last());
            }
        }

        public void LoadMonster(ContentManager Content, PreventMonster _preventMonster,Player _player)
        {
            var spawnPoint = TileMaper.GetObjectLayer("SpawnPoint");
            foreach (var obj in spawnPoint.Objects)
            {
                if (obj.Name == "Melee")
                {
                    if (obj.Type == "Light")
                    {
                        Monsters.Add(new MonsterMelee(obj.Position, _preventMonster, _player, HitParticle, DeadParticle, ElementType.Light));
                    }
                    else
                    {
                        Monsters.Add(new MonsterMelee(obj.Position, _preventMonster, _player, HitParticle, DeadParticle, ElementType.Dark));
                    }
                }
                if (obj.Name == "Range")
                {
                    if (obj.Type == "Light")
                    {
                        Monsters.Add(new MonsterRange(obj.Position, _preventMonster, _player, HitParticle, DeadParticle, FireParticleLight, ElementType.Light));
                    }
                    else
                    {
                        Monsters.Add(new MonsterRange(obj.Position, _preventMonster, _player, HitParticle, DeadParticle, FireParticleDark, ElementType.Dark));
                    }
                }
            }
            //_monster.Add(new MonsterRange(new Vector2(2500, 2603), _preventMonster, _player, particle, Element.light));
            //_monster.Add(new MonsterMelee(new Vector2(2500, 2603), _preventMonster, _player, particle, Element.light));
            foreach (MonsterMelee monster in Monsters.OfType<MonsterMelee>().ToList())
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
                monster.LoadUI(Content, "HealthBar_thumb");
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
                Ysort.Add(monster);
                Collisions.Add(monster.HurtBox);
                Collisions.Add(monster.Collision);
            }
            foreach (MonsterRange monster in Monsters.OfType<MonsterRange>().ToList())
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
                monster.LoadUI(Content, "HealthBar_thumb");
                monster.CreateAnimation();
                monster.SetProperty(
                    speed: 100f,
                    sreachRadius: 500f,
                    hp: 150,
                    damage: 10,
                    attackRange: (int)(monster.Width * 2.5f),
                    dashForce: 300
                );
                Ysort.Add(monster);
                Collisions.Add(monster.HurtBox);
                Collisions.Add(monster.Collision);
            }
        }

        // IMPORTANT NOTE : อาจจะไม่ค่อยเสถียรและแก้ไขยาก
        public void LoadAll(ContentManager Content, string sceneName/*, PreventMonster preventMonster, Player player*/)
        {
            LoadCamera();
            LoadParticle();
            LoadTiledMap(Content, sceneName);
            //LoadMonster(Content, preventMonster, player);
        }
        // ----------------------------------------------------------------------------------------------------- //
        //                                          LOAD END                                                     //
        // ----------------------------------------------------------------------------------------------------- //

        // ----------------------------------------------------------------------------------------------------- //
        //                                          UPDATE                                                       //
        // ----------------------------------------------------------------------------------------------------- //

        public void UpdateCamera(Vector2 position)
        {
            // Camera
            Camera.Update(position);
            Camera.AdjustZoom();
            //Debug.WriteLine(_camera.Zoom);
        }

        public void UpdateParticle(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            // Particle
            if (HitParticle != null && DeadParticle != null && FireParticleLight != null && FireParticleDark != null)
            {
                HitParticle.Update(deltaTime);
                DeadParticle.Update(deltaTime);
                FireParticleLight.Update(deltaTime);
                FireParticleDark.Update(deltaTime);
            }
            else
            {
                Debug.WriteLine("ERROR : Particle is null (Update)");
            }
        }

        public void UpdateYsort()
        {
            // Ysort
            Ysort.Sort((a, b) =>
            {
                // เปรียบเทียบ SortY ก่อน
                int yComparison = a.SortY.CompareTo(b.SortY);
                if (yComparison != 0)
                    return yComparison;

                // ถ้า SortY เท่ากัน ใช้ Position.X เป็นเงื่อนไขรอง
                return b.SortX.CompareTo(a.SortX);
            });
        }

        public void UpdateTiledMaper(GameTime gameTime)
        {
            TileMaper.UpdateMap(gameTime);
        }

        public void UpdateMonster(GameTime gameTime, Player _player, Texture2D _healTexture)
        {
            foreach (MonsterMelee monster in Monsters.OfType<MonsterMelee>().ToList())
            {
                monster.UpdateState(gameTime, Collisions, CollisionComponents, _player._movement.Position);
                if (monster.ShakeViewport)
                {
                    Camera.ShakeCamera(gameTime);
                    monster.ShakeViewport = Camera.ShakeViewport;
                }
                // Temporary
                // Will make additional method for monster dead and drop
                // ps. make a new global class and make a drop heal there, then call it in remove monster(maybe)
                if (monster.IsDead)
                {
                    monster.DropHeal(Collisions, CollisionComponents, _healTexture, _player);
                    monster.DeleteHitBox(1f, Collisions, CollisionComponents);
                    monster.RemoveMonster();
                    Monsters.Remove(monster);
                    break; // Exit the loop to avoid modifying the collection while iterating; list bug prevented
                }
                //--------------
            }
            foreach (MonsterRange monster in Monsters.OfType<MonsterRange>().ToList())
            {
                monster.UpdateState(gameTime, Collisions, CollisionComponents, _player._movement.Position);
                if (monster.ShakeViewport)
                {
                    Camera.ShakeCamera(gameTime);
                    monster.ShakeViewport = Camera.ShakeViewport;
                }
                // Temporary
                // Will make additional method for monster dead and drop
                // ps. make a new global class and make a drop heal there, then call it in remove monster(maybe)
                if (monster.IsDead)
                {
                    monster.DropHeal(Collisions, CollisionComponents, _healTexture, _player);
                    monster.DeleteHitBox(1f);
                    monster.RemoveMonster();
                    Monsters.Remove(monster);
                    break; // Exit the loop to avoid modifying the collection while iterating; list bug prevented
                }
                //--------------
            }
        }

        // ----------------------------------------------------------------------------------------------------- //
        //                                          UPDATE END                                                   //
        // ----------------------------------------------------------------------------------------------------- //

        // ----------------------------------------------------------------------------------------------------- //
        //                                          DRAW                                                         //
        // ----------------------------------------------------------------------------------------------------- //

        public void DrawObject(SpriteBatch _spriteBatch)
        {
            // Object
            foreach (var item in Ysort)
            {
                item.Draw(_spriteBatch);
            }
        }

        public void DrawParticle(SpriteBatch _spriteBatch)
        {
            // Particle
            if (HitParticle != null && DeadParticle != null && FireParticleLight != null && FireParticleDark != null)
            {
                HitParticle.Draw(_spriteBatch);
                DeadParticle.Draw(_spriteBatch);
                FireParticleLight.Draw(_spriteBatch);
                FireParticleDark.Draw(_spriteBatch);
            } 
            else
            {
                Debug.WriteLine("ERROR : Particle is null (Draw)");
            }
        }

        public void DrawTiledMaper()
        {
            TileMaper.DrawMap(_Camera);
        }

        // IMPORTANT NOTE : อาจจะไม่ค่อยเสถียรและแก้ไขยาก
        public void DrawAll(SpriteBatch spriteBatch)
        {
            DrawTiledMaper();
            DrawObject(spriteBatch);
            DrawParticle(spriteBatch);
        }
        // ----------------------------------------------------------------------------------------------------- //
        //                                        DRAW END                                                       //
        // ----------------------------------------------------------------------------------------------------- //

        // ----------------------------------------------------------------------------------------------------- //
        //                                        UNLOAD                                                         //
        // ----------------------------------------------------------------------------------------------------- //

        public void UnloadAll()
        {
            foreach (IMonster monster in Monsters)
            {
                monster.UnLoad(); // actually calls UnLoad on each monster
            }

            foreach (var item in Collisions)
            {
                CollisionComponents.Remove(item);
            }
            Collisions.Clear(); // Collision
            Monsters.Clear(); // Monster
            Ysort.Clear(); // Ysort
            GameObjects.Clear(); // Object

            // Particle
            HitParticle = null;
            DeadParticle = null;
            FireParticleDark = null;
            FireParticleLight = null;
        }

        // ----------------------------------------------------------------------------------------------------- //
        //                                        UNLOAD END                                                     //
        // ----------------------------------------------------------------------------------------------------- //
    }
}

