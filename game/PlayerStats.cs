using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game1
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


    }
}
