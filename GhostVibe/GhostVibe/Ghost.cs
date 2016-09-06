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
        private Sprite ghostAnim2;
        private int frameWidth;
        private int frameHeight;
        private int frameCount;
        private int frameWidth2;
        private int frameHeight2;
        private int frameCount2;
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
        private int laneNumber;

        private bool isInShootingRange;
        private bool wasKilledByPlayer;
        private bool isDying;
        private bool hasFinishedDying;

        public Ghost(Texture2D staticTexture, int positionIndex, float scaleF, string gColor)
        {
            laneNumber = positionIndex; //GhostPosition.GetIndex();
            StageOnePosition = GhostPosition.GetInitialPosition(laneNumber);
            StageTwoPosition = GhostPosition.GetSecondPosition(laneNumber);
            StageThreePosition = GhostPosition.GetThirdPosition(laneNumber);
            currentPos = StageOnePosition;
            ghostImg = Sprite.Create(staticTexture, currentPos);
            scale = scaleF;
            ghostImg.Scale = scaleF;
            color = gColor;
            isAnimated = false;
            stage = 1;
            isInvincible = true;
            isActive = false;
            isInShootingRange = false;
            wasKilledByPlayer = false;
            isDying = false;
            hasFinishedDying = false;
        }

        public Ghost(Texture2D dynamicTexture, Texture2D dynamicTexture2, int positionIndex, int frameW1, int frameH1, int numFrame1, int frameW2, int frameH2, int numFrame2, float scaleF, string gColor)
        {
            laneNumber = positionIndex; //GhostPosition.GetIndex();
            StageOnePosition = GhostPosition.GetInitialPosition(laneNumber);
            StageTwoPosition = GhostPosition.GetSecondPosition(laneNumber);
            StageThreePosition = GhostPosition.GetThirdPosition(laneNumber);
            currentPos = StageOnePosition;
            ghostAnim = Sprite.Create(dynamicTexture, frameW1, frameH1, numFrame1);
            ghostAnim2 = Sprite.Create(dynamicTexture2, frameW2, frameH2, numFrame2);
            ghostAnim.Position = currentPos;
            ghostAnim2.Position = currentPos;
            frameWidth = frameW1;
            frameHeight = frameH1;
            frameCount = numFrame1;
            frameWidth2 = frameW2;
            frameHeight2 = frameH2;
            frameCount2 = numFrame2;
            scale = scaleF;
            ghostAnim.Scale = scaleF;
            ghostAnim2.Scale = scaleF;
            color = gColor;
            isAnimated = true;
            stage = 1;
            isInvincible = true;
            isActive = false;
            isInShootingRange = false;
            wasKilledByPlayer = false;
            isDying = false;
            hasFinishedDying = false;
        }


        public Ghost(Texture2D dynamicTexture, int positionIndex, int frameW, int frameH, int numFrame, float scaleF, string gColor)
        {
            laneNumber = positionIndex; //GhostPosition.GetIndex();
            StageOnePosition = GhostPosition.GetInitialPosition(laneNumber);
            StageTwoPosition = GhostPosition.GetSecondPosition(laneNumber);
            StageThreePosition = GhostPosition.GetThirdPosition(laneNumber);
            currentPos = StageOnePosition;
            ghostAnim = Sprite.Create(dynamicTexture, frameW, frameH, numFrame);
            ghostAnim.FrameTime = 90;
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
            isInShootingRange = false;
            wasKilledByPlayer = false;
            isDying = false;
            hasFinishedDying = false;
        }

        public void MoveForward(float duration)
        {
            isInvincible = true;
            
            currentPos = StageThreePosition;
            Sprite sprite = isAnimated ? ghostAnim : ghostImg;
            
            // animate the movement and scaling
            ActionManager.Instance.addAction(MoveTo.create(duration, currentPos), sprite);
            ActionManager.Instance.addAction(ScaleTo.create(duration, scale * 4.2f), sprite);

            // callback to notify that the ghost has reached the shooting range
            Scheduler.Instance.scheduleDelegateOnce(new UpdateDelegate(EnableInShootingRange), duration * 0.6f);
            // callback to notify that the ghost has finished moving
            Scheduler.Instance.scheduleDelegateOnce(new UpdateDelegate(FinishedMoving), duration);

            isInvincible = false;
        }

        public void EnableInShootingRange(float dt)
        {
            isInShootingRange = true;
        }

        public void FinishedMoving(float dt)
        {
            Die(false);
        }

        public void Die(bool killedByPlayer)
        {
            // even a ghost can die only once...
            if (isDying) return;

            isDying = true;
            wasKilledByPlayer = killedByPlayer;

            // find which sprite is being used
            Sprite sprite = isAnimated ? ghostAnim : ghostImg;

            // if not killed by player, play the slicing animation
            if (!killedByPlayer)
            {
                //sprite.Color = Microsoft.Xna.Framework.Color.RosyBrown;
                // play the second animation here
                // TODO
                sprite.Color = Microsoft.Xna.Framework.Color.Red;
            }
            else
            {
                sprite.Color = Microsoft.Xna.Framework.Color.Green;
            }

            // callback a function when the death animation is finished
            Sequence deathSequence = Sequence.createWithTwoActions(ScaleTo.create(Game1.BeatFrequency, 1.0f), CallFunc.create(new CallbackDelegate(FinishedDying)));
            ActionManager.Instance.addAction(deathSequence, sprite);
            ActionManager.Instance.addAction(FadeOut.create(Game1.BeatFrequency), sprite);
        }

        public void FinishedDying()
        {
            hasFinishedDying = true;
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
                if (isActive)
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

        public bool IsInShootingRange
        {
            get { return isInShootingRange; }
        }

        public bool WasKilledByPlayer
        {
            get { return wasKilledByPlayer; }
        }

        public bool IsDying
        {
            get { return isDying; }
        }

        public bool HasFinishedDying
        {
            get { return hasFinishedDying; }
        }

        public int LaneNumber
        {
            get { return laneNumber; }
            set { laneNumber = value; }
        }
        
    }
}
