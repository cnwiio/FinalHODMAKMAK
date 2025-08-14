
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game
{
    public class PlayerAnimation
    {
        public AnimController _animController;
        private AnimatedTexture _textureChar;
        private int _row = 1;
        private int overLoad;

        public PlayerAnimation(AnimController texture)
        {
            _animController = texture;
            overLoad = 1;
        }
        public PlayerAnimation(AnimatedTexture texture)
        {
            _textureChar = texture;
            overLoad = 2;
        }

        public void Update(GameTime gameTime, Vector2 direction, Vector2 position)
        {
            if (overLoad == 1)
            {
                if (direction.Y < 0) _animController.ChangeAnim("up"); // Up
                else if (direction.Y > 0) _animController.ChangeAnim("down"); // Down
                else if (direction.X < 0) _animController.ChangeAnim("left"); // Left
                else if (direction.X > 0) _animController.ChangeAnim("right"); // Right

                if (direction != Vector2.Zero)
                {
                    _animController.UpdateFrame(gameTime, position );
                } 
            }
            else if (overLoad == 2)
            {
                if (direction.Y < 0) _row = 4; // Up
                else if (direction.Y > 0) _row = 1; // Down
                else if (direction.X < 0) _row = 2; // Left
                else if (direction.X > 0) _row = 3; // Right

                if (direction != Vector2.Zero)
                {
                    _textureChar.UpdateFrame((float)gameTime.ElapsedGameTime.TotalSeconds);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _animController.DrawFrame(spriteBatch);
        }
        public void Draw(SpriteBatch spriteBatch, Vector2 position)
        {
            _textureChar.DrawFrame(spriteBatch, position, _row);
        }
    }
}
