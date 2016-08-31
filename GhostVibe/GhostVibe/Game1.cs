using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Helper;
using System.Diagnostics;
using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework.Audio;

namespace GhostVibe
{
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        ActionManager actionManager;
        Scheduler scheduler;
        SongManager songManager;
        
        List<Ghost> ghostList;
        bool mustRemoveGhost;

        protected Texture2D animationTexture, spriteTexture, dummyTexture;

        // testing player-rhythm-keypress
        protected Sprite beatIndicatorSprite, resultIndicatorSprite;
        protected UpdateDelegate delegateResultColorReset;
        protected UpdateDelegate delegateSpawnGhosts;

        // mouse states
        protected MouseState currentMouseState, previousMouseState;
        protected bool isLeftMouseDown;
        
        // keyboard states used to determine keyboard states
        private KeyboardState currentKeyboardState, previousKeyboardState;
        private bool isSpaceKeyPressed, isAKeyPressed;

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
            songManager = SongManager.Instance;
            Helper.Helper.ViewportWidth = GraphicsDevice.Viewport.Width;
            Helper.Helper.ViewportHeight = GraphicsDevice.Viewport.Height;
            
            isLeftMouseDown = false;
            isSpaceKeyPressed = false;
            isAKeyPressed = false;
            delegateResultColorReset = new UpdateDelegate(ResetResultColor);
            delegateSpawnGhosts = new UpdateDelegate(SpawnGhost);

            base.Initialize();
        }
        
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // this.Content to load your game content here
            animationTexture = Content.Load<Texture2D>("Graphics\\walk_anim");
            spriteTexture = Content.Load<Texture2D>("Graphics\\walk");

            songManager.SoundEffect = Content.Load<SoundEffect>("Audio\\02");
            songManager.SoundEngine = songManager.SoundEffect.CreateInstance();

            beatIndicatorSprite = Sprite.Create(Content.Load<Texture2D>("Graphics\\1"), new Vector2(GraphicsDevice.Viewport.Width * 0.5f, GraphicsDevice.Viewport.Height * 0.5f));
            beatIndicatorSprite.Scale = 0.25f;
            resultIndicatorSprite = Sprite.Create(Content.Load<Texture2D>("Graphics\\2"), new Vector2(GraphicsDevice.Viewport.Width * 0.5f, GraphicsDevice.Viewport.Height * 0.5f));
            resultIndicatorSprite.Opacity = 0.2f;
            resultIndicatorSprite.Scale = 0.25f;

            //HapticFeedback.startBeats(0.5f, 0.1f, 0.1f);

            ghostList = new List<Ghost>();
            mustRemoveGhost = false;
            //ghostList.Add(new Ghost(animationTexture, new Vector2(GraphicsDevice.Viewport.Width * 0.50f, GraphicsDevice.Viewport.Height * 0.20f), 184, 325, 8, 0.20f, "Blue"));
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
            {
                Exit();
            }

            UpdateKeyboard();
            //UpdateMouse();
            UpdateGhosts(gameTime);

            actionManager.update(gameTime.ElapsedGameTime.Milliseconds / 1000.0f);
            scheduler.update(gameTime.ElapsedGameTime.Milliseconds / 1000.0f);

            base.Update(gameTime);
        }

        private void UpdateKeyboard()
        {
            previousKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();

            if (currentKeyboardState.IsKeyDown(Keys.Space)) isSpaceKeyPressed = true;
            if (currentKeyboardState.IsKeyDown(Keys.A)) isAKeyPressed = true;

            if (isSpaceKeyPressed && currentKeyboardState.IsKeyUp(Keys.Space))
            {
                isSpaceKeyPressed = false;
                StartSongTest();
            }

            if (isAKeyPressed && currentKeyboardState.IsKeyUp(Keys.A))
            {
                isAKeyPressed = false;
                resultIndicatorSprite.Color = (songManager.acceptKey(Keys.A)) ? Color.Green : Color.Red;

                mustRemoveGhost = true;
            }
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
            if (mustRemoveGhost)
            {
                mustRemoveGhost = false;
                ghostList.RemoveAt(0);
            }

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

            beatIndicatorSprite.Opacity = (songManager.IsAcceptingKeys) ? 1.0f : 0.2f;
            //beatIndicatorSprite.Draw(spriteBatch);
            //resultIndicatorSprite.Draw(spriteBatch);
            
            spriteBatch.End();

            base.Draw(gameTime);
        }

        protected void StartSongTest()
        {
            songManager.playSong(0);
            float spawnStartDelay = songManager.CurrentSong.StartDelayMilliseconds * 0.001f;
            scheduler.scheduleDelegate(delegateSpawnGhosts, songManager.CurrentSong.BeatFrequencyMilliseconds * 0.001f, Timer.RepeatForever, spawnStartDelay);
            //scheduler.scheduleDelegate(delegateResultColorReset, songManager.CurrentSong.BeatFrequencyMilliseconds * 0.001f);
        }

        protected void SpawnGhost(float deltaTime)
        {
            ghostList.Add(new Ghost(animationTexture, new Vector2(GraphicsDevice.Viewport.Width * 0.50f, GraphicsDevice.Viewport.Height * 0.20f), 184, 325, 8, 0.20f, "Blue"));
        }

        protected void ResetResultColor(float deltaTime)
        {
            resultIndicatorSprite.Color = Color.White;
        }
    }
}
