#region File Description
//-----------------------------------------------------------------------------
// AnimatedTexture.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace game
{
    public class AnimatedTexture
    {
        public int framecount;
        public Texture2D myTexture;
        private float TimePerFrame;
        private int Frame;
        public int framerow = 1; // frame row
        private int frame_r; // count frame row 
        private int startframe;
        private int endframe;
        private float TotalElapsed;
        private bool Paused;
        private bool Ended;
        private int Overload;
        private int startrow;
        private bool flip = false;
        private int pauseFrame = -1;
        private int pauseRow = -1;

        public float Rotation, Scale, Depth;
        public Vector2 Origin;

        public AnimatedTexture(Vector2 origin, float rotation, float scale, float depth)
        {
            this.Origin = origin;
            this.Rotation = rotation;
            this.Scale = scale;
            this.Depth = depth;
        }

        // --- Public getters for PlayerAnimation ---
        public Texture2D MyTexture => myTexture;
        public int FrameCount => framecount;
        public int FrameRow => framerow;

        public float TextureWidth => myTexture != null ? myTexture.Width / framecount : 0;
        public float TextureHeight => myTexture != null ? myTexture.Height / framerow : 0;

        // --- Load, Update, Draw methods (unchanged) ---
        public void Load(ContentManager content, string asset, int frameCount, int frameRow, int framesPerSec)
        {
            framecount = frameCount;
            framerow = frameRow;
            startframe = 0;
            endframe = (frameCount * framerow) - 1;
            myTexture = content.Load<Texture2D>(asset);
            TimePerFrame = (float)1 / framesPerSec;
            Frame = 0;
            frame_r = 0;
            TotalElapsed = 0;
            Paused = false;
            Ended = false;
            Overload = 1;
        }

        public void Load(ContentManager content, string asset, int frameCount, int frameRow, int framesPerSec, int startRow)
        {
            framecount = frameCount;
            framerow = frameRow;
            startframe = 0;
            endframe = (frameCount * framerow) - 1;
            myTexture = content.Load<Texture2D>(asset);
            TimePerFrame = (float)1 / framesPerSec;
            Frame = 0;
            frame_r = 0;
            TotalElapsed = 0;
            Paused = false;
            Ended = false;
            Overload = 2;
            startrow = startRow;
        }

        public void UpdateFrame(float elapsed)
        {
            if (pauseFrame > -1 && pauseRow > -1)
            {
                frame_r = pauseRow;
                Frame = pauseFrame;
                Paused = true;
                pauseFrame = -1;
                pauseRow = -1;
            }
            if (Paused)
                return;

            TotalElapsed += elapsed;
            if (TotalElapsed > TimePerFrame)
            {
                Frame++;
                if (Frame == framecount)
                {
                    frame_r++;
                    if (Overload == 2) Ended = true;
                }
                if (frame_r == framerow)
                {
                    frame_r = 0;
                    if (Overload == 1) Ended = true;
                }

                Frame %= framecount;
                TotalElapsed -= TimePerFrame;
            }
        }

        public void DrawFrame(SpriteBatch batch, Vector2 screenPos, bool flip)
        {
            this.flip = flip;
            DrawFrame(batch, Frame, screenPos);
        }

        public void DrawFrame(SpriteBatch batch, Vector2 screenPos)
        {
            DrawFrame(batch, Frame, screenPos);
        }

        public void DrawFrame(SpriteBatch batch, Vector2 screenPos, int row)
        {
            DrawFrame(batch, Frame, screenPos, row);
        }

        public void DrawFrame(SpriteBatch batch, int frame, Vector2 screenPos)
        {
            int FrameWidth = myTexture.Width / framecount;
            int FrameHeight = myTexture.Height / framerow;
            Rectangle sourcerect;
            if (Overload == 1)
            {
                sourcerect = new Rectangle(FrameWidth * frame, FrameHeight * frame_r, FrameWidth, FrameHeight);
            }
            else
            {
                sourcerect = new Rectangle(FrameWidth * frame, FrameHeight * (startrow - 1), FrameWidth, FrameHeight);
            }

            batch.Draw(myTexture, screenPos, sourcerect, Color.White, Rotation, Origin, Scale, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Depth);
        }

        public void DrawFrame(SpriteBatch batch, int frame, Vector2 screenPos, int row)
        {
            int FrameWidth = myTexture.Width / framecount;
            int FrameHeight = myTexture.Height / framerow;
            startrow = row;
            Rectangle sourcerect = new Rectangle(FrameWidth * frame, FrameHeight * (startrow - 1), FrameWidth, FrameHeight);
            batch.Draw(myTexture, screenPos, sourcerect, Color.White, Rotation, Origin, Scale, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Depth);
        }

        // --- Pause / Reset / Play / Stop helpers ---
        public bool IsPaused => Paused;
        public bool IsEnd => Ended;

        public void Reset() { Frame = 0; TotalElapsed = 0f; }
        public void Stop() { Pause(); Reset(); }
        public void Play() { Paused = false; }
        public void Pause() { Paused = true; }
        public void Pause(int frame, int row) { pauseFrame = frame; pauseRow = row; }
    }
}
