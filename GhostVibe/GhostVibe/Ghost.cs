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
        private float scale;
        private string color;
        private int stage;
        private int initPosNum;


        public Ghost(Texture2D staticTexture, float scaleF, string gColor)
        {
            initPosNum = GhostPosition.GetIndex();
            StageOnePosition = GhostPosition.GetInitialPosition(initPosNum);
            StageTwoPosition = GhostPosition.GetSecondPosition(initPosNum);
            StageThreePosition = GhostPosition.GetThirdPosition(initPosNum);
            //StageOnePosition = new Vector2(Helper.Helper.ViewportWidth * 0.5f, Helper.Helper.ViewportHeight * 0.25f);
            //StageTwoPosition = new Vector2(Helper.Helper.ViewportWidth * 0.5f, Helper.Helper.ViewportHeight * 0.40f);
            //StageThreePosition = new Vector2(Helper.Helper.ViewportWidth * 0.5f, Helper.Helper.ViewportHeight * 0.60f);
            //StageOverPosition = new Vector2(Helper.Helper.ViewportWidth * 0.5f, Helper.Helper.ViewportHeight * 0.75f);
            currentPos = StageOnePosition;
            ghostImg = Sprite.Create(staticTexture, currentPos);
            scale = scaleF;
            ghostImg.Scale = scaleF;
            color = gColor;
            isAnimated = false;
            stage = 1;
            isInvincible = true;
            isActive = false;
        }

        public Ghost(Texture2D dynamicTexture, int frameW, int frameH, int numFrame, float scaleF, string gColor)
        {
            initPosNum = GhostPosition.GetIndex();
            //StageOnePosition = new Vector2(Helper.Helper.ViewportWidth * 0.5f, Helper.Helper.ViewportHeight * 0.75f);
            //StageTwoPosition = new Vector2(Helper.Helper.ViewportWidth * 0.5f, Helper.Helper.ViewportHeight * 0.60f);
            //StageThreePosition = new Vector2(Helper.Helper.ViewportWidth * 0.5f, Helper.Helper.ViewportHeight * 0.40f);
            //StageOverPosition = new Vector2(Helper.Helper.ViewportWidth * 0.5f, Helper.Helper.ViewportHeight * 0.25f);
            StageOnePosition = GhostPosition.GetInitialPosition(initPosNum);
            StageTwoPosition = GhostPosition.GetSecondPosition(initPosNum);
            StageThreePosition = GhostPosition.GetThirdPosition(initPosNum);
            currentPos = StageOnePosition;
            ghostAnim = Sprite.Create(dynamicTexture, frameW, frameH, numFrame);
            ghostAnim.Position = currentPos;
            frameWidth = frameW;
            frameHeight = frameH;
            frameCount = numFrame;
            scale = scaleF;
            ghostAnim.Scale = scaleF;
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
                    ActionManager.Instance.addAction(ScaleTo.create(duration, scale * 1.5f), ghostAnim);
                }
                else
                {
                    ActionManager.Instance.addAction(MoveTo.create(duration, StageTwoPosition), ghostImg);
                    ActionManager.Instance.addAction(ScaleTo.create(duration, scale * 1.5f), ghostImg);
                }
                stage++;
            }
            else if (stage == 2)
            {
                currentPos = StageThreePosition;
                if (isAnimated)
                {
                    ActionManager.Instance.addAction(MoveTo.create(duration, StageThreePosition), ghostAnim);
                    ActionManager.Instance.addAction(ScaleTo.create(duration, scale * 1.8f), ghostAnim);
                }
                else
                {
                    ActionManager.Instance.addAction(MoveTo.create(duration, StageThreePosition), ghostImg);
                    ActionManager.Instance.addAction(ScaleTo.create(duration, scale * 1.8f), ghostImg);
                }
                stage++;
            }
            else
            {
                Destroy();
                return;
            }
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
                stage = -1;
                // first remove all running actions on the sprite
                if (!isAnimated)
                    ActionManager.Instance.removeAllActionsFromTarget(ghostImg);
                else
                    ActionManager.Instance.removeAllActionsFromTarget(ghostAnim);
            }
            isInvincible = true;
        }

        public int GetStage()
        {
            return stage;
        }

        public float GetScale()
        {
            return scale;
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
