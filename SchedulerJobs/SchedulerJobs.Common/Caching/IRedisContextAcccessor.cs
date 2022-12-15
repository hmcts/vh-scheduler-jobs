using System;
using System.Threading.Tasks;
using RedLockNet;

namespace SchedulerJobs.Common.Caching
{
    public interface IRedisContextAcccessor
    {
        Task<IRedLock> CreateLockAsync(string resource, TimeSpan expiryTime);
        void DisposeContext();
    }
}
