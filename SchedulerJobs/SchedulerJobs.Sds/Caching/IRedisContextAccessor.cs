using RedLockNet;
namespace SchedulerJobs.Sds.Caching
{
    public interface IRedisContextAccessor
    {
        Task<IRedLock> CreateLockAsync(string resource, TimeSpan expiryTime);
        void DisposeContext();
    }
}
