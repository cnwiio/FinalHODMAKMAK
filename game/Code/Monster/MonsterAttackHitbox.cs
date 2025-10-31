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
        public bool AlwaysDraw => true;
        public bool PlaySound = false;
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
            PlaySound = false;
            if ((collisionInfo.Other is PlayerHurtbox || collisionInfo.Other is PlayerAttackHitbox) && Monster is MonsterRange)
            {
                var mon = Monster as MonsterRange;
                mon.BulletVisible = false;
                if (collisionInfo.Other is PlayerAttackHitbox)
                {
                    PlaySound = true;
                }
            }

            if ((collisionInfo.Other is PlayerHurtbox || collisionInfo.Other is PlayerAttackHitbox) && Monster is MonsterBoss)
            {
                bulletVisible = false;
                if (collisionInfo.Other is PlayerAttackHitbox)
                {
                    PlaySound = true;
                }
            }

            if (collisionInfo.Other is PlayerHurtbox && (Monster is MonsterMelee || Monster is MonsterSlime || Monster is MonsterBoss)) 
            {
                TimeToLiveSeconds = 0.001f;
            }
        }
    }
}
