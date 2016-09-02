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
