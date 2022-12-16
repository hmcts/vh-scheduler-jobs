using SchedulerJobs.Sds.Caching;
using SchedulerJobs.Services;

namespace SchedulerJobs.Sds.Jobs
{
    public class GetJudiciaryUsersJob : BaseJob
    {
        private readonly ILogger<GetJudiciaryUsersJob> _logger;
        private readonly IServiceProvider _serviceProvider;
        private bool _jobSucceeded;

        public GetJudiciaryUsersJob(
            IHostApplicationLifetime lifetime,
            ILogger<GetJudiciaryUsersJob> logger,
            IServiceProvider serviceProvider,
            IDistributedJobRunningStatusCache distributedJobRunningStatusCache,
            IRedisContextAccessor redisContextAccessor) : base(lifetime, logger, distributedJobRunningStatusCache, redisContextAccessor)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public override async Task DoWorkAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var jobHistoryService = scope.ServiceProvider.GetRequiredService<IJobHistoryService>();
            var eLinksService = scope.ServiceProvider.GetRequiredService<IELinksService>();
            
            var lastRun = await jobHistoryService.GetMostRecentSuccessfulRunDate(GetType().Name);
            var updatedSince = lastRun ?? DateTime.UtcNow.AddDays(-1);
            _logger.LogInformation("Started GetJudiciaryUsers job at: {Now} - param UpdatedSince: {UpdatedSince}",
                DateTime.UtcNow, updatedSince.ToString("yyyy-MM-dd"));

            try
            {
                await eLinksService.ImportJudiciaryPeopleAsync(updatedSince);
                await eLinksService.ImportLeaversJudiciaryPeopleAsync(updatedSince);
                _jobSucceeded = true;
            }
            catch (Exception ex)
            {
                _jobSucceeded = false;
                _logger.LogError(ex, ex.Message);
                throw;
            }
            finally
            {
                await jobHistoryService.UpdateJobHistory(GetType().Name, _jobSucceeded);
            }
            _logger.LogInformation("Finished GetJudiciaryUsers job at: {Now} - param UpdatedSince: {UpdatedSince}",
                DateTime.UtcNow, updatedSince.ToString("yyyy-MM-dd"));
        }
    }
}