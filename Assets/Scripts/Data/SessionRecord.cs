using System;
using System.Collections.Generic;

namespace ARCircuitsLab.Data
{
    /// <summary>The results of one finished lab session (saved to the device).</summary>
    [Serializable]
    public class SessionRecord
    {
        public string date;          // ISO 8601, e.g. 2026-10-20T08:15:00Z
        public int score;
        public float timeTaken;      // seconds
        public int tasksCompleted;
        public int mistakes;

        public SessionRecord(int score, float timeTaken, int tasksCompleted, int mistakes)
        {
            date = DateTime.UtcNow.ToString("o");
            this.score = score;
            this.timeTaken = timeTaken;
            this.tasksCompleted = tasksCompleted;
            this.mistakes = mistakes;
        }
    }

    /// <summary>Wrapper so JsonUtility can save a list.</summary>
    [Serializable]
    public class SessionHistory
    {
        public int schemaVersion = 1;
        public List<SessionRecord> sessions = new List<SessionRecord>();
    }
}
