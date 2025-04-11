using System.Diagnostics.CodeAnalysis;
using SchedulerJobs.Common.Exceptions;
using SchedulerJobs.Common.Logging;
using SchedulerJobs.Sds.Caching;

namespace SchedulerJobs.Sds.Jobs
{
    [ExcludeFromCodeCoverage]
    public abstract class BaseJob : BackgroundService
    {
        private readonly IHostApplicationLifetime _lifetime;
        private readonly ILogger _logger;
        private readonly IDistributedJobRunningStatusCache _distributedJobRunningStatusCache;

        protected BaseJob(
            IHostApplicationLifetime lifetime, 
            ILogger logger, 
            IDistributedJobRunningStatusCache distributedJobRunningStatusCache)
        {
            _lifetime = lifetime;
            _logger = logger;
            _distributedJobRunningStatusCache = distributedJobRunningStatusCache;
        }
        
        public abstract Task DoWorkAsync();
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var jobName = GetType().Name;
            var lockAcquired = false;

            try
            {
                #if !DEBUG
                    await using var redLock = await _distributedJobRunningStatusCache.CreateLockAsync(jobName);
                    lockAcquired = redLock.IsAcquired;
                    if (!lockAcquired)
                    {
                        _logger.LogInformationJobNameAlreadyRunning(jobName);
                        _lifetime.StopApplication();
                        return;
                    }
                    await _distributedJobRunningStatusCache.UpdateJobRunningStatus(true, jobName);
                #endif

                await DoWorkAsync();

                _lifetime.StopApplication();
            }
            catch (Exception ex)
            {
                // Signal to the OS that this was an error condition
                // Indicates to Kubernetes that the job has failed
                Environment.ExitCode = 1;
                
                _logger.LogErrorJobFailed(jobName);
                throw new JobFailedException($"Job '{jobName}' failed.", ex);
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
