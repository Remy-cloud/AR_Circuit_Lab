namespace ARCircuitsLab.Pooling
{
    /// <summary>Anything the object pool can reuse must know how to reset itself.</summary>
    public interface IPoolable
    {
        void OnSpawn();
        void OnDespawn();
    }
}
