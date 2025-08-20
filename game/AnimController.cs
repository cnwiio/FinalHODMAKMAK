using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Animations;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Input.InputListeners;
using MonoGame.Extended.Particles;


namespace game1
{
    /*
    How to use Example
    1.
            private AnimController _player;
    2.
            _player = new AnimController(_position);
            _player.LoadFrame(Content, "Char", "Char01", 32, 48);
            _player.LoadFrame(Content, "attackDown", "idletestnohat", 48, 53);
            _player.CreateAnimation("Char", "Attack/Down", false, 2, 0, 4);
            _player.CreateAnimation("Char", "Walk/Left", true, 12, 4, 4);
    3.
            _player.UpdateFrame(gameTime, _playerPos);
    4.
            _player.DrawFrame(_spriteBatch);
    5.
            _player.SetAnimation("spriteSheet", "animationName");
            _player.SetAnimation("attackDown", "Attack/Down", OnAnimationEvent);
    6.
            _player.Unload(OnAnimationEvent);
    */
    public class AnimController
    {
        public Texture2D SourceTexture { get; set; }
        public Dictionary<string,Texture2DAtlas> Atlas { get; set; } = new Dictionary<string, Texture2DAtlas>();
        public Dictionary<string,SpriteSheet> SpriteSheet { get; set; } = new Dictionary<string, SpriteSheet>();
        public Dictionary<string, AnimatedSprite> AnimSprite { get; set; } = new Dictionary<string, AnimatedSprite>(); 
        public Vector2 Position { get; set; }
        public Vector2 Origin { get; set; }
        public float TextureWidth { get; set; }
        public float TextureHeight { get; set; }
        public string CurrentSpriteSheet { get; set; }
        public string CurrentAnimation { get; set; }
        public AnimController(Vector2 position)
        {
            Position = position;
        }

        // Ex. _player.LoadFrame(Content, "Char", "Char01", 32, 48);
        public void LoadFrame(ContentManager content, string spriteSheetName, string textureName, int textureWidth, int textureHeight)
        {
            SourceTexture = content.Load<Texture2D>("Texture/" + textureName);
            Atlas.Add(spriteSheetName, Texture2DAtlas.Create("Atlas/" + textureName, SourceTexture, textureWidth, textureHeight));
            SpriteSheet.Add(spriteSheetName, new SpriteSheet("SpriteSheet/" + textureName, Atlas[spriteSheetName]));
        }


        // Ex. _player.UpdateFrame(gameTime);
        public void UpdateFrame(GameTime gameTime)
        {
            AnimSprite[CurrentSpriteSheet].Update(gameTime);
        }


        // Ex. _player.UpdateFrame(gameTime, _playerPos);
        public void UpdateFrame(GameTime gameTime, Vector2 position)
        {
            Position = position; 
            AnimSprite[CurrentSpriteSheet].Update(gameTime);
            Origin = AnimSprite[CurrentSpriteSheet].Origin;
            TextureWidth = AnimSprite[CurrentSpriteSheet].TextureRegion.Width;
            TextureHeight = AnimSprite[CurrentSpriteSheet].TextureRegion.Height;
        }


        // Ex. _player.DrawFrame(_spriteBatch);
        public void DrawFrame(SpriteBatch spriteBatch)
        {
            AnimSprite[CurrentSpriteSheet].Draw(spriteBatch, Position, 0f, Vector2.One);
        }


        // Ex. _player.DrawFrame(_spriteBatch, Color.Red);
        public void DrawFrame(SpriteBatch spriteBatch, Color tintColor = default)
        {
            if (tintColor == default) tintColor = Color.White;
            AnimSprite[CurrentSpriteSheet].Color = tintColor;
            DrawFrame(spriteBatch);
        }


        // Ex.  _player.CreateAnimation("Char", "Attack/Down", false, 2, 0, 4);
        //      _player.CreateAnimation("Char", "Walk/Left", true, 12, 4, 4);
        public void CreateAnimation(string spriteSheetName,string animationName, bool isLoop, int fps, int startIndexFrames, int framesCount)
        {

            float _fps = 1f / fps;
            SpriteSheet[spriteSheetName].DefineAnimation(animationName, builder =>
            {
                builder.IsLooping(isLoop);
                for (int i = startIndexFrames; i <= startIndexFrames + framesCount - 1; i++)
                {
                    builder.AddFrame(i, TimeSpan.FromSeconds(_fps));
                }
            });

            if (CurrentSpriteSheet == null)
            {
                CurrentSpriteSheet = spriteSheetName;
                CurrentAnimation = animationName;
            }

            if (!AnimSprite.ContainsKey(spriteSheetName))
            {
                AnimSprite.Add(spriteSheetName, new AnimatedSprite(SpriteSheet[spriteSheetName], animationName));
                AnimSprite[spriteSheetName].Origin = new Vector2(
                    AnimSprite[spriteSheetName].TextureRegion.Width / 2f,
                    AnimSprite[spriteSheetName].TextureRegion.Height / 2f
                    ); // Centerize; not sure if change in future 
            }

        }


        // Ex.  _player.SetAnimation("fart", "idle", OnAnimationEvent);
        public void SetAnimation(string spriteSheet, string animationName, Action<IAnimationController, AnimationEventTrigger> onEvent = null)
        {
            if (CurrentSpriteSheet == spriteSheet && CurrentAnimation == animationName)
                return;

            CurrentSpriteSheet = spriteSheet;
            CurrentAnimation = animationName;

            var sprite = AnimSprite[CurrentSpriteSheet];
            var controller = sprite.SetAnimation(animationName);

            if (AnimSprite[CurrentSpriteSheet].Controller != null)
                AnimSprite[CurrentSpriteSheet].Controller.OnAnimationEvent -= onEvent;

            if (onEvent != null)
                controller.OnAnimationEvent += (s, e) => onEvent(s, e);
        }


        // Ex. _player.Unload(OnAnimationEvent);
        public void Unload(Action<IAnimationController, AnimationEventTrigger> onEvent)
        {
            foreach (var item in AnimSprite.Values)
            {
                if (item.Controller != null)
                    item.Controller.OnAnimationEvent -= onEvent;
            }
        }
    }
}



// Old Method

//public void ChangeAnim(string spriteSheetName,string animationName)
//{
//    CurrentSpriteSheet = spriteSheetName;
//    //CurrentAnimation = animationName;
//    if (AnimSprite[CurrentSpriteSheet].CurrentAnimation != animationName)
//    {
//        AnimSprite[CurrentSpriteSheet].SetAnimation(animationName);  
//    }
//}
//public void DrawFrame(SpriteBatch spriteBatch, Color tintColor = default,  SpriteEffects effect)
//{
//    if (tintColor == default) tintColor = Color.White;
//    AnimSprite.Effect = effect;
//    DrawFrame(spriteBatch, tintColor);
//}