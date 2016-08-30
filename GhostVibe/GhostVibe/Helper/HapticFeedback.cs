using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace Helper
{
    class HapticFeedback
    {
        protected static UpdateDelegate delegateStartVibration = new UpdateDelegate(HapticFeedback.startVibration);
        protected static UpdateDelegate delegateStopVibration = new UpdateDelegate(HapticFeedback.stopVibration);
        protected static float beatIntensity = 0.25f;

        private HapticFeedback() { }

        protected static void startVibration(float deltaTime)
        {
            GamePad.SetVibration(PlayerIndex.One, HapticFeedback.beatIntensity, HapticFeedback.beatIntensity);
        }

        protected static void stopVibration(float deltaTime)
        {
            GamePad.SetVibration(PlayerIndex.One, 0.0f, 0.0f);
        }

        public static void startBeats(float beatFrequency, float beatDuration, float beatIntensity)
        {
            HapticFeedback.beatIntensity = beatIntensity;
            Scheduler.Instance.scheduleDelegate(delegateStartVibration, beatFrequency, Timer.RepeatForever, 0.0f);
            Scheduler.Instance.scheduleDelegate(delegateStopVibration, beatFrequency, Timer.RepeatForever, beatDuration);
        }

        public static void stopBeats()
        {
            Scheduler.Instance.unscheduleDelegate(delegateStartVibration);
            Scheduler.Instance.unscheduleDelegate(delegateStopVibration);
            stopVibration(0.0f);
        }
    }
}
