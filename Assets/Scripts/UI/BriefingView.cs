using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ARCircuitsLab.Tasks;

namespace ARCircuitsLab.UI
{
    /// <summary>Explains the chosen experiment before the camera opens.</summary>
    public class BriefingView : ViewBase
    {
        [SerializeField] private Button backButton;
        [SerializeField] private Button findTableButton;
        [SerializeField] private TMP_Text numberText;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text goalText;
        [SerializeField] private TMP_Text learnText;
        [SerializeField] private TMP_Text[] partChips;

        private void Awake()
        {
            Bind(backButton, app.GoToMenu);
            Bind(findTableButton, app.StartLab);
        }

        private void OnEnable()
        {
            TaskManager tasks = app.Tasks;
            TaskData t = tasks.GetTask(tasks.StartIndex);
            numberText.text = $"Experiment {tasks.StartIndex + 1} of {tasks.TaskCount}";
            titleText.text = t.title;
            goalText.text = t.goal;
            learnText.text = t.learn;
            for (int i = 0; i < partChips.Length; i++)
            {
                bool show = t.parts != null && i < t.parts.Length;
                // The chip is the text's parent box.
                partChips[i].transform.parent.gameObject.SetActive(show);
                if (show) partChips[i].text = t.parts[i];
            }
        }
    }
}
