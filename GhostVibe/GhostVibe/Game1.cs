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
        ParticleEngine particleEngine;

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
        protected UpdateDelegate delegateTickGhosts;
        protected static float beatFrequency;

        // mouse states
        protected MouseState currentMouseState, previousMouseState;
        protected bool isLeftMouseDown;
        
        // keyboard states
        private KeyboardState currentKeyboardState, previousKeyboardState;
        private bool acceptKeys;

        // gamepad states
        private GamePadState currentGamepadState, previousGamepadState;

        protected int score;
        protected int lifeRemaining;
        protected Random random;

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
        protected int index = -1;
        protected int rhythm01Count = 20;
        protected int[] rhythm01 = { 2, 2, 2, 2,
            2, 2, 2, 2,
            2, 2, 2, 2,
            2, 2, 2, 2,
            2, 2, 2, 2 };


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

            delegateTickGhosts = new UpdateDelegate(TickGhosts);
            beatFrequency = 0.5f;

            isLeftMouseDown = acceptKeys = false;
            
            score = 0;
            lifeRemaining = 3;
            random = new Random();

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
            List<Texture2D> notetextures = new List<Texture2D>();
            notetextures.Add(Content.Load<Texture2D>("red_note1"));
            notetextures.Add(Content.Load<Texture2D>("blue_note1"));
            notetextures.Add(Content.Load<Texture2D>("green_note1"));

            notetextures.Add(Content.Load<Texture2D>("orange_note1"));
            Texture2D CloudTexture;
            CloudTexture = this.Content.Load<Texture2D>("cloud");
            particleEngine = new ParticleEngine(notetextures, CloudTexture, new Vector2(400, 240));

            StartGame();
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected void StartGame()
        {
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

        private void TickGhosts(float deltaTime)
        {
            ++index;
            index = (index >= rhythm01Count) ? 0 : index;

            if (rhythm01[index] != 0)
            {
                SpawnGhost(rhythm01[index] - 1);
            }            

            acceptKeys = true;
        }

        private void SpawnGhost(int laneNumber)
        {
            Ghost ghost = new Ghost(ghostTextures["plain"], laneNumber, 0.3f, "");
            ghostList.Add(ghost);
            ghost.MoveForward(beatFrequency * 2);
        }

        private void ShootGhost(Keys keyPressed)
        {
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

                // the ghost needs to be in the right lane AND in the shooting range
                if (ghost.LaneNumber == laneNumber && ghost.IsInShootingRange)
                {
                    // kill the ghost
                    ghost.Die(true);
                }
            }
        }

        public static float BeatFrequency
        {
            get { return Game1.beatFrequency; }
        }

    }
}
