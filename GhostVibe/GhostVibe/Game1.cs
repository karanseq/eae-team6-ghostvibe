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

        protected Texture2D animationTexture, spriteTexture;
        protected Texture2D hallway;
        protected Texture2D blueGun, greenGun, redGun, yellowGun;
        protected SpriteFont arialFont;

        protected readonly int maxColors = 4;
        protected string[] colorNames = { "plain", "blue", "green", "red", "yellow" };
        protected Dictionary<string, Keys> colorKeyMap = new Dictionary<string, Keys>(){ 
            { "blue", Keys.X },
            { "green", Keys.A },
            { "red", Keys.B },
            { "yellow", Keys.Y } };

        protected Dictionary<string, Texture2D> ghostTextures;
        protected List<Ghost> ghostList;
        protected UpdateDelegate delegateTickGhosts, delegateTickClock;
        protected static float beatFrequency;

        // mouse states
        protected MouseState currentMouseState, previousMouseState;
        protected bool isLeftMouseDown;
        
        // keyboard states
        private KeyboardState currentKeyboardState, previousKeyboardState;
        private bool acceptKeys;

        // gamepad states
        private GamePadState currentGamepadState, previousGamepadState;

        protected int score, streak, multiplier;
        protected int lifeRemaining;
        public Random random;

        // audio objects
        protected SoundEffect ghostPoof;
        protected SoundEffect ghostSpawn;
        protected SoundEffect whistle;
        protected SoundEffect bgm;
        protected SoundEffectInstance bgmInst;

        protected SoundEffect bass;
        protected SoundEffect tom;
        protected SoundEffect cymbal;
        protected SoundEffect snare;
        protected SoundEffect hihat;

        // rhythms
        int seconds, currentDifficultyIndex;
        protected int index = -1;
        protected List<int> rhythm;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;

            //graphics.IsFullScreen = true;
            //IsMouseVisible = true;

            Content.RootDirectory = "Content";
        }
        
        protected override void Initialize()
        {
            actionManager = ActionManager.Instance;
            scheduler = Scheduler.Instance;
            Helper.Helper.ViewportWidth = GraphicsDevice.Viewport.Width;
            Helper.Helper.ViewportHeight = GraphicsDevice.Viewport.Height;

            delegateTickClock = new UpdateDelegate(TickClock);
            delegateTickGhosts = new UpdateDelegate(TickGhosts);

            base.Initialize();
        }
        
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            arialFont = Content.Load<SpriteFont>("Arial");
            ghostPoof = Content.Load<SoundEffect>("ghostpoof");
            ghostSpawn = Content.Load<SoundEffect>("newghostspawn");
            whistle = Content.Load<SoundEffect>("trainwhistle");
            bgm = Content.Load<SoundEffect>("newbgmsize");
            bgmInst = bgm.CreateInstance();
            bass = Content.Load<SoundEffect>("Bass");
            tom = Content.Load<SoundEffect>("tom");
            cymbal = Content.Load<SoundEffect>("cymbal");
            snare = Content.Load<SoundEffect>("snaredrum");
            hihat = Content.Load<SoundEffect>("hi-hat_2");
            hallway = Content.Load<Texture2D>("newhallway");
            blueGun = Content.Load<Texture2D>("blue");
            yellowGun = Content.Load<Texture2D>("yellow");
            greenGun = Content.Load<Texture2D>("green");
            redGun = Content.Load<Texture2D>("red");

            ghostTextures = new Dictionary<string, Texture2D>();
            ghostTextures.Add("plain", Content.Load<Texture2D>("ghost_01"));
            ghostTextures.Add("blue", Content.Load<Texture2D>("ghost_02"));
            ghostTextures.Add("green", Content.Load<Texture2D>("ghost_03"));
            ghostTextures.Add("red", Content.Load<Texture2D>("ghost_04"));
            ghostTextures.Add("yellow", Content.Load<Texture2D>("ghost_05"));

            StartGame();
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected void StartGame()
        {
            random = new Random();

            // start the clock
            seconds = 0;
            scheduler.scheduleDelegate(delegateTickClock, 1.0f);

            // generate first rhythm
            currentDifficultyIndex = 0;
            beatFrequency = 0.4f;
            rhythm = Helper.Helper.GenerateRhythm(currentDifficultyIndex, beatFrequency, random);

            // initialize score and health
            score = 0;
            streak = 0;
            multiplier = 1;
            lifeRemaining = 3;

            isLeftMouseDown = acceptKeys = false;

            // start scheduled functions
            HapticFeedback.startBeats(beatFrequency, 0.1f, 0.1f);
            scheduler.scheduleDelegate(delegateTickGhosts, beatFrequency);

            bgmInst.Volume = 0.3f;
            bgmInst.IsLooped = true;
            bgmInst.Play();

            ghostList = new List<Ghost>();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            UpdateKeyboardGamepad();
            UpdateMouse();
            UpdateGhosts(gameTime);

            actionManager.update(gameTime.ElapsedGameTime.Milliseconds * 0.001f);
            scheduler.update(gameTime.ElapsedGameTime.Milliseconds * 0.001f);

            base.Update(gameTime);
        }

        private void UpdateKeyboardGamepad()
        {
            previousKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();

            previousGamepadState = currentGamepadState;
            currentGamepadState = GamePad.GetState(PlayerIndex.One);

            if (acceptKeys && (currentKeyboardState.IsKeyDown(Keys.D) || currentGamepadState.IsButtonDown(Buttons.A)))
            {
                ShootGhost(Keys.A);
                snare.Play();
            }

            if (acceptKeys && (currentKeyboardState.IsKeyDown(Keys.F) || currentGamepadState.IsButtonDown(Buttons.B)))
            {
                ShootGhost(Keys.B);
                hihat.Play();
            }

            if (acceptKeys && (currentKeyboardState.IsKeyDown(Keys.J) || currentGamepadState.IsButtonDown(Buttons.X)))
            {
                ShootGhost(Keys.X);
                tom.Play();
            }

            if (acceptKeys && (currentKeyboardState.IsKeyDown(Keys.K) || currentGamepadState.IsButtonDown(Buttons.Y)))
            {
                ShootGhost(Keys.Y);
                cymbal.Play();
            }

            if (!acceptKeys)
            {
                acceptKeys = (currentKeyboardState.IsKeyUp(Keys.D) && currentGamepadState.IsButtonUp(Buttons.A) &&
                    currentKeyboardState.IsKeyUp(Keys.F) && currentGamepadState.IsButtonUp(Buttons.B) &&
                    currentKeyboardState.IsKeyUp(Keys.J) && currentGamepadState.IsButtonUp(Buttons.X) &&
                    currentKeyboardState.IsKeyUp(Keys.K) && currentGamepadState.IsButtonUp(Buttons.Y));
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
            }
        }

        private void UpdateGhosts(GameTime gameTime)
        {
            if (ghostList.Count == 0) return;

            List<Ghost> ghostsToBeDeleted = new List<Ghost>();
            foreach (Ghost ghost in ghostList)
            {
                ghost.Update(gameTime);
                if (ghost.HasFinishedDying)
                {
                    ghost.Destroy();
                    ghostsToBeDeleted.Add(ghost);
                }
            }

            foreach (Ghost ghost in ghostsToBeDeleted)
            {
                // if ghost was not killed by the player, reset the streak and multiplier
                if (!ghost.WasKilledByPlayer)
                {
                    streak = 0;
                    multiplier = 1;
                }

                ghostList.Remove(ghost);
            }
        }

        private void DrawUI()
        {
            spriteBatch.DrawString(arialFont, "Score: " + score, new Vector2(GraphicsDevice.Viewport.Width / 2 - 400, 27), Color.Black, 0.0f, Vector2.Zero, 2.0f, SpriteEffects.None, 1.0f);
            //spriteBatch.DrawString(arialFont, "Life: " + lifeRemaining, new Vector2(20, GraphicsDevice.Viewport.Height - 50), Color.Black, 0.0f, Vector2.Zero, 2.0f, SpriteEffects.None, 1.0f);
            spriteBatch.DrawString(arialFont, "Green: D, Red: F, Blue: J, Yellow: K", new Vector2(GraphicsDevice.Viewport.Width / 2 - 100, 27), Color.Black, 0.0f, Vector2.Zero, 2.0f, SpriteEffects.None, 1.0f);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            spriteBatch.Draw(hallway, Vector2.Zero, null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f);
            //spriteBatch.Draw(blueGun, new Vector2(GraphicsDevice.Viewport.Width * 0.1f, GraphicsDevice.Viewport.Height * 0.97f), null, Color.White, Helper.Helper.DegreesToRadians(45.0f), new Vector2(blueGun.Width / 2, blueGun.Height / 2), 0.65f, SpriteEffects.None, 0.0f);
            //spriteBatch.Draw(yellowGun, new Vector2(GraphicsDevice.Viewport.Width * 0.9f, GraphicsDevice.Viewport.Height * 0.97f), null, Color.White, Helper.Helper.DegreesToRadians(-45.0f), new Vector2(blueGun.Width / 2, blueGun.Height / 2), 0.65f, SpriteEffects.None, 0.0f);
            //spriteBatch.Draw(redGun, new Vector2(GraphicsDevice.Viewport.Width * 0.35f, GraphicsDevice.Viewport.Height), null, Color.White, Helper.Helper.DegreesToRadians(25.0f), new Vector2(blueGun.Width / 2, blueGun.Height / 2), 0.65f, SpriteEffects.None, 0.0f);
            //spriteBatch.Draw(greenGun, new Vector2(GraphicsDevice.Viewport.Width * 0.65f, GraphicsDevice.Viewport.Height), null, Color.White, Helper.Helper.DegreesToRadians(-25.0f), new Vector2(blueGun.Width / 2, blueGun.Height / 2), 0.65f, SpriteEffects.None, 0.0f);

            if (ghostList.Count != 0)
            {
                //foreach (Ghost ghost in ghostList)
                for (int i = ghostList.Count - 1; i >= 0; --i)
                {
                    ghostList[i].Activate();
                    ghostList[i].Draw(spriteBatch);
                }
            }

            DrawUI();

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void TickClock(float deltaTime)
        {
            ++seconds;
            if (seconds >= Helper.Helper.difficultyTimeMatrix[currentDifficultyIndex])
            {
                ++currentDifficultyIndex;
                currentDifficultyIndex = currentDifficultyIndex > Helper.Helper.maxDifficulty ? currentDifficultyIndex - 1 : currentDifficultyIndex;
                rhythm.Clear();
                rhythm = Helper.Helper.GenerateRhythm(currentDifficultyIndex, beatFrequency, random);
                Trace.WriteLine("Difficulty upgraded to " + currentDifficultyIndex + "...");
            }
        }

        private void TickGhosts(float deltaTime)
        {
            // move to the next note in the rhythm
            ++index;

            // check if its time to get a new rhythm
            if (index >= rhythm.Count)
            {
                index = 0;
                rhythm = Helper.Helper.GenerateRhythm(currentDifficultyIndex, beatFrequency, random);
            }            

            // check if there is indeed a note on this beat
            if (rhythm[index] != 0)
            {
                SpawnGhost(rhythm[index] - 1);
            }
        }

        private void SpawnGhost(int laneNumber)
        {
            Ghost ghost = new Ghost(ghostTextures["plain"], laneNumber, 0.3f, "");
            ghostList.Add(ghost);
            ghost.MoveForward(beatFrequency * 2.5f);
        }

        private void ShootGhost(Keys keyPressed)
        {
            acceptKeys = false;

            // first get the lane number based on the key pressed
            int laneNumber = 0;
            switch (keyPressed)
            {
                case Keys.A:
                    laneNumber = 0;
                    break;
                case Keys.B:
                    laneNumber = 1;
                    break;
                case Keys.X:
                    laneNumber = 2;
                    break;
                case Keys.Y:
                    laneNumber = 3;
                    break;
            }

            // loop all current ghosts to see if there is a ghost in the shooting range for this lane
            for (int i = 0; i < ghostList.Count; ++i)
            {
                Ghost ghost = ghostList[i];

                // the ghost needs to be in the right lane AND in the shooting range AND not already killed
                if (ghost.LaneNumber == laneNumber && ghost.IsInShootingRange && !ghost.IsDying)
                {
                    // kill the ghost
                    ghost.Die(true);

                    // update streak
                    ++streak;

                    // check if the multiplier needs to be upgraded
                    for (int j = Helper.Helper.numMultipliers - 1; j >= multiplier - 1; --j)
                    {
                        // check if streak has exceeded the required value
                        if (streak >= Helper.Helper.multiplier[j])
                        {
                            // increase multiplier
                            multiplier = j + 2;
                            // reset streak
                            streak = 0;
                            Trace.WriteLine("Multiplier upgraded to " + multiplier + "...");
                            break;
                        }
                    }

                    // update score
                    score += 10 * multiplier;
                    return;
                }
            }

            streak = 0;
            multiplier = 1;
        }

        public static float BeatFrequency
        {
            get { return Game1.beatFrequency; }
        }

    }
}
