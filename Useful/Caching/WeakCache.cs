using System;

namespace IHI.Server.Useful
{
    public class WeakCache<TKey, TValue> where TValue : class
    {
        #region Fields
        #region Field: _cache
        /// <summary>
        ///   Stores the cached instances.
        /// </summary>
        private readonly BluedotDictionary<TKey, TValue> _cache;
        #endregion
        #region Field: _weakCacheWeakReferenceBehaviour
        private readonly BluedotDictionary<TKey, TValue>.WeakReferenceBehaviour _weakCacheWeakReferenceBehaviour;
        #endregion
        #region Field: _weakCacheLazyLoadingBehaviour
        private readonly BluedotDictionary<TKey, TValue>.LazyLoadingBehaviour _weakCacheLazyLoadingBehaviour;
        #endregion
        #endregion

        #region Methods
        #region Method: WeakCache (Constructor)
        internal WeakCache(Func<TKey, TValue> instanceGenerator)
        {
            _weakCacheWeakReferenceBehaviour = new BluedotDictionary<TKey, TValue>.WeakReferenceBehaviour(true);
            _weakCacheLazyLoadingBehaviour = new BluedotDictionary<TKey, TValue>.LazyLoadingBehaviour(true, true, instanceGenerator);

             _cache = new BluedotDictionary<TKey, TValue>(weakReference: _weakCacheWeakReferenceBehaviour, lazyLoading: _weakCacheLazyLoadingBehaviour);
        }
        #endregion
        #endregion

        #region Indexers
        #region Indexer: TKey
        internal TValue this[TKey index]
        {
            get
            {
                return _cache[index];
            }
            set
            {
                _cache[index] = value;
            }
        }
        #endregion
        #endregion

        #region Methods
        #region Method: ContainsKey
        internal bool ContainsKey(TKey key)
        {
            return _cache.ContainsKey(key);
        }
        #endregion

        #region Method: Add
        internal WeakCache<TKey, TValue> Add(TKey key, TValue value)
        {
            _cache.Add(key, value);
            return this;
        }
        #endregion
        #endregion
    }
}