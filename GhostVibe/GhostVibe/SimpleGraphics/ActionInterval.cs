using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace GhostVibe.SimpleGraphics
{
    public class ActionInterval : FiniteTimeAction
    {
        protected float elapsed;
        protected bool firstTick;
        private readonly float FLT_EPSILON = 1.192092896e-07F;

        public override bool isDone()
        {
            return elapsed >= duration;
        }

        public override void step(float dt)
        {
            if (firstTick)
            {
                firstTick = false;
                elapsed = 0;
            }
            else
            {
                elapsed += dt;
            }

            float updateDt = MathHelper.Max(0.0f, MathHelper.Min(1, elapsed / duration));
            update(updateDt);
        }

        public override void startWithTarget(Sprite t)
        {
            base.startWithTarget(t);
            elapsed = 0.0f;
            firstTick = true;
        }

        public bool initWithDuration(float d)
        {
            duration = d;
            if (duration < FLT_EPSILON)
            {
                duration = FLT_EPSILON;
            }

            elapsed = 0;
            firstTick = true;

            return true;
        }

        public new ActionInterval clone()
        {
            return null;
        }

        public new ActionInterval reverse()
        {
            return null;
        }

        public float Elapsed
        {
            get { return elapsed; }
        }
    }

    public class RotateTo : ActionInterval
    {
        protected float startAngle, destAngle, diffAngle;

        protected RotateTo() { }

        public static RotateTo create(float duration, float destAngle)
        {
            RotateTo rotateTo = new RotateTo();
            rotateTo.initWithDuration(duration, destAngle);
            return rotateTo;
        }

        protected bool initWithDuration(float duration, float destAngle)
        {
            if (base.initWithDuration(duration))
            {
                this.destAngle = destAngle;
                return true;
            }
            return false;
        }

        public override void startWithTarget(Sprite t)
        {
            base.startWithTarget(t);
            startAngle = target.Rotation;
            diffAngle = destAngle - startAngle;
        }

        public override void update(float time)
        {
            if (target != null)
            {
                target.Rotation = startAngle + diffAngle * time;
            }
        }

        public new RotateTo clone()
        {
            return RotateTo.create(duration, destAngle);
        }

        public new RotateTo reverse()
        {
            return null;
        }
    }

    public class RotateBy : ActionInterval
    {
        protected float startAngle, deltaAngle;

        protected RotateBy() { }

        public static RotateBy create(float duration, float deltaAngle)
        {
            RotateBy rotateBy = new RotateBy();
            rotateBy.initWithDuration(duration, deltaAngle);
            return rotateBy;
        }

        protected bool initWithDuration(float duration, float deltaAngle)
        {
            if (base.initWithDuration(duration))
            {
                this.deltaAngle = deltaAngle;
                return true;
            }
            return false;
        }

        public override void startWithTarget(Sprite t)
        {
            base.startWithTarget(t);
            startAngle = t.Rotation;
        }

        public override void update(float time)
        {
            if (target != null)
            {
                target.Rotation = startAngle + deltaAngle * time;
            }
        }

        public new RotateBy clone()
        {
            return RotateBy.create(duration, deltaAngle);
        }

        public new RotateBy reverse()
        {
            return RotateBy.create(duration, -deltaAngle);
        }
    }

    public class MoveBy : ActionInterval
    {
        protected Vector2 startPosition, deltaPosition, previousPosition;

        protected MoveBy() { }

        public static MoveBy create(float duration, Vector2 deltaPosition)
        {
            MoveBy moveBy = new MoveBy();
            moveBy.initWithDuration(duration, deltaPosition);
            return moveBy;
        }

        protected bool initWithDuration(float duration, Vector2 deltaPosition)
        {
            if (base.initWithDuration(duration))
            {
                this.deltaPosition = deltaPosition;
                return true;
            }
            return false;
        }

        public override void startWithTarget(Sprite t)
        {
            base.startWithTarget(t);
            previousPosition = startPosition = target.Position;
        }

        public override void update(float time)
        {
            if (target != null)
            {
                Vector2 currentPosition = new Vector2(target.Position.X, target.Position.Y);
                Vector2 diffPosition = currentPosition - previousPosition;
                startPosition = startPosition + diffPosition;
                Vector2 newPosition = startPosition + (deltaPosition * time);
                target.Position = new Vector2(newPosition.X, newPosition.Y);
                previousPosition = newPosition;
            }
        }

        public new MoveBy clone()
        {
            return MoveBy.create(duration, deltaPosition);
        }

        public new MoveBy reverse()
        {
            return MoveBy.create(duration, -deltaPosition);
        }
    }

    public class MoveTo : MoveBy
    {
        protected Vector2 endPosition;

        protected MoveTo() { }

        public new static MoveTo create(float duration, Vector2 endPosition)
        {
            MoveTo moveTo = new MoveTo();
            moveTo.initWithDuration(duration, endPosition);
            return moveTo;
        }

        protected new bool initWithDuration(float duration, Vector2 endPosition)
        {
            if (base.initWithDuration(duration))
            {
                this.endPosition = endPosition;
                return true;
            }
            return false;
        }

        public override void startWithTarget(Sprite t)
        {
            base.startWithTarget(t);
            deltaPosition = endPosition - target.Position;
        }

        public new MoveTo clone()
        {
            return MoveTo.create(duration, endPosition);
        }

        public new MoveTo reverse()
        {
            return null;
        }
    }
}
