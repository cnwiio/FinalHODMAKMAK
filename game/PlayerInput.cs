using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game1
{
    public class PlayerInput
    {
        public Vector2 Direction { get; private set; }
        public Vector2 Direction2 { get; private set; } = Vector2.Zero;
        public bool DashTriggered { get; private set; }
        public bool AttackTriggered { get; private set; }

        private KeyboardState _keyboardState;
        private KeyboardState _oldkeyboardState;
        private MouseState _oldMouseState;

        private const float DoubleTapTime = 0.3f; // seconds allowed between taps

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

            // Dash
            if (IsKeyDoubleTapped(Keys.W, ref _lastTapTimeW, now)) DashTriggered = true;
            if (IsKeyDoubleTapped(Keys.A, ref _lastTapTimeA, now)) DashTriggered = true;
            if (IsKeyDoubleTapped(Keys.S, ref _lastTapTimeS, now)) DashTriggered = true;
            if (IsKeyDoubleTapped(Keys.D, ref _lastTapTimeD, now)) DashTriggered = true;

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

            if (dir != Vector2.Zero)
            {
                Direction2 = dir;
            }
            Direction = dir;
            _oldkeyboardState = _keyboardState;
        }
        private bool IsKeyDoubleTapped(Keys key, ref double lastTapTime, double now)
        {
            var keyboardState = Keyboard.GetState();

            // Detect key press down event
            bool justPressed = keyboardState.IsKeyDown(key) && !_oldkeyboardState.IsKeyDown(key);

            if (justPressed)
            {
                if (lastTapTime < 0)
                {
                    lastTapTime = now;
                    return false; // first tap
                }
                else if (now - lastTapTime <= DoubleTapTime)
                {
                    lastTapTime = -1; // reset
                    return true; // double tap detected
                }
                else
                {
                    lastTapTime = now; // too late, treat as new first tap
                }
            }

            return false;
        }
    }
}
