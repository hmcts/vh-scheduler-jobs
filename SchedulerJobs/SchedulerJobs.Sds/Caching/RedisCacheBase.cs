using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace SchedulerJobs.Sds.Caching
{
    public abstract class RedisCacheBase<TKey, TEntry>(IDistributedCache distributedCache)
    {
        public abstract DistributedCacheEntryOptions CacheEntryOptions { get; protected set; }

        public async Task WriteToCache(TKey key, TEntry toWrite)
        {
            if (CacheEntryOptions == null)
                throw new InvalidOperationException($"Cannot write to cache without setting the {nameof(CacheEntryOptions)}");

            var serialisedLayout = JsonConvert.SerializeObject(toWrite, CachingHelper.SerializerSettings);
            var data = Encoding.UTF8.GetBytes(serialisedLayout);
            await distributedCache.SetAsync(GetKey(key), data, CacheEntryOptions);
        }

        protected async Task<TEntry?> ReadFromCache(TKey key)
        {
            try
            {
                var data = await distributedCache.GetAsync(GetKey(key));
                if (data == null || data.Length == 0)
                    return default;
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

        protected async Task RemoveFromCache(TKey key)
        {
            await distributedCache.RemoveAsync(GetKey(key));
        }

        protected abstract string GetKey(TKey key);
    }
}
