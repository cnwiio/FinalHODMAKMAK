using Microsoft.Xna.Framework.Graphics;

namespace game
{
    //drawables.Sort((a, b) => a.SortY.CompareTo(b.SortY));
    /// <summary>
    /// Draw a object in ysort order
    /// </summary>
    public interface IYsort
    {
        /// <summary>
        /// A Y position that at the bottom of charecter
        /// </summary>
        float SortY { get; }
        void Draw(SpriteBatch spriteBatch);
    }
}
