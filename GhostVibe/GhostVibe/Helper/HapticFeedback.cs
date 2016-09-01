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

        public static void playBeat(float beatIntensity, float beatDuration)
        {
            startVibration(0.0f);
            Scheduler.Instance.scheduleDelegateOnce(delegateStopVibration, beatDuration);
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

        public static float GetBeatIntensity(Keys keyPressed)
        {
            switch (keyPressed)
            {
                case Keys.A:
                    return 0.1f;
                case Keys.B:
                    return 0.5f;
                case Keys.X:
                    return 0.1f;
                case Keys.Y:
                    return 0.5f;
                default:
                    return 0.3f;
            }
        }

        public static float GetBeatDuration(Keys keyPressed)
        {
            switch (keyPressed)
            {
                case Keys.A:
                    return 0.1f;
                case Keys.B:
                    return 0.5f;
                case Keys.X:
                    return 0.1f;
                case Keys.Y:
                    return 0.5f;
                default:
                    return 0.3f;
            }
        }
    }
}
