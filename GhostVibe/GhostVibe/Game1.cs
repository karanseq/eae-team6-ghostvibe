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
        protected SpriteFont arialFont;

        protected enum GameState { Spawning, Highlighting, Moving };
        protected GameState currentState;

        protected string[] colorNames = { "plain", "blue", "green", "red", "yellow" };
        protected Dictionary<string, Texture2D> ghostTextures;
        protected List<Ghost> ghostList;
        protected UpdateDelegate delegateTickGhosts;
        protected float beatFrequency;
        protected int totalGhostsInWave, remainingGhostsInWave, numGhostsAlive;
        protected int prevGhostHoverIndex, ghostHoverIndex;
        protected int firstToggleCounter;

        // mouse states
        protected MouseState currentMouseState, previousMouseState;
        protected bool isLeftMouseDown;
        
        // keyboard states used to determine keyboard states
        private KeyboardState currentKeyboardState, previousKeyboardState;
        private bool isSpaceKeyPressed, isAKeyPressed;

        protected int counter;
        protected int score;
        protected int lifeRemaining;
        protected Random random;

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

            delegateTickGhosts = new UpdateDelegate(TickGhosts);
            beatFrequency = 0.5f;

            isLeftMouseDown = false;
            isSpaceKeyPressed = false;
            isAKeyPressed = false;

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

            ghostTextures = new Dictionary<string, Texture2D>();
            ghostTextures.Add("plain", Content.Load<Texture2D>("Graphics\\ghost_01"));
            ghostTextures.Add("blue", Content.Load<Texture2D>("Graphics\\ghost_02"));
            ghostTextures.Add("green", Content.Load<Texture2D>("Graphics\\ghost_03"));
            ghostTextures.Add("red", Content.Load<Texture2D>("Graphics\\ghost_04"));
            ghostTextures.Add("yellow", Content.Load<Texture2D>("Graphics\\ghost_05"));

            ghostList = new List<Ghost>();

            StartGame();
        }

        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected void StartGame()
        {
            // initialize variables
            totalGhostsInWave = remainingGhostsInWave = 4;
            numGhostsAlive = 0;
            prevGhostHoverIndex = ghostHoverIndex = 0;

            // start scheduled functions
            HapticFeedback.startBeats(beatFrequency, 0.1f, 0.1f);
            scheduler.scheduleDelegate(delegateTickGhosts, beatFrequency);

            // set initial state
            currentState = GameState.Spawning;
            firstToggleCounter = totalGhostsInWave;
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            UpdateKeyboard();
            UpdateMouse();
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
            }

            if (isAKeyPressed && currentKeyboardState.IsKeyUp(Keys.A))
            {
                isAKeyPressed = false;
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

                //List<int> ghostsToBeDeleted = new List<int>();

                //for (int i = 0; i < ghostList.Count; ++i)
                //{
                //    Ghost ghost = ghostList[i];

                //    if (ghost.GetStage() == 1 || ghost.GetStage() == 2)
                //    {
                //        ghost.MoveForward(1.0f);
                //    }
                //    else
                //    {
                //        ghostsToBeDeleted.Add(i);
                //        ghost.Destroy();
                //    }
                //}
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
            spriteBatch.DrawString(arialFont, "Score: " + score, new Vector2(20, 20), Color.White, 0.0f, new Vector2(0, 0), 2.0f, SpriteEffects.None, 1.0f);
            spriteBatch.DrawString(arialFont, "Life: " + lifeRemaining, new Vector2(20, GraphicsDevice.Viewport.Height - 50), Color.White, 0.0f, new Vector2(0, 0), 2.0f, SpriteEffects.None, 1.0f);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            foreach(Ghost ghost in ghostList)
            {
                ghost.Activate();
                ghost.Draw(spriteBatch);
            }
            
            DrawUI();

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void TickGhosts(float deltaTime)
        {
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
                int randomIndex = 1 + random.Next(0, 4);
                string randomColor = colorNames[randomIndex];

                Trace.WriteLine("Spawning a " + randomColor + " ghost...");

                // create a new ghost and add it to the list
                Ghost ghost = new Ghost(ghostTextures["plain"], 0.35f, randomColor);
                //ghost.MoveForward(2.5f);
                ghostList.Add(ghost);
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

            if (firstToggleCounter == 0)
            {
                currentState = GameState.Moving;
            }
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
            Trace.WriteLine("Moving ghost " + prevGhostHoverIndex + "...");
        }

        private void ShootGhost(Keys keyPressed)
        {
            // first get the currently highlighted ghost

            // get the currently highlighted ghost's color

            // check if the correct key has been hit
        }

    }
}
