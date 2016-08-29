﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostVibe.SimpleGraphics
{
    class Helper
    {
        private Helper()
        { }

        public static float DegreesToRadians(float degrees)
        {
            return ((float)Math.PI * degrees / 180.0f);
        }
    }
}
