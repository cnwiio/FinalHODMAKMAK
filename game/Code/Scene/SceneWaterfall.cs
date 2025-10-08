using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Screens;

namespace game
{
    public class SceneWaterfall : GameScreen
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
        private SpriteFont spriteFont;

        // HUD
        private HealthBarHUD healthBar;
        private ElementHUD _elementHUD;
        private PotionHUD _potionHUD;
        private SkillHUD _skillHUD;

        public SceneWaterfall(Game game) : base(game)
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
            _healTexture = Content.Load<Texture2D>("Texture/Potion");
            spriteFont = Content.Load<SpriteFont>("Fonts/Pixeltype");

            globalContext.LoadAll(Content, "SceneWaterfall", preventMonster, player);

            // HUD
            healthBar = new HealthBarHUD(game1.Player.Stats, GraphicsDevice);
            healthBar.LoadContent(Content);

            _potionHUD = new PotionHUD(player, GraphicsDevice);
            _potionHUD.LoadContent(Content);


            _elementHUD = new ElementHUD(player, GraphicsDevice);
            _elementHUD.LoadContent(Content);

            _skillHUD = new SkillHUD(player, GraphicsDevice);
            _skillHUD.LoadContent(Content);

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
            if (_ks.IsKeyDown(Keys.M) && !_oldKs.IsKeyDown(Keys.M))
            {
                globalContext.audioController.ToggleMute();
            }
            if (player.Stats.CurrentHP == 0)
            {
                ScreenManager.LoadScreen(new SceneDead(game1));
                return;
            }
            #endregion

            player.Update(gameTime, globalContext._Camera);
            var playerpos = player._movement.Position;
            preventMonster.UpdatePosition(playerpos);

            globalContext.UpdateCamera(playerpos - new Vector2(globalContext.Camera.cameraWidth / 2, globalContext.Camera.cameraHeight / 2));
            globalContext.UpdateParticle(gameTime);
            globalContext.UpdateMonster(gameTime, player, _healTexture); // รอ player
            globalContext.UpdateChest(playerpos);
            globalContext.UpdatePendinQueue();
            globalContext.UpdateTiledMaper(gameTime);
            globalContext.UpdateYsort();

            // HUD
            healthBar.Update();
            _elementHUD.Update(gameTime);
            _potionHUD.Update(gameTime);

            // Collision
            _collisionComponent.Update(gameTime);
        }
        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                transformMatrix: globalContext._Camera.GetViewMatrix()
            );

            globalContext.DrawAll(_spriteBatch);

            if (isDebug)
                DebugDraw();

            _spriteBatch.End();

            // UI sprite batch
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, samplerState: SamplerState.PointClamp);
            globalContext.DrawPotionUI(_spriteBatch, player, _healTexture, spriteFont);

            healthBar.Draw(_spriteBatch);
            _elementHUD.Draw(_spriteBatch);
            _potionHUD.Draw(_spriteBatch);
            _skillHUD.Draw(_spriteBatch);

            _spriteBatch.End();
        }
        public override void UnloadContent()
        {
            _healTexture = null;
            globalContext.UnloadAll();
            spriteFont = null;
            globalContext = null;
            base.UnloadContent();
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
            //_spriteBatch.DrawCircle(new CircleF(monster.SpawnPosition, monster.AwaySpawnRadius), 16, Color.DarkViolet, 2);
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
    }
}
