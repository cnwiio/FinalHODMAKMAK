using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using MonoGame.Extended;

namespace game
{
    public class Telegraph
    {
        public float Scale;
        public float Alpha;
        public Vector2 Position { get; set; }
        public Texture2D Texture;
        public AnimController Texture2;
        public float LifeTime = 0;
        public bool isVisible => LifeTime > 0f;
        public bool isStart;
        public bool isFinish;

        public bool isSkillVisible;
        public float SkillLifeTime;
        
        public Telegraph(Texture2D texture, AnimController texture2)
        {
            Texture = texture;
            Texture2 = texture2;
        }

        public void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.GetElapsedSeconds();
            Texture2.UpdateFrame(gameTime);
            if (LifeTime > 0f)
            {
                LifeTime -= dt;
                isStart = true;
                isFinish = false;
                if (LifeTime < 0f)
                {
                    LifeTime = 0;
                    if (isStart)
                    {
                        isFinish = true;
                        isStart = false;
                        SkillLifeTime = 1f;
                    }
                }
            }

            if (SkillLifeTime > 0f)
            {
                isSkillVisible = true;
                SkillLifeTime -= dt;
            }
            else
            {
                SkillLifeTime = 0;
                isSkillVisible = false;
            }
            if (isFinish)
            {
                Texture2.SetAnimation("Attack", "yes");
            }
            if (isVisible)
            {
                Texture2.SetAnimation("Idle", "no");

            }

        }
        private float _scale;
        private float _alpha;
        private float _lifeTime;
        public void Draw(SpriteBatch spriteBatch)
        {
            var rect = Texture.Bounds;
            var origin = new Vector2(rect.Width / 2, rect.Height / 2);
            if (isVisible)
            {
                var alpha = Alpha + (1 - Alpha) * (1 - (LifeTime / _lifeTime));
                var tint = alpha >= 0.9 ? Color.White : Color.Red;
                spriteBatch.Draw(Texture, Position, null, tint * alpha, 0, origin, Vector2.One * Scale, SpriteEffects.None, 0); 
            }
            else if (isSkillVisible)
            {
                //var rect2 = Texture2.Bounds;
                //var origin2 = new Vector2(rect2.Width / 2, rect2.Height / 2);
                //spriteBatch.Draw(Texture2, Position, null, Color.White, 0, origin2, Vector2.One * 2, SpriteEffects.None, 0);
                Texture2.DrawFrame(spriteBatch);
            }

        }

        public void Create(float scale, float alpha, Vector2 position, float lifeTime)
        {
            Scale = scale;
            Alpha = alpha;
            Position = position;
            Texture2.Position = position;
            LifeTime = lifeTime;
            _lifeTime = lifeTime;
        }

        public bool isFinished()
        {
            if (isFinish)
            {
                isFinish = false;
                return true;
            }
            return isFinish;
        }
    }
}
