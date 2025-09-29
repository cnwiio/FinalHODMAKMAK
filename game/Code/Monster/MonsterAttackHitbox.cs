using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;


namespace game
{
    public class MonsterAttackHitbox : IEntity
    {
        public IMonster Monster { get; private set; }
        public IShapeF Bounds { get; set; }
        public string LayerName { get; set; }
        public float TimeToLiveSeconds { get; set; }
        public bool bulletVisible { get; set; }
        public MonsterAttackHitbox(RectangleF bounds, float timeToLiveSeconds, IMonster monster)
        {
            Bounds = bounds;
            TimeToLiveSeconds = timeToLiveSeconds;
            Monster = monster;
        }
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawRectangle((RectangleF)Bounds, Color.Red, 3);
        }
        public void OnCollision(CollisionEventArgs collisionInfo)
        {
            if ((collisionInfo.Other is Wall || collisionInfo.Other is PlayerHurtbox || collisionInfo.Other is PlayerAttackHitbox) && Monster is MonsterRange)
            {
                var mon = Monster as MonsterRange;
                mon.BulletVisible = false;
            }
            if ((collisionInfo.Other is Wall || collisionInfo.Other is PlayerHurtbox || collisionInfo.Other is PlayerAttackHitbox) && Monster is MonsterBoss)
            {
                bulletVisible = false;
            }
        }
    }
}
