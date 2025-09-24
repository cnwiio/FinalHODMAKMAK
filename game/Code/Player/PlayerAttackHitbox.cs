using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using System;
using System.Diagnostics;

namespace game
{
    public class PlayerAttackHitbox : IEntity
    {
        public IShapeF Bounds { get; private set; }
        public string LayerName { get; set; } /*= "PlayerAttack";*/ // ยังไม่มีเลเยอร์นี้ เลยคอมเม้นไว้ก่อน
        private Player _player;
        private float _lifetime;
        private float _elapsed;

        private CollisionComponent _collisionComponent;
        private bool _addedToWorld = false;

        public PlayerAttackHitbox(Player player, RectangleF bounds, float lifetime = 0.2f, CollisionComponent collisionComponent = null)
        {
            _player = player;
            Bounds = bounds;
            _lifetime = lifetime;
            _collisionComponent = collisionComponent;
        }

        public void Update(GameTime gameTime)
        {
            _elapsed += (float)gameTime.ElapsedGameTime.TotalSeconds;

            //if (!_addedToWorld && _collisionComponent != null)
            //{
            //    //_collisionComponent.Insert(this); // ย้ายไปเพิ่มที่ player // ทั้งบรรทัด if ลบทิ้งได้เลย
            //    _addedToWorld = true;
            //}

            // Lifetime expired → remove from world
            if (_elapsed >= _lifetime && _collisionComponent != null)
            {
                _player.RemoveAttackHitbox(this);
                //_collisionComponent.Remove(this); // ย้ายไปเพิ่มที่ player
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
                if (!monster.Monster.isHit)
                {
                    // ย้ายไปเช็คที่มอนแต่ละตัวแทน
                    //float multiplier = _player.CurrentElement != monster.Monster.ElementType
                    //    ? 2.0f
                    //    : 0.5f;

                    monster.Monster.isHit = true;
                    monster.Monster.ApplyDamage(_player.Stats.AttackDamage.Value); // ใส่ดาเมจไปเลยตรงๆ
                    //System.Diagnostics.Debug.WriteLine($"Hit monster! HP: {monster.Monster.HP}");

                    //monster.Monster.HP -= (int)(_player.Stats.AttackDamage.Value * multiplier); ไม่ใช้แล้ว
                }
            }
        }
    }
}
