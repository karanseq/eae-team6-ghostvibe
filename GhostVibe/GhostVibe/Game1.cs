using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Helper;
using System.Diagnostics;
using System.Collections.Generic;
using System;

namespace GhostVibe
{
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        ActionManager actionManager;
        Scheduler scheduler;
        UpdateDelegate delegateUpdate01, delegateUpdate02, delegateUpdate03;
        
        List<Ghost> ghostList;

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
            this.IsMouseVisible = true;
            Content.RootDirectory = "Content";
        }
        
        protected override void Initialize()
        {
            actionManager = ActionManager.Instance;
            scheduler = Scheduler.Instance;
            Helper.Helper.ViewportWidth = GraphicsDevice.Viewport.Width;
            Helper.Helper.ViewportHeight = GraphicsDevice.Viewport.Height;

            debugMessage = "Click to start test!";
            isLeftMouseDown = false;
            counter = 0;
            

            base.Initialize();
        }
        
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // this.Content to load your game content here
            animationTexture = Content.Load<Texture2D>("Graphics\\walk_anim");
            spriteTexture = Content.Load<Texture2D>("Graphics\\walk");
            debugFont = Content.Load<SpriteFont>("Arial");

            HapticFeedback.startBeats(0.5f, 0.1f, 0.1f);

            ghostList = new List<Ghost>();
            ghostList.Add(new Ghost(animationTexture, new Vector2(GraphicsDevice.Viewport.Width * 0.50f, GraphicsDevice.Viewport.Height * 0.20f), 184, 325, 8, 0.20f, "Blue"));
            //ghostList.Add(new Ghost(animationTexture, new Vector2(GraphicsDevice.Viewport.Width * 0.50f, GraphicsDevice.Viewport.Height * 0.60f), 184, 325, 8, 0.20f, "Blue"));
            //ghostList.Add(new Ghost(animationTexture, new Vector2(GraphicsDevice.Viewport.Width * 0.50f, GraphicsDevice.Viewport.Height * 0.40f), 184, 325, 8, 0.20f, "Blue"));
            //ghostList.Add(new Ghost(animationTexture, new Vector2(GraphicsDevice.Viewport.Width * 0.50f, GraphicsDevice.Viewport.Height * 0.20f), 184, 325, 8, 0.20f, "Blue"));
        }


        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            UpdateMouse();
            UpdateGhosts(gameTime);

            actionManager.update(gameTime.ElapsedGameTime.Milliseconds / 1000.0f);
            scheduler.update(gameTime.ElapsedGameTime.Milliseconds / 1000.0f);

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
                HapticFeedback.stopBeats();
            }
        }

        private void UpdateGhosts(GameTime gameTime)
        {
            foreach (Ghost ghost in ghostList)
            {
                ghost.Update(gameTime);
                ghost.MoveForward(1.0f);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            for(int i = 0; i < ghostList.Count; i++)
            {
                ghostList[i].Activate();
                ghostList[i].Draw(spriteBatch);
            }

            spriteBatch.DrawString(debugFont, debugMessage, new Vector2(GraphicsDevice.Viewport.Width * 0.5f, GraphicsDevice.Viewport.Height * 0.1f), Color.LightGreen,
                0, debugFont.MeasureString(debugMessage) / 2, 1.0f, SpriteEffects.None, 0.5f);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
