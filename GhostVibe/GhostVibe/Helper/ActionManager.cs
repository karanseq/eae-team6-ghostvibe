using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Helper
{
    public class ActionManager
    {
        private static ActionManager instance = null;

        public HashSet<Sprite> targetSet;

        private ActionManager()
        {
            targetSet = new HashSet<Sprite>();
        }

        public static ActionManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ActionManager();
                }
                return instance;
            }
        }

        public void addAction(Action action, Sprite target)
        {
            Trace.Assert(action != null, "Null action sent to ActionManager.addAction!");
            Trace.Assert(target != null, "Null target sent to ActionManager.addAction!");

            // check if this action is already running on some target
            foreach (Sprite t in targetSet)
            {
                Trace.Assert(!t.ActionSet.Contains(action), "Action is already running!");
            }

            // check if we have an instance of the target
            if (!targetSet.Contains(target))
            {
                targetSet.Add(target);
            }

            // store the instance and start the animation
            target.ActionSet.Add(action);
            action.startWithTarget(target);
        }

        public void removeAllActions()
        {
            foreach (Sprite target in targetSet)
            {
                removeAllActionsFromTarget(target);
            }
        }

        public void removeAllActionsFromTarget(Sprite target)
        {
            if (target == null) return;

            if (targetSet.Contains(target))
            {
                target.ActionSet.Clear();
                targetSet.Remove(target);
            }
        }

        public void removeAction(Action action)
        {
            if (action == null) return;

            Sprite target = action.Target;
            if (targetSet.Contains(target))
            {
                if (target.ActionSet.Contains(action))
                {
                    target.ActionSet.Remove(action);
                }
                else
                {
                    Trace.TraceError("ActionManager.removeAction could not find the action to be removed...possible memory leak!");
                }
            }
        }

        public void removeActionByTag(int tag, Sprite target)
        {
            if (target == null) return;

            if (targetSet.Contains(target))
            {
                HashSet<Action> actionsToRemove = new HashSet<Action>();

                foreach (Action action in target.ActionSet)
                {
                    if (action.Tag == tag)
                    {
                        actionsToRemove.Add(action);
                    }
                }

                target.ActionSet.ExceptWith(actionsToRemove);
            }
        }

        public Action getActionByTag(int tag, Sprite target)
        {
            if (tag == -1) return null;

            if (targetSet.Contains(target))
            {
                foreach (Action action in target.ActionSet)
                {
                    if (action.Tag == tag)
                    {
                        return action;
                    }
                }
            }

            return null;
        }

        public int getNumberOfRunningActions(Sprite target)
        {
            if (target == null) return 0;

            if (targetSet.Contains(target))
            {
                return target.ActionSet.Count;
            }

            return 0;
        }

        public void pauseTarget(Sprite target)
        {
            if (target != null && targetSet.Contains(target) == true)
            {
                target.IsPaused = true;
            }
        }

        public void resumeTarget(Sprite target)
        {
            if (target != null && targetSet.Contains(target) == true)
            {
                target.IsPaused = false;
            }
        }

        public void pauseAllRunningActions()
        {
            foreach (Sprite target in targetSet)
            {
                if (!target.IsPaused)
                {
                    target.IsPaused = true;
                }
            }
        }

        public void resumeTargets(List<Sprite> targetsToResume)
        {
            foreach (Sprite target in targetsToResume)
            {
                resumeTarget(target);
            }
        }

        public void update(float dt)
        {
            HashSet<Sprite> targetsToRemove = new HashSet<Sprite>();

            foreach (Sprite target in targetSet)
            {
                if (!target.IsPaused)
                {
                    HashSet<Action> actionsToRemove = new HashSet<Action>();

                    foreach (Action action in target.ActionSet)
                    {
                        if (action == null) continue;

                        action.step(dt);

                        if (action.isDone())
                        {
                            action.stop();
                            actionsToRemove.Add(action);
                        }
                    }

                    target.ActionSet.ExceptWith(actionsToRemove);
                }

                if (target.ActionSet.Count == 0)
                {
                    targetsToRemove.Add(target);
                }
            }

            targetSet.ExceptWith(targetsToRemove);
        }

    } // class ActionManager

} // namespace Helper
