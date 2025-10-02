using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Collisions;
using System.Collections.Generic;
using System.Diagnostics;

namespace game
{
    public static class DropManager
    {
        /// <summary>
        /// Spawn a HealPickup at the given position and insert into collision system.
        /// </summary>
        public static HealPickup DropHeal(
                Texture2D texture,
                Player player,
                CollisionComponent collisionComponent,
                Vector2 position,
                List<IEntity> scenePendingRemove,
                int healAmount = 25)
        {
            var healPickup = new HealPickup(position, texture, player, collisionComponent, scenePendingRemove, healAmount);

            // Add to collision system immediately
            collisionComponent.Insert(healPickup);

            Debug.WriteLine($"HealPickup spawned at: {position}");
            return healPickup;
        }
    }
}
