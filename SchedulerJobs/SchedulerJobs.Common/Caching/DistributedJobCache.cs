using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace SchedulerJobs.Common.Caching
{
    public class DistributedJobCache : RedisCacheBase<string,bool>, IDistributedJobCache
    {
        public DistributedJobCache(IDistributedCache distributedCache) : base(distributedCache)
        {
            CacheEntryOptions = new DistributedCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromSeconds(15) 
            };
        }

        public override DistributedCacheEntryOptions CacheEntryOptions { get; protected set; }
        
        public override string GetKey(string key)
        {
            return key;
        }

        public async Task UpdateJobRunningStatus(bool isRunning, string keyName)
        {
            await base.WriteToCache(keyName, isRunning);
        }
        
        public async Task<bool> IsJobRunning(string jobKey)
        {
            return await base.ReadFromCache(jobKey);
        }
    }
}
