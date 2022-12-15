using System.Diagnostics.CodeAnalysis;
using System.Net;
using Microsoft.Extensions.Options;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using SchedulerJobs.Common.Caching;
using SchedulerJobs.Common.Configuration;
using StackExchange.Redis;

namespace SchedulerJobs.Sds.Jobs
{
    [ExcludeFromCodeCoverage]
    public abstract class BaseJob : BackgroundService
    {
        private readonly IHostApplicationLifetime _lifetime;
        private readonly ILogger _logger;
        private readonly IDistributedJobRunningStatusCache _distributedJobRunningStatusCache;
        private readonly ConnectionStrings _connectionStrings;

        protected BaseJob(IHostApplicationLifetime lifetime, ILogger logger, 
            IDistributedJobRunningStatusCache distributedJobRunningStatusCache, IOptions<ConnectionStrings> connectionStrings)
        {
            _lifetime = lifetime;
            _logger = logger;
            _distributedJobRunningStatusCache = distributedJobRunningStatusCache;
            _connectionStrings = connectionStrings.Value;
        }
        
        public abstract Task DoWorkAsync();
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // TODO move this to startup and a helper class. See https://github.com/samcook/RedLock.net/issues/84
            var redisConnectionString = _connectionStrings.RedisCache;
            var muxer = await ConnectionMultiplexer.ConnectAsync(redisConnectionString);
            var connectionMultiplexers = new List<RedLockMultiplexer> { new RedLockMultiplexer(muxer) };
            var redLockFactory = RedLockFactory.Create(connectionMultiplexers);

            var jobName = GetType().Name;
            var lockAcquired = false;

            try
            {
                var resource = $"job_running_status_{jobName}";
                var expiry = TimeSpan.FromHours(24);

                await using var redLock = await redLockFactory.CreateLockAsync(resource, expiry);
                lockAcquired = redLock.IsAcquired;
                _logger.LogInformation($"Starting job - lock acquired: {lockAcquired} at {DateTime.Now}");
                if (!lockAcquired)
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
                if (lockAcquired)
                {
                    await _distributedJobRunningStatusCache.UpdateJobRunningStatus(false, jobName);
                }
                redLockFactory.Dispose();
                
                _logger.LogInformation($"Job ended at {DateTime.Now}");
            }
        }
    }
}
