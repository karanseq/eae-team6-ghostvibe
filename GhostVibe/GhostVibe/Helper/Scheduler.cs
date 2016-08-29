using System;
using System.Collections.Generic;

namespace Helper
{
    interface Schedulable
    {
        void scheduledUpdate(float dt);
    }

    class Scheduler
    {
        private static Scheduler instance = null;

        protected HashSet<Schedulable> listenerSet;

        private Scheduler()
        {
            listenerSet = new HashSet<Schedulable>();
        }

        public static Scheduler Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Scheduler();
                }
                return instance;
            }
        }

        public void scheduleUpdate()
        {

        }

        public void update(float dt)
        {

        }
    }
}
