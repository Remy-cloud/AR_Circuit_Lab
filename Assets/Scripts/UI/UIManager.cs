using System;
using System.Collections.Generic;
using UnityEngine;
using ARCircuitsLab.States;

namespace ARCircuitsLab.UI
{
    /// <summary>UI Manager: shows the screen(s) that belong to the current app state.</summary>
    public class UIManager : MonoBehaviour
    {
        [Serializable]
        public struct ScreenEntry
        {
            public AppStateId state;
            public GameObject panel;
        }

        [SerializeField] private List<ScreenEntry> screens = new List<ScreenEntry>();

        public void Show(AppStateId state)
        {
            // Hide everything first, then show the panels for this state
            // (one panel may belong to more than one state).
            foreach (ScreenEntry s in screens)
                if (s.panel != null) s.panel.SetActive(false);
            foreach (ScreenEntry s in screens)
                if (s.panel != null && s.state == state) s.panel.SetActive(true);
        }
    }
}
