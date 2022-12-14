using System.Diagnostics.CodeAnalysis;
using SchedulerJobs.Common.Caching;

namespace SchedulerJobs.Sds.Jobs
{
    [ExcludeFromCodeCoverage]
    public abstract class BaseJob : BackgroundService
    {
        private readonly IHostApplicationLifetime _lifetime;
        private readonly ILogger _logger;
        private readonly IDistributedJobRunningStatusCache _distributedJobRunningStatusCache;

        protected BaseJob(IHostApplicationLifetime lifetime, ILogger logger, IDistributedJobRunningStatusCache distributedJobRunningStatusCache)
        {
            _lifetime = lifetime;
            _logger = logger;
            _distributedJobRunningStatusCache = distributedJobRunningStatusCache;
        }
        
        public abstract Task DoWorkAsync();
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var jobName = GetType().Name;

            try
            {
                var isRunning = await _distributedJobRunningStatusCache.IsJobRunning(jobName);
                if (isRunning)
                {
                    _logger.LogInformation($"Job {jobName} already running");
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
                await _distributedJobRunningStatusCache.UpdateJobRunningStatus(false, jobName);
            }
        }
    }
}
