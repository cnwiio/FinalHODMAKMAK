using Microsoft.Xna.Framework.Graphics;

namespace game
{
    //drawables.Sort((a, b) => a.SortY.CompareTo(b.SortY));
    public interface IYsort
    {
        float SortY { get; }
        void Draw(SpriteBatch spriteBatch);
    }
}
