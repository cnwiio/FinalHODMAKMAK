using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game1
{
    public class PlayerMovement
    {
        public Vector2 Position { get; private set; }
        public Vector2 Direction { get; private set; }

        private PlayerStats _stats;

        // Dash control variables
        private readonly float _dashDuration = 0.2f;  // dash lasts 0.2 seconds
        private readonly float _dashCooldown = 1.0f;  // 1 second cooldown between dashes
        private float _dashTimer = 0f;
        private float _cooldownTimer = 0f;
        private bool _isDashing = false;

        public PlayerMovement(Vector2 startPosition, PlayerStats stats)
        {
            Position = startPosition;
            _stats = stats;
        }
        public void Update(GameTime gameTime, Vector2 direction, bool dashTriggered)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Reduce cooldown timer if > 0
            if (_cooldownTimer > 0f)
                _cooldownTimer -= deltaTime;

            // Start dash if triggered, not dashing, not on cooldown, and moving
            if (dashTriggered && !_isDashing && _cooldownTimer <= 0f && direction != Vector2.Zero)
            {
                _isDashing = true;
                _dashTimer = _dashDuration;
            }

            // Handle dash timer and cooldown
            if (_isDashing)
            {
                _dashTimer -= deltaTime;
                if (_dashTimer <= 0f)
                {
                    _isDashing = false;
                    _cooldownTimer = _dashCooldown;
                }
            }

            Direction = direction;

            if (Direction != Vector2.Zero)
                Direction.Normalize();

            float speed = _stats.Speed.Value;
            if (_isDashing)
                speed *= 5f; // Dash speed multiplier

            Position += Direction * speed * deltaTime;
        }
    }
}
