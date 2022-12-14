using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace SchedulerJobs.Common.Caching
{
    public class DistributedJobRunningStatusRunningStatusCache : RedisCacheBase<string,bool>, IDistributedJobRunningStatusCache
    {
        public DistributedJobRunningStatusRunningStatusCache(IDistributedCache distributedCache) : base(distributedCache)
        {
            CacheEntryOptions = new DistributedCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromHours(24) 
            };
        }

        public override DistributedCacheEntryOptions CacheEntryOptions { get; protected set; }
        
        public override string GetKey(string key)
        {
            return key;
        }

        public async Task UpdateJobRunningStatus(bool isRunning, string keyName)
        {
            if (isRunning)
            {
                await base.WriteToCache(keyName, true);
                return;
            }

            await base.RemoveFromCache(keyName);
        }
        
        public async Task<bool> IsJobRunning(string keyName)
        {
            return await base.ReadFromCache(keyName);
        }
    }
}
