using RedLockNet;
namespace SchedulerJobs.Sds.Caching
{
    public interface IRedisContextAcccessor
    {
        Task<IRedLock> CreateLockAsync(string resource, TimeSpan expiryTime);
        void DisposeContext();
    }
}
