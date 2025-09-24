using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace game
{
    public interface IParticle
    {
        void Update(float deltaTime);
        void Draw(SpriteBatch spriteBatch);
    }
}
