using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace SchedulerJobs.Common.Caching
{
    public class DistributedJobRunningStatusCache : RedisCacheBase<string,bool>, IDistributedJobRunningStatusCache
    {
        private readonly string _entryPrefix = "job_running_status_";
        
        public DistributedJobRunningStatusCache(IDistributedCache distributedCache) : base(distributedCache)
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

        public async Task UpdateJobRunningStatus(bool isRunning, string jobName)
        {
            var key = $"{_entryPrefix}{jobName}";
            
            if (isRunning)
            {
                await base.WriteToCache(key, true);
                return;
            }

            await base.RemoveFromCache(key);
        }
        
        public async Task<bool> IsJobRunning(string jobName)
        {
            var key = $"{_entryPrefix}{jobName}";
            return await base.ReadFromCache(key);
        }
    }
}
