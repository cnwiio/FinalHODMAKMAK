using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace game
{
    public class PlayerStats
    {
        public Stat HP { get; } = new Stat() { BaseValue = 100 };
        public Stat MP { get; } = new Stat() { BaseValue = 100 };
        public Stat AttackDamage { get; } = new Stat() { BaseValue = 50 };
        public Stat AttackPower { get; } = new Stat() { BaseValue = 50 };
        public Stat Defense { get; } = new Stat() { BaseValue = 10 };
        public Stat MagicResistance { get; } = new Stat() { BaseValue = 5 };
        public Stat Speed { get; } = new Stat() { BaseValue = 200 };
        public Stat AttackSpeed { get; } = new Stat() { BaseValue = 1 };

        // EXP & Level
        public int Level { get; private set; } = 1;
        public int CurrentExp { get; private set; } = 0;
        public int ExpToNextLevel { get; private set; } = 100;

        public void GainExp(int amount)
        {
            CurrentExp += amount;
            while (CurrentExp >= ExpToNextLevel)
            {
                LevelUp();
            }
        }

        private void LevelUp()
        {
            CurrentExp -= ExpToNextLevel;
            Level++;
            ExpToNextLevel = (int)(ExpToNextLevel * 1.5f); // increase exp required per level
                                                           // Optionally increase stats on level up
            HP.BaseValue += 10;
            AttackDamage.BaseValue += 5;
            Speed.BaseValue += 10;
            // You can also trigger an event or callback here
        }

        /* 
        About LevelUp mechanics for example if player kills an enemy,
        you can call GainExp with the amount of EXP the enemy gives by
        player.Stats.GainExp(50);
        this will increase the player's current experience points by 50.
        */
    }
}
