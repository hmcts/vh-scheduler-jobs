using System.Threading.Tasks;

namespace SchedulerJobs.Common.Caching
{
    public interface IDistributedJobRunningStatusCache
    {
        Task UpdateJobRunningStatus(bool isRunning, string keyName);
        Task<bool> IsJobRunning(string keyName);
    }
}