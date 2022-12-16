using Microsoft.Extensions.Caching.Distributed;
using RedLockNet;

namespace SchedulerJobs.Sds.Caching
{
    public class DistributedJobRunningStatusCache : RedisCacheBase<string,bool>, IDistributedJobRunningStatusCache
    {
        private readonly IRedisContextAccessor _redisContextAccessor;
        private readonly string _entryPrefix = "job_running_status_";
        private readonly TimeSpan _cacheExpiryTime = TimeSpan.FromHours(23);
        
        public DistributedJobRunningStatusCache(
            IDistributedCache distributedCache, 
            IRedisContextAccessor redisContextAccessor) : base(distributedCache)
        {
            _redisContextAccessor = redisContextAccessor;
            CacheEntryOptions = new DistributedCacheEntryOptions
            {
                SlidingExpiration = _cacheExpiryTime
            };
        }

        public DistributedJobRunningStatusCache(
            IDistributedCache distributedCache, 
            DistributedCacheEntryOptions cacheEntryOptions,
            IRedisContextAccessor redisContextAccessor) : base(distributedCache)
        {
            _redisContextAccessor = redisContextAccessor;
            CacheEntryOptions = cacheEntryOptions;
        }

        public override DistributedCacheEntryOptions CacheEntryOptions { get; protected set; }

        protected override string GetKey(string key)
        {
            return key;
        }

        public async Task<IRedLock> CreateLockAsync(string jobName)
        {
            var resource = $"{_entryPrefix}{jobName}";
            return await _redisContextAccessor.CreateLockAsync(resource, _cacheExpiryTime);
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

        public void DisposeCache()
        {
            _redisContextAccessor.DisposeContext();
        }
    }
}
