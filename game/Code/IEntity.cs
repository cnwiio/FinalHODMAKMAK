using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Collisions;

namespace game
{
    public interface IEntity : ICollisionActor
    {
        //public void Update(GameTime gameTime);
        public void Draw(SpriteBatch spriteBatch);
    }
}
