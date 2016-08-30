using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Helper;
using System.Diagnostics;

namespace GhostVibe
{
    class Ghost
    {
        private Vector2 initPosition;
        private Sprite ghostImg;
        private Sprite ghostAnim;
        private int frameWidth;
        private int frameHeight;
        private int frameCount;
        public bool isActive;
        public bool isAnimated;
        public bool isInvincible;
        Vector2 currentPos;
        Vector2 StageOnePosition;
        Vector2 StageTwoPosition;
        Vector2 StageThreePosition;
        Vector2 StageOverPosition;
        private float scale;
        private string color;
        private int stage;
        private Random random;


        public Ghost(Texture2D staticTexture, Vector2 pos, float scaleF, string gColor)
        {
            random = new Random();
            StageOnePosition = new Vector2(Helper.Helper.ViewportWidth * 0.5f, Helper.Helper.ViewportHeight * 0.25f);
            StageTwoPosition = new Vector2(Helper.Helper.ViewportWidth * 0.5f, Helper.Helper.ViewportHeight * 0.40f);
            StageThreePosition = new Vector2(Helper.Helper.ViewportWidth * 0.5f, Helper.Helper.ViewportHeight * 0.60f);
            StageOverPosition = new Vector2(Helper.Helper.ViewportWidth * 0.5f, Helper.Helper.ViewportHeight * 0.75f);
            initPosition = pos;
            currentPos = initPosition;
            ghostImg = Sprite.Create(staticTexture, pos);
            scale = scaleF;
            color = gColor;
            isAnimated = false;
            stage = 1;
            isInvincible = true;
            isActive = false;
        }

        public Ghost(Texture2D dynamicTexture, Vector2 pos, int frameW, int frameH, int numFrame, float scaleF, string gColor)
        {
            random = new Random();
            StageOnePosition = new Vector2(Helper.Helper.ViewportWidth * 0.5f, Helper.Helper.ViewportHeight * 0.75f);
            StageTwoPosition = new Vector2(Helper.Helper.ViewportWidth * 0.5f, Helper.Helper.ViewportHeight * 0.60f);
            StageThreePosition = new Vector2(Helper.Helper.ViewportWidth * 0.5f, Helper.Helper.ViewportHeight * 0.40f);
            StageOverPosition = new Vector2(Helper.Helper.ViewportWidth * 0.5f, Helper.Helper.ViewportHeight * 0.25f);
            initPosition = pos;
            currentPos = initPosition;
            ghostAnim = Sprite.Create(dynamicTexture, frameW, frameH, numFrame);
            ghostAnim.Position = pos;
            frameWidth = frameW;
            frameHeight = frameH;
            frameCount = numFrame;
            scale = scaleF;
            color = gColor;
            isAnimated = true;
            stage = 1;
            isInvincible = true;
            isActive = false;
        }

        public void MoveForward(float duration)
        {
            isInvincible = true;
            if (stage == 1)
            {
                currentPos = StageTwoPosition;
                if (isAnimated)
                {
                    ActionManager.Instance.addAction(MoveTo.create(duration, StageTwoPosition), ghostAnim);
                    ActionManager.Instance.addAction(ScaleTo.create(duration, scale * 1.2f), ghostAnim);
                }
                else
                {
                    ActionManager.Instance.addAction(MoveTo.create(duration, StageTwoPosition), ghostImg);
                    ActionManager.Instance.addAction(ScaleTo.create(duration, scale * 1.2f), ghostImg);
                }
                stage++;
            }
            else if (stage == 2)
            {
                currentPos = StageThreePosition;
                if (isAnimated)
                {
                    ActionManager.Instance.addAction(MoveTo.create(duration, StageThreePosition), ghostAnim);
                    ActionManager.Instance.addAction(ScaleTo.create(duration, scale * 1.5f), ghostAnim);
                }
                else
                {
                    ActionManager.Instance.addAction(MoveTo.create(duration, StageThreePosition), ghostImg);
                    ActionManager.Instance.addAction(ScaleTo.create(duration, scale * 1.5f), ghostImg);
                }
                stage++;
            }
            else if (stage == 3)
            {
                currentPos = StageOverPosition;
                if (isAnimated)
                {
                    ActionManager.Instance.addAction(MoveTo.create(duration, StageOverPosition), ghostAnim);
                    ActionManager.Instance.addAction(ScaleTo.create(duration, scale * 1.8f), ghostAnim);
                }
                else
                {
                    ActionManager.Instance.addAction(MoveTo.create(duration, StageOverPosition), ghostImg);
                    ActionManager.Instance.addAction(ScaleTo.create(duration, scale * 1.8f), ghostImg);
                }
                Destroy();
            }
            else
                return;
            isInvincible = false;
        }

        public void GotHitTurnWhite()
        {
            // Fade works. May use that here.
            isInvincible = true;
        }

        public void Destroy()
        {
            if (!isActive)
                return;
            else
            {
                isActive = false;
                stage = 0;
            }
            isInvincible = true;
        }

        public void Activate()
        {
            if (!isActive)
                isActive = true;
            else
                return;
        }

        public void Update(GameTime gameTime)
        {
            if (isAnimated)
            {
                ghostAnim.Update(gameTime);
            }
            else
            {
                ghostImg.Update(gameTime);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            //if (isAnimated)
            //{
            //    if (isActive)
            //    {
            //        spriteBatch.Draw(ghostAnim.Texture, currentPos, ghostAnim.SourceRect, ghostAnim.Color, ghostAnim.Rotation, ghostAnim.Origin, ghostAnim.Scale, ghostAnim.SpriteEffects, ghostAnim.LayerDepth);
            //    }
            //}
            //else
            //{
            //    if (isActive)
            //    {
            //        spriteBatch.Draw(ghostImg.Texture, currentPos, null, ghostImg.Color, ghostImg.Rotation, ghostImg.Origin, ghostImg.Scale, ghostImg.SpriteEffects, ghostImg.LayerDepth);
            //    }
            //}

            if (isAnimated)
            {
                if(isActive)
                    ghostAnim.Draw(spriteBatch);
            }
            else
            {
                if(isActive)
                    ghostImg.Draw(spriteBatch);
            }
        }

    }
}
