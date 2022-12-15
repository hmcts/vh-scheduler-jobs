using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
namespace SchedulerJobs.Sds.Caching
{
    public abstract class RedisCacheBase<TKey, TEntry>
    {
        private readonly IDistributedCache _distributedCache;
        public abstract DistributedCacheEntryOptions CacheEntryOptions { get; protected set; }

        protected RedisCacheBase(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        public virtual async Task WriteToCache(TKey key, TEntry toWrite)
        {
            if (CacheEntryOptions == null)
                throw new InvalidOperationException($"Cannot write to cache without setting the {nameof(CacheEntryOptions)}");

            var serialisedLayout = JsonConvert.SerializeObject(toWrite, CachingHelper.SerializerSettings);
            var data = Encoding.UTF8.GetBytes(serialisedLayout);
            await _distributedCache.SetAsync(GetKey(key), data, CacheEntryOptions);
        }

        public virtual async Task<TEntry> ReadFromCache(TKey key)
        {
            try
            {
                var data = await _distributedCache.GetAsync(GetKey(key));
                var profileSerialised = Encoding.UTF8.GetString(data);
                var layout =
                    JsonConvert.DeserializeObject<TEntry>(profileSerialised,
                        CachingHelper.SerializerSettings);
                return layout;
            }
            catch (Exception)
            {
                return default;
            }
        }

        public virtual async Task RemoveFromCache(TKey key)
        {
            await _distributedCache.RemoveAsync(GetKey(key));
        }

        public abstract string GetKey(TKey key);
    }
}
