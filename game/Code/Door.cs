using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Screens.Transitions;

namespace game
{
    public class Door : IEntity
    {
        public IShapeF Bounds { get; set; }
        public string LayerName { get; set; }
        public string TargetScene { get; set; }
        private ScreenManager screenManager;
        private Game1 game;
        private bool db;
        public Door(RectangleF bounds, string targetScene, Game game)
        {
            Bounds = bounds;
            TargetScene = targetScene;
            this.game = (Game1)game;
            screenManager = this.game.screenManager;
            db = false;
        }
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawRectangle((RectangleF)Bounds, Color.Red, 3);
        }
        public void OnCollision(CollisionEventArgs collisionInfo)
        {
            if (collisionInfo.Other is PlayerCollisionBox)
            {
                if (TargetScene == "SceneHome" && !db)
                {
                    screenManager.LoadScreen(new SceneMenu(game), new FadeTransition(game.GraphicsDevice, Color.Black, 1f)); 
                    db = true;
                }
            }
        }
    }
}
