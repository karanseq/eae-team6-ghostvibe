using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace Helper
{
    public class ProgressBar : Sprite
    {
        protected float progress;
        protected bool isVertical, isUpToDown, isLeftToRight;

        protected Rectangle destRect;

        public ProgressBar()
        {
            progress = 0.0f;
            isVertical = true;
            isUpToDown = false;
            isLeftToRight = false;

            destRect = Rectangle.Empty;
        }

        public static ProgressBar Create(Texture2D texture, bool isVertical, Vector2 position = default(Vector2))
        {
            ProgressBar progressBar = new ProgressBar();
            if (progressBar.Initialize(texture, isVertical, position))
            {
                return progressBar;
            }
            progressBar = null;
            return null;
        }

        protected bool Initialize(Texture2D texture, bool isVertical, Vector2 position = default(Vector2))
        {
            if (!base.Initialize(texture, position))
            {
                return false;
            }

            this.isVertical = isVertical;
            this.destRect.X = (int)position.X;
            this.destRect.Y = (int)position.Y;
            this.destRect.Width = texture.Width;
            this.destRect.Height = texture.Height;
            return true;
        }

        public override void Update(GameTime gameTime)
        {
            if (isVertical)
            {
                if (isUpToDown)
                {
                    sourceRect.Height = (int)(progress * texture.Height);
                    sourceRect.Y = 0;
                    destRect.Height = (int)(progress * texture.Height);
                }
                else
                {
                    sourceRect.Y = (int)((1.0f - progress) * texture.Height);
                    sourceRect.Height = texture.Height;
                    destRect.Y = (int)(position.Y + (1.0f - progress) * texture.Height);
                }
                sourceRect.Width = texture.Width;
            }
            else
            {
                sourceRect.Width = (int)(progress * texture.Width);
                sourceRect.Height = texture.Height;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, destRect, sourceRect, color * opacity, rotation, origin, spriteEffects, layerDepth);
        }

        public float Progress
        {
            get { return progress; }
            set { progress = value > 1.0f ? 1.0f : (value < 0.0f ? 0.0f : value); }
        }

        public bool IsVertical
        {
            get { return isVertical; }
            set { isVertical = value; }
        }

        public bool IsUpToDown
        {
            get { return isUpToDown; }
            set { isUpToDown = isVertical ? value : false; }
        }

        public bool IsLeftToRight
        {
            get { return isLeftToRight; }
            set { isLeftToRight = !isVertical ? value : false; }
        }
    }
}
