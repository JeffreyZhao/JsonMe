using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace JsonMe
{
    internal abstract class ConcurrentCache<TKey, TValue>
    {
        private ReaderWriterLockSlim m_rwLock = new ReaderWriterLockSlim();
        private Dictionary<TKey, TValue> m_cache = new Dictionary<TKey, TValue>();

        protected abstract TValue Create(TKey key);

        public TValue Get(TKey key)
        {
            TValue value;

            this.m_rwLock.EnterReadLock();
            try
            {
                var cacheHit = this.m_cache.TryGetValue(key, out value);
                if (cacheHit) return value;
            }
            finally
            {
                this.m_rwLock.ExitReadLock();
            }

            this.m_rwLock.EnterWriteLock();
            try
            {
                if (!this.m_cache.TryGetValue(key, out value))
                {
                    value = this.Create(key);
                    this.m_cache[key] = value;
                    return value;
                }
                else
                {
                    return default(TValue);
                }
            }
            finally
            {
                this.m_rwLock.ExitWriteLock();
            }
        }
    }
}
