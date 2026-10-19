namespace ARCircuitsLab.Data
{
    /// <summary>Everything the HUD needs to show about the running lab.</summary>
    public struct SessionProgress
    {
        public int experimentNumber;   // 1-based
        public int experimentCount;
        public string title;
        public string goal;
        public string hint;
        public int score;
        public int mistakes;
        public float current;          // amps
        public bool circuitClosed;
    }

    /// <summary>A message shown to the student after something important happens.</summary>
    public struct Feedback
    {
        public string title;
        public string body;
    }
}
