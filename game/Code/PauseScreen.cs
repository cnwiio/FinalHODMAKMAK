using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace game
{
    public class PauseScreen
    {
        private Texture2D texture;
        private KeyboardState ks, oks;
        private MouseState ms, oms;
        private bool paused;
        public bool IsPaused => paused;

        private const int FrameWidth = 1920;
        private const int FrameHeight = 1080;
        private int currentFrame = 0; // 0 = none, 1 = Resume, 2 = Main Menu, 3 = Quit

        // Button hit areas
        private Rectangle resumeRect = new Rectangle(832, 540, 256, 40);
        private Rectangle menuRect = new Rectangle(792, 652, 336, 80);
        private Rectangle quitRect = new Rectangle(888, 760, 144, 40);

        public bool ResumeClicked { get; private set; }
        public bool MainMenuClicked { get; private set; }
        public bool QuitClicked { get; private set; }

        // Track if mouse was pressed over each button
        private bool resumePressed, menuPressed, quitPressed;

        public PauseScreen(Texture2D pauseTexture)
        {
            texture = pauseTexture;
        }

        public void Update(GameTime gameTime)
        {
            oks = ks;
            ks = Keyboard.GetState();
            oms = ms;
            ms = Mouse.GetState();

            ResumeClicked = MainMenuClicked = QuitClicked = false;

            // toggle pause 
            if (oks.IsKeyDown(Keys.Escape) && ks.IsKeyUp(Keys.Escape))
                paused = !paused;

            if (!paused) return;

            Point mousePoint = new Point(ms.X, ms.Y);
            currentFrame = 0;

            // determine which button is hovered
            if (resumeRect.Contains(mousePoint)) currentFrame = 1;
            else if (menuRect.Contains(mousePoint)) currentFrame = 2;
            else if (quitRect.Contains(mousePoint)) currentFrame = 3;

            // handle click logic (press + release inside button)
            // Resume

            if (resumeRect.Contains(mousePoint) && ms.LeftButton == ButtonState.Pressed)
                resumePressed = true;
            if (resumePressed && ms.LeftButton == ButtonState.Released && resumeRect.Contains(mousePoint))
            {
                ResumeClicked = true;
                resumePressed = false;
            }
            if (!resumeRect.Contains(mousePoint))
                resumePressed = false;

            // Main Menu
            if (menuRect.Contains(mousePoint) && ms.LeftButton == ButtonState.Pressed)
                menuPressed = true;
            if (menuPressed && ms.LeftButton == ButtonState.Released && menuRect.Contains(mousePoint))
            {
                MainMenuClicked = true;
                menuPressed = false;
            }
            if (!menuRect.Contains(mousePoint))
                menuPressed = false;

            // Quit
            if (quitRect.Contains(mousePoint) && ms.LeftButton == ButtonState.Pressed)
                quitPressed = true;
            if (quitPressed && ms.LeftButton == ButtonState.Released && quitRect.Contains(mousePoint))
            {
                QuitClicked = true;
                quitPressed = false;
            }
            if (!quitRect.Contains(mousePoint))
                quitPressed = false;
        }
        public void Resume() 
        {
            paused = false;
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            if (!paused) return;

            Rectangle source = new Rectangle(FrameWidth * currentFrame, 0, FrameWidth, FrameHeight);
            spriteBatch.Draw(texture, Vector2.Zero, source, Color.White);
        }
    }
}
