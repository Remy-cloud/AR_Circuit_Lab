using System.Collections.Generic;
using System.IO;
using UnityEngine;
using ARCircuitsLab.Data;
using ARCircuitsLab.Events;

namespace ARCircuitsLab.Managers
{
    /// <summary>
    /// Data Manager: saves each finished session and loads history at start-up.
    /// Data is stored locally in Application.persistentDataPath/sessions.json.
    /// </summary>
    public class DataManager : MonoBehaviour
    {
        public const string FileName = "sessions.json";
        [SerializeField] private int maxSessions = 20;

        private SessionRepository repository;

        public string FilePath => Path.Combine(Application.persistentDataPath, FileName);

        private void Awake()
        {
            repository = new SessionRepository(FilePath, maxSessions);
            LoadSessions();
        }

        private void OnEnable() => GameEvents.SessionEnded += SaveSession;
        private void OnDisable() => GameEvents.SessionEnded -= SaveSession;

        public void LoadSessions()
        {
            repository.Load();
            Debug.Log($"[DataManager] Loaded {repository.All.Count} sessions from {FilePath}");
        }

        /// <summary>The session that just finished (shown on the Summary screen).</summary>
        public SessionRecord LastSession { get; private set; }

        public int BestScore
        {
            get
            {
                int best = 0;
                foreach (SessionRecord r in repository.All) best = Mathf.Max(best, r.score);
                return best;
            }
        }

        public void SaveSession(SessionRecord record)
        {
            LastSession = record;
            repository.Save(record);
            Debug.Log($"[DataManager] Saved session: score {record.score}, {record.tasksCompleted} tasks");
        }

        /// <summary>The History screen shows the 5 most recent sessions (FR-10).</summary>
        public List<SessionRecord> GetRecent(int count = 5) => repository.GetRecent(count);
    }
}
