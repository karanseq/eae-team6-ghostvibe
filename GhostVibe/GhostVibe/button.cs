using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;

namespace gameUI
{
    class button
    {
        public int buttonX, buttonY;
        private Texture2D mTexture;
        private Texture2D mTexture1;
        private Texture2D mTexture2;
        private string Name;
        public MouseState CurrentMouseState;
        private float scale;
        private SpriteFont font1;
        public bool EndGame = false;

        public int ButtonX
        {
            get
            {
                return buttonX;
            }
            set { }
        }

        public int ButtonY
        {
            get
            {
                return buttonY;
            }
            set { }
        }

        public void Create(string name, int buttonX, int buttonY)
        {
            Name = name;
            this.buttonX = buttonX;
            this.buttonY = buttonY;
        }
        public void LoadContent(ContentManager theContentManager, string theAssetName, string theAssetNameTwo)
        {
            mTexture1 = theContentManager.Load<Texture2D>(theAssetName);
            mTexture2 = theContentManager.Load<Texture2D>(theAssetNameTwo);
            font1 = theContentManager.Load<SpriteFont>("Courier New");
            scale = 0.1f;
            mTexture = mTexture1;
        }
        /**
         * @return true: If a player enters the button with mouse
         */
        public bool enterButton()
        {
            if (CurrentMouseState.X < buttonX + mTexture.Width * scale &&
                    CurrentMouseState.X > buttonX &&
                    CurrentMouseState.Y < buttonY + mTexture.Height * scale &&
                    CurrentMouseState.Y > buttonY)
            {
                mTexture = mTexture2;
                return true;
            }
            mTexture = mTexture1;
            return false;
        }



        public void Update(GameTime gameTime)
        {
            CurrentMouseState = Mouse.GetState();
            //       KeyboardState aCurrentKeyboardState = Keyboard.GetState();
            if (enterButton() && CurrentMouseState.LeftButton == ButtonState.Pressed)
            {
                switch (Name)
                {
                    case "EndGame": //the name of the button
                        {
                            EndGame = true;
                            break;
                        }
                    case "StartGame":
                        {
                            break;
                        }

                    default:
                        break;
                }
            }
        }
        public void Draw(SpriteBatch theSpriteBatch)
        {
            theSpriteBatch.Draw(mTexture, new Vector2((int)buttonX, (int)buttonY), null, Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
            theSpriteBatch.DrawString(font1, Name, new Vector2(buttonX + mTexture.Width / 2 * scale, buttonY + mTexture.Height / 2 * scale) - font1.MeasureString(Name) / 2, Color.Green, 0, Vector2.Zero, 1, SpriteEffects.None, 0);

        }
    }
}
