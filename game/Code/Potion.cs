using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using MonoGame.Extended;

namespace game
{
    public class Potion
    {
        #region Property
        public short Amout = 1;
        public const short MAXAMOUT = 5;
        public short HealPower = 40;
        public const short COOLDOWN = 3;
        #endregion
        #region Calculater Value
        private float timer = 0;
        public bool isinCoolDown => timer > 0;
        private Player player;
        #endregion
        public Potion(Player player)
        {
            this.player = player;
        }

        public void Add()
        {
            if (Amout < MAXAMOUT) 
                Amout++;
        }
        public void Use(AudioController audio, SoundEffect soundEffect)
        {
            if (Amout > 0 && !isinCoolDown)
            {
                Amout--;
                timer = COOLDOWN;
                player.Stats.Heal(HealPower);
                audio.PlaySoundEffect(soundEffect);
                Debug.WriteLine("Healed! HP: " + player.Stats.CurrentHP);
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
