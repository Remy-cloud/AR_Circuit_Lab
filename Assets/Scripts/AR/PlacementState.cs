namespace ARCircuitsLab.AR
{
    /// <summary>The stages of putting the lab bench on a real surface.</summary>
    public enum PlacementState
    {
        Scanning,      // looking for a flat surface
        ReadyToPlace,  // a surface is under the screen centre
        Placed         // the bench is anchored on the table
    }
}
