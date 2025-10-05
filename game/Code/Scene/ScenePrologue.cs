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
        private GlobalContext globalContext;

        // Player
        private Player player;
        private PreventMonster preventMonster;

        // Collision & Layer
        private List<IEntity> _collision;
        private CollisionComponent _collisionComponent;

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

            player = game1.Player;
            preventMonster = game1.PreventMonster;

            globalContext = new GlobalContext(Game);
        }


        public override void LoadContent()
        {
            // Load temporary drop texture
            _healTexture = Content.Load<Texture2D>("Texture/Health");

            globalContext.LoadAll(Content, "ScenePrologue", preventMonster, player);

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
            #region Debug
            if (_ks.IsKeyDown(Keys.O) && !_oldKs.IsKeyDown(Keys.O))
            {
                isDebug = !isDebug;
            }
            if (_ks.IsKeyDown(Keys.L) && !_oldKs.IsKeyDown(Keys.L))
            {
                if (player.Stats.Speed.Value <= 900)
                {
                    player.Stats.Speed.AddModifier(1500);
                    player.Stats.AttackDamage.AddModifier(10000000);
                    globalContext._Camera.MinimumZoom = 0.1f;
                }
                else
                {
                    player.Stats.Speed.RemoveModifier(1500);
                    player.Stats.AttackDamage.RemoveModifier(10000000);
                    globalContext._Camera.MinimumZoom = 1;
                    globalContext._Camera.Zoom = 1;
                }
            }
            if (!_ks.IsKeyDown(Keys.Enter) && _oldKs.IsKeyDown(Keys.Enter))
            {
                ScreenManager.LoadScreen(new SceneMenu(game1));
            }
            if(player.Stats.CurrentHP == 0)
            {
                ScreenManager.LoadScreen(new SceneMenu(game1));
                player.Stats.Heal(100000);
            }
            #endregion

            // Player
            player.Update(gameTime, globalContext._Camera);
            var playerpos = player._movement.Position;
            preventMonster.UpdatePosition(playerpos);

            globalContext.UpdateCamera(playerpos - new Vector2(game1.ScreenWidth / 2, game1.ScreenHeight / 2));
            globalContext.UpdateParticle(gameTime);
            globalContext.UpdateMonster(gameTime, player, _healTexture); // รอ player
            globalContext.UpdatePendinQueue();
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



            // Begin main camera sprite batch (world space)
            _spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                transformMatrix: globalContext._Camera.GetViewMatrix()
            );

            globalContext.DrawAll(_spriteBatch);

            // Optional debug overlay (collisions, monster ranges, etc.)
            if (isDebug)
                DebugDraw();

            _spriteBatch.End();

            globalContext.DrawBossUI(_spriteBatch);
        }
        private void DebugDraw()
        {
            // Camera reference point
            _spriteBatch.DrawRectangle(new RectangleF(globalContext._Camera.Position, new SizeF(5, 5)), Color.Red, 5, 0);

            foreach (IEntity entity in _collision)
                entity.Draw(_spriteBatch);

            preventMonster.Draw(_spriteBatch);

            foreach (var monster in globalContext.Monsters)
            {
                if (monster is MonsterMelee mm)
                    DrawMonsterDebug(mm);
                else if (monster is MonsterRange mr)
                    DrawMonsterDebug(mr);
                else if (monster is MonsterBoss mb && !mb.IsDead)
                    DrawMonsterDebug(mb);
                else if (monster is MonsterSlime ms)
                    DrawMonsterDebug(ms);
            }
        }

        private void DrawMonsterDebug(IMonster monster)
        {
            _spriteBatch.DrawCircle(new CircleF(monster.SpawnPosition, monster.AwaySpawnRadius), 16, Color.DarkViolet, 2);
            _spriteBatch.DrawCircle(new CircleF(monster.Position, monster.SreachRadius), 16, Color.RoyalBlue, 2);

            switch (monster)
            {
                case MonsterMelee mm:
                    _spriteBatch.DrawCircle(new CircleF(mm.Position, mm.ActiveRadius), 16, Color.DeepSkyBlue, 2);
                    _spriteBatch.DrawCircle(new CircleF(mm.Position, mm.AttackRange), 16, Color.Aqua, 2);
                    break;
                case MonsterRange mr:
                    _spriteBatch.DrawCircle(new CircleF(mr.Position, mr.AttackRange), 16, Color.Aqua, 2);
                    break;
                case MonsterBoss mb:
                    if (!mb.IsDead)
                    {
                        _spriteBatch.DrawCircle(new CircleF(mb.Position, mb.ActiveRadius), 16, Color.DeepSkyBlue, 2);
                        _spriteBatch.DrawCircle(new CircleF(mb.Position, mb.AttackRange), 16, Color.Aqua, 2);
                    }
                    break;
                case MonsterSlime ms:
                    _spriteBatch.DrawCircle(new CircleF(ms.Position, ms.ActiveRadius), 16, Color.DeepSkyBlue, 2);
                    _spriteBatch.DrawCircle(new CircleF(ms.Position, ms.AttackRange), 16, Color.Aqua, 2);
                    break;
            }
        }



        public override void UnloadContent()
        {
            //foreach (IMonster monster in _monster)
            //{
            //    monster.UnLoad(); // actually calls UnLoad on each monster
            //}

            //foreach (var item in _collision)
            //{
            //    _collisionComponent.Remove(item);
            //}
            //_collision.Clear();
            //_monster.Clear();
            //_gameObject.Clear();
            //_ysort.Clear();
            //_shadow.Clear();
            //hitParticle = null;
            //deadParticle = null;
            //fireParticleDark = null;
            //fireParticleLight = null;

            _healTexture = null;
            globalContext.UnloadAll();
            //globalContext = null;

            base.UnloadContent();
        }
        // {------------------------------ Monster ------------------------------------------- } //
        //#region Load All Monster
        //private void LoadMonster()
        //{
        //    var spawnPoint = _tileMaper.GetObjectLayer("SpawnPoint");
        //    foreach (var obj in spawnPoint.Objects)
        //    {
        //        if (obj.Name == "Melee")
        //        {
        //            if (obj.Type == "Light")
        //            {
        //                _monster.Add(new MonsterMelee(obj.Position, _preventMonster, _player, hitParticle, deadParticle, ElementType.Light));
        //            } 
        //            else
        //            {
        //                _monster.Add(new MonsterMelee(obj.Position, _preventMonster, _player, hitParticle, deadParticle, ElementType.Dark));
        //            }
        //            LoadMonsterMelee((MonsterMelee)_monster.Last());
        //        }
        //        else if (obj.Name == "Range")
        //        {
        //            if (obj.Type == "Light")
        //            {
        //                _monster.Add(new MonsterRange(obj.Position, _preventMonster, _player, hitParticle, deadParticle, fireParticleLight, ElementType.Light)); 
        //            }
        //            else
        //            {
        //                _monster.Add(new MonsterRange(obj.Position, _preventMonster, _player, hitParticle, deadParticle, fireParticleDark, ElementType.Dark));
        //            }
        //            LoadMonsterRange((MonsterRange)_monster.Last());
        //        }
        //        else if (obj.Name == "Boss")
        //        {
        //            _monster.Add(new MonsterBoss(obj.Position, _preventMonster, _player, hitParticle, deadParticle, fireParticleLight, ElementType.Light));
        //            LoadMonsterBoss((MonsterBoss)_monster.Last());
        //        }
        //        else if (obj.Name == "Slime")
        //        {
        //            if (obj.Type == "Light")
        //            {
        //                _monster.Add(new MonsterSlime(obj.Position, _preventMonster, _player, hitParticle, deadParticle, ElementType.Light));
        //            }
        //            else
        //            {
        //                _monster.Add(new MonsterSlime(obj.Position, _preventMonster, _player, hitParticle, deadParticle, ElementType.Dark));
        //            }
        //            LoadMonsterSlime((MonsterSlime)_monster.Last());
        //        }
        //    }
        //    //_monster.Add(new MonsterRange(new Vector2(2500, 2603), _preventMonster, _player, particle, Element.light));
        //    //_monster.Add(new MonsterMelee(new Vector2(2500, 2603), _preventMonster, _player, particle, Element.light));
        //}
        //#endregion

        //#region Load Monster Melee
        //private void LoadMonsterMelee(MonsterMelee monster)
        //{
        //    if (monster.ElementType == ElementType.Light)
        //    {
        //        monster.LoadAnim("Walk", "LightGoonWalk", monster.Position, 128, 128, Content);
        //        monster.LoadAnim("Idle", "LightGoonIdle", monster.Position, 128, 128, Content);
        //        monster.LoadAnim("Attack", "LightGoonAttack", monster.Position, 128, 128, Content);
        //        monster.LoadAnim("Charge", "LightGoonCharge", monster.Position, 128, 128, Content);
        //        monster.LoadAnim("Die", "LightGoonFuckingDie-Sheet", monster.Position, 128, 128, Content);
        //    }
        //    else if (monster.ElementType == ElementType.Dark)
        //    {
        //        monster.LoadAnim("Walk", "DarkGoonWalk", monster.Position, 128, 128, Content);
        //        monster.LoadAnim("Idle", "DarkGoonIdle", monster.Position, 128, 128, Content);
        //        monster.LoadAnim("Attack", "DarkGoonAttack", monster.Position, 128, 128, Content);
        //        monster.LoadAnim("Charge", "DarkGoonCharge", monster.Position, 128, 128, Content);
        //        monster.LoadAnim("Die", "DarkGoonFuckingDie", monster.Position, 128, 128, Content);
        //    }
        //    monster.LoadUI(Content, "HealthBar5");
        //    monster.LoadSound(Content, _audioController, "WoodHit", "WoodDie");
        //    monster.CreateAnimation();
        //    monster.SetProperty(
        //        speed: 100f,
        //        sreachRadius: 500f,
        //        hp: 250,
        //        damage: 10,
        //        attackRange: (int)(monster.Width * 1.5),
        //        activeRadius: (int)(monster.Width * 1.5),
        //        dashForce: monster.Width * 7
        //    );
        //    _ysort.Add(monster);
        //    _collision.Add(monster.HurtBox);
        //    _collision.Add(monster.Collision);
        //}
        //#endregion

        //#region Load Monster Range
        //private void LoadMonsterRange(MonsterRange monster)
        //{
        //    if (monster.ElementType == ElementType.Light)
        //    {
        //        monster.loadBullet(Content, "LightBullet");
        //        monster.LoadAnim("Walk", "LightRegimogusIdle", monster.Position, 128, 128, Content);
        //        monster.LoadAnim("Attack", "LightRegimogusAttack", monster.Position, 128, 128, Content);
        //        monster.LoadAnim("Charge", "LightRegimogusCharge", monster.Position, 128, 128, Content);
        //        monster.LoadAnim("Idle", "LightRegimogusIdle", monster.Position, 128, 128, Content);
        //        monster.LoadAnim("Die", "LightRegimogusFuckingDie", monster.Position, 128, 128, Content);
        //    }
        //    else if (monster.ElementType == ElementType.Dark)
        //    {
        //        monster.loadBullet(Content, "DarkBullet");
        //        monster.LoadAnim("Walk", "DarkRegimogusIdle", monster.Position, 128, 128, Content);
        //        monster.LoadAnim("Attack", "DarkRegimogusAttack", monster.Position, 128, 128, Content);
        //        monster.LoadAnim("Charge", "DarkRegimogusCharge", monster.Position, 128, 128, Content);
        //        monster.LoadAnim("Idle", "DarkRegimogusIdle", monster.Position, 128, 128, Content);
        //        monster.LoadAnim("Die", "DarkRegimogusFuckingDie", monster.Position, 128, 128, Content);
        //    }
        //    monster.LoadUI(Content, "HealthBar5");
        //    monster.LoadSound(Content, _audioController, "StoneHit", "StoneDie");
        //    monster.CreateAnimation();
        //    monster.SetProperty(
        //        speed: 100f,
        //        sreachRadius: 500f,
        //        hp: 150,
        //        damage: 10,
        //        attackRange: (int)(monster.Width * 2.5f),
        //        dashForce: 300,
        //        bulletSpeed: 750
        //    );
        //    _ysort.Add(monster);
        //    _collision.Add(monster.HurtBox);
        //    _collision.Add(monster.Collision);
        //}
        //#endregion

        //#region Load Monster Boss

        //private void LoadMonsterBoss(MonsterBoss monster)
        //{
        //    monster.LoadAnim("Idle", "Light-VoidDevourer-Idle", monster.Position, 320, 384, Content);
        //    monster.LoadAnim("Walk", "Light-VoidDevourer-Idle", monster.Position, 320, 384, Content);
        //    monster.LoadAnim("Attack", "LightGoonAttack", monster.Position, 128, 128, Content);
        //    monster.LoadAnim("Charge", "LightGoonCharge", monster.Position, 128, 128, Content);
        //    monster.LoadAnim("ChargeFire", "Light-VoidDevouer-HeavyMachineGun", monster.Position, 320, 384, Content);
        //    monster.LoadAnim("Fire", "Light-VoidDevouer-HeavyMachineGun", monster.Position, 320, 384, Content);
        //    monster.LoadAnim("EndFire", "Light-VoidDevouer-HeavyMachineGun", monster.Position, 320, 384, Content);
        //    monster.LoadAnim("ChargeFire3Ball", "Light-VoidDevourer-3Balls", monster.Position, 320, 384, Content);
        //    monster.LoadAnim("Fire3Ball", "Light-VoidDevourer-3Balls", monster.Position, 320, 384, Content);
        //    monster.LoadAnim("Casting", "Light-VoidDevourer-gooning", monster.Position, 320, 384, Content);
        //    monster.LoadAnim("Die", "LightGoonFuckingDie-Sheet", monster.Position, 128, 128, Content);
        //    monster.loadBullet(Content, "LightBullet", "DarkBullet");
        //    monster.LoadAssets(Content);
        //    monster.LoadUI(Content, "HealthBar7");
        //    monster.CreateAnimation();
        //    monster.SetProperty(
        //        speed: 100f,
        //        sreachRadius: 2000f,
        //        hp: 1000,
        //        damage: 10,
        //        attackRange: (int)(monster.Width * 5),
        //        activeRadius: (int)(monster.Width * 2),
        //        dashForce: monster.Width * 4,
        //        bulletSpeed: 750
        //    );
        //    _ysort.Add(monster);
        //    _collision.Add(monster.HurtBox);
        //    _collision.Add(monster.Collision);
        //}
        //#endregion

        //#region Load Monster Slime
        //private void LoadMonsterSlime(MonsterSlime monster)
        //{
        //    if (monster.ElementType == ElementType.Light)
        //    {
        //        monster.LoadAnim("Idle", "LightSlimeIdle", monster.Position, 64, 64, Content);
        //        monster.LoadAnim("Walk", "LightSlimeAttack", monster.Position, 64, 192, Content);
        //        monster.LoadAnim("Attack", "LightSlimeAttack", monster.Position, 64, 192, Content);
        //        monster.LoadAnim("Charge", "LightSlimeCharge", monster.Position, 64, 64, Content);
        //        monster.LoadAnim("Die", "LightSlimeDie", monster.Position, 64, 64, Content);
        //    }
        //    else if (monster.ElementType == ElementType.Dark)
        //    {
        //        monster.LoadAnim("Idle", "DarkSlimeIdle", monster.Position, 64, 64, Content);
        //        monster.LoadAnim("Walk", "DarkSlimeAttack", monster.Position, 64, 192, Content);
        //        monster.LoadAnim("Attack", "DarkSlimeAttack", monster.Position, 64, 192, Content);
        //        monster.LoadAnim("Charge", "DarkSlimeCharge", monster.Position, 64, 192, Content);
        //        monster.LoadAnim("Die", "DarkSlimeDie", monster.Position, 64, 64, Content);
        //    }
        //    monster.LoadUI(Content, "HealthBar5");
        //    monster.LoadSound(Content, _audioController, "SlimeHit", "SlimeDie");
        //    monster.CreateAnimation();
        //    monster.SetProperty(
        //        speed: 100f,
        //        sreachRadius: 500f,
        //        hp: 250,
        //        damage: 10,
        //        attackRange: (int)(monster.Width * 1.5),
        //        activeRadius: (int)(monster.Width * 1.5),
        //        dashForce: monster.Width * 7
        //    );
        //    _ysort.Add(monster);
        //    _collision.Add(monster.HurtBox);
        //    _collision.Add(monster.Collision);
        //}
        //#endregion

        //#region Update Monster
        //private void UpdateMonster(GameTime gameTime)
        //{
        //    // Loop through all monsters
        //    foreach (var monster in _monster.ToList())
        //    {
        //        switch (monster)
        //        {
        //            case MonsterMelee m:
        //                m.UpdateState(gameTime, _collision, _collisionComponent, _player._movement.Position);
        //                HandleShakeCamera(m, gameTime);
        //                if (m.IsDead)
        //                    HandleMonsterDeath(m);
        //                break;

        //            case MonsterRange r:
        //                r.UpdateState(gameTime, _collision, _collisionComponent, _player._movement.Position);
        //                HandleShakeCamera(r, gameTime);
        //                if (r.IsDead)
        //                    HandleMonsterDeath(r);
        //                break;

        //            case MonsterBoss b:
        //                b.UpdateState(gameTime, _collision, _collisionComponent, _player._movement.Position);
        //                HandleShakeCamera(b, gameTime);
        //                if (b.IsDead)
        //                    HandleMonsterDeath(b);
        //                break;

        //            case MonsterSlime s:
        //                s.UpdateState(gameTime, _collision, _collisionComponent, _player._movement.Position);
        //                HandleShakeCamera(s, gameTime);
        //                if (s.IsDead)
        //                    HandleMonsterDeath(s);
        //                break;
        //        }

        //        // Remove dead monsters
        //        foreach (var deadMonster in _pendingMonsterRemove)
        //            _monster.Remove(deadMonster);
        //        _pendingMonsterRemove.Clear();
        //    }
        //}
        //#endregion

        //#region Monster Extra
        //// Shake camera helper
        //private void HandleShakeCamera(dynamic monster, GameTime gameTime)
        //{
        //    if (monster.ShakeViewport)
        //    {
        //        camera.ShakeCamera(gameTime);
        //        monster.ShakeViewport = camera.ShakeViewport;
        //    }
        //}

        //// Handle monster death and spawn heal pickup
        //private void HandleMonsterDeath(dynamic monster)
        //{
        //    // Spawn heal pickup via DropManager
        //    var healPickup = DropManager.DropHeal(
        //        _healTexture,
        //        _player,
        //        _collisionComponent,
        //        monster.Position,
        //        _pendingRemove
        //    );
        //    _pendingAdd.Add(healPickup);
        //    _ysort.Add(healPickup);

        //    // Clean up monster
        //    if (monster is MonsterRange)
        //        monster.DeleteHitBox(1f); // Only 1 parameter
        //    else
        //        monster.DeleteHitBox(1f, _collision, _collisionComponent); // 3 parameters

        //    monster.RemoveMonster();
        //    _pendingMonsterRemove.Add(monster);
        //}

        ///// <summary>
        ///// Helper function to spawn a heal pickup
        ///// </summary>
        //private void SpawnHeal(Vector2 position)
        //{
        //    var healPickup = DropManager.DropHeal(_healTexture, _player, _collisionComponent, position, _pendingRemove);
        //    _pendingAdd.Add(healPickup);
        //    Debug.WriteLine("Spawned HealPickup at: " + position);
        //}
        //#endregion
    }
}
