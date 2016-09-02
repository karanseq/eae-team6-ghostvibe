using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helper
{
    // define callback delegates here
    public delegate void UpdateDelegate(float deltaTime);
    public delegate void CallbackDelegate();

    class Helper
    {
        private static float viewportWidth = 0.0f, viewportHeight = 0.0f;

        private Helper()
        { }

        public static float DegreesToRadians(float degrees)
        {
            return ((float)Math.PI * degrees / 180.0f);
        }

        public static float RadiansToDegrees(float radians)
        {
            return ((float)Math.PI * 180.0f / radians);
        }

        public static float ViewportWidth
        {
            get { return viewportWidth; }
            set { viewportWidth = value; }
        }

        public static float ViewportHeight
        {
            get { return viewportHeight; }
            set { viewportHeight = value; }
        }

    }

} // namespace Helper
