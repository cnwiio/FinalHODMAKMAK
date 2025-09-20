using game;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using System.Diagnostics;

public class PlayerAttackHitbox : IEntity
{
    public IShapeF Bounds { get; private set; }
    public string LayerName { get; set; }
    private Player _player;

    public PlayerAttackHitbox(Player player, RectangleF bounds)
    {
        _player = player;
        Bounds = bounds;
    }

    public void Update()
    {
        // Attack hitbox does not move; if you want, you can attach it to player
    }

    public void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
    {
        spriteBatch.DrawRectangle((RectangleF)Bounds, Color.Red, 2);
    }

    public void OnCollision(CollisionEventArgs collisionInfo)
    {
        if (collisionInfo.Other is MonsterHurtbox monster)
        {
            if (monster.Monster.isHit) return;

            int baseDamage = _player.Stats.AttackDamage.Value;
            int finalDamage = baseDamage;

            // Elemental multiplier
            //if (monster.Monster.Element != _player.CurrentElement)
            //    finalDamage = (int)(baseDamage * 2f);
            //else
            //    finalDamage = (int)(baseDamage * 0.5f);

            monster.Monster.HP -= finalDamage;
            monster.Monster.isHit = true;

            Debug.WriteLine($"Hit monster! Damage: {finalDamage}, Remaining HP: {monster.Monster.HP}");
        }
    }
}
