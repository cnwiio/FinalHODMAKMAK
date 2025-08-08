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


namespace game
{
    /*
    How to use Example
    1.
            private AnimController _player;
    2.
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
        public Texture2D _sourceTexture { get; set; }
        public Texture2DAtlas _atlas { get; set; }
        public SpriteSheet _spriteSheet {  get; set; }
        public AnimatedSprite _animSprite {  get; set; }
        public string _textureName { get; set; }
        public int _textureWidth { get; set; }
        public int _textureHeight {  get; set; }
        public Vector2 _position { get; set; }
        public AnimController(string textureName, Vector2 position, int textureWidth, int textureHeight)
        {
            _textureName = textureName;
            _position = position;   
            _textureWidth = textureWidth;
            _textureHeight = textureHeight;
        }

        public void LoadFrame(ContentManager content)
        {
            _sourceTexture = content.Load<Texture2D>(_textureName);
            _atlas = Texture2DAtlas.Create("Atlas/" + _textureName, _sourceTexture, _textureWidth, _textureHeight);
            _spriteSheet = new SpriteSheet("SpriteSheet/" + _textureName, _atlas);
        }
        public void UpdateFrame(GameTime gameTime)
        {
            _animSprite.Update(gameTime);
        }
        public void UpdateFrame(GameTime gameTime, Vector2 position)
        {
            _animSprite.Update(gameTime);
            _position = position;
        }
        public void DrawFrame(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_animSprite, _position);
        }
        public void SetDefaultAnim(string animationName)
        {
            _animSprite = new AnimatedSprite(_spriteSheet, animationName);
        }
        public void CreateAnimation(string animationName, bool isLoop, float intervalBetweenFrame, int startIndexFrames, int endIndexFrames)
        {

            // sprite sheet
            _spriteSheet.DefineAnimation(animationName, builder =>
            {
                builder.IsLooping(isLoop);
                for (int i = startIndexFrames; i <= endIndexFrames; i++)
                {
                    builder.AddFrame(i, TimeSpan.FromSeconds(intervalBetweenFrame)); 
                }
            });

            // animated sprite
            if (_animSprite == null)
            {
                this.SetDefaultAnim(animationName);
            }
        }
        
        public void ChangeAnim(string animationName)
        {
            _animSprite.SetAnimation(animationName);
        }
        public void ChangeAnim(string NewAnimation, string ChangeBackAnimation)
        {
            _animSprite.SetAnimation(NewAnimation).OnAnimationEvent += (sender, trigger) =>
            {
                if (trigger == AnimationEventTrigger.AnimationCompleted)
                {
                    _animSprite.SetAnimation(ChangeBackAnimation);
                }
            };
        }
        // Delegate for methods that take no parameters and return void
        public delegate void AnimationCallback();
        
        public void ChangeAnim(string NewAnimation, string ChangeBackAnimation, AnimationCallback methodName)
        {
            _animSprite.SetAnimation(NewAnimation).OnAnimationEvent += (sender, trigger) =>
            {
                if (trigger == AnimationEventTrigger.AnimationCompleted)
                {
                    _animSprite.SetAnimation(ChangeBackAnimation);
                    methodName?.Invoke(); // Call the callback method if it's not null
                }
            };
        }
    }
}
