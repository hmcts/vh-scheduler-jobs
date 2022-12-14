using System.Threading.Tasks;

namespace SchedulerJobs.Common.Caching
{
    public interface IDistributedJobRunningStatusCache
    {
        Task UpdateJobRunningStatus(bool isRunning, string jobName);
        Task<bool> IsJobRunning(string jobName);
    }
}