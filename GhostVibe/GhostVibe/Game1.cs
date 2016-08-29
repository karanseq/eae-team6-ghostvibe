using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using GhostVibe.SimpleGraphics;
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

        Texture2D animationTexture, spriteTexture;
        Sprite animation, sprite;

        // mouse states
        MouseState currentMouseState, previousMouseState;
        bool isLeftMouseDown;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
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

            sprite = new Sprite();
            animation = new Sprite();

            isLeftMouseDown = false;

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

            animation.Initialize(animationTexture, 184, 325, 8, 60, true, new Vector2(GraphicsDevice.Viewport.Width * 0.33f, GraphicsDevice.Viewport.Height * 0.5f));
            sprite.Initialize(spriteTexture, new Vector2(GraphicsDevice.Viewport.Width * 0.66f, GraphicsDevice.Viewport.Height * 0.5f));
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
                actionManager.addAction(RotateBy.create(2.0f, Helper.DegreesToRadians(180.0f)), animation);
                actionManager.addAction(RotateBy.create(1.0f, Helper.DegreesToRadians(90.0f)), sprite);
                actionManager.addAction(MoveBy.create(1.0f, new Vector2(-240, 120)), animation);
                actionManager.addAction(MoveBy.create(2.0f, new Vector2(240, -120)), sprite);
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

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
