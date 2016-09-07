using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Helper
{
    public class ActionInstant : FiniteTimeAction
    {
        protected ActionInstant()
        { }

        public override void update(float time)
        { }

        public override void step(float dt)
        {
            float updateDt = 1;
            update(updateDt);
        }

        public override bool isDone()
        {
            return true;
        }

        public new ActionInstant clone()
        {
            return null;
        }

        public new ActionInstant reverse()
        {
            return null;
        }

    } // class ActionInstant

    public class Show : ActionInstant
    {
        protected Show() { }

        public static Show create()
        {
            Show show = new Show();
            return show;
        }

        public override void update(float time)
        {
            target.IsVisible = true;
        }

        public new Show clone()
        {
            return Show.create();
        }

        public new Hide reverse()
        {
            return Hide.create();
        } 

    } // class Show

    public class Hide : ActionInstant
    {
        protected Hide() { }

        public static Hide create()
        {
            Hide hide = new Hide();
            return hide;
        }

        public override void update(float time)
        {
            target.IsVisible = false;
        }

        public new Hide clone()
        {
            return Hide.create();
        }

        public new Show reverse()
        {
            return Show.create();
        }

    } // class Hide

    public class CallFunc : ActionInstant
    {
        protected CallbackDelegate callbackDelegate;

        protected CallFunc()
        { }

        public static CallFunc create(CallbackDelegate callbackDelegate)
        {
            CallFunc CallFunc = new CallFunc();
            if (CallFunc.initWithCallback(callbackDelegate))
            {
                return CallFunc;
            }
            CallFunc = null;
            return null;
        }

        protected bool initWithCallback(CallbackDelegate callbackDelegate)
        {
            this.callbackDelegate = callbackDelegate;
            return true;
        }

        public override void update(float time)
        {
            callbackDelegate();
        }

        public CallbackDelegate Callback
        {
            get { return callbackDelegate; }
            set { callbackDelegate = value; }
        }

        public new CallFunc clone()
        {
            return CallFunc.create(callbackDelegate);
        }

        public new CallFunc reverse()
        {
            return CallFunc.create(callbackDelegate);
        }

    } // class CallFunc
}
