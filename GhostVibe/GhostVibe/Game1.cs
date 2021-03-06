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
        protected bool isGameStarted;
        protected string gameoverText;
        protected string pausedText;
        protected string restartText;
        protected string startText;
        protected string exitText;
        protected string titleText;

        protected Random rand;

        protected int bgmIndicator;

        protected Texture2D animationTexture, spriteTexture;
        protected Texture2D hallway;
        protected Texture2D hallwayOpen;
        //protected Texture2D blueGun, greenGun, redGun, yellowGun;
        protected SpriteFont UIFont;
        protected SpriteFont startFont;
        protected SpriteFont titleFont;

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

        int seconds, currentDifficultyIndex;
        protected int index = -1;
        protected List<int> rhythm;

        // feedback sprites
        protected List<Sprite> positiveSprites, negativeSprites;
        protected Sprite gameOver;

        // mouse states
        protected MouseState currentMouseState, previousMouseState;
        protected bool isLeftMouseDown;
        
        // keyboard states
        private KeyboardState currentKeyboardState, previousKeyboardState;
        private bool acceptKeys;

        // gamepad states
        private GamePadState currentGamepadState, previousGamepadState;

        protected int score, streak, multiplier, negativeMultiplier;
        protected int lifeRemaining;
        public Random random;
        protected ProgressBar lifeBar, streakBar;

        // audio objects
        protected SoundEffect bgm;
        protected SoundEffectInstance bgmInst;

        protected SoundEffect bgm2;
        protected SoundEffectInstance bgmInst2;

        protected SoundEffect bgm3;
        protected SoundEffectInstance bgmInst3;

        protected SoundEffect bgm4;
        protected SoundEffectInstance bgmInst4;

        protected List<SoundEffectInstance> bgmList;

        protected SoundEffect A;
        protected SoundEffect C;
        protected SoundEffect E;
        protected SoundEffect highA;
        protected SoundEffect positive;
        protected SoundEffect negative;

        protected SoundEffect gameoverSound;
        protected SoundEffectInstance gameoverInst;

        protected SoundEffectInstance positiveInst;

        // particle Engine
        ParticleEngine particleEngine;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;

            graphics.IsFullScreen = true;
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
            SoundEffect.MasterVolume = 1.0f;
            rand = new Random();

            isPaused = true;
            isGameOver = false;
            isGameStarted = false;
            gameoverText = "Game Over!";
            restartText = "Play Again? Y/N";
            pausedText = "Paused";
            startText = "Start [Y]";
            titleText = "SPACE REAPER";
            exitText = "Exit [N]";

            base.Initialize();
        }
        
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            UIFont = Content.Load<SpriteFont>("interface");
            startFont = Content.Load<SpriteFont>("start");
            titleFont = Content.Load<SpriteFont>("title");
            bgm = Content.Load<SoundEffect>("newbgmsize");
            bgm2 = Content.Load<SoundEffect>("final-battle");
            bgm3 = Content.Load<SoundEffect>("boss-battle");
            bgm4 = Content.Load<SoundEffect>("superstar-saga");
            bgmList = new List<SoundEffectInstance>();
            //bgmInst = bgm.CreateInstance();
            bgmInst2 = bgm2.CreateInstance();
            //bgmInst3 = bgm3.CreateInstance();
            //bgmInst4 = bgm4.CreateInstance();
            //bgmList.Add(bgmInst);
            bgmList.Add(bgmInst2);
            //bgmList.Add(bgmInst3);
            //bgmList.Add(bgmInst4);
            A = Content.Load<SoundEffect>("A2");
            C = Content.Load<SoundEffect>("C2");
            E = Content.Load<SoundEffect>("E2");
            highA = Content.Load<SoundEffect>("highA2");
            positive = Content.Load<SoundEffect>("happysound");
            negative = Content.Load<SoundEffect>("badsound");
            gameoverSound = Content.Load<SoundEffect>("Evil_Laugh");
            gameoverInst = gameoverSound.CreateInstance();
            gameoverInst.Pitch = 0.5f;
            hallway = Content.Load<Texture2D>("hallway_colorlight");
            hallwayOpen = Content.Load<Texture2D>("hallway_opening");
            positiveInst = positive.CreateInstance();

            ghostTextures = new Dictionary<string, Texture2D>();
            ghostTextures.Add("plain", Content.Load<Texture2D>("ghost_01"));

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
            List<Texture2D> notetextures = new List<Texture2D>();
            notetextures.Add(Content.Load<Texture2D>("red_note1"));
            notetextures.Add(Content.Load<Texture2D>("blue_note1"));
            notetextures.Add(Content.Load<Texture2D>("green_note1"));

            notetextures.Add(Content.Load<Texture2D>("orange_note1"));
            List<Texture2D> CloudTexture = new List<Texture2D>();
            CloudTexture.Add(Content.Load<Texture2D>("cloud01"));
            CloudTexture.Add(Content.Load<Texture2D>("cloud02"));
            CloudTexture.Add(Content.Load<Texture2D>("cloud03"));
            CloudTexture.Add(Content.Load<Texture2D>("cloud04"));
            particleEngine = new ParticleEngine(notetextures, CloudTexture, new Vector2(0, 0));

            positiveSprites = new List<Sprite>();
            LoadPositiveFeedback(Sprite.Create(Content.Load<Texture2D>("Great")));
            LoadPositiveFeedback(Sprite.Create(Content.Load<Texture2D>("awesome!")));
            LoadPositiveFeedback(Sprite.Create(Content.Load<Texture2D>("Fantastic!")));
            LoadPositiveFeedback(Sprite.Create(Content.Load<Texture2D>("Amazing!")));

            negativeSprites = new List<Sprite>();
            LoadNegativeFeedback(Sprite.Create(Content.Load<Texture2D>("-10")));
            LoadNegativeFeedback(Sprite.Create(Content.Load<Texture2D>("-20")));
            LoadNegativeFeedback(Sprite.Create(Content.Load<Texture2D>("-30")));
            LoadNegativeFeedback(Sprite.Create(Content.Load<Texture2D>("-40")));
            LoadNegativeFeedback(Sprite.Create(Content.Load<Texture2D>("-50")));

            gameOver = Sprite.Create(Content.Load<Texture2D>("hallway_gameover"), new Vector2(GraphicsDevice.Viewport.Width * 0.5f, GraphicsDevice.Viewport.Height * 0.5f));
            gameOver.IsVisible = false;
        }

        private void LoadPositiveFeedback(Sprite sprite)
        {
            sprite.IsVisible = false;
            positiveSprites.Add(sprite);
        }

        private void LoadNegativeFeedback(Sprite sprite)
        {
            sprite.IsVisible = false;
            sprite.Color = Color.Aqua;
            negativeSprites.Add(sprite);
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected void StartGame()
        {
            if (isGameStarted)
            {
                return;
            }
            isGameStarted = true;
            
            random = new Random();

            // start the clock
            seconds = 0;

            scheduler.scheduleDelegate(delegateTickClock, 0.5f);

            // generate first rhythm
            currentDifficultyIndex = 0;
            beatFrequency = 0.3f;
            rhythm = Helper.Helper.GenerateRhythm(currentDifficultyIndex, beatFrequency, random);

            // initialize score and health
            score = 0;
            streak = 0;
            multiplier = 1;
            negativeMultiplier = 1;
            lifeRemaining = 10;
            lifeBar.Progress = 1.0f;

            isLeftMouseDown = acceptKeys = false;

            // start scheduled functions
            //HapticFeedback.startBeats(beatFrequency, 0.1f, 0.1f);
            scheduler.scheduleDelegate(delegateTickGhosts, beatFrequency, -1, 1.5f);
            /*
            foreach (SoundEffectInstance inst in bgmList)
            {
                inst.Pitch = 0.0f;
                inst.Volume = 1.0f;
                inst.IsLooped = true;
            }
            //bgmIndicator = rand.Next(0, 4);
            bgmIndicator = rand.Next(0, 1);
            bgmList[bgmIndicator].Play();
            */

            bgmInst2.Pitch = 0.0f;
            bgmInst2.Volume = 1.0f;
            bgmInst2.IsLooped = true;
            bgmInst2.Play();

            ghostList = new List<Ghost>();

            isPaused = false;
            isGameOver = false;
        }

        protected void PlayAgain()
        {
            // only process these events if this is game over
            if (!isGameOver)
            {
                return;
            }

            scheduler.unscheduleDelegate(delegateTickClock);
            //HapticFeedback.stopBeats();
            scheduler.unscheduleDelegate(delegateTickGhosts);
            gameOver.IsVisible = false;

            ghostList.Clear();
            isGameStarted = false;
        }

        protected override void Update(GameTime gameTime)
        {
            // always listen for events
            UpdateKeyboardGamepad();            

            if (isGameStarted && !isPaused)
            {
                UpdateMouse();
                UpdateGhosts(gameTime);

                actionManager.update(gameTime.ElapsedGameTime.Milliseconds * 0.001f);
                scheduler.update(gameTime.ElapsedGameTime.Milliseconds * 0.001f);
                particleEngine.Update();

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

            if (acceptKeys && (!isGameStarted || isGameOver))
            {
                if (currentKeyboardState.IsKeyDown(Keys.N) || currentGamepadState.IsButtonDown(Buttons.Back))
                {
                    acceptKeys = false;
                    Exit();
                }
                else if (currentKeyboardState.IsKeyDown(Keys.Y) || currentGamepadState.IsButtonDown(Buttons.Start))
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
                acceptKeys = (currentGamepadState.IsButtonUp(Buttons.Start) &&
                    currentKeyboardState.IsKeyUp(Keys.Y) && currentKeyboardState.IsKeyUp(Keys.N) &&
                    currentKeyboardState.IsKeyUp(Keys.Escape) && currentGamepadState.IsButtonUp(Buttons.Back) &&
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

            if (isGameOver)
            {
                foreach (Ghost ghost in ghostList)
                {
                    ghost.Update(gameTime);
                }
            }
            else
            {
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
                            HapticFeedback.stopVibration(0.0f);

                            bgmList[bgmIndicator].Stop();
                            //gameoverSound.Play();
                            gameoverInst.Play();

                            GameOver();
                        }

                        // reset streak and multiplier
                        streak = 0;
                        multiplier = 1;
                        UpdateStreakBar();

                        // play sound when player misses ghost
                        negative.Play();

                        // generate haptic feedback
                        if (!isGameOver)
                        {
                            HapticFeedback.playBeat(1.0f, 0.25f);
                        }
                    }

                    ghostList.Remove(ghost);
                }
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Transparent);

            spriteBatch.Begin();

            if (!isGameStarted)
            {
                spriteBatch.Draw(hallwayOpen, Vector2.Zero, null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f);
                spriteBatch.DrawString(titleFont, titleText, new Vector2(GraphicsDevice.Viewport.Width / 2 - 350, GraphicsDevice.Viewport.Height / 2 - 250), Color.GreenYellow, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f);
                spriteBatch.DrawString(startFont, startText, new Vector2(GraphicsDevice.Viewport.Width / 2 - 140, GraphicsDevice.Viewport.Height / 2 + 40), Color.Aqua, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f);
                spriteBatch.DrawString(startFont, exitText, new Vector2(GraphicsDevice.Viewport.Width / 2 - 100, GraphicsDevice.Viewport.Height / 2 + 180), Color.Red, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f);
            }
            else
            {
                spriteBatch.Draw(hallway, Vector2.Zero, null, Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f);
                spriteBatch.DrawString(UIFont, "X" + multiplier, new Vector2(GraphicsDevice.Viewport.Width / 2 + 472, GraphicsDevice.Viewport.Height / 4 + 285), Color.Aqua, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
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
                particleEngine.Draw(spriteBatch);
                if (isGameOver)
                {
                    gameOver.Draw(spriteBatch);
                }
                else if (isPaused)
                {
                    spriteBatch.DrawString(startFont, pausedText, new Vector2(GraphicsDevice.Viewport.Width / 2 - 125, GraphicsDevice.Viewport.Height / 2 - 30), Color.GreenYellow, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f);
                }
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawUI()
        {
            spriteBatch.DrawString(UIFont, "Score: " + score, new Vector2(GraphicsDevice.Viewport.Width / 2 - 60, 30), Color.Magenta, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 1.0f);
            
            for (int i = 0; i < positiveSprites.Count; ++i)
            {
                positiveSprites[i].Draw(spriteBatch);
            }

            for (int i = 0; i < negativeSprites.Count; ++i)
            {
                negativeSprites[i].Draw(spriteBatch);
            }
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

                    //particle effect
                    particleEngine.EmitterLocation = ghost.GetCurrentPosition();
                    int totalParticle = 24;
                    int totalCloud = 4;

                    for (int j = 0; j < totalParticle; j++)
                    {
                        particleEngine.particles.Add(particleEngine.GenerateNewParticle(j, ghost.LaneNumber));
                    }

                    for (int j = 0; j < totalCloud; j++)
                    {
                        particleEngine.particles.Add(particleEngine.GenerateNewCloud(j, ghost.LaneNumber));
                    }

                    // generate haptic feedback
                    HapticFeedback.playBeat(0.1f, 0.1f);

                    // update streak
                    ++streak;
                    UpdateStreakBar();
                    negativeMultiplier = 1;

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
                            UpdateStreakBar();
                            
                            // animate the feedback sprite
                            AnimateFeedbackSprite(positiveSprites[j], false);

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
            score -= negativeMultiplier++ * 10;
            AnimateFeedbackSprite(negativeSprites[negativeMultiplier - 2], true);
            negativeMultiplier = negativeMultiplier > 5 ? 5 : negativeMultiplier; negativeMultiplier = negativeMultiplier > 5 ? 5 : negativeMultiplier;
            score = score < 0 ? 0 : score;

            streak = 0;
            multiplier = 1;
            UpdateStreakBar();

            // play sound when player misses ghost
            negative.Play();

            // generate haptic feedback
            HapticFeedback.playBeat(0.5f, 0.25f);
        }

        private void GameOver()
        {
            isGameOver = true;

            // add four ghosts that will come and kill the player for the game over screen
            for (int i = 0; i < 4; ++i)
            {
                Ghost ghost = new Ghost(ghostTextureAnim[GetGhostColor(i)], i, 1000, 720, 15, 0.1f, "");
                ghostList.Add(ghost);
                ghost.MustNotDie = true;
                ghost.MoveForward(beatFrequency * 4.0f);
            }

            scheduler.scheduleDelegateOnce(new UpdateDelegate(ShowGameOver), beatFrequency * 5.0f);
        }

        private void ShowGameOver(float deltaTime)
        {
            gameOver.IsVisible = true;
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

        public void AnimateFeedbackSprite(Sprite sprite, bool mustRandomizePosition)
        {
            if (sprite != null)
            {
                actionManager.removeAllActionsFromTarget(sprite);
                sprite.IsVisible = true;
                sprite.Scale = 0.0f;
                sprite.Opacity = 1.0f;
                if (mustRandomizePosition)
                {
                    float randomMultiplier = MathHelper.Clamp(random.Next(10) * 0.1f, 0.25f, 0.75f);
                    sprite.Position = new Vector2(GraphicsDevice.Viewport.Width* randomMultiplier, GraphicsDevice.Viewport.Height* 0.3f);
                }
                else
                {
                    sprite.Position = new Vector2(GraphicsDevice.Viewport.Width* 0.5f, GraphicsDevice.Viewport.Height* 0.3f);
                }
 
                List<FiniteTimeAction> actionList = new List<FiniteTimeAction>()
                {
                    ScaleTo.create(0.2f, 1.0f),
                    DelayTime.create(0.5f),
                    MoveBy.create(0.5f, new Vector2(0, GraphicsDevice.Viewport.Height* -0.3f)),
                    Hide.create()
                };
                actionManager.addAction(Sequence.create(actionList), sprite);
                actionManager.addAction(Sequence.createWithTwoActions(DelayTime.create(0.7f), FadeOut.create(0.5f)), sprite);
            }
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
