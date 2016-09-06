using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Helper;
using System.Diagnostics;
using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace GhostVibe
{
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        protected ActionManager actionManager;
        protected Scheduler scheduler;
        protected bool isPaused;

        protected Texture2D animationTexture, spriteTexture;
        protected Texture2D hallway;
        //protected Texture2D blueGun, greenGun, redGun, yellowGun;
        protected SpriteFont UIFont;

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
        protected SoundEffect bgm;
        protected SoundEffectInstance bgmInst;

        protected SoundEffect A;
        protected SoundEffect C;
        protected SoundEffect E;
        protected SoundEffect highA;
        protected SoundEffect positive;
        protected SoundEffect negative;

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
            IsMouseVisible = true;

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

            isPaused = true;

            base.Initialize();
        }
        
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            UIFont = Content.Load<SpriteFont>("interface");
            bgm = Content.Load<SoundEffect>("newbgmsize");
            bgmInst = bgm.CreateInstance();
            A = Content.Load<SoundEffect>("A2");
            C = Content.Load<SoundEffect>("C2");
            E = Content.Load<SoundEffect>("E2");
            highA = Content.Load<SoundEffect>("highA2");
            positive = Content.Load<SoundEffect>("happysound");
            negative = Content.Load<SoundEffect>("badsound");
            hallway = Content.Load<Texture2D>("newhallway");
            //blueGun = Content.Load<Texture2D>("blue");
            //yellowGun = Content.Load<Texture2D>("yellow");
            //greenGun = Content.Load<Texture2D>("green");
            //redGun = Content.Load<Texture2D>("red");

            ghostTextures = new Dictionary<string, Texture2D>();
            ghostTextures.Add("plain", Content.Load<Texture2D>("ghost_01"));
            ghostTextures.Add("blue", Content.Load<Texture2D>("ghost_blue"));
            ghostTextures.Add("green", Content.Load<Texture2D>("ghost_green"));
            ghostTextures.Add("red", Content.Load<Texture2D>("ghost_red"));
            ghostTextures.Add("yellow", Content.Load<Texture2D>("ghost_yellow"));

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
            beatFrequency = 0.3f;
            rhythm = Helper.Helper.GenerateRhythm(currentDifficultyIndex, beatFrequency, random);

            // initialize score and health
            score = 0;
            streak = 0;
            multiplier = 1;
            lifeRemaining = 10;

            isLeftMouseDown = acceptKeys = false;

            // start scheduled functions
            HapticFeedback.startBeats(beatFrequency, 0.1f, 0.1f);
            scheduler.scheduleDelegate(delegateTickGhosts, beatFrequency);

            bgmInst.Volume = 1.0f;
            bgmInst.IsLooped = true;
            bgmInst.Play();

            ghostList = new List<Ghost>();

            isPaused = false;
    }

        protected override void Update(GameTime gameTime)
        {
            // always listen for events
            UpdateKeyboardGamepad();

            if (!isPaused)
            {
                UpdateMouse();
                UpdateGhosts(gameTime);

                actionManager.update(gameTime.ElapsedGameTime.Milliseconds * 0.001f);
                scheduler.update(gameTime.ElapsedGameTime.Milliseconds * 0.001f);
            }

            base.Update(gameTime);
        }

        private void UpdateKeyboardGamepad()
        {
            previousKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();

            previousGamepadState = currentGamepadState;
            currentGamepadState = GamePad.GetState(PlayerIndex.One);

            if (acceptKeys && (currentKeyboardState.IsKeyDown(Keys.Escape) || currentGamepadState.IsButtonDown(Buttons.Back)))
            {
                acceptKeys = false;
                isPaused = !isPaused;
            }

            Keys keyPressed = Keys.None;
            if (acceptKeys && (currentKeyboardState.IsKeyDown(Keys.D) || currentGamepadState.IsButtonDown(Buttons.LeftTrigger)))
            {
                keyPressed = Keys.A;
            }
            else if (acceptKeys && (currentKeyboardState.IsKeyDown(Keys.F) || currentGamepadState.IsButtonDown(Buttons.LeftShoulder)))
            {
                keyPressed = Keys.B;
            }
            else if (acceptKeys && (currentKeyboardState.IsKeyDown(Keys.J) || currentGamepadState.IsButtonDown(Buttons.RightShoulder)))
            {
                keyPressed = Keys.X;
            }
            else if (acceptKeys && (currentKeyboardState.IsKeyDown(Keys.K) || currentGamepadState.IsButtonDown(Buttons.RightTrigger)))
            {
                keyPressed = Keys.Y;
            }

            // only forward the event if the game is not paused
            if (!isPaused && keyPressed != Keys.None)
            {
                ShootGhost(keyPressed);
            }

            if (!acceptKeys)
            {
                acceptKeys = (currentKeyboardState.IsKeyUp(Keys.Escape) && currentGamepadState.IsButtonUp(Buttons.Back) && 
                    currentKeyboardState.IsKeyUp(Keys.D) && currentGamepadState.IsButtonUp(Buttons.LeftTrigger) &&
                    currentKeyboardState.IsKeyUp(Keys.F) && currentGamepadState.IsButtonUp(Buttons.LeftShoulder) &&
                    currentKeyboardState.IsKeyUp(Keys.J) && currentGamepadState.IsButtonUp(Buttons.RightShoulder) &&
                    currentKeyboardState.IsKeyUp(Keys.K) && currentGamepadState.IsButtonUp(Buttons.RightTrigger));
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
                    // deduct life
                    --lifeRemaining;
                    // TODO: remove the '<=' from below condition
                    if (lifeRemaining <= 0)
                    {
                        // TODO: remove the following statement
                        lifeRemaining = 0;

                        // stop everything!
                        scheduler.unscheduleDelegate(delegateTickClock);
                        scheduler.unscheduleDelegate(delegateTickGhosts);
                        HapticFeedback.stopBeats();
                    }

                    // reset streak and multiplier
                    streak = 0;
                    multiplier = 1;
                }

                ghostList.Remove(ghost);
            }
        }

        private void DrawUI()
        {
            spriteBatch.DrawString(UIFont, "Score: " + score, new Vector2(GraphicsDevice.Viewport.Width / 2 - 400, 30), Color.Blue, 0.0f, Vector2.Zero, 2.0f, SpriteEffects.None, 1.0f);
            spriteBatch.DrawString(UIFont, "Life: " + lifeRemaining, new Vector2(20, GraphicsDevice.Viewport.Height - 50), Color.Purple, 0.0f, Vector2.Zero, 2.0f, SpriteEffects.None, 1.0f);
            spriteBatch.DrawString(UIFont, "Green: D, Red: F, Blue: J, Yellow: K", new Vector2(GraphicsDevice.Viewport.Width / 2 - 100, 30), Color.ForestGreen, 0.0f, Vector2.Zero, 2.0f, SpriteEffects.None, 1.0f);
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
            Ghost ghost = new Ghost(ghostTextures[GetGhostColor(laneNumber)], laneNumber, 0.3f, "");
            ghostList.Add(ghost);
            ghost.MoveForward(beatFrequency * 2.5f);
        }

        private void ShootGhost(Keys keyPressed)
        {
            // stop listening for keys
            acceptKeys = false;

            // first get the lane number based on the key pressed
            int laneNumber = 0;
            switch (keyPressed)
            {
                case Keys.A:
                    laneNumber = 0;
                    A.Play();
                    break;
                case Keys.B:
                    laneNumber = 1;
                    C.Play();
                    break;
                case Keys.X:
                    laneNumber = 2;
                    E.Play();
                    break;
                case Keys.Y:
                    laneNumber = 3;
                    highA.Play();
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

                    // play positive sound here
                    switch(keyPressed)
                    {
                        case Keys.A:
                            A.Play();
                            break;
                        case Keys.B:
                            C.Play();
                            break;
                        case Keys.X:
                            E.Play();
                            break;
                        case Keys.Y:
                            highA.Play();
                            break;
                    }
                    return;
                }
            }
            
            // reset the streak and multiplier
            streak = 0;
            multiplier = 1;

            // play sound when player misses ghost
            negative.Play();
        }

        public static float BeatFrequency
        {
            get { return Game1.beatFrequency; }
        }

        public string GetGhostColor(int laneNumber)
        {
            if (laneNumber == 0)
            {
                return "green";
            }
            else if (laneNumber == 1)
            {
                return "red";
            }
            else if (laneNumber == 2)
            {
                return "blue";
            }
            else if (laneNumber == 3)
            {
                return "yellow";
            }
            else
                return "plain";
        }

    }
}
