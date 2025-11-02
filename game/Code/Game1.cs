using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
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
        #region Player Sound
        public AudioController audioController;
        private string attackSound = "PlayerAttack";
        private string hurtSound = "PlayerHurt";
        private string skill1Sound = "Skill1";
        private string skill2Sound = "Skill2";
        private string potionSound = "SipPotion";
        private string dashSound = "Dash";
        private string eleSound = "ChangeElement";
        #endregion

        #region Monster Sound
        public SoundEffect monhurtSound;
        public SoundEffect deadSound;
        public SoundEffect parrySound;
        public SoundEffect fireSound;
        public SoundEffect spikeSound;
        public Song bossbgmSound;
        public Song BGMSound;
        #endregion

        public GlobalCamera camera;
        public short MapWidth, MapHeight;
        public short ScreenWidth = 1920, ScreenHeight = 1080;

        // Collision 
        public CollisionComponent CollisionComponent { get; set; }
        public List<IEntity> Collision { get; set; } = new List<IEntity>();

        // Player and monsters
        private AnimController _playerTexture;
        public Player Player;
        public short SavedHP;// ใช้ในCheckPoint 
        public short SavedPotion { get; set; } // ใช้ในCheckPoint    
        public PreventMonster PreventMonster;
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
            var viewportAdapter = new BoxingViewportAdapter(Window, GraphicsDevice, 1280, 720);
            camera = new GlobalCamera(viewportAdapter);

            Window.Title = "The Lost Witch";

            _graphics.PreferredBackBufferWidth = ScreenWidth;
            _graphics.PreferredBackBufferHeight = ScreenHeight;
            _graphics.IsFullScreen = true;
            _graphics.ApplyChanges();
                
            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Audio
            monhurtSound = Content.Load<SoundEffect>("Audio/MonHurt");
            deadSound = Content.Load<SoundEffect>("Audio/MonDead");
            parrySound = Content.Load<SoundEffect>("Audio/Parry");
            fireSound = Content.Load<SoundEffect>("Audio/MonShoot");
            spikeSound = Content.Load<SoundEffect>("Audio/BossSpike");
            bossbgmSound = Content.Load<Song>("Audio/BgmBoss");
            BGMSound = Content.Load<Song>("Audio/BgmNormal");


            // Player
            _playerTexture = new AnimController(new Vector2(400, 400));
            _playerTexture.LoadFrame(Content, "Walk", "Player_Walk", 288, 192);

            // Walk Animations
            _playerTexture.CreateAnimation("Walk", "down", true, 98, 0, 8);
            _playerTexture.CreateAnimation("Walk", "right", true, 98, 8, 8);
            _playerTexture.CreateAnimation("Walk", "left", true, 98, 16, 8);
            _playerTexture.CreateAnimation("Walk", "up", true, 98, 24, 8);

            // Idle animation (4 directions, 6 frames per row)
            _playerTexture.LoadFrame(Content, "Idle", "Player_Idle", 288, 192);

            _playerTexture.CreateAnimation("Idle", "down", true, 200, 0, 6);   // row 0
            _playerTexture.CreateAnimation("Idle", "right", true, 200, 6, 6);  // row 1
            _playerTexture.CreateAnimation("Idle", "left", true, 200, 12, 6);  // row 2
            _playerTexture.CreateAnimation("Idle", "up", true, 200, 18, 6);    // row 3

            // Attack animation (4 directions, 6 frames per row)
            _playerTexture.LoadFrame(Content, "AttackLight", "Player_Attadck_Light", 288, 192);
            _playerTexture.LoadFrame(Content, "AttackDark", "Player_Attadck_Dark", 288, 192);

            _playerTexture.CreateAnimation("AttackLight", "down", false, 25, 0, 8);   // row 0
            _playerTexture.CreateAnimation("AttackLight", "right", false, 25, 8, 8);  // row 1
            _playerTexture.CreateAnimation("AttackLight", "left", false, 25, 16, 8);  // row 2
            _playerTexture.CreateAnimation("AttackLight", "up", false, 25, 24, 8);    // row 3

            _playerTexture.CreateAnimation("AttackDark", "down", false, 25, 0, 8);   // row 0
            _playerTexture.CreateAnimation("AttackDark", "right", false, 25, 8, 8);  // row 1
            _playerTexture.CreateAnimation("AttackDark", "left", false, 25, 16, 8);  // row 2
            _playerTexture.CreateAnimation("AttackDark", "up", false, 25, 24, 8);    // row 3

            _playerTexture.LoadFrame(Content, "DashLight", "Dash_final_Light", 288, 192);
            _playerTexture.LoadFrame(Content, "DashDark", "Dash_final_Dark", 288, 192);

            _playerTexture.CreateAnimation("DashLight", "down", false, 90, 0, 2);   // row 0
            _playerTexture.CreateAnimation("DashLight", "right", false, 90, 2, 2); // row 1
            _playerTexture.CreateAnimation("DashLight", "left", false, 90, 4, 2); // row 2
            _playerTexture.CreateAnimation("DashLight", "up", false, 90, 6, 2); // row 3
            
            _playerTexture.CreateAnimation("DashDark", "down", false, 90, 0, 2);   // row 0
            _playerTexture.CreateAnimation("DashDark", "right", false, 90, 2, 2); // row 1
            _playerTexture.CreateAnimation("DashDark", "left", false, 90, 4, 2); // row 2
            _playerTexture.CreateAnimation("DashDark", "up", false, 90, 6, 2); // row 3

            _playerTexture.LoadFrame(Content, "ElementalShift", "EleChange_Final", 288, 192);
            _playerTexture.CreateAnimation("ElementalShift", "ElementalShiftLight", false, 50, 0, 8); // row 0
            _playerTexture.CreateAnimation("ElementalShift", "ElementalShiftDark", false, 50, 8, 8); // row 1 (assuming it's row 1)

            // Faint animation
            _playerTexture.LoadFrame(Content, "Faint", "Faint_Final", 288, 192); // frame width/height
            _playerTexture.CreateAnimation("Faint", "no", false, 170, 0, 10); // 10 frames, 170ms each


            var skillTexture = new AnimController(Vector2.Zero);
            skillTexture.LoadFrame(Content, "Light", "BoneOfMySword", 192, 384);
            skillTexture.LoadFrame(Content, "Dark", "BoneOfMySwordButBlack", 192, 384);
            skillTexture.LoadFrame(Content, "idle", "BoneOfMySword", 192, 384);
            skillTexture.CreateAnimation("Light", "Active", false, 30, 0, 36);
            skillTexture.CreateAnimation("Dark", "Active", false, 30, 0, 36);
            skillTexture.CreateAnimation("idle", "no", true, 30, 0, 36);


            Player = new Player(_playerTexture, new Vector2(0));
            var skill2Texture = Content.Load<Texture2D>("Texture/Skill2");

            Player.SetWorldReferences(Collision, CollisionComponent);
            Player.LoadSound(Content, audioController, attackSound, hurtSound, skill1Sound, skill2Sound, potionSound, dashSound, eleSound);
            Player.LoadSkill(skillTexture, skill2Texture);
            PreventMonster = new PreventMonster(new Vector2(400, 400), 350f);

            SavedHP = (short)Player.Stats.CurrentHP; // checkpoint
            SavedPotion = Player.potion.Amout; // checkpoint

            screenManager.LoadScreen(new SceneMenu(this));
            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            //if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            //    Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }

        protected override void UnloadContent()
        {
            _playerTexture = null;
            base.UnloadContent();
        }
    }
}
