using System.Diagnostics.CodeAnalysis;
using SchedulerJobs.Common.Caching;

namespace SchedulerJobs.Sds.Jobs
{
    [ExcludeFromCodeCoverage]
    public abstract class BaseJob : BackgroundService
    {
        private readonly IHostApplicationLifetime _lifetime;
        private readonly ILogger _logger;
        private readonly IDistributedJobCache _distributedJobCache;

        protected BaseJob(IHostApplicationLifetime lifetime, ILogger logger, IDistributedJobCache distributedJobCache)
        {
            _lifetime = lifetime;
            _logger = logger;
            _distributedJobCache = distributedJobCache;
        }
        
        public abstract Task DoWorkAsync();
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var jobName = GetType().Name;
            var keyName = $"job_running_status_{jobName}";
            
            try
            {
                var isRunning = await _distributedJobCache.IsJobRunning(keyName);
                if (isRunning)
                {
                    _logger.LogInformation($"Job {jobName} already running");
                    return;
                }
                await _distributedJobCache.UpdateJobRunningStatus(true, keyName);

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
                await _distributedJobCache.UpdateJobRunningStatus(false, keyName);
            }
        }
    }
}
