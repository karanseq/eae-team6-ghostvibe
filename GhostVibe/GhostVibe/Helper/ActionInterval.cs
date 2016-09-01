using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Helper
{
    public class ActionInterval : FiniteTimeAction
    {
        protected float elapsed;
        protected bool firstTick;
        protected readonly float FLT_EPSILON = 1.192092896e-07F;

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
    } // class ActionInterval

    public class ExtraAction : FiniteTimeAction
    {
        protected ExtraAction() { }

        public static ExtraAction create()
        {
            ExtraAction extraAction = new ExtraAction();
            return extraAction;
        }

        public override void update(float time)
        { }

        public override void step(float deltaTime)
        { }

        public new ExtraAction clone()
        {
            return ExtraAction.create();
        }

        public new ExtraAction reverse()
        {
            return ExtraAction.create();
        }
    }

    public class Sequence : ActionInterval
    {
        protected FiniteTimeAction[] actions;
        float split;
        int last;

        protected Sequence() { }

        public static Sequence createWithTwoActions(FiniteTimeAction actionOne, FiniteTimeAction actionTwo)
        {
            Sequence sequence = new Sequence();
            if (sequence.initWithTwoActions(actionOne, actionTwo))
            {
                return sequence;
            }
            sequence = null;
            return null;
        }

        public static Sequence create(List<FiniteTimeAction> actionList)
        {
            Sequence sequence = new Sequence();
            if (sequence.init(actionList))
            {
                return sequence;
            }
            sequence = null;
            return null;
        }

        protected bool initWithTwoActions(FiniteTimeAction actionOne, FiniteTimeAction actionTwo)
        {
            Trace.Assert((actionOne != null), "Null action passed to Sequence.initWithTwoActions!");
            Trace.Assert((actionTwo != null), "Null action passed to Sequence.initWithTwoActions!");

            float totalDuration = actionOne.Duration + actionTwo.Duration;
            base.initWithDuration(totalDuration);

            actions = new FiniteTimeAction[2];
            actions[0] = actionOne;
            actions[1] = actionTwo;

            return true;
        }

        protected bool init(List<FiniteTimeAction> actionList)
        {
            var numActions = actionList.Count;
            if (numActions == 0)
            {
                return false;
            }

            if (numActions == 1)
            {
                return initWithTwoActions(actionList[0], ExtraAction.create());
            }

            var first = actionList[0];
            for (int i = 1; i < numActions - 1; ++i)
            {
                first = createWithTwoActions(first, actionList[i]);
            }

            return initWithTwoActions(first, actionList[numActions - 1]);
        }

        public override void startWithTarget(Sprite t)
        {
            if (actions[0] == null || actions[1] == null)
            {
                Trace.TraceError("Sequence.startWithTarget either of the actions is null!");
            }

            if (duration > FLT_EPSILON)
            {
                split = actions[0].Duration / duration;
            }

            base.startWithTarget(t);
            last = -1;
        }

        public override void stop()
        {
            if (last != -1 && actions[last] != null)
            {
                actions[last].stop();
            }

            base.stop();
        }

        public override void update(float time)
        {
            int found = 0;
            float newTime = 0.0f;

            if (time < split)
            {
                found = 0;
                if (split != 0)
                {
                    newTime = time / split;
                }
                else
                {
                    newTime = 1;
                }
            }
            else
            {
                found = 1;
                if (split == 1)
                {
                    newTime = 1;
                }
                else
                {
                    newTime = (time - split) / (1 - split);
                }
            }

            if (found == 1)
            {
                if (last == -1)
                {
                    actions[0].startWithTarget(target);
                    actions[0].update(1.0f);
                    actions[0].stop();
                }
                else if (last == 0)
                {
                    actions[0].update(1.0f);
                    actions[0].stop();
                }
            }
            else if (found == 0 && last == 1)
            {
                actions[1].update(0);
                actions[1].stop();
            }

            if (found == last && actions[found].isDone())
            {
                return;
            }

            if (found != last)
            {
                actions[found].startWithTarget(target);
            }
            actions[found].update(newTime);
            last = found;
        }

        public new Sequence clone()
        {
            if (actions[0] != null && actions[1] != null)
            {
                return Sequence.createWithTwoActions(actions[0].clone(), actions[1].clone());
            }
            else
            {
                return null;
            }
        }

        public new Sequence reverse()
        {
            return null;
        }

    } // class Sequence

    public class Repeat : ActionInterval
    {
        protected int times, total;
        float nextDeltaTime;
        bool actionInstant;
        FiniteTimeAction innerAction;

        protected Repeat() { }

        public static Repeat create(FiniteTimeAction action, int times)
        {
            Repeat repeat = new Repeat();
            if (repeat.initWithAction(action, times))
            {
                return repeat;
            }
            repeat = null;
            return null;
        }

        protected bool initWithAction(FiniteTimeAction action, int times)
        {
            float d = action.Duration * times;

            if (action != null && base.initWithDuration(d))
            {
                this.times = times;
                innerAction = action;

                actionInstant = false;
                total = 0;

                return true;
            }
            return false;
        }

        public override void startWithTarget(Sprite t)
        {
            total = 0;
            nextDeltaTime = innerAction.Duration / duration;
            base.startWithTarget(t);
            innerAction.startWithTarget(t);
        }

        public override void stop()
        {
            innerAction.stop();
            base.stop();
        }

        public override void update(float time)
        {
            if (time >= nextDeltaTime)
            {
                while (time >= nextDeltaTime && total < times)
                {
                    innerAction.update(1.0f);
                    total++;

                    innerAction.stop();
                    innerAction.startWithTarget(target);
                    nextDeltaTime = innerAction.Duration / duration * (total + 1);
                }

                if (Math.Abs(time - 1.0f) < FLT_EPSILON && total < times)
                {
                    innerAction.update(1.0f);
                    total++;
                }

                // don't set an instant action back or update it, it has no use because it has no duration
                if (!actionInstant)
                {
                    if (total == times)
                    {
                        innerAction.stop();
                    }
                    else
                    {
                        innerAction.update(time - (nextDeltaTime - innerAction.Duration / duration));
                    }
                }
            }
            else
            {
                innerAction.update((time * times) % 1.0f);
            }
        }

        public override bool isDone()
        {
            return total == times;
        }

        public FiniteTimeAction InnerAction
        {
            get { return innerAction; }
            set { innerAction = value; }
        }

        public new Repeat clone()
        {
            return Repeat.create(innerAction.clone(), times);
        }

        public new Repeat reverse()
        {
            return null;
        }
    } // class Repeat

    public class RepeatForever : ActionInterval
    {
        protected ActionInterval innerAction;

        protected RepeatForever() { }

        public static RepeatForever create(ActionInterval action)
        {
            RepeatForever repeatForever = new RepeatForever();
            if (repeatForever.initWithAction(action))
            {
                return repeatForever;
            }
            repeatForever = null;
            return null;
        }

        protected bool initWithAction(ActionInterval action)
        {
            if (action == null)
            {
                Trace.TraceError("Null action sent to RepeatForever!");
                return false;
            }
            innerAction = action;
            return true;
        }

        public override void startWithTarget(Sprite t)
        {
            base.startWithTarget(t);
            innerAction.startWithTarget(t);
        }

        public override void step(float dt)
        {
            innerAction.step(dt);
            if (innerAction.isDone())
            {
                float diff = innerAction.Elapsed - innerAction.Duration;
                if (diff > innerAction.Duration)
                {
                    diff = diff % innerAction.Duration;
                }
                innerAction.startWithTarget(target);
                innerAction.step(0.0f);
                innerAction.step(diff);
            }
        }

        public override bool isDone()
        {
            return false;
        }

        public ActionInterval InnerAction
        {
            get { return innerAction; }
            set { innerAction = value; }
        }

        public new RepeatForever clone()
        {
            return RepeatForever.create(innerAction.clone());
        }

        public new RepeatForever reverse()
        {
            return null;
        }

    } // class RepeatForever

    public class RotateTo : ActionInterval
    {
        protected float startAngle, destAngle, diffAngle;

        protected RotateTo() { }

        public static RotateTo create(float duration, float destAngle)
        {
            RotateTo rotateTo = new RotateTo();
            if (rotateTo.initWithDuration(duration, destAngle))
            {
                return rotateTo;
            }
            rotateTo = null;
            return null;
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
    } // class RotateTo

    public class RotateBy : ActionInterval
    {
        protected float startAngle, deltaAngle;

        protected RotateBy() { }

        public static RotateBy create(float duration, float deltaAngle)
        {
            RotateBy rotateBy = new RotateBy();
            if (rotateBy.initWithDuration(duration, deltaAngle))
            {
                return rotateBy;
            }
            rotateBy = null;
            return null;
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
    } // class RotateBy

    public class MoveBy : ActionInterval
    {
        protected Vector2 startPosition, deltaPosition, previousPosition;

        protected MoveBy() { }

        public static MoveBy create(float duration, Vector2 deltaPosition)
        {
            MoveBy moveBy = new MoveBy();
            if (moveBy.initWithDuration(duration, deltaPosition))
            {
                return moveBy;
            }
            moveBy = null;
            return null;
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
    } //  class MoveBy

    public class MoveTo : MoveBy
    {
        protected Vector2 endPosition;

        protected MoveTo() { }

        public new static MoveTo create(float duration, Vector2 endPosition)
        {
            MoveTo moveTo = new MoveTo();
            if (moveTo.initWithDuration(duration, endPosition))
            {
                return moveTo;
            }
            moveTo = null;
            return null;
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
    } // class MoveTo

    public class ScaleTo : ActionInterval
    {
        protected float scale, startScale, endScale, deltaScale;

        protected ScaleTo() { }

        public static ScaleTo create(float duration, float s)
        {
            ScaleTo scaleTo = new ScaleTo();
            if (scaleTo.initWithDuration(duration, s))
            {
                return scaleTo;
            }
            scaleTo = null;
            return null;
        }

        protected bool initWithDuration(float duration, float s)
        {
            if (base.initWithDuration(duration))
            {
                this.endScale = s;
                return true;
            }
            return false;
        }

        public override void startWithTarget(Sprite t)
        {
            base.startWithTarget(t);
            startScale = t.Scale;
            deltaScale = endScale - startScale;
        }

        public override void update(float time)
        {
            if (target != null)
            {
                target.Scale = (startScale + deltaScale * time);
            }
        }

        public new ScaleTo clone()
        {
            return ScaleTo.create(duration, endScale);
        }

        public new ScaleTo reverse()
        {
            return null;
        }
    } // class ScaleTo

    public class ScaleBy : ScaleTo
    {
        protected ScaleBy() { }

        public new static ScaleBy create(float duration, float s)
        {
            ScaleBy scaleBy = new ScaleBy();
            if (scaleBy.initWithDuration(duration, s))
            {
                return scaleBy;
            }
            scaleBy = null;
            return null;
        }

        public override void startWithTarget(Sprite t)
        {
            base.startWithTarget(t);
            deltaScale = startScale * endScale - startScale;
        }

        public new ScaleBy clone()
        {
            return ScaleBy.create(duration, endScale);
        }

        public new ScaleBy reverse()
        {
            return ScaleBy.create(duration, 1 / endScale);
        }
    } // class ScaleBy

    public class FadeTo : ActionInterval
    {
        protected float fromOpacity, toOpacity;

        protected FadeTo() { }

        public static FadeTo create(float duration, float opacity)
        {
            FadeTo fadeTo = new FadeTo();
            if (fadeTo.initWithDuration(duration, opacity))
            {
                return fadeTo;
            }
            fadeTo = null;
            return null;
        }

        protected bool initWithDuration(float duration, float opacity)
        {
            if (base.initWithDuration(duration))
            {
                this.toOpacity = opacity;
                return true;
            }
            return false;
        }

        public override void startWithTarget(Sprite t)
        {
            base.startWithTarget(t);
            fromOpacity = t.Opacity;
        }

        public override void update(float deltaTime)
        {
            if (target != null)
            {
                target.Opacity = fromOpacity + (toOpacity - fromOpacity) * deltaTime;
            }
        }

        public new FadeTo clone()
        {
            return FadeTo.create(duration, toOpacity);
        }

        public new FadeTo reverse()
        {
            return null;
        }
    } // class FadeTo

    public class FadeIn : FadeTo
    {
        protected FadeIn() { }

        public static FadeIn create(float duration)
        {
            FadeIn fadeIn = new FadeIn();
            if (fadeIn.initWithDuration(duration))
            {
                return fadeIn;
            }
            fadeIn = null;
            return null;
        }

        public override void startWithTarget(Sprite t)
        {
            base.startWithTarget(t);
            toOpacity = 1.0f;
            fromOpacity = t.Opacity;
        }

        public new FadeIn clone()
        {
            return FadeIn.create(duration);
        }

        public new FadeTo reverse()
        {
            return FadeOut.create(duration);
        }
    } // class FadeIn

    public class FadeOut : FadeTo
    {
        protected FadeOut() { }

        public static FadeOut create(float duration)
        {
            FadeOut fadeOut = new FadeOut();
            if (fadeOut.initWithDuration(duration))
            {
                return fadeOut;
            }
            fadeOut = null;
            return null;
        }

        public override void startWithTarget(Sprite t)
        {
            base.startWithTarget(t);
            toOpacity = 0.0f;
            fromOpacity = t.Opacity;
        }

        public new FadeOut clone()
        {
            return FadeOut.create(duration);
        }

        public new FadeTo reverse()
        {
            return FadeIn.create(duration);
        }

    } // class FadeOut

    public class DelayTime : ActionInterval
    {
        protected DelayTime() { }

        public static DelayTime create(float duration)
        {
            DelayTime delayTime = new DelayTime();
            if (delayTime.initWithDuration(duration))
            {
                return delayTime;
            }
            delayTime = null;
            return null;
        }

        public new DelayTime clone()
        {
            return DelayTime.create(duration);
        }

        public new DelayTime reverse()
        {
            return null;
        }

    }

} // namespace Helper
