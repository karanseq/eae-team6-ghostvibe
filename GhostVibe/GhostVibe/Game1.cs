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

        protected enum GameState { Spawning, Highlighting, Moving };
        protected GameState currentState;

        protected readonly int maxColors = 4;
        protected string[] colorNames = { "plain", "blue", "green", "red", "yellow" };
        protected Dictionary<string, Keys> colorKeyMap = new Dictionary<string, Keys>(){ 
            { "blue", Keys.X },
            { "green", Keys.A },
            { "red", Keys.B },
            { "yellow", Keys.Y } };

        protected Dictionary<string, Texture2D> ghostTextures;
        protected readonly int minGhosts = 2;
        protected readonly int maxGhosts = 5;
        protected List<Ghost> ghostList;
        protected UpdateDelegate delegateTickGhosts;
        protected float beatFrequency;
        protected int totalGhostsInWave, remainingGhostsInWave, numGhostsAlive;
        protected int prevGhostHoverIndex, ghostHoverIndex;
        protected int firstToggleCounter;

        // mouse states
        protected MouseState currentMouseState, previousMouseState;
        protected bool isLeftMouseDown;
        
        // keyboard states
        private KeyboardState currentKeyboardState, previousKeyboardState;
        private bool acceptKeys;

        // gamepad states
        private GamePadState currentGamepadState, previousGamepadState;

        protected int counter;
        protected int score;
        protected int lifeRemaining;
        protected Random random;

        // audio objects
        protected SoundEffect ghostPoof;
        protected SoundEffect ghostSpawn;
        protected SoundEffect whistle;
        protected SoundEffect bgm;
        protected SoundEffectInstance bgmInst;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            this.graphics.IsFullScreen = true;
            this.IsMouseVisible = true;
            Content.RootDirectory = "Content";
        }
        
        protected override void Initialize()
        {
            actionManager = ActionManager.Instance;
            scheduler = Scheduler.Instance;
            Helper.Helper.ViewportWidth = GraphicsDevice.Viewport.Width;
            Helper.Helper.ViewportHeight = GraphicsDevice.Viewport.Height;

            delegateTickGhosts = new UpdateDelegate(TickGhosts);
            beatFrequency = 0.75f;

            isLeftMouseDown = acceptKeys = false;

            counter = 0;
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
            // start scheduled functions
            HapticFeedback.startBeats(beatFrequency, 0.1f, 0.1f);
            scheduler.scheduleDelegate(delegateTickGhosts, beatFrequency);
            bgmInst.Volume = 0.3f;
            bgmInst.IsLooped = true;
            bgmInst.Play();
            StartNewWave();
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

            actionManager.update(gameTime.ElapsedGameTime.Milliseconds / 1000.0f);
            scheduler.update(gameTime.ElapsedGameTime.Milliseconds / 1000.0f);

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
            }

            if (acceptKeys && (currentKeyboardState.IsKeyDown(Keys.F) || currentGamepadState.IsButtonDown(Buttons.B)))
            {
                ShootGhost(Keys.B);
            }

            if (acceptKeys && (currentKeyboardState.IsKeyDown(Keys.J) || currentGamepadState.IsButtonDown(Buttons.X)))
            {
                ShootGhost(Keys.X);
            }

            if (acceptKeys && (currentKeyboardState.IsKeyDown(Keys.K) || currentGamepadState.IsButtonDown(Buttons.Y)))
            {
                ShootGhost(Keys.Y);
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
            foreach (Ghost ghost in ghostList)
            {
                ghost.Update(gameTime);
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

            foreach (Ghost ghost in ghostList)
            {
                ghost.Activate();
                ghost.Draw(spriteBatch);
            }
            
            DrawUI();

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void StartNewWave()
        {
            // initialize variables
            totalGhostsInWave = remainingGhostsInWave = 4; // random.Next(minGhosts, maxGhosts);
            numGhostsAlive = 0;
            prevGhostHoverIndex = ghostHoverIndex = 0;
            ghostList = new List<Ghost>();

            // set initial state
            currentState = GameState.Spawning;
            firstToggleCounter = totalGhostsInWave;
        }

        private void TickGhosts(float deltaTime)
        {
            // check here if the ghosts have finished spawning AND highlighting for the first time
            if (currentState == GameState.Highlighting && firstToggleCounter == 0)
            {
                currentState = GameState.Moving;
            }

            switch (currentState)
            {
                case GameState.Spawning:
                    SpawnGhosts();
                    break;

                case GameState.Highlighting:
                    ToggleGhostHighlights();
                    break;

                case GameState.Moving:
                    ToggleGhostHighlights();
                    MoveGhosts();
                    break;
            }
        }

        private void SpawnGhosts()
        {
            if (currentState != GameState.Spawning)
            {
                return;
            }

            // check if there are any ghosts still to spawn
            if (remainingGhostsInWave > 0)
            {
                // reduce remaining ghosts
                --remainingGhostsInWave;

                // randomly pick one from the available colors
                int randomIndex = 1 + random.Next(0, maxColors);
                string randomColor = colorNames[randomIndex];

                // create a new ghost and add it to the list
                Ghost ghost = new Ghost(ghostTextures["plain"], 0.3f, randomColor);
                ghostList.Add(ghost);
                ghostSpawn.Play();

                // add a subtle float animation
                MoveBy moveUp = MoveBy.create(0.5f, new Vector2(0.0f, 10.0f));
                RepeatForever floating = RepeatForever.create(Sequence.createWithTwoActions(moveUp, moveUp.reverse()));
                actionManager.addAction(floating, ghost.Image);
            }

            // check if all ghosts have spawned
            if (remainingGhostsInWave == 0)
            {
                // now start highlighting them
                numGhostsAlive = totalGhostsInWave;
                currentState = GameState.Highlighting;
            }
        }

        private void ToggleGhostHighlights()
        {
            if (currentState != GameState.Highlighting && currentState != GameState.Moving)
            {
                return;
            }

            acceptKeys = true;
            // check if there are any ghosts alive
            if (numGhostsAlive > 0)
            {
                // first unhighlight the previous ghost
                if (prevGhostHoverIndex != ghostHoverIndex)
                {
                    UnhighlightGhost();
                }

                // now highlight the next ghost
                HighlightGhost();
                prevGhostHoverIndex = ghostHoverIndex;
                ++ghostHoverIndex;
                ghostHoverIndex = (ghostHoverIndex >= totalGhostsInWave) ? 0 : ghostHoverIndex;                
            }
            
            if (firstToggleCounter > 0)
            {
                --firstToggleCounter;
            }

            //if (firstToggleCounter == 0)
            //{
            //    currentState = GameState.Moving;
            //}
        }

        private void UnhighlightGhost()
        {
            Ghost ghost = ghostList[prevGhostHoverIndex];
            ghost.Image.Texture = ghostTextures["plain"];
        }

        private void HighlightGhost()
        {
            Ghost ghost = ghostList[ghostHoverIndex];
            ghost.Image.Texture = ghostTextures[ghost.Color];
        }

        private void MoveGhosts()
        {
            ghostList[prevGhostHoverIndex].MoveForward(0.5f);
        }

        private void ShootGhost(Keys keyPressed)
        {
            // once the player presses one of the keys, accept no more...
            acceptKeys = false;

            // only allow shooting when ghosts have finished spawning & highlighting
            if (currentState != GameState.Moving)
                return;

            // first get the currently highlighted ghost
            Ghost currentlyHighlightedGhost = ghostList[prevGhostHoverIndex];

            // check if this ghost is alive
            if (currentlyHighlightedGhost.Image.Opacity < 1.0f)
            {
                return;
            }

            // get the key mapped to the current ghost's color
            Keys colorKey = colorKeyMap[currentlyHighlightedGhost.Color];

            // check if the correct key has been hit
            if (colorKey == keyPressed)
            {
                KillGhost();
            }
        }

        private void KillGhost()
        {
            // reduce number of ghosts alive
            --numGhostsAlive;

            // determine how much score the player gets
            score += 100;

            // reduce opacity and play sounds
            ghostList[prevGhostHoverIndex].Image.Opacity = 0.2f;
            ghostPoof.Play();

            if (numGhostsAlive <= 0)
            {
                // wave over...delete all ghosts
                foreach (Ghost ghost in ghostList)
                {
                    actionManager.removeAllActionsFromTarget(ghost.Image);
                }
                ghostList.Clear();

                // start a new wave!
                StartNewWave();
            }
        }

    }
}
