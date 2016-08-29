using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Helper;
using System.Diagnostics;

namespace GhostVibe
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        ActionManager actionManager;

        protected Texture2D animationTexture, spriteTexture;
        protected Sprite animation, sprite;
        protected SpriteFont debugFont;
        protected string debugMessage;

        // mouse states
        protected MouseState currentMouseState, previousMouseState;
        protected bool isLeftMouseDown;

        protected int counter;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            actionManager = ActionManager.Instance;

            debugMessage = "Click to start test!";
            isLeftMouseDown = false;
            counter = 0;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            animationTexture = Content.Load<Texture2D>("Graphics\\walk_anim");
            spriteTexture = Content.Load<Texture2D>("Graphics\\walk");
            debugFont = Content.Load<SpriteFont>("Arial");

            animation = Sprite.Create(animationTexture, 184, 325, 8, 60, true, new Vector2(GraphicsDevice.Viewport.Width * 0.33f, GraphicsDevice.Viewport.Height * 0.5f));
            sprite = Sprite.Create(spriteTexture, new Vector2(GraphicsDevice.Viewport.Width * 0.66f, GraphicsDevice.Viewport.Height * 0.5f));
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            UpdateMouse();

            actionManager.update(gameTime.ElapsedGameTime.Milliseconds / 1000.0f);

            animation.Update(gameTime);
            sprite.Update(gameTime);

            base.Update(gameTime);
        }

        private void UpdateMouse()
        {
            previousMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();
            if (currentMouseState.LeftButton == ButtonState.Pressed)
            {
                isLeftMouseDown = true;
            }

            if (isLeftMouseDown && currentMouseState.LeftButton == ButtonState.Released)
            {
                isLeftMouseDown = false;
                RunTestAction();
            }
        }

        private void RunTestAction()
        {
            // reset everything
            actionManager.removeAllActionsFromTarget(sprite);
            actionManager.removeAllActionsFromTarget(animation);
            animation.Position = new Vector2(GraphicsDevice.Viewport.Width * 0.33f, GraphicsDevice.Viewport.Height * 0.5f);
            sprite.Position = new Vector2(GraphicsDevice.Viewport.Width * 0.66f, GraphicsDevice.Viewport.Height * 0.5f);
            animation.Rotation = 0.0f;
            sprite.Rotation = 0.0f;
            animation.Scale = 1.0f;
            sprite.Scale = 1.0f;

            switch (++ counter)
            {
                case 1:
                    debugMessage = "MoveBy";
                    actionManager.addAction(MoveBy.create(1.0f, new Vector2(GraphicsDevice.Viewport.Width * -0.1f, GraphicsDevice.Viewport.Height * 0.1f)), sprite);
                    break;

                case 2:
                    debugMessage = "MoveTo";
                    actionManager.addAction(MoveTo.create(1.0f, new Vector2(GraphicsDevice.Viewport.Width * 0.5f, GraphicsDevice.Viewport.Height * 0.75f)), animation);
                    break;

                case 3:
                    debugMessage = "RotateBy";
                    actionManager.addAction(RotateBy.create(1.0f, Helper.Helper.DegreesToRadians(720.0f)), sprite);
                    break;

                case 4:
                    debugMessage = "RotateTo";
                    actionManager.addAction(RotateTo.create(1.0f, Helper.Helper.DegreesToRadians(360.0f)), animation);
                    break;

                case 5:
                    debugMessage = "ScaleBy";
                    actionManager.addAction(ScaleBy.create(1.0f, 0.5f), sprite);
                    break;

                case 6:
                    debugMessage = "ScaleTo";
                    actionManager.addAction(ScaleTo.create(1.0f, 1.5f), animation);
                    break;

                default:
                    counter = 0;
                    debugMessage = "Click to start test!";
                    break;
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            animation.Draw(spriteBatch);
            sprite.Draw(spriteBatch);

            spriteBatch.DrawString(debugFont, debugMessage, new Vector2(GraphicsDevice.Viewport.Width * 0.5f, GraphicsDevice.Viewport.Height * 0.1f), Color.LightGreen,
                0, debugFont.MeasureString(debugMessage) / 2, 1.0f, SpriteEffects.None, 0.5f);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
