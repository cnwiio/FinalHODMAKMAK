
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace game1
{
    public class Player
    {
        private PlayerStats _stats;
        private PlayerInput _input;
        public PlayerMovement _movement;
        private PlayerAnimation _animation;

        public PlayerStats Stats => _stats;


        public Player(AnimController texture, Vector2 startPosition)
        {
            _stats = new PlayerStats();
            _input = new PlayerInput();
            _movement = new PlayerMovement(startPosition, _stats);
            _animation = new PlayerAnimation(texture);
        }

        public void Update(GameTime gameTime)
        {
            _input.Update(gameTime);
            _movement.Update(gameTime, _input.Direction, _input.DashTriggered);
            _animation.Update(gameTime, _movement.Direction, _movement.Position - new Vector2(_animation._animController.TextureWidth /2 , _animation._animController.TextureHeight/2));
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            _animation.Draw(spriteBatch);
        }
    }
}
