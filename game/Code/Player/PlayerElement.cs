using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game
{
    public enum ElementType
    {
        Light,
        Dark
    }

    public partial class Player
    {
        public ElementType CurrentElement { get; private set; } = ElementType.Light;

        // Call this to toggle element manually (ex: on key press)
        public void ToggleElement()
        {
            CurrentElement = CurrentElement == ElementType.Light
                ? ElementType.Dark
                : ElementType.Light;

            audioController.PlaySoundEffect(changeElementSound);
            // Optional: feedback
            System.Diagnostics.Debug.WriteLine($"Element changed to: {CurrentElement}");
        }
    }
}


