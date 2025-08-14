using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game1
{
    public class Stat
    {
        public int BaseValue { get; set; }
        private readonly List<int> _modifiers = new List<int>();
        public int Value => Math.Max(0, BaseValue + _modifiers.Sum());

        public void AddModifier(int amount) => _modifiers.Add(amount);
        public void RemoveModifier(int amount) => _modifiers.Remove(amount);
    }
}
