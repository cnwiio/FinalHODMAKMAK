using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace game
{
    public class Button
    {
        public Rectangle Rect;
        public Vector2 Position;
        public Texture2D Texture;
        public bool IsHover;
        public bool IsClick = false;
        public bool IsReleased = false;
        public bool Active => IsReleased;
        private MouseState ms, oms;
        private short height, width;

        public Button(Vector2 position, short width, short height, Texture2D texture)
        {

            var origin = new Point(width / 2, height / 2);
            Point point = new Point((int)position.X, (int)position.Y);
            Size size = new Size(width, height);
            Rect = new Rectangle(point - origin, size);


            Position = position;
            Texture = texture;
            this.height = height;
            this.width = width;
        }

        public void Update()
        {
            oms = ms;
            ms = Mouse.GetState();
            int mouseX = ms.X;
            int mouseY = ms.Y;

            if (mouseX > Rect.Left && mouseX < Rect.Right && mouseY > Rect.Top && mouseY < Rect.Bottom)
            {
                IsHover = true;
                CheckClick();
            }
            else
            {
                IsHover = false;
                IsClick = false;
                IsReleased = false;
            }
            //Debug.WriteLineIf(Active, "Click");
        }

        public void Draw(SpriteBatch spriteBatch, short index = 0)
        {
            Color hover = IsClick ? Color.Gray : IsHover ? Color.DarkGray : Color.White;
            var origin = new Vector2(width / 2, height / 2);
            spriteBatch.Draw(Texture, Position, new Rectangle(0, height * index, width, height),hover
                ,0, origin, 1, SpriteEffects.None, 0);
            //spriteBatch.Draw(Texture, Rect,hover);
        }

        private void CheckClick()
        {
            if (ms.LeftButton == ButtonState.Pressed || oms.LeftButton == ButtonState.Pressed)
            {
                IsClick = true;
                if (ms.LeftButton == ButtonState.Released)
                {
                    IsReleased = true;
                }
            }
            else
            {
                IsClick = false;
                IsReleased = false;
            }
        }
    }
}
