using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ARCircuitsLab.Data
{
    /// <summary>
    /// Plain C# class that reads and writes session history as a JSON file.
    /// Kept separate from MonoBehaviours so it can be tested on its own.
    /// </summary>
    public class SessionRepository
    {
        private readonly string filePath;
        private readonly int maxSessions;
        private SessionHistory history = new SessionHistory();

        public SessionRepository(string filePath, int maxSessions)
        {
            this.filePath = filePath;
            this.maxSessions = maxSessions;
        }

        /// <summary>Newest session first.</summary>
        public IReadOnlyList<SessionRecord> All => history.sessions;

        public void Load()
        {
            history = new SessionHistory();
            if (!File.Exists(filePath)) return;

            try
            {
                var loaded = JsonUtility.FromJson<SessionHistory>(File.ReadAllText(filePath));
                if (loaded != null && loaded.sessions != null) history = loaded;
            }
            catch (Exception e)
            {
                // A broken file must never crash the app (NFR-03): keep a copy and start fresh.
                Debug.LogWarning($"[SessionRepository] Could not read history, starting fresh: {e.Message}");
                File.Copy(filePath, filePath + ".bad", true);
            }
        }

        public void Save(SessionRecord record)
        {
            history.sessions.Insert(0, record);
            if (history.sessions.Count > maxSessions)
                history.sessions.RemoveRange(maxSessions, history.sessions.Count - maxSessions);

            // Safe saving: write a temporary file first, then swap it in.
            string tempPath = filePath + ".tmp";
            File.WriteAllText(tempPath, JsonUtility.ToJson(history, true));
            if (File.Exists(filePath)) File.Delete(filePath);
            File.Move(tempPath, filePath);
        }

        public List<SessionRecord> GetRecent(int count)
        {
            int n = Mathf.Min(count, history.sessions.Count);
            return history.sessions.GetRange(0, n);
        }
    }
}
