using System;
using System.Threading.Tasks;
using RedLockNet;
using RedLockNet.SERedis;

namespace SchedulerJobs.Common.Caching
{
    public class RedisContextAccessor : IRedisContextAcccessor
    {
        private readonly RedLockFactory _lockFactory;

        public RedisContextAccessor(RedLockFactory lockFactory)
        {
            _lockFactory = lockFactory;
        }

        public async Task<IRedLock> CreateLockAsync(string resource, TimeSpan expiryTime)
        {
            return await _lockFactory.CreateLockAsync(resource, expiryTime);
        }

        public void DisposeContext()
        {
            _lockFactory.Dispose();
        }
    }
}
