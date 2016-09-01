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
    public class Ghost
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
            /*
            if (stage == 1)
            {
                currentPos = StageTwoPosition;
                if (isAnimated)
                {
                    ActionManager.Instance.addAction(MoveTo.create(duration, StageTwoPosition), ghostAnim);
                    ActionManager.Instance.addAction(ScaleTo.create(duration, scale * 1.4f), ghostAnim);
                }
                else
                {
                    ActionManager.Instance.addAction(MoveTo.create(duration, StageTwoPosition), ghostImg);
                    ActionManager.Instance.addAction(ScaleTo.create(duration, scale * 1.4f), ghostImg);
                }
                stage++;
            }
            else if (stage == 2)
            {
                currentPos = StageThreePosition;
                if (isAnimated)
                {
                    ActionManager.Instance.addAction(MoveTo.create(duration, StageThreePosition), ghostAnim);
                    ActionManager.Instance.addAction(ScaleTo.create(duration, scale * 1.7f), ghostAnim);
                }
                else
                {
                    ActionManager.Instance.addAction(MoveTo.create(duration, StageThreePosition), ghostImg);
                    ActionManager.Instance.addAction(ScaleTo.create(duration, scale * 1.7f), ghostImg);
                }
                stage++;
            }
            else
            {
                Destroy();
                return;
            }
            */
            currentPos = StageThreePosition;
            if (isAnimated)
            {
                ActionManager.Instance.addAction(MoveTo.create(duration, currentPos), ghostAnim);
                ActionManager.Instance.addAction(ScaleTo.create(duration, scale * 1.4f), ghostAnim);
            }
            else
            {
                ActionManager.Instance.addAction(MoveTo.create(duration, currentPos), ghostImg);
                ActionManager.Instance.addAction(ScaleTo.create(duration, scale * 1.7f), ghostImg);
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
                // remove all running actions on the sprite
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

        public Sprite Image
        {
            get { return ghostImg; }
            set { ghostImg = value; }
        }

        public Sprite Animation
        {
            get { return ghostAnim; }
            set { ghostAnim = value; }
        }

        public string Color
        {
            get { return color; }
            set { color = value; }
        }
    }
}
