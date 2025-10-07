using System.Diagnostics;
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
        public Vector2 TargetPos { get; set; }
        public string CurrentScene { get; set; }
        private ScreenManager screenManager;
        private Game1 game;
        private Player player;
        //private bool db;
        public bool AlwaysDraw => true;
        public Door(RectangleF bounds, string currenttScene, string targetScene, Vector2 targetPos, Game game)
        {
            Bounds = bounds;
            CurrentScene = currenttScene;
            TargetScene = targetScene;
            TargetPos = targetPos;
            this.game = (Game1)game;
            player = this.game.Player;
            screenManager = this.game.screenManager;
            //db = false;
        }
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawRectangle((RectangleF)Bounds, Color.Red, 3);
        }
        public void OnCollision(CollisionEventArgs collisionInfo)
        {
            if (collisionInfo.Other is PlayerCollisionBox)
            {
                player.DestinationPos = TargetPos;
                player.CurrentScene = TargetScene;
                game.SavedHP = (short)game.Player.Stats.CurrentHP;
                game.SavedPotion = game.Player.potion.Amout;
                if (TargetScene == "SceneHome")
                {
                    screenManager.LoadScreen(new SceneHome(game), new FadeTransition(game.GraphicsDevice, Color.Black, 1f));
                }
                else if (TargetScene == "ScenePrologue")
                {
                    screenManager.LoadScreen(new ScenePrologue(game), new FadeTransition(game.GraphicsDevice, Color.Black, 1f));
                }
                else if (TargetScene == "SceneSmallBrigde")
                {
                    screenManager.LoadScreen(new SceneSmallBrigde(game), new FadeTransition(game.GraphicsDevice, Color.Black, 1f));
                }
                else if (TargetScene == "SceneHomeNorth")
                {
                    screenManager.LoadScreen(new SceneHomeNorth(game), new FadeTransition(game.GraphicsDevice, Color.Black, 1f));
                }
                else if (TargetScene == "SceneFlowerHills")
                {
                    screenManager.LoadScreen(new SceneFlowerHills(game), new FadeTransition(game.GraphicsDevice, Color.Black, 1f));
                }
                else if (TargetScene == "SceneWaterfall")
                {
                    screenManager.LoadScreen(new SceneWaterfall(game), new FadeTransition(game.GraphicsDevice, Color.Black, 1f));
                }
                else if (TargetScene == "SceneUnderWaterfall")
                {
                    screenManager.LoadScreen(new SceneUnderWaterfall(game), new FadeTransition(game.GraphicsDevice, Color.Black, 1f));
                }
            }
        }
    }
}
