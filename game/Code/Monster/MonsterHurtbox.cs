using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;

namespace game
{
    public class MonsterHurtbox : IEntity
    {
        public IShapeF Bounds { get; set; }
        public string LayerName { get; set; }
        public IMonster Monster { get; set; }
        public MonsterHurtbox(RectangleF bounds, IMonster monsterMelee)
        {
            Bounds = bounds;
            Monster = monsterMelee;
        }
        public void Update(Vector2 position)
        {
            var rect = (RectangleF)Bounds;
            rect.Position = position - (rect.Size / 2f);
            Bounds = rect;
        }
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            var rect = (RectangleF)Bounds;
            spriteBatch.DrawRectangle(rect, Color.Lime, 3);

            //// Draw a small cross at the origin (center) 
            var center = rect.Center;
            float crossSize = 4f;
            spriteBatch.DrawLine(center - new Vector2(crossSize, 0), center + new Vector2(crossSize, 0), Color.BlueViolet, 2);
            spriteBatch.DrawLine(center - new Vector2(0, crossSize), center + new Vector2(0, crossSize), Color.BlueViolet, 2);
        }
        public void OnCollision(CollisionEventArgs collisionInfo)
        {
            if (collisionInfo.Other is PlayerAttack)
            {
                if (!Monster.isHit)
                {
                    Monster.isHit = true;
                }
            }
        }
    }
}
