using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.ViewportAdapters;

namespace game
{
    /*
        Ex how to use:
    1.
        private GlobalCamera camera;
    2.
        var viewPortAdapter = new BoxingViewportAdapter(Window, GraphicsDevice, _mapWidth, _mapHeight);
        camera = new GlobalCamera(viewportAdapter);
            _camera = camera.Cam;
    3.
        camera.Update(_player._movement.Position - new Vector2(
                    (_mapWidth / 2),
                    (_mapHeight / 2)
                    ));
        camera.AdjustZoom();
    4.
        if (monster.ShakeViewport)
        {
            camera.ShakeCamera(gameTime);
            monster.ShakeViewport = camera.ShakeViewport;
        }
    5.
        _spriteBatch.Begin(transformMatrix: _camera.GetViewMatrix());
    */
    public class GlobalCamera
    {
        public OrthographicCamera Cam;
        public Vector2 Position;
        private short _cameraWidth;
        private short _cameraHeight;
        public GlobalCamera(ViewportAdapter viewportAdapter)
        {
            Cam = new OrthographicCamera(viewportAdapter);
            _cameraWidth = (short)viewportAdapter.VirtualWidth;
            _cameraHeight = (short)viewportAdapter.VirtualHeight;
            Position = Vector2.Zero;
            Cam.MaximumZoom = 2f;
            Cam.MinimumZoom = 1f;
            Cam.Zoom = 1.5f;
        }

        public void Update(Vector2 position)
        {
            if (position.X > 0 && position.X < 4480 - _cameraWidth)
            {
                Position.X = position.X;
                Cam.Position = Position;
            }
            if (position.Y > 0 && position.Y < 3200 - _cameraHeight)
            {
                Position.Y = position.Y;
                Cam.Position = Position;
            }
        }

        public void AdjustZoom()
        {
            var state = Keyboard.GetState();
            float zoomPerTick = 0.1f;
            if (state.IsKeyDown(Keys.Z))
            {
                Cam.ZoomIn(zoomPerTick);
            }
            if (state.IsKeyDown(Keys.X))
            {
                Cam.ZoomOut(zoomPerTick);
            }
            if (state.IsKeyDown(Keys.C))
            {
                Cam.ZoomIn(zoomPerTick * 0.1f);
            }
            if (state.IsKeyDown(Keys.V))
            {
                Cam.ZoomOut(zoomPerTick * 0.1f);
            }
            if (state.IsKeyDown(Keys.P))
            {
                Cam.Zoom = 1.5f;
            }
        }

        public bool ShakeViewport { get; set; } = false;
        private const float _shakeRadius = 5f,  _shakeStartAngle = 0f,  _shakeDuration = 0.2f;
        private float       _sr = _shakeRadius, _sa = _shakeStartAngle, _sd = _shakeDuration;
        public void ShakeCamera(GameTime gameTime)
        {
            Random rand = new Random();
            float deltaTime = gameTime.GetElapsedSeconds();
            ShakeViewport = true;
            Cam.Position += new Vector2(
                (float)(Math.Sin(_sa) * _sr),
                (float)(Math.Cos(_sa) * _sr)
            ); // Logic mai ruu gu copy ma 
            // comment gu kor moar
            _sr *= 0.95f; // reduce shake Strength
            _sa += MathHelper.ToRadians(150 + rand.Next(60)); // Random direction
            _sd -= deltaTime;
            if (_sd <= 0 || _sr <= 0)
            {
                _sr = _shakeRadius;
                _sd = _shakeDuration;
                _sa = _shakeStartAngle;
                ShakeViewport = false;
            }
        }
        public void ShakeCamera(GameTime gameTime, float strength, float duration)
        {
            if(ShakeViewport == false)
            {
                _sr = strength;
                _sd = duration;
                _sa = _shakeStartAngle;
            }
            Random rand = new Random();
            float deltaTime = gameTime.GetElapsedSeconds();
            ShakeViewport = true;
            Cam.Position += new Vector2(
                (float)(Math.Sin(_sa) * _sr),
                (float)(Math.Cos(_sa) * _sr)
            ); // Logic mai ruu gu copy ma 
            // comment gu kor moar
            _sr *= 0.85f; // reduce shake Strength
            _sa += MathHelper.ToRadians(150 + rand.Next(60)); // Random direction
            _sd -= deltaTime;
            if (_sd <= 0 || _sr <= 0)
            {
                _sr = _shakeRadius;
                _sd = _shakeDuration;
                _sa = _shakeStartAngle;
                ShakeViewport = false;
            }
        }
    }
}
