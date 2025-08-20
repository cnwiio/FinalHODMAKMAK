using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Toolkit.HighPerformance;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;
using MonoGame.Extended.ViewportAdapters;

namespace game
{
    public class GameObject
    {
        public Microsoft.Xna.Framework.Vector2 Position { get; set; }
        public Texture2D Texture { get; set; }
        public bool Ysort { get; set; }
        public string Name { get; set; }

        public GameObject(Vector2 position, Texture2D texture, bool ysort)
        {
            Position = position;
            Texture = texture;
            Ysort = ysort;
        }
        public GameObject(Vector2 position, Texture2D texture, bool ysort, string name)
        {
            Position = position;
            Texture = texture;
            Ysort = ysort;
            Name = name;
        }

        public GameObject(Vector2 position, bool ysort)
        {
            Position = position;
            Ysort = ysort;
        }
        public GameObject(Vector2 position, bool ysort, string name)
        {
            Position = position;
            Ysort = ysort;
            Name = name;
        }
    }
}
