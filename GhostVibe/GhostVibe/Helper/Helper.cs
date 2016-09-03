using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public static readonly int maxNotesInBar = 8;
        public static readonly int maxDifficulty = 9;
        public static readonly Dictionary<int, int> difficultyTimeMatrix = new Dictionary<int, int> {
            {0, 5},
            {1, 10},
            {2, 25},
            {3, 45},
            {4, 80},
            {5, 120},
            {6, 170},
            {7, 230},
            {8, 310},
            {9, 400}
        };
        public static readonly Dictionary<int, int> difficultyRhythmMatrix = new Dictionary<int, int>
        {
            {0, 1},
            {1, 2},
            {2, 2},
            {3, 3},
            {4, 4},
            {5, 5},
            {6, 6},
            {7, 7},
            {8, 8},
            {9, 9}
        };

        public static readonly int numMultipliers = 4;
        public static readonly List<int> multiplier = new List<int> { 10, 20, 50, 100 };

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

        public static List<int> GenerateRhythm(int currentDifficulty, float beatFrequency, Random random)
        {
            // first calculate how many notes we need in this rhythm
            int numNotes = (int)(difficultyTimeMatrix[currentDifficulty] / beatFrequency);
            
            // initialize the rhythm
            List<int> rhythm = new List<int>(new int[numNotes]);

            // initialize variables to be used for the note generation
            int numNotesPerBar = difficultyRhythmMatrix[currentDifficulty];
            
            // feed in the notes
            for (int i = 0; i < numNotes; ++i)
            {
                // is this a note or an blank?
                if ((maxNotesInBar == numNotesPerBar) || (i % (maxNotesInBar - numNotesPerBar) == 0))
                {
                    // feed in a note
                    rhythm[i] = 1 + random.Next(4);
                }
                else
                {
                    // feed in a blank
                    rhythm[i] = 0;
                }
            }

            return rhythm;
        }
    }

} // namespace Helper
