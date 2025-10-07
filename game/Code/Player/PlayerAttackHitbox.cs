using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace game
{
    public class PlayerAttackHitbox : IEntity
    {
        public virtual IShapeF Bounds { get; protected set; }
        public string LayerName { get; set; }
        private Player _player;
        private float _lifetime;
        private float _elapsed;
        private int _damage;
        private int _maxHitsPerMonster; // controlled via constructor
        private float _hitDelay = 0f; // seconds
        private Dictionary<IMonster, int> _hitCounts = new Dictionary<IMonster, int>();
        private Dictionary<IMonster, float> _hitTimers = new Dictionary<IMonster, float>();
        public bool AlwaysDraw => true;

        private CollisionComponent _collisionComponent;
        public PlayerAttackHitbox(Player player, RectangleF bounds, float lifetime = 0.5f, CollisionComponent collisionComponent = null, int damage = 0, int maxHitsPerMonster = 1, float hitDelay = 0.3f)
        {
            _player = player;
            Bounds = bounds;
            _lifetime = lifetime;
            _collisionComponent = collisionComponent;

            _damage = damage > 0 ? damage : (int)_player.Stats.AttackDamage.Value;
            _maxHitsPerMonster = maxHitsPerMonster;
            _hitDelay = hitDelay;
        }

        public virtual void Update(GameTime gameTime)
        {
            _elapsed += (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Update timers for each monster
            var keys = new List<IMonster>(_hitTimers.Keys);
            foreach (var monster in keys)
            {
                _hitTimers[monster] -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (_elapsed >= _lifetime && _collisionComponent != null)
            {
                _player.RemoveAttackHitbox(this);

            }
        }

        public void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            // Optional: draw debug rectangle
            spriteBatch.DrawRectangle((RectangleF)Bounds, Microsoft.Xna.Framework.Color.Red, 2);
        }

        public void OnCollision(CollisionEventArgs collisionInfo)
        {
            if (collisionInfo.Other is MonsterHurtbox monster)
            {

                if (!_hitCounts.ContainsKey(monster.Monster))
                    _hitCounts[monster.Monster] = 0;


                if (!_hitTimers.ContainsKey(monster.Monster))
                    _hitTimers[monster.Monster] = 0f;

                if (_hitCounts[monster.Monster] < _maxHitsPerMonster && _hitTimers[monster.Monster] <= 0f && !monster.Monster.isHit)
                {
                    monster.Monster.ApplyDamage(_damage);
                    _hitCounts[monster.Monster]++;
                    _hitTimers[monster.Monster] = _hitDelay;
                    monster.Monster.isHit = true;
                }
            }
        }
    }
}
