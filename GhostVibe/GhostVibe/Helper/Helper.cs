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
        public static readonly Dictionary<int, int> DifficultyMatrix = new Dictionary<int, int> { {0, 15},
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
            int numNotes = (int)(DifficultyMatrix[currentDifficulty] / beatFrequency);
            
            // initialize the rhythm
            List<int> rhythm = new List<int>(new int[numNotes]);

            // initialize variables to be used for the note generation
            int numTypes = 0, numNotesPerBar = 0;
            List<int> noteTypes;

            switch (currentDifficulty)
            {
                case 0:
                    numTypes = 2;
                    numNotesPerBar = 1;
                    break;
                case 1:
                    numTypes = 2;
                    numNotesPerBar = 2;
                    break;
                case 2:
                    numTypes = 3;
                    numNotesPerBar = 2;
                    break;
                case 3:
                    numTypes = 4;
                    numNotesPerBar = 4;
                    break;
                case 4:
                    numTypes = 2;
                    numNotesPerBar = 4;
                    break;
                case 5:
                    numTypes = 3;
                    numNotesPerBar = 4;
                    break;
                case 6:
                    numTypes = 4;
                    numNotesPerBar = 4;
                    break;
                case 7:
                    numTypes = 2;
                    numNotesPerBar = 8;
                    break;
                case 8:
                    numTypes = 3;
                    numNotesPerBar = 8;
                    break;
                case 9:
                    numTypes = 4;
                    numNotesPerBar = 8;
                    break;
            }

            noteTypes = new List<int>(new int[numTypes]);
            // pick <numTypes> notes from the 4
            for (int i = 0; i < numTypes; ++i)
            {
                // randomly pick a note
                noteTypes[i] = 1 + random.Next(4);

                // ensure it is not repeated
                for (int j = 0; j < numTypes; ++j)
                {
                    // check other notes
                    if (i == j) continue;

                    // if same note, search again
                    if (noteTypes[i] == noteTypes[j])
                    {
                        --i;
                        break;
                    }
                }
            }

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
