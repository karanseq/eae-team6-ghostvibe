using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace Helper
{
    public class Action
    {
        protected Sprite target;
        protected int tag;

        public Action()
        {
            target = null;
            tag = -1;
        }

        public virtual bool isDone()
        {
            return true;
        }

        public virtual void startWithTarget(Sprite target)
        {
            Trace.Assert((target != null), "Null target passed to Action.startWithTarget!");
            this.target = target;
        }

        public virtual void stop()
        {
            target = null;
        }

        public virtual void step(float dt)
        { }

        public virtual void update(float time)
        { }

        public virtual Action clone()
        {
            return null;
        }

        public virtual Action reverse()
        {
            return null;
        }

        public Sprite Target
        {
            get { return target; }
            set { target = value; }
        }

        public int Tag
        {
            get { return tag; }
            set { tag = value; }
        }
    } // class Action

    public class FiniteTimeAction : Action
    {
        protected float duration;

        public FiniteTimeAction()
        {
            duration = 0.0f;
        }

        public new FiniteTimeAction clone()
        {
            return null;
        }

        public new FiniteTimeAction reverse()
        {
            return null;
        }

        public float Duration
        {
            get { return duration; }
            set { duration = value; }
        }
    } // class FiniteTimeAction

} // namespace Helper
