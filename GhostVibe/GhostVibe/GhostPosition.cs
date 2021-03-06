﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Helper;
using System.Diagnostics;
using System.Collections.Generic;
using System;

namespace GhostVibe
{
    class GhostPosition
    {
        private static Random rand = new Random();
        private static int index = -1;
        private static List<int> indexList = new List<int>() { 0, 1, 2, 3 };

        private GhostPosition()
        {
        }

        public static void ResetIndexList()
        {
            index = -1;
            indexList = new List<int>() { 0, 1, 2, 3 };
        }

        public static void Shuffle()
        {
            int count = indexList.Count;
            while (count > 1)
            {
                count--;
                int k = rand.Next(count + 1);
                int value = indexList[k];
                indexList[k] = indexList[count];
                indexList[count] = value;
            }
        }

        public static int GetIndex()
        {
            //index = rand.Next(4);
            //if (!indexList.Contains(index))
            //{
            //    indexList.Add(index);
            //    return index;
            //}

            ++index;
            //index = (index > 3) ? 0 : index;
            return indexList[index];
        }

        public static Vector2 GetInitialPosition(int ind)
        {
            if (ind == 0)
                return new Vector2(Helper.Helper.ViewportWidth * 0.475f, Helper.Helper.ViewportHeight * 0.5f);
            else if (ind == 1)
                return new Vector2(Helper.Helper.ViewportWidth * 0.485f, Helper.Helper.ViewportHeight * 0.5f);
            else if (ind == 2)
                return new Vector2(Helper.Helper.ViewportWidth * 0.515f, Helper.Helper.ViewportHeight * 0.5f);
            else
                return new Vector2(Helper.Helper.ViewportWidth * 0.525f, Helper.Helper.ViewportHeight * 0.5f);
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
                return new Vector2(Helper.Helper.ViewportWidth * 0.175f, Helper.Helper.ViewportHeight * 0.8f);
            else if (ind == 1)
                return new Vector2(Helper.Helper.ViewportWidth * 0.4f, Helper.Helper.ViewportHeight * 0.8f);
            else if (ind == 2)
                return new Vector2(Helper.Helper.ViewportWidth * 0.625f, Helper.Helper.ViewportHeight * 0.8f);
            else
                return new Vector2(Helper.Helper.ViewportWidth * 0.85f, Helper.Helper.ViewportHeight * 0.8f);
        }
    }
}
