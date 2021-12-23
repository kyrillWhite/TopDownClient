using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace TopDown
{
    public class MainGame : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        public Scene _scene;
        public double Scale { get; set; } = 1;

        public MainGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            GameData.Scale = (float)Scale;
            _graphics.PreferredBackBufferWidth = (int)(1280 * GameData.Scale);
            _graphics.PreferredBackBufferHeight = (int)(720 * GameData.Scale);
            _graphics.ApplyChanges();
            GameData.WindowSize = new Vector2(1280, 720);
            GameData.Initialize(Content);
            _scene = new SceneMenu();
            _scene.Initialize(this);
            Exiting += (e1, e2) => { GameData.Clear(); };
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            _scene.Update(this);
            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);
            _spriteBatch.Begin(SpriteSortMode.FrontToBack);
            lock (GameData.GameObjects)
            {
                foreach (var gameObject in GameData.GameObjects)
                {
                    gameObject.Draw(_spriteBatch);
                }
            }
            foreach (var label in GameData.Labels)
            {
                label.Draw(_spriteBatch);
            }
            _spriteBatch.End();
            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
