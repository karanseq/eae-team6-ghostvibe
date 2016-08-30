using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Helper;
using System.Diagnostics;
using System.Collections.Generic;
using System;

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
        Scheduler scheduler;
        UpdateDelegate delegateUpdate01, delegateUpdate02, delegateUpdate03;

        UpdateDelegate delegateStartVibration, delegateStopVibration;
        bool forLeftMotor;
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

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
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

            HapticFeedback.startBeats(0.5f, 0.1f, 0.1f);

            //animation = Sprite.Create(animationTexture, 184, 325, 8, 60, true, new Vector2(GraphicsDevice.Viewport.Width * 0.33f, GraphicsDevice.Viewport.Height * 0.5f));
            //sprite = Sprite.Create(spriteTexture, new Vector2(GraphicsDevice.Viewport.Width * 0.66f, GraphicsDevice.Viewport.Height * 0.5f));

            ghostList = new List<Ghost>();
            ghostList.Add(new Ghost(animationTexture, new Vector2(GraphicsDevice.Viewport.Width * 0.50f, GraphicsDevice.Viewport.Height * 0.20f), 184, 325, 8, 0.20f, "Blue"));
            //ghostList.Add(new Ghost(animationTexture, new Vector2(GraphicsDevice.Viewport.Width * 0.50f, GraphicsDevice.Viewport.Height * 0.60f), 184, 325, 8, 0.20f, "Blue"));
            //ghostList.Add(new Ghost(animationTexture, new Vector2(GraphicsDevice.Viewport.Width * 0.50f, GraphicsDevice.Viewport.Height * 0.40f), 184, 325, 8, 0.20f, "Blue"));
            //ghostList.Add(new Ghost(animationTexture, new Vector2(GraphicsDevice.Viewport.Width * 0.50f, GraphicsDevice.Viewport.Height * 0.20f), 184, 325, 8, 0.20f, "Blue"));
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
            UpdateGhosts(gameTime);

            actionManager.update(gameTime.ElapsedGameTime.Milliseconds / 1000.0f);
            scheduler.update(gameTime.ElapsedGameTime.Milliseconds / 1000.0f);

            //animation.Update(gameTime);
            //sprite.Update(gameTime);

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
                //RunTestAction();
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
                //case 1:
                //    debugMessage = "Scheduling testUpdate01...";
                //    delegateUpdate01 = new UpdateDelegate(testUpdate01);
                //    scheduler.scheduleDelegate(delegateUpdate01, 1.0f);
                //    break;

                //case 2:
                //    debugMessage = "Unscheduling testUpdate01";
                //    scheduler.unscheduleDelegate(delegateUpdate01);
                //    break;

                //case 3:
                //    debugMessage = "Scheduling testUpdate02 to start right now and repeat thrice...";
                //    delegateUpdate02 = new UpdateDelegate(testUpdate02);
                //    scheduler.scheduleDelegate(delegateUpdate02, 0.1f, 3, 0.0f);
                //    break;

                //case 4:
                //    debugMessage = "Uncheduling testUpdate02";
                //    scheduler.unscheduleDelegate(delegateUpdate02);
                //    break;

                //case 5:
                //    debugMessage = "Scheduling testUpdate03 to start in two seconds and to repeat thrice...";
                //    delegateUpdate03 = new UpdateDelegate(testUpdate03);
                //    scheduler.scheduleDelegate(delegateUpdate03, 0.5f, 3, 2.0f);
                //    break;

                //case 6:
                //    debugMessage = "Unscheduling testUpdate03";
                //    scheduler.unscheduleDelegate(delegateUpdate03);
                //    break;

                //case 1:
                //    debugMessage = "MoveBy";
                //    actionManager.addAction(MoveBy.create(1.0f, new Vector2(GraphicsDevice.Viewport.Width * -0.1f, GraphicsDevice.Viewport.Height * 0.1f)), sprite);
                //    break;

                //case 2:
                //    debugMessage = "MoveTo";
                //    actionManager.addAction(MoveTo.create(1.0f, new Vector2(GraphicsDevice.Viewport.Width * 0.5f, GraphicsDevice.Viewport.Height * 0.75f)), animation);
                //    break;

                //case 3:
                //    debugMessage = "RotateBy";
                //    actionManager.addAction(RotateBy.create(1.0f, Helper.Helper.DegreesToRadians(720.0f)), sprite);
                //    break;

                //case 4:
                //    debugMessage = "RotateTo";
                //    actionManager.addAction(RotateTo.create(1.0f, Helper.Helper.DegreesToRadians(360.0f)), animation);
                //    break;

                //case 5:
                //    debugMessage = "ScaleBy";
                //    actionManager.addAction(ScaleBy.create(1.0f, 0.5f), sprite);
                //    break;

                //case 6:
                //    debugMessage = "ScaleTo";
                //    actionManager.addAction(ScaleTo.create(1.0f, 1.5f), animation);
                //    break;

                default:
                    counter = 0;
                    debugMessage = "Click to start test!";
                    break;
            }
        }

        public void testUpdate01(float deltaTime)
        {
            Trace.WriteLine("In testUpdate01...");
        }

        public void testUpdate02(float deltaTime)
        {
            Trace.WriteLine("In testUpdate02...");
        }

        public void testUpdate03(float deltaTime)
        {
            Trace.WriteLine("In testUpdate03...");
        }

        public void startVibration(float deltaTime)
        {
            GamePad.SetVibration(PlayerIndex.One, 0.25f, 0.25f);
        }

        public void stopVibration(float deltaTime)
        {
            GamePad.SetVibration(PlayerIndex.One, 0.0f, 0.0f);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            //animation.Draw(spriteBatch);
            //sprite.Draw(spriteBatch);
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
