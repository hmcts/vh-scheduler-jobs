using System.Diagnostics.CodeAnalysis;
using SchedulerJobs.Sds.Caching;

namespace SchedulerJobs.Sds.Jobs
{
    [ExcludeFromCodeCoverage]
    public abstract class BaseJob : BackgroundService
    {
        private readonly IHostApplicationLifetime _lifetime;
        private readonly ILogger _logger;
        private readonly IDistributedJobRunningStatusCache _distributedJobRunningStatusCache;
        private readonly string _cacheEntryNameOverride;

        protected BaseJob(
            IHostApplicationLifetime lifetime, 
            ILogger logger, 
            IDistributedJobRunningStatusCache distributedJobRunningStatusCache,
            string cacheEntryNameOverride = "")
        {
            _lifetime = lifetime;
            _logger = logger;
            _distributedJobRunningStatusCache = distributedJobRunningStatusCache;
            _cacheEntryNameOverride = cacheEntryNameOverride;
        }
        
        public abstract Task DoWorkAsync();
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var jobName = _cacheEntryNameOverride != "" ? _cacheEntryNameOverride : GetType().Name;
            var lockAcquired = false;

            try
            {
                await using var redLock = await _distributedJobRunningStatusCache.CreateLockAsync(jobName);
                lockAcquired = redLock.IsAcquired;
                if (!lockAcquired)
                {
                    _logger.LogInformation($"Job {jobName} already running");
                    _lifetime.StopApplication();
                    return;
                }
                await _distributedJobRunningStatusCache.UpdateJobRunningStatus(true, jobName);

                await DoWorkAsync();

                _lifetime.StopApplication();
            }
            catch (Exception ex)
            {
                // Signal to the OS that this was an error condition
                // Indicates to Kubernetes that the job has failed
                Environment.ExitCode = 1;
                
                _logger.LogError(ex, $"Job failed: {jobName}");
                throw;
            }
            finally
            {
                if (lockAcquired)
                {
                    await _distributedJobRunningStatusCache.UpdateJobRunningStatus(false, jobName);
                }
                _distributedJobRunningStatusCache.DisposeCache();
            }
        }
    }
}
