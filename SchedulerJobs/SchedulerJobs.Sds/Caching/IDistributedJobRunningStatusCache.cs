namespace SchedulerJobs.Sds.Caching
{
    public interface IDistributedJobRunningStatusCache
    {
        Task UpdateJobRunningStatus(bool isRunning, string jobName);
        Task<bool> IsJobRunning(string jobName);
    }
}