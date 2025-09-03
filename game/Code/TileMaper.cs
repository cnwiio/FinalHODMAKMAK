using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using Microsoft.Toolkit.HighPerformance;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Collisions;
using MonoGame.Extended.Collisions.Layers;
using MonoGame.Extended.Collisions.QuadTree;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Screens.Transitions;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;
using MonoGame.Extended.ViewportAdapters;

namespace game
{
    /*
     Example how to use :
    1.
            private List<GameObject> _gameObjects;
            private TileMaper _tileMaper;
            ...
            _tileMaper = new TileMaper(Game);
    2.
            _tileMaper.LoadMap(Content, "TileMapName");
            _tileMaper.LoadObjects(Content, "LayerName", _gameObjects);
            _tileMaper.LoadCollision(_collisionComponent, _collision, "Collision");
    3. 
            _tileMaper.UpdateMap(gameTime);
    4.
            _tileMaper.DrawMap(_camera);
            _tileMaper.DrawObjects(_spriteBatch, _gameObjects);
     */
    public class TileMaper
    {
        public TiledMap TiledMap;
        public TiledMapRenderer TiledMapRenderer;
        public Game game { get; }



        public TileMaper(Game game)
        {
            this.game = game;
        }




        public void LoadMap(ContentManager content, string tiledMapName)
        {
            TiledMap = content.Load<TiledMap>("TileMap/" + tiledMapName);
            TiledMapRenderer = new TiledMapRenderer(game.GraphicsDevice, TiledMap);
        }


        public void LoadObjects(ContentManager content, string LayerName, List<GameObject> gameObject)
        {
            var objectLayer = TiledMap.GetLayer<TiledMapObjectLayer>(LayerName);
            foreach (var obj in objectLayer.Objects)
            {
                if (obj.Properties.ContainsKey("Ysort") && obj.Properties["Ysort"] == "true")
                {
                    gameObject.Add(new GameObject
                    (
                        obj.Position,
                        content.Load<Texture2D>(obj.Type),
                        true
                    ));
                }
            }
        }


        public void LoadCollision(CollisionComponent collisionComponent, List<IEntity> collisionList, string layerName)
        {
            var collisionLayer = TiledMap.GetLayer<TiledMapObjectLayer>(layerName);
            foreach (var obj in collisionLayer.Objects)
            {
                collisionList.Add(new Wall(new RectangleF(obj.Position.X, obj.Position.Y, obj.Size.Width, obj.Size.Height)));
            }
            foreach (IEntity obj in collisionList)
            {
                collisionComponent.Insert(obj);
            }
        }


        public void UpdateMap(GameTime gameTime)
        {
            TiledMapRenderer.Update(gameTime);
        }


        public void DrawMap(OrthographicCamera camera)
        {
            BlendState previousBlendState = game.GraphicsDevice.BlendState;
            game.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            TiledMapRenderer.Draw(camera.GetViewMatrix());
        }


        public void DrawMap(OrthographicCamera camera, int layerIndex)
        {
            BlendState previousBlendState = game.GraphicsDevice.BlendState;
            game.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            TiledMapRenderer.Draw(layerIndex ,camera.GetViewMatrix());
        }


        public void DrawObjects(SpriteBatch spriteBatch, List<GameObject> gameObjects)
        {
            foreach (var obj in gameObjects)
            {
                if (obj.Ysort)
                {
                    spriteBatch.Draw(obj.Texture, obj.Position, Color.White);
                }
            }
        }

        /// <summary>
        /// get ObjectLayer of tile map 
        /// </summary>
        /// <param name="layerName"> name of layer</param>
        /// <returns></returns>
        public TiledMapObjectLayer GetObjectLayer(string layerName)
        {
            return TiledMap.GetLayer<TiledMapObjectLayer>(layerName);
        }

    }
}
