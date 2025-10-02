using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace game
{
    // IMPORTANT NOTE: now only recieve bottom-left position if want to add other value pls change
    public class GameObject : IYsort
    {
        public Vector2 Position { get; set; }
        public Texture2D Texture { get; set; }
        public float SortY { get; }
        public float SortX { get; }

        public GameObject(Vector2 position, Texture2D texture)
        {
            Texture = texture;
            Position = new Vector2(position.X, position.Y - Texture.Height);
            SortY = position.Y;
            SortX = position.X;
        }
        public GameObject(Vector2 position, ContentManager content,string textureName)
        {
            Texture = content.Load<Texture2D>("Texture/" + textureName);
            Position = new Vector2(position.X, position.Y - Texture.Height);
            SortY = position.Y;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Position, Color.White);
        }
    }
}
