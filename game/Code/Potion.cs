using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace game
{
    public class Potion
    {
        public short Amout = 0;
        public const short MAXAMOUT = 3;
        public short HealPower = 10;
        public const short COOLDOWN = 3;
        public float timer = 0;
        public bool isinCoolDown => timer > 0;
        private Player player;
        public Potion(Player player)
        {
            this.player = player;
        }

        public void Add()
        {
            if (Amout < MAXAMOUT) 
                Amout++;
        }
        public void Use()
        {
            if (Amout > 0 && !isinCoolDown)
            {
                Amout--;
                timer = COOLDOWN;
                player.Stats.Heal(HealPower);
            }
        }

        public void Update(GameTime gameTime)
        {
            float dt = gameTime.GetElapsedSeconds();
            if(isinCoolDown)
            {
                timer -= dt;
                if (!isinCoolDown)
                {
                    timer = 0;
                }
            }
        }
    }
}
