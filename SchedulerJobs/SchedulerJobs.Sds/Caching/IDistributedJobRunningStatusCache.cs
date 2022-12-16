using RedLockNet;

namespace SchedulerJobs.Sds.Caching
{
    public interface IDistributedJobRunningStatusCache
    {
        Task<IRedLock> CreateLockAsync(string jobName);
        Task UpdateJobRunningStatus(bool isRunning, string jobName);
        Task<bool> IsJobRunning(string jobName);
        void DisposeCache();
    }
}