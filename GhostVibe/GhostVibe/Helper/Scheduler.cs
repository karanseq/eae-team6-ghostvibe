using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Helper
{
    public class Timer
    {
        public static readonly int RepeatForever = -1;
        protected float elapsed, delay, interval;
        protected bool isPaused, isCancelled, runForever, useDelay;
        protected int timesExecuted, repeat;
        protected UpdateDelegate updateDelegate;
        protected Scheduler scheduler;

        public Timer()
        {
            elapsed = -1.0f;
            delay = interval = 0.0f;
            isPaused = isCancelled = runForever = useDelay = false;
            timesExecuted = repeat = 0;
            updateDelegate = null;
            scheduler = null;
        }

        public bool initialize(Scheduler scheduler, UpdateDelegate updateDelegate, float seconds, int repeat, float delay, bool isPaused)
        {
            this.scheduler = scheduler;
            this.updateDelegate = updateDelegate;
            this.elapsed = -1.0f;
            this.interval = seconds;
            this.delay = delay;
            this.isPaused = isPaused;
            this.useDelay = (delay > 0.0f);
            this.repeat = repeat - 1;
            runForever = (repeat == RepeatForever);
            return true;
        }

        public virtual void trigger(float deltaTime)
        {
            if (updateDelegate != null)
            {
                updateDelegate(deltaTime);
            }
        }

        public virtual void cancel()
        {
            isCancelled = true;
        }

        public void update(float deltaTime)
        {
            if (elapsed == -1)
            {
                elapsed = 0;
                timesExecuted = 0;
                return;
            }

            // update elapsed time
            elapsed += deltaTime;

            // deal with delay
            if (useDelay)
            {
                if (elapsed < delay)
                {
                    return;
                }

                trigger(delay);
                elapsed = elapsed - delay;
                timesExecuted += 1;
                useDelay = false;

                if (!runForever && timesExecuted > repeat)
                {
                    cancel();
                    return;
                }
            }

            // if interval is 0, trigger once every frame
            float adjustedInterval = interval > 0 ? interval : elapsed;
            while (elapsed >= interval)
            {
                trigger(adjustedInterval);
                elapsed -= adjustedInterval;
                timesExecuted += 1;

                if (!runForever && timesExecuted > repeat)
                {
                    cancel();
                    return;
                }

                if (elapsed <= 0.0f)
                {
                    break;
                }
            }
        }

        public float Interval
        {
            get { return interval; }
            set { interval = value; }
        }

        public bool IsPaused
        {
            get { return isPaused; }
            set { isPaused = value; }
        }

        public bool IsCancelled
        {
            get { return isCancelled; }
        }
    }

    public class Scheduler
    {
        private static Scheduler instance = null;

        protected Dictionary<UpdateDelegate, Timer> delegateDictionary;
        protected Dictionary<UpdateDelegate, Timer> delegatesToAdd;

        private Scheduler()
        {
            delegateDictionary = new Dictionary<UpdateDelegate, Timer>();
            delegatesToAdd = new Dictionary<UpdateDelegate, Timer>();
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

        public void scheduleDelegateOnce(UpdateDelegate updateDelegate, float interval)
        {
            scheduleDelegate(updateDelegate, interval, 1);
        }

        public void scheduleDelegate(UpdateDelegate updateDelegate, float interval = 0.0f, bool paused = true)
        {
            scheduleDelegate(updateDelegate, interval, Timer.RepeatForever, 0.0f, paused);
        }

        public void scheduleDelegate(UpdateDelegate updateDelegate, float interval, int repeat, float delay = 0.0f, bool paused = true)
        {
            Trace.Assert((updateDelegate != null), "Null delegate provided to function scheduleDelegate!");

            // check if this delegate has already been scheduled
            if (delegateDictionary.ContainsKey(updateDelegate))
            {
                Trace.TraceError("Delegate is already scheduled!");
                return;
            }

            // add the delegate and timer
            Timer timer = new Timer();
            timer.initialize(this, updateDelegate, interval, repeat, delay, paused);
            delegatesToAdd.Add(updateDelegate, timer);
        }

        public void unscheduleDelegate(UpdateDelegate updateDelegate)
        {
            if (updateDelegate == null) return;

            if (delegateDictionary.ContainsKey(updateDelegate))
            {
                delegateDictionary[updateDelegate].cancel();
            }
            else
            {
                Trace.TraceError("Scheduler could not find the delegate to be unscheduled!");
            }
        }

        public void unscheduleAllDelegates()
        {
            foreach (var updateDelegate in delegateDictionary)
            {
                ((Timer)updateDelegate.Value).cancel();
            }
        }

        public void pauseDelegate(UpdateDelegate updateDelegate)
        {
            if (updateDelegate == null) return;

            if (delegateDictionary.ContainsKey(updateDelegate))
            {
                delegateDictionary[updateDelegate].IsPaused = true;
            }
            else
            {
                Trace.TraceError("Scheduler could not find the delegate to be paused!");
            }
        }

        public void pauseAllDelegates()
        {
            foreach (var timer in delegateDictionary)
            {
                ((Timer)timer.Value).IsPaused = true;
            }
        }

        public void resumeDelegate(UpdateDelegate updateDelegate)
        {
            if (updateDelegate == null) return;

            if (delegateDictionary.ContainsKey(updateDelegate))
            {
                delegateDictionary[updateDelegate].IsPaused = false;
            }
            else
            {
                Trace.TraceError("Scheduler could not find the delegate to be resumed!");
            }
        }

        public void resumeAllDelegates()
        {
            foreach (var timer in delegateDictionary)
            {
                ((Timer)timer.Value).IsPaused = false;
            }
        }

        public void update(float deltaTime)
        {
            HashSet<UpdateDelegate> delegatesToRemove = new HashSet<UpdateDelegate>();

            // add any new delegates that haven't been added to the main list
            if (delegatesToAdd.Count > 0)
            {
                foreach (var item in delegatesToAdd)
                {
                    delegateDictionary.Add(item.Key, item.Value);
                }
                delegatesToAdd.Clear();
            }

            // update the timers for each delegate
            foreach (var item in delegateDictionary)
            {
                Timer timer = (Timer)item.Value;
                // save any delegates that must be cancelled
                if (!timer.IsCancelled)
                {
                    timer.update(deltaTime);
                }
                else
                {
                    delegatesToRemove.Add(item.Key);
                }
            }

            // remove any delegates that got cancelled
            foreach (var item in delegatesToRemove)
            {
                if (delegateDictionary.ContainsKey(item))
                {
                    delegateDictionary.Remove(item);
                }
            }
        }

        public bool IsDelegateBeingScheduled(UpdateDelegate updateDelegate)
        {
            return delegateDictionary.ContainsKey(updateDelegate);
        }

    }
}
