using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VacantLot.VLUtil
{
    /// <summary>
    /// Dictionary for UnityEngine.Object based on UnityEngine.Object.GetInstanceID()
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class InstnaceIDDictionary<TKey, TValue> : Dictionary<TKey, TValue>
        where TKey : UnityEngine.Object
    {
        public InstnaceIDDictionary() : base(new ObjectComparator<TKey>()) { }
        public InstnaceIDDictionary(int capacity) : base(capacity, new ObjectComparator<TKey>()) { }

        class ObjectComparator<T> : IEqualityComparer<T> where T : UnityEngine.Object
        {
            public bool Equals(T x, T y) => x?.GetInstanceID() == y?.GetInstanceID();
            public int GetHashCode(T obj) => obj.GetInstanceID();
        }

        /// <summary>
        /// Remove Key-Value Pairs where Key is not exists.
        /// </summary>
        /// <param name="onRemove">process Values before remove</param>
        public void Clean(Action<TValue> onRemove = null)
        {
            this.Where(pair => !pair.Key).ToList().ForEach(pair => {
                if(onRemove != null) { onRemove(pair.Value); }
                Remove(pair.Key);
            });
        }
    }
}
