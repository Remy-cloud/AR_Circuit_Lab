using System.Collections.Generic;
using UnityEngine;

namespace ARCircuitsLab.Pooling
{
    /// <summary>
    /// Object Pool pattern: creates objects once and reuses them, instead of
    /// calling Instantiate/Destroy every time (which causes lag on phones).
    /// </summary>
    public class ObjectPool<T> where T : Component, IPoolable
    {
        private readonly T prefab;
        private readonly Transform parent;
        private readonly Stack<T> available = new Stack<T>();
        private readonly HashSet<T> inUse = new HashSet<T>();

        public int ActiveCount => inUse.Count;
        public int AvailableCount => available.Count;

        public ObjectPool(T prefab, int initialSize, Transform parent)
        {
            this.prefab = prefab;
            this.parent = parent;
            for (int i = 0; i < initialSize; i++)
                available.Push(CreateNew());
        }

        public T Get()
        {
            T item = available.Count > 0 ? available.Pop() : CreateNew();
            inUse.Add(item);
            item.gameObject.SetActive(true);
            item.OnSpawn();
            return item;
        }

        public void Release(T item)
        {
            if (!inUse.Remove(item)) return;  // ignore double releases
            item.OnDespawn();
            item.gameObject.SetActive(false);
            available.Push(item);
        }

        public void ReleaseAll()
        {
            foreach (T item in new List<T>(inUse))
                Release(item);
        }

        private T CreateNew()
        {
            T item = Object.Instantiate(prefab, parent);
            item.gameObject.SetActive(false);
            return item;
        }
    }
}
