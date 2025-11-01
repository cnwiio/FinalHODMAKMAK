using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using MonoGame.Extended;
using MonoGame.Extended.Animations;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Collisions.Layers;
using MonoGame.Extended.Collisions.QuadTree;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Input.InputListeners;
using MonoGame.Extended.Particles;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Screens.Transitions;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;
using MonoGame.Extended.Timers;
using MonoGame.Extended.ViewportAdapters;
namespace game
{
    public class Chest : IYsort
    {
        /*
        Example how to use: 
        1. 
            private Chest chest = new Chest();
        2.
            chest.Load(Content, "Chests", 32,32, new Vector2(100,100), "F");
        3.
            chest.Update(_player._movement.Position);
        4. 
            chest.Draw(_spriteBatch);
        */
        public Texture2DAtlas ChestAtlas;
        public Texture2DRegion Region;
        public Texture2D Text;

        public Texture2D UI;
        private Vector2 UIPosition;
        private float UIalpha = 1f;

        public Vector2 Position;
        public bool playerInRadius;
        public bool isActive = true;
        public Wall Hitbox;
        private KeyboardState ks, oks;
        private Potion potion;

        private AudioController audioController;
        private SoundEffect soundEffect;

        private SpriteFont font;
        private float fontAlpha = -1f;
        private string fontMessage;
        private Vector2 fontPosition;
        public float SortY { get => Position.Y; }
        public float SortX { get => Position.X; }
        public Chest() { }
        public void Load(Texture2DAtlas ChestAtlas, Texture2D TextTexture, Vector2 position)
        {
            this.ChestAtlas = ChestAtlas;
            Text = TextTexture;
            Region = ChestAtlas[0];
            Position = position;
        }
        public void Load(ContentManager content, string textureName, int textureWidth, int textureHeight, Vector2 position, string TextTextureName, string UITextureName, string FontName, Potion potion, AudioController audioController, SoundEffect soundEffect)
        {
            var texture2D = content.Load<Texture2D>("Texture/" + textureName);
            ChestAtlas = Texture2DAtlas.Create("Atlas/" + textureName, texture2D, textureWidth, textureHeight);
            Position = position;
            Text = content.Load<Texture2D>("Texture/" + TextTextureName);
            UI = content.Load<Texture2D>("Texture/" + UITextureName);
            font = content.Load<SpriteFont>("Fonts/" + FontName);
            Region = ChestAtlas[0];

            // Important NOTE: change this in future
            var origin = new Vector2(32, 32);
            Hitbox = new Wall(new RectangleF(position.X - origin.X, position.Y - origin.Y, 64, 64));

            this.potion = potion;

            this.audioController = audioController;
            this.soundEffect = soundEffect;
        }
        public void Update(Vector2 targetpos)
        {
            oks = ks;
            ks = Keyboard.GetState();
            if (isActive)
            {
                var distance = Vector2.Distance(Position, targetpos);
                if (distance < 100)
                {
                    playerInRadius = true;
                }
                else if (distance >= 100)
                {
                    playerInRadius = false;
                }

                if (playerInRadius)
                {
                    if (ks.IsKeyDown(Keys.F) && !oks.IsKeyDown(Keys.F))
                    {
                        if (!potion.IsFull())
                        {
                            isActive = false;
                            GiveReward();
                            TriggerPopupUI();
                            var pos = new Vector2(UIPosition.X + UI.Width / 2, UIPosition.Y - 10);
                            TriggerMessage(pos, "+ 1");
                        }
                        else
                        {
                            var pos = Position;
                            pos += new Vector2( 0, -Region.Height * 2);
                            TriggerMessage(pos, "Your Potion is full");
                        }
                    }
                } 
            } 
            else
            {
                playerInRadius = false;
                if (UIalpha >= 0f)
                {
                    UIalpha -= 0.02f;
                    UIPosition.Y -= 1f;
                }
            }

            if (fontAlpha >= 0f && isActive)
            {
                fontAlpha -= 0.02f;
            }
            else if (fontAlpha >= 0f && !isActive)
            {
                fontAlpha -= 0.02f;
                fontPosition.Y = UIPosition.Y - 10;
            }
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            if (isActive)
            {
                Region = ChestAtlas[0];
            }
            else if (!isActive) 
            {
                Region = ChestAtlas[1];
            }
            var origin = new Vector2(Region.Width/2, Region.Height/2);
            spriteBatch.Draw(Region, Position, Color.White, 0f, origin, Vector2.One, SpriteEffects.None, 0);


            if (playerInRadius)
            {
                var offset = new Vector2(0, Region.Height);
                var textOrigin = new Vector2(Text.Width /2, Text.Height/2);
                spriteBatch.Draw(Text, Position - offset, null, Color.White, 0f, textOrigin, Vector2.One, SpriteEffects.None, 0);
            }

            if (UIalpha >= 0f)
            {
                var textOrigin = new Vector2(UI.Width / 2, UI.Height / 2);
                spriteBatch.Draw(UI, UIPosition, null, Color.White * UIalpha, 0f, textOrigin, Vector2.One, SpriteEffects.None, 0);
            }

            if (fontAlpha >= 0f)
            {
                if (isActive)
                {
                    var textOrigin = new Vector2(fontMessage.Length * 5, 3);
                    spriteBatch.DrawString(font, fontMessage, fontPosition, Color.Red * fontAlpha, 0f, textOrigin, Vector2.One, SpriteEffects.None, 0); 
                } 
                else 
                {
                    spriteBatch.DrawString(font, fontMessage, fontPosition, Color.White * fontAlpha);
                }
            }
        }

        public void GiveReward()
        {
            potion.Add();
            audioController.PlaySoundEffect(soundEffect);
        }

        public void Unload()
        {
            audioController = null;
            soundEffect = null;
            ChestAtlas = null;
            Region = null;
            Text = null;
            UI = null;
        }

        public void TriggerPopupUI()
        {
            UIalpha = 1f;
            var offset = new Vector2(0, Region.Height);
            UIPosition = Position - offset;
        }

        public void TriggerMessage(Vector2 position, string message)
        {
            fontAlpha = 1f;
            fontPosition = position;
            fontMessage = message;
        }
    }
}
