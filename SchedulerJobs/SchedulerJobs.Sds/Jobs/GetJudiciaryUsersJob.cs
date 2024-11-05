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
            IDistributedJobRunningStatusCache distributedJobRunningStatusCache) : base(lifetime, logger, distributedJobRunningStatusCache)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public override async Task DoWorkAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var jobHistoryService = scope.ServiceProvider.GetRequiredService<IJobHistoryService>();
            var eLinksService = scope.ServiceProvider.GetRequiredService<IELinksService>();

            var updatedSince = await eLinksService.GetUpdatedSince();
            _logger.LogInformation("Started GetJudiciaryUsers job at: {Now} - param UpdatedSince: {UpdatedSince}",
                DateTime.UtcNow, updatedSince.ToString("yyyy-MM-dd"));

            try
            {
                await eLinksService.ImportJudiciaryPeopleAsync(updatedSince);
                await eLinksService.ImportLeaversJudiciaryPeopleAsync(updatedSince);
                _jobSucceeded = true;
            }
            catch (Exception)
            {
                _jobSucceeded = false;
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