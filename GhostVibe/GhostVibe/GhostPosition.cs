using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Helper;
using System.Diagnostics;
using System.Collections.Generic;
using System;
using Helper;

namespace GhostVibe
{
    class GhostPosition
    {
        private static Random rand = new Random();
        private static int index = -1;

        private GhostPosition()
        {
        }

        public static int GetIndex()
        {
            //index = rand.Next(4);
            //return index;
            ++index;
            index = (index > 3) ? 0 : index;
            return index;
        }

        public static Vector2 GetInitialPosition(int ind)
        {
            if (ind == 0)
                return new Vector2(Helper.Helper.ViewportWidth * 0.425f, Helper.Helper.ViewportHeight * 0.5f);
            else if (ind == 1)
                return new Vector2(Helper.Helper.ViewportWidth * 0.475f, Helper.Helper.ViewportHeight * 0.5f);
            else if (ind == 2)
                return new Vector2(Helper.Helper.ViewportWidth * 0.525f, Helper.Helper.ViewportHeight * 0.5f);
            else
                return new Vector2(Helper.Helper.ViewportWidth * 0.575f, Helper.Helper.ViewportHeight * 0.5f);
        }

        public static Vector2 GetSecondPosition(int ind)
        {
            if (ind == 0)
                return new Vector2(Helper.Helper.ViewportWidth * 0.3f, Helper.Helper.ViewportHeight * 0.55f);
            else if (ind == 1)
                return new Vector2(Helper.Helper.ViewportWidth * 0.425f, Helper.Helper.ViewportHeight * 0.55f);
            else if (ind == 2)
                return new Vector2(Helper.Helper.ViewportWidth * 0.575f, Helper.Helper.ViewportHeight * 0.55f);
            else
                return new Vector2(Helper.Helper.ViewportWidth * 0.7f, Helper.Helper.ViewportHeight * 0.55f);
        }

        public static Vector2 GetThirdPosition(int ind)
        {
            if (ind == 0)
                return new Vector2(Helper.Helper.ViewportWidth * 0.2f, Helper.Helper.ViewportHeight * 0.6f);
            else if (ind == 1)
                return new Vector2(Helper.Helper.ViewportWidth * 0.4f, Helper.Helper.ViewportHeight * 0.6f);
            else if (ind == 2)
                return new Vector2(Helper.Helper.ViewportWidth * 0.6f, Helper.Helper.ViewportHeight * 0.6f);
            else
                return new Vector2(Helper.Helper.ViewportWidth * 0.8f, Helper.Helper.ViewportHeight * 0.6f);
        }
    }
}
