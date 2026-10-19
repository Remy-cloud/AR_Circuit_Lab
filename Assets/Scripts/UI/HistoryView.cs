using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ARCircuitsLab.Data;

namespace ARCircuitsLab.UI
{
    /// <summary>Shows the five most recent saved sessions (persists between launches).</summary>
    public class HistoryView : ViewBase
    {
        [Serializable]
        public struct Row
        {
            public GameObject root;
            public TMP_Text dateText;
            public TMP_Text detailText;
            public TMP_Text scoreText;
        }

        [SerializeField] private Button backButton;
        [SerializeField] private TMP_Text bestText;
        [SerializeField] private GameObject emptyText;
        [SerializeField] private Row[] rows = new Row[5];

        private void Awake() => Bind(backButton, app.GoToMenu);

        private void OnEnable()
        {
            List<SessionRecord> recent = app.Data.GetRecent(rows.Length);
            int best = app.Data.BestScore;
            bestText.text = best > 0 ? $"{best} points" : "No labs yet";
            emptyText.SetActive(recent.Count == 0);

            for (int i = 0; i < rows.Length; i++)
            {
                bool has = i < recent.Count;
                rows[i].root.SetActive(has);
                if (!has) continue;

                SessionRecord r = recent[i];
                rows[i].dateText.text = FormatDate(r.date);
                rows[i].detailText.text = $"{r.tasksCompleted} of 3 done, {FormatTime(r.timeTaken)}, {r.mistakes} mistake{(r.mistakes == 1 ? "" : "s")}";
                rows[i].scoreText.text = r.score.ToString();
            }
        }

        private static string FormatDate(string iso)
        {
            if (!DateTime.TryParse(iso, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime d))
                return iso;
            d = d.ToLocalTime();
            if (d.Date == DateTime.Today) return $"Today, {d:HH:mm}";
            if (d.Date == DateTime.Today.AddDays(-1)) return $"Yesterday, {d:HH:mm}";
            return d.ToString("ddd d MMM, HH:mm", CultureInfo.InvariantCulture);
        }
    }
}
