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


namespace game
{
    /*
    How to use Example
    1.
            private AnimController _player;
    2.
            _player = new AnimController(textureName, position, width, height);
            _player.LoadFrame(Content);
            _player.CreateAnimation("idle", true, 0.8f, 0, 3);
            _player.CreateAnimation("fart", false, 0.1f, 0, 3);
    3.
            _player.UpdateFrame(gameTime, _playerPos);
    4.
            _player.DrawFrame(_spriteBatch);
    5.
            _player.ChangeAnim("fart", "idle");
            _player.ChangeAnim("fart", "idle", OnFartAnimationComplete);
     */
    public class AnimController
    {
        public Texture2D SourceTexture { get; set; }
        public Texture2DAtlas Atlas { get; set; }
        public SpriteSheet SpriteSheet { get; set; }
        public AnimatedSprite AnimSprite { get; set; }
        public string TextureName { get; set; }
        public int TextureWidth { get; set; }
        public int TextureHeight { get; set; }
        public Vector2 Position { get; set; }
        public AnimController(string textureName, Vector2 position, int textureWidth, int textureHeight)
        {
            TextureName = textureName;
            Position = position;
            TextureWidth = textureWidth;
            TextureHeight = textureHeight;
        }

        public void LoadFrame(ContentManager content)
        {
            SourceTexture = content.Load<Texture2D>("Texture/" + TextureName);
            Atlas = Texture2DAtlas.Create("Atlas/" + TextureName, SourceTexture, TextureWidth, TextureHeight);
            SpriteSheet = new SpriteSheet("SpriteSheet/" + TextureName, Atlas);
        }
        public void UpdateFrame(GameTime gameTime)
        {
            AnimSprite.Update(gameTime);
        }
        public void UpdateFrame(GameTime gameTime, Vector2 position)
        {
            AnimSprite.Update(gameTime);
            Position = position;
        }
        public void DrawFrame(SpriteBatch spriteBatch)
        {
            //spriteBatch.Draw(AnimSprite, Position);
            AnimSprite.Draw(spriteBatch, Position, 0f, Vector2.One);
        }
        public void DrawFrame(SpriteBatch spriteBatch, Color tintColor = default)
        {
            if (tintColor == default) tintColor = Color.White;
            AnimSprite.Color = tintColor;
            DrawFrame(spriteBatch);
        }
        //public void DrawFrame(SpriteBatch spriteBatch, Color tintColor = default,  SpriteEffects effect)
        //{
        //    if (tintColor == default) tintColor = Color.White;
        //    AnimSprite.Effect = effect;
        //    DrawFrame(spriteBatch, tintColor);
        //}
        public void SetDefaultAnim(string animationName)
        {
            AnimSprite = new AnimatedSprite(SpriteSheet, animationName);
        }
        public void CreateAnimation(string animationName, bool isLoop, int fps, int startIndexFrames, int endIndexFrames)
        {

            // sprite sheet
            float _fps = 1f / fps;
            SpriteSheet.DefineAnimation(animationName, builder =>
            {
                builder.IsLooping(isLoop);
                for (int i = startIndexFrames; i <= endIndexFrames; i++)
                {
                    builder.AddFrame(i, TimeSpan.FromSeconds(_fps));
                }
            });

            // animated sprite
            if (AnimSprite == null)
            {
                this.SetDefaultAnim(animationName);
            }
        }

        public void ChangeAnim(string animationName)
        {
            if (AnimSprite.CurrentAnimation != animationName)
            {
                AnimSprite.SetAnimation(animationName);  
            }
        }
        public void ChangeAnim(string NewAnimation, string ChangeBackAnimation)
        {
            AnimSprite.SetAnimation(NewAnimation).OnAnimationEvent += (sender, trigger) =>
            {
                if (trigger == AnimationEventTrigger.AnimationCompleted)
                {
                    AnimSprite.SetAnimation(ChangeBackAnimation);
                }
            };
        }
        // Delegate for methods that take no parameters and return void
        public delegate void AnimationCallback();

        public void ChangeAnim(string NewAnimation, string ChangeBackAnimation, AnimationCallback methodName)
        {
            AnimSprite.SetAnimation(NewAnimation).OnAnimationEvent += (sender, trigger) =>
            {
                if (trigger == AnimationEventTrigger.AnimationCompleted)
                {
                    AnimSprite.SetAnimation(ChangeBackAnimation);
                    methodName?.Invoke(); // Call the callback method if it's not null
                }
            };
        }
    }
}
