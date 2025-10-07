using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Assimp;
using Assimp.Unmanaged;
using game;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Collisions.Layers;
using MonoGame.Extended.Screens;
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
        public List<IEntity> PendingAdd = new List<IEntity>();
        public List<IEntity> PendingRemove = new List<IEntity>();
        public List<IMonster> PendingMonsterRemove = new List<IMonster>();

        // Collision & Layer
        public List<IEntity> Collisions = new List<IEntity>();
        public CollisionComponent CollisionComponents;

        public List<GameObject> GameObjects = new List<GameObject>();
        public List<GameObject> Shadow = new List<GameObject>();

        // Camera
        public GlobalCamera Camera;
        public OrthographicCamera _Camera;

        // Particle
        public HitParticle HitParticle;
        public DeadParticle DeadParticle;
        public FireParticle FireParticleLight;
        public FireParticle FireParticleDark;

        // Audio
        public AudioController audioController;

        // Chest
        public List<Chest> Chests = new List<Chest>();

        // Other Setting
        public Game1 _Game1;
        public SpriteBatch SpriteBatch;
        public KeyboardState Ks, OldKs; // keyboard
        public bool isGameEnd = false;
        private string hitSound = "AttackHitWhosh";
        private string deadSound = "dead5";
        private string parrySound = "AttackHitWhosh";
        private string fireSound = "Attack.NoHit";

        public GlobalContext(Game game) {
            SpriteBatch = new SpriteBatch(game.GraphicsDevice);
            _Game1 = (Game1)game;

            // Collision
            Collisions = _Game1.Collision;
            CollisionComponents = _Game1.CollisionComponent;

            // Tile Map
            TileMaper = new TileMaper(game);

            // Audio
            audioController = _Game1.audioController;

            // Camera
            Camera = _Game1.camera;
            _Camera = Camera.Cam;
        }

        public void PlayBGM()
        {
            var Content = _Game1.Content;
            var song = Content.Load<Song>("Audio/mixkit-jumping-around-8");
            audioController.SongVolume = 0.1f;
            audioController.PlaySong(song);
        }

        // ----------------------------------------------------------------------------------------------------- //
        //                                          LOAD                                                         //
        // ----------------------------------------------------------------------------------------------------- //

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

            var shadowLayer = TileMaper.GetObjectLayer("Shadow");
            foreach (var item in shadowLayer.Objects)
            {
                Shadow.Add(new GameObject(item.Position, Content.Load<Texture2D>("TileMap/" + item.Type)));
            }
        }

        public void LoadChests(Player player)
        {
            var Content = _Game1.Content;
            var spawnpoint = TileMaper.GetObjectLayer("SpawnPoint");
            foreach (var obj in spawnpoint.Objects)
            {
                if (obj.Name == "Chest")
                {
                    var sfx = Content.Load<SoundEffect>("Audio/OpenChest");
                    var _chest = new Chest();
                    _chest.Load(Content, "chest", 64, 64, obj.Position, "F", player.potion, audioController, sfx);
                    Chests.Add(_chest);
                    Collisions.Add(_chest.Hitbox);
                    Ysort.Add(_chest);
                }
            }
        }

        #region Load Player
        public void LoadPlayer(PreventMonster preventMonster, Player player)
        {
            if (player.DestinationPos == Vector2.Zero)
            {
                var spawnPoint = TileMaper.GetObjectLayer("SpawnPoint");
                foreach (var obj in spawnPoint.Objects)
                {
                    if (obj.Name == "Player")
                    {
                        player._movement.SetPosition(obj.Position);
                        //Debug.WriteLine("OBJ Pos : " + obj.Position);
                        break;
                    }
                }
                //Debug.WriteLine("No DestinationPos : " + player.DestinationPos);
            }
            else
            {
                player._movement.SetPosition(player.DestinationPos);
                //Debug.WriteLine("Have DestinationPos : " + player.DestinationPos);
            }

            player.SetWorldReferences(Collisions, CollisionComponents);
            Ysort.Add(player);
            Collisions.Add(preventMonster);
            CollisionComponents.Insert(preventMonster);
        }
        #endregion

        #region Load All Monster
        public void LoadMonster(PreventMonster _preventMonster, Player _player)
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
                    LoadMonsterMelee((MonsterMelee)Monsters.Last());
                }
                else if (obj.Name == "Range")
                {
                    if (obj.Type == "Light")
                    {
                        Monsters.Add(new MonsterRange(obj.Position, _preventMonster, _player, HitParticle, DeadParticle, FireParticleLight, ElementType.Light));
                    }
                    else
                    {
                        Monsters.Add(new MonsterRange(obj.Position, _preventMonster, _player, HitParticle, DeadParticle, FireParticleDark, ElementType.Dark));
                    }
                    LoadMonsterRange((MonsterRange)Monsters.Last());
                }
                else if (obj.Name == "Boss")
                {
                    Monsters.Add(new MonsterBoss(obj.Position, _preventMonster, _player, HitParticle, DeadParticle, FireParticleLight, ElementType.Light));
                    LoadMonsterBoss((MonsterBoss)Monsters.Last());
                }
                else if (obj.Name == "Slime")
                {
                    if (obj.Type == "Light")
                    {
                        Monsters.Add(new MonsterSlime(obj.Position, _preventMonster, _player, HitParticle, DeadParticle, ElementType.Light));
                    }
                    else
                    {
                        Monsters.Add(new MonsterSlime(obj.Position, _preventMonster, _player, HitParticle, DeadParticle, ElementType.Dark));
                    }
                    LoadMonsterSlime((MonsterSlime)Monsters.Last());
                }
            }
            //_monster.Add(new MonsterRange(new Vector2(2500, 2603), _preventMonster, _player, particle, Element.light));
            //_monster.Add(new MonsterMelee(new Vector2(2500, 2603), _preventMonster, _player, particle, Element.light));
        }
        #endregion

        #region Load Monster Melee
        private void LoadMonsterMelee(MonsterMelee monster)
        {
            var Content = _Game1.Content;
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
            monster.LoadSound(Content, audioController, hitSound, deadSound);
            monster.CreateAnimation();
            monster.SetProperty(
                speed: 100f,
                sreachRadius: 500f,
                hp: 140,
                damage: 30,
                attackRange: (int)(monster.Width * 1.5),
                activeRadius: (int)(monster.Width * 1.5),
                dashForce: monster.Width * 7
            );
            Ysort.Add(monster);
            Collisions.Add(monster.HurtBox);
            Collisions.Add(monster.Collision);
        }
        #endregion

        #region Load Monster Range
        private void LoadMonsterRange(MonsterRange monster)
        {
            var Content = _Game1.Content;
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
            monster.LoadSound(Content, audioController, hitSound, deadSound, parrySound, fireSound);
            monster.CreateAnimation();
            monster.SetProperty(
                speed: 75f,
                sreachRadius: 500f,
                hp: 100,
                damage: 35,
                attackRange: (int)(monster.Width * 2.5f),
                dashForce: 300,
                bulletSpeed: 750
            );
            Ysort.Add(monster);
            Collisions.Add(monster.HurtBox);
            Collisions.Add(monster.Collision);
        }
        #endregion

        #region Load Monster Boss

        private void LoadMonsterBoss(MonsterBoss monster)
        {
            var Content = _Game1.Content;
            monster.LoadAnim("Idle", "Light-VoidDevourer-Idle", monster.Position, 320, 384, Content);
            monster.LoadAnim("Walk", "Light-VoidDevourer-Idle", monster.Position, 320, 384, Content);
            monster.LoadAnim("Die", "LightGoonFuckingDie-Sheet", monster.Position, 128, 128, Content);

            monster.LoadAnim("ChargeRapidFire", "Light-VoidDevouer-HeavyMachineGun", monster.Position, 320, 384, Content);
            monster.LoadAnim("RapidFire", "Light-VoidDevouer-HeavyMachineGun", monster.Position, 320, 384, Content);
            monster.LoadAnim("EndRapidFire", "Light-VoidDevouer-HeavyMachineGun", monster.Position, 320, 384, Content);

            monster.LoadAnim("ChargeFire3Ball", "Light-VoidDevourer-3Balls", monster.Position, 320, 384, Content);
            monster.LoadAnim("Fire3Ball", "Light-VoidDevourer-3Balls", monster.Position, 320, 384, Content);

            monster.LoadAnim("ChargeLineSpike", "Light-VoidDevouer-Attack", monster.Position, 320, 384, Content);
            monster.LoadAnim("LineSpike", "Light-VoidDevouer-Attack", monster.Position, 320, 384, Content);
            monster.LoadAnim("EndLineSpike", "Light-VoidDevouer-Attack", monster.Position, 320, 384, Content);

            monster.LoadAnim("ChargeDash", "Light-VoidDevouer-Dash", monster.Position, 320, 384, Content);
            monster.LoadAnim("Dash", "Light-VoidDevouer-Dash", monster.Position, 320, 384, Content);
            monster.LoadAnim("EndDash", "Light-VoidDevouer-Dash", monster.Position, 320, 384, Content);

            monster.LoadAnim("ChargeFollowSpike", "Light-VoidDevouer-Dash", monster.Position, 320, 384, Content);
            monster.LoadAnim("FollowSpike", "Light-VoidDevouer-Dash", monster.Position, 320, 384, Content);
            monster.LoadAnim("EndFollowSpike", "Light-VoidDevouer-Dash", monster.Position, 320, 384, Content);

            monster.LoadAnim("Casting", "Light-VoidDevourer-gooning", monster.Position, 320, 384, Content);

            monster.loadBullet(Content, "LightBullet", "DarkBullet");
            monster.LoadSound(Content, audioController, hitSound, deadSound, parrySound);
            monster.LoadAssets(Content);
            monster.LoadUI(Content, "HealthBar7");
            monster.CreateAnimation();
            monster.SetProperty(
                speed: 100f,
                sreachRadius: 2000f,
                hp: 1000,
                damage: 10,
                attackRange: (int)(monster.Width * 5),
                activeRadius: (int)(monster.Width * 2),
                dashForce: monster.Width * 4,
                bulletSpeed: 750
            );
            Ysort.Add(monster);
            Collisions.Add(monster.HurtBox);
            Collisions.Add(monster.Collision);
        }
        #endregion

        #region Load Monster Slime
        private void LoadMonsterSlime(MonsterSlime monster)
        {
            var Content = _Game1.Content;
            if (monster.ElementType == ElementType.Light)
            {
                monster.LoadAnim("Idle", "LightSlimeIdle", monster.Position, 64, 64, Content);
                monster.LoadAnim("Walk", "LightSlimeAttack", monster.Position, 64, 192, Content);
                monster.LoadAnim("Attack", "LightSlimeAttack", monster.Position, 64, 192, Content);
                monster.LoadAnim("Charge", "LightSlimeCharge", monster.Position, 64, 64, Content);
                monster.LoadAnim("Die", "LightSlimeDie", monster.Position, 64, 64, Content);
            }
            else if (monster.ElementType == ElementType.Dark)
            {
                monster.LoadAnim("Idle", "DarkSlimeIdle", monster.Position, 64, 64, Content);
                monster.LoadAnim("Walk", "DarkSlimeAttack", monster.Position, 64, 192, Content);
                monster.LoadAnim("Attack", "DarkSlimeAttack", monster.Position, 64, 192, Content);
                monster.LoadAnim("Charge", "DarkSlimeCharge", monster.Position, 64, 192, Content);
                monster.LoadAnim("Die", "DarkSlimeDie", monster.Position, 64, 64, Content);
            }
            monster.LoadUI(Content, "HealthBar5");
            monster.LoadSound(Content, audioController, hitSound, deadSound);
            monster.CreateAnimation();
            monster.SetProperty(
                speed: 100f,
                sreachRadius: 500f,
                hp: 60,
                damage: 10,
                attackRange: (int)(monster.Width * 1.5),
                activeRadius: (int)(monster.Width * 1.5),
                dashForce: monster.Width * 9
            );
            Ysort.Add(monster);
            Collisions.Add(monster.HurtBox);
            Collisions.Add(monster.Collision);
        }
        #endregion

        // IMPORTANT NOTE : อาจจะไม่ค่อยเสถียรและแก้ไขยาก
        public void LoadAll(ContentManager Content, string sceneName, PreventMonster preventMonster, Player player)
        {
            LoadParticle();
            LoadTiledMap(Content, sceneName);
            LoadChests(player);
            LoadMonster(preventMonster, player);
            LoadPlayer(preventMonster, player);
            PlayBGM();
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

        public void UpdateChest(Vector2 playerPos)
        {
            foreach (var item in Chests)
            {
                item.Update(playerPos);
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

        public void UpdatePendinQueue()
        {
            // Flush queued additions
            foreach (var entity in PendingAdd)
            {
                Collisions.Add(entity);
                CollisionComponents.Insert(entity);
            }

            // Flush queued additions
            foreach (var entity in PendingRemove)
            {
                Collisions.Remove(entity);
                CollisionComponents.Remove(entity);
            }

            // Clear queues
            PendingAdd.Clear();
            PendingRemove.Clear();
        }

        public void UpdateTiledMaper(GameTime gameTime)
        {
            TileMaper.UpdateMap(gameTime);
        }

        //public void UpdateMonster(GameTime gameTime, Player _player, Texture2D _healTexture)
        //{
        //    foreach (MonsterMelee monster in Monsters.OfType<MonsterMelee>().ToList())
        //    {
        //        monster.UpdateState(gameTime, Collisions, CollisionComponents, _player._movement.Position);
        //        if (monster.ShakeViewport)
        //        {
        //            Camera.ShakeCamera(gameTime);
        //            monster.ShakeViewport = Camera.ShakeViewport;
        //        }

        //        if (monster.IsDead)
        //        {
        //            DropManager.DropHeal(_healTexture, _player, _collisionComponent, monster.Position, _pendingRemove);
        //            monster.DeleteHitBox(1f, Collisions, CollisionComponents);
        //            monster.RemoveMonster();
        //            Monsters.Remove(monster);
        //            break;
        //        }
        //    }

        //    foreach (MonsterRange monster in Monsters.OfType<MonsterRange>().ToList())
        //    {
        //        monster.UpdateState(gameTime, Collisions, CollisionComponents, _player._movement.Position);
        //        if (monster.ShakeViewport)
        //        {
        //            Camera.ShakeCamera(gameTime);
        //            monster.ShakeViewport = Camera.ShakeViewport;
        //        }

        //        if (monster.IsDead)
        //        {
        //            DropManager.DropHeal(_healTexture, _player, _collisionComponent, monster.Position, _pendingRemove);
        //            monster.DeleteHitBox(1f);
        //            monster.RemoveMonster();
        //            Monsters.Remove(monster);
        //            break;
        //        }
        //    }
        //}

        // ----------------------------------------------------------------------------------------------------- //
        //                                          UPDATE END                                                   //
        // ----------------------------------------------------------------------------------------------------- //

        // ----------------------------------------------------------------------------------------------------- //
        //                                          DRAW                                                         //
        // ----------------------------------------------------------------------------------------------------- //

        #region Update Monster
        public void UpdateMonster(GameTime gameTime, Player _player, Texture2D healTexture)
        {
            // Loop through all monsters
            foreach (var monster in Monsters.ToList())
            {
                var playerPos = _player._movement.Position;
                HandleUpdateEachMonster(monster, gameTime,_player, healTexture);
            }

            // Remove dead monsters
            foreach (var deadMonster in PendingMonsterRemove)
                Monsters.Remove(deadMonster);
            PendingMonsterRemove.Clear();
        }
        #endregion

        #region Monster Extra
        private void HandleUpdateEachMonster(IMonster monster, GameTime gameTime, Player _player, Texture2D healTexture)
        {
            var playerPos = _player._movement.Position;
            monster.UpdateState(gameTime, Collisions, CollisionComponents, playerPos);
            HandleShakeCamera(monster, gameTime);
            if (monster.IsDead)
                HandleMonsterDeath(monster, healTexture, _player);
        }

        // Shake camera helper
        private void HandleShakeCamera(dynamic monster, GameTime gameTime)
        {
            if (monster.ShakeViewport)
            {
                Camera.ShakeCamera(gameTime);
                monster.ShakeViewport = Camera.ShakeViewport;
            }
        }

        // Handle monster death and spawn heal pickup
        private void HandleMonsterDeath(IMonster monster, Texture2D _healTexture, Player _player)
        {
            // Spawn heal pickup via DropManager
            var healPickup = DropManager.DropHeal(
                _healTexture,
                _player,
                CollisionComponents,
                monster.Position,
                PendingRemove
            );
            PendingAdd.Add(healPickup);
            Ysort.Add(healPickup);

            if (monster is MonsterBoss)
            {
                isGameEnd = true;
            }

            monster.UnLoad();
            PendingMonsterRemove.Add(monster);
        }

        /// <summary>
        /// Helper function to spawn a heal pickup
        /// </summary>
        private void SpawnHeal(Vector2 position, Texture2D _healTexture, Player _player)
        {
            var healPickup = DropManager.DropHeal(_healTexture, _player, CollisionComponents, position, PendingRemove);
            PendingAdd.Add(healPickup);
            Debug.WriteLine("Spawned HealPickup at: " + position);
        }
        #endregion

        public void DrawObject(SpriteBatch _spriteBatch)
        {
            // Draw shadows behind entities
            foreach (var shadow in Shadow)
                shadow.Draw(_spriteBatch);

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

        public void DrawBossUI(SpriteBatch _spriteBatch)
        {
            // Begin UI sprite batch (screen space)
            //_spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, samplerState: SamplerState.PointClamp);

            // Draw UI elements like boss health bars
            if (Monsters.Exists(x => x is MonsterBoss))
            {
                var Boss = (MonsterBoss)Monsters.Find(x => x.GetType() == typeof(MonsterBoss));
                if (!Boss.IsDead && Boss.isInActiveRadius)
                {
                    Boss.DrawUI(_spriteBatch, new Vector2(_Game1.ScreenWidth / 2, 50));
                } 
            }

            //_spriteBatch.End();
        }

        public void DrawBossSkill(SpriteBatch _spriteBatch)
        {
            if (Monsters.Exists(x => x is MonsterBoss))
            {
                var Boss = (MonsterBoss)Monsters.Find(x => x.GetType() == typeof(MonsterBoss));
                if (!Boss.IsDead)
                {
                    Boss.DrawSkill(_spriteBatch);
                }
            }
        }

        public void DrawPotionUI(SpriteBatch spriteBatch, Player player, Texture2D UI, SpriteFont font)
        {
            var potionAmout = player.potion.Amout;
            string potionStr = "Potion x" + potionAmout;
            Color tint = player.potion.isinCoolDown ? Color.Gray : Color.White;
            spriteBatch.Draw(UI, new Vector2(1060, 650 - 8), tint);
            spriteBatch.DrawString(font, potionStr, new Vector2(1100, 650), tint);
        }

        // IMPORTANT NOTE : อาจจะไม่ค่อยเสถียรและแก้ไขยาก
        public void DrawAll(SpriteBatch spriteBatch)
        {
            DrawTiledMaper();
            DrawObject(spriteBatch);
            DrawParticle(spriteBatch);
            DrawBossSkill(spriteBatch);
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
            Monsters.Clear(); // Monster
            Monsters = null;

            foreach (var item in Collisions)
            {
                CollisionComponents.Remove(item);
            }
            CollisionComponents = null;

            Collisions.Clear(); // Collision
            Collisions = null;

            Ysort.Clear(); // Ysort
            Ysort = null;

            Shadow.Clear(); // Shadow
            Shadow = null;

            GameObjects.Clear(); // Object
            GameObjects = null;

            // Chest
            foreach (var chest in Chests)
            {
                chest.Unload();
            }
            Chests.Clear(); 
            Chests = null;

            Camera = null; // Camera
            _Camera = null; //Camera

            // TileMaper
            TileMaper = null;

            // Particle
            HitParticle = null;
            DeadParticle = null;
            FireParticleDark = null;
            FireParticleLight = null;

            // AudioController
            audioController.PauseAudio();
            audioController = null;

        }

        // ----------------------------------------------------------------------------------------------------- //
        //                                        UNLOAD END                                                     //
        // ----------------------------------------------------------------------------------------------------- //
    }
}

