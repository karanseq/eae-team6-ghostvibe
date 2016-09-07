﻿using Microsoft.Xna.Framework;
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
        protected bool isGameOver;
        protected string gameoverText;
        protected string pausedText;
        protected string restartText;

        protected Random rand;

        protected int bgmIndicator;

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
        protected Dictionary<string, Texture2D> ghostTextureAnim;
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
        protected ProgressBar lifeBar, streakBar;

        // audio objects
        protected SoundEffect bgm;
        protected SoundEffectInstance bgmInst;

        protected SoundEffect bgm2;
        protected SoundEffectInstance bgmInst2;

        protected List<SoundEffectInstance> bgmList;

        protected SoundEffect A;
        protected SoundEffect C;
        protected SoundEffect E;
        protected SoundEffect highA;
        protected SoundEffect positive;
        protected SoundEffect negative;

        protected SoundEffectInstance positiveInst;

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
            SoundEffect.MasterVolume = 1.0f;
            rand = new Random();

            isPaused = true;
            isGameOver = false;
            gameoverText = "Game Over!";
            restartText = "Play Again? Y/N";
            pausedText = "Paused";

            base.Initialize();
        }
        
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            UIFont = Content.Load<SpriteFont>("interface");
            bgm = Content.Load<SoundEffect>("newbgmsize");
            bgm2 = Content.Load<SoundEffect>("final-battle");
            bgmList = new List<SoundEffectInstance>();
            bgmInst = bgm.CreateInstance();
            bgmInst2 = bgm2.CreateInstance();
            bgmList.Add(bgmInst);
            bgmList.Add(bgmInst2);
            A = Content.Load<SoundEffect>("A2");
            C = Content.Load<SoundEffect>("C2");
            E = Content.Load<SoundEffect>("E2");
            highA = Content.Load<SoundEffect>("highA2");
            positive = Content.Load<SoundEffect>("happysound");
            negative = Content.Load<SoundEffect>("badsound");
            hallway = Content.Load<Texture2D>("hallway_bar");
            positiveInst = positive.CreateInstance();
            //blueGun = Content.Load<Texture2D>("blue");
            //yellowGun = Content.Load<Texture2D>("yellow");
            //greenGun = Content.Load<Texture2D>("green");
            //redGun = Content.Load<Texture2D>("red");

            ghostTextures = new Dictionary<string, Texture2D>();
            ghostTextures.Add("plain", Content.Load<Texture2D>("ghost_01"));

            //ghostTextures.Add("blue", Content.Load<Texture2D>("ghost_blue"));
            //ghostTextures.Add("green", Content.Load<Texture2D>("ghost_green"));
            //ghostTextures.Add("red", Content.Load<Texture2D>("ghost_red"));
            //ghostTextures.Add("yellow", Content.Load<Texture2D>("ghost_yellow"));

            ghostTextures.Add("blue", Content.Load<Texture2D>("ghost_blue_animation_01"));
            ghostTextures.Add("green", Content.Load<Texture2D>("ghost_green_animation_01"));
            ghostTextures.Add("red", Content.Load<Texture2D>("ghost_red_animation_01"));
            ghostTextures.Add("yellow", Content.Load<Texture2D>("ghost_yellow_animation_01"));

            ghostTextureAnim = new Dictionary<string, Texture2D>();
            ghostTextureAnim.Add("green", Content.Load<Texture2D>("ghost_green_animation_02"));
            ghostTextureAnim.Add("red", Content.Load<Texture2D>("ghost_red_animation_02"));
            ghostTextureAnim.Add("blue", Content.Load<Texture2D>("ghost_blue_animation_02"));
            ghostTextureAnim.Add("yellow", Content.Load<Texture2D>("ghost_yellow_animation_02"));

            lifeBar = ProgressBar.Create(Content.Load<Texture2D>("life_bar"), true, new Vector2(GraphicsDevice.Viewport.Width / 4 - 147, GraphicsDevice.Viewport.Width / 2 - 305));
            lifeBar.IsUpToDown = false;
            streakBar = ProgressBar.Create(Content.Load<Texture2D>("streak_bar"), true, new Vector2(GraphicsDevice.Viewport.Width / 2 + 486, GraphicsDevice.Viewport.Width / 2 - 303));
            streakBar.IsUpToDown = false;

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
            scheduler.scheduleDelegate(delegateTickClock, 0.25f);

            // generate first rhythm
            currentDifficultyIndex = 0;
            beatFrequency = 0.3f;
            rhythm = Helper.Helper.GenerateRhythm(currentDifficultyIndex, beatFrequency, random);

            // initialize score and health
            score = 0;
            streak = 0;
            multiplier = 1;
            lifeRemaining = 10;
            lifeBar.Progress = 1.0f;

            isLeftMouseDown = acceptKeys = false;

            // start scheduled functions
            //HapticFeedback.startBeats(beatFrequency, 0.1f, 0.1f);
            scheduler.scheduleDelegate(delegateTickGhosts, beatFrequency);
            foreach (SoundEffectInstance inst in bgmList)
            {
                inst.Pitch = 0.0f;
                inst.Volume = 1.0f;
                inst.IsLooped = true;
            }
            bgmIndicator = rand.Next(0, 2);
            bgmList[bgmIndicator].Play();

            ghostList = new List<Ghost>();

            isPaused = false;
            isGameOver = false;
        }

        protected void PlayAgain()
        {
            scheduler.unscheduleDelegate(delegateTickClock);
            //HapticFeedback.stopBeats();
            scheduler.unscheduleDelegate(delegateTickGhosts);
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

                //UpdateProgressBars(gameTime);
                lifeBar.Update(gameTime);
                streakBar.Update(gameTime);
            }

            base.Update(gameTime);
        }

        private void UpdateKeyboardGamepad()
        {
            previousKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();

            previousGamepadState = currentGamepadState;
            currentGamepadState = GamePad.GetState(PlayerIndex.One);

            if (acceptKeys && !isGameOver && (currentKeyboardState.IsKeyDown(Keys.Escape) || currentGamepadState.IsButtonDown(Buttons.Back)))
            {
                acceptKeys = false;
                isPaused = !isPaused;

                if (isPaused)
                {
                    bgmList[bgmIndicator].Pause();
                }
                else
                {
                    bgmList[bgmIndicator].Resume();
                }
            }

            if (acceptKeys && isGameOver)
            {
                if (currentKeyboardState.IsKeyDown(Keys.N) || currentGamepadState.IsButtonDown(Buttons.Back))
                {
                    acceptKeys = false;
                    Exit();
                }
                else if (currentKeyboardState.IsKeyDown(Keys.Y) || currentGamepadState.IsButtonDown(Buttons.Back))
                {
                    acceptKeys = false;
                    PlayAgain();
                    StartGame();
                }
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
            if (!isPaused && !isGameOver && keyPressed != Keys.None)
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
                    UpdateLifeBar();

                    // are there any lives left?
                    if (lifeRemaining <= 0)
                    {
                        lifeRemaining = 0;

                        // stop everything!
                        scheduler.unscheduleDelegate(delegateTickClock);
                        scheduler.unscheduleDelegate(delegateTickGhosts);
                        //HapticFeedback.stopBeats();
                        isGameOver = true;
                        bgmList[bgmIndicator].Stop();
                    }

                    // reset streak and multiplier
                    streak = 0;
                    multiplier = 1;
                    UpdateStreakBar();
                }

                ghostList.Remove(ghost);
            }
        }

        private void DrawUI()
        {
            spriteBatch.DrawString(UIFont, "Score: " + score, new Vector2(GraphicsDevice.Viewport.Width / 2 - 350, 30), Color.Blue, 0.0f, Vector2.Zero, 2.0f, SpriteEffects.None, 1.0f);
            //spriteBatch.DrawString(UIFont, "Life: " + lifeRemaining, new Vector2(20, GraphicsDevice.Viewport.Height - 50), Color.Purple, 0.0f, Vector2.Zero, 2.0f, SpriteEffects.None, 1.0f);
            spriteBatch.DrawString(UIFont, "Life: " + lifeRemaining, new Vector2(GraphicsDevice.Viewport.Width / 2 + 200, 30), Color.ForestGreen, 0.0f, Vector2.Zero, 2.0f, SpriteEffects.None, 1.0f);
            //spriteBatch.DrawString(UIFont, "Green: D, Red: F, Blue: J, Yellow: K", new Vector2(GraphicsDevice.Viewport.Width / 2 - 220, 30), Color.ForestGreen, 0.0f, Vector2.Zero, 2.0f, SpriteEffects.None, 1.0f);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Transparent);

            spriteBatch.Begin();

            spriteBatch.Draw(hallway, Vector2.Zero, null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f);
            //spriteBatch.Draw(blueGun, new Vector2(GraphicsDevice.Viewport.Width * 0.1f, GraphicsDevice.Viewport.Height * 0.97f), null, Color.White, Helper.Helper.DegreesToRadians(45.0f), new Vector2(blueGun.Width / 2, blueGun.Height / 2), 0.65f, SpriteEffects.None, 0.0f);
            //spriteBatch.Draw(yellowGun, new Vector2(GraphicsDevice.Viewport.Width * 0.9f, GraphicsDevice.Viewport.Height * 0.97f), null, Color.White, Helper.Helper.DegreesToRadians(-45.0f), new Vector2(blueGun.Width / 2, blueGun.Height / 2), 0.65f, SpriteEffects.None, 0.0f);
            //spriteBatch.Draw(redGun, new Vector2(GraphicsDevice.Viewport.Width * 0.35f, GraphicsDevice.Viewport.Height), null, Color.White, Helper.Helper.DegreesToRadians(25.0f), new Vector2(blueGun.Width / 2, blueGun.Height / 2), 0.65f, SpriteEffects.None, 0.0f);
            //spriteBatch.Draw(greenGun, new Vector2(GraphicsDevice.Viewport.Width * 0.65f, GraphicsDevice.Viewport.Height), null, Color.White, Helper.Helper.DegreesToRadians(-25.0f), new Vector2(blueGun.Width / 2, blueGun.Height / 2), 0.65f, SpriteEffects.None, 0.0f);

            spriteBatch.DrawString(UIFont, "X" + multiplier, new Vector2(GraphicsDevice.Viewport.Width / 2 + 472, GraphicsDevice.Viewport.Height / 4 + 285), Color.BlueViolet, 0.0f, Vector2.Zero, 2.0f, SpriteEffects.None, 1.0f);
            lifeBar.Draw(spriteBatch);
            streakBar.Draw(spriteBatch);

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

            if (isGameOver)
            {
                spriteBatch.DrawString(UIFont, gameoverText, new Vector2(GraphicsDevice.Viewport.Width / 2 - 220, GraphicsDevice.Viewport.Height / 2 - 100), Color.Red, 0.0f, Vector2.Zero, 5.0f, SpriteEffects.None, 0.0f);
                spriteBatch.DrawString(UIFont, restartText, new Vector2(GraphicsDevice.Viewport.Width / 2 - 350, GraphicsDevice.Viewport.Height / 2), Color.Red, 0.0f, Vector2.Zero, 5.0f, SpriteEffects.None, 0.0f);
            }
            else if (isPaused)
            {
                spriteBatch.DrawString(UIFont, pausedText, new Vector2(GraphicsDevice.Viewport.Width / 2 - 125, GraphicsDevice.Viewport.Height / 2 - 30), Color.Red, 0.0f, Vector2.Zero, 5.0f, SpriteEffects.None, 0.0f);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void TickClock(float deltaTime)
        {
            ++seconds;
            if (seconds >= Helper.Helper.difficultyTimeMatrix[currentDifficultyIndex])
            {
                ++currentDifficultyIndex;
                bgmList[bgmIndicator].Pitch += bgmList[bgmIndicator].Pitch <= 0.9f ? 0.1f : 0.0f;
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
            Ghost ghost = new Ghost(ghostTextureAnim[GetGhostColor(laneNumber)], laneNumber, 1000, 720, 15, 0.1f, "");
            //Ghost ghost = new Ghost(ghostTextures[GetGhostColor(laneNumber)], ghostTextureAnim[GetGhostColor(laneNumber)], laneNumber, 1000, 720, 8, 1000, 720, 15, 0.3f, "");
            ghostList.Add(ghost);
            ghost.MoveForward(beatFrequency * 4.0f);
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
                    //A.Play();
                    break;
                case Keys.B:
                    laneNumber = 1;
                    //C.Play();
                    break;
                case Keys.X:
                    laneNumber = 2;
                    //E.Play();
                    break;
                case Keys.Y:
                    laneNumber = 3;
                    //highA.Play();
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

                    // play a sound
                    positiveInst.Volume = 0.8f;
                    positiveInst.Play();

                    // generate haptic feedback
                    HapticFeedback.playBeat(0.1f, 0.1f);
                    
                    // update streak
                    ++streak;
                    UpdateStreakBar();

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
                            //A.Play();
                            break;
                        case Keys.B:
                            //C.Play();
                            break;
                        case Keys.X:
                            //E.Play();
                            break;
                        case Keys.Y:
                            //highA.Play();
                            break;
                    }
                    return;
                }
            }

            // reset the streak and multiplier
            score -= 10;
            score = score < 0 ? 0 : score;

            streak = 0;
            multiplier = 1;
            UpdateStreakBar();

            // play sound when player misses ghost
            negative.Play();

            // generate haptic feedback
            HapticFeedback.playBeat(0.5f, 0.25f);
        }

        private void UpdateLifeBar()
        {
            actionManager.removeAllActionsFromTarget(lifeBar);
            actionManager.addAction(ProgressTo.create(0.2f, lifeRemaining / 10.0f), lifeBar);
        }

        private void UpdateStreakBar()
        {
            if (multiplier < 5)
            {
                float newProgress = (float)streak / (float)Helper.Helper.multiplier[multiplier - 1];

                actionManager.removeAllActionsFromTarget(streakBar);
                actionManager.addAction(ProgressTo.create(0.2f, newProgress), streakBar);
            }
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
