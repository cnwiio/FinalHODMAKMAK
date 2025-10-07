using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game
{
    public class PlayerInput
    {
        public Vector2 Direction { get; private set; }
        public bool DashTriggered { get; private set; }
        public bool AttackTriggered { get; private set; }
        public bool ElementToggleTriggered { get; private set; } // Q key
        public bool Skill1Triggered { get; private set; } // E key
        public bool Skill2Triggered { get; private set; } // R key


        private KeyboardState _keyboardState;
        private KeyboardState _oldkeyboardState;
        private MouseState _oldMouseState;

        private const float DoubleTapTime = 0.3f; // seconds allowed between taps
        public bool PotionTriggered { get; private set; }

        private double _lastTapTimeW = -1;
        private double _lastTapTimeA = -1;
        private double _lastTapTimeS = -1;
        private double _lastTapTimeD = -1;

        private GameTime _gameTime;

        public void Update(GameTime gameTime)
        {
            _gameTime = gameTime;
            DashTriggered = false;
            AttackTriggered = false;

            _keyboardState = Keyboard.GetState();
            var dir = Vector2.Zero;

            double now = gameTime.TotalGameTime.TotalSeconds;

            //// Dash
            //if (IsKeyDoubleTapped(Keys.W, ref _lastTapTimeW, now)) DashTriggered = true;
            //if (IsKeyDoubleTapped(Keys.A, ref _lastTapTimeA, now)) DashTriggered = true;
            //if (IsKeyDoubleTapped(Keys.S, ref _lastTapTimeS, now)) DashTriggered = true;
            //if (IsKeyDoubleTapped(Keys.D, ref _lastTapTimeD, now)) DashTriggered = true;

            // Dash: press Spacebar
            DashTriggered = _keyboardState.IsKeyDown(Keys.Space) && !_oldkeyboardState.IsKeyDown(Keys.Space);

            // Movement
            if (_keyboardState.IsKeyDown(Keys.W)) dir.Y -= 1;
            if (_keyboardState.IsKeyDown(Keys.S)) dir.Y += 1;
            if (_keyboardState.IsKeyDown(Keys.A)) dir.X -= 1;
            if (_keyboardState.IsKeyDown(Keys.D)) dir.X += 1;

            // Attack
            MouseState mouseState = Mouse.GetState();
            bool justClicked = mouseState.LeftButton == ButtonState.Pressed && _oldMouseState.LeftButton == ButtonState.Released;
            AttackTriggered = justClicked;
            _oldMouseState = mouseState;

            // Element Toggle: press Q
            ElementToggleTriggered = _keyboardState.IsKeyDown(Keys.Q) && !_oldkeyboardState.IsKeyDown(Keys.Q);

            // Skill 1 (E)
            Skill1Triggered = _keyboardState.IsKeyDown(Keys.E) && !_oldkeyboardState.IsKeyDown(Keys.E);

            // Skill 2 (R)
            Skill2Triggered = _keyboardState.IsKeyDown(Keys.R) && !_oldkeyboardState.IsKeyDown(Keys.R);


            PotionTriggered = _keyboardState.IsKeyDown(Keys.LeftShift) && !_oldkeyboardState.IsKeyDown(Keys.LeftShift);


            Direction = dir;
            _oldkeyboardState = _keyboardState;
        }
        //private bool IsKeyDoubleTapped(Keys key, ref double lastTapTime, double now)
        //{
        //    var keyboardState = Keyboard.GetState();

        //    // Detect key press down event
        //    bool justPressed = keyboardState.IsKeyDown(key) && !_oldkeyboardState.IsKeyDown(key);

        //    if (justPressed)
        //    {
        //        if (lastTapTime < 0)
        //        {
        //            lastTapTime = now;
        //            return false; // first tap
        //        }
        //        else if (now - lastTapTime <= DoubleTapTime)
        //        {
        //            lastTapTime = -1; // reset
        //            return true; // double tap detected
        //        }
        //        else
        //        {
        //            lastTapTime = now; // too late, treat as new first tap
        //        }
        //    }

        //    return false;
        //}
    }
}
