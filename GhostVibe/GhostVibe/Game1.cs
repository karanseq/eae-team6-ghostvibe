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

        protected Texture2D animationTexture, spriteTexture, dummyTexture;

        HashSet<Ghost> ghostSet;

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
        protected int score;
        protected int lifeRemaining;

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
            
            isLeftMouseDown = false;
            isSpaceKeyPressed = false;
            isAKeyPressed = false;

            counter = 0;
            score = 0;
            lifeRemaining = 3;
			
            base.Initialize();
        }
        
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            //animationTexture = Content.Load<Texture2D>("ghost_02");
            spriteTexture = Content.Load<Texture2D>("ghost_01");

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

            ghostSet = new HashSet<Ghost>();
            ghostSet.Add(new Ghost(spriteTexture, 0.5f, "Blue"));
            ghostSet.Add(new Ghost(spriteTexture, 0.5f, "Blue"));
            ghostSet.Add(new Ghost(spriteTexture, 0.5f, "Blue"));
            ghostSet.Add(new Ghost(spriteTexture, 0.5f, "Blue"));
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
                // HapticFeedback.stopBeats();

                HashSet<Ghost> toBeDeleted = new HashSet<Ghost>();

                foreach (Ghost ghost in ghostSet)
                {
                    if (ghost.GetStage() == 1 || ghost.GetStage() == 2)
                        ghost.MoveForward(1.0f);
                    else
                    {
                        toBeDeleted.Add(ghost);
                        ghost.Destroy();
                    }
                }
                ghostSet.ExceptWith(toBeDeleted);
            }
        }

        private void UpdateGhosts(GameTime gameTime)
        {
            foreach (Ghost ghost in ghostSet)
            {
                ghost.Update(gameTime);
            }
        }

        private void DrawUI()
        {
            spriteBatch.DrawString(debugFont, "Score: " + score, new Vector2(20, 20), Color.White, 0.0f, new Vector2(0, 0), 2.0f, SpriteEffects.None, 1.0f);
            spriteBatch.DrawString(debugFont, "Life: " + lifeRemaining, new Vector2(20, GraphicsDevice.Viewport.Height - 50), Color.White, 0.0f, new Vector2(0, 0), 2.0f, SpriteEffects.None, 1.0f);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            foreach(Ghost ghost in ghostSet)
            {
                ghost.Activate();
                ghost.Draw(spriteBatch);
            }

            beatIndicatorSprite.Opacity = (songManager.IsAcceptingKeys) ? 1.0f : 0.2f;
            //beatIndicatorSprite.Draw(spriteBatch);
            //resultIndicatorSprite.Draw(spriteBatch);
            
            DrawUI();

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
