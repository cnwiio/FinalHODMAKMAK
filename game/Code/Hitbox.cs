using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Animations;
using System;
using MonoGame.Extended.Input.InputListeners;
using System.Diagnostics;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using System.Collections.Generic;

namespace game
{
    public class PlayerAttack : IEntity
    {
        public IShapeF Bounds { get; set; }
        public string LayerName { get; set; }
        public bool AlwaysDraw => true;
        public PlayerAttack(RectangleF bounds)
        {
            Bounds = bounds;
        }
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            var rect = (RectangleF)Bounds;
            spriteBatch.DrawRectangle(rect, Color.Lime, 3);

            // Draw a small cross at the origin (center) 
            var center = rect.Center;
            float crossSize = 4f;
            spriteBatch.DrawLine(center - new Vector2(crossSize, 0), center + new Vector2(crossSize, 0), Color.BlueViolet, 2);
            spriteBatch.DrawLine(center - new Vector2(0, crossSize), center + new Vector2(0, crossSize), Color.BlueViolet, 2);
        }
        public void OnCollision(CollisionEventArgs collisionInfo)
        {
            if (collisionInfo.Other is Wall)
            {
                Bounds.Position -= collisionInfo.PenetrationVector;
            }
        }
    }

    /*
    IMPORTANT NOTE: Now only receive Texture2D, If want animation need to change in future.(Change in constuctor to recieve AnimController instead)
    Ex how to use:
        _collision.Add(new HealDrops(
            new RectangleF(
                monster.Position,
                new SizeF(_dropTexture.Width, _dropTexture.Height)
                ), 
            _dropTexture,
            _player
        )); 
    */
}
